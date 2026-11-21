# Research log — Bulwark

Date: 2026-11-13. Owner of this file: the scribe. Project: Bulwark (game 4), `<WORKSPACE>\4 Megastructures\4 Megastructures`.

## Method

- **Scope.** MASTER_PROMPT Part 14.9 asks for research before building. Three read-only research passes ran in parallel on 2026-11-13:
  - **A** (`research-sea-dam`): 14.9 items 1–2 and hypothesis H1 — the ocean without sparkle, the wall, dams, spillways and breakwaters.
  - **B** (`research-mist-optics`): 14.9 items 3–7 and hypotheses H2, H3, H4, H5, H7 — waterfall and mist, rainbows, underwater optics, high-altitude atmospherics, filtering and dither, and URP 17.6 specifics read from the package source.
  - **C** (`research-audio-ui`): 14.9 item 8, 14.3 and 14.11 — procedural low-frequency water audio, FDN reverb, look-ahead limiter, modal footsteps, `OnAudioFilterRead` in Unity 6000.6, and UI Toolkit runtime text.
- **Sources.** Web pages (opened, or only seen as search-result summaries), the local package source under `Library\PackageCache` (URP 17.6.0 `com.unity.render-pipelines.universal@8457e85b8184`, Core 17.6.0 `com.unity.render-pipelines.core@2d66c71e606e`), the local Unity 6000.6.0f1 install (documentation, `CGIncludes`, resources), this project's settings, and the code, logs and research notes of the earlier games (Thread, The Ring, Abyss), read-only.
- **Arithmetic.** Every number marked "computed" (A) or DERIVED (C) was evaluated in Python (`python -I`) during the research, not copied from a source. The Descartes and Airy values of B§3.4 were likewise computed in the session.
- **Reliability tags.** Each report keeps its own tags:

  | Report | Tag | Meaning |
  |---|---|---|
  | A | "computed" | Evaluated in Python from the H1 formulas. |
  | A | "not re-fetched", "search snippet only", "from memory, verify", "Not verified" | Inline caveats; check before quoting. |
  | B | [F] | Fetched and checked in this session ("[F, via search]": the content was confirmed through a search result). |
  | B | [S] | Standard literature reference, not re-fetched; numbers approximate; check before a test threshold relies on them. |
  | C | VERIFIED-LOCAL | Read from this machine's Unity install, this project, or the earlier games' code, logs and assets. |
  | C | VERIFIED-DOCS | An official page that was opened. |
  | C | SEARCH | From a search-result summary only; verify before relying on it. |
  | C | DERIVED | Arithmetic done here (`python -I`). |
  | C | CHOSEN | Not in the specification; a proposed value that must be logged as a decision. |

- **Editing.** The three reports are reproduced faithfully: every formula, number, code block and URL is kept. Only the formatting changed: each report is one H2 below, and its own headings moved down one or two levels. Each report keeps its own section numbers, so this log cites them as A§n, B§n and C§n (B starts at 3 because it covers items 3–7; C starts at 0).
- **Decisions are not settled here.** Anything a report calls a decision, deviation, inconsistency, conflict or CHOSEN value is to be logged in `DEVELOPMENT.md` (or as a 14.10.5 arbitration) by the lead. Where they are:
  - A: §1.3 (optional Donelan–Banner spreading), §2.5 (Eulerian vs Lagrangian mean level), §3.3 (H1 far-field D numbers), §4.3 (glint gating by sun visibility), §4.6 (foam threshold unreachable; statistical replacement), §6.2 (approach velocity at x 46), §6.4 (Colossopod slimmer than a real Tetrapod; placement resolver).
  - B: the list "Spec deviations and decisions found by this research" (items 1–10).
  - C: §0 (all CHOSEN values), §3 (relay clicks "at each switch"; footsteps in S5; pause duck −12 dB), §4.2 (room HF ratios and wets), §7 (T-AUDIO interpretations), §9.1 and §9.4 (hint size), §9.6 (letter-spacing correction factor, linear vs gamma blending).
- **Sources.** Each report's own source list is kept in place. The consolidated [Source index](#source-index) at the end lists every URL once.

## Contents

- [A. Ocean without sparkle, dams and breakwaters (14.9 items 1–2, H1)](#report-a)
  - 1 Wave model · 2 Vertex stage · 3 Pixel stage · 4 Lighting · 5 The wall (clapotis) · 6 Dams and breakwaters · 7 HLSL skeleton · 8 Tests this research feeds · 9 Sources
- [B. Waterfall, mist, rainbows, underwater, high altitude, filtering, URP 17.6 (14.9 items 3–7; H2, H3, H4, H5, H7)](#report-b)
  - 3 Waterfall physics and rainbow optics · 4 Underwater optics · 5 High-altitude atmospherics · 6 Filtering and dither · 7 URP 17.6 specifics · Spec deviations and decisions · Sources
- [C. Audio and UI (14.9 item 8, 14.3, 14.11)](#report-c)
  - 0 Decisions in one screen · 1 `OnAudioFilterRead` in 6000.6 · 2 DSP building blocks · 3 Water and layer recipes · 4 FDN reverb · 5 Look-ahead limiter · 6 Footsteps · 7 T-AUDIO measurement notes · 8 Reuse from Thread and The Ring · 9 UI Toolkit runtime text · 10 Sources
- [Source index](#source-index)

---

<a id="report-a"></a>

## A. Research: 14.9 items 1–2 and H1. Ocean without sparkle, dams and breakwaters

*Sources accessed 2026-11-13. Every number marked "computed" came from a Python evaluation of the H1 formulas. Each piece of advice targets a handwritten URP 17.6 HLSL shader: a clipmap vertex stage plus a per-pixel normal built from 32 components. The source list is at the end.*

---

### 1. Wave model: spectrum, 32 components, conventions

**1.1 Spectrum.** H1 uses the omnidirectional spectrum `S(k) = 0.5 k^-3 exp(-(kp/k)^2)`, with `kp = (2π/Tp)^2 / g` and `ω = sqrt(g k)`.
- Its `k^-3` tail is Phillips' saturation range. Tessendorf's directional spectrum `P(k) = A exp(-1/(kL)^2) / k^4 |k̂·ŵ|^2` with `L = V²/g` (eq. 40) integrates over direction to the same `k^-3`. His `exp(-k²l²)` small-wave suppression (eq. 41) is not needed, because the component set stops at 0.5 m.
- JONSWAP in the wavenumber domain, if more peakiness is wanted later: `S(k) = (α/2) k^-3 exp(-5/4 (kp/k)^2) γ^r`, with `r = exp(-(sqrt(k/kp) - 1)^2 / (2σ²))`, γ = 3.3, σ = 0.07 below the peak and 0.09 above. This is from Horvath 2015, eq. 28, with ω/ωp = sqrt(k/kp).
- The spec's cutoff `exp(-(kp/k)^2)` is a little broader than the Pierson–Moskowitz `exp(-5/4 (kp/k)^2)`. Keep it; Hs is normalised afterwards anyway.
- With log-spaced components, amplitude peaks where `k·S(k)` peaks, which is exactly at `k = kp`. The stated peak wavelengths are therefore right: 126.5, 99.9 and 188.9 m. (`S(k)` on its own peaks at 0.816 kp.)

**1.2 Discretisation.**
- `k_i = 0.6 kp · r^i`, i = 0..31, with `r = (k_max / k_min)^(1/31)` and `k_max = 2π/0.5`.
- Bin width `Δk_i = k_i (r^0.5 - r^-0.5)`.
- Amplitude `a_i = sqrt(2 S(k_i) Δk_i)`, then scaled so that `Hs = 4 sqrt(Σ a_i²/2)`.
- In this discretisation `a_i k_i → sqrt(2 C ln r)` for short waves: every short component has the same steepness, about 0.031.

| Sea state | λ_max m | ratio r | Σ a k | mss = Σ(ak)²/2 | rms slope | largest a (λ) | Eulerian mean offset λΣa²k/2 |
|---|---|---|---|---|---|---|---|
| B2 Hs 2.8 / Tp 9 | 210.8 | 1.2153 | 0.905 | **0.01335** | 0.116 | 0.379 m (117 m) | -0.026 m |
| B3 Hs 2.0 / Tp 8 | 166.5 | 1.2061 | 0.800 | 0.01043 | 0.102 | 0.266 m (95 m) | -0.017 m |
| B4 Hs 3.2 / Tp 11 | 314.9 | 1.2311 | 0.718 | 0.00839 | 0.092 | 0.445 m (208 m) | -0.023 m |

Wave periods run from 11.6 s / 10.3 s / 14.2 s for the longest component down to 0.57 s for the 0.5 m component. That is 17 frames per period at 30 fps, so there is no temporal aliasing.

**1.3 Directions and phases.**
- Waves travel west: `D_i = (-cos ψ_i, sin ψ_i)` in (x, z).
- Use a low-discrepancy spread, `ψ_i = 35° (2 frac(0.618034 i + u0) - 1)`, rather than independent draws. Then neighbouring wavelengths never share a direction, and 32 components really fill ±35°.
- Phases come from PCG on the CPU in C#, passed as uniforms. No hashing is needed in HLSL.
- Optional, as a logged deviation: Donelan–Banner spreading (Horvath eq. 38) is narrow at the peak (sech² half-width about 21°) and broadens strongly for short waves. Widening ψ for λ < 10 m toward ±60–80° makes the near sea look less "corduroy".
- Consequence of ±35° uniform spreading: the slope covariance is about 7.7 : 1 anisotropic. Mean cos²ψ is 0.885, so σx² ≈ 0.885·mss and σz² ≈ 0.115·mss. For comparison, Cox–Munk up-wind/cross-wind is about 1.5 : 1. Carry the full 2×2 covariance (section 3.2); an isotropic s2 would put the glitter in the wrong shape.

**1.4 Sign convention (a common bug).** Use `η = Σ a cos θ`, `θ = k (D·x0) - ω t + φ`, and horizontal position `x = x0 - λ Σ a D sin θ`.
- With the minus sign, crests compress: `∂x/∂x0 = 1 - λ a k cos θ`.
- Tessendorf's eq. 44 with `+λD` and his `e^{+iωt}` convention stretches crests and folds troughs if it is transcribed literally with a cos-crest. Foam then appears in the troughs.
- "Steepness 0.6" here means λ = 0.6 on every component. Σ λ a k = 0.54 / 0.48 / 0.43, which is below 1, so no loops can form on the open sea (GPU Gems 1, ch. 1).

**1.5 Precision.**
- On the CPU, in double, each frame: `phase_i = (k_i D_i·O - ω_i t + φ_i) mod 2π`. O is a reference origin, for example the level-6 snap origin.
- The shader adds `k_i D_i·(x0 - O)`. Active components never exceed about 600 rad, because short waves are filtered out before they get far away.
- The soak (30 min) and long sessions stay exact. A frozen capture clock gives exact stills.

---

### 2. Vertex stage: clipmap, ring weights, geomorph, sphere, wall

**2.1 Per-ring weight.** `w_i = 1 - smoothstep(0.125, 0.25, s_eff / λ_i)`. A component is full above 8 spacings and gone below 4. Computed for B2:

| Level | Spacing m | Components full / partial | Displaced Hs |
|---|---|---|---|
| 0 | 0.5 | 21 / 3 | 2.80 |
| 3 | 4 | 10 / 4 | 2.75 |
| 4 | 8 | 7 / 3 | 2.60 |
| 5 | 16 | 3 / 4 | 2.09 |
| 6 | 32 | 0 / 3 | 0.84 |

For B4 the displaced Hs at level 6 is 1.89. Geometry beyond about 1 km is therefore flatter than the true sea. That is acceptable: at 1–2 km the whole relief is 1–3 px at 4K, and the BRDF (Smith masking, section 3.3) carries the look.

**2.2 Geomorph continuity (must do).**
- Morph the vertex position as in Hoppe's geometry clipmaps: odd vertices slide to the coarse-edge midpoint over the outer 15 %.
- Use `α = saturate((max|x0 - cam| - (H_k - W - 2 s_k)) / W)`, measured from the continuous camera position, not the snapped centre. The extra `2 s_k` keeps the ramp inside the level after a snap.
- Also morph the weight spacing: `s_eff = s_k (1 + α)`. At the boundary the fine level then equals the coarse level exactly. Otherwise displacement jumps between levels and opens cracks.
- Compute even-vertex positions from integer indices with identical arithmetic in both levels.
- Add a 1 m downward skirt on each level's outer edge. MSAA otherwise shows hairline background through T-junction rounding (Part 10 #7). A skirt avoids degenerate triangles (T-MESH).

**2.3 Sphere.**
- The sea sphere is centred under the camera: radius `R_sea = 6371000 + 5992`, the same value as in BwFarField (H4).
- Bend each vertex by `y -= r² / (R_sea + sqrt(R_sea² - r²))` with `r = |x0 - cam.xz|`. This is stable, with no `R - R cos`.
- The drop is 0.33 m at 2048 m, 0.08 mm at 100 m. Do not centre the sphere at the world origin: the sea at z = 1500 would sink by 0.18 m against the straight dam.

**2.4 Wall and clipping.**
- Clamp `x0.x ≥ 6.0`, then clamp the displaced `x ≥ 6.0`. In the pixel shader, `clip(posWS.x - 6.0)` so no sea shows under the deck or past the lee face.
- With the image wave (section 5) at Kr = 1, horizontal displacement is exactly 0 at x = 6, so the water stays on the wall.
- Inside the spillway slots (z 420–704, x 6–46) either clip the clipmap and let the flow surface take over, or add the draw-down to the base height.

**2.5 Cost and offset.**
- The vertex cost is trivial: 229 k vertices × at most 24 active components.
- An optional optimisation shifts each level's centre east by `0.5 H_k`. Nesting holds, and half the vertices stop being wasted west of the wall.
- To make the Eulerian mean level exactly -8.000, raise the base by `λ Σ a²k/2` (0.026 / 0.017 / 0.023 m). Alternatively, define the 14.5.2 acceptance as the Lagrangian mean, which is exact.

**2.6 Other passes must match.** DepthOnly and DepthNormals must run the same displacement include as ForwardLit. MistVolumeFeature reads the MSAA depth (H2). A flat depth under a displaced colour surface would put mist halos on every crest.

---

### 3. Pixel stage: footprint filtering, normal, variance

**3.1 Footprint.**
- Interpolate the undisplaced, camera-relative `x0` (xz), with no `centroid` qualifier so derivatives stay valid.
- Take `dPdx = ddx(x0)` and `dPdy = ddy(x0)`. Do not use ddx of the displaced position: it is noisy at crests.
- Per component, project the footprint onto the wave direction: `f_i = length(float2(dot(dPdx, D_i), dot(dPdy, D_i)))`. This equals the spec's "footprint along the view" when D_i points along the view (beats 2 and 4, looking east). It keeps detail for waves crossing the view (looking north along the road), and costs two dot products.
- Then `w_i = 1 - smoothstep(0.5, 1.0, 4 f_i / λ_i)`.
- The spec's footprints check out (4K, vFOV 64, pixel angle 2 tan 32° / 2160 = 5.786e-4 rad, eye 9.7 m above the sea): 0.60 m at 100 m, 59.6 m at 1 km, 537 m at 3 km. With curvature, the 3 km value is 579 m.

**3.2 Accumulators per component.** Loop over all 32 and skip `sincos` with `[branch] if (w > 0)`. With `θ = k D·x0 + phase`, `s, c = sincos θ`, `ak = a k w`:
- `slope += -ak s D` (∂η/∂x0, ∂η/∂z0)
- `Jxx -= λ ak c Dx²`, `Jzz -= λ ak c Dz²`, `Jxz -= λ ak c Dx Dz`
- `u = (1 - w²)(a k)²/2`; `cov += u (Dx², Dz², Dx Dz)`. This is the unresolved slope covariance: the spec's formula, written per direction as in LEAN mapping.

Normal: `Tx = (Jxx, slope.x, Jxz)`, `Tz = (Jxz, slope.y, Jzz)`, `N = normalize(cross(Tz, Tx))`. For a flat sea this gives +Y. Tilt by the sphere normal (`-r/R_sea`) so the reflection converges on the true horizon. If `dot(V, N) < 0`, use `N = reflect(N, V)` (Bruneton's back-facing fix).

**3.3 Roughness (spec 14.7 H1).**
- `Σ = diag(0.004, 0.004) + cov`. The floor 0.008 is split across the two axes.
- Plus a geometric specular-AA backstop on the resolved normal: `var_g = 0.25 (|ddx N|² + |ddy N|²)`, clamped at 0.01 and added isotropically. This is the same form as URP core `GeometricNormalVariance` in `Library\PackageCache\com.unity.render-pipelines.core@2d66c71e606e\ShaderLibrary\CommonMaterial.hlsl` (lines 209–215; projected-space variant at 192–202, after Tokuyoshi & Kaplanyan 2021). It catches the band where w ≈ 0.5.
- **Numeric inconsistency in H1, needs a logged decision.** With `s2 = 0.008 + Σ(1 - w²)(ak)²/2`, the fully unresolved s2 is 0.0214 / 0.0184 / 0.0164 (B2 / B3 / B4), so the far-field `D = 1/(π s2)` is **14.9 / 17.3 / 19.4**.
  - The spec's "23.9 for the full spectrum" and "0.0133 at 3 km" are the B2 component sum without the floor (computed 0.01335, D 23.8).
  - "At most 39.8" (floor only, every component resolved, near field) is correct.
  - Resolved rms slope for B2: 0.116 at 30 m (matches), 0.091 at 100 m, 0.053 at 300 m (spec 0.060), 0 at 1 km.
  - Keep the formula (the floor stands for waves under 0.5 m, which it should) and correct the quoted numbers.
- Context: Cox–Munk give a real total mss of `0.003 + 5.12e-3 U`, about 0.066 at the Pierson–Moskowitz wind for Tp 9 (12.3 m/s). Most of a real sea's slope variance is in waves under 0.5 m, so 0.008 is a smooth, safe floor, not a physical one.

**3.4 Precision and cost.** Use camera-relative positions only. Expect about 25 ALU per active component; near the camera with the image wave, about 64 sincos. That is negligible on the RTX 5070 at 4K.

---

### 4. Lighting: glint, sky reflection, body, foam, fog, horizon

The formulas below are from Bruneton, Neyret & Holzschuch 2010, verified in the authors' shader (`Ocean.shader` in the Brunetons-Ocean port, INRIA BSD-3 licence; credit it in a code comment if adapted).

**4.1 Slope pdf (anisotropic Gaussian).** `p(ζ) = exp(-½ ζᵀ Σ⁻¹ ζ) / (2π sqrt(det Σ))`, where ζ is the half-vector slope in the mean-normal frame. The isotropic peak is `1/(π s2)`, which is the spec's D.

**4.2 Sun glint.**
`L_sun = E_sun T_sun F(v·h) p(ζ_h) / (4 max(n·v, 0.01) (n·h)^4 (1 + Λ(l) + Λ(v)))`, then `min(·, 8)` (spec clamp; bloom clamp 40).
- Fresnel: `F = 0.02 + 0.98 (1 - v·h)^5`. Water F0 = ((1.333 - 1)/(1.333 + 1))² = 0.020.
- Smith masking: `Λ(μ) = (exp(-ν²) - ν sqrt(π) erfc(ν)) / (2 ν sqrt(π))` with `ν = μ / sqrt(2 σ_dir² (1 - μ²))`.
- erfc approximation: `erfc(x) ≈ 2 exp(-x²) / (2.319 x + sqrt(4 + 1.52 x²))`.
- σ_dir² is the slope variance projected on that direction's azimuth.
- The spec's "sun × D × Fresnel" omits `1/(4 n·v)` and Smith. Keep both, or the near-horizon sea is too bright.

**4.3 Hidden sun (important for sparkle).** In KS04 (beat 2, sun az 100 / el 6, yaw 80, pitch -8) and KS07 (s 492, sun az 101.6 / el 13.8, yaw 70) the specular path lies inside the frame.
- With the "hidden" key 0.35 the formula gives about 19 before the clamp (computed: F 0.58, D 39.8, n·v 0.105, Λ 0.012), so 8 after the clamp: a glittering white path.
- Gate the glint with a direct-sun visibility `T_sun`: 0 in beat 2 and in beat 3 before the break, then `u_sun(s)` times the H2 cloud-hole term.
- Let the storm sky model carry a broad bright forward lobe toward the sun. Its reflection (4.4) then gives a soft sheen instead of glints.
- After the break (s ≥ 520) the gaze looks west and the sun is behind. In beat 4 the specular point is 72–98° right of yaw 78 at 48–56° depression, out of frame. So no in-frame glint survives where the key is strong. Check this with the visibility audit.

**4.4 Sky reflection.**
- `R = reflect(-V, N)`, then `R.y = max(R.y, 0)`.
- Average 3 BwSkyColor taps in elevation at `e_R` and `e_R ± 2 sqrt(σ_v²)` (reflection spreads by twice the facet slope). This replaces Bruneton's filtered sky-map lookup with an analytic equivalent.
- Mean Fresnel (Bruneton fit): `F̄ = 0.02 + 0.98 pow(1 - n·v, 5 exp(-2.69 σ_v)) / (1 + 22.7 σ_v^1.5)`. Here `σ_v² = (vx² Σxx + 2 vx vz Σxz + vz² Σzz) / (1 - vy²)`. At grazing with a rough sea, F̄ settles around 0.6–0.7, not 1.

**4.5 Body colour.** Use `L_body = SeaToken × ambient(beat) × (1 - F̄)`: SeaStorm for beat 2, crossfading to SeaTeal. Treat the palette token as an appearance (Part 3.2 #13). For reference, Bruneton uses `seaColor · E_sky / π`, with seaColor = (0.0039, 0.046, 0.09).

Total: `L = L_body + F̄ L_skyrefl + L_sun`, then foam (4.6), then `BwFogApply` per pixel. Optional and alias-free: a crest-transmission glow ∝ `saturate(η/Hs) · pow(saturate(dot(V, -L)), 4)`, because η is low-frequency.

**4.6 Foam: the spec threshold is unreachable.** To first order `J ≈ 1 - λ Σ a k cos θ`, so `σ_J = λ sqrt(mss)` = 0.069 / 0.061 / 0.055.

Monte Carlo over the 32 components (400 k samples), computed:

| | Open-sea J min | Open-sea 1 % quantile | At the wall with image: J min | At the wall: 1 % quantile |
|---|---|---|---|---|
| B2 | 0.71 | 0.84 | 0.44 | 0.69 |
| B4 | 0.75 | 0.87 | 0.51 | 0.75 |

**"Foam from the Jacobian below 0.15" therefore never fires.** Replace it (14.10.5 arbitration) with a statistical threshold that is filtered like roughness:
- `foam = Φ((J_thr - J_r) / σ_u)`. J_r is the footprint-resolved Jacobian. `σ_u = λ sqrt(Σ(1 - w²)(ak)²/2)`, which is the same accumulator as the roughness.
- Φ by the logistic approximation `1/(1 + exp(-1.702 x))`, with σ_u ≥ 1e-3.
- Far away this converges to the mean coverage by construction: no foam sparkle, the same idea as LEAN applied to foam.
- Choose `J_thr = 1 + σ_J Φ⁻¹(W)` from the Monahan & O'Muircheartaigh whitecap fraction `W = 3.84e-6 U^3.41`. At the PM winds 12.3 / 11.0 / 15.1 m/s, W = 2.0 / 1.3 / 4.0 % and **J_thr = 0.858 / 0.864 / 0.904** (B2 / B3 / B4).
- The wall doubles σ_J automatically through the image term, so the wall gets more foam.
- Add the wall band `exp(-(x - 6)/4 m) (0.3 + 0.7 impact envelope)` and reef occupancy.
- Foam texture detail (cellular, advected) only where its cell is at least 4 px; otherwise use the mean.
- `L = lerp(L_sea, L_foam, coverage)`, linear, so pixel averages stay exact. `L_foam = FoamWhite (E_sun T_sun max(n·l, 0) + E_sky)/π`, clamped to 4.

**4.7 Horizon on the sphere.**
- Dip from the crest eye (9.7 m above the sea) is `sqrt(2h/R) = 0.100°`, and the horizon is 11.1 km away. That is 3 px below the eye line at 4K. Everything beyond 1 km occupies only about 13 rows at 4K.
- BwFarField: compute the sea/sky coverage per pixel analytically, `α = saturate(0.5 + (e_horizon - e_pixel)/pixel_angle)`. MSAA cannot anti-alias inside one dome triangle. Blend sea and sky, giving a 1–2 px edge.
- Fog transmittance at the horizon (14.2.2 coefficients, eye inside the storm slab): B2 `exp(-11.1 km × 7.8e-4) ≈ 2e-4` (the horizon dissolves, as intended); late B3 about 0.04; B4 `exp(-11.1 km × 1.6e-4) ≈ 0.17`, leaving a soft, natural horizon line.
- Put `BwSeaShade(P, V, N̄, Σ, foam)` in one include used by both BwSea and BwFarField. At 2048 m every component already has w = 0, so the two tiers are identical (T7 dE ≤ 2).
- The far sphere takes over outside the clipmap's square footprint. Use the same square test, with no overlap, to avoid z-fighting.

---

### 5. The wall: clapotis by an image wave

**5.1 Mirror.** `η_total = η(x, z, t) + Kr(z) b(x) η(12 - x, z, t)`, with `b = 1 - smoothstep(150, 400, x - 6)`. Per component the image has direction `(-Dx, Dz)` and phase `phase_i + 12 k_i Dx` (precomputed on the CPU). Its horizontal displacement uses the mirrored direction, so Dx cancels at the wall. Add its unresolved variance with weight `(Kr b)²`.

**5.2 Physics.**
- Full reflection from a vertical wall gives a standing wave: antinode at the wall, nodes λ/4 out, height almost doubled.
- For oblique incidence (±35°) the crossed pattern is the *clapotis gaufré*, whose intersection crests travel **parallel to the wall**. That is a strong near-field cue.
- Linear crest at the wall, computed: up to 5.7 m above SWL in B2 and 6.5 m in B4, so y ≈ -2.3 / -1.5. That stays below the road (0) and the parapet top (1.1).

**5.3 Kr(z).** Smooth over about 30 m in z.
- About 0.95 on the plain vertical wall (beat 2, z 120–416; cove 1040–1560).
- About 0.2 along the spillway (z 420–704): open slots transmit.
- **About 0.4 along the armour belt** (z 704–1040). From Zanuttigh & van der Meer, `Kr = tanh(a ξ0^b)` with `a = 0.167 (1 - exp(-3.2 γf))` and `b = 1.49 (γf - 0.38)² + 0.86`. For γf 0.38, slope 1 : 1.5, Hm0 3.2, Tm-1,0 ≈ 10 s: ξ0 ≈ 4.7, giving Kr ≈ 0.42.
- Over the belt, also attenuate the short-wave amplitudes by occupancy: waves break over the units, and foam covers the band.

---

### 6. Dams and breakwaters (14.9 item 1)

**6.1 Radial (Tainter) gates.**
- The curved skin plate is an arc centred on the trunnion, so the water-pressure resultant passes through the pivot. Lifting only has to overcome weight and friction.
- History: patent by Thomas Parker (US 226,455, 1880), with the Tainters as assignees. Folsom Dam gate failure in 1995, caused by trunnion friction stress.
- Suggested layout for the spec's gates (radius 14, width 16, raised 5): sill -14; skin-plate top about -7 (1 m above the sea). Put the trunnion about 12.9 m downstream (west) at y ≈ -8.5, under the road deck and above the jet.
- Raising the gate rotates it about 21°: the lip moves from -14 to -9, still 1 m under the surface, so the gates throttle slightly.
- Closed hydrostatic load: `½ ρ g h² b = 0.5 × 1025 × 9.81 × 6² × 16 ≈ 2.9 MN` per gate, through the trunnion.

**6.2 Spillway discharge.** The spec's lip numbers are self-consistent as a free overfall.
- Rouse's end-depth ratio is `y_brink = 0.715 y_c`.
- Brink depth 2.5 m gives y_c = 3.5 m and `q = sqrt(g y_c³) = 20.5 m²/s`.
- Then `v_lip = q / 2.5 = 8.2–8.3 m/s` (Bernoulli for the 3.5 m drop gives 8.29). The critical section sits about 3–4 y_c (10–14 m) upstream of the lip, with specific energy 5.25 m (0.75 m lost at the gates).
- Discharge: 332 m³/s per bay, about 4,650 m³/s for all 14.
- **Inconsistency:** "0.8 m/s at x 46" needs an approach depth of `20.75/0.8 ≈ 26 m` (floor about y -34 at x 46, ramping up to the sill near the gates). With the sill at -14 all the way out, the approach velocity would be about 3.5 m/s. Either deepen the approach floor or re-state the speed; log it.
- For BwSheet (continuity, before breakup): sheet thickness `h(t) = q / sqrt(8.3² + (9.81 t)²)`, which is 2.5 m at the lip and 0.44 m at t = 4.7 s (v = 46.8 m/s).

**6.3 Delta Works context.**
- Built 1954–1997 after the 1953 North Sea flood: 14 works on Wikipedia's list. Safety standard for North and South Holland: complete failure of a dike ring at most once in 10,000 years.
- Oosterscheldekering: about 9 km long, 65 piers 35–38.75 m tall of about 18,000 t each, 62 steel gates each 42 m wide. Built 1976–1986, opened 4 October 1986. It closes when the water is at least 3 m above normal: 28 closures by January 2022.
- The lowest Dutch polders lie about 6.5–6.8 m below NAP. The game's sea, 6 km above the land, is the same fact at roughly 1000× scale.
- Not verified: Maeslantkering (1997) with two arms of about 210 m. Check before quoting.

**6.4 Armour units** (TU Delft breakwater catalogue; EurOtop Table 6.2):

| Unit (year, origin) | Layers, placement | K_D trunk / head | k_Δ | k_s = V/H³ | γ_f |
|---|---|---|---|---|---|
| Tetrapod (1950, Danel & Anglès d'Auriac, LDH Grenoble) | 2, random | 7.2 breaking / 8.3 non-breaking | 1.02 | 0.28 | 0.38 |
| Accropode (1981, Sogreah) | 1, prescribed grid, random attitude, no parallel neighbouring faces | 15 / 11.5 | 1.29 | 0.341 | 0.46 |
| Accropode II (1999) | 1 | 15 / 11.5 | 1.29 | 0.341 | 0.44 |
| Xbloc (DMC, 2001–2005) | 1, staggered "brick" grid, random orientation | 16 / 13 | 1.4 | 0.333 | 0.44 |

Hudson's formula: `M = ρ_s Hs³ / (K_D Δ³ cot α)`, with Δ = ρ_s/ρ_w - 1 ≈ 1.34. Tetrapods do not work in a single layer.

**Colossopod geometry.**
- Legs point along the tetrahedral directions: one at (0, 1, 0), three at `(sqrt(8/9) cos φ, -1/3, sqrt(8/9) sin φ)` for φ = 0, 120, 240°.
- 40 m tip to tip gives a centre-to-tip length L = 40 / sqrt(8/3) = 24.49 m. Standing on three legs it is 4L/3 = 32.7 m tall, and the three foot tips form a 40 m triangle.
- A real Tetrapod (V = 0.28 H³) of that height has base radius about 0.22 H ≈ 7.3 m and tip radius about 0.13–0.15 H ≈ 4.4–4.9 m (derived from k_s). The spec's 6 → 3.6 m is about 18 % slimmer: fine, but say so.
- Scale: V ≈ 6,500–9,800 m³, about 16–24 kt each. A Hudson design wave of about 60 m. Roughly 6.5× the linear size of the 80 t Tetrapod monument at Hakusan, the largest one documented.

**Packing check.** Two-layer Tetrapod density is `N/A = 1.02 / V^(2/3)`, which gives about 30 m spacing per layer for this unit. The spec's 28 m jittered grid per layer is realistic.

**Placement and interpenetration (Part 10 #28).** Jitter of ±8 m with 24.5 m arms will make neighbouring arms intersect.
- Resolve it deterministically in C#. Per cell, try 16 hash-seeded orientations: bottom layer resting tripod-down with random yaw; upper layers tilted 15–35°. Choose the one with the least capsule–capsule penetration (arm capsules of radius about 5 m) against units already placed, then lower it until contact.
- Bake the result into a StructuredBuffer that `ReefUnitAt` reads for the belt and islets. Pure-hash cells remain only for the horns (pyramids, where contacts are invisible).

**6.5 EurOtop overtopping and spray.**
- Vertical wall, non-impulsive (eq. 7.1): `q / sqrt(g Hm0³) = 0.047 exp(-(2.35 Rc/Hm0)^1.3)`.
- With Rc = 9.1 m (parapet top above SWL): B2 q ≈ 5e-7 m³/s per m (0.0005 l/s/m); B4 q ≈ 6e-6 (0.006 l/s/m).
- Rubble-mound maximum with Tetrapod γ_f 0.38: `0.09 exp(-(1.5 Rc/(Hm0 γf))^1.3)`, about 1e-10. Negligible.
- **So no green water reaches the road; everything over the parapet is spray or mist,** as the spec intends.
- Impulsive (flip-through) impacts throw vertical jets (Peregrine 2003). A plume top of 6 / 14 / 22 m needs a jet of 10.8 / 16.6 / 20.8 m/s (`sqrt(2 g h)`), about 0.8–1.5 × the deep-water celerity (14.0 m/s at Tp 9, 17.2 at Tp 11). Rise time is 1.1–2.1 s, so the 3 s plume life is consistent. A 9 m/s wind carries mist across the 12 m road in about 1.3 s.
- Wave transmission over the belt: waves break over the units, and roughly 30–60 % of the height is transmitted (d'Angremond 1996; from memory, verify).

---

### 7. HLSL skeleton (load-bearing parts)

```hlsl
// Globals set once per frame by SeaState (arrays fixed at 32 on first set; not in UnityPerMaterial)
float4 _BwWaveA[32]; // k, a, Dx, Dz
float4 _BwWaveB[32]; // phase(t) [double mod 2pi on CPU], mirrorPhase = phase + 12 k Dx, (a k)^2/2, lambda
float4 _BwSea0;      // chop 0.6, s2Floor 0.008, glintClamp 8, J_thr
float4 _BwSea1;      // R_sea, sunVisibility T_sun, foamClamp 4, unused

void BwSeaAccumulate(float2 x0, float2 dPdx, float2 dPdy, float amp, bool mirror,
    inout float2 slope, inout float3 J /*xx,zz,xz*/, inout float3 cov)
{
    [loop] for (int i = 0; i < 32; i++) {
        float4 A = _BwWaveA[i], B = _BwWaveB[i];
        float2 D = mirror ? float2(-A.z, A.w) : A.zw;
        float  f = length(float2(dot(dPdx, D), dot(dPdy, D)));
        float  w = 1.0 - smoothstep(0.5, 1.0, 4.0 * f / B.w);
        cov += amp * amp * (1.0 - w * w) * B.z * float3(D.x * D.x, D.y * D.y, D.x * D.y);
        [branch] if (w > 0.0) {
            float s, c; sincos(A.x * dot(D, x0) + (mirror ? B.y : B.x), s, c);
            float ak = amp * A.x * A.y * w;
            slope -= ak * s * D;
            J -= _BwSea0.x * ak * c * float3(D.x * D.x, D.y * D.y, D.x * D.y);
        }
    }
}
// Then: J = float3(1,1,0) - ...; Tx = (J.x, slope.x, J.z); Tz = (J.z, slope.y, J.y); N = normalize(cross(Tz, Tx));
// Sigma = 0.004*I + cov (+ geometric-SAA var); foam = logistic(1.702 * (J_thr - det2x2(J)) / max(chop*sqrt(trace(cov)), 1e-3));
// L = body*(1-Fbar) + Fbar*sky3tap + min(Esun*T_sun*F*p/(4*max(nv,0.01)*nh^4*(1+Lam_l+Lam_v)), 8); lerp foam; clip(x-6); BwFogApply.
```

---

### 8. Tests this research feeds (14.10)

- **T-NUM:** Hs 2.80 / 2.00 / 3.20 exact by scaling. The 32-component mss is within about 1 % of a fine reference (0.01335 vs 0.0135 continuous for B2), so the 3 % check passes.
- **Flicker and pan:** run at KS04 framing with `T_sun = 0` (beat 2) and with `T_sun = 1` (a forced stress test, to prove the clamp and filter).
- **Wall foot (T5):** surface clamped at x 6; waterline swing ±2–5 m on the wall; no gap.
- **T7 crossover at 2048 m:** shared `BwSeaShade`, all w = 0, dE ≤ 2.
- **Horizon:** analytic coverage, 1–2 px.
- **Foam:** coverage 1–4 % on the open sea at J_thr from section 4.6.

### 9. Sources (all accessed 2026-11-13)

| Source | URL | Used for |
|---|---|---|
| Tessendorf, *Simulating Ocean Water* (course notes 1999–2004) | https://jtessen.people.clemson.edu/reports/papers_files/coursenotes2004.pdf | Phillips spectrum and suppression (eq. 40–41), h̃0 (42), choppy D (44), Jacobian and eigenvalues (45–49), grid advice |
| Bruneton, Neyret, Holzschuch 2010, CGF 29(2), DOI 10.1111/j.1467-8659.2009.01618.x | https://hal.archives-ouvertes.fr/inria-00443630v3 (the HAL PDF was blocked by bot protection) | Geometry → normal → BRDF transition, variance from footprint |
| Bruneton's ocean shader (INRIA BSD-3), Unity port | https://github.com/unitycoder/Brunetons-Ocean (`Assets/BrunetonsOcean/Shaders/Ocean.shader`) | Exact meanFresnel, Smith Λ, erfc approximation, reflected sun radiance, back-face normal fix, sky filtering |
| Horvath 2015, *Empirical Directional Wave Spectra for Computer Graphics* | https://cdn.shopify.com/s/files/1/0134/1131/9867/files/EmpiricalDirectionalWaveSpectra_DigiPro2015_Jun20_2015_Optimized.pdf | JONSWAP (eq. 28), Mitsuyasu, Hasselmann, Donelan–Banner spreading (eq. 33–38) |
| Finch, GPU Gems 1 ch. 1 | https://developer.nvidia.com/gpugems/gpugems/part-i-natural-effects/chapter-1-effective-water-simulation-physical-models | Gerstner Q normalisation, Σ Q ω A ≤ 1, direction choice |
| Asirvatham & Hoppe, GPU Gems 2 ch. 2 (geometry clipmaps) | https://developer.nvidia.com/gpugems/gpugems2/part-i-geometric-complexity/chapter-2-terrain-rendering-using-gpu-based-geometry | Transition region and geomorph from the viewer position (not re-fetched) |
| Crest ocean system notes (SIGGRAPH 2017 Advances) | https://crest.readthedocs.io/en/4.17/user/system-notes.html ; http://advances.realtimerendering.com/s2017/ | Waves rendered into the LOD matching their wavelength; LOD blending |
| Olano & Baker 2010, LEAN mapping | https://userpages.cs.umbc.edu/olano/papers/lean/ | First and second slope moments → anisotropic Beckmann covariance |
| Dupuy et al. 2013, LEADR | https://perso.liris.cnrs.fr/victor.ostromoukhov/publications/pdf/SAsia2013-LEADR.pdf | Displacement-aware moments with masking |
| Toksvig, *Mipmapping Normal Maps* | https://developer.download.nvidia.com/whitepapers/2006/Mipmapping_Normal_Maps.pdf | Variance from normal shortening, for other materials (not re-fetched) |
| Tokuyoshi & Kaplanyan 2021, JCGT 10(2) | https://www.jcgt.org/published/0010/02/02/ | Projected-space geometric specular AA |
| Kaplanyan, GDC 2016 specular AA | https://developer.download.nvidia.com/gameworks/events/GDC2016/akaplanyan_specular_aa.pdf | Clamp the filter width from above |
| URP core `CommonMaterial.hlsl` (local, this project) | `Library\PackageCache\com.unity.render-pipelines.core@2d66c71e606e\ShaderLibrary\CommonMaterial.hlsl` | `GeometricNormalVariance` and `NormalFiltering` forms in this exact version |
| Cox & Munk via the Ocean Optics Web Book | https://oceanopticsbook.info/view/surfaces/cox-munk-sea-surface-slope-statistics | mss = 0.003 + 5.12e-3 U; up-wind/cross-wind split |
| Monahan whitecap fit (cited in PMC6209970) | https://pmc.ncbi.nlm.nih.gov/articles/PMC6209970 | W = 3.84e-6 U^3.41 (fraction, U at 10 m) |
| EurOtop 2018 formulas (via the breakwater package docs) | https://breakwater.readthedocs.io/en/latest/overtopping.html ; manual: http://www.overtopping-manual.com/ | Eq. 7.1/7.5/7.7/7.8, rubble-mound maximum, γ_f table |
| Zanuttigh & Andersen 2010 (cites Zanuttigh & van der Meer 2008) | https://data-ww3.ifremer.fr/BIB/Zanuttigh_Andersen_CE2010.pdf | Kr = tanh(a ξ0^b) |
| Wikipedia: Clapotis | https://en.wikipedia.org/wiki/Clapotis | Standing wave, doubling, clapotis gaufré |
| Wikipedia: Tainter gate | https://en.wikipedia.org/wiki/Tainter_gate | Principle, patent, Folsom 1995 |
| Rouse 1936 end-depth ratio (secondary sources) | https://www.iahrmedialibrary.net/the-library/geophysical-hydraulics/fluvial-hydraulics/the-free-overfall/1156 | y_e = 0.715 y_c, control 3–4 y_c upstream |
| Clemmens et al. 2003, radial gate calibration (USBR) | https://www.usbr.gov/tsc/techreferences/computer%20software/software/wingate/Clemmens%20et%20al%202003.pdf | Q = Cd b w sqrt(2 g y1), Cd depends on the opening |
| Wikipedia: Oosterscheldekering ; Delta Works | https://en.wikipedia.org/wiki/Oosterscheldekering ; https://en.wikipedia.org/wiki/Delta_Works | Context numbers |
| Wikipedia: Tetrapod (structure) | https://en.wikipedia.org/wiki/Tetrapod_(structure) | Origin 1950, Accropode 1981, Xbloc 2001, 80 t monument |
| TU Delft breakwater unit catalogue | https://kennisbank-waterbouw.nl/breakwaters/units/Tetrapod.html ; .../Accropode.html ; https://kennisbank-waterbouw.tudelft.nl/breakwaters/units/Accropode%20II.html ; https://kennisbank-waterbouw.nl/breakwaters/units/Xbloc.html | K_D, k_Δ, k_s, placement |
| Xbloc design manual (DMC) | https://www.conteches.com/media/b2cj2q4w/xbloc-design-manual.pdf | Staggered grid, random placement (search snippet only) |
| Peregrine 2003, *Water-wave impact on walls*, Annu. Rev. Fluid Mech. 35:23–43 | https://data-ww3.ifremer.fr/BIB/Peregrine_ARFM2003.pdf (HTTP 403 when fetched) | Flip-through vertical jets (from abstracts) |

---

<a id="report-b"></a>

## B. Research log: Bulwark 14.9 items 3-7 (MASTER_PROMPT 14.7 H2, H3, H4, H5, H7; URP 17.6 specifics)

Access date for every URL: **2026-11-13**. Source labels: [F] means fetched and checked in this session. [S] means a standard literature reference that was not re-fetched; its numbers are approximate and should be checked before any test threshold relies on them. Package source paths are relative to `<WORKSPACE>\4 Megastructures\4 Megastructures\Library\PackageCache\`: `URP\` = `com.unity.render-pipelines.universal@8457e85b8184\` (package.json version 17.6.0) and `CORE\` = `com.unity.render-pipelines.core@2d66c71e606e\` (17.6.0).

---

### 3. Waterfall physics and rainbow optics (H2)

#### 3.1 Sheet breakup
- **Breakup length of a rectangular falling jet (Horeni 1956):** `L_b = 6 q^0.32` (L_b in m, q = unit discharge in m²/s). It is the only nappe/rectangular-jet formula, as cited by Carrillo, Ortega, Castillo & García 2021, *Water* 13(11):1593 [F, via search]. For the Bulwark lip (8.3 m/s, about 2.5 m thick), q ≈ 21 m²/s, which gives **L_b ≈ 16-20 m**. Beyond that the jet has no continuous core: it falls on as aerated clusters and streaks. This is lab-scale extrapolation, so use it for look development only.
- **Look-development consequence for `SheetBuilder` and `BwSheet`.** The sheet is glassy green only in the first ~5-10 m below the lip, turns white and aerated with streaks by ~20 m, and is ligaments and clusters by the end of the 120 m near sheets. Free-fall speed is `v = sqrt(v0² + 2 g Δy)`: 46.9 m/s at the 108.5 m drop to y -120.
- **Atomisation.** Clusters accelerate until drag balances weight. Stable raindrops are capped at about 2.5-3 mm radius, terminal speed about 9 m/s; larger masses are torn apart by Rayleigh-Taylor and Kelvin-Helmholtz instabilities. After hundreds of metres to about 1 km the water is rain plus spray. Angel Falls (979 m) and Staubbach (297 m; "Staub" means dust) are the real-world anchors for "the fall becomes mist" [S]. The spec's curtain is consistent with this: Gaussian radius 3 m at 90 m growing to 60 m at 1 km, and water fraction 0 at 1000 m.

#### 3.2 Mist plume
- **Mechanism.** The falling water drags air downward. At the foot this air spreads out, converges, and rises again, carrying fine spray. Evaporation and the extra liquid-water load make the plume air cooler and denser than its surroundings.
- **Scale anchor.** The Victoria Falls spray ("Mosi-oa-Tunya") typically rises 400+ m and is visible from about 50 km [S]. The spec's 260 m top and σ 150 m along z are therefore conservative.

#### 3.3 Gravity currents (the "river of cloud")
- **Front speed:** `U = Fr sqrt(g' h)` with `g' = g Δρ/ρ ≈ g ΔT/T_v`.
  - `Fr = sqrt(2)` in Benjamin's energy-conserving deep-ambient limit (Benjamin 1968, JFM 31:209 [S]).
  - Fr ≈ 0.7-1.2 is typical in the lab and atmosphere (Huppert & Simpson 1980, JFM 99:785; Simpson 1997, *Gravity Currents* [S]).
- **Density sources.**
  - Evaporative cooling toward the wet-bulb temperature gives up to about 3-5 K. Example: 15 °C at 60 % RH has a wet-bulb of about 10.8 °C.
  - Liquid water content of 1-5 g/m³ adds Δρ/ρ ≈ 1-4e-3, which is equivalent to about 0.3-1.2 K.
- **Worked numbers.**
  - ΔT 3 K, T 285 K: g' = 0.103 m/s².
  - h 100 m: U ≈ 2.2-3.9 m/s.
  - h 200 m: U ≈ 3.2-5.5 m/s.
  - The spec's mist rivers at 4 m/s are physical.
- **Visual traits.** The current flows down the valleys. Its head is about twice the tail depth, with Kelvin-Helmholtz billows behind the head. The fbm is advected along the flow.

#### 3.4 Rainbow optics

**Descartes (geometric) angles from the antisolar point.** Computed in this session for k = 1 and k = 2 internal reflections with water n(λ) at 20 °C:

| λ nm | n | primary ° | secondary ° |
|---|---|---|---|
| 404.7 | 1.3428 | 40.67 | 53.43 |
| 435.8 | 1.3404 | 41.01 | 52.81 |
| 486.1 | 1.3371 | 41.49 | 51.96 |
| 546.1 | 1.3345 | 41.86 | 51.28 |
| 589.3 | 1.3330 | 42.08 | 50.89 |
| 656.3 | 1.3311 | 42.36 | 50.39 |
| 706.5 | 1.3300 | 42.52 | 50.10 |

The dominant wavelengths of the sRGB primaries are about R 612, G 549, B 465 nm. The geometric channel centres are therefore primary ≈ (42.2, 41.85, 41.3) and secondary ≈ (50.6, 51.25, 52.3).

**Airy theory (derived and evaluated in this session).**
- Near the rainbow ray the exit wavefront is cubic, which gives `I(θ) ∝ Ai²(-u)`, where:
  - `u = ε (k a)^{2/3} (2 cos² i0 / D_ii)^{1/3}`
  - ε is the angle from the Descartes angle toward the bright side (inside the primary, outside the secondary);
  - `D_ii` is the second derivative of deviation with respect to the incidence angle: 2.529 (primary) and 5.405 (secondary) at n = 1.3345;
  - i0 is 59.32° (primary) and 71.79° (secondary).
- **First (main) maximum:** u = 1.0188, so `ε1 = 1.0188 (D_ii/(2cos² i0))^{1/3} (ka)^{-2/3}`.
- **Peak positions for 546 nm:**

| drop radius a | primary peak ° | secondary peak ° |
|---|---|---|
| 1 mm | 41.67 | 51.63 |
| 0.5 mm | 41.55 | 51.83 |
| 0.25 mm | 41.37 | 52.16 |
| 0.1 mm | 40.96 | 52.89 |
| 50 µm | 40.43 | 53.84 |
| 10 µm | 37.7 (fogbow regime; Airy is unreliable below about 50 µm, use Lorenz-Mie) | — |

- **Broadening.** Each colour lobe is broadened by the sun disc (0.53° diameter) and by the drop-size distribution. A full width at half maximum of about 1.5° per channel is realistic.
- **Optional upgrade (no external assets).** A C#-generated 1D float LUT (θ 35-60°, 512 texels) holding the Airy intensity, integrated over λ with CIE 1931 colour-matching functions and the solar spectrum, convolved with the sun disc and a log-normal drop distribution around 0.3 mm, then converted XYZ → linear sRGB and clamped to gamut.

**Sadeghi et al. 2012** ("Physically-based simulation of rainbows", ACM TOG 31(1), art. 3, doi 10.1145/2077341.2077344) [F]. Their ray-tracing method handles dispersion, polarisation, interference and diffraction. It matches Lorenz-Mie for spherical drops, and non-spherical (oblate) large drops explain twinned bows. For waterfall spray (small, nearly spherical drops) this means Mie or Airy-quality lobes are right, while geometric optics alone misses the width and the supernumeraries.

**Alexander's dark band and relative brightness.**
- Between about 42° and 51° no light leaves the drop after one or two internal reflections, so the band is darker.
- Inside the primary the sky is brighter (primary rays fill θ < 42°). Outside the secondary it is slightly brighter.
- The secondary peak is roughly 0.4 of the primary in geometric optics and visually about 0.25-0.45 once broadened, so the spec's weight 0.25 is fine [S].
- Make the band edges smooth to avoid a hard step: `band = 1 - 0.4 * smoothstep(42.0, 43.0, θ) * (1 - smoothstep(49.5, 50.5, θ))`.

**Spec check: KS08 bow-top test conflict (needs a decision).**
- The spec's primary θ_c = (42.0, 41.0, 40.3) for R, G, B.
- Luma is dominated by G, so the luma-weighted peak is about 0.2126·42.0 + 0.7152·41.0 + 0.0722·40.3 = **41.16°**. With sun elevation 16.6° that puts the bow top at **24.56°**, which fails "25.4 ±0.5".
- The spec's R-B spread (1.7°) is also about 1.6 times the physical spread.
- **Option A (recommended).** Use primary (42.25, 41.85, 41.30) and secondary (50.55, 51.30, 52.30) for drops of about 1 mm or larger. This gives a luma peak of about 41.8° and a top of 25.2°, which passes.
- **Option B (a = 0.3 mm Airy).** Use primary (41.95, 41.50, 40.95) and secondary (51.0, 51.9, 53.0). This gives a top of about 24.85°, at the edge of the tolerance.
- **Option C.** Keep the spec's values and redefine the test as "R-channel peak of (bow on − bow off) on the vertical through the antisolar azimuth". That gives 42.0 − 16.6 = 25.4.
- Log whichever option is chosen as an arbitration.
- **Lobe shape.** The Airy shape is asymmetric: a sharp outer edge and a broad inner side. An asymmetric Gaussian (σ_out ≈ 0.35°, σ_in ≈ 0.8°) reads more saturated than the symmetric `exp(-(Δ/0.9)²)` (FWHM 1.50°) and keeps "red outermost".
- **High vantage point.** From 6 km up the bow is not cut by the horizon: its arms continue down into the lit mist river, and a full circle is possible.

#### 3.5 Single scattering in mist
- **Henyey-Greenstein:** `p(θ) = (1 - g²) / (4π (1 + g² - 2 g cos θ)^{3/2})`, where θ is the scattering angle (sun to view). For the KS08 geometry (sun behind) the bow sits at a scattering angle of 138°.
- **g = 0.55:** p(0°) = 0.609, p(138°) = 0.0180, p(180°) = 0.0149 per steradian.
- **g = 0.85** (typical of real cloud and fog droplets, closer to Mie): p(138°) = 0.0043. So g 0.55 makes back-lit-from-behind mist about 4.2 times brighter than real Mie and much weaker in the forward direction.
- That is acceptable as a calibrated artistic choice (K and exposure absorb it). For physical forward glow, use a two-lobe HG (for example 0.8·HG(0.8) + 0.2·HG(-0.3)) or the HG+Draine fit for fog and cloud droplets (Jendersie & d'Eon 2023, SIGGRAPH Talks, "An Approximate Mie Scattering Function for Fog and Cloud Rendering") [S].
- **In-scatter per step:** `dL = σ ρ dt T(t) [L_sun T_sun(P) (p_HG(cos θ) + K·bow(θ_anti)) + L_amb/(4π)]`, with `θ_anti = acos(dot(v, -ŝ))`.
- **Optional quality upgrade (analytic plume transmittance).** The base plume density is Gaussian in z times exponential in y. Along a ray both are linear in t, so the exponent is quadratic in t: `E(t) = -A t² - B t - C`, with:
  - `A = d_z²/(2σ²)`
  - `B = (z_e - z0) d_z/σ² + d_y/H`
  - `C = (z_e - z0)²/(2σ²) + y_e/H`
  - Then `∫ e^E dt = e^{B²/(4A) - C} (√π / (2√A)) [erf(√A (t1 + B/2A)) - erf(√A (t0 + B/2A))]`. When A → 0, fall back to the exponential integral; when both erf arguments are large, use erfc.
- With this, τ is exact (no slicing). The 24 fixed samples are then needed only for the fbm-modulated in-scatter, normalised to the analytic τ. With σ_z 150 m and a 2.5 km plume, 24 samples give 37-104 m steps, which is adequate.

---

### 4. Underwater optics (H3)

**Pure water and seawater [S].**
- Absorption a_w (Pope & Fry 1997, Appl. Opt. 36:8710), approximately: 450 nm 0.0092, 475 nm 0.0114, 500 nm 0.0204, 525 nm ≈ 0.05, 550 nm 0.0565, 600 nm 0.222, 650 nm 0.34, 700 nm 0.65 m⁻¹.
- Scattering of pure seawater: `b_sw(λ) ≈ 0.00288 (λ/500)^-4.32`, which is 0.0045 m⁻¹ at 450 nm and 0.0019 at 550 nm (Morel 1974).
- Clearest-water diffuse attenuation K_w (Smith & Baker 1981, Appl. Opt. 20:177): about 0.017 at 450 nm, 0.026 at 500, 0.064 at 550, 0.24 at 600, 0.35 at 650 m⁻¹.

**Jerlov types [F]** (definitions from the Ocean Optics Web Book, which also states the approximate chlorophyll mapping: I 0-0.01, IA 0.05, IB 0.1, II 0.5, III 1.5-2 mg/m³). There are five oceanic types (I, IA, IB, II, III) and nine coastal types (1-9), defined by K_d (Jerlov 1976, Tables XXVI and XXVII). A consistent IOP set is given by Solonenko & Mobley 2015, Appl. Opt. 54(17):5392. Approximate K_d values [S] (verify against the Web Book spreadsheet before quoting them in tests):

| K_d m⁻¹ | 450 | 475 | 500 | 525 | 550 | 600 | 650 |
|---|---|---|---|---|---|---|---|
| I | 0.019 | 0.018 | 0.027 | 0.043 | 0.063 | 0.235 | 0.35 |
| IB | 0.030 | 0.028 | 0.034 | 0.049 | 0.068 | 0.24 | 0.36 |
| II | 0.052 | 0.046 | 0.046 | 0.057 | 0.075 | 0.25 | 0.37 |
| III | 0.096 | 0.083 | 0.072 | 0.075 | 0.088 | 0.26 | 0.39 |

- **The spec's values match.** K_d = (0.28, 0.050, 0.034) corresponds to Jerlov IB at about (610, 520, 475) nm.
- **Beam attenuation.** c = (0.34, 0.085, 0.050) lies between pure water (c_w ≈ 0.016 at 470 nm, 0.05 at 525 nm, 0.22 at 600 nm) and Petzold's "clear ocean" at 530 nm (a 0.114, b 0.037, c 0.151 m⁻¹; Mobley 1994 Table 3.10) [S]. The ratio c/K_d ≈ 1.5-1.7 is typical of clear ocean water.
- **Sighting range.** Black-target horizontal visibility is `y ≈ 4.8/c` (Zaneveld & Pegau 2003, Opt. Express 11:2997) [S]: 56 m for G. The 2 % contrast range is `ln 50 / c` = 46 m. Both are consistent with the spec.
- **Contrast and veil.** Duntley: `L = L0 e^{-c r} + L_∞ (1 - e^{-c r})` for horizontal paths. For an inclined path at zenith angle θ, replace the veil growth rate by `c - K_L cos θ`, because the ambient light changes with depth along the path (Mobley 1994) [S]. This matters for the whale's top and bottom only to second order.
- **Radiance distribution with depth (Mobley, *Light and Water*, ch. 9-10; Tyler 1960) [S].**
  - Near the surface, the maximum radiance lies near the refracted sun direction.
  - With depth the distribution approaches an asymptotic shape that decays as `e^{-K∞ z}` and is peaked near the zenith.
  - Irradiance reflectance `R = E_u/E_d ≈ 0.33 b_b/a` (Morel & Prieur 1977): about 0.04-0.07 in blue and about 0.005 in green. The nadir is therefore very dark and blue.
  - The spec's zenith:horizontal:nadir ratio of 1 : e^-2.6 (0.074) : e^-5.2 (0.0055) is the right order for single-scattering albedo of about 0.3-0.5.
- **Snell's window.** The critical angle is `θc = asin(1/1.333) = 48.6°`, so the sky appears in a 97.2° cone, whose footprint on the surface has radius `1.134 z`. Looking up from 44 m (+40° pitch, slant path about 68 m, `c r ≈ 5.8`), the surface has 0.3 % contrast: only a diffuse brightening is visible. This is consistent with the β-veil model.
- **Flat-window refraction (portal shader).**
  - The water-side ray is `refract(d_air, n̂, 1/1.333)`. Every air angle up to 90° maps into a 48.6° cone in the water, so oblique views are compressed: 60° in air becomes 40.5° in water. Objects appear about 1.33 times larger and closer.
  - The 2.4 m acrylic slab (n 1.49) cancels out in direction (parallel faces) but shifts the ray sideways by `t sin(θa - θp)/cos θp`: 0.78 m at 45°. This is visible at the reveals; apply it as a ray-origin offset or accept it.
  - Fresnel at normal incidence: water-acrylic 0.3 % plus acrylic-air 3.9 % ≈ **4.2 %**, which matches the spec's 4 %.
  - Because the whale (Geometry+20) is drawn after the portal and overwrites it, draw the 4 % reflection as a separate additive pass at Transparent queue, ZTest against interior depth, so that it also lies over the whale.
- **Real thick acrylic windows [F].** Okinawa Churaumi Kuroshio Sea: 22.5 × 8.2 m, 60 cm thick, tank 10 m deep. Dubai Aquarium main panel: about 32.6 × 8.2 m (107 × 27 ft), reported as the world's largest tank viewing panel (about 0.75 m thick per Guinness 2008 [S]).
- **Caustics and shafts.**
  - A wave crest acts as a refracting lens with curvature `A k²`, so its focal depth is `f ≈ n / ((n - 1) A k²) ≈ 4.0/(A k²)`.
  - λ 14 m, A 0.5 m: f ≈ 40 m. λ 5 m, A 0.2 m: f ≈ 13 m.
  - The spec's 5-14 m caustic wavelengths therefore focus at about 10-40 m: physically right for windows at 44-80 m depth. At that depth, caustics on the whale should be low-contrast (the spec's footprint filter is right).
- **Marine snow [S].**
  - Definition: aggregates of 0.5 mm or more. Sinking speed 16-368 m/day, i.e. about 0.2-4 mm/s (Alldredge & Silver 1988, Prog. Oceanogr. 20:41).
  - The spec's 0.03 m/s downward drift is 10-100 times faster than real (note only). The 2.5 m/s walk dominates the apparent motion through parallax anyway.
  - If realism is wanted: 0.003-0.005 m/s drift and 0.01-0.02 m/s turbulence.

---

### 5. High-altitude atmospherics (H4)

**Horizon dip (geometric).**
- `cos δ = R/(R+h)`, i.e. `sin²δ = h(2R+h)/(R+h)²`, approximately `δ ≈ sqrt(2h/R)`.
- R 6371 km: δ = 2.486° at h ≈ 6000 m (crest) and 2.467° at h ≈ 5907 m (ledge). Both match the spec.
- Horizon distance: `sqrt(2Rh + h²) ≈ 276 km`.
- With standard refraction (k ≈ 0.13, R_eff = R/(1-k)) the apparent dip is about 1.76√h arcmin = 2.27°. Keep the geometric 2.486° (the tests use it), but note the difference.
- On the GPU, compute small dips with the well-conditioned form `δ = atan2(sqrt(h(2R+h)), R)`, not acos near 1. Values: 900 m → 0.963°, 2200 m → 1.505°, 6800 m (cloud deck) → 2.65°.
- From 6 km the geometric horizon is hidden by haze: a grazing path through the boundary layer has Rayleigh τ well above 3, so the sky meets pure in-scatter. This supports "horizon colour = fog colour".

**Hillaire 2020** ("A Scalable and Production Ready Sky and Atmosphere Rendering Technique", CGF 39(4), EGSR 2020; https://sebh.github.io/publications/egsr2020.pdf; code at github.com/sebh/UnrealEngineSkyAtmosphere [F], MIT licence).
- **LUTs:** transmittance 256×64, multiple scattering 32×32, sky-view 192×108 (hard-coded in `RenderSkyCommon.hlsl` [F]), aerial perspective 32³ froxels [S].
- **Multiple scattering:** isotropic second order times `1/(1 - f_ms)`, i.e. the geometric series of the transfer factor.
- **Sky-view LUT mapping for a viewer at altitude [F]:** `Vhorizon = sqrt(r² - R²); CosBeta = Vhorizon/r; Beta = acos(CosBeta); ZenithHorizonAngle = PI - Beta` (= 90° + dip). The non-linear latitude mapping is centred on the dipped horizon, which concentrates resolution there.
- **Earth atmosphere parameters [S]:**
  - Rayleigh scattering (5.802, 13.558, 33.1)e-6 /m, scale height 8 km.
  - Mie scattering 3.996e-6 /m, extinction 4.440e-6 /m (absorption 0.444e-6 /m), scale height 1.2 km, g 0.8.
  - Ozone absorption (0.650, 1.881, 0.085)e-6 /m, tent profile centred at 25 km, width 30 km.
  - Radii 6360 / 6460 km.
- **Use for Bulwark.** The far-field sky and haze can stay analytic (14.2.2). Take Hillaire's horizon-centred parameterisation and the planet-shadow visibility rule.

**Twilight from altitude and the terminator.**
- Do not use the eye's sun elevation for distant lowland points. The local elevation is `sin el_P = dot(ŝ, û_P)` with `û_P = normalize((0, R+h_eye, 0) + t d)` (camera-relative; float32 is fine).
- A point is lit iff `el_P > -dip(h_P)`. The ground is lit iff el_P > 0; mist at 900-2200 m iff el_P > -0.96..-1.51°.
- A point 100 km toward the sun has el_P = el_eye + 0.9°. This makes Earth's shadow sweep across the lowland at dusk, and the crest eye stays lit about 2.5° of solar descent longer (about 10-15 min of real time).
- Above the antisolar horizon the shadow band and the Belt of Venus appear.
- **Spec impact.** The spec's `sun_el + dip(h_mist) > 0` should use el_P, not el_eye.

**Float32 ray-sphere numerics.**
- Generic reference: Haines, Günther & Akenine-Möller, "Precision Improvements for Ray/Sphere Intersection", *Ray Tracing Gems* ch. 7, 2019 [S]. It computes the discriminant from the ray-to-centre distance and uses the stable quadratic `q = -b - sign(b) sqrt(Δ)`, `t0 = c/q`.
- **For Bulwark,** with `s = sin δ` and `s2 = s²` computed in double on the CPU, and `Rh = R + h`:
  - `disc' = (-dy - s)(-dy + s)`: no cancellation near the horizon, as in the spec.
  - **Near root: `t = Rh * s2 / (-dy + sqrt(disc'))`.** It is algebraically equal to the spec's `Rh(-dy - sqrt(disc'))`, but the spec's form cancels for steep downward views. At the nadir, `t/Rh = 1 - cos δ = 9.4e-4`, so float32 error (about 1.2e-7 relative) times Rh gives about **0.76 m** error in 6 km (about 0.5 px of the 1.6 m nadir footprint at 4K). The citardauq form above gives about 1 mm. Product check: `t_near · t_far = Rh² s2 = Rh² - R²`.
- **Pattern coordinates.** Never use `P = o + t d` in world space (|P| ≈ 6.4e6 m has a float ulp of 0.5 m). Instead:
  - horizontal distance `d_h = t sqrt(1 - dy²)`;
  - arc angle `φ = atan2(d_h, Rh + t dy)`;
  - arc length `s_arc = R φ` along the heading (note that `d_h/R` is off by 173 m at 276 km);
  - add the camera's world offset, computed in double on the CPU and wrapped to each pattern period (cell index plus fraction).
- **Sea sphere (beat 2):** eye 9.7 m above it gives a dip of 0.0999°, about 3.4 px at 4K. Same formulas.

---

### 6. Filtering and dither (H5, H7)

**Analytic filtering (Inigo Quilez) [F, via search].**
- **Band-limited cosine:** `fcos(x) = cos(x) * sin(0.5w)/(0.5w)` (exact box filter), or the practical `cos(x) * smoothstep(2π, 0, w)`, with `w = fwidth(x)`. Weight each octave the same way (the spec's `w = 1 - smoothstep(0.5, 1.0, 4f/λ)` is equivalent).
- **Box-filtered checker:** `w = max(abs(ddx), abs(ddy)) + 0.01; i = 2*(abs(frac((uv-0.5w)/2)-0.5) - abs(frac((uv+0.5w)/2)-0.5))/w; checker = 0.5 - 0.5 i.x i.y`.
- **Filtered grid:** `a = p + 0.5w; b = p - 0.5w; i = (floor(a) + min(frac(a)N, 1) - floor(b) - min(frac(b)N, 1))/(N w); grid = (1 - i.x)(1 - i.y)`, with N = cell/line ratio.
- Quilez's triangle-kernel variant ("more checker filtering") is temporally steadier under motion: use it for gratings.
- **Phone-wire AA** (E. Persson, GPU Pro 4, 2013) [S] for `BwLine`: clamp the width to at least 1.5 px and multiply alpha by `true/clamped` (energy preservation; the spec's floor is 0.35), with alpha-to-coverage under MSAA.

**TPDF dither theory** (Lipshitz, Wannamaker & Vanderkooy 1992, JAES 40:355 [S]).
- A triangular distribution on [-A, A] (A = peak, in 8-bit codes) has variance A²/6 (RMS A/√6). The spec's definition is correct.
- A = 1 LSB is the minimum that makes the error's first and second moments independent of the signal (no noise modulation, no contouring). A = 1.5-2.0 is margin for the encoder.
- **Inverse CDF:** `v = 2u - 1; n = sign(v)(1 - sqrt(1 - |v|))`, which is triangular on [-1, 1]; multiply by A. Alternatively use the sum of two decorrelated blue-noise channels minus 1.
- Blue-noise source: a void-and-cluster 64×64 tile (Ulichney 1993, SPIE 1913 [S]; Peters, "Free blue noise textures", http://momentsingraphics.de/BlueNoise.html [S]).
- Gjøl and Svendsen recommend TPDF blue noise in output (gamma) space after tonemapping ("Banding in games", http://loopit.dk/banding_in_games.pdf [F, PDF exists]; GDC 2016, "Low Complexity, High Fidelity: The Rendering of INSIDE").
- Add the dither in sRGB code space right before quantisation. If the target is an `_SRGB` format, decode back to linear after adding it (Thread's `ThreadGrade` already does this; keep it).
- sRGB codes ≤ 10 are on the linear segment, so floor 6 = linear 0.00182, which matches the spec.

**Encoder behaviour (H.264/x264, VP9) [S].**
- **Quantiser step:** `Qstep ≈ 0.625 · 2^(QP/6)`: QP 23 → 8.9, QP 30 → 20.
- **Why ±1-2 code dither disappears.** Per-pixel dither with σ ≈ 0.7 Y' codes gives AC coefficients far below about 2/3 Qstep, so trellis or deadzone quantisation and x264's DCT decimation (in P/B frames) zero them. The deblocking filter then smooths the result.
- **What remains.** The block mean (DC) is quantised coarsely too, so dark flat areas become block plateaus.
- **Mitigations that matter in the game:** a static pattern, sufficient gradient slope, a floor, and low-frequency cell noise.
  - A static dither is predicted exactly by zero-motion P-frames, so the noise coded in the (finer-QP) I-frame persists. A temporal dither must be re-coded every frame and is erased. This matches the first set's finding (Part 3.2 #6).
  - AQ modes: x264 `--aq-mode 3` biases bits toward dark flat blocks. YouTube's encoder settings are unknown, so test with defaults (11.12).
- **Limited range:** BT.709 8-bit `Y' = 16 + 219 E'`, so 1 RGB code ≈ 0.86 Y' code and floor sRGB 6 → Y' ≈ 21. Chroma 4:2:0 halves the chroma dither.
- Upload at 1440p or higher (VP9/AV1 at a higher bitrate). References: https://trac.ffmpeg.org/wiki/Encode/H.264 and https://support.google.com/youtube/answer/1722171 [S].

---

### 7. URP 17.6 specifics (read from the package source)

#### 7.1 Can a raster pass bind the multisampled camera depth as Texture2DMS? Yes, under two conditions

**Proof by example: URP's own CopyDepth pass does exactly this.**
- `URP\Runtime\Passes\CopyDepthPass.cs:182` creates `AddRasterRenderPass`.
- `:279` calls `builder.UseTexture(source, AccessFlags.Read)` on the MSAA depth attachment.
- `:133` calls `copyDepthMaterial.SetTexture(_CameraDepthAttachment, source)` inside the render function.
- `:88-124` selects the `_DEPTH_MSAA_2/4/8` keyword from `source.rt.antiAliasing`.
- The shader side, `URP\Shaders\Utils\CopyDepthPass.hlsl:23,32`, declares `Texture2DMS<float, MSAA_SAMPLES> _CameraDepthAttachment` and loads it with `LOAD_TEXTURE2D_MSAA` (`CORE\ShaderLibrary\API\D3D11.hlsl:149` maps that to `tex.Load(coord, sampleIndex)`).
- `CORE\Runtime\RenderGraph\RenderGraphUtilsBlit.cs:917` documents: "special handling of MSAA buffers … using the bindMS flag on the source texture and per-sample pixel shader invocation … (e.g. using SV_SampleIndex)".

**Condition 1: the depth attachment must be created with `bindTextureMS`.**
- `URP\Runtime\UniversalRendererRenderGraph.cs:1770-1781`: `desc.bindTextureMS = hasMSAA && RenderingUtils.ShouldDepthAttachmentBindMS()`.
- `URP\Runtime\RenderingUtils.cs:333-358`:
  - `MultisampleDepthResolveSupported() = SystemInfo.supportsMultisampleResolveDepth && supportsMultisampleResolveStencil` (always false on macOS);
  - `bindMS = !canResolveDepth && SystemInfo.supportsMultisampledTextures != 0` (false on GLES3).
- On **Direct3D 11** depth formats cannot be resolved with `ResolveSubresource` (Microsoft documents only colour/typeless resolves [F]). So `supportsMultisampleResolveDepth` is expected to be false, which makes `bindMS` true, which makes the camera depth `Texture2DMS`-bindable. This is the same path URP's CopyDepth takes on D3D11.
- **Verify at runtime (mandatory).** Log once `SystemInfo.supportsMultisampleResolveDepth/Stencil/MultisampledTextures/usesReversedZBuffer` and `renderGraph.GetTextureDesc(resourceData.activeDepthTexture).bindTextureMS` and `.msaaSamples` (`CORE\Runtime\RenderGraph\RenderGraph.cs:747`). If bindTextureMS is false or msaaSamples ≠ 4, take the fallback (7.5).

**Condition 2: the pass must not render to the backbuffer.**
- `resourceData.activeDepthTexture` is `cameraDepth` only when the intermediate target is active (`URP\Runtime\FrameData\UniversalResourceData.cs:61-77`). The imported backbuffer depth is not bind-MS.
- Set `requiresIntermediateTexture = true` on the pass (`URP\Runtime\Passes\ScriptableRenderPass.cs:223`, honoured at `URP\Runtime\UniversalRenderer.cs:597-603`). Check `!resourceData.isActiveTargetBackBuffer`.

**Resolve behaviour in Render Graph.** For a bind-MS resource read later through `UseTexture`, the native pass compiler stores the MSAA samples (`Store`). It requests a resolve only for non-bind-MS readers (`CORE\Runtime\RenderGraph\Compiler\NativePassCompiler.cs:1538-1577`). Creating a 1-sample texture with `bindTextureMS` throws (`CORE\Runtime\RenderGraph\RenderGraphResourceRegistry.cs:1167-1169`).

#### 7.2 `_CameraDepthTexture` is a resolved copy that keeps the farthest sample
- `URP\Shaders\Utils\CopyDepthPass.hlsl:35-55`: with `UNITY_REVERSED_Z` it uses `DEPTH_OP min` over the samples. Under reversed Z (D3D11: near = 1, far = 0; `CORE\ShaderLibrary\API\D3D11.hlsl:4-7`) the minimum raw value is the **farthest** sample.
- The copy target is single-sample: `UniversalRendererRenderGraph.cs:1817-1822` (`depthDescriptor.msaaSamples = MSAASamples.None`).
- **Consequence.** Rail edges carry the background depth, which causes the mist halo. A rail that never covers all 4 samples of any pixel (BwLine 1.5 px straddling a pixel boundary) vanishes from the copy entirely.
- The stock `FullScreenPassRendererFeature` reads only `cameraDepthTexture` (`URP\Runtime\RendererFeatures\FullScreenPassRendererFeature.cs:228-229`) and can bind the depth only as an attachment (`:254`). A custom `ScriptableRendererFeature` is therefore required.

#### 7.3 The stencil cannot come from the camera depth while you sample that depth
- Render Graph forbids `UseTexture` and `SetRenderAttachment`/`SetRenderAttachmentDepth` on the same texture in one pass, in either order:
  - `CORE\Runtime\RenderGraph\RenderGraphBuilders.cs:343-370` (`CheckNotUseFragment`, message `k_TextureAlreadyBeingUsedThroughSetAttachment`);
  - `:443-477` (`CheckUseFragment`);
  - messages in `RenderGraph.ExceptionMessages.cs:94-98`.
- These checks run only with `UNITY_ENABLE_CHECKS`, i.e. in the Editor and development players. A release build would silently be undefined, so test in a development player.
- **Therefore give the mist its own transient MSAA depth-stencil:** a copy of the camera depth desc with `format = GraphicsFormat.D24_UNorm_S8_UInt`, `bindTextureMS = false`, `clearBuffer = true`, and the same `msaaSamples` (attachments must match: `MismatchInMSAASamlpes`, `RenderGraph.ExceptionMessages.cs:78-82`).
- **Memory at 4K with MSAA 4×:** own stencil about 133 MB. For comparison, camera colour RGBA16F 265 MB and camera depth D32S8 (64-bit per sample on D3D11) 265 MB.
- **Alternative without the extra 133 MB:** write a 1× R8 edge mask, then run the per-sample pass over the whole screen and `discard` non-edge pixels. That costs about 33 M cheap invocations at 4K, roughly 0.2-0.4 ms.

#### 7.4 Recommended pass structure: two raster passes after the opaques and the sky
- **Event:** `RenderPassEvent.AfterRenderingSkybox`, so it runs after the opaques and the far-field dome and before the transparents (`UniversalRendererRenderGraph.cs:1278-1284`).
- **Pass 0, per pixel.**
  - Stencil `{Ref 1 Comp Always Pass Replace}`, `ZTest Always`, `ZWrite Off`, `Cull Off`, `Blend One SrcAlpha` (premultiplied: rgb = in-scatter, a = transmittance).
  - Load the 4 depths and convert with `LinearEyeDepth(raw, _ZBufferParams)` (`CORE\ShaderLibrary\Common.hlsl:1206`).
  - If `zmax > 1.02 zmin`, `discard` (no colour, no stencil). Otherwise integrate once and write; this also marks the pixel in the stencil.
- **Pass 1, per sample.**
  - Stencil `{Ref 1 Comp NotEqual}`. The fragment takes `uint s : SV_SampleIndex`, reads its own depth with `_BwDepthMS.Load(px, s)`, and integrates up to it.
  - Build the ray from `floor(positionCS.xy) + 0.5 + _BwDepthMS.GetSamplePosition(s)`; do not assume SV_Position semantics or the D3D sample pattern.
  - Convert eye depth to ray distance by dividing by `dot(rayDir, camForward)`.
- **Keep the sample grid independent of depth** (24 fixed samples between the analytic entry and exit of the bounding volume) so that the per-pixel and per-sample paths are identical where depths agree. Integrating to depth d = the cumulative table interpolated at d.
- Expected edge fraction about 3 % gives about 0.2-0.3 ms extra at 4K, which matches the spec.
- **Shader target:** `#pragma target 4.5` (ps_5_0 on D3D11) covers `Texture2DMS<float,4>` and `SV_SampleIndex`. MSAA is fixed at 4×; assert it.
- **C# sketch:**

```csharp
sealed class BwMistPass : ScriptableRenderPass {
  sealed class PassData { public TextureHandle depthMS; public Material mat; public MaterialPropertyBlock mpb; public int pass; }
  static readonly int k_DepthMS = Shader.PropertyToID("_BwDepthMS");
  public BwMistPass() { renderPassEvent = RenderPassEvent.AfterRenderingSkybox; requiresIntermediateTexture = true; }
  public override void RecordRenderGraph(RenderGraph rg, ContextContainer frame) {
    var res = frame.Get<UniversalResourceData>();
    if (res.isActiveTargetBackBuffer) { /* log once; fallback */ return; }
    TextureHandle color = res.activeColorTexture, depth = res.activeDepthTexture;
    TextureDesc dd = rg.GetTextureDesc(depth);
    if (dd.msaaSamples != MSAASamples.MSAA4x || !dd.bindTextureMS) { /* fallback 7.5 */ return; }
    TextureDesc sd = dd; sd.name = "BwMistStencil"; sd.format = GraphicsFormat.D24_UNorm_S8_UInt;
    sd.bindTextureMS = false; sd.clearBuffer = true;
    TextureHandle st = rg.CreateTexture(sd);
    for (int p = 0; p < 2; p++)
      using (var b = rg.AddRasterRenderPass<PassData>(p == 0 ? "BwMist pixel" : "BwMist sample", out var d)) {
        d.depthMS = depth; d.mat = m_Mat; d.mpb = m_Mpb; d.pass = p;
        b.SetRenderAttachment(color, 0, AccessFlags.ReadWrite);
        b.SetRenderAttachmentDepth(st, p == 0 ? AccessFlags.Write : AccessFlags.Read);
        b.UseTexture(depth, AccessFlags.Read);
        b.SetRenderFunc(static (PassData d, RasterGraphContext ctx) => {
          d.mpb.SetTexture(k_DepthMS, d.depthMS);   // TextureHandle -> Texture only inside the render func (RenderGraphResourceTexture.cs:138)
          ctx.cmd.DrawProcedural(Matrix4x4.identity, d.mat, d.pass, MeshTopology.Triangles, 3, 1, d.mpb); // RasterCommandBuffer.cs:341
        });
      }
  }
}
```

- The two passes share the same attachments and may merge into one native pass. The camera colour stays MSAA until the grade pass reads it (then it is resolved), so per-sample blending is correct.

#### 7.5 Fallback (14.7 H2) needs a correction
- "Minimum depth of the 3×3 after the resolve" must mean the **nearest**: the minimum eye depth, which is the **maximum raw** value on D3D11.
- `_CameraDepthTexture` cannot serve, because it holds the farthest sample and thin rails can be missing from it (7.2).
- If the MSAA depth is bindable, write your own nearest-sample resolve (CopyDepth with `max`) into R32_SFloat, then take the 3×3 nearest.
- Only if it is not bindable, use `_CameraDepthTexture` with 3×3 max-raw and accept that sub-pixel rails lose their mask (log it).

#### 7.6 Custom queues Geometry+N
- The opaque renderer list covers `RenderQueueRange.opaque` (0-2500; `URP\Runtime\UniversalRenderer.cs:390`) sorted by `SortingCriteria.CommonOpaque` (SortingLayer, then **RenderQueue**, then front-to-back; `URP\Runtime\UniversalRenderPipeline.cs:1840-1845`). Geometry (2000) < Geometry+10 (`BwWaterPortal`, ZWrite Off) < Geometry+20 (whale, ZWrite On) are therefore drawn strictly in that order.
- Write `ZTest LEqual` in ShaderLab. Unity flips the comparison for reversed Z by itself; never hand-write GEqual.
- The portal must have **no DepthOnly, DepthNormals or ShadowCaster pass**. Otherwise a prepass would write pane depth and hide the whale.
- Snow and the 4 % glass reflection go at Transparent (3000), ZTest against interior and whale depth.

#### 7.7 `Graphics.RenderMeshIndirect` with explicit bounds
- **Pattern to copy:** The Ring's `InstancedField`:
  - `<WORKSPACE>\2nd Megastructures\2nd Megastructures\Assets\_Game\Scripts\Runtime\Rendering\InstancedField.cs:99-114` sets `new RenderParams(material)`, never `default`, because `renderingLayerMask` would be 0. It sets `worldBounds`, `shadowCastingMode` and `matProps`, and creates an `IndirectArguments` buffer of `IndirectDrawIndexedArgs.size`.
  - `:204-207` calls `Graphics.RenderMeshIndirect(in rp, mesh, args, 1, 0)`, with `rp.camera = cam` per capture camera.
- **Shader side:** `#define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs` and `#include "UnityIndirect.cginc"` (`C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Data\Resources\CGIncludes\UnityIndirect.cginc:45-86`; it is plain HLSL and works in URP), then `InitIndirectDrawArgs(0)` and `GetIndirectInstanceID(svInstanceID)`. On D3D this returns SV_InstanceID unchanged (`:64-69`).
- **Horns:** the compute shader appends to an `Append` buffer (`SetCounterValue(0)` each frame), then `GraphicsBuffer.CopyCount(append, args, 4)` (byte offset 4 = instanceCount).
- **Bounds:** re-centre `worldBounds` on the camera every frame, with a half-size of at least 1.65 km in x and z and y from -20 to +30. Bounds that are too small cause popping, because culling (camera and shadow) uses only these bounds.
- `unity_ObjectToWorld` is identity: compute positions and normals from the buffer. Forward+ clustered lights still apply. Indirect draws do not use the SRP Batcher. GPU Resident Drawer must be off.

#### 7.8 Depth format and reversed Z
- **Format.** The default resolves to `D32_SFloat_S8_UInt` on Windows (`CORE\Runtime\Utilities\CoreUtils.cs:1944-1951`). Set it explicitly through the public API: `UniversalRendererData.depthAttachmentFormat = DepthFormat.Depth_32_Stencil_8` (enum at `URP\Runtime\UniversalRendererData.cs:59-101`, setter at `:276-299`) and `depthTextureFormat` (`:303-318`). The Ring did exactly this (`2nd Megastructures\...\Scripts\Editor\Batch\UrpSetup.cs:60-61`). The attachment uses it via `UniversalRenderer.cs:212`.
- **Reversed Z** is a platform property, not a setting. D3D11 always has `UNITY_REVERSED_Z 1` (`CORE\ShaderLibrary\API\D3D11.hlsl:4-7`). Assert `SystemInfo.usesReversedZBuffer` at startup.

#### 7.9 HDR 64-bit precision and the project's current asset state
- **Format choice.** With `_32Bits`, `MakeRenderTextureGraphicsFormat` returns `B10G11R11_UFloatPack32` unless alpha is needed. Only `_64Bits` forces `R16G16B16A16_SFloat` (`URP\Runtime\UniversalRenderPipelineCore.cs:1477-1492`; enum `URP\Runtime\Data\UniversalRenderPipelineAsset.cs:166-175`, default `_32Bits` at `:452`, public property `:1096-1100`, `msaaSampleCount` setter `:1106-1110`).
- **Current state of `Assets\Settings\PC_RPAsset.asset`:** `m_HDRColorBufferPrecision: 0` (32-bit, line 27), `m_MSAA: 1` (disabled, line 28), `m_GPUResidentDrawerMode: 1` (InstancedDrawing, line 85).
- **Current state of `Assets\Settings\PC_Renderer.asset`:** Forward+ (`m_RenderingMode: 2`), copy depth after opaques, and an **active SSAO renderer feature** (lines 61-73) that forces a depth-normals prepass.
- **`PipelineConfigurator` must:**
  - set `hdrColorBufferPrecision = _64Bits`, `msaaSampleCount = 4`, `gpuResidentDrawerMode = Disabled`;
  - set the depth formats as in 7.8;
  - remove SSAO;
  - keep `requireDepthTexture` only if the fallback or another shader needs `_CameraDepthTexture`.
- Thread's precedent: `3rd Megastructures\...\Scripts\Editor\PipelineConfigurator.cs:70`.

---

### Spec deviations and decisions found by this research
1. **KS08 bow-top test.** The primary θ_c for G (41.0°) puts the luma peak at 24.56°, outside 25.4 ±0.5. Either use the Descartes channel set (42.25, 41.85, 41.30) / (50.55, 51.30, 52.30), or define the test on the R peak (3.4).
2. **MSAA mist.** It is feasible in a URP 17.6 raster pass on D3D11, but it needs its own MSAA stencil (D24S8, about 133 MB at 4K) or a discard-mask variant. The camera stencil cannot be used while its depth is sampled (7.3).
3. **Fallback wording.** "Minimum depth" means nearest, which is maximum raw. The stock `_CameraDepthTexture` holds the farthest sample and can lose sub-pixel rails (7.2, 7.5).
4. **Far-field intersection.** Use `t = (R+h) s² / (-dy + sqrt((-dy-s)(-dy+s)))` and arc-length pattern coordinates (5).
5. **Amber mist rivers.** Use the local sun elevation at P, not the eye's (5).
6. **Mist appearance.** Smooth the dark-band edges and prefer an asymmetric bow lobe. g 0.55 is a calibrated, non-physical choice: back-scatter is about 4.2 times Mie (3.4, 3.5).
7. **Near sheets** turn white and broken within about 20 m (Horeni) (3.1).
8. **Marine snow** drift of 0.03 m/s is 10-100 times real; note only (4).
9. **The 4 % glass reflection** must be a Transparent-queue pass so it covers the whale (4).
10. **Assets out of spec.** The current pipeline assets violate Part 3.2 #5 (32-bit HDR, MSAA off, GPU Resident Drawer on, SSAO) (7.9).

### Sources (accessed 2026-11-13)
- Sadeghi et al. 2012, Physically-based simulation of rainbows: https://cs.dartmouth.edu/wjarosz/publications/sadeghi11physically.html ; https://graphics.unizar.es/papers/sadeghi11physically.pdf [F]
- Carrillo, Ortega, Castillo & García 2021, Air-Water Properties in Rectangular Free-Falling Jets (Horeni formula): https://www.mdpi.com/2073-4441/13/11/1593 [F, via search]
- Ocean Optics Web Book, classification schemes (Jerlov): https://www.oceanopticsbook.info/view/inherent-and-apparent-optical-properties/classification-schemes [F]
- Solonenko & Mobley 2015, Appl. Opt. 54(17):5392, doi 10.1364/AO.54.005392 [F, via search]
- Mobley 1994, Light and Water; Pope & Fry 1997; Smith & Baker 1981; Petzold 1972; Zaneveld & Pegau 2003; Morel & Prieur 1977; Alldredge & Silver 1988 [S]
- Okinawa Churaumi Aquarium: https://en.wikipedia.org/wiki/Okinawa_Churaumi_Aquarium ; Dubai Aquarium: https://en.wikipedia.org/wiki/Dubai_Aquarium_%26_Underwater_Zoo [F]
- Hillaire 2020: https://sebh.github.io/publications/egsr2020.pdf (too large to fetch) ; https://github.com/sebh/UnrealEngineSkyAtmosphere ; https://raw.githubusercontent.com/sebh/UnrealEngineSkyAtmosphere/master/Resources/RenderSkyCommon.hlsl [F]
- Haines, Günther & Akenine-Möller 2019, Precision Improvements for Ray/Sphere Intersection, Ray Tracing Gems ch. 7 [S]
- Benjamin 1968; Huppert & Simpson 1980; Simpson 1997 [S]
- Quilez: https://iquilezles.org/articles/filterableprocedurals ; https://iquilezles.org/articles/checkerfiltering/ ; https://iquilezles.org/articles/morecheckerfiltering ; https://iquilezles.org/articles/bandlimiting/ (returned 404 when fetched; listed by search) [F, via search]
- Gjøl, Banding in games: http://loopit.dk/banding_in_games.pdf [F, binary] ; Peters, blue noise: http://momentsingraphics.de/BlueNoise.html [S] ; Lipshitz et al. 1992; Ulichney 1993 [S]
- Persson 2013, Phone-Wire AA (GPU Pro 4) [S] ; Jendersie & d'Eon 2023 [S]
- ffmpeg H.264 guide: https://trac.ffmpeg.org/wiki/Encode/H.264 ; YouTube encoding settings: https://support.google.com/youtube/answer/1722171 [S]
- Microsoft, ID3D11DeviceContext::ResolveSubresource: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-resolvesubresource [F]
- Unity, SystemInfo.supportsMultisampleResolveDepth: https://docs.unity.com/en-us/engine/current/script-reference/unityengine/systeminfo/supportsmultisampleresolvedepth [F]
- Package source as cited above (URP 17.6.0 and Core 17.6.0 under Library\PackageCache) and the Unity 6000.6.0f1 editor's `UnityIndirect.cginc` [read locally]

---

<a id="report-c"></a>

## C. R-AUDIO-UI: Procedural low-frequency water audio, FDN hall reverb, look-ahead limiter, modal footsteps, OnAudioFilterRead in 6000.6, and UI Toolkit runtime text (Bulwark 14.9 item 8, 14.3, 14.11)

Read-only research for Bulwark, accessed 2026-11-13. Target: Unity 6000.6.0f1, URP 17.6, Windows player, D3D11.

**Tags**
- **VERIFIED-LOCAL**: read from this machine's Unity 6000.6.0f1 install, from this project's files, or from the earlier games' code, logs and assets.
- **VERIFIED-DOCS**: an official page I opened (URL below).
- **SEARCH**: taken from a search-result summary only; the page was not opened, so verify it before relying on it.
- **DERIVED**: arithmetic done here. The numbers were computed with `python -I`.
- **CHOSEN**: not in the specification; a proposed value that must be logged as a decision.

---

### 0. Decisions in one screen

1. **Engine wiring.** Use one `OnAudioFilterRead` generator (`BwSynth`) on a dedicated "Audio" GameObject.
   - That GameObject carries a clip-less 2D `AudioSource` and is not the AudioListener's GameObject.
   - Order: create the AudioSource (playOnAwake false) → add `BwSynth` → build the DSP → `src.Play()`. Play() is the call where Unity classifies the source as a generator.
   - **Everything is synthesised inside `BwDsp`, including the footsteps.** That way one chain (DC blocker → room FDNs → master gain → look-ahead limiter) processes the whole mix, and the EditMode offline render is bit-for-bit the same code path.
   - Thread's primary path was a *listener* filter with a generator fallback. Part 3.2 #14 forbids the listener path, so invert it and drop the fallback.
2. **Sample rate.**
   - Set `ProjectSettings/AudioManager.asset` `m_SampleRate: 48000` from `ProjectConfigurator`. It is **0** today (device default; VERIFIED-LOCAL).
   - At runtime, call `AudioSettings.Reset` with 48 kHz *before* the generator is created (not in batch mode), like The Ring did.
   - Always build the DSP from `AudioSettings.outputSampleRate`, and rebuild it plus `Play()` again on `OnAudioConfigurationChanged`.
   - Render offline at 48 kHz. Do the dump comparison at the dump's own rate.
3. **Blocks** (Thread code, reused):
   - Kellet refined pink.
   - Simper/Cytomic trapezoidal SVF.
   - xorshift32 streams.
   - Sine table.
   - Per-sample one-pole smoothing.
   - Impulse-energy calibration so that every layer generator has unit RMS.

   New:
   - **Brown noise** = a leaky integrator with a 20 Hz corner, normalised to unit variance.
   - **DC blocker at 20 Hz.** Thread used 10 Hz. Compensate its in-band loss in the calibration: −1.5 dB at 31 Hz, −0.9 dB at 41 Hz.
   - **Layer-gain smoothing τ = 50 ms.** Thread used 20 ms; the spec says "layers ramp over at least 50 ms".
4. **Reverb: two 8-line Householder FDNs (Thread's `FdnReverb8`) with per-line Jot absorption filters.** Thread used one unity-DC damping low-pass, which cannot express an HF ratio.
   - **Hall FDN**: delay lengths derived from the 13 × 28 × 300 m room. Total 0.918 s (supports T60 up to 6.1 s); RT 5.5 s, HF ratio 0.35 at 5 kHz.
   - **Small FDN**: Thread base primes × 1.15. Total 0.467 s (supports up to 3.1 s). Used for the gallery, the exterior, the atrium, the stairwell (3.0 s), the tunnel and the ledge.
   - Rooms switch as a step function of s. Time-domain crossfades: 0.6 s at s 120 and s 920, 3 s at s 1070 and s 1373, at least 2 s elsewhere.
   - Do **not** use `AudioReverbFilter`. It is native FMOD, the offline renderer cannot reproduce it, and its `reflectionsLevel` default of −10000 mB silences the early reflections.
5. **Limiter.** Port The Ring's `DspLimiter`, a Signalsmith-style look-ahead design: moving minimum, box smoothing, tanh knee.
   - Ceiling −1.42 dBFS: this is the "asymptote −1.42 dBFS".
   - Threshold −4 dBFS (CHOSEN).
   - **Look-ahead 16 ms** instead of 5 ms, so the gain never moves within one cycle of the 31-63 Hz content.
   - Hold 20 ms; release 250 ms (CHOSEN).
   - Buffer capacity 4096.
   - The limiter sits after the reverb, inside `BwDsp`.
   - Measure the true peak with a 4× oversampler (BS.1770 Annex 2 method).
6. **Footsteps.** 3 ms Hann noise bursts into constant-peak-gain two-pole resonators (zeros at ±1). This is Thread's `Resonate`.
   - Decays are read as T60 (Thread D-021).
   - Stride L(v) = 0.75 + 0.1 (v − 1.5) m (0.75 m at 1.5 m/s, 0.85 m at 2.5 m/s); 0.9 m on the stairs.
   - Alternate feet ±0.8 dB, pitch ±4 %, peak −30 dBFS.
   - Steps are mixed by a voice pool inside `BwDsp`, so they reach the room FDN and the limiter.
7. **`BulwarkScore`.**
   - Pure function of (s, t, latched event times, intro or ending clocks, pause, listener yaw).
   - `Render(speed)` drives a deterministic motion: 0.5 s start ramp, constant speed, goal trigger at s 1498.5, 1.5 m stop at s 1500, ending clocks.
   - Impacts come from `SeaState.Impacts(t)` on the **same clock** as the player (t = seconds since LoadComplete). The first impact is at t = 0.5 s and is **exempt from the intro fade** (14.11, T9).
8. **UI.** Import The Ring's `UiDirector` + `UiTimeline` with Bulwark constants:
   - Title 56 px, 0.22 em → **12.32 px** letter-spacing at the 1080 reference.
   - Title centre 38 %, alpha 0.88; end line 51 px with 11.22 px spacing.
   - Black 0-1.0 s, fade 1.0 → 3.5 s; title 1.4 / 2.6 / 4.6 / 6.0 s; hint at 6.5 s.

   Rules:
   - USS has **no em unit** in 6000.6 (px and % only; VERIFIED-LOCAL and VERIFIED-DOCS), so compute px in code.
   - The PanelSettings asset and a two-line `.tss` (`@import url("unity-theme://default")`) come from the editor script. Add the UI shaders to Always Included Shaders.
   - Font: the theme default font first; if it measures blank, `FontDefinition.FromFont(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"))`. The Ring's `FontCheck` already implements the switch; flip its default mode to Theme.
   - Set `UiDirector.ShaderFade = true` and feed `PictureMul` into BwGrade's fade multiplier, so the picture fade is applied before the one dither (Thread's grade already does this). Do not import Thread's `FadeController`, `GlyphText` or `PauseOverlay`.
9. **Verify text in the built player.** Measure the ink of the title in an overlay capture. The Ring's player log implies the effective tracking was only about 55 % of the requested px (section 9.6).

---

### 1. `OnAudioFilterRead` in Unity 6000.6: rules and pitfalls

| # | Fact | Tag / source |
|---|---|---|
| A1 | `void OnAudioFilterRead(float[] data, int channels)`. Data is interleaved floats in [-1, 1]; frames = `data.Length / channels`. It is called about every 20 ms; this project's buffer is 1024 frames (21.3 ms at 48 kHz). Never hard-code the length. | VERIFIED-LOCAL `Documentation/en/ScriptReference/MonoBehaviour.OnAudioFilterRead.html` (6000.6); VERIFIED-DOCS [U1]; `AudioManager.asset` `m_DSPBufferSize: 1024` |
| A2 | "If this is the first filter in the chain and a clip isn't attached to the audio source, this filter will be played as the audio source." The 6000.6 page has no deprecation note. | VERIFIED-LOCAL; VERIFIED-DOCS [U1] |
| A3 | Generator detection happens in `AudioSource.Play` / playOnAwake. A clip-less source's first OAFR MonoBehaviour is the generator and "doesn't receive the contributions of other inputs" (closed as by design). Add every component first, activate the GameObject, then call `Play()` once. | SEARCH [U3]; The Ring research 05 F8 |
| A4 | It runs on the audio thread. No UnityEngine calls (`Time`, `Debug.Log`, `AudioSettings.outputSampleRate`, `GetComponent`, UnityEngine `Random`), no allocation, no locks, no logging. | VERIFIED-LOCAL; Thread R5 §2 |
| A5 | One OAFR component may serve either one AudioSource or the AudioListener, never both (UUM-61733). Never put `BwSynth` on the camera or listener GameObject. | The Ring 05 F11; The Ring `AudioDirector.Awake` guard |
| A6 | `AudioSource.Stop()` does not stop an OAFR generator (UUM-117120, open). Mute with a DSP gain. Never set `AudioSource.volume = 0`; keep priority 0 so the source is never virtualised. | Thread R5 §1; The Ring 05 F9 |
| A7 | OAFR is attached to the scripting thread and is sensitive to GC interrupts (UUM-122520, Won't Fix). Allocate nothing per frame. Thread measured 4 KB of engine-side allocation per callback (D-033); this is harmless with incremental GC on. | SEARCH [U4]; Thread `DEVELOPMENT.md` D-033 |
| A8 | UUM-116871 (listener filter not applied after enable) is fixed in 6000.6.0a5. It is irrelevant once the listener path is dropped. | Thread R5 [S5] |
| A9 | The AudioSource's "Bypass Effects" does not bypass OAFR effects (tracker title). Nothing in Bulwark relies on it. | SEARCH [U5] |
| A10 | Sample rate. `AudioSettings.outputSampleRate` is main-thread only and cannot be set from script; the project setting is a request. `AudioSettings.Reset(cfg)` raises `OnAudioConfigurationChanged(false)` and stops sources, so re-`Play()` afterwards. A device change (HDMI or USB) raises the same event. | The Ring 05 §2; Thread R5 §1 |
| A11 | This project today: `m_SampleRate: 0`, `m_DSPBufferSize: 1024`, `Default Speaker Mode: 2`, `m_DisableAudio: 0`, `runInBackground: 0`. | VERIFIED-LOCAL `ProjectSettings/AudioManager.asset`, `ProjectSettings.asset:90` |
| A12 | The Ring's player (6000.6.0f1, this PC) logged: generator on an AudioSource, 48000 Hz, 2 channels, 1024 frames per callback, DSP buffer 1024 × 4, callbacks running. Thread's player logged 48000 Hz, 1024 × 4, stereo. | VERIFIED-LOCAL `2nd…/QA/autotest/at01/player.log`, Thread logs |
| A13 | `AudioReverbFilter` pitfall: `reflectionsLevel` defaults to **−10000 mB** (early reflections off), and fields only take effect after `reverbPreset = AudioReverbPreset.User`. The ScriptReference text for `reflectionsDelay` is a copy-paste error; the real range is 0-0.3 s. Per-frame parameter changes are not guaranteed click-free, and the offline C# render cannot reproduce FMOD's reverb. **Bulwark: no AudioReverbFilter, no AudioReverbZone.** | The Ring 05 §4 (VERIFIED-LOCAL docs + XML); The Ring `AudioTests.cs:652` |
| A14 | The new scriptable audio pipeline (`IAudioGenerator`, `AudioSource.generator`) exists in 6000.6. It is not needed; stay with OAFR as Part 3.2 #14 requires. | The Ring 05 F19 |
| A15 | Batchmode / `-nographics`: no documented audio behaviour. EditMode tests never touch OAFR; they call `BwDsp.Process` directly (Part 11.9). | Thread R5 §1 |

#### 1.1 Wiring recipe (main thread)

```csharp
// BwAudioDirector.CreateRig(): call from GameBootstrap, never on the camera / AudioListener object.
var cfg = AudioSettings.GetConfiguration();
if (!Application.isBatchMode && cfg.sampleRate != 48000) { cfg.sampleRate = 48000; AudioSettings.Reset(cfg); } // before any Play()
int fs = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 48000;   // log it
var go = new GameObject("Audio");                      // active; no AudioListener here
var src = go.AddComponent<AudioSource>();              // first component = top of the chain
src.clip = null; src.playOnAwake = false; src.loop = true; src.spatialBlend = 0f; src.volume = 1f;
src.panStereo = 0f; src.dopplerLevel = 0f; src.priority = 0; src.bypassReverbZones = true;
var synth = go.AddComponent<BwSynth>();                // OAFR generator = first filter on a clip-less source
synth.Init(fs, seed);                                  // builds BwDsp (allocates + calibrates) on the MAIN thread
src.Play();                                            // generator detection happens here
AudioSettings.OnAudioConfigurationChanged += changed => { /* rebuild BwDsp at the new fs, SnapTo(current), src.Play() */ };
```

Audio-thread body (Thread's `ThreadSynth` pattern):
- `Array.Clear(data, 0, data.Length)`. Generator input is silence; overwrite it rather than add to it.
- Read the published params (double-buffer mailbox with a volatile index).
- Drain the footstep and event SPSC ring.
- `dsp.Process(data, channels, in p)`.
- Optional `AudioCapture.Write`.
- `Interlocked.Increment(callbacks)`.
- `catch (Exception e) { lastError = e; }`.

#### 1.2 Thread-safety checklist (keep everything Thread had)

- **Parameters:** Thread's double-buffer mailbox (`m_front` volatile) is sufficient. A torn read mixes two consecutive frames' targets, and the 50 ms smoothing hides it.
- **Discrete events** (footsteps, forced impacts, relay clicks) **must not** go through the mailbox, where they could be lost or duplicated. Use a fixed-size SPSC ring:
  - The producer writes the slot, then `Volatile.Write(head)`.
  - The consumer `Volatile.Read(head)`, then reads the slot, then `Volatile.Write(tail)`.
  - Better still, keep the cadence inside the DSP, keyed to the published s (section 6.4).
- **DSP rebuild:** construct on the main thread, `Volatile.Write` the reference, and snap it to the current targets (`SnapTo`).
- **Static tables** (`SineTable`) are warmed in `Awake` so no static constructor runs on the audio thread.
- **NaN guard:** if the block sum is non-finite, reset the whole DSP, output silence and count it; the main thread logs it once. **Flush denormals** after every block (FDN, SVF and brown states).
- **Heartbeat:** count callbacks; the main thread checks for more than 0 after 1 s and 20 frames. Set `Application.runInBackground = true` for `-autotest`, `-capture` and `-audio-dump` (the project has 0).
- **Dump:** Thread's `AudioCapture` + `WavWriter` + JSON sidecar. Because the whole chain is inside `BwDsp`, the in-callback capture is the final mix. It cannot see the AudioSource's 2D pan law; an optional cross-check compares its RMS with `AudioListener.GetOutputData` blocks over the same seconds, which should agree within 0.5 dB.

---

### 2. DSP building blocks

| Block | Formula / implementation | Notes |
|---|---|---|
| **Kellet pink** (refined, "pk3") | `b0=.99886b0+.0555179w; b1=.99332b1+.0750759w; b2=.96900b2+.1538520w; b3=.86650b3+.3104856w; b4=.55000b4+.5329522w; b5=-.7616b5-.0168980w; pink=b0+…+b6+.5362w; b6=.115926w` | "Accurate to within ±0.05 dB above 9.2 Hz (44.1 kHz)". Unity gain at Nyquist. For uniform white in [-1, 1) the RMS is 1.755, so multiply by 1/1.755; crest about 13.7 dB (Thread R5, measured). Reuse `PinkKellet` as is. [D1] |
| **Brown** (new) | `y = a·y + sqrt(3(1−a²))·w`, with `a = exp(−2π·20/fs)`, w uniform in [-1, 1) | Unit variance, 1/f² (−6 dB/oct) above 20 Hz, flat below (Ornstein-Uhlenbeck), followed by the 20 Hz DC blocker. At 48 kHz: a = 0.9973854, gain 0.125168. **Do not** use the common `(b + 0.02w)/1.02` form: at 48 kHz its corner is 151 Hz, not "brown" where the water lives. [D2] |
| **Simper/Cytomic SVF** (TPT) | `g=tan(πfc/fs); k=1/Q; a1=1/(1+g(g+k)); a2=g·a1; a3=g·a2`. Tick: `v3=x−ic2; v1=a1·ic1+a2·v3; v2=ic2+a2·ic1+a3·v3; ic1=2v1−ic1; ic2=2v2−ic2`. LP = v2, BP = v1 (k·v1 = 0 dB peak), HP = x − k·v1 − v2 | Stable for every fc below Nyquist; frequency response equals the bilinear-transform design; safe to modulate per 32-sample block. The Chamberlin form diverges above about 8.3 kHz at Q 0.707 (Thread R5, measured). Reuse `SvfTpt` as is. A band "lo-hi Hz" means `fc = sqrt(lo·hi)`, `Q = fc/(hi−lo)` (Thread convention); for 120-900 Hz that is fc 328.6, Q 0.421. [D3] |
| **DC blocker** | `y = x − x1 + R·y1`, `R = exp(−2π·20/fs)` | 14.3 says 20 Hz (Thread used 10 Hz). In-band loss to compensate in calibration (DERIVED): 31 Hz −1.50 dB, 38 Hz −1.05, 41 Hz −0.92, 46.5 Hz −0.73, 60 Hz −0.45, 63 Hz −0.41. |
| **Smoothing** | One-pole per sample: τ = 50 ms for layer gains (CHOSEN, meets "≥ 50 ms ramps"), 20 ms for cutoffs. Slow targets (reverb RT, 8 s silences) run in **double** at control rate. | A float one-pole with τ above about 1 s stalls short of its target (Thread R5). |
| **Sines** | 4096-entry table plus a guard sample, linear interpolation (max error 3.5e-7), double phase accumulators | FM layers (P, V) integrate the instantaneous frequency in the double phase: no clicks. |
| **Calibration** | Every static chain is normalised by impulse-response energy; sines in closed form, including the DC-blocker magnitude; modulation power `1 + d²/2` divided out. | Thread `ThreadDsp.Configure`, reused as the pattern. A table gain g then gives exactly 20·log10(g) dBFS RMS. |

---

### 3. Water and layer recipes (14.3), keyed to s and the clock t

Physics notes that justify the recipes:
- **Mass law.** A wall passes low frequencies: transmission loss TL ≈ 20 log10(m·f) − 48 dB, about +6 dB per octave [P1]. For the 0.6 m gallery wall (m = 1440 kg/m²) TL is about 47 dB at 41 Hz, 57 dB at 125 Hz and 69 dB at 500 Hz (DERIVED). So the beat 1 sea "through concrete" is a low-pass at about 250 Hz plus the booms, and the slits add a little air path.
- **Surf.** Plunging breakers radiate low-frequency energy below about 20 Hz and audible "booms" from the entrained air pocket. A pocket resonance near 75 Hz for a 1 m wave is reported (SEARCH [P2, P3, P4]), and larger waves resonate lower. That supports booms at 41 and 63 Hz for Hs 2.8-3.2 m.
- **Procedural sea beds.** The common recipe is noise → low-pass at a few hundred Hz, with **two slightly different slow modulations** for level and for cutoff, or a quasi-random LFO so it never repeats audibly [P5, P6].
- **Variation.** Listeners prefer water sounds with **large temporal variation** and low sharpness [P7]. Add slow random level and cutoff drift (±1.5 dB, 0.1-0.7 Hz) to the roar and conduit layers.

| ID | Recipe to implement (14.3 values plus CHOSEN details) | Reuse |
|---|---|---|
| **W wind** | Pink → SVF BP 380 Hz Q 0.7, cutoff ±30 % by a 0.06 Hz LFO; plus BP 1100 Hz (Q 2, CHOSEN) at −9 dB. Gust knots every 1/0.13 s = 7.7 s, cosine-interpolated (`0.55 + 0.45 n`). Pan +0.35 (sea side, right) in beats 2-4. Extra LP 400 Hz in beat 5. **No W inside the tunnel**: the B7 tunnel power sum −25.4 dBFS only closes without W (DERIVED); W rises at S7. | Thread `Wind()` with new constants plus a post LP and a pan. |
| **S sea** | Brown → SVF LP 700 Hz (250 Hz in beat 1; cutoff blended in log-frequency at the door). Swell from `SeaState` (normalised wall elevation `w(t)` in [-1, 1], sum of components near Tp): amplitude `1 + 0.35w`, cutoff `× 2^(0.4w)` (CHOSEN). "HP wash −14 dB": pink → HP 1.5 kHz → LP 6 kHz gated by `max(0, w)²` (run-up). Beat 4 crash bursts at plume impacts: pink → BP 800 Hz Q 0.5, attack 80 ms, T60 2.5 s (CHOSEN). | New generator, calibrated like Thread (modulation power divided out). |
| **B boom** | Event voices from `SeaState.Impacts(t)` (strength 0.3-1.0). Sines at 41 and 63 Hz, T60 1.6 s, attack 10 ms (raised cosine, CHOSEN). A 3 ms Hann noise click → LP 180 Hz. Brown → LP 250 Hz rumble with T60 0.9 s. Beat 6: only a 31 Hz sub. Optional pan by the impact z relative to the listener. **The impact at t = 0.5 s bypasses the intro fade.** | Thread's star-voice pool (attack, decay, stealing) as the pattern. |
| **F fall roar** | Brown → LP 1.1 kHz, plus pink → BP 300 Hz, plus a 38 Hz sub (mix 0 / −3 / −9 dB, CHOSEN). Gain `1/(1 + (d/40)^1.2)`, d = eye distance to the nearest slot (z 424+20k..440+20k) or outlet. S3 +12 dB over s 380-520 (20 m smootherstep edges, CHOSEN). Slow drift ±1.5 dB. | New; distance in `BulwarkScore` (analytic, double). |
| **T room** | Beat 1: brown → LP 160 Hz. Beat 5: ballast sines 100 and 200 Hz (a mains-hum family). | New (trivial). |
| **P pressure** | Sines 31.0 and 46.5 Hz, FM 0.03 Hz ±3 Hz (phase-integrated), level a function of eye depth below the sea: B5 −46 → B6 −39 → B7 −45. | New. |
| **V whale** | At s 1222: FM sine 33 → 47 → 36 Hz over 6.5 s (smoothstep segments in log f), attack 0.8 s and release 1.5 s (CHOSEN). Pan = sin(bearing of the whale centre `z_c(t, s)` relative to the listener yaw); offline uses the gaze schedule yaw. Creaks at −48 dBFS from short modal bursts at 180-600 Hz. Relay clicks: 2 ms at −34 dBFS at s 1219 and 1226 (14.5.6 says "at each switch": log which one wins). | New. |
| **C conduit** | Brown → LP 500 Hz, plus pink → BP 120-900 Hz (fc 328.6, Q 0.421), plus a 60 Hz sub. Gain bell over s 1380-1423 (smootherstep up and down, CHOSEN), tunnel level −26 dBFS. Turbulence: ±2 dB level and ±15 % BP-centre drift from filtered random walks at 0.5-3 Hz. S7 at the mouth (s 1423): C and F −14 dB over 1.5 s. | New. |

**Silences and swells.** Use spatial keys in s and timed ramps from latched crossing times; the offline renderer computes the same latches from `MotionS`.
- S1: no boom for s 100-108 (last boom at s 100); beds duck to "near silence" over s 100.5-104.
- S2: from the door trigger at s 104, a dB-linear smootherstep from −60 dBFS to beat 2 levels over 3.0 s, matching the door gap curve.
- S3: as in the F row.
- S4: at s 920 the beat 4 → 5 change is a 0.6 s timed ramp, not a 30 m crossfade.
- S5: at s 1070, every layer except P and B is below −50 dBFS for 8 s. Footsteps are included unless the lead decides otherwise; log it.
- S6: whale call at s 1222.
- S7: at the mouth, as in the C row.
- S8: audio follows `UiTimeline.EndPicture`.
- Pause duck: −12 dB over 0.4 s (Thread used −20 dB over 0.6 s; change it).

**Power sums (DERIVED, they match 14.3):**

| Beat | Power sum (dBFS) |
|---|---|
| B1 | −33.70 |
| B2 | −23.90 |
| B3 | −18.31 |
| B4 | −22.48 |
| B5 | −31.68 |
| B6 | −37.51 |
| B7 tunnel (C, F, S, B, P; no W) | −25.37 |
| B7 ledge (W, S, B, F, P, hum −52) | −33.94 |

**Boom level calibration.** Long-term B power = λ · E[strength²] · E_env, where E_env is the energy of one boom with unit amplitude, computed numerically as Thread computes its star notes. Uniform strength in 0.3-1 gives E[strength²] = 0.4633. λ = 1/9 s (beats 1-2) or 1/6 s (beat 4). For a stable per-beat RMS use a **stratified schedule**: interval = mean × (0.7 + 0.6u), strengths from a low-discrepancy sequence, first impact fixed at t 0.5 s. A Poisson schedule gives about ±2 dB per 48 s beat.

---

### 4. FDN reverb sized from the room

#### 4.1 Window hall: 13 × 28 × 300 m, RT 5.5 s, HF ratio 0.35 (DERIVED)

| Quantity | Value |
|---|---|
| Volume V | 109,200 m³ |
| Surface S | 25,328 m² |
| Mean free path 4V/S | 17.25 m (50.3 ms) |
| Sabine absorption A = 0.161 V / T | 3,197 m² (mean α 0.126; Eyring α 0.119) |
| Schroeder frequency 2000·sqrt(T/V) | 14.2 Hz (diffuse field over the whole band) |
| Mode-density rule ΣM ≥ 0.15·T60·fs [R1] | at least 39,600 samples (0.825 s) |
| Mean-free-path rule (JOS: a lower bound for the mean delay) [R2] | at least 2,414 samples |
| First reflections | side wall about 32 ms (5.5 m from the axis); glass about 35 ms; ceiling slap 153 ms (26.3 m above the eye) |
| Flutter between the parallel 13 m walls | 13.2 Hz |

**Delay set.** Eight delays spaced geometrically from the **width round trip** 2W/c = 75.8 ms to the **height round trip** 2H/c = 163.3 ms, rounded to distinct primes:

`3637, 4057, 4523, 5051, 5641, 6299, 7019, 7841` samples at 48 kHz (75.8-163.4 ms)

- Sum 44,068 samples = 0.918 s, which supports T60 up to 6.12 s.
- Distinct primes are mutually prime, as JOS recommends [R3].
- At other rates, scale by fs/48000 and re-prime once (Thread's constructor).
- For comparison, Thread's base primes × 2.2 give 3137-7789 samples (0.894 s, up to 5.96 s), almost the same span, so `FdnReverb8` only needs a constructor that accepts an explicit length array.

**Per-line absorption filter (Jot), matched exactly at DC and at the HF reference of 5 kHz.** HF ratio is "the ratio of the decay time at high frequencies to the decay time at low frequencies" [R4], at HFReference 5000 Hz (I3DL2 default) [R5]. For line i (length M):

```
R0  = 10^(-3 M / (T0  fs))        T0  = 5.5 s
Rhf = 10^(-3 M / (Thf fs))        Thf = 0.35 · 5.5 = 1.925 s, at f_hf = 5000 Hz
rho = (Rhf/R0)^2,  c = cos(2π f_hf / fs)
p   = [ (1 - rho c) - sqrt((1 - rho c)^2 - (1 - rho)^2) ] / (1 - rho)     // root with |p| < 1
g   = R0 (1 - p)                                                          // H(z) = g / (1 - p z^-1), |H(1)| = R0
in the loop:  lp_i = g_i · y_i + p_i · lp_i      (replaces Thread's  lp += dampC (y - lp); g = gain · lp)
```

This is the two-point form of JOS "First-Order Delay Filter Design" [R6]. JOS matches DC and Nyquist; here the second point is moved to 5 kHz. Results at 48 kHz (DERIVED):

| M | p | g | T60 at DC | 1 kHz | 5 kHz | 10 kHz | Nyquist |
|---|---|---|---|---|---|---|---|
| 3637 | 0.38634 | 0.557956 | 5.500 | 5.04 | 1.925 | 0.94 | 0.58 |
| 4057 | 0.40878 | 0.531679 | 5.500 | 5.03 | 1.925 | 0.96 | 0.60 |
| 4523 | 0.43158 | 0.504978 | 5.500 | 5.02 | 1.925 | 0.98 | 0.63 |
| 5051 | 0.45518 | 0.477371 | 5.500 | 5.01 | 1.925 | 1.00 | 0.65 |
| 5641 | 0.47920 | 0.449332 | 5.500 | 5.00 | 1.925 | 1.03 | 0.68 |
| 6299 | 0.50356 | 0.421006 | 5.500 | 4.98 | 1.925 | 1.05 | 0.71 |
| 7019 | 0.52777 | 0.392998 | 5.500 | 4.96 | 1.925 | 1.08 | 0.74 |
| 7841 | 0.55283 | 0.364223 | 5.500 | 4.94 | 1.925 | 1.11 | 0.78 |

- Every line decays at the same rate per second at every frequency. A single shared damping cutoff, as in Thread, makes long lines darker than short ones and cannot hit a stated HF ratio.
- Recompute (g_i, p_i) at control rate from the smoothed T0 and Thf. Changing gains and poles is click-free; delay lengths never change (Thread R5: no click when RT jumped from 7 s to 0.15 s).
- Jot's **tonal-correction** filter T(z) equalises modal energy across bands [R7]. Omit it: the darker hall spectrum, about −4.6 dB of tail energy at 5 kHz, is the intended look of concrete.

#### 4.2 Stairwell and the other rooms: small FDN

- **Stair geometry:** inclined tunnel 3.6 × ~3.0 × 97.8 m. V ≈ 1056 m³, S ≈ 1313 m², mean free path 3.22 m (9.4 ms), Sabine α 0.043 for RT 3.0 s (bare concrete and steel, plausible), Schroeder frequency 107 Hz. Modal rule: ΣM ≥ 21,600 samples.
- **Small FDN:** Thread base primes × 1.15 = `1637, 2053, 2269, 2411, 2963, 3313, 3701, 4073` (sum 22,420 = 0.467 s, up to 3.11 s, mean 58 ms).
- **Exterior rooms:** at RT 0.5 s the loss per pass is only 4-10 dB, so echo density holds. Thread warned that lines above 170 ms give single echoes at short RT, which is why one long FDN is not used for everything.

Room table: RT from 14.3. Pre-delay from the first reflection path. HF ratio and wet level outside the hall are CHOSEN starting points to be trimmed in `QA/audio-trims.json`.

| Room (s) | FDN | RT s | Pre-delay ms | HF ratio | Wet (tail rel. dry) | Transition into it |
|---|---|---|---|---|---|---|
| Gallery (0-120) | small | 1.4 | 11 (walls 2 m) | 0.5 | −14 dB | — |
| Crest (120-420) | small | 0.7 | 41 (parapet 7 m) | 0.6 | −26 | 0.6 s at the sill, s 120 |
| Spillway (420-700) | small | 0.9 | 40 | 0.6 | −24 | ≥ 2 s |
| Armour (700-920) | small | 0.8 | 41 | 0.6 | −26 | ≥ 2 s |
| Atrium (920-936) | small | 2.2 | 26 (walls 4.5 m) | 0.5 | −12 | 0.6 s hard edge, s 920 |
| Stairwell (936-1035) | small | 3.0 | 10 (walls 1.8 m) | 0.45 | −10 | ≥ 2 s |
| Exit hall (1035-1070) | small | stair → hall | — | — | — | — |
| Window hall (1070-1373) | **hall** | 5.5 | 32 | **0.35** | −6 | 3 s, s 1070 |
| Tunnel (1373-1423) | small | 2.0 | 9 (walls 1.6 m) | 0.4 | −14 | 3 s, s 1373 |
| Ledge (1423-1500) | small | 0.5 | 19 (lee face 3.2 m) | 0.6 | −30 | ≥ 2 s at the mouth |

- **Room selection** is a step function of s. Time-domain blends make the durations identical at both walking speeds; a 0.6 s edge would be 1.5 m at 2.5 m/s but 0.9 m at 1.5 m/s if keyed to s.
- **Within the small FDN:** glide RT, HF ratio and wet with a double one-pole, τ = T/4.6. Change pre-delay with Thread's two-tap crossfade, its duration set to T (minimum 0.6 s) instead of a fixed 1 s.
- **Between FDNs** (s 1070 and s 1373): equal-power crossfade (cos/sin) of both the input sends and the output wets over T = 3 s. The outgoing tail fades rather than ringing 5.5 s into the tunnel.
- **Wet calibration:** keep Thread's `wetNorm = wet·sqrt(2 N M̄/τ)`, τ = RT·fs/ln 1000. With HF damping it slightly over-estimates, so trim per room from the offline render.
- **Optional:** feed the hall FDN through a 3-tap early-reflection block (32, 35 and 153 ms, −6 / −8 / −14 dB, CHOSEN) for the ceiling slap.
- **CPU:** both FDNs, the limiter and every layer are a few hundred flops per sample, under 2 % of one core. Thread measured its whole chain at 0.45 % of real time.

---

### 5. Look-ahead limiter (post-reverb, in `BwDsp`)

**Design** (Signalsmith [L1], ported by The Ring as `DspLimiter`):
1. Per sample, compute the gain required to keep the stereo-linked peak under the knee. The Ring uses a tanh knee in dB: `out = thr + (ceil − thr)·tanh((p − thr)/(ceil − thr))`, which gives the **asymptote at the ceiling**.
2. Take the moving minimum over L samples (peak hold).
3. Release exponentially, clamped to never exceed that minimum.
4. Box-average over L.
5. Delay the audio by L − 1 samples and apply the gain.

The sample peak never exceeds the ceiling. Inter-sample peaks need an oversampled meter.

**Bulwark parameters.** Ceiling **−1.42 dBFS (0.84918)**, threshold −4 dBFS (CHOSEN). A 0 dBFS input peak comes out at −1.64 dBFS (DERIVED).

**Window length.** The Ring used 5 ms, but Bulwark's content is 31-63 Hz: half a 31 Hz period is 16 ms, and a 5 ms window lets the gain move within one cycle, which is audible distortion. Use **L = 16 ms**, an extra hold of 20 ms ("hold prevents fluttering" [L1]) and release 250 ms. Raise `Cap` from 1024 to **4096** so 16 ms fits at 192 kHz.

**Chain:** layers + steps → DC 20 Hz → FDN sends (small, hall) → dry + wet → master gain (intro or ending fade, pause duck −12 dB / 0.4 s, S-events) → limiter → `data`.
- Keep a final `|x| ≤ 0.99` clamp only as a NaN or overflow guard.
- Drop Thread's always-on `0.84·tanh(x/0.84)`. It is not a look-ahead limiter and compresses −0.95 dB already at a −6 dBFS input.

**True-peak meter (T-AUDIO, offline only).** Upsample 4× with a 48-tap, 4-phase polyphase FIR (12 taps per phase; the BS.1770 Annex 2 method [L2, L3]), then take max |x|. Gate at ≤ −1.0 dBFS; the limiter leaves 0.42 dB of margin, and low-frequency content overshoots far less. Also gate DC < 0.001 per channel and the sample step ≤ 0.25 (relay windows excepted).

---

### 6. Footsteps by modal resonators

#### 6.1 Resonator

```
y[n] = 2R cos(w) y[n-1] - R^2 y[n-2] + 0.5 (x[n] - x[n-2]),   w = 2π f / fs,   R = 10^(-3 / (T60 fs))
```

This is the constant-peak-gain resonator with zeros at z = ±1. Peak gain 2/(1−R²) for a numerator (1 − z⁻²) and 1/(1−R²) with the 0.5 factor; multiply by (1−R²)/2 for unity peak [F1]. Bandwidth B = ln(1000)/(π·T60). Thread's `FootstepRenderer.Resonate` is exactly this; reuse it.

| Mode | R at 48 kHz | Bandwidth |
|---|---|---|
| 190 Hz, T60 80 ms | 0.998203 | 27.5 Hz |
| 420 Hz, T60 50 ms | 0.997126 | 44 Hz |
| 1100 Hz, T60 30 ms | 0.995214 | 73 Hz |

#### 6.2 Recipes

All decays are T60; exciter = 3 ms Hann white burst; keep Thread's best-of-8 burst candidates so a burst never cancels itself at a mode.

| Surface (s) | Modes | Extra | CHOSEN details |
|---|---|---|---|
| Concrete (beat 1, atrium, exit hall, beat 6) | 190 / 420 / 1100 Hz, T60 80 / 50 / 30 ms | wet: 6 kHz tick | mode gains 0 / −4 / −9 dB; tick T60 15 ms, −18 dB, 5-15 ms late |
| Road (beats 2-4) | concrete recipe | wet tick while the road is wet (outside the sun-break window and in spray) | — |
| Grating (spillway slot spans z 424+20k..440+20k; tunnel) | 310 / 880 / 2100 Hz | 4 kHz ring | metal T60s 150 / 90 / 60 ms; ring T60 150 ms at −20 dB |
| Steel stair (s 936.094-1033.894) | 260 / 700 / 1800 Hz | goes "into the well reverb" (it does: in-DSP send) | T60s 180 / 110 / 70 ms |
| Ledge (rough concrete, s ≥ 1436) | concrete recipe | 1.5 kHz scrape | BP noise Q 2, 40 ms, −16 dB, 20 ms after the impact |

Variation:
- Six variants per surface: frequency ±4 % (the spec's pitch ±4 %) and decay ±10 %.
- Alternate feet ±0.8 dB with ±0.1 pan (CHOSEN).
- Peak-normalise to −30 dBFS; 1 ms attack; cosine fade to an exact zero.
- Optional heel-then-ball double impact (second hit −6 dB, 25-45 ms later). The literature models a step as heel and toe micro-impacts driven by the ground reaction force [F2], but gives no fixed interval here.

#### 6.3 Cadence

Stride `L(v) = 0.75 + 0.1·clamp(v − 1.5, 0, 1)` m (2.00 steps/s at 1.5 m/s, 2.94 at 2.5); on stairs 0.9 m (1.67 and 2.78 steps/s).
- Footstep power differs by 1.67 dB between the speeds (DERIVED). At −30 dBFS peaks this is at most +0.3 dB on beat 6 totals.
- Runtime advances by the horizontal eye displacement; offline by Δs × (ground/plan) ≈ Δs (at most 1.04).

#### 6.4 Where the steps are mixed

Inside `BwDsp`: a pre-rendered bank (Thread's `FootstepBank` pattern, new recipes) plus a 6-voice pool with linear-interpolation resampling for pitch. That is Thread's offline mixer, made allocation-free inside the DSP.

Do not use AudioSources:
- A generator source does not receive other sources (A3).
- Separate sources would bypass the room FDN and the limiter.
- The offline render would no longer match the player.

Cadence: either the main thread runs `FootstepSequencer` and pushes `StepEvent`s through the SPSC ring, or the DSP runs the sequencer from the published s and speed. Both are deterministic; jitter of at most one 21 ms block is inaudible.

---

### 7. T-AUDIO measurement notes (log as 14.10.5 arbitrations if needed)

- **Sliding 3 s RMS against booms.** In beat 1 the boom carries most of the power: B −35 of a −33.7 total. A 3 s window with a boom reads about −31; one without reads about −39.6 (S −50, T −40). The "sliding 3 s RMS ±1.5 dB" target is therefore met only as a **per-beat power mean of the sliding RMS** (= beat RMS). Report the minimum and maximum, and log the interpretation.
- **"Same per s at both speeds within 1.5 dB".** Impacts are clock-driven, so a 3 s per-s comparison of the full mix fails for statistical, not acoustic, reasons. Compare the **beds stem** (all layers except B, footsteps and V) per 3 s window keyed to s, and the **full mix per beat**. With the stratified impact schedule the per-beat difference is below 1 dB.
- **Dump parity** ("within 1 dB RMS"). The dump sidecar must log (t, s) per callback or block. The offline renderer then **replays** that trajectory with the same t clock (same impacts) and the same seed at the dump's fs. Thread only logged s0 and s1, which is not enough to align impacts.

---

### 8. What to reuse from Thread's `RT/Audio/` (and The Ring) and what must change

| Thread file | Reuse as is | Must change for Bulwark |
|---|---|---|
| `ThreadDsp.cs`: `XorShift32`, `PinkKellet`, `SvfTpt`, `DcBlocker`, `SineTable` | yes (rename prefix only; namespace `Megastructure.Audio`) | DC blocker initialised at **20 Hz** |
| `ThreadDsp.cs`: `FdnReverb8` | Householder N = 8, prime lengths re-primed per fs, two-tap pre-delay crossfade, Hadamard L/R outputs, denormal flush, control-rate double smoothing, `wetNorm` | Constructor takes an **explicit delay array**. **Per-line Jot (g_i, p_i)** for T60(DC) and T60(5 kHz) replaces the shared `dampHz` one-pole. Blend time **per transition** (0.6 / 2 / 3 s) instead of the constant 8 s. **Two instances** (small, hall) with equal-power send and wet crossfades. |
| `ThreadDsp` (score class) | Block 32, impulse-energy calibration, closed-form sine calibration with DC loss, per-sample gain smoothing, NaN reset, `FlushDenormals`, `SnapTo`, `Process(buf, ch, in p)` | All layers replaced by W, S, B, F, T, P, V, C (section 3) plus in-DSP footstep voices and event voices. Gain τ 50 ms. Generator mode **clears and writes** `data`. Master chain = DC → FDNs → master gain → **look-ahead limiter** (The Ring's `DspLimiter`, ceiling −1.42, L = 16 ms, Cap 4096); Thread's tanh 0.84 is removed. |
| `ThreadSynth.cs` | OAFR host, mailbox, `AudioCapture`, heartbeat, `TakeError`, `Rebuild` | Lives on a **clip-less AudioSource generator** (primary; no listener path, no `Bypass` fallback). Adds an SPSC event ring. `Array.Clear(data)` first. |
| `ThreadAudioParams.cs` | `PanGains`, the layer-table plus crossfade pattern (`LayerAmp`) | Replaced by **`BulwarkScore`**: 14.3 levels per beat, 30 m crossfades (Rule P) except the timed events, S1-S8, `SeaState` coupling, F distance law, P depth law, V pan, reverb room by s, latched event times, intro/ending from `UiTimeline` (first impact exempt), pause −12 dB / 0.4 s. |
| `AudioDirector.cs` | `-audio-dump` flow, watchdog, `OnAudioConfigurationChanged` rebuild, `Footstep(...)` hook interface | Remove the footstep AudioSource pool, the metal re-render and the listener fallback. Create the generator rig (The Ring's `CreateRig` + 48 kHz `Reset` + Play after AddComponent). Never on the camera. `runInBackground` true in test and dump modes. Dump sidecar logs (t, s) per block. |
| `FootstepBank.cs` | `FootstepRenderer` pattern, `Resonate`, best-of-8 burst, `NormalisePeak`, `FadeOut`, `FootstepSequencer`, `StepEvent` | New recipes (concrete or wet, grating, steel, ledge). Stride by speed, 0.9 m on stairs, ±4 % pitch, ±0.8 dB alternate feet, peak −30. Surfaces by s. Mixed inside the DSP. |
| `OfflineAudioRenderer.cs` | `IAudioBlockSink`, `RenderToWav`, sidecar, stems, `AudioMeter`, `WalkLevelMeter` (K-weighting, momentary) | `MotionS` (below); event latches; `BulwarkScore.Render(speed)` entry; meters for sliding 3 s RMS per beat, 4× true peak, beds-stem per-s comparison; beat boundaries 0/120/420/700/920/1070/1370/1500. |
| `WavWriter.cs` | as is | — |

**`BulwarkScore.Render(speed)` motion model** (both speeds; tests render both):

```
ramp Tr = AutoWalker start ramp (0.5 s): v(t) = V·min(1, t/Tr)  ->  s(t) = V t²/(2Tr) for t < Tr, else V (t - Tr/2)
trigger s* = 1498.5 at t* = s*/V + Tr/2   (599.65 s at 2.5 m/s, matching 14.10.4; 999.25 s at 1.5 m/s with the same ramp)
stop:  v = V (1 - smoothstep(0, T, τ)),  T = 3/V  -> distance V·T/2 = 1.5 m, so it stops exactly at s 1500
ending clocks from t*: dwell 3.0 s, audio fade = UiTimeline.EndPicture(te) (smootherstep 3.0 -> 7.0 s), then silence to 18.5 s
event latches: t(s=104) door/S2, t(s=920) S4, t(s=1070) S5, t(s=1222) V, t(s=1423) S7; SeaState.Impacts(t) on the same clock
```

---

### 9. UI Toolkit runtime text in Unity 6000.6

#### 9.1 Facts

| Fact | Tag |
|---|---|
| USS length units are **px and % only**: "UI Toolkit supports pixels (px) and percentages (%) as units of measurement for length"; a unit-less value is px. `LengthUnit` has only `Pixel` and `Percent`. **There is no em.** `letter-spacing: <length>`. | VERIFIED-LOCAL (`Manual/UIE-USS-PropertyTypes.html`, `UIE-USS-SupportedProperties.html`, `UnityEngine.UIElementsModule.xml:13560-13575`); VERIFIED-DOCS [UI1, UI2] |
| Therefore, at the 1080 reference: title 56 px with 0.22 em = **12.32 px** (24.64 physical px at 4K); end line 51 px with 0.22 em = **11.22 px**; hint 19.4 px (The Ring's 1.8 % rule; 14.11 is silent on the size, so log it) at 0.06 em. Set it from code: `style.letterSpacing = 0.22f * fontPx` (The Ring's `SetType`). Do not use % (its reference for letter-spacing is undocumented). | DERIVED |
| PanelSettings: Theme Style Sheet ("Apply a default TSS file to every UI Document"); Text Settings (auto-created if empty); Scale With Screen Size with Screen Match Mode Match Width or Height, Reference Resolution, and **Match: "0 matches the width; 1 matches the height"**; Sort Order; Clear settings (Screen Space Overlay only). API: `scaleMode = PanelScaleMode.ScaleWithScreenSize`, `referenceResolution` (**Vector2Int**), `screenMatchMode = MatchWidthOrHeight`, `match = 1f`, `sortingOrder` (**float**), `clearColor = false`. | VERIFIED-LOCAL `Manual/UIE-Runtime-Panel-Settings.html`; VERIFIED-DOCS [UI3]; The Ring research 06 §1.1 (reflection) |
| With no theme, the player warns "No Theme Style Sheet set to PanelSettings …, UI will not render properly", and the default `PanelTextSettings` has **no default font**. Import the built-in theme with `@import url("unity-theme://default");`; this special URL works only for the default theme. | The Ring 06 §1.2 (IL); VERIFIED-LOCAL `Manual/UIE-tss.html` |
| `PanelSettings.OnEnable` fills its UI shader fields by `Shader.Find` only when null, and the shaders live in editor resources. **Create the asset in the editor** so the references serialise (The Ring's asset has `m_DefaultShader: {fileID: 9100, …f000…}` and `m_SDFShader: {fileID: 19011, …}`), and add the UI shaders to Always Included Shaders. | VERIFIED-LOCAL `2nd…/Assets/_Game/Settings/RingPanel.asset:45,49`; The Ring 06 §1.3 |
| `UIDocument` is documented as obsolete in 6000.6 (superseded by Panel Renderer; it cannot be added from the Editor menu), but it has no `[Obsolete]` attribute, so `AddComponent<UIDocument>()` from the scene script compiles. It **clears its tree on every OnEnable/OnDisable**: never toggle it; hide with opacity, visibility or `display`. Panel Renderer is the forward path (`RegisterUIReloadCallback`). | VERIFIED-LOCAL `Manual/UIE-create-ui-document-component.html`; The Ring 06 §1.1 |
| Fonts on this install. `LegacyRuntime` exists in the Win64 player's `unity default resources` (binary string). The default runtime theme and the **Inter** font assets (`default-runtime-theme`, `Inter-Regular SDF`) were found only in the editor's `unity editor resources`. Whether the theme font ships depends on the build pulling the theme dependency, so **verify in the player**. | VERIFIED-LOCAL (string scan of both files) |
| `FontDefinition.FromFont(font)` builds a dynamic SDF font asset at runtime (it logs "Unable to load font face …" on failure). The SDF material uses `Hidden/TextCore/Distance Field SSD`, which must be in the build (Always Included). | The Ring 06 §1.3, §1.5 |
| **Evidence:** The Ring's built player (same Unity build, same PC) rendered LegacyRuntime through `FromFont` and logged `UI font in use: LegacyRuntime (mode Legacy), title 353 x 75 px; glyphs warmed`. | VERIFIED-LOCAL `2nd…/QA/autotest/at01/player.log`, `Logs/player-*-cap*.log` |
| Overlay UI is drawn after URP's final blit (no post) and is **not** in a camera RenderTexture capture. The Ring's `UiDirector.SetOverlayTarget(rt)` renders the panel into the capture RT through a runtime clone of the PanelSettings (`targetTexture`, `clearColor = false`). | The Ring 06 §1.6; `UiDirector.cs:212-251` |
| Some fonts showed wrong spacing or kerning in UI Toolkit after Unity 6 (forum, no fix given). | SEARCH [UI4] |

#### 9.2 Editor script: PanelSettings plus theme (The Ring's `ProjectSetup.CreateOrUpdateUi`, reuse)

```csharp
const string ThemePath = "Assets/_Game/Settings/BwTheme.tss", PanelPath = "Assets/_Game/Settings/BwPanel.asset";
EditorIO.WriteTextIfChanged(ThemePath, "@import url(\"unity-theme://default\");\nVisualElement {}\n");
AssetDatabase.ImportAsset(ThemePath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);            // null -> BatchLog.Error (fail loudly)
var ps = EditorIO.LoadOrCreateAsset(PanelPath, () => ScriptableObject.CreateInstance<PanelSettings>());
ps.themeStyleSheet = theme;
ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
ps.referenceResolution = new Vector2Int(1920, 1080);
ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
ps.match = 1f;                     // match height (14.11)
ps.sortingOrder = 100f; ps.clearColor = false;
EditorUtility.SetDirty(ps); AssetDatabase.SaveAssets();
// Validate: SerializedObject(ps).FindProperty("m_DefaultShader").objectReferenceValue != null (and m_SDFShader)
// Always Included: Hidden/Internal-UIRDefault, Hidden/Internal-UIRAtlasBlitCopy, Hidden/UIR/GaussianBlur, Hidden/UIR/ColorEffect,
//   Hidden/UIR/DropShadowComposite, Hidden/TextCore/Distance Field SSD, Hidden/Internal-GUITextureClipText,
//   Hidden/TextCore/Sprite, Hidden/Internal-UIE-ColorConversionBlit
```

The scene script adds `UIDocument` to `[Game]`, assigns `ps`, adds `UiDirector`, and wires `document`. Add Validate checks: theme not null, the shader references, Always Included entries, and the Scale/Match values.

#### 9.3 Font choice (14.11: the player's default font; LegacyRuntime if blank)

- Default `UiFontMode.Theme`; The Ring defaulted to `Legacy`.
- The Ring's `FontCheck` measures the title after two style frames. If it is blank, it switches once to the other mode; if both are blank, it logs an error.
- Extend the log line with `title.resolvedStyle.unityFontDefinition.fontAsset?.name` and `.font?.name`, so the player log states *which* font rendered.
- Keep `-uifont theme|legacy`. Weight: `FontStyle.Normal` (regular). Rich text off. `WhiteSpace.Pre` so the hint keeps its double and quadruple spaces.

#### 9.4 Typography and timeline values for Bulwark

| Item | The Ring | **Bulwark (14.11)** |
|---|---|---|
| Title text / size / tracking | "THE RING" 67 px, 0.12 em | **"BULWARK" 56 px, 0.22 em = 12.32 px** |
| Title centre / alpha | 40 % / 0.92 | **38 % (410.4 of 1080) / 0.88** |
| End line | 51 px, 0.12 em, alpha 0.92 | **"Someone held the sea back, so the land could stay dry." 51 px, 0.22 em = 11.22 px, alpha 0.88** |
| Hint | 1.8 % (19.4 px), 0.06 em, bottom 6 %, alpha 0.7 | same values (size not in 14.11; log it); text "WASD  move    Mouse  look    Shift  faster    C  cinematic" |
| Picture and audio fade-in | black 0-0.6, smoothstep 0.6 → 3.1 | **black 0-1.0, smoothstep 1.0 → 3.5**; **first impact audible at 0.5 s in the black** |
| Title envelope | 0.8 → 2.0, hold to 4.0, out to 5.4 | **1.4 → 2.6, hold to 4.6, out to 6.0** |
| Hint | at 6.0 for 6 s, 0.6 s fades, kill 0.35 s | **at 6.5 for 6 s**, 0.6 s fades, gone within 0.4 s of first movement; never with `-cinematic`, `-autotest`, capture or `-nofade` |
| Ending | 3.0 dwell, 4.0 smootherstep, 1.5 black, 2 + 6 + 2 | identical (18.5 s); lantern flicker until the fade ends |
| Pause | menu dim; audio by AudioDirector | same UI; **audio duck −12 dB over 0.4 s** |

#### 9.5 What to reuse from The Ring's `UiDirector` / `UiTimeline`

Reuse as is:
- The architecture: one component, a tree built in `OnEnable` so frame 0 is black, cached elements, style writes only on change, opacity plus visibility.
- `FontCheck` and glyph warm-up.
- `ApplyLayout` (sizes from panel height, so k = 1 with match height).
- `SetOverlayTarget` for captures, `ForceIntroTime`, `SkipIntro` / `PlayIntro`, `Refresh`, `Restart`.
- Menu input (mouse, keys, gamepad, own hit test); the cursor rules.
- Hint gating (cinematic, autotest, capture, nofade, first movement with a 0.3 m/s fallback).
- The `-uifont` flag.
- `UiTimeline` as pure functions, testable in EditMode.

Must change:
- **Constants** (section 9.4); `FontMode` default Theme.
- Event and state adapters: `RingEvents`, `RingState`, `RingRig`, `RingAssets`, `RingPalette`, `RingConstants`, `RingMath` map to Bulwark's framework (`GameBootstrap`, `ProgressDirector`, `CliArgs`, `BulwarkConst`, `BulwarkPalette` with the `TitleText` hex used directly as an sRGB UI colour).
- **`ShaderFade = true`**, with `PictureMul` (= picture^2.2 × pause dim) written into BwGrade's fade multiplier. Thread's grade already applies the fade in float before its single blue-noise dither (`ThreadGrade.shader` step 5) and keeps pure black exact. The black overlay then only covers the frame before LoadComplete, so fades never re-quantise a dithered frame (H5, T9).
- Audio fade: the score takes `Picture(t)` and `EndPicture(te)` from `UiTimeline` (same functions offline) instead of The Ring's reflection binding to `FadeAudio`. The B layer is exempt before t = 1.0 s.
- Do **not** import Thread's `FadeController`, `GlyphText` or `PauseOverlay`: two fade owners and two pause UIs.

#### 9.6 Verifying the text in the built player (required by Part 6.5)

1. **Log.** One line like `[Bw] UI font in use: <asset or font name> (mode X), title W x H px; glyphs warmed`, with no warning about the theme or font face. Expected values with LegacyRuntime (Arial metrics, DERIVED):
   - Line box 1.117 × 56 = 62.6 px; cap height 1467/2048 × 56 = 40.1 px.
   - Advance sum of "BULWARK" = 276.9 px. Plus 6 gaps × 12.32 = **350.9 px** (7 gaps: 363.2) if tracking applies in full; about 276.9 if it does not.
   - Inter is wider; accept its own values and log them.
2. **Tracking anomaly to resolve.** The Ring's log reports 353 px for "THE RING" at 67 px. The untracked advance sum is 320.1 px, and full 8.04 px tracking predicts 376.4 px. That is only about 4.1-4.7 px per gap, roughly 55 % of the request (`MeasureTextSize` versus applied spacing, or a TextCore spacing conversion; cause unknown). Measure the **ink** in a capture:
   - If the effective tracking is short, scale the px value until the ink width equals the untracked ink plus 6 × 12.32 px.
   - Log the factor as a decision.
   - Expected ink width at 1080p is about 347 px with full tracking (693 px at 4K); kerning (W-A) adds about ±3 px.
3. **Capture.** KS00-style title shot at intro t = 4.0 s (title at full alpha, picture fully in), through `SetOverlayTarget(rt)`, at 1920×1080, 3840×2160 (UI scale 2.0), 1680×1050 (scale 0.972) and 2560×1080 (scale 1.0). Checks:
   - Ink bounding box centred horizontally within 2 px, with its vertical centre at 38 % of the height within 4 px.
   - Ink height equals the cap height within 2 px.
   - Peak glyph-stem colour is about sRGB (224, 222, 216) if the panel blends in linear light, or (209, 207, 202) if it blends in gamma. Accept either, log which, and keep that in the test.
   - No magenta or blank.
   - The end line likewise at its own time.

---

### 10. Sources (all accessed 2026-11-13)

**Unity / engine**
- [U1] https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/monobehaviour/onaudiofilterread: generator rule, interleaving, audio thread, ~20 ms, no deprecation note. Local 6000.6 copy: `C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Data\Documentation\en\ScriptReference\MonoBehaviour.OnAudioFilterRead.html`.
- [U2] https://issuetracker.unity3d.com/issues/onaudiofilterread-does-not-affect-signal-on-audiolistener-after-enable: UUM-116871, fixed in 6000.6.0a5 (SEARCH).
- [U3] https://issuetracker.unity3d.com/issues/monobehaviour-onaudioreadfilter-processing-does-not-affect-audio-when-audiosource-playonawake-is-checked: a clip-less source's first OAFR is the generator and takes no other input; by design (SEARCH; the page itself 404s after migration).
- [U4] https://issuetracker.unity3d.com/issues/audio-stuttering-occurs-when-heavy-processing-is-performed-while-onaudiofilterread-is-in-use: OAFR attached to the scripting thread, GC interrupts, Won't Fix (SEARCH).
- [U5] https://issuetracker.unity3d.com/issues/audio-bypass-effects-in-the-audio-source-does-not-bypass-effects-caused-by-onaudiofilterread: title only (SEARCH).
- [UI1] https://docs.unity.com/en-us/engine/6000.6/manual/uitoolkits/uielements/uie-uss/properties/property-types: px and % only.
- [UI2] https://docs.unity.com/en-us/engine/6000.6/manual/uitoolkits/uielements/uie-uss/properties/reference: `letter-spacing: <length>` (SEARCH; local `UIE-USS-SupportedProperties.html` read).
- [UI3] https://docs.unity.com/en-us/engine/6000.6/manual/uitoolkits/uielements/uie-support-for-runtime-ui/uie-render-runtime-ui/uie-runtime-panel-settings: Panel Settings properties, Match semantics.
- [UI4] https://discussions.unity.com/t/fonts-have-wrong-spacing-kerning-in-uitoolkit-after-switch-to-unity-6/1544347: spacing issues after Unity 6 (SEARCH).
- Local, VERIFIED-LOCAL:
  - `…\Documentation\en\Manual\UIE-tss.html` (default theme `@import`).
  - `UIE-create-ui-document-component.html` (UIDocument obsolete, lifecycle).
  - `…\Managed\UnityEngine\UnityEngine.UIElementsModule.xml` (`LengthUnit`).
  - `…\PlaybackEngines\windowsstandalonesupport\Variations\win64_player_nondevelopment_mono\Data\Resources\unity default resources` (LegacyRuntime).
  - `…\Data\Resources\unity editor resources` (default-runtime-theme, Inter SDF).
  - Bulwark `ProjectSettings\AudioManager.asset`, `ProjectSettings.asset`.

**DSP**
- [D1] https://www.firstpr.com.au/dsp/pink-noise/: Kellet refined and economy coefficients, ±0.05 dB above 9.2 Hz, unity gain at Nyquist.
- [D2] https://en.wikipedia.org/wiki/Brownian_noise: integration and leaky integrator (Ornstein-Uhlenbeck), 1/f².
- [D3] https://cytomic.com/files/dsp/SvfLinearTrapOptimised2.pdf (the PDF could not be parsed here) plus the port https://gist.github.com/hollance/2891d89c57adc71d9560bcf0e1e55c4b: g, k, a1-a3, tick equations, BZT-equivalent response (SEARCH); equations cross-checked against Thread's measured `SvfTpt`.
- [R1] https://ccrma.stanford.edu/~jos/pasp/Mode_Density_Requirement.html: ΣM ≥ 0.15·T60·fs (Schroeder and Logan).
- [R2] https://ccrma.stanford.edu/~jos/pasp/Mean_Free_Path.html: 4V/S; mean delay ≈ mean free path, "a lower bound".
- [R3] https://ccrma.stanford.edu/~jos/pasp/Choice_Delay_Lengths.html: mutually prime lengths, sufficient mode density.
- [R4] https://learn.microsoft.com/windows/win32/api/xaudio2fx/ns-xaudio2fx-xaudio2fx_reverb_i3dl2_parameters: DecayHFRatio definition and range, HFReference, Reflections, ReverbDelay.
- [R5] https://www.un4seen.com/doc/bass/BASS_DX8_I3DL2REVERB.html: HF reference default 5000 Hz (SEARCH).
- [R6] https://ccrma.stanford.edu/~jos/pasp/First_Order_Delay_Filter_Design.html: p_i, g_i from R0 and Rπ.
- [R7] https://uspto.report/patent/grant/5491754 (Jot and Chaigne) and https://www.arxiv.org/pdf/2402.11216v2: FDN with absorption filters H_i(z) and a tonal-correction filter T(z) that equalises modal energy (SEARCH).
- Also https://ccrma.stanford.edu/~jos/pasp/Achieving_Desired_Reverberation_Times.html (G = 0.001^(1/N60)).
- [L1] https://signalsmith-audio.co.uk/writing/2022/limiter/: look-ahead limiter (moving minimum, box smoothing, hold, release clamp, no sample overs; inter-sample peaks need oversampling).
- [L2] https://essentia.upf.edu/tutorial_audioproblems_truepeakdetector.html and [L3] https://docs.rs/crate/math-dsp/0.5.20/source/src/true_peak.rs: BS.1770 Annex 2 true peak, 4×, 48-tap 4-phase FIR (SEARCH). Authoritative text: ITU-R BS.1770-4.
- [F1] https://ccrma.stanford.edu/~jos/filters/Constant_Peak_Gain_Resonator.html: (1 − z⁻²)/(1 − 2R cos θ z⁻¹ + R² z⁻²), peak gain 2/(1 − R²).
- [F2] https://vbn.aau.dk/files/37619408/Serafin_Turchet_Nordahl_DAFx10_P82.pdf, https://vbn.aau.dk/ws/files/18424996/serafin_turchet_nordahl.pdf: footsteps as micro-impacts, the GRF as exciter, heel and toe events, modal synthesis (SEARCH). Also https://soundlab.cs.princeton.edu/publications/billsgait.pdf (Cook, "Modeling Bill's Gait"; the PDF could not be parsed here).

**Water and acoustics physics**
- [P1] https://web.mit.edu/parmstr/Public/NRCan/CanBldgDigests/cbd239_e.html: mass law TL = 20 log10(m·f) − 48, +6 dB per doubling (SEARCH).
- [P2] https://data-ww3.ifremer.fr/BIB/Garces_etal2003.pdf: surf infrasound 1-5 Hz, amplitude proportional to wave height (SEARCH).
- [P3] https://data-ww3.ifremer.fr/BIB/Park_etal_JASA2008.pdf: plunging breakers give low-frequency energy below 20 Hz (SEARCH).
- [P4] JCAA paper on airborne breaking-wave sound, about 75 Hz pocket resonance for a 1 m wave: https://jcaa.caa-aca.ca/index.php/jcaa/article/download/2779/2502/3433 (SEARCH; the download timed out; verify which JCAA paper holds the figure).
- [P5] https://www.aes.org/e-lib/browse.cfm?elib=21678: Tez, Selfridge, Reiss 2022, real-time procedural ocean waves from filtered noise (SEARCH).
- [P6] https://forum.audacityteam.org/t/synthetic-ocean-wave-sound/58776 and https://www.oreilly.com/library/view/programming-sound-with/9781941222492/f_0034.html: LFO-modulated low-pass noise, Farnell-style band-pass and low-pass waves (SEARCH).
- [P7] https://researchportal.hw.ac.uk/en/publications/acoustical-and-perceptual-assessment-of-water-sounds-and-their-us/: Galbrun and Ali 2013, JASA 133(1) 227-237; large temporal variation and low sharpness preferred (SEARCH).

**Earlier games (read-only, VERIFIED-LOCAL)**
- Thread: `<WORKSPACE>\3rd Megastructures\3rd Megastructures\Assets\_Game\Scripts\Runtime\Audio\` (`ThreadDsp.cs`, `ThreadSynth.cs`, `AudioDirector.cs`, `ThreadAudioParams.cs`, `FootstepBank.cs`, `OfflineAudioRenderer.cs`, `WavWriter.cs`), `docs\research\R5_audio.md`, `Assets\_Game\Shaders\ThreadGrade.shader`, `Scripts\Runtime\UI\FadeController.cs`.
- The Ring: `<WORKSPACE>\2nd Megastructures\2nd Megastructures\Assets\_Game\Scripts\Runtime\UI\UiDirector.cs`, `UiTimeline.cs`, `Runtime\Audio\AudioDsp.cs` (`DspLimiter`), `Runtime\Audio\AudioDirector.cs`, `Editor\Batch\ProjectSetup.cs`, `Editor\Batch\SceneFactory.cs`, `Assets\_Game\Settings\RingPanel.asset`, `docs\research\05_procedural_audio.md`, `docs\research\06_ui_toolkit_input_build.md`, `QA\autotest\at01\player.log`.

---

<a id="source-index"></a>

## Source index

Every URL cited by the three reports, once. "Accessed" is the research date plus the fetch status the report gave (no note: listed as accessed without a caveat). "Used for" names the report section (A§, B§, C§ and C's bracket labels). The two tables after the URL index list the references cited without a URL and the local files that were read.

### URLs

| URL | Accessed | Used for |
|---|---|---|
| https://jtessen.people.clemson.edu/reports/papers_files/coursenotes2004.pdf | 2026-11-13 | Tessendorf, *Simulating Ocean Water*: Phillips spectrum and suppression, choppy displacement, Jacobian (A§1, A§9) |
| https://hal.archives-ouvertes.fr/inria-00443630v3 | 2026-11-13 (HAL PDF blocked by bot protection) | Bruneton, Neyret, Holzschuch 2010: geometry → normal → BRDF transition, variance from footprint (A§3, A§4) |
| https://github.com/unitycoder/Brunetons-Ocean | 2026-11-13 | Bruneton ocean shader port (`Ocean.shader`): mean Fresnel, Smith Λ, erfc, sun radiance, back-face fix (A§4) |
| https://cdn.shopify.com/s/files/1/0134/1131/9867/files/EmpiricalDirectionalWaveSpectra_DigiPro2015_Jun20_2015_Optimized.pdf | 2026-11-13 | Horvath 2015: JONSWAP eq. 28, directional spreading eq. 33–38 (A§1) |
| https://developer.nvidia.com/gpugems/gpugems/part-i-natural-effects/chapter-1-effective-water-simulation-physical-models | 2026-11-13 | GPU Gems 1 ch. 1: Gerstner steepness normalisation, no loops (A§1.4) |
| https://developer.nvidia.com/gpugems/gpugems2/part-i-geometric-complexity/chapter-2-terrain-rendering-using-gpu-based-geometry | 2026-11-13 (not re-fetched) | Geometry clipmaps: transition region and geomorph (A§2.2) |
| https://crest.readthedocs.io/en/4.17/user/system-notes.html | 2026-11-13 | Crest: waves rendered into the LOD matching their wavelength (A§9) |
| http://advances.realtimerendering.com/s2017/ | 2026-11-13 | SIGGRAPH 2017 Advances (Crest): LOD blending (A§9) |
| https://userpages.cs.umbc.edu/olano/papers/lean/ | 2026-11-13 | LEAN mapping: slope moments → anisotropic covariance (A§3.2) |
| https://perso.liris.cnrs.fr/victor.ostromoukhov/publications/pdf/SAsia2013-LEADR.pdf | 2026-11-13 | LEADR: displacement-aware moments with masking (A§9) |
| https://developer.download.nvidia.com/whitepapers/2006/Mipmapping_Normal_Maps.pdf | 2026-11-13 (not re-fetched) | Toksvig: variance from normal shortening, other materials (A§9) |
| https://www.jcgt.org/published/0010/02/02/ | 2026-11-13 | Tokuyoshi & Kaplanyan 2021: projected-space geometric specular AA (A§3.3) |
| https://developer.download.nvidia.com/gameworks/events/GDC2016/akaplanyan_specular_aa.pdf | 2026-11-13 | Kaplanyan GDC 2016: clamp the filter width (A§9) |
| https://oceanopticsbook.info/view/surfaces/cox-munk-sea-surface-slope-statistics | 2026-11-13 | Cox–Munk mss and up/cross-wind split (A§1.3, A§3.3) |
| https://pmc.ncbi.nlm.nih.gov/articles/PMC6209970 | 2026-11-13 | Monahan whitecap fraction W = 3.84e-6 U^3.41 (A§4.6) |
| https://breakwater.readthedocs.io/en/latest/overtopping.html | 2026-11-13 | EurOtop 2018 overtopping formulas, γ_f (A§6.5) |
| http://www.overtopping-manual.com/ | 2026-11-13 | EurOtop manual (A§6.4, A§6.5) |
| https://data-ww3.ifremer.fr/BIB/Zanuttigh_Andersen_CE2010.pdf | 2026-11-13 | Reflection coefficient Kr = tanh(a ξ0^b) (A§5.3) |
| https://en.wikipedia.org/wiki/Clapotis | 2026-11-13 | Standing wave at a wall, clapotis gaufré (A§5.2) |
| https://en.wikipedia.org/wiki/Tainter_gate | 2026-11-13 | Radial gate principle, patent, Folsom 1995 (A§6.1) |
| https://www.iahrmedialibrary.net/the-library/geophysical-hydraulics/fluvial-hydraulics/the-free-overfall/1156 | 2026-11-13 | Rouse end-depth ratio 0.715 y_c (A§6.2) |
| https://www.usbr.gov/tsc/techreferences/computer%20software/software/wingate/Clemmens%20et%20al%202003.pdf | 2026-11-13 | Radial gate discharge calibration (A§9) |
| https://en.wikipedia.org/wiki/Oosterscheldekering | 2026-11-13 | Delta Works context numbers (A§6.3) |
| https://en.wikipedia.org/wiki/Delta_Works | 2026-11-13 | Delta Works context numbers (A§6.3) |
| https://en.wikipedia.org/wiki/Tetrapod_(structure) | 2026-11-13 | Tetrapod origin, Accropode, Xbloc, 80 t monument (A§6.4) |
| https://kennisbank-waterbouw.nl/breakwaters/units/Tetrapod.html | 2026-11-13 | TU Delft unit catalogue: K_D, k_Δ, k_s, placement (A§6.4) |
| https://kennisbank-waterbouw.nl/breakwaters/units/Accropode.html | 2026-11-13 | TU Delft unit catalogue, Accropode (written as `.../Accropode.html` in A§9) (A§6.4) |
| https://kennisbank-waterbouw.tudelft.nl/breakwaters/units/Accropode%20II.html | 2026-11-13 | TU Delft unit catalogue, Accropode II (A§6.4) |
| https://kennisbank-waterbouw.nl/breakwaters/units/Xbloc.html | 2026-11-13 | TU Delft unit catalogue, Xbloc (A§6.4) |
| https://www.conteches.com/media/b2cj2q4w/xbloc-design-manual.pdf | 2026-11-13 (search snippet only) | Xbloc staggered grid, random placement (A§6.4) |
| https://data-ww3.ifremer.fr/BIB/Peregrine_ARFM2003.pdf | 2026-11-13 (HTTP 403; abstracts only) | Flip-through vertical jets (A§6.5) |
| https://cs.dartmouth.edu/wjarosz/publications/sadeghi11physically.html | 2026-11-13 (fetched) | Sadeghi et al. 2012, physically based rainbows (B§3.4) |
| https://graphics.unizar.es/papers/sadeghi11physically.pdf | 2026-11-13 (fetched) | Sadeghi et al. 2012, paper PDF (B§3.4) |
| https://www.mdpi.com/2073-4441/13/11/1593 | 2026-11-13 (via search) | Carrillo et al. 2021, Horeni breakup length L_b = 6 q^0.32 (B§3.1) |
| https://www.oceanopticsbook.info/view/inherent-and-apparent-optical-properties/classification-schemes | 2026-11-13 (fetched) | Jerlov water types, chlorophyll mapping (B§4) |
| https://en.wikipedia.org/wiki/Okinawa_Churaumi_Aquarium | 2026-11-13 (fetched) | Thick acrylic window dimensions (B§4) |
| https://en.wikipedia.org/wiki/Dubai_Aquarium_%26_Underwater_Zoo | 2026-11-13 (fetched) | Largest viewing panel dimensions (B§4) |
| https://sebh.github.io/publications/egsr2020.pdf | 2026-11-13 (too large to fetch) | Hillaire 2020 sky and atmosphere (B§5) |
| https://github.com/sebh/UnrealEngineSkyAtmosphere | 2026-11-13 (fetched) | Hillaire reference code, MIT licence (B§5) |
| https://raw.githubusercontent.com/sebh/UnrealEngineSkyAtmosphere/master/Resources/RenderSkyCommon.hlsl | 2026-11-13 (fetched) | LUT sizes, sky-view mapping at altitude (B§5) |
| https://iquilezles.org/articles/filterableprocedurals | 2026-11-13 (via search) | Band-limited and box-filtered procedurals (B§6) |
| https://iquilezles.org/articles/checkerfiltering/ | 2026-11-13 (via search) | Box-filtered checker and grid (B§6) |
| https://iquilezles.org/articles/morecheckerfiltering | 2026-11-13 (via search) | Triangle-kernel checker filtering for gratings (B§6) |
| https://iquilezles.org/articles/bandlimiting/ | 2026-11-13 (404 when fetched; listed by search) | Band-limited cosine (B§6) |
| http://loopit.dk/banding_in_games.pdf | 2026-11-13 (fetched; binary) | Gjøl, "Banding in games": TPDF blue noise after tonemapping (B§6) |
| http://momentsingraphics.de/BlueNoise.html | 2026-11-13 (not re-fetched) | Peters, free blue-noise textures (B§6) |
| https://trac.ffmpeg.org/wiki/Encode/H.264 | 2026-11-13 (not re-fetched) | x264 encoding reference (B§6) |
| https://support.google.com/youtube/answer/1722171 | 2026-11-13 (not re-fetched) | YouTube recommended upload settings (B§6) |
| https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-resolvesubresource | 2026-11-13 (fetched) | D3D11 cannot resolve depth formats → MSAA depth is bind-MS (B§7.1) |
| https://docs.unity.com/en-us/engine/current/script-reference/unityengine/systeminfo/supportsmultisampleresolvedepth | 2026-11-13 (fetched) | `SystemInfo.supportsMultisampleResolveDepth` (B§7.1) |
| https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/monobehaviour/onaudiofilterread | 2026-11-13 (opened) | [U1] OAFR generator rule, interleaving, audio thread (C§1) |
| https://issuetracker.unity3d.com/issues/onaudiofilterread-does-not-affect-signal-on-audiolistener-after-enable | 2026-11-13 (search only) | [U2] UUM-116871 (C§1) |
| https://issuetracker.unity3d.com/issues/monobehaviour-onaudioreadfilter-processing-does-not-affect-audio-when-audiosource-playonawake-is-checked | 2026-11-13 (search only; page 404s) | [U3] generator takes no other input (C§1 A3) |
| https://issuetracker.unity3d.com/issues/audio-stuttering-occurs-when-heavy-processing-is-performed-while-onaudiofilterread-is-in-use | 2026-11-13 (search only) | [U4] OAFR and GC interrupts (C§1 A7) |
| https://issuetracker.unity3d.com/issues/audio-bypass-effects-in-the-audio-source-does-not-bypass-effects-caused-by-onaudiofilterread | 2026-11-13 (search only; title) | [U5] Bypass Effects (C§1 A9) |
| https://docs.unity.com/en-us/engine/6000.6/manual/uitoolkits/uielements/uie-uss/properties/property-types | 2026-11-13 (opened) | [UI1] USS lengths px and % only (C§9.1) |
| https://docs.unity.com/en-us/engine/6000.6/manual/uitoolkits/uielements/uie-uss/properties/reference | 2026-11-13 (search only) | [UI2] `letter-spacing: <length>` (C§9.1) |
| https://docs.unity.com/en-us/engine/6000.6/manual/uitoolkits/uielements/uie-support-for-runtime-ui/uie-render-runtime-ui/uie-runtime-panel-settings | 2026-11-13 (opened) | [UI3] Panel Settings, Match semantics (C§9.1) |
| https://discussions.unity.com/t/fonts-have-wrong-spacing-kerning-in-uitoolkit-after-switch-to-unity-6/1544347 | 2026-11-13 (search only) | [UI4] spacing issues after Unity 6 (C§9.1) |
| https://www.firstpr.com.au/dsp/pink-noise/ | 2026-11-13 | [D1] Kellet pink-noise coefficients (C§2) |
| https://en.wikipedia.org/wiki/Brownian_noise | 2026-11-13 | [D2] brown noise as a leaky integrator (C§2) |
| https://cytomic.com/files/dsp/SvfLinearTrapOptimised2.pdf | 2026-11-13 (PDF could not be parsed) | [D3] Simper/Cytomic SVF (C§2) |
| https://gist.github.com/hollance/2891d89c57adc71d9560bcf0e1e55c4b | 2026-11-13 (search only) | [D3] SVF port: g, k, a1–a3, tick equations (C§2) |
| https://ccrma.stanford.edu/~jos/pasp/Mode_Density_Requirement.html | 2026-11-13 | [R1] ΣM ≥ 0.15·T60·fs (C§4.1) |
| https://ccrma.stanford.edu/~jos/pasp/Mean_Free_Path.html | 2026-11-13 | [R2] mean free path 4V/S (C§4.1) |
| https://ccrma.stanford.edu/~jos/pasp/Choice_Delay_Lengths.html | 2026-11-13 | [R3] mutually prime delay lengths (C§4.1) |
| https://learn.microsoft.com/windows/win32/api/xaudio2fx/ns-xaudio2fx-xaudio2fx_reverb_i3dl2_parameters | 2026-11-13 | [R4] DecayHFRatio and HFReference definitions (C§4.1) |
| https://www.un4seen.com/doc/bass/BASS_DX8_I3DL2REVERB.html | 2026-11-13 (search only) | [R5] HF reference default 5000 Hz (C§4.1) |
| https://ccrma.stanford.edu/~jos/pasp/First_Order_Delay_Filter_Design.html | 2026-11-13 | [R6] per-line absorption filter design (C§4.1) |
| https://uspto.report/patent/grant/5491754 | 2026-11-13 (search only) | [R7] Jot and Chaigne FDN patent (C§4.1) |
| https://www.arxiv.org/pdf/2402.11216v2 | 2026-11-13 (search only) | [R7] FDN absorption and tonal correction (C§4.1) |
| https://ccrma.stanford.edu/~jos/pasp/Achieving_Desired_Reverberation_Times.html | 2026-11-13 | G = 0.001^(1/N60) (C§10) |
| https://signalsmith-audio.co.uk/writing/2022/limiter/ | 2026-11-13 | [L1] look-ahead limiter design (C§5) |
| https://essentia.upf.edu/tutorial_audioproblems_truepeakdetector.html | 2026-11-13 (search only) | [L2] BS.1770 Annex 2 true peak (C§5) |
| https://docs.rs/crate/math-dsp/0.5.20/source/src/true_peak.rs | 2026-11-13 (search only) | [L3] 4× 48-tap polyphase true-peak FIR (C§5) |
| https://ccrma.stanford.edu/~jos/filters/Constant_Peak_Gain_Resonator.html | 2026-11-13 | [F1] constant-peak-gain resonator (C§6.1) |
| https://vbn.aau.dk/files/37619408/Serafin_Turchet_Nordahl_DAFx10_P82.pdf | 2026-11-13 (search only) | [F2] footsteps as micro-impacts (C§6.2) |
| https://vbn.aau.dk/ws/files/18424996/serafin_turchet_nordahl.pdf | 2026-11-13 (search only) | [F2] footstep modal synthesis (C§6.2) |
| https://soundlab.cs.princeton.edu/publications/billsgait.pdf | 2026-11-13 (PDF could not be parsed) | Cook, "Modeling Bill's Gait" (C§10) |
| https://web.mit.edu/parmstr/Public/NRCan/CanBldgDigests/cbd239_e.html | 2026-11-13 (search only) | [P1] mass law (C§3) |
| https://data-ww3.ifremer.fr/BIB/Garces_etal2003.pdf | 2026-11-13 (search only) | [P2] surf infrasound (C§3) |
| https://data-ww3.ifremer.fr/BIB/Park_etal_JASA2008.pdf | 2026-11-13 (search only) | [P3] plunging breakers below 20 Hz (C§3) |
| https://jcaa.caa-aca.ca/index.php/jcaa/article/download/2779/2502/3433 | 2026-11-13 (search only; download timed out) | [P4] ≈ 75 Hz air-pocket resonance (C§3) |
| https://www.aes.org/e-lib/browse.cfm?elib=21678 | 2026-11-13 (search only) | [P5] procedural ocean waves from filtered noise (C§3) |
| https://forum.audacityteam.org/t/synthetic-ocean-wave-sound/58776 | 2026-11-13 (search only) | [P6] LFO-modulated low-pass noise (C§3) |
| https://www.oreilly.com/library/view/programming-sound-with/9781941222492/f_0034.html | 2026-11-13 (search only) | [P6] band-pass and low-pass waves (C§3) |
| https://researchportal.hw.ac.uk/en/publications/acoustical-and-perceptual-assessment-of-water-sounds-and-their-us/ | 2026-11-13 (search only) | [P7] Galbrun and Ali 2013, preferred water sounds (C§3) |

### Cited without a URL

| Reference | Status | Used for |
|---|---|---|
| Phillips spectrum; Pierson–Moskowitz; Donelan–Banner spreading (via Horvath) | standard | Spectrum shape and spreading (A§1) |
| Hudson's formula; EurOtop Table 6.2 | standard | Armour unit mass and γ_f (A§6.4) |
| Thomas Parker, US patent 226,455 (1880) | as reported | Tainter gate history (A§6.1) |
| d'Angremond 1996 | from memory, verify | Wave transmission over the belt (A§6.5) |
| Maeslantkering arm length | not verified | Context only (A§6.3) |
| Bruneton et al. 2010, DOI 10.1111/j.1467-8659.2009.01618.x | HAL PDF blocked | Ocean BRDF transition (A§9) |
| Sadeghi et al. 2012, ACM TOG 31(1), doi 10.1145/2077341.2077344 | [F] | Rainbow simulation (B§3.4) |
| Solonenko & Mobley 2015, Appl. Opt. 54(17):5392, doi 10.1364/AO.54.005392 | [F, via search] | Jerlov IOP set (B§4) |
| Horeni 1956 (via Carrillo et al. 2021) | [F, via search] | Jet breakup length (B§3.1) |
| Benjamin 1968, JFM 31:209; Huppert & Simpson 1980, JFM 99:785; Simpson 1997 | [S] | Gravity-current front speed (B§3.3) |
| Jendersie & d'Eon 2023, SIGGRAPH Talks | [S] | HG+Draine fit for fog droplets (B§3.5) |
| Mobley 1994, *Light and Water*; Tyler 1960; Pope & Fry 1997, Appl. Opt. 36:8710; Morel 1974; Smith & Baker 1981, Appl. Opt. 20:177; Petzold 1972; Zaneveld & Pegau 2003, Opt. Express 11:2997; Morel & Prieur 1977; Alldredge & Silver 1988, Prog. Oceanogr. 20:41; Jerlov 1976 | [S] | Underwater absorption, attenuation, visibility, radiance, marine snow (B§4) |
| Guinness 2008 (Dubai panel thickness) | [S] | Window thickness (B§4) |
| Haines, Günther & Akenine-Möller 2019, *Ray Tracing Gems* ch. 7 | [S] | Float32 ray-sphere precision (B§5) |
| Lipshitz, Wannamaker & Vanderkooy 1992, JAES 40:355; Ulichney 1993, SPIE 1913 | [S] | TPDF dither theory; void-and-cluster (B§6) |
| Gjøl & Svendsen, GDC 2016, "Low Complexity, High Fidelity: The Rendering of INSIDE" | [S] | Dither in output space (B§6) |
| Persson 2013, Phone-Wire AA, GPU Pro 4 | [S] | `BwLine` width clamp and alpha (B§6) |
| ITU-R BS.1770-4 | authoritative text, not fetched | True-peak method (C§5, C§10) |
| Schroeder and Logan (via JOS) | standard | Mode-density rule (C§4.1) |

### Local sources read

| Path | Read | Used for |
|---|---|---|
| `B:Library\PackageCache\com.unity.render-pipelines.core@2d66c71e606e\ShaderLibrary\CommonMaterial.hlsl` (lines 192–215) | 2026-11-13 | `GeometricNormalVariance`, `NormalFiltering` (A§3.3) |
| `B:Library\PackageCache\com.unity.render-pipelines.universal@8457e85b8184\` — `Runtime\Passes\CopyDepthPass.cs`, `Shaders\Utils\CopyDepthPass.hlsl`, `Runtime\UniversalRendererRenderGraph.cs`, `Runtime\RenderingUtils.cs`, `Runtime\FrameData\UniversalResourceData.cs`, `Runtime\Passes\ScriptableRenderPass.cs`, `Runtime\UniversalRenderer.cs`, `Runtime\RendererFeatures\FullScreenPassRendererFeature.cs`, `Runtime\UniversalRenderPipeline.cs`, `Runtime\UniversalRendererData.cs`, `Runtime\UniversalRenderPipelineCore.cs`, `Runtime\Data\UniversalRenderPipelineAsset.cs` | 2026-11-13 | MSAA depth binding, CopyDepth behaviour, queues, depth format, HDR precision (B§7, line numbers there) |
| `B:Library\PackageCache\com.unity.render-pipelines.core@2d66c71e606e\` — `ShaderLibrary\API\D3D11.hlsl`, `ShaderLibrary\Common.hlsl`, `Runtime\RenderGraph\RenderGraphUtilsBlit.cs`, `RenderGraph.cs`, `Compiler\NativePassCompiler.cs`, `RenderGraphResourceRegistry.cs`, `RenderGraphBuilders.cs`, `RenderGraph.ExceptionMessages.cs`, `Runtime\Utilities\CoreUtils.cs` | 2026-11-13 | Render Graph rules, reversed Z, default depth format (B§7) |
| `C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Data\Resources\CGIncludes\UnityIndirect.cginc` (lines 45–86) | 2026-11-13 | Indirect draw args in URP (B§7.7) |
| `C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Data\Documentation\en\` — `ScriptReference\MonoBehaviour.OnAudioFilterRead.html`, `Manual\UIE-USS-PropertyTypes.html`, `UIE-USS-SupportedProperties.html`, `UIE-Runtime-Panel-Settings.html`, `UIE-tss.html`, `UIE-create-ui-document-component.html` | 2026-11-13 | OAFR rules, USS units, Panel Settings, theme import, UIDocument lifecycle (C§1, C§9) |
| Unity 6000.6.0f1 `Managed\UnityEngine\UnityEngine.UIElementsModule.xml` (lines 13560–13575); win64 player `unity default resources`; editor `unity editor resources` | 2026-11-13 | `LengthUnit`; LegacyRuntime and Inter font presence (C§9.1) |
| `B:Assets\Settings\PC_RPAsset.asset`, `B:Assets\Settings\PC_Renderer.asset` | 2026-11-13 | Current pipeline asset state (B§7.9) |
| `B:ProjectSettings\AudioManager.asset`, `B:ProjectSettings\ProjectSettings.asset` | 2026-11-13 | Sample rate 0, DSP buffer 1024, `runInBackground` 0 (C§1 A11) |
| `<WORKSPACE>\2nd Megastructures\2nd Megastructures\Assets\_Game\Scripts\Runtime\Rendering\InstancedField.cs` (lines 99–114, 204–207); `...\Scripts\Editor\Batch\UrpSetup.cs` (lines 60–61) | 2026-11-13 | Indirect draw pattern; explicit depth format (B§7.7, B§7.8) |
| `<WORKSPACE>\3rd Megastructures\3rd Megastructures\...\Scripts\Editor\PipelineConfigurator.cs` (line 70) | 2026-11-13 | 64-bit HDR precedent (B§7.9) |
| Thread audio, grade and UI sources, `docs\research\R5_audio.md`, `DEVELOPMENT.md` D-033 | 2026-11-13 | DSP blocks, measurements, allocation note (C§1, C§2, C§8) |
| The Ring `UiDirector.cs`, `UiTimeline.cs`, `AudioDsp.cs`, `AudioDirector.cs`, `ProjectSetup.cs`, `SceneFactory.cs`, `RingPanel.asset` (lines 45, 49), `docs\research\05_procedural_audio.md`, `06_ui_toolkit_input_build.md`, `QA\autotest\at01\player.log`, `Logs\player-*-cap*.log` | 2026-11-13 | Limiter, generator rig, UI Toolkit setup, font and tracking evidence (C§1, C§5, C§9) |
