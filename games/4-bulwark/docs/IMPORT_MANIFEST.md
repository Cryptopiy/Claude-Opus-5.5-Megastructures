# Import manifest — Bulwark

Date: 2026-11-13. Owner of this file: the scribe. Status: survey record and work list for the Phase 2 framework import.

## Purpose

MASTER_PROMPT Part 3.1 lets Bulwark (game 4) import the **framework** of the three earlier games, never their look:

- **Thread** (game 3) is the primary base: post stack, QA, capture harness, audio DSP blocks, editor batch tools, tests and tools.
- **Abyss** (game 1) supplies the mesh kit, instanced batches, backdrop rig, warm-up loader, composition lint and the beam shader.
- **The Ring** (game 2) supplies the fillet path, the walk planner and gaze schedule, the UI Toolkit title/ending/pause layer, the lit shader template, the atmosphere driver, the indirect instanced field and the builder/critic briefs.

Two read-only surveys mapped those files onto Bulwark. This file reproduces both surveys faithfully (only the formatting was fixed: heading levels, table pipes inside code spans escaped), then merges them into one ordered **Copy list** and one list of **Conflicts and decisions to make**. Nothing here is game content: no beats, constants, palette values, key-shot tables, gaze keys, mixes or materials are copied.

How to use it:

1. Settle every open item in [Conflicts and decisions to make](#conflicts-and-decisions-to-make) and log each one in `DEVELOPMENT.md`.
2. Copy in the order of the [Copy list](#copy-list). Rows marked **Decision C-nn** depend on that decision.
3. For the reasoning behind any row, read the matching survey section (A§n = Survey A section n, B§n = Survey B section n; C§n and the other research references point to `docs/RESEARCH.md`).
4. Run the gates of A§9 and B§8 before any beauty work.

### Roots and abbreviations

| Prefix | Path | Role |
|---|---|---|
| `T:` | `<WORKSPACE>\3rd Megastructures\3rd Megastructures\` | Thread, read-only source |
| `A:` | `<WORKSPACE>\1st Megastructures\1st Megastructures\` | Abyss, read-only source |
| `R:` | `<WORKSPACE>\2nd Megastructures\2nd Megastructures\` | The Ring, read-only source |
| `B:` | `<WORKSPACE>\4 Megastructures\4 Megastructures\` | Bulwark, destination |
| `RT/` | `Assets/_Game/Scripts/Runtime/` | same in all four projects |
| `ED/` | `Assets/_Game/Scripts/Editor/` | |
| `TS/` | `Assets/_Game/Scripts/Tests/` | |
| `SH/` | `Assets/_Game/Shaders/` | |

Survey B wrote its paths relative to each project root. The scribe confirmed on 2026-11-13 that the `A:` and `R:` roots exist and that every Abyss and Ring source file named in the Copy list exists there (existence only; contents were not re-read). Shaders of Abyss and The Ring live under `Assets/_Game/Shaders/` (and `Shaders/Include/`). `RingEvents` is defined in `R:RT/Core/RingRig.cs` (line 55), `DspLimiter` in `R:RT/Audio/AudioDsp.cs` (line 260), and the Ring camera probe in `R:RT/Player/CameraRig.cs`.

### Contents

- [Survey A — Thread framework](#survey-a--thread-framework-import-manifest-master_prompt-part-31-thread-bullet)
- [Survey B — Abyss and The Ring](#survey-b--framework-import-from-abyss-and-the-ring-part-31-survey-read-only-2026-11-13)
- [Copy list](#copy-list)
- [Conflicts and decisions to make](#conflicts-and-decisions-to-make)

---

## Survey A — Thread framework import manifest (MASTER_PROMPT Part 3.1, Thread bullet)

Surveyor: `survey-thread`.

This survey only read files; nothing was created, edited or deleted. It covers the Thread docs (`DEVELOPMENT.md`, `STATE.md`, `README.md`, `CLAUDE_FABLE_5.2_FINAL_REPORT.md`, `FEATURE_MATRIX.md`, `HOW_TO_RUN.md`, `docs/RESEARCH.md`, `docs/research/R1`, `R6`), every file under `Tools/`, every `Assets/_Game` script, the asmdefs, the shaders and `Packages/manifest.json`. Thread has **no** `docs/BUILDER_BRIEF.md` or `docs/CRITIC_BRIEF.md`; those briefs come from The Ring.

**Roots and abbreviations used below**
- `T:` = `<WORKSPACE>\3rd Megastructures\3rd Megastructures\` (read-only source)
- `B:` = `<WORKSPACE>\4 Megastructures\4 Megastructures\` (destination)
- `RT/` = `Assets/_Game/Scripts/Runtime/`, `ED/` = `Assets/_Game/Scripts/Editor/`, `TS/` = `Assets/_Game/Scripts/Tests/`, `SH/` = `Assets/_Game/Shaders/`

**Destination state, verified 2026-11-13**
- `B:` already contains:
  - `.gitattributes` = `* -text`
  - a `.gitignore` with `Assets/Resources/PerformanceTestRun*`, `/Tools/fastcompile/runs/` and the QA intermediates
  - `Tools/gpu-guard.ps1`
  - the URP template: `Assets/Scenes`, `TutorialInfo`, `Readme.asset`, `Settings/{PC,Mobile}_{RPAsset,Renderer}`, `SampleSceneProfile`
  - `activeInputHandler: 1`, product name `4 Megastructures`, company `DefaultCompany`
- `B:Packages/manifest.json` is **byte-identical** to Thread's: URP 17.6.0, Input System 1.20.0, Test Framework 1.8.0, UGUI 2.6.0, Timeline 6.6.0, AI Navigation 2.0.14, visualscripting, collab-proxy, ide.rider and ide.visualstudio. Do not touch it. No extra package is needed for the Thread framework.
- Package hashes in `B:Library/PackageCache`: `com.unity.render-pipelines.universal@8457e85b8184` and `com.unity.render-pipelines.core@2d66c71e606e` (the same as Thread's).

### 1. Import conventions

1. **Rename the prefix.**
   - `Thread*` becomes `Bw*` in class and file names. `Th`/`_Th*` becomes `Bw`/`_Bw*` in shader identifiers, uniforms, HLSL functions and `static const` names. `TC_*` and `PAL_*` macro prefixes stay; only the files they come from change.
   - The spec gives some names directly: `BwGradeFeature`, `BwGradePass`, `BwGradeMath`, shader `Megastructure/BwGrade`, `BwLine`, `BwDsp`, `BwSynth`, `BulwarkConst` (replaces `ThreadConstants`) and `BulwarkPalette` (replaces `ThreadPalette`, and generates `BulwarkPalette.hlsl`).
   - Files without a game prefix keep their names: `BlueNoise`, `R2Sequence`, `CliArgs`, `WorldContext`, `MaterialLibrary`, `GameBootstrap`, `BeatDirector`, `ProgressDirector`, `CameraRig`, `QualityBootstrap`, `FadeController`, `GlyphText`, `PauseOverlay`, every QA file, `WavWriter`, `OfflineAudioRenderer`, `FootstepBank` and the editor tools. This avoids churn. The phrase "renamed Bw*" in 14.3 is read as applying only to `Thread*` names; this is logged as a decision.
2. **Namespaces.** Keep `Megastructure` and Thread's sub-namespaces: `Megastructure.Rendering`, `.Player`, `.UI`, `.QA`, `.Audio`, `.EditorTools` and `.Tests`. Asmdef `rootNamespace` values stay as they are.
3. **Log tags.** Replace `"[THREAD] "` with `"[BULWARK] "` and `"[THREAD-FAIL]"` with `"[BULWARK-FAIL]"` everywhere: the runtime, `ED/EditorCommon.cs` `ThreadLog.Prefix`, and the grep patterns in `Tools/unity-queue.ps1` and `Tools/run-player.ps1`. Change `CaptureWatchdog.ExitCodeEnv` from `THREAD_EXIT_CODE` to `BULWARK_EXIT_CODE`.
4. **Do not copy `.meta` files.** Unity generates new GUIDs. Thread's asmdefs and the asmref refer to each other by name, not by GUID. The scene, materials and renderer feature are regenerated by `RebuildAll`, so there are no GUID references to preserve.
5. **Copy no content.** That means no beats, constants values, palette values, key-shot tables, gaze cues, audio mixes, material rows or game shaders. Where a framework file needs a game type, copy a minimal skeleton of that type (marked "skeleton" below) and fill it from Part 14.

### 2. Assembly layout (copy the files, keep the contents; only the destination paths are new)

| Source | Destination | Content / change |
|---|---|---|
| `T:RT/Megastructure.Runtime.asmdef` | `B:RT/Megastructure.Runtime.asmdef` | Copy unchanged. `name` `Megastructure.Runtime`, `rootNamespace` `Megastructure`. References: `Unity.InputSystem`, `Unity.RenderPipelines.Core.Runtime`, `Unity.RenderPipelines.Universal.Runtime`, `UnityEngine.UI`. `autoReferenced` true. Add `Unity.Mathematics` / `Unity.Burst` only if they are used (R3: they are engine modules in 6000.6). UI Toolkit (`UnityEngine.UIElementsModule`) needs no reference. |
| `T:Assets/_Game/Beats/Megastructure.Beats.asmref` | `B:Assets/_Game/Beats/Megastructure.Beats.asmref` | `{"reference": "Megastructure.Runtime"}`. **Required:** `GameBootstrap.DiscoverBuilders` only scans `typeof(IBeatBuilder).Assembly`, so every `BeatNN_*` builder must compile into Runtime through this asmref. |
| `T:ED/Megastructure.Editor.asmdef` | `B:ED/Megastructure.Editor.asmdef` | Unchanged. `rootNamespace` `Megastructure.EditorTools`, `includePlatforms` `["Editor"]`. References: Runtime, InputSystem, Core.Runtime, Core.Editor, Universal.Runtime, Universal.Editor, `Unity.RenderPipelines.GPUDriven.Runtime` (for `GPUResidentDrawerMode`), `UnityEngine.UI`. |
| `T:TS/EditMode/Megastructure.Tests.EditMode.asmdef` | `B:TS/EditMode/…` | Unchanged. `overrideReferences` true with `nunit.framework.dll`, `defineConstraints` `UNITY_INCLUDE_TESTS`, Editor only, `autoReferenced` false. References: Runtime, **Editor**, Core/Universal Runtime and Editor, GPUDriven, TestRunner (Engine and Editor), InputSystem. |
| `T:TS/PlayMode/Megastructure.Tests.PlayMode.asmdef` | `B:TS/PlayMode/…` | Unchanged. All platforms; references Runtime, InputSystem, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`; nunit. |

### 3. File manifest

The "Remove / neutralise" column lists game content inside a framework file. The "Adapt" column lists hard-coded items and the contract changes the Bulwark spec needs.

#### 3.1 Rendering and shaders (the canonical post stack, Part 3.1 rule 6)

| Source → Destination | Depends on | Remove / neutralise | Adapt |
|---|---|---|---|
| `T:RT/Rendering/ThreadGradeFeature.cs` (55 lines) → `B:RT/Rendering/BwGradeFeature.cs` | `BwGradePass`, `GradeState`, `BlueNoise`, URP Runtime | `[DisallowMultipleRendererFeature("Thread Grade")]` | Use the name "Bw Grade". `PipelineConfigurator.GradeFeatureType` must become `"Megastructure.Rendering.BwGradeFeature"`. Keep `applyInSceneView = false` and the camera filter (Game cameras, which include capture render requests). |
| `T:RT/Rendering/ThreadGradePass.cs` (314) → `B:RT/Rendering/BwGradePass.cs` | `GradeState`, `BwGradeMath`, `R2Sequence`, `BlueNoise`, `BwClock.Frame` and `.FreezeDither`, `ThreadMath`-free | Pass names `ThreadBloomPrefilter`, `ThreadBloomDown`, `ThreadBloomUp`, `ThreadGrade`; property IDs `_ThGradeLow`, `Noise`, `SrcSize`, `LowSize`, `P0`, `P1`, `P2`; RT names `_ThreadBloomDown{i}`, `_ThreadBloomUp{i}`, `_ThreadGradeOut`; profiler names; "[THREAD]" warnings | Add the colour-matrix uniforms (section 4.3). `GradeState.Darkness` goes; the dither peak comes per beat. The static dither is the default. |
| `T:RT/Rendering/ThreadGradeMath.cs` (198) → `B:RT/Rendering/BwGradeMath.cs` | `ThreadMath.Smoothstep` (move to `BwMath`), `ThreadConstants.DitherDarkGain` | `DitherDarkD0/D1`, the dark gain in `DitherAmplitude`, the `BloomClamp` comment ("beacon 6 × 1.06") | Set `BloomClamp` from 64 to **40** (14.2.5 "bloom 40"). Add the WB and saturation matrix builder and the 0.18 contrast. Update the order in the `Display()` mirror (section 4.3). Keep the toe-less Neutral (B03-1). |
| `T:RT/Rendering/BlueNoise.cs` (234) → `B:RT/Rendering/BlueNoise.cs` | `ThreadConstants.DitherTile` (64), `DitherSigma` (1.5) | Texture name `ThreadBlueNoise64` becomes `BwBlueNoise64` | Read the constants from `BulwarkConst`. Keep `Seed = 20261029UL` (deterministic, already tested); void-and-cluster takes about 0.1 s. |
| `T:RT/Rendering/R2Sequence.cs` (43) → `B:RT/Rendering/R2Sequence.cs` | none | none | `DitherFrame(frame, freeze)`: freeze is now the default (static). |
| `T:SH/ThreadGrade.shader` (288) → `B:SH/BwGrade.shader` | Core `Common.hlsl`, `Filtering.hlsl`, URP `Core.hlsl`, Core `Runtime/Utilities/Blit.hlsl`, `Generated/ThreadConstantsGen.hlsl` (only for `TC_DitherTile`) | `Shader "Megastructure/ThreadGrade"`, the 4 pass names, `_ThGrade*`, `ThGrade*()`, `kThGrade*` | Include `Generated/BulwarkConstGen.hlsl`. Insert the colour matrix and contrast (section 4.3). Keep the `// pal:ok` markers on float3/float4 literals (T-PAL shader scan). Keep `#pragma target 4.5`, `editor_sync_compilation` and `Fallback Off`. |
| `T:SH/ThreadLine.shader` (389) → `B:SH/BwLine.shader` | `Include/ThreadCommon.hlsl` (fog, `ThreadShadeFlat`, cable shadow, global `_ThScreen`), `Include/ThreadPatterns.hlsl` (coverage fade, ordered dither), `Include/ThreadFlatPasses.hlsl`, generated palette | Flat shading, `ThreadFogTaus`/`ThreadFogMix`, `_CableShadowOn`, `_BelowFade`, `_BelowHaze`, `_ShadeColor`/`_UseShadeColor`, all helix and altitude logic | Rewire to The Ring's lit template (`UniversalFragmentPBR`) and to `BulwarkAtmosphere.hlsl` (`BwFogApply`). **Keep the width maths:** <ul><li>`w_min = max(1.5, 1.75·H/1080)` px, from the global `_BwScreen.y` set in `RenderPipelineManager.beginCameraRendering` (Thread does this in `ProgressDirector.OnBeginCamera`)</li><li>energy alpha `max(_AlphaFloor 0.35, true/w_min)`</li><li>`_LINE_TUBE` clip-space radial push with alpha-to-coverage only when MSAA > 1</li><li>`_LINE_RIBBON` homogeneous-line pixel distance, premultiplied blend</li><li>`_ScaledScreenParams`, `noperspective` varyings</li><li>passes ForwardLit, DepthOnly, DepthNormals; `multi_compile _ _CLUSTER_LIGHT_LOOP` and Lit's shadow keywords</li></ul>H7 needs at least 1.5 px, alpha floor 0.35, A2C under MSAA 4x. |

Not copied: `T:Assets/_Game/Materials/Grade.mat`. `MaterialFactory` regenerates it as `B:Assets/_Game/Materials/Grade.mat` (shader `Megastructure/BwGrade`) and `PipelineConfigurator` assigns it to the feature.

#### 3.2 Core

| Source → Destination | Depends on | Remove / neutralise | Adapt |
|---|---|---|---|
| `T:RT/Core/CliArgs.cs` (108) → `B:RT/Core/CliArgs.cs` | `ThreadConstants.PathLength`, `BeatCount`, `BeatBoundaries` | The header doc (`-tier` and others) | `StartS` from `BulwarkConst`. Add `-dither static\|temporal` (default static), keep `-freeze-dither` as an alias of static, keep `-dither-lsb X` (encode trials; 0 gives the undithered baseline). Document `-report-dir`, `-grade-off`, `-capture-only`, `-capture-4k`, `-seq-res`, `-perf-rt`, `-audio-dump-seconds`, `-graphics-api`, `-dev-build`, `-editor-timeout`. Add every Part 14 flag. |
| `T:RT/Core/WorldContext.cs` (74) → `B:RT/Core/WorldContext.cs` | `MaterialLibrary`, `MeshKit.TriangleCount` (take Abyss's `MeshKit`), `FrameState` | `FrameState.aVirt`, `darkness`, `groundActive`, `nearActive`; `NearRoot` and `GroundGroup` and their comments | Keep `IWorldUpdater`, `IBeatBuilder`, `Register`, `AddMesh` (light probes and reflection probes off, triangle budget) and `BuildLog`. Use Bulwark roots and frame fields: s, ambient t, camera pose, render height, vFOV, beat. |
| `T:RT/Core/ThreadClock.cs` (52) → `B:RT/Core/BwClock.cs` plus `B:RT/Rendering/GradeState.cs` (split the class out) | none | `GradeState.Darkness`; the defaults `DitherLsb 1.0`, `Vignette 0.10`, `BloomIntensity 0.10` | `BwClock`: `Frozen` (`-freeze-ambient`), `Stepped` (capture), `Frame` (dither index), `FreezeDither` (**default true**). Add the H6 capture rule hook (the time branch equals the progress branch). `GradeState`: add `WhiteBalance`, `Tint` and `DitherPeak` (per beat); keep `Enabled` (`-grade-off`) and `FadeBlack`. |
| `T:RT/Core/MaterialLibrary.cs` (49) → `B:RT/Core/MaterialLibrary.cs` (skeleton) | none | All 33 Thread fields (Salt … ThreadRibbon, Sky) | Bulwark fields `BwConcrete`, `BwSea`, …, `BwLine`; keep `Grade`. `[CreateAssetMenu]` stays. |
| `T:RT/Core/ThreadServices.cs` (78) → `B:RT/Core/BwServices.cs` | none | `FootSurface` values (Salt, Stone, Terrace, Metal, Pier); "beacon trigger s >= 858"; `BeginEnding` "6 s" | Surfaces: concrete, wet concrete, road, grating, steel stair, rough ledge. Keep the interfaces `IPlayerState`, `ICameraControl`, `IAudioHooks`, `IFadeUI` and reconcile them with The Ring's walker API (14.11 timings). |
| `T:RT/Core/ShaderIds.cs` (54) → `B:RT/Core/ShaderIds.cs` (skeleton) | none | All `_Th*` globals and Thread material property names | Use `_Bw*` globals. Global names must not collide with material property names (3.2 #5). |
| `T:RT/Core/ThreadPalette.cs` (91) → pattern for `B:RT/Core/BulwarkPalette.cs` | UnityEngine `ScriptableObject` | The whole `Pal` enum and `Table` | Keep the API: `Table` of (id, hex), `Linear()`, `FillFromTable()`, `PaletteToken`. Fill with the 14.2.1 tokens (T-PAL tolerance 0.004 linear). |
| `T:RT/Core/ThreadConstants.cs` (321) → pattern for `B:RT/Core/BulwarkConst.cs` | `KeyShot`, `GazeCue` structs | Everything | Keep the `KeyShot` struct shape; drop `wide`, because 14.10.2 says vFOV is the only value a capture may use. Do not port `GazeCue` (The Ring's `GazeSchedule` keys are in metres). `gen_hlsl.py` only parses `public const double\|float\|int NAME = literal;`, so keep consts literal (no expressions). |
| `T:RT/Core/ThreadMath.cs` (409) → **extract only** `struct D3` and the easing helpers (`Deg`, `Clamp01`, `Lerp`, `Smoothstep`, `Smootherstep01`, `Smootherstep`) into `B:RT/Core/BwMath.cs` | none | Everything else (path, helix, thread law, ascent, fog, FOV) | `D3` is used by `QaProjection`, `ShotPlan` and `WallBot`. |

#### 3.3 Systems

| Source → Destination | Depends on | Remove / neutralise | Adapt |
|---|---|---|---|
| `T:RT/Systems/GameBootstrap.cs` (101) → `B:RT/Systems/GameBootstrap.cs` | `MaterialLibrary`, `ThreadPalette`, `GradeState`, `ThreadClock`, `CliArgs`, `WorldContext`, `IBeatBuilder`, `ProgressDirector` | Fields `palette` (type), `nearRoot`, `groundGroup` | Use the `BulwarkPalette` type and Bulwark roots. Replace `Shader.WarmupAllShaders()` with Abyss's `WarmupLoader` (render every material behind the black fade; 8.6). Keep reflection discovery sorted by `Name`, try/catch per builder with `[BULWARK-FAIL]`, `LoadSeconds` (T-LOAD < 15 s) and `WorldReady`. `-grade-off` stays. |
| `T:RT/Systems/BeatDirector.cs` (52) → `B:RT/Systems/BeatDirector.cs` | `ProgressDirector.S`, `ThreadMath.BeatIndex`, `ThreadConstants.BeatBoundaries`, `BeatCount`, `BeatNames`, `Hysteresis` (12 m), `Player.Finished` | Thread names | Boundaries 0, 120, 420, 700, 920, 1070, 1370 and 1500. "Lantern once" uses `GoalFired`/`GoalCount`; goal trigger at s 1498.5. Keep the `OrderOk` logic, which also allows a `-progress` start. |
| `T:RT/Systems/ProgressDirector.cs` (217) → `B:RT/Systems/ProgressDirector.cs` (**skeleton**) | `ThreadMath`, `FogMath`, `ThreadConstants`, `ThreadPalette` | The **whole** `ApplyGlobals` body: ground sink `-A_virt`, near-frame switch at s 262, sun, ambient, bank, deck slab, cable shadow, far thread, planet, Earth, sea, beacon; `m_sun5600`/`6200`/`6500`; `Darkness` | Keep: `S`, `OverrideActive`/`OverrideS` (capture hook), `Apply()` (the harness calls it before every render; it must run The Ring's `AtmosphereDriver` and the updaters, so instanced draws are re-submitted per capture), `FrameState`, the updater loop with try/catch, `BwClock.Tick` in `LateUpdate`, and the `beginCameraRendering` hook that sets the `_BwScreen` min-width global. **One writer of globals (14.2.2):** `AtmosphereDriver` writes globals and `GradeState` (exposure slew 0.30 EV/s, WB 6 units/s, bloom 0.1/s, bypassed in capture mode). Move `-dither-lsb` to the driver. |

#### 3.4 Player (Part 3.1: `CameraRig` and `ThreadInput` only; the walker comes from The Ring)

| Source → Destination | Depends on | Remove / neutralise | Adapt |
|---|---|---|---|
| `T:RT/Player/CameraRig.cs` (137) → `B:RT/Player/CameraRig.cs` | `PlayerTuning` and `PlayerMath.WrapDeg` (extracted below), `ThreadServices`, `CliArgs`, `ThreadConstants.CameraNear`/`CameraFar`/`GazeCues`/`GazeDefaultPitch`, `ThreadMath.Heading`/`Fov`, `ProgressDirector.S`, `ThreadInput` | Initial pitch from `GazeCues[0]`; the "near 0.1, far 250 000" comment | Near 0.05. The far plane follows s: 150 m in beat 1, 12 000 m outdoors, 300 m and 600 m in beats 5 and 6, switching at s 100, 934, 1030 and 1373, where the exterior cannot be seen. Heading and FOV come from The Ring's `FilletPath` and the 14.2.5 vFOV table with 30 m crossfades. Keep raw mouse counts, pitch ±88, no roll, `SetLook`/`SetPoseOverride`, execution order −50, and the rule that no input is integrated while the AutoWalker drives. |
| `T:RT/Player/ThreadInput.cs` (199) → `B:RT/Player/BwInput.cs` | Input System only | none | Rename only. It null-checks every device, never uses `UnityEngine.Input`, allocates nothing and contains `AnyCancelInput`, C, Esc, F11, Alt+Enter and Shift. |
| `T:RT/Player/PlayerMath.cs`: **only** `class PlayerTuning` (lines 12–48) and `PlayerMath.WrapDeg`/`CritDamp` → `B:RT/Player/PlayerTuning.cs` | none | Head-bob salt values, corridor slab constants (keep only if The Ring's `PathGuide` uses layer 30) | Keep `MouseDegPerCount 0.08`, `StickDegPerSec 120`, `PitchLimit 88`, `LookThresholdCounts 3`, `StickThreshold 0.3`. **Do not port `EyeFollower`:** it is a 0.12 s *time* follower, and Part 9 / 14.12 require smoothing in *distance* (2.5 m y kernel). `StopRamp` and `FootstepCadence` are usable patterns only. Cadence must become speed-dependent: 0.75 m at 1.5 m/s, 0.85 m at 2.5 m/s, 0.9 m on stairs. |
| `T:RT/Player/QualityBootstrap.cs` (84) → `B:RT/Player/QualityBootstrap.cs` (recommended; Part 7.3 requires it, Part 3.1 does not list it) | `CliArgs.PerfLog`, `TryGetRes`, `ThreadInput` | none | VSync stays on by default. `-perf-log` sets vSync 0 and target −1. **Add** `Application.targetFrameRate = 60` for `-autotest`, `-wallbot`, `-soak` and long captures (2.5.3). Keep the F11 and Alt+Enter fallback and `runInBackground`. |

#### 3.5 UI

| Source → Destination | Depends on | Remove / neutralise | Adapt |
|---|---|---|---|
| `T:RT/UI/FadeController.cs` (341) → `B:RT/UI/FadeController.cs` | `GradeState.FadeBlack`, `ThreadServices.Fade`, `CliArgs`, `GameBootstrap.WorldReady`, `GlyphText`, `PauseOverlay`, `ThreadConstants` (FadeIn*, Title*, Hint*, End*, `TitleText`, `EndText`), `ThreadMath.Smoothstep` | Title "THREAD", "the thread goes on", the 14 s ending, the smoothstep easing | **Keep the mechanism:** the picture fade is done inside the grade through `GradeState.FadeBlack`, so the text never passes through the tone curve. Time Scale is 0 while paused, timers use unscaled time, the pause dims the picture inside the grade, there is a kill-volume blink, and there is no UI in `-capture`. Use the 14.11 timings: <ul><li>black 0–1.0 s</li><li>picture 1.0→3.5 s smoothstep</li><li>title 1.4→2.6, hold to 4.6, out to 6.0</li><li>hint at 6.5 s for 6 s with 0.6 s fades, gone 0.4 s after the first movement</li><li>ending: 3.0 s dwell, 4.0 s smootherstep fade, 1.5 s black, the line 2/6/2 s</li><li>pause duck 12 dB over 0.4 s</li></ul>**Text and pause rendering go to The Ring's `UiDirector`/`UiTimeline` (UI Toolkit, 14.11).** |
| `T:RT/UI/GlyphText.cs` (198) → `B:RT/UI/GlyphText.cs` (optional fallback only) | UGUI, `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`, `ThreadPalette` CREAM | CREAM colour | Only if UI Toolkit fails. The colour is TitleText #EDEBE5 at alpha 0.88. |
| `T:RT/UI/PauseOverlay.cs` (115) → `B:RT/UI/PauseOverlay.cs` (logic pattern) | `GlyphText`, `ThreadInput`, `ThreadConstants.TitleSpacingEm` | UGUI rendering | Keep Resume/Restart/Quit, keyboard, gamepad and mouse rectangle hit-testing without an EventSystem. Render it in UI Toolkit so it shares the title typography (Part 9). |

#### 3.6 QA runtime (`RT/QA`)

| Source → Destination | Depends on | Remove / neutralise | Adapt |
|---|---|---|---|
| `QaJson.cs` (146) → same | none (pure C#) | none | none |
| `QaReport.cs` (98) → same | `CliArgs`, `QaJson` | none | In `QaEnvironment`, write the effective dither mode and peak (not `CliArgs.FreezeDither`) and `targetFrameRate`. Rename the "[THREAD]" log prefix. |
| `QaCommon.cs` (396): `QaMode`, `QaLog`, `QaExit`, `PathGuideProbe`, `QaDrive`, `QaChecks` → same | `PathGuide` by reflection on `"Megastructure.Player.PathGuide"` (`Instance`, `KillCount`, `CoverageCheck`), `ThreadConstants.WalkSpeed`/`BriskSpeed`, `ThreadMath.Heading`/`PathGrade`/`Deg`, `WallBot`, `PerfMonitor` | `GradeRangeBehind` (stair and helix grade) | **`QaDrive.Prepare(virtualTime, vSync)` sets `targetFrameRate = -1`, which leaves the bots uncapped:** add the 60 fps cap (2.5.3). The Ring's `PathGuide` must expose `Instance`, `KillCount` and `CoverageCheck`, or the probes skip silently. `DriveSpeed`: add `-cinematic` (2.5 m/s through the AutoWalker). Heading comes from `FilletPath`. |
| `QaRunnerBase.cs` (117) → same | `GameBootstrap.Ready`/`LoadSeconds`, `PathGuideProbe`, `QaLog`, `QaExit`, `QaJson` | none | The common checks are no error logs, no kill-volume events, collider coverage and load under 15 s. Keep them. |
| `AutoTestRunner.cs` (193) → same | `ThreadConstants` Beat*, `WalkSpeed`, `BriskSpeed`, `PathLength`, `TotalTimeWalk`, `BeaconTriggerS`, `GazeDefaultPitch`; `BeatDirector`; `ThreadServices` | The 573.33 ± 8 and 344 ± 5 expectations, "rest at 860", `triggerAt858` | Use the 14.10.4 numbers: <ul><li>1.5 m/s: trigger at 999.15 ± 4 s, ending done at 1017.6 ± 4, exit 0 by 1026 s</li><li>2.5 m/s through the **AutoWalker** cinematic (not `SetDrive` with a forward look): trigger at 599.65 ± 3, done at 618.1 ± 3, exit by 626</li><li>measured progress speed 2.50; ground speed peak ≤ 2.60</li><li>roll 0.00 ± 0.01 manual, ≤ 0.3 cinematic</li><li>beats once and in order; the lantern once</li></ul>Wait for the **ending** to finish, not just "rest". With the 60 fps cap the 1.5 m/s run lasts about 17 min, so it is a long GPU job (section 5). |
| `WallBot.cs` (256) → same | `ThreadMath.HalfWidth`/`PathY`/`PathPoint`/`ProjectToPath`/`Right`/`PathGrade`, `ThreadConstants.EyeHeight`/`CameraYFollowTime`/`HeadBobMax`, `PathGuide` | "0.5 m beyond ±HalfWidth", helix grade-lag logic | 14.10.4: 0.40 m inside both edges in lane coordinates, with `Lateral(s)`. Never leaves the lane; floor-profile deviation ≤ 0.3 m; progress loss ≤ 0.3 m. Add bots T1 at x −3.15 and 0.15 over z 0–120. Cap at 60 fps. |
| `SoakRunner.cs` (188) → same | `ThreadConstants.PathLength`/`GazeDefaultPitch` | Loops back before the beacon trigger (858) | Loop back before 1498.5. Real time with an explicit cap (`targetFrameRate 60`, not only vSync). Run 1800 s through `gpu-guard` with a stall limit of 35 min or more. Keep the 50 MB growth limit, warm-up baseline and tail minimum. |
| `PerfMonitor.cs` (408) → same | `ThreadConstants.Msaa`, `ThreadServices`, `GameBootstrap`, `GradeState.FadeBlack`, `QaMode` | none | Use `BulwarkConst.Msaa = 4`. It runs uncapped for perf runs only (11.7). `-perf-rt WxH` gives 4K offscreen. Log VRAM with `nvidia-smi` in the wrapper (2.5.4); the profiler's "graphics memory" is not the budget number. |
| `CaptureWriter.cs` (311): `CaptureResult`, `WriteJob`, `CaptureWriter`, `CaptureMetrics` → same | none | none | `CaptureMetrics` uses luma 0..1 and `BlackLuma 0.02`; clipped means a channel ≥ 0.999. Re-express with the 14.10.3 definition: luma 0–255, clipped = any channel ≥ 254. |
| `QaProjection.cs` (468): `QaView`, `QaProjection`, `QaSubject`, `QaFraming` → same | `D3`, `ThreadMath` (Deg, `SunDirection`, `SunAlpha`, `PlanetAlpha`, `ThreadAxis`, `ThreadApparentRange`, `ThreadThetaRad`), 18 `ThreadConstants` (planet, cable, colossi, door, pad, beacon) | Every Thread subject builder: sun, planet, thread column, hoods, doorway, cable, beacon | Keep `AzElDir`, `Basis` (identical to `Quaternion.Euler(-pitch, yaw, 0)`), `ProjectDir`, the box and disc projection, and `FrameMargin` 0.03. Subjects come from the "Must be visible" column of 14.10.2 (door, horizon row, parapet row, plume, lamp post, cart, bow, islets, tower top, pendants, whale, lantern). This is the projection core of the 11.13 audit; Abyss's `CompositionLint` adds the occlusion ray casts. |
| `ShotPlan.cs` (508) → same | `ThreadConstants.KeyShots`, `BeatBoundaries`, `BeatNames`, `EyeHeight`, `WalkSpeed`, `GazeDefaultPitch`, `NearCableOnS`, `ExhaleS0`, `SwellWindowEnd`, `EndLineOut1`; `ThreadMath.CorridorToWorld`/`Heading`/`Fov`/`PathPoint` | `TransitionWindows`, `SwapWindow*`, `DeckWindow*`, `BankStaticS`, `MoireCamAim`/`MoireNames`, "KF3 s305 / KF7a s750" extras, `_wide` variants | Keep the set grammar (keyshots, beats, parity, sequences, transitions, statics, pans, edges, encode, stills, all, everything), 1080, 4K, `_1610`, `_219`, free-looks, 30 fps sequences and 60 fps × 10 s encode clips. Windows: ±10 s around each 14.6 boundary plus s 520–640, 880–960, 1060–1080 and 1405–1450. Pans: 0.5 px per frame for 30 frames, and 0.1 px per frame for the lee rail. Statics: 120 frames. Free-looks: yaw +90, −90 and 180 at pitch 0, +60 and −60, plus beat 3 at pitch −90. Walk speed is a parameter (2.5 m/s cinematic). |
| `CaptureHarness.cs` (1093) → same | `ShotPlan`, `CaptureWriter`, `QaProjection`, `ThreadServices`, `ThreadClock`, `GradeState`, `ProgressDirector`, `GameBootstrap`, `ThreadConstants.Msaa`/`KeyShots`/`BeaconTriggerS`/`WalkSpeed`, `ThreadMath.Fov`/`Darkness`/`Deg` | `StaticDiff*` "D" logic (Darkness); grade probe on `KeyShots[0]`; teleport cap `BeaconTriggerS − 1` | Keep the RT path: `SubmitRenderRequest(StandardRequest)` into RGBA16F, MSAA 4, D32_SFloat_S8_UInt; `Graphics.Blit` to `R8G8B8A8_SRGB`; `AsyncGPUReadback.WaitForCompletion`; PNG on worker threads; render in `LateUpdate` at order 32000. Teleport cap becomes 1498.5 − 1. **`HideUi()` only disables UGUI `Canvas`:** also disable `UIDocument`s. Set the H6 clock rule and frozen `SeaState` impacts per shot, and bypass the slew. It currently sets vSync 0 and target −1: add a 60 fps cap for long sequence and encode sets (the output is unchanged because time is virtual). Flags `-capture-only`, `-capture-4k`, `-seq-res`, `-capture-warmup`, `-capture-settle`, `-capture-threads`, `-capture-queue-mb` and `-capture-flip` stay. |

#### 3.7 Audio (`RT/Audio`, Unity-free DSP)

| Source → Destination | Depends on | Remove / neutralise | Adapt |
|---|---|---|---|
| `T:RT/Audio/ThreadDsp.cs` (1079) → `B:RT/Audio/BwDsp.cs` | `AudioSpec` (in `ThreadAudioParams.cs`), `ThreadConstants.AudioSampleRate`/`Drone*`/`PadFreqs`/`HumBase`/`StarNoteFreqs`, `ThreadAudioParams` | **Lines 333–1079** (the `ThreadDsp` score class: layers L1–L8 wind, drone, pad, breath, hush, hum, stars, beacon; master LP sweep) | **Keep lines 1–332:** `XorShift32` (seeded streams), `PinkKellet`, `SvfTpt` (Simper), `DcBlocker` (10 Hz → **20 Hz**), `SineTable` (4096 entries), `FdnReverb8` (Householder, 8 lines, base primes 1427–3539 × size factor, Jot damping, pre-delay crossfade). Make the FDN blend time a parameter (Thread uses a fixed 8 s; Bulwark needs ≥ 2 s on doorways and 0.6 s hard edges) and derive the size factor from the room. Add a brown-noise block, a **look-ahead limiter** (asymptote −1.42 dBFS: c = 10^(−1.42/20) ≈ 0.849 after the gain computer; Thread's limiter is a memoryless `c·tanh(x/c)`, c = 0.84) and true-peak ≤ −1 dBFS. The new score is `BulwarkScore` (14.3). |
| `T:RT/Audio/ThreadSynth.cs` (137) → `B:RT/Audio/BwSynth.cs` | `ThreadDsp`, `ThreadAudioParams`; contains `AudioCapture` | The "lives on the AudioListener" design | **Generator mode on a clip-less AudioSource is primary** (3.2 #14; Thread used the listener filter with the generator as fallback). Keep the double-buffered parameter mailbox, the heartbeat, the exception latch, no allocation or locks on the audio thread, the `-audio-dump` capture and the rebuild on `OnAudioConfigurationChanged`. Mix footsteps *inside* the DSP, as `OfflineAudioRenderer` does, so they pass through the well reverb and the limiter and the player dump matches the offline render within 1 dB. |
| `T:RT/Audio/WavWriter.cs` (181) → same | none (pure C#), `IAudioBlockSink` | none | none (PCM16 with TPDF, float32 with fact chunk, streaming) |
| `T:RT/Audio/OfflineAudioRenderer.cs` (527): `AudioBlockInfo`, `IAudioBlockSink`, `OfflineRenderSettings`, `OfflineRenderInfo`, `WalkLevelMeter`, `AudioMeter` → same | `ThreadDsp`, `ThreadAudioParams`, `FootstepBank`, `ThreadConstants.PathLength`/`WalkSpeed`/`AudioSampleRate`/`BeaconTriggerS` | Goal at 858 with stop ramp T = 4/v0 and a 6 s master fade | `Render(speed)` at both 1.5 and 2.5 (`OfflineRenderSettings.Speed` exists). Use the 1498.5 trigger and the 14.11 ending (18.5 s) and render at 48 kHz. Meters: sliding 3 s RMS ± 1.5 dB, DC < 0.001, step ≤ 0.25 except relays. |
| `T:RT/Audio/FootstepBank.cs` (427): `FootstepRules`, `FootstepSpec`, `FootstepRenderer`, `FootstepBank`, `StepEvent`, `FootstepSequencer` → same | `ThreadConstants` (stair and helix s ranges, step lengths, `AudioSampleRate`), `FootSurface`, `AudioSpec.FootPeakDb` | `SurfaceAt(s)` ranges, the Salt/Stone/Metal recipes, 0.78 and 0.80 m steps | 14.3 recipes: <ul><li>concrete resonators 190/420/1100 Hz with 80/50/30 ms decays, plus a 6 kHz tick when wet</li><li>grating 310/880/2100 Hz plus a 4 kHz ring</li><li>steel stair 260/700/1800 Hz</li><li>ledge plus a 1.5 kHz scrape</li></ul>Feet alternate with ±0.8 dB and ±4 % pitch; peak −30 dBFS. Stride depends on speed. |
| `T:RT/Audio/ThreadAudioParams.cs` (387) → pattern only: extract the generic parts of `AudioSpec` (`Db()`, `SineTableSize`, limiter ceiling, sample rate) into `B:RT/Audio/BwAudioSpec.cs`; the parameter struct becomes `BwAudioParams`, produced by `BulwarkScore` | `ThreadConstants`, `ThreadMath` | `AudioLayer` (8 Thread layers), every level, swell and window | `BulwarkScore` layers W/S/B/F/T/P/V/C, as pure functions of (s, t, speed). |
| `T:RT/Audio/AudioDirector.cs` (431) → pattern only (`B:RT/Audio/AudioDirector.cs` rewritten) | `ThreadServices`, `ProgressDirector`, `ThreadSynth`, `FootstepBank` | The 6 × 5 footstep AudioSource pool, the listener path, metal re-render by D | Keep: `AudioSettings.outputSampleRate` (44.1 kHz devices), `Play()` after `AddComponent`, `-audio-dump [-audio-dump-seconds N]`, the pause duck, execution order 700 (after Progress 500 and Beat 600). |

#### 3.8 Editor tools (`ED/`)

| Source → Destination | Depends on | Remove / neutralise | Adapt |
|---|---|---|---|
| `EditorCommon.cs` (129): `ThreadPaths`, `ThreadLog`, `EditorUtil` → `BwPaths`, `BwLog`, `EditorUtil` | none | `PaletteHlsl`/`ConstantsHlsl`/`PaletteAsset`/`LightingAsset` Thread names; the 5 `Shader*` constants and `ShaderNames` | `ProductName = "Bulwark"`. Keep `CompanyName = "Megastructures"`, so Player.log lands in `%USERPROFILE%\AppData\LocalLow\Megastructures\Bulwark\`. Paths: `Shaders/Generated/BulwarkPalette.hlsl`, `BulwarkConstGen.hlsl`, `Settings/BulwarkPalette.asset`, `Settings/BulwarkLighting.lighting`. `ShaderNames` lists every `Megastructure/Bw*` shader (all go into Always Included). Keep `CorridorLayer 30 "Corridor"` only if The Ring's `PathGuide` uses it. Prefix becomes `"[BULWARK] "`. |
| `BatchTools.cs` (446) → same | `BwPaths`, `BwLog`, `PaletteWriter`, `ShaderGen`, `MaterialFactory`, `PipelineConfigurator`, `ProjectConfigurator`, `SceneBuilder`, `TemplateCleanup`, `CaptureWatchdog`, `CliArgs`, `ThreadConstants.Msaa`/`CameraNear`/`CameraFar`, `GameBootstrap` fields | `CheckSceneContract` items `World/NearRoot`, `World/GroundGroup`, the skybox = ThreadSky check, `gb.nearRoot`/`groundGroup` | Keep `RebuildAll` (steps with try/catch → problem list → exit 1), `BuildWindows` (shader `ShaderHasError` pre-check, `-dev-build`, `-graphics-api`), `BuildMac` (optional), `CompileCheck`, `Validate` (section 6) and `CaptureInEditor`. **`PlayerExePath()` gets an environment override for per-owner build folders** (3.2 #10), e.g. `BW_BUILD_DIR` → `<dir>\Bulwark.exe`. |
| `MaterialFactory.cs` (447): `MaterialConstants`, `ShaderFamily`, `MatSpec`, `MaterialFactory` → same | `ThreadPalette`/`Pal`, `MaterialLibrary`, `BwPaths` | `MaterialConstants` (tints, sway, post fade, windows, halo), the whole `Specs()` list, the `s_FlatKeywords`, `s_LineKeywords` and `s_EmissiveKeywords` sets, the `_ShadeTint` path, the SALT_WHITE default | Keep the machinery: rewrite `.mat` in place with `EditorUtility.CopySerialized` (GUID-stable, idempotent), contract check `Check()`, `LibraryFields()`, linear colour via `.gamma`, explicit keywords, queue, instancing and pass enables. Bulwark families: `BwConcrete`, `BwSea`, `BwSheet`, `BwArmour`, `BwGrating`, `BwSteel`, `BwRoad`, `BwFarField`, `BwWaterPortal`, `BwMist`, `BwSnow`, `BwBeam`, `BwCreature`, `BwEmissive`, `BwLantern`, `BwLine`, `BwGrade` (14.2.6). |
| `ProjectConfigurator.cs` (163) → same | `BwPaths`, `MaterialFactory.FindShader`, `CliArgs` | none | `companyName`/`productName` come from `BwPaths`. Default `-graphics-api d3d11` is correct (8.1). Note: the **Editor uses the new API only from its next start**. Keep: splash off (read back and log), FullScreenWindow, resizable, `runInBackground`, `allowFullscreenSwitch`, `allowHDRDisplaySupport` false, Linear colour space, `enableFrameTimingStats`, Mono, `activeInputHandler` repair, Always Included Shaders, corridor layer. |
| `PipelineConfigurator.cs` (310) → same | `BwPaths`, `ThreadConstants.Msaa` | `ShadowDistance 160`, "flat look" SSAO comment | HDR 64-bit, MSAA 4, render scale 1, depth texture on, opaque texture off, SRP Batcher on, GPU Resident Drawer **off**, Forward+, depth priming disabled, `copyDepthMode AfterOpaques`, one quality level "PC" with vSync 1, SSAO removed, grade feature with material. **Change:** shadow distance **150 m**, 4096, 4 cascades, soft (14.2.3). Set `depthAttachmentFormat`/`depthTextureFormat = DepthFormat.Depth_32_Stencil_8` (exists in URP 17.6 `UniversalRendererData.cs`) instead of `Default`, because 14.12 wants it explicit. Register `MistVolumeFeature` when it exists. Additional-light shadows are a decision (Bulwark has at most 4 local lights). |
| `TemplateCleanup.cs` (98) → same | `BwPaths` (KeptRoots) | none | The candidates match `B:` exactly (`Readme.asset`, `TutorialInfo`, `Scenes/SampleScene.unity`, `Settings/SampleSceneProfile.asset`, `Mobile_RPAsset`, `Mobile_Renderer`). Uses `MoveAssetToTrash`. |
| `SceneBuilder.cs` (202) → same (skeleton; Part 3.1 does not list it, but `RebuildAll` and `Validate` need it) | reflection add-by-name, `MaterialLibrary`, palette | The Thread roots (World/NearRoot/GroundGroup), the skybox = Sky.mat, "far 250 000" | Contract lists `GameComponents`, `PlayerComponents`, `CameraRigComponents`, `PathGuideComponents`, `RootNames` → the Bulwark contract: GameBootstrap, ProgressDirector, BeatDirector, QualityBootstrap, FadeController / The Ring's UiDirector, CaptureHarness, AutoTestRunner, PerfMonitor, WallBot, AtmosphereDriver, a UIDocument with PanelSettings. The camera keeps `renderPostProcessing` false, AA None, dithering false, `allowMSAA`, HDR and an AudioListener. Unity fog off. |
| `PaletteWriter.cs` (32) → same | `ThreadPalette` → `BulwarkPalette` | none | Writes `BulwarkPalette.asset`. |
| `ShaderGen.cs` (158) → same | `ThreadPalette.Table`, `ThreadConstants` const fields | Header text, `THREAD_*_INCLUDED` guards | Output `BulwarkPalette.hlsl` (`PAL_*`) and `BulwarkConstGen.hlsl` (`TC_*`). It must stay **byte-identical** to `Tools/gen_hlsl.py`: F6 "{v:.6f}" trimmed, LF, UTF-8 without BOM, declaration order. |
| `CaptureWatchdog.cs` (115) → same | `CliArgs` | none | `ExitCodeEnv` becomes `BULWARK_EXIT_CODE`. `-editor-timeout` defaults to 1800 s. |

#### 3.9 Tests

| Source → Destination | Keep (exact names) | Drop or rewrite |
|---|---|---|
| `T:TS/EditMode/GradeTests.cs` (567) → `B:TS/EditMode/GradeTests.cs` | `BlueNoise_RankMapIsPermutationOf0To4095`, `BlueNoise_IsDeterministic`, `BlueNoise_GeneratesInUnderOneSecond`, `BlueNoise_HistogramIsUniform`, `BlueNoise_LowFrequencyEnergyFarBelowWhiteNoise`, `BlueNoise_SpatialStatisticsAreBlue`, `BlueNoise_TextureMatchesRanks`, `R2_FirstOffsetsMatchReference`, `R2_OffsetsAreWellDistributed`, `R2_FrozenDitherIsStaticAndLargeFramesStayPrecise`, `Neutral_IdentityBelowStartAndHalfMapsToHalf`, `Neutral_IsMonotonicAndContinuous`, `Neutral_PreservesHue`, `Dither_TpdfRemapIsMonotonicSymmetricTriangular`, `Vignette_IsSubtleSymmetricAndHorPlus`, `Bloom_PyramidIsResolutionNormalised`, `Shader_ConstantsMatchCpuMirror` (with the Bw names) | Rewrite `Neutral_SunDiscStaysBelow098` and `Neutral_BeaconStaysBelow0999` for sun disc 60, lantern 6 and bloom clamp 40. Replace `Dither_AmplitudeBlendsSmoothlyToDarkGain` with a per-beat peak crossfade over 30 m. Replace `Contrast_IsMonotonicWithFixedEndsAndPivot` with the 0.18-pivot contrast test. Add the matrix tests of section 4.3. |
| `T:TS/EditMode/ProjectTests.cs` (542) → same | All 12: `TPal_MaterialColoursArePaletteTokensOrLerps`, `TPal_GeneratedHlslEqualsShaderGen`, `TPal_PaletteAssetEqualsTable`, `TPal_ShaderGenEqualsPythonGenerator`, `TPal_ShaderLiteralsArePaletteMacrosOrMarked`, `BuildSettings_MainIsTheOnlyEnabledScene`, `MainScene_HasNoMissingScriptsAndFollowsTheContract`, `MaterialLibrary_EveryFieldAssignedAndContractHolds`, `Shaders_ExistAndCompileWithoutErrors`, `TDepth_ReversedZAndFloatDepthFormats`, `PlayerSettings_AreConfigured`, `Pipeline_IsConfigured` | Update the expected values (Bw names, depth `Depth_32_Stencil_8`, shadow 150, API exactly D3D11). |
| `T:TS/EditMode/QaTests.cs` (612) → same | `Projection_ForwardMapsToCentre`, `Projection_VerticalFovEdgeIsNdcOne_HorizontalFollowsAspect`, `Projection_BehindCameraIsRejected`, `Projection_BasisMatchesUnityEulerConvention`, `Projection_PanStepMovesTheCentreByThePixelsAsked`, `Projection_DiscBoxMatchesAnIndependentDenseSampling`, `KeyFrameId_StripsVariantSuffixes`, `KeyShots_EveryShotAtBothResolutions`, `KeyShots_FramingVariantsAt1610And219`, `KeyShots_TagBeatMatchesTheBeatOfS`, `Tags_FollowBeatNameTagPattern`, `BeatStills_StartMidEndAndFreeLook`, `Plan_TagsFilesAndSequenceNamesAreUnique`, `Plan_SetNamesAndFilters`, `Sequences_EveryTransitionWindowCoveredForAtLeast20s`, `Sequences_WalkPoseFollowsThePathCentreLookingForward`, `Sequences_StartFirstAndEndingLast`, `Encode_TenSecondsAt60FpsAnd4kOnRequest`, `CaptureMetrics_LumaClipMagentaDiffAndPacking`, `PerfSummary_AveragesLowsAndBudget`, `Soak_GrowthUsesWarmupMinimumAndTailMinimum`, `QaMode_PrecedenceAndSpeed`, `Json_EscapesStringsAndWritesInvariantNumbers` | Drop `Kf1_*`, `Kf2b_*`, `Kf4a_*`, `Kf6b_*`, `Kf7b_*`, `KeyShots_WideShotsAlsoGetTheInGameFovVersion` and `KeyShots_ExtraStillsOfTheTableNotes`. Rewrite: `KeyShots_TableFovMatchesGameFovUnlessWide` → FOV equals FOV(s); `Sequences_StaticClipsPerBeatAndBankPositions` → per-beat statics; `Sequences_SlowPansAndMoirePans` → 0.5 and 0.1 px pans; `AutoTest_ExpectedTimesOf14_10_4` → Bulwark numbers. |
| `T:TS/EditMode/AudioTests.cs` (386) → same | `Dsp_ProcessIsAllocationFree`, `Dsp_IsDeterministicPerSeed`, `Dsp_StableAtDeviceRates`, `Dsp_LayerLevelsAreCalibrated`, `Svf_StableNearNyquist`, `Params_AreContinuousInS`, `Footsteps_BankMeetsRecipe`, `Footsteps_CadenceAndSurfaces`, `Wav_RoundTrip` | `Limiter_CeilingBelowMinus1p5dBFS` → −1.42 asymptote and true peak ≤ −1. `Params_SilencesAndSwells` and `Params_IntroEndingAndPause` → 14.3 S1–S8. Add a render at 1.5 and 2.5 (T-AUDIO). |
| `T:TS/EditMode/CoreTests.cs` (1338), `PlayerTests.cs` (308) | Patterns only (rewrite against Bulwark maths): `TPath_ContinuousAtFiveCentimetreSampling`, `TPath_PlanLength`, `TPath_HeadingContinuity`, `TPath_BeatBoundariesAndIndex`, `Fov_KnotsAndEasing`, `Fov_ChangeWindowsRates`, `Exposure_ScheduleContinuity`, `Palette_HexToLinearMatchesTable`, `Palette_TableCompleteAndAliases`, `Constants_Table1412`, `TCurv_HorizonDip`; `Footsteps_SpacingOverWholePath`, `HeadBob_TinyOffOnStairsNeverAboveLimit`, `Fov_RateBelowOneDegreePerSecondAndEasedSegmentsLong`, `WrapDeg_StaysInRange`, `StopRamp_SlowArrivalStaysFinite` | Do not copy the files. Drop `Cinematic_DurationWithinBudgetAndAllCuesPlayed` (use a constant 2.5 m/s test) and the `EyeFollower_*` tests (time follower). |
| `T:TS/EditMode/FogTests.cs`, `SkyTests.cs` | Patterns only for H4 / 14.2.2: `TDistance_ClosedFormMatchesNumericIntegral`, `TDeck_ClosedFormMatchesNumericIntegral` (closed form against a numeric integral), `ShellIntersection_FloatMatchesDouble` (float32 ray-sphere against double, for T-NUM), `Impostors_AreRoundWithTheOnAxisSize_AtEveryAspect`, `HorizonDip_MatchesAscentTable` | Do not copy. |
| `T:TS/PlayMode/InputTests.cs` (104) → same | `A_ActiveInputHandling_IsInputSystemOnly`, `B_CurrentDevices_ExistWhenPresent`, `C_ThreadInput_SafeWithAnyDeviceSet` (renamed `C_BwInput_…`), `D_VirtualKeyboard_DrivesMoveAndBrisk` | none |

EditMode run (from Thread `STATE.md`): `Tools/unity-queue.ps1 -Name tests -NoQuit -StallMin 0 -ExtraArgs -runTests,-testPlatform,EditMode,-testResults,<abs xml>`. Do not pass `-quit` with `-runTests`, and set no stall limit because tests log nothing while they run.

#### 3.10 Tools and root files

| Source → Destination | Adapt |
|---|---|
| `T:Tools/unity-queue.ps1` (105) → `B:Tools/unity-queue.ps1` | <ul><li>Mutex line 43 `"Global\ThreadUnityQueue_3rdMegastructures"` → `"Global\BulwarkUnityQueue_4Megastructures"`. It must differ from gpu-guard's `Global\MegastructuresLongGpuJob`.</li><li>Unity path line 24: verify `C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe`.</li><li>Project pattern: `$root` comes from `$PSScriptRoot` and `Get-ProjectUnityProcesses` uses a plain substring match. Tighten it to `-projectPath "<root>"` followed by a quote or end, so `…\4 Megastructures\4 Megastructures2` cannot match.</li><li>Grep patterns lines 95 and 97: `[THREAD-FAIL]`/`[THREAD]` → `[BULWARK-FAIL]`/`[BULWARK]`.</li><li>`-StallMin` default 6 → 0 for tests and at least 15 for imports and builds (3.2 #2).</li><li>Add `-BuildDir` to set `$env:BW_BUILD_DIR` for per-owner builds (3.2 #10).</li><li>Keep the comma split of `-ExtraArgs` (3.2 #1), the graceful close of an interactive Editor, the stale-lockfile removal and the Burst filter.</li></ul> |
| `T:Tools/run-player.ps1` (59) → `B:Tools/run-player.ps1` | <ul><li>Exe line 19 `Builds\Thread\Thread.exe` → `Builds\Bulwark\Bulwark.exe`, plus an `-Exe`/`-BuildDir` override.</li><li>Rename the tag greps (lines 53 and 56) and the "start: Thread.exe" text.</li><li>`-StallMin 6` kills silent runs: use 0 for autotest, wallbot and soak, or at least 35 min.</li><li>The single-string comma split breaks `-capture-only a,b`. Add `-ArgString` (one string) and keep the array form for single-valued flags.</li><li>**Long jobs go through `gpu-guard.ps1`**: a `-Long` switch, also implied by `-soak`, 4K `-perf-log`/`-perf-rt`, `-capture-set encode\|sequences\|everything`, `-capture-4k`, any 4K capture, and autotests longer than 2 min. Call `& (Join-Path $PSScriptRoot 'gpu-guard.ps1') -Label $Name -TimeoutMin $T -MinFreeVramMiB (6500 at 4K, else 5000) -FilePath $exe -WorkingDirectory (Split-Path $exe) -ArgumentString $oneString`. Exit 75 means retry later or lower the resolution; 76 means no turn; 124 means the time limit was hit.</li><li>Do not trust `ExitCode` alone (PowerShell 5.1 can return null without a cached handle): also read the QA JSON `pass` field.</li><li>Before starting, refuse if another `Bulwark.exe` or a play-mode Editor of this project is rendering (2.5.2).</li><li>Log `nvidia-smi` VRAM for 2.5.4.</li></ul> |
| `T:Tools/fastcompile.py` (132) → `B:Tools/fastcompile.py` | `DOTNET` path is the same version. Reads refs and defines from `Assembly-CSharp-Editor.csproj` (present in `B:`). Output to per-run folders `Tools/fastcompile/runs/<id>/` (already gitignored in `B:`) instead of a shared `Temp/fastcompile`. Add the mscorlib shim if the Editor project needs it (compare with The Ring's `fastcheck.py`). It is blind to asmdef references and shaders, so always finish with a real Unity compile. |
| `T:Tools/gen_hlsl.py` (69) → `B:Tools/gen_hlsl.py` | Inputs `ThreadPalette.cs`/`ThreadConstants.cs` → `BulwarkPalette.cs`/`BulwarkConst.cs`. Outputs `BulwarkPalette.hlsl`/`BulwarkConstGen.hlsl`. Guards `THREAD_PALETTE_INCLUDED`/`THREAD_CONSTANTS_INCLUDED` → `BULWARK_*`. Header text. Regex `\(Pal\.(\w+),\s*"#hex"\)` must match the `BulwarkPalette.Table` syntax. Byte-identical to `ShaderGen` (test). |
| `T:Tools/metrics.py` (245) → `B:Tools/metrics.py` | <ul><li>`FOLDER_GLOB "*3rd-Megastructures*"` (line 23), `CWD_MARKER "3rd Megastructures"` (line 24), the hard-coded `"3rd-Megastructures"` (line 73) and the row text (line 198) become arguments `--folder-glob "*4-Megastructures*"` and `--cwd-marker "4 Megastructures"`. This project's folder is `C:\Users\<USER>\.claude\projects\W--YouTube-Megastructures-4-Megastructures-4-Megastructures` (observed).</li><li>**Thread's script has no anchor at all.** Add `--anchor`, defaulting to the first line of `STATE.md` (`Run started at <ISO>`, timezone-aware). Find the session by grepping the newest `*.jsonl` under **all** project folders for that exact line; the chat may have started in another project's folder. Include that session's `subagents/**` and workflow files and later sessions whose `cwd` contains the marker.</li><li>Count only records with timestamp ≥ anchor. Wall-clock runs **from the anchor**, not from the first record.</li><li>Exclude the consulting folder `W--YouTube-Megastructures` by **exact** basename (it is a prefix of every project folder).</li><li>`folder_confirmed()` only reads the first `cwd`, so it rejects continued chats.</li><li>Keep: dedup by `message.id`/`requestId` (the largest output wins), the 5 m / 1 h cache split, gaps > 600 s, the optional `long_context` multiplier, the Markdown and JSON output.</li><li>Test early on real transcripts (3.2 #16).</li></ul> |
| `T:Tools/pricing.json` (7) → `B:Tools/pricing.json` | Template only. Re-fetch the official page at the end (15.3.4) and update `source_url` and `accessed`. Thread used https://platform.claude.com/docs/en/about-claude/pricing on 2026-10-30: input 4, cache write 5 m 5, 1 h 8, cache read 0.20, output 20 $/MTok. |
| `T:Tools/qa/qa_common.py` (1224) → `B:Tools/qa/qa_common.py` | `CONSTANTS_CS` → `BulwarkConst.cs`. Rewrite `Spec` for the Bulwark `KeyShot` signature, boundaries and names. Delete `_FALLBACK` (Thread literals). Replace the `TH` table with 14.10.3: luma 0–255; clipped = a channel ≥ 254; magenta 0; isolated pixels > 8 × the 5 × 5 median = 0; BandScore longest run ≤ 6 px at 1080p and ≤ 12 px at 4K, mean run ≤ 0.5/slope for slopes < 0.1 level/px, profile over 128+ columns; sequences ≤ 1.2 levels per frame (2.0 in the sun break), activation diff ≤ 0.5, EV slope ≤ 0.30 EV/s; pan std ≤ 2.0 (1.5 for beat 6 water and beat 7 lowland); flicker 64 × 64 block std ≤ 3 (1.5); audio true peak ≤ −1, RMS ± 1.5 dB. Keep the report.json input contract (shots, subjects, regions, horizonRow). |
| `T:Tools/qa/image_metrics.py` (476), `sequences.py` (325), `encode_test.py` (417), `audio_analysis.py` (475), `contact_sheet.py` (152), `run_all.py` (136), `selftest.py` (471) → same paths | `encode_test.py` already has the correct ffmpeg flags (`scale=…:flags=accurate_rnd+full_chroma_int`, BT.709 TV range, libx264 medium yuv420p, VP9 `-b:v 0`). Add the 14.10.3 (2) gate: dithered longest run ≤ 0.6 × the undithered run of the same clip (`-dither-lsb 0`) on the **4K CRF 23** decode. `sequences.py`: remove the T-SWAP, bank and "D" thresholds. `audio_analysis.py`: 4× oversampled true peak, 3 s RMS against `BulwarkConst` targets; remove the −9 dBFS momentary, LUFS −30 and wind-off checks. Run `selftest.py` early. Verify `python -I -c "import numpy, PIL"`: Thread found numpy missing under 3.12 and used `py -3.10`. |
| `T:Tools/qa/regions_default.json` (156) → `B:Tools/qa/regions_default.json` | Format only: keep `_doc` and an empty `regions` list. Every Thread region (KF1 thread, sky strips, sun exclusion) is game-specific. |
| `T:Tools/qa/run_encode_clips.sh` (21) → `B:Tools/qa/run_encode_clips.sh` | Rewrite the clip and region table for Bulwark (storm sky, dusk sky, deep water veil, mist; clean-gradient regions only). It must call `run-player.ps1 -Long` (gpu-guard), run the undithered twin, and delete frames after measuring (3.2 #15). |
| `T:PLAY_WINDOWS.bat` → `B:PLAY_WINDOWS.bat` | `Thread` → `Bulwark` in `Game\Windows\Bulwark.exe` and `Builds\Bulwark\Bulwark.exe`. |
| `T:.gitattributes`, `T:.gitignore` | **Already present in `B:`** (`* -text`; PerformanceTestRun ignored). Optionally add Thread's `/Assets/Resources.meta` and `/Assets/Resources/`, which the performance package generates. |

### 4. BwGrade: how the Thread pass works and how to extend it for 14.2.5

#### 4.1 Pass structure (keep it)
- **Renderer feature:** URP 17.6 Render Graph only (`RecordRenderGraph`; an `Execute()` override is a silent no-op). The event is `AfterRenderingPostProcessing` with `requiresIntermediateTexture = true`.
- **Skip guards:**
  - URP post enabled on the camera: warn once and skip. The contract is `renderPostProcessing = false`, camera dithering off and AA None.
  - HDR display output active: skip.
  - `!resolveFinalTarget`: skip. Camera stacking is unsupported, so `BwFarField` must stay a dome and never become a second camera; check that Abyss's `BackdropRig` does not use one.
  - `isActiveTargetBackBuffer`: skip.
- **Bloom pyramid:** every level is RGBA16F with Bilinear and Clamp set explicitly (the `TextureDesc` defaults are Point and Repeat).
  - Prefilter at half resolution: Jimenez 13-tap with a partial Karis average, then exposure `×2^EV` (so the threshold acts on exposed values), then `min(c, BloomClamp)`, then the URP soft knee (knee = 0.5 × threshold).
  - Down chain: 13-tap.
  - Up chain: 3×3 tent, `lerp(hi, lo, 0.05 + 0.9·scatter)`.
  - Resolution-normalised: `skip = round(log2(H/1080))` clamped to 0..2 intermediate levels, then 6 bloom levels, coarsest level at least 2 px, at most 10 levels. The glow has the same shape at 1080p and 4K.
  - The bloom is sampled bicubically in the grade pass.
- **Allocations:** `PassData` is pooled, there is one static `MaterialPropertyBlock` per draw (material state is not snapshotted), `DrawProcedural` draws a 3-vertex triangle, names are cached, and nothing allocates per frame.
- **Output:** the grade writes a fresh `R8G8B8A8_SRGB` Point texture, the only 8-bit quantisation, and sets `resourceData.cameraColor = dst`. URP `FinalBlit` copies it 1:1 to the screen, and also into the capture harness's RGBA16F request target. The parity was proven at 0.06 levels.

#### 4.2 Fragment order in Thread (`FragGrade`)
1. `c = sanitize(scene)·2^EV + bloom·intensity`. NaN becomes 0 and values are clamped to 65000.
2. Saturation in linear HDR around Rec.709 luminance: `l + (c − l)·(1 + Sat/100)`, clamped at 0.
3. **Khronos PBR Neutral without the 0.04 toe** (decision B03-1, keep it): identity below 0.76.
   - Above 0.76: `newPeak = 1 − d²/(peak + d − 0.76)` with d = 0.24, a common scale, then a mix toward white with `w = 1 − 1/(0.15·(peak − newPeak) + 1)`.
   - The toe would crush 14.2.1's sRGB-6 floor (linear 0.0018 → about 2e-5, because the toe gives 6.25·x² for x < 0.08), so it stays out even though Bulwark is PBR.
4. Contrast as a display-space S-curve `s + k(s − 0.5)·16s²(1 − s)²` with k = Cont/100 in [−1, 1]. The pivot is sRGB 0.5, and it runs **after** the curve.
5. Vignette: circular in pixels, in half-height units (Hor+). `r² = |p|²/(1 + (16/9)²)`, `shape = (1 − e^(−1.5r²))/(1 − e^(−1.5))`, factor `1 − I·shape`, which equals `1 − I` exactly at a 16:9 corner. It is multiplied by the fade `1 − FadeBlack`.
6. Dither:
   - `s = linearToSRGB(c)`.
   - `n = TPDF(u)·A/255` with `u = BlueNoise[(pixel + (ox, oy)) & 63]`, a rank map stored as `(rank + 0.5)/4096` in RFloat, Point, Repeat.
   - TPDF remap: `sign(t)(1 − sqrt(1 − |t|))` with `t = 2u − 1`, which is URP's `ApplyDithering` remap.
   - `s += n·saturate(min(s, 1 − s)·510)`, so the dither fades within 0.5 code of 0 and 255.
   - It is **monochrome** (one value for R, G and B).
   - The output is `SRGBToLinear(s)`; the GPU re-encodes and rounds once.

**How the amplitude is set in Thread.**
- `A = DitherAmplitude(GradeState.DitherLsb, GradeState.Darkness) = lsb·(1 + 0.5·smoothstep(0.45, 0.55, D))`.
- `GradeState.DitherLsb` is written every frame by `ProgressDirector` from `-dither-lsb` (default `ThreadConstants.DitherLsb` = 1.0).
- A is the **peak** of the triangular distribution in 8-bit codes; the RMS is A/√6.

**How static or temporal is set in Thread.**
- The tile offset `(ox, oy) = R2Sequence.TileOffset(DitherFrame(ThreadClock.Frame, ThreadClock.FreezeDither), 64)` is an integer toroidal R2 shift of about (16, 28) texels per frame. The noise values themselves are never animated.
- `ThreadClock.Frame++` runs in `Tick` unless frozen.
- `-freeze-dither` makes the offset (0, 0), which is static. **Thread's default was temporal.**

**What Bulwark changes (14.7 H5, 14.12).**
- Static is the **default** (`-dither static`). `-dither temporal` is only for high-bitrate cinematic captures; keep `-freeze-dither` as an alias of static.
- The peak comes from `BulwarkConst`: 2.0 in beats 1, 6 and 7, and 1.5 in beats 2–5, crossfaded over 30 m (Rule P). It is written to `GradeState.DitherPeak` by `AtmosphereDriver`.
- Remove the Darkness gain.
- `-dither-lsb X` overrides the peak for encode trials; X = 0 gives the undithered baseline for the 0.6× gate.
- The encode test fixes the final A per beat within 1.0–2.0 and logs it.
- The edge fade only covers 0.5 code. Bulwark's floor (sRGB 6, 4 after a 2.0 dither) keeps pixels away from 0, so it is safe. Optionally scale the fade width to A to avoid clamp bias.

#### 4.3 Colour-matrix extension (14.2.5: exposure, WB, saturation, contrast with pivot 0.18 before the curve, bloom, Neutral, vignette, dither)

**CPU side (`BwGradeMath`, double precision):**
- **White balance:** `W = LMS_2_LIN · diag(k) · LIN_2_LMS`.
  - `k = ColorUtils.ColorBalanceToLMSCoeffs(temperature, tint)`, verified at `com.unity.render-pipelines.core@2d66c71e606e/Runtime/Utilities/ColorUtils.cs:79`.
  - The matrices are copied from Core `ShaderLibrary/Color.hlsl:254-272`. This is URP's own LUT white balance (`ColorGradingLutPass.cs:139`): temperature > 0 is warmer, matching Bulwark's +6/+10 in the sodium and tungsten beats and −8/−6 in noon and water.
  - Normalise W so the luminance of (1, 1, 1) is unchanged; exposure stays the only brightness control.
- **Saturation:** `S = (1 − f)·[lum; lum; lum] + f·I`, f = 1 + Sat/100.
- **Combined:** `M = S·W`. It replaces Thread's separate saturation step.

**Uniforms:** add three `float4` rows `_BwGradeM0`, `_BwGradeM1`, `_BwGradeM2` (xyz used) to the shader's `UnityPerMaterial` CBUFFER and set them through the pass's MPB. Pack them as new P3/P4/P5 vectors in `AddPass`.

**Shader order:**
1. Prefilter: `c = min(mul(M, c·2^EV), clamp)`, then threshold, so the bloom colour matches the graded scene.
2. Grade: `c = max(mul(M, scene·2^EV), 0)`.
3. Contrast in linear HDR, before the curve: `Y = dot(lum, c)`, `Y' = 0.18·(Y/0.18)^(1+k)`, `c *= Y'/max(Y, ε)`. This is hue-preserving with k = Cont/100, blended with a C1 smoothstep to the identity below about Y 0.01. The toe protection preserves the sRGB-6 floor (3.2 #6: URP-style contrast crushed dark fog in Abyss). Log this as a DEVELOPMENT decision with the numbers: without protection, Y 0.0018 at k 0.04 maps to 0.0015, about sRGB 5.
4. `c += bloom·intensity`.
5. Neutral, toe-less.
6. Vignette × fade.
7. Dither.
8. Delete Thread's display-space S-curve, or keep it only at k = 0.

**State and slews:**
- `GradeState` gains `WhiteBalance`, `Tint` and `DitherPeak`; `Saturation` and `Contrast` stay.
- The slews live in `AtmosphereDriver`, never in the pass: EV 0.30 EV/s, WB 6 units/s, bloom 0.1/s. They are bypassed in capture mode.
- Bloom: threshold 0.8–1.0, intensity 0.15–0.30, scatter 0.65–0.80 (URP semantics, unchanged).

**Tests to add to `GradeTests`:**
- Identity at WB 0 and Sat 0.
- White luminance preserved.
- Saturation keeps grey and luminance.
- WB sign matches URP (T > 0 raises R/B).
- 0.18 is a fixed point; contrast is monotonic.
- The floor 0.0018 maps to at least sRGB 5.5.
- The CPU `Display()` mirror equals the shader order (`Shader_ConstantsMatchCpuMirror` asserts the call strings).
- The dither peak crossfades over 30 m and is static by default.

### 5. Hard-coded items checklist (Part 3.1 rule 3)
- `Tools/unity-queue.ps1`: mutex name, Unity path, project pattern, log tags, stall default, build-dir environment variable (section 3.10).
- `Tools/run-player.ps1`: exe name and path, gpu-guard for long jobs, stall default, argument passing, VRAM logging.
- `Tools/metrics.py`: folder glob, cwd marker, line-73 literal, row text, **anchor (missing)**, consulting-folder exclusion, price file refresh.
- `Tools/gen_hlsl.py`, `ED/ShaderGen.cs`: file names and guards.
- `ED/EditorCommon.cs`: `ProductName = "Bulwark"`, `CompanyName = "Megastructures"`, every path and shader name, prefix.
- `ED/BatchTools.cs`: `PlayerExePath` reads the per-owner environment variable.
- `ED/PipelineConfigurator.cs`: `GradeFeatureType`, shadow 150, depth `Depth_32_Stencil_8`.
- `ED/CaptureWatchdog.cs`: environment variable name.
- Asmdef references: unchanged; add Mathematics or Burst only if used.
- Runtime `[THREAD]` strings: about 60 sites across Systems, QA, Player, Audio and Rendering.
- The QA frame cap.
- `BulwarkConst.Msaa`.

### 6. `Validate` (`BatchTools.ValidateInternal`): the 37 rows of a passing Thread run
1. `scene file`
2. `missing scripts`. A `material on <obj>` row appears only on a failure.
3. 9 rows `Game : <type>`: GameBootstrap, ProgressDirector, BeatDirector, Player.QualityBootstrap, UI.FadeController, QA.CaptureHarness, QA.AutoTestRunner, QA.PerfMonitor, QA.WallBot.
4. 2 rows `Player : PlayerController`, `AutoWalker`.
5. 2 rows `Player/CameraRig : CameraRig`, `Audio.AudioDirector`.
6. `PathGuide : Player.PathGuide`. For `Sun`, `World/NearRoot` and `World/GroundGroup` only the object's existence is checked, and a row appears only on a failure.
7. `camera`: MainCamera tag, `allowHDR`, clear = Skybox, near and far equal the constants.
8. `camera URP data`: `renderPostProcessing` false, AA None, dithering false, shadows on.
9. `audio listener` on CameraRig.
10. `sun light`: Directional with Soft shadows.
11. `GameBootstrap refs`: materials, palette, nearRoot, groundGroup, sun, camera.
12. `skybox` = ThreadSky.
13. `unity fog off`.
14. `build scenes`: only Main.unity.
15. `material contract` (`MaterialFactory.Check`, plus one row per issue).
16. 5 rows `shader <name>` (ThreadFlat, Line, Emissive, Sky, Grade). First `ShaderUtil.CompilePass` runs synchronously on every pass of every library material, then `GetShaderMessages` must give 0 errors and `ShaderHasError` must be false.
17. `default pipeline` = PC_RPAsset.
18. `quality levels`: one level on PC_RPAsset.
19. `hdr 64-bit / msaa`: `_64Bits` and MSAA 4.
20. `forward+`.
21. `grade feature`: present, active, material set.
22. `input handler` = 1.
23. `graphics API`: auto off.

Totals: 2 + 9 + 2 + 2 + 1 + 5 + 1 + 1 + 1 + 5 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 = 37.

**Changes for Bulwark:**
- New scene-contract rows and the new shader list (one row per Bw shader).
- Remove the NearRoot and GroundGroup checks.
- Skybox becomes the `BwFarField` or sky decision.
- The camera far plane equals the beat-1 value.
- `graphics API` must equal **exactly `[Direct3D11]`**; Thread only checked that auto was off.

**Add rows for:**
- depth format D32_SFloat_S8_UInt
- GPU Resident Drawer off
- no SSAO feature
- every Bw shader in Always Included Shaders
- no Volume overrides in the volume profile (no URP colour overrides)
- UIDocument and PanelSettings present
- `MistVolumeFeature` present once built
- splash off read back
- product and company names
- shadows: 4 cascades, 4096, 150 m
- the Mobile template assets removed

### 7. Lessons in Thread's docs that the import must respect
1. **URP 17.6** (R1, R2, R6):
   - Implement `RecordRenderGraph` only.
   - Turn URP post off on the camera (otherwise LDR grading clamps and dithers before the grade), and keep camera dithering off.
   - Use 64-bit HDR (R11G11B10 has 5-bit blue steps of up to 2.6 codes).
   - Never `CreateTexture(cameraColor)` for an output (it inherits MSAA).
   - Depth priming and TAA are ignored or unavailable with MSAA 4.
   - Declare `_CLUSTER_LIGHT_LOOP` even with one light.
   - Use `GetMainLight(shadowCoord, positionWS, 1)` to get the shadow-distance fade.
   - Every lit shader has four passes; Unity fog is not used.
   - Keep `allowHDRDisplaySupport` false.
2. **Shipping shaders:**
   - The materials live in `MaterialLibrary` and the shaders are in Always Included.
   - The grade material is referenced by the renderer feature.
   - `Validate` compiles every pass synchronously before reading `ShaderHasError`.
   - Keywords are explicit.
3. **Captures:**
   - Built player only; `WaitForEndOfFrame` does not run in Editor batch mode.
   - The RT format decides the intermediate format.
   - `camera.allowMSAA` must be on.
   - The capture harness calls `ProgressDirector.Apply()` before every render, so updaters re-submit `RenderMeshInstanced` draws (Thread's `InstancedSet` pattern) and globals match the pose.
4. **Encode** (`QA/reports/encode_summary.md`):
   - x264 and VP9 at 130–540 kbit/s flattened the 1 LSB dither into plateaus. Raising it to 1.5 or 2.0 did not help at those bitrates.
   - Regions that contain structure (stars, rails, cable, clouds) are not gradient tests.
   - Thread tested at 1080p only; Bulwark gates the 4K CRF 23 run against the undithered run.
   - Recommend uploading at 1440p or 4K.
5. **Temporal dither:** it produced an 8-frame periodicity in sequence-difference measurements (critique run07 PAC-16). Measure sequences with the static dither, which is Bulwark's default anyway.
6. **Byte-exact HLSL:** `.gitattributes * -text` (a global autocrlf broke `TPal_ShaderGenEqualsPythonGenerator`). `ShaderGen` and `gen_hlsl.py` must agree on F6 formatting, LF line endings and no BOM.
7. **Audio** (R5, D-012, D-021, D-033):
   - Use the Simper SVF; the Chamberlin form diverges above about 8 kHz at 48 kHz.
   - The limiter asymptote is c, not the printed ceiling (0.89 gives −1.01 dBFS).
   - Do the 8 s parameter smoothing in double; a float one-pole stalls short of its target.
   - The device may run at 44.1 kHz (`AudioManager` `m_SampleRate: 0`): compute coefficients from `outputSampleRate` and rebuild on configuration change. Render offline at 48 kHz.
   - Use a heartbeat, `Play()` after `AddComponent`, and `runInBackground`.
   - Unity allocates 4096 B per `OnAudioFilterRead` marshalling call in about 4 % of frames. That allocation is unavoidable, so prove zero game allocations with `GC.CollectionCount` and heap growth.
   - Read every "decay" as T60.
   - The player dump matched the offline render within 0.6 dB when both shared `FootstepBank`.
8. **Batch discipline:**
   - Batch methods return a problem list and exit 1.
   - `MaterialFactory` is idempotent (`CopySerialized`, GUIDs kept).
   - Burst compiler-server pipe errors in batch logs are harmless.
   - `-runTests` runs without `-quit` and with no stall limit.
   - `ProjectConfigurator` sets D3D11, but the **Editor uses it only from its next start**. Check `SystemInfo.graphicsDeviceType` in the `CompileCheck` log.
9. **Runtime contracts:**
   - `IBeatBuilder` types must be in the Runtime assembly (asmref).
   - QA runners are added at runtime (`RuntimeInitializeOnLoadMethod`) when absent from the scene.
   - `QaLog` is installed before scene load whenever a QA flag is present. A FAIL is logged as an error, so a failing run also shows a Player.log error.
   - `PathGuideProbe` silently skips the kill-count and coverage checks unless `PathGuide` exposes `Instance`, `KillCount` and `CoverageCheck`.
10. **Execution order** (keep it consistent when merging with The Ring's walker):

    | Component | Order |
    |---|---|
    | GameBootstrap | −1000 |
    | QualityBootstrap | −900 |
    | FadeController | −800 |
    | CameraRig (Update) and QA runners | −50 |
    | AutoWalker | −40 |
    | PlayerController | −30 |
    | PathGuide | −20 |
    | ProgressDirector | 500 |
    | BeatDirector | 600 |
    | AudioDirector | 700 |
    | CaptureHarness | 32000 |
    | PerfMonitor | 32100 |

11. **Thin lines and fades** (R8, D-042): use `_ScaledScreenParams` and `noperspective` varyings. Use alpha-to-coverage only with MSAA > 1. Fade by A2C, never with an ordered-dither clip (it showed as a screen-door stipple).
12. **Precision** (D-010, D-024):
    - Use exact constants; 70.028 instead of 440/2π made the path jump 1.1 mm.
    - Never subtract two antiderivatives of about 1e7: clamp into the slab first. Thread saw ±10 codes of night-sky noise.
    - Screen-space circular impostors for round bodies (D-046).
13. **Process:**
    - 12 blind parallel builders.
    - A usage-limit interruption (B-001).
    - No third critic round: Definition of Done item 2 was not met.
    - The sequences and the encode test were not re-run on the final build.
    - The docs contradict each other on the soak: `STATE.md` and the final report say it passed, `FEATURE_MATRIX` says it was not run on the final build. Keep Bulwark's docs consistent.
    - `HOW_TO_RUN.md` misnames the folder "3st Megastructures".
    - Thread cost $513.53, 22:06 wall-clock and 7:55 active time.
14. **Frame cap** (Part 2.5): Thread's autotest, wall bot and captures ran uncapped (vSync 0, target −1), and the soak relied on vSync alone. Cap all of them at 60 fps; at that cap the 1.5 m/s autotest becomes a long GPU job (about 17 min).
15. **Footstep audio routing:** Thread's runtime design (synth on the listener, footsteps as AudioSources) conflicts with 3.2 #14, which overrides it.

### 8. Files NOT to copy
- `T:Library/`, `Temp/`, `Logs/`, `Builds/`, `QA/` (all of it, including `final`, `reports`, `audio`, `encode`, critiques), `UserSettings/`, `.git/`, `*.csproj`, `3rd Megastructures.slnx`, `.vsconfig`. `B:` has its own copies of the last three.
- `T:MASTER_PROMPT.md`, `DEVELOPMENT.md`, `STATE.md`, `README.md`, `CLAUDE_FABLE_5.2_FINAL_REPORT.md`, `FEATURE_MATRIX.md`, `HOW_TO_RUN.md`, `PLAY_MAC.command`, `docs/RESEARCH.md`, `docs/research/R1…R8*.md`. Read them as references and cite them; never copy them.
- `T:Packages/manifest.json`, `packages-lock.json` (identical in `B:`), all of `T:ProjectSettings/*` (copying `GraphicsSettings` would bring Thread shader GUIDs and the wrong product), `T:Assets/Settings/*`, `T:Assets/InputSystem_Actions.inputactions`.
- `T:Assets/_Game/Beats/**`: all 23 `.cs` under `Beat01_SaltFlat` … `Beat07_Top`. Only the asmref is copied.
- `T:Assets/_Game/Materials/*.mat` (33 files) and `MaterialLibrary.asset`; `T:Assets/_Game/Scenes/Main.unity`; `T:Assets/_Game/Settings/ThreadLighting.lighting`, `ThreadPalette.asset`. All of these are regenerated.
- `T:SH/ThreadFlat.shader`, `ThreadEmissive.shader`, `ThreadSky.shader`, `SH/Include/ThreadCommon.hlsl`, `ThreadFlatPasses.hlsl`, `ThreadPatterns.hlsl`, `ThreadSkyLib.hlsl`, `SH/Generated/ThreadConstantsGen.hlsl`, `ThreadPalette.hlsl`. Bulwark uses The Ring's lit template and its own atmosphere.
- `T:RT/Sky/Beat00_Sky.cs`, `SkyMath.cs`; `T:RT/Mesh/MeshKit.cs` (take Abyss's richer kit); `T:RT/Core/ThreadConstants.cs`, `ThreadMath.cs` (except the extracted `D3` and easing), `FogMath.cs`, `ThreadPalette.cs` (API pattern only).
- `T:RT/Player/AutoWalker.cs`, `PathGuide.cs`, `PlayerController.cs` (helix-coupled; The Ring's are used, per 3.1), the rest of `PlayerMath.cs`.
- `T:RT/Audio/AudioDirector.cs` and `ThreadAudioParams.cs` (patterns only), and `ThreadDsp.cs` lines 333–1079.
- `T:TS/EditMode/Beat01_02Tests.cs`, `Beat03_04Tests.cs`, `Beat05_07Tests.cs`, `FogTests.cs`, `SkyTests.cs`, `CoreTests.cs`, `PlayerTests.cs` (patterns only).
- Any `.meta` file.

### 9. Import order and gate (Phase 2)
1. Copy the asmdefs and asmref.
2. Core skeletons: `BwMath` (`D3` and easing), `BulwarkConst` and `BulwarkPalette` (minimal), `BwClock` and `GradeState`, `BwServices`, `CliArgs`, `WorldContext`, `MaterialLibrary`, `ShaderIds`.
3. Rendering and shaders: BwGrade with the colour matrix, BlueNoise, R2, then BwLine rewired to the lit template.
4. Systems and the player pieces (CameraRig, BwInput, QualityBootstrap, PlayerTuning), plus The Ring's walker and path.
5. UI: FadeController as the grade-fade bridge, plus The Ring's `UiDirector`.
6. QA (12 files).
7. Audio (`BwDsp` blocks, `BwSynth`, WavWriter, Offline, FootstepBank, `BulwarkScore` stub).
8. Editor tools (10 files).
9. Tests.
10. Tools.

Then run `python -I Tools/gen_hlsl.py`, then `python -I Tools/fastcompile.py all`, then a Unity `CompileCheck`, then `RebuildAll`, then `Validate`, then the EditMode tests, then `BuildWindows`, then `-autotest` at 1.5 and 2.5 m/s on a trivial three-beat graybox through `run-player.ps1` and gpu-guard (Part 3.1 rule 2). Only then build the game on top.

---

## Survey B — Framework import from Abyss and The Ring (Part 3.1 survey, read-only, 2026-11-13)

Surveyor: `survey-abyss-ring`.

Scope: the Abyss and The Ring bullets of MASTER_PROMPT Part 3.1, mapped onto Thread's layout (Thread is the primary base). Paths are relative to each project root. `RT/` = `Assets/_Game/Scripts/Runtime/`. Destination root: `<WORKSPACE>\4 Megastructures\4 Megastructures`. Every claim below was checked against the source files. Line counts come from `wc -l`.

### 1. Import conventions

**Renames.**
- `Ab*`, `Abyss*` and `Ring*` become `Bw*`. `AbyssConst` and `RingConstants` become `BulwarkConst`. `AbyssPalette` and `RingPalette` become `BulwarkPalette`.
- Shader globals `_Ab*` and `_Ring*` become `_Bw*`. Shader names become `Megastructure/Bw*`.
- Log tags `[ABYSS]`, `[ABYSS-ERROR]` and `[Ring]` become `[BW]` and `[BW-FAIL]`, following Thread's `[THREAD]` / `[THREAD-FAIL]`. The log scan keys on these tags.
- Names without a game prefix keep their names: `MeshKit`, `MeshBuilder`, `BackdropRig`, `InstancedBatch`, `DetailTier`, `CompositionLint`, `WarmupLoader`, `FilletPath`, `GazeSchedule`, `WalkPlanner`, `UiDirector`, `UiTimeline`, `AtmosphereDriver`.

**Folder map.** Thread's runtime folders are Audio, Core, Mesh, Player, QA, Rendering, Sky, Systems and UI.

| Source folder | Destination |
|---|---|
| Abyss `RT/Geometry/` | `RT/Mesh/` |
| Abyss `RT/Rendering/` | `RT/Rendering/` |
| Abyss `RT/Core/` | `RT/Core/` (clock merged into `BwClock`); `WarmupLoader` goes to `RT/Systems/` |
| Abyss `RT/QA/` | `RT/QA/` |
| Ring `RT/Player/` | `RT/Player/` |
| Ring `RT/UI/` | `RT/UI/` |
| Ring `RT/Directors/` | `RT/Systems/` (next to Thread's `BeatDirector` and `ProgressDirector`) |
| Ring `RT/Terrain/` and `RT/Rendering/InstancedField*` | `RT/Rendering/` |
| Ring `RT/Core/RingBend.cs` | `RT/Core/` |
| Shaders | `Assets/_Game/Shaders/` and `Shaders/Include/` |

**Namespaces.** Abyss and The Ring use the root `Megastructure` namespace (Abyss audio uses `Megastructure.Audio`). Thread uses sub-namespaces: `Megastructure.Player`, `.QA`, `.Rendering`, `.UI` and `.Audio`, with root `Megastructure` for Core, Mesh and Systems. An imported file takes the namespace of its destination folder and adds `using` lines for the others. Everything stays under `Megastructure`.

**Assembly definitions.**
- Thread's Runtime asmdef references InputSystem, RP Core, URP and UnityEngine.UI. This covers every imported file:
  - `UnsafeUtility` and `UIElements` are engine modules.
  - UnityEngine.UI can be dropped if `GlyphText` is not copied.
- The Editor asmdef needs `Unity.RenderPipelines.GPUDriven.Runtime`. The Ring's first compile failed without it.

### 2. Classes with the same role: which one wins

| Role | Thread | Abyss | The Ring | Winner and wiring |
|---|---|---|---|---|
| Clock | `ThreadClock` (Ambient, Frozen, Stepped, Frame, FreezeDither) plus `GradeState` in the same file; 62 uses in CaptureHarness, ProgressDirector, Grade pass, SoakRunner, FadeController | `SceneClock` (Time, Frozen, RealTime, `Wrapped(period)`) and `ClockDriver` (order -1000, publishes `_AbTime`, `-clock T`, `-freeze-clock`, freezes on pause) | `RingState.T`, `AnimT`, `Dt` | **Thread** becomes `BwClock`. Merge in from Abyss: `RealTime` (never freezes; UI fades), `Wrapped(period)` in double, freeze while paused, publish `_BwTime` (mod 3600 in double), CLI `-clock T`. Make `-freeze-clock` an alias of `-freeze-ambient`. Do not copy `SceneClock` or `ClockDriver`. |
| Shared state | `ThreadServices` (IPlayerState, ICameraControl, IAudioHooks, IFadeUI) and `WorldContext`/`FrameState` (s in double) | `GameState` (D, E, Beat, Lift*, Ride*, Camera*, PlayerPos, mode flags, GoalReached, PictureFade, Seed, HoldH7) | `RingState`, `RingRig`, `RingEvents` | **Thread services are primary.** Add a slim `GameState` (Abyss name, no prefix) only for the plain flags that UI and audio need: S (double), Beat, CameraPos/Rot/MainCamera, PlayerPos, Cinematic, CaptureMode, AutoTest, Paused, Ended, GoalReached, GoalRealTime, PictureFade, Seed. Delete `LiftPhase`, E, Lift, RideSeconds, RideU, RideStarted, HoldH7. Add `BwEvents`, built from `RingEvents`: PauseChanged, LoadComplete, PlayerMoved, GoalReached, CinematicChanged, EndingComplete, BeatEntered. Each field keeps one writer, documented per field (Abyss style). |
| CLI | `CliArgs` (Has, Get, StartS, -capture-set, -brisk, -speed, -wallbot, ...) | `CliArgs` (Parse, Clock, FreezeClock, Bot, Res, Float/Int/String) | `CliArgs` (Str, Capturing, WallTest) | **Thread.** Add `-clock T`, `-freeze-clock`, the `pose:` token (section 3), `-uifont theme\|legacy` (Ring), `-dither static\|temporal\|off`. `-dither off` is needed for the dithered/undithered encode pair of 14.10.3. |
| GameBootstrap | IBeatBuilder discovery by reflection, `WorldContext`, Ready/WorldReady, `Shader.WarmupAllShaders()` (its comment says "WarmupLoader may extend this") | load sequence, `InstancedRegistry.Clear()` | `Restarting`/`CinematicOnLoad`, set pre-warm | **Thread.** Add: `InstancedRegistry.Clear()` at load start; `WarmupLoader` behind the black fade; set pre-warm per 14.6; carry the cinematic flag through Restart (Ring). |
| `IBeatBuilder` | `{ string Name; void Build(WorldContext) }` | `BeatBase` | `{ int Order; string Name; IEnumerator Build(BuildContext) }` (Ring's PathGuide implements it) | **Thread.** Ring code ported must not implement Ring's interface. |
| PlayerController | lane coordinates: state (s, lateral) in double, analytic floor, analytic clamp, redundant capsule check on layer 30 | CharacterController plus kinematic lift | CharacterController, in-code InputActions, safety snap, footsteps | **Conflict.** Part 3.1 says take Ring's (Thread's is "specific to a helix"). Part 14.1 lists Thread's. Part 1.6.1: Part 14 wins. **Recommend Thread's.** It is generic once the `ThreadMath` path API is re-implemented on `FilletPath` (section 6.1). 14.4 asks for a CharacterController (slope 45, step 0.3, radius 0.35): keep one configured that way as the physical check. Each frame `cc.Move(analyticTarget - pos)`; if xz differs by more than 1 cm, count a rejection and project back into (s, lateral); y always comes from the analytic floor. Log this in DEVELOPMENT.md. From Ring take the 0.2 m camera probe, the Alt+Enter double-toggle guard and the safety snap to the last safe point at the current s. |
| CameraRig | raw mouse, stick rate, no roll, SetLook, SetPoseOverride | | 0.2 m `CameraProbe` sphere, `Physics.ComputePenetration` push-out, `Violations`/`MaxRollDeg` counters | **Thread**, plus Ring's probe and QA counters (14.4 asks for the 0.2 m probe). |
| PathGuide | colliders generated from `HalfWidth(s)` per segment, kill volume, `CoverageCheck` ray grid, `CapsuleBlockedAt` | `CorridorKit` helpers (Box, Floor, Ramp, Wall, WallPair, SafetyVolume) | hand-listed `Shapes` lane table, `DistanceOutsideLanes`, `LaneHalfWidth`, `LaneSamplePoints`, fallback floors 3 cm under decks | **Thread's generator, driven by one Bulwark lane function.** Lesson 3.2 #11 bans hand-listed span tables (58 misses). Use Ring's conventions, which match 14.4: 1 m thick, 6 m high, 1 m overlap, boxes every 4 m. Keep Ring's API names and add `FloorY(s)`. Use `CorridorKit` for ramp colliders on stairs and gallery. Ray grid 0.5 m along s and across the lane. |
| AutoWalker | shell: C toggle, cancel on input with 0.3 s grace, test mode, ElapsedVirtual, `SetDrive`; plus `CinematicPlan` slowdowns | ProfileSpeed zones, hold H7 | `WalkPlanner` (pure) and `GazeSchedule` (s keys, `LateralOffset`) | **Thread shell, Ring's planner and gaze core.** See section 5. |
| MeshKit / MeshBuilder | `Mesh/MeshKit.cs` (both classes) | richest kit, 4 files | `Core/MeshKit.cs` | **Abyss.** Do not copy Thread's `Mesh/MeshKit.cs`: same class names, so it would not compile. Do not copy Ring's. Add `MeshKit.TriangleCount(Mesh)` (Thread's `WorldContext.AddMesh` calls it). Thread's framework uses its MeshKit only in WorldContext and the beats. |
| Instanced draws | none in the framework (only `Beats/Beat05/InstancedSet`) | `InstancedBatch` (CPU matrices, `RenderMeshInstanced`, chunks of 511) and registry | `InstancedField` (`RenderMeshPrimitives`/`RenderMeshIndirect`, GPU placement) | **Keep both, with one capture rule.** Both registries expose `SubmitAllFor(cam)`. In capture mode the drivers do not submit; `CaptureHarness` calls both right before each `SubmitRenderRequest` (lesson 3.2 #8). |
| Text, fade, pause, ending | `FadeController` (fade inside the grade through `GradeState.FadeBlack`), `GlyphText` (UGUI glyph per letter), `PauseOverlay` | UiViews | `UiDirector` and `UiTimeline` (UI Toolkit) | **Ring** (14.11 names it). Do not copy Thread's three UI files. `UiDirector` implements `IFadeUI` and writes `GradeState.FadeBlack`; set `ShaderFade = true` so the fade happens before the one dither. Pause has one owner (UiDirector), exposed through `IFadeUI.Paused`. |
| Atmosphere and globals writer | `ProgressDirector.Apply()` writes about 20 globals, the sun and `GradeState` | | `AtmosphereDriver` (one writer, slew limiters) | **Ring's `AtmosphereDriver`.** Thread's ProgressDirector keeps s and `IWorldUpdater`. Its `Apply()` calls `AtmosphereDriver.Tick(s, dt, bypass)` so `CaptureHarness` (which calls `Apply()` after setting a pose) gets the shot's uniforms. Delete Thread's helix uniform writes. |
| Post | `ThreadGrade*` | `FinalDither.shader` | `RingFinalDither`, URP Volume overrides | **Thread `BwGrade`** (rule 3.1.6). `AtmosphereDriver.ApplyVolume` is rewritten to write `GradeState` (EV, WB matrix, saturation, contrast, bloom, vignette, dither peak). No Volume overrides. |
| Material library | `MaterialLibrary` with named fields | list plus `AllVariants` | `RingAssets.Get(id)` | **Thread.** Add an `All()` enumerator for `WarmupLoader`. `InstancedField.Create` takes a `Material`, not an id. |
| QualityBootstrap | `Player/QualityBootstrap.cs` | yes | yes (`ToggleFullscreen`) | **Thread.** |
| Maths | `ThreadMath` (double; path API is helix-specific) | `VertexBake.Pcg` | `RingMath` (Slew, CriticalDamp(omega), CriticalDampAngle, Pcg, U01, ValueNoise, Smax) | `BwMath`: Thread's generic part plus the RingMath helpers. The path API moves to `BwPath` (section 6.1). |
| Collider layers | `PlayerTuning.CorridorLayer = 30` | `CorridorKit.LayerCorridor = 8`, `LayerSafety = 9` | | **Thread 30.** Point `CorridorKit` at it; Safety = 31. |
| Fast compile | `Tools/fastcompile.py` (separate Runtime/Editor/Tests projects, Unity's bundled dotnet) | `fastcompile.ps1` with `FC_RUN` per run | `Tools/fastcheck.py` (honours asmdef references, `--only`) | Thread base, Ring features merged (section 4). |
| Encode test | `Tools/qa/encode_test.py` (`accurate_rnd+full_chroma_int`, BT.709 tv) | the same plus `+bitexact`, the step-edge band rule, worst crops | `Tools/encode_test.py` (default swscale, wrong) | Thread base, Abyss merges (section 3). Never use the Ring version. |
| Key-shot table | `ShotPlan` (keyshots, beats, sequences, encode) | `KeyShots` (Abyss table, `pose:` token) | `CapturePlans` | **Thread's ShotPlan**, plus the Abyss `pose:` token. |
| Wall bot and ray grid | `WallBot` (lane coordinates), `PathGuide.CoverageCheck` | `QaBots` (wallhug, raygrid, ceiling and side rays, per-point reports) | `WallTest` (never run in the player) | **Thread**, plus Abyss raygrid reporting. |

### 3. Abyss files

| Abyss source (lines) | Bulwark destination | Depends on | Strip or change |
|---|---|---|---|
| `RT/Geometry/MeshBuilder.cs` (181) | `RT/Mesh/MeshBuilder.cs` | none | Keep as is: R = AO and G = wear convention, UV1/UV2, Push/Pop matrix, 32-bit indices above 65 535 vertices, exact bounds. |
| `RT/Geometry/MeshKit.cs` (630) | `RT/Mesh/MeshKit.cs` | MeshBuilder, VertexBake, MeshValidation | Keep CrossSection, LoftFrame, ChamferBox (44 triangles), ChamferMember, HexBolt, GridQuad, Disc, Annulus, Sphere, Torus, BakeAO/EdgeWear wrappers, oriented `Tri`, degenerate guard. Comment: the UV1/UV2 "ThinLine" data was for Abyss's `AbThinLineExpand`. `BwLine` (= ThreadLine `_LINE_TUBE`) pushes along smooth clip-space normals instead. So either build rails with smooth `Cylinder` tubes (thinLine off) or teach `BwLine` the UV2 axis push. Add `TriangleCount(Mesh)`. |
| `RT/Geometry/MeshKit.Loft.cs` (567) | `RT/Mesh/MeshKit.Loft.cs` | MeshBuilder | Keep: Loft, LoftVarying, PathFrames, Extrude, IBeam, Helix/HelixTube/HelixRibbon, RoundedRectangle (gallery loft, r 0.3), Rectangle, Circle, IBeamSection, Triangulate. Airfoil (fan blade) is optional. |
| `RT/Geometry/MeshKit.Revolve.cs` (343) | `RT/Mesh/MeshKit.Revolve.cs` | MeshBuilder | Keep Lathe (lantern, lamp posts), Cylinder, TaperedCylinder. `BandedColumnProfile` is Abyss-flavoured but harmless. |
| `RT/Geometry/MeshKit.Structures.cs` (437) | `RT/Mesh/MeshKit.Structures.cs` | MeshBuilder | **Strip** the honeycomb and shaft generators: HexCorner, HexFaceNormal, HexApothemOf, HexWallPanel, HexAnnulus, MitredRing, HexTube, and the `AbyssConst.SHAFT_FACE_NORMAL_DEG` comments. Keep WarrenTruss with gussets; HelicalAnnulus is optional. |
| `RT/Geometry/MeshValidation.cs` (216) | `RT/Mesh/MeshValidation.cs` | MeshBuilder | Comment says `BeatBase.CollectMeshes`. Call it from T-MESH and from Thread's `Validate` over every mesh passed to `WorldContext.AddMesh` (validate before `UploadMeshData(true)`). |
| `RT/Geometry/VertexBake.cs` (183) | `RT/Mesh/VertexBake.cs` | MeshBuilder | Seed comes from `WorldContext.Seed` instead of `GameState.Seed`. |
| `RT/Geometry/CorridorKit.cs` (109) | `RT/Player/CorridorKit.cs` | none | Layers 8/9 become `PlayerTuning.CorridorLayer` (30) and Safety (31). Defaults change to 14.4: `WallThickness` 2 to **1** m, `WallHeight` 4 to **6** m. Used by `PathGuide` for the stair and gallery ramp colliders. |
| `RT/Rendering/BackdropRig.cs` (65) | `RT/Rendering/BackdropRig.cs` (namespace `.Rendering`) | GameState.MainCamera/CameraPos | Read the camera from `BwServices.CameraControl.Camera`. Order 150 is still after Thread's `CameraRig` (-50). **Add a static list and `FollowAll(pos)`**; `CaptureHarness` must call it after `SetPoseOverride` and before rendering, or the backdrop follows the previous pose. Use it for `BwFarField` (exact follow) and the far gate towers. |
| `RT/Rendering/InstancedBatch.cs` (233) | `RT/Rendering/InstancedBatch.cs` | ShaderIds.AbFade, InstancedRegistryDriver | `_AbFade` becomes `_BwFade` (`ShaderIds.BwFade` in Thread's ShaderIds). Log tags. Add `Submit(Camera)` (sets `RenderParams.camera`). Keep: 511 per draw, spatial chunks, explicit bounds inflated 10 %, shadows set explicitly, probes off. **Shaders drawn this way need `#pragma multi_compile_instancing` and the instance-ID macros.** The Ring lit template has neither (section 4). |
| `RT/Rendering/InstancedRegistry.cs` (61) and `InstancedRegistryDriver.cs` (32) | `RT/Rendering/` | InstancedBatch, CameraRig, GameBootstrap | Add `SubmitAllFor(cam)`. The driver (LateUpdate +200) does not submit in capture mode. `GameBootstrap` calls `Clear()` at the start of each load. |
| `RT/Rendering/DetailTier.cs` (139) | `RT/Rendering/DetailTier.cs` | InstancedBatch, ShaderIds.AbFade, `QaHooks.TierOverride` | Move `TierOverride` into a small static (e.g. `QaCommon.TierOverride`). Shader side: replace the static IGN complement pattern with the blue-noise tile or alpha-to-coverage under MSAA. Lesson 3.2 #6 (IGN repeats every 13 rows) and Thread D-042 (ordered-dither stipple). Use for units-to-horns at 400 m (T7: dE at most 2.0). A property block on a renderer breaks SRP batching only while fading. |
| `RT/Core/GameState.cs` (71) | `RT/Core/GameState.cs`, slimmed | everyone | See section 2. |
| `RT/Core/SceneClock.cs` (28) and `ClockDriver.cs` (116) | not copied; features merged into `RT/Core/BwClock.cs` | CliArgs, ShaderIds.AbTime, GameState.Paused | See section 2. |
| `RT/Core/WarmupLoader.cs` (259) | `RT/Systems/WarmupLoader.cs` | MaterialLibrary (`materials`, `AllVariants`), InstancedRegistry, `AbyssConst.YawPitch`, `GetEntityId()` | Use Thread `MaterialLibrary.All()`. Replace `YawPitch(205, -72)` with `Quaternion.Euler(72, 205, 0)`. **Add a third light state:** sun off plus one temporary point light in front of the camera, so the Forward+ additional-light variants compile (beats 1 and 5 have no sun and use point lights). Called by `GameBootstrap` behind black. This is separate from the 14.6 set pre-warm at key poses; both are needed. |
| `RT/QA/CompositionLint.cs` (390) | `RT/QA/CompositionLint.cs` (`.QA`) | BeatDirector.Instance.Beats, BeatBase.CollectSubjects, KeyShots.CollectQaSubjects, KeyShotSubject (defined in KeyShots.cs), Abyss QaJson (Obj, Arr, PropRect, Inline) | Move `KeyShotSubject {Name, WorldBounds, MinScreenFraction}` into this file. Subjects come from an `IKeyShotSubjects` interface on Bw beat builders, registered through `WorldContext`. Port `Write()` to Thread's QaJson (Key/BeginObject/BeginArray/Prop). Empty `LoggedConflicts`. Keep: in-frame centre rule for oversize subjects, 3 % margin, depth occlusion (8 x 8 rays, at least 30 % visible, tolerance 1 % + 5 cm). **It needs a depth buffer:** port `QaDepthGrabPass` (Abyss `RT/QA/QaRenderer.cs`, about lines 14-45 and 262-300; a Render Graph pass that copies the resolved camera depth into R32F, linearised afterwards) into Thread's `CaptureHarness`. Complements Thread's `QaProjection` (double-precision framing at three aspects). This is the base of the Part 11.13 audit. |
| `RT/QA/KeyShots.cs` (996) | not copied (77 `AbyssConst` uses); port only the `TryPose` parser into Thread `ShotPlan` | PathGuide.Project, AbyssConst | Token `pose:NAME:x:y:z:yaw:pitch:fov`: s = projection of the feet onto `FilletPath`, clamped to at most 1497 (the goal trigger 1498.5 never fires); lateral from the eye; frozen clock; kind "custom". Per 14.10.2 a key shot must use the schedule vFOV; the free fov is for look-dev only. |
| `RT/QA/QaSequences.cs` (866) | not copied (partial class of Abyss's `QaCaptureRunner`) | | Port the concepts into Thread `ShotPlan.SeqMode`/`SequenceSpec`: `WalkPinned` (the player walks so s events fire while the camera holds a pose: door at 1.5 and 2.5 m/s, H6 whale), `MovingPose`, `Steps` (thin-line sideways steps; Thread `PanYaw` covers the 0.1 px pan of H7), and `CueRef`/`CueMarks` (frames at seconds after an event). |
| `RT/QA/QaBots.cs` (709) | not copied; merge into Thread `WallBot` and `PathGuide.CoverageCheck` | QaHarness, CorridorKit, Abyss PathGuide | Bring over: per-point failure lists (world position, s, offset, expected and hit floor y, collider name); upward ceiling rays in gallery, stairs and tunnel (soffit clearance 0.62 m, 14.4); sideways wall-gap rays; stall and narrowing logic for the door, the funnel at s 898-920 and the 2.9 m stairs lane. Bulwark bot: 0.40 m inside both edges, lane coordinates. |
| `RT/QA/QaDrivers.cs` (272) | not copied | `IPlayerDriver`/`PlayerInputFrame` (Abyss only), GazeCues | `QaHoldDriver` becomes `PlayerController.Frozen` plus `SetPoseOverride`. `QaPathDriver` becomes the Bw AutoWalker test mode at any speed with a look-back option (needed for the 14.6 look-back at s 1034, yaw 183). `QaWallHugDriver` becomes Thread `WallBot`. |
| `RT/Player/LiftController.cs` (393) | not copied (beat-5 lift content) | AbyssConst, GameState, PlayerController | **Pattern only**, for a `BwDoor` owned by the beat-1 builder and for H6 helpers: trigger-started double clock; analytic position `gap(t) = 4.0 smootherstep(t / 3.0)` (trigger s 104); `SetForCapture(s)`; exact compare then `Physics.SyncTransforms()` when the leaves move; the collider switch at gap 0.5 m (leaves off, jambs at x -3.5 / 0.5 on); `Frozen` for stills. |
| `Shaders/BeamWedge.shader` (366) | `Shaders/BwBeam.shader` (plus `ShaderMeshes.BeamWedgeMesh` from Abyss `RT/Rendering/ProceduralTextures.cs` lines 288-333, into `RT/Mesh/ShaderMeshes.cs`) | AbyssCommon.hlsl (AbSmootherstep, AbSafeNormalize, AbPixelAngle, AbViewRay), AbyssFog.hlsl (AbGetFogSet, AbyssFogAdditive, AbFogSkySet), `_AbBeamFade`, `_AbFanTheta`, `_AbFade`, `_AbDomain` | Keep recipe 0 (analytic convex prism built in the vertex shader from UV1, intersected per pixel with 6 planes, clamped to near plane and scene depth, 12 Gauss-Legendre samples, no jitter, at least 12 px feathers, `Cull Off`, winding-independent) and recipe 2 (rectangular prism). **Strip** recipe 1 (fan wall patches) and recipe 3 (`ReverseSubtract` key-shadow lobe tied to Abyss fog). Fog becomes `BwFogApply` transmittance; the pixel angle becomes `_BwCam.w`. Fits the H3 shafts (12 fixed samples over at most 60 m). It reads the resolved `_CameraDepthTexture`, so the MSAA-halo caveat of H2 applies at thin foreground edges. |
| `Tools/qa/encode_test.py` (504; band rule in `Tools/qa/abyss_qa.py` `encode_band_runs`, 1907 lines, game-specific) | merge into Thread's `Tools/qa/encode_test.py` (uses `qa_common.py`) | ffmpeg, numpy, Pillow | Thread already has `accurate_rnd+full_chroma_int`, BT.709 tv in, `out_range=pc` out, colour tags. Add from Abyss: `+bitexact` in the swscale flags, the encoder-availability check, the worst-location crops at 100 %, 200 % and x8 contrast, and the step-edge band rule (a run counts only if it ends in a step of 2 or more levels absent from the source, or is longer than max(24 px, 1.5 / local slope over +/-48 px)) as information. Add what 14.10.3 needs and neither has: the same 10 s **with and without dither** (`-dither off`), pass if the dithered longest run is at most 0.6 x the undithered one; CRF 23 and 30. |
| `Tools/lookdev.ps1` (40) | `Tools/lookdev.ps1` | unity-queue.ps1, image_metrics.py, contact_sheet.py | `Abyss.exe` becomes `Bulwark.exe`; `ABYSS_BUILD_DIR` becomes `BW_BUILD_DIR`. **Thread's `BatchTools.PlayerExePath()` has no output-directory override:** add `BW_BUILD_DIR` (lesson 3.2 #10). Launch the player through `Tools/run-player.ps1`, not a bare `Start-Process`, so long or 4K jobs go through `gpu-guard.ps1` (Part 2.5). `-Extra` arrives as one comma-joined string under `-File`: split it (lesson 3.2 #1; Thread's run-player already does). `-Shots` becomes Thread's `-capture-set` and `-capture-only`. |

### 4. The Ring files

| Ring source (lines) | Bulwark destination | Depends on | Strip or change |
|---|---|---|---|
| `RT/Player/FilletPath.cs` (232) | `RT/Player/FilletPath.cs` (`.Player`) | the convenience constructor uses RingConstants.Waypoints and RingWorld.Cumulative; generic constructor `(Vector2[] w, float[] cum, float[] radius)` has none | **Delete** the 3-radius convenience constructor and `CornerRadii`. Keep the generic constructor (tangent length t = r tan(theta/2), clamped to half the adjacent leg). **Bulwark's s is the filleted distance itself** (Ring's d), so `SharpS`/`DAtSharp` are unused. Pass `cum` = sharp cumulative lengths and expose the piece start distance. **Port to double**: Thread's (s, lateral) state is double; float overloads for meshes. Build the waypoints from the 14.4 table (corners A1 r 40, A2 r 40, A3 r 30, A4 r 20; none of them is clamped). T-PATH: length 1500.000 +-0.01; piece boundaries 934 / 936.094 / 1033.894 / 1035.388 / 1370 / 1379.875 / 1430.038 / 1436.321 / 1500; end (-9.199, 1497.114). Keep `PointAt`, `TangentAt`, `RightAt` (+X when walking +Z), `HeadingAt` (0 = +Z), `Project(xz, hint, window)`, `Lateral`. |
| `RT/Player/PathGuide.cs` (438) | merged into Thread's `RT/Player/PathGuide.cs` | RingConstants lane numbers, RingMath, BuildContext, Ring IBeatBuilder | Do not copy the `Shapes` table, `BridgeY`, `RampY`, `WalkHeight`, `SmoothFloor`, `ChamferDist` or `SwapRevertS`. Keep the API idea: `HalfWidth(s)` (Bulwark's single lane function: corridor -3.15..0.15, road -5.4..+5.1, funnel s 898-920, door, stairs 2.9, exit hall 3.7, gallery axis -5.0..+2.65, tunnel 2.5, ledge -11.8..-6.4); `FloorY(s)`; `CentreAt(s)`; `DistanceOutsideLanes`; `LaneSamplePoints(0.5)`; fallback floors 3 cm under the visible deck. |
| `RT/Player/PlayerController.cs` (331) | not copied as a controller (Thread's wins, section 2) | RingRig, RingState, RingEvents, RingConstants, RingMath, PathGuide, ProgressDirector, CameraRig, QualityBootstrap, AutoWalker, GameBootstrap | Merge into Thread's controller: the CharacterController settings (radius 0.35, slope 45, step 0.3, `minMoveDistance` 0), the cancel rule (mouse over 2 px per frame, stick over 0.3, move over 0.2, Shift press), the Alt+Enter guard, the safety snap with a warning log. |
| `RT/Player/AutoWalker.cs` (191) | not copied; Thread's AutoWalker shell is used | WalkPlanner, RingRig, RingState, RingEvents, PathGuide.WalkHeight, ProgressDirector.SnapTo, CliArgs.Str("speed") | Its `Activate()` gives the cinematic `cruise = RingConstants.Walk` (1.5): that is the bug class of 3.1(c). Ring's `QaHooks` line 326 also starts the cinematic at `RingConstants.Walk`. Do not port the reflection glue in `RingQaHooks`. |
| `RT/Player/WalkPlanner.cs` (297) | `RT/Player/WalkPlanner.cs`, trimmed and pure | FilletPath, GazeSchedule, RingMath (Pcg, U01, Smootherstep01, CriticalDamp, CriticalDampAngle, ValueNoise), RingConstants.Accel | See section 5. Integration of s moves into Thread's `PlayerController`; the planner outputs the speed command, the lateral target and the look. |
| `RT/Player/GazeSchedule.cs` (225) | `RT/Player/GazeSchedule.cs` | RingMath, RingConstants (StarOutYaw, CinematicHolds), AutoWalker.Path | See section 5. Key data goes to `BulwarkConst` (or one table per beat folder) with comments quoting the 14.5 gaze line. |
| `RT/UI/UiDirector.cs` (1030) | `RT/UI/UiDirector.cs` (`.UI`) | RingEvents (22 uses), RingState (16), RingRig (10), CliArgs, UiTimeline, RingConstants, PlayerController, RingAssets.font, RingPalette.HexToSrgb/Entries, RingMath, AudioDirector (`FadeAudio` bound by reflection), InputSystem, SceneManager | Uses `BwEvents`, `GameState` and `IFadeUI`. 14.11 values: "BULWARK"; 56 px; letter-spacing 0.22 em as px (12.32 px at 56 px; USS accepts only px or %); `TitleText` #EDEBE5 at alpha 0.88; title centre 38 % from the top; hint at the bottom 6 %, alpha 0.7, text "WASD  move    Mouse  look    Shift  faster    C  cinematic"; end line 51 px "Someone held the sea back, so the land could stay dry."; Scale With Screen Size 1920 x 1080, match height. **Font mode:** Ring defaults to Legacy; 14.11 wants the theme default font with `LegacyRuntime.ttf` via `FontDefinition.FromFont` as the fallback. Default to Theme; Ring's blank-measure switch-once logic already provides the fallback. Hint never shows in cinematic or `-autotest`. `ShaderFade = true`. Bind audio directly, not by reflection. **Editor companion required:** Ring `Scripts/Editor/Batch/ProjectSetup.cs` `CreateOrUpdateUi()` (theme `.tss` = `@import url("unity-theme://default");`, PanelSettings asset, `m_DefaultShader` check) and `UpdateAlwaysIncludedShaders()` (`UiRuntimeShaders`: Internal-UIRDefault, UIRAtlasBlitCopy, UIR/GaussianBlur, UIR/ColorEffect, UIR/DropShadowComposite, TextCore/Distance Field SSD, Internal-GUITextureClipText, TextCore/Sprite, Internal-UIE-ColorConversionBlit). Port these into Thread's `ProjectConfigurator`; the scene builder adds the `UIDocument`. |
| `RT/UI/UiTimeline.cs` (58) | `RT/UI/UiTimeline.cs` | RingConstants, RingMath | Pure functions; keep. Constants to BulwarkConst 14.11: picture 1.0 to 3.5 s smoothstep; title in 1.4 to 2.6, hold to 4.6, out to 6.0; hint at 6.5 s for 6 s with 0.6 s fades (`HintKill` 0.35 s, within the 0.4 s rule). The ending numbers are already identical (3.0 dwell, 4.0 smootherstep fade, 1.5 black, 2 + 6 + 2 line = 18.5 s). |
| `Shaders/Include/RingLitCore.hlsl` (64) | `Shaders/Include/BwLitCore.hlsl` | RingBend.hlsl, RingAtmosphere.hlsl, RingGround.hlsl | **Strip the bend.** Bulwark is flat, so theta = 0 and `positionFlatWS == positionWS`. Remove `RingPerturbNormal`'s bend rotate and the `RingGround` include. Include `BulwarkAtmosphere.hlsl`, `BulwarkPalette.hlsl` and a `BwCommon.hlsl`. Keep the hook contract: `SurfaceHook` always; `VERTEX_HOOK`, `CLIP_HOOK` and `INDIRECT` optional; `_ENVIRONMENTREFLECTIONS_OFF` and `_SCREENSPACEREFLECTIONS_OFF` defined before the includes. |
| `Shaders/Include/RingLitPass.hlsl` (186) | `Shaders/Include/BwLitPass.hlsl` | Lighting.hlsl, Shadows.hlsl, `_RingCam` | Four passes (ForwardLit, ShadowCaster with punctual or directional bias and clamping, DepthOnly, DepthNormals) share one vertex path. Remove the bend. `_RingCam` becomes `_BwCam` (xyz eye, w pixel angle) for the footprint. `RingFog` becomes `BwFogApply(c, posWS)`. **Add instancing:** `UNITY_VERTEX_INPUT_INSTANCE_ID` in the attributes, `UNITY_SETUP_INSTANCE_ID` in the vertex shader, `#pragma multi_compile_instancing` in every pass. Without them `InstancedBatch` (`RenderMeshInstanced`) cannot draw lit materials. `RING_INDIRECT` draws stay without instancing (InstancedField contract). |
| `Shaders/Include/RingLighting.hlsl` (80) | `Shaders/Include/BwLighting.hlsl` | `_AmbSky/_AmbEquator/_AmbGround`, RingSkyColor, RingFogHeadingColor, RingKeyAtten, `_RingSet` | Mirrors `UniversalFragmentPBR`: `EnvironmentBRDF` with our ambient and reflection; 6-argument `LightingPhysicallyBased`; `GetMainLight(shadowCoord, positionWS, shadowMask)`; extra directional loop over `URP_FP_DIRECTIONAL_LIGHTS_COUNT` under `USE_CLUSTER_LIGHT_LOOP`; `LIGHT_LOOP_BEGIN` with a local `inputData`. Changes: drop all bend rotations; ambient becomes trilight `_BwAmbSky/_BwAmbMid/_BwAmbGround` (global names must not collide with material properties, lesson 3.2 #5); reflection becomes `BwSkyColor(reflect)` blended toward the fog colour by roughness; `RingKeyAtten` becomes `BwSunBreak(posWS)` (H2: T_sun = 1 - 0.85 (1 - hole(P'))); **delete** the `_RingSet.x > 1.5` tunnel key mask. Keep a NaN-safe clamp. |
| `Shaders/RingCeramic.shader` (126) | `Shaders/BwConcrete.shader` | RingLitCore, RingCommon (`RingFilteredGrid1`, `RingValueNoise`) | Becomes the BwConcrete recipe of 14.2.6: ConcreteMid/Dark/Wet by `_Wet`; triplanar formwork panels 3.0 x 1.5 m with 12 mm gaps; tie holes on 0.6 m; filtered fade below 4 px; macro tone +-6 %; vertex R = AO, G = wear. Copy the two helpers into `BwCommon.hlsl`. Keep `_CLUSTER_LIGHT_LOOP` declared, `_SHADOWS_SOFT` only, one `UnityPerMaterial` cbuffer identical in all passes. |
| `RT/Directors/AtmosphereDriver.cs` (408) | `RT/Systems/AtmosphereDriver.cs` | RingRig.RegisterDirector, RingState (S, Dt, T, AnimT, Capture, SequenceMode, SunsetU, ArkCue, Set, Gust), RingSchedule/RingParams, RingConstants (EvSlew, WbSlew, BloomSlew, LightCal, NearPlane, Star*, WallX, FogSigmaDay, SunsetS0), RingMath.Slew, RingShadowParams, ArkPhase, BeatDirector.IsOn, ProgressDirector.SunsetLead | **Keep:** one writer of globals, lights and camera; `RenderSettings.fog = false`; `RenderSettings.sun = key`; per-light bias with `usePipelineSettings = false`; `_BwCam` per camera in `beginCameraRendering` (capture cameras too); slew limiters EV 0.30/s, WB 6/s, bloom 0.1/s, vFOV 1 deg/s; bypass for stills, teleports (`JumpS` 20 m) and the first frame; slew kept in sequences; `LogGraphics` (API, reversed Z, depth format). **Strip:** arch and rim fills, ship and cloud shadows, ark, swarm, star directions, sunset lead and `BlendTowardSunset`, `_RingSet`, `_Lamps`, `_Wind`. **Change:** `RingSchedule` becomes `BulwarkSchedule`, pure in s (14.2.3-14.2.5, Rule P 30 m crossfades; sun az/el, u_sun, sigma0, sigma_s, sky state, sea Hs/Tp, far plane 12000/150/300/600 switching at s 100, 934, 1030, 1373); `ApplyVolume` becomes `GradeState` writes; shadows constant (4 cascades, 4096, casters within 150 m), so no shadow-distance slew. Ordering: called from `ProgressDirector.Apply()` (section 2) instead of the Ring director chain. |
| `RT/Terrain/TerrainIndirectField.cs` (41) plus `RT/Rendering/InstancedField.cs` (246), `InstancedFieldDriver.cs` (50), `InstancedFieldHandle.cs` (19) | `RT/Rendering/` (the "horn field" of 14.1) | BuildContext, RingSet, RingAssets ids, RingBend.InflateBounds, RingState.Capture, `IRingDrawable` | TerrainIndirectField is only a per-set wrapper over InstancedField; all the logic is in InstancedField. Bounds: `RingBend.InflateBounds` becomes plain explicit `worldBounds`. Material instead of id. Holder under the `WorldContext` root. Visibility toggled by Thread's `BeatDirector` instead of `IRingDrawable`. Keep: `new RenderParams(material)` (never `default`: rendering layer mask 0); indirect args with `IndexedArgs`; `UnityIndirect.cginc` with `InitIndirectDrawArgs(0)` in every pass; per-camera submission in capture mode; release on destroy; `ResetStatics`. |
| `RT/Core/RingBend.cs` (75) | `RT/Core/BwCurvature.cs` (C#) and an HLSL twin in `BulwarkAtmosphere.hlsl` | RingConstants.RingR | Do not copy `Bend`, `Rotate` or `InflateBounds` (no bent geometry). Keep the method: the stable form `2R sin^2(t/2)` instead of `R(1 - cos t)`, a float32 replica next to a double reference, tests that compare them. Bulwark uses: drop `2R sin^2(d / 2R)`; H4 dip `sin(delta)^2 = h(2R + h)/(R + h)^2`; factored discriminant `(R + h)^2(-dy - sin delta)(-dy + sin delta)`. T-NUM: dips 2.486 / 2.467 deg; discriminant relative error at most 1e-5 against double. |
| `docs/BUILDER_BRIEF.md` (53), `docs/CRITIC_BRIEF.md` (38) | `docs/BUILDER_BRIEF.md`, `docs/CRITIC_BRIEF.md` | | Templates. Change: root path; files read-only for builders (`Shaders/Include/**`, `RT/Core/**`, `RT/Systems/**`, `RT/Player/**`, editor tools, scenes, ProjectSettings, BulwarkConst); fast check command; build and capture through `unity-queue.ps1` and `run-player.ps1` (`-capture`, `-capture-set`, `-capture-only`, gpu-guard for long or 4K jobs); one build folder per owner (`BW_BUILD_DIR`); about 6-8 build and capture cycles; critic output JSON (lens, run, findings with S1/S2/S3, `beat_verdicts`). Add Part 11.13 sight-line evidence and the "look at your own captures" rule (Part 5.8). |
| `Tools/fastcheck.py` (189) | merged into Thread's `Tools/fastcompile.py` (132) | dotnet SDK, generated `Assembly-CSharp-Editor.csproj` | Thread base: separate Runtime/Editor/Tests projects with `ProjectReference` (catches Runtime calling Editor), Unity's bundled `DotNetSdk\dotnet.exe`. Add from Ring: package DLLs filtered by the asmdef `references` (would have caught the missing GPUDriven reference) and `--only` folders. Add (lesson 3.2 #3): **per-run output folders** (`FC_RUN` or pid suffix; Ring's single `Temp/fastcheck/<kind>` is not safe in parallel) and a `mscorlib` shim note. Shaders and asmdefs stay unchecked: always finish with a Unity compile. |

### 5. The cinematic walker (Part 3.1 a-f, Part 9, 14.1, 14.5 "Common", 14.12)

**5.1 Composition.**
- **Thread `AutoWalker` shell:** C toggle, `-cinematic` pending start after `GameBootstrap.Ready`, cancel on any input after a 0.3 s grace (also after unpause), test mode `StartTest`, `ElapsedVirtual`, `PlayerController.SetDrive(active, speed, lateral)`.
- **Ring `WalkPlanner`, trimmed:** speed command, lateral target, gaze follower, sway.
- **Ring `GazeSchedule`:** world keys and the lateral table.
- **Thread `PlayerController`:** integrates s and lateral in double. On the goal trigger it runs its existing `StopRamp`.
- Part 9 says the same walker serves the tests: `-autotest` runs it at 1.5 m/s; `-autotest -cinematic` at 2.5 m/s. Same seed, same gaze and lateral keys. Both run from s 0 and cannot be cancelled.

**5.2 Speed.**
1. **Cinematic speed.** `v_cmd = BulwarkConst.Brisk` (2.5), one constant shared with Shift. Start ramp `v = v0 + (2.5 - v0) * smootherstep(t / 0.5)`, with v0 the player's current progress speed (Ring's `StartAccel` form). A 0.5 s ramp from rest loses exactly 0.25 s.
2. **Progress integration.** `ds/dt = v_cmd` exactly in drive mode. Set `DriveNoSmoothing = true` for the cinematic too: Thread's 0.2 s critically damped acceleration would add about 0.2 s more loss. In drive mode do **not** multiply by Thread's `ForwardToS(s, lat)`: speed is measured along the path (Part 9).
3. **No slowing.** No zones, holds, dwell or braking until the goal trigger.
4. **Goal stop.** At s >= 1498.5, Thread `StopRamp` (smoothstep deceleration) over the remaining 1.5 m: T = 2 x 1.5 / v0 = 1.2 s at 2.5 m/s, rest at s 1500 (1.9 m short of the lantern). The ending timeline starts at the trigger.
5. **Expected timings.** At 2.5 m/s: trigger at 599.65 s (1498.5 / 2.5 + 0.25), ending done at about 618.15 s, as in 14.10.4. At 1.5 m/s with the same 0.5 s ramp: 999.25 s. The spec's 999.15 implies a 0.15 s start loss. Both are inside +-4 s; record which ramp the 1.5 m/s walk uses.
6. **Brisk flag** (3.1 e). Thread `PlayerController` sets `m_brisk = m_driveSpeed > 2.0`, which flips mid-ramp. Instead, set brisk from the mode (cinematic means brisk from frame 1) and interpolate the stride length continuously from speed (0.75 m at 1.5, 0.85 m at 2.5). Footstep cadence by distance.
7. **No catch-up or wait logic.** Ring's `MaxLag` wait (0.6 m) and its 1.5 x catch-up step are not needed with the analytic controller. A capsule rejection during the cinematic is a test failure, not a pause.

**5.3 Gaze.**
- Keys are world (s, yaw, pitch) from each 14.5.x "Gaze" line. Example, beat 1: (0,0,0) (44,0,1) (60,60,4) (72,60,4) (88,0,1) (104,0,2) (120,0,0). Smootherstep between consecutive keys. Yaw interpolates by the shortest angle (`Mathf.DeltaAngle`).
- A hold is two equal keys; the feet keep moving.
- Evaluate at `s + lead` with `lead = (2 / omega) * v`, which is 2.0 m at 2.5 m/s. The critically damped follower has ω = 2.5 rad/s (lag 0.8 s), so with the lead the camera reaches each key at its s ("lead the cues", 3.1 b).
- After damping, rate limits: yaw at most 18 deg/s, pitch at most 13 deg/s. Thread's `LimitRate` currently uses 60 and 40. T-NUM expects peaks of 17.6 and 12.9.
- Sway at most 0.3 deg combined, seeded, faded in over the 0.5 s ramp, no roll.
- Validator test: any key pair closer than 1 m with a changed pose must be replaced by a ramp of at least 4 m. Ring's paired keys at (s, s + 0.05) after holds would become whip pans.

**5.4 Lateral offset table** (3.1 f, 14.1 deviation, 14.12). `Lateral(s)` is in lane coordinates, positive to the right of travel. 14.12 gives world x on L2 (centre x -1.5, heading 0, right = +X), so lateral = x + 1.5:

| Beat | (s, lateral) keys |
|---|---|
| 2 | (120, 0) (122, 0) (160, +5.7) (166, +5.7) (226, -3.0) (242, -3.0) (282, 0) (420, 0) |
| 3 | (466, +2.5) (502, +2.5) (544, -2.5) (612, -2.5) (660, 0) |
| 4 | (740, +4.5) (800, +4.5) (850, 0) |

The lateral is 0 elsewhere; the funnel at s 898-920 is at the centre line. Use smootherstep between keys. Check: s 824 gives x 0.92, matching KS11.

- **Lateral follows the table exactly** (feed-forward: lateral velocity = dLateral/ds x ds/dt). Thread's proportional lateral drive (gain 1.5/s, cap 1.2 m/s) would lag the 0.70 m/s peak by about 0.5 m.
- **Starting offset.** A player's offset when the cinematic starts decays over **distance** (for example e-folding 4 m), not Ring's 1.5 s time constant, so the path is identical at any speed.
- **Constraints.**
  - |Lateral| <= HalfWidth(s) - 0.15 m.
  - Ground-speed peak 2.597 m/s at s 141 (ramp 122 to 160, slope peak 1.875 x 5.7 / 38).
  - Lateral speed peak 0.70 m/s.
  - Key-shot poses use the same lateral values (KS04 x 4.2, KS05 -4.5, KS07 1.0, KS08 -4.0, KS10 3.0).

**5.5 Anything keyed to real time** (3.1 c).
- Sway, follower and rate limits are time-based on purpose. Events are not.
- Door: trigger by s, then the door's own clock.
- Whale and other set pieces: `smin(x_time, x_prog)` (H6).
- Lamps, sun break, `el(s)` and `N(s)`: by s.
- EV, WB and bloom slew in `AtmosphereDriver` is per second. At 2.5 m/s the distance ramps peak at 0.054 / 0.19 / 0.08 EV/s, below 0.30.
- `BulwarkScore.Render(speed)` takes the speed.
- QA must never force 1.5 m/s on the cinematic (Ring `QaHooks` line 326 did).

**5.6 Deletions, per source.**
- **Thread `AutoWalker.cs`:** delete `CinematicPlan`, including `MinSlowFactor`, `DelayBudget`, `BudgetScale`, `FactorSmoothTime`, `HoldForever`, `ExtraTime`, `PlannedDelay` and per-cue speed. Delete the `ThreadConstants.GazeCues` / `GazeCue` structure (relative yaw, hold seconds, yawEnd/pitchEnd, speed factor) and `CinematicExtraMax`. Change `StartEase` 2.5 to 0.5. Change `MaxYawRate` 60 to 18 and `MaxPitchRate` 40 to 13. Change `GazeSmoothTime` 1.2 to ω 2.5 (smooth time 0.8). Make `-speed` and `-brisk` apply to test walks only.
- **Ring `WalkPlanner.cs`:** delete `Phase.Brake`, `Phase.Hold`, `StartBrake`, `NextStopD`, `BeginHold`, `EndHold`, `BeginRelease`, `BrakeTime`, `_holdDone`, `_glanceDone`, `_glanceIdx`, `_releasing`, `_evalFloorS`, `MaxLag` and its own integration of `_d`. Change `CineAccelTime` 1.5 to 0.5 and `LookOmega` 3 to 2.5. Remove the forward-gaze `AutoTest` look branch unless a `-gaze forward` option is wanted.
- **Ring `GazeSchedule.cs`:** delete `Hold`, `Step`, `HoldSteps`, `HoldRelease`, `Holds`, `BuildHolds`, `Glances`, `StepPose`, `SwayOverlay`, the `Rel`/`F`/`A` flags, the heading look-ahead and `Evaluate(s, out ...)` via `AutoWalker.Path.DAtSharp`. Keep a heading-free `Evaluate(s, out yaw, out pitch)` for QA. Replace the hard-coded `LateralOffset` with the table of 5.4.
- **Not imported:** Abyss `WalkPlanner.ProfileSpeed`, `ZonesFactor`, `Beat7Factor`, `SlowK`, the H7 hold and `GameState.HoldH7`.

**5.7 Tests and docs to rewrite.**
- Delete every test or document that quotes the old numbers: Ring 636.8 s at 1.5 m/s; the 49 s of holds; Thread's cinematic budget of 8-10.5 min.
- New EditMode or player tests:
  - progress speed 2.500 +- 0.001 from t = 0.5 s to the trigger at every s;
  - ramp loss 0.25 s;
  - trigger at 599.65 +-3 s and 999.15 +-4 s;
  - rest at s 1500;
  - gaze peaks 17.6 / 12.9 deg/s;
  - lateral margin at least 0.15 m and ground-speed peak 2.597 m/s at s 141;
  - brisk flag constant through the ramp;
  - roll 0.00 manual and at most 0.3 cinematic;
  - cancellation by mouse, key, stick or Shift; C toggles; restart from the current s.

### 6. Integration seams the lead must wire

**6.1 `BwPath` replaces the helix path API of `ThreadMath`.** Thread's controller, AutoWalker, PathGuide, WallBot, ShotPlan, CaptureHarness and QaProjection all call `ThreadMath.PathPoint`, `PathY`, `Heading`, `Tangent`, `Right`, `CorridorToWorld(s, lat, h)`, `ForwardToS`, `HalfWidth`, `ProjectToPath`, `Fov`, `BeatIndex` and `BeatBlend`. Re-implement these on:
- a static double `FilletPath` built from the 14.4 table;
- `FloorY(s)`: road 0; stairs profile (15 flights x 18 steps, riser 0.20, tread 0.30, landings 1.2 m); gallery ramp -0.12 per m from s 1070 to -90;
- the eye height as the **2.5 m triangular-kernel average** of that profile, analytic in s. Thread's time-based `EyeFollower` (`CameraYFollowTime`) is replaced, because Part 9 forbids a time filter;
- `HalfWidth(s)`;
- `Fov(s)`.

The Thread QA stack then works unchanged.

**6.2 Capture path** (Thread `CaptureHarness`, LateUpdate 32000). For each shot:
1. `BwClock.Set` or freeze the clock.
2. Set the progress override.
3. `SetPoseOverride`.
4. `ProgressDirector.Apply()`, which calls `AtmosphereDriver` with bypass.
5. `BackdropRig.FollowAll(eye)`.
6. `InstancedRegistry.SubmitAllFor(cam)` and `InstancedField.SubmitAllFor(cam)`.
7. `SubmitRenderRequest`.
8. `QaDepthGrabPass` when the lint is on.
9. `CompositionLint.Run` with depth.

The UI overlay is never in the render texture; Ring's `SetOverlayTarget` and `ForceIntroTime` handle title stills.

**6.3 Execution order.**

| Order | Component |
|---|---|
| -1000 | `GameBootstrap` |
| -800 | `UiDirector` (was Thread `FadeController`; Ring used -90) |
| -50 | `CameraRig` Update |
| -40 | `AutoWalker` |
| -30 | `PlayerController` |
| -20 | `PathGuide` |
| LateUpdate -50 | `CameraRig` |
| 150 | `BackdropRig` |
| 200 | `InstancedRegistryDriver` |
| 500 | `ProgressDirector`, then `AtmosphereDriver` |
| 32000 | `CaptureHarness`, `InstancedFieldDriver` |

**6.4 Pending Thread-side work.** These cover the Thread-side conflicts this survey found; the remaining Thread-side checks are listed in the Thread survey.
- `ShaderIds` needs `BwFade`.
- `GradeState` needs a white-balance matrix and a per-beat dither peak (rule 3.1.6).
- `MaterialLibrary.All()`.
- `BatchTools.PlayerExePath` honours `BW_BUILD_DIR`.
- `ProjectConfigurator` gains Ring's UI Toolkit setup and Always Included Shaders.

### 7. Lessons from DEVELOPMENT.md and the final reports

**Abyss**
- **Fast compile:** standalone SDK projects against the Unity DLLs. Use per-run obj and bin through `Directory.Build.props` and the env var `FC_RUN`. Under PowerShell 5.1, `-p:` paths with spaces break (a trailing backslash escapes the quote), so pass them through the environment.
- **Colliders (ADR 3):** invisible thick box colliders on a dedicated layer, from the same constants as the geometry. Its bots found real holes at ledge starts, landings and pylons.
- **Tier fades (ADR 6):** `_AbFade` complementary dithered fades, never alpha blending.
- **One dither (ADR 8).** Abyss's IGN `FinalDither` is not copied.
- **Captures (ADR 9):** R16G16B16A16 with MSAA 4, then 8-bit sRGB.
- **Text (ADR 10):** UI Toolkit with an editor-generated PanelSettings and theme. A runtime-created PanelSettings has no ICU data. Letter spacing in px.
- **Far plane (ADR 12):** the spec plane would have clipped visible content. Check sight lines before switching far planes; Bulwark 14.6 switches only where the exterior is hidden.
- **URP (ADR 7):** Forward+ `_CLUSTER_LIGHT_LOOP`, MSAA 4x, 64-bit HDR, GPU Resident Drawer off, 4096 / 4 cascades, LOD cross-fade off.
- **Encode run e01:** no 1-2 LSB dither survives x264/VP9 at low bitrates on near-flat gradients. A static dither survives better (CRF 20 longest run 108 vs 240 px). 2.0 LSB static fixed the source banding. The gate was recalibrated to the step-edge rule. The URP warning about stripped DoF, motion blur and Panini shaders is benign: whitelist it in the log scan.
- **Composition lint (R1-20, R1-29, R1-39):** box-centre rule for subjects that fit the frame, in-frame centre for oversize subjects. Occlusion of at least 30 % caught false passes (the KS2b truss at 0 %, the KS5c gantry at 23 %). About 140 arbitrations were sight-line arithmetic, so run the lint on the graybox.
- **Cinematic (calibration table):** the eased starts and stops around holds added 4-5 s on their own (688 vs 684 s). This is why holds are gone.
- **Ending (D2):** the trigger distance was moved so the goal stays visible during the ending.
- **Thin lines:** long members need a vertex ring at least every 4 m for the widening shader.
- **Distance-keyed rates:** a ramp in EV per metre gives 0.35 EV/s at walk and 0.58 EV/s at brisk.

**The Ring**
- **Environment:** URP package-cache folders are hashed. `com.unity.mathematics` is not installed: plain C# maths.
- **First compile:** failed on the missing `Unity.RenderPipelines.GPUDriven.Runtime` reference in the editor asmdef; `fastcheck` then learned to honour asmdef references.
- **Shaders:**
  - Forward+ keyword `_CLUSTER_LIGHT_LOOP`; extra directional lights need their own loop.
  - `LIGHT_LOOP_BEGIN` needs a local `inputData`. Use `GetMainLight(shadowCoord, positionWS, shadowMask)` and never the 7-argument `LightingPhysicallyBased`.
  - Only `_SHADOWS_SOFT` on Windows. Never write `unity_Ambient*` or `_GlossyEnvironmentColor` (URP overwrites them per camera).
- **Rendering:**
  - Render Graph only. A saved Volume asset decides which post variants survive stripping. Set the depth format explicitly; reversed Z.
  - GPU Resident Drawer off. `GetInstanceID` is obsolete-as-error in 6000.6 (`GetEntityId`; `WarmupLoader` already uses it). `Mesh.AllocateWritableMeshData`.
  - Indirect draws need explicit `worldBounds` and `new RenderParams(material)`.
- **Captures:** `StandardRequest` into an HDR render texture whose format becomes URP's internal format; the asset MSAA is ignored for RT targets, so set `msaaSamples` on the RT.
- **Audio:** generator as the first filter on a clip-less AudioSource, not on the listener; `Play()` after adding components; 48 kHz; the default `reflectionsLevel` mutes the reverb.
- **UI Toolkit:** editor-created PanelSettings with a `.tss` theme; UI shaders in Always Included Shaders; letter-spacing in px or % only; keep the UIDocument enabled (toggling it wipes a code-built tree).
- **Build:** Mono only; `NamedBuildTarget.Standalone`; set `runInBackground`.
- **ADR-003:** sky dome at the far plane with clip z forced (reversed-Z aware). Relevant to `BwFarField` with `BackdropRig`.
- **ADR-007:** the far-plane arbitration again.
- **ADR-010:** switching the shadow distance popped every shadow, so it was slewed at 40 m/s. Bulwark keeps it constant.
- **ADR-011 and ADR-012:** geometry and fog arbitrations found only from captures. Run sight-line checks early.
- **Session 2:** ended early. The wall bot, soak, encode test, frame sequences and EditMode re-run were never run in the player. The reflection-based `RingQaHooks` hid a cinematic forced to 1.5 m/s. Builders and critics briefed with `BUILDER_BRIEF` and `CRITIC_BRIEF` worked well, with resume after a usage limit from the workflow run id.

### 8. Import order and gates
1. **Copy.** Thread base first. Then Abyss `RT/Mesh/*`, `CorridorKit`, `RT/Rendering/*` (BackdropRig, InstancedBatch, InstancedRegistry and driver, DetailTier), slim `GameState`, `WarmupLoader`, `CompositionLint` with `KeyShotSubject`, `BwBeam` with `ShaderMeshes`. Then The Ring's FilletPath, WalkPlanner, GazeSchedule, UiDirector, UiTimeline, the BwLit includes, BwConcrete, AtmosphereDriver, InstancedField set and TerrainIndirectField, BwCurvature, the two briefs, and the merged tools.
2. **Do not copy:** Thread `Mesh/MeshKit.cs`, `FadeController`, `GlyphText`, `PauseOverlay`, `CinematicPlan`; Abyss `SceneClock`, `ClockDriver`, `KeyShots`, `QaSequences`, `QaBots`, `QaDrivers`, `LiftController`, `FinalDither`; Ring `PlayerController`, `AutoWalker`, `CameraRig`, `PathGuide` body, `RingFinalDither`, `RingBend.hlsl`, `QaHooks`.
3. **Compile.** Run `fastcompile` all, then a real Unity compile, then the shader gate (`ShaderUtil.ShaderHasError` on every `Megastructure/Bw*`). On a three-beat graybox path, `-autotest` must pass at 1.5 m/s and with `-cinematic` at 2.5 m/s, with the section 5.7 tests, before any beauty work (14.8 P0).

---

## Copy list

One table that merges both surveys, ordered for copying. Steps: **S1** assemblies, **S2** core, **S3** mesh, **S4** rendering and shaders, **S5** systems, **S6** player, **S7** UI, **S8** QA, **S9** audio, **S10** editor tools, **S11** tests, **S12** tools and root files, **S13** docs. Inside a step, copy top to bottom: a row only depends on rows above it or on files generated later by `RebuildAll`. Rules for every row: no `.meta` files, no game content, namespace of the destination folder (A§1, B§1). "Rename" lists the renames beyond the generic rules (`Thread*`/`Ab*`/`Abyss*`/`Ring*` → `Bw*`, `_Th*`/`_Ab*`/`_Ring*` → `_Bw*`, shader names `Megastructure/Bw*`, log tags per C-23); "—" means the file keeps its name. Rows tagged **Decision C-nn** wait for that decision. "pattern" means read the source and write a new file; nothing is copied verbatim.

| Source | Destination | Rename | Strip / adapt notes |
|---|---|---|---|
| **S1** `T:RT/Megastructure.Runtime.asmdef` | `B:RT/Megastructure.Runtime.asmdef` | — | Copy unchanged (`name` `Megastructure.Runtime`, `rootNamespace` `Megastructure`, references InputSystem, RP Core, URP, `UnityEngine.UI`). Add `Unity.Mathematics`/`Unity.Burst` only if used. `UnityEngine.UI` can go if `GlyphText` is not copied (C-04). A§2, B§1 |
| **S1** `T:Assets/_Game/Beats/Megastructure.Beats.asmref` | `B:Assets/_Game/Beats/Megastructure.Beats.asmref` | — | `{"reference": "Megastructure.Runtime"}`. Required: builder discovery only scans the Runtime assembly. A§2 |
| **S1** `T:ED/Megastructure.Editor.asmdef` | `B:ED/Megastructure.Editor.asmdef` | — | Unchanged; must keep `Unity.RenderPipelines.GPUDriven.Runtime` (The Ring's first compile failed without it). A§2, B§1 |
| **S1** `T:TS/EditMode/Megastructure.Tests.EditMode.asmdef` | `B:TS/EditMode/Megastructure.Tests.EditMode.asmdef` | — | Unchanged. A§2 |
| **S1** `T:TS/PlayMode/Megastructure.Tests.PlayMode.asmdef` | `B:TS/PlayMode/Megastructure.Tests.PlayMode.asmdef` | — | Unchanged. A§2 |
| **S2** `T:RT/Core/ThreadMath.cs` (extract `struct D3`, `Deg`, `Clamp01`, `Lerp`, `Smoothstep`, `Smootherstep01`, `Smootherstep`) + `R:RT/Core/RingMath.cs` (`Slew`, `CriticalDamp(omega)`, `CriticalDampAngle`, `Pcg`, `U01`, `ValueNoise`, `Smax`) | `B:RT/Core/BwMath.cs` | `ThreadMath`, `RingMath` → `BwMath` | Extract only. Drop path, helix, thread law, ascent, fog and FOV (the path API is rebuilt as `BwPath`, S6). `D3` is used by `QaProjection`, `ShotPlan`, `WallBot`. A§3.2, B§2 (C-17) |
| **S2** `T:RT/Core/ThreadConstants.cs` (pattern) | `B:RT/Core/BulwarkConst.cs` | `ThreadConstants` → `BulwarkConst` | Write from Part 14; copy no value. Keep the `KeyShot` struct shape without `wide`; no `GazeCue`. Consts literal (`gen_hlsl.py` parses only `public const double\|float\|int NAME = literal;`). `Msaa = 4`. Also receives the Ring-derived constants (14.11 UI timings, gaze and lateral keys, or per-beat tables). A§3.2, B§4, B§5 |
| **S2** `T:RT/Core/ThreadPalette.cs` (pattern) | `B:RT/Core/BulwarkPalette.cs` | `ThreadPalette` → `BulwarkPalette` | Keep the API (`Table` of (id, hex), `Linear()`, `FillFromTable()`, `PaletteToken`); replace the whole `Pal` enum and `Table` with the 14.2.1 tokens (T-PAL tolerance 0.004 linear). A§3.2 |
| **S2** `T:RT/Core/ThreadClock.cs` + features of `A:RT/Core/SceneClock.cs` and `A:RT/Core/ClockDriver.cs` | `B:RT/Core/BwClock.cs` and `B:RT/Rendering/GradeState.cs` | `ThreadClock` → `BwClock`; `GradeState` split into its own file | `BwClock`: `Frozen` (`-freeze-ambient`), `Stepped`, `Frame`, `FreezeDither` default **true**, H6 capture hook; merged from Abyss: `RealTime`, `Wrapped(period)` in double, freeze while paused, `_BwTime` (mod 3600, double), `-clock T`, `-freeze-clock` as alias of `-freeze-ambient`. `GradeState`: drop `Darkness` and the old defaults; add `WhiteBalance`, `Tint`, `DitherPeak`; keep `Enabled`, `FadeBlack`, `Saturation`, `Contrast`. Abyss clock files are not copied. A§3.2, B§2 (C-05) |
| **S2** `A:RT/Core/GameState.cs` | `B:RT/Core/GameState.cs` (slimmed) | — | Keep only S (double), Beat, CameraPos/Rot/MainCamera, PlayerPos, Cinematic, CaptureMode, AutoTest, Paused, Ended, GoalReached, GoalRealTime, PictureFade, Seed. Delete `LiftPhase`, E, Lift, RideSeconds, RideU, RideStarted, HoldH7. One documented writer per field. B§2, B§3 (C-06) |
| **S2** `R:RT/Core/RingRig.cs` (`RingEvents`, line 55) | `B:RT/Core/BwEvents.cs` (file name not fixed by the surveys) | `RingEvents` → `BwEvents` | Events: PauseChanged, LoadComplete, PlayerMoved, GoalReached, CinematicChanged, EndingComplete, BeatEntered. Do not bring `RingRig`/`RingState` themselves. B§2 (C-06) |
| **S2** `T:RT/Core/ThreadServices.cs` | `B:RT/Core/BwServices.cs` | `ThreadServices` → `BwServices` | Strip `FootSurface` values, the 858 beacon trigger, `BeginEnding` 6 s. Surfaces: concrete, wet concrete, road, grating, steel stair, rough ledge. Keep `IPlayerState`, `ICameraControl`, `IAudioHooks`, `IFadeUI`; `UiDirector` implements `IFadeUI` and exposes the single pause owner through `IFadeUI.Paused`. A§3.2, B§2 |
| **S2** `T:RT/Core/CliArgs.cs` | `B:RT/Core/CliArgs.cs` | — | `StartS` from `BulwarkConst`. Dither flags per C-07 (keep `-freeze-dither` alias and `-dither-lsb X`). Add `-clock T`, `-freeze-clock`, the `pose:` token, `-uifont theme\|legacy`, every Part 14 flag; document `-report-dir`, `-grade-off`, `-capture-only`, `-capture-4k`, `-seq-res`, `-perf-rt`, `-audio-dump-seconds`, `-graphics-api`, `-dev-build`, `-editor-timeout`. Abyss and Ring `CliArgs` are not copied. A§3.2, B§2 |
| **S2** `T:RT/Core/WorldContext.cs` | `B:RT/Core/WorldContext.cs` | — | Strip `FrameState.aVirt`, `darkness`, `groundActive`, `nearActive`, `NearRoot`, `GroundGroup`. Keep `IWorldUpdater`, `IBeatBuilder`, `Register`, `AddMesh` (probes off, triangle budget; calls Abyss `MeshKit.TriangleCount`; run `MeshValidation` before `UploadMeshData(true)`), `BuildLog`. Bulwark frame fields: s, ambient t, camera pose, render height, vFOV, beat. Provide `Seed` (for `VertexBake`) and `IKeyShotSubjects` registration (for `CompositionLint`). A§3.2, B§3 |
| **S2** `T:RT/Core/MaterialLibrary.cs` | `B:RT/Core/MaterialLibrary.cs` (skeleton) | — | Drop all 33 Thread fields; Bulwark fields `BwConcrete`, `BwSea`, …, `BwLine`; keep `Grade` and `[CreateAssetMenu]`; add `All()` for `WarmupLoader`. A§3.2, B§2 (C-15) |
| **S2** `T:RT/Core/ShaderIds.cs` | `B:RT/Core/ShaderIds.cs` (skeleton) | `_Th*` → `_Bw*` | Drop Thread names. Add `BwFade` (InstancedBatch, DetailTier), `_BwTime`, `_BwScreen`, `_BwCam` and the other `_Bw*` globals; globals must not collide with material properties (3.2 #5). A§3.2, B§3, B§6.4 |
| **S2** `R:RT/Core/RingBend.cs` (method only) | `B:RT/Core/BwCurvature.cs` + HLSL twin in `BulwarkAtmosphere.hlsl` | `RingBend` → `BwCurvature` | Do not copy `Bend`, `Rotate`, `InflateBounds`. Keep the stable `2R sin²(t/2)` form, a float32 replica next to a double reference, and the comparison tests. Uses: drop `2R sin²(d/2R)`, H4 dip, factored discriminant. T-NUM: dips 2.486 / 2.467°, discriminant relative error ≤ 1e-5. B§4 |
| **S3** `A:RT/Geometry/MeshBuilder.cs` | `B:RT/Mesh/MeshBuilder.cs` | — | Keep as is (R = AO, G = wear, UV1/UV2, Push/Pop matrix, 32-bit indices above 65 535 vertices, exact bounds). B§3 |
| **S3** `A:RT/Geometry/MeshKit.cs` | `B:RT/Mesh/MeshKit.cs` | — | Keep the listed generators; add `TriangleCount(Mesh)`. Thin-line UV1/UV2 data: C-27. Never copy `T:RT/Mesh/MeshKit.cs` or `R:RT/Core/MeshKit.cs` (same class names). B§3 (C-11) |
| **S3** `A:RT/Geometry/MeshKit.Loft.cs` | `B:RT/Mesh/MeshKit.Loft.cs` | — | Keep Loft, LoftVarying, PathFrames, Extrude, IBeam, Helix/HelixTube/HelixRibbon, RoundedRectangle (gallery, r 0.3), Rectangle, Circle, IBeamSection, Triangulate; Airfoil optional. B§3 |
| **S3** `A:RT/Geometry/MeshKit.Revolve.cs` | `B:RT/Mesh/MeshKit.Revolve.cs` | — | Keep Lathe, Cylinder, TaperedCylinder (`BandedColumnProfile` harmless). B§3 |
| **S3** `A:RT/Geometry/MeshKit.Structures.cs` | `B:RT/Mesh/MeshKit.Structures.cs` | — | Strip HexCorner, HexFaceNormal, HexApothemOf, HexWallPanel, HexAnnulus, MitredRing, HexTube and the `AbyssConst.SHAFT_FACE_NORMAL_DEG` comments. Keep WarrenTruss with gussets; HelicalAnnulus optional. B§3 |
| **S3** `A:RT/Geometry/MeshValidation.cs` | `B:RT/Mesh/MeshValidation.cs` | — | Call from T-MESH and from `Validate` over every mesh passed to `WorldContext.AddMesh`. B§3 |
| **S3** `A:RT/Geometry/VertexBake.cs` | `B:RT/Mesh/VertexBake.cs` | — | Seed from `WorldContext.Seed` instead of `GameState.Seed`. B§3 |
| **S4** `T:RT/Rendering/ThreadGradeFeature.cs` | `B:RT/Rendering/BwGradeFeature.cs` | `ThreadGradeFeature` → `BwGradeFeature`; feature name "Bw Grade" | `PipelineConfigurator.GradeFeatureType = "Megastructure.Rendering.BwGradeFeature"`. Keep `applyInSceneView = false`, the Game-camera filter and the skip guards (A§4.1). A§3.1 (C-14) |
| **S4** `T:RT/Rendering/ThreadGradePass.cs` | `B:RT/Rendering/BwGradePass.cs` | pass, property, RT and profiler names `Thread*`/`_Th*` → `Bw*`/`_Bw*` | Add `_BwGradeM0..M2` (packed as P3/P4/P5 in `AddPass`). Drop `GradeState.Darkness`; dither peak per beat; static dither by default. A§3.1, A§4.3 |
| **S4** `T:RT/Rendering/ThreadGradeMath.cs` | `B:RT/Rendering/BwGradeMath.cs` | `ThreadGradeMath` → `BwGradeMath` | Strip `DitherDarkD0/D1`, the dark gain, the beacon comment. `BloomClamp` 64 → **40**. Add the double-precision WB + saturation matrix (`ColorUtils.ColorBalanceToLMSCoeffs`, Core `Color.hlsl:254-272`, white luminance preserved) and the 0.18-pivot linear contrast with toe protection; update the `Display()` mirror; keep the toe-less Neutral (B03-1). A§3.1, A§4.3 |
| **S4** `T:RT/Rendering/BlueNoise.cs` | `B:RT/Rendering/BlueNoise.cs` | texture `ThreadBlueNoise64` → `BwBlueNoise64` | Constants from `BulwarkConst` (`DitherTile` 64, `DitherSigma` 1.5); keep `Seed = 20261029UL`. A§3.1 |
| **S4** `T:RT/Rendering/R2Sequence.cs` | `B:RT/Rendering/R2Sequence.cs` | — | `DitherFrame(frame, freeze)`: freeze (static) is the default. A§3.1 |
| **S4** `T:SH/ThreadGrade.shader` | `B:SH/BwGrade.shader` | `Megastructure/ThreadGrade` → `Megastructure/BwGrade`; `_ThGrade*`, `ThGrade*()`, `kThGrade*` → `Bw` | Include `Generated/BulwarkConstGen.hlsl`. New order: matrix, linear contrast (pivot 0.18), bloom, Neutral, vignette × fade, dither; delete the display-space S-curve (or keep it only at k = 0). Keep `// pal:ok`, `#pragma target 4.5`, `editor_sync_compilation`, `Fallback Off`. A§3.1, A§4.2, A§4.3 |
| **S4** `R:Assets/_Game/Shaders/Include/RingCommon.hlsl` (only `RingFilteredGrid1`, `RingValueNoise`) | `B:SH/Include/BwCommon.hlsl` | `Ring*` → `Bw*` | Only the two helpers BwConcrete needs. B§4 |
| **S4** `R:Assets/_Game/Shaders/Include/RingLitCore.hlsl` | `B:SH/Include/BwLitCore.hlsl` | `Ring*` → `Bw*` | Strip the bend (θ = 0) and the `RingPerturbNormal` bend rotate; drop the `RingGround` include; include `BulwarkAtmosphere.hlsl`, `BulwarkPalette.hlsl`, `BwCommon.hlsl`. Keep the hook contract and `_ENVIRONMENTREFLECTIONS_OFF`/`_SCREENSPACEREFLECTIONS_OFF` before the includes. B§4 |
| **S4** `R:Assets/_Game/Shaders/Include/RingLitPass.hlsl` | `B:SH/Include/BwLitPass.hlsl` | `_RingCam` → `_BwCam`; `RingFog` → `BwFogApply` | Remove the bend. **Add instancing** (`UNITY_VERTEX_INPUT_INSTANCE_ID`, `UNITY_SETUP_INSTANCE_ID`, `#pragma multi_compile_instancing` in every pass) so `InstancedBatch` can draw lit materials; `RING_INDIRECT` draws stay uninstanced. B§4 (C-12) |
| **S4** `R:Assets/_Game/Shaders/Include/RingLighting.hlsl` | `B:SH/Include/BwLighting.hlsl` | `Ring*` → `Bw*`; `RingKeyAtten` → `BwSunBreak` | Drop bend rotations; trilight `_BwAmbSky/_BwAmbMid/_BwAmbGround`; reflection `BwSkyColor(reflect)` blended toward fog by roughness; `BwSunBreak(posWS)` (T_sun = 1 − 0.85 (1 − hole(P'))); delete the `_RingSet.x > 1.5` tunnel mask; keep the NaN-safe clamp. B§4 |
| **S4** `R:Assets/_Game/Shaders/RingCeramic.shader` | `B:SH/BwConcrete.shader` | → `Megastructure/BwConcrete` | Becomes the 14.2.6 BwConcrete recipe (ConcreteMid/Dark/Wet by `_Wet`, formwork 3.0 × 1.5 m with 12 mm gaps, tie holes 0.6 m, filtered fade below 4 px, macro tone ±6 %, vertex AO/wear). Keep `_CLUSTER_LIGHT_LOOP`, `_SHADOWS_SOFT` only, one identical `UnityPerMaterial` cbuffer. B§4 |
| **S4** `T:SH/ThreadLine.shader` | `B:SH/BwLine.shader` | → `Megastructure/BwLine`; `_ThScreen` → `_BwScreen` | Strip flat shading, Thread fog, cable shadow, below fade/haze, shade colour, helix and altitude logic. Rewire to the BwLit template (`UniversalFragmentPBR`) and `BwFogApply`. Keep the width maths (`w_min = max(1.5, 1.75·H/1080)` px, alpha floor 0.35, `_LINE_TUBE` A2C only when MSAA > 1, `_LINE_RIBBON`, `_ScaledScreenParams`, `noperspective`, ForwardLit/DepthOnly/DepthNormals, `_CLUSTER_LIGHT_LOOP`). Mesh input: C-27. A§3.1, B§3 |
| **S4** `A:Assets/_Game/Shaders/BeamWedge.shader` | `B:SH/BwBeam.shader` | → `Megastructure/BwBeam`; `_Ab*` → `_Bw*` | Keep recipes 0 and 2; strip recipe 1 (fan wall patches) and 3 (`ReverseSubtract` lobe). Fog → `BwFogApply` transmittance; pixel angle → `_BwCam.w`. Replace the AbyssCommon/AbyssFog helpers. Note the H2 MSAA-halo caveat (reads resolved `_CameraDepthTexture`). B§3 |
| **S4** `A:RT/Rendering/ProceduralTextures.cs` lines 288–333 (`ShaderMeshes.BeamWedgeMesh`) | `B:RT/Mesh/ShaderMeshes.cs` | — | Extract that mesh builder only. B§3 |
| **S4** `A:RT/Rendering/BackdropRig.cs` | `B:RT/Rendering/BackdropRig.cs` (namespace `.Rendering`) | — | Camera from `BwServices.CameraControl.Camera`; order 150. Add a static list and `FollowAll(pos)` (CaptureHarness calls it after `SetPoseOverride`). Used for `BwFarField` and far gate towers; no second camera (A§4.1). B§3 |
| **S4** `A:RT/Rendering/InstancedBatch.cs` | `B:RT/Rendering/InstancedBatch.cs` | `_AbFade` → `_BwFade` | Add `Submit(Camera)`; keep 511 per draw, spatial chunks, bounds +10 %, explicit shadows, probes off. Needs the instanced BwLit passes. B§3 (C-12) |
| **S4** `A:RT/Rendering/InstancedRegistry.cs`, `A:RT/Rendering/InstancedRegistryDriver.cs` | `B:RT/Rendering/InstancedRegistry.cs`, `B:RT/Rendering/InstancedRegistryDriver.cs` | — | Add `SubmitAllFor(cam)`; the driver (LateUpdate +200) does not submit in capture mode; `GameBootstrap` calls `Clear()` at each load. B§3 (C-12) |
| **S4** `A:RT/Rendering/DetailTier.cs` | `B:RT/Rendering/DetailTier.cs` | `ShaderIds.AbFade` → `ShaderIds.BwFade` | `QaHooks.TierOverride` → a small static (e.g. `QaCommon.TierOverride`). Shader fade pattern: C-28. Units-to-horns at 400 m (T7 dE ≤ 2.0). B§3 |
| **S4** `R:RT/Rendering/InstancedField.cs`, `InstancedFieldDriver.cs`, `InstancedFieldHandle.cs` + `R:RT/Terrain/TerrainIndirectField.cs` | `B:RT/Rendering/` (same file names) | — | Plain explicit `worldBounds` (no `RingBend.InflateBounds`); `Material` instead of id; holder under the `WorldContext` root; visibility by `BeatDirector` instead of `IRingDrawable`. Keep `new RenderParams(material)`, `IndexedArgs`, `UnityIndirect.cginc` + `InitIndirectDrawArgs(0)` in every pass, per-camera submission in capture mode, release on destroy, `ResetStatics`. Expose `SubmitAllFor(cam)`. B§4 (C-12) |
| **S5** `T:RT/Systems/GameBootstrap.cs` | `B:RT/Systems/GameBootstrap.cs` | — | `BulwarkPalette` type and Bulwark roots; drop `nearRoot`/`groundGroup`. Replace `Shader.WarmupAllShaders()` with `WarmupLoader` behind the black fade; `InstancedRegistry.Clear()` at load start; 14.6 set pre-warm; cinematic flag through Restart. Keep sorted reflection discovery, per-builder try/catch with the fail tag (C-23), `LoadSeconds` < 15 s, `WorldReady`, `-grade-off`. A§3.3, B§2 (C-08) |
| **S5** `T:RT/Systems/BeatDirector.cs` | `B:RT/Systems/BeatDirector.cs` | — | Boundaries 0, 120, 420, 700, 920, 1070, 1370, 1500; lantern once via `GoalFired`/`GoalCount`, goal trigger s 1498.5; keep `OrderOk`. A§3.3 |
| **S5** `T:RT/Systems/ProgressDirector.cs` | `B:RT/Systems/ProgressDirector.cs` (skeleton) | — | Delete the whole `ApplyGlobals` body. Keep `S`, `OverrideActive`/`OverrideS`, `Apply()` (now calls `AtmosphereDriver.Tick(s, dt, bypass)` and the updaters), `FrameState`, the updater loop, `BwClock.Tick` in `LateUpdate`, the `beginCameraRendering` `_BwScreen` hook. Move `-dither-lsb` to the driver. A§3.3, B§2 (C-13) |
| **S5** `R:RT/Directors/AtmosphereDriver.cs` | `B:RT/Systems/AtmosphereDriver.cs` | `RingSchedule` → `BulwarkSchedule` (new, pure in s) | Keep: one writer of globals, lights and camera; `RenderSettings.fog = false`; `RenderSettings.sun`; per-light bias; `_BwCam` per camera; slews EV 0.30/s, WB 6/s, bloom 0.1/s, vFOV 1°/s; bypass for stills, teleports (`JumpS` 20 m), first frame; `LogGraphics`. Strip arch/rim fills, ship/cloud shadows, ark, swarm, stars, sunset, `_RingSet`, `_Lamps`, `_Wind`. `ApplyVolume` → `GradeState` writes (no Volume overrides); constant shadows; writes `GradeState.DitherPeak`. B§4, A§3.3 (C-13) |
| **S5** `A:RT/Core/WarmupLoader.cs` | `B:RT/Systems/WarmupLoader.cs` | — | Use `MaterialLibrary.All()`; `YawPitch(205, -72)` → `Quaternion.Euler(72, 205, 0)`; add a third light state (sun off + temporary point light) for Forward+ additional-light variants. Separate from the 14.6 set pre-warm. B§3 (C-08) |
| **S6** `T:RT/Player/ThreadInput.cs` | `B:RT/Player/BwInput.cs` | `ThreadInput` → `BwInput` | Rename only. A§3.4 |
| **S6** `T:RT/Player/PlayerMath.cs` (only `PlayerTuning`, lines 12–48, and `PlayerMath.WrapDeg`/`CritDamp`) | `B:RT/Player/PlayerTuning.cs` | — | Keep `MouseDegPerCount 0.08`, `StickDegPerSec 120`, `PitchLimit 88`, `LookThresholdCounts 3`, `StickThreshold 0.3`; `CorridorLayer` 30 (Safety 31, C-18). No `EyeFollower`. `StopRamp`, `FootstepCadence` as patterns; cadence by speed. A§3.4, B§2 |
| **S6** `T:RT/Player/QualityBootstrap.cs` | `B:RT/Player/QualityBootstrap.cs` | — | VSync on by default; `-perf-log` uncapped; add `Application.targetFrameRate = 60` for `-autotest`, `-wallbot`, `-soak` and long captures; keep F11/Alt+Enter fallback and `runInBackground`. A§3.4, B§2 (C-16) |
| **S6** `R:RT/Player/FilletPath.cs` | `B:RT/Player/FilletPath.cs` (`.Player`) | — | Delete the 3-radius convenience constructor and `CornerRadii`; keep the generic constructor; s = filleted distance (`SharpS`/`DAtSharp` unused); port to double with float overloads; waypoints from 14.4 (A1 r 40, A2 r 40, A3 r 30, A4 r 20); T-PATH values of B§4. B§4 |
| **S6** new, on the signatures of the `T:RT/Core/ThreadMath.cs` path API | `B:RT/Core/BwPath.cs` (location not fixed by the surveys) | `ThreadMath.PathPoint` etc. → `BwPath.*` | Re-implement `PathPoint`, `PathY`, `Heading`, `Tangent`, `Right`, `CorridorToWorld`, `ForwardToS`, `HalfWidth`, `ProjectToPath`, `Fov`, `BeatIndex`, `BeatBlend` on the static double `FilletPath`, `FloorY(s)` (road, stairs 15 × 18, gallery ramp), the 2.5 m triangular-kernel eye height, `HalfWidth(s)`, `Fov(s)`. B§6.1 (C-17) |
| **S6** `T:RT/Player/CameraRig.cs` + `CameraProbe` and counters from `R:RT/Player/CameraRig.cs` | `B:RT/Player/CameraRig.cs` | — | Near 0.05; far plane by s (150 / 12 000 / 300 / 600 m, switching at s 100, 934, 1030, 1373); heading and FOV from `BwPath`/`FilletPath` and the 14.2.5 vFOV table (30 m crossfades). Add the 0.2 m probe sphere with `Physics.ComputePenetration`, `Violations`, `MaxRollDeg`. Keep raw mouse, pitch ±88, no roll, `SetLook`/`SetPoseOverride`, order −50, no input while the AutoWalker drives. Ring's CameraRig file itself is not copied. A§3.4, B§2 (C-10) |
| **S6** `T:RT/Player/PlayerController.cs` + parts of `R:RT/Player/PlayerController.cs` | `B:RT/Player/PlayerController.cs` | — | **Decision C-01.** If Survey B wins: Thread's lane-coordinate controller on `BwPath`; CharacterController check (radius 0.35, slope 45, step 0.3, `minMoveDistance` 0; `cc.Move(analyticTarget − pos)`, rejections > 1 cm counted and projected back; y from the analytic floor); Ring cancel rule, Alt+Enter guard, safety snap with warning. Drive mode: `ds/dt = v_cmd`, `DriveNoSmoothing`, no `ForwardToS`, brisk from the mode, `StopRamp` at 1498.5, lateral feed-forward. B§2, B§4, B§5 |
| **S6** `T:RT/Player/AutoWalker.cs` (shell) | `B:RT/Player/AutoWalker.cs` | — | **Decision C-02.** Delete `CinematicPlan` and its fields, `GazeCues`, `CinematicExtraMax`; `StartEase` 2.5 → 0.5; `MaxYawRate` 60 → 18, `MaxPitchRate` 40 → 13; gaze ω 2.5; `-speed`/`-brisk` only for test walks. `-autotest` = 1.5 m/s, `-autotest -cinematic` = 2.5 m/s, same seed and keys. B§5 |
| **S6** `R:RT/Player/WalkPlanner.cs` | `B:RT/Player/WalkPlanner.cs` (trimmed, pure) | — | Delete `Phase.Brake`, `Phase.Hold`, `StartBrake`, `NextStopD`, `BeginHold`, `EndHold`, `BeginRelease`, `BrakeTime`, the glance and release fields, `_evalFloorS`, `MaxLag`, its own `_d` integration. `CineAccelTime` 1.5 → 0.5; `LookOmega` 3 → 2.5. Outputs speed command, lateral target, look; sway ≤ 0.3°. B§5 (C-02) |
| **S6** `R:RT/Player/GazeSchedule.cs` | `B:RT/Player/GazeSchedule.cs` | — | Delete Hold, Step, HoldSteps, HoldRelease, Holds, BuildHolds, Glances, StepPose, SwayOverlay, the `Rel`/`F`/`A` flags, the heading look-ahead and the `DAtSharp` evaluate. Keep heading-free `Evaluate(s, out yaw, out pitch)`. World keys from 14.5.x, lead `(2/ω)·v`; `LateralOffset` → the B§5.4 table; validator for key pairs < 1 m. B§5 (C-02) |
| **S6** `T:RT/Player/PathGuide.cs` (generator) + API names of `R:RT/Player/PathGuide.cs` | `B:RT/Player/PathGuide.cs` | — | **Decision C-03.** If Survey B wins: Thread's generator driven by one lane function `HalfWidth(s)`; Ring API names plus `FloorY(s)`, `CentreAt(s)`, `DistanceOutsideLanes`, `LaneSamplePoints(0.5)`, fallback floors 3 cm under the deck; boxes 1 m thick, 6 m high, 1 m overlap, every 4 m; ray grid 0.5 m. Never the Ring `Shapes` table, `BridgeY`, `RampY`, `WalkHeight`, `SmoothFloor`, `ChamferDist`, `SwapRevertS`. Either way: expose `Instance`, `KillCount`, `CoverageCheck`; do not implement Ring's `IBeatBuilder`. A§3.6, B§2, B§4 |
| **S6** `A:RT/Geometry/CorridorKit.cs` | `B:RT/Player/CorridorKit.cs` | — | Layers 8/9 → 30/31; `WallThickness` 2 → 1 m; `WallHeight` 4 → 6 m; used for stair and gallery ramp colliders. B§3 (C-18) |
| **S7** `R:RT/UI/UiTimeline.cs` | `B:RT/UI/UiTimeline.cs` | — | Pure functions. Constants to `BulwarkConst` (14.11): black 0–1.0 s, picture 1.0 → 3.5 s smoothstep, title 1.4 → 2.6, hold to 4.6, out to 6.0, hint at 6.5 s for 6 s with 0.6 s fades (`HintKill` 0.35 s); ending 3.0 / 4.0 / 1.5 / 2 + 6 + 2 = 18.5 s unchanged. B§4 |
| **S7** `R:RT/UI/UiDirector.cs` | `B:RT/UI/UiDirector.cs` (`.UI`) | `RingEvents`/`RingState`/`RingRig`/`RingAssets`/`RingPalette`/`RingConstants`/`RingMath` → `BwEvents`/`GameState`/`IFadeUI`/`BulwarkConst`/`BulwarkPalette`/`BwMath` | "BULWARK" 56 px, letter-spacing 12.32 px; `TitleText` #EDEBE5 α 0.88; title centre 38 %; hint bottom 6 %, α 0.7, text "WASD  move    Mouse  look    Shift  faster    C  cinematic"; end line 51 px (11.22 px) "Someone held the sea back, so the land could stay dry."; Scale With Screen Size 1920 × 1080, match height; font mode default Theme with `LegacyRuntime.ttf` fallback; hint never in cinematic or `-autotest`; `ShaderFade = true` writing `GradeState.FadeBlack`; audio bound directly; single pause owner; order −800. B§4, B§6.3 (C-04) |
| **S7** `T:RT/UI/FadeController.cs` | `B:RT/UI/FadeController.cs` (grade-fade bridge) or not copied | — | **Decision C-04.** Survey A: keep the mechanism (fade inside the grade via `GradeState.FadeBlack`, unscaled timers, pause dim, kill-volume blink, no UI in `-capture`) with the 14.11 timings. Survey B: do not copy; `UiDirector` owns fade and pause. A§3.5, B§2 |
| **S7** `T:RT/UI/GlyphText.cs` | `B:RT/UI/GlyphText.cs` (optional fallback) or not copied | — | **Decision C-04.** Survey A: only if UI Toolkit fails (TitleText #EDEBE5 α 0.88). Survey B: do not copy. A§3.5, B§2 |
| **S7** `T:RT/UI/PauseOverlay.cs` | logic pattern for `UiDirector`, or not copied | — | **Decision C-04.** Survey A: keep Resume/Restart/Quit and the EventSystem-free keyboard/gamepad/mouse hit test, rendered in UI Toolkit. Survey B: do not copy (UiDirector already has menu input). A§3.5, B§2 |
| **S8** `T:RT/QA/QaJson.cs` | `B:RT/QA/QaJson.cs` | — | None. A§3.6 |
| **S8** `T:RT/QA/QaReport.cs` | `B:RT/QA/QaReport.cs` | — | Write the effective dither mode and peak and `targetFrameRate`; log prefix per C-23. A§3.6 |
| **S8** `T:RT/QA/QaCommon.cs` | `B:RT/QA/QaCommon.cs` | — | Drop `GradeRangeBehind`. `QaDrive.Prepare` gets the 60 fps cap. `PathGuideProbe` reflection needs `Instance`, `KillCount`, `CoverageCheck`. `DriveSpeed` adds `-cinematic` (2.5 m/s via the AutoWalker); heading from `FilletPath`. Home of `TierOverride` (DetailTier). A§3.6, B§3 |
| **S8** `T:RT/QA/QaRunnerBase.cs` | `B:RT/QA/QaRunnerBase.cs` | — | Keep the common checks (no error logs, no kill-volume events, coverage, load < 15 s). A§3.6 |
| **S8** `T:RT/QA/AutoTestRunner.cs` | `B:RT/QA/AutoTestRunner.cs` | — | 14.10.4: 1.5 m/s trigger 999.15 ± 4 s, done 1017.6 ± 4, exit by 1026; 2.5 m/s through the AutoWalker trigger 599.65 ± 3, done 618.1 ± 3, exit by 626; progress speed 2.50; ground peak ≤ 2.60; roll 0.00 ± 0.01 manual, ≤ 0.3 cinematic; beats once and in order; lantern once. Wait for the ending. Long GPU job (≈ 17 min at 1.5 m/s). A§3.6, B§5 |
| **S8** `T:RT/QA/WallBot.cs` + reporting from `A:RT/QA/QaBots.cs` | `B:RT/QA/WallBot.cs` | — | 0.40 m inside both edges in lane coordinates with `Lateral(s)`; floor deviation ≤ 0.3 m; progress loss ≤ 0.3 m; T1 bots at x −3.15 and 0.15 over z 0–120; 60 fps cap. From Abyss: per-point failure lists, ceiling rays (soffit 0.62 m), side wall-gap rays, stall and narrowing logic (door, funnel s 898–920, 2.9 m stairs). `QaBots.cs`/`QaDrivers.cs` are not copied. A§3.6, B§3 (C-22) |
| **S8** `T:RT/QA/SoakRunner.cs` | `B:RT/QA/SoakRunner.cs` | — | Loop back before 1498.5; `targetFrameRate 60`; 1800 s through gpu-guard, stall ≥ 35 min; keep the 50 MB limit, warm-up baseline, tail minimum. A§3.6 |
| **S8** `T:RT/QA/PerfMonitor.cs` | `B:RT/QA/PerfMonitor.cs` | — | `BulwarkConst.Msaa = 4`; uncapped for perf runs only; `-perf-rt WxH`; VRAM via `nvidia-smi` in the wrapper. A§3.6 |
| **S8** `T:RT/QA/CaptureWriter.cs` | `B:RT/QA/CaptureWriter.cs` | — | `CaptureMetrics` in 14.10.3 terms (luma 0–255, clipped = any channel ≥ 254). A§3.6 |
| **S8** `T:RT/QA/QaProjection.cs` | `B:RT/QA/QaProjection.cs` | — | Drop every Thread subject builder; keep `AzElDir`, `Basis`, `ProjectDir`, box/disc projection, `FrameMargin` 0.03; subjects from 14.10.2 "Must be visible". A§3.6 |
| **S8** `T:RT/QA/ShotPlan.cs` + `TryPose` parser from `A:RT/QA/KeyShots.cs` + concepts of `A:RT/QA/QaSequences.cs` | `B:RT/QA/ShotPlan.cs` | — | Drop Thread windows, swap/deck/bank/moire extras, `_wide`. Keep the set grammar, 1080/4K/`_1610`/`_219`, free-looks, 30 fps sequences, 60 fps × 10 s encode clips; Bulwark windows, pans (0.5 / 0.1 px), statics (120 frames); walk speed as a parameter. Add `pose:NAME:x:y:z:yaw:pitch:fov` (s clamped ≤ 1497, frozen clock, kind "custom", free fov for look-dev only) and `SeqMode`/`SequenceSpec` `WalkPinned`, `MovingPose`, `Steps`, `CueRef`/`CueMarks`. `KeyShots.cs`/`QaSequences.cs` are not copied. A§3.6, B§3 (C-21) |
| **S8** `T:RT/QA/CaptureHarness.cs` + `QaDepthGrabPass` from `A:RT/QA/QaRenderer.cs` (about lines 14–45 and 262–300) | `B:RT/QA/CaptureHarness.cs` | — | Keep the RT path (RGBA16F, MSAA 4, D32_SFloat_S8_UInt, sRGB blit, async readback, worker PNG, LateUpdate 32000). Teleport cap 1498.5 − 1. UI hiding: C-29. H6 clock rule and frozen `SeaState` per shot; bypass slews; 60 fps cap for long sets. Per shot (B§6.2): clock, progress override, pose, `Apply()`, `BackdropRig.FollowAll`, both `SubmitAllFor`, `SubmitRenderRequest`, depth grab, lint. Drop the "D" static diff and the Thread grade probe. A§3.6, B§3, B§6.2 |
| **S8** `A:RT/QA/CompositionLint.cs` | `B:RT/QA/CompositionLint.cs` (`.QA`) | — | Move `KeyShotSubject {Name, WorldBounds, MinScreenFraction}` into it; subjects via `IKeyShotSubjects` on Bw builders through `WorldContext`; port `Write()` to Thread `QaJson`; empty `LoggedConflicts`. Keep the centre rules, 3 % margin, 8 × 8 occlusion rays (≥ 30 % visible, tolerance 1 % + 5 cm). Needs the depth grab. B§3 |
| **S8** `A:RT/Player/LiftController.cs` (pattern) | pattern for a `BwDoor` owned by the beat-1 builder and for H6 helpers | — | Not copied (beat-5 content). Trigger-started double clock; `gap(t) = 4.0 smootherstep(t / 3.0)` (trigger s 104); `SetForCapture(s)`; `Physics.SyncTransforms()` when leaves move; collider switch at gap 0.5 m; `Frozen` for stills. B§3 |
| **S9** `T:RT/Audio/ThreadDsp.cs` lines 1–332 | `B:RT/Audio/BwDsp.cs` (namespace `Megastructure.Audio`) | `ThreadDsp` → `BwDsp` | Keep `XorShift32`, `PinkKellet`, `SvfTpt`, `DcBlocker` (now **20 Hz**), `SineTable`, `FdnReverb8` (explicit delay-array constructor, per-line Jot (g_i, p_i) for T60 at DC and 5 kHz, blend time per transition, two instances small/hall). Add brown noise, the look-ahead limiter, true peak ≤ −1 dBFS. Lines 333–1079 (the Thread score) are not copied. A§3.7, RESEARCH C§2, C§4, C§8 |
| **S9** `R:RT/Audio/AudioDsp.cs` (`DspLimiter`, line 260) | into `B:RT/Audio/BwDsp.cs` | — | Ceiling −1.42 dBFS (0.84918), threshold −4 dBFS, look-ahead 16 ms, hold 20 ms, release 250 ms, `Cap` 4096; after the reverb; replaces Thread's `0.84·tanh(x/0.84)`. RESEARCH C§5 (C-24) |
| **S9** `T:RT/Audio/ThreadSynth.cs` | `B:RT/Audio/BwSynth.cs` | `ThreadSynth` → `BwSynth` | Generator on a clip-less AudioSource is the only path (no listener path, no fallback). Keep the mailbox, heartbeat, exception latch, allocation-free and lock-free audio thread, `-audio-dump`, rebuild on configuration change. Add an SPSC event ring; `Array.Clear(data)` first; footsteps mixed inside the DSP. A§3.7, RESEARCH C§1 (C-25) |
| **S9** `T:RT/Audio/WavWriter.cs` | `B:RT/Audio/WavWriter.cs` | — | None. A§3.7 |
| **S9** `T:RT/Audio/OfflineAudioRenderer.cs` | `B:RT/Audio/OfflineAudioRenderer.cs` | — | `Render(speed)` at 1.5 and 2.5; trigger 1498.5; 14.11 ending (18.5 s); 48 kHz; meters (3 s RMS ± 1.5 dB, DC < 0.001, step ≤ 0.25 except relays); `MotionS` model, event latches, 4× true peak, beds-stem comparison, Bulwark beat boundaries; replay of the dump's (t, s). A§3.7, RESEARCH C§7, C§8 |
| **S9** `T:RT/Audio/FootstepBank.cs` | `B:RT/Audio/FootstepBank.cs` | — | Strip `SurfaceAt(s)` ranges, Salt/Stone/Metal recipes, 0.78/0.80 m steps. 14.3 recipes (concrete, wet tick, grating, steel stair, ledge scrape); stride by speed (0.75 / 0.85 m, stairs 0.9 m); ±0.8 dB, ±4 % pitch; peak −30 dBFS; surfaces by s. A§3.7, RESEARCH C§6 |
| **S9** `T:RT/Audio/ThreadAudioParams.cs` (pattern) | `B:RT/Audio/BwAudioSpec.cs` + `BwAudioParams` + new `BulwarkScore` | `AudioSpec` → `BwAudioSpec`; parameter struct → `BwAudioParams` | Extract only `Db()`, `SineTableSize`, the limiter ceiling and the sample rate. Drop the 8 Thread layers, levels, swells and windows. `BulwarkScore`: layers W/S/B/F/T/P/V/C as pure functions of (s, t, speed). A§3.7, RESEARCH C§3, C§8 |
| **S9** `T:RT/Audio/AudioDirector.cs` (pattern) + `CreateRig` from `R:RT/Audio/AudioDirector.cs` | `B:RT/Audio/AudioDirector.cs` (rewritten) | — | Keep `outputSampleRate`, `Play()` after `AddComponent`, `-audio-dump [-audio-dump-seconds N]`, the pause duck (−12 dB over 0.4 s), order 700. Remove the footstep AudioSource pool, the metal re-render, the listener path. 48 kHz `AudioSettings.Reset` before the generator; never on the camera; `runInBackground` in test and dump modes; dump sidecar logs (t, s) per block. A§3.7, RESEARCH C§1.1, C§8 (C-26) |
| **S10** `T:ED/EditorCommon.cs` | `B:ED/EditorCommon.cs` | `ThreadPaths` → `BwPaths`, `ThreadLog` → `BwLog` | `ProductName = "Bulwark"`, `CompanyName = "Megastructures"`; generated paths `Shaders/Generated/BulwarkPalette.hlsl`, `BulwarkConstGen.hlsl`, `Settings/BulwarkPalette.asset`, `Settings/BulwarkLighting.lighting`; `ShaderNames` = every `Megastructure/Bw*`; `CorridorLayer 30` (C-18); prefix per C-23. A§3.8 |
| **S10** `T:ED/BatchTools.cs` | `B:ED/BatchTools.cs` | — | Strip the NearRoot/GroundGroup/ThreadSky contract items. Keep `RebuildAll`, `BuildWindows`, `BuildMac` (optional), `CompileCheck`, `Validate`, `CaptureInEditor`. `PlayerExePath()` honours `BW_BUILD_DIR`. Validate rows per A§6 (D3D11 exact, depth format, GRD off, no SSAO, Always Included, no Volume overrides, UIDocument/PanelSettings, splash, names, shadows, Mobile assets gone). A§3.8, A§6 |
| **S10** `T:ED/MaterialFactory.cs` | `B:ED/MaterialFactory.cs` | — | Drop `MaterialConstants`, `Specs()`, the Thread keyword sets, `_ShadeTint`, the SALT_WHITE default. Keep the GUID-stable machinery. Bulwark families: `BwConcrete`, `BwSea`, `BwSheet`, `BwArmour`, `BwGrating`, `BwSteel`, `BwRoad`, `BwFarField`, `BwWaterPortal`, `BwMist`, `BwSnow`, `BwBeam`, `BwCreature`, `BwEmissive`, `BwLantern`, `BwLine`, `BwGrade`. A§3.8 |
| **S10** `T:ED/ProjectConfigurator.cs` + `CreateOrUpdateUi()` and `UpdateAlwaysIncludedShaders()` from `R:Assets/_Game/Scripts/Editor/Batch/ProjectSetup.cs` | `B:ED/ProjectConfigurator.cs` | — | Names from `BwPaths`; `-graphics-api d3d11` (Editor uses it from its next start); keep the listed player settings. Add the theme `.tss` (`@import url("unity-theme://default");`) and PanelSettings asset (Scale With Screen Size 1920 × 1080, match 1, `m_DefaultShader` check) and the 9 UI shaders in Always Included; `AudioManager` `m_SampleRate: 48000`. A§3.8, B§4, RESEARCH C§0, C§9.2 |
| **S10** `T:ED/PipelineConfigurator.cs` | `B:ED/PipelineConfigurator.cs` | — | Drop `ShadowDistance 160`. HDR 64-bit, MSAA 4, render scale 1, GPU Resident Drawer off, Forward+, SSAO removed, grade feature. Shadows 150 m, 4096, 4 cascades, soft. `depthAttachmentFormat`/`depthTextureFormat = DepthFormat.Depth_32_Stencil_8` (as The Ring's `R:Assets/_Game/Scripts/Editor/Batch/UrpSetup.cs:60-61`). Register `MistVolumeFeature` when it exists. Additional-light shadows: decision. A§3.8, RESEARCH B§7.8–7.9 |
| **S10** `T:ED/TemplateCleanup.cs` | `B:ED/TemplateCleanup.cs` | — | Candidates match `B:` exactly; `MoveAssetToTrash`. A§3.8 |
| **S10** `T:ED/SceneBuilder.cs` (skeleton) | `B:ED/SceneBuilder.cs` | — | Drop Thread roots, Sky.mat skybox, far 250 000. Bulwark contract: GameBootstrap, ProgressDirector, BeatDirector, QualityBootstrap, UiDirector (C-04), CaptureHarness, AutoTestRunner, PerfMonitor, WallBot, AtmosphereDriver, UIDocument with PanelSettings; camera post off, AA None, dithering off, `allowMSAA`, HDR, AudioListener; Unity fog off. A§3.8 |
| **S10** `T:ED/PaletteWriter.cs` | `B:ED/PaletteWriter.cs` | `ThreadPalette` → `BulwarkPalette` | Writes `BulwarkPalette.asset`. A§3.8 |
| **S10** `T:ED/ShaderGen.cs` | `B:ED/ShaderGen.cs` | guards `THREAD_*_INCLUDED` → `BULWARK_*` | Outputs `BulwarkPalette.hlsl` (`PAL_*`) and `BulwarkConstGen.hlsl` (`TC_*`); byte-identical to `Tools/gen_hlsl.py` (F6 trimmed, LF, UTF-8 no BOM, declaration order). A§3.8 |
| **S10** `T:ED/CaptureWatchdog.cs` | `B:ED/CaptureWatchdog.cs` | `THREAD_EXIT_CODE` → `BULWARK_EXIT_CODE` | `-editor-timeout` default 1800 s. A§3.8 |
| **S11** `T:TS/EditMode/GradeTests.cs` | `B:TS/EditMode/GradeTests.cs` | Bw names | Keep the 17 listed tests; rewrite the sun-disc/beacon, dither-gain and contrast tests; add the matrix tests of A§4.3. A§3.9 |
| **S11** `T:TS/EditMode/ProjectTests.cs` | `B:TS/EditMode/ProjectTests.cs` | — | All 12 tests; new expected values (Bw names, `Depth_32_Stencil_8`, shadow 150, API exactly D3D11). A§3.9 |
| **S11** `T:TS/EditMode/QaTests.cs` | `B:TS/EditMode/QaTests.cs` | — | Keep the 23 listed; drop the `Kf*` and wide/extra-still tests; rewrite the four listed in A§3.9. |
| **S11** `T:TS/EditMode/AudioTests.cs` | `B:TS/EditMode/AudioTests.cs` | — | Keep the 9 listed; rewrite the limiter (−1.42 asymptote, true peak ≤ −1) and params tests (S1–S8); add renders at 1.5 and 2.5. A§3.9 |
| **S11** `T:TS/PlayMode/InputTests.cs` | `B:TS/PlayMode/InputTests.cs` | `C_ThreadInput_…` → `C_BwInput_…` | All four tests. A§3.9 |
| **S11** `T:TS/EditMode/CoreTests.cs`, `PlayerTests.cs`, `FogTests.cs`, `SkyTests.cs` (patterns) | new test files in `B:TS/EditMode/` | — | Files not copied; rewrite the listed patterns against Bulwark maths; add the walker tests of B§5.7. A§3.9, B§5.7 |
| **S12** `T:Tools/unity-queue.ps1` | `B:Tools/unity-queue.ps1` | mutex → `Global\BulwarkUnityQueue_4Megastructures` | Verify the Unity path; tighten the project match; log tags per C-23; `-StallMin` 0 for tests, ≥ 15 for imports/builds; `-BuildDir` → `BW_BUILD_DIR`; keep comma split, graceful close, lockfile removal, Burst filter. A§3.10 |
| **S12** `T:Tools/run-player.ps1` | `B:Tools/run-player.ps1` | exe `Builds\Bulwark\Bulwark.exe` | `-Exe`/`-BuildDir`; tag greps; stall 0 or ≥ 35 min; `-ArgString`; `-Long` through `gpu-guard.ps1` (VRAM 6500/5000 MiB; exits 75/76/124); read the QA JSON `pass`; refuse a concurrent renderer; log `nvidia-smi`. A§3.10 |
| **S12** `T:Tools/fastcompile.py` + features of `R:Tools/fastcheck.py` + per-run idea of `A:Tools/fastcompile.ps1` | `B:Tools/fastcompile.py` | — | Per-run output `Tools/fastcompile/runs/<id>/` (`FC_RUN` or pid); package DLLs filtered by asmdef `references`; `--only`; mscorlib shim note; always finish with a Unity compile. A§3.10, B§4 (C-19) |
| **S12** `T:Tools/gen_hlsl.py` | `B:Tools/gen_hlsl.py` | inputs/outputs/guards → Bulwark | Regex must match the `BulwarkPalette.Table` syntax; byte-identical to `ShaderGen`. A§3.10 |
| **S12** `T:Tools/metrics.py` | `B:Tools/metrics.py` | — | `--folder-glob "*4-Megastructures*"`, `--cwd-marker "4 Megastructures"`, `--anchor` (first line of `STATE.md`), session search across all project folders, records ≥ anchor, wall-clock from the anchor, exact exclusion of `W--YouTube-Megastructures`, fix `folder_confirmed()`; keep dedup, cache split, gaps, `long_context`, outputs; test early. A§3.10 |
| **S12** `T:Tools/pricing.json` | `B:Tools/pricing.json` | — | Template only; re-fetch the official page at the end (15.3.4). A§3.10 |
| **S12** `T:Tools/qa/qa_common.py` | `B:Tools/qa/qa_common.py` | — | `CONSTANTS_CS` → `BulwarkConst.cs`; rewrite `Spec`; delete `_FALLBACK`; `TH` → 14.10.3 thresholds; keep the report.json contract. A§3.10 |
| **S12** `T:Tools/qa/encode_test.py` + `A:Tools/qa/encode_test.py` merges (+ band rule from `A:Tools/qa/abyss_qa.py` `encode_band_runs`) | `B:Tools/qa/encode_test.py` | — | Keep Thread's ffmpeg flags; add `+bitexact`, encoder check, worst-location crops, the step-edge band rule as information; dithered vs undithered pair (C-07) with gate ≤ 0.6× on the 4K CRF 23 decode, CRF 23 and 30. Never `R:Tools/encode_test.py`. A§3.10, B§3 (C-20) |
| **S12** `T:Tools/qa/image_metrics.py`, `sequences.py`, `audio_analysis.py`, `contact_sheet.py`, `run_all.py`, `selftest.py` | `B:Tools/qa/` (same names) | — | `sequences.py`: remove T-SWAP, bank, "D". `audio_analysis.py`: 4× true peak, 3 s RMS vs `BulwarkConst`; remove −9 dBFS momentary, LUFS −30, wind-off. Run `selftest.py` early; check `python -I -c "import numpy, PIL"`. A§3.10 |
| **S12** `T:Tools/qa/regions_default.json` | `B:Tools/qa/regions_default.json` | — | Keep `_doc` and an empty `regions` list. A§3.10 |
| **S12** `T:Tools/qa/run_encode_clips.sh` | `B:Tools/qa/run_encode_clips.sh` | — | Rewrite clips and regions (storm sky, dusk sky, deep water veil, mist); `run-player.ps1 -Long`; undithered twin; delete frames after measuring. A§3.10 |
| **S12** `A:Tools/lookdev.ps1` | `B:Tools/lookdev.ps1` | `Abyss.exe` → `Bulwark.exe`; `ABYSS_BUILD_DIR` → `BW_BUILD_DIR` | Launch through `run-player.ps1` (gpu-guard); split `-Extra`; `-Shots` → `-capture-set`/`-capture-only`. B§3 |
| **S12** `T:PLAY_WINDOWS.bat` | `B:PLAY_WINDOWS.bat` | `Thread` → `Bulwark` | `Game\Windows\Bulwark.exe`, `Builds\Bulwark\Bulwark.exe`. A§3.10 |
| **S12** `T:.gitignore` (two optional lines) | `B:.gitignore` (already present) | — | Optionally add `/Assets/Resources.meta` and `/Assets/Resources/`. `.gitattributes` and `Tools/gpu-guard.ps1` are already in `B:`. A§3.10 |
| **S13** `R:docs/BUILDER_BRIEF.md`, `R:docs/CRITIC_BRIEF.md` | `B:docs/BUILDER_BRIEF.md`, `B:docs/CRITIC_BRIEF.md` | — | Templates. Root path; read-only file list for builders; fast check command; `unity-queue.ps1`/`run-player.ps1` (gpu-guard for long or 4K jobs); `BW_BUILD_DIR` per owner; 6–8 cycles; critic JSON (lens, run, findings S1/S2/S3, `beat_verdicts`); Part 11.13 sight-line evidence; Part 5.8 "look at your own captures". B§4 |

**Generated or regenerated, never copied:** every `.mat` (including `Grade.mat`) and `MaterialLibrary.asset` (MaterialFactory); `Main.unity` (SceneBuilder); `BulwarkPalette.asset` (PaletteWriter); `BulwarkLighting.lighting`; `Shaders/Generated/BulwarkPalette.hlsl` and `BulwarkConstGen.hlsl` (`gen_hlsl.py` / ShaderGen); the theme `.tss` and PanelSettings asset (ProjectConfigurator).

**New files the copied ones expect (written for Bulwark, no source):** `BulwarkAtmosphere.hlsl` (`BwFogApply`, curvature twin), `BulwarkSchedule`, `BulwarkScore`, `SeaState`, `MistVolumeFeature`, `IKeyShotSubjects`, `BwPath`, `BwEvents`, and the Bulwark material shaders (`BwSea`, `BwFarField`, …).

**Never copied (summary):** A§8 and B§8.2. Notably Thread `Mesh/MeshKit.cs`, `CinematicPlan`, the Thread beats, materials, scenes, ProjectSettings and docs; Abyss `SceneClock`, `ClockDriver`, `KeyShots`, `QaSequences`, `QaBots`, `QaDrivers`, `LiftController`, `FinalDither`; Ring `AutoWalker`, `CameraRig` (file), the `PathGuide` body, `RingFinalDither`, `RingBend.hlsl`, `QaHooks`/`RingQaHooks`, `R:Tools/encode_test.py`; any `.meta` file.

---

## Conflicts and decisions to make

Each item below is a place where Thread, Abyss and The Ring have a class with the same role, or where the two surveys (and the research) disagree. Record every decision, with its reason, in `DEVELOPMENT.md` before copying the affected rows.

### Open: the surveys disagree, or a choice is explicitly left open

**C-01 Player controller.**
- Candidates: Thread `PlayerController` (lane coordinates, (s, lateral) in double, analytic floor and clamp, capsule check on layer 30); Abyss (CharacterController plus kinematic lift); The Ring (CharacterController, in-code InputActions, safety snap, footsteps).
- Survey A: Part 3.1 imports only `CameraRig` and `ThreadInput` from Thread's player; "the walker comes from The Ring"; Thread's `PlayerController.cs` is in "Files NOT to copy" (helix-coupled).
- Survey B: **Conflict** in the spec itself: Part 3.1 says take The Ring's, Part 14.1 lists Thread's, and Part 1.6.1 makes Part 14 win. **Recommends Thread's**, generic once the path API is rebuilt on `FilletPath` (`BwPath`), with a CharacterController (radius 0.35, slope 45, step 0.3) kept as the physical check (`cc.Move(analyticTarget − pos)`; rejections over 1 cm counted and projected back; y from the analytic floor). From The Ring: the 0.2 m camera probe, the Alt+Enter guard, the safety snap, the CC settings and the cancel rule. Log it in DEVELOPMENT.md.
- Decide: which controller is the base; log the Part 3.1 vs 14.1 arbitration.

**C-02 Cinematic walker (AutoWalker, planner, gaze).**
- Candidates: Thread `AutoWalker` (shell plus `CinematicPlan` slowdowns), Abyss (`ProfileSpeed` zones, H7 hold), The Ring (`AutoWalker` + pure `WalkPlanner` + `GazeSchedule`).
- Survey A: Thread's `AutoWalker.cs` is not copied (The Ring's is used per 3.1); `AutoTestRunner` must drive the 2.5 m/s cinematic through "the AutoWalker".
- Survey B: **Thread shell, Ring planner and gaze core** (B§5). The Ring's `AutoWalker` is not copied: its `Activate()` and `QaHooks` line 326 force the cinematic to 1.5 m/s (the 3.1 (c) bug class). No holds, zones or catch-up; 0.5 s start ramp; gaze ω 2.5 with lead; rate limits 18 / 13 deg/s.
- Decide: which `AutoWalker` file is the base. Whichever wins must satisfy B§5 (constant 2.5 m/s, no slowing, same walker for `-autotest` at 1.5 and `-autotest -cinematic` at 2.5).

**C-03 PathGuide (colliders and lane function).**
- Candidates: Thread (colliders generated from `HalfWidth(s)`, kill volume, `CoverageCheck`, `CapsuleBlockedAt`), Abyss (`CorridorKit` helpers), The Ring (hand-listed `Shapes` lane table, `DistanceOutsideLanes`, `LaneHalfWidth`, `LaneSamplePoints`, fallback floors).
- Survey A: Thread's `PathGuide.cs` is not copied (The Ring's is used); The Ring's `PathGuide` must expose `Instance`, `KillCount`, `CoverageCheck` or `PathGuideProbe` skips silently.
- Survey B: **Thread's generator driven by one Bulwark lane function**; lesson 3.2 #11 bans hand-listed span tables (58 misses). Ring conventions (1 m thick, 6 m high, 1 m overlap, boxes every 4 m), Ring API names plus `FloorY(s)`; `CorridorKit` for ramps; ray grid 0.5 m. The Ring's `PathGuide` body and `Shapes` table are not copied.
- Decide: base file. Either way the lane is one function, and `Instance`/`KillCount`/`CoverageCheck` exist.

**C-04 Title, fade, pause and ending UI.**
- Candidates: Thread `FadeController` (fade inside the grade via `GradeState.FadeBlack`) + `GlyphText` (UGUI glyphs) + `PauseOverlay`; Abyss UiViews; The Ring `UiDirector` + `UiTimeline` (UI Toolkit).
- Survey A: copy `FadeController` as the grade-fade bridge (A§9 step 5), `GlyphText` as an optional fallback, `PauseOverlay` as a logic pattern; text and pause rendering go to `UiDirector`/`UiTimeline`.
- Survey B (and RESEARCH C§9.5): **The Ring wins** (14.11 names it). Do not copy Thread's three UI files: two fade owners and two pause UIs. `UiDirector` implements `IFadeUI`, writes `GradeState.FadeBlack` with `ShaderFade = true` (fade before the one dither), owns pause (`IFadeUI.Paused`); execution order −800 (Thread's `FadeController` slot; Ring used −90).
- Also affected: `UnityEngine.UI` asmdef reference (droppable without `GlyphText`), `SceneBuilder` contract ("FadeController / The Ring's UiDirector"), Thread `Validate` row `UI.FadeController`.
- Decide: one fade owner and one pause owner.

**C-07 Dither command-line flags.**
- Survey A: `-dither static|temporal` (default static), `-freeze-dither` alias of static, `-dither-lsb X` with X = 0 as the undithered baseline for the 0.6× encode gate.
- Survey B: `-dither static|temporal|off`; `-dither off` gives the undithered half of the 14.10.3 pair.
- Decide: whether both `-dither off` and `-dither-lsb 0` exist (and are equivalent), and which one `encode_test.py` and `run_encode_clips.sh` use.

**C-23 Log tags (the log-scan key).**
- Survey A: `[BULWARK]` and `[BULWARK-FAIL]` (runtime, `EditorCommon` prefix, `unity-queue.ps1` and `run-player.ps1` greps).
- Survey B: `[BW]` and `[BW-FAIL]` (replacing `[ABYSS]`, `[ABYSS-ERROR]`, `[Ring]`).
- RESEARCH C§9.6 writes the UI font line as `[Bw] UI font in use: …`.
- Decide: one tag pair, used by every runtime site (about 60 Thread sites plus the imported Abyss/Ring files) and by both PowerShell greps.

**C-27 Thin-line geometry.**
- Abyss `MeshKit` writes UV1/UV2 "ThinLine" data for Abyss's `AbThinLineExpand`; Thread's `BwLine` (`_LINE_TUBE`) pushes along smooth clip-space normals.
- Survey B: either build rails with smooth `Cylinder` tubes (thinLine off) or teach `BwLine` the UV2 axis push. Abyss lesson: long members need a vertex ring at least every 4 m for the widening shader.
- Decide: one thin-line method for `BwLine`.

**C-28 Tier-fade pattern (`DetailTier`).**
- Abyss uses a static IGN complementary pattern (`_AbFade`, ADR 6). Lesson 3.2 #6: IGN repeats every 13 rows; Thread D-042: an ordered-dither clip showed as screen-door stipple.
- Survey B: replace it with the blue-noise tile or alpha-to-coverage under MSAA. Survey A §7.11: fade by A2C, never with an ordered-dither clip.
- Decide: blue-noise tile or A2C.

**C-29 Hiding UI during captures.**
- Survey A (`CaptureHarness`): `HideUi()` only disables UGUI `Canvas`; also disable `UIDocument`s.
- Survey B §7 (The Ring) and RESEARCH C§9.1: keep the UIDocument enabled; it clears its code-built tree on every OnEnable/OnDisable, so hide with opacity, visibility or `display`. B§6.2: the UI overlay is never in the capture render texture; `SetOverlayTarget` and `ForceIntroTime` handle title stills.
- Decide: how `CaptureHarness` hides UI without toggling `UIDocument`.

### Settled by the surveyors (record the decision)

| ID | Role | Thread | Abyss | The Ring | Surveyors' recommendation |
|---|---|---|---|---|---|
| C-05 | Clock | `ThreadClock` (+ `GradeState` in the same file) | `SceneClock`, `ClockDriver` | `RingState.T`, `AnimT`, `Dt` | **Thread** → `BwClock`, with Abyss `RealTime`, `Wrapped(period)`, pause freeze, `_BwTime`, `-clock T`, `-freeze-clock` alias merged in; `GradeState` split out with WB, tint and dither peak. Abyss clock files not copied (A§3.2, B§2). |
| C-06 | Shared state | `ThreadServices`, `WorldContext`/`FrameState` | `GameState` | `RingState`, `RingRig`, `RingEvents` | **Thread services primary**; slim Abyss `GameState` for UI/audio flags; `BwEvents` from `RingEvents`; one writer per field (B§2). |
| C-08 | Bootstrap and shader warm-up | `GameBootstrap`, `Shader.WarmupAllShaders()` | load sequence, `InstancedRegistry.Clear()`, `WarmupLoader` | `Restarting`/`CinematicOnLoad`, set pre-warm | **Thread** `GameBootstrap` + Abyss `WarmupLoader` behind black (with a point-light state) + registry clear + 14.6 set pre-warm + Ring cinematic-through-restart (A§3.3, B§2, B§3). |
| C-09 | Beat builder interface | `IBeatBuilder { Name; Build(WorldContext) }` | `BeatBase` | `IBeatBuilder { Order; Name; IEnumerator Build(BuildContext) }` | **Thread**; ported Ring code must not implement Ring's interface (B§2). |
| C-10 | Camera rig | `CameraRig` (raw mouse, no roll, `SetLook`, `SetPoseOverride`) | — | 0.2 m `CameraProbe`, `ComputePenetration`, `Violations`/`MaxRollDeg` | **Thread** plus Ring's probe and QA counters (14.4) (A§3.4, B§2). |
| C-11 | Mesh kit | `Mesh/MeshKit.cs` | 4-file `MeshKit` + `MeshBuilder` | `Core/MeshKit.cs` | **Abyss**; Thread's and Ring's are not copied (same class names); add `TriangleCount` (A§3.2, B§2). |
| C-12 | Instanced draws | none in the framework | `InstancedBatch` + registry (`RenderMeshInstanced`) | `InstancedField` (`RenderMeshIndirect`) | **Keep both** with one capture rule: drivers do not submit in capture mode; `CaptureHarness` calls both `SubmitAllFor(cam)` before each request. BwLit passes need instancing for `InstancedBatch` (B§2, B§3, B§4). |
| C-13 | Globals and atmosphere writer | `ProgressDirector.Apply()` (≈ 20 globals, sun, `GradeState`) | — | `AtmosphereDriver` (one writer, slews) | **The Ring's `AtmosphereDriver`**, called from Thread `ProgressDirector.Apply()`; Thread helix uniform writes deleted (A§3.3, B§2). |
| C-14 | Post-processing and dither | `ThreadGrade*` | `FinalDither.shader` (IGN) | `RingFinalDither`, URP Volume overrides | **Thread `BwGrade`** (rule 3.1.6); `ApplyVolume` rewritten to write `GradeState`; no Volume overrides; one dither (A§3.1, A§4, B§2, B§7). |
| C-15 | Material library | `MaterialLibrary` (named fields) | list + `AllVariants` | `RingAssets.Get(id)` | **Thread**, plus `All()`; `InstancedField.Create` takes a `Material` (B§2). |
| C-16 | Quality bootstrap | `QualityBootstrap` | yes | yes (`ToggleFullscreen`) | **Thread**, plus the 60 fps cap for bots, soak and long captures (A§3.4, B§2). |
| C-17 | Maths and path API | `ThreadMath` (path API helix-specific) | `VertexBake.Pcg` | `RingMath` | `BwMath` = Thread generic part + RingMath helpers; path API → `BwPath` on `FilletPath` (B§2, B§6.1). |
| C-18 | Collider layers | `PlayerTuning.CorridorLayer = 30` | `CorridorKit.LayerCorridor = 8`, `LayerSafety = 9` | — | **Thread 30**; Safety 31; point `CorridorKit` at them (B§2, B§3). Survey A keeps layer 30 only if `PathGuide` uses it. |
| C-19 | Fast compile | `Tools/fastcompile.py` | `fastcompile.ps1` (`FC_RUN`) | `Tools/fastcheck.py` (asmdef refs, `--only`) | **Thread base**, Ring features, per-run folders (A§3.10, B§4). |
| C-20 | Encode test | `Tools/qa/encode_test.py` | same + `+bitexact`, step-edge rule, crops | `Tools/encode_test.py` (default swscale, wrong) | **Thread base + Abyss merges**; never the Ring version (A§3.10, B§2, B§3). |
| C-21 | Key-shot table and sequences | `ShotPlan` | `KeyShots` (`pose:` token), `QaSequences` | `CapturePlans` | **Thread `ShotPlan`** + Abyss `pose:` parser and sequence concepts (B§2, B§3). |
| C-22 | Wall bot and ray grid | `WallBot`, `PathGuide.CoverageCheck` | `QaBots` (wallhug, raygrid, ceiling/side rays) | `WallTest` (never run in the player) | **Thread** + Abyss ray-grid reporting (B§2, B§3). |
| C-24 | Master limiter | memoryless `0.84·tanh(x/0.84)` | — | `DspLimiter` (look-ahead) | Survey A: add a look-ahead limiter, asymptote −1.42 dBFS. RESEARCH C§5: port The Ring's `DspLimiter` with ceiling −1.42, threshold −4, look-ahead 16 ms, hold 20 ms, release 250 ms, `Cap` 4096; remove Thread's tanh. |
| C-25 | Audio host and footstep routing | synth on the listener, footsteps as AudioSources | — | generator as the first filter on a clip-less AudioSource | **Generator on a clip-less AudioSource** (3.2 #14 overrides Thread); footsteps mixed inside `BwDsp` (A§3.7, A§7.15, B§7, RESEARCH C§0, C§6.4). |
| C-26 | Audio director | `AudioDirector` (pool, listener path) | — | `AudioDirector.CreateRig` | Thread pattern rewritten, with The Ring's `CreateRig` + 48 kHz `Reset` + `Play()` after `AddComponent`; never on the camera (A§3.7, RESEARCH C§1.1, C§8). |
| C-30 | Execution order | GameBootstrap −1000, QualityBootstrap −900, FadeController −800, CameraRig/QA −50, AutoWalker −40, PlayerController −30, PathGuide −20, Progress 500, Beat 600, Audio 700, CaptureHarness 32000, PerfMonitor 32100 | BackdropRig 150, InstancedRegistryDriver +200 | UiDirector −90 | Merged (B§6.3 + A§7.10): UiDirector takes −800 (C-04); CameraRig LateUpdate −50; BackdropRig 150; InstancedRegistryDriver 200; ProgressDirector then AtmosphereDriver at 500; InstancedFieldDriver 32000. |

### Other decisions the surveys flagged (not class-role conflicts)

- **Naming rule:** files without a game prefix keep their names; 14.3's "renamed Bw*" is read as applying only to `Thread*` names (A§1).
- **Bloom clamp** 64 → 40 (A§3.1).
- **Contrast** at pivot 0.18 before the curve, with toe protection (without it Y 0.0018 at k 0.04 maps to 0.0015, about sRGB 5) (A§4.3).
- **Toe-less Neutral** kept (B03-1) even though Bulwark is PBR (A§4.2).
- **Dither:** static default; peak 2.0 in beats 1, 6, 7 and 1.5 in beats 2–5, 30 m crossfade; optional fade width scaled to A (A§4.2).
- **Additional-light shadows** on or off (Bulwark has at most 4 local lights) (A§3.8).
- **1.5 m/s start ramp:** a 0.5 s ramp gives a trigger at 999.25 s, the spec's 999.15 implies a 0.15 s loss; record which ramp the 1.5 m/s walk uses (B§5.2).
- **Spec deviation logged for the walker:** gaze rate limits 18 / 13 deg/s instead of Thread's 60 / 40; key pairs closer than 1 m become ≥ 4 m ramps (B§5.3).
- **Frame cap:** 60 fps for autotest, wall bot, soak and long captures; the 1.5 m/s autotest becomes a ≈ 17 min GPU job (A§7.14).
- **Graphics API:** Validate must require exactly `[Direct3D11]`; the Editor uses a changed API only from its next start (A§6, A§7.8).
- **`metrics.py` anchor:** Thread's script has none; add `--anchor` from the first line of `STATE.md` (A§3.10).
