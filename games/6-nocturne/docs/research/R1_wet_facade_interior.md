# R1: Wet floor mirror (H4), facade filtering (H2 NocFacade), interior mapping (NocSection)

Researcher report for Nocturne (game 6). Scope: 14.2.5 (NocWet, NocFacade, NocSection), 14.7 H2 and H4, 14.5.3.
Unity 6000.6.0f1, URP 17.6.0, Direct3D 11, MSAA 4x, HDR 64-bit.

Path aliases used below (both `package.json` files say `"version": "17.6.0"`; the cache folders are named by hash, not `@17.6.0`):
- `URP` = `Library/PackageCache/com.unity.render-pipelines.universal@8457e85b8184`
- `CORE` = `Library/PackageCache/com.unity.render-pipelines.core@2d66c71e606e`

Tags: **[src]** means verified in the package source (file:line). **[paper]** means verified in the text of the cited paper. **[computed]** means checked numerically for this report (the scripts are in the session scratchpad and are not project files). **[derived]** means my own algebra, checked by hand.

---

## 0. Summary

1. **The H4 lobe as written in the spec gives round blobs, not streaks.** `exp(-(1 - dot(r, s))/sigma2)` is a Phong lobe around the reflection vector. On a floor seen at grazing angles it produces round blobs, or blobs wider than they are tall. It does not produce the long vertical streaks of wet streets. [computed]: for a source 40 m ahead, the 1/e region is 187 x 173 px (w x h) with the spec lobe and 33 x 173 px with a lobe that respects the half vector. Recommendation: keep the spec's `sigma2` as the **in-plane** variance and narrow the **lateral** variance by `(n.v)^2`. This is the anisotropic spherical-Gaussian warp of a microfacet NDF (Wang 2009 / Xu 2013, via MJP), and it is the same cost.
2. **Energy normalisation.** Model every H4 source as a small area emitter: a segment of glow half-width `rho` with effective radiance `L_eff <= 3`. Its reflected radiance is `F * G1 * L_eff * mass`. Here `mass` is the Gaussian measure of the source's angular footprint under the lobe, in closed form (one `exp`, two `erf`). The term conserves energy and reduces to a perfect mirror (`F * L_eff`, never more) when the lobe is narrower than the source. So no source can make a firefly brighter than its own emitter. The spec's fixed `R_i` in `1/(1 + (d/R_i)^2)` turns out to be `R_i^2 = A_i/(2 pi sigma_x sigma_y)`, which depends on roughness and on the view. Do not use a fixed `R_i`.
3. **Karis 2013** [paper] supplies the representative point for spheres (eq. 11) and for segments (eq. 19), and the normalisations `(alpha/alpha')^2` (eq. 14) and `alpha/alpha'` (eq. 20). Karis himself reports an abrupt end-point jump when the reflection ray points away from a line light. The closed-form Gaussian mass below replaces his normalisation and, together with clipping the segment to the r-hemisphere, removes that jump.
4. **Specular anti-aliasing.** Add the pixel-footprint covariance of `r`, `(ddx r)(ddx r)^T + (ddy r)(ddy r)^T`, scaled by the pixel-filter variance. That is the Kaplanyan 2016 / Tokuyoshi 2021 method [paper], expressed directly in the reflection space of our analytic lobe. Clamp it at KAPPA, keep a lateral floor of 0.75 px, and band-limit the ripples by footprint while moving the lost slope variance into the lobe (Toksvig-equivalent; The Ring's water rule).
5. **NocWet must not take specular from the Forward+ point lights** (`specularHighlightsOff = true`, `URP/ShaderLibrary/Lighting.hlsl:48-96`). Lamps and the goal window enter H4 as sources. The 7-argument overload at `Lighting.hlsl:131-139` is buggy: it passes the view vector as the normal. The goal streak source must be the **1.2 x 1.5 m window**. The 0.2 m lamp sphere reflects only about 0.013 linear [computed], which is invisible.
6. **Fog of reflections.** Multiply reflected wall and source radiance by `exp(-sigma^2 t (t + 2 d))`, toward `NocSkyColor(r)`. Combined with the floor's own fog `exp(-sigma^2 d^2)`, the total is exactly `exp(-sigma^2 (t + d)^2)`, the fog of the mirror image. Exp-squared fog is not multiplicative, so fogging the two legs separately under-fogs reflections.
7. **NocFacade.** Inside the 2.5-5.5 px fade band, evaluating one hash at the pixel centre is wrong: the footprint straddles two cells, so window states flip as the camera moves and shimmer. Use the exact box filter over the 2 x 2 cells under the footprint (valid while the footprint is under 0.4 cell, which the fade guarantees). Blend per axis (bilinear in `w_u`, `w_v`) so that window rows survive at grazing angles. Add one coarser hierarchical level, "zones", so the city keeps patchy floor-lighting structure out to 6 km with no shimmer. The mean stays 0.34 and toggling falls to 0.28 %/s [computed].
8. **The H2 footprint test needs a linear-light metric.** A perfectly energy-conserving fade from a window grid to its mean changes the mean **8-bit gamma luma by 21-37 %** [computed] (Jensen's inequality on the sRGB curve), while the linear-light means match within 0.1 %. Run the "w forced to 0 and 1, within 3 %" test on linearised pixels. The spec's Fresnel value at 30 m is **0.717**, not 0.74 (0.397 at 10 m is correct) [computed].
9. **Interior mapping.** Use the van Dongen 2008 regular-plane ray cast [paper] in the branch-free reciprocal form, with a soft-min AA of the plane boundaries. There are three regimes: full rooms while the slab pitch is at least about 9 px (up to about 370 m at 1080p, 740 m at 4K), a box-filtered grid of per-bay mean colours, and the flat macro below 4 px (the fade ends at 2.5 px). The spec's parallax acceptance (1.8-2.7 px at 300 m) checks out [computed].

---

## 1. Already settled by the earlier games (reuse; do not re-research)

| Item | Where | Use in Nocturne |
|---|---|---|
| pcg3d / pcg4d (Jarzynski & Olano, JCGT 2020), `U01(h) = (h >> 8) / 2^24`, no `sin` hashes, integer cells | The Ring `docs/research/07_rendering_techniques.md` §3; Thread `R7_art_science.md` §4.1 (with C# mirror) | window, zone, epoch, room and ripple hashes |
| Box-filtered grid lines, checker, fade to the **mean** between N px (Quilez, Apodaca & Gritz) | The Ring 07 §2.1-2.2; Abyss `docs/research/art-technique.md` (numerically checked) | strata, slab lines, LED grid |
| Band-limited cosine / octave fade `1 - smoothstep(0.25, 0.5, f * fw)` | The Ring 07 §2.3; Thread R7 §4.3 (fade toward the octave **mean**) | puddle noise, sky cloud octaves in the reflection |
| SDF edge coverage, exact box overlap for thin lines `cov = saturate((min(x + fw/2, h) - max(x - fw/2, -h))/fw)` with energy x (true/drawn) | The Ring 07 §2.4; Thread `R8_lines_ui_input_settings.md` | vertical edge lights (min 2 px) |
| Water ripples: per-wave footprint attenuation and lost slope variance into roughness (`roughness^2 += 2 var`) | The Ring 07 §8 | NocWet ripples (§2.6 here) |
| Core spec-AA helpers `NormalFiltering`, `GeometricNormalVariance`, `ProjectedSpaceNormalFiltering`, `TextureNormalVariance` | `CORE/ShaderLibrary/CommonMaterial.hlsl:183-190, 192-202, 209-215, 224-228, 241-261` [src] | lit Noc materials (walls, steel) |
| Facade window identity derived from the world position, so tiers coincide | Abyss `docs/SHADERS.md` §8 | tier identity of NocFacade (§3.6) |
| `fwidth` is undefined in divergent branches; compute footprints for every pixel before any branch | Thread R7 §6.3 item 5 | all three shaders |

---

## 2. Wet surfaces without SSR (14.7 H4, NocWet)

### 2.1 Spec arithmetic check

- Fresnel `F = refl (0.03 + 0.97 (1 - n.v)^5)` on the plane y 0, eye at 1.7 m [computed]:
  - At 10 m: n.v = 0.1676, giving 0.4177, and x 0.95 = **0.397**. This matches the spec's 0.40.
  - At 30 m: n.v = 0.0566, giving 0.7549, and x 0.95 = **0.717**. The spec says 0.74. Correct the number or the test.
- KS17 streak position: the mirror point lies at `1.7/(1.7 + y_src)` of the horizontal distance. That is 0.2464 for y 5.2 (the point light) and 0.248 for the window centre at 5.15. The spec's 0.246 is right, and it holds for any lobe shape below, because the peak always sits in the specular direction.
- "roughness 0.08 / 0.35" (14.2.5): read it as GGX alpha (URP `BRDFData.roughness`, `= perceptualRoughness^2`, `URP/ShaderLibrary/BRDF.hlsl:59` [src]). For URP-side terms that means smoothness 0.717 in puddles and 0.408 elsewhere. The H4 lobe uses the spec's own `sigma2 = alpha^2 + 0.004`, giving 0.0104 in puddles and 0.1265 elsewhere.
- **ALU budget.** "About 90 ALU per floor pixel" cannot hold with 32 sources. The recommended source term costs about 60 ALU per source, so about 2 k ALU at 32 sources. A floor covering 30 % of a 4K frame (2.5 Mpx) then costs about 5e9 ALU per frame, roughly 0.2-0.4 ms on an RTX 5070 [derived]. That is affordable. The spec's fallback (12 sources) stays as the cheap tier.

### 2.2 Finding: the reflection-vector lobe cannot make streaks

The spec's term `exp(-(1 - dot(r, s))/sigma2)` is an isotropic Gaussian around the mirror direction `r`, which makes it a Phong lobe. Microfacet (half-vector) lobes stretch at grazing angles. Karis lists "stretched highlights" as a defining feature of the microfacet model [paper, p. 14]. Wang et al. 2009 showed that the NDF lobe, warped into reflection space, has its own axis at `r` with the following sharpnesses (Xu et al. 2013, anisotropic SG warp, as summarised by MJP):

- `lambda_x = lambda/(8 (n.v)^2)` across the plane of incidence, along `X = normalize(cross(n, r))`
- `lambda_y = lambda/8` in the plane of incidence
- `lambda = 2/alpha^2`

In per-axis Gaussian variances (`exp(-delta^2/(2 sigma^2))`) [derived]:

```
sigma_y^2 = 2 alpha^2            (in-plane: unchanged with the view angle)
sigma_x^2 = 2 alpha^2 (n.v)^2    (lateral: narrows at grazing view)
```

The spec's `sigma2` is the variance at normal incidence (`1 - cos t ~ t^2/2`). So keep **`Syy = sigma2`** and set **`Sxx = sigma2 (n.v)^2`**.

Simulation [computed] at 1080p, vFOV 66 (f = 831.6 px), flat floor, camera level. The table gives the 1/e region of the lobe on screen, width x height in pixels:

| Source | sigma2 | Spec lobe (r-space, isotropic) | Half-vector lobe `exp(-4(1 - n.h)/sigma2)` | ASG warp (recommended) |
|---|---|---|---|---|
| sign, y 40, 130 m ahead | 0.0104 | 243 x 254 (h/w 1.05) | 77 x 254 (3.3) | 81 x 255 (3.2) |
| lamp, y 5, 40 m ahead | 0.0104 | 187 x 173 (0.93) | 33 x 173 (5.2) | 39 x 174 (4.5) |
| sign, y 40, 130 m | 0.1265 | 801 x 518 (0.65) | 341 x 518 (1.5) | 415 x 518 (1.25) |
| lamp, y 5, 40 m | 0.1265 | 765 x 496 (0.65) | 187 x 496 (2.65) | 273 x 496 (1.8) |

The peak position is identical in all three columns (the specular direction). Only the shape changes. **Decision for the lead:** adopt the anisotropic form. It is still "the spec lobe" along the streak, and the cost is one cross product per pixel, not per source.

### 2.3 Energy normalisation: the Gaussian mass of the source footprint

Notation: lobe frame `(X, Y, r)`, with `X = normalize(cross(n, r))` and `Y = cross(r, X)`. A source direction is mapped to gnomonic coordinates `p = (dot(q, X), dot(q, Y)) / dot(q, r)`, where `q = Q - P` is the vector to a source point. Gnomonic projection maps great circles to straight lines, so **a straight 3D segment stays a straight 2D segment**. Near `r`, `p` equals the angular offset.

The lobe is the normalised 2D Gaussian `N(p; 0, S)`, where `S = [[Sxx, Sxy], [Sxy, Syy]]` (§2.2 plus the anti-aliasing term of §2.5). For a source of radiance `L` covering the angular region `A`:

```
L_r = F * G1 * L * mass,    mass = integral over A of N(p; 0, S) dp       (0 <= mass <= 1)
```

Properties [derived]:
- **Mirror limit.** When S tends to 0 and r lies inside the source, mass = 1, so L_r = F G1 L. A source can never reflect brighter than itself, so a small, bright source cannot produce a firefly.
- **Point limit.** For a small source, mass = Omega N(delta; S). The integral over outgoing directions is `F G1 L Omega`, which is energy-conserving for every roughness and every n.v.
- This is the Gaussian analogue of Karis's normalisations. Eq. 14, `(alpha/alpha')^2`, is the widened 2D normalisation for spheres. Eq. 20, `alpha/alpha'`, is the widened 1D normalisation for tubes [paper p. 16-18]. Here both follow from convolving two Gaussians, where the variances add.

**Closed forms.** A source is a disc or a segment. Its angular glow width is `w = 2 rho / d`. The width enters as a **peak-matched** variance, so the mirror limit equals 1 exactly. A variance-matched Gaussian would give a peak 1.38 times too high.

- **Disc** (lamp head, koi spine point): radius `w/2`, `Omega = pi w^2/4`, peak-matched variance `w^2/8` added to `Sxx` and `Syy`:
  `mass = Omega exp(-q/2) / (2 pi sqrt(det S'))`, with `q = p0^T S'^-1 p0`.
- **Segment** (blade, vertical strip, edge light, window column): from `p0` to `p1`, length `l`, unit direction `t`, width variance `w^2/(2 pi)` added. The line integral of the Gaussian [derived] is:

  ```
  a = t^T S'^-1 t,  b = t^T S'^-1 p0,  m = p0 - (b/a) t,  qp = m^T S'^-1 m   (no c - b^2/a cancellation)
  mass = w * exp(-qp/2) * sqrt(pi/(2a)) * [erf((a l + b)/sqrt(2a)) - erf(b/sqrt(2a))] / (2 pi sqrt(det S'))
  ```

  Checked against brute-force integration of the box source [computed]:

  | Case | Closed form | Exact |
  |---|---|---|
  | narrow lateral lobe | 0.840 | 0.947 |
  | medium | 0.369 | 0.379 |
  | rough | 0.340 | 0.353 |
  | mirror limit | 0.988 | 1.000 |

  The error is at most 11 % low and never high.
- **erf**: Winitzki 2008 with a = 0.147, `erf(x) ~ sign(x) sqrt(1 - exp(-x^2 (4/pi + a x^2)/(1 + a x^2)))`. Maximum absolute error is 1.24e-4 [computed]; the published maximum relative error is about 1.3e-4.

**What the spec's `S_i exp(...)/(1 + (d_i/R_i)^2)` becomes:** amplitude = `L * Omega/(2 pi sqrt(det S))`. For a source of area A at distance d this is `L * A/(A + 2 pi sqrt(det S) d^2)`, so `R_i^2 = A_i/(2 pi sqrt(Sxx Syy))`. With the spec's isotropic lobe, a 14 x 200 m blade gives R = 207 m in a puddle and 59 m elsewhere. A fixed `R_i` would therefore make puddle and non-puddle energy inconsistent.

**Source parameters (the generator writes them).** Each source has:
- ends `P0` and `P1` (equal for a disc)
- glow half-width `rho`
- `L_eff` (linear HDR) x LightCal x crossfade weight

Its integrated "intensity" is `I = L_eff x area`. Rule: **`L_eff <= 3.0`** (the Neutral tone-mapping limit of 14.2.1). Use the **real emitter radiance and size** for signs, blades and the screen wall. For lamps, give the emitter a glow-sized proxy: pick `rho` so that `L_eff = I/(pi rho^2) <= 3`. For example, a lamp that should produce a streak peak of about 1.0 at 30 m needs I of about 44, so rho of about 2.2 m. This single knob tunes the streak strength without fireflies.

**Goal window (beat 7, "H4 source weight 1.0").** Make the source the 1.2 x 1.5 m window: a vertical segment from y 4.4 to 5.9 at x 1156, z 387.9, rho 0.6 m, `L_eff` equal to the window's mean radiance (about 1.5-2.5). Do **not** use the 0.2 m lamp sphere. Numbers [computed]:
- At KS19 (8 m away), the mirror point is close, n.v = 0.65, F = 0.033, so the window streak is about 0.027 linear. The lamp sphere alone gives 0.013.
- At KS17 (212 m), n.v = 0.033, F = 0.81, G1 = 0.59, and the window streak peak is about 0.03 linear over a floor of about 0.02. That is roughly +23 levels of 8-bit luma (39 to 63), a visible warm line that is long vertically.

### 2.4 Representative point and segment clipping (Karis 2013)

[paper, p. 15-18]:
- **Sphere, eq. 11:** `centerToRay = L - (L.r) r`, `closestPoint = L + centerToRay * saturate(sourceRadius/|centerToRay|)`.
- **Segment, eq. 19:** `t = ((r.L0)(r.Ld) - (L0.Ld)) / (|Ld|^2 - (r.Ld)^2)`, with `Ld = L1 - L0`, then clamped with `saturate`.
- **Normalisations:** eq. 10 `alpha' = saturate(alpha + sourceRadius/(3 distance))`, eq. 14 sphere `(alpha/alpha')^2`, eq. 20 line `alpha/alpha'`.
- **Known artefact:** Karis states that eqs. 18-19 treat r as a line rather than a ray, so "This can cause an abrupt change from one end point to the other even for perfectly flat surfaces" [paper, p. 17].

In Nocturne, use eq. 19 only to get the representative **distance** `d_q`. That distance feeds the angular width `w = 2 rho/d_q` and the fog. Then clip the segment to the half-space `dot(Q - P, r) >= 0.1 d_q`. Points more than 84 degrees off `r` contribute nothing, and the clip is linear along the segment. After that, evaluate the closed-form mass. The clip plus the Gaussian line integral gives smooth ends and no jump.

Test (T-H4-CONT): sweep r past both ends and behind the source. The maximum step between adjacent samples must be below 1 % of the peak.

### 2.5 Specular anti-aliasing: Toksvig, geometric, footprint

Literature [paper]:
- **Toksvig (NVIDIA 2004/2005):** `f_t = |N_a| / (|N_a| + s (1 - |N_a|))` widens a Blinn-Phong exponent by the shortening of the averaged normal. Core's `TextureNormalVariance` (`CommonMaterial.hlsl:241-261` [src]) is the vMF version of it. It needs mipmapped normal **textures**. Nocturne has none (the ripples are analytic), so the analogous tool is the analytic variance transfer below.
- **Kaplanyan et al. 2016 (HPG):** filter the NDF with the pixel footprint of the half vector in slope space. Pixel-filter variance 0.25, axis-aligned rectangle kernel, bandwidth clamped at `kappa = 0.18`.
- **Tokuyoshi & Kaplanyan 2021 (JCGT 10(2)):** projected-space filtering. Listing 5, the isotropic version for deferred or forward rendering:
  `kernelRoughness2 = 2 SIGMA2 (|ddx n|^2 + |ddy n|^2)`, `SIGMA2 = 0.15915494` (= 1/(2 pi), Appendix B), `KAPPA = 0.18`, `filteredRoughness2 = saturate(roughness2 + min(kernelRoughness2, KAPPA))`.
  Unity Core ships this as `ProjectedSpaceNormalFiltering` (`CommonMaterial.hlsl:192-202` [src]) and the older form as `GeometricNormalVariance` and `NormalFiltering` (`:209-215`, `:183-190`).

**For the analytic mirror (recommended):** filter the lobe itself. Our lobe is a 2D Gaussian in `(X, Y)`, and the pixel spreads `r` linearly, so the convolution is exact [derived]:

```
ja = (dot(ddx(r), X), dot(ddx(r), Y)),  jb = (dot(ddy(r), X), dot(ddy(r), Y))
S_AA = PIXVAR * (ja ja^T + jb jb^T)     PIXVAR = 0.25 (Kaplanyan; more conservative than 1/(2 pi) for video)
trace(S_AA) <= 0.36                       (KAPPA 0.18 mapped to our sigma2 = 2 alpha^2 convention)
Sxx >= (0.75 * pixelAngle)^2              (lateral floor: a streak is never thinner than about 1.5 px at 1/e)
```

`ddx(r)` contains both the flat-mirror term (about one pixel angle) and every residual ripple slope. Compute it once per pixel, **before** the source loop and before any branch.

**Ripple band-limit and variance transfer** (The Ring 07 §8, applied here):
- Fade the ripple slope by `fade = smoothstep(4, 8, lambda/fw_m)` (pixels per wavelength).
- Add the removed variance: `sigma2 += 4 v_n` with `v_n = 0.12^2 (1 - fade^2) C_RIPPLE rho_rain`. The factor 4 is because r tilts twice as much as n; `2 v_n` in alpha^2 equals `4 v_n` in sigma2.
- `C_RIPPLE` is the mean squared normalised ring slope per axis. Measure it once in EditMode from the HLSL mirror; an estimate is 0.05.

**Fireflies and stability checklist** (each item names its failure mode):
1. No point-light GGX specular on NocWet (§2.8). A point source at alpha 0.08 has `D = 1/(pi alpha^2) = 50`, which is the classic puddle sparkle.
2. Every source is an area source with `L_eff <= 3`, so the mass is at most 1 and the reflection is never brighter than its emitter.
3. The `sigma2` floor of 0.004 (the spec) plus `S_AA` plus the lateral 0.75 px floor.
4. Keep positions, `t`, `p` and the dot products in `float`, not `half`. Use `0.5 * dot(r - s, r - s)` for `1 - dot(r, s)` (cancellation-free) wherever it appears.
5. `r.y = max(r.y, 0.01)` after the ripple tilt, so the floor never reflects below the horizon at grazing angles.
6. Sources change only through 12 m crossfades and breathe at no more than 0.2 Hz. The static test (`-rain 0 -freeze-clock`) therefore yields identical frames, because the ripples scale with `rho_rain = 0`.
7. Guard every `normalize` and division. Wrap the final colour in `ClampToFloat16Max` (`CORE/ShaderLibrary/Common.hlsl:1698` [src]). Keep the spec's specular clamp of 8 as a NaN/INF safety net only: by construction the sum is at most about 3 x F.

### 2.6 Ripples and puddles

Physics [derived]: capillary-gravity waves have `c^2 = g lambda/(2 pi) + 2 pi gamma/(rho lambda)`. The minimum is **0.231 m/s at lambda = 1.71 cm** (gamma 0.0728 N/m), so real rain rings expand at about 0.23 m/s with 1-2 cm wavelengths.

Visibility (1080p, eye 1.7 m, 4 px per wavelength) [computed]:

| lambda | resolved across the view up to | resolved along the depth up to |
|---|---|---|
| 2 cm | 4.2 m | 2.7 m |
| 4 cm | 8.3 m | 3.8 m |
| 6 cm | 12.5 m | 4.6 m |

So ripples are a near-field effect only, and beyond that they become roughness (§2.5).

Recommended parameters (art-exaggerated):
- cell 0.6 m, centres hashed inside the central half of the cell
- ring speed 0.23 m/s, start radius 0.03 m, life 0.9 s
- lambda 4 cm, envelope `exp(-1.5 x^2)`, `x = (r - R)/lambda`, decay `(1 - age/life)^2`
- spawn period per cell `1.1/max(rho_rain, 0.05)` s with a hashed phase
- slope scale 0.12 (spec)

The maximum reach from a centre is about 0.30 m, at most half a cell, so the **2 x 2 cells around the nearest corner** cover every ring that can touch the point (4 hashes). Ripples apply only inside puddles. Disable them beyond `fade = 0` with a branch taken **after** the derivatives.

Puddle mask (35 %):
- fBm of value noise at 4.0 m and 1.3 m (rotated octaves)
- threshold at the 65th percentile of the noise, measured in EditMode like Thread's 0.569
- edge `smoothstep(th - e, th + e, n)` with `e = 0.04 + fwidth(n)` (Thread R7 §4.5)
- fade to the mean 0.35 by `smoothstep(3, 6, 1.3 m / fw_m)`, using the anisotropic footprint `fw_m = max(length(ddx(xz)), length(ddy(xz)))`

Blend `sigma2` linearly in variance. Reflectivity is `lerp(0.5, 0.95, m)`, so the far mean is 0.6575.

### 2.7 Sky term, wall planes and mirror-consistent fog

- **(a) Sky.** Use `NocSkyColor(r)`. The cloud noise (900 m and 300 m octaves on the plane at 458 m) is seen in reflection at grazing elevations (for example 2.6 km away at e = 10 degrees, where 300 m subtends 0.11 rad). Give the sky function a blur argument: `NocSkyColorBlur(dir, blurRad)` fades octave k by `1 - smoothstep(0.25, 0.5, blurRad * hitDist / lambda_k)` toward its mean. Use `blurRad = sqrt(Syy)`.
- **(b) Walls.** Treat them as **finite rectangles**, not infinite planes. Alley example:
  - west wall x -2.2, z <= 120, y <= 22
  - east wall x 2.2, z <= 120, y <= 31
  - "mouth backdrop" plane z 270 (the boulevard's north facade, y <= 560)
  - dead-end wall z -2.5

  Take the first hit in t. Soften every rectangle edge by the lobe footprint on the wall, `soft = t * sqrt(Syy) + 1e-3` m: coverage `saturate(0.5 + dist_to_edge/(2 soft))`. Without this, the wall-to-sky boundary in the floor is a hard line that crawls under the ripples. Evaluate `NocFacadeMacro(H, soft)` with transition widths of at least `soft`. At t = 75 m and sigma = 0.1 rad, soft is 7.5 m, so windows (2.4 m) are never resolved in a reflection and only the strata, floors and wave functions are.
- **Mirror-consistent fog** [derived]. The floor fragment is fogged over d (eye to floor) by `NocFog`. A reflected wall or source at distance t beyond the floor is pre-fogged by
  `T_r = exp(-sigma_eff^2 t (t + 2 d))`, toward `NocSkyColor(r)`,
  so that `T_r * exp(-sigma^2 d^2) = exp(-sigma^2 (t + d)^2)`, which is the transmittance of the virtual image. Use the beat sigma with height factor 1 (all walls and sources here have y >= 0).

### 2.8 Integration with the lit template (The Ring `RingLitCore` / `RingLighting`)

- `RingLighting.hlsl:56-73` calls `LightingPhysicallyBased(brdfData, noClearCoat, light, n, v, 0.0h, false)` for every light. NocWet passes **`true`** (the 9-argument form at `URP/ShaderLibrary/Lighting.hlsl:48-96` [src]). The Forward+ lights then light the floor **diffusely only**, and all floor specular comes from H4. This avoids double counting and point-light sparkle.
- **Do not use** the overload at `Lighting.hlsl:131-139`: it calls `LightingPhysicallyBased(brdfData, light, viewDirectionWS, specularHighlightsOff, specularHighlightsOff)` and so passes the view direction as the normal [src].
- URP's direct specular has no 1/pi (`BRDF.hlsl:182-254`, the D times approximate V*F form [src]). H4 avoids the question entirely because it works in radiance: `L_r = F G1 L_eff mass`, in the same units as the emission buffer.
- Floor composition: `col = albedo * (ambient + lights_diffuse) * (1 - F) + F * G1 * (env_or_wall + sum_sources)`, then `NocFog`, then `NocFloor`. `G1 = G_MaskingSmithGGX(n.v, alpha)` from `CORE/ShaderLibrary/BSDF.hlsl:211-221` [src], with `alpha = sqrt(0.5 sigma2)`. For example G1 is 0.77 for a puddle and 0.36 for rough floor at 30 m [computed].
- Source upload: global arrays `_NocSrcP0`, `_NocSrcP1`, `_NocSrcL` of 32 `float4`, plus a float count. Write them with `Shader.SetGlobalVectorArray`, always with 32 entries: the array size is fixed by the first set. Rename these if they collide with any material property name (Part 3.2 item 5).

### 2.9 HLSL sketch (`NocMirror.hlsl`)

```hlsl
// Include after Core.hlsl, CORE BSDF.hlsl (G_MaskingSmithGGX), NocAtmosphere.hlsl.
#define NOC_SRC_MAX 32
float4 _NocSrcP0[NOC_SRC_MAX];  // xyz end 0 (world m), w = glow half-width rho (m)
float4 _NocSrcP1[NOC_SRC_MAX];  // xyz end 1 (= end 0 for a disc)
float4 _NocSrcL[NOC_SRC_MAX];   // rgb = L_eff * LightCal * crossfade weight (linear, L_eff <= 3)
float  _NocSrcCount, _NocFogSigma, _NocPixelAngle;   // pixelAngle = 2 tan(vfov/2) / screenHeight

static const float NOC_PIXVAR = 0.25, NOC_AA_CLAMP = 0.36, NOC_LAT_MIN_PX = 0.75;
static const float NOC_INV_2PI = 0.15915494;

float NocErf(float x)   // Winitzki 2008, a = 0.147, |err| < 1.3e-4
{
    float x2 = x * x, ax2 = 0.147 * x2;
    return sign(x) * sqrt(1.0 - exp(-x2 * (1.2732395 + ax2) / (1.0 + ax2)));
}

struct NocLobe { float3 r, X, Y; float3 S; float FG; };   // S = (Sxx, Syy, Sxy)

NocLobe NocMirrorLobe(float3 P, float3 eye, float3 n, float sigma2, float reflectivity)
{
    NocLobe o;
    float3 v = normalize(P - eye);                              // eye -> floor
    float3 r = reflect(v, n);
    r.y = max(r.y, 0.01);  o.r = normalize(r);                  // never below the horizon
    float nv = saturate(dot(n, -v));
    float3 X = cross(n, o.r); float lx = length(X);
    o.X = (lx > 1e-4) ? X / lx : normalize(cross(float3(0, 1, 0), v + float3(0, 0, 1e-3)));
    o.Y = cross(o.r, o.X);
    // pixel footprint of r (derivatives here, before any loop or branch)
    float3 drx = ddx(o.r), dry = ddy(o.r);
    float2 ja = float2(dot(drx, o.X), dot(drx, o.Y)), jb = float2(dot(dry, o.X), dot(dry, o.Y));
    float3 Saa = NOC_PIXVAR * float3(ja.x*ja.x + jb.x*jb.x, ja.y*ja.y + jb.y*jb.y, ja.x*ja.y + jb.x*jb.y);
    Saa *= min(1.0, NOC_AA_CLAMP / max(Saa.x + Saa.y, 1e-12));
    float latMin = Sq(NOC_LAT_MIN_PX * _NocPixelAngle);
    o.S = float3(max(sigma2 * nv * nv + Saa.x, latMin), sigma2 + Saa.y, Saa.z);   // ASG warp + AA
    float F = reflectivity * (0.03 + 0.97 * PositivePow(1.0 - nv, 5.0));
    o.FG = F * G_MaskingSmithGGX(max(nv, 1e-3), sqrt(0.5 * sigma2));
    return o;
}

float NocSegmentMass(float2 p0, float2 p1, float3 S, float w)
{
    float2 dlt = p1 - p0; float len = length(dlt);
    bool disc = len < 1e-3 * w;
    float sw = disc ? 0.125 * w * w : NOC_INV_2PI * w * w;      // peak-matched width variance
    float sxx = S.x + sw, syy = S.y + sw, sxy = S.z;
    float det = max(sxx * syy - sxy * sxy, 1e-20);
    float3 Si = float3(syy, sxx, -sxy) / det;
    float norm = NOC_INV_2PI * rsqrt(det);
    if (disc) {
        float q = p0.x*p0.x*Si.x + p0.y*p0.y*Si.y + 2.0*p0.x*p0.y*Si.z;
        return saturate(0.78539816 * w * w * norm * exp(-0.5 * q));
    }
    float2 t = dlt / len;
    float a = t.x*t.x*Si.x + t.y*t.y*Si.y + 2.0*t.x*t.y*Si.z;
    float b = t.x*p0.x*Si.x + t.y*p0.y*Si.y + (t.x*p0.y + t.y*p0.x)*Si.z;
    float2 m = p0 - (b / a) * t;
    float qp = m.x*m.x*Si.x + m.y*m.y*Si.y + 2.0*m.x*m.y*Si.z;
    float k = rsqrt(2.0 * a);
    float line = exp(-0.5 * qp) * sqrt(1.5707963 / a) * (NocErf((a * len + b) * k) - NocErf(b * k));
    return saturate(w * line * norm);
}

float3 NocMirrorSources(float3 P, float dEye, NocLobe o)
{
    float3 sum = 0;
    uint count = (uint)_NocSrcCount;
    [loop] for (uint i = 0; i < count; i++)
    {
        float3 A = _NocSrcP0[i].xyz - P, B = _NocSrcP1[i].xyz - P, Ld = B - A;
        float rLd = dot(o.r, Ld);
        float tq = saturate((dot(o.r, A) * rLd - dot(A, Ld)) / max(dot(Ld, Ld) - rLd * rLd, 1e-6)); // Karis eq. 19
        float dq = max(length(A + tq * Ld), 1e-2);
        float la = dot(A, o.r), lb = dot(B, o.r), lc = 0.1 * dq;            // clip to the r hemisphere
        if (max(la, lb) <= lc) continue;
        float3 Ac = (la < lc) ? lerp(A, B, (lc - la) / (lb - la)) : A;
        float3 Bc = (lb < lc) ? lerp(B, A, (lc - lb) / (la - lb)) : B;
        float2 p0 = float2(dot(Ac, o.X), dot(Ac, o.Y)) / dot(Ac, o.r);       // gnomonic
        float2 p1 = float2(dot(Bc, o.X), dot(Bc, o.Y)) / dot(Bc, o.r);
        float mass = NocSegmentMass(p0, p1, o.S, 2.0 * _NocSrcP0[i].w / dq);
        float Tr = exp(-Sq(_NocFogSigma) * dq * (dq + 2.0 * dEye));          // mirror-consistent fog
        sum += _NocSrcL[i].rgb * (mass * Tr);
    }
    return o.FG * sum;
}
```

Implementation notes:
- The sky and wall term (`NocMirrorEnv`, §2.7) is multiplied by the same `o.FG`.
- Write a C# double-precision mirror (`NocMirrorMath`) for the EditMode tests.
- `Sq` and `PositivePow` come from Core `Common.hlsl`.

### 2.10 Tests for H4

- **T-H4-ENERGY** (EditMode, mirror). Numerically integrate `L_r` over outgoing directions for a small disc. It must equal `F G1 L Omega` within 3 % for sigma2 in {0.004, 0.0104, 0.1265} and n.v in {1, 0.2, 0.05}. For a large source with a narrow lobe, the peak must be at most `F G1 L_eff` (mirror limit).
- **T-H4-STREAK.** For the sign and lamp configurations of §2.2, the 1/e region must have h/w of at least 2.5 at sigma2 0.0104.
- **T-H4-KS17.** At the KS17 pose, the floor maximum of the goal-window source along the eye-to-window line lies at 0.246 +- 10 % of the distance (spec).
- **T-H4-CONT.** End-point and behind-ray sweeps (§2.4): adjacent-sample step below 1 % of the peak.
- In the player:
  - the 14.10 shimmer pan on the floor crops of KS00, KS04 and KS17 with `-rain 0`
  - a static run of 60 frames
  - an isolated-pixel scan (above 8 x the 5 x 5 median) equal to 0 on floor masks
  - the encode test with rain on (ripples are moving, fine-scale content)

---

## 3. Procedural facade filtering (14.7 H2, NocFacade)

### 3.1 Footprints and distances

Use per-axis metres per pixel, `fw = float2(length(float2(ddx(u), ddy(u))), length(float2(ddx(v), ddy(v))))`. This is Golus's choice: `fwidth` is up to sqrt(2) too wide on diagonals, and his final grid shader computes `uvDeriv` from `length(ddx, ddy)` and fades to the line coverage as the derivative grows (`lerp(grid, targetWidth, saturate(uvDeriv * 2 - 1))`).

On planar faces u and v are linear, so the derivatives over a 2 x 2 quad are exact.

**Do not** declare pattern coordinates `centroid` under MSAA: centroid interpolation makes the derivatives inaccurate. Use centroid only for normals (Thread R7 §6.2). Clamp u to the face's `[uMin, uMax]` before taking cells. A pixel centre outside a sliver of triangle extrapolates u, which would otherwise light a window past the corner.

Pitch distances (1080p, frontal; double them at 4K) [computed]:

| Pitch | 8 px | 5.5 px | 2.5 px |
|---|---|---|---|
| column 2.4 m | 249 m | 363 m | 798 m |
| floor 3.6 m | 374 m | 544 m | 1 197 m |
| zone 19.2 m | 1 996 m | 2 903 m | 6 386 m |

These agree with the spec's "5 px at 399 m".

### 3.2 Exact box filter of a random window field (2 x 2 cells)

The spec's `coverage = lerp(0.55, shapeMask, w_u w_v)` with one hash per pixel is not exact inside the fade band. At 4 px pitch the footprint is 0.25 cell = 0.6 m, wider than the 0.4 m mullion between windows. A pixel centred on a mullion overlaps two windows that can have different states. A single hash assigns both to one cell, and the value flips when the centre crosses the boundary. That is shimmer, and it is exactly the band where the eye can still see windows.

The fix: evaluate the 2 x 2 cells under the footprint, each weighted by its own exact 1D coverage. For a window interval `[x0, x1]` the coverage is `cov = saturate((min(u + fw/2, x1) - max(u - fw/2, x0))/fw)`. While `w > 0` the footprint stays under 0.4 cell, so two cells per axis always suffice and the result is the exact box filter [derived].

The fade must also be **separable** (bilinear in `w_u` and `w_v`). With only the product `w_u w_v`, a facade seen along a street (columns unresolved, floors resolved) collapses to a flat mean. Real oblique facades show floors as streaky bands of light.

```
R11 = sum over i, j of cov_u(i) cov_v(j) E(i, j)        (both axes resolved, 2 x 2 hashes)
Rrow = sum over j of cov_v(j) * m_u * Ebar               (rows resolved, columns averaged)
Rcol = sum over i of cov_u(i) * m_v * Ebar               (columns resolved, rows averaged)
M    = m_u * m_v * Ebar
value = w_u w_v R11 + (1 - w_u) w_v Rrow + w_u (1 - w_v) Rcol + (1 - w_u)(1 - w_v) M
w_u = smoothstep(2.5, 5.5, P_u/fw.x),  w_v likewise
m_u = window width / P_u,  m_v = window height / P_v     (exact means of the shape; e.g. 2.0/2.4 x 2.4/3.6 = 0.556)
Ebar = expected window emission at this location (zone level, 3.3)
```

Cost: 4 pcg3d for the states plus 8 pcg4d for the epochs (3.4), about 250 ALU, only where `w_u w_v > 0`. Branch on it **after** computing `fw`; hashes need no derivatives.

### 3.3 Filterable randomness: a zone level

Once windows average out (beyond about 1.2 km at 1080p), the spec leaves a flat stratum mean. Real night skylines keep **patchwork** (whole floors or zones lit). Make the randomness hierarchical so that each level has an analytic expectation:

```
zone (8 columns x 4 floors = 19.2 x 14.4 m residential; per stratum P_zone = 8 P_u x 4 P_v):
    p_zone = 0.34 * stratumFactor * (0.4 + 1.2 U01(hash(zone)))   -> E[p_zone] = 0.34 * stratumFactor
window: lit(e) = U01(pcg4d(i, j, face, e)) < p_zone              -> E[lit | zone] = p_zone exactly
Ebar(zone) = p_zone * meanColour * intensity                      (known analytically; box-filtered over the 2 x 2 zones)
zone fade: w_z = smoothstep(2.5, 5.5, P_zone_px), Ebar -> lerp(Ebar_stratum, Ebar(zone), w_z)
```

[computed]: `E[p_zone] = 0.340`, maximum 0.544; toggle rate `E[2p(1 - p)]/150 = 0.281 %/s` (flat p: 0.299 %/s), so the spec limit of 0.3 %/s holds.

**Graph-paper guard** (Part 3.2 lesson 13): offset each zone row in u by `P_u * floor(8 U01(hash(row)))` so zone boundaries never line up vertically. Draw no zone edges: the zones exist only as probabilities.

### 3.4 Stateless toggling and hue

- Epochs (spec): `x = t/150 + U01(h.z)`, `e = floor(x)`, `lit = lerp(a(e - 1), a(e), smoothstep(0, 1.2/150, frac(x)))`.
- Use **pcg4d** on `(i + 32768, j + 32768, face, e)` for `a(e)`. Jarzynski & Olano warn that linear combinations such as `hash(aX + bY + ...)` greatly reduce quality (The Ring 07 §3).
- Pass `t/150` from C# in double, wrapped modulo 65 536.
- **Warm fraction per beat without hue pops.** Rule P ramps the warm fraction `w_f` over 24 m. A hard per-window threshold would switch each window's hue instantly. Use a band of delta = 0.03:
  `warm = saturate((w_f - 0.5 delta - U01(h.w))/delta + 1) * saturate(w_f/delta)`.
  This is exactly 0 at `w_f = 0`, which beat 7 needs: the goal window must be the only warm blob. The mean is `w_f` for `w_f >= delta`, and each window takes about 2.9 s to change at 2.5 m/s.
- Hue thresholds on `U01(h.y)`: cold < 0.82 <= cyan < 0.90 <= magenta < 0.96 <= white.
- The C# side computes `meanColour = (1 - w_f) sum p_k C_k + w_f WinWarm` from `NocturnePalette`.

### 3.5 Vertical edge lights (min 2 px)

`drawHalf = max(trueHalf, 1.0 * fw.x)`. Use the box-overlap coverage of 3.1 with `drawHalf`, then multiply by `trueHalf/drawHalf`; this is Thread R8 / ThreadLine energy preservation. Optionally use the NocLine energy floor of 0.35.

### 3.6 Identity across tiers (T1 ring 1 and T2 aggregates at 1.8 km)

Use world-derived coordinates, as in Abyss SHADERS §8:
- `u = dot(P.xz, t_face)`, with `t_face` the face tangent quantised to 0.5 degrees of yaw
- `v = y`
- `face id = pcg3d(round(yaw/0.5 deg), round(planeOffset/0.25 m), 0x4E6F63)`

Coplanar pieces of a T2 union footprint then get identical windows and zones. Pass `uMin` and `uMax` per face as UV2 for the corner margin and the clamp. u up to 7 km in float32 has a 0.5 mm step, which is fine for cells of 2.4 m.

### 3.7 The H2 "w forced to 0 and 1" test needs linear light

[computed]: a facade at 2.5 / 4 / 5.5 px pitch, ideal box filter (16 x 16 supersampled), lit 0.34, window emission 0.25 / 0.5 / 1.0 over a wall at 0.01. The linear-light means of the pattern and of the flat mean agree within 0.1 %. Their **8-bit gamma luma** means differ by **-21 % to -37 %**, because the mean of sRGB(x) is less than sRGB of the mean of x.

So the spec test "mean luma within 3 %" fails by construction on gamma codes, even for a perfect fade. **Measure it on linearised pixels:** decode sRGB to linear, then average, then compare within 3 %. Log the gamma figure for information only. Visually, the fade spans a distance ratio of 2.2, so the gamma shift appears as a gentle gradient, never a step.

YouTube and players downscale in gamma space too, so a 4K-resolved grid darkens slightly at 1080p playback. That is expected and not a defect.

### 3.8 HLSL sketch (core of `NocFacade`)

```hlsl
float NocCover(float x, float fw, float x0, float x1) { return saturate((min(x + 0.5*fw, x1) - max(x - 0.5*fw, x0)) / fw); }

// E(i, j): lit (two epochs) x hue x intensity; p from the zone level
float3 NocWindowE(int2 c, uint face, float pZone, float tEpoch /* t/150 wrapped */)
{
    uint3 h = Pcg3d(uint3(asuint(c + 32768), face));
    float x = tEpoch + U01(h.z); float e = floor(x);
    uint4 k = uint4(asuint(c + 32768), face, (uint)e);
    float aNow  = step(U01(Pcg4d(k).x), pZone);
    float aPrev = step(U01(Pcg4d(k - uint4(0, 0, 0, 1)).x), pZone);
    float lit = lerp(aPrev, aNow, smoothstep(0.0, 1.2 / 150.0, x - e));
    return lit * NocWindowHue(h, _NocWarmFraction) * _NocWindowGain;
}

float3 NocWindows(float u, float v, float2 fw, uint face, NocStratum st, float3 Ebar, float pZone)
{
    float2 P = st.pitch, g = st.gap;                       // gap = (pitch - window) / 2 per axis
    float wu = smoothstep(2.5, 5.5, P.x / fw.x), wv = smoothstep(2.5, 5.5, P.y / fw.y);
    float2 m = (P - 2.0 * g) / P;
    float3 M = m.x * m.y * Ebar;
    if (wu + wv <= 0.0) return M;                          // fw computed before this branch
    int2 c0 = int2(floor((float2(u, v) - 0.5 * fw) / P));
    float2 cu = float2(NocCover(u, fw.x, c0.x * P.x + g.x, (c0.x + 1) * P.x - g.x),
                       NocCover(u, fw.x, (c0.x + 1) * P.x + g.x, (c0.x + 2) * P.x - g.x));
    float2 cv = float2(NocCover(v, fw.y, c0.y * P.y + g.y, (c0.y + 1) * P.y - g.y),
                       NocCover(v, fw.y, (c0.y + 1) * P.y + g.y, (c0.y + 2) * P.y - g.y));
    float3 R11 = 0;
    [branch] if (wu * wv > 0.0) {
        R11 = cu.x * (cv.x * NocWindowE(c0, face, pZone, _NocEpochT) + cv.y * NocWindowE(c0 + int2(0, 1), face, pZone, _NocEpochT))
            + cu.y * (cv.x * NocWindowE(c0 + int2(1, 0), face, pZone, _NocEpochT) + cv.y * NocWindowE(c0 + int2(1, 1), face, pZone, _NocEpochT));
    }
    float3 Rrow = (cv.x + cv.y) * m.x * Ebar, Rcol = (cu.x + cu.y) * m.y * Ebar;
    return wu*wv*R11 + (1 - wu)*wv*Rrow + wu*(1 - wv)*Rcol + (1 - wu)*(1 - wv)*M;
}
```

`pZone` and `Ebar` come from the zone level (3.3), which is itself box-filtered over its 2 x 2 zones with the same `NocCover`. Strata, floor lines and edge lights are added the same way.

### 3.9 Tests for NocFacade

- **T-WIN-BOX** (EditMode mirror): the 2 x 2 exact filter against 32 x 32 supersampling, within 1 % for footprints of 0.05-0.4 cell. The one-hash variant must fail the same test, which proves the test can catch the defect.
- **T-WIN-MEAN:** w forced to 0 and to 1 at w = 0.5, **linear-light** mean within 3 % (3.7). Both tiers at 1.8 km (Part 10 item 34), linear.
- **T-WIN-RATE:** at most 0.30 % of windows change per second; each transition takes 1.2 s; a 600 s histogram.
- **T-WIN-WARM:** over the s 1 236-1 284 ramp, no window changes hue faster than 1 % per frame at 60 fps. With w_f = 0, zero warm windows.
- In the player: the 14.10 shimmer pan on the KS04 and KS05 facade crops at 100-400 m, and the static 120 frames.

---

## 4. Interior mapping for NocSection (14.2.5, 14.5.3)

### 4.1 Method and later variants

**van Dongen 2008, "Interior Mapping", CGI 2008** [paper]:
- Ceilings, floors and walls are infinite planes at regular spacing, in object space.
- For each pixel, the ray from the camera through the surface point is intersected with the next plane of each family. For ceilings that plane is `ceil(y/d) * d` looking up and `(ceil(y/d) - 1) * d` looking down. The closest of the three intersections wins and selects the texture.
- Constant time, independent of the number of rooms; works on curved geometry.
- Lights vary per room as "lit if r < c", with c the probability of being lit; room textures vary through an atlas.
- A "furniture plane" parallel to the facade carries silhouettes.
- A seam appears at building corners; hide it with opaque exterior wall at the corners.
- Rooms of varying size stay constant-time if walls move less than one room size, checking two walls.

Later variants:
- **Box form with a reciprocal direction** (Dixon 2025, branch-free): `pos = (frac(uv)*2 - 1, zOffset)`, `distToAxisBorder = abs(1/dir) - pos/dir`, `dist = min(xyz)`; the hit is `pos + dir * dist`. Point-sample the per-room settings, because bilinear sampling of per-room data made rooms flash.
- **Cubemap or atlas interiors:** Insomniac's Spider-Man (2018) popularised them; Gaijin's DagorEngine and Unigine ship nodes. These need textures, which are excluded here (no external assets, procedural only). The same box maths applies.

### 4.2 Bay frame and numbers

- **Face frame:** `T` = horizontal tangent along the face (the ruled Gulf faces bend gently; take `T` per pixel from the geometric normal and up), `B` = +Y, `N` = outward normal. The view ray in the frame is `d = (dot(V, T), V.y, -dot(V, N))`, where `d.z > 0` points into the face.
- **Bay:** 9 m (columns) x 4 m (slabs). Room box = the whole bay. Depth `D = 8 + 4 U01(hash)`.
- **Facade layer in front:** opaque spandrel and slab band 0.9 m, column 0.8 m; the glazing is the rest. Box-filter these masks with `NocCover`.
- **Parallax acceptance** (14.5.3 item 4) [computed]: shift = `b D / (d (d + D)) * f`. For b = 25 m, d = 300 m, f = 831.6: **1.80 px** at D = 8 and **2.67 px** at D = 12. This matches the spec's 1.8-2.7 px.

### 4.3 HLSL sketch (about 30 ALU for the ray cast, about 15 more for AA)

```hlsl
float3 NocInterior(float u, float v, float3 V, float3 T, float3 N, uint face, out float lit)
{
    const float2 BAY = float2(9.0, 4.0);
    float3 d = float3(dot(V, T), V.y, max(-dot(V, N), 1e-3));      // into the face
    float2 cell = floor(float2(u, v) / BAY);
    float2 a = float2(u, v) - (cell + 0.5) * BAY;                   // entry point, room-centred
    uint3 h = Pcg3d(uint3(asuint(int2(cell) + 32768), face));
    float depth = 8.0 + 4.0 * U01(h.x);
    float3 inv = 1.0 / float3(d.x + (d.x >= 0 ? 1e-5 : -1e-5), d.y + (d.y >= 0 ? 1e-5 : -1e-5), d.z);
    float2 tSide = abs(0.5 * BAY * inv.xy) - a * inv.xy;            // van Dongen planes, Dixon form
    float  tBack = depth * inv.z;
    float3 t3 = float3(tSide, tBack);
    float  tMin = min(t3.x, min(t3.y, t3.z));
    // soft-min AA of the room corners: about 1 px transition, per-plane footprint
    float3 fwT = max(float3(fwidth(t3.x), fwidth(t3.y), fwidth(t3.z)), 1e-4);
    float3 w = saturate(1.0 - (t3 - tMin) / fwT);  w /= max(w.x + w.y + w.z, 1e-4);
    float3 Hs = float3(a, 0) + d * t3.x, Hc = float3(a, 0) + d * t3.y, Hb = float3(a, 0) + d * t3.z;
    uint tpl = h.y % 6u;  lit = step(U01(h.z), 0.40);
    return w.x * NocRoomWall(Hs, tpl, lit) + w.y * NocRoomCap(Hc, d.y, tpl, lit) + w.z * NocRoomBack(Hb, depth, tpl, lit);
}
```

`fwidth` of the `t` values is smooth inside a bay. Bay seams sit under the opaque column and slab bands, which are at least 1 px in the interior regime (column 0.8 m is 2.2 px at 300 m). Compute all derivatives for every pixel before the regime branch (4.4).

### 4.4 Filtering regimes ("flat macro below 4 px")

Using `slabPx = 4/fw.y`, `colPx = 9/fw.x`, `s = min(slabPx, colPx * 4/9)`:

| Regime | Condition (1080p distance, frontal) | Content |
|---|---|---|
| Interior | `w_i = smoothstep(5, 9, s)` > 0 (full up to about 370 m; 740 m at 4K) | the ray-cast rooms of 4.3 |
| Bay mean | blend `lerp(bayMean, interior, w_i)` | one flat colour per bay (`lit x templateMean`), box-filtered over the 2 x 2 bays with `NocCover` (exact while the footprint is under 0.4 bay) |
| Macro | `w_b = smoothstep(2.5, 5.5, slabPx)` toward `_NocSectionMacro` (complete by 2.5 px, about 1.33 km; 2.7 km at 4K) | the spec's flat macro = `0.40 litMean + 0.60 darkMean` over the glazing fraction, plus the band albedo |

At 300 m (acceptance 4) the slab pitch is 11.1 px, so `w_i = 1` and the parallax is real. Compute `templateMean` for each of the six templates, and the macro, **on the CPU** from a C# mirror of the room functions: Monte Carlo over bays and view directions within +-45 degrees of the normal. Pass them as uniforms, and check the linear-light match within 3 % (same metric as 3.7).

### 4.5 Six room templates (cold by default; amber only where the beat allows)

1. **Office:** ceiling `SignWhite x 1.0` panel band (at least 1.2 m wide, so no lines under 2 px at 300 m); walls `ConcreteDark` lit by a ceiling gradient; floor dark with a soft brighter band.
2. **Residential cold:** a `WinCold x 0.6` soft radial glow on the back wall (radius 1.5 m), dim ceiling.
3. **Rack room:** 2-3 `NeonCyan x 0.5` vertical bars at least 0.6 m wide on the back wall.
4. **Shop:** a `NeonMagenta x 0.45` wash on the back wall, with floor bounce.
5. **Dark:** the unlit state of every template; the `Floor` token + 0.02 x the city glow (`HazeHorizon`) toward the glazing.
6. **Atrium** (only the sky-lobby rows every 250 m): mist `lerp(mistColour, back, 1 - exp(-t/20))`.

Mechanical bands every 300 m carry amber slits. Gate their amber by the beat's warm uniform, so no amber exists in beat 7. Furniture plane, if wanted: one plane 2 m inside, carrying a dark band (desk height 0.75-1.0 m) against lit rooms only. No people.

Use toggling for the 40 % lit as in 3.4, with epoch 150 s, so rooms never pop. Fog by the face fragment distance; the extra 8-12 m of depth is negligible.

### 4.6 Pitfalls and tests for NocSection

- **Grazing views along the Gulf** (the faces run away for kilometres): the per-axis `s` (4.4) drops the interior where the column pitch is compressed. Guard `d.z` (done) and the near-zero `d.x` and `d.y` (done).
- **Corner seams** (van Dongen §2.4): keep the glazing at least 1 m away from the face ends and the terrace steps.
- **Additive light from interiors must not exceed the window emission budget:** cap lit-room radiance at the window gain x 1.2 (14.2.1 HDR factors).
- **Tests:**
  - **T-INT-PARALLAX:** CPU mirror reproduces 1.80 / 2.67 px.
  - **T-INT-MEAN:** interior vs bay-mean vs macro within 3 % linear light at the regime boundaries.
  - The shimmer pan on the KS09 and KS12 face crops.
  - The static test with the clock frozen.

---

## 5. Sources (all accessed 2026-11-13)

| URL | Used for |
|---|---|
| https://cdn2.unrealengine.com/Resources/files/2013SiggraphPresentationsNotes-26915738.pdf | Karis, "Real Shading in Unreal Engine 4": eqs. 10, 11, 14, 19, 20, stretched highlights, the line/ray end-point artefact (text extracted locally; eq. 10 checked by span positions) |
| https://www.jcgt.org/published/0010/02/02/paper-lowres.pdf | Tokuyoshi & Kaplanyan 2021, projected-space NDF filtering: SIGMA2 = 1/(2 pi), KAPPA 0.18, listings 4-5, Appendix B; their summary of Kaplanyan 2016 (sigma^2 0.25, rectangle kernel, clamp) |
| https://research.nvidia.com/sites/default/files/pubs/2016-06_Filtering-Distributions-of/NDFFiltering.pdf | Kaplanyan, Hill, Patney, Lefohn 2016, "Filtering Distributions of Normals for Shading Antialiasing" (cited; details via the 2021 paper) |
| https://developer.download.nvidia.com/whitepapers/2006/Mipmapping_Normal_Maps.pdf and https://blog.selfshadow.com/page/13 | Toksvig factor `|Na|/(|Na| + s(1 - |Na|))` and its caveats (search summaries) |
| https://therealmjp.github.io/posts/sg-series-part-4-specular-lighting-from-an-sg-light-source/ | SG NDF fit (sharpness 2/m^2), Wang 2009 warp `lambda/(4 |mu.o|)`, ASG warp `lambda_x = lambda/(8 (v.n)^2)`, `lambda_y = lambda/8`, axis frame `X = cross(n, r)` |
| https://www.proun-game.com/Oogst3D/CODING/InteriorMapping/InteriorMapping.pdf | van Dongen 2008, "Interior Mapping" (full text extracted): plane selection, lights per room, atlas, furniture plane, corner seam, varying room sizes |
| http://kristiandixon.com//2025/02/27/InteriorMapping.html | branch-free reciprocal box form, point-sampled room data (bilinear flashed) |
| https://hannomalie.github.io/posts/interior_mapping.html ; https://gaijinentertainment.github.io/DagorEngine/assets/shaders/rendinst_interior_mapping.html ; https://developer.unigine.com/docs/latest/content/materials/graph/node_library/misc/interior_mapping_cubemap | cubemap interior variants (Spider-Man 2018 popularisation, engine nodes), search summaries |
| https://hn.jakechampion.name/reader/https://bgolus.medium.com/the-best-darn-grid-shader-yet-727f9278b9d8 | Golus: `length(ddx, ddy)` per axis instead of fwidth, phone-wire AA, fade to the line coverage `saturate(uvDeriv * 2 - 1)` |
| https://iquilezles.org/articles/bandlimiting/ ; https://iquilezles.org/articles/morecheckerfiltering | band-limiting by footprint; a tent filter is more stable than a box (search summaries; filterableprocedurals returned 404, formulas settled in The Ring 07 §2) |
| https://www.jcgt.org/published/0009/03/02/paper.pdf | Jarzynski & Olano 2020, pcg3d / pcg4d, the warning against linear combinations (settled in The Ring and Thread) |
| https://en.wikipedia.org/wiki/Error_function ; https://ar5iv.arxiv.org/html/1201.1320 | Winitzki erf approximation, a = 0.147, max error about 1.2e-4 (also checked numerically here) |
| `URP/ShaderLibrary/BRDF.hlsl:50-73, 157-165, 182-254` | URP roughness = perceptual^2, normalisation term, environment BRDF, DirectBRDFSpecular (no 1/pi) |
| `URP/ShaderLibrary/Lighting.hlsl:48-96, 131-139` | `specularHighlightsOff`, the buggy 7-argument overload |
| `CORE/ShaderLibrary/CommonMaterial.hlsl:40-48, 183-266` | roughness conversions, NormalFiltering, ProjectedSpaceNormalFiltering (cites Tokuyoshi 2021), GeometricNormalVariance (`screenSpaceVariance <= 0.25`), TextureNormalVariance |
| `CORE/ShaderLibrary/BSDF.hlsl:32-56, 194-273` | F_Schlick, D_GGX, G_MaskingSmithGGX (211-221), V_SmithJointGGX |
| `CORE/ShaderLibrary/Common.hlsl:1698` | ClampToFloat16Max |
| The Ring `docs/research/07_rendering_techniques.md`, Thread `docs/research/R7_art_science.md` and `R8_lines_ui_input_settings.md`, Abyss `docs/SHADERS.md` §7-8, The Ring `Assets/_Game/Shaders/Include/RingLighting.hlsl:26-77` | settled techniques (section 1), the lit-template integration point |
