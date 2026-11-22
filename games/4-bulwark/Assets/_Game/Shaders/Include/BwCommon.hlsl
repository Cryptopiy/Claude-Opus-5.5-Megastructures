// BwCommon.hlsl - small shared HLSL helpers of every Bw* shader. Imported from The Ring's RingCommon.hlsl
// (only the hash / value-noise / filtered-grid helpers BwConcrete needs, plus smootherstep and luma).
// C# twins: BwMath.Pcg / U01 / ValueNoise (EditMode tests compare them).
#ifndef BW_COMMON_INCLUDED
#define BW_COMMON_INCLUDED

#define BW_PI 3.14159265
#define BW_TWO_PI 6.2831853
#define BW_DEG2RAD 0.01745329
#define BW_RAD2DEG 57.2957795

float BwSmootherstep01(float t) { t = saturate(t); return t * t * t * (t * (t * 6.0 - 15.0) + 10.0); }
float BwSmootherstep(float a, float b, float x) { return BwSmootherstep01((x - a) / (b - a)); }
float BwLuma(float3 c) { return dot(c, float3(0.2126, 0.7152, 0.0722)); } // pal:ok (Rec.709 luminance weights)
float BwSafeDiv(float a, float b) { return a / (abs(b) < 1e-8 ? (b < 0.0 ? -1e-8 : 1e-8) : b); }

// ---------------- hashes (PCG, Jarzynski & Olano 2020); twin of BwMath.Pcg ----------------
// PCG: Melissa E. O'Neill / PCG Project, Apache-2.0. See repository-root THIRD_PARTY_NOTICES.md.
uint BwPcg(uint v)
{
    uint state = v * 747796405u + 2891336453u;
    uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
    return (word >> 22u) ^ word;
}
uint BwPcg2(int2 p) { return BwPcg(asuint(p.x) + BwPcg(asuint(p.y))); }
uint BwPcg3(int3 p) { return BwPcg(asuint(p.x) + BwPcg(asuint(p.y) + BwPcg(asuint(p.z)))); }
float BwU01(uint h) { return (h >> 8u) * (1.0 / 16777216.0); }
float BwHash21(int2 p) { return BwU01(BwPcg2(p)); }
float2 BwHash22(int2 p) { uint h = BwPcg2(p); return float2(BwU01(h), BwU01(BwPcg(h))); }

// 2D value noise in [0, 1], quintic interpolation; twin of BwMath.ValueNoise
float BwValueNoise(float2 x)
{
    float2 f = floor(x);
    int2 i = (int2)f;
    float2 t = x - f;
    float2 u = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    float a = BwHash21(i), b = BwHash21(i + int2(1, 0));
    float c = BwHash21(i + int2(0, 1)), d = BwHash21(i + int2(1, 1));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

// Exact box-filtered coverage of a single line of width w centred at distance 0, for a filter footprint fp
// (same units). Converges to w / fp (energy preserving) once the line is sub-footprint.
float BwFilteredLine(float distToLine, float width, float fp)
{
    float h = 0.5 * max(fp, 1e-6);
    float a = min(distToLine + h, 0.5 * width);
    float b = max(distToLine - h, -0.5 * width);
    return saturate(max(a - b, 0.0) / (2.0 * h));
}

// Exact box-filtered periodic line train (IQ, "filterable procedurals"): lines of width w every period P,
// centred on multiples of P. fp = footprint in the same units. Fades to 0 once the spacing drops below
// fadeSpacingPx footprints (far surfaces turn smooth instead of grey). Wrap x modulo the period before calling
// when it can exceed ~1e4 (lesson 3.2 #12).
float BwFilteredGrid1(float x, float period, float width, float fp, float fadeSpacingPx)
{
    float N = period / max(width, 1e-6);
    float p = x / period + 0.5 / N;
    float w = max(fp / period, 1e-6);
    float a = p + 0.5 * w, b = p - 0.5 * w;
    float cov = (floor(a) + min(frac(a) * N, 1.0) - floor(b) - min(frac(b) * N, 1.0)) / (N * w);
    float spacingPx = period / max(fp, 1e-6);
    float fade = saturate((spacingPx - fadeSpacingPx) / max(fadeSpacingPx, 1e-3));
    return saturate(cov) * fade;
}

// ---------------- surface-state global (materials owner; written by AtmosphereDriver once it publishes it) ----------------
// All 0 when unset, which means: dry, open sky, lamp gain 1. x = rain wetness of exposed surfaces by s (0 dry .. 1 wet;
// materials with _WetFollow > 0 use it), y = interior factor of the environment reflection (0 open sky .. 1 enclosed:
// reflections become the lamp-lit ambient), z = lamp gain of BwEmissive (0 means 1: the beat-5 portal boost, 14.2.5),
// w = reserved. Guarded so BulwarkAtmosphere.hlsl may declare it too with the same guard.
#ifndef BW_SURFACE_DECLARED
#define BW_SURFACE_DECLARED
float4 _BwSurface;
#endif

// ---------------- footprint-filtered procedural helpers (MASTER_PROMPT 8.6, 8.11.6; materials owner) ----------------
// 2D gradient (Perlin) noise in about [-0.7, 0.7], quintic fade, PCG gradients (no sin hash). Twin of BwTexGen.Grad
// up to the gradient table (the C# generator is periodic; this one is not and is used for macro variation only).
float BwGradNoise(float2 x)
{
    float2 f = floor(x);
    int2 i = (int2)f;
    float2 t = x - f;
    float2 u = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    float2 ga = BwHash22(i) * 2.0 - 1.0, gb = BwHash22(i + int2(1, 0)) * 2.0 - 1.0;
    float2 gc = BwHash22(i + int2(0, 1)) * 2.0 - 1.0, gd = BwHash22(i + int2(1, 1)) * 2.0 - 1.0;
    float a = dot(ga, t), b = dot(gb, t - float2(1, 0)), c = dot(gc, t - float2(0, 1)), d = dot(gd, t - float2(1, 1)); // pal:ok (lattice offsets)
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

// Value-noise fBm in [0, 1] with octaves weighted by the footprint (an octave whose cell is under 2 footprints fades to
// its mean 0.5, Quilez band limiting): x in cells of the first octave, fpCells = pixel footprint in the same units.
float BwFbmFiltered(float2 x, float fpCells, int octaves)
{
    float sum = 0.0, amp = 0.5, norm = 0.0, scale = 1.0;
    [unroll(5)] for (int o = 0; o < octaves; o++)
    {
        float w = 1.0 - smoothstep(0.25, 0.5, fpCells * scale);   // cell = 1/scale; fade between 4 and 2 footprints
        sum += amp * lerp(0.5, BwValueNoise(x * scale + o * 17.31), w);
        norm += amp;
        amp *= 0.5;
        scale *= 2.03;
    }
    return sum / max(norm, 1e-4);
}

// Signed macro term of a BwFbmFiltered / BwFbmFiltered3 value m (mean 0.5): about -1 / +1 at the fBm's 5th / 95th
// percentiles (3 octaves: p5..p95 about 0.5 +- 0.24 in 2D, 0.5 +- 0.22 in 3D: the 3D form uses the x5 stretch), clamped;
// 0 once the fBm is filtered flat. Twins BwTexGen.MacroSigned / MacroSigned3 (MaterialTests).
float BwMacroSigned(float m) { return clamp((m - 0.5) * 4.0, -1.0, 1.0); }
float BwMacroSigned3(float m) { return clamp((m - 0.5) * 5.0, -1.0, 1.0); }

// 3D value noise in [0, 1] (quintic) and its footprint-filtered fBm: the world-space macro fields of surfaces in every
// orientation. A 2D domain such as (p.xz + p.y) is degenerate on walls that contain its null direction (the Wave A
// review round saw 4:1 streaks on a wall at yaw 300 and on curved armour), a 3D lattice never is. Twins:
// BwTexGen.ShaderNoise3 / ShaderFbm3 (MaterialTests).
float BwHash31(int3 p) { return BwU01(BwPcg3(p)); }
float BwValueNoise3(float3 x)
{
    float3 f = floor(x);
    int3 i = (int3)f;
    float3 t = x - f;
    float3 u = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    float a = BwHash31(i), b = BwHash31(i + int3(1, 0, 0)), c = BwHash31(i + int3(0, 1, 0)), d = BwHash31(i + int3(1, 1, 0));
    float e = BwHash31(i + int3(0, 0, 1)), g = BwHash31(i + int3(1, 0, 1)), h = BwHash31(i + int3(0, 1, 1)), k = BwHash31(i + int3(1, 1, 1));
    float z0 = lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
    float z1 = lerp(lerp(e, g, u.x), lerp(h, k, u.x), u.y);
    return lerp(z0, z1, u.z);
}
float BwFbmFiltered3(float3 x, float fpCells, int octaves)
{
    float sum = 0.0, amp = 0.5, norm = 0.0, scale = 1.0;
    [unroll(5)] for (int o = 0; o < octaves; o++)
    {
        float w = 1.0 - smoothstep(0.25, 0.5, fpCells * scale);
        sum += amp * lerp(0.5, BwValueNoise3(x * scale + o * 17.31), w);
        norm += amp;
        amp *= 0.5;
        scale *= 2.03;
    }
    return sum / max(norm, 1e-4);
}

// 3D gradient (Perlin, 12 cube-edge gradients) noise in about [-0.7, 0.7] and its footprint-filtered fBm mapped to [0, 1]
// (0.5 + 0.75 fbm: p5..p95 about 0.5 +- 0.22, the same spread as BwFbmFiltered3). For masks that are THRESHOLDED
// (colony patches, foam, zone wobble): the isocontours of value noise follow its lattice and read as square blocks, those
// of gradient noise are round and organic. Twins: BwTexGen.ShaderGrad3 / ShaderFbmGrad3.
static const float3 BwGrad12[12] = {
    float3(1, 1, 0), float3(-1, 1, 0), float3(1, -1, 0), float3(-1, -1, 0),     // pal:ok (gradient table)
    float3(1, 0, 1), float3(-1, 0, 1), float3(1, 0, -1), float3(-1, 0, -1),     // pal:ok (gradient table)
    float3(0, 1, 1), float3(0, -1, 1), float3(0, 1, -1), float3(0, -1, -1) };   // pal:ok (gradient table)
float BwGradCorner(int3 i, float3 d) { return dot(BwGrad12[BwPcg3(i) % 12u], d); }
float BwGradNoise3(float3 x)
{
    float3 f = floor(x);
    int3 i = (int3)f;
    float3 t = x - f;
    float3 u = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    float a = BwGradCorner(i, t), b = BwGradCorner(i + int3(1, 0, 0), t - float3(1, 0, 0));   // pal:ok (lattice offsets)
    float c = BwGradCorner(i + int3(0, 1, 0), t - float3(0, 1, 0)), d = BwGradCorner(i + int3(1, 1, 0), t - float3(1, 1, 0));   // pal:ok
    float e = BwGradCorner(i + int3(0, 0, 1), t - float3(0, 0, 1)), g = BwGradCorner(i + int3(1, 0, 1), t - float3(1, 0, 1));   // pal:ok
    float h = BwGradCorner(i + int3(0, 1, 1), t - float3(0, 1, 1)), k = BwGradCorner(i + int3(1, 1, 1), t - float3(1, 1, 1));   // pal:ok
    return lerp(lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y), lerp(lerp(e, g, u.x), lerp(h, k, u.x), u.y), u.z);
}
float BwFbmGrad3(float3 x, float fpCells, int octaves)
{
    float sum = 0.0, amp = 0.5, norm = 0.0, scale = 1.0;
    [unroll(5)] for (int o = 0; o < octaves; o++)
    {
        float w = 1.0 - smoothstep(0.25, 0.5, fpCells * scale);
        sum += amp * w * BwGradNoise3(x * scale + o * 17.31);
        norm += amp;
        amp *= 0.5;
        scale *= 2.03;
    }
    return 0.5 + 0.75 * sum / max(norm, 1e-4);
}

// Fade weight of a feature of world size `size` seen with footprint `fp`: 0 below px0 pixels, 1 above px1 pixels.
float BwPxFade(float size, float fp, float px0, float px1)
{
    float px = size / max(fp, 1e-6);
    return saturate((px - px0) / max(px1 - px0, 1e-3));
}

// Exact box-filtered coverage of the interval [c - w/2, c + w/2] at x for a box footprint fp (1D; separable products
// give axis-aligned rectangles). Energy preserving: converges to w / fp once the interval is sub-footprint.
float BwBoxCover(float x, float c, float w, float fp) { return BwFilteredLine(x - c, w, fp); }

// Periodic pulse train (duty d of period 1, pulse on [k, k + d)) filtered by a TRIANGLE kernel of half-width h (period
// units; Quilez "more checker filtering": steadier than the box under motion, gratings, RESEARCH B 6). Evaluated as the
// sum over the nearby pulses of the triangle kernel's CDF at both pulse edges, in coordinates relative to the wrapped
// fragment position t = frac(x): every term is a small number, so there is no cancellation at any h (the double-integral
// second-difference form lost all precision below h ~ 1e-3 and drew a barcode). h above 4 periods returns the mean d.
float BwTriCdf(float u, float h)
{
    float v = clamp(u, -h, h) / h;          // -1 .. 1
    return v <= 0.0 ? 0.5 * (1.0 + v) * (1.0 + v) : 1.0 - 0.5 * (1.0 - v) * (1.0 - v);
}
float BwPulseTri(float x, float d, float h)
{
    if (h > 4.0) return d;
    h = max(h, 1e-5);
    float t = x - floor(x);
    float sum = 0.0;
    [unroll] for (int k = -5; k <= 5; k++)
    {
        if (abs((float)k - t) > h + 1.0) continue;
        sum += BwTriCdf(k + d - t, h) - BwTriCdf(k - t, h);
    }
    return saturate(sum);
}

// ---------------- tier fades (D-08): static 64 x 64 blue-noise threshold, never IGN or an ordered matrix ----------------
// _BwBlueNoise is the grade's rank tile (BlueNoise.cs, (rank + 0.5) / 4096), bound globally by ProgressDirector.
// A fade value f in [0, 1] keeps the pixel when noise < f; the complementary tier uses 1 - f with the same noise,
// so the two tiers tile the screen exactly with no overlap and no hole. Under MSAA prefer alpha-to-coverage.
TEXTURE2D(_BwBlueNoise);
float BwBlueNoiseAt(float2 positionSS)
{
    uint2 p = uint2(positionSS) & 63u;
    return LOAD_TEXTURE2D(_BwBlueNoise, int2(p)).r;
}
void BwTierClip(float fade, float2 positionSS)
{
    if (fade < 0.999) clip(fade - BwBlueNoiseAt(positionSS));
}

#endif
