# R2: Rain, height fog and haze sheets, banding under video encoding (Nocturne)

Researcher: R2 (read-only except this file). Date: 2026-11-13.

**Ground truth.**
- The URP package folder is `Library/PackageCache/com.unity.render-pipelines.universal@8457e85b8184`. Its `package.json` says version 17.6.0. The task text names `@17.6.0`, which does not exist.
- Core is `com.unity.render-pipelines.core@2d66c71e606e`. These are the same folders Thread used, so Thread's file:line citations still hold. Below, URP = the universal folder and CORE = the core folder.

**How the numbers were made.**
- "(sim)" marks a number computed in this session with numpy (`py -3.12 -I`, scripts piped through stdin, no files written).
- "(enc)" marks a number from an in-memory encode experiment: frames piped to ffmpeg 9.0.2 (gyan full build) with libx264 preset medium, then decoded through pipes.

**Spec parts covered.** 14.2.2, 14.2.4, 14.7 H1, H3 and H5, 11.12 and 8.6. Related reports:
- R3 (`R3_urp_rendergraph_instancing.md` §1) already covers the Render Graph plumbing of NocRain. This report agrees with it and adds the shading model, the coordinate maths and the numbers.
- Thread's `docs/research/R6_banding_dither.md` is the base for section 3. It is summarised there, not repeated.

---

## 0. Summary: decisions for the lead

1. **Rain blending.**
   - Make rain an **additive** radiance (`Blend One One`). Use one full-screen draw for all 4 layers into the still-MSAA `cameraColor` at `BeforeRenderingPostProcessing`.
   - Get per-sample occlusion by reading the MSAA depth attachment as `Texture2DMS<float>` (bindMS is true on D3D11) and weighting each layer by its visible-sample fraction. For an additive term this equals per-sample compositing followed by the resolve (§1.5).
   - Fall back to R3's per-layer `SV_Depth` "approach A" if `bindTextureMS` is false.
   - Never use `_CameraDepthTexture`: under MSAA it holds the **farthest** sample (`URP/Shaders/Utils/CopyDepthPass.hlsl:35-55`).
   - Do not use premultiplied over-blending with the H5 brightness. It turns streaks into **dark** lines over bright signs and dark haze (§1.5).
2. **Three defects in the H5 formulas need fixing before coding** (§1.3):
   - **Drift.** `arc += tilt q` with a time-varying tilt and `q = ... + 9 t_r` moves drops sideways at `9 t dtilt/dt`. That is 27 m/s at t = 600 s, and it does not match the streak orientation. Instead, integrate the drift `X(t) = ∫ 9 slope dt` on the CPU in double, and orient each streak by the current slope.
   - **Seam.** 0.30 z cells do not tile the circle (2π/0.3 = 20.94), so a seam appears at azimuth 180° (south). Use **21 cells** (width 0.2992 z) and a periodic density noise.
   - **Precision.** Pass the scroll phases as an integer part plus a fraction.
3. **Rain brightness.** The H5 formula taken literally adds **+18 to +24 codes** to every near-layer streak in dark haze (code 15-57). That contradicts "adds no noise to dark gradients".
   - Multiply it by `smoothstep(0, 0.15, Lloc)`. Dark uniform haze then gets ≤ 4 codes, lit regions are unchanged, and dark gaps next to lights still get bright streaks (§1.4).
   - Tint by the blurred scene colour. Clamp the exposed add to ≤ 0.8 so moving streaks never cross the bloom threshold.
   - The literal formula cost 2.8× the rain bitrate of the gated one in a synthetic x264 test (enc).
4. **Zenith.**
   - Density per steradian grows as 1/cos³(el), and streak length shrinks as cos²(el). At KS02 (pitch +66) the 4 layers put **≈25 600** dashes on screen (sim).
   - Add a per-layer fade on projected streak length, `smoothstep(3, 8 px at 1080p)`. That leaves ≈440 (sim).
   - Use seam-free analytic derivatives of the azimuth (never `fwidth` across the `atan2` cut or across cells).
   - Add a short list of shelter boxes and tube capsules where rain is suppressed (gate tunnels, train tubes).
5. **H5 test band is inconsistent with H5's own parameters.** Cells of 0.30 z × 0.80 z with 6 slots at p = 0.6 give **118 streaks** per 1080p frame at pitch 0 and ρ_eff = 1 (sim and analytic), and about 91 at mean ρ_eff. Coverage is 0.81 %. The spec says 180, and its test band 140-210 cannot be reached.
   - Option (a): arbitrate the band to the computed value ±15 %.
   - Option (b): raise the slot probability to 0.9 to keep the 180 target. That makes coverage about 1.2 %.
6. **Fog base term: no change.** `σ (1 + 1.8 smoothstep(0, 1400, -y_end)) d` is **exactly** the optical depth of the physical density `σ (1 + 1.8 (9x² - 8x³))`, x = clamp(-y/1400, 0, 1), along any ray from y = 0. The 1.7 m eye height causes |ΔT| ≤ 0.0009 (sim). The T-FOG values stand.
7. **Haze sheets: replace the per-fragment factor.** The factor `(1 + 0.5 Σ exp(-((y-y_k)/45)²))` is not a path quantity: along one ray, T rises by up to **0.153** (-41° rays) because it implies negative density below each sheet.
   - Use the closed-form path integral with `erf` (§2.2). It is monotone and veils everything seen through a sheet.
   - The spec's straight-down check "0.10 at -1 500" ignores the sheet at -1 500. With the sheet it is 0.0058 (endpoint form) or 0.083 (path form). Test T-FOG with sheets off and log the sheet-on values separately.
8. **KS02 cannot work with the alley fog as specified.**
   - With σ 5.0e-3 at s 88, the zenith cloud has T = 0.005 and the tower flank in the slit (el ≈ 86°, 491 m) has T = 0.0025.
   - Inside `NocSkyColor`, fog the cloud with `min(σ_beat, 6e-4)`.
   - Make the alley haze a height-capped extra layer: closed form in §2.3, fading over y 18-30 m. Horizontal views (KS00) are unchanged, and the slit then reads (T 0.91-0.98).
9. **Horizon seam in 14.2.2.** The sky is HazeHorizon at e = 0⁺ and AbyssNear at e = 0⁻. That is a hard step of 17/5/8 RGB codes (6.7 Y', 5.2 Cr codes), and it fails the H3 "no step ≥ 2 codes through the horizon" test. Fix: smootherstep HazeHorizon to AbyssNear over e = 0 to -6°, then AbyssNear to AbyssGlow as specified.
10. **Chroma needs its own dither** (§3).
    - Thread's dither is monochrome, which leaves Cb/Cr dithered only by rounding residue (0.18 code RMS, sim).
    - In Nocturne's violets one Cb/Cr code is ΔE00 0.46-0.92, against 0.35 for one Y' code.
    - In the abyss, chroma changes 3.4× faster than luma (Cb every 256 px, Y' every 861 px at 4K).
    - Add a **2×2-block blue-noise TPDF chroma dither** of ±1 Cb/Cr code along the exact BT.709 zero-luma RGB directions, weighted by saturation (§3.4). In the 4K x264 CRF 23 test it removed the abyss Cb steps (2.3 → 0 per 1000 px), and CRF 30 also passed (enc).
    - Upload a 2160p60 master. YouTube serves 2160p60 VP9 at about 11× the 1080p60 bitrate (one observed video: 17.4 vs 1.5 Mbps).

---

## 1. Rain (14.7 H5)

### 1.1 What the literature and earlier games settle

**Tatarchuk 2006, ATI ToyShop** (SIGGRAPH 2006 course 26 chapter 3, also NPH 2006). Points used here:
- "A novel post-processing rain effect simulating multiple layers of falling raindrops in a single compositing pass." It renders as "a full-screen pass before the final post-processing".
- Layers at different depths get motion parallax from a projective texture fetch whose w is a per-drop "rain parallax" value. All drops share a single direction vector, which the authors call crucial for a consistent rainfall effect.
- Random per-frame movement gave "excessive visual noise". The rain must be coherent drops, not noise. This is H5's reason for analytic drops.
- Shading: drops refract a large solid angle of the environment, so a drop "tends to be much brighter than its background", and its average brightness "does not depend strongly on its background" (they cite Garg & Nayar 2004).
- "Realistic rain is very faint in bright regions but tends to appear stronger when the light falls in a dark area". They bias drops toward white (the "milk in the water" film trick) to make rain read.
- Motion blur and glow are a post blur of the rain layer.
- Rain is switched off by script when the camera is indoors; a precomputed sky-visibility map is the alternative. This is the basis of §1.7 "shelters".

**Garg & Nayar** ("Photorealistic rendering of rain streaks", SIGGRAPH 2006; "Vision and Rain", IJCV 2007; TR 2004).
- A streak is the motion-blurred image of a drop: its pixel value is (1 - τ/T) background + (τ/T) drop radiance, where τ is the time the drop covers the pixel.
- Real streaks are therefore only a few percent opaque. They become visible mainly when lit by nearby lights against a dark background.
- Nocturne's 3.3 px opaque streak is a deliberate stylisation, like Tatarchuk's milk.

**Camera-attached shapes.**
- Wang & Wade (flight simulator) map scrolling textures onto a camera-aligned **double cone**.
- Forum experience with rain cones and cylinders: they "work until you look down" (the ground clips the cone) or up ("rain falls sideways"). Those failures are exactly the zenith and nadir cases handled in §1.3 (length fade) and by the depth test.

**Earlier games.** None of Abyss, The Ring or Thread had rain or a full-screen composite over MSAA. Nothing to reuse except the Render Graph patterns in Thread R1 and Nocturne R3.

### 1.2 Verified URP facts the design rests on

| Fact | Where |
|---|---|
| Camera depth attachment gets `bindTextureMS = hasMSAA && ShouldDepthAttachmentBindMS()` | `URP/Runtime/UniversalRendererRenderGraph.cs:1781` |
| `bindMS = !(supportsMultisampleResolveDepth && ...ResolveStencil) && supportsMultisampledTextures`; D3D11 cannot resolve depth, so true is expected (log it) | `URP/Runtime/RenderingUtils.cs:334-357` |
| `_CameraDepthTexture` under MSAA = `DEPTH_OP min` over samples with `UNITY_REVERSED_Z`, i.e. the **farthest** sample | `URP/Shaders/Utils/CopyDepthPass.hlsl:35-55` |
| A bindMS resource read through `UseTexture` keeps its samples (store `Store`, no resolve). A non-bindMS read gets `Resolve`/`StoreAndResolve` | `CORE/Runtime/RenderGraph/Compiler/NativePassCompiler.cs:1538-1600` |
| The colour intermediate is not bindMS on desktop; reading it resolves it, so the resolved scene **is** available before bloom (the H5 "after ThreadGrade" fallback is unnecessary) | `UniversalRendererRenderGraph.cs:1737`; R3 §1.1 |
| Events: AfterRenderingTransparents 500, BeforeRenderingPostProcessing 550, AfterRenderingPostProcessing 600 | `URP/Runtime/Passes/ScriptableRenderPass.cs:148-158` |
| `LOAD_TEXTURE2D_MSAA(t, coord, s)` = `t.Load(coord, s)`; `LinearEyeDepth(d, _ZBufferParams)` | `CORE/ShaderLibrary/API/D3D11.hlsl:149`, `CORE/ShaderLibrary/Common.hlsl:1206` |
| `cameraColor` and `cameraDepth` have public setters (pass swaps are allowed) | `URP/Runtime/FrameData/UniversalResourceData.cs:126-142` |

### 1.3 Exact analytic geometry (four camera-centred, world-aligned cylinders)

**Notation.**
- Layer radii z ∈ {3, 6, 12, 20} m. Ray d is a unit vector in world axes, with the camera at the origin.
- h = |d.xz| = cos(el); θ = atan2(d.x, d.z) (0 = north, +90° = east); τ_e = d.y/h = tan(el).
- A point on layer z has unrolled coordinates A = zθ (arc, metres) and B = z τ_e (height on the cylinder, metres). Its distance along the ray is D = z/h.

**Cells.**
- N = **21** cells around the circle. Cell width c_w = 2πz/21 = 0.2992 z; cell height c_h = 0.8 z.
- In angular terms every layer has **the same grid**: 21 azimuth columns, and τ_e rows of height 0.8. Only the scroll phases differ: the near layer scrolls faster in cells per second. This is the classic parallax-layer construction.

**Moving frame (fixes the H5 drift defect).** Drops are fixed in a frame that falls at 9 m/s and drifts horizontally by X(t):
```
U = θ·21/(2π) − D_l(t)        D_l = X(t)/c_w (cells, wrapped mod 21)
V = τ_e/0.8 + Φ_l(t) + stagger(i)        Φ_l = 9 t_r / c_h (cells)
i = floor(U) mod 21,  fu = frac(U);   j = floor(V),  fv = frac(V)
X(t) = ∫ 9·slope(t) dt   (C#, double, fixed step 1/240 s from t = 0, so captures are deterministic)
slope(t) = lerp(0.10 sin(0.05 t), tan(0.28) sin(2π t/17), gust(s))   (gust(s): Rule-P weight of beat 4)
streak axis in (A, B) metres: u = normalize(slope, −1);  normal n = (−u.y, u.x)
```
- A drop's on-screen velocity is (9 slope, -9) m/s on the cylinder, and its streak is oriented along the same vector, so they match.
- The base drift is bounded: X ∈ [0, 36] m. Gusts add up to ±6.8 m.

Why the spec form fails:
- In `arc += tilt q` with `q = z d.y/h + 9 t_r`, a drop is fixed in (arc + tilt q, q). Its arc then moves at d(tilt)/dt · q₀, where q₀ ≈ 9 t.
- With tilt = 0.10 sin(0.05 t), that is 0.10 · 0.05 · 9 t ≈ 0.045 t m/s: 4.5 m/s at t = 100 s and 27 m/s at t = 600 s, with orientation slope ≤ 0.10. Streaks would visibly slide sideways along their own length in the second half of the walk.

**Stagger.**
- `stagger(i) = hash(i, layer)` offsets each column's rows. Cell rows then never line up horizontally across columns.
- Without it, any per-cell margin shows as horizontal "rain sheets" that move down at 9 m/s.

**Drops.**
- Up to 6 slots per cell. Slot k has hash (r₀, r₁, r₂) from `pcg3d(i, j & 0xFFFFF, 8·layer + k)` (Jarzynski & Olano 2020).
- Slot k is present with soft weight `a_k = saturate((p_cell − r₀)/0.06)`, where `p_cell = 0.6 ρ (0.55 + 0.45 vnoise)`. When ρ or the noise changes, drops fade over a few frames, never pop.
- Evaluate `vnoise` once per cell at the cell-centre azimuth. It must be **periodic in azimuth**: 11 lattice cells per turn, index `(x mod 11)`. The spec's `az·1.7` (period 10.68) leaves a density seam at θ = ±π.
- Drop centre in metres inside the cell: `(m_h + (c_w − 2m_h) r₁, m_v + (c_h − 2m_v) r₂)`, with m_h = 0.09 m and m_v = 0.16 m.
  - m_h ≥ |slope|·0.15 + drawn half-width + ½ px AA = 0.043 + 0.026 + 0.012 at 20 m (1 px at 1080p is 0.024 m there).
  - m_v ≥ half the streak length plus AA.
- No neighbour-cell lookup is needed. The H5 "15 % fade at the cell ends" is unnecessary in the moving frame: drops never wrap. Keep it out, because it would modulate density in rows.

**Pixel-exact capsule in pixel units (no `fwidth` on cell coordinates).**
- `fwidth` of cell-local coordinates spikes at every cell border and at the `atan2` cut. A smoothstep with that width draws half-coverage lines along all cell borders.
- Instead, take the Jacobian from derivatives of the continuous ray d:
```
∂θ/∂x = (d.z ∂d.x/∂x − d.x ∂d.z/∂x)/h²          (same for y; seam-free)
∂h/∂x = (d.x ∂d.x/∂x + d.z ∂d.z/∂x)/h ;  ∂τ_e/∂x = (∂d.y/∂x · h − d.y ∂h/∂x)/h²
J = [[z ∂θ/∂x, z ∂θ/∂y], [z ∂τ_e/∂x, z ∂τ_e/∂y]]   (metres per pixel)
δ = (fu c_w, fv c_h) − centre ;  along = δ·u ; across = δ·n ;  along_c = sign(along) max(|along| − 0.15, 0)
dist_px = |J⁻¹ (across n + along_c u)| ;  halfTrue = 0.006 |J⁻¹ n| ;  halfDraw = max(halfTrue, 1.1·H/1080)
α = saturate(halfDraw + 0.5 − dist_px) ;  k_E = halfTrue/halfDraw   (energy: true/drawn width)
L_px1080 = 0.30 |J⁻¹ u| · 1080/H ;  k_len = smoothstep(3, 8, L_px1080)   (zenith/nadir fade, per layer)
```
- Spec widths: 0.012 m is 3.33 px at z = 3, so it is drawn true. Farther layers are clamped to 2.2 px at 1080p (4.4 px at 4K, so the look is resolution-independent).
- Energy factors k_E (sim): 1.0, 0.76, 0.38, 0.23 for z = 3, 6, 12, 20. They make the 12 m and 20 m layers the faint "far veil" H5 asks for, instead of a uniform curtain of equally bright lines.

**Numbers** (1080p, vFOV 66, f = 831.5 px; sim):

| z (m) | streak px at el 0 | motion px/frame 60 fps | true width px | k_E | streak px at pitch 22 / 52 / 66 |
|---|---|---|---|---|---|
| 3 | 83.2 | 41.6 | 3.33 | 1.00 | 71.5 / 31.5 / 13.8 |
| 6 | 41.6 | 20.8 | 1.66 | 0.76 | 35.7 / 15.8 / 6.9 |
| 12 | 20.8 | 10.4 | 0.83 | 0.38 | 17.9 / 7.9 / 3.4 |
| 20 | 12.5 | 6.2 | 0.50 | 0.23 | 10.7 / 4.7 / 2.1 |

- **Strobing.** Streak length / per-frame motion = 2.0 at every elevation, because both scale with cos²(el). At 30 fps they abut (ratio 1). No strobing anywhere; the shortening near the zenith does not change this ratio.
- **At pitch 0 every streak is ≥ f·0.3/z px.** Rectilinear magnification (sec²) exactly cancels cylinder foreshortening (cos²), so "none shorter than 10 px" holds at pitch 0 for all layers. Shortening comes only from pitching the camera.

**Counts** (sim at 1080p with the full model; the analytic area integral agrees: 8.2 cells per layer in frame at pitch 0):

| Pose | Streaks (ρ_eff 1) | Energy coverage | Pixels with rain > 0.02 | With length fade 3-8 px |
|---|---|---|---|---|
| pitch 0, vFOV 66 | 119 | 0.81 % | 1.14 % | 119 |
| pitch 22 (KS04) | 160 | 0.87 % | 1.27 % | 160 |
| pitch 52 (KS05) | 1 320 | 0.91 % | 1.90 % | 361 |
| pitch 66, vFOV 62.1 (KS02) | 25 593 | 0.97 % | 3.27 % | 437 |
| pitch -30, vFOV 68 (KS10) | 212 | 0.76 % | 1.19 % | 200 |

Zenith physics:
- One layer is a shell of fixed horizontal radius. The ray crosses it at distance z/cos(el) over a path Δ/cos(el), so drops per steradian scale as 1/cos³(el): 8× at 60°, 25× at 70°, 191× at 80°.
- Energy per drop falls as cos³, so the mean added light stays constant. The *count* of sub-pixel dashes explodes, and that reads as noise and is expensive to encode.
- The per-layer length fade removes each layer once its dashes are shorter than 3-8 px.
- Optional density compensation `p_cell ·= min(1, (h/0.866)³)` keeps drops per steradian constant above 30° elevation.
- Exactly at h → 0, guard every division with `max(h, 1e-4)`. k_len is 0 there anyway.

### 1.4 Brightness: the spec formula, its problem, the fix

Spec: `B = 0.35 (0.04 + 0.9 clamp(Lloc/0.8, 0, 1.5))`, added per streak (α ≈ 1 for the near layer).

Added codes for a uniform region where Lloc = background luminance, EV 0, before the dither (sim). "Gated" means B × smoothstep(0, 0.15, Lloc):

| Background sRGB code | Spec near (k_E 1) | Spec far (k_E 0.23) | Gated near | Gated far |
|---|---|---|---|---|
| 15 (near Floor) | +24.4 | +7.7 | +0.1 | +0.0 |
| 25 | +21.2 | +6.2 | +0.4 | +0.1 |
| 40 | +18.9 | +5.1 | +1.3 | +0.3 |
| 57 (abyss haze) | +18.3 | +4.8 | +3.8 | +0.9 |
| 80 | +19.4 | +4.9 | +11.3 | +2.8 |
| 120 | +23.3 | +5.8 | +23.3 | +5.8 |
| 160 | +28.2 | +7.0 | +28.2 | +7.0 |
| 200 | +33.5 | +8.3 | +33.5 | +8.3 |

Dark gap at code 40 next to a light, gated near layer: Lloc 0.05 → +8; 0.1 → +30; 0.2 → +55; 0.4 → +81; 0.8 → +120 codes.

Rain therefore appears where light is, which is the photographic look and Tatarchuk's observation. Uniform dark haze stays quiet.

The spec's absolute floor `0.35·0.04 = 0.014` linear is the cause:
- It is +100 % relative contrast at the darkest haze, against about 40 % in lit areas. That is the opposite of the stated intent.
- In a synthetic 4K x264 CRF 23 test (120 moving streaks over the dark gradients of §3, 48 frames), the literal formula raised the bitrate from 0.74 to 1.50 Mbps, against 1.01 Mbps gated (enc). The encoder must code every dark-haze streak.

**Recommended rain radiance** (exposed units; divide by the exposure gain before adding to the pre-exposure scene):
```
ΔC_rgb = B(Lloc) · smoothstep(0, 0.15, Lloc) · tint · Σ_l  w_l · k_len,l · vis_l · shelter_l · Σ_k α_k a_k k_E (0.7 + 0.3 r)
tint   = min(lerp(1, C_env / max(Lloc, 1e-4), 0.6), 2.5)    // the streak takes 60 % of the local light's hue (magenta by signs, cyan by the koi)
ΔC     = min(ΔC, 0.8)                                       // below the bloom threshold 0.9: moving streaks never drive bloom flicker
```
- `C_env` is the blurred HDR scene colour, and `Lloc` its luma.
- `C_env` comes from a 1/16-resolution blur: full → 1/4 13-tap with Karis average → 1/16 13-tap, sampled bicubically with `SampleTexture2DBicubic`, `CORE/ShaderLibrary/Filtering.hlsl:111`. That is about a 3° radius, about one lamp halo.
- R3 uses 1/8. Either works; 1/16 is cheaper and smoother.
- Lloc must include the koi and signs. Compute it after transparents.

### 1.5 Occlusion per MSAA sample and blending

- A layer is visible behind sample s if `eyeDepth_s > (z/h)·dot(d, camForward)` (eye depth of the cylinder point).
- For an **additive** term R, compositing per sample and then resolving gives `mean_s(C_s + R v_s) = resolve(C) + R·mean_s(v_s)`. So one per-pixel evaluation weighted by the visible-sample fraction `f = Σ v_s / 4`, written to all samples of the MSAA target, is **exactly** the per-sample result Part 8.6 asks for. Edge pixels of a 1.75 px rail get f = 0.25-0.75, so there is no halo.
- **Premultiplied over** (R3's sketch uses `Blend One OneMinusSrcAlpha`) is exact only with per-sample depth (`SV_Depth` per layer, R3 approach A). It also **darkens** wherever the rain colour is below the background: the gated B is near zero in dark haze, and the spec B (≤ 0.49) is below a sign core at 0.8-3.0. Streaks would become black lines in haze and dark lines across signs.
- Keep rain additive. Light scattered by drops only adds; Tatarchuk's "glow" path is also additive.

**Recommended passes** (agree with R3 §1.2 except for the blend and the single draw):
1. `NocRainEnv` (raster, BeforeRenderingPostProcessing): `UseTexture(cameraColor)` (MSAA resolve), then down to 1/4 and 1/16 RGBA16F, Bilinear and Clamp set explicitly.
2. `NocRain` (raster, same event, after 1):
   - `SetRenderAttachment(cameraColor, 0, AccessFlags.ReadWrite)` (MSAA), no depth attachment.
   - `UseTexture(cameraDepth)`, read as `Texture2DMS<float>` (assert `rg.GetTextureDesc(cameraDepth).bindTextureMS`, else R3 approach A).
   - `UseTexture(env)`.
   - One `DrawProcedural` (3 vertices), `ZTest Always ZWrite Off Cull Off Blend One One`, `discard` where ΔC < 1e-5. About 98 % of pixels write nothing.
3. ThreadGrade at 600 reads `cameraColor`, so the native pass compiler resolves again. That second resolve exists anyway.
- With `-rain 0`, record no pass, so there are no extra resolves.

### 1.6 HLSL sketch (one pass, 4 layers)

```hlsl
// Megastructure/NocRain, pass "NocRainLayers": ZTest Always  ZWrite Off  Cull Off  Blend One One   (#pragma target 4.5)
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
Texture2DMS<float> _NocDepthMS;              // resourceData.cameraDepth (bindTextureMS asserted in C#)
TEXTURE2D(_NocRainEnv);                      // 1/16-res blurred HDR scene (RGBA16F, bilinear, clamp)
float4   _NocRainEnvSize;                    // w, h, 1/w, 1/h
float4   _NocRainScreen;                     // target w, h, 1/w, 1/h
float4x4 _NocRainIVP;                        // camera-relative inverse VP, GL.GetGPUProjectionMatrix(proj, true) (R3 §1.2)
float4   _NocRainCam;                        // xyz camera forward (world), w exposure gain
float4   _NocRainA;                          // x rho (Rule-P), y slope, z H/1080, w noise x-time (t/9 mod 11)
float4   _NocRainB;                          // x noise y-time (t/13), y length-fade lo 3, z hi 8, w density comp 0/1
float4   _NocRainU[4];                       // per layer: x uFrac, y vFrac, z radius z, w layer weight
float4   _NocRainI[4];                       // per layer: x iBase (0..20), y jBase (mod 2^20), exact integers in float
float4   _NocShelterMin[8], _NocShelterMax[8]; float _NocShelterCount;   // camera-relative AABBs (tubes: see 1.7)

static const float kTwoPi = 6.28318530718, kN = 21.0, kCellH = 0.8;
uint3 NocPcg3d(uint3 v) { v = v * 1664525u + 1013904223u; v.x += v.y*v.z; v.y += v.z*v.x; v.z += v.x*v.y;
                          v ^= v >> 16u; v.x += v.y*v.z; v.y += v.z*v.x; v.z += v.x*v.y; return v; }
float3 NocHash3(uint3 v) { return float3(NocPcg3d(v)) * (1.0 / 4294967296.0); }
float NocNoiseP11(float x, float y)          // value noise, period 11 in x (seam-free around the circle)
{
    float2 i = floor(float2(x, y)), f = float2(x, y) - i; f = f * f * (3.0 - 2.0 * f);
    uint x0 = (uint)(((int)i.x % 11 + 11) % 11), x1 = (x0 + 1u) % 11u, y0 = (uint)max(i.y, 0.0), y1 = y0 + 1u;
    float a = NocHash3(uint3(x0, y0, 977u)).x, b = NocHash3(uint3(x1, y0, 977u)).x;
    float c = NocHash3(uint3(x0, y1, 977u)).x, d = NocHash3(uint3(x1, y1, 977u)).x;
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}
struct V2F { float4 positionCS : SV_POSITION; float2 ndc : TEXCOORD0; };
V2F VertRain(uint id : SV_VertexID)
{ float2 p = float2((id << 1) & 2, id & 2) * 2.0 - 1.0; V2F o; o.positionCS = float4(p, 0.5, 1.0); o.ndc = p; return o; }

float4 FragRain(V2F i) : SV_Target
{
    float4 hp = mul(_NocRainIVP, float4(i.ndc, 0.5, 1.0));
    float3 d = normalize(hp.xyz / hp.w), dX = ddx(d), dY = ddy(d);
    float h2 = max(dot(d.xz, d.xz), 1e-8), h = sqrt(h2);
    // local light first: dark pixels exit before any drop maths
    float3 env = SampleTexture2DBicubic(TEXTURE2D_ARGS(_NocRainEnv, sampler_LinearClamp),
                     i.positionCS.xy * _NocRainScreen.zw, _NocRainEnvSize, 1.0.xx, 0u).rgb * _NocRainCam.w;
    float Lloc = dot(env, float3(0.2126, 0.7152, 0.0722));
    float B = 0.35 * (0.04 + 0.9 * clamp(Lloc / 0.8, 0.0, 1.5)) * smoothstep(0.0, 0.15, Lloc);
    if (B * _NocRainA.x < 1e-5) discard;
    float theta = atan2(d.x, d.z), tanEl = d.y / max(h, 1e-4);
    float2 gTh = float2(d.z * dX.x - d.x * dX.z, d.z * dY.x - d.x * dY.z) / h2;      // seam-free
    float2 gH  = float2(dot(d.xz, dX.xz), dot(d.xz, dY.xz)) / h;
    float2 gTe = (float2(dX.y, dY.y) * h - d.y * gH) / h2;
    float2 ax = normalize(float2(_NocRainA.y, -1.0)), nr = float2(-ax.y, ax.x);
    float halfMin = 1.1 * _NocRainA.z, U0 = theta * (kN / kTwoPi);
    float layerSum[4];
    [unroll] for (int l = 0; l < 4; l++)
    {
        float z = _NocRainU[l].z, cw = z * (kTwoPi / kN), ch = z * kCellH;
        float2 gA = gTh * z, gB = gTe * z;                               // J rows (m per px)
        float det = gA.x * gB.y - gA.y * gB.x; det = abs(det) < 1e-12 ? 1e-12 : det;
        float2x2 Ji = float2x2(gB.y, -gA.y, -gB.x, gA.x) / det;          // px per m
        float kLen = smoothstep(_NocRainB.y, _NocRainB.z, 0.30 * length(mul(Ji, ax)) / _NocRainA.z);
        float halfTrue = 0.006 * length(mul(Ji, nr)), halfDraw = max(halfTrue, halfMin), kE = halfTrue / halfDraw;
        float U = U0 - _NocRainU[l].x, iF = floor(U), fu = U - iF;
        uint ic = (uint)(((int)iF - (int)_NocRainI[l].x) % 21 + 21) % 21;
        float stagger = NocHash3(uint3(ic, 41u, l)).x;
        float V = tanEl / kCellH + _NocRainU[l].y + stagger, jF = floor(V), fv = V - jF;
        uint jc = ((uint)(int)(jF + _NocRainI[l].y)) & 0xFFFFFu;
        float pCell = 0.6 * _NocRainA.x * (0.55 + 0.45 * NocNoiseP11((iF + 0.5 + _NocRainU[l].x) * (11.0 / 21.0) + _NocRainA.w, _NocRainB.x));
        if (_NocRainB.w > 0.5) pCell *= min(1.0, pow(h / 0.866, 3.0));
        float2 p = float2(fu * cw, fv * ch), acc = 0;
        [unroll] for (uint k = 0; k < 6; k++)
        {
            float3 r = NocHash3(uint3(ic, jc, 8u * l + k));
            float ak = saturate((pCell - r.x) / 0.06);
            float2 c = float2(0.09 + (cw - 0.18) * r.y, 0.16 + (ch - 0.32) * r.z);
            float2 dl = p - c; float al = dot(dl, ax), ac = dot(dl, nr);
            float alc = sign(al) * max(abs(al) - 0.15, 0.0);
            float dist = length(mul(Ji, ac * nr + alc * ax));
            acc.x += saturate(halfDraw + 0.5 - dist) * ak * (0.7 + 0.3 * frac(r.x * 7.31));
        }
        layerSum[l] = acc.x * kE * kLen * _NocRainU[l].w;
    }
    float any = layerSum[0] + layerSum[1] + layerSum[2] + layerSum[3];
    if (any < 1e-4) discard;
    // per-sample visibility (exact for an additive term) and shelters
    int2 pix = int2(i.positionCS.xy); float4 ze;
    [unroll] for (int s = 0; s < 4; s++) ze[s] = LinearEyeDepth(_NocDepthMS.Load(pix, s), _ZBufferParams);
    float cosF = dot(d, _NocRainCam.xyz), sum = 0;
    [unroll] for (int m = 0; m < 4; m++)
    {
        float D = _NocRainU[m].z / max(h, 1e-4);
        float vis = dot(step(D * cosF, ze), 0.25.xxxx);
        float3 P = d * D; float dry = 0;                                  // camera-relative cylinder point
        for (int b = 0; b < (int)_NocShelterCount; b++)
            dry = max(dry, all(P > _NocShelterMin[b].xyz) && all(P < _NocShelterMax[b].xyz) ? 1.0 : 0.0);
        sum += layerSum[m] * vis * (1.0 - dry);
    }
    float3 tint = min(lerp(1.0.xxx, env / max(Lloc, 1e-4), 0.6), 2.5);
    float3 add = min(B * tint * sum, 0.8.xxx) / max(_NocRainCam.w, 1e-4);  // back to pre-exposure units
    return float4(add, 0.0);
}
```
Notes:
- Shelter boxes use hard tests. Fade the near layers out over 1 m with `smoothstep` on a box SDF if a hard cut ever shows.
- A capsule test is needed for the round train tubes (§1.7).
- `_NocRainI` holds integers below 2^24 in floats, which are exact. Unity has no int-array setter.

### 1.7 C# side, shelters, flags

- **Per-frame uniforms, computed in double:**
  - `X(t)`: fixed-step integration of `9·slope` (1/240 s) from t = 0. Recompute deterministically when capture mode jumps to `-progress`.
  - `D_l = X / c_w mod 21` gives `iBase = floor`, `uFrac = frac`.
  - `Φ_l = 9 t_r / (0.8 z_l)` gives `jBase = floor mod 2^20`, `vFrac = frac`.
  - Noise times `(t/9) mod 11` and `t/13`.
- **Layer weights** `w_l`: 1, 1, 1, 1. The energy factor already makes the far layers faint. Add a weight here only if a critic asks.
- **Clock.** `t_r` is real time in play, `virtual time = frame/captureFramerate` in capture, and frozen with `-freeze-rain`. `-rain 0` removes both passes.
- **Shelters** (camera-relative boxes, uploaded per beat; at most 8):
  - Gate A tunnel x 372-384, z 189-201, y 0-8.
  - Gate B tunnel x 683-695, same z and y.
  - Train tubes N and S: the cylinder points of the 6 m and 12 m layers fall inside the r 4.5 glass tubes at z 183 and 207. Glass is ZWrite Off, so the depth test cannot remove that rain. Add two capsules: `(y−2)² + (z−z_tube)² < 4.5²`, x 384-683.
  - Arcade beat 6 only if the walker ever stands under it; per 14.4 they do not.
- **Spec flags.** `-rain X` overrides ρ. Use a `-rain-noise off` (or equivalent) test flag to force vnoise = 1 so the count test is deterministic.

### 1.8 Cost estimate (RTX 5070, estimate; measure with the Part 11.7 perf log)

- **Per pixel, worst case:**
  - Ray, derivatives and bicubic env (4 taps): about 80 ALU.
  - Per layer: cell, stagger and noise (5 hashes) about 60 ALU, plus 6 slots × (1 pcg3d, about 12, plus capsule with 2×2 inverse, about 25). About 280 ALU per layer, 1 120 for 4 layers.
  - Depth: 4 MSAA loads plus 4 `LinearEyeDepth`, only for about 2-5 % of pixels.
- **Total:** about 1 200 ALU per lit pixel. Dark pixels exit after the env fetch.
- **Throughput:** 8.3 M px × 1 200 ≈ 10 G ops, about 0.4-0.6 ms at 4K and about 0.12 ms at 1080p.
- **Bandwidth:** blends only on the ≈1-2 % of pixels with rain (discard elsewhere). The env pyramid costs under 0.05 ms.
- **The extra MSAA resolve** for the env pass is the dominant fixed cost: about 0.14 ms at 1080p and about 0.55 ms at 4K (R3).
- **Total NocRain:** about 0.3 ms at 1080p and about 1.0-1.2 ms at 4K, well inside the 16.7 ms 4K budget.
- **Fallback (14.8 never-cut list: "it may drop to one layer"):** keep z = 3 only, about 0.35× the ALU.

### 1.9 Pitfalls (rain)

1. `fwidth` on cell-local or `atan2` coordinates draws lines on every cell border and a seam at south. Use the analytic Jacobian above.
2. A non-integer cell count around the circle leaves a seam at θ = ±π. A non-periodic density noise does too.
3. A time-varying tilt multiplied by an unbounded q makes streaks slide (§1.3). Integrate the drift instead.
4. Floats for `9 t_r` lose about 1 mm at 1 000 s. Split integer and fraction on the CPU.
5. Reading `_CameraDepthTexture` (farthest sample) puts rain over rail edges. Use MSAA depth.
6. Over-blending with a brightness that can be below the background gives dark streaks. Use additive.
7. Rain above the bloom threshold makes bloom flicker as streaks move. Clamp to 0.8.
8. Near the zenith there are thousands of dashes. Use the length fade, plus density compensation if critics still see noise.
9. Rain inside tunnels and tubes. Use shelters.
10. The gust transition: do not blend the drift X itself between beats, because `dw/ds · ΔX` creates a transient 3.75 m/s lateral jump. Blend the slope and integrate.
11. MSAA: write the same value to all samples. Do not run at sample frequency (no `SV_SampleIndex` input), or the cost goes ×4.

### 1.10 Tests (rain)

- **EditMode (C# mirror of the HLSL maths):**
  - Cell index continuity across θ = ±π: the hash pattern at θ = π − ε and −π + ε belongs to cells 20 and 0, adjacent and wrapping.
  - Drift integral against a numeric integration of slope, ≤ 1 mm over 1 000 s.
  - Count at the pitch-0 pose, `-rain 1`, vnoise forced to 1: 118 ± 15 % (or 180 ± 15 % if p = 0.9 is adopted).
  - No NaN for d = (0, ±1, 0).
  - k_len = 0 for h < 0.2 at every layer.
- **Player, static camera, `-freeze-clock`, rain on, 120 frames:**
  - Pixel temporal change outside the streak masks ≤ 6 levels (H5).
  - Mean luma of the KS10 abyss rectangle with rain on minus rain off ≤ 1.0 level. This checks that dark haze gets no noise.
  - Per-streak peak contrast over the abyss median ≤ 4 codes.
- **T-RAIN-EDGE** (R3 §1.3, Part 8.6): a 1.75 px rail at 1.5-2 m in front of the 3 m layer. The rail's interior pixels must not change by more than 0.5 level with rain on vs off, and the edge profile must match a 4× supersampled reference within 1 level.
- **Encode test with rain on and off** (H5 last item): log the bitrate difference, and verify no new steps appear in the gradient regions.

---

## 2. Height-dependent exp-squared fog, haze sheets, night atmosphere (14.2.2, H3)

### 2.1 The base term is a true path integral (keep it)

Define x(y) = clamp(−y/1400, 0, 1). The spec's per-fragment form is `τ_e = σ (1 + 1.8 S(x(y₁))) d`, with S = smoothstep and y₁ the fragment height.

Claim: for any ray starting at y = 0 this equals the optical depth `∫₀^d σ_p(y(s)) ds` of the physical density
```
σ_p(y) = σ (1 + 1.8 (9x² − 8x³)),   x = clamp(−y/1400, 0, 1)      (σ_p = 2.8 σ below −1 400, = σ above 0)
```

Proof:
- Along a straight ray, y is linear in s, so `∫ σ_p ds = (d/Δy) ∫ σ_p dy`.
- Write F(u) = ∫₀^u σ_p(u′) du′ in depth u = −y. Then F(u) = u (1 + 1.8 S(u/1400)), because d/du[u S] = S + u S′ = 3x² − 2x³ + 6x² − 6x³ = 9x² − 8x³.
- So τ = σ F(u₁)/sinθ = σ (1 + 1.8 S) u₁/sinθ = σ (1 + 1.8 S) d.

Properties:
- σ_p is non-negative and peaks at 4.04 σ at y = −1 050 (a denser stratum), settling to 2.8 σ.
- Because τ is a path integral of a non-negative density, T = exp(−τ²) is monotone along every ray.

Numeric check (sim):
- With the eye at 1.7 m, |T_path − T_endpoint| ≤ 0.0009 over all downward rays (−0.5° to −90°, d ≤ 8 km).
- Every value in 14.2.2 reproduces: 0.995 / 0.878 / 0.596 / 0.31 / 0.039 / 0.0042 at 0.2 to 6.5 km; 0.46 / 0.10 / 0.017 / 0.0017 straight down to −1 000 to −2 500 (sheets off); B face y −800 at 943 m: 0.602.

Conclusion: keep `σ_eff(y_end)·d` exactly as specified for the height term. It is cheap and physically consistent.

### 2.2 The sheet factor is not (replace it)

The spec factor `S_k(y₁) = 1 + 0.5 Σ exp(−((y₁ − y_k)/45)²)` evaluated at the fragment height corresponds to `σ_p = d/du[u σ(u) S(u)]`. The term `u σ S′` is **negative** on the far flank of each sheet: about −2.7 σ at y −282.

Consequence (sim): T **increases** with distance along a ray just below each sheet.

| Ray | Max T rise along the ray |
|---|---|
| −41° | 0.153 |
| −10° | 0.062 (at y −312, d 1 806 m) |
| −20° | 0.040 (at y −775) |
| −5° | 0.023 |

Visually, each sheet is a bright painted stripe on the faces with clear face below it. It is not a layer of mist you look through, and a critic would read it as a glowing line artefact.

**Exact replacement** (monotone, veils everything seen through a sheet):
```
τ = σ (1 + 1.8 S(x(y₁))) d                                   // §2.1, unchanged
  + σ · s_w · Σ_k (1 + 1.8 S(x(y_k))) · ḡ_k · d               // sheets, s_w = 0.5 (0 when cut, 14.8 item 9)
ḡ_k = (w√π/2) (erf((y₁ − y_k)/w) − erf((y₀ − y_k)/w)) / (y₁ − y₀)   if |y₁ − y₀| ≥ 1 m
    = exp(−((½(y₀ + y₁) − y_k)/w)²)                                 otherwise              (w = 45, y₀ = camera y)
T = exp(−τ²)
```
- (1 + 1.8 S) is taken at the sheet centre. It varies about ±9 % over ±2w, but it is almost linear there, so the error is 0.2 % for a ray that crosses the whole sheet and ≤ 3 % for a fragment inside a sheet.
- erf: Abramowitz & Stegun 7.1.26, max error 1.4e-7 (sim). It costs 1 exp and 6 FMA.
- Skip the sheet sum when `min(y₀, y₁) > −60` (the nearest sheet then contributes under 1e-6). Most fragments, all towers above ground, skip it.

New values (sim; spec endpoint with sheets in brackets):

| Point | Path model | Spec endpoint |
|---|---|---|
| Straight down −250 | 0.987 | 0.976 |
| Straight down −700 | 0.771 | 0.596 |
| Straight down −1 000 | 0.425 | 0.460 |
| Straight down −1 500 | 0.083 | 0.0058 |
| Straight down −2 000 | 0.012 | 0.017 |
| Straight down −2 500 | 0.0011 | 0.0017 |
| B face y −800 at 943 m | 0.56 | 0.60 |
| −45° ray at 1.96 km | 0.016 | 0.020 |

Geometry bottoms (y −3 300) stay invisible.

Update T-FOG:
- Keep the listed values as the sheets-off test.
- Add the path-model sheet-on values above (tolerance 0.002).
- Add a monotonicity test: T non-increasing along 180 rays from −1° to −89° in 5 m steps.
- Freeze the sheet drift (y_k ± 15 m, 120 s) under `-freeze-clock`.

The sky side stays as specified: for e < 0, `NocSkyColor` is multiplied by `1 + 0.10 Σ exp(−((e − e_k)/5)²)`, e_k = −5.7, −15.6, −31°.
- Each e_k is where a ray crosses its sheet 2.5 km out (checked: 251.7/sin 5.7° = 2 534 m, 701.7/sin 15.6° = 2 608 m).
- So geometry fading into the fog colour near 2.5 km shows the sky's bands at the same place: geometry and sky agree where it matters.

### 2.3 Alley fog against the KS02 slit (a real conflict)

At s 88, σ = 5.0e-3 (it falls only over s 96-124). Looking up at KS02 (frame +35..+97°):

| Target | Spec value | Problem |
|---|---|---|
| Cloud deck at el 60 / 75 / 90° | T 0.0009 / 0.0036 / 0.0052 | `NocSkyColor`'s own cloud term, fogged with the beat σ, collapses to the bare gradient S₀(e) = NightZenith, luma code about 14. KS02 requires slit mean luma ≥ 55. CloudBase is luma 64. |
| Tower flank in the slit (KS02 lists it at y +0.58..+0.66, el ≈ 86.5°, ≈491 m) | T = 0.0025 | invisible |

Recommendations, both cheap:
1. **Fog the cloud inside `NocSkyColor` with `σ_sky = min(σ_beat, 6.0e-4)`.** The cloud term is part of the sky function: everything fogs toward it, so it must not vanish.
2. **Make the alley haze a height-capped extra layer** instead of a global σ.
   - Use the regional σ_r = 3.6e-4 everywhere.
   - Add `σ_a − σ_r = 4.64e-3` (Rule-P blended to 0 over s 96-124) with weight `1 − smoothstep(18, 30, y)` (podia 22 and 31 m).
   - Exact path mean:
```
Q(y) = 0 (t ≤ 0);  12 (t³ − t⁴/2) (0 < t < 1);  12 (t − 0.5) (t ≥ 1),  t = (y − 18)/12
mean_w(y₀, y₁) = 1 − (Q(y₁) − Q(y₀))/(y₁ − y₀)   (|y₁ − y₀| ≥ 0.01),  else 1 − S((y₀ − 18)/12)
τ += (σ_a − σ_r) · mean_w · d
```
   - Horizontal rays at eye height keep the spec's alley fog: KS00 mouth at 120 m, T = 0.698.
   - Upward (sim): el 66° at 40 m, T 0.984 (spec 0.961); el 80° at 200 m, 0.969 (spec 0.368); el 85° at 344 m, 0.949 (spec 0.052).
   - The slit then shows the tower flanks against the magenta cloud as KS02 describes.

### 2.4 The horizon seam in the sky definition

- Above the horizon, 14.2.2 starts at HazeHorizon (#5B2F78) at e = 0⁺. Below, it starts at AbyssNear (#4A2A70) at e = 0⁻.
- That is a C0 step of 17/5/8 RGB codes, ΔY′ 6.7 and ΔCr 5.2 codes (sim). It fails H3's "no step ≥ 2 codes in the luma profile through the horizon of KS09" and shows as a hard horizon line.

Fix (keeps "monotone, no dip" for the part between AbyssNear and AbyssGlow):
```
e ≥ 0:  as specified (smoothstep from HazeHorizon, so zero slope at e = 0)
e < 0:  c = lerp(HazeHorizon, A(e), smootherstep(0, 6°, −e)),   A(e) = lerp(AbyssNear, AbyssGlow, smoothstep(0°, 90°, −e))
```
- The result is C1 at the horizon, which becomes the brightest line: the city-glow band.
- Over 6° at 4K vFOV 68 (167 px) the 8.6-code luma change is 0.05 code/px. That is dither-smooth.
- Linear luma: HazeHorizon 0.0561, AbyssNear 0.0428, AbyssGlow 0.0478. The haze 6° below the horizon is the darkest point of the lower sky, by 3 codes. That is physically plausible (the nadir glow is the lit abyss) and is not a seam.

### 2.5 Night-city atmospheric colour

- **Clouds amplify urban skyglow.** Kyba et al. 2011 (PLoS ONE): overcast skies were **10.1×** brighter than clear skies inside Berlin and 2.8× brighter 32 km out. City overcast nights were 4.1× brighter than clear rural moonlit nights.
  - This justifies a cloud deck that is the brightest large surface in the frame (CloudBase luma 64 > HazeHorizon 62 > the abyss 57).
  - The colour is the colour of the emitters: sodium skies are orange and LED skies whiter. Nocturne's magenta is the neon mix and is artistically licensed.
- **Cloud-base glow kernel.**
  - A source at height y_s lights a horizontal cloud base at height H = 460 − y_s with irradiance ∝ cos³/H², which gives the profile `(1 + r²/H²)^(−3/2)`. Its FWHM is 1.53 H: about 700 m for street level and about 400 m for sources at y 200.
  - The spec's Gaussians (σ 220 m, FWHM 518 m) are a fair stand-in with lighter tails.
  - If critics find the hot spots too "blobby", switch to the Lorentzian-cubed form with H_eff 260-460 m. It costs the same and spreads with a natural long tail.
- **Fog colour = `NocSkyColor(ray direction)`** for every fragment, and the dome evaluates the same function. That is exact agreement by construction.
  - Physically, the airlight in a city haze is the haze lit from below. Its asymptote in a given direction *is* the sky colour in that direction (an infinitely deep fog), so the rule is physical. The one exception, the cloud (an object), is handled by the cloud mix of H3.
- **Do not use URP's fog.** URP's EXP2 fog is per-vertex `density·z` in **clip/view depth**, passed as `real`/`half`, with `exp2(−f²)` (`URP/ShaderLibrary/ShaderVariablesFunctions.hlsl:326-353, 366-384`).
  - View depth makes fog change when the camera rotates: objects at the frame edge get less fog than at the centre.
  - Per-vertex fog on Gulf faces with large triangles gives triangle-shaped errors, because exp(−τ²) is very non-linear.
  - Half precision cannot hold 12 km.
  - `NocFog` must use **per-pixel radial distance** `length(posWS − cameraPos)` in float.

### 2.6 `NocAtmosphere.hlsl` fog excerpt

```hlsl
float4 _NocFogA;     // x sigma_beat (Rule P), y alley extra (sigma_a - sigma_r, 0 outside s<124), z sheet strength s_w (0.5|0), w unused
float4 _NocSheetY;   // xyz sheet heights y_k incl. drift (+-15 m, 120 s), w unused
static const float kSheetW = 45.0, kSqrtPiHalfW = 0.88622692545 * 45.0;    // w sqrt(pi)/2

float NocS01(float t) { t = saturate(t); return t * t * (3.0 - 2.0 * t); }
float NocDown(float y) { return 1.0 + 1.8 * NocS01(-y / 1400.0); }
float NocErf(float x)                                  // Abramowitz-Stegun 7.1.26, |err| < 1.5e-7
{   float s = sign(x); x = abs(x); float t = 1.0 / (1.0 + 0.3275911 * x);
    float p = ((((1.061405429 * t - 1.453152027) * t + 1.421413741) * t - 0.284496736) * t + 0.254829592) * t;
    return s * (1.0 - p * exp(-x * x)); }
float NocSheetMean(float y0, float y1, float yk)
{   float dy = y1 - y0;
    if (abs(dy) < 1.0) { float m = (0.5 * (y0 + y1) - yk) / kSheetW; return exp(-m * m); }
    return kSqrtPiHalfW * (NocErf((y1 - yk) / kSheetW) - NocErf((y0 - yk) / kSheetW)) / dy; }
float NocCapQ(float y) { float t = (y - 18.0) / 12.0; return t <= 0.0 ? 0.0 : (t >= 1.0 ? 12.0 * (t - 0.5) : 12.0 * (t*t*t - 0.5*t*t*t*t)); }
float NocCapMean(float y0, float y1)
{   float dy = y1 - y0; return abs(dy) < 0.01 ? 1.0 - NocS01((y0 - 18.0) / 12.0) : 1.0 - (NocCapQ(y1) - NocCapQ(y0)) / dy; }

float NocOpticalDepth(float3 posWS)
{
    float3 v = posWS - _WorldSpaceCameraPos; float d = length(v);
    float y0 = _WorldSpaceCameraPos.y, y1 = posWS.y;
    float tau = _NocFogA.x * NocDown(y1) * d;                                   // exact (2.1)
    [branch] if (_NocFogA.z > 0.0 && min(y0, y1) < -60.0)
    {   float s = NocDown(_NocSheetY.x) * NocSheetMean(y0, y1, _NocSheetY.x)
                + NocDown(_NocSheetY.y) * NocSheetMean(y0, y1, _NocSheetY.y)
                + NocDown(_NocSheetY.z) * NocSheetMean(y0, y1, _NocSheetY.z);
        tau += _NocFogA.x * _NocFogA.z * s * d; }                               // monotone sheets (2.2)
    [branch] if (_NocFogA.y > 0.0) tau += _NocFogA.y * NocCapMean(y0, y1) * d;    // alley haze (2.3)
    return tau;
}
float3 NocFog(float3 col, float3 posWS)
{   float tau = NocOpticalDepth(posWS); float T = exp(-tau * tau);
    return col * T + NocSkyColor(normalize(posWS - _WorldSpaceCameraPos)) * (1.0 - T); }
```

**Fog tests.**
- T-FOG: the sheets-off values of 14.2.2 within 0.002; sheet-on values per §2.2; monotonicity along rays; KS02 cloud T ≥ 0.9 and tower flank T ≥ 0.9.
- Horizon CIE76 ΔE ≤ 2 for fog, sky and dome at 12 elevations (H3).
- KS09 horizon luma profile: no step ≥ 2 codes (needs §2.4).
- C# double-precision mirror of `NocOpticalDepth` against numeric quadrature of σ_p (1e-4).

---

## 3. Banding under video encoding (H1, Part 11.12)

### 3.1 What the earlier games settled (do not redo)

- **Thread R6.**
  - One display-referred dither, last, inside ThreadGrade.
  - 64-bit HDR: R11G11B10 has steps up to 2.6 sRGB LSB in blue.
  - A 64² void-and-cluster rank tile (σ 1.5) with the monotonic TPDF remap (URP's own form). Thread's implementation is `ThreadGrade.shader:151-160`.
  - Noise added in sRGB code space and converted back with the exact functions. Same n for R, G and B (monochrome).
  - A ½-LSB edge fade keeps 0 and 255 exact.
  - R2 integer tile offsets; `-freeze-dither` gives a static tile.
  - Exact ffmpeg flags: `scale=out_color_matrix=bt709:out_range=tv:flags=accurate_rnd+full_chroma_int,format=yuv420p` (swscale otherwise defaults to BT.601).
  - Pitfalls: LDR grading clamps, FXAA/FSR forcing FinalPost after the dither, render scale ≠ 1.
  - The literal plateau metric is blind for slopes < 0.06 code/px. Add a residual metric (RMS ≤ 0.10, p-p ≤ 0.60 code against a 129-px Savitzky-Golay fit).
- **Abyss** (DEVELOPMENT.md "Encode test and dither decision").
  - At about 1.1 Mbps near-static 1080p, every 1-2 LSB dither variant was flattened into 1-level plateaus by x264 and VP9. Runs were 108-424 px, yet no step was visible at 100 % or 200 %; blocks showed only at contrast ×8.
  - **Static 2.0 LSB** survived best: CRF 20 run 108 against 240 temporal. It also fixed source-frame banding (8 px against 14 px).
  - Decision: static 2.0 TPDF by default, temporal via `-dither temporal`. The gate was redefined to visible steps.
- **Thread encode summary** (`QA/reports/encode_summary.md`).
  - In clean gradient regions no codec added a step (0.00 steps/1000 after decoding). The encoders discarded 1-2 LSB noise at 130-540 kbit/s.
  - Raising the dither to 1.5 or 2.0 did not change that.
  - Recommendation: upload at 1440p or 4K.
- **Nocturne H1 already adopts these.** Static by default; amplitude `2.0 × lerp(1.0, 0.6, saturate((Y′ − 0.35)/0.4))` codes peak, where Y′ is the **display luma** of the pre-dither sRGB colour (Rec.709 weights on the encoded values, 0-1); ladder 2.5.
  - Rationale for more amplitude in the dark: the limited-range Y′ quantiser is 219/255 = 0.86 of an RGB code. Encoders spend fewer bits on flat dark blocks. A dark code step is a larger luminance ratio (4.4 % per code at code 40, against 1.5 % at code 150).

### 3.2 New: Nocturne's gradients in YCbCr, and why chroma matters here

BT.709 limited range (sim). The ΔE00 columns give the visual size of a step of 1 code in each component, holding the other two fixed.

| Token | Y′ | Cb | Cr | ΔE00 of 1 Y′ code | ΔE00 of 1 Cb code | ΔE00 of 1 Cr code |
|---|---|---|---|---|---|---|
| NightZenith #0A0C2A | 27.8 | 141.4 | 125.9 | 0.35 | **0.92** | 0.68 |
| NightMid #1C1244 | 36.4 | 149.0 | 130.4 | 0.34 | 0.58 | 0.51 |
| HazeHorizon #5B2F78 | 68.9 | 155.6 | 144.4 | 0.36 | 0.49 | 0.46 |
| CloudBase #7A2A72 | 71.1 | 151.6 | 160.2 | 0.35 | 0.52 | 0.40 |
| AbyssNear #4A2A70 | 62.3 | 155.5 | 139.2 | 0.36 | 0.49 | 0.48 |
| AbyssGlow #5A2C5E | 65.3 | 145.3 | 146.2 | 0.36 | 0.63 | 0.50 |
| Floor #0B0820 | 24.9 | 138.2 | 128.4 | 0.37 | **1.08** | 0.81 |

Pixels per code step at 4K, vFOV 66 (sim; f = 1 663 px/rad):

| Gradient | Y′ | Cb | Cr | R, G, B (sRGB) |
|---|---|---|---|---|
| sky 0-35° (HazeHorizon → NightMid) | 31 | 152 | 73 | 16, 35, 20 |
| sky 35-90° (NightMid → NightZenith) | 186 | 211 | 358 | 89, 266, 61 (H1's figures) |
| abyss 0 → −90° (AbyssNear → AbyssGlow) | **861** | **256** | **376** | 163, 1 306, 145 |

Findings:
1. In Nocturne's dark violets **a chroma code is the larger visual step**. 1 Cb or Cr code is ΔE00 0.46-0.92 (up to 1.08 at Floor), against 0.34-0.37 for 1 Y′ code.
   - Denes, Ash, Fang and Mantiuk (HVEI 2019) report that chromatic banding visibility follows ΔE2000 closely.
   - Netflix's CAMBI is luma-only, but explicitly "although it has been shown that chromatic banding exists". The industry metric would miss this.
2. **The abyss is a hue gradient at almost constant luma.** Y′ changes 3 codes over 90°; Cb 10 and Cr 7. Its contours are chroma contours, 2.3-3.4× denser than luma contours.
3. **Monochrome dither does not dither chroma.** Adding the same n to R′, G′ and B′ leaves `B′ − Y′` and `R′ − Y′` unchanged, because the Rec.709 coefficients sum to 1. The only chroma noise is the per-channel rounding residue: 0.18-0.20 Cb/Cr code RMS after 4:2:0 (sim), against 0.41 for a ±1 TPDF.
4. **Blue noise per pixel cannot dither chroma anyway.** 4:2:0 averages chroma over 2×2 pixels, and blue noise has almost no energy at that scale: the variance of 4×4 block means is 0.00091 against 0.0052 for white noise (Thread R6). A chroma dither must therefore be **constant per 2×2 block and blue across blocks**.

### 3.3 In-memory encode experiment (4K, x264 medium, BT.709 limited, yuv420p; enc)

**Setup.**
- Frames 3 840 × 2 160, 24 frames at 60 fps.
- Left half: the abyss gradient (AbyssNear → AbyssGlow at the real slope, 2 612 px per 90°). Right half: sky 35-90° (1 596 px).
- Variants:
  - `none`
  - `mono2`: the H1 dither, static void-and-cluster TPDF ±2
  - `mono2+chroma1`: plus ±1 Cb and ±1 Cr code TPDF on 2×2 blocks (§3.4)
- Motion: static, and a 1 px/frame pan (gradient moves, dither static).

**Metric: the H1 gate.** Planes Y′/Cb/Cr; 64-px strips (32 chroma samples); 9-sample box; steps ≥ 1.0 code between flat plateaus ≥ 24 luma px. The table shows steps per 1 000 px (max step in codes); the gate is ≤ 2 and none > 1.5.

| Motion / variant / stage | Mbps | Abyss Y′ / Cb / Cr | Sky Y′ / Cb / Cr | Residual RMS abyss Y′ / Cb / Cr |
|---|---|---|---|---|
| static none, 4:2:0 only | - | 1.9 / 3.3 / 3.3 | 2.8 / 1.4 / 1.4 | 0.06 / 0.10 / 0.09 |
| static none, CRF 23 | 0.19 | 1.9 / 3.3 / 3.3 | 2.8 / 1.9 / 0.9 | 0.06 / 0.10 / 0.09 |
| static mono2, 4:2:0 only | - | 0 / 0 / 0 | 0 / 0 / 0 | 0.77 / 0.18 / 0.20 |
| static mono2, CRF 23 | 0.30 | 0.9 / **2.3** / 0.9 | 1.9 / 1.9 / 1.4 | 0.06 / 0.11 / 0.10 |
| static mono2, CRF 30 | 0.20 | 0 / **2.3** / **2.3** | 1.9 / 0.5 / 0 | 0.11 / 0.11 / 0.09 |
| static mono2+chroma1, CRF 23 | 0.51 | **0 / 0 / 0** | 1.4 / 0.5 / 0 | 0.07 / 0.16 / 0.14 |
| static mono2+chroma1, CRF 30 | 0.29 | 0 / 0 / 0.5 | 0.5 / 0 / 0 | 0.22 / 0.24 / 0.17 |
| pan mono2, CRF 23 | 0.38 | 0.5 / 1.4 / 0 | 0.5 / 1.9 / 0.9 | 0.06 / 0.11 / 0.14 |
| pan mono2, CRF 30 | 0.21 | 0.5 / 1.9 / **2.3** | 0.5 / 0.5 / 0.5 | 0.03 / 0.11 / 0.09 |
| pan mono2+chroma1, CRF 23 | 0.70 | 0.5 / 0.5 / 0 | 0.5 / 0 / 0 | 0.10 / 0.16 / 0.16 |
| pan mono2+chroma1, CRF 30 | 0.28 | 0 / 0 / 1.4 | 0 / 0 / 0 | 0.21 / 0.21 / 0.11 |

Reading:
- (a) Before encoding, the H1 dither already passes all three planes: the rounding residue is enough for the plateau gate.
- (b) x264 removes the dither: residual 0.06-0.11 code at 0.2-0.4 Mbps. Chroma plateaus return in the abyss, at 2.3 per 1 000 px in Cb or Cr, which fails the H1 gate ≤ 2.
- (c) The 2×2 chroma dither keeps the CRF 23 encode at ≤ 1.4 per 1 000 everywhere, and CRF 30 at ≤ 1.4. It costs about 0.2-0.3 Mbps more on a pure gradient.
- (d) No step anywhere exceeded 1.1 codes.

Caveats:
- One synthetic run, the last frame only, and counts of 1-5 steps per 2 160 px. Treat this as directional and confirm it with the real Part 11.12 run on KS10 and KS16.
- CRF on static gradients runs at absurdly low rates: **0.2-0.7 Mbps at 4K60** here, and 130-540 kbps at 1080p in Thread. That is far below YouTube's 4K ladder (§3.5), so the CRF gate is a stress test, not a YouTube simulation.

### 3.4 Recommendation: a 2×2-block chroma dither inside ThreadGrade (still "one dither")

It is part of the same final quantisation step. It adds a chroma component to the one TPDF and needs no second pass.

Exact RGB directions with ΔY′ = 0 for BT.709 (in sRGB codes per one limited-range chroma code):
- **Cr: (R, G, B) = (+1.7928, −0.5329, 0)**, from 1.5748·255/224 and −0.2126·1.5748/0.7152·255/224.
- **Cb: (R, G, B) = (0, −0.2132, +2.1124)**.

```hlsl
// ThreadGrade FragGrade step 6, replacing the monochrome dither (Nocturne delta)
float3 s = ThGradeLinearToSrgb(c);                                   // display sRGB 0..1
float  Yd = dot(s, float3(0.2126, 0.7152, 0.0722));                  // display luma Y'
float  ampY = 2.0 * lerp(1.0, 0.6, saturate((Yd - 0.35) / 0.4));     // H1 (codes, TPDF peak)
float  nY = ThGradeTpdf(input.positionCS.xy) * ampY;
uint2  q  = uint2(input.positionCS.xy) >> 1;                         // 2x2 chroma block of the 4:2:0 grid (even origin)
float  nCb = NocTpdfTile(_NocChromaNoiseA, q + uint2(_ThGradeP2.xy)) * _NocChromaAmp;   // +-1.0 code peak
float  nCr = NocTpdfTile(_NocChromaNoiseB, q + uint2(_ThGradeP2.xy)) * _NocChromaAmp;   // separate 64^2 tiles (seeds 2, 3)
float  cb = (s.b - Yd) / 1.8556, cr = (s.r - Yd) / 1.5748;           // normalised chroma, -0.5..0.5
float  wC = smoothstep(0.02, 0.06, length(float2(cb, cr)));          // palette atmosphere tokens >= 0.06; greys get none
float3 dRGB = nY.xxx + wC * (nCr * float3(1.7928, -0.5329, 0.0) + nCb * float3(0.0, -0.2132, 2.1124));
float3 edge = saturate(min(s, 1.0 - s) * 510.0);                     // R6 edge fade, per channel
s = saturate(s + dRGB * edge * (1.0 / 255.0));
return float4(ThGradeSrgbToLinear(s), 1.0);
```

- **Static**, with the same R2 offset policy as luma. `-dither temporal` moves both, and `-dither off` removes both.
- **Amplitude** `_NocChromaAmp = 1.0`, with a ladder step to 1.5.
- **Visibility.** The chroma noise is ±1 code at a 2-px period at 4K, about 15 cycles/degree at 1.5 picture heights, where chromatic contrast sensitivity is near zero. At 1080p it is about 7.5 cpd. Check KS10 and KS16 crops at 200 % for colour grain. If grain is visible at 1080p only, scale `_NocChromaAmp` by `saturate(targetHeight/2160)·0.5 + 0.5`.
- **Siting.** swscale and x264 use MPEG-2 "left" 4:2:0 siting: horizontally co-sited (taps ¼ ½ ¼) and vertically interstitial (average of 2 rows). A 2-px block therefore keeps about 75 % horizontally and 100 % vertically, which is enough (residual 0.45 code before encoding, sim). Keep capture crops at even offsets.
- **Effect on other metrics.** The RGB BandScore and mean-luma tests are unaffected: ΔY′ = 0 by construction, and strip averaging removes the noise. Add a check that the per-pixel mean of Y′ over 64² blocks moves ≤ 0.05 code between chroma dither on and off.

### 3.5 YouTube ladder and upload

**Official upload recommendations** (support.google.com/youtube/answer/1722171, read 2026-11-13).
- H.264 High, CABAC, 2 consecutive B-frames, closed GOP of half the frame rate, VBR, 4:2:0, BT.709. YouTube converts full range to limited.
- SDR bitrates: 2160p60 53-68 Mbps; 1440p60 24; 1080p60 12.
- Colour primaries that need high bit depth without an HDR transfer are converted to BT.709 8-bit "to avoid banding".

**What YouTube serves.** No official ladder is published.
- One `yt-dlp -F` listing (r/youtubedl, April 2025, one video): VP9 2160p60 (itag 315) **17.4 Mbps**, 1440p60 (308) **4.4 Mbps**, 1080p60 (303) **1.5 Mbps**.
- Treat this as one sample. Rates are content-adaptive.
- The 4K rendition gets about 11× the bits of 1080p, and the 1440p rendition about 3×.
- 1080p60 at about 1.5 Mbps is the same regime as Thread's 130-540 kbps CRF tests per gradient area, where dither is discarded.

**Recommendations.**
1. **Upload a 2160p60 master even when the shots are composed at 1080p.**
   - Use x264 CRF ≤ 14 (or `-preset slow -crf 12`), 4:2:0, BT.709-tagged, with the exact swscale flags.
   - Optionally a 10-bit 4:2:0 master: the 8-bit dithered RGB maps into 10-bit YCbCr without a second rounding, and our noise then dithers YouTube's own 8-bit quantiser.
   - 1440p and 2160p uploads get VP9/AV1 renditions.
2. **Add a rate-capped encode to the Part 11.12 report** (logged, not gated). It mimics YouTube's VBR, where rain and detail compete with the gradients for bits, unlike CRF:
```
ffmpeg -hide_banner -y -framerate 60 -i "cap/%05d.png" -vf "scale=out_color_matrix=bt709:out_range=tv:flags=accurate_rnd+full_chroma_int,format=yuv420p" -c:v libvpx-vp9 -b:v 17M -maxrate 25M -bufsize 34M -deadline good -cpu-used 2 -row-mt 1 -pass 1 -an -f null NUL
ffmpeg ... same ... -pass 2 -colorspace bt709 -color_primaries bt709 -color_trc bt709 -color_range tv "enc/shot_vp9_17M.webm"
```
   - Also run 1440p at 4.4 Mbps.
   - libsvtav1, libaom-av1 and libx265 are also present in the local ffmpeg 9.0.2 build, if an AV1 run is wanted.
3. **Optional metric.** The local ffmpeg has the `libvmaf` filter and accepted `feature=name=cambi`. CAMBI (Netflix, luma-only, a 2×2 anti-dither low-pass, 65×65 windows at 4K) is an independent banding check for the decoded clips. Log it next to the H1 gate.

### 3.6 Pitfalls (banding)

1. Measuring only luma: the H1 gate already includes Cb and Cr; keep it. Nocturne's abyss contours are chroma.
2. Per-pixel chroma noise, or independent per-channel R, G, B blue noise: per-pixel blue noise is averaged out by 4:2:0, and per-channel noise is mostly luma. An independent-channel variant was run only with a faulty first version of the step metric, so it is unmeasured here.
3. Odd crop offsets in the encode test shift the 2×2 grid and halve the chroma dither.
4. `scale` without `out_color_matrix=bt709` uses BT.601, which shifts the hue. Without `accurate_rnd+full_chroma_int` it darkens about 1 level (Thread R6, Abyss).
5. Contrast or saturation changes in ThreadGrade after the dither. The dither must stay last; the saturation matrix of 14.2.4 goes before the tone curve.
6. Rain contrast in the dark costs bits that the gradients then lack under VBR (§1.4). Keep rain gated.
7. Temporal dither under rate caps: P/B frames drop it first (Thread R6 hypothesis, Abyss measured static better). Keep static unless the rate-capped run says otherwise.

### 3.7 Tests (banding)

- The H1 gate on Y′, Cb and Cr at 4K CRF 23 (and the logged CRF 30, VP9 17M and 1440p 4.4M runs) for KS04 sky, KS10 abyss, KS16 wall and KS17 plaza. Run A/B with `_NocChromaAmp` 0 and 1.
- Crops at 100/200 % of the worst strip, plus a contrast ×8 diagnostic.
- Source 8-bit captures: 0 steps per 1 000 in all three planes after `ffmpeg ... format=yuv420p` with no codec. This checks the conversion step itself.
- A colour-grain check of chroma dither on vs off at KS10, 1080p and 4K, 200 % crops. The critic lens (b) judges it.

---

## 4. Sources (all accessed 2026-11-13)

| Source | Used for |
|---|---|
| Local URP `Library/PackageCache/com.unity.render-pipelines.universal@8457e85b8184` (package.json 17.6.0): `Runtime/UniversalRendererRenderGraph.cs:1737,1781`; `Runtime/RenderingUtils.cs:334-357`; `Shaders/Utils/CopyDepthPass.hlsl:35-55`; `Runtime/Passes/ScriptableRenderPass.cs:148-158`; `Runtime/FrameData/UniversalResourceData.cs:126-142`; `ShaderLibrary/ShaderVariablesFunctions.hlsl:326-384` | MSAA depth binding, farthest-sample copy, events, resource setters, URP fog form |
| Local Core `com.unity.render-pipelines.core@2d66c71e606e`: `Runtime/RenderGraph/Compiler/NativePassCompiler.cs:1538-1600`; `ShaderLibrary/API/D3D11.hlsl:149`; `ShaderLibrary/TextureXR.hlsl:90-95`; `ShaderLibrary/Common.hlsl:1206`; `ShaderLibrary/Filtering.hlsl:111` | store/resolve rules, MSAA load macros, LinearEyeDepth, bicubic sampling |
| Thread `Assets/_Game/Shaders/ThreadGrade.shader:151-160, 227-234`; `docs/research/R6_banding_dither.md`; `QA/reports/encode_summary.md`; Abyss `DEVELOPMENT.md` "Encode test and dither decision" | existing dither, earlier encode findings |
| Nocturne `docs/research/R3_urp_rendergraph_instancing.md` §1 | NocRain Render Graph plumbing (approach A, SV_Depth), costs of the extra resolve |
| https://advances.realtimerendering.com/s2006/Chapter3-Artist-Directable_Real-Time_Rain_Rendering_in_City_Environments.pdf (Tatarchuk, SIGGRAPH 2006 course 26 ch. 3; text extracted locally) | composite multi-layer rain pass, drop shading (Garg-Nayar summary), "faint in bright regions", milk trick, script/sky-visibility rain switch |
| https://diglib.eg.org/items/b4d11d22-3e3d-4ac5-b33c-6b091143add1/full (Tatarchuk & Isidoro, NPH 2006) | citation of the paper version |
| https://history.siggraph.org/?p=113590 (Garg & Nayar 2006); https://www.cs.columbia.edu/CAVE/publications/pdfs/Garg_TR04.pdf | streak appearance model, motion-blurred streak photometry |
| https://www.fxguide.com/?p=42356 ; https://discourse.panda3d.org/t/rain-and-snow/14782 ; https://gamedev.net/forums/topic/654661-methods-for-drawing-rain/5149579/?page=2 ; https://history.siggraph.org/?p=93028 (Wang & Wade) (search summaries) | camera-attached cone/cylinder rain and its look-up/look-down failure modes |
| https://www.jcgt.org/published/0009/03/02/paper.pdf and https://www.reedbeta.com/blog/hash-functions-for-gpu-rendering (Jarzynski & Olano, JCGT 9(3) 2020; search summaries; the pcg3d body above is from memory, so check its constants against the paper) | drop hashing |
| https://support.google.com/youtube/answer/1722171 | upload codec, chroma, BT.709, SDR/HDR bitrate tables |
| https://reddit.birdcat.cafe/r/youtubedl/comments/1jxqec5/why_doesnt_youtubedlp_grab_the_largest_video (search snippet only; page returned 403) | one observed VP9 ladder: 315 17 423k, 308 4 409k, 303 1 533k |
| https://gist.github.com/MartinEesmaa/2f4b261cb90a47e9c41ba115a011a4aa | itag to codec/resolution mapping |
| https://arxiv.org/pdf/2102.00079 (Tandon et al., CAMBI, Netflix 2021; text extracted locally) | luma-only assumption, "chromatic banding exists", 2×2 anti-dither LPF, 65×65 window |
| https://www.cl.cam.ac.uk/~gd355/publications/hvei19_paper.pdf (Denes, Ash, Fang, Mantiuk 2019; text extracted locally) | chromatic banding model, agreement with ΔE2000 |
| https://doaj.org/article/76138bef7cc1440fa360ea5f9ecb6957 (Kyba et al. 2011, PLoS ONE 6(3) e17307) | overcast amplification of urban skyglow (10.1× in Berlin, 2.8× at 32 km) |
| https://mailman.videolan.org/pipermail/x264-devel/2011-July/008663.html | x264 chroma QP offset behaviour (automatic +6 only in 4:4:4 with psy; no automatic offset in 4:2:0) |
| Local `ffmpeg -h encoder=libx264`, `ffmpeg -encoders`, `ffmpeg -filters` (9.0.2 gyan full build) | `-chromaoffset` default 0, available encoders (x264, x265, VP9, SVT-AV1, aom, rav1e), libvmaf with CAMBI |
| Abramowitz & Stegun 7.1.26 (verified numerically here, max error 1.39e-7) | erf approximation for the sheet integral |
