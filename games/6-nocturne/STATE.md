Run started at 2026-11-13T19:22:09.6952535+01:00

# STATE — Nocturne (game 6)

## DONE 2026-11-21 23:35: THE ARK WAVE (gate: EditMode 527/528, 0 failed; autotests exact 599.050 / 601.450 and 998.100 / 1002.100; perf alone 1080p 288.3 fps, GPU 3.45 / p99 3.64 ms, 0 frames > 33 ms; 4K 83.8 fps, GPU 11.9 / p99 20.43 ms, s 580-600 over 20 ms at 4K (DR-09, known); Ark stills in QA/final/ark; build copied to Builds/Nocturne; metrics at Fable 5.2 in QA/reports/metrics_fable52.md: 45:09:37 active, 6,396,532,634 tokens, $4,270.64)
## (history) NOW: THE ARK WAVE (user's order 2026-11-21 ~20:10, lead session <session>; D-043)
- The user: the flying ship is great; make it colossal and let it come smoothly out of the haze, "the most mesmerizing scene in the game"; remove the traffic "lasers" that pass too close to the walker on the skybridge; at the end report the project's totals (active time, tokens, API-equivalent cost) at **Claude Fable 5.2** prices in a table (prices re-fetched: Tools/pricing_fable52.json; run `python -I Tools/metrics.py --pricing Tools/pricing_fable52.json --md QA/reports/metrics_fable52.md --json QA/reports/metrics_fable52.json --only-sessions <session>,<session>`).
- Design and ownership: Logs/ark/ARK.md (the Ark: 5 km x 440 m x 260 m, belly y 330, along the rift south -> north, bow emerging from the southern haze in beat 3, the bridge crossed under its belly, stern lost north in beat 5; traffic at least 200 m from the walk in beats 3-4).
- Copies: .worktrees/abyss (DR, AB, AT; Library re-copied from integ) and .worktrees/integ (FX, AU), both at master f808f1b. Script Logs/workflows/nocturne-ark.js, args {role, deadline 23:00, stop 22:30}. Runs / tasks: DR wf_8af5fa23-f46 / wrlhbxhvq; AB wf_d7d2ee35-fa3 / wcxj9p3nk; AT wf_dab99683-e2e / wg8me86av; FX wf_a3673a7c-521 / wzgzi3du0; AU wf_e3edf600-fb2 / wv4klcit7.
- Then: commit both copies, merge abyss into integ, lead check, Ark captures (beats 3-5 stills, the ark sequence), look at them, a focused review if needed, final gate (autotests, perf alone, QA/final refresh), copy the build to Builds/Nocturne, docs update, metrics at Fable 5.2, final Russian message with the table.
- If paused: TaskStop the five tasks, commit each copy on its branch, PAUSED note here.
- 22:15 ALL FIVE DONE; FX (gaze: coming bow s 492-500, bow overhead s 576-586, long look-up under the belly s 748-822; "ark" capture set) and AU committed in integ; n6-abyss merged into n6-integ (948c767, master merged too). FINAL GATE running in integ (Logs/workflows/final_gate.sh: lead check + build into Builds/Nocturne, autotests both speeds, perf alone 1080p / 4K, key shots -> master QA/final, the ark set -> QA/final/ark). Next: look at QA/final/ark and the key shots; then fast-forward master to n6-integ, robocopy the build to master Builds/Nocturne, D-043 entry in DEVELOPMENT.md, update README / FEATURE_MATRIX / FINAL_REPORT (the Ark, traffic, new gate numbers), metrics at Fable 5.2 (command above), final Russian message with the cost table. DR DONE 21:58 (the Ark comes DOWN the rift from the north, because every beat-3 gaze faces north: accepted, D-043 amended; bow over the walk at s 596, stern s 1055.7; 39 k triangles; 1080p GPU p99 6.95 ms; open DR-09: +0.82 ms at 4K s 580-600); abyss committed f6f5199 on n6-abyss. Waiting for FX (integ). AT DONE 21:44, AU DONE 21:58 (reports in Logs/ark/reports; AU uses an ArkCourseMirror of DR's course until the merge: replace it by DreadCourse after merging abyss). AB DONE 21:33 (report Logs/ark/reports/AB.json; traffic >= 218 m from the walk, boulevard vehicles hidden within 300 m over the rift, longest streak 64 px at 60 fps; it also re-mirrored the abyss Library). Weekly usage 86 % at 21:35 (other projects use the account too): check get_usage every 10-20 min; at 99 % stop everything, commit both copies, PAUSED note. Save each report as Logs/ark/reports/<ROLE>.json.

## (history) FINISHED 2026-11-21 07:30 (lead session <session>)
- The game is built and released as of master (this commit): player `Builds/Nocturne/Nocturne.exe` (from b14abae, 0 errors). Final gate (D-042): Validate 127/127; EditMode 510/511 (0 failed, 1 explicit skip); smoke PASS; autotests exact (599.050 / 601.450 s, 998.100 / 1 002.100 s); wall bots PASS at both speeds; perf ALONE 1080p 283.9 fps, GPU 3.51 / p99 6.02 ms, 0 frames over 33 ms; 4K 88.4 fps, GPU 11.29 / p99 18.55 ms. Final 4K key shots in QA/final (git-ignored, 80 images + contact sheets).
- Run shape completed (the user's orders): one wave of eleven roles -> merge -> full gate -> ONE six-lens panel + chief -> ONE fix wave (twelve roles) -> ONE three-agent integration round -> final gate. Decisions D-035..D-042. Known issues accepted for this release are listed in FEATURE_MATRIX.md and FINAL_REPORT.md (no second panel was run, so MASTER_PROMPT 15.1 item 2 is not met).
- Docs: README.md, FEATURE_MATRIX.md, FINAL_REPORT.md written by three writer agents from Logs/final_facts.md (git-ignored, verified facts). Metrics (Part 15.3, QA/reports/metrics.md): 179:35:54 wall, 42:39:46 active, 5,955,782,440 tokens, $2,585.44 API-equivalent at the API prices re-fetched 2026-11-21 (superseded by the Fable 5.2 table in FINAL_REPORT.md) (`python -I Tools/metrics.py --md QA/reports/metrics.md --json QA/reports/metrics.json --only-sessions <session>,<session>`).
- Copies left on disk: `.worktrees/integ` (with its Library, the lead's integration copy at b14abae), `.worktrees/abyss, blvd, heart, waveb` (checkouts only; Libraries deleted). Remove them with `git worktree remove` when no further work is planned; the branches n6-* keep their history.
- If work resumes: start from master, fast-forward `.worktrees/integ`, and take the open items from FEATURE_MATRIX.md "Known issues".

## (history) NOW: PHASE 4 WAVE A, FIFTH START (2026-11-20 20:12, lead session <session>, user's resume order with "ultracode")
- Machine OK again: `powershell -NoProfile -Command 1` 0.6 s; MBAMService idle. Patch 20d re-applied and committed as WIP **2fe0da9**; master CompileCheck 0 errors (ScriptAssemblies restored, Logs/lead-resume5-compile.log).
- New run shape (user's orders 2026-11-20): parallel git worktrees, one Unity queue per folder, no gpu-guard turn, players capped at 3 of ours (2 with 4K); builders never commit (the lead commits each copy for the merge); deadline 23:30, stop features 23:00.
- Tools (6ae7aa1, 5be5277): unity-queue.ps1 mutex derived from the folder path, exact `-projectPath "<root>"` match (a substring test would kill the worktrees' Unity), -BuildDir; run-player.ps1 without gpu-guard, cap 3 / 2 under a short start mutex, -BuildDir / -ArgString (QaTests' parsed lists kept); lookdev.ps1 -BuildDir. `/.worktrees/` ignored.
- Copies (git worktrees, Library robocopied without Bee / BuildHistory, CompileCheck pre-warm 0 errors each): `.worktrees\abyss` (branch n6-abyss: AB + AT), `.worktrees\blvd` (n6-blvd: BV + FX), `.worktrees\heart` (n6-heart: HT + SG), `.worktrees/integ` (n6-integ: the lead's pre-merge integration copy). Only three builder copies because W: had 28 GB free (19.3 GB after the copies).
- Runs (one Workflow run per role, script `C:\Users\<user>\.claude\projects\W--YouTube-Megastructures-6-Megastructures-6-Megastructures\<session>\workflows\scripts\nocturne-p4a-builder-wf_cdcd9699-caf.js`, args {role, deadline, stop}):
  | Role | Copy | Run | Task |
  |---|---|---|---|
  | AB | abyss | wf_cdcd9699-caf | wc97k15fm |
  | AT | abyss | wf_ad5036ea-e79 | wf46bm41u |
  | BV | blvd | wf_eda0a337-8ed | wj9ab1msb |
  | FX | blvd | wf_2d30e1dc-532 | wmf51wcwx |
  | HT | heart | wf_ee45c9cc-290 | w156z6i01 |
  | SG | heart | wf_0128ef8d-7cc | wgkenlxdv |
  | AL (beat 1, new) | waveb | wf_609bab4b-524 | w8v3b3y4j |
  | TR (beat 4 trains, new) | waveb | wf_1342920c-bbd | we07tmlo9 |
  | GD (beat 5 garden, new) | waveb | wf_622d442a-b06 | wer4fbf02 |
  | AU (D-034 sound) | waveb | wf_aeb5834a-bad | wmdepsisr |
  | DR (D-034 hull, shadow, elevators, panel; new) | abyss | wf_2ba71e17-3ec | wb0al767e |
- 20:45: the wave B content and the D-034 layer launched at once with wave A (the user's run shape: every open role in one wave), deadline 23:45 / stop 23:15, in a fifth copy `.worktrees\waveb` (n6-waveb: AL, TR, GD, AU) and DR in abyss. Their FX requests go into shared_change_requests (FX's brief predates them). Disk: master's Library and Builds/Nocturne removed (re-copy from integ when master needs Unity); W: 19.1 GB free after the five copies.
- Builders keep a live progress note `Logs/<role>_progress.md` in their copy (git-ignored, survives a pause in place) and their decision log docs/logs/<role>.md; cross-copy requests are "REQUEST <ID>:" lines read read-only across copies, plus shared_change_requests in the structured report.
- Lead background: GPU watchdog `Logs/gpu_watch.ps1` (monitoring only, Logs/gpu_watch.csv); baseline lead check in integ (RebuildAll, Validate, EditMode, build Builds\integ, smoke; Logs/lead_check_base.txt in the copy).
- If paused: TaskStop the six tasks above and the watchdog; in each copy `git add -A` + commit on its branch (WIP); commit master; write a PAUSED note here. To resume: relaunch each role with the same script and args (the builders continue from their progress notes).
- 23:26 status: reported HT (done), SG (partial), AB (done), BV (partial), FX (partial), AT (partial), AU (partial), DR (done); reports in Logs/p4a5_reports/<ROLE>.json (git-ignored); rulings D-036, D-037, D-038. Branches n6-heart (4d35d89), n6-blvd (5b481ac), n6-abyss (292c333) merged into n6-integ (784521a; report conflicts kept ours); RebuildAll + build OK, keyshots1080 in .worktrees/integ/QA/captures/m2_ks looked at. Waiting for AL, TR, GD (waveb, 23:45). Watch: transient native crash in Mesh.SetColors at load (OldQuarterBuilder), waveb's TR-final shows Q1IntegrationTests A/B and KS10 drop failures on that copy.
- 01:40 GATE p4_01 on the merged game (master caddb90, all eleven roles; integ copy): EditMode 464/465 (1 explicit skip), Validate 127/127, build 0 errors, smoke PASS; autotests exact (cinematic 599.050 / 601.450 s, walk 998.100 / 1002.100 s); soak 1 200 s PASS; wall bots PASS at 2.5 and 1.5 m/s (the 1.5 m/s run needs about 35 min); sequences PASS: signs (photosensitivity), rain, koi, traffic, texpans, dolly, boundaries (all nine), statics with -freeze-clock -rain 0 (7/7), pans (7/7); encode x264 CRF 23 at 1080p 10/11 PASS (enc_beat01_wall: BandScore only, 0 visible steps). Captures in .worktrees/integ/QA/captures/p4_01* (stills, beats, 4K, 16:10, 21:9, texclose, facade sheet). Lesson: the whole "encode" and "boundaries" sets write 13-17 GB of frames and filled W: twice; run them clip by clip and delete frames (Logs/workflows/full_gate2.sh, gate2_laneE.sh). Critic panel running: run wf_86ee9eb5-945 (task w7cu3ly2h), script Logs/workflows/nocturne-panel.js; the chief writes Logs/fix/<ROLE>.md; lead rulings ready in Logs/fix/_lead_rulings.md and the grouped requests in Logs/fix/_requests_<OWNER>.md.
- 02:00 FIX WAVE launched (D-040): twelve runs of the same builder script with args {role, deadline 05:00, stop 04:30, fix: Logsix\<ROLE>.md, merged caddb90}; copies fast-forwarded to caddb90 and pre-warmed. Runs / tasks: FX wf_d8de44e2-be6 / woxps9xvk; QA (in integ) wf_08997966-fa7 / wj0p22ba8; AB wf_0e02c498-06c / wk83um00m; AT wf_4d29bd6f-5b2 / wpvt3ufqj; DR wf_d5eb7b5b-b4a / wnp5apnid; HT wf_8abd812b-02a / wzdzj3kv9; SG wf_98385208-6e7 / wa4tdr73c; BV wf_6976e3df-4ed / wun8m117o; AL wf_7313dc3f-6b1 / we2vbkv19; TR wf_e080af81-90a / wc4g2s4jf; GD wf_c9e667da-ac6 / w1qxrhl0w; AU wf_a212f4e8-c7a / wu88pekj7. Panel result: Logs/p4_01_panel.json, Logs/fix/_summary.md. If paused: TaskStop these twelve, commit each copy on its branch (git add -A on n6-<team>), commit master, PAUSED note; to resume, relaunch each role with the same args (the builders continue from Logs/<ROLE>_progress.md "FIX WAVE" sections).
- 04:30 fix wave DONE and merged into n6-integ (a9d4f5a); lead check of the merge running in integ (Logs/lead_check_fix.txt); INTEGRATION ROUND running: run wf_f8bdf4c1-7ec (task wjbcf2olb), script Logs/workflows/nocturne-integration.js, three agents INT-A (rendering / shared), INT-B (QA / visibility), INT-C (beat content / dread / audio) in .worktrees/integ, task files Logs/integ/INT-*.md, deadline 06:30. Then: commit integ, merge into master, final gate (lead check, autotests both speeds, perf ALONE at 1080p and 4K, final 4K key shots in QA/final), docs (Logs/workflows/nocturne-docs.js), metrics, FINISHED note.
- 05:35 integration round DONE (INT-A partial, INT-B done, INT-C partial; Logs/integ/INT_reports.json); Validate 127/127 after the NocKoi Ghost pass joined the variant baseline; master fast-forwarded to b14abae. FINAL GATE running in .worktrees/integ (Logs/workflows/final_gate.sh, log .worktrees/integ/Logs/final_gate.txt): lead check with the build into Builds/Nocturne, autotests and wall bots at both speeds, perf ALONE at 1080p and 4K (perf_windows), final 4K key shots copied to QA/final (git-ignored). Builder copies' Libraries deleted (no more builder waves). Then: docs by three writers (Logs/workflows/nocturne-docs.js) from Logs/final_facts.md, metrics (prices re-fetched 2026-11-21, unchanged), FINISHED note, final Russian message.
- Then (user's run shape): merge each copy into integ as its roles report (lead check there), then master; full gate (autotests 2.5 / 1.5, soak, captures incl. 4K / 16:10 / 21:9 and every special set, contact sheets); ONE six-lens critic panel + chief; ONE fix wave (Logs\fix\<role>.md); ONE integration round; final (perf alone, QA/final 4K, docs by three writers, metrics, FINISHED note). Wave B content (AL beat 1, TR trains, GD garden) and D-034 go into the fix wave's role list.

## (history) PAUSED 2026-11-20 ~11:12 (user: "всю работу останови"; lead session <session>)
- The fourth start (run wf_ab1bc94d-acd, task w3p074iub) is stopped. It ended with the previous Claude Code process before the user's message. Its background tasks are gone too: the GPU watcher b1dv57ess and the waiter b009ah3f1. No Unity editor and no Nocturne player is running on this machine; our Temp/UnityLockfile is free.
- Committed in the fourth start: only FX's d394e2e (the INT-25 facade-sheet floor gate and BV's Tram material row).
- Everything uncommitted is saved as `docs/wip/p4a-wip-2026-11-20d.patch`: 38 files, a superset of patch 19c. It holds AB's edits (AbyssTraffic, Beat03_Edge, TonguePier, the Relief builders, NocLine, critique-AB), HT's Heart, Spire and koi work (SpireDriver.cs, NocInlay.shader, critique-HT), AT's AtmosphereDriver, the shared NocSurfaceConstants and NocMirror, NocWet with WetFloorTests, BoulevardStreetTests, the SG log and critique-SG, and refreshed QA reports. `git apply --check` against HEAD is OK, and those files are reset to HEAD.
- **Known environment problem:** `Library/ScriptAssemblies` is EMPTY, because the three hung Unity runs of this morning wiped it before they stalled. So fastcheck now fails at HEAD with CS0234 "UnityEngine.InputSystem" in Tests/PlayMode/InputTests.cs. That is not a code defect: fastcheck was green on this code at 08:03. One successful Unity compile restores the DLLs.
- **The machine:** all morning .NET processes started very slowly (PowerShell 10-65 s), and MBAMService (Malwarebytes) sat at about 1-2 cores. Every Unity run hung at "IL Post Processor runner cannot be established". Tools/unity-queue.ps1 now fails fast on that (exit 96).
- **To resume:**
  1. Check that `powershell -NoProfile -Command 1` starts in a few seconds.
  2. `git apply docs/wip/p4a-wip-2026-11-20d.patch`.
  3. Run a Unity CompileCheck through the queue (it also restores ScriptAssemblies), then fastcheck.
  4. Re-arm the GPU watcher.
  5. Relaunch the same script by scriptPath, with a FIFTH-start note (patch 20d, d394e2e, and the UNITY AVAILABILITY note only if the machine is still slow).

## (history) NOW: PHASE 4 WAVE A, FOURTH START RUNNING (2026-11-20 ~10:10, lead session <session>)
- Running: run **wf_ab1bc94d-acd**, task **w3p074iub**. Script: `C:\Users\<user>\.claude\projects\W--YouTube-Megastructures-6-Megastructures-6-Megastructures\<session>\workflows\scripts\nocturne-phase4-wave-a-wf_2b101732-ff0.js`.
- Owners: BV, AB, AT, HT, SG and FX. AU is done.
- Launched without a green Unity CompileCheck, because Unity cannot start on the slow machine (see below). fastcheck is green.
- The COMMON note "UNITY AVAILABILITY" tells the owners:
  - to work Unity-free first: fastcheck, fxc, analysis;
  - to batch their Unity runs;
  - that the unity-queue ILPP stall guard (12 retries, then exit 96) gives a fast failure;
  - to retry Unity at most every 20 min;
  - to report pending Unity steps instead of idling.
- The GPU watcher is b1dv57ess; the system-calm waiter is b009ah3f1.
- If paused: TaskStop w3p074iub, then save `docs/wip/p4a-wip-<date>d.patch`.

## (history) RESUMING PHASE 4 WAVE A, FOURTH START (2026-11-20 ~08:00, lead session <session>, after the user's "продолжай")
- `docs/wip/p4a-wip-2026-11-19c.patch` is re-applied (23 files, uncommitted, owned by the wave A owners). fastcheck runtime 199 / editor 265 OK. The GPU watcher is re-armed (background task b1dv57ess, Logs/gpu_watch.csv).
- The workflow script (same path as below) now has a FOURTH-start restart note. AU is skipped because it is done; IA gets AU's summary from its log.
- **BLOCKED on the machine, not on the project.**
  - Two Unity CompileCheck runs (Logs/lead-resume4-compile.log at 08:04 and lead-resume4b-compile.log at 08:32) hung for 20 min. They kept printing "Connectivity with IL Post Processor runner cannot be established yet". Licensing also failed: the IPC channel timed out after 60 s, and "Code 10 while verifying Licensing Client signature". The lead killed both of its own Unity runs. No Unity has this project open.
  - Cause: every .NET process starts very slowly (powershell -Command 1 takes 65 s, dotnet --version 157 s), while native tools are fast. CPU is at 24 % and 42 GB of RAM are free. MBAMService.exe (Malwarebytes) uses about 2 cores all the time (104 CPU-s in 52 s), which points to real-time scanning.
  - The lead does not change security software (a system setting). The user may pause or exclude it.
  - A background waiter (task b4q4qte6w) times the PowerShell start every 5 min. It exits when the start takes 12 s or less twice in a row (6 h cap). Then: a CompileCheck, then the launch of wave A by scriptPath.
- Two other Unity editors (pids 13648 and 13296, other projects) are running; they are not ours, leave them.
- Third attempt, 09:11 (Logs/lead-resume4c-compile.log), after the first waiter saw a 9 s PowerShell start:
  - licensing worked this time, but the ILPP runner (pid 20888 in Library/ilpp.pid) never opened its pipe; 30 retries in 16 min;
  - killing the runner did not make Unity restart it, so the lead stopped the run; no Unity of ours is left;
  - MBAMService was still at about 1 core.
- A stricter waiter now runs (task b009ah3f1). It needs a PowerShell start of 3 s or less and MBAM under 0.25 cores, three checks in a row, checked every 10 min, with an 8 h cap.

## (history) PAUSED 2026-11-19 ~20:05 (user: "останови пока работу"; lead session <session>)
- Run wf_8e439303-949 (task wsu28qqf3) stopped by the lead. The GPU watcher (b2oexu530) is stopped too. No Unity or Nocturne player of this project is running.
- In the third start, 14 owner commits landed (1e9f3e6..e5e394b): AU is DONE (EditMode 408/410, the failure is outside audio; the -audio-dump check is +0.10 dB). BV, AB, SG, FX and HT committed green steps and are still in progress. AT and IA had not reported.
- The uncommitted owners' work (HT Heart / Spire / koi, SpireDriver.cs, NocInlay.shader, Tram.mat, MaterialLibrary, GraphicsSettings, facade_sheet.py, SG log and critique-SG.md, QA report refreshes: 23 files) is saved as `docs/wip/p4a-wip-2026-11-19c.patch`. The `git apply --check` against HEAD is OK. Those files are reset to HEAD; fastcheck runtime 198 / editor 264 OK. The untracked QA/reports artifacts are left in place.
- **To resume:**
  1. `git apply docs/wip/p4a-wip-2026-11-19c.patch`.
  2. Run fastcheck, then a Unity CompileCheck.
  3. Re-arm the GPU watcher (see memory gpu-care).
  4. Relaunch the same script by scriptPath (below) with a FOURTH-start restart note: AU is done (skip it); owners read their logs and commits first; HT must re-check its patched files (Unity tests pending).
  5. Wave B and D-034 follow as planned.

## (history) NOW: PHASE 4 WAVE A, THIRD START (2026-11-19 ~17:45, lead session <session>, after the user's "продолжай")
- `docs/wip/p4a-wip-2026-11-16b.patch` re-applied (38 files, uncommitted, owned by the wave A owners). Before the launch: fastcheck runtime 198 / editor 263 OK; Unity CompileCheck 0 errors (Logs/lead-resume3-compile.log).
- Running: run **wf_8e439303-949**, task **wsu28qqf3**, script `C:\Users\<user>\.claude\projects\W--YouTube-Megastructures-6-Megastructures-6-Megastructures\<session>\workflows\scripts\nocturne-phase4-wave-a-wf_2b101732-ff0.js`. Its COMMON now carries a "THIRD start" restart note: owners continue from their logs, the in-flight Unity work to redo, commit early, D-034 is for wave B (leave hooks, no AI or imported meshes), and GPU CARE.
- GPU care (user, 2026-11-19: "remember the GPU load, watch it"). The lead runs a watcher (scratchpad gpu_watch.sh, background task b2oexu530). It logs `Logs/gpu_watch.csv` every 20 s and exits on a critical reading: 84 C or more for 40 s, free VRAM under 1 500 MiB, thermal or HW slowdown, or an nvidia-smi failure. On an exit the lead stops this project's players and re-arms the watcher. At the launch the GPU was at 100 % from another project's player (Cleft.exe), 62 C, 8.2 GB free; Blender was open.
- If paused: TaskStop wsu28qqf3, save the owners' uncommitted diff as `docs/wip/p4a-wip-<date>c.patch`, reset only those files, commit STATE.md + the patch. To resume: re-apply it and relaunch the same script.

## (history) PAUSED 2026-11-16 11:37 (user: "останови пока что работу. Я напишу когда продолжать")
- Wave A second start, run **wf_82c73c1d-0e6** (task w00xcwyqj, 10:05-11:37), stopped by the lead with TaskStop at the user's request. No owner had written "<ID> DONE"; IA and the six critics had not started. No Unity or player process of game 6 is left running.
- Committed during the second start (67f0762..c692b3a, 15 commits): AU score checkpoint, AudioScoreTests, AudioGates, docs/AUDIO.md, QA/critique-AU.md, Tools/audio/analyze.py; HT koi spine of D-032 2 and drop knots (HT-t02 38/38); AT abyss of H3, deck billows, abyss scroll (AT-t02 66/66); FX lead requests of D-032 9, QA-2/6/7, TP-2 (v1 facade and surface paths deleted); BV tram T_A (schedule, builder, driver, NocTram, overhead line, depot, terminus hall, TramTests); SG LED screen wall, glyph signs, arcade block, glyph column (SG-t02 16/16); AB abyss content first cut (eight traffic layers as vehicle rivers, bridge lamps, lit Gulf faces, AbyssTests).
- Uncommitted remainder saved as **docs/wip/p4a-wip-2026-11-16b.patch** (38 files, +1 666 / -383): BV BoulevardLights.cs, ShopFronts.cs, tram / terminus / facade / median / blade-sign edits, Tram.mat, MaterialLibrary.asset, NocShopfront.shader; AB AbyssTraffic, Beat03_Edge, EdgeDims, TonguePier, GulfRibBuilder, SkybridgeLamps, AbyssTests, Beat03EdgeTests, Q1IntegrationTests, QA/reports/AB_ks10_search.md; SG NocScreenMath, NocScreenWave.hlsl, Beat06SignsTests; HT HeartLayout, OldQuarterBuilder, Beat07HeartTests; FloorMoss.mat, GraphicsSettings.asset, visibility and graybox01 reports; logs BV and HT. `git apply --check` OK, and those files were reset to HEAD. HEAD check: fastcheck runtime 196 / editor 261 OK, Unity CompileCheck 0 errors (Logs/lead-pause-compile.log). EditMode was not run at HEAD.
- The first-start checkpoint docs/wip/p4a-wip-2026-11-16a.patch is superseded (its content is committed or inside 16b): do not apply it.
- Unity work in flight at the stop, to redo: lookdev_BV and lookdev_FX builds, lookdev_AU (in-game -audio-dump check), lookdev_SG KS16 r01, AB r02. Open note from SG's log: VisibilityAuditTests KS19 koi body had 0 core samples before HT's 7b0dbaa; re-run to confirm it is fixed.
- **To resume:** `git apply docs/wip/p4a-wip-2026-11-16b.patch`; fastcheck + CompileCheck; update the RESTART NOTE in the script (second stop: the tree is HEAD + 16b; commit early) and relaunch it with `resumeFromRunId` wf_82c73c1d-0e6 (no agent finished, so all seven owners run live and read their logs first); record the new run and task here. The first start ran into the account weekly limit (resets Nov 20 9:00 Warsaw).

## NOW: PHASE 4 WAVE A (started 2026-11-16 ~08:40, lead session <session>)
- Look v2 is done and accepted (D-031): run wf_2c6d782f-763, all eight packages DONE, INT DONE 07:32 (HEAD 1cb2a6f). The lead re-verified: fastcheck OK, EditMode 361 / 361 (lead-verify-tall01), looked at the lookv2_01 sheet and KS00 / KS04 / KS09 / KS16 / KS17 / KS19. Open questions arbitrated in D-032; the Phase 4 plan is D-033.
- **Second start, paused 11:37 (see PAUSED above):** run **wf_82c73c1d-0e6** (task w00xcwyqj, launched 10:05, same script with a RESTART NOTE: owners continue from their logs and the uncommitted tree, and commit as soon as fastcheck and their shaders pass). The first start, run wf_2b101732-ff0 (task w8kbv70w0, 08:05-09:56), was cut off by the account usage limit ("weekly limit, resets Nov 20 9am"). Every agent stopped mid-step with nothing committed; the user said "Try again". Its work is still in the tree, with a checkpoint copy in docs/wip/p4a-wip-2026-11-16a.patch (124 files, f0b8ce5). The lead fixed the AbyssTests string literal; fastcheck and CompileCheck were green.
- First start: workflow wf_2b101732-ff0 (task w8kbv70w0; script `C:\Users\<user>\.claude\projects\W--YouTube-Megastructures-6-Megastructures-6-Megastructures\<session>\workflows\scripts\nocturne-phase4-wave-a-wf_2b101732-ff0.js`): BV, AB, AT, HT, SG, FX, AU in parallel (exclusive files below), then IA (gate, captures `QA/captures/p4a_01*`, `QA/critique-p4a-01.md`), then six read-only critics (scale, tech, art, pacing, spec, wow) returning structured findings to the lead.
- Lead prep before the launch (a9d7137): material row hooks `MaterialFactory.{Boulevard,Heart,Signs}.cs` (empty, hooked into Specs(); CompileCheck 0 errors); stale builds and intermediate capture folders deleted (W: 24 -> 32 GB free).
- If paused: TaskStop w00xcwyqj, save `git diff` of the owners' uncommitted files as `docs/wip/p4a-wip-<date>.patch` (verify `git apply --check`), reset only those files, commit STATE.md + the patch. To resume: re-apply the patch, fastcheck + CompileCheck, relaunch the script with `resumeFromRunId` (owners read their logs first: RESUME RULE in the prompt).
- Then: merge the IA critique and the six critics into one list, decide, and launch wave B (AL beat 1, TR beat 4 trains, GD beat 5 garden + critic fixes to wave-A owners) + the horror and megalophobia layer of D-034 / docs/DREAD.md (new package DR for the hull, elevators, abyss shadow and falling panel; sound, camera, darkness wave, koi eye, wall eye, sheet lightning and traces of emptiness to the area owners; order: sound first, then hull, abyss shadow, look-ups and vertigo, darkness wave); add a seventh critic lens "dread".

## RESUMED, THIRD START (2026-11-15 ~21:25, new lead session <session> after the user's "continue")
- `docs/wip/lookv2-build-wip-2026-11-14b.patch` re-applied (131 files, uncommitted, owned by the packages). Checked before the relaunch: fastcheck runtime 162 / editor 215 OK; real Unity CompileCheck of every assembly incl. Megastructure.Tests.EditMode 0 errors (Logs/lead-resume-compile.log, 21:17). The FoliageTests / FacadeSkinTests `NocPaths` errors noted below were already fixed inside the patch.
- Build workflow relaunched as run **wf_2c6d782f-763** (script `C:\Users\<user>\.claude\projects\W--YouTube-Megastructures-6-Megastructures-6-Megastructures\<session>\workflows\scripts\nocturne-lookv2-build-r3-wf_2c6d782f-763.js`): TP, TQ (new, D-030), PA, KO, FA, SU, FO, GE in parallel from their logs, then INT. Per-package resume notes are in the script.
- D-030: TQ takes TP-0 step 6 (ShotPlan, CaptureHarness, PerfMonitor, tiling.py, debugview.py, sequences.py dolly, run_all.py; marker "TQ SETS READY"); TP-0's acceptance re-arbitrated for the shared tree (PA's palette v2 is live, so keyshots1080 is compared structurally, not within 0.2 levels); every package commits each green step; every package ends with "<ID> DONE <time>" in its log.
- Machine at the resume: games 4 and 5 are running in other sessions (Bulwark player, Unity on 5 Megastructures): never touch them; gpu-guard shares the GPU turn. W: 31 GB free.
- If paused again: stop the workflow (TaskStop wrj80v49q / run wf_2c6d782f-763), save `git diff` of the uncommitted package files as `docs/wip/lookv2-build-wip-<date>c.patch` (it supersedes patch b), reset only those files, commit STATE.md + the patch; to resume, re-apply it and relaunch the r3 script with its RESUME paragraph updated.

## PAUSED AGAIN (2026-11-14 13:30, by the user: "stop development for now, record where you stopped, continue when I write")
State at this pause (supersedes the 10:10 pause below):
- The resumed Look v2 build workflow (run wf_6053e1ff-988, started 12:22, same script file as below) was stopped after about 66 minutes. No Unity or player process of this project is left running.
- The tree is reset to the last green commit (373a0ef + this pause commit); fastcheck runtime 150 / editor 188 OK. No package committed during the resumed run; no new marker (still only PA TOKENS READY).
- ALL unfinished work (the 10:10 patch plus 66 more minutes) is saved as ONE patch: `docs/wip/lookv2-build-wip-2026-11-14b.patch` (131 files, 1.15 MB; it supersedes `...-2026-11-14.patch`, do not apply both). `git apply --check` passes on this commit. With it applied, fastcheck was OK (runtime 162 / editor 215 files) at the pause; the EditMode test assembly had one open compile error (FO's FoliageTests.cs:33 needs `using Megastructure.EditorTools` for NocPaths - REQUEST in SU.md).
- Where each package stood (details and resume rules in docs/logs/<ID>.md):
  - TP: TP-0 steps 1-5 in the tree (lit template, NocLookGlobals, debug views, NocTexturing.hlsl + NocTexturingMath, NocTextureImport, gen_hlsl/ShaderGen headers, MaterialFactory/MaterialLibrary/MaterialTests split into .Facade/.Surface/.Look/.Foliage and handed over); step 6 (capture sets, PerfMonitor) and the TP-0 acceptance run not done; TP-1 (texprep of the sets) not started.
  - PA: palette v2 values, Floor, fog 4.4e-4, the 4-octave cloud model with domain warp and per-glow tints, post tables, KS13/KS03 rows, gaze keys, GoalLampCore, AtmosphereDriver, light colours in the beat files, shader edits (Atmosphere, Sky, Grade, Neon, Streak, Rain, Screen) - all uncommitted, not yet verified in captures.
  - KO: koi v2 in progress (Kohaku patches 21.5 %, scale/vein structure, pectorals, dorsal, 4 barbels) in NocKoi/KoiBuilder/KoiDriver/KoiShaderMath + KoiShaderTests; critique-KO.md.
  - FA: NocFacadeConstants, NocFacadeSkin.hlsl, v2 path in NocFacade.shader behind NocFacadeV2(), NocFacadeMath twin, CityMesher/FacadeKit/StreetFacade/G4Kit writers, FacadeSkinTests; FA-1 not finished (no marker).
  - SU: NocSurfaceConstants (52 constants) + generated header, NocSurfaceMath, NocMirror wall blend, SurfaceTests; took over MaterialFactory.Surface / MaterialLibrary.Surface / MaterialTests.Surface; waiting for TP-1 sets.
  - FO: NocFoliage v2 shader, GardenLeafCards, GardenTree.EmitCrowns, GardenDims/GardenMaterials, FoliageTests; REQUEST SU for the Beat05_Garden.cs call; waiting for TP-1 leaf atlases.
  - GE: GB-19 AutoTestRunner (-beat N) and visibility edits (VisibilityAudit/Subjects, VisibilityAuditTests) in the patch; log not updated since 09:31.
How to resume: `git apply docs/wip/lookv2-build-wip-2026-11-14b.patch`, then relaunch the build workflow script (RESUME paragraph already in its COMMON; update its first lines to name patch b and this pause), agents continue from their logs; then INT.

## RESUMED 2026-11-14 12:22 (WIP patch re-applied, build workflow relaunched in resume mode as run wf_6053e1ff-988); history of the pause:
## PAUSED (2026-11-14 10:10, by the user: "stop development for now, record where you stopped, continue when I write")
State at the pause:
- The Look v2 build workflow (run wf_5d27e29a-183, script `C:\Users\<user>\.claude\projects\W--YouTube-Megastructures-6-Megastructures-6-Megastructures\<session>\workflows\scripts\nocturne-lookv2-build-wf_5d27e29a-183.js`) was stopped after about 40 minutes; no Unity or player process of this project is left running.
- The tree is at the last green commit plus this pause commit: fastcheck runtime 150 / editor 188 files OK. Committed Look v2 work: PA's 15 new LOOK_V2 tokens (d0c4a25, marker "PA TOKENS READY"), TP-0 step 1 shader-variant baseline + Validate gate (63206a5), SU's carried Q1 lines (KS02 catwalk z 96.0-97.5, GB-18 balcony slab y 3.4-3.6, lamp y 5.0), the inactive leak-anchor bake and the surface.json draft (23646db).
- Unfinished edits of the packages (did not compile at the pause: PA was mid-way through the palette values, NocSchedule.cs still referenced CloudNoiseAmp / CloudGlowGain) are saved, not lost: `docs/wip/lookv2-build-wip-2026-11-14.patch` (18 files: PA NocturneConstants/NocturnePalette; TP NocLitPass/NocLitCore/NocCommon/NocLighting/ShaderIds + new NocLookGlobals, NocDebugViewFeature, NocDebugView.shader; FA NocFacadeConstants (new), NocFacadeMath; SU NocMirror; FO GardenTree/GardenDims + foliage.json; GE AutoTestRunner (GB-19)). `git apply --check` passes on this commit.
- Markers reached: PA TOKENS READY only (TP-0 READY, TP-1 READY, PA-1 READY, FA-1 READY not yet). Each package's progress and its "Resume rule" are in docs/logs/{TP,PA,KO,FA,SU,FO,GE}.md.
How to resume (when the user writes):
1. `git apply docs/wip/lookv2-build-wip-2026-11-14.patch` (or skip it and let each agent redo its step from its log).
2. Relaunch the Look v2 build workflow from its script file with one line added to COMMON: "RESUME after a pause: read your log docs/logs/<ID>.md first, the lead re-applied the saved WIP patch; continue from your last step, keep your files compiling". Agents TP, PA, KO, FA, SU, FO, GE; then INT.
3. Then STATE "Next actions" below.

## Current phase
Phase 4 (beat production), wave A (D-033): BV beat 2 + tram, AB beat 3 + abyss content (S1 INT-01), AT atmosphere / sky / post / light rig, HT beat 7 (koi outline, goal window, plaza, spire), SG beat 6 (wall of signs, back side, arcade), FX cross-beat look polish + all shared files + lead requests + v1 deletion, AU score and soundscape; then IA and the six-lens critic panel. Look v2 (D-027..D-031) is complete.

## Milestones
| Milestone | Commit | Notes |
|---|---|---|
| Look v2 build accepted | 1cb2a6f | TP, TQ, PA, KO, FA, SU, FO, GE + INT: palette v2 (no violet), textured facades (v2 default), surfaces, foliage, koi v2, relief and Q1 gate structures; Validate 103/103, EditMode 361/361 (lead re-run), build 0/0, autotests exact, wall bots PASS, coverage 0/48 204; perf 1080 GPU 2.88 ms, 4K 9.0 ms; critique-lookv2-01: S1 1, S2 15, S3 26 |
| Phase 3 graybox gate (Q1) | bf38311 | builders G1-G4 + I0 + V; RebuildAll, Validate 90, EditMode 259/259, PlayMode 2+2 ign., build 0/0; autotests 599.050/601.450 and 998.100/1002.100 exact; wall bots PASS; coverage 0/48 204; Q1IntegrationTests (floor rays, body/camera capsule); critique-graybox01: GB-01 S1 + GB-02..07 S2 + GB-08..20 S3 (lead arbitration pending); Q2 stopped (superseded by Look v2) |
| Phase 2 completion wave A | f1a880b | A1 audit fixes + every verification system once (parity, statics, boundaries, perf 1080/4K, audio, autotests 1.5/2.5 exact, wall bot, ray grid 0/48 204); A2a atmosphere/lit template/22-material PBR library/NocWet; A2b NocFacade/CityField/NocNeon/NocStreak/traffic/NocRain/D-008; EditMode 177/177; decisions D-012..D-026 |
| Phase 2 framework import | ee52341 | 126/126 EditMode, Validate 66/66, D3D11 build clean, autotest-until 400, full autotest 1.5 and 2.5 m/s exact, 20-still capture; audit: 7 S2 + 15 S3 (fixed in wave A) |
| Phase 1 research | 94c475a | R1-R5 reports + IMPORT_PLAN.md, decisions D-002..D-011 |
| Phase 0 preflight | f220895 | Unity 6000.6.0f1 (f7f8ed4d1e24), URP 17.6.0, Input System 1.20.0, windows standalone support present, no lock, 66 GB free on W:, git init, .gitattributes `* -text` |

## Ownership table - Phase 4 wave A (D-033; full lists in the workflow script; cross-owner changes via "REQUEST <ID>:" lines in docs/logs)
| Owner | Files (under Assets/_Game unless noted) |
|---|---|
| BV | Beats/Beat02_Boulevard/**, Relief/BoulevardRelief.cs, Relief/CanyonBridge.cs, NocShopfront.shader, MaterialFactory/Library/Tests .Boulevard, Beat02BoulevardTests |
| AB | Beats/Beat03_Edge/**, Relief/GulfRibBuilder, ReliefBuilder, ReliefKit, GateDims, NotchGate, Beat04 SkybridgeLamps.cs, City/NocTraffic.cs, NocStreak.shader, NocLine.shader, Beat03EdgeTests, ReliefTests, Q1IntegrationTests |
| AT | NocAtmosphere.hlsl, NocSchedule.cs, NocSky.shader, NocGrade.shader + NocGrade*.cs, NocHalos.cs, AtmosphereDriver.cs, MaterialFactory/Library/Tests .Look, AtmosphereTests, GradeTests, Tools/qa/lookv2_check.py |
| HT | Beats/Beat07_Heart/** (KoiMath included), NocKoi.shader, Relief/NotchEastBlock, PromenadeLintel, QA/NocVisibility + Visibility*, MaterialFactory/Library/Tests .Heart, KoiMathTests, KoiShaderTests, VisibilityAuditTests, VisibilityWorld, Beat07HeartTests |
| SG | Beats/Beat06_Signs/**, NocScreen.shader, NocNeon.shader, NocSignSheet.cs, Relief/SignBaseRelief.cs, MaterialFactory/Library/Tests .Signs, Beat06SignsTests |
| FX | every other file (facade / surface / wet / glass / rain shaders, lit template, Core, Rendering, Player, Systems except AtmosphereDriver, QA except Visibility*, City except NocTraffic, Editor, Tools except lookv2_check / gpu-guard / metrics / pricing, Textures, Materials, Scenes, Settings, ProjectSettings) and their tests; carries requests |
| AU | Scripts/Runtime/Audio/**, AudioTests, Tools/audio/**, docs/AUDIO.md |
| nobody (wave B) | Beats/Beat01_Alley/**, Beats/Beat04_Skybridge/** except SkybridgeLamps.cs, Beats/Beat05_Garden/**, NocFoliage.shader, .Foliage partials |
| Lead | STATE.md, DEVELOPMENT.md, README.md, FEATURE_MATRIX.md, FINAL_REPORT.md, docs/RESEARCH.md, Tools/metrics.py, Tools/pricing.json |

## Ownership table (Part 5.2) - Phase 3, historical (logs in docs/logs/<ID>.md)
| Area / files | Owner |
|---|---|
| Scripts/Editor/**, Runtime Systems/Audio/Rendering/Core/Player/UI/Geometry/QA (except NocVisibility, Visibility*), Shaders/** (except Shaders/Beats), Beats/GrayboxKit.cs, existing tests, Tools/** (except gpu-guard, metrics, pricing), ProjectSettings, Scenes, Materials, Settings, Textures, docs/BUILDER_BRIEF.md | I0 integrator |
| Beats/Beat07_Heart/KoiMath.cs, QA/NocVisibility.cs + Visibility*.cs, KoiMathTests, VisibilityAuditTests, QA/reports/visibility.* | V |
| Beats/Beat01_Alley, Beat02_Boulevard (+ their tests, Shaders/Beats files they create) | G1 |
| Beats/Beat03_Edge: tongue A, Gulf faces (portals, garden notch walls), cross bridges, static layer lines | G2 |
| Beats/Beat04_Skybridge, Beat05_Garden (NocFoliage) | G3 |
| Beats/Beat06_Signs, Beat07_Heart except KoiMath (NocScreen, NocKoi, koi driver) | G4 |
| Runtime/City/** | frozen (lead) |
| After the builders: everything | Q1, then Q2 (sequential) |
| STATE.md, DEVELOPMENT.md, docs/RESEARCH.md, docs/research/*, Tools/metrics.py, Tools/pricing.json | Lead |

Geometry agreements given to the builders: walkable tops at y 0 (lib.FloorFor); boulevard floor ends at x 215.3 (tongue from there), slabs end at x 213 and G1 closes to face A; face A above y 0 only for z < 120 and z > 270; tongue deck to x 375.3, bridge deck from 375.3, gate blocks are G3's; Gulf faces carry the tube and boulevard-lane portals; face B above y 0 cut away for z 115..275 between x_B and x 914 (garden notch, side walls by G2); terrace ends at z 275, G4 floors north of it (promenade, C3, x 914..930 gap, plaza); Gulf faces are closed masses readable from behind.

## Open defects
- Look (for Phase 4/5 critics): sky reads as a uniform saturated magenta (top-of-frame RGB about 150, 50, 145, luma 75-82 in KS04-KS18 of A2b_03); towers fade into the cloud as a comb of tops; puddle mask reads as large camouflage blots; mid-distance facades grey-violet (A2b open issue).
- Audio per-beat RMS 17-23 dB under 14.3 targets (skeleton score; Phase 4 audio).
- `-tier` reaches builders only as WorldContext.Tier.

## Next actions
1. When wave A reports: verify IA's gates myself (EditMode, autotests, captures), look at p4a_01 key shots / beats / 4K, merge IA's critique with the six critics into one ranked list, log decisions, update this file.
2. Wave B: AL beat 1 (alley windows / signs / steam / AC / clutter / noodle bar), TR beat 4 (trains T1-T7, KS11, KS13), GD beat 5 (8 840 lanterns, fireflies, light under the crowns, foliage v1 deletion), plus the critic findings for wave-A owners; then IA + critic panel again.
3. Phases 5-7: transitions / title / ending, bug-bash, final verification (soak, perf on a quiet machine, 4K encode in chunks, 4K finals), docs, metrics (Part 15.3), final Russian message. Keep README.md, FEATURE_MATRIX.md and the draft FINAL_REPORT.md current at every milestone (Part 2.4 safe landing).

## Commands (verified in Phase 2; see docs/IMPORT_LOG.md)
All Unity calls go through the queue; all player runs through run-player.ps1 (long GPU jobs are routed through gpu-guard automatically; never two players at once).
- GPU state: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/gpu-guard.ps1 -Status`
- Fast C# check (~9 s): `python -I Tools/fastcheck.py all`
- Unity compile: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/unity-queue.ps1 -Name <log> -Method Megastructure.EditorTools.BatchTools.CompileCheck`
- Rebuild all (scene, materials, settings): `... unity-queue.ps1 -Name <log> -Method Megastructure.EditorTools.BatchTools.RebuildAll -TimeoutMin 30`
- Validate (66 checks): `... unity-queue.ps1 -Name <log> -Method Megastructure.EditorTools.BatchTools.Validate -TimeoutMin 30`
- EditMode tests: `... unity-queue.ps1 -Name <log> -NoQuit -StallMin 0 -TimeoutMin 30 -ExtraArgs "-runTests,-testPlatform,EditMode,-testResults,<absolute root>\QA\reports\<name>.xml"`
- Build: `... unity-queue.ps1 -Name <log> -Method Megastructure.EditorTools.BatchTools.BuildWindows -TimeoutMin 40` -> `Builds/Nocturne/Nocturne.exe` (env `NOC_BUILD_DIR` gives a per-owner build folder; run-player `-Exe` runs it)
- Smoke: `powershell -NoProfile -ExecutionPolicy Bypass -File Tools/run-player.ps1 -Name smoke -TimeoutMin 12 -PlayerArgs "-autotest,-brisk,-autotest-until,400"`
- Autotest cinematic 2.5 m/s: `... run-player.ps1 -Name autotest_cinematic -TimeoutMin 15 -PlayerArgs "-autotest,-cinematic"`; walk 1.5 m/s: `-Name autotest_walk -TimeoutMin 25 -PlayerArgs "-autotest"`
- Captures: `... run-player.ps1 -Name cap -TimeoutMin 10 -PlayerArgs "-capture,QA\captures\<run>,-capture-set,keyshots1080,-freeze-clock"` (one value per option; one set per run)
- Metrics (Part 15.3): `python -I Tools/metrics.py --md QA/reports/metrics.md --json QA/reports/metrics.json`

## Digest (MASTER_PROMPT 14.0; the sections of Part 14 win if they differ)
- **Game.** A night megacity in rain with no ground: shafts from a magenta cloud deck (y +460) down into luminous haze (about y -2 000); the ground (y -5 000) is never built; the walk stays at y 0. Path **1 500.0 m** (filleted centre line, 3 corners), **600.0 s at 2.5 m/s (cinematic, constant)**, 1 000.0 s at 1.5 m/s. No people, no real text, no flashing, no shadow maps.
- **Beats (s range, m).** 1 Alley 0-120 | 2 Boulevard canyon 120-400 | 3 The Edge 400-560 | 4 Skybridge 560-880 | 5 Hanging garden 880-1 080 | 6 Wall of signs 1 080-1 260 (cut first) | 7 Heart Square 1 260-1 500. Starts in seconds at 2.5 m/s: 0, 48, 160, 224, 352, 432, 504; end 600.
- **Money shots.** KS04-KS06 boulevard look-up; KS09-KS10 the Edge and the plunge; KS17-KS19 the koi, then the warm window. 20 key shots (14.10).
- **Path.** C1 right at (0, 195) r 24; C2 left at (901.74, 195) r 30 (arc centre s 1 080); C3 right at (901.74, 387.88) r 30 (centre s 1 260); end (1 148.18, 387.88). Heading 0 to s 171, 90 from 208.7, 0 for 1 103.6-1 236.4, 90 from 1 283.6; x = s - 184.70 on the east leg.
- **Places.** A rift 600 m wide at the neck (faces x 215.3 and 815.3) flaring to 2 km north and south, a 300 m bridge between two tongues, the garden on the far tongue, Heart plaza (R 400 about the spire S = (1 330, 388)); the goal is a warm window 1.2 x 1.5 m at (1 156, y 4.4-5.9, z 387.9) on a 14.4 m tenement.
- **Constants.** Eye 1.7; speeds 1.5 and 2.5 m/s; fog `T = exp(-(sigma_eff d)^2)`, sigma 5.0e-3 (alley), 3.6e-4 (beats 2-4), 6.0e-4, 4.5e-4, 3.4e-4; vFOV 62-68; EV -0.2 to +0.3; near 0.05, far 12 000; Thread's ThreadGrade post stack with its one static TPDF dither (2.0 codes peak), 64-bit HDR; koi L 900, R 260, phase 6.64 + 1.76 (s - 1 260) degrees.
- **Cinematic gaze.** Keys (s, yaw rel, pitch) per beat, critically damped, yaw <= 22 and pitch <= 18 deg/s (peaks 19.3 and 17.9); every key shot is a schedule point, readable 5.6 to 25 s.
- **Hard problems (14.7).** H1 banding of dark haze, also after 4:2:0 encoding; H2 24 000 signs and millions of windows without moire; H3 no ground; H4 wet streets without SSR; H5 rain; H6 the koi; H7 traffic, tram, trains; H8 the pad DSP.
- **Cut order (14.8).** 1 wall of signs, 2 glass strip, 3 lanterns and fireflies, 4 steam and alley clutter, 5 tram, 6 cross bridges and lower traffic layers, 7 koi fins and scales, 8 FaceRib relief and interior mapping, 9 haze sheets. Never cut: fog and abyss, facade shader, wet floor, dither, rain, the koi core, the warm window, QA.
- **Easy to forget.** Events keyed to s; no shadow maps; the 14.10 table is the only source of capture poses; reuse per Part 3.1, write only the deltas.
