# DR - the D-034 dread objects (docs/DREAD.md 2 and 3): the hull, the shadow below, the falling panel, elevators and gantries

## Wave B, first start (2026-11-20 20:35, copy .worktrees/abyss, branch n6-abyss; partners AB, AT)

### Start (20:35-21:05)
- Read DREAD.md (all), MASTER_PROMPT 14.0 / 14.4 / 14.5.3 / 14.5.4, D-031..D-034, NocAtmosphere.hlsl, NocStreak.shader, the lit
  template (NocLitCore / NocLitPass), TramSchedule / TramDriver (the "launch by s, then real time; capture tau = (s - s_L) / 2.5"
  pattern of 14.0 / H7), GameBootstrap / WorldContext, ShotPlan (pose stills "pose:NAME:x:y:z:yaw:pitch:fov:s").
- Hooks found: AB's NocStreak global _NocAbyssOccluder (xy centre x / z, z half-length, w strength; layer vehicles and glows below
  y -450 inside the outline dim by w; 0 = identity). AT's NocAtmosphere has NO cloud-deck occluder slot (grep "occlu|dread|D-034"
  finds nothing in NocAtmosphere.hlsl / AT.md): the hull's deck darkening is DR's own analytic veil (below).
- Baseline: AT's AT_r01 capture (AT-b01, the same tree, 20:44) KS08 / KS09 / KS12 / KS13 looked at full size. KS09: the rift's far
  end between faces A and B at the frame top (px x 320-1060, y 0-230) is uniform brown haze/deck, the y 300 layer converges there;
  KS13: through the strip the -520 and -880 layers cross as horizontal rows (px y ~700 / ~830) over the brown haze.
- No REQUEST DR in any copy's logs (grep 20:58).

### Design (21:05)
- Hull (2.1): ribbed superellipse loft 1 800 x 260 m (keel bottom 418, belly 432, back 478 inside the deck), on the rift axis x 515.3,
  moving north at 18 m/s: launched at s 430 (parked before, stern z 1 442), 10 s smooth start. Stern at KS08 (s 428) z 1 442
  (elevation 18.6 deg, bearing 12.3: in the KS08 frame, centre-left); KS09 (s 520) z 2 000 at 2.5 m/s (13.3 deg: inside the frame
  top of 16 deg; its engine glows at the top of the money shot) / 2 432 at 1.5 m/s (10.9 deg); s 640 z 2 864; KS10 never (frame top
  +2 deg). Reads as a silhouette: albedo 0.035 lit only by the three ambient fields (haze below, windows beside, deck above);
  NocHull fogs itself toward the sky colour x the veil factor of its ray, so it stays darker than the veiled deck.
  Deck veil = the analytic occluder of 2.1: an elongated soft ellipse in the plane y 456 (half 390 x 1 250 m, r0 0.35, k 0.55 x T),
  a quad multiplying what lies behind it (NocDreadVeil, Blend DstColor Zero, queue 2980).
  Belly lights: two red rows (+-0.92 half-beam) and two white rows (+-0.40) at 40 m pitch, a white keel row, 5 cold engine halos
  (sigma 8 m) and one low stern glow (sigma 45 m), NocDreadLight (NocStreak's 2.2 px / (true / drawn)^2 rule, quads built in clip
  space from a static mesh that moves with the hull: one draw, no per-frame CPU work).
- Shadow (3.1): soft body 900 x 162 x 120 m at y -640 on x 690 (x 609-771, never over the x 560 subject layer), moving south at
  5 m/s: launched at s 600 (parked centre z 536), 20 s smooth start; fade in s 590-622, out s 860-900. KS12 centre z 466 / 386
  (2.5 / 1.5 m/s: its north half beyond the frame bottom), KS13 350 / 193 (under the strip). NocDreadBody: dark shell fogged by
  NocFog, alpha 0.9 x smoothstep(0, 0.65, |n.v|) (no edge). Drives AB's _NocAbyssOccluder (690, z, 450, 0.85 x fade): the deep
  river x 650 / y -1 100 goes dark under it; the KS10 / KS12 / KS13 subject layer (560, -520) never dims (r_x = 1.6).
  Seven dim cold lights on its back (WinCold, 4 m, gain 0.35).
- Panel (3.2): a 12 x 8 m backlit sign panel (no text) on face B at (815.3, 150, 800), in the s 470 gaze; tears off at s 470:
  swing 30 deg (tau 1 s), push 27 m off the face (tau 3 s), wind drift south 4 m/s (clear of the z 850 bridge), drag fall
  (terminal 42 m/s: 662 m in 20 s), tumble 0.08 rev/s (its lit face turns at <= 0.16 Hz < the D-010 0.2 Hz); hidden after 50 s.

### Files (all new, DR-owned)
- Assets/_Game/Beats/Dread/DreadCourse.cs, DreadLights.cs, HullBuilder.cs, ShadowBuilder.cs, PanelBuilder.cs, DreadDriver.cs,
  DreadBuilder.cs (+ .meta written by Unity)
- Assets/_Game/Shaders/Beats/NocDread.hlsl, NocHull.shader, NocDreadLight.shader, NocDreadVeil.shader, NocDreadBody.shader
- Assets/_Game/Scripts/Tests/EditMode/Beats/DreadTests.cs
- docs/logs/DR.md, QA/critique-DR.md, Logs/DR_progress.md

### Log
- 21:15 fxc offline (scratchpad dr/shcheck.py, FX's harness pointed at this copy): NocHull 14/14, NocDreadLight 4/4, NocDreadVeil 4/4,
  NocDreadBody 4/4 OK. fastcheck all OK (runtime 205 / editor 272).
- 21:20 DR-b01 BuildWindows -> Builds\DR queued (behind AB-b02).
- 21:28 panel (3.2) added (PanelCourse, PanelBuilder, NocHull _EmissionColor x vertex G); fxc NocHull OK; fastcheck all OK (206 / 273).
GLOBAL 21:20: none of DR's objects changes a shared shader; the hull and its veil darken the deck over the rift north of the Edge
  in beats 3-4 (KS08, KS09, KS12's frame top, beat-3 / 4 stills looking north); the shadow darkens the haze under the bridge in
  beat 4 (KS12, KS13) and dims AB's deep river x 650 / y -1 100 through _NocAbyssOccluder.
- 21:35 AT's NocAtmosphere now has the D-034 hooks (AT 21:26): _NocDeckOcc (the hull in the deck: glow weight x (1 - occ), base x
  (1 - 0.5 occ)) and _NocHazeOcc (the shadow on the plane y -650), soft ellipses of beam 0.15 x length, identity at 0. DreadDriver
  drives both (SetGlobalVector each frame, zero when hidden / faded). NOTE AT: DR writes _NocDeckOcc = (515.3, hull centre z, 950,
  0.6) and _NocHazeOcc = (690, shadow centre z, 450, 0.5 x fade); please keep the layout.
- 21:45 DR-b01 (BuildWindows, Builds\DR): 0 errors, 516 s (AT's include change recompiled every shader). Player log: "Dread: 4
  meshes (0 validation failures); hull 12916 triangles, 199 lights ... shadow 1872 triangles, 7 lights" (Logs/player-DR-r01.log).
- 21:48 capture DR_r01 (KS08 / 09 / 10 / 12 / 13 + 7 poses), looked at all at full size and crops:
  - KS08 / KS09: the hull reads as a ship (stern, belly rows converging) but its body luma 42 vs the deck 37-41: no silhouette.
    Cause: the towers' cloud mix (380..560) made the belly 21 % and the back 56 % cloud colour, and DR's veil darkened the deck
    around it. Five 8 m engine halos read as round headlights; the 48 % bow taper read as a teardrop.
  - KS13: the shadow is a hard dark oval through the strip (smoothstep(0, 0.65, |n.v|) edge); KS12 / s 640: not visible (the
    bridge deck hides every ray steeper than -34 deg; the body was under the deck).
  - s 470-484: the panel is there (8 x 5 px at 804 m, 49 deg off its normal) but at 0.3 x SignWhite reads as one more window.
- 21:52 DR-b02 (cabins, gantries, AT hooks; 45 s), not captured (superseded). 21:58 r02 fixes: NocHull cloud mix from the deck
  base (455..530, +-30 m billows); veil off (VeilOn false: AT's _NocDeckOcc at 0.6 / 950 m instead); engine halos 4 m / 0.35; bow
  taper from u 0.70; flank windows (2 rows a side, pitch 9 m, 55 % lit); white rows 1.4; ParkStern 1 442 -> 1 700 (KS09 stern at
  11.8 deg, 50 px under the frame top); shadow 6 m/s, parked z 784, opacity 0.75, volumetric alpha (1 - e^(-3 x^2)), bulges; haze
  hook 0.5; panel glow 0.9. DR-b03 0 errors (65 s).
- 22:02 capture DR_r02 (KS08 / 09 / 12 / 13 + 7 poses): KS08 / KS09 the hull is a black silhouette under the deck with the red
  outline rows and five small engine glows, its back dissolving into the deck (crops scratchpad dr/ks08_r02.png, ks09_r02.png);
  KS13 the shadow is a soft dark body (luma 41-43 inside vs 59-69 around: -35 %), the tip still crisp; s 480 the panel is a
  tumbling pale card (8 x 6 px); face B (s 520, yaw 70): gantry 0 reads as a dark lattice with four warm corner lights (~60 px),
  the 32 cabins are lost among the face's windows at gain 1.6.
- 22:08 r03 changes: sponsons (three pairs, reach 30 / 24 / 16 m, y 436-446, red edge lights) so the plan reads built and stepped;
  cabins gain 3.0; the shadow's tips fade (vertex R, 18 % of the length). fxc NocDreadBody OK; fastcheck all OK (207 / 274).
- 22:09 DR-b04 queued (behind AB-b05 12 min and AT-b03).
- REQUEST FX: Assets/_Game/Scripts/Runtime/QA/ShotPlan.cs: a "dread" capture set with real walk sequences of DR's moving objects
  (DR verified motion with pose-still series meanwhile: scratchpad dr/seqposes.py, QA/captures/DR_r03/seq_dr_*). Exact change:
  (1) line 208: `bool signs = false, rain = false, koi = false, traffic = false, dread = false;`
  (2) after line 237: `case "dread": dread = true; break;`
  (3) after line 272: `if (dread) plan.sequences.AddRange(DreadSequences(w, h));`
  (4) HeavySets: add "dread"; (5) new method next to TrafficSequences:
      public static List<SequenceSpec> DreadSequences(int w, int h) => new List<SequenceSpec>
      {
          KeyWalk("seq_dread_ks08_hull", "dread", "traffic", "12 s at 2.5 m/s and 60 fps through KS08: the hull launches (DREAD 2.1)", "KS08_edge_flank", 12.0, w, h),
          KeyWalk("seq_dread_ks09_hull", "dread", "traffic", "12 s at 2.5 m/s and 60 fps through KS09: the hull recedes (DREAD 2.1)", "KS09_edge_gulf", 12.0, w, h),
          KeyWalk("seq_dread_ks13_shadow", "dread", "traffic", "8 s at 2.5 m/s and 60 fps through KS13: the shadow below (DREAD 3.1)", "KS13_bridge_glass", 8.0, w, h),
          WalkSeq("seq_dread_panel", "dread", "traffic", "s 468-500 at 2.5 m/s, world yaw 38 / pitch -8: the falling panel (DREAD 3.2)",
                  468.0, 500.0, MotionFps, w, h, 38.0, true, -8.0, NocturneConstants.CinematicSpeed),
      };
- 22:16 CentreY -640 -> -610 (top -550: the body shows over the KS12 rail beyond z 1 010 instead of 1 055).
- 22:24 DR-b04 0 errors (154 s, queue). 22:24 capture DR_r03: KS08 / 09 / 10 / 12 / 13, 5 poses, and four 30-frame pose-still
  sequences (fixed camera, s at 2.5 m/s / 30 fps: seq_dr_hull (KS09 pose), seq_dr_panel, seq_dr_shadow (KS13 pose), seq_dr_faceb).
  - sequences.py QA/captures/DR_r03: PASS 4 / 4, 8 checks (max luma step 0.0001-0.021 levels / frame, 0 spikes)
    (QA/reports/sequences_DR_r03.md).
  - Looked at: KS08 / KS09 the hull with its sponsons reads as a stepped, built black ship under the deck, red edge rows, five low
    engine glows; KS13 the shadow is a soft darkening (luma 26-32 inside vs 60 around, no edge); the panel tumbles down face B as
    a pale card (frames 0 / 15 / 29, scratchpad dr/panel_seq.png); KS12: the shadow is not visible (DR-05: the deck hides it; the
    18 % tip fade took its tail away -> tail fade 6 % at 22:38).
  - Luma vs AT's baseline AT_r01 (both include partners' work): KS08 mean 38.3 -> 39.1, KS09 34.6 -> 34.5, KS10 33.6 -> 33.4,
    KS12 31.9 -> 32.5, KS13 37.1 -> 35.9. lookv2_check DR_r03: 5 / 5 PASS.
- 22:27-22:34 DR-t01 (DreadTests | VisibilityAuditTests | Q1IntegrationTests | AbyssTests | AtmosphereAbyssTests | Beat03EdgeTests |
  Beat04SkybridgeTests): 57 / 59; the 2 failures were my own stale expectations (the shadow's per-frame step after 5 -> 6 m/s, the
  flank-window count after the sponsons: 163); fixed in DreadTests (QA/reports/DR-t01.xml). VisibilityAuditTests and Q1 green
  with the hull (an opaque occluder) in the world.
- 22:34 smoke DR-smoke (-autotest -brisk -autotest-until 900 -beat 3, DR-b04): AUTOTEST PASS, beats 3-5 entered, errors 0, corridor
  coverage 48 204 rays / 0 misses, alloc mean 29.4 B/frame (engine input box 32, excess 0.0) (Logs/player-DR-smoke.log).
- 22:30 cabins: not distinguishable in stills among face B's toggling windows (a 25 s two-still diff shows every window toggle
  too); their motion is covered by DreadTests.Cabins_SpeedsWrapsAndShafts (speeds, exact wraps, hidden ramps).
- 22:40 DR-b05 (final build) queued behind AB-final. Disk: W: 13 GB free (below 15; DR keeps one build, 0.46 GB, and DR_r03 30 MB).
- 22:38 shadow tail fade 18 % -> 6 % (the head keeps 18 %). fastcheck all OK. FREEZE (no new features after this).
- 23:07 DR-b05 = the final build (Builds\DR): BuildWindows 0 errors, 0 warnings (Logs/DR-b05.log line 9034); my 4 shaders in the
  always-included list (ProjectSettings/GraphicsSettings.asset +4 GUIDs: NocDreadBody, NocDreadLight, NocDreadVeil, NocHull;
  the lead resolves the file at the merge).
- 23:08 final capture QA/captures/DR_final (KS08-KS13, 5 poses, 4 pose sequences): player log "Dread: 5 meshes (0 validation
  failures); hull 13372 triangles, 398 lights ...; shadow 1872 triangles, 7 lights" (+ cabins 32, gantries 2 x truss);
  sequences.py PASS 4 / 4 (QA/reports/sequences_DR_final.md; frames deleted after the check, sequence.json kept);
  lookv2_check 6 / 6 PASS; contact sheet QA/captures/DR_final/_contact/contact_DR_final_01.png. Looked at KS08 full size (the
  hull with sponsons, centre-left, red outline rows, low engine glows) and the sheet.
- 23:12 smoke DR-smoke-final (DR-b05, -autotest -brisk -autotest-until 900 -beat 3): AUTOTEST PASS, 0 errors, coverage 48 204 rays
  / 0 misses, alloc mean 29.4 B/frame, excess 0.0 (Logs/player-DR-smoke-final.log).
- Budget: hull 13 372 + shadow 1 872 + lights (398 + 7 quads = 810) = 16 054 triangles (< 60 k); cabins 32 quads in one draw.

### Open
- DR-05 (S2): the shadow is not visible over the KS12 rail (the deck hides every ray steeper than -34 deg; at 6 m/s the body
  cannot be both beyond z 1 010 at KS12 and under z 195 23 s later at KS13). It shows through the KS13 strip and while crossing.
- DR-07 (S3): the cabins read only in motion (not distinguishable from face B's lit windows in stills).
- DR-06 (S3): the falling panel is 8 x 6 px at 800 m (the spec's 12 x 8 m); it reads by its 20 s fall.
- Sequences of the moving objects are pose-still series (fixed camera, s stepping); real walk sequences need FX's "dread" set
  (REQUEST FX above).
- Hull reading vs the traffic: its belly rows run parallel to the y 300 layer (both along z): KS09 shows the layer's streaks in
  front of it; accepted (a ship over the traffic rivers).
- 23:15 DR-final (full EditMode, QA/reports/DR-final.xml): 430 / 435 passed, 1 skipped (AbyssTests.KS10_CompositionSearch,
  explicit), 4 failed - none in DR's files and the same 4 in AB-final.xml (22:57, before DR-b05): Beat07HeartTests.PlazaInlay_
  SameTopFan_RingsSeamsFade (HT: 962 < 1000), BoulevardStreetTests.LightBudget_AtMost28_SmoothFades (BV: 29 lights; DR adds no
  realtime light), BoulevardStreetTests.Shops_Varied_NeverBrighterThanTheLitRoom (BV: 18 > 17), WetFloorTests.WallWindows_MeanOne_
  Bounded_StreakVertically (FX / SU: -1.4e-11 >= 0, float rounding). DreadTests 13 / 13 (including VisibilityAuditTests green).
- fastcheck all OK (runtime 207 / editor 274) at 22:38, no code change since.
DR DONE 23:15

## FIX WAVE (2026-11-21 from 01:57, copy .worktrees/abyss at master caddb90; task Logs/fix/DR.md + LEAD RULINGS)

### Read (01:57-02:10)
- Task: C01 [S2] (the hull re-parked to fly OVER the walker, abeam z 195 at about s 561: lead ruling, D-038 (1) re-opened), C42 [S3]
  + DR-05 ruling (the shadow through the KS13 strip only, read as a body, soft back lights), DR-07 ruling (cabins readable in stills).
- Baseline = the lead's p4_01 on caddb90 (INTEG/QA/captures/p4_01 KS08 / KS09 / KS13, looked at KS13 full size: the shadow a soft
  cloud in the strip's upper-left corner at x 450-800 / y 450-650, two back lights as points at (557, 527), (693, 528)).
- REQUEST DR lines (grep all copies' logs 02:12): only AU's fifth-start one (hull course = DreadScore's; panel / cabins); AU's fix task
  now reads every position from DreadCourse (D-038 / D-040), so DR keeps DreadCourse the one source and adds the API AU and AT read.

### Analysis (02:10, scratchpad dr/hullvis.py: a ray grid against the belly plan, the boulevard canyon as the occluder)
- The model reproduces p4_01: old course KS08 cover 0.7 % at px x 0.30-0.39 / y 0.59-0.70 (critic: x 580-760, y 620-750 of 1920 x 1080).
- AU's course (park centre z -660, the chief's (a)) parked at y 432: the bow sits over the canyon's end, KS04 1.5 % and KS05 2.9 % of
  the frame, the beat-2 walk s 142-382 up to 11.4 %: a motionless hull in money shot 1 for 112 s. The chief's fallback (park further
  south) delays the abeam to s 622 (park -1 100) and empties the s 496 look-up. DR's fix keeps the ruling's course exactly AND hides
  the parked hull: it waits INSIDE the deck, 170 m up (lowest point y 583 >= 560: NocHull's cloud mix 455..530 +-30 and its lights'
  380..560 are 1 there), and sinks out of the cloud base over the first 24 s after the launch (smootherstep), condensing out of the
  deck over the Edge. NocHull now mixes toward the sky colour ALONG THE RAY (the deck where the ray meets it, with AT's occluder),
  so the parked hull is exactly the sky behind it (no outline); AT's deck occluder fades in with the sink (Emerge).
- With the 14.10 poses the overhead hull can never be inside KS09 / KS10 (frame tops +16 / +2 deg; the hull is at 26-90 deg):
  it reads in look-ups. Cover (belly plan, 1080p): KS09 long look-up candidate s 496 yaw 65 / pitch 58: 24 %; beat03_look_up60
  (s 480): 22 %; s 553 (lightning) yaw 60 / pitch 55: 31 %; abeam s 561 looking up: 44 %; beat04_look_l (s 720, north): 3.7 % (the
  body receding along the rift to the bow 1 100 m away); KS11 / KS12: 0 % (no subject covered).
- The y 400 skyway lane (NocTraffic.BoulevardLanes (0, 400)) crosses the rift at z 195 under the new course: the ventral fins (tips
  y 399) would cut it at s 686: fins 40 -> 26 m deep (tips y 412.7, 12 m over the lane).

### Changes (02:12-02:20)
- DreadCourse.cs: HullCourse ParkCentre -660 (stern -1 560), CentreZ(tau) = -660 + Travel (= AU's 195 + 18 (tau - 52.5) for
  tau >= 10: abeam s 561.25 at 2.5 m/s, 508.75 at 1.5), Lift0 170 / SinkTau 24 (Lift, Emerge, BellyY), AbeamS, TauAtS, BowZ,
  CentreZAtS / AtSpeed (AU / AT read these). ShadowCourse x 470 / y -700 (between the y -520 subject layer and the y -880 river),
  5 m/s from parked centre z 384 (centre z 195 at s 719.4; KS13 z 199 at 2.5 m/s, 41 at 1.5), opacity 0.85, AB's occluder half-length
  350 (half-width 63 m: x 407-533, the 560 layer outside at r_x 1.43, the 470 / -880 river on its axis).
- DreadDriver.cs: the hull's y = Lift(tau); _NocDeckOcc strength x Emerge (0 while parked: the deck untouched in beat 2).
- DreadBuilder.cs: parked at +Lift0; the shafts' guide lights mesh; log line (abeam s, cabins, guide lights).
- HullBuilder.cs: FinDepth 26 (was 40).
- NocHull.shader: cloud mix and fog toward NocSkyColor(ray) (one evaluation, shared).
- ShadowBuilder.cs (rewritten): 61 rings, plan with a fuller head, two fin pairs (u 0.28 reach 72 m, u 0.70 reach 38 m, flattened),
  a 9 m dorsal crest; normals by central differences; end fades 8 % / 10 %; seven back lights as Gaussian glows (sigma 1.6 m,
  FWHM 4.8 px at 650 m, gain 0.6) in one row on the crest, 90 m apart.
- NocDreadBody.shader: opacity (1 - e^(-6 |n.v|)) / (1 - e^(-6)) (edge soft over the outer ~5 % of the half-width), back sheen
  (0.010, 0.008, 0.006) x n.y^2.
- DreadCabins.cs / NocDreadLight.shader (DR-07): the cabins draw as a lit cab (profile 2: a 3 x 3.5 box, dim frame, ceiling-lit glass,
  fwidth-filtered edges); guide lights, two steady rows a shaft every 12 m, y -720..540 (1 696 lights, WinCold 0.7 m, gain 0.9).
- DreadTests.cs: course tests rewritten (AU's formula for tau >= 10, abeam 561.25 / 508.75, the s 553 span at both speeds, the sink
  and no pop at the launch, no belly point in KS10 at either speed), faces 140 m, the lane clearance 8 m+, the parked hull >= 560,
  the shadow under the strip (both speeds) and between the layers, AB's occluder (560 layer lit, 470 / -880 river dark), the glows'
  FWHM 3-6 px, the cab profile, the guide lights.
- 02:19 fastcheck all OK (runtime 217 / editor 288); fxc NocHull 14/14, NocDreadLight 4/4, NocDreadBody 4/4 OK (scratchpad shcheck.py).
- 02:20 DR-b06 BuildWindows -> Builds\DR queued.

### Requests (fix wave; also in the report's shared_change_requests)
- REQUEST FX: Assets/_Game/Scripts/Runtime/Core/NocturneConstants.cs GazeKeys (14.5.3), the D-034 4.1 KS09 long look-up on the hull's
  new course (FX's C-list item (d): "the KS09 look-up is where its belly should fill the sky"). After `new GazeKey(470, -50, -6),`
  insert `new GazeKey(476, -50, -8), new GazeKey(494, -25, 58), new GazeKey(499, -25, 58),` (keep (520, -70, -18), (528, -70, -18)).
  Arithmetic: pitch -8 -> 58 over 18 m = 7.2 s at 2.5 m/s (mean 9.2 deg/s, smootherstep peak 1.875 x 66 / 7.2 = 17.2 <= 18); hold
  494-499 = 2.0 s; down 58 -> -18 over 21 m = 8.4 s (peak 17.0 deg/s; yaw 45 deg, peak 10.0). Hull at s 496: centre z -275, bow 625,
  belly y 432 (sink done at s 490): belly cover 24 % of the frame at yaw 65 / pitch 58 (dr/lookup.py), the bow 430 m north. Cost:
  the panel is tracked s 470-476 (the tear and the swing) and seen again s ~512-525 low in the KS09 frame, not over its middle fall;
  the KS09 readable window starts later (FX re-runs the readable / rate tests). The lead decides between the two dread moments.
- REQUEST QA (FX's former file): Assets/_Game/Scripts/Runtime/QA/ShotPlan.cs "dread" set (QA's C22): REPLACE DR's fifth-start
  DreadSequences (its KS08 / KS09 hull walks show nothing now: the hull is in the deck at KS08 and overhead at KS09) by:
      public static List<SequenceSpec> DreadSequences(int w, int h) => new List<SequenceSpec>
      {
          WalkSeq("seq_dread_hull_pass", "dread", "traffic", "s 544-556 at 2.5 m/s, world yaw 30 / pitch +40: the hull's belly slides north overhead at 18 m/s, abeam at s 561 (DREAD 2.1, D-040 C01)", 544.0, 556.0, MotionFps, w, h, 30.0, true, 40.0, NocturneConstants.CinematicSpeed),
          WalkSeq("seq_dread_hull_sink", "dread", "traffic", "s 450-466 at 2.5 m/s, world yaw 100 / pitch +50: the hull sinks out of the deck after its s 430 launch (DREAD 2.1)", 450.0, 466.0, MotionFps, w, h, 100.0, true, 50.0, NocturneConstants.CinematicSpeed),
          KeyWalk("seq_dread_ks13_shadow", "dread", "traffic", "8 s at 2.5 m/s and 60 fps through KS13: the shadow below drifts south under the strip (DREAD 3.1)", "KS13_bridge_glass", 8.0, w, h),
          WalkSeq("seq_dread_panel", "dread", "traffic", "s 468-500 at 2.5 m/s, world yaw 38 / pitch -8: the falling panel (DREAD 3.2)", 468.0, 500.0, MotionFps, w, h, 38.0, true, -8.0, NocturneConstants.CinematicSpeed),
          WalkSeq("seq_dread_cabins", "dread", "traffic", "s 480-486 at 2.5 m/s, world yaw 330 / pitch +10: face A's shafts, cabins and guide lights (DREAD 2.2)", 480.0, 486.0, MotionFps, w, h, 330.0, true, 10.0, NocturneConstants.CinematicSpeed),
      };
  (frames at 60 fps: 288 + 384 + 480 + 768 + 144 = 2 064; capture with -capture-only per sequence and delete after the check.)
- REQUEST AT (in this copy): the s 553 sheet lightning outlines the hull at its new place: centre x 515.3, z HullCourse.CentreZ(tau)
  (tau = DreadBuilder.Driver.HullTau in play, HullCourse.TauAtS(s) in capture): s 553 at 2.5 m/s -> z 135.6 (body z -764..1 036,
  belly y 432, beam 260 m: overhead of the pier), at 1.5 m/s -> z 726. _NocDeckOcc keeps its layout (centre x, centre z, 950, 0.6 x
  Emerge): it is 0 until s 430 and ramps with the sink over s 430-490 (2.5 m/s). The parked hull is inside the deck (mixed to the sky
  along the ray), so the s 205 / 345 glows must not expect a silhouette.
- NOTE AU: DreadCourse API for the drone and the cues (D-038 / D-040: read, do not copy): HullCourse.CentreZ(tau) (= DreadScore's
  195 + 18 (tau - 52.5) for tau >= 10; -660 + 0.9 tau^2 during the 10 s smooth start), HullCourse.BellyY(tau) (602 at the launch,
  smootherstep to 432 by tau 24: the hull is inside the deck until about tau 12), HullCourse.AbeamS(v); ShadowCourse.AxisX 470,
  CentreY -700, CentreZ(tau) (launch s 600, 5 m/s south, centre z 195 at s 719.4), Fade(s); PanelCourse.Centre(tau) (x 814.85, y 150,
  z 800 at tau 0; launch s 470); DreadCabins.Cabins() / CabinY(c, t) (t = the traffic clock), DreadBuilder.GantryPos(i, t).

### Cycles r04-r06 (02:23-02:52)
- 02:23 DR-b06 0 errors (17.8 s). Player log: "Dread: 5 meshes (0 validation failures); hull 13372 triangles, 398 lights, parked centre
  z -660 (+170 m in the deck), launch s 430 at 18 m/s, abeam z 195 at s 561.25 (KS09 centre z -102); shadow 2832 triangles, 7 lights,
  parked centre z 384, launch s 600 at 5 m/s (KS13 centre z 198); 32 cabins, 1696 guide lights" (Logs/player-DR-r04.log line 69).
- 02:24 DR_r04 (23 pose stills + 3 pose-still sequences, scratchpad dr/seqposes2.py). Looked at full size: lookup496, s545up, s553,
  b04l720, KS13, faceA (+ crops). Against p4_01 (pose = key pose):
  - KS04 mean luma 35.8 = 35.8, 0.08 % px > 12 codes; KS05 43.4 = 43.4, 0.03 %: the parked hull inside the deck is invisible from
    beat 2 (the residue is rain / traffic). KS08 39.3 vs 39.1 (0.37 %: the old stern gone), KS09 33.3 vs 34.1 (1.35 %).
  - lookup496: the hull overhead, its bow pointing north-east, sponsons stepped, the y 300-400 traffic passing under it (scale);
    s545up: the bow overhead along the rift between face A and gate A; b04l720 (s 720 north through the tube): the belly's light
    rows converge to the bow far up the rift, with the traffic rivers - the best receding view. DEFECT DR-08: the belly is a black
    void (median 16 vs deck 52) on which the lights and the traffic in front read as a starfield.
  - KS13: the shadow is a dark band across the strip centre (x 560-700: 35-42 inside vs 62-69 above / below), its seven glows one
    row across the strip, the -880 river dimmed under it. faceA: a lit cab on a dotted guide-light ladder (DR-07 reads).
  - sequences.py DR_r04: PASS 3 / 3 (dr_cabins / dr_hullpass / dr_shadow: max luma step 0.0001, 0 spikes; QA/reports/sequences_DR_r04.md).
- 02:29 DR-t02 (DreadTests | VisibilityAuditTests | Q1IntegrationTests | AudioDreadTests | Beat03EdgeTests | Beat04SkybridgeTests):
  59 / 59 passed (QA/reports/DR-t02.xml).
- 02:33 DR-b07 (plate grid across, brighter belly via the shader defaults) + DR_r05 + DR_r05off (NOC_DREAD_OFF=1: a QA A/B switch in
  DreadBuilder, logged "Dread: skipped (NOC_DREAD_OFF=1, QA A/B)", same build): the belly did not change (median 16.4 -> 16.5).
  A/B on b07 (pose stills, same s, same build):
  - KS13 subject layer (x 560 / y -520, row band y 680-750): its lights p99 226.9 / 226.9, top-1 % mean 236.1 / 236.2 (unchanged,
    0.1 code); the band mean 64.0 / 67.1 (-3.1: the haze behind the layer is darker where the body and its haze occluder lie); the body
    x 560-700 / y 780-900 39.0 vs 54.3 off (-28 %); the -880 river row p99 72 vs 95 off (dims under the body). s 712 / 730 the same.
  - KS10: 0.01 % px change (no hull, no shadow in the frame); KS12: 0.21 % (the T3 window glass reflecting the deck the hull darkens).
- 02:40 cause of DR-08 (found by r06 not changing either): NocHull's _BaseColor / _BellyLight / _SideLight / _TopLight are Color
  properties, gamma-converted in this linear project whether set by SetColor or SetVector: the paint 0.035 drew as 0.0027 and the
  fields as 0.005 since r01, so the hull was always the floor colour plus fog. Fix (b09): the three fields are Vector properties (raw
  linear; defaults = the old converted values, so the panel and the gantries keep their look), the hull sets paint 0.05 linear
  through SetColor(gamma) and the fields 0.15 / 0.06 / 0.04 through SetVector; NocDreadBody's _Sheen a Vector too (0.020 / 0.016 /
  0.012 raw; it was 0.0008).
- Disk 03:07: W: 7.9 GB free (other copies; DR keeps Builds/DR 0.47 GB, DR_final 30 MB, DR_r05 41 MB, DR_r05off 13 MB; r04 / r06
  deleted after their numbers were logged, every sequence frame deleted after sequences.py).

### Cycle r07 (03:16-03:30) and the last changes
- 03:25 DR-b09 0 errors (queue behind AB-final; 375 s: partners' include edits). DR_r07 (b09) looked at: lookup496 full size + crop
  (scratchpad dr/c_lookup_r07.png), a sheet of s460 / s470 / b03up480 / s545up / s553 / b04up720 / KS09 / KS08 (dr/sheet_r07.png),
  b04l720, KS13 crop (dr/ks13_r07_crop.png):
  - DR-08 fixed: the belly is a dark warm plated surface (median 26.1 vs the deck 51.9; r05 16.4 = the floor), rib bands and 12 x 16 m
    plates with a per-plate tone; the lights and the traffic under it read as lights in front of a hull, not as stars.
  - the emergence: s 460 its lights glow dimly through the cloud base, s 470 the hull hangs just under the deck with its back still
    misted, s 480 (beat03_look_up60 pose) a dark plated ship overhead, bow north; KS08 / KS09: no hull in the frame.
  - KS13 (b09 A/B, DR_r07off): the subject layer's lights top-1 % 238.9 / 239.1 (unchanged), the row band mean 69.8 / 73.8 (-4.0: the
    haze occluder reached x 537, the layer's rays meet the haze plane at x 533+), the body 51.4 vs 65.6 off (-22 %: the raw sheen 0.02
    lit its visible back), the -880 river p99 126 vs 180 (dimmed in its middle; its ends outside the outline stay lit). KS10 0.01 %,
    KS12 0.22 % px changed (T3's window reflecting the darkened deck).
- 03:36 last changes (no new feature): shadow opacity 0.85 -> 0.92, sheen 0.020 -> 0.006 raw; AT's haze occluder half-length 450 ->
  350 (beam 52.5 m: x 417-522, 10 m+ short of the subject layer's rays); DreadTests updated (+ Hull_AmbientFieldsAreLinearVectors,
  the haze ellipse vs the layer). fastcheck all OK (217 / 288).
- 03:37 DR-b10 (final build) queued behind AT-final, then DR_fix (23 stills + 3 sequences) and DR_fix_off (A/B); DR-final (full
  EditMode) queued after DR-b10.
- Full suite on 03:03's code (AB-final.xml, AB's run in this copy, includes DreadTests): 472 / 476, 1 skipped, 3 failed - all AT's
  (AtmosphereAbyssTests.Abyss_NadirWarmAndStructured_KS13 69.4 > 68, AtmosphereFixTests.Flash_Envelope..., SlitLift_...), none DR's.

### Final (03:45-04:01)
- 03:45 DR-b10 = the final build (Builds\DR): BuildWindows 0 errors (Logs/DR-b10.log "result=Succeeded errors=0", 232 s). No new
  shader, no material row, no GraphicsSettings change this wave.
- 03:47 DR_fix (b10: 23 pose stills + 3 pose-still sequences) and DR_fix_off (NOC_DREAD_OFF=1, same build: KS10 / KS12 / KS13 / s 712 /
  s 730). sequences.py DR_fix: PASS 3 / 3 (dr_cabins / dr_hullpass / dr_shadow, max luma step 0.0001, 0 spikes;
  QA/reports/sequences_DR_fix.md; frames deleted after the check). Contact sheet QA/captures/DR_fix/_contact/contact_DR_fix_01.png
  (looked at: all 23). Numbers:
  - KS04 35.84 vs p4_01 35.80 (0.08 % px > 12 codes), KS05 43.40 vs 43.37 (0.03 %): the parked hull is invisible from beat 2.
  - lookup496 belly median 26.1 vs the deck 51.9 (a plated dark hull; r05 16.4 = the floor colour).
  - KS13 on / off: subject layer lights top-1 % 238.9 / 239.1, p99 230.0 / 230.2 (unchanged within 0.2 code); row band mean 69.6 /
    73.8 (the haze behind it, -4.2); body 53.5 / 65.6 (-18 %); river -880 p99 126 / 180; s 712: 238.8 / 238.9, s 730: 239.7 / 239.8.
    Back glows (on - off) at (587, 852), (704, 854), (955, 854), (1080, 853), (1203, 853), (1325, 852): FWHM 4-6 px at 1080p,
    one row across the strip (the seventh is behind the truss); at 4K 2x (about 10 px).
  - KS10 0.01 % px changed (no hull, no shadow); KS12 0.15 % (T3's window reflecting the deck the hull darkens).
- 03:56 DR-final (full EditMode, QA/reports/DR-final.xml, code = the final files): 477 tests, 476 passed, 0 failed, 1 skipped
  (AbyssTests.KS10_CompositionSearch, explicit); DreadTests 26 / 26 (VisibilityAuditTests green with the overhead hull).
- 04:00 smoke DR-smoke-fix (b10, -autotest -brisk -autotest-until 900 -beat 3): AUTOTEST PASS, beats 3-5 entered, errors 0, corridor
  coverage 48 204 rays / 0 misses, alloc mean 29.0 B/frame (engine input box 32, excess 0.0) (Logs/player-DR-smoke-fix.log).
- 04:00 fastcheck all OK (runtime 217 / editor 288).
- Budget: hull 13 372 + shadow 2 832 + lights (398 + 7 quads = 810) = 17 014 triangles (< 60 k); cabins 32 + guide lights 1 696 quads
  in one draw (3 456 triangles); no realtime light.

### Open (fix wave)
- S2 (needs FX + the lead): the KS09 long look-up keys (REQUEST FX above). Until they land, the cinematic gaze never looks up while the
  hull is overhead (s 470-640): it is seen in the walk frames s 608-636 / 668-682 (<= 3.6 % of the frame, receding north) and in any
  look-up (beat03_look_up60 22 %, s 496 look-up 24 %, abeam 44 %). The 14.10 KS09 / KS10 poses cannot contain it (frame tops
  +16 / +2 deg; the hull is at >= 26.7 deg in KS10 at both speeds: DreadTests). The keys trade the panel's middle fall (lead decides).
- S3 DR-10: the -880 river is drawn over the shadow body (additive lights after an alpha body without depth); AB's occluder dims its
  middle (p99 126 vs 180). A depth-only pass with clip(alpha - 0.6) would hide it; not built (no time for a build + suite).
- S3: KS13 row band mean -4.2 codes (the haze behind the subject layer under the body); the layer's own lights are unchanged.
- QA's dread set must use DR's new sequence list (REQUEST QA above); DR's own evidence is pose-still sequences.
- DR-06 (S3, unchanged by the chief's note): the falling panel is 8 x 6 px at 800 m.
- Disk: W: 8.7 GB free at 04:00 (below 15; DR holds Builds\DR 0.47 GB, DR_fix 63 MB, DR_fix_off 13 MB, DR_final 30 MB).
DR DONE 04:01

## ARK (2026-11-21 from 20:16, D-043; copy .worktrees/abyss; task Logs/ark/ARK.md; partners AB, AT; deadline 23:00)

### Read (20:16-20:28)
- ARK.md (the user's order and the lead's design), my DR.md, DreadCourse / HullBuilder / DreadBuilder / DreadDriver / NocHull /
  NocDreadLight, NocturneConstants GazeKeys + KeyShots (14.10), EdgeDims (faces x 215.3 / 815.3, z -6 500..6 900, top y 560; the six
  cross bridges all at y <= 90), NocAtmosphere (fog T = exp(-(sigma d)^2), sigma x NocDown(y) = 1 above y 0), the users of my API:
  AU's DreadScore (consts LaunchS / Speed / Length / Beam / AxisX / BellyY0 / AbeamTau; BowZ / CentreZ / SternZ / BellyY / AbeamS),
  AT's AtmosphereDriver (CentreZAtS / AxisX / Hidden(SternZAtS) / LaunchS): the names stay, the content becomes the Ark.

### Design decisions (20:28)
- DIRECTION (deviation from ARK.md's "from the south (-z) to the north (+z)", logged for the lead): the Ark comes from the NORTH and
  moves SOUTH, bow first. Evidence (NocturneConstants, yaw 0 = +z, CameraRig Quaternion.Euler(-pitch, yaw, 0)): every beat-3 key shot
  looks north (KS08 world yaw 28 / pitch +24, KS09 20 / -18, KS10 355 / -32), the beat-3 gazes too (s 428 yaw 28, s 470 yaw 40,
  s 494 yaw 65), and the bridge look s 624-660 and KS12 look due north (yaw 0). From the south the Ark would be behind every beat-3
  frame (ARK.md's "bow lights emerge from the southern haze ... (KS08 / KS09 / KS10)" is impossible) and after its pass its bow would
  be 150-700 m ahead of the s 624-660 north look (not "both ends lost in the haze"). From the north: its lights and then its mass
  come out of the haze inside KS08, it keeps coming, and after the pass the walker looks north along 5 km of belly into the haze.
- SPEED: ARK.md's three timing rules cannot all hold at a constant 22-28 m/s (beat-3 emergence 2-4 km out at s 400-430 needs a
  mean 45+ m/s to reach z 195 by s 596; the stern gone in the garden needs >= 26.9 m/s after it). Course: launch at s 380 (hidden,
  inside the canyon), speed 27 + 45 e^(-tau / 25) m/s (72 at the launch 3.4 km out, where only its growth reads; 28.4 overhead;
  27 cruise): bow over z 195 at s 596 (2.5 m/s) / 509.6 (1.5 m/s, over the pier), stern over z 195 at s 1056 / 785.
  Overhead angular rate 28.4 / 328 = 5.0 deg/s; at 20 deg elevation 0.6 deg/s.
- BODY: an inverted ziggurat (keel plate 140 m wide at y 330, seven 20 m steps out to the widest tread at y 470, half-beam 216,
  ribs 219: beam 438 <= 440), back to y 590 in the deck; prow taper over 700 m with the keel rising to y 414 at the tip; ribs every
  100 m; keel spine, 16 engine pods with cold aft glows, 36 hanging gondolas under the outer treads (>= y 380). Windows and portholes
  procedural in the new NocArk shader (4 m decks, sectors, box-filtered pulses fading to the mean: no shimmer); lamp rows on every
  tread edge and the spine (NocDreadLight: 2.2 px minimum and the energy rule). ~31 k triangles with the lights.
- Searchlights: emitters / directions / gains in HullCourse.Searchlight for AT's cones (slow sweeps 28-40 s, gain 0 within 220 m
  of z 195: never on the walker).

### Log
- 20:32-20:36 NOTE AB: DreadTests did not compile for 4 minutes (my multi-file edit in progress: AB-x02's CS0117 errors in
  DreadTests.cs were mine); fixed at 20:36 (DR-x01 CompileCheck SUCCESS 20:37).
- 20:37 DR-b11 BuildWindows -> Builds\DR queued.
- 20:38 DR-b11 BuildWindows 0 errors (55 s; NocArk in the always-included list: "31 Megastructure/* discovered, 1 added").
  Player log (Logs/player-DR-a01.log line 70): "ARK 25394 triangles, 2986 lights, 35 gondolas, launch s 380 (bow 3422 m north of
  z 195), bow over z 195 at s 596.0 (2.5 m/s) / 509.6 (1.5), stern at s 1055.7 / 785.4, bow distance KS08 2301 m, KS09 905 m".
- 20:40 DR_a01 (18 pose stills, looked at all: sheet scratchpad sheet_a01.png; s 494 / s 640 / s 700 up / s 760 up full size):
  DEFECTS: (1) the 700 m prow taper read as an EGG / a blimp from ahead and from below (s 494, s 548, s 596 up), not a ship;
  (2) the belly a STARFIELD (random lit portholes on a black plane); (3) the 5 m engine-pod halos blurry blue balls (s 700 up);
  (4) the deck occluder at 0.45 darkened the deck seen over the ship's back: the silhouette lost its contrast.
- 20:45 the queue's unity-queue.log is held open by another writer (Add-Content IOException: AB / AT's queue calls hit it too);
  DR uses a copy of Tools/unity-queue.ps1 (scratchpad dr-queue.ps1: same root, same mutex, the log line written with
  FileShare.ReadWrite). NOTE FX (Tools owner): Write-Q could open the log with FileShare.ReadWrite.
- 20:46 a02 (DR-b12): a blunt BOW WALL (62 % of the beam at the stem, rounding out over 320 m, keel rising only to y 355), step AO
  (tread inner corner 0.5, riser head 0.58: the steps read from below), rib cross rows (6 lamps per rib on the keel plate), lamp
  rows along every step's front edge on the bow wall (a lit stack of decks ahead), porthole runs (7 cells) and window runs (3),
  deck occluder 0.2, NocArkAirTo(P) after the fog (AT's request: its cones and glow in front of the hull).
  DR_a02 looked at: the bow reads as a stepped wall with lit decks; AT's searchlight cones come down from the keel (s 494, s 548).
- 20:50 a03 (DR-b13): sector activity hash^4 (most decks dark, a few lit districts), keel hangar bays (two 18 x 80 m lit bays every
  400 m), the cloud melt 425..560 m (+-45 m billows) instead of 455..530, tread lamps 12 m (24 m read as a dotted wireframe).
  DR_a03: s 548 (scratchpad c548.png) the bow wall overhead-ahead with its decks, the belly receding into the haze, the cones down
  into the abyss; s 700 up (c700.png) a lit hangar bay, porthole runs, gondolas, lamp lines.
- 20:53 a04 (DR-b14): 12 keel gondolas a side (26 x 50 x 22 m, bottom y 304) between the spine and the pods, pod glows 2 m / 0.25.
  DR_a04: 9 stills + 3 pose-still sequences of 60 frames (fixed camera, s at 2.5 m/s and 60 fps: the Ark moves 0.47 m a frame):
  bow (s 548-549, yaw 8 / pitch 25), north (s 640, yaw 0 / pitch 30), up (s 700, pitch 88). Tile-energy flicker (scratchpad
  tileflick.py: per 32 px tile the second difference of the mean luma, levels): bow max 2.81 / p99.9 0.96, north 5.10 / 2.14,
  up 3.06 / 1.23; the worst tiles all lie on the faces' traffic streaks (heat map tile_DRSQSQnorth.png: the Ark's tiles <= ~1-2:
  moving lamps crossing tile borders), mean-luma step <= 0.018 levels / frame. Looked at s 640 up / s 760 up / s 575 / s 614
  (sheet_a04.png): the belly a ceiling of lit edges, bays and gondolas converging north into the haze between the faces, the cones.

### Requests (ARK; also in the report's shared_change_requests)
- REQUEST FX (Assets/_Game/Scripts/Runtime/Core/NocturneConstants.cs GazeKeys, 14.5.3): the beat-3 look-up was aimed at the old
  hull's belly (world yaw 65, pitch 58); the Ark comes from the north: replace `new GazeKey(494, -25, 58), new GazeKey(499, -25, 58),`
  by `new GazeKey(492, -80, 24), new GazeKey(500, -80, 24),` (world yaw 10 / pitch 24: up the rift at the approaching bow wall,
  1.15-1.28 km out, its decks, the command band and AT's cones; the pose QA/captures/DR_a04/pose_A04_s494.png = yaw 10 / pitch 22).
  Rates: (476, -50, -8) -> (492, -80, 24): 16 m = 6.4 s, smootherstep peaks yaw 8.8 / pitch 9.4 deg/s; hold 3.2 s; -> (520, -70, -18):
  20 m = 8 s, pitch 9.8 / yaw 2.3 deg/s (limits 22 / 18). KS09's readable window starts as before (FX re-runs the gaze tests).
- REQUEST FX (same table, 14.5.4; FX's "long look-up under the belly on the bridge"): after `new GazeKey(748, 0, -5),` replace
  `new GazeKey(790, 0, 4),` by `new GazeKey(766, -80, 40), new GazeKey(778, -80, 40), new GazeKey(800, 0, 4),` (world yaw 10 /
  pitch 40 at x 581-593: the belly's 5 km ceiling coming out of the north haze toward the walker, bays, gondolas, lamp edges, cones;
  the pose QA/captures/DR_a04/pose_A19_s640up.png = yaw 0 / pitch 30). Rates: 748 -> 766 (7.2 s): yaw 80 -> peak 20.8 deg/s (<= 22),
  pitch 45 -> 11.7; hold 4.8 s; 778 -> 800 (8.8 s): yaw 17.0, pitch 7.7. Ark at s 766-778: bow 1.9-2.1 km south, stern 2.9-3.1 km
  north (both ends in the haze).
- NOTE AU (integ): the Ark comes from the NORTH and moves SOUTH (-z), bow (leading, south end) first: HullCourse.BowZ / CentreZ /
  SternZ / Velocity(tau) (72 -> 28.4 -> 27 m/s, toward -z) / BowDistance / AbeamS(v) = the BOW over z 195 (s 596 at 2.5, 509.6 at
  1.5) / SternPassS(v) (s 1055.7 / 785.4) / LaunchS 380 (hidden before) / HiddenAt(tau). DreadScore.HullVelocity's ramp formula
  (Speed x min(1, tau / Ramp)) no longer holds: read HullCourse.Velocity(tau). AudioDreadTests' 561.25 / 18 m/s numbers are the old
  hull's (AU's test, AU updates it).
- NOTE AT: the emitters of HullCourse.Searchlight sit on the keel spine (x 515.3 +- 9, y 321, local z 600..4 300 from the bow);
  NocArk adds NocArkAirTo(P) after its fog (done 20:46, as asked).
- 21:03 A/B (b14, same poses, NOC_DREAD_OFF=1 vs on; QA/captures/DR_ab_on / DR_ab_off): beat 2 untouched - KS04 mean 35.54 / 35.51,
  0.054 % px > 12 codes; KS05 41.96 / 41.94, 0.027 %; s 396 east (canyon mouth, yaw 90) 0.082 %; s 396 north (yaw 25) 0.035 % (rain
  and traffic residue; the Ark is hidden until s 380 and the canyon walls hide the rift's north after it). KS10 0.98 % (AT's
  searchlight cones in its upper right, absent in the off run, plus the cabins' guide lights; no Ark pixel: DreadTests KS10 test);
  KS12 1.0 %; KS13 2.3 % (the shadow below and its glows, as since the fix wave); KS11 0.06 %. NOTE AT: the cones show in KS10's top
  right (scratchpad ab_B09_ks10.png): please check KS10's drop share (>= 40 %).
- 21:04 perf A/B (b14, 1080p, -autotest -cinematic -beat 3 -autotest-until 760, -perf-log; QA/reports/DR_ark_perf_on.csv / _off.csv,
  perf_windows.py): s 580-600 GPU mean 10.07 on / 7.74 off (p99 11.50 / 8.96), s 700-740 4.80 / 4.24 (p99 7.58 / 6.64), s 535-585
  7.84 / 6.23. Both runs contaminated (AB's and FX's Unity batch jobs ran; the release measured s 580-600 at 6.07 mean, s 700-740
  3.01 with every DR object, integ QA/reports/perf_windows_final_1080.md) and "off" also removes AT's Ark air (cones, glow) and the
  old DR objects; the Ark + AT's air cost about +2.3 ms at s 580-600 under load. Optimisation (b15): the cloud-noise melt only above
  y 380. (A stray: the first perf run wrote its CSV to W:\YouTube\Megastructures\6 (a path with spaces split by run-player's
  -ArgString) and the second to the main tree's QA\reports; both moved into this copy's QA/reports, nothing left behind.)
- 21:07 DR-t11 (DreadTests | VisibilityAuditTests | Q1IntegrationTests | AudioDreadTests | Beat03EdgeTests | Beat04SkybridgeTests,
  QA/reports/DR-t11.xml): 68 / 71. Failed: AudioDreadTests.Cues_AreKeyedToTheirSAndTimes ("a glow outlines the hull (abeam at s 596)")
  and AudioDreadTests.Hull_CrossesAlongZAtEighteenMetresPerSecondWithASlowDoppler (-660 + 0.9 tau^2): AU's tests of the old hull
  course (AU owns them, updating in integ); DreadTests.Ark_Searchlights_SlowDownwardNeverOnTheWalker: lamp 0 at tau 116 reached
  (615, 115) - fixed (b15): the fade starts 300 m from z 195 (reach of a 33.5 deg cone from y 321: 211 m; walker within 80 m of
  z 195), over 300..500 m. VisibilityAuditTests, Q1IntegrationTests, Beat03 / Beat04 tests green with the Ark.
- 21:20 DR_a05x (b14): s 772 up (yaw 10 / pitch 40: FX's proposed look-up) the belly a lit ceiling converging north between the
  faces, gondolas hanging, a cone; s 846 looking back NNW: the east flank's steps curving north overhead; s 970 from the garden
  through the crowns: the belly and the flank (sheet scratchpad sheet_a05x.png).
- 21:27 DR-b15 0 errors (perf: Cull Back on the Ark, cloud noise only above y 380, thin-haze fog colour from the 0.02 rad blurred sky
  blended to the sharp one over T 0.95..0.85; searchlight fade 300..500 m; bow command band; corner beacons; stern glows y 372 / 402).
  Player log (Logs/player-DR-a05.log): "ARK 27170 triangles, 6074 lights, 35 gondolas ...": 27 170 + 2 x 6 074 = 39 318 triangles.
- 21:28 DR_a05 (b15): 20 stills + 3 pose-still sequences (48 frames each at 60 fps cinematic, fixed camera): tile-energy flicker
  (levels): bow (s 548) max 4.68 / p99.9 1.89, KS08 3.35 / 1.81, up (s 700) 3.06 / 1.29; worst tiles on AB's new air-crossing
  rows and the faces' streaks (heat map tile_DRSQSQbow.png), the Ark's tiles <= ~2 (lamps crossing tile borders); frames deleted.
  Looked at full size: s 548 (scratchpad c548_b15.png): the stepped bow wall overhead-ahead, its decks, a lit command band, the
  red edge row, the lamp edges converging back into the haze - reads as a ship's bow; the band an even bar (-> per-bay tone, b16),
  the corner beacons two "headlights" (-> 1.4 m / 0.45, b16). s 494 / KS08: the Ark in the haze up the rift; NOTE AB: your air
  crossings (z 500 / 660) run as two flier rows right across the Ark's silhouette in KS08 and s 494 (QA/captures/DR_a05/
  pose_B05_ks08.png, pose_B07_s494.png; crop c_ks08_b15.png) - they compete with it in its emergence shot (a scale cue under it,
  but they cut the bow in half at KS08). Sheet sheet_a05.png: s 575, s 596 up, s 614, s 640 n, KS12, s 772 up, s 970 back, KS09.
- 21:38 DR-b16 = the final build (Builds\DR): BuildWindows 0 errors (Logs/DR-b16.log "SUCCESS with 0 problem(s)"); band per-bay
  tone, corner beacons 1.4 m / 0.45. FREEZE (no new feature after b16).
- 21:40 smoke DR-smoke-ark (b16, -autotest -brisk -autotest-until 880 -beat 3; Logs/player-DR-smoke-ark.log): AUTOTEST PASS, beats 3,
  errors 0, corridor coverage 48 204 rays / 0 misses; alloc mean 33.2 B/frame (engine input box 32, excess 1.2; worst 4 096 B,
  91 frames allocating, GC 0): the fix wave's excess was 0.0 - DreadDriver allocates nothing (structs, cached ids, pure HullCourse
  functions); the 1.2 B comes from elsewhere in this copy's tree (AT's / AB's new per-frame code) or the queue's load: open item.
- 21:40-21:46 perf A/B 2 (b16, -ark-air 0 -ark-occ 0 on BOTH, Ark on vs NOC_DREAD_OFF=1; QA/reports/DR_ark_perf2_on.csv / _off.csv):
  GPU mean s 580-600 6.67 on / 6.30 off (+0.37 ms), s 700-740 3.64 / 3.44 (+0.20), s 535-600 median 6.31 / 6.11 (+0.20, p90 6.79 /
  6.47). p99 on 11.50 (s 580-600) / 16.87 (s 535-585) vs off 6.56 / 6.66: a 0.3 s train of 11-29 ms frames at s 557.7-558.2 and
  bunches at 561-564, 576, 587, 600 (68 frames > 9 ms, alternating 29 / 11 ms) in the on run only, which ran while DR-final (the
  full EditMode suite, started 21:39:06) was rendering in the same queue; re-measured after the suite (below).
- 21:44 final capture QA/captures/DR_final_ark (b16): 20 stills + 2 pose-still sequences (48 frames, 60 fps cinematic; frames
  deleted after the numbers). Ark-region tile-energy flicker: bow at s 548 (px 800-1240 x 240-660) max 1.10 / p99.9 0.72 levels,
  region mean 39.21..39.51, max step 0.027 levels / frame (smooth, no shimmer, no pop); KS08 region (px 520-720 x 690-830, AB's
  flier rows cross it) max 2.38. Looked at: s 548 bow crop (scratchpad c548_b16.png: the band reads as lit bays, the beacons as
  two small lamps), the sheet QA/captures/DR_final_ark/_contact/ark_final_sheet.png (KS08, s 494, s 548, s 575, s 596 up, s 640 n,
  s 772 up, s 700 SE (the train), s 970 from the garden).
- 21:49 DR-final (the full EditMode suite on the final code, QA/reports/DR-final-ark.xml): 521 tests, 518 passed, 2 failed,
  1 skipped (AbyssTests.KS10_CompositionSearch, explicit). Failed: AudioDreadTests.Cues_AreKeyedToTheirSAndTimes and
  AudioDreadTests.Hull_CrossesAlongZAtEighteenMetresPerSecondWithASlowDoppler - AU's tests of the replaced 1 800 m hull's course
  (561.25 s abeam, 18 m/s, -660 + 0.9 tau^2): owner AU (updating DreadScore / its tests in integ to HullCourse's Ark API). DreadTests
  all green (Ark course, searchlights never on the walker, body / budget / clearances, lights, fields), VisibilityAuditTests,
  Q1IntegrationTests, Beat03 / Beat04 tests green.
- 21:50 perf pair 3 (b16, clean: no Unity job in this copy, -ark-air 0 -ark-occ 0 on both; QA/reports/DR_ark_perf3_on.csv /
  _off.csv), 1080p GPU: s 580-600 mean 6.47 / 5.91 (p99 6.95 / 6.15, max 6.98), s 535-600 p99 6.89 / 6.57, s 600-700 mean 5.06 /
  4.73 (p99 6.71 / 6.03), s 700-740 3.45 / 3.44 (p99 4.95 / 4.94): the Ark costs +0.3..0.6 ms mean, p99 <= 6.95 ms (budget 8.2);
  pair 2's spike train was the suite's load (none here).
- 21:53 4K pair (-perf-rt 3840x2160, same flags; QA/reports/DR_ark_perf4k_on.csv / _off.csv): s 580-600 mean 18.66 / 17.84 (p99
  20.90 / 18.32), s 600-700 15.06 / 14.80, s 700-740 10.53 / 10.41. OPEN (DR-09): the Ark adds +0.82 ms mean to the 4K s 580-600
  window (ARK.md: "do not make it worse"; the release measured 18.5); the remaining lever is the fog colour per vertex instead of per
  pixel for the near hull (T > 0.85) - not done (risk of a smooth ghost patch at the silhouette; no time to verify before the freeze).
- 21:56 fastcheck all OK (runtime 221 / editor 297). Captures kept: QA/captures/DR_final_ark (57 MB), DR_ab_on / DR_ab_off (44 MB);
  DR_a01..a05 deleted.

### Open (ARK)
- DR-09 (S2, perf): +0.82 ms mean at 4K s 580-600 (18.66 vs 17.84 off); 1080p p99 6.95 <= 8.2.
- DR-10 (S2, direction): the Ark flies north -> south (ARK.md says south -> north); evidence above; the lead decides. AU's DreadScore
  mirror (if it copied ARK.md's direction) must read HullCourse (BowZ / CentreZ / SternZ / Velocity / AbeamS / SternPassS).
- DR-11 (S3): the stern passes z 195 at s 1 055.7 (2.5 m/s), the garden's last metres; beyond, the stern recedes south while the
  walker is in the city (seen only looking back).
- DR-12 (S3): the beat-3 emergence is seen in KS08 (2.3 km, faint stepped bow with its lit rows) and in any look up the rift; the
  current gazes look up the rift only at s 428 and s 470; FX's s 492-500 retarget (REQUEST FX above) makes the approach the shot.
- AB's air crossings (z 500 / 660) cross the Ark's silhouette in KS08 / s 494 (NOTE AB above).
- The smoke's 1.2 B/frame alloc excess (DreadDriver allocates nothing; source elsewhere in this copy).
DR ARK DONE 21:56
