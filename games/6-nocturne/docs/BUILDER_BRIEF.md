# Builder brief (template imported from The Ring, adapted for Nocturne)

Common rules for every beat builder of Nocturne. Your own prompt names your beat, your files and your tasks.

## Project
- Unity 6000.6.0f1, URP 17.6, procedural first-person walk through a night megacity in rain. Root:
  `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures` (the path has spaces: quote it everywhere).
- Spec: `MASTER_PROMPT.md`. Read Part 14.0 (digest), 14.2 (look bible: palette, lighting rig, fog, post, materials), your
  beat's section of 14.5, the relevant rows of 14.10 (tests, thresholds) and 14.12 (constants). Part 10 is the defect list.
- Engineering notes: `DEVELOPMENT.md` (architecture, global uniform contract, colour rule: palette values are linear,
  plain Color properties are gamma-encoded by `MaterialFactory`, ADRs). Constants: `Assets/_Game/Scripts/Runtime/Core/NocturneConstants.cs`, palette `NocturnePalette.cs`.
- Shared shader includes (read-only for you): `Assets/_Game/Shaders/Include/` (`NocLitPass.hlsl` = the standard lit pass
  structure with hooks, `NocLighting.hlsl`, `NocAtmosphere.hlsl` (`NocSkyColor`, `NocFog`, `NocFloor`), `NocCommon.hlsl`,
  generated `Shaders/Generated/NocPalette.hlsl` (`PAL_<Token>`, linear) and `NocConstantsGen.hlsl` (`NC_<Name>`)).
- The latest capture run and critique are named in your prompt (`QA/captures/<run>/`, `QA/critique-<run>.md`).

## Ownership and concurrency
- Edit ONLY the files your prompt lists. Read anything. Never edit `Shaders/Include/**`, `Scripts/Runtime/Core/**`,
  `Scripts/Runtime/Systems/**`, `Scripts/Runtime/Player/**`, `Scripts/Editor/**`, scenes, `ProjectSettings/**`,
  `NocturneConstants`, `NocturnePalette`, or another builder's files. If you need a shared change, put the exact code in your final report.
- Other builders work on other beats and the lead may change the shared fog, sky, lights, LightCal and exposure at the same time. Light levels may shift between your builds: tune your
  materials for the spec's albedos and recipes and for relative contrast inside your subject; report the numbers you measure.
- Never commit to git. Never run Unity or the player except through the two scripts below (they share one machine-wide
  mutex, so builds and player runs of all agents are serialised; a call may wait for others).

## Loop
1. Edit. Run `python -I Tools/fastcheck.py all` (C# compile check, about 6 s) until it prints OK for runtime and editor.
2. Only if you changed a material spec in `MaterialFactory` (new material, new property, new default): rebuild assets
   `powershell -NoProfile -File Tools/unity-queue.ps1 -Name <you>-rebuildNN -Method Megastructure.EditorTools.BatchTools.RebuildAll`
3. Build the player: `powershell -NoProfile -File Tools/unity-queue.ps1 -Name <you>-buildNN -Method Megastructure.EditorTools.BatchTools.BuildWindows`
   (2-3 min plus queue time). Read the printed digest. Shader problems: `grep -n "Shader error\|error:" Logs/<you>-buildNN.log`.
   If the build fails in a file you do not own, another agent is mid-edit: wait about 3 minutes and retry; never touch it.
4. Capture with your own build slot: `powershell -NoProfile -File Tools/lookdev.ps1 -Slot <you> -Run NN -CaptureSet keyshots`
   (or `-CaptureSet beats`, one `-CaptureOnly <id>` per call; see `Assets/_Game/Scripts/Runtime/QA/ShotPlan.cs`). The player
   runs through `Tools/run-player.ps1` (one player at a time, gpu-guard for long jobs, Part 2.5).
   Look at the PNGs with the Read tool (that is the point). Metrics: `python -I Tools/qa/image_metrics.py QA/captures/<you>_NN`.
   Frame sequences of 14.10 / 14.12: `-sequence boundaries|statics|signs|rain|koi|traffic|encode` (statics and pans with
   `-freeze-clock -rain 0`), analysed with `python -I Tools/qa/sequences.py QA/captures/seq_<name>`; run-player routes every
   sequence, `keyshots`, `stills`, `parity`, `all`, `everything` and every 4K render through gpu-guard (6500 MiB for 4K).
5. Batch several fixes per build. Budget: about 6 to 8 build-and-capture cycles. Stop earlier when your acceptance list is met.

## Beat content and activation (14.6, binding; audit S2 item 1)
- Put all beat content under `ctx.BeatRoot(b)` (or `ctx.CityRoot` / `ctx.BackdropRoot`). **Beat roots are always active**: the city
  is one persistent set, nothing activates inside the frustum, and content seen when looking back never disappears (Part 10
  item 36). Never call `SetActive` on a beat root or on visible content to save cost; use culling, LOD fade by footprint, fog.
- The 8 m lead / 12 m hysteresis window (`BeatDirector.InEventWindow(b, s)`) is for EVENTS only: starting a pass, a sound, a
  launch keyed to s, or flipping flags and uniforms. A visible swap still needs fog >= 98 %, darkness or a sealed mesh (14.6).
- Events are keyed to s (never wall-clock time); they fire once, in order, and behave identically at 1.5 and 2.5 m/s.
- Fallback tiers: `ctx.Tier` (`-tier N`: 0 full, 1 simplified, 2 graybox) is available to builders that have tiers.

## Quality rules (Part 8, 10, 14.7)
- Everything procedural: C# meshes (`MeshKit`), handwritten HLSL, procedural textures. No imported assets.
- Shaders: follow the pattern of the existing `Megastructure/Noc*` shaders (SRP Batcher: one `UnityPerMaterial` CBUFFER;
  shared lighting, then `NocFog` and `NocFloor` last in every forward pass (14.2.2); no shadow maps (14.2.3); ShadowCaster/DepthOnly/DepthNormals passes kept). Guard every division,
  `pow`, `sqrt`, `normalize`. Filter procedural lines and patterns with `fwidth` or the pixel footprint and fade them out
  before they alias (no shimmer, no moire). No co-planar faces (offset or recess). Bevels on near geometry.
- New shaders: one file per shader in `Assets/_Game/Shaders/Beats/` (yours alone), named `Megastructure/Noc<Name>`. Every
  shader whose name starts with `Megastructure/` anywhere under `Assets/_Game/Shaders` is discovered from the files
  (`ShaderCatalog`, `Scripts/Editor/EditorCommon.cs`): `RebuildAll` and `BuildWindows` put it into Always Included Shaders, and
  `Validate` compiles every pass of it and checks the inclusion. Never edit ProjectSettings (or MaterialFactory, MaterialLibrary,
  `Shaders/Include`, `Shaders/Generated`) for a shader: derive material variants at runtime (`new Material(lib.X)`) or create the
  material from your own shader at build time. A shader file with errors fails `BuildWindows` for everyone: fix it before your
  next Unity call. (In place since the line "SHADERS-BEATS READY" in `docs/logs/I0.md`.)
- Keep triangle budgets of 14.5. No per-frame allocations, no mesh generation after load. The autotest report checks it
  (`gcAllocPerFrame`: steady bytes per frame above the engine's own 17 B / 32 B input box <= 3 B). To find an allocation:
  a development build (`NOC_BUILD_DIR=Builds\<you>dev`, `-ExtraArgs "-dev-build"`), run it with `-alloc-profile QA\reports\<you>_gc`
  (300 profiled frames into `<exe folder>\QA\reports\<you>_gc.raw`), then `unity-queue.ps1 -Method
  Megastructure.EditorTools.BatchTools.AnalyzeGcProfile -ExtraArgs "-profile-in,<raw>,-profile-out,<md>"`: every GC.Alloc by
  marker stack (your OnFrame shows under ProgressDirector). `python -I Tools/fastcheck.py all --warnings` lists compiler warnings.
- Match the surrounding code: naming, comment density, units in comments, references to the spec section.

## Final report (your last message, plain text)
Changes per file; the final capture folder and the shots in it; measured numbers against your acceptance list; what is
still open and why; exact shared-file changes you need from the lead (if any).
