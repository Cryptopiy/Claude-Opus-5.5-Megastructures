# Phase 2 import audit (read-only auditor, 2026-11-13)

**Nocturne Phase 2 import audit (read-only)**

**Verdict: conditional go.** Phase 3 can start once findings 1–4 below are fixed, plus 5 if possible. All of them are small. Part 13 says no new phase starts while S1 or S2 defects of the current one are open. Finding 1 also changes what every graybox builder does, so it has to come first. Findings 6 and 7 are needed before the Phase 3 baseline captures and metrics, not before graybox work begins. The S3 items are housekeeping.

**What checks out**
- **Completeness:** the plan's file table and the minimal closed set are present. No `.meta` file was copied: none of the 631 GUIDs from the three old games appear here. The namespace is flat `Megastructure` (+ `.EditorTools`, `.Tests`).
- **Not imported:** DetailTier (optional in the plan), FootstepBank and Thread's audio layer table (the integrator logged both as deviations).
- **Inherited defects fixed:**
  - The audio generator sits on a clip-less AudioSource, not the listener (SceneBuilder.cs:124-127, NocSynth.cs).
  - WarmupLoader draws 3 frames behind black instead of `Shader.WarmupAllShaders`.
  - The dither is static by default and Thread's darkness gain is gone.
  - Shadows are off, depth is `Depth_32_Stencil_8`, and the API is D3D11 only (ProjectSettings `m_APIs: 02000000`, `m_Automatic: 0`).
  - F11 is handled only by QualityBootstrap; Alt+Enter is native with a fallback that does not double-toggle.
  - `-progress` is a fraction, and `NOC_BUILD_DIR` gives per-owner build folders.
  - Autotest, soak and wall-bot runs have the 60 fps cap.
- **Reflective strings (plan 4.9):** all updated, and tested in ProjectTests.cs:84-86.
- **Constants and cinematic edits (a)–(f):** all match 14.4, 14.5, 14.10 and 14.12. That covers waypoints, sharp cumulative distances, radii, lane half-widths, all gaze keys of 14.5 (one shared seam key kept once), the key shots, the vFOV/EV/fog schedules, cinematic ramp 0.5 s, QA ramp 0.2 s, omega 2.5, rate limits 22/18 deg/s, stop ramp T = 6/v0, path state in double, and lateral offset 0.
- **Grade (14.7 H1, D-007):** amplitude is 2.0 × lerp(1, 0.6, saturate((Y' − 0.35)/0.4)) on encoded Rec.709 luma, with chroma ±1 code per 2×2 block as a zero-luma BT.709 offset. Bloom clamp is 30. Picture fade goes through `GradeState.FadeBlack`.
- **Evidence matches the report:** smoke run 160.10 s and 48,204 rays with 0 misses; cinematic trigger 599.05 s, beats at 48.25…504.25 s; walk run 998.1 / 1002.1 s; 126/126 tests; Validate 66 checks; build D3D11 with 0 errors and 0 warnings.

**S2 findings**

1. **Beat content pops in and out inside the view.**
   - Where: `Assets/_Game/Scripts/Runtime/Systems/BeatDirector.cs:36-52`, `Scripts/Runtime/Core/WorldContext.cs:48-64`, `Beats/GrayboxKit.cs:24`.
   - Problem: each beat's whole root, its floor included, is switched on only 8 m before the beat and off 12 m after it. In the graybox the next beat's floor appears 8 m ahead of the walker, inside the view (fog at 112 m in the alley still shows 73 % of the surface). Looking back makes the previous beat vanish. Key shots only show their own beat (KS04 cannot show the Gulf face). 14.6 says "the city is one persistent set" and nothing switches on inside the view. Plan 8.3 misread the BeatDirector hysteresis as geometry activation.
   - Fix: keep beat roots always active. Use the 8 m / 12 m window only for beat events. Any future culling goes in an explicit opt-in sub-root, justified by fog of 98 % or more or by an occluder. Write this rule into `docs/BUILDER_BRIEF.md`.

2. **Captures run uncapped.**
   - Where: `Scripts/Runtime/QA/CaptureHarness.cs:302-304`.
   - Problem: it sets `vSyncCount = 0` and `targetFrameRate = -1` for every capture run, overriding QualityBootstrap's QA cap. Long sequences and encode captures would run at 100 % GPU, which breaks 2.5.3 and plan §6 item 3.
   - Fix: `targetFrameRate = QaMode.PerfRequested ? -1 : QualityBootstrap.QaFrameCap`.

3. **4K captures bypass gpu-guard.**
   - Where: `Tools/run-player.ps1:53-57`, with `ShotPlan.cs:6-7,163,173-175`.
   - Problem: the capture sets `keyshots`, `stills` and `all` render 3840×2160, but only `-capture-4k` or `-Width ≥ 3840` sends a run through gpu-guard (and its 6500 MiB check). 2.5.1 requires 4K captures to go through gpu-guard. Also, `boundaries`, `signs`, `rain`, `koi` and `traffic` are listed as heavy sets but ShotPlan does not know them, so they fail as unknown sets.
   - Fix: add `keyshots`, `stills`, `all`, `everything` and `parity` to the heavy list (with 6500 MiB for the 4K ones). Keep the list in sync with ShotPlan.

4. **Soak loop end is Thread's number.**
   - Where: `Scripts/Runtime/QA/SoakRunner.cs:23`.
   - Problem: `LoopEndS = 857.0  // before the beacon trigger at 858` is Thread's. The Nocturne soak would loop over s 0–857 and never run beats 5–7.
   - Fix: `NocturneConstants.GoalTriggerS - 2.0`.

5. **Walls can sit up to 35 cm outside the lane edge, and the coverage test was loosened to hide it.**
   - Where: `Scripts/Runtime/Player/PathGuide.cs:156-176,292`.
   - Problem: the player log shows "max wall shift 35,1 cm" at the alley-mouth funnel. There the player's centre can reach the 14.4 barrier line (lane + 0.35), so the camera can touch the facade face. The coverage ray now allows `WallOffset + m_maxShift + 0.5` everywhere along the path, so a wall misplaced by up to 0.85 m anywhere still passes.
   - Fix: compute the shift only over each segment's own range (no overlap on steep funnels) and close the joints with small corner boxes. Restore a per-s tolerance.

6. **The 14.12 flag `-sequence NAME` does nothing.**
   - Where: `Scripts/Runtime/Core/CliArgs.cs:101` (only QualityBootstrap reads it); `ShotPlan.cs:160-176`.
   - Problem: ShotPlan has no `boundaries`, `signs`, `rain`, `koi` or `traffic` sets, so a run with that flag is just a capped normal game.
   - Fix: map `-sequence` onto ShotPlan sets per 14.10. At least `boundaries` and `statics` are needed before the Phase 3 sequence check.

7. **The Python QA tools still carry Thread's key-shot data.**
   - Where: `Tools/qa/regions_default.json` (whole file: `beat01_salt_KF1`, `beat06_clouds_KF6a`…) and `Tools/qa/qa_common.py:126-133,186-190` (sun, planet, beacon, inhale/swell fields).
   - Problem: Part 3.1 rule 1 forbids copying key-shot tables. No region matches a Nocturne `KSnn` tag, so the banding and aliasing region checks are skipped silently. The QA self-test passes 79 of 85.
   - Fix: rewrite the regions from 14.10 and remove the Thread fields before the Phase 3 baseline metrics.

**S3 findings**

8. **Autotest trigger and stop times come from the walker's own clock.**
   - Where: `WalkPlanner.cs:132,145,148,154`; `AutoTestRunner.cs` trigger/stop lines.
   - Problem: `_walkT` stops while the walker is blocked, so a stall would not show. The "exact" 599.050 / 998.100 match the formula by construction. The ending and beat-entry times use the runner's clock and do agree. Fix: also record the runner's own crossing times and check those.

9. **Cinematic evidence predates the stop-detection fix.** `autotest_cinematic.json` still says `stopSeconds 601.35` (build_3). Fix: rerun the cinematic test.

10. **The final build was not smoke-run.** build_final (22:41) differs from build_4 (22:21), which produced the walk and capture evidence. The changes are string renames only; the capture report still says `Megastructure.QA.CaptureHarness`. Fix: rerun the `-autotest-until 400` smoke on the final exe.

11. **Leftover Thread content in code and comments:**
   - `QaProjection.cs:3,322-331` (KF1 sun, KF7b planet, KF2b hood, Kf4a cable constants)
   - `CaptureHarness.cs:478,680,704` ("KF1 pose", "beacon trigger never fired", "14 s ending")
   - `NocDsp.cs:406` (`RingState.Gust`, grass sway)
   - `GradeTests.cs:4`, `NocGrade.shader:88` (sun disc / beacon / cable comments)
   - `WallBot.cs:8-9,24` (eye-on-grade checks the plan said to strip)

12. **Untriaged URP warnings.** Every player log has 16+ "Hidden/Universal Render Pipeline/... stripped" lines, which Part 11.10 says must be triaged. Cause: `Assets/Settings/PC_Renderer.asset:46` still assigns `postProcessData`. Fix: set it to null in PipelineConfigurator, or log the triage.

13. **Unity services setting flipped.** `ProjectSettings/UnityConnectSettings.asset:7` changed `m_Enabled` 0→1 in commit `ee52341`. Fix: force it to 0 in ProjectConfigurator.

14. **Logs use the machine's locale.** `[NOC]` lines print decimal commas ("2,00", "s=120,00", "13,9 s"), e.g. `GameBootstrap.cs:53`, `BeatDirector.cs:70`, `PathGuide.cs:131`. Fix: use `CultureInfo.InvariantCulture`.

15. **Wall gap is 6 cm wider than spec.** `PlayerMath.cs:32` (`WallGap` 0.03) and `PathGuide.cs:129-131` give 2(h + 0.38) against 14.4's "collider gap minus 0.70". On the bridge the wall is at 2.53 m against the rail at 2.50 m. Fix: log it as a decision, or set the gap to 0.

16. **WallBot speed.** `WallBot.cs:71` uses `QaMode.DriveSpeed` (1.5 m/s by default); plan 8.13 says 2.5 m/s.

17. **Spec deviations not logged as decisions** (NocturneConstants header rule, Part 1.6):
   - lane transition windows (NocLanes.cs)
   - follower peaks 18.05 / 16.86 deg/s against 19.3 / 17.9; the test only asserts more than 10 (`PathScheduleTests.cs:198`)
   - vFOV slope 0.3125 deg/s
   - `WallGap`, 0.25 m steep segments, wall shift
   - For the lead: `DEVELOPMENT.md` Architecture and Commands still say "(filled in Phase 2)", and the STATE.md command list has only gpu-guard (resume risk, lesson 18).

18. **Flags with no code behind them.** `-render-audio` and `-tier` (`CliArgs.cs:104,108`) do nothing. Fix: note them as deferred in STATE.md.

19. **No camera roll check.** The autotest does not check the 14.10 roll limits (≤ 0.01° manual, ≤ 0.3° cinematic).

20. **Second input reader.** `UiDirector.cs:646-676` reads Keyboard, Gamepad and Mouse directly for the menu, against plan §2 (NocInput is the single reader). Escape stays off there by default, so there is no double toggle.

21. **Lead file in an integrator commit.** Commit `d2e2f48` includes STATE.md, which the lead owns. The lead should review that diff.

22. **Verification systems not yet run.** Wall bot, soak, perf at 1080p and 4K, window-vs-RenderTexture parity, sequences, encode test, audio render and dump, and the PlayMode InputTests have not run. Part 11.11 requires them as soon as the graybox exists, so they are Phase 3 exit items. The Part 8.11 PBR material library and texture generator (Phase 2 scope in Part 13) has not been started.

Key files: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\docs\IMPORT_PLAN.md`, `...\docs\IMPORT_LOG.md`, `...\QA\reports\autotest_cinematic.json`, `...\QA\reports\autotest_walk.json`, `...\QA\reports\autotest_brisk_until400.json`, `...\Logs\player-smoke_until400.log`.
