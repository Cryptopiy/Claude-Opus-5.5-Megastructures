// NocAtmosphere.hlsl - the one source of truth for atmosphere (MASTER_PROMPT 14.2.2, 14.7 H1 / H3, D-006, R2 section 2),
// included by every custom shader. Interface: NocSkyColor(dir), NocSkyColorBlur(dir, blurRad), NocFog(col, posWS),
// NocFloor(col); helpers NocOpticalDepth, NocCloudColor, NocCloudMix. The C# double mirror is NocFogMath (NocSchedule.cs);
// AtmosphereTests checks both against 14.2.2 / H3. Order in every forward pass: shade, NocFog (cloud mix, then fog toward
// NocSkyColor(ray)), NocFloor last. Unity fog is off; no FOG_* keywords anywhere.
// Globals (written once per frame from s by the AtmosphereDriver, never by a material):
//   _NocFog        x = sigma_r (regional sigma_beat, 1/m), y = alley haze extra sigma_a - sigma_r (D-006), z = sheet strength
//                  s_w (0.5, 0 when cut, 14.8 item 9), w = rain rho (the rain pass and the ripples)
//   _NocFogSheets  xyz = haze-sheet heights y_k (m, with the +-15 m / 120 s drift), w = sheet width (45 m)
//   _NocSheetE     xyz = sheet elevations e_k (deg): the centres of the art-directed sky bands of the three sheets (H3: where a ray
//                  crosses a sheet 2.5 km out, -5.7 / -15.6 / -31 undrifted), w = sheet scale (1, 0 when the sheets are cut)
//   _NocAbyss      xy = sheet billow scroll (m, wrapped to NA_SheetScrollPeriod), zw = deep-glow scroll (wrapped to NA_DeepScrollPeriod);
//                  both are added in the rotated frame of every octave (exact wraps)
//   _NocCloud      xy = octave-1 scroll (m, wrapped to 64 x 900 m), zw = octave-2 scroll (wrapped to 64 x 300 m)
//   _NocCloud2     xy = octave-3 scroll (wrapped to 64 x 110 m), zw = octave-4 scroll (wrapped to 64 x 40 m)
//   _NocGlowSeg[8] (ax, az, bx, bz) glow centre segments (m); _NocGlowPar[8] (amplitude, sigma m, ring radius m, tint: 0 CloudHot,
//                  1 CloudCool, 2 CloudBase, 3 = the Gulf shadow multiplier of the base deck);
//   _NocGlowCount  number of active glows (boulevard axis, sign wall, spire, koi ring; beats may feed their own centres)
//   _NocAmber      x = k_a(s) = w_f(s) / 0.32 (LOOK_V2 R6: every city-wide SodiumAmber use x k_a, the remainder to SignWhite;
//                  1 to s 400, 0.625 at s 880, 0 from s 1 250), y = the screen wall's amber-stop weight (1 -> 0 over s 1 230-1 290)
// The camera is at y ~1.7, so everything is a function of direction and distance. Colours are linear PAL_* tokens.
#ifndef NOC_ATMOSPHERE_INCLUDED
#define NOC_ATMOSPHERE_INCLUDED

#include "NocCommon.hlsl"

#define NOC_GLOW_MAX 8

// ------------------------------------------------------------------ AT constants (Phase 4: the abyss of H3 and the deck form)
// C# twin: NocAbyss in NocSchedule.cs (AtmosphereTests.AbyssConstants_HlslMatchesCSharp parses every "#define NA_" line here).
// Base gradient below the horizon: lerp(AbyssNear, AbyssGlow, 1 - (1 - |e| / 90)^1.3) (monotone, flat at the nadir; the glow rises
// faster than v1's smoothstep, so the shallow drop of KS09 / KS10 / KS12 reads 45-58 instead of 40-47: LOOK_V2 8.1 drop 50-66).
// Haze sheets as geometry fog: sigma x s_w x the Gaussian sheet layers (H3; s_w 0.5 -> 1.5 so a face visibly loses contrast under
// each sheet). Sky bands: opacity a = coverage(n) x (A_k band(e - e_k) + top_k(e)), band = exp(-(x / 1.6)^2) above e_k (the far
// edge of the layer), exp(-(x / 5.5)^2) below; top_k = (1 - exp(-K_k / sin|e|)) x V_k x smoothstep over e_k .. e_k - 12 deg (the
// layer seen from above, V_k = exp(-(sigma Down(y_k) t_k)^2) its fog visibility); coverage = smoothstep(0.5 - h, 0.5 + h, n),
// h = max(0.10, 10 g), g = blurM sum(w_o fade_o / L_o) (the threshold filtered by the noise change per pixel: no aliased cloud
// edge at grazing, < 1 code per row through the KS09 horizon), n = a 4-octave billow noise on the sheet plane (1200 / 400 /
// 133.3 / 44.4 m, weights 0.30 / 0.30 / 0.25 / 0.15, each faded to its mean below 6..12 px); composited deep to near over the
// base gradient and the deep glow of the lit city (plane y -2000, octaves 675 / 225 m, +-35 % where e < -10).
#define NA_SheetFogStrength 1.5
#define NA_BandA0 0.75
#define NA_BandA1 0.75
#define NA_BandA2 0.75
#define NA_BandUp 1.6
#define NA_BandDn 5.5
#define NA_TopK0 0.5
#define NA_TopK1 0.5
#define NA_TopK2 0.5
#define NA_TopRamp 12.0
#define NA_CovHalf 0.10
#define NA_CovAA 10.0
#define NA_SheetFadePx0 6.0
#define NA_SheetFadePx1 12.0
#define NA_SheetMaxA 0.95
#define NA_SheetDetail 0.10
#define NA_SheetThick 0.30
#define NA_SheetL0 1200.0
#define NA_SheetL1 400.0
#define NA_SheetL2 133.333333
#define NA_SheetL3 44.444444
#define NA_SheetW0 0.30
#define NA_SheetW1 0.30
#define NA_SheetW2 0.25
#define NA_SheetW3 0.15
#define NA_SheetCol0R 0.046
#define NA_SheetCol0G 0.039
#define NA_SheetCol0B 0.032
#define NA_SheetCol1R 0.060
#define NA_SheetCol1G 0.048
#define NA_SheetCol1B 0.037
#define NA_SheetCol2R 0.090
#define NA_SheetCol2G 0.066
#define NA_SheetCol2B 0.044
#define NA_AbyssExp 1.3
#define NA_DeepY -2000.0
#define NA_DeepAmp 0.35
#define NA_DeepL0 675.0
#define NA_DeepL1 225.0
#define NA_DeepW0 0.55
#define NA_DeepW1 0.45
#define NA_DeepE0 10.0
#define NA_DeepE1 70.0
#define NA_SheetScrollPeriod 76800.0
#define NA_DeepScrollPeriod 43200.0
// Deck form (INT-11): f = x / sqrt(1 + x^2), x = 4.5 (n - 0.5) (never flat, no plateau), base x (1 + 0.5 f), tint x (1 + 0.5 f):
// +-22 % at one std of n (v1 +-13 / +-11 %), and the cloud mix of geometry starts 70 m x f higher or lower (tops dissolve into billows).
#define NA_DeckK 4.5
#define NA_DeckBaseAmp 0.5
#define NA_DeckTintAmp 0.5
#define NA_CloudWisp 70.0

float4 _NocFog;
float4 _NocFogSheets;
float4 _NocSheetE;
float4 _NocAbyss;
float4 _NocCloud;
float4 _NocCloud2;
float4 _NocGlowSeg[NOC_GLOW_MAX];
float4 _NocGlowPar[NOC_GLOW_MAX];
float  _NocGlowCount;
float4 _NocAmber;

// ------------------------------------------------------------------ small helpers
float NocS01(float t) { t = saturate(t); return t * t * (3.0 - 2.0 * t); }
float NocSmoother01(float t) { t = saturate(t); return t * t * t * (t * (t * 6.0 - 15.0) + 10.0); }

// Radians per pixel (vertical) of the current camera: 2 / (P11 H). Used as the default blur of the sky noise.
float NocPixelAngle()
{
    return 2.0 / max(abs(UNITY_MATRIX_P[1][1]) * max(_NocScreen.x, 1.0), 1e-4);
}

// Erf, Abramowitz-Stegun 7.1.26 (|err| < 1.5e-7): the monotone haze-sheet path integral (R2 2.2).
float NocErfAS(float x)
{
    float s = x < 0.0 ? -1.0 : 1.0;
    x = abs(x);
    float t = 1.0 / (1.0 + 0.3275911 * x);
    float p = ((((1.061405429 * t - 1.453152027) * t + 1.421413741) * t - 0.284496736) * t + 0.254829592) * t;
    return s * (1.0 - p * exp(-x * x));
}

// Value noise with a lattice periodic in `period` cells (the cloud scroll wraps exactly; twin of NocFogMath.ValueNoiseP).
float NocValueNoiseP(float2 p, float period)
{
    p -= period * floor(p / period);                       // [0, period]: float wrap (no integer modulus)
    float2 f = floor(p);
    float2 t = p - f;
    f = f >= period ? f - period : f;                      // rounding can land p exactly on the period (a 1 px seam line)
    float2 g = f + 1.0;
    g = g >= period ? g - period : g;
    int2 i = int2(f), j = int2(g);
    float2 u = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    float a = NocU01(NocPcg2(i.x, i.y)), b = NocU01(NocPcg2(j.x, i.y));
    float c = NocU01(NocPcg2(i.x, j.y)), d = NocU01(NocPcg2(j.x, j.y));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

// The same lattice with an integer seed added after the wrap (a different pattern per sheet and octave, still periodic).
float NocValueNoisePS(float2 p, float period, int2 seed)
{
    p -= period * floor(p / period);
    float2 f = floor(p);
    float2 t = p - f;
    f = f >= period ? f - period : f;
    float2 g = f + 1.0;
    g = g >= period ? g - period : g;
    int2 i = int2(f) + seed, j = int2(g) + seed;
    float2 u = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    float a = NocU01(NocPcg2(i.x, i.y)), b = NocU01(NocPcg2(j.x, i.y));
    float c = NocU01(NocPcg2(i.x, j.y)), d = NocU01(NocPcg2(j.x, j.y));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

// Rotation i (0..5) of the abyss noise lattices: +17 / -53 / +38 / -71 / +64 / -29 deg (no lattice aligns with the rift, the city
// grid, another octave or the deck's +23 / -41 / +71 / -59). Tables for the [loop]s of NocAbyss (small code, branchy execution).
static const float2 NocRot6CS[6] = { float2(0.95630476, 0.29237170), float2(0.60181502, -0.79863551), float2(0.78801075, 0.61566148),
                                     float2(0.32556815, -0.94551858), float2(0.43837115, 0.89879405), float2(0.87461971, -0.48480962) };
float2 NocRot6(float2 p, int i) { float2 cs = NocRot6CS[i]; return float2(cs.x * p.x - cs.y * p.y, cs.y * p.x + cs.x * p.y); }
static const float NocSheetLs[4] = { NA_SheetL0, NA_SheetL1, NA_SheetL2, NA_SheetL3 };
static const float NocSheetWs[4] = { NA_SheetW0, NA_SheetW1, NA_SheetW2, NA_SheetW3 };
static const float NocSheetBandA[3] = { NA_BandA0, NA_BandA1, NA_BandA2 };
static const float NocSheetTopK[3] = { NA_TopK0, NA_TopK1, NA_TopK2 };
static const float3 NocSheetCols[3] = { float3(NA_SheetCol0R, NA_SheetCol0G, NA_SheetCol0B), float3(NA_SheetCol1R, NA_SheetCol1G, NA_SheetCol1B),
                                        float3(NA_SheetCol2R, NA_SheetCol2G, NA_SheetCol2B) };

// R6 per-item amber band: the weight of SodiumAmber (vs SignWhite) for an item with hash u in [0, 1) at k_a. Each item switches
// over a k_a band of 0.06 (about 14 s at 2.5 m/s in beats 5-6), so nothing pops; the mean over items is k_a; 0 at k_a 0, 1 at 1.
float NocAmberMix(float u, float ka) { return saturate((ka - (NC_AmberBand + u * (1.0 - NC_AmberBand))) / NC_AmberBand + 1.0); }

// ------------------------------------------------------------------ fog (14.2.2, H3 with D-006 / R2 2.1-2.3)
// Height factor of sigma_eff: 1 + 1.8 smoothstep(0, 1400, -y) (the exact optical depth of a real density along any ray from y 0).
float NocDown(float y) { return 1.0 + NC_FogDownFactor * NocS01(-y / NC_FogDownDepth); }

// Mean of one Gaussian haze sheet (centre yk, width w) along the segment y0 -> y1: closed-form erf integral (monotone).
float NocSheetMean(float y0, float y1, float yk, float w)
{
    float dy = y1 - y0;
    if (abs(dy) < 1.0) { float m = (0.5 * (y0 + y1) - yk) / w; return exp(-m * m); }
    return 0.88622692545 * w * (NocErfAS((y1 - yk) / w) - NocErfAS((y0 - yk) / w)) / dy;
}

// Alley haze weight 1 - smoothstep(18, 30, y): exact mean along y0 -> y1 through its antiderivative Q.
float NocCapQ(float y)
{
    float h = NC_AlleyHazeY1 - NC_AlleyHazeY0;
    float t = (y - NC_AlleyHazeY0) / h;
    if (t <= 0.0) return 0.0;
    if (t >= 1.0) return h * (t - 0.5);
    return h * (t * t * t - 0.5 * t * t * t * t);
}
float NocCapMean(float y0, float y1)
{
    float dy = y1 - y0;
    if (abs(dy) < 0.01) return 1.0 - NocS01((y0 - NC_AlleyHazeY0) / (NC_AlleyHazeY1 - NC_AlleyHazeY0));
    return 1.0 - (NocCapQ(y1) - NocCapQ(y0)) / dy;
}

// Optical depth tau from the eye to posWS; T = exp(-tau^2). sigmaBase replaces _NocFog.x (the sky's cloud uses min(sigma, 6e-4)).
float NocOpticalDepthS(float3 posWS, float sigmaBase)
{
    float3 v = posWS - _WorldSpaceCameraPos.xyz;
    float d = length(v);
    float y0 = _WorldSpaceCameraPos.y, y1 = posWS.y;
    float tau = sigmaBase * NocDown(y1) * d;                                           // exact height term (R2 2.1)
    [branch] if (_NocFog.z > 0.0 && min(y0, y1) < NC_SheetSkipY)
    {
        float w = _NocFogSheets.w;
        float sh = NocDown(_NocFogSheets.x) * NocSheetMean(y0, y1, _NocFogSheets.x, w)
                 + NocDown(_NocFogSheets.y) * NocSheetMean(y0, y1, _NocFogSheets.y, w)
                 + NocDown(_NocFogSheets.z) * NocSheetMean(y0, y1, _NocFogSheets.z, w);
        tau += sigmaBase * _NocFog.z * sh * d;                                           // monotone sheets (R2 2.2)
    }
    [branch] if (_NocFog.y > 0.0) tau += _NocFog.y * NocCapMean(y0, y1) * d;            // alley haze layer (D-006)
    return tau;
}
float NocOpticalDepth(float3 posWS) { return NocOpticalDepthS(posWS, _NocFog.x); }
float NocFogT(float3 posWS) { float tau = NocOpticalDepth(posWS); return exp(-tau * tau); }

// ------------------------------------------------------------------ cloud deck (plane y 460, LOOK_V2 3.1 tint model)
// Glow union W in [0, 1] (soft union of Gaussians, sigma 220 m, over segments / points / rings in xz), the g-weighted tint of
// the glows (the deck colour over each district) and the Gulf shadow union (tint 3: a multiplier of the base, never a glow).
float NocCloudGlowT(float2 xz, out float3 tint, out float gulf)
{
    float keep = 1.0, keepG = 1.0, gsum = 0.0;
    float3 tsum = 0.0;
    int n = (int)min(_NocGlowCount, (float)NOC_GLOW_MAX);
    [loop] for (int i = 0; i < n; i++)
    {
        float4 sg = _NocGlowSeg[i], pr = _NocGlowPar[i];
        float2 a = sg.xy, ab = sg.zw - sg.xy;
        float h = saturate(dot(xz - a, ab) / max(dot(ab, ab), 1e-6));
        float dist = abs(length(xz - a - ab * h) - pr.z);
        float q = dist / max(pr.y, 1.0);
        float g = saturate(pr.x * exp(-0.5 * q * q));
        float sh = step(2.5, pr.w);                                                  // 1 = Gulf shadow
        float3 tc = pr.w < 0.5 ? PAL_CloudHot : (pr.w < 1.5 ? PAL_CloudCool : PAL_CloudBase);
        keepG *= 1.0 - g * sh;
        float gg = g * (1.0 - sh);
        keep *= 1.0 - gg;
        gsum += gg;
        tsum += gg * tc;
    }
    tint = tsum / max(gsum, 1e-4);
    gulf = 1.0 - keepG;
    return 1.0 - keep;
}
float NocCloudGlowG(float2 xz) { float3 t; float g; return NocCloudGlowT(xz, t, g); }

// One octave's fade at the pixel footprint blurM (m): smoothstep(4, 8, L / blurM) (no octave under 4 px of wavelength).
float NocOctaveFade(float L, float blurM) { return smoothstep(NC_CloudFadePx0, NC_CloudFadePx1, L / max(blurM, 1e-6)); }

// Deck noise n in 0..1, mean 0.5: octaves 900 / 300 / 110 / 40 m (weights 0.36 / 0.19 / 0.30 / 0.15), each rotated (+23 / -41 /
// +71 / -59 deg, so no lattice aligns with the city grid or another octave) and scrolled 3 m/s; the 110 and 40 m octaves are
// domain-warped by 60 m x (2 n900 - 1, 2 n300 - 1); faded octaves sit at their mean (no shimmer below 8 px of wavelength).
float NocCloudNoise(float2 xz, float blurM)
{
    float k1 = NocOctaveFade(NC_CloudNoise1, blurM), k2 = NocOctaveFade(NC_CloudNoise2, blurM);
    float k3 = NocOctaveFade(NC_CloudNoise3, blurM), k4 = NocOctaveFade(NC_CloudNoise4, blurM);
    float2 r1 = float2(0.92050485 * xz.x - 0.39073113 * xz.y, 0.39073113 * xz.x + 0.92050485 * xz.y);
    float2 r2 = float2(0.75470958 * xz.x + 0.65605903 * xz.y, -0.65605903 * xz.x + 0.75470958 * xz.y);
    float n1 = lerp(0.5, NocValueNoiseP((r1 + _NocCloud.xy) / NC_CloudNoise1, NC_CloudNoisePeriod), k1);
    float n2 = lerp(0.5, NocValueNoiseP((r2 + _NocCloud.zw) / NC_CloudNoise2, NC_CloudNoisePeriod), k2);
    float n3 = 0.5, n4 = 0.5;
    [branch] if (k3 > 0.0)
    {
        float2 w = xz + NC_CloudWarp * float2(2.0 * n1 - 1.0, 2.0 * n2 - 1.0);
        float2 r3 = float2(0.32556815 * w.x - 0.94551858 * w.y, 0.94551858 * w.x + 0.32556815 * w.y);
        n3 = lerp(0.5, NocValueNoiseP((r3 + _NocCloud2.xy) / NC_CloudNoise3, NC_CloudNoisePeriod), k3);
        [branch] if (k4 > 0.0)
        {
            float2 r4 = float2(0.51503807 * w.x + 0.85716730 * w.y, -0.85716730 * w.x + 0.51503807 * w.y);
            n4 = lerp(0.5, NocValueNoiseP((r4 + _NocCloud2.zw) / NC_CloudNoise4, NC_CloudNoisePeriod), k4);
        }
    }
    return NC_CloudNoiseW1 * n1 + NC_CloudNoiseW2 * n2 + NC_CloudNoiseW3 * n3 + NC_CloudNoiseW4 * n4;
}

// Deck billow form f in (-1, 1), mean 0 at n 0.5: x / sqrt(1 + x^2), x = 4.5 (n - 0.5) (a soft S that never flattens: no H1 plateau).
float NocDeckForm(float n) { float x = NA_DeckK * (n - 0.5); return x * rsqrt(1.0 + x * x); }

// Ceiling colour at (x, z) (LOOK_V2 3.1 tint model, AT form): base = CloudBase (1 + 0.5 f) (1 - 0.35 g_gulf), cloud = lerp(base,
// tint (1 + 0.5 f), W): warm grey over the boulevard, cool over the sign wall, spire and koi; +-22 % billows at one std of n.
float3 NocCloudColorN(float2 xz, float blurM, out float f)
{
    f = NocDeckForm(NocCloudNoise(xz, blurM));
    float3 tint; float gulf;
    float W = NocCloudGlowT(xz, tint, gulf);
    float3 base = PAL_CloudBase * ((1.0 + NA_DeckBaseAmp * f) * (1.0 - NC_CloudGulfShade * gulf));
    return lerp(base, tint * (1.0 + NA_DeckTintAmp * f), W);
}
float3 NocCloudColor(float2 xz, float blurM) { float f; return NocCloudColorN(xz, blurM, f); }

// ------------------------------------------------------------------ sky colour (14.2.2 + D-006 + H3 sheet modulation)
// Gradient part for elevation e (deg), without the cloud deck (the colour the cloud and every fog fades toward).
float3 NocSkyGradient(float e)
{
    float3 c;
    if (e >= 0.0)
    {
        c = lerp(PAL_HazeHorizon, PAL_NightMid, smoothstep(0.0, 35.0, e));
        c = lerp(c, PAL_NightZenith, smoothstep(35.0, 90.0, e));
    }
    else
    {
        float x = 1.0 - saturate(-e / 90.0);
        float3 a = lerp(PAL_AbyssNear, PAL_AbyssGlow, 1.0 - pow(x, NA_AbyssExp));          // monotone, no dip (14.2.2), flat at -90
        float wb = NocSmoother01(-e / NC_HorizonBlendDeg);                                 // C1 at the horizon (D-006)
        c = lerp(PAL_HazeHorizon, a, wb);                                                  // the sheets: NocAbyss (needs the ray)
    }
    return c;
}

// ------------------------------------------------------------------ the abyss (H3, AT): deep glow of the lit city + three haze sheets
// One sheet's billow noise at plane point p (m), footprint blurM (m): 4 octaves, lattice seeds per sheet / octave; the scroll s (m,
// wrapped to NA_SheetScrollPeriod, a whole number of every octave's 64-cell lattice) is added in each octave's rotated frame, so
// the wrap is exact (each octave drifts along its own direction: the billows evolve rather than slide as one sheet).
float NocSheetNoise(float2 p, float2 s, float blurM, int k, out float nFine, out float g)
{
    float n = 0.0;
    nFine = 0.5;
    g = 0.0;
    [loop] for (int o = 0; o < 4; o++)
    {
        float L = NocSheetLs[o];
        float fd = smoothstep(NA_SheetFadePx0, NA_SheetFadePx1, L / max(blurM, 1e-6));
        float v = 0.5;
        [branch] if (fd > 0.0)
            v = lerp(0.5, NocValueNoisePS((NocRot6(p, o + k) + s) / L, NC_CloudNoisePeriod, int2(37 * (k + 1) + 11 * o, 53 * (k + 1) + 7 * o)), fd);
        n += NocSheetWs[o] * v;
        g += NocSheetWs[o] * fd / L;
        nFine = v;                                                                         // the last (44 m) octave
    }
    g *= blurM;                                                                            // noise change per pixel (scale)
    return n;
}

// Band profile of a sheet in elevation: the far edge of the layer above e_k (1.6 deg), a long soft tail below (5.5 deg).
float NocSheetBand(float x) { float q = x > 0.0 ? x / NA_BandUp : x / NA_BandDn; return exp(-q * q); }

// c = the base gradient of a downward ray d (unit, d.y < 0) at elevation e (deg); blurRad = pixel angle (rad). Deep glow first
// (plane y -2000, fully fogged), then the sheets deep to near: c = lerp(c, sheet colour, a_k).
float3 NocAbyss(float3 c, float3 d, float e, float blurRad)
{
    float sy = max(-d.y, 1e-4);
    float3 cam = _WorldSpaceCameraPos.xyz;
    float dw = smoothstep(NA_DeepE0, NA_DeepE1, -e);
    [branch] if (dw > 0.0)
    {
        float t = (cam.y - NA_DeepY) / sy;
        float2 h = cam.xz + d.xz * t;
        float blurM = blurRad * t / max(sy, 0.05);
        float f0 = NocOctaveFade(NA_DeepL0, blurM), f1 = NocOctaveFade(NA_DeepL1, blurM);
        float n = NA_DeepW0 * lerp(0.5, NocValueNoisePS((NocRot6(h, 2) + _NocAbyss.zw) / NA_DeepL0, NC_CloudNoisePeriod, int2(900, 300)), f0)
                + NA_DeepW1 * lerp(0.5, NocValueNoisePS((NocRot6(h, 5) + _NocAbyss.zw) / NA_DeepL1, NC_CloudNoisePeriod, int2(901, 301)), f1);
        c *= 1.0 + NA_DeepAmp * 2.0 * (n - 0.5) * dw;
    }
    [loop] for (int k = 2; k >= 0; k--)
    {
        float yk = _NocFogSheets[k], ek = _NocSheetE[k];
        float t = (cam.y - yk) / sy;
        float tau = _NocFog.x * NocDown(yk) * t;
        float V = exp(-tau * tau);
        float aE = (NocSheetBandA[k] * NocSheetBand(e - ek) + (1.0 - exp(-NocSheetTopK[k] / sy)) * V * NocS01((ek - e) / NA_TopRamp)) * _NocSheetE.w;
        [branch] if (aE > 0.002)
        {
            float2 h = cam.xz + d.xz * t;
            float nFine, g;
            float n = NocSheetNoise(h, _NocAbyss.xy, blurRad * t / max(sy, 0.05), k, nFine, g);
            float hw = max(NA_CovHalf, NA_CovAA * g);
            float a = min(smoothstep(0.5 - hw, 0.5 + hw, n) * aE, NA_SheetMaxA);
            float shade = 1.0 + NA_SheetDetail * 2.0 * (nFine - 0.5) - NA_SheetThick * saturate((n - 0.5 - hw) * 4.0);
            c = lerp(c, NocSheetCols[k] * shade, a);
        }
    }
    return c;
}

// Sky colour (linear) of a direction (any length); blurRad = angular footprint of the pixel (rad) for the cloud noise.
float3 NocSkyColorBlur(float3 dir, float blurRad)
{
    float3 d = dir * rsqrt(max(dot(dir, dir), 1e-12));
    float e = degrees(asin(clamp(d.y, -1.0, 1.0)));
    float3 c = NocSkyGradient(e);
    [branch] if (e < 0.0) c = NocAbyss(c, d, e, blurRad);                                 // H3 (AT): deep glow, three sheets
    [branch] if (e > NC_CloudFadeE0)
    {
        float t = (NC_CloudBaseY - _WorldSpaceCameraPos.y) / max(d.y, 1e-3);
        float3 hit = _WorldSpaceCameraPos.xyz + d * t;
        float blurM = blurRad * t / max(d.y, 0.05);                                        // footprint along the plane (grazing)
        float3 cc = NocCloudColor(hit.xz, blurM);
        float tau = NocOpticalDepthS(hit, min(_NocFog.x, (float)NC_CloudSkySigmaMax));     // fogged like geometry (D-006 cap)
        float T = exp(-tau * tau);
        c = lerp(c, cc, T * smoothstep(NC_CloudFadeE0, NC_CloudFadeE1, e));
    }
    // LOOK_V2 3.1: the warm low haze of the alley layer (D-006) lives in the sky function, so the dome and every fog colour carry
    // it: c = lerp(c, HazeLow, 0.6 sa m / (sr + sa m)), m = the layer's mean along the ray to min(6 500 m, the cloud hit); the
    // path length cancels (continuous across the cloud fade). 0.55 at eye level, 0.20 toward the zenith; gone from s 124.
    [branch] if (_NocFog.y > 0.0)
    {
        float yc = _WorldSpaceCameraPos.y;
        float sam = _NocFog.y * NocCapMean(yc, yc + min(NC_HazeLowPath * d.y, NC_CloudBaseY - yc));
        c = lerp(c, PAL_HazeLow, NC_HazeLowShare * sam / max(_NocFog.x + sam, 1e-9));
    }
    return max(c, 0.0);
}
float3 NocSkyColor(float3 dir) { return NocSkyColorBlur(dir, NocPixelAngle()); }

// ------------------------------------------------------------------ geometry: cloud mix, fog, floor
// H3: geometry mixes toward the ceiling colour above it (smoothstep(380, 560, y + 70 f)), so shaft tops vanish into the deck; the
// billow form f of the deck at the fragment moves the mix 70 m up or down, so a tower dissolves into hanging bellies and gaps (AT).
float3 NocCloudMix(float3 col, float3 posWS)
{
    [branch] if (posWS.y > NC_CloudMixY0 - NA_CloudWisp)
    {
        float blurM = NocPixelAngle() * length(posWS - _WorldSpaceCameraPos.xyz);
        float f;
        float3 cc = NocCloudColorN(posWS.xz, blurM, f);
        col = lerp(col, cc, smoothstep(NC_CloudMixY0, NC_CloudMixY1, posWS.y + NA_CloudWisp * f));
    }
    return col;
}

// col T + fogColour (1 - T), fogColour = NocSkyColor(direction of the fragment): the horizon always agrees (8.4, 14.2.2).
float3 NocFogT3(float3 col, float3 posWS, float T)
{
    [branch] if (T < 0.99995) col = lerp(NocSkyColor(posWS - _WorldSpaceCameraPos.xyz), col, T);
    return col;
}
float3 NocFog(float3 col, float3 posWS)
{
    col = NocCloudMix(col, posWS);
    return NocFogT3(col, posWS, NocFogT(posWS));
}

// The last operation of every shader and of the sky: FloorLin + col (1 - FloorLin).
float3 NocFloor(float3 col)
{
    float3 f = float3(NC_FloorLinR, NC_FloorLinG, NC_FloorLinB);
    return f + max(col, 0.0) * (1.0 - f);
}

#endif
