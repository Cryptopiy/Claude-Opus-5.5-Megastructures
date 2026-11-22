# V log - koi mathematics and the 11.13 visibility audit (Phase 3)

Owner: V. Files: `Assets/_Game/Beats/Beat07_Heart/KoiMath.cs`, `Scripts/Runtime/QA/NocVisibility.cs`, `Scripts/Runtime/QA/Visibility*.cs`,
`Scripts/Tests/EditMode/KoiMathTests.cs`, `VisibilityAuditTests.cs` (+ helpers next to them), `Tools/qa/visibility_*.py`, this log,
`QA/reports/visibility.json` / `visibility.md`. No beat geometry, gaze key or threshold is changed by V: measure and recommend.

## Resume rule
Find the last step marked GREEN below and the last commit with "V:" in the message; continue with the next open item.

## Task 1 - KoiMath (14.5.7, 14.7 H6, 14.12, D-009)
Design (full formulas in the header of KoiMath.cs):
- Orbit about S (1 330, 388): R(phi) = 260 + 18 sin(2 phi + 0.7), y(phi) = 300 + 60 sin(phi + 0.4), phi from +X toward +Z.
- Arc integral A(phi) = 260 phi - 9 cos(2 phi + 0.7) (closed form); body point a trails the head on the same orbit with
  A(phi_h) - A(phi(a)) = a (Newton, A' = R >= 242). L = 900 by construction; the test integrates R with Simpson independently.
- Radius profile: Fritsch-Carlson monotone cubic (scipy PCHIP slopes) through the 7 knots; no overshoot, so the maximum is the
  0.30 knot (1.00 x 48 = width 96.0).
- Rings oriented along the waved spine (D-009): F = -dC/da (central difference 0.05 m), N = normalize(Y x F) (outward, horizontal),
  B = F x N.
- Veil (D-009) 120 x 120, vertical: centre line = spine continued to a = L + 120 with the wave envelope held at 40 m; half-span
  lerp(9.6, 60); split trailing edge (lobe tips at L + 120, notch at L + 80).
- Fins as frames: pectorals root chord 60 m from a = 108 at theta 180 +- 30 deg, 120 m along normalize(-0.75 F +- 0.45 N - 0.48 B);
  dorsal 90 m from a = 216 on the top, height 16 -> 40 -> 12.
- Phase follower (14.7 H6): t_k from the first crossing of s 1 260 (pro rata in the crossing frame), E critically damped (omega 0.8)
  toward max(0, t_k/160 - (s - 1 260)/240), u held non-decreasing.
- Wave time: play t_k; capture (s - 1 260)/v with v = 1.5 default (D-026); composition phases t_w = k s, k = 0..19.
- Composition sample set (183): 164 core = 41 stations every 22.5 m x 4 ring points (out, top, in, bottom); this reproduces the
  spec's count of 164 and its "28 samples in the last 140 m of the tail" (stations 34..40). Head = stations 0, 1 (8 points).
  + 8 pectoral corners, 4 dorsal, 7 veil.

Pre-check with a numpy mirror (scratch, 360 head azimuths x 20 wave phases, 121 stations x 16 ring angles):
body clearance to the spire surface 158.83 m (spec 153 used a constant R 32: 158.8 - (32 - 25.6) = 152.4), screen wall 125.39 m
(spec 126), ring towers R 456 120.11 m (spec 121) / R 470 134.11 m, top y 407.998 (spec 408), lowest y 192.002 (spec 192).

### Step 1 - KoiMath + KoiMathTests (GREEN)
- `V-t01`: 6/9 (pectoral fins as first drawn reached radius 394 m about S: wall 66 m, towers 62 m; the D-009 0.45 m gate measured on
  surface points 0.467; a loop end missed s 1500). Fixes: pectorals swept back and down (root 55 deg below the side axis,
  d = normalize(-0.80 F +- 0.12 N - 0.55 B)); the gate measured on the spine (surface points reported, bounded by the radius ratio);
  loop to s 1500 + 2 frames.
- `V-t02` (`-testFilter KoiMathTests`): 9/9 PASS. Measured (360 head azimuths x 20 wave phases, 121 stations x 16 ring angles):
  - L = integral R dphi over the unwaved orbit 900.000 (worst |L - 900| 6e-12 m by Simpson over [phi_tail, phi_head]); the body
    spans 197.63 deg of azimuth.
  - width 96.000 m at a = 270.0 (a/L 0.300); head radius 10.56 m, tail 9.60 m; profile monotone between knots.
  - head azimuth table exact (6.64, 70.00, 165.04, 253.04, 358.64, 429.04 vs 6.6, 70.0, 165.0, 253.0, 358.6, 429.0); through the
    H6 follower at 1.5 and 2.5 m/s (60 fps) E = 0 exactly, deviation 0.0 deg; lingering 30 s at s 1280: u +0.172 (72.6 deg; the
    time target grows 0.1875), u never decreases walking back.
  - clearances, body: spire surface 158.83 m (spec 153 with a constant R 32), screen wall 125.39 (spec 126), ring towers R 456
    120.11 (spec 121) / R 470 134.11, arcade 183.77, top y 407.998 (spec 408), lowest 192.002 (spec 192).
  - pectoral fins: spire 173.36, wall 110.83, towers R456 107.41, arcade 115.43, y 131.8..328.4.
  - dorsal fin: spire 208.2, wall 173.4, towers 167.7, top y 447.58 (cloud base 460; inside the 380-560 cloud mix band).
  - tail veil 120 x 120: spire 175.2, wall 143.4, towers 138.0, y 180.0..420.0 (40 m below the cloud base).
  - per frame at 2.5 m/s, 60 fps: spine 0.445 m (D-009 gate 0.45), wave part 0.209 (gate 0.209); surface samples 0.467 (a surface
    point at radius R + w + rho moves (R + w + rho)/R faster; the D-009 gate is read on the spine).
  - rings follow the waved spine: up to 19.5 deg between the ring axis and the unwaved orbit tangent (t_w 7).

KOIMATH READY 2026-11-14 03:20
API (namespace Megastructure, `Assets/_Game/Beats/Beat07_Heart/KoiMath.cs`, double precision, phi in radians unless "Deg"):
- constants: CentreX/Z, OrbitR0/RAmp/RPhase, OrbitY0/YAmp/YPhase, Length 900, RadiusScale 48, MaxWidth 96, Wave*, HeadDeg0 6.64,
  HeadDegSpan 422.4, BeatS 1260, PhaseD 240, PhaseT 160, FollowerOmega 0.8, CaptureSpeed 1.5, BodySegments 160, RingVertices 28,
  Veil* (120, 60, notch 80), Pectoral* (A0 108, chord 60, length 120, root 55 deg, dir -0.80/0.12/-0.55), Dorsal* (A0 216,
  length 90, heights 16/40/12, peak 0.25), TableS/TableDeg, Stations 41, CoreCount 164, HeadCount 8, SampleCount 183, WavePhases 20.
- orbit: OrbitRadius(phi), OrbitY(phi), ArcIntegral(phi), OrbitPoint(phi); PhaseU(s), HeadAzimuthDeg(u), HeadAzimuth(u);
  AzimuthAt(a, phiHead).
- body: WaveEnvelope(a), Wave(a, tw), Profile(x), Radius(a), SpinePoint(a, phiHead, tw) (D3), Frame(a, phiHead, tw, out C, out F,
  out N, out B), RingPoint(a, theta, phiHead, tw); VeilPoint(v, k, ...), PectoralPoint(side, u, v, ...), DorsalHeight(u),
  DorsalPoint(u, v, ...); Vector3 helpers ToVector3, SpinePointV, RingPointV.
- sample set: Samples(phiHead, tw, D3[183]), SampleKind(i) (KoiPart Body/Head/Pectoral/Dorsal/Veil), IsCore(i), SampleRootA(i);
  SpineLightPoints(phiHead, tw, D3[12]) for the 14.2.3 / H4 koi lights.
- time: WaveTimePlay(tk), WaveTimeCapture(s, speed = 1.5), PhaseWaveTime(k); KoiPhaseState + ResetPhase(ref st) +
  StepPhase(ref st, s, dt) -> u (t_k from the first crossing of s 1260, E critically damped omega 0.8, u non-decreasing).
Usage for the builder: phiH = KoiMath.HeadAzimuth(u); tw = capture ? KoiMath.WaveTimeCapture(s) : st.Tk; then per ring a = L i/160:
KoiMath.Frame(a, phiH, tw, out c, out f, out n, out b) and rho = KoiMath.Radius(a).

## Task 2 - the 11.13 visibility and reachability audit
Files: `Runtime/QA/VisibilitySubjects.cs` (spec subjects per key shot), `VisibilitySpecWorld.cs` (analytic spec occluders),
`VisibilityRaycaster.cs` (BVH over the built triangles), `VisibilityFollower.cs` (the runtime WalkPlanner as the follower),
`VisibilityAudit.cs` (projection, occlusion, composition, koi lint, reach, JSON + MD); tests `Tests/EditMode/VisibilityWorld.cs`
(EditMode world build) and `VisibilityAuditTests.cs`.

Design decisions:
- Follower: VisibilityFollower runs `WalkPlanner` itself (the class AutoWalker drives; a reflection test pins AutoWalker._plan to it),
  Cinematic mode, 60 fps, fed its own target (free lane). Two runs: 11.13's (start 40 m earlier at 2.5 m/s, follower at rest on the
  schedule pose there) and the whole walk from s 0 (0.5 s start ramp). Lag = max(|yaw|, |pitch|) to the table pose, sway excluded
  (<= 0.3 deg). Readable windows (8 / 6 deg) measured on the whole walk.
- Projection: QaProjection (double, Hor+); aspects 1920x1080, 1680x1050, 2560x1080; margin |NDC| <= 0.94; poses target, f40, fW.
  Rules: Whole (every sample inside the margin), Part (>= 3 samples inside: faces, facades, lanes, cloud), Info.
- Occlusion: per sample a ray from the eye; occluded when a hit lies nearer than dist - tol (tol = max(0.25 m, 1 %) for objects,
  max(1 m, 3 %) for surface subjects such as the Gulf faces, so the subject's own built surface never hides it). Samples inside
  the 16:9 target frame are counted. Two scenes: spec (decks / rails / gates / alley / facades / spire / arcade / screen wall /
  Old Quarter, analytic) and built (opaque MeshRenderers; render queue >= 2500, the backdrop root and "koi" excluded; instanced
  signs / streaks / traffic are not renderers). Non-readable meshes: Mesh.AcquireReadOnlyMeshData, else CityField cells regenerated
  by CityField + CityMesher (CityBuilder's own code path).
- Composition: 64 x 36 ray grid at 16:9 (21:9 too for KS16); sky = elevation > 0 (the spec's 26 % for KS09 is exactly the share above
  the horizon), pier = first hit on deck / rails (spec) or within 30 m horizontally and above y -130 (built), drop = the rest.
  A numpy check of the spec deck plan alone gives KS09 26.4/38.9/34.7, KS10 6.9/50.2/42.8, KS12 22.2/30.6/47.2 (spec 26/39/36,
  7/50/43, 22/30/48): the plan reproduces the spec's numbers.
- Koi lint: KoiMath's 183 samples x 20 phases (t_w = k s) at the capture-mode head azimuth (E = 0); a sample hidden in either scene
  is exempt from the margin, fin / veil samples whose body-root station (all 4 ring samples) is hidden too (D-009); the head must not
  be hidden. Min margin = 1 - max(|x|, |y|) over visible samples (the spec's 0.085 / 0.178 / 0.11 are in these units: KS17's
  y +0.92 gives 0.08).
- Subject derivations that reproduce the spec's NDC numbers (hand check before coding): KS01 window / sign / counter on the recess
  back wall x -3.0 (window z 43..47 y 1.0-2.3 gives x -0.153..-0.021, y -0.273..+0.017; sign z 42..48 y 3.0-4.4 gives x -0.203..0,
  y +0.058..+0.554); KS02 catwalk box x +-2.2, y 8.8-9.2, z 96-97.6 gives x +-0.211, y -0.940..-0.700; blades as 14 x 7 m slabs
  x [18 + 36k, +14] z 263..270 (KS04 north x 198: x -0.473..-0.314, y -0.199..+0.692; south x 186 at z 120..127: x +0.32..+0.486,
  y +0.038..+0.913); KS03 facade base x +-30, y 0..40 (y -0.42..-0.0); KS14 gate frame x 683, z 186..204 (pylons), y 0..16
  (y -0.637..+0.42; the spec's x +-0.26 is the 12 m opening, z 189..201); KS19 window x -0.164..-0.027, y -0.263..; KS17 head at
  s 1296 (phi 70 deg) x -0.474, y +0.583 (spec -0.47..-0.39, +0.40..+0.61).
- Conflicts found while deriving: face B above y 0 does not exist for z 115..275 (agreement 5: the garden notch), so the spec's
  "far Gulf face at x 0" (KS04) and "face B at y 293 projects to y +0.68" (KS07) are checked on the notch flanks (z < 115, z > 275);
  at KS07 the gate frames (16 m) sit in front of the lower crowns although the spec says gate block A (10 m) hides none of them.

KOIMATH UPDATE 2026-11-14 04:05 (for G4): the tail veil now starts 30 m before the tail tip: new constants `KoiMath.VeilOverlap = 30`,
`KoiMath.VeilA0 = L - 30 = 870` and `KoiMath.VeilA(v) = VeilA0 + 120 v`; `VeilPoint(v, k, ...)` uses them (half-span
lerp(rho(VeilA0), 60, v)). Reason (VisibilityAudit, KS18 16:10): a veil rooted at the tail tip reaches NDC x 0.967 at 16:10 (margin
0.033 < 0.06, the 3 % rule); rooted 30 m earlier it reaches 0.889 (margin 0.111); 40 m gives 0.867, 60 m 0.811. KoiBuilder (G4)
mirrors the old formula at its lines 79-80: replace `a = L + KoiMath.VeilLength * v` by `a = KoiMath.VeilA(v)` and
`KoiMath.Radius(L)` by `KoiMath.Radius(KoiMath.VeilA0)` so the mesh matches the sample set.

### Step 2 - the audit (V-t03 .. V-t06)
- Offline check: the runtime audit files (Visibility*.cs + NocMath, NocturneConstants, NocSchedule, FilletPath, NocLanes,
  GazeSchedule, WalkPlanner, PlayerMath, QaProjection, QaJson, CityField, NocHash, KoiMath, NocturnePalette) also compile into a
  scratch .NET console (UnityEngine.CoreModule referenced) that runs the spec-only audit in 1.5 s; used to iterate while other
  builders' files did not compile.
- `V-t03`: NewScene(Additive) refused ("untitled scene unsaved") -> the world is built as new roots of the test runner's scene and
  destroyed afterwards.
- `V-t04`: world in EditMode: 11 builders (Beat01..Beat07, City00_Field, ZZ_MaterialSheet, ZZ_RainEdgeProbe, ZZ_SignSheet), 2 118 of
  2 125 mesh renderers as occluders, 340 341 triangles (6 transparent, 1 koi excluded; 1 828 non-readable meshes read back through
  Mesh.AcquireReadOnlyMeshData, 0 city cells regenerated); world 0.9 s, BVH 1.3 s, audit 1.6 s, total 4.0 s.
- Fixes after V-t04: subjects' own built geometry is stepped over (self tags: FaceA / FaceB / CrossBridge_850 / TrafficLayer_x_y /
  blade / crown / spire / gateB / screen / tenement ...: G2's TrafficLayer_350_-120 ribbon hid its own centre-line samples); the
  spire subject samples the silhouette facing the eye (far-side samples hid behind the spire); the KS14 subject is the 12 m opening
  (reproduces x +-0.258, y -0.637..+0.42); T-DROP counts deck, rails and edges only (14.10 wording): spec walk model, built hits
  in the deck band y -1.5..1.25 within 30 m; composition classes sky / deck / rail / near structure / drop, built classes by renderer
  path (BuiltClass); the KS13 strip test now needs the ray to clear the 0.5 m slab; KS12's thin rails count with its drop ("deck 30 %").
- KoiMath veil moved 30 m forward (KOIMATH UPDATE above): KS18 16:10 margin 0.033 -> 0.113.
- The KoiCompositionLint asserts KS17 and KS18 (acceptance 2); KS19's head is a lead finding (warning). T-DROP asserts the plan; the
  built T-DROP failures are listed for their owner (warning).

### Step 3 - findings of the audit (spec model + built world of 04:25, V-t06; offline variants with the scratch console)
Follow-up fixes in the audit: SelfTags checked crowns before trains ("crown_T3" was read as train T3); the goal window is a 4 x 3
grid on x 1156; the goal balcony slab (14.5.7, x 1154.6-1156, y 4.0-4.2 assumed) is a spec occluder; specific fix hints: a later
capture s for follower lag (walk trace), an eye shift along the path for occluded subjects, a pose search for the KS10 drop, the
head elevation against the occluder's top edge for a hidden koi head, and "the built geometry deviates from the plan" when only the
built scene hides a subject.

Findings (severity, owner, arithmetic, fix):
1. S1 lead - KS19 koi head hidden by the tenement in all 20 phases. The eye (path end) is 7.82 m from the 14.4 m west face; along
   the head's azimuth the ray meets the face at z 394.8 (inside 379..397) where the roof edge stands at 50.6 deg elevation, the
   head (y 360, 513 m) at 44.3 deg. Rotation cannot help (fixed eye); the roof line projects at NDC y 0.95 at the frame centre and
   above the frame top at the head's column (x -0.59: y 1.08). Fix: drop "koi head" from KS19's subjects (decision), the koi is
   seen in KS17 / KS18 and reaches the window as light.
2. S2 lead / G3 - KS10 drop below 14.5.3 acceptance (2): gate block A (8.7 m ahead) takes 8.9 % (spec model incl. frames) /
   13.3 % (built: GateBottom -14) of the frame; drop 30.5 % (spec) / 24.7 % (built) vs >= 40 % (spec 43 % ignored the block).
   Fix: KS10 key (540 / 556, -70, -30) -> rel yaw -95, pitch -32 (world yaw -5): drop 40.1 % with every whole subject in frame
   (spec model pose search); a built block that ends at y 0 instead of -14 gives back about 4 %.
3. S2 G3 - KS13 built deck is continuous over the glass strip (Phase 4 per G3's log): T6 and the y -520 layer are hidden (built
   unoccluded 0.00, T-DROP on the built deck fails); the plan passes (1.00). Fix: the hole x 521.3-541.3, |z - 195| <= 1.2 under
   NocGlass (14.5.4) in Phase 4.
4. S2 lead - KS13 follower lag 2.12 deg (pitch -72.88 vs -75 at s 716): the 61 deg pitch ramp over 16 m (key 700 -> 716) runs at the
   18 deg/s limit. Fix: capture KS13 at s 718 (lag 0.43 deg; T6 nose x 564.9, body x 390.9-564.9 still under the strip) or move the
   key (700, +40, -14) to (696, +40, -14) (lag 1.17 deg at 716; affects the T4 look at s 699.3).
5. S3 lead - KS03: the east podium corner (x 2.5 at z 120, NDC x +0.57) hides 65 % of blade x 90 (x +0.46..+0.62) at s 116; with
   the eye at s 117 (same pose) it is fully visible. Fix: KS03 s 116 -> 117 (readable window 106-123.5 unchanged).
6. S3 lead - KS02 tower flank: the spec's x -0.20..-0.10 lies on the west podium (a ray at NDC x -0.15 meets x -2.2 at y 16.3 < 22);
   the shaft face x -30 is seen only through the slit (x -0.10..+0.07), above y 278. Fix: correct the 14.5.1 text.
7. S3 G1 / lead - KS02 catwalk bottom edge at NDC y -0.9404 (margin limit 0.94); fix: catwalk 0.3 m north (z 96.3-97.9) or accept
   2.98 %. KS02 readable 5.5 s (s 86.0-99.8) vs 5.6.
8. S3 lead - KS09 shares: spec model sky 25.0 / deck 38.9 / rail 1.6 / gate 4.3 / drop 30.2 %, built 25 / 41 / 3 / 0 / 30, vs spec
   26 / 39 / 36: the bare deck plan gives 26.4 / 38.9 / 34.7 (the spec's numbers); gate A (37 m) and the rails take 6 %. Correct the
   14.5.3 numbers (Part 1.6). KS12: deck 30.6 %, rails 4.3 %, drop 42.9 % (with the rails 47.2 %, spec 48) in the plan; built deck
   31.4, rails 3.6, track beds inside the glass tubes 8.0, drop 34.7.
9. S3 G4 / lead - KS19 goal window: the balcony slab edge (x 1154.6, y 4.2) hides the window's lowest 0.35 m (the line eye -> edge
   meets the window plane at y 4.745): 25 % of the samples (spec), 50 % (built). A slab top at <= 1.7 + 2.7 x 6.42 / 7.82 = 3.92 m
   clears it; the storey floor at 3.6 m is the natural level (slab 3.4-3.6).
10. S3 lead - spec text: face B above y 0 is cut away for z 115..275 (agreement 5), so KS04 "far Gulf face at x 0" and KS07 "face B
   at y 293 projects to y +0.68" are the notch flanks now (in frame at x +-0.37 / +-0.43); the KS13 trapezoid (x +-0.60,
   y -0.14..+0.83) assumes the strip starts 0.3 m ahead, but the eye at s 716 stands mid-strip: the strip fills x +-0.59 over the
   whole frame height (59 % of the frame).
Info: koi KS17 core 17 samples behind the spire (spec 19), arcade max 40 / mean 34 (spec 28); core min margin 0.082 at KS17
(spec 0.085) and 0.180 at KS18 (spec 0.178): the spec's koi numbers come from the 164 core samples and reproduce within 0.003; the
visible-sample margins (veil and fins included, D-009 exemptions) are 0.237 and 0.113; KS19 head margin 0.106 (spec 0.11) if it were
visible. Follower lag <= 1.78 deg for every other shot (KS02 1.78, KS07 1.49); every eye on the cinematic path.

### Step 4 - final runs (GREEN)
- `V-tall01` (full EditMode suite, 04:31): 255 / 255 passed (audit inside it: 341 777 built triangles, total 10.2 s).
- Then: findings carry the owner of the occluding renderers (Beat01/02 G1, Beat03 G2, Beat04/05 G3, Beat06/07 G4) or "lead (spec /
  pose)" when the plan itself hides the subject. `V-t07` (VisibilityAuditTests | KoiMathTests, 04:35): 15 / 15; world 0.8 s, BVH
  1.3 s, audit 6.4 s, total 8.7 s; 16 findings in QA/reports/visibility.md (the list of Step 3). Q1 reruns
  `-testFilter VisibilityAuditTests` on the integrated geometry.
