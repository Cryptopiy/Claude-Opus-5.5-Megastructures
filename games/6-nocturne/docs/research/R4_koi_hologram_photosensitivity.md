# R4: Koi locomotion, additive hologram look, photosensitivity (Nocturne)

Researcher R4. Accessed 2026-11-13. Read first: MASTER_PROMPT Parts 3, 8, 11 and 14 (lines 464-994), Thread `ThreadGrade.shader` and `ThreadGradeMath.cs`, and the earlier games' research. Abyss, The Ring and Thread have **no research** on fish kinematics, holograms or photosensitivity. The only reusable lessons are Abyss's additive fog form (`AbyssFogAdditive`: radiance times transmittance, never plus the fog colour; Abyss `docs/SHADERS.md` line 74) and Thread's bloom prefilter (partial Karis average, soft knee on exposed values). Everything below is new.

Numbers marked **[computed]** come from my Python model of the spec koi and the spec wall (scratchpad scripts, summarised in Appendix B). The koi model reproduces the spec's own published numbers:

| Check | Spec | Model |
|---|---|---|
| KS17 extents | x -0.50..+0.72, y +0.10..+0.92 | x -0.498..+0.719, y +0.090..+0.919 |
| KS18 extents | x -0.37..+0.56, y -0.54..+0.82, margin 0.178 | x -0.377..+0.565, y -0.543..+0.820, margin 0.180 |
| Body heights | top 408, lowest 192 | 408.0 and 192.1 |
| Head azimuth table | as listed | exact at all six s |

The model can therefore be ported directly into the EditMode tests (T-KOI, composition lint).

Package note: the URP folder is `Library/PackageCache/com.unity.render-pipelines.universal@8457e85b8184` (its `package.json` says 17.6.0). The core folder is `com.unity.render-pipelines.core@2d66c71e606e`. There is no `@17.6.0` folder name.

---

## 0. Summary: decisions for the lead

1. **The koi kinematics read as natural. Keep the spec numbers.**
   - The shape matches real fish: wavelength/length 0.90 (real subcarangiform and carangiform fish ≈ 1 L), Strouhal number 0.33 at 1.5 m/s and 0.20 at 2.5 m/s (fish ≈ 0.3), slip U/V 0.30 and 0.49.
   - Tail amplitude 0.044 L is about half of a sprinting saithe, i.e. calm cruising. Real fish at under 1 body length per second also keep the front body still.
   - The 20 s period fits "giant" time scaling (√1500 × a koi tail beat ≈ 26 s).
   - Do not raise the amplitude: the spire, wall and tower clearances and the frame margins were computed at 40 m.
2. **Add body-yaw coupling.** Orient every ring with the tangent of the *waved* spine (analytic derivative, §1.6). Frame margins change by ≤ 0.001 [computed]. Optionally use the "pivot" envelope `A(f) = 0.15 - 0.6 f + 1.45 f²`. It is ≤ the spec envelope everywhere and equal at head and tail, so every frame and clearance test stays valid by construction (§1.4.2).
3. **The spec's caudal veil (200 m span, 160 m long) breaks composition and must change.**
   - KS18 at 16:10: a fin margin of -0.11.
   - KS17: for any veil ≥ 50 m, the part above the arcade roof leaves the top of the frame.
   - A vertical 200 m span reaches y 466, which is above the cloud base at 460.
   - Recommendation: **120 m long × 120 m span, vertical** (KS18 margin ≥ 0.05 at all three aspects, top y 447). Gate the body samples as the spec does. Add fin samples to the lint at KS18; at KS17 exempt fin samples whose body root is occluded by the arcade (§1.4.3).
4. **Fix two H6 test definitions.**
   - (a) "Body displacement ≤ 0.35 m per frame at 60 fps" cannot hold at 2.5 m/s: orbital speed alone is 18.6-21.4 m/s, giving 0.42 m per frame [computed]. Gate the *wave* displacement instead (≤ 0.35; measured 0.209) and the total with ≤ 0.45.
   - (b) Capture-mode wave time `t_w = (s - 1260)/1.5` makes 2.5 m/s *sequences* swim 1.67× too fast (period 12 s). Use `(s - 1260)/v_seq` for sequences and keep `/1.5` for stills.
5. **Hologram energy: budget per layer, not per fragment.** The additive shells are front, back and fins. The spec's per-fragment clamp of 3.0 lets two rims sum to 6, which clips at the tone curve.
   - Use a hue-preserving max-channel clamp of **2.6 on front faces and 0.8 on back faces** (back faces weighted 0.35, rising to 0.75 on patches), and 1.0 on fins.
   - Flip the normal with `SV_IsFrontFace`.
   - Fog additively (`col × T × (1 - cloudMix)`), and **never apply `NocFloor`** to an additive shader.
6. **Hex scales.**
   - Use material coordinates with a constant 48 cells around the body, so a mid-body cell is about 6 m and cells shrink toward head and tail like real scales.
   - Use the pristine-grid filtering: width at least 1.5 px with energy compensation, fading to the mean coverage `4w` at a pitch of 2-4 px.
   - Line gain ≤ 0.08 of the body term (ΔY ≤ 0.05), so moving scales can never be a flash or a harmful pattern, even when the koi fills more than 25 % of a free-look frame.
7. **The wall of signs (14.5.6) fails its own luminance-change rules at the KS16 pose.**
   - One near panel covers 23 % of the frame. The spec breath (±0.25 at 0.08-0.20 Hz) gives a whole-frame mean-luma step of 0.32 %/frame (gate 0.3) and a wall mean change of 15-26 %/s (design limit 4 %/s) [computed].
   - The waves alone already give 7 %/s.
   - Fix the picture (Part 14.10 forbids relaxing flash rules): **breath ±0.05 at 0.05-0.10 Hz**, and **ramp stops luminance-normalised with k = 0.8**. Scaling each stop by a scalar keeps the palette chromaticities; stop luminance becomes 0.255/0.310/0.265.
   - Result: frame-luma step 0.11 %/frame, wall 3.0 %/s, zero transitions [computed].
   - Even the spec version has zero flashes, every block's dominant frequency ≤ 0.2 Hz, and a red share of 0.587.
8. **Photosensitivity test** (exact algorithm in §3.3, reference code in Appendix A, self-tested).
   - Compute 16×16-block means of *linear* RGB from the 8-bit sRGB frames (IEC LUT, threshold 0.04045), relative luminance Y = 0.2126 R + 0.7152 G + 0.0722 B, and per-block peak-valley ("zig-zag") transitions of ≥ 0.1 whose darker side is < 0.8.
   - Gates:
     - G1, the spec's zero flashes: < 25 % of the screen has two opposing transitions within 1 s.
     - G2, WCAG general: no 1/3 × 1/3 window has ≥ 25 % of its blocks with more than 6 transitions in 1 s.
     - G3, WCAG red: as G2, with transitions to or from saturated red (R/(R+G+B) ≥ 0.8 linear, Δu'v' > 0.2).
     - G4: saturated-red area ≤ 10 % of any window.
     - G5: dominant block frequency ≤ 0.5 Hz, with an amplitude floor of 0.002.
     - G6: mean-luma step ≤ 0.3 %/frame.
     - G7: wall linear red share p99.9 ≤ 0.70.
     - G8: pattern contrast ≤ 0.05.
   - Run the whole walk too: cinematic play timing, 30 fps, plus the 17 s ending.
9. **Whole-walk design rules** (each removes a real flash or pattern risk):
   - Train window strips must not go dark at car gaps (dip ≤ 0.08 in relative luminance).
   - LED grid and seam contrast ≤ 0.05.
   - Scanlines ≤ 0.5 Hz per point.
   - No hologram flicker or glitch effects.
   - Total saturated-red area (beacons, tail lights) under 1 % of the frame.

---

## 1. Koi locomotion

### 1.1 What biology and theory say (verified)

**Kinematic model of the body wave.** Undulatory swimmers are described by a lateral midline displacement `h(x, t) = A(x) sin(2π (x/λ - f t) + φ)`, with x along the body from the snout.
- The standard carangiform envelope (steady saithe; Videler 1993, used in Khalid et al. 2020) is `A(x/L) = 0.02 - 0.0825 (x/L) + 0.1625 (x/L)²`, in body lengths. That is 0.02 L at the head, a minimum of about 0.0095 L at x ≈ 0.25 L, and 0.10 L at the tail.
- Sub-carangiform and carangiform fish carry roughly one body wavelength: λ/L ≈ 1, "equal to or slightly larger than L". A measured jack has 1.05 L. Anguilliform eels have λ ≪ L.
- Sources: Khalid et al. (arXiv 2010.04748); Lauder & Tytell 2006, Fig. 11.1.

**Modes are less different than the textbook pictures.**
- Lauder & Tytell stress that at slow speeds the envelopes of eel, sub-carangiform and carangiform fish are nearly identical.
- **Below about 1 body length per second the anterior body barely oscillates.** Bending is confined to the posterior half and the head starts swinging only at higher speeds (their Fig. 11.2).
- The common carp (*Cyprinus carpio*, the koi species) is a sub-carangiform cruiser.

**Strouhal number.** St = f × A_pp / U, where A_pp is the peak-to-peak tail excursion.
- Fish, cetaceans and flyers cluster near St ≈ 0.3 (thrust efficiency peaks near 0.3). Eels measure 0.31-0.41.
- Slow fish run higher: salmon at 0.5 L/s show St > 0.6 (Lauder & Tytell, Fig. 11.3).

**Lighthill's elongated-body theory (EBT).** It is a reactive, added-mass model; only the trailing edge matters on average.
- The water's lateral velocity at the tail is `w = W (V - U)/V`, where W is the tail's lateral velocity, V the backward wave speed and U the swimming speed.
- Mean thrust ∝ `V² - U²`, so the wave must outrun the body (V > U).
- Froude efficiency is `η = 1 - (V - U)/(2V) = (U + V)/(2V)`, between 0.5 and 1.
- Lighthill also adds rigid "recoil" terms `h* = h + x Ω*(t) + W*(t)`, a yaw and a sway that conserve the body's momentum. Carangiform fish minimise recoil with a heavy front body and a thin peduncle.
- Sources: Imperial College BioFluids lecture 7-8, eqs. 7.13-7.19; Lauder & Tytell eqs. 5-11.

**Speed versus frequency (Bainbridge 1958; dace, trout and goldfish, a cyprinid like koi).** Above about 5 beats per second speed is proportional to tail-beat frequency, and the distance per beat depends on amplitude. Amplitude is roughly constant at moderate and high speeds (SICB abstract that reviews it).

### 1.2 The spec's koi checked against this

Spec: L 900 m on an orbit of R 260 ± 18 m, wave `w(a, t) = 40 (0.15 + 0.85 (a/L)²) sin(2π (a/810 - t/20))`, head azimuth rate 1.76 deg per metre of s. Values are [computed] unless noted.

| Quantity | 1.5 m/s walk (and a lingering player) | 2.5 m/s cinematic | Real fish | Verdict |
|---|---|---|---|---|
| Body speed U (plan, head; all body points move at the same plan speed) | 11.98 m/s = 0.013 L/s | 19.97 m/s = 0.022 L/s | koi cruise about 0.3-1 L/s | "slow motion" giant, see the time-scale row |
| λ / L | 0.90 | 0.90 | ≈ 1.0 (0.9-1.1) | ✓ |
| Tail half-amplitude / L | 0.044 (p-p 0.089) | same | 0.10 (p-p 0.2) steady saithe | ~45 %: calm, stiff-ish cruising ✓ |
| Head half-amplitude / L | 0.0067 (6 m) | same | 0.02 at speed, ~0 when slow | ✓ (slow-swim look) |
| Wave speed V = λ/T | 40.5 m/s | 40.5 m/s | V > U | ✓ thrust positive |
| Slip U/V | 0.30 | 0.49 | 0.6-0.8 steady, lower when slow | plausible for slow swimming |
| St = A_pp/(T U) | 0.33 | 0.20 | ≈ 0.3 (0.2-0.4) | ✓ both inside the band |
| Stride (distance per beat) U·T | 240 m = 0.27 L | 399 m = 0.44 L | 0.4-0.7 L | ✓ at 2.5, short at 1.5 |
| EBT efficiency (U+V)/2V | 0.65 | 0.75 | 0.67-0.74 (saithe, Cheng & Blickhan via search) | ✓ |
| Max lateral tail speed | 12.6 m/s | 12.6 m/s | | 0.21 m per 60 fps frame |
| Max body yaw (slope of w) | head 2.7°, mid 6.4°, 0.75 L 11.2°, tail 17.8° | same | | natural |
| Angular speed of spine points seen from the walker | | KS17 1.5-2.2 °/s, KS18 1.6-3.6 °/s | | calm, majestic |

**Time scale.** By Froude-type dynamic similarity (the miniature-cinematography rule), time scales with the square root of length. A 0.6 m koi with a tail-beat period of about 0.67 s (≈ 1.5 Hz cruising; my estimate, not from a source) at ×1500 scale gives T ≈ 0.67 × √1500 ≈ 26 s. Cruising at 0.5 L/s = 0.3 m/s gives U ≈ 0.3 × 38.7 ≈ 12 m/s. The spec's 20 s and 12-20 m/s sit right on this scaling, which is why the koi will read as huge rather than sped-up.

**Verdict.** The numbers read as natural, calm swimming. They are on the stiff, low-amplitude side, which suits a 900 m creature. Do not raise the amplitude to the 0.1 L of a sprinting fish: the spec's clearances (spire 153 m, wall 126 m, towers 121 m) and the KS17 frame margin of 0.08 were computed with 40 m.

### 1.3 The spec's frame numbers, reproduced

Model conventions:
- Plan arc length `l(φ) = ∫R dφ = 260 φ - 9 cos(2φ + 0.7)` (closed form).
- Body point at arc distance a behind the head: `l(φ_a) = l(φ_h) - a`.
- Wave along the radial unit `(cos φ, 0, sin φ)`.
- Body radius = 48 × profile, linearly interpolated.
- Camera basis: forward `(sin yaw cos pitch, sin pitch, cos yaw cos pitch)`, right `(cos yaw, 0, -sin yaw)`, up = forward × right.
- 20 wave phases `t_w + k`, k = 0..19 s.

Results:
- KS17 and KS18 match the spec within 0.01 (table above). The body spans 198.0° of the orbit.
- **KS17 tail.** From a ≈ 780 m to the tail (a = 900), the body is occluded by the arcade box (x 914-990, z 236-360, y 0-38). This confirms the spec's "last 140 m behind the arcade roof".
- **Koi screen coverage is only 5.6 % (KS17), 7.0 % (KS18) and 11.9 % (KS19)** of the frame, not the 20 % the beat-7 mean-luma budget assumes. Expected beat-7 mean luma is then about 50, not 64 (still inside 34-85).
- **No self-overlap.** At KS17, KS18 and KS19 no pixel is covered by two body parts more than 250 m apart. Additive layering is therefore front, back and fins only, never body over body.

### 1.4 Refinements that keep the spec's frame tests valid

#### 1.4.1 Body-yaw coupling (do this)

Build each ring perpendicular to the **waved** spine's tangent, not the orbit's:
- `P(a) = C(φ_a) + w(a, t) n(φ_a)`
- `T = normalize(dP/da)`
- `lateral = normalize(cross(worldUp, T))`
- `up = cross(T, lateral)`

This gives each cross-section the local yaw of the wave: 2.7° at the head and 17.8° at the tail. It is the cue that makes a fish read as swimming rather than a sine bump sliding along a rail. It costs nothing on the GPU (the frames are computed on the CPU, §1.6). The frame extents change by ≤ 0.001: KS17 margin 0.080 unwaved, 0.081 waved; KS18 0.180 both.

#### 1.4.2 Optional "pivot" envelope (slow-swim realism)

`A_alt(f) = 0.15 - 0.6 f + 1.45 f²` (f = a/L) has a minimum of 0.088 at f = 0.21. That is the real fish's "pivot" a fifth of a body length behind the snout (§1.1).

Proof that it keeps every test: `A_spec - A_alt = 0.6 f (1 - f) ≥ 0`, with equality at head and tail. Each body point therefore moves on a sub-segment of the radial segment it sweeps under the spec envelope.
- **Frame tests.** The set of points that project inside the margin box is convex (an intersection of half-spaces, points in front of the camera), so the frame tests stay valid.
- **Clearances.** The clearance minimum over a smaller interval is ≥ the minimum over the larger one.
- Measured: KS17 margin 0.082, KS18 0.181.

Look: the front third becomes quieter and the tail livelier, as in a cruising koi. This is the lead's call; the spec's envelope is acceptable as is.

#### 1.4.3 Fins (ribbons with trailing waves)

**Common trailing-wave form for a ribbon.**
- Span coordinate σ ∈ [0, 1] (root to tip), chord coordinate c ∈ [0, 1] (leading to trailing edge).
- Root frame from the spine station it hangs on (§1.6).
- Displacement along the fin normal: `δ(σ, c, t) = A_fin σ^1.5 c² sin(2π (t/T_fin - 0.3 c) + φ_0)`.
- The trailing edge lags 0.3 cycle (passive flexion), so the fin reads as cloth in water.
- Keep `A_fin ≤ 0.12 × chord`.

**Pectoral fins (spec 120 × 60 m).**
- Root at a = 0.15-0.18 L on the lower flank (30° below the lateral line).
- Swept back 35° and down 30°.
- "Rowing" angle `±10° sin(2π t_w/20 + π/2)`, phase-locked to the body wave so the motion reads coordinated.
- Koi pectorals are large and rounded (carp: "large" pectoral fins; Wikipedia), so 120 m is about 0.13 L, right.
- Frame margins with 120 × 60 m (and even 150 × 70 m): KS17 ≥ 0.57, KS18 ≥ 0.27 [computed].

**Dorsal fin (spec 90 × 40 m).**
- The carp's dorsal fin is long-based and tapers along the back.
- A low, long fin reads more "carp" than a short one: base 0.42-0.72 L (270 m), height 40 m falling to 10 m.
- Margins with 270 × 40: KS17 0.57, KS18 0.27 [computed].
- Seen from below it shows through the translucent body at back-face weight (§2.2).

**Caudal veil: the spec's 200 m span × 160 m long does not fit.** Model: a vertical two-lobed veil continuing the tail with the same wave plus a 0.25-cycle lag at the tip. Worst margin over 20 phases, visible veil samples only (arcade occlusion applied):

| Veil (length × span) | KS18 16:9 | KS18 16:10 | KS18 21:9 | KS17 | Top y over the beat |
|---|---|---|---|---|---|
| 160 × 200 (spec) | -0.00 | **-0.11** | +0.25 | out of frame | **466 (> cloud 460)** |
| 160 × 140 | +0.04 | -0.07 | +0.28 | out of frame | about 455 |
| 120 × 200 | +0.12 | +0.02 | +0.34 | out of frame | 466 |
| 120 × 140 | +0.15 | +0.05 | +0.36 | out of frame | about 455 |
| **120 × 120 (recommended)** | +0.17 | ≈ +0.06 | +0.37 | out of frame | 447 |
| 100 × 100 | +0.21 | +0.13 | +0.41 | out of frame | about 440 |
| 50 × 100 | +0.32 | +0.25 | +0.47 | out of frame (margin -0.36) | |

Why KS17 always fails:
- The body behind the tail is hidden by the arcade roof.
- The orbit beyond the tail climbs toward the walker's zenith: a = 960 m is at elevation 44.8°, ndc y +1.03. The tail tip itself is at 38° elevation, behind the roof.
- So any veil lobe rises above the roof line *and* above the frame top. Raising the KS17 pitch to +27° still leaves a 60 × 100 veil at -0.03.

Recommendation:
- **120 m × 120 m vertical veil**, fork depth about 35 % of the length, lobe tips fading with the cloud mix (§2.6).
- The composition lint gates body samples at all three key shots (as the spec does). It gates fin samples at KS18 (margin 0.03).
- At KS17, fin samples whose root station is occluded are exempt. Log this as a decision: the tail sweeping up and out of the frame behind the roof is natural framing, not a defect.
- The only alternative that keeps the veil wholly inside the KS17 frame is ≤ 40 m, which loses the "butterfly koi" look.

#### 1.4.4 Optional anatomy cues (cheap, high read value; cut with "koi fins and scales")

- **Two pairs of barbels** (the defining carp cue; Wikipedia "Common carp"): 40-50 m curved tubes of radius 1.5 m, drawn with the NocLine minimum-width rule (≥ 1.75 px). They are not K-light sources.
- **Eyes**: two additive discs of radius 4 m at 0.06 L, at KoiGlow × 1.5. That is 21 px at 1080p at KS18's 308 m.
- **Operculum (gill cover) line**: a soft 6 m ring brightening (+15 %) at a = 0.15 L. It marks "head" in silhouette.

#### 1.4.5 Do not

- **Inextensibility correction.** The waved spine is 4.3-7.3 m (0.5-0.8 %) longer than L [computed]; this is invisible. Keep the material coordinate a = arc length of the unwaved orbit, as T-KOI defines it.
- **Recoil translation (W\*).** It moves the head outside the spec envelope; the waved-frame yaw already gives the visual cue.
- **Speed-coupled tail-beat frequency** (wave phase from travelled distance). It would keep the slip constant but break `t_w = t_k` in play and the H6 tests. Not worth it.

### 1.5 Koi pattern: Kohaku in neon

Kohaku is a white ground (shiroji) with red markings (hi). The neon reading is KoiBody cyan-white with KoiPatch magenta. Standards (Wikipedia "Kōhaku"; koi glossaries):
- The head must carry hi; a head without it is "bozu", a fault.
- Plates should be large (at least a quarter of the length is valued) and well separated: two steps "nidan", three "sandan", four "yondan".
- Hi should not pass the lateral line, wrap the belly or reach into the tail.
- Leave a white "odome" before the tail.
- The trailing edge of a plate ("kiwa") is crisp. The leading edge ("sashi", white scales over red) is soft, at most about 2 scales deep.
- Showa (black sumi with red and white) needs black. In an additive hologram, black is a hole, so treat Showa as optional thin "sumi" bands at 35 % emission.

Recipe (material coordinates f = a/L ∈ [0, 1], θ measured from the dorsal line, r = local radius):
- Plates k = 0..3, centres c = (0.05, 0.24, 0.42, 0.60), half-lengths h = (0.035, 0.07, 0.065, 0.055). That is a head spot plus sandan; nothing for f > 0.70 (odome).
- Normalised coordinate `x = (f - c_k + warp)/h_k`.
- Along-body mask `M_k = smoothstep(-1.35, -0.95, x) × (1 - aaStep(1.0, x))`. The soft rise is the sashi (≈ 11 m wide). `aaStep` is the 1.5-px anti-aliased step built from `fwidth(x)` (the kiwa).
- Angular mask `1 - smoothstep(φ_k - 0.15, φ_k + 0.05, |θ|)` with φ_k ≈ 80° ± 10° warp. This stops at the lateral line.
- `warp` = 2-octave value noise in metres (120 m and 40 m), amplitude 0.25 h_k. This is the spec's "cell noise 120 m", used to make organic outlines instead of a leopard pattern.
- Patch = max over k of `M_k × angular mask`.
- From below (KS18 pitch 46°) the plates on the back show **through** the translucent body. That is why the back-face weight rises to 0.75 on patches (§2.2).
- KoiPatch linear red share 0.67, under 0.70 and far from saturated red (0.8).

### 1.6 Implementation sketch (deterministic, exact, cheap)

**C# `KoiDirector`, once per frame, in double.**
- Head azimuth from u (H6); `t_w` per §1.7.
- For k = 0..160, `a_k = 5.625 k` m.
- `φ_k` solves `l(φ) = l(φ_h) - a_k` with `l(φ) = 260 φ - 9 cos(2φ + 0.7)`. Newton: `φ ← φ - (l(φ) - target)/R(φ)`, 3 iterations from a linear guess. R ≥ 242 > 0, so it always converges.
- Analytic tangent:
  - `dC/dφ = (R' cos φ - R sin φ, y', R' sin φ + R cos φ)`, with `R' = 36 cos(2φ + 0.7)` and `y' = 60 cos(φ + 0.4)`.
  - `dn/dφ = (-sin φ, 0, cos φ)`, `dφ/da = -1/R(φ)`.
  - `dP/da = (dC/dφ + w dn/dφ) dφ/da + (∂w/∂a) n`.
- Frame per station: `pos`, `radius`, `tangent`, `lateral`, `a`, `cellV` (§2.5). Upload to a `GraphicsBuffer` (Structured, 161 × 16 floats) set as a global.
- The analytic K light (14.2.3) samples 12 of the same stations.

**Mesh.** Topology only: 161 rings × 28 vertices, plus caps and fins.
- `uv0 = (ring index, θ)`.
- The vertex shader skins each ring as `pos + r (cos θ lateral + sin θ up)` with normal `cos θ lateral + sin θ up` (`#pragma target 4.5` for StructuredBuffer in the vertex stage on D3D11).
- Fins carry `(root station, σ, c)` and apply §1.4.3 in the vertex shader.

**Culling.** The deformation happens on the GPU, so set explicit bounds: `meshRenderer.localBounds = new Bounds(new Vector3(1330, 300, 388), new Vector3(1040, 400, 1040))` (koi object at the identity transform). The setter and `ResetLocalBounds` exist in the 6000.6.0f1 `UnityEngine.CoreModule.dll` (verified). A MeshRenderer, rather than `Graphics.RenderMesh`, is also seen by every capture camera (Part 3.2 lesson 8).

**Precision.** World coordinates up to about 1 650 m are fine in float (0.12 mm at 1 km). Compute `φ`, `w` and the scanline phase in double and wrap time-phases in C#.

### 1.7 H6 tests to correct (spec text vs arithmetic)

| Spec text | Arithmetic | Recommended test |
|---|---|---|
| "body displacement ≤ 0.35 m per frame at 60 fps" | All body points move along the orbit at R(φ_h) × 4.4 °/s = 18.6-21.4 m/s at 2.5 m/s, before the wave. Max spine displacement per 60 fps frame: 0.321 m at 1.5 m/s, **0.423 m at 2.5 m/s**, 0.535 m in capture mode at 2.5 m/s [computed] | `max |w(a, t+Δ) - w(a, t)| ≤ 0.35 m` (measured 0.209), plus total `max |P(t+Δ) - P(t)| ≤ 1.05 × the analytic bound` (0.45 m at 2.5 m/s). This catches pops, which is the purpose. Log the change as an arbitration |
| "wave time `t_w = (s - 1 260)/1.5` in capture mode" | At 2.5 m/s the wave then runs 1.67× fast in 30 fps sequences (period 12 s) | Stills: keep `/1.5` (deterministic). Sequences (`-sequence koi`, boundaries): `t_w = (s - 1 260)/v_seq` |
| "koi 20 % at 150" in the beat-7 mean-luma budget | Coverage 5.6-7.0 % at KS17/KS18 | Expect a mean luma of about 50 at KS17. Tighten after measurement (allowed by 14.10) |

---

## 2. Additive hologram look done well

### 2.1 Render state and sorting (package facts)

- Spec state: `Blend One One`, `ZWrite Off`, `ZTest LEqual`, `Cull Off`, queue `Transparent+30` (3030).
  - Queue 3030 is inside `RenderQueueRange.transparent`, drawn by URP's `DrawTransparentObjects` pass at `RenderPassEvent.BeforeRenderingTransparents` (450) (`Runtime/UniversalRenderer.cs:411`, `Runtime/Passes/ScriptableRenderPass.cs:143`).
  - Transparents sort with `SortingCriteria.CommonTransparent`, back to front by object (`Runtime/Passes/DrawObjectsPass.cs:204`).
  - Additive blending is commutative, so koi order versus NocStreak (3010, also additive) does not matter.
- **Glass caveat.** NocGlass (3000, alpha-blended, ZWrite off) draws *before* the koi. If glass ever lies between the camera and the koi, the koi is added unattenuated; it should be × (1 - α).
  - In beat 7 every glass is at street level, and the arcade's opaque roof occludes the tail, so this does not occur on the path.
  - Add a capture check: no koi pixel behind a NocGlass pixel at the key poses or in free-look.
  - If it ever occurs, move the koi to 2990, before glass. That is correct whenever glass is in front of the koi, and the koi is never in front of glass from the walkway.
- **Front and back faces.** On D3D11 the core library defines `FRONT_FACE_SEMANTIC SV_IsFrontFace`, `FRONT_FACE_TYPE bool` and `IS_FRONT_VFACE(VAL, FRONT, BACK)` (`com.unity.render-pipelines.core@2d66c71e606e/ShaderLibrary/API/D3D11.hlsl:10-12`). Use them; never use the geometric normal on back faces.
- **Rain and depth.** The rain pass runs after transparents and reads depth. The koi writes no depth, so rain correctly draws in front of it (all rain layers are within 20 m). Rain's `Lloc` sees the koi glow, so rain brightens in front of the koi, a nice free effect.
- **No depth passes.** The koi needs no `DepthOnly`, `DepthNormals` or `ShadowCaster` pass (no depth write, no shadows in this game). Give it only `UniversalForward`. The SRP Batcher is irrelevant for one draw (keep a `UnityPerMaterial` CBUFFER anyway).
- **MSAA 4x and reversed-Z.** MSAA resolves the additive silhouette edges; per-sample blending is correct. Unity flips `ZTest LEqual` for reversed-Z automatically.

### 2.2 Radiance model with per-layer energy budgets

Spec radiance: `KoiBody × 0.9 × (0.55 + 0.45 vein) + KoiGlow × 2.2 × (1 - NdotV)^2.2`, clamp 3.0. With `Cull Off`, every pixel inside the body silhouette gets two layers (front and back faces). Fins add a third where they cross the body.

Mapped through Thread's grade (EV +0.1; Neutral curve start 0.76, desaturation 0.15, `ThreadGradeMath.cs:34-36`; `ThreadGrade.shader:121-132`; sky behind about (0.06, 0.03, 0.09); no bloom) [computed]:

| Case (linear, max channel) | 8-bit result | Luma | Relative Y |
|---|---|---|---|
| Front body, vein 0 (0.50 B) | (158, 186, 207) | 181 | 0.47 |
| Front body, vein 1 (0.90 B) | (185, 222, 243) | 216 | 0.69 |
| Front + back body, back weight 1.0 | (194, 231, 249) | 225 | 0.76 |
| Rim, one face (≈ 2.7) | (177, 233, 252) | 223 | 0.75 |
| **Rim, both faces at the spec clamp (≈ 5.9)** | (200, 240, **254**) | 233 | 0.82 (**clipped**) |
| Patch, front | (231, 94, 167) | 128 | 0.28 |

A channel reaches code 254 at linear peak ≈ 6 (≈ 4.5 once the ±1.2-code dither near white is counted). So bound the **sum**:
- **Front faces**: max-channel clamp 2.6.
- **Back faces**: weight `lerp(0.35, 0.75, patch)`, clamp 0.8.
- **Fins**: clamp 1.0.

The worst-case sum is 4.4, reached only where a fin crosses a rim. The clipped area is limited to the outer about 1 % of the radius on each side, about 2-3 % of koi pixels, i.e. 0.12-0.36 % of the frame. The gate is ≤ 0.6 %.

Clamp by **scaling to the max channel**, `c *= min(1, budget / max3(c))`. A per-channel `min` shifts the hue toward white-cyan.

The back-face weight gives the "glass volume" read: dorsal patches and the far wall of the tube show faintly through the near wall.

### 2.3 Fresnel rim done right

```hlsl
float4 KoiFrag(Varyings i, FRONT_FACE_TYPE ff : FRONT_FACE_SEMANTIC) : SV_Target
{
    float3 n   = normalize(i.normalWS) * IS_FRONT_VFACE(ff, 1.0, -1.0);   // flip on back faces
    float3 v   = normalize(_WorldSpaceCameraPos - i.positionWS);
    float  ndv = saturate(dot(n, v));                                     // never negative -> no (1-ndv)>1
    float  rim = pow(1.0 - ndv, 2.2);
    float  patch = KoiPatchMask(i.matF, i.matTheta, i.matA);              // 1.5
    float  vein  = KoiVein(i.matA, i.matTheta);                           // filtered low-frequency noise 0..1
    float  hexL  = KoiHexLines(i.cellUV, 0.025);                          // 2.5, already footprint-filtered
    float3 base  = lerp(_KoiBody, _KoiPatch, patch);
    float3 c = base * 0.9 * (0.55 + 0.45 * vein) * (1.0 + 0.08 * hexL) + _KoiGlow * 2.2 * rim;
    c *= 1.0 + 0.06 * sin(TWO_PI * (i.positionWS.y / 24.0 - _NocKoiScanPhase));  // 2.4
    float w      = IS_FRONT_VFACE(ff, 1.0, lerp(0.35, 0.75, patch));
    float budget = IS_FRONT_VFACE(ff, 2.6, 0.8);
    c *= w;
    c *= min(1.0, budget / max(max(c.r, c.g), max(c.b, 1e-4)));          // hue-preserving clamp
    c *= NocFogTransmittance(i.positionWS) * (1.0 - NocCloudMix(i.positionWS.y));   // 2.6
    return float4(c, 0.0);                                                // Blend One One; no NocFloor
}
```

Pitfalls:
- **Normals.** The normal must be the analytic ring normal from the spine frame (§1.6), interpolated across 28 vertices. Faceted normals make the rim band zig-zag on a 96 m tube.
- **Back-face rim.** With the flipped normal, the back face also rims at the silhouette. It is weighted, so the silhouette reads as a soft double edge, not a hard white outline.
- **Exponent.** 2.2 gives a rim about 10 % of the radius wide. A higher exponent (4-5) thins the rim below 2 px at KS17's 500-700 m and makes it crawl. Keep 2.2.
- **No time-based flicker or glitch** (classic hologram tropes). Photosensitivity and the "static 120 frames ≤ 2 levels" test forbid them.

### 2.4 Scanlines

- Use horizontal world-space bands with a smooth sine profile: `1 + 0.06 sin(2π (y/24 - phase))`, with `phase = frac(6 t/24)` computed in double in C#.
- A sine is band-limited, so it never aliases. At 300-700 m a 24 m pitch is 29-67 px at 1080p.
- A point on the koi sees the bands pass at `(6 + |dy/dt|)/24 ≤ (6 + 5.3)/24 = 0.47 Hz` (vertical koi speed ≤ 5.3 m/s [computed]). Amplitude ±6 % is ΔY ≤ 0.07 peak-to-peak: below the 0.1 flash threshold and below 0.5 Hz.
- Do not use square bands; they need footprint filtering and add little.

### 2.5 Hex scales filtered by footprint

**Coordinates.** Material coordinates, so the scales ride with the body.
- Circumferential `cellU = θ/(2π) × N`, with N = 48 cells around. Store it unwrapped in [0, 48] with a duplicated seam column; 48 is an integer, so the lattice wraps seamlessly.
- Along the body `cellV(a) = ∫0^a N/(2π r(a')) da'`, precomputed per station.
- Cells are locally isotropic hexagons whose size follows the body radius: 6.3 m mid-body (the spec's 6 m), 1.3 m at the tail and 1.4 m at the head. They shrink toward the ends like real scale rows, and the small ones fade to the mean automatically.

```hlsl
// Pointy-top hex lattice, flat-to-flat = 1 cell (apothem 0.5). Distance to the nearest edge in cell units.
float HexEdgeDist(float2 p)
{
    const float2 r = float2(1.0, 1.7320508);
    const float2 h = r * 0.5;
    float2 a = p - r * floor(p / r + 0.5);
    float2 b = (p - h) - r * floor((p - h) / r + 0.5);
    float2 g = abs(dot(a, a) < dot(b, b) ? a : b);
    return 0.5 - max(g.x, dot(g, float2(0.5, 0.8660254)));
}

// Footprint-filtered lines (after Golus' "pristine grid"): >= 1.5 px wide, energy-preserving, fade to mean.
float KoiHexLines(float2 cellUV, float halfW)          // halfW in cells (0.025 = 0.3 m total width at 6 m)
{
    float  d   = HexEdgeDist(cellUV);
    float  cpp = max(max(length(ddx(cellUV)), length(ddy(cellUV))), 1e-6);   // cells per pixel
    float  pitchPx = 1.0 / cpp;
    float  drawW = max(halfW, 0.75 * cpp);                                     // total >= 1.5 px
    float  cov = 1.0 - smoothstep(drawW - cpp, drawW + cpp, d);
    cov *= halfW / drawW;                                                      // keep the true line energy
    return lerp(4.0 * halfW, cov, smoothstep(2.0, 4.0, pitchPx));              // mean coverage of a hex band = 4w
}
```

Notes:
- **Mean coverage.** A hex of flat-to-flat 1 has area √3/2 and perimeter 2√3. A band of half-width w inside it covers `2√3 w / (√3/2) = 4w` of the area. Fading to this value keeps the macro energy constant, as H2 does for windows.
- **Contrast cap for photosensitivity (§3.6).** Line gain 0.08 of the body term gives ΔY ≤ 0.05.
  - In free-look directly under the koi (s ≈ 1 404-1 422, koi at about 275 m overhead), 6 m cells pass a pixel at 20/6 ≈ 3.3 Hz and the koi can cover more than 25 % of the frame.
  - With ΔY < 0.1 these are not transitions at all, so neither the flash rule nor the BT.1702 pattern rule (lighter and darker difference ≥ 20 cd/m² = 0.1) can trigger.
  - The scales also "flow smoothly in one direction", which BT.1702 exempts.
- **Patch edges.** Body to patch is ΔY ≈ 0.3. Patches are at least 60 m long and the body moves at ≤ 21 m/s, so a pixel sees at most about 0.35 patch transitions per second: harmless.

### 2.6 Fog, cloud and floor for additive shells

- **Fog.** For an opaque surface, `NocFog(col) = col T + fogColour (1 - T)`. For an additive layer the background already carries the fog colour, so **only** `col × T` is correct; adding `fogColour (1 - T)` per layer double-fogs and lifts the silhouette. This is Abyss's lesson (`AbyssFogAdditive`).
  - Expose `NocFogTransmittance(posWS)` from `NocAtmosphere.hlsl`: the same `sigma_eff` and the same `exp(-(sigma_eff d)²)`.
  - At 300-700 m and sigma 3.4e-4, T = 0.99-0.94.
- **Cloud mix.** The H3 rule mixes toward the ceiling colour with `smoothstep(380, 560, y)`. For additive layers this becomes `× (1 - mix)`. The veil and fin tips near y 440-447 then fade into the glowing cloud instead of ending.
- **Floor.** `NocFloor(col) = FloorLin + col (1 - FloorLin)` must **not** run in an additive shader: it would add FloorLin once per layer (twice inside the body). The 14.5 rule "every shader ends with NocFog and NocFloor" needs this one stated exception. Log it in DEVELOPMENT.md.

### 2.7 Bloom interplay (ThreadGrade)

- **Prefilter.** It thresholds on the exposed max channel with the URP soft knee, after a 13-tap downsample with a **partial Karis average** on the first level (`ThreadGrade.shader:83-110, 165-177`).
  - The Karis weighting `1/(1 + luma)` suppresses single-pixel rim fireflies, so the bloom of the 1-2 px rim does not flicker as the rim crawls. Keep it on.
  - Threshold 0.9 and intensity 0.50 (beat 7) mean the body (B channel 0.5-0.9 front, about 1.1-1.3 with the back weight) blooms softly and the rim strongly. That is the "glow".
- **Bloom energy stability (±3 %/frame, H6).** The rim area changes only with the wave (≥ 20 s period) and the walk. At most 0.2 % per frame is expected.

### 2.8 Hologram tests

1. **Static.** `-freeze-clock -rain 0`, 120 frames at KS17 and KS18: change ≤ 2 levels. The koi is frozen, so this tests rim and hex stability under the static dither.
2. **Pan shimmer** (14.10 shimmer test) on a koi crop at KS18. The hex lines are the risk; the 1.5 px minimum width and the energy rule handle it.
3. **Clipped koi pixels ≤ 0.6 %** of the frame, any channel ≥ 254.
4. **Layer budget unit test.** Render the koi alone over black with `Blend One One` into an RGBA16F target. Max channel ≤ 4.4; p99 of the koi mask ≤ 3.4.
5. **Back-face check.** No koi pixel brighter on a back-face-only region than the front at the same location. Test with `Cull Front` in a debug variant.
6. **Glass occlusion check (§2.1).**
7. **Additive fog parity.** The koi at 300 m and 700 m: the ratio of the koi contribution (koi frame minus koi-off frame) to the unfogged render equals `T(d)` within 1 %.

---

## 3. Photosensitivity

### 3.1 The standards (paraphrased; numbers exact)

**WCAG 2.2, SC 2.3.1 Three Flashes or Below Threshold (Level A).**
- Content must not flash more than three times in any 1 s, unless it stays below the general flash and red flash thresholds.
- A **general flash** is a pair of opposing changes in relative luminance of ≥ 10 % of the maximum (≥ 0.1), where the darker state is below 0.80.
- A **red flash** is a pair of opposing transitions involving a saturated red. Working definition (WCAG 2.2, from ISO 9241-391): one state has R/(R+G+B) ≥ 0.8, and the two states differ by more than 0.2 in CIE 1976 u'v' chromaticity (Euclidean distance; `u' = 4X/(X + 15Y + 3Z)`, `v' = 9Y/(X + 15Y + 3Z)`).
- A flash or sequence is below the threshold if there are ≤ 3 general and ≤ 3 red flashes per second, **or** the concurrently flashing area is ≤ 0.006 sr within any 10° visual field (25 % of that field).
- For software, a **341 × 256 px rectangle at 1024 × 768** approximates a 10° field. That is 1/3 × 1/3 of the screen, so 25 % of it is 2.78 % of the screen. Lower resolutions are used because they are more conservative.
- A transition is the change between adjacent peaks and valleys of the luminance-versus-time plot (Understanding 2.3.1).
- Exemption: fine balanced patterns (white noise, checkerboards) with elements smaller than 0.1°.
- Relative luminance: linearise sRGB with threshold 0.04045 (0.03928 before 2021, "no practical effect"), then `L = 0.2126 R + 0.7152 G + 0.0722 B`.
- HDR (Understanding): for a darker state ≥ 160 cd/m², use Michelson contrast ≥ 1/17. Not relevant here (SDR output).

**ITU-R BT.1702-3 (11/2023), Annex 1.**
- Guideline 1 (flashes):
  - A potentially harmful flash is a pair of opposing luminance changes of **≥ 20 cd/m²** when the darker image is **below 160 cd/m²**. For HDR with darker ≥ 160 cd/m², Michelson > 1/17.
  - A transition **to or from a saturated red** is harmful irrespective of luminance.
  - Isolated single, double or triple flashes are acceptable. A sequence is not permitted when **both** the concurrently flashing area exceeds **25 % of the displayed screen area** **and** there are more than three flashes (six changes) in any 1 s.
  - Successive flashes whose leading edges are ≥ 334 ms apart (60 Hz environments; 360 ms at 50 Hz) are acceptable regardless of brightness or area.
  - Flashing lasting longer than 5 s "might constitute a risk" even when compliant.
- Guideline 2 and Attachment 1 (patterns):
  - Harmful regular patterns have **more than five light-dark pairs** of clearly discernible stripes (any orientation; may be dots in rows).
  - Limits: stationary over **> 40 %** of the screen, or changing, oscillating, flashing or reversing over **> 25 %**.
  - The luminance difference of the bars must be the same as for a harmful flash (≥ 20 cd/m²).
  - Patterns that flow smoothly across, into or out of the screen in one direction are exempt.
- Annex 2: SDR is assumed displayed with **peak white 200 cd/m²**. So WCAG's 0.1 is BT.1702's 20 cd/m², and 0.8 is 160 cd/m²: the two standards agree.

**Harding test.** The Harding FPA (Cambridge Research Systems, 2001; Prof. Graham Harding) is proprietary. It implements the UK Ofcom/ITC guidelines, the parent of BT.1702: luminance flashes, red flashes and stationary patterns, second by second, with a pass certificate. Broadcasters and the UK ASA require a Harding pass "or equivalent". There is no public algorithm, so implement BT.1702 and WCAG directly (below).

**EA IRIS (open source, 2023): the best public reference implementation.**
- 8-bit frame → linear via a 256-entry LUT (IEC formula, 0.04045).
- Y per pixel; red value `320 (R - G - B)` only where R/(R+G+B) ≥ 0.8 (linear), else 0.
- Frame difference. If the count of changed pixels is ≥ 25 % of the frame, the frame's change is the change of the **whole-frame mean**.
- Same-sign changes accumulate over ≤ 1 s. A transition registers once per run when the accumulated change is ≥ 0.1 (red ≥ 20) and the darker frame mean is < 0.8.
- Fail if more than 6 transitions in the trailing 1 s. "Extended fail" if 4-6 transitions per second persist for 4 s of a 5 s window. Warning at ≥ 4 transitions.
- Pattern detection: ≥ 6 stripes, area 25 %, persisting ≥ 0.5 s.
- IRIS dilutes a local flash into the frame mean, so it is lenient for local flashes. The block test below is stricter, and it is the one to gate on.

### 3.2 From captures to the tested quantities

- **Source.** Use the final 8-bit sRGB frames, exactly what a viewer or the YouTube encoder receives: after ThreadGrade, its static TPDF dither, the vignette and UI.
- **Linearise** with the 256-entry LUT `c/12.92` (c ≤ 0.04045) else `((c + 0.055)/1.055)^2.4`.
- **Relative luminance:** `Y = 0.2126 R + 0.7152 G + 0.0722 B` (BT.709 / sRGB, D65).
- **Red share:** `R/(R + G + B)` on **linear** values (IRIS convention, and the convention of the spec's own "≤ 0.59" and "≤ 0.70" numbers). Ignore pixels with `R + G + B < 1e-4`.
  - Large-colour checks for reference: NeonMagenta 0.717, CloudHot 0.585, CloudBase 0.505, wall magenta stop 0.587, KoiPatch 0.67, AviationRed 0.962 (saturated red), SodiumAmber 0.71.
  - In gamma space the same colours give lower shares (NeonMagenta 0.55, AviationRed 0.77), so linear is the conservative choice.
- **XYZ** (IEC 61966-2-1): `X = 0.4124 R + 0.3576 G + 0.1805 B`, `Z = 0.0193 R + 0.1192 G + 0.9505 B`.
- **Blocks.** Average linear RGB over 16 × 16 px at 1920 × 1080 (120 × 67 blocks; drop the last 8 rows). This is what the eye integrates, and averaging in linear space is the physically correct order.
  - The WCAG window is 40 × 22 blocks (1/3 × 1/3 of the frame), slid one block at a time.
- **Display mapping.** Relative 0.1 corresponds to 20 cd/m² at the BT.1702 200 cd/m² SDR peak.

### 3.3 Exact detector

The reference implementation is `photosens.py`, Appendix A. It was self-tested:
- A 4 Hz full-screen flash fails G1 and G2 (8 transitions/s).
- A 2 Hz flash over 27.8 % of the screen fails G1 (spec zero-flash) and passes G2 (4 transitions/s ≤ 6).
- A 5 Hz saturated-red half-screen flash fails G3 and G4.

Per block b, with linear series `Y_b[n]` at fps F (30):

1. **Transitions.** A zig-zag (peak-valley) tracker with threshold 0.1 (WCAG adjacent peaks and valleys). A transition is counted at the frame where the reversal from the running extreme reaches 0.1, and only if the darker side is < 0.8. No time limit: a slow 0.12 swing over 3 s is one transition, as WCAG intends.
2. **Red transitions.** The block is saturated red when R/(R+G+B) ≥ 0.8 (linear). A red transition is counted when, against the last transition state, one of the two states is saturated red and `Δu'v' > 0.2`.
3. **Counts.** `c_b[n]` = transitions in the trailing 1 s (F frames); the same for red, `r_b[n]`.
4. **Flags.** "flashing now" if `c_b ≥ 2` (a rise and a fall within 1 s); "failing now" if `c_b ≥ 7` (more than 3 flashes in 1 s). The same for red.
5. **Areas per frame.** The screen fraction of flashing blocks, and the max over WCAG windows of the window fraction (integral image) for flashing, failing, red-failing and saturated-red blocks.

Gates:

| Gate | Rule | Applies to |
|---|---|---|
| **G1 (spec "zero flashes")** | `max_n screen fraction(c_b ≥ 2) < 0.25`. This is the spec's own definition: an increase and a decrease of ≥ 10 % within 1 s over ≥ 25 % of the screen | KS16 sequence, whole walk, ending |
| **G2 (WCAG 2.3.1 general)** | `max_n max_window fraction(c_b ≥ 7) < 0.25` | both |
| **G3 (WCAG 2.3.1 red)** | the same with `r_b ≥ 7` | both |
| **G4 (red area)** | `max_n max_window fraction(saturated red) ≤ 0.10` (then no red-flash area condition can ever be met) | both; expected < 0.01 |
| **G5 (block frequency)** | per block over 60 s: detrend, Hann window, FFT, amplitude `2|X|/Σw`. Among blocks whose max amplitude ≥ 0.002 (0.2 % relative luminance), the dominant frequency ≤ 0.5 Hz, and the RMS amplitude above 0.5 Hz ≤ 0.002 | KS16 sequence (Δf = 1/60 Hz) |
| **G6 (mean-luma step)** | frame mean of Rec.709 luma on 8-bit codes (`0.2126 R' + 0.7152 G' + 0.0722 B'`): `max |L_n - L_{n-1}| / L_{n-1} ≤ 0.003` | KS16 sequence |
| **G7 (red share)** | p99.9 of the linear red share over wall pixels with Y > 0.01 ≤ 0.70; whole walk: fraction of blocks with share > 0.70 and Y > 0.02 ≤ 2 % per frame | KS16 still, whole walk |
| **G8 (BT.1702 pattern)** | wall mask on the 4K KS16 still: high-pass `Y - GaussBlur(Y, σ = 32 px)`; p99 of `|HP|` ≤ 0.05 (no stripe pair can differ by 0.1). Also log a 2D FFT peak count (≥ 6 cycles) as information | KS16 still; LED grid and seams |
| **Info** | max transitions/s, max window flash fraction (target < 0.25), IRIS-style whole-frame result, BT.1702 "> 5 s of flashing" check (no block flashing in more than 5 consecutive seconds) | logged |

Why the amplitude floor in G5: with rain on (beat 6, rho 0.7), a 16 × 16 block gets random streak hits. My estimate is σ ≈ 0.004, which spreads to about 2e-4 per FFT bin, an order below the 0.002 floor. So G5 works with rain on, and dark sky blocks (no modulation) are excluded instead of reporting noise frequencies. The cloud noise (3 m/s over 300 m, 0.01 Hz) and haze sheets (period 120 s) are far below 0.5 Hz.

### 3.4 Running it

**KS16 sequence (`-sequence signs`).**
- KS16 pose, 60 s, 30 fps virtual time (`Time.captureFramerate = 30`), clock running, beat-6 rain, static dither, 1920 × 1080.
- Stream: read each PNG, reduce it to 120 × 67 linear block means (float32) plus the frame-mean luma, then delete the PNG (Part 3.2 lesson 15). That is 1 800 × 67 × 120 × 3 × 4 B = 174 MB.
- Also capture one 4K still for G7 and G8.

**Whole walk (`-sequence walk`, new).**
- Cinematic **play timing** (`t_w = t_k`, trains on tau), 30 fps virtual time, from black at t = 0 through the ending (617 s + 0.6 s ≈ 18 530 frames).
- Reduce in the player:
  - `AsyncGPUReadback` of the final 8-bit sRGB capture target.
  - A worker thread does the LUT, the 16 × 16 means and the luma sum, and writes a float16 file: 120 × 67 × 3 × 2 B = 48 KB per frame, ≈ 0.9 GB. Use 32 × 32 blocks (230 MB) if disk is tight; WCAG windows are still 20 × 11 blocks.
- Python then runs G1-G4 and G7 plus the info metrics.
- The first and last 60 frames are part of it (defect 22).

**One 60 fps check (10 s each)** at the highest-motion moments: T1 meeting (s 595-605), boulevard traffic (s 250), rain gusts on the bridge (s 700). 30 fps cannot see 15-30 Hz flicker (rail MSAA crawl, rain), and BT.1702 Annex 4 names 10-30 Hz as the provocative band. Gate: the same G1-G3 at F = 60.

### 3.5 The KS16 wall: analysis and required changes

Model: wall plane x = 874, z 275-520, y -1 500..560 seen from the KS16 pose.
- Spec waves and ramp: Violet → Cyan → Magenta → Violet, each stop mixed 25 % with violet.
- Panel level `0.55 + A sin(2π f_p t + φ_p)` per 36 × 20 m panel, with f_p and φ_p from a hash.
- Emission 0.45 × level; cloud mix above y 380; non-wall pixels at 0.03.
- The wall covers 58.4 % of the frame (spec: 59 %). **113 panels are visible, and the nearest one covers 23 % of the frame** (the next 9.8 %).

**Per-pixel bound (T-WAVE).**
- `|dY/dt| ≤ 0.45 [0.8 × 0.986 × 0.0622 + 0.25 × 2π × 0.2 × 0.5315] = 0.097 /s`.
  - 0.0622/s is the max of `|dh/dt| = 2π (0.28/58 + 0.18/60 + 0.12/58)`.
  - 0.986 is the max ramp-luminance slope.
  - Three quarters of the bound is the breath.
- Simulated max 0.084/s; max change over any 1 s 0.076. The spec's "computed bound 0.10" is confirmed.

**Simulation, 60 s at 30 fps** [computed]:

| Variant | Frame-luma step (G6, ≤ 0.3 %) | Frame-luma change | Wall Y change | Transitions ≥ 0.1 | G1-G5 |
|---|---|---|---|---|---|
| **Spec** (breath ±0.25, 0.08-0.20 Hz) | **0.322 %** | 9.8 %/s | **25.8 %/s** | up to 1 per s per block (breath swings 0.12 on cyan) | pass (dominant 0.20 Hz, red share 0.586) |
| Breath ±0.08 | 0.192 % | 5.8 %/s | 11.2 %/s | | pass |
| No breath (waves only) | 0.177 % | 4.7 %/s | 7.4 %/s | | pass |
| Ramp k = 0.6, breath ±0.05 at 0.05-0.10 Hz | 0.129 % | 3.5 %/s | 4.4 %/s | | pass |
| **Recommended: ramp k = 0.8, breath ±0.05 at 0.05-0.10 Hz** | **0.111 %** | 3.0 %/s | **3.0 %/s** | **0** | pass (dominant 0.10 Hz, HF RMS 6e-5) |
| Ramp k = 1.0 (isoluminant), breath ±0.05 | 0.092 % | 2.5 %/s | 2.2 %/s | 0 | pass |

Findings:
- The spec wall **fails G6** (0.322 % > 0.3 %) and misses its own "wall mean change ≤ 4 % per second" (14.5.6) by ×6. The cause is the per-panel breath on the giant near panel; the waves alone also exceed 4 %/s.
- The waves exceed 4 %/s because the near wall is a small fraction of a wavelength. The whole visible near region therefore changes colour coherently, and the ramp's luminance contrast is 2.6:1 (violet stop Y 0.203, cyan 0.532, magenta 0.245).
- **Luminance normalisation k.** Scale each stop by `(Y_target / Y_stop)^k`, with Y_target the geometric mean of the stops. A scalar per stop keeps every token's chromaticity: palette appearance, red share 0.587 and hue all unchanged. Only cyan crests get less bright.
  - At k = 0.8 the stops are 0.255/0.310/0.265 (ratio 1.21). The wave becomes a hue wave with ±10 % luminance, which is exactly "slow gradient waves, no flashing".
- Breath ±0.05 (±9 %) at 0.05-0.10 Hz keeps the panels alive without pumping. Per Part 14.10 the threshold is not relaxed; the picture changes.
- With the recommended variant the T-WAVE per-pixel bound drops to `0.45 [0.6 × 0.163 × 0.0622 + 0.05 × 2π × 0.1 × 0.31] ≈ 0.007 /s`.
- The 12 wall lights (≤ 0.2 Hz) and the floor mirror use the same function, so they inherit the fix.
- **LED grid and seams (G8).**
  - Seams are 0.4 m on a 36 × 20 m panel: duty 0.02, fundamental `(2d/π) sin(π × 0.02) ≈ 0.04 d`, negligible.
  - The 4 m LED grid must stay at modulation depth ≤ 15 % of the panel luminance. At Y ≈ 0.12 that is ΔY ≤ 0.02, far below the 0.1 pattern threshold, with the spec's < 5 px fade.
  - The wall covers 59 % > 40 % (BT.1702's stationary limit), so the pattern rule must be met through contrast, not area.

### 3.6 The whole walk: risk register and design rules

| Source | Mechanism | Rule (makes the gate safe by construction) |
|---|---|---|
| Title and ending fades | monotonic, at most one transition | none; checked by G1 on the first and last 60 frames |
| **Train window strips (T1-T7)** | the strip goes dark at each car gap; the relative speed of 40.5 m/s over a 24 m car pitch flickers each strip pixel at 1.7 Hz. At 12 m (KS11) a 1 m strip is 4.8° tall, about 22 % of a WCAG window | strips continuous across gaps, dip ≤ 0.08 relative luminance (bellows lit at ≥ 70 % or a 2 m taper). The car-end edge then makes no transition |
| Pans across blades | a 14 m blade at 100 m (8°) passes a pixel in 0.4 s at the 19.3 °/s peak gaze rate: one flash per pixel, per-blade area < 25 % of the screen | no change; G1 and G2 margin logged. Do not add more than one blade per 1/3 window in a pan |
| **Koi scales in free-look under the koi** | 6 m cells at 20 m/s give 3.3 Hz per pixel; the koi can exceed 25 % of the frame looking up | hex line ΔY ≤ 0.05 (§2.5), so not a transition |
| Koi patches and scanlines | ≤ 0.35 Hz and ≤ 0.47 Hz | none |
| Aviation beacons | saturated red, 0.2 Hz, point-sized | G4 (area) |
| Red tail-light streaks (boulevard, layers) | saturated red if AviationRed-like; moving | G4; keep the total red streak area < 1 % of the frame |
| Rain streaks | per-pixel 1-2 frame blips, < 1 % coverage | none (G1 area, G5 floor) |
| Blades, signs, ring lights breathing | ≤ 0.2 Hz, ±8 % | none |
| Glass strip view of T6 | cars 142 m below, slow angular motion | none |
| Sign or window pops | would be single transitions | covered by 14.10 pop tests |

Forbidden effects (state them in the materials sheet):
- Any strobe, lightning or blink above 0.2 Hz.
- Hologram glitch or flicker.
- Neon "buzz" flicker (sound only).
- A UI flash.
- A white or red fade.

---

## 4. Sources (all accessed 2026-11-13)

| Source | URL | Used for |
|---|---|---|
| W3C, WCAG 2.2 Recommendation | https://www.w3.org/TR/WCAG22/ | SC 2.3.1 text; definitions of general and red flash thresholds (0.1, 0.8, 0.006 sr, 341 × 256 at 1024 × 768, R/(R+G+B) ≥ 0.8, Δu'v' > 0.2); relative luminance and the 0.04045 note |
| W3C, Understanding SC 2.3.1 | https://www.w3.org/WAI/WCAG22/Understanding/three-flashes-or-below-threshold.html | transition = adjacent peak and valley; u'v' formulas; ISO 9241-391 attribution; HDR 160 cd/m² and 1/17; techniques G19, G176, G15 |
| ITU-R BT.1702-3 (11/2023), PDF | https://www.itu.int/dms_pubrec/itu-r/rec/bt/R-REC-BT.1702-3-202311-I!!PDF-E.pdf | flash rule (20 and 160 cd/m², 25 % of screen, > 3 per s, 334 and 360 ms, red, the 5 s note), pattern attachment (> 5 pairs, 40 % and 25 %, flowing exempt), 200 cd/m² SDR assumption, Annex 4 (10-30 Hz) |
| Electronic Arts IRIS (GitHub): README, `config/appsettings.json`, `src/Flash.cpp`, `src/RedSaturation.h`, `src/RelativeLuminance.h`, `src/TransitionTracker.cpp`, `src/FrameRgbConverter.cpp` | https://github.com/electronicarts/IRIS | reference algorithm: LUT linearisation, red value 320 (R - G - B), 25 % area, 0.1 and 20 thresholds, dark 0.8, 1 s accumulation, > 6 transitions fail, extended 4 of 5 s, pattern defaults |
| Wikipedia, "Harding test" | https://en.wikipedia.org/wiki/Harding_test | what the Harding FPA checks, Ofcom/ITC origin |
| UK ASA, "Flashing images in advertising" (search result) | https://www.asa.org.uk/news/flashing-images-in-advertising.html | the "Harding or equivalent" practice (context only) |
| G. V. Lauder and E. D. Tytell, "Hydrodynamics of undulatory propulsion", Fish Physiology 23 (2006), PDF | https://bpb-us-e1.wpmucdn.com/sites.harvard.edu/dist/6/58/files/2022/03/Lauder.Tytell.2006.pdf | modes; slow-speed envelopes; anterior stillness below 1 L/s; St ≈ 0.3 (eels 0.31-0.41, slow salmon > 0.6); EBT w = W (V-U)/V; η formula |
| Imperial College BioFluids lectures 7-8, "Slender fish" (PDF) | https://www.ma.ic.ac.uk/~ajm8/BioFluids/lec1107.pdf | EBT derivation: mean thrust ∝ V² - U², η = (U+V)/2V, recoil terms |
| M. S. U. Khalid et al., "Larger wavelengths suit hydrodynamics of carangiform swimmers" (arXiv 2010.04748) | https://ar5iv.arxiv.org/html/2010.04748 | envelope 0.02 - 0.0825 x + 0.1625 x² (saithe, Videler 1993); λ/L ≈ 1 (jack 1.05 L); St 0.30 |
| SICB abstract, "Revisiting the relationship between tail beat frequency, amplitude and speed" | https://sicb.org/abstracts/revisiting-the-relationship-between-tail-beat-frequency-amplitude-and-speed-in-swimming-fishes/ | Bainbridge 1958 summary (speed proportional to frequency, amplitude about constant) |
| Cheng and Blickhan (JEB 192), via search summary | https://cob.silverchair.com/jeb/article-pdf/192/1/169/2854472/jexbio_192_1_169.pdf | EBT efficiency 0.67-0.74 and speed ratio 0.65-0.70 for saithe (search summary only, not read in full) |
| Wikipedia, "Kōhaku (fish)" | https://en.wikipedia.org/wiki/K%C5%8Dhaku_(fish) | Kohaku rules: head hi, nidan, sandan, yondan, lateral line, odome, faults |
| Pond Trade Magazine, koi glossary (search summary) | https://www.pondtrademag.com/language-of-koi-glossary/ | kiwa (crisp trailing edge), sashi (soft leading edge, ≤ 2 scales) |
| Wikipedia, "Common carp" | https://en.wikipedia.org/wiki/Common_carp | two pairs of barbels, large pectorals, dorsal fin along the back, wild length:depth ≈ 4 |
| B. Golus, "The Best Darn Grid Shader (Yet)" and gist | https://bgolus.medium.com/the-best-darn-grid-shader-yet-727f9278b9d8 , https://gist.github.com/bgolus/d49651f52b1dcf82f70421ba922ed064 | line AA with derivative-sized smoothstep, minimum draw width with energy compensation, fade to coverage (§2.5 is my own adaptation to hexes) |
| URP 17.6.0 package source | `Library/PackageCache/com.unity.render-pipelines.universal@8457e85b8184/Runtime/Passes/DrawObjectsPass.cs:204`; `Runtime/UniversalRenderer.cs:411`; `Runtime/Passes/ScriptableRenderPass.cs:143,148,153` | transparent sorting, the transparent pass event and queue range |
| Core RP 17.6.0 package source | `Library/PackageCache/com.unity.render-pipelines.core@2d66c71e606e/ShaderLibrary/API/D3D11.hlsl:10-12` | `FRONT_FACE_SEMANTIC`, `FRONT_FACE_TYPE`, `IS_FRONT_VFACE` |
| Unity 6000.6.0f1 `UnityEngine.CoreModule.dll` | `C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll` | `Renderer.set_localBounds` and `ResetLocalBounds` present |
| Thread (game 3) | `Assets/_Game/Shaders/ThreadGrade.shader:83-132,165-235`; `Assets/_Game/Scripts/Runtime/Rendering/ThreadGradeMath.cs:34-36` | prefilter, Karis average, Neutral curve (start 0.76, desaturation 0.15), dither edge fade, used in the §2.2 table |
| Abyss (game 1) | `docs/SHADERS.md:74` | the additive fog form |

---

## Appendix A: `photosens.py` (reference implementation, numpy only, self-tested)

```python
import numpy as np

_c = np.arange(256) / 255.0
SRGB_LUT = np.where(_c <= 0.04045, _c / 12.92, ((_c + 0.055) / 1.055) ** 2.4)   # IEC 61966-2-1 / WCAG 2.2
LUM = np.array([0.2126, 0.7152, 0.0722])
XYZ = np.array([[0.4124, 0.3576, 0.1805], [0.2126, 0.7152, 0.0722], [0.0193, 0.1192, 0.9505]])

def blocks_from_srgb8(img, block):                      # img (H, W, 3) uint8 sRGB -> (BY, BX, 3) linear means
    lin = SRGB_LUT[img]; H, W, _ = lin.shape; by, bx = H // block, W // block
    return lin[:by * block, :bx * block].reshape(by, block, bx, block, 3).mean(axis=(1, 3))

def zigzag_transitions(y, thr, dark_max=None):          # WCAG: change between adjacent peak and valley
    out = []; hi = lo = ext = y[0]; d = 0
    for i in range(1, len(y)):
        v = y[i]
        if d == 0:
            hi, lo = max(hi, v), min(lo, v)
            if hi - v >= thr:
                if dark_max is None or v < dark_max: out.append(i)
                d, ext = -1, v
            elif v - lo >= thr:
                if dark_max is None or lo < dark_max: out.append(i)
                d, ext = 1, v
        elif d == 1:
            if v > ext: ext = v
            elif ext - v >= thr:
                if dark_max is None or v < dark_max: out.append(i)
                d, ext = -1, v
        else:
            if v < ext: ext = v
            elif v - ext >= thr:
                if dark_max is None or ext < dark_max: out.append(i)
                d, ext = 1, v
    return out

def analyse(blocks, fps, win_frac=1/3):                 # blocks (N, BY, BX, 3) linear RGB
    N, BY, BX, _ = blocks.shape
    Y = blocks @ LUM; S = blocks.sum(-1)
    share = np.where(S > 1e-4, blocks[..., 0] / np.maximum(S, 1e-12), 0.0)
    X3 = blocks @ XYZ.T; den = X3[..., 0] + 15 * X3[..., 1] + 3 * X3[..., 2]
    up = np.where(den > 1e-9, 4 * X3[..., 0] / np.maximum(den, 1e-12), 0.2105)
    vp = np.where(den > 1e-9, 9 * X3[..., 1] / np.maximum(den, 1e-12), 0.4737)
    sat = share >= 0.8
    T = np.zeros((N, BY, BX), np.int8); R = np.zeros_like(T)
    for j in range(BY):
        for i in range(BX):
            for k in zigzag_transitions(Y[:, j, i], 0.1, 0.8): T[k, j, i] = 1
            if sat[:, j, i].any():
                last = 0
                for n in range(1, N):
                    if (sat[n, j, i] or sat[last, j, i]) and \
                       np.hypot(up[n, j, i] - up[last, j, i], vp[n, j, i] - vp[last, j, i]) > 0.2:
                        R[n, j, i] = 1; last = n
    w = int(round(fps))
    def trailing(x):
        c = np.cumsum(x, axis=0, dtype=np.int32)
        return c - np.concatenate([np.zeros((w, BY, BX), np.int32), c[:-w]])
    cnt, cntr = trailing(T), trailing(R)
    wy, wx = max(1, round(BY * win_frac)), max(1, round(BX * win_frac))
    def win_max(m):
        ii = np.pad(m.astype(np.float32).cumsum(1).cumsum(2), ((0, 0), (1, 0), (1, 0)))
        s = ii[:, wy:, wx:] - ii[:, :-wy, wx:] - ii[:, wy:, :-wx] + ii[:, :-wy, :-wx]
        return s.reshape(len(m), -1).max(1) / (wy * wx)
    r = dict(screen_flash_area=(cnt >= 2).reshape(N, -1).mean(1), window_flash_area=win_max(cnt >= 2),
             window_fail_area=win_max(cnt >= 7), window_red_fail_area=win_max(cntr >= 7),
             window_sat_red_area=win_max(sat), max_transitions_per_s=int(cnt.max()),
             red_share_p999=float(np.percentile(share[Y > 0.01], 99.9)) if (Y > 0.01).any() else 0.0)
    r["G1"] = bool(r["screen_flash_area"].max() < 0.25)
    r["G2"] = bool(r["window_fail_area"].max() < 0.25)
    r["G3"] = bool(r["window_red_fail_area"].max() < 0.25)
    r["G4"] = bool(r["window_sat_red_area"].max() <= 0.10)
    return r, Y

def block_dominant_frequency(Y, fps, amp_floor=0.002, f_max=0.5):   # G5
    N = Y.shape[0]; B = Y.reshape(N, -1).astype(np.float64); t = np.arange(N)
    A = np.vstack([t, np.ones(N)]).T; B = B - A @ np.linalg.lstsq(A, B, rcond=None)[0]
    win = np.hanning(N); amp = 2 * np.abs(np.fft.rfft(B * win[:, None], axis=0)) / win.sum()
    f = np.fft.rfftfreq(N, 1 / fps); amp[0] = 0
    valid = amp.max(0) >= amp_floor; dom = f[amp.argmax(0)]; hf = np.sqrt((amp[f > f_max] ** 2).sum(0))
    return dict(dominant_max=float(dom[valid].max()) if valid.any() else 0.0, hf_rms_max=float(hf.max()),
                G5=bool((dom[valid] <= f_max).all() and hf.max() <= amp_floor))
```

G6: `max |L_n - L_{n-1}| / L_{n-1}`, where L_n is the frame mean of `0.2126 R' + 0.7152 G' + 0.0722 B'` on 8-bit codes. G7 and G8 are computed on the KS16 4K still as in §3.3.

## Appendix B: koi reference model (for T-KOI, the composition lint and the H6 tests)

```text
S = (1330, 388); L = 900; R(φ) = 260 + 18 sin(2φ + 0.7); y(φ) = 300 + 60 sin(φ + 0.4)   (φ in rad, +X toward +Z)
l(φ) = 260 φ - 9 cos(2φ + 0.7)                     plan arc length, so L = l(φ_h) - l(φ_tail) = 900
φ_h(s) = (6.64 + 1.76 (s - 1260)) deg              capture form; play form via u (H6)
φ_a: l(φ_a) = l(φ_h) - a   (Newton, 3 iterations, step (l - target)/R)
C(a) = (Sx + R cos φ_a, y(φ_a), Sz + R sin φ_a);  n(a) = (cos φ_a, 0, sin φ_a)
w(a, t) = 40 E(a/L) sin(2π (a/810 - t/20)),  E = 0.15 + 0.85 f²  (or the pivot form 0.15 - 0.6 f + 1.45 f²)
P(a) = C + w n;  T = normalize(dP/da) (analytic, §1.6);  lateral = normalize(cross(up, T));  ring up = cross(T, lateral)
radius r(a) = 48 × piecewise-linear profile {0:0.22, .06:.60, .18:.92, .30:1.0, .50:.90, .80:.45, 1:.20}
surface = P + r (cos θ lateral + sin θ up)
camera: forward (sin ψ cos θp, sin θp, cos ψ cos θp); right (cos ψ, 0, -sin ψ); up = forward × right
ndc x = (d·right)/(d·forward)/(tan(vFOV/2) aspect);  ndc y = (d·up)/(d·forward)/tan(vFOV/2)
phases: t = t_w + k, k = 0..19 s;  arcade occluder box x 914-990, z 236-360, y 0-38
```

Expected [computed]:
- KS17: x -0.498..+0.719, y +0.090..+0.919, min margin 0.081 over three aspects.
- KS18: x -0.377..+0.565, y -0.543..+0.820, margin 0.180.
- Body top 408.0, bottom 192.1.
- Max spine displacement per 60 fps frame: 0.321 m at 1.5 m/s, 0.423 m at 2.5 m/s, wave-only 0.209 m.
- Angular speeds: KS17 1.5-2.2 °/s, KS18 1.6-3.6 °/s.
