// NocCommon.hlsl - shared globals and helpers of every Megastructure/Noc* shader (the HLSL side of ShaderIds.cs).
// Globals are written once per frame by the AtmosphereDriver / ProgressDirector; lesson 5: no global name equals a material
// property name (globals are _Noc*). Hashes are the PCG twin of NocMath.Pcg (no sin-based hash, Part 8.6).
#ifndef NOC_COMMON_INCLUDED
#define NOC_COMMON_INCLUDED

#include "../Generated/NocConstantsGen.hlsl"
#include "../Generated/NocPalette.hlsl"

float4 _NocTime;     // x = scene time wrapped to 3600 s, y = rain time wrapped, z = s (m), w = beat index
float4 _NocScreen;   // x = render height (px), y = minimum line width (px), z = 1 / height, w = 0
float4 _NocFloor;    // xyz = linear floor colour (NocFloor), w = LightCal
float  _NocKoiK;     // analytic koi light strength (14.2.3), 0 outside beat 7

// Look v2 globals (TEXTURE_PLAN 1.6, 7.1, 7.4, 7.6; NocLookGlobals.cs writes them once at start-up). Uniform branches only, never
// keywords. A global nobody wrote reads 0 (editor previews, EditMode renders): 0 always means the default.
float  _NocTexDetail;   // -tex 0|1: -1 = texture detail off, +1 or 0 = on
float  _NocDebugView;   // -debug-view N (7.1): 0 = off, 1..10 = the debug view (raw codes, no fog, floor or grade)
float  _NocFoliageV;    // -foliage-v 1|2: 2 = the Look v2 foliage path; 0 or 1 = v1
float  _NocParity;      // ShaderParityTests (7.4): 1 = NocFog and NocFloor skipped, so the output is the shading alone

bool NocTexOn()    { return _NocTexDetail > -0.5; }
// TP-2 (FX, Phase 4 wave A): the v1 facade and surface paths and their flags (-facade-v, -surf-v) are deleted; Look v2 is the
// only path. NocSurfV2() stays as a constant for one more owner's shader (AB's NocLine; REQUEST AB in docs/logs/FX.md).
bool NocSurfV2()   { return true; }
bool NocFoliageV2(){ return _NocFoliageV > 1.5; }
int  NocDebugView(){ return (int)(_NocDebugView + 0.5); }

// Exact sRGB decode (IEC 61966-2-1), component-wise: a display code / 255 -> its linear value (debug views, captures)
float3 NocSrgbToLinear(float3 c)
{
    float3 lo = c / 12.92;
    float3 hi = pow(abs((c + 0.055) / 1.055), 2.4);
    return c <= 0.04045 ? lo : hi;
}

uint NocPcg(uint v)
{
    uint state = v * 747796405u + 2891336453u;
    uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
    return (word >> 22u) ^ word;
}
uint NocPcg2(int x, int y) { return NocPcg((uint)x + NocPcg((uint)y)); }
float NocU01(uint h) { return (h >> 8) * (1.0 / 16777216.0); }

// pcg3d / pcg4d (Jarzynski & Olano 2020, "Hash Functions for GPU Rendering"): full-avalanche multi-dimensional hashes for
// lattice ids (windows, epochs, rain cells); never a linear combination fed to a 1D hash (R1 3.4). C# twin: NocHash.Pcg3d/4d.
uint3 NocPcg3d(uint3 v)
{
    v = v * 1664525u + 1013904223u;
    v.x += v.y * v.z; v.y += v.z * v.x; v.z += v.x * v.y;
    v ^= v >> 16u;
    v.x += v.y * v.z; v.y += v.z * v.x; v.z += v.x * v.y;
    return v;
}
uint4 NocPcg4d(uint4 v)
{
    v = v * 1664525u + 1013904223u;
    v.x += v.y * v.w; v.y += v.z * v.x; v.z += v.x * v.y; v.w += v.y * v.z;
    v ^= v >> 16u;
    v.x += v.y * v.w; v.y += v.z * v.x; v.z += v.x * v.y; v.w += v.y * v.z;
    return v;
}

// Exact 1D box coverage of the interval [x0, x1] by a pixel footprint of width fw centred at x (R1 3.2).
float NocCover(float x, float fw, float x0, float x1)
{
    return saturate((min(x + 0.5 * fw, x1) - max(x - 0.5 * fw, x0)) / max(fw, 1e-6));
}

// 2D value noise in [0, 1], quintic interpolation (twin of NocMath.ValueNoise)
float NocValueNoise(float2 p)
{
    float2 f = floor(p);
    int2 i = int2(f);
    float2 t = p - f;
    float2 u = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    float a = NocU01(NocPcg2(i.x, i.y)), b = NocU01(NocPcg2(i.x + 1, i.y));
    float c = NocU01(NocPcg2(i.x, i.y + 1)), d = NocU01(NocPcg2(i.x + 1, i.y + 1));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

// Filtered 1D line grid: coverage of a line of width w (m) every `period` (m) for a pixel footprint fp (m per pixel);
// the line is widened to at least one pixel with its energy kept (w / max(w, fp)), and the grid blends to its mean
// coverage w / period once the period falls below minPx pixels (no shimmer, no moire; Part 8.6).
float NocFilteredGrid1(float x, float period, float w, float fp, float minPx)
{
    float fw = max(fp, 1e-5);
    float wd = max(w, fw);
    float d = abs(frac(x / period + 0.5) - 0.5) * period;      // distance to the nearest line (m)
    float ln = (1.0 - smoothstep(0.5 * wd - 0.5 * fw, 0.5 * wd + 0.5 * fw, d)) * (w / wd);
    return lerp(ln, saturate(w / period), saturate(fw * minPx / period));
}

#endif
