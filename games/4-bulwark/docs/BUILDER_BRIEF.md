# Builder brief

Common rules for every beat builder of **Bulwark** (template from The Ring, adapted at import). Your own prompt names
your beat, your files, your build folder and your tasks.

## Project
- Unity 6000.6.0f1, URP 17.6 (Render Graph), Direct3D 11. Procedural first-person walk along the crest, through and
  down a 6 km sea dam. Root: `<WORKSPACE>\4 Megastructures\4 Megastructures` (the path has spaces:
  quote it everywhere).
- Spec: `MASTER_PROMPT.md`. Read Part 14.0 (digest), 14.2 (look bible: palette, lighting rig, fog, post, materials),
  your beat's section of 14.5, the rows of 14.10 for your beat (key shots, thresholds, tests) and 14.12 (constants).
  Part 10 is the defect catalogue, Part 8.11 the material quality bar.
- Engineering notes: `DEVELOPMENT.md` (ADRs D-01..D-10 and later decisions), `docs/FRAMEWORK.md` (what the framework
  provides, the execution order, **how to add a beat**, every command). Constants: `Assets/_Game/Scripts/Runtime/Core/
  BulwarkConst.cs` (14.12, single source; `FrameworkConst` only aliases it). Path: `BwPath` (Length, Point, HeadingDeg,
  Tangent, FloorY, EyeY, Lane, CinematicLateral, World, Project, BeatAt). Palette: `BulwarkPalette.cs` ->
  `Shaders/Include/BulwarkPalette.hlsl` (`PAL_<Token>`, linear).
- Shared shader includes: `Assets/_Game/Shaders/Include/` (`BwLitCore.hlsl` + `BwLitPass.hlsl` = the lit pass with
  hooks, `BwLighting.hlsl`, `BulwarkAtmosphere.hlsl` (`BwFogApply`, `BwSkyColor`, `BwHorizon`, `BwDrop`),
  `BwCommon.hlsl` (hashes, filtered lines and grids, `BwTierClip`, the blue-noise tile), generated
  `Shaders/Generated/BulwarkConstGen.hlsl`).

## Ownership and concurrency
- Edit ONLY the files your prompt lists (normally `Assets/_Game/Beats/BeatNN_<Name>/**` and your beat's material
  specs). Read anything.
- **Read-only for builders:** `Shaders/Include/**`, `Shaders/Generated/**`, `Scripts/Runtime/Core/**` (BulwarkConst,
  BulwarkPalette, BwPath, FilletPath included), `Scripts/Runtime/Systems/**`, `Scripts/Runtime/Player/**`,
  `Scripts/Runtime/Rendering/**`, `Scripts/Runtime/QA/**`, `Scripts/Runtime/UI/**`, `Scripts/Runtime/Audio/**`,
  `Scripts/Editor/**` (editor tools and the batch methods), scenes, `ProjectSettings/**`, `Tools/**`, and another
  builder's files. If you need a shared change, put the exact code in your final report.
- Other builders and the lead work at the same time; the lead may change the shared light, fog, sky, exposure and white
  balance (`BulwarkSchedule`, one writer: `AtmosphereDriver`). Tune your materials to the spec's recipes and to the
  relative contrast inside your subject; report the numbers you measure.
- Never commit to git. Never run Unity or the player except through the scripts below. `unity-queue.ps1` serialises
  Unity per project (mutex `Global\BulwarkUnityQueue_4Megastructures`); `run-player.ps1` waits for any other
  `Bulwark.exe` (never two renderers of ours at once, 2.5.2) and sends long or 4K jobs through `Tools/gpu-guard.ps1`
  (machine-wide turn, exit 75 = GPU busy: retry later, never force).
- **One build folder per owner** (lesson 3.2 #10): always pass `-BuildDir Builds\<you>` to both scripts (it sets
  `BW_BUILD_DIR` for `BatchTools.BuildWindows`).

## Loop
1. Edit. Run `python -I Tools/fastcompile.py all` (C# type check, about 10-30 s) until every project prints OK. It is
   blind to asmdef references and shaders, so the Unity build below is the real compile.
2. Only if you changed a material spec (new material, property or default): rebuild assets
   `powershell -NoProfile -File Tools\unity-queue.ps1 -Name <you>-rebuildNN -Method Megastructure.EditorTools.BatchTools.RebuildAll`
3. Build the player:
   `powershell -NoProfile -File Tools\unity-queue.ps1 -Name <you>-buildNN -Method Megastructure.EditorTools.BatchTools.BuildWindows -BuildDir Builds\<you>`
   (a few minutes plus queue time; stall watchdog 15 min). Read the printed digest; shader problems:
   `Select-String -Path Logs\<you>-buildNN.log -Pattern "Shader error|error:"`. If the build fails in a file you do not
   own, another agent is mid-edit: wait about 3 minutes and retry; never touch it.
4. Capture (beat stills, key shots, a custom pose):
   - `powershell -NoProfile -File Tools\run-player.ps1 -Name <you>-capNN -BuildDir Builds\<you> -PlayerArgs "-capture,QA\captures\<you>-NN,-capture-set,beats"`
   - comma lists go in ONE string: `-ArgString "-capture QA\captures\<you>-NN -capture-only KS04,KS05"` (never in
     `-PlayerArgs`, which is split on commas, lesson 3.2 #1);
   - a free pose: `-ArgString "-capture QA\captures\<you>-NN -capture-only pose:myshot:x:y:z:yaw:pitch:fov"`;
   - 4K (`-capture-4k`, `-res 3840x2160`), sequences and the encode set are long GPU jobs: `run-player.ps1` routes them
     through gpu-guard automatically (`-Long` forces it).
   Or all in one: `powershell -NoProfile -File Tools\lookdev.ps1 -Slot <you> -Run rNN -Shots KS04,KS05` (build +
   capture + metrics + contact sheet; `-NoBuild` to reuse the build).
5. **Look at your own captures** with the Read tool before anything else (Part 5.8 b): every key shot of your beat at
   1920x1080, the beat stills and the free-look stills (yaw +90, -90, 180; pitch +60 and -60). Judge them against your
   beat's acceptance list and Part 10. Then run the metrics:
   `python -I Tools/qa/image_metrics.py QA\captures\<you>-NN` and `python -I Tools/qa/contact_sheet.py QA\captures\<you>-NN`
   (14.10.3: mean luma per beat, clipped %, BandScore, fireflies, magenta, horizon pairs, key-shot rows).
6. **Sight lines (Part 11.13).** For every key shot and named subject of your beat report the evidence: the subject's
   projected box in pixels at 16:9, 16:10 and 21:9 (3 % margin), its size at 1080p and 4K, the unoccluded fraction
   (`CompositionLint` subjects in `report.json`, or the EditMode visibility test output `QA/reports/visibility.json`), and
   the AutoWalker follower pose at that s (2.5 m/s, rate-limited gaze) versus the target pose. A subject outside the
   frame, hidden or below its readability threshold is a finding to fix (geometry, pose or a logged threshold change),
   never silently.
7. Batch several fixes per build. Budget: about **6 to 8 build-and-capture cycles**. Stop earlier when your acceptance
   list is met. Delete intermediate capture folders you no longer need (disk budget, lesson 3.2 #15).

## Quality rules (Part 8, 10, 14.2.6)
- Everything procedural: C# meshes (`MeshKit`, `MeshBuilder`, `MeshKit.Loft`/`Revolve`/`Structures`, `CorridorKit`),
  handwritten HLSL (`Megastructure/Bw*`), procedural textures. No imported assets.
- Shaders: follow the existing `Megastructure/Bw*` shaders (SRP Batcher: one `UnityPerMaterial` CBUFFER; `BwLitPass`
  hooks, shared lighting and `BwFogApply` in every colour pass; ShadowCaster, DepthOnly and DepthNormals passes kept;
  instancing where the shader is used by `InstancedBatch`). Palette colours only through `PAL_<Token>` or
  `MaterialFactory` (no literal colours: the palette test scans for them). Guard every division, `pow`, `sqrt`,
  `normalize`. Filter procedural lines and patterns by the pixel footprint and fade them out before they alias (no
  shimmer, no moire; thin lines through `BwLine`, minimum width max(1.5, 1.75 H/1080) px). No co-planar faces. Bevels on
  near geometry. Everything keyed to s (never to time) unless the spec says it moves.
- Keep the triangle budgets of 14.5. No per-frame allocations, no mesh generation after load (`IBeatBuilder.Build` runs
  once behind the black fade). Code-drawn instanced fields register with `InstancedRegistry` / `InstancedField` so the
  capture cameras re-submit them (capture rule C-12).
- Match the surrounding code: naming (`Bw*`), comment density, units in comments, references to the spec section.

## Final report (your last message, plain text)
Changes per file; the final build folder and capture folder and the shots in it; what you saw in your captures;
measured numbers against your acceptance list (metrics and sight-line evidence); what is still open and why; exact
shared-file changes you need from the lead (if any).
