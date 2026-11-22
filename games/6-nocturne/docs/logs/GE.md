# GE log - Look v2: geometry relief (TEXTURE_PLAN 5 / 8.3) and the Q1 gate structure fixes (D-029)

Owner: GE. Build slot `Builds/lookdev_GE` (`Tools/lookdev.ps1 -Slot GE`). Critique: `QA/critique-GE.md`.

## Resume rule
Find the last step marked GREEN below and the last commit with "GE:" in the message; continue with the next open item.

## Ownership (recorded before editing; TEXTURE_PLAN 8.3 / 8.4 as amended by the workflow prompt and D-028 / D-029)
Mine (exclusive):
- New `Assets/_Game/Beats/Relief/**` (ReliefBuilder, ReliefKit, CrownBuilder, GulfRibBuilder, SignBaseRelief, and the gate
  structures of job B) and `Scripts/Tests/EditMode/ReliefTests.cs`.
- `Beats/Beat07_Heart/SpireBuilder.cs` (8.4: GE; carries FA `SkinOverride(0)` and PA's strip / base colours at 109-110).
- `Beats/Beat03_Edge/EdgeDims.cs`, `EdgeMesh.cs`, `GulfFace.cs`, `GulfPortal.cs`, `CrossBridge.cs`, `TrafficLayerLines.cs`
  (not `Beat03_Edge.cs` = SU, not `TonguePier.cs` = PA).
- `Beats/Beat04_Skybridge/*.cs` except PA's `SkybridgeLamps.cs`, `SkybridgeMaterials.cs`: `Beat04_Skybridge.cs`, `BridgeGardenKit.cs`,
  `SkybridgeDeck.cs`, `SkybridgeDims.cs`, `SkybridgeFace.cs`, `SkybridgeGate.cs`, `SkybridgeRails.cs`, `SkybridgeTruss.cs`,
  `SkybridgeTubes.cs`.
- `Beats/Beat02_Boulevard/` files not owned by FA / PA / SU: `BoulevardLayout.cs`, `StreetLevel.cs`, `TramTerminus.cs`
  (not FacadeKit / StreetFacade = FA, Beat02_Boulevard = SU, BladeSign / MedianPromenade = PA).
- `Beats/Beat07_Heart/` plaza and ring files not owned by PA / SU / KO: `HeartFloorBuilder.cs`, `HeartLayout.cs`,
  `Beat07_Heart.cs` (RingTowersBuilder.cs is "not edited" in 8.4: left alone).
- `Scripts/Runtime/City/NocTraffic.cs`: lane list and lane ends only (lead decision 1).
- `Scripts/Runtime/QA/AutoTestRunner.cs`, `NocVisibility.cs`, `Visibility*.cs`; the test helper `Scripts/Tests/EditMode/VisibilityWorld.cs`.
- Tests: `Scripts/Tests/EditMode/Beats/Q1IntegrationTests.cs`, `VisibilityAuditTests.cs`, `ReliefTests.cs`,
  `Beats/Beat03EdgeTests.cs`, `Beats/Beat04SkybridgeTests.cs`.
- `docs/logs/GE.md`, `QA/critique-GE.md`.
Not mine (REQUEST lines): GardenDims / GardenTree (FO), KoiMath (KO), TonguePier / MedianPromenade / NocturneConstants /
NocStreak (PA), Beat03_Edge.cs / OldQuarterBuilder (SU), FacadeKit / StreetFacade / G4Kit / CityMesher / CityTests (FA),
GardenTerrace / GardenFurniture / Beat06 files (no package: lead).

## Steps
- 09:31 start. Read BUILDER_BRIEF, STATE, DEVELOPMENT D-001..D-029, TEXTURE_PLAN 0, 5, 8, 9, 10, 11; LOOK_V2 1, 2, 8.1, 10;
  MASTER_PROMPT 14.4, 14.5.2-14.5.7; critique-graybox01; visibility.md; Q1.md; G2.md; the beat 3 / 4 / 7 code, NocTraffic.

## Third start (2026-11-15, resume after the second pause; run wf_2c6d782f-763)
- 21:35 resumed. Read STATE (RESUMED / PAUSED AGAIN), D-029 / D-030, this log, critique-graybox01, Q1.md, G2.md (G2-D4),
  TEXTURE_PLAN 5 / 8.3 / 8.4 / 9, and `git diff` of my files. Markers: PA TOKENS READY only (no FA-1 READY: job A waits).
- What the re-applied patch holds of mine (written at 09:31-13:30 on 2026-11-14 by the previous GE session, uncommitted):
  - `AutoTestRunner.cs` (GB-19): `-beat N` / `-progress X` start the walk at CliArgs.StartS (teleport to s0), beats N.. are
    expected once and in order, beats before the start never, crossings / trigger / stop / until limits measured from the start
    ((B - s0) / v + ramp / 2); mode name gets `_from<s0>`; report adds startS / startBeat. Done in code, NOT yet run in a player.
  - `VisibilitySubjects.cs` (GB-01, D-029): KS19 `koiBody` (the body is a partial subject, the head is reported only) and the new
    subject `goal_lamp` (VisRule.Whole, the 0.2 m sphere at (1 157.2, 5.0, 387.9)); self tags for the lamp.
  - `VisibilityAudit.cs`: KS19 body lint (minBodyIn: fewest core samples inside the 3 % margin over phases / poses / aspects >=
    MinPartSamples; head occlusion becomes a note); the pre-reveal koi check (GB-03): KS04-KS16 at their target poses (3 aspects) +
    every beat 2-6 still of ShotPlan.BeatStills, 183 koi samples x 20 wave phases at the koi phase of the pose's s, ray-cast
    against the built meshes, each visible sample classified by the opening it passes (notch / promenade gap / over the arcade /
    other), the spire's silhouette points too; json "preReveal" and a md table; failures owner GE.
  - `VisibilityAuditTests.cs`: koi lint asserts KS19's body; new `KS19SubjectsAreWindowLampAndKoiBody`, `PreRevealKoiIsHidden`.
  - Not done: never run (no GE-t* report); line endings of VisibilityAudit / VisibilitySubjects / VisibilityAuditTests had been
    turned to LF by the editor: restored to CRLF (21:40; diff now 303 / 35 lines instead of 2 163 / 1 705).
- Left (job B): VisibilitySpecWorld balcony slab y 3.4-3.6 (SU's note), run the audit (GE-t01), GB-02 / GB-06 / GB-03 structure,
  GB-05 strip, GB-14, GB-15, GB-16, GB-17, GB-20, GB-19 player check; job A after FA-1 READY.
- 21:44 `GE-t01` (VisibilityAuditTests | Q1IntegrationTests) 10 / 11: Q1 floor / body probes pass, KS19 subjects (window, lamp,
  body) pass, the koi lint passes; PreRevealKoiIsHidden fails as expected: KS04 2 430, KS06 2 271, KS07 1 456, KS08 696, KS11
  1 159, KS14 35, KS15 468 koi sample-phases visible, all through the garden notch; beat stills 2-5 up to 2 348 (notch); the garden
  end / signs start stills (s 1 078 / 1 082) 1 144 / 669 through the notch's open east end (x 914, z 115..236). Spire points through
  the notch: KS15 13, beat 4-5 stills 2-13.
- 21:50 VisibilitySpecWorld: the goal balcony slab at y 3.4-3.6 (OldQuarterBuilder.BalconyTop / BalconyT, SU's note; GB-18) and the
  tenement modelled as the shell around the goal room with the window open (the lamp at x 1 157.2 is behind the window: the spec
  model reported it 0.00 visible "by tenement"); VisibilitySubjects: KS02 catwalk z 96.0-97.5 (AlleyFixtures, D-029 GB-12).
- 22:31 `GE-t02` (+ Beat03EdgeTests, Beat04SkybridgeTests, AutoTest): 29 / 31, 1 skipped (PreRevealKoiIsHidden, Ignore until the
  gate structures land), 1 failed: Beat04SkybridgeTests.Lamps21 expected the v1 intensity 4.0, PA's LOOK_V2 lamps are 3.5
  (SkybridgeLamps.LampIntensityV2): the test now accepts 3.0..4.0. Visibility failures left: KS02 pose / tower flank (lead), KS09 /
  KS10 composition (lead; KS10 built drop 0.324, gate block A), KS13 strip (GB-05, mine). KS02 catwalk and KS19 lamp: gone.
  GREEN step 1 (GB-19 code, GB-01 audit, GB-12 subject, GB-18 spec slab, pre-reveal check); committed below.

## Gate design (GB-02 / GB-03 / GB-06 / GB-14; G2-D4 option (a), refined) - 22:20-23:20
Why not (b) portal masses alone: there is no ground, so a portal for the upper tube at z 195 in the canyon mouth or in the notch
must span between masses anyway. Why not ending lanes at the canyon end: a lane's alpha ramp is e L = 0.04 x 4 815 = 193-200 m
(one shared _Ramp in the StreakTail / StreakHead materials, not mine), so a lane can only end where 200 m of enclosed run follow
its portal; the canyon end has none. Why not a low face-B lintel for the low lanes: FO's crown envelopes (1.10 x the ellipsoids)
reach y 98-125 at the face-B plane (T4, T8, B3) and 135.4 inside the notch (B4); a wall there would hide the KS07 crowns.
Built (Beats/Relief, new IBeatBuilder "Relief"; numbers in GateDims.cs):
1. CanyonBridge: a 6-storey skybridge x 212.25..232.25 (east of the last north blade x 198..212), y 108..132, z 98..292 keyed into
   the slab-6 shafts and face A's flanks; NocFacade facade faces, concrete soffit with 2.4 m fascias, ribs every 8 m, parapets;
   the upper tube's west portal (panel, collar, NeonCyan ring, 16 m tunnel) on its east face. Crowns / lanes: lowest lane y 140,
   parapet top 133.2. KS04: a band at NDC y +0.37..+0.54 across the canyon end (face B below it stays visible: spec y -0.63..-0.19).
   KS05: the bridge hides the skyway-lane sample the spec quotes at NDC y -0.08 (x 401); the lane shows above and below it
   (Part rule): a logged deviation.
2. NotchGate: face B closed above the garden by a lintel x 815.9..839.3 (0.6 m behind face B's plane), soffit y 129 (ribs to
   128.0; crowns <= 124.6 at x <= 839.3), up to 560; west face section mode with face B's id and u; head beam 1.5 m proud; keystone
   x 813.5..818, z 186..204, y 105.5..129.6 with the upper tube's east portal (T4's envelope <= 102.2 under it). One lane tunnel per
   lane through the lintel (lit 4 m mouths, frames).
3. NotchEastBlock: the notch's east end closed from x 914 to the arc R 366 about the spire (koi parts <= R 356; 10 m margin),
   z 115..275, y -360..560 (above the arcade only for z 236..275); facade faces, ledges, 30 m lane tunnels. It is also GB-14's fix
   (the void x 914..plaza disc is filled; the plaza disc runs into the arc face).
4. PromenadeLintel (added after GE-t03, see below): x 873.5..914, z 280..286, y 40..560, between the screen-wall slab and the east
   block's line: the courtyard's north side above the promenade entrance.
Lanes (NocTraffic lane list, my lane edit): (-48, 60) -> 150, (-42, 95) -> 175, (+44, 70) -> 145, (+50, 112) -> 185; the other six
unchanged. All ten enter the lintel at x 815.9, cross the courtyard 4.6 m or more above the crown envelopes, and end in the east
block (alpha 1.00 at face B, 0.96 at the lintel's east face, 0.51 at the block; at the arc face <= 0.04 north, 0 south).
KS04 subject lane_n112 -> lane_n185 (VisibilitySubjects; the spec NDC x range stays the reference).
- 23:02 `GE-t03` 37 / 41: PreRevealKoiIsHidden down from 26 failing poses to 3 (KS15 344, beat05 mid 2, beat05 look_up60 390):
  from the garden the koi showed through the promenade gap (x 874..914 at z 275) over the 38 m arcade (sight lines at y 58-78 over
  z 275) -> PromenadeLintel (B8's envelope reaches z 276.7: lintel from z 280). Lanes test fixed (south lanes end inside the block,
  the arc lies east of x 1 015.9); the floor probe's 92 in-lane steps were the rails (GB-16, below) and the truss seen through the
  glass strip (now counted as strip); the strip test looked up "glassStrip" instead of "glassStrip_back".
Other fixes in this cycle:
- GB-05: SkybridgeDeck: the glass strip x 521.3-541.3, |z - 195| <= 1.2 cut through top and soffit on shared vertex lines, four
  steel walls following the seam profile, a NocGlass pane 3 cm below the top (glassStrip_back / _front); Q1IntegrationTests: the
  strip is the accepted hole (InGlassStrip; PathGuide's y -0.05 collider); Beat04SkybridgeTests.GlassStrip_20x2_4_HoleUnderGlass.
- GB-16: StreetLevel rails embedded (head 8 mm proud, axis at -0.027): Q1's "floor furniture" exception list is empty again.
- GB-17: EdgeDims.BarrierX0 216 -> 214 (first planter x 214.3-216.7; the median barrier ends at 213.8).
- GB-20: TramTerminus east and south faces: plinth, pilasters every 7.25 m, 8 recessed hall windows (0.35 m reveals, steel mullions,
  the lit hall behind), a service door to the tongue plaza, cornice 0.4 m proud on three faces.
- GB-15: EdgeDims.BackX / GulfFace: face B's back north of the screen wall steps out x 850 -> 871.3 -> 892.7 -> 914 at z 750 / 850 /
  950 (planar facades, south-facing jog walls) instead of the smooth blend.
- 23:35 requests carried: DONE FA ReliefKit.PanelWithHole uSign (CanyonBridge east panel -1: one u direction over face 0x6101);
  DONE FO SkybridgeDeck: the strip's z lines exist only on the samples x 521.3..541.3 (a fan onto them at both ends, top and
  soffit), so the deck end x 695 keeps its vertex set (Beat05GardenTests.FloorSeam); DONE PA SpireBuilder strip base MetalPaint;
  DONE PA Beat04SkybridgeTests lamp intensity = SkybridgeLamps.LampIntensityV2 (committed by PA in 3154d6c).
- 23:32 `GE-t04` 48 / 50: PreRevealKoiIsHidden PASSES (0 koi sample-phases in KS04-KS16 and the beat 2-6 stills; 0 spire points
  through the notch), ReliefTests, Q1IntegrationTests (floor probe: 0 in-lane holes / steps, the strip open), Beat03 / Beat04 /
  CityTests pass. Fails not mine: Beat02BoulevardTests.Blades (PA's LOOK_V2 blade colours). Mine: Agreements_FloorSeam_Closure_FaceA
  (the new terminus pilasters / cornice reached x 215.55 > 215.3): the east wall now stands 0.45 m behind x 215.25.
- 23:42 build `lookdev_GE` r01 + keyshots1080 `QA/captures/GE_r01` (the player log: "Relief: 18 meshes, 573 921 triangles" = the
  first Gulf-rib version). Looked at the sheet, KS04, KS05, KS09, KS13, KS14, KS15, KS16 full size:
  - KS04 / KS05: the canyon bridge reads as a six-storey skybridge across the canyon end (windows, a dark soffit with ribs), face B
    now continuous behind it with the garden trees in the gateway and the keystone's cyan ring above them. Good.
  - KS14 / KS15: the lintel and keystone over the crowns, the east block's lit facade at the end of the path and in the crown gap
    (KS15's "tower flank" in the gaps). KS15 mean luma 0.184 (window 0.078-0.173) and its banding region crown_gap_sky now lie on
    the east block's lit v1 facade: REQUEST TQ below. KS16: the promenade lintel shows as a beam over the wall's top-left; the
    stepped back of face B reads as three lit towers (GB-15 fixed).
  - KS13: the strip shows the abyss, the truss node beam at x 531.3 and the y -520 layer through the glass (T-DROP passes). Mean
    luma 0.207 (window 0.094-0.196; the graybox had 0.066): the brown haze fills the strip (PA's grade / exposure, GB-07); banding
    [abyss] 5.13 is measured across the strip's walls and the truss (region geometry): REQUEST TQ below.
  - Gulf ribs v1 (dark concrete slabs, pipes, stair towers with 44-triangle chamfers): black dashes and streaks on the lit section
    faces at 300-800 m, and 574 k triangles (budget 400 k): rejected. v2: pods / towers in NocFacade facade mode (lit rooms), decks
    with a dim soffit light line, risers only within 500 m, plain 12-triangle boxes, 8 draws.
- 00:08 build r02 + keyshots1080 `QA/captures/GE_r02` ("Relief: 20 meshes (0 validation failures), 43 937 triangles"); `GE-t06`
  27 / 27 (ReliefTests 6/6 incl. GulfRibs_OnTheSectionGrid: 524 pods, 297 towers, 529 decks, 64 riser bundles, 110 terrace edge
  runs; face A 711, face B 703 pieces; keystone 3.5 m over T4; VisibilityAuditTests incl. PreReveal; Q1IntegrationTests;
  Beat03EdgeTests). KS09 crop: the pods / towers read as lit modules plugged into the section; no black dashes.
- Spire (job A item 4, in this commit, Beat07HeartTests not yet run by me): FA's SkinOverride(0), five steel belts every 92 m with
  ring lines (the fins' strip colour), 16 aviation beacons on four fins below the cloud base; envelope unchanged (koi lint).
REQUEST TQ: Tools/qa/regions_default.json: KS15 `crown_gap_sky` now covers the east block's facade in the crown gap (GE's notch
  closure, D-029 GB-03): re-place it on a remaining sky / soffit gradient or drop it; KS13 `abyss` should lie inside the glass
  strip's trapezoid (x 0.35-0.65 of the frame width, y 0.3-0.75), clear of the strip walls and the truss beam.
GREEN step 2: committed below (job B structures, lanes, strip, rails, barrier, terminus, face B back; Gulf ribs v2; spire).
- 00:10-00:45 job A (FA-1 READY 22:58):
  - ReliefBuilder.ReliefOn: the TEXTURE_PLAN 5 relief (Gulf ribs, boulevard T0 relief, sign-wall base, the spire's belts / lines /
    beacons) is built with the v2 facade only (`-relief-v`, default = `-facade-v`), so the default picture (v1) only gains the
    D-029 gate structures (D-030 item 2); the gates are always built.
  - BoulevardRelief: the hero slabs' street faces (13 faces, y 40..160, 79-170 m from the walk) on FA's cell API: 914 balconies on
    the S4 faces (the cell's slab band 0.9 m out + a 0.9 m rail panel + top rail), 330 fins on the S0 faces (cell edges, 0.12 x
    0.45 m), AC units under 1 in 8 windows of S1-S3 residential bands (none this seed set: the slabs drew S0 / S4 skins); test
    BoulevardRelief_OnFAsCells re-derives every element from NocFacadeMath.
  - SignBaseRelief (CRIT0 #22, no NocShopfront shader exists: library rows): a 6.6 m arcade in front of the screen face (x 874..
    882.3, z 281..515): columns every 9 m, canopy with steel fascia and a light line, 26 shopfronts (4 in 5 cold, 1 in 5 warm) at
    x 874.3; shop gain 0.40 -> 0.28 after GE-r03 KS16 (the shop row was the brightest band under the screen).
  - Gulf rib pods / towers in the section mode (GE-r03 with -facade-v 2: facade-mode pods read as pale panels on the dark section).
- 00:35 `GE-t07` 44 / 45 (ReliefTests 7/7, VisibilityAuditTests, Q1IntegrationTests, Beat07HeartTests (the spire), Beat02
  BoulevardTests (terminus agreement passes now), Beat04SkybridgeTests); the one failure is PA's Blades colour order.
- 00:36 build r03 + keyshots1080 with `-facade-v 2` (`QA/captures/GE_r03`): "Relief: 26 meshes, 83 111 triangles, v2 relief on".
  Looked at the sheet, KS05, KS06, KS16, crops of KS04: the canyon bridge reads as a lit skybridge with a ribbed soffit in v2; the
  sign base reads as a shop arcade under the screen (human scale); the spire shows its belts and ring lines in KS17 / KS18; the
  balconies are hard to see at KS04's grazing angle (the slabs above the podium sit behind the blades).
- GB-19 player check: `-autotest -brisk -beat 3 -autotest-until 560` with the GE build: stop 64.10 s (expected 64.1), crossings
  exact, but beatsInOrder failed twice: (1) the runner's own teleport onto s 400 could land short of the boundary -> it now
  teleports only when the player is not already at StartS (+1 mm); (2) independent of the runner, the BeatDirector enters beat 2 at
  s 400.00, t 0.03, right after beat 3 at t 0.02, before the walk starts (the start placement projects s a few 1e-5 m short of the
  boundary): the runner counts that once as a start-boundary artifact (never a walk entry) and skips BeatDirector.OrderOk for a
  -beat start (it assumes beat 1 first). Re-run pending with build r04.
REQUEST lead: Scripts/Runtime/Player/PlayerController.cs (or ProgressDirector.cs): with -beat N the first frames report s =
  StartS - 1e-5 (beat N - 1 is entered once after beat N); place the player at StartS + 1e-3 or clamp ProgressDirector.S >=
  StartS until the walk starts. Systems/BeatDirector.cs:73: OrderOk assumes beat 1 first; with StartS > 0 compare b with
  (first entered beat + EnteredCount).
- 00:56 build r04 (`-facade-v 2`, "Relief: 27 meshes, 83 111 triangles") + keyshots1080 `QA/captures/GE_r04`; GB-19 player run
  `-autotest -brisk -beat 3 -autotest-until 560` with r04: AUTOTEST PASS (stop 64.10 s vs 64.1, beats 3..4 once and in order,
  1 start-boundary artifact counted apart, 0 errors). KS06 / KS05: the pale flat rectangles on face B are FA's lit section atria
  (also in r03 before the pods moved to section mode), not ribs: noted for FA, no request.
- Inspection poses (`-facade-v 2`): `QA/captures/GE_r04p_terminus_east` (the terminus reads as a finished hall: 8 tall mullioned
  windows, pilasters, door, cornice; but the lit panes sat in the window plane and read as flat white blocks -> the panes are gone,
  the windows look into the hall: lit far walls (west, north), dark floor and ceiling; fastcheck OK, to be tested),
  `GE_r04p_canyon_bridge_east`, `GE_r04p_north_slab_balconies`.
- 01:04 `GE-tall01` full EditMode suite: 354 / 357. Failures, none mine: Beat02BoulevardTests.Blades (PA's LOOK_V2 blade colours),
  FoliageTests.Crowns_AreLobed (FO, B3 lobes), TextureTests.Import_AlbedoLastMipMean (TP, LeafCrown albedo). Mine all pass
  (ReliefTests 7/7, VisibilityAuditTests incl. PreReveal, Q1IntegrationTests, Beat03 / Beat04, AutoTest maths).
GREEN step 3 (job A relief behind -relief-v, spire, sign base, ribs in section mode, GB-19 runner) committed below; the terminus
interior follows after its test.
- 01:12 `GE-t08` (Beat02BoulevardTests | ReliefTests | Q1IntegrationTests) 18 / 19 with the terminus interior (the failure is PA's
  Blades); committed. Pose `QA/captures/GE_r05p_bridge_portal` (r04 exe, v1 facade, from the tongue at x 330 looking back): the
  canyon end reads as a gateway: the skybridge's lit east face with the upper tube's cyan portal ring at its centre, the ribbed
  soffit, the terminus's east face below. Two rebuilds for the terminus pose failed on other owners' mid-edits (SU NocWet.shader
  compile errors at 01:14; FA FacadeSkinTests.cs:184 syntax at 01:22): retried below.
- 01:30 DONE FA REQUEST (FA.md:180): EdgeMesh.FacadeWriter.Quad, GulfFace.PortalPanel and ReliefKit.PanelWithHole write uMin /
  uMax through NocFacadeMath.SafeExtent (TQ-09); DONE FA REQUEST (FA.md:114) spire seed SkinOverride(OverrideSpire) (693a102).
  `GE-t09` (Beat03EdgeTests | ReliefTests | FacadeSkinTests | VisibilityAuditTests) 32 / 32. Seen: DONE TQ for my regions request
  (KS15 crown_gap_sky dropped, KS13 abyss inside the strip).
- 01:45 beat stills (default v1, `QA/captures/GE_r05b`, 51 stills, framing failures 0): looked at sheets 1-3 and
  beat05_garden_end full size (the east block's west facade stands beside the arcade: no void, no koi); image_metrics flags are
  the GB-08 / GB-09 window-gap / lamp-head class (FA / QA). Pose `GE_r05p_terminus`: the hall interior reads (ceiling band,
  see-through to the south windows); the far walls stay G1's flat EmitShopCold (critique GE-21, SU / PA's material).
- Disk: deleted my superseded folders GE_r01, GE_r02, GE_r03, GE_r04p_terminus_east (numbers above). Kept: GE_r04 (keyshots1080,
  -facade-v 2), GE_r05b (beat stills, v1), GE_r04p_canyon_bridge_east, GE_r04p_north_slab_balconies, GE_r05p_bridge_portal,
  GE_r05p_terminus. W: 21 GB free.
- Not done (job A): relief on other beat towers within 150 m (alley towers, ring towers, Old Quarter) and setback parapets; no
  CityField / ring-tower top lies below the cloud base (all tops at y 560), so there is no crown to build (decision 3).
GE DONE 2026-11-16T01:49+01:00
