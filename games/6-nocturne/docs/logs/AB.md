# AB - Phase 4 wave A: beat 3 (The Edge), money shot 2 (KS07-KS10), the abyss from beat 4 (KS12, KS13)

Owner files: Beats/Beat03_Edge/** (and new files there); Beats/Relief/GulfRibBuilder.cs, ReliefBuilder.cs, ReliefKit.cs,
GateDims.cs, NotchGate.cs; Beats/Beat04_Skybridge/SkybridgeLamps.cs (deck lamp fixtures only); Scripts/Runtime/City/NocTraffic.cs;
Shaders/NocStreak.shader; Shaders/NocLine.shader; Tests/EditMode/Beats/Beat03EdgeTests.cs, Tests/EditMode/ReliefTests.cs,
Tests/EditMode/Beats/Q1IntegrationTests.cs, new tests (AbyssTests.cs); docs/logs/AB.md; QA/critique-AB.md.

## Start (2026-11-16)
- No earlier AB log: fresh start. HEAD a9d7137 (lead: D-031..D-033, wave A launched).
- Read: BUILDER_BRIEF, MASTER_PROMPT 14.0 / 14.5.3 / 14.5.4 / 14.6 / 14.7 / 14.8 / 14.10, DEVELOPMENT D-001..D-033,
  critique-lookv2-01 (INT-01 S1 is mine; INT-03, INT-09, INT-15, INT-18, INT-19, INT-42).
- Looked at (full size): lookv2_01 KS09, KS10, KS12, KS13; lookv2_01b beat03_edge_look_l, beat03_edge_look_down60; crop KS12
  (560-1000, 280-460) x3.
  - KS10: the drop is a flat brown field between face A (left) and gate block A (right); two white NocLine "laser beams" (the
    Phase-3 layer stand-ins) cross it; the cross bridge at z 850 is a thin white line; no light below the deck.
  - KS09: the same; face B reads (windows, orange band dashes) down to the rail line, nothing below.
  - KS12: the same two beams; a horizontally stretched white blob at (700, 375) and orange blobs at (905, 170) / (1890, 150)
    float in the air (INT-19).
  - KS13: the glass strip shows a brown field, one white beam and an orange halo.
  - beat03_edge_look_l: the "diagonal flare spike" of INT-18 above the balustrade light is the y -120 layer line running north
    from under the pier, not a flare; the light itself is a white blob.
- Visibility (QA/reports/visibility.md): KS10 built sky / deck / rail / near / drop 2.8 / 50.3 / 4.6 / 10.0 / 32.4 %, near
  structure = Beat04_skybridge/gateA/block (227 samples); pose yaw -5 / pitch -32 (D-029).

## First start, reconstructed (08:05-09:56; cut off by the usage limit before the log was written)
The tree holds this work (git diff, read at the restart):
- AbyssTraffic.cs (new): the eight H7 layers of 14.5.3 at their (x, y), z -5 500..5 500, 4 lanes each (2 per direction, 5 m apart,
  10 m median), 45 m mean spacing (+-30 % jitter via NocTraffic.Vehicles), 17-26 m/s per layer, ramp e 0.06
  (NocturneConstants.TrafficRampLayer); 24 cross-bridge lanes (4 per deck, 30 m, 14 m/s, ramps behind the face planes); a layer
  glow strip (NocStreak kind 6) per layer. TrafficLayerLines.cs (+ .meta) deleted, EdgeDims.Layer* and EdgeMat.LayerLine* gone.
- NocStreak.shader: kind 4 (layer vehicles: head pair to a camera in front, tail pair behind, the pair resolved into two lobes
  from 3 px of separation, energy = emitting area / drawn area), kind 5 (steady structure lights), kind 6 (layer glow strip);
  tails (kinds 0 and 4) at least 3 px and 18 % further toward SodiumAmber (INT-42).
- NocTraffic.cs: RibbonMesh / RibbonMatrix for kind 6.
- CrossBridge.cs: deck lamp posts (6 m, every 25 m, both edges, staggered) whose heads are a kind-5 batch (Beat03_Edge.BridgeLamps,
  LampWarm x 4.0, 0.6 m), lit portals (16 x 8 m opening, concrete surround 3 m, WinNeutral glow wall) at both faces; the edge
  strips dimmed to a WinNeutral LED handrail line (x 0.35).
- Beat03_Edge.cs: the balustrade top rail is a warm lit handrail (LampWarm x 1.2), balustrade pucks 0.75 -> 0.4 (INT-18).
- GulfRibBuilder.cs: pieces on levels 0-5 (to y -1 860; GE had 0-2), galleries (lit glazed corridors, 30 % per 24-column segment,
  |z - 195| < 2 000), atria R 12-30 m (4 % per 8-column segment; dim hall + lit floor-plate edges, concrete surround), terrace lip
  lights (0.6 m strip on every edge beam, edge runs 20 m). ReliefBuilder / ReliefKit: the five emissive slots.
- AbyssTests.cs (new): layer placement / counts / clearances, the H7 ramp rule from every key pose and the walk box corners, bridge
  lanes' ramps behind the faces, lamps on their decks, the Gulf faces' light content, an explicit KS10 composition search tool.
- Build lookdev_AB 09:25 (515 s: shared-include recompile), capture QA/captures/AB_r01 (keyshots) 09:27, image metrics
  QA/reports/image_metrics_AB_r01.md: KS07-KS13 mean luma .121/.148/.128/.116/.114/.122/.141; new NaN-like / isolated hits in
  KS08 (27 dark), KS09 (2 bright, 10 dark), KS10 (2 bright), KS12 (7 bright in a row at y 469 = point lights 31 px apart).
- After r01 (not built yet): atria as dim hall + floor-plate edge lines (r01 showed flat beige cards), the kind-6 glow, kind-4
  changes in NocStreak (09:44).

## Second start (2026-11-16 10:05, run wf_82c73c1d-0e6)
- 10:10 read the diff of my files; line endings normalised back to each file's HEAD convention (the first start had written CRLF
  into LF files: whole-file diffs). SignBaseRelief.cs is SG's edit, not mine (not committed by me).
- 10:15 fastcheck all OK (runtime 196 / editor 260). Offline shader check: a scratch fxc harness (scratchpad fxc/shcheck.py:
  Windows SDK 10.0.26100 fxc, vs_5_0 / ps_5_0, base + INSTANCING_ON, the URP / core / universal-config ShaderLibrary copied, the
  empty PopMarker() macro patched; a planted undeclared identifier is caught): NocStreak 4/4 OK, 0 own warnings; NocLine 4/4 OK.
- 10:16 commit of the in-tree work (restart rule: commit early), then r02.
- (reconstructed at the third start from the tree; the log stopped at 10:16) 10:36 commit 4203a66. AB-t01 (10:08, AbyssTests +
  Beat03EdgeTests + ReliefTests + Q1, 34 tests): 33/34, BridgeLanes_RampsBehindTheFaces_H7 failed (bridge 3's lane ramp stood
  3 m in front of face A: the faces flare with |z - 195|, the lanes sit at z +-1.5..5 of the axis).
- After the commit (uncommitted at the 11:37 pause, re-applied by the lead):
  - AbyssTraffic: 6 lanes per layer (3 per direction) at 32 m mean spacing (r01 KS10: 4 lanes at 45 m read as two dotted
    lines): 2 064 vehicles per layer, 16 512 in all (2-triangle instanced quads, 33 k triangles); bridge lane ramps from the face
    planes at each lane's own z (the AB-t01 failure).
  - QA/reports/AB_ks10_search.md (AbyssTests' KS10 composition search, built world, 128 x 72 rays): yaw -30..+10, pitch
    -24..-34: drop 26.2..38.9 %, never 40 %: the deck takes 40-58 % at every pitch (the pier fills the lower frame), so no
    camera composition reaches D-032 1's 40 %.
  - The glass edge of the pier (TonguePier, EdgeDims.Ledge*): x 338..371.95, lateral 1.75..w on both sides, a walk-on NocGlass
    pane 3 cm below the top on a steel frame (edge girder under the shoe, nosing, cross beams every 2.5 m), slab and hull pulled
    in to 1.75 there, end faces at x 338 / 371.95, the slab soffit inside gate block A; the half-widths (Acceptance (1)) are
    unchanged. Q1IntegrationTests skips the ledge like the KS13 glass strip (InGlassLedge) and asserts it is open;
    Beat03EdgeTests' lane-clearance test ignores parts at or below the walk plane.
  - Balustrade lights (INT-18): step-light fixtures (steel housing 0.20 x 0.07 x 0.045 on the shoe, lens slit 0.13 x 0.012,
    BalLightGain 1.0 on the slit, BalWetGain 0.4 keeps the wet-floor source at the r01 level).
  - SkybridgeLamps (INT-18): housing 0.62 x 0.10 x 0.28 (Steel) with the emissive lens 0.50 x 0.012 x 0.20 in its underside,
    NocHalos queued at the lens.
  - GulfRibBuilder: PGallery 0.3 -> 0.5 (r01: the faces below the deck nearly dark).

## Third start (2026-11-19 ~17:40, run wf_8e439303-949; GPU at 100 % from other projects: CompileCheck / EditMode first,
## -capture-only 1080p shots only)
- Read the log, git diff of my files (9 files, +145 / -38, as above). fastcheck: runtime OK (198); editor FAILED once (another
  agent mid-edit), OK (263) on the retry 5 min later.
- FX's REQUEST AB (NocLine.shader:211 drop the NocSurfV2() branch; optional Beat03_Edge.cs:172, ReliefBuilder.cs:34): open, to do
  with the next shader batch.
- 18:34 AB-t02 (AbyssTests incl. the explicit KS10 search, Beat03EdgeTests, ReliefTests, Q1IntegrationTests, VisibilityAuditTests):
  41/42. The bridge-lane ramp fix passes. Failed: Q1 A_EveryLaneMetreIsFlooredAtYZero, 1 in-lane hole at s 550.5 lat 2.43 (and
  1 margin hole at s 544.5 lat 2.67): the probe slipped between InGlassLedge's limit w(x) - 0.30 and the girder box built at
  w(x_mid) - 0.30 per 1 m (the bow moves w by up to 2 cm in half a metre).
  - Built KS10 (visibility.md, audit pose yaw 355 / pitch -32): sky / deck / rail / near / drop 3 / 34 / 5 / 22 / 36 % (r01 world:
    3 / 50 / 5 / 10 / 32): the glass edge took the deck from 50 to 34 %, but the ledge's steel (girder 0.30 x 0.42, ribs 0.08 x 0.22,
    nosing) seen through the glass took 12 % (search: Tongue_ledgeFrame x1 100 of 9 216 rays at every pose). Best search pose
    yaw -25 / pitch -34: drop 42.8 %. KS09 built 25 / 34 / 4 / 5 / 33 (spec 26 / 39 / - / - / 36).
  - Ray model of the ledge (scratchpad ab/ledge_model.py, the eye, the pier, shoe and frame; reproduces the built deck 34.1 and
    frame 11.8 %): frame share 11.8 % (r02 steel) -> 4.7 % with girder 0.14 x 0.14, ribs 0.05 x 0.16 every 2.5 m, nosing 0.21:
    built drop expected about 36 + 7 = 43 %.
- 18:50 slim ledge steel (EdgeDims LedgeGirderW / D 0.14 / 0.14, LedgeBeamW / D 0.05 / 0.16, LedgeGlassLip 0.04; the girder is a
  prism swept along the deck samples with w(x) at both ends of each interval, closed at both ends, so the floor probe meets its
  top wherever InGlassLedge stops (1 cm slack)). New test AbyssTests.KS10_DropShare_AtLeast40_OnTheBuiltWorld (14.5.3 (2),
  D-032 1).
- DONE FX REQUEST AB (NocLine.shader:211): the NocSurfV2() branch is gone, the v2 paint is the only path; fxc offline (fx/shcheck):
  NocLine 7/7 OK. (The optional Beat03_Edge.cs:172 / ReliefBuilder.cs:34 cleanups: later.)
- fastcheck all OK (runtime 198 / editor 264).
- 19:30 D-034 hook (DREAD 3.1, free): NocStreak global _NocAbyssOccluder (xy centre x / z, z half-length, w strength) and
  NocAbyssOcclusion(P): layer vehicles (kind 4) and layer glows (kind 6) below y -450 inside the shadow's outline dim by w; all
  zero (unset) it is exactly 1. fxc offline NocStreak 4/4 OK. Build lookdev_AB queued (behind lookdev_SG).

## Fourth start (2026-11-20 ~10:05, run wf_ab1bc94d-acd; .NET / Unity slow on this machine, see STATE.md)
- Read the log and my commits since 34e5d92 (540de0e, 7ae33ef, ac27f97); git status: none of my files was uncommitted.
- In flight at the 11-19 pause: the lookdev_AB build had finished (19:55, exit 0: it carries 7ae33ef and ac27f97), its capture had
  not run; AB-t03 (the slim ledge steel's floor probe and KS10_DropShare_AtLeast40) never ran.
- 10:08-10:25 capture AB_r02 (that build, keyshots1080 KS07-KS10; 17 min, mostly PowerShell start-up), 10:45 AB_r02b (KS11-KS13,
  2 min). Looked at all seven at full size and crops (scratchpad ab/ks09_star.png, ks10_faceA.png, ks08_atria.png).
  luma (8-bit, Rec.709)      mean   p99  p99.9  share >= 120     (r01 p99)
  KS07                       34.7  120.7  220   1.05 %           112.8
  KS08                       42.7  130.8  231   1.26 %           143.2
  KS09                       37.7  210.3  224   2.12 %            87.6
  KS10                       35.4  211.0  216   2.03 %            56.1
  KS11                       29.6  120.7  221   1.07 %           123.4
  KS12                       32.4   96.2  224   0.56 %            77.7
  KS13                       37.3  103.0  223   0.64 %            90.3
  lookv2_check AB_r02 + AB_r02b: 6 / 7 PASS (KS12 p99 96.2). image_metrics: NaN-like / isolated pixel flags as in r01 (3-6 px).
  KS10 regions: haze 50-58 (median luma of the drop in 30-75), layer -120 river max 250, bridge lamps max 242.
- What r02 shows (defects, QA/critique-AB.md):
  - AB-01 S2: the lit handrail (NocLine tube, LampWarm x 1.2 all round) is a flat clipped peach bar 47 px thick across KS09 / KS10
    (with bloom); it alone is 37 466 of KS10's 42 027 pixels >= 120 (29 414 of 44 030 in KS09): the p99 pass of KS09 / KS10 is that
    bar. Without it KS10 has 0.22 % and KS09 0.70 % of the frame >= 120.
  - AB-02 S1: the layer glows (NocStreak kind 6, sigma 14 m, gain 0.12) read as hard searchlight beams fanning from the vanishing
    point (KS08 top half, KS09, KS12): the "laser beam" of INT-01 in a new form. In KS10 the -120 glow is +35-60 codes over the haze.
  - AB-03 S2: atria read as flat beige cards (AtriumLo 0.3 x WinWarm is brighter than a lit window); in KS08 a riser bundle runs down
    through one.
  - AB-04 S3: a balustrade step light at KS09's right edge (1880, 490) draws a white star (halo plus two specular streaks).
  - INT-19 is not a light over the void: the KS12 blobs (700, 375) white, (905, 170) and (1890, 150) orange, and KS13 (1240, 730) /
    (1380, 1050), are the Forward+ point-light specular of the skybridge lamps on the curved tube glass and the glass floor (NocGlass):
    soft 30-60 px blobs instead of a lamp-sized image. REQUEST FX below.
  - The vehicle rivers read as rivers (KS07's horizon line of heads and tails, KS10 / KS13 looking down); the bridges read as lit
    trusses with lamp rows at 650-1 200 m; the faces below the deck carry galleries and lip lights down to the haze.
- r03 changes (not built yet; Unity unavailable most of the morning):
  - NocLine.shader: _EmissionDown (0 = all-round emission as before for every other line; 1 = the lower half of the tube glows,
    1 - smoothstep(-0.55, -0.05, n.y)); fxc offline 10/10 OK (scratchpad fx/shcheck.py).
  - Beat03_Edge.cs: the handrail variant sets _EmissionDown 1 (a dark steel rail with an LED channel underneath), RailGlowGain
    1.2 -> 1.6; FX's optional TP-2 cleanup (the wet-wall macro is FacadeMacro only).
  - TonguePier.cs: a warm LED safety line (LampHead, 1.5 cm, 0.8 mm above the walk plane, x 338.05-371.90 both sides) in the ledge
    nosing's top, where the stone walk meets the glass.
  - AbyssTraffic.cs: glow sigma 14 -> 22 m, gain 0.12 -> 0.06, per-layer GlowIntensity (0.15 above the deck, 1 at y -600 and below);
    HeadGain / TailGain 6 / 4.4 -> 8 / 5.
  - GulfRibBuilder.cs: atria get steel mullions every 3 m (0.35 m, 0.45-1.0 m proud), luminaire lines 1.0 -> 0.6 m, never across
    another piece (Overlaps); galleries 0.5 -> 0.65 per segment and 1.6 -> 2.0 m tall. ReliefBuilder.cs: AtriumLoGain 0.3 -> 0.12,
    FX's optional TP-2 cleanup (-relief-v defaults to 2).
- REQUEST FX: Shaders/NocGlass.shader (or its lit include): the Forward+ point-light specular on the skybridge tube glass and the
  KS13 glass floor draws soft 30-60 px white / orange blobs that read as lights floating over the void (INT-19; KS12 (700, 375),
  (905, 170), (1890, 150); KS13 (1240, 730), (1380, 1050) in QA/captures/AB_r02b). Please give the glass's point-light term a
  lamp-sized lobe (smoothness 0.98+ for that term only, or scale it by about 0.3) and keep the environment reflection as it is.

## Fifth start (2026-11-20 20:23, wave A; the fourth start's work is in the lead's WIP commit 2fe0da9)
- Read the log, my commits (4203a66, 540de0e, 7ae33ef, ac27f97, 2fe0da9 for my files), critique-AB. No open REQUEST AB in any copy's
  log (FX's NocLine request DONE in the third start, its optional TP-2 cleanups in r03). Progress note: Logs/AB_progress.md.
- 20:27 fastcheck all OK (runtime 199 / editor 265) after copying the generated Assembly-CSharp(-Editor).csproj from the main
  project into this copy (gitignored; fastcheck reads its hint paths only).
- 20:45 / 20:53 my AB-b01 queue calls failed at once: my own `tail -f Logs/unity-queue.log` monitor held the file open, so
  unity-queue's Add-Content threw (lesson: never tail the queue log; watch the job's own Unity log). Orphaned tail / grep killed;
  20:55 AB-b01 started (AT-b01 before it: SUCCESS, 640 s, the shared-include recompile of this copy's cold Library).
- 20:58 capture AB_r03 (the r03 tree, KS07-KS13 1080p, 6 s). Luma (8-bit Rec.709; scratchpad ab/lstats.py):
  shot   mean   p50   p99   p99.9  >=120 %   (r02 p99)
  KS07   31.6  28.4  108.7  225.6  0.86      120.7
  KS08   38.3  36.3  118.7  231.7  0.98      130.8
  KS09   34.6  28.8  153.4  227.5  1.24      210.3 (r02: the handrail bar)
  KS10   33.6  27.1  167.9  220.7  1.22      211.0 (r02: the handrail bar)
  KS11   29.3  24.7  108.2  228.1  0.75      120.7
  KS12   31.9  25.1   87.7  225.1  0.63       96.2
  KS13   37.1  27.7   97.7  224.4  0.63      103.0 (KS13 rule >= 90)
  Looked at KS07, KS09, KS10, KS11, KS12 full size, crops KS10 (800-1280, 200-720) x2, KS07 atria (640-1120, 0-420) x2, KS09 star
  (1700-1920, 400-600) x4, highlight masks (scratchpad ab/hmask.py).
  - AB-01 fixed: the handrail is a ~5 px warm line (the LED channel), no bar; the ledge LED reads as a thin warm line at the glass edge.
  - AB-02 fixed: no searchlight beams; the rivers are rivers of heads / tails converging to the vanishing point, but sparse
    (dotted single files where near): denser in r04.
  - AB-03 open: the atria still read as pale beige cards with horizontal lines at 600 m (KS07 (740-775, 40-100), (1020-1057, 245-310)).
  - AB-04 open: the KS09 step light at (1890, 495) still draws a star (point-light specular on the balustrade glass and the shoe).
  - AB-05 new (S2): KS11 vehicles of the y 30 layer seen broadside draw "=" glyphs: NocStreak spread the lamp pair 1.5 m across the
    streak in screen space whatever the view (the pair axis is horizontal across the vehicle, near-parallel to the view broadside).
  - INT-19 unchanged (FX's NocGlass point-light lobe; KS12 (705, 375), (905, 170)).
  - KS07 / KS08 / KS11 fell below 120 since r02: the r03 glow (0.15 above the deck) and the handrail bar no longer add light there.
- 21:05 AB-t04 (AbyssTests|Beat03EdgeTests|ReliefTests|Q1IntegrationTests|VisibilityAuditTests, QA/reports/AB-t04.xml): 43 / 43.
  KS10_DropShare_AtLeast40_OnTheBuiltWorld: sky / deck / rail / near / drop 4.2 / 34.2 / 4.7 / 15.4 / 41.5 % (near: Beat04 gateA/block
  x884, Tongue_ledgeFrame x467 of 9 216 rays). Q1 floor probe: no hole (the 7ae33ef girder prism). The run rewrote
  QA/reports/visibility.md / .json, AB_ks10_search.md and q1_floor_probe.txt (test outputs).
- r04 (21:15): NocStreak kind 4 pair separation = the projection of the vehicle's cross axis (AB-05; fxc offline 4/4 OK);
  AbyssTraffic spacing 32 -> 24 m, heads / tails 8 / 5 -> 10 / 6; galleries in lit runs of bays (18 % inner bays dark, >= 9 m
  breaks) plus a second corridor per band and segment (0.45), never across another piece; gallery gains 1.4 / 1.2 -> 1.7 / 1.45;
  AtriumLo 0.12 -> 0.04; bridge deck lamps 4 -> 6; balustrade lens and light LampWarm, light 2.0 -> 1.4 and 0.32 m inside the
  edge (AB-04). fastcheck all OK (205 / 271).
- 21:31 AB-b02 FAILED: "script class layout is incompatible between the editor and the player", NocAtmoState (AT's file) gained
  DeckOcc* / HazeOcc* fields while my build compiled (AT mid-edit); not my files. 21:50 AB-b03 (after DR-b01) SUCCESS 23 s.
  (The b02 chain had captured KS07-KS13 with the old exe: that AB_r04 was deleted unseen.)
- 21:51 capture AB_r04 (r04 + AT's NocAtmosphere / NocSchedule edits in the tree). p99: KS07 94.0, KS08 113.0, KS09 113.1, KS10 147.7,
  KS11 102.0, KS12 79.4, KS13 97.4. Looked at KS12 full size, KS11 (0-1920, 300-460), KS07 atria crop x2:
  - AB-05 fixed: broadside vehicles are single dashes (white or red), no "=" glyphs (AB_r04/KS11 row 330-445).
  - The rivers are denser (KS12: the -120 and -520 layers read as streams).
  - Regression: the galleries almost vanished (KS12 r03 had a dozen long lines, r04 one): my new Overlaps() test in Gallery()
    rejected nearly every corridor (they are 54-216 m long and cross pods, decks and edge runs). r05 drops that test.
  - AB-03 is not mine: a KS07 capture with -relief-v 1 (no Gulf relief at all) shows the same beige cards (KS07 (740-775, 40-100)
    etc.), so they are the facade's sky-lobby panels (NocFacade, FX), not my atria; AtriumLo 0.12 -> 0.04 left them unchanged
    (same pixel values at (1040, 60-124)). Logged for FX below.
- r05 (21:54): galleries without the Overlaps test; cross-bridge edge strips 0.35 -> 0.8 (KS12: the bridges are the lines across
  the gulf). AB-b04 + AB_r05 running.
NOTE FX (not a request on a file of mine): the facade's sky-lobby bays read as flat pale-beige cards with floor lines at 600-700 m
  (AB_r03 / AB_r04 KS07 (740-775, 40-100), (1020-1057, 245-310), (400-440, 330-395); KS11 (812-860, 55-150), (1262-1325, 55-165)),
  with or without the Gulf relief; a sky lobby lit at that level reads as a card, not a hall.
- 21:59 AB_r05 (AB-b04 21:58, SUCCESS; one transient BuildPlayerDataGenerator TypeDb exit 3 in the log, result Succeeded). p99: KS07
  114.0, KS08 126.3, KS09 151.4, KS10 177.6, KS11 118.6, KS12 104.8, KS13 98.4; lookv2_check AB_r05 4 / 7 (KS07 / KS11 / KS12 p99).
  Looked at KS09, KS12 full size: the galleries are back as dashed corridors (lit runs with dark bays), the KS09 step light is a
  soft warm glow without the white star (AB-04), the rivers read as streams. (DR's hull is in the KS09 / KS12 sky now: not mine.)
- r06 (22:02): galleries 0.65 -> 0.75 (first) and 0.45 -> 0.55 (second), gains 1.7 / 1.45 -> 2.1 / 1.8 (beyond 800 m they sat at
  90-120 after the fog); bridge-lane vehicles 2.4 / 1.8 -> 6 / 4. fastcheck OK (207 / 274). AB-b05 + AB_r06 + the abyss encode clip
  (AB_enc06) running.
- 22:16 AB_r06 (AB-b05 22:15, 451 s: AT's NocAtmosphere edit recompiled every shader). p99: KS07 121.7, KS08 130.4, KS09 158.1,
  KS10 184.1, KS11 122.7, KS12 107.6, KS13 97.1; p99.9 KS09 233.1, KS10 221.9; lookv2_check AB_r06 6 / 7 (KS12 p99).
  KS12 profile (scratchpad): the edge strips of the 655 m bridge (y 471-477) and the 1 185 m bridge (y 188-190) sit at luma 100-110.
- r07 (22:17): cross-bridge edge strips 0.8 -> 1.25. AB-b06 queued behind AT-b03 (the final build if r07 passes).
- 22:18 AB_enc06: the enc_beat03_abyss clip (600 frames, s 533-548, r06 build). encode_test x264 CRF 23:
  - without a region: PASS (fidelity only; QA/reports/encode_test_AB_enc06.md);
  - region 1080,30,1460,220 (the open haze right of the -120 river, above the handrail; QA/reports/encode_test_AB_enc06r.md): FAIL,
    5.26 visible steps / 1000 (max 1.41 code), BandScore 2.39 (lookv2_01: 9.06), source 0.503 PASS. The x8 crop
    (QA/encode/AB_enc06r/.../x264_crf23_cli0_f00342_x8.png) shows x264's blocks in the smooth haze between the rivers: the
    content of my part sits outside that patch; the haze texture / dither is AT's (INT-15 stays open with AT). Frames deleted (1.6 GB).
  - Frame 342 showed the r04 balustrade light inset (0.32 m) as two separate orbs (its specular images in the balustrade glass and
    the glass ledge, 55 px above / 80 px below the lens): back to 0.2 m (images over the fixture), warm colour and 1.4 kept.
- 22:24 fastcheck all OK (207 / 274) on r07; AB-b06 (the final build) started 22:23:55; AB-final (the full EditMode suite) queued
  behind it; then AB_r07 capture and the smoke (-autotest -brisk -autotest-until 880 -beat 3).
- 22:25 AB-b06 (final build, Builds\AB): SUCCESS, 0 errors, 0 warnings (Logs/AB-b06.log; the player scripts compiled from the 22:24
  sources: strip 1.25 and inset 0.2 included). Beat03_Edge 113 meshes, 0 validation failures, 97 872 triangles, 24 realtime lights;
  21 984 layer + 1 267 bridge vehicles (instanced, 2 triangles each), 707 bridge lamps; Relief 155 397 triangles (r03 150 735).
- 22:25 AB_r07 (final captures, QA/captures/AB_r07, contact sheet _contact/contact_AB_r07_01.png):
  shot   mean   p50   p99   p99.9  >=120 %
  KS07   32.3  28.8  122.4  232.5  1.02
  KS08   39.1  36.4  131.9  237.7  1.19
  KS09   34.5  28.3  177.1  233.1  1.49
  KS10   33.4  26.9  188.1  222.0  1.41
  KS11   30.4  25.1  122.9  235.1  1.05
  KS12   32.5  25.3  120.6  233.0  1.01
  KS13   35.9  28.0   97.0  228.2  0.64   (KS13 rule >= 90)
  lookv2_check AB_r07: 7 / 7 PASS (QA/reports/lookv2_check_AB_r07.md). Looked at KS09, KS10 full size, KS12 bridges crop
  (480-1440, 150-500), KS09 step light crop x3: the bridges read as lit decks (continuous edge lines, lamp rows, deck traffic),
  the rivers as dense streams, the galleries as dashed corridor lines; the KS09 step light is a warm glow with soft glass
  streaks (the NocGlass point-light lobe, REQUEST FX of the fourth start).
  image_metrics AB_r07: NaN-like 2-12 px per shot (isolated single-pixel lamps / heads in the haze, e.g. KS09 (1023, 478),
  (998, 480) on the 655 m bridge row; KS08's 5 dark ones at (194-210, 303-307) are not in my content), KS13 banding[abyss] 1.117
  (AT's haze); as in r01 / r02 (3-6 px). Proposed fix for wave B: kinds 4 / 5 drawn at least 3 px (energy kept), not done now
  because it lowers the peaks and KS12's p99 margin is 0.6.
- 22:26 smoke AB-smoke (-autotest -brisk -autotest-until 880 -beat 3): FAIL gcAllocPerFrame (mean 86.1 B / frame, worst 606 208 B,
  87 frames allocating; Logs/player-AB-smoke.log). 22:36 AB-smoke2, the same build and arguments: PASS, 30.7 B / frame (excess
  0), worst 4 096 B, 0 coverage misses (48 204 rays), beats 3-5 entered at s 400 / 560 / 880 (Logs/player-AB-smoke2.log): the
  first run's allocation did not repeat (DR's smoke of the same section in this copy at 22:34 also passed, worst 4 KB).
REQUEST FX (restated with code; INT-19, AB-04 residue): Shaders/NocGlass.shader, NocForwardHook, glass branch: evaluate the
  point / main lights with a lamp-sized lobe instead of the glass's own (grime-widened) roughness, so a lamp reflects as a small
  bright image at the fixture, not a 30-60 px blob (AB_r07 KS12 (705, 375), (905, 170); KS13 (1240, 730), (1380, 1060); KS09 step
  light (1720-1920, 400-600)); the environment term stays as it is:
      BRDFData brdfL = brdf;
      brdfL.roughness = max(brdf.roughness * 0.35h, 0.02h);          // GGX alpha of the light lobe only
      brdfL.roughness2 = brdfL.roughness * brdfL.roughness;
      brdfL.roughness2MinusOne = brdfL.roughness2 - 1.0h;
      brdfL.normalizationTerm = brdfL.roughness * 4.0h + 2.0h;
      half3 glass = brdf.diffuse * inputData.bakedGI * aGlass + env * F + NocDirect(brdfL, inputData);
  (replacing "+ NocDirect(brdf, inputData)" in the glass line; the energy of the lobe is kept by the GGX normalisation).
- 22:58 AB-final (the full EditMode suite on the final tree, QA/reports/AB-final.xml): 435 tests, 429 passed, 5 failed, 1 skipped
  (AbyssTests.KS10_CompositionSearch, Explicit). Mine all green: AbyssTests 7, Beat03EdgeTests 10, ReliefTests 7, Q1IntegrationTests 3,
  VisibilityAuditTests 8, MaterialTests 15 (KS10 drop 41.5 %; layer vehicles 21 984 = 2 748 per layer; galleries 936, atria 12,
  lights on levels 3-5 327; beat 3 meshes 97 872 triangles). The 5 failures are in other owners' files, none touches mine:
  AtmosphereTests.LookV2_DisplayLumaTable_3_2 (AT, s 300 e -6 40.2 vs 50.1 +- 3: AT's NocAtmosphere / NocSchedule edits in flight),
  Beat07HeartTests.PlazaInlay_SameTopFan_RingsSeamsFade (HT, 962 vs > 1000), BoulevardStreetTests.LightBudget_AtMost28_SmoothFades
  (BV, BoulevardLights 29 vs 28) and .Shops_Varied_NeverBrighterThanTheLitRoom (BV, 18 vs 17), WetFloorTests.WallWindows_MeanOne_
  Bounded_StreakVertically (FX / SU, -1.4e-11 vs >= 0: a rounding tolerance).
- Files of mine changed in this start (uncommitted): Beats/Beat03_Edge/AbyssTraffic.cs, Beat03_Edge.cs, TonguePier.cs;
  Beats/Relief/GulfRibBuilder.cs, ReliefBuilder.cs; Shaders/NocStreak.shader; QA/critique-AB.md; docs/logs/AB.md. Test outputs
  rewritten by the runs: QA/reports/AB_ks10_search.md, q1_floor_probe.txt, visibility.md / .json; new: QA/reports/AB-t04.xml,
  AB-final.xml, encode_test_AB_enc06(.md/.json), encode_test_AB_enc06r(.md/.json), lookv2_check_AB_r07.md, image_metrics_AB_r07.*.
  Captures kept: QA/captures/AB_r03 (before), QA/captures/AB_r07 (final, with _contact); AB_r04-r06 and the encode frames deleted.
  Logs/AB_progress.md is the live note. Assembly-CSharp(-Editor).csproj copied into this copy (gitignored) for fastcheck.
AB DONE 22:59

## FIX WAVE (panel p4_01, 2026-11-21; task Logs/fix/AB.md, lead rulings D-040)
- 01:57 start. Copy at master caddb90 (pre-warmed). No open "REQUEST AB" in any copy's logs (checked abyss / blvd / heart / waveb /
  integ). Baseline: AB-b10 01:59-02:02 SUCCESS (the merged tree, 0 errors); the p4_01 captures in INTEG are the baseline pictures.
- Diagnosis C13 (step-light pops): the balustrade light was a point light 0.15 m from the balustrade glass and 6 cm from its own
  steel housing. From 88 m (b23 s 392.6) the glass's GGX image of the light and the housing's lit chamfer are millimetre-sized
  spots of extreme HDR radiance (E = 1.4 / 0.0225 = 62 on the glass, about 200 on the housing top), far below a pixel: the
  MSAA samples catch or miss them as the walker moves about 1 px/s, and the bloom turns a caught one into a white orb for the
  6-8 frames it dwells on a sample (the critic's 0.3-0.4 s events, the left lamp one frame after the right). Not the wet
  registry (the step lights' sources have weight 0 at s 392.6: window s - 32 .. s + 72) and not a Forward+ light cap (Forward+
  ignores the per-object limit 4; fewer than 256 visible lights).
- Fix C13: the 14 step lights are spot lights aimed 45 deg down and inward (outer 100, inner 60 deg; EdgeDims.BalSpot*): the
  balustrade glass (>= 95 deg off the axis), the shoe and the housing (>= 92 deg) lie outside the cone; the floor under the
  light sits in the soft edge (E <= 1.4 / 0.0256 x 0.29 = 16). Position, colour, 1.4 and range 10 unchanged; the wet sources
  unchanged. Test StepLights_AreSpots_TheGlassShoeAndHousingLieOutsideTheCone.
- Fix C05 (KS10 girder bar): the glass edge is frameless: the 0.14 x 0.14 girder becomes a 0.06 m stainless channel under the
  shoe (Tongue_railChannel: the balustrade's base, a rail part), the cantilever ribs become glass fins 0.04 m (Tongue_glassFins,
  NocGlass, not occluders), the nosing is a deck part (Tongue_deckNosing) and the ledge LED "Tongue_deckLED". Pose unchanged.
  Test GlassEdge_IsFrameless_ChannelShallow_FinsGlass.
- Fix C03 (portal "HUD reticle"): ReliefKit.TubePortal and GulfPortal draw 8 separate collar lamps (one of 32 lip segments
  each, at 22.5 + 45 k deg: none on the axes), the rest of the lip concrete; NeonCyan x EmissionTube x 0.45 (GulfPortal.LampGain)
  in both materials (Relief_portalRing, Beat03_portalRing); the keystone's west face (panel around the collar + the strips above
  and below) is face B's NocFacade skin (section mode, face B's id and u = ArcLength(1, z), as the lintel above), so the portal
  sits in the window field instead of on a dark concrete square framing the ring. A lit tunnel mouth was tried in code and
  dropped before the build: TR's lining (r 5.25, NocTrain, black beyond 2.5 m) covers the r 5.3 tunnel by design (D-039).
  NOTE BV: CanyonBridge.cs calls ReliefKit.TubePortal and shares Relief_portalRing, so the canyon bridge's east tube portal gets
  the same 8 lamps at 0.45 (no change in BV's file); KS05 / beat02 stills to be checked (below).
  Tests PortalLamps_Segmented_NoContinuousRing, ReliefTests.TubePortals_SegmentedLamps_KeystoneInFaceBSkin.
- Fix C43 (KS07 dead plaza): face B's two wet wall planes start at s 400 (fade 388-400) and face A's at s 424 (fade 412-424;
  at s 400-424 face A is behind the walker); G1's boulevard walls fade over 400-412: never more than 4 walls.
  GLOBAL 02:15: beat-3 wet floor only (s 388-424): face B's window macro mirrored on the tongue plaza from s 388 (KS07 at s 402);
  face A's mirror starts 24 m later (seen only looking back). Test WetWalls_FaceBFromThePlaza_NeverOverTheCap.
- Fix C30 (single-pixel heads, KS13 p99): NocStreak kind-4 heads and kind-5 lamps are drawn at least 3 px (LIGHT_MIN_PX, energy x
  (true / drawn)^2 as before), and every minimum is in output pixels (x _NocPxScale, unset = 1): REQUEST QA (CaptureHarness) to
  set it under -capture-ss 2 (shared_change_requests).
- 02:16 fastcheck OK (217 / 287); AB-b11 (build) + AB-t11 (Beat03Edge / Abyss / Relief / Q1 / FacadeSkin / WetFloor tests) queued.
- 02:22 AB-b11 SUCCESS (0 errors, 53.8 s; NocStreak compiled). 02:26 AB-t11 59 / 59 PASS (QA/reports/AB-t11.xml: Beat03Edge,
  Abyss, Relief, Q1Integration, FacadeSkin, WetFloor). KS10 on the built world: sky / deck / rail / near / drop 4.2 / 35.5 / 6.5 /
  10.1 / 43.8 % (AB-final of the fifth start: 4.2 / 34.2 / 4.7 / 15.4 / 41.5); near = City/Beat04_skybridge/gateA/block x884 +
  gateA/frame x48 only (TR's gate block A, kept by the ruling); none of my parts is near structure any more (ledgeFrame x467 and
  ledgeLED x31 before).
- AB_r11 (keyshots1080 + beat stills, looked at KS10, KS13, KS14 crop, KS07): KS10 rib gone, the remaining band at the edge is
  the balustrade shoe (0.11) over the 0.06 channel (4K crop: about 100 px of 2160 at the frame centre); the KS14 portal at 0.45
  pure NeonCyan still read as a dashed cyan circle -> r12 below. KS13 p99 74.9 (p4_01 83.9): pixels >= 60 fell from 208 k to
  111 k in the haze rows (AT's NocAtmosphere / NocSky edits in this copy darken the abyss haze, and DR's shadow (D-040 (3)) darkens
  the -520 river under the strip); the 3 px heads move only the heads' own few thousand pixels.
- C13 verification 1 (AB_c13a, AB_c13b: 142 pose stills, s 391-394 every 0.025 m = 60 fps at 1.5 m/s, and s 402.4-403.4 every
  0.05 m; rain on, then -rain 0): the step-light blocks (x 864-1056, the lamps at x 296 project to +-54 px) had 0 pops; 36 pops
  (d up to 10.4 on 32 px blocks, 3.5 on 64 px) at x 768-830 / 1088-1150, y 512-544 = the plaza sodium lamps at x 272 / 282
  (lateral 13.12, 68 m: +-160 px): the same mechanism (the realtime point light 0.12 m under the lamp head lights the emissive
  head 7 cm away). The critic's b23 list has these too (px (1152, 480), (736, 480), (768, 480), (1088, 480)).
- r12 (02:33): the 10 realtime plaza lamps are spots aimed down (outer 130, inner 80 deg, like BV's boulevard lamps; the head lies
  >= 100 deg off the axis); the collar lamps NeonCyan half-way to WinNeutral (LampWhite 0.5) x EmissionTube x 0.35
  (GulfPortal.LampLinear, both ring materials). AB-b12 02:34 SUCCESS (23.8 s, 0 errors).
- C13 verification 2 (AB_c13c, same 142 poses, -rain 0, the r12 build): 0 pops on 32 px and 64 px blocks over the whole window
  (max one-pair block step 1.61 / 0.48 levels at s 391-394, 2.25 / 0.72 at s 402.4-403.4; the critic's b23 event was 13.5).
  QA/reports/AB_c13c_blockpops.json, crops QA/reports/AB_c13c_lamps.png; frames deleted (375 MB).
- AB_r12 (keyshots1080, beat stills, KS07 / KS10 / KS13 / KS14 / KS15 at 1610 and 219, and AB_r12_4k: KS07 / KS10 / KS13 / KS14 4K):
  shot      mean  p99    (p4_01 p99)
  KS07      32.7  122.4  (122.6)   plaza y 600-1080 mean 29.8 (29.2)
  KS08      38.8  132.5
  KS09      33.3  157.6  (166.5)
  KS10      33.1  184.6  (188.1)   1610 189.6, 219 176.4
  KS11      30.5  124.9  (124.3)
  KS12      32.1  180.3  (180.6)
  KS13      33.5   74.9  (83.9)    1610 77.4, 219 74.2, 4K 76.2
  KS14      31.4  107.7  (107.9)
  C03 looked at (crops: KS14 1080 / 4K, beat03_edge_end, KS07, KS15, beat04_skybridge_mid upper and lower, KS04): the portal is 8
  small pale dashes on a dark disc inside face B's window grid; in KS04, KS07 and beat03_edge_end it is a few faint dots, no ring;
  the dark concrete square around it is gone. CanyonBridge's portal (BV) uses the same kit and material: beat02_boulevard_end
  p99 116.4 (p4_01 117.4), no new defect seen in KS05 / beat02_boulevard_end.
- 02:44 smoke AB-smoke (-autotest -brisk -autotest-until 880 -beat 3, the r12 build): AUTOTEST PASS, 0 coverage misses (48 204
  rays), beats 3 / 4 / 5 entered at s 400 / 560 / 880, alloc mean 28.8 B / frame (excess 0), worst 4 096 B
  (Logs/player-AB-smoke.log). Triangles: Beat03_Edge 98 184 (115 meshes, 0 validation failures), Relief 155 397.
- GLOBAL 02:33: beat 3 only: the 14 step lights and the 10 realtime plaza lamps are spot lights (pier floor pools inward of
  the balustrade; plaza pools within the 130 deg cone): KS07 plaza mean unchanged (29.8 both builds), KS08 mean 39.3 -> 38.8,
  p99 138.6 -> 132.5 (>= 120 kept), KS09 p99 157.6.
- 03:03 C13 verification 3 (AB_c13d, 98 poses at 0.025 m over s 407-408.2 and 414.2-415.4, the critic's later events, -rain 0):
  0 pops on 32 / 64 px blocks, max block step 1.79 / 0.70 (QA/reports/AB_c13d_blockpops.json); frames deleted.
- image_metrics AB_r12 (QA/reports/image_metrics_AB_r12.md): KS09 bright NaN-like 9 -> 5: the cross-bridge lamps (998, 480),
  (1023, 478) of the critique are gone (kind 5 now >= 3 px); the remaining ones ((1126, 55), (1062, 112), (1181, 271) in KS09,
  (1215, 437) = 216 on 55 in KS04) are exact single pixels with no spread to the neighbours, so they are not NocStreak quads (a
  3 px soft quad spreads >= 20 % into its neighbours): NOTE DR / FX: likely point lights of another shader (the hull's
  NocDreadLight in the KS04 / KS09 sky?), not mine.
- lookv2_check AB_r12: KS04-KS12 PASS; KS13 FAIL p99 74.9 (see C30), KS14 FAIL p99 107.7 (TR's C-item, p4_01 107.9), KS15 FAIL
  dark cast R - G -4.77 (GD's canopy; D-040 (5) window [-8, +4]).
- 03:03 AB-final (the full EditMode suite) started.
- 03:16 AB-final (the full EditMode suite on the r12 tree, QA/reports/AB-final.xml): 476 tests, 472 passed, 3 failed, 1 skipped
  (AbyssTests.KS10_CompositionSearch, Explicit). Mine all green: Beat03EdgeTests 14 / 14 (4 new), AbyssTests 7 (+1 explicit),
  ReliefTests 8 / 8 (1 new), Q1IntegrationTests 3 / 3, VisibilityAuditTests 8 / 8, FacadeSkinTests 9 / 9, WetFloorTests 9 / 9,
  MaterialTests 15 / 15. KS10 on the built world 4.2 / 35.5 / 6.5 / 10.1 / 43.8 %. The 3 failures are AT's in-flight atmosphere
  work in this copy, none touches my files: AtmosphereAbyssTests.Abyss_NadirWarmAndStructured_KS13 (nadir mean 69.4 vs 58-68),
  AtmosphereFixTests.Flash_EnvelopeScheduleAmplitudeAndHullOutline (decay 1.0 vs <= 0.998), AtmosphereFixTests.SlitLift_
  OnlyOverTheAlley (0.0168 vs < 0.002).
REQUEST QA: Assets/_Game/Scripts/Runtime/QA/CaptureHarness.cs (C30, FX's request): NocStreak's size minima are output pixels since
  the fix wave and read the global _NocPxScale (render pixels per output pixel; unset = 1). Set it around every supersampled
  render, so the -capture-ss 2 reference draws the same 3 px blobs as the 1x render:
      static readonly int s_pxScale = Shader.PropertyToID("_NocPxScale");
      // before SubmitRenderRequest of a still / sequence frame:
      Shader.SetGlobalFloat(s_pxScale, m_ss);
      // after the frame's readback is queued, and in the teardown:
      Shader.SetGlobalFloat(s_pxScale, 1f);
NOTE BV: CanyonBridge.cs needs no change: its east tube portal uses ReliefKit.TubePortal and Relief_portalRing, so it carries the
  same 8 collar lamps (GulfPortal.LampLinear) since r12; beat02_boulevard_end p99 116.4 (p4_01 117.4) and KS05 196.5 (196.5).
NOTE TR: KS10's near structure is now only gate block A (block x884 + frame x48 = 10.1 % of the 128 x 72 rays); <= 10 % (C05)
  needs 12 rays of it, so it stays with the lead's ruling on the block.
NOTE AT / DR / lead (C30): KS13 p99 74.9 / 77.4 / 74.2 / 76.2 at 16:9 / 16:10 / 21:9 / 4K (rule 90, p4_01 83.9): in this copy the
  abyss haze is darker (pixels >= 60: 208 k -> 111 k between p4_01 and AB_r11, the haze rows of the strip) and DR's shadow (D-040
  (3)) darkens the -520 river under the strip; the heads I draw are a few thousand pixels, the rule needs about 6 k more pixels
  >= 90 than the frame has. Proposal: re-measure KS13 after AT's final haze; if it stays < 90 with the shadow, the lead rules
  whether D-037 (6)'s 90 holds while the shadow passes (14.5.4 (3)'s centre median 47.8 passes).
- Files of mine changed in this wave (uncommitted): Beats/Beat03_Edge/Beat03_Edge.cs, EdgeDims.cs, EdgeMesh.cs, GulfPortal.cs,
  TonguePier.cs; Beats/Relief/NotchGate.cs, ReliefBuilder.cs, ReliefKit.cs; Shaders/NocStreak.shader; Tests/EditMode/Beats/
  Beat03EdgeTests.cs, Tests/EditMode/ReliefTests.cs; QA/critique-AB.md; docs/logs/AB.md. New reports: QA/reports/AB-t11.xml,
  AB-final.xml, AB_c13c_blockpops.json, AB_c13c_lamps.png, AB_c13d_blockpops.json, image_metrics_AB_r12.*; rewritten by the tests:
  AB_ks10_search.md, q1_floor_probe.txt, autotest_brisk_from400_until880.json. Captures kept: QA/captures/AB_r12 (+ _contact),
  AB_r12x, AB_r12_4k (114 MB); AB_r11 and the C13 pose frames deleted.
AB DONE 03:19

## ARK (D-043, 2026-11-21 evening; order Logs/ark/ARK.md "The traffic lasers"; copy .worktrees\abyss with DR / AT)
- 20:16 start. Read ARK.md (AB row and the traffic rule), this log, AbyssTraffic / NocTraffic / NocStreak / AbyssTests.
  The walk of beats 3-4 is x 215.3-695.3 on z 195 (x = s - 184.7), eye y 1.7; the skybridge spans y -14..+16 (piers, gate frames).
  Who passes too close: layer (540, 30) runs 28 m over the deck (KS11 in QA/final shows its heads / tails as 25-30 px blobs right
  over the bridge: the "lasers"), (380, 180) 178 m, (350, -120) 122 m; (600, 300) lies inside the Ark (x 295-735, y 300-600).
  Not named in ARK.md but under the same rule: the BOULEVARD sky lanes (NocTraffic.BoulevardLanes, mine) run from x -4 000 across
  the rift into face B's notch at y 140-230, 36-52 m beside the axis (147 m from the walker), and the two skyway lanes (z 195,
  y 320 east / 400 west) cross the rift inside the Ark's volume (the ship would sail through them).
- Environment (blocked every Unity call in this copy): the lead's prewarm-ark RebuildAll (20:16) and my AB-x01 (20:22) failed with
  every runtime ScriptableObject / renderer feature "No script asset" and wrote PC_Renderer.asset, MaterialLibrary.asset,
  Main.unity, NocturnePalette.asset with m_Script fileID 0. This copy's Library (mirrored from integ's after its 05:35 build)
  lacked integ's Library/DataStore/UDSData_280.bin (written by that build). 20:31 AB-remirror (scratchpad ab/remirror.ps1: both
  copies' queue locks held, no Unity running): robocopy /MIR integ\Library -> abyss\Library (665 files), the four assets back to
  HEAD's bytes (git show; nothing else touched). 20:41 AT's CompileCheck loaded PC_Renderer without the error.
- 20:46 an orphan `tail -F Logs/unity-queue.log` (pid 37304, a monitor for "AT-b01] exit", not mine) held the queue log open:
  every queue call failed at its first Add-Content (my AB-b01 twice). Killed it, noted in unity-queue.log.
- Design (D-043, my part):
  - Layers: (600, 300) / (380, 180) / (540, 30) / (350, -120) -> (300, -225) / (720, -240) / (640, -340) / (360, -440); the deep
    four (560, -520) [the KS09-KS13 subject], (470, -880), (650, -1 100), (420, -1 400) unchanged. All eight lie below the walker
    (>= 227 m from the walk, LaneClearance), out of the Ark, 60 m from the faces, 15 m from every cross bridge and tube, clear of
    DR's shadow body (x 470 +-153, y -640..-760) and the cabin shafts. The new four come in pairs along the faces and step in
    toward the axis as they go down: from the bridge the eight rivers fan out of the haze as a funnel into the deep (spoke angles
    from KS12's camera -45 / -64 / -81 / -82 / -90 / -92 / -105 / -128 deg), the first pair just above the y -250 veil.
  - Boulevard sky lanes (mine, NocTraffic): the skyway lanes y 320 / 400 -> 275 / 290 (out of the Ark: it would have sailed
    through them); over the rift (camera x 215.3-695.3 on the axis, eased over the 24 m before s 400 and after s 880) NocStreak's
    NocWalkClear draws no boulevard vehicle within 300 m of the camera (full at 400 m): the lanes still converge into the canyon
    and into face B's notch, but nothing passes over the walker, and the sky under the Ark stays clear. When the walker reaches
    the garden the overhead traffic fades back in (the city returns, as the order's last line asks).
  - D-032 (5) light content lost with the eye-level layers (release p99 KS07 122.4 / KS08 132.5 / KS11 121.1 -> 111.7 / 109.6 /
    110.5 in AB_a01): air crossings (CrossBridge.AirZ / AirY: fliers along x through a lit portal in each face, the bridge-lane
    layout, ramps behind the face planes, 305 / 465 m north of the walk) and more corridors on the faces above the deck
    (GulfRibBuilder PGalleryUp / PGallery2Up / PGallery3Up), which also give the Ark's dark mass lit walls to stand against.
  - QA switch NOC_TRAFFIC_OFF=1 (NocTraffic.Off): vehicle batches and layer glows are not registered: an A/B pair isolates
    exactly the traffic's light (the streak proof below).
REQUEST FX (integ, Assets/_Game/Scripts/Runtime/QA/VisibilitySubjects.cs; ARK D-043 moved the subjects' traffic):
  - KS10: the y -120 layer is gone (now x 300, y -225). Replace
      S(k, "layer_120", VisRule.Part, Line(new D3(350.0, -120.0, 300.0), new D3(350.0, -120.0, 3000.0), 28), -0.34, -0.33, 0.32, 0.70,
        "layer y -120 (x 350) x -0.34..-0.33, y +0.32..+0.70");
    with
      S(k, "layer_225", VisRule.Part, Line(new D3(300.0, -225.0, 300.0), new D3(300.0, -225.0, 3000.0), 28), N, N, N, N,
        "layer y -225 (x 300; ARK D-043: the y -120 layer moved 200 m below the walk)");
    and in the renderer map
      if (name.StartsWith("layer_120", StringComparison.Ordinal)) return new[] { "TrafficLayer_350_-120" };
    with
      if (name.StartsWith("layer_225", StringComparison.Ordinal)) return new[] { "TrafficLayer_300_-225" };
  - KS05: the skyway lanes run at y 275 / 290 now (NocTraffic.BoulevardLanes; 320 / 400 lay inside the Ark): in "lane_skyway_320"
    change both D3 y values 320.0 -> 275.0 and the name to "lane_skyway_275" (the window N, N, -0.08, -0.08 still holds: the
    y 275 line spans y -0.89..+1.16 of the KS05 frame).
NOTE AU (TrafficWash, integ): the "near pass-bys" (a vehicle passing overhead, MeanInterval 9 / 8 s in beats 3 / 4) have no
  visual source any more on the pier and the bridge: no vehicle is drawn within 300 m of the walker over the rift (beats 3-4),
  the nearest traffic is the air crossing 305 m north and the rivers 227 m and more below. Please gate the pass-by events out
  for s 400-880 (the wash itself, distant traffic, can stay; it might also duck under the Ark).
- Builds (Builds\AB unless noted, all 0 errors): AB-b01 20:49, AB-b02 20:55, AB-b03 21:01, AB-b04 21:08 (Builds\ABi), AB-b05 21:14
  (the final: Logs/AB-b05.log "result=Succeeded errors=0"). Player log (AB-a05): Beat03_Edge 121 meshes (0 validation failures),
  98 484 triangles (fix wave 98 184), 24 realtime lights; 21 984 layer + 1 267 bridge + 432 air-crossing vehicles.
- Key shots 1080p, p99 (luma 8-bit Rec.709; scratchpad ab/lstats.py), release QA/final -> a01 (layers moved only) -> a05 (final):
    KS07 122.4 -> 111.7 -> 150.8     KS08 132.5 -> 109.6 -> 133.3     KS09 160.0 -> 153.6 -> 197.7     KS10 176.6 -> 190.8 -> 192.2
    KS11 121.1 -> 110.5 -> 122.2     KS12 127.3 -> 127.9 -> 129.6     KS13 70.9 -> 73.1 -> 73.1 (rule 90: the open C30 item, unchanged)
    KS14 143.6 -> 143.5 -> 143.5. D-032 (5) p99 >= 120 at KS07-KS12: 6 / 6 (QA/captures/AB_a05).
  Looked at (full size): a01 KS11 (no blobs over the bridge), KS08, KS12; a02 KS07, KS08, KS11; a03 KS11 crossing crop x2; a04 KS09;
  a05 KS12, KS10 (+ crop of face B's dashes vs release: the same gallery / lip lines, plus the new (720, -240) river).
- The user's "lasers", before / after: QA/reports/AB_ark_ks11_before_after.png (release exe vs AB-b03, seq_traffic_ks11_train
  frames 0 / 120 / 239 / 360 / 479, rows 120-260 of 540): before, the y 30 layer's heads and tails are 20-30 px blobs right over the
  bridge, growing as the walk nears them; after, only distant dots (the air crossings converging into their portals).
- 60 fps proof (ARK.md): A/B pairs, A = traffic, B = NOC_TRAFFIC_OFF=1, same frames (virtual time, -rain 0 -freeze-dither); the
  traffic's light = A - B; vehicle cores = components of (A - B) >= 40 codes split at 0.85 of their own peak (so separate vehicles
  merged by distance stay separate); length = long side of the min-area rectangle, in 1080p pixels (scratchpad ab/abcore.py,
  ab/abwatch.py):
    seq_traffic_ks11_train (s 590-610, KS11 gaze, 480 frames, 960x540, AB-b03, no split): longest 37.2 px, 0 >= 80 px
      (QA/reports/AB_ark_streaks_ks11.json);
    seq_cin60_s0640 (s 640-730, the recorded cinematic camera incl. its look-ups and the -75 deg dive, 2 160 frames, 960x540,
      AB-b05): longest 64.3 px, p99 of the frame maxima 44.0, median 18.8, 0 >= 80 px (QA/reports/AB_ark_streaks_cin60_s0640.json);
    (a 480x270 pass of s 640-730 and 860-900 was too coarse - 3 px minimum quads of a far row merge at 4 px per 1080 px - its
    report stays as QA/reports/AB_ark_streaks_cin60_270p.json: max 80.0 with the split, one far crossing row.)
  The analytic bound (AbyssTests.Streaks_ShortFromTheWalk_60fps_ARK, every 1 m of s 400-880, +-2.5 m off the axis, vFOV 50 at
  1080p): nothing drawn within 200 m; longest possible streak layers 15.2 px (nearest 226.7 m, <= 2.1 px / frame at 60 fps), air
  9.0, boulevard 6.7 (nearest drawn 300 m), bridges 3.0.
- Tests: AB-t01 61 / 61 (QA/reports/AB-t01.xml). AB-final, the full EditMode suite on the b05 tree (QA/reports/AB-final.xml): 521,
  518 passed, 2 failed, 1 skipped (AbyssTests.KS10_CompositionSearch, Explicit). Mine all green. The 2 failures are AU's
  AudioDreadTests (Cues_AreKeyedToTheirSAndTimes "a glow outlines the hull (abeam at s 596)", Hull_CrossesAlongZAt...
  "-660 + 0.9 tau^2 ... was 5778.37"): they read DR's DreadCourse, which DR is rewriting for the Ark in this copy (owners AU / DR).
- Smoke AB-smoke (AB-b05, -autotest -brisk -autotest-until 880 -beat 3; Logs/player-AB-smoke.log): AUTOTEST PASS, beats 3 / 4 / 5
  entered at s 400 / 560 / 880, 0 coverage misses (48 204 rays), errors 0, alloc mean 32.5 B / frame (excess 0.5), worst 4 096 B.
- More of the 60 fps proof (AB-b05, 960x540, watch mode: each pair analysed then deleted): seq_cin60_s0530 (s 530-590, 1 440
  frames): longest core 42.4 px, 0 >= 80 (QA/reports/AB_ark_streaks_cin60_s0530.json); seq_cin60_s0860 (s 860-900, 960 frames):
  longest 2.0 px - no traffic at all near the walk there (QA/reports/AB_ark_streaks_cin60_s0860.json). In all, 5 040 frames at
  60 fps over s 530-610, 640-730 and 860-900: 0 vehicle cores of 80 px or more, the longest 64.3 px (a far chain).
- Final look (AB-b05, QA/captures/AB_p05: six poses at 1080p): s 640 looking up north (60 deg) and east (45 deg): the Ark's belly with
  its lit rows fills the sky, no vehicle between it and the walker; s 640 looking back west into the canyon: no traffic overhead
  (the boulevard lanes start beyond 300-400 m); s 820 east into gate B and s 900 up into the garden crowns: calm; s 400 east-up:
  the air crossings converge into face B's portals at the horizon, face B's corridors lit.
- Files (mine, uncommitted): Beats/Beat03_Edge/AbyssTraffic.cs, CrossBridge.cs, Beat03_Edge.cs (log line);
  Beats/Relief/GulfRibBuilder.cs; Scripts/Runtime/City/NocTraffic.cs; Shaders/NocStreak.shader;
  Scripts/Tests/EditMode/Beats/AbyssTests.cs; docs/logs/AB.md; Logs/AB_progress.md. New reports: QA/reports/AB-t01.xml, AB-final.xml,
  AB_ark_before_ks11_strip.png, AB_ark_ks11_before_after.png, AB_ark_streaks_ks11.json, AB_ark_streaks_cin60_s0530.json,
  AB_ark_streaks_cin60_s0640.json, AB_ark_streaks_cin60_s0860.json, AB_ark_streaks_cin60_270p.json, player_AB-smoke.json / .md;
  rewritten by the tests / smoke: visibility.json / .md, autotest_brisk_from400_until880.json. Captures kept: QA/captures/AB_a05
  (final key shots), AB_p05 (final poses); every other AB capture deleted; Builds\ABi deleted.
- Repairs outside my files (this copy's environment, see above): Library re-mirrored from integ; PC_Renderer.asset,
  MaterialLibrary.asset, Main.unity, NocturnePalette.asset back to HEAD's bytes; an orphan tail on the queue log killed.
AB ARK DONE 21:32
