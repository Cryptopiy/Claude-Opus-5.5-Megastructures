# IMPORT LOG — Phase 2 framework import (integrator)

Work order: `docs/IMPORT_PLAN.md` sections 2-8 (11 steps of section 7, compile gate after each). Sources are read-only:
T = Thread (`W:\YouTube\Megastructures\3rd Megastructures\3rd Megastructures`), R = The Ring (`...\2nd Megastructures\2nd Megastructures`),
A = Abyss (`...\1st Megastructures\1st Megastructures`). No `.meta` file is copied (Unity creates new GUIDs).

Resume rule: find the last step marked GREEN below, check `git log`, continue with the next step of the plan's section 7.

## Commands that work (verified in this run)
- GPU state: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/gpu-guard.ps1 -Status`
- gpu-guard exit code pass-through (plan 1.3): `powershell -NoProfile -ExecutionPolicy Bypass -File Tools\gpu-guard.ps1 -Label exitcode-check -TimeoutMin 1 -FilePath cmd.exe -ArgumentString "/c exit 3"` -> exit code 3 (verified 2026-11-13 20:40).

## Steps

### Step 1 — folders, asmdefs, asmref, tools
Files: `Assets/_Game/{Scripts/{Runtime,Editor,Tests/EditMode,Tests/PlayMode},Shaders/{Include,Generated},Beats,Materials,Settings,Scenes}`;
asmdefs `Megastructure.Runtime` / `.Editor` / `.Tests.EditMode` / `.Tests.PlayMode` and `Beats/Megastructure.Beats.asmref` verbatim from T.
Tools: `fastcheck.py` (R; per-run output folder `Temp/fastcheck/<pid>`, removed after the run), `gen_hlsl.py` (T; reads
`NocturneConstants.cs` / `NocturnePalette.cs`, writes `Shaders/Generated/NocConstantsGen.hlsl` (`NC_*`) and `NocPalette.hlsl` (`PAL_*`)),
`qa/*` (T; constants path, Nocturne fallback beats/names/RMS, 14.10 clip / p1 / mean-luma thresholds), `lookdev.ps1` (A; rewritten:
`NOC_BUILD_DIR` slot build, player through `run-player.ps1 -Exe`), `PLAY_WINDOWS.bat` (T), `docs/BUILDER_BRIEF.md`, `docs/CRITIC_BRIEF.md` (R, Nocturne paths).
`run-player.ps1`: new `-Exe` parameter (lookdev slots); heavy detector also routes `-autotest-until` and the long `-capture-set`
values (sequences, transitions, statics, pans, edges, encode, all, everything, boundaries, signs, rain, koi, traffic) through gpu-guard (plan 1.2).
Gate U: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity-queue.ps1 -Name imp_1 -TimeoutMin 20` -> exit 0 in 106 s;
only "assembly has no scripts" warnings (expected for empty asmdefs) and the usual licensing / Burst pipe noise. **GREEN.**

### Step 2 — core pure C#
Files (`Scripts/Runtime/Core`): `NocturneConstants.cs` (new: 14.12/14.4 numbers, path waypoints / sharp cum / radii, BEAT_S, goal
1497/1500, 14.2.4 schedules as knot curves, per-beat post/rain tables, dither, UI timings of 14.11, 14.5 gaze keys (repeated
(585,-2,+2) kept once), 14.10 key shots in Thread's `KeyShot` shape, autoplay expectations), `NocturnePalette.cs` (from T
`ThreadPalette`: 28 tokens of 14.2.1 + `SpecLinear` column, enum `NocPal`), `NocMath.cs` (T `ThreadMath` easing/D3/beat blend +
R `RingMath` hashes, value noise, critically damped followers, slew in double; `LimitRate`; knot schedules), `NocSchedule.cs`
(new: vFOV / EV / fog sigma / rain / bloom / sat / vignette of s; `NocFogMath` = C# mirror of the 14.2.2 fog and sky gradient),
`NocClock.cs` (T `ThreadClock` + A `SceneClock.Wrapped` + rain clock + `DitherMode` (static default) + `GradeState` without
Darkness, with DitherPeak / DitherChroma / FadeBlack), `NocServices.cs` (T interfaces; Nocturne surfaces; `IFadeUI.EndingDone`),
`NocRig.cs` (R RingState/RingRig/RingEvents trimmed: NocState / NocRig / NocEvents + 14.4 surface table), `CliArgs.cs` (T + 4.5
flag set; `-progress` is a fraction; `-autotest-until`), `WorldContext.cs` (T; CityRoot / BackdropRoot / BeatRoot(b); triangle
count by index counts; no shadows), `MaterialLibrary.cs` (graybox fields; `All()` by reflection), `ShaderIds.cs` (`_Noc*` globals).
`Scripts/Runtime/Rendering`: `BlueNoise.cs`, `R2Sequence.cs` (T verbatim + renames), `NocGradeMath.cs` (T; BloomClamp 30;
`DitherAmplitude(peak, Y')` of 14.7 H1; D-007 zero-luma chroma offset; Darkness gain removed).
Generated: `python -I Tools/gen_hlsl.py` -> `Shaders/Generated/NocConstantsGen.hlsl` (NC_*), `NocPalette.hlsl` (PAL_*).
Gate F: `python -I Tools/fastcheck.py all` -> runtime OK / editor OK (~9 s). Gate U: `Tools/unity-queue.ps1 -Name imp_2` -> exit 0 (20 s). **GREEN.**

### Step 3 — Abyss geometry, backdrop, instancing
Files: `Scripts/Runtime/Geometry/` MeshBuilder, MeshKit (+ .Loft, .Revolve, .Structures), MeshValidation, VertexBake,
CorridorKit (A; layers 30 Corridor / 31 Safety, tags [NOC]); `Scripts/Runtime/Rendering/` BackdropRig (A; follows
`NocRig.Camera` / `Camera.main`), InstancedBatch (A; `_NocFade`, receiveShadows default false, new `DrawFor(Camera)`),
InstancedRegistry (A; new `SubmitAllFor(Camera)` for capture cameras, lesson 8), InstancedRegistryDriver (A).
WorldContext counts triangles itself (step 2). Tests: `Scripts/Tests/EditMode/MeshKitTests.cs` (A, 33 tests; Abyss constants
replaced by a local `Fx` fixture class).
Gate F OK (26/27 files). Gate T: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity-queue.ps1 -Name tests_3 -NoQuit -StallMin 0 -TimeoutMin 30 -ExtraArgs "-runTests,-testPlatform,EditMode,-testResults,<abs root>\QA\reports\tests_3.xml"`
-> exit 0, 33/33 passed (also the compile gate). **GREEN.**

### Steps 4 + 5 — player, systems, UI (one gate: CameraRig reads ProgressDirector.S, so step 4 does not compile alone)
Player (`Scripts/Runtime/Player`): `FilletPath` (R generic ctor, ported to double; s = filleted distance; SharpS/DAtSharp only for
the 14.4 table), `NocLanes` (new: 14.4 lanes as one function of s, transitions documented in the file; Ring PathGuide API),
`GazeSchedule` (R; 14.5 keys keyed to s, yaw relative to the heading at s; holds/glances/sway overlay/lateral table deleted),
`WalkPlanner` (R; phases Accel/Cruise/Stop/Done, 0.5 s / 0.2 s ramps, exact trigger crossing at s 1497 then StopRamp T = 6/v0,
follower omega 2.5 + LimitRate 22/18 deg/s, sway <= 0.3 deg, all state double, lag measured along the tangent),
`PlayerMath` (T; PlayerTuning (layers 30/31), stride lerp(0.75, 0.85), head bob 0.008, StopRamp GoalS 1500; EyeFollower dropped),
`NocInput` (T ThreadInput), `CameraRig` (T; near 0.05 / far 12 000, vFOV from NocSchedule, SnapEye, 0.2 m sphere check counter),
`QualityBootstrap` (T; 60 fps cap for -autotest/-autotest-until/-soak/-wallbot/-sequence/captures, uncapped only with -perf-log),
`AutoWalker` (R; QA requests only, cinematic always 2.5 m/s, no QA hook forcing speeds), `PlayerController` (R + IPlayerState;
NocInput only, no fullscreen code, goal stop ramp for manual walking, footsteps to NocEvents + IAudioHooks, safety y < -3 /
> 1 m outside lanes), `PathGuide` (T generic builder fed by NocLanes: 1 m floor boxes top y 0, 6 m walls at lane + 0.38,
segments 10 / 2 / 0.5 m chosen from the lane slope and the arcs, end caps, kill triggers y -5, CoverageCheck, KillCount).
Systems: `GameBootstrap` (T; resets NocState/NocEvents/InstancedRegistry, dither flags, WarmupLoader 3 frames behind black instead
of Shader.WarmupAllShaders, LoadComplete), `ProgressDirector` (T skeleton; NocState writer; AtmosphereDriver call; _NocScreen),
`BeatDirector` (T; entry at the exact boundary, content window 8 m lead / 12 m behind), `AtmosphereDriver` (R pattern, new code:
schedules, EV/bloom slew, capture bypass, globals, sky fill light without shadows, trilight, LightCal), `Rendering/WarmupLoader` (A).
UI: `UiDirector` (R; IFadeUI, ShaderFade = true -> GradeState.FadeBlack, 14.11 typography/timings, Blink, PanelSettings/font fields
instead of RingAssets, gamepad Start left to the PlayerController), `UiTimeline` (R; re-timed ending 1.5 + 4.0 / line 7-17 s, audio 6 s).
Tests: `PathScheduleTests` (13: length 1500.007, tangent points, arc midpoints/centres, headings, x = s - 184.70, sharp table,
projection, lanes, stop ramp, cinematic trigger 599.050 / stop 601.450 s, QA 998.100 / 1002.100 s, frame-rate independence, key
spacing, follower readability at every KS (max 1.4 deg yaw / 2.1 deg pitch)), `ConstantsTests` (10).
Gate F OK. Gate T: `... unity-queue.ps1 -Name tests_5 -NoQuit -StallMin 0 ...` -> 58/58 passed, 0 compiler warnings. **GREEN.**
Note: the vFOV peak slope is 0.3125 deg/s (spec rounds to 0.3); the test pins 0.3125 and the 1 deg/s Part 9 limit.

### Step 6 — rendering
Files: `Rendering/NocGradeFeature.cs`, `NocGradePass.cs` (T; renames, `[DisallowMultipleRendererFeature("Noc Grade")]`, P1.w = GradeState.DitherPeak
(0 with -dither off), new P3.x = chroma amplitude, R2 offset from NocClock (frozen unless -dither temporal), no Darkness),
`Shaders/NocGrade.shader` (T; passes NocBloomPrefilter/Down/Up, NocGrade; props `_NocGrade*`; step 6 = per-pixel luma amplitude
2.0 x lerp(1, 0.6, saturate((Y' - 0.35)/0.4)) on the encoded Rec.709 luma + D-007 chroma +-1 code per 2 x 2 block (zero-luma
BT.709 offset); bloom clamp from C# = 30), `Shaders/Include/NocCommon.hlsl` (globals, PCG hash, value noise, filtered grid),
`NocAtmosphere.hlsl` (14.2.2 interface: NocSkyColor (gradient + D-006 below-horizon blend), NocFog (T = exp(-(sigma_eff d)^2),
fog colour = sky colour), NocFloor), `NocLitCore.hlsl` / `NocLitPass.hlsl` / `NocLighting.hlsl` (R lit template, flat world, trilight,
analytic sky reflection, Forward+ loop, shadow keywords dropped, NocFog then NocFloor last), `NocGraybox.shader` (R RingCeramic
structure), `NocSky.shader` (dome around the camera in the vertex shader, r 11 000).
Gate U: `Tools/unity-queue.ps1 -Name imp_6` -> exit 0, no shader or compiler errors in the import log. **GREEN** (variant compile
errors are checked by Validate / ShaderUtil in step 7).

### Steps 7-10 + graybox builders — editor tools, QA runtime, audio, NocLine (one gate run: SceneBuilder resolves the QA and
audio components by name, so RebuildAll / Validate need all of them)
Editor (`Scripts/Editor`): `EditorCommon` (NocPaths / NocLog ([NOC] / [NOC-FAIL]) / EditorUtil), `BatchTools` (RebuildAll, BuildWindows
with `NOC_BUILD_DIR`, CompileCheck, Validate with 66 checks: scene contract, every material vs its contract, every shader compiled per
material pass + ShaderUtil errors + Always Included, D-008 pipeline, D3D11 only, input, product/company, Mono, 48 kHz, layers, UI assets,
lesson-5 global names), `ProjectConfigurator`, `PipelineConfigurator` (D-008: HDR 64, MSAA 4, Depth_32_Stencil_8, depth/opaque textures
off, all shadows off, additional lights per pixel, Forward+, GRD off, SSAO removed, NocGradeFeature + Grade.mat, one quality level),
`SceneBuilder` (Main.unity: Game / Player / Audio / SkyFill / World / PathGuide; camera SolidColor = Floor, no skybox, post off, dither off,
shadows off, near 0.05 far 12 000; generator AudioSource; NocSky dome mesh + BackdropRig), `MaterialFactory` (graybox + Line + Sky + Grade),
`ShaderGen`, `PaletteWriter`, `TemplateCleanup`, `CaptureWatchdog` (T, renamed), `NocUiSetup` (R: NocTheme.tss, NocPanel.asset).
Shader: `Shaders/NocLine.shader` (T ThreadLine: tube push + ribbon image line + A2C energy floor 0.35; Thread flat shading, cable shadow,
fades and shadow keywords stripped; lit by NocLighting, NocFog, NocFloor).
QA (`Scripts/Runtime/QA`, from T): QaJson, QaReport, QaCommon (PathGuideProbe "Megastructure.PathGuide"; QA frame cap 60), QaRunnerBase,
AutoTestRunner (rewritten for 14.10: walk / brisk / cinematic modes, `-autotest-until S`, trigger / stop / 17 s ending / beats at the
boundaries / safety / camera sphere checks), WallBot (0.40 m inside both lane edges through the AutoWalker edge mode, progress-drop check),
SoakRunner, PerfMonitor, CaptureWriter, QaProjection (Thread subjects removed; box subjects; KS19 goal window), ShotPlan (14.10 sets,
`keyshots1080`, free-look +-90/180, +60, -60 for beats 3-4, Nocturne windows and encode views, `pose:` parser of A KeyShots),
CaptureHarness; new `NocVisibility` (A CompositionLint maths).
Audio (`Scripts/Runtime/Audio`): `NocDsp` (T ThreadDsp blocks 1-334 + R DspHash/DspBrown/DspSmooth/DspSmoothRandom/DspLimiter),
`WavWriter` (T), `NocMixer` (new generator chain: placeholder rain bed + footstep voices -> DC 20 Hz -> FDN -> master -> limiter -1.5 dBFS),
`NocSynth` (generator on a clip-less AudioSource, mailbox, capture), `OfflineAudioRenderer` (block loop + sinks, speed parameter),
`AudioDirector` (IAudioHooks, FadeAudio x pause duck, -audio-dump).
Beats: `Beats/GrayboxKit.cs` + `Beat01_Alley` .. `Beat07_Heart` (one IBeatBuilder each: floor slab lane + 1 m, start posts, edge markers;
beat 7 adds the 14.4 m tenement and the warm window).
Tests added: GradeTests (T, 23: Thread sun/beacon/dark-gain tests replaced by 14.7 H1 amplitude, D-007 chroma, static/temporal/off, clamp 30,
palette darks), QaTests (T generic + Nocturne plan tests), ProjectTests (new, 10), AudioTests (7), PlayerTests (7), PlayMode InputTests (T).
Commands (all exit 0):
- `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity-queue.ps1 -Name imp_7_compile -Method Megastructure.EditorTools.BatchTools.CompileCheck`
- `... -Name rebuild_1 -Method Megastructure.EditorTools.BatchTools.RebuildAll -TimeoutMin 30` -> SUCCESS, 0 problems (7 template leftovers removed)
- `... -Name validate_1 -Method Megastructure.EditorTools.BatchTools.Validate -TimeoutMin 30` -> VALIDATE PASS, 66 checks, 0 problems
- `... -Name tests_9 -NoQuit -StallMin 0 -TimeoutMin 30 -ExtraArgs "-runTests,-testPlatform,EditMode,-testResults,<abs root>\QA\reports\tests_9.xml"` -> 126/126
Warnings triaged: 5 x CS0618 (FindFirstObjectByType -> FindAnyObjectByType; LightingSettings.autoGenerate removed) fixed; none left.
Fix found by the tests: the FDN reverb blends from silence over 8 s unless `Snap()` is called after `SetTargets` (NocMixer now snaps). **GREEN.**

### Step 11 — build and smoke gate in the built player
- Build: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity-queue.ps1 -Name build_3 -Method Megastructure.EditorTools.BatchTools.BuildWindows -TimeoutMin 40`
  -> Builds/Nocturne/Nocturne.exe, release, Mono, [Direct3D11], 0 errors, 0 warnings (first build 104 s, incremental 13 s).
- Smoke: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/run-player.ps1 -Name smoke_until400 -TimeoutMin 12 -PlayerArgs "-autotest,-brisk,-autotest-until,400"`
  (routed through gpu-guard, 60 fps cap) -> exit 0: s 400.000 at t 160.10 s (expected 400 / 2.5 + 0.1 = 160.10), beats 1-3 at
  s 0.000 / 120.000 / 400.000, 0 errors, 0 kill-volume events, 0 safety snaps, collider coverage 0 misses of 48 204 rays, load 0.09 s.
  Report: QA/reports/autotest_brisk_until400.json.
- Fixes found on the way: (1) run-player started gpu-guard as a child powershell.exe and Windows PowerShell 5.1 re-quoted the
  -ArgumentString (paths with spaces) -> gpu-guard is now called in-process with a parameter splat; (2) the corridor floor boxes left
  gaps on the outer side of the fillet arcs (2 m chords, 13 m half-width) -> each floor box is extended by 2 h tan(dTheta / 2);
  (3) the coverage wall ray now reaches WallOffset(s) + the logged max wall shift (steep alley-mouth funnel; steep segments 0.25 m);
  (4) -autotest-until finishes one frame after the crossing so the BeatDirector logs the boundary beat; (5) the UiDirector timeline runs
  on game time (Time.deltaTime honours captureDeltaTime) so the 17 s ending is deterministic in the virtual-time autotest.
- Full cinematic autotest: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/run-player.ps1 -Name autotest_cinematic -TimeoutMin 15 -PlayerArgs "-autotest,-cinematic"`
  -> exit 0 (617 s real, 60 fps cap): trigger 599.050 s at s 1497.000 (expected 599.05 +- 2), stop 601.350 s at s 1499.999 (expected
  601.45; the runner took the frame where the speed fell below 1 cm/s, 0.1 s before the ramp end -> now waits for the walker's exact
  stop time), ending complete 17.00 s after the trigger, plan speed 2.490-2.512 m/s, beats at s 0/120/400/560/880/1080/1260 at
  t 0/48.25/160.25/224.25/352.25/432.25/504.25 s, 0 errors, 0 kill, 0 snaps, 0 camera-sphere contacts, coverage 0 misses.
  Report QA/reports/autotest_cinematic.json.
- Full 1.5 m/s autotest: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/run-player.ps1 -Name autotest_walk -TimeoutMin 25 -PlayerArgs "-autotest"`
  -> exit 0: trigger 998.100 s (expected 998.1 +- 3), stop 1002.100 s at s 1500 (expected 1002.1), ending 17.00 s, beats 7 in order at
  the boundaries, 0 errors, all checks green. Report QA/reports/autotest_walk.json.
- Capture smoke: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/run-player.ps1 -Name capture_smoke01 -TimeoutMin 10 -PlayerArgs "-capture,QA\captures\smoke01,-capture-set,keyshots1080,-freeze-clock"`
  -> exit 0 in 5.7 s: 20 key-shot stills at 1920x1080 through the RenderTexture path, 0 failures, 0 framing failures (KS19 goal window
  framed), 6 stills outside the 14.10 mean-luma windows (expected for an unlit graybox), 0 errors. Looked at KS00, KS04, KS10, KS19:
  sky gradient (14.2.2 + D-006 horizon blend), fogged floor slab, markers, NocGrade bloom on the warm window, no magenta, no NaN.
  report copied to QA/reports/capture_smoke01_report.json (QA/captures is gitignored).
- Final: EditMode 126/126 (`-Name tests_final`), Validate PASS 66 checks (`-Name validate_final`), build 0 errors 0 warnings (`-Name build_final`).

## Deviations from the plan
- Gates of steps 4+5 and 7-10 were run together (each step's code references the next: CameraRig -> ProgressDirector.S; SceneBuilder
  resolves the QA and audio components by name). Every step still got fastcheck; Unity gates as logged above.
- Audio: Thread's score (ThreadDsp class, ThreadAudioParams layers, FootstepBank recipes) is game content and was not imported; a
  placeholder rain bed + footstep voices in a new NocMixer carries the master chain (DC, FDN, limiter -1.5 dBFS). NocAudioParams is the
  control struct, not Thread's layer table. The 14.3 layers and footstep recipes are Phase-4 work.
- AtmosphereDriver is new code following The Ring's pattern (Ring's file is 90 % ship / sunset / Volume content).
- NocLanes transitions (alley mouth 4 m, gate B funnel 2.15 -> 3.5 -> 6.0, garden -> promenade and promenade -> plaza 12 m around the
  arc midpoints, median -> tongue over s 396-404) are interpretations of the 14.4 lane list; documented in NocLanes.cs.
- QaProjection keeps only the generic projection maths plus the KS19 goal-window subject; the per-shot subjects arrive with the beats.
- Python QA tools: constants path, fallbacks and the 14.10 clip / p1 / luma thresholds adapted; the 14.10 region file, the H1 encode
  gate and Thread-specific synthetic checks are not rewritten yet (selftest 79/85: the 6 failures are Thread's KF1 thread-line and
  wind-stem checks).
- PathGuide wall segments on steep funnels are 0.25 m (plan said 0.5) and the floor boxes get an arc extension; the coverage wall ray
  reaches WallOffset + max shift (35 cm at the alley mouth funnel).
- Main.unity is regenerated from scratch by RebuildAll, so a second run changes its local file IDs (semantically identical; Thread
  behaved the same); everything else is idempotent (second RebuildAll: 0 materials/settings changes).

## Open issues
- WallBot, SoakRunner, PerfMonitor, parity capture, sequences and the encode test were imported and compile but were not run in this phase.
- NocVisibility (11.13 audit) has the maths only; the EditMode audit writing QA/reports/visibility.json needs the beat subjects.
- 14.10 mean-luma windows fail on 6 graybox stills (no lights yet; LightCal not calibrated).
- The cinematic follower peaks at 18.05 / 16.86 deg/s (spec quotes 19.3 / 17.9); all key shots are within 1.4 deg yaw / 2.1 deg pitch.
- vFOV peak slope is 0.3125 deg/s at 2.5 m/s (spec text: 0.3).
- PlayMode InputTests were imported but only EditMode tests were run.
