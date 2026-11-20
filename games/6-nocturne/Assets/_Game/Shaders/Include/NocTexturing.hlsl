// NocTexturing.hlsl - the Look v2 sampling library (docs/TEXTURE_PLAN.md 1.6, 1.7, 2.5; owner TP). Functions only: no CBUFFER, no
// sampler or texture declarations, no keyword, so every shader keeps its own SRP Batcher layout and sampler count (8 or fewer
// per stage). Include it after the render-pipeline core (NocLitCore.hlsl includes URP Core.hlsl); constants NC_Tex* come from
// Generated/NocTexConstantsGen.hlsl (NocTexConstants.cs). C# twin: Scripts/Runtime/Rendering/NocTexturingMath.cs
// (NocTexturingTests).
//
// Discipline (TEXTURE_PLAN 2.9 S1-S7):
//   - every tap is SampleGrad with the true (rotated) gradients, or SampleLevel of the last mip; so every sampling function may
//     sit inside a branch. The SETUP functions (NocHexSetup, NocTriSetup, NocModuleSetup, NocUvFrame) take their derivatives
//     with ddx / ddy or receive them: call them before any divergent branch.
//   - fade to the mean: w_tex = smoothstep(2, 4, tile / fp_box) with the bounding-box footprint |ddx| + |ddy| (CRIT0 #19). Below
//     the fade every map blends to its last mip (Toksvig included in the mask's mips), the slope to 0; at w_tex = 0 no tap runs.
//     -tex 0 (_NocTexDetail -1) forces w_tex = 0 everywhere.
//   - normals are surface gradients (Mikkelsen 2020): a tap's tangent normal becomes the height slope (dh/du, dh/dv) =
//     -n.xy / n.z, slopes blend linearly, the slope becomes a volume gradient (triplanar) or a surface gradient along the frame
//     (UV0, facade), and n' = normalize(n - sg).
//
// Contents:
//   NocFootprintBox, NocTexFade, NocLastMip*          footprint, the fade weight, the last-mip mean
//   NocTangentToSlope, NocResolveNormal, NocSurfGrad*  slope and surface-gradient helpers
//   NocHex* (and *_Arr)                                hex tiling (Mikkelsen 2022): setup, colour (luminance weighted), mask,
//                                                      slope, height-aware weights
//   NocTriSetup / NocTriSample                         world triplanar, each plane hex-tiled, as one surface gradient
//   NocModuleSetup                                     module shuffle of gridded sets (seams in the middle of the joints)
//   NocPanelUV, NocPanelTone                           the cell-aligned panel shuffle of the facade skins
//   NocUvFrame                                         the tangent frame of UV0 metres from screen derivatives (GenBasisTB)
#ifndef NOC_TEXTURING_INCLUDED
#define NOC_TEXTURING_INCLUDED

#include "NocCommon.hlsl"
#include "../Generated/NocTexConstantsGen.hlsl"

// ===================================================================================================== footprint and fade

// Bounding-box footprint of a 2D coordinate (its unit: metres for metric coordinates, tiles for uv): the larger per-axis box.
float NocFootprintBox(float2 dx, float2 dy) { return max(abs(dx.x) + abs(dy.x), abs(dx.y) + abs(dy.y)); }

// w_tex = smoothstep(2, 4, tile / fp_box) (tile and footprint in the same unit); 0 with -tex 0.
float NocTexFade(float tile, float fpBox)
{
    float w = smoothstep(NC_TexFadePx0, NC_TexFadePx1, tile / max(fpBox, 1e-7));
    return NocTexOn() ? w : 0.0;
}

float4 NocLastMip(TEXTURE2D_PARAM(tex, samp)) { return SAMPLE_TEXTURE2D_LOD(tex, samp, float2(0.5, 0.5), NC_TexLastMip); }
float4 NocLastMipArr(TEXTURE2D_ARRAY_PARAM(tex, samp), float slice) { return SAMPLE_TEXTURE2D_ARRAY_LOD(tex, samp, float2(0.5, 0.5), slice, NC_TexLastMip); }

// One tap with the fade: lerp(last mip, SampleGrad, fade); no tap at fade 0 (uniform per pixel, safe in branches).
float4 NocSampleFade(TEXTURE2D_PARAM(tex, samp), float2 uv, float2 dx, float2 dy, float fade)
{
    float4 m = NocLastMip(TEXTURE2D_ARGS(tex, samp));
    [branch] if (fade > 0.0) m = lerp(m, SAMPLE_TEXTURE2D_GRAD(tex, samp, uv, dx, dy), fade);
    return m;
}
float4 NocSampleFadeArr(TEXTURE2D_ARRAY_PARAM(tex, samp), float2 uv, float slice, float2 dx, float2 dy, float fade)
{
    float4 m = NocLastMipArr(TEXTURE2D_ARRAY_ARGS(tex, samp), slice);
    [branch] if (fade > 0.0) m = lerp(m, SAMPLE_TEXTURE2D_ARRAY_GRAD(tex, samp, uv, slice, dx, dy), fade);
    return m;
}

// ===================================================================================================== slopes and surface gradients

// Tangent-space normal tap (GL convention, +Y = +v) -> height slope (dh/du, dh/dv) along the map's axes.
float2 NocTangentToSlope(float4 packedNormal, float strength)
{
    float3 t = UnpackNormalScale(packedNormal, strength);
    return -t.xy / max(t.z, 1.0 / 128.0);
}

// Surface gradient of a volume gradient (triplanar): sg = vg - n (n . vg).
float3 NocSurfGradVolume(float3 n, float3 vg) { return vg - n * dot(n, vg); }
// Surface gradient of a slope along a (unit) frame: sg = s.x T + s.y B (UV0 metres, facade (u, v)).
float3 NocSurfGradTB(float2 slope, float3 T, float3 B) { return slope.x * T + slope.y * B; }
// n' = normalize(n - sg), guarded.
float3 NocResolveNormal(float3 n, float3 sg)
{
    float3 r = n - sg;
    float l = dot(r, r);
    return l > 1e-12 ? r * rsqrt(l) : n;
}

// ===================================================================================================== hex tiling (Mikkelsen 2022)

// The taps of one hex-tiled lookup. Coordinates are in tiles (uv = metres / tile). rot rotates a tap's slope back into the uv
// frame (float4 = row-major 2 x 2). w = the continuous barycentric weights w_i^gamma, normalised, cut (w' = max(w - 0.02, 0),
// renormalised: continuous, and a zero weight skips its tap). fade = w_tex.
struct NocHex
{
    float2 uv0, uv1, uv2;
    float2 dx0, dy0, dx1, dy1, dx2, dy2;
    float4 rot0, rot1, rot2;
    float3 w;
    float fade;
};

// Blend weights of the maps: col (luminance-weighted, S-curve r 0.70) for colour and the mask; nrm (no S-curve) for slopes.
struct NocHexW
{
    float3 col;
    float3 nrm;
};

// S-curve of the weights (Mikkelsen's Gain3): r > 0.5 raises contrast, 0.5 is the identity; continuous, gain(0) = 0.
float3 NocHexGain(float3 x, float r)
{
    float k = log(1.0 - r) / log(0.5);
    float3 s = 2.0 * step(0.5, x);
    float3 m = 2.0 * (1.0 - s);
    float3 res = 0.5 * s + 0.25 * m * pow(max(0.0, s + x * m), k);
    return res / max(res.x + res.y + res.z, 1e-7);
}

// Triangle grid of hex centres spaced 1 / rate tiles apart: barycentric weights and the three vertex ids (skewed lattice).
void NocHexGrid(float2 st, out float3 w, out int2 v1, out int2 v2, out int2 v3)
{
    // skew into the simplex grid: (x - y / sqrt 3, 2 y / sqrt 3)
    float2 sk = float2(st.x - 0.57735027 * st.y, 1.15470054 * st.y);
    int2 b = int2(floor(sk));
    float2 f = frac(sk);
    float z = 1.0 - f.x - f.y;
    float s = z < 0.0 ? 1.0 : 0.0;
    float s2 = 2.0 * s - 1.0;
    w = float3(-z * s2, s - f.y * s2, s - f.x * s2);
    int si = (int)s;
    v1 = b + int2(si, si);
    v2 = b + int2(si, 1 - si);
    v3 = b + int2(1 - si, si);
}

// Centre of a vertex in tiles (inverse skew / rate).
float2 NocHexCentre(int2 v, float rate) { return float2(v.x + 0.5 * v.y, 0.86602540 * v.y) / rate; }

// Per-vertex offset (a full tile, uniform) and rotation angle (+-pi x rotate) from pcg3d(vertex, seed).
void NocHexVertex(int2 v, uint seed, float rotate, out float2 offset, out float4 rot)
{
    uint3 h = NocPcg3d(uint3((uint)v.x, (uint)v.y, seed));
    offset = float2(NocU01(h.x), NocU01(h.y));
    float a = (2.0 * NocU01(h.z) - 1.0) * PI * rotate;
    float cs = cos(a), sn = sin(a);
    rot = float4(cs, -sn, sn, cs);
}

float2 NocRotRow(float2 v, float4 r) { return float2(v.x * r.x + v.y * r.z, v.x * r.y + v.y * r.w); }   // mul(v, R)
float2 NocRotCol(float4 r, float2 g) { return float2(r.x * g.x + r.y * g.y, r.z * g.x + r.w * g.y); }   // mul(R, g)

// Hex setup: uv and its screen gradients in tiles, the hex rate (hexes per tile), rotation strength (0 = offsets only, 1 = full),
// a per-material seed and the fade weight (NocTexFade). Derivatives are passed in, so this may run anywhere.
NocHex NocHexSetup(float2 uv, float2 dUVdx, float2 dUVdy, float rate, float rotate, uint seed, float fade)
{
    NocHex h;
    float3 bw; int2 v1, v2, v3;
    NocHexGrid(uv * rate, bw, v1, v2, v3);
    float2 o1, o2, o3;
    NocHexVertex(v1, seed, rotate, o1, h.rot0);
    NocHexVertex(v2, seed, rotate, o2, h.rot1);
    NocHexVertex(v3, seed, rotate, o3, h.rot2);
    float2 c1 = NocHexCentre(v1, rate), c2 = NocHexCentre(v2, rate), c3 = NocHexCentre(v3, rate);
    h.uv0 = NocRotRow(uv - c1, h.rot0) + c1 + o1;
    h.uv1 = NocRotRow(uv - c2, h.rot1) + c2 + o2;
    h.uv2 = NocRotRow(uv - c3, h.rot2) + c3 + o3;
    h.dx0 = NocRotRow(dUVdx, h.rot0); h.dy0 = NocRotRow(dUVdy, h.rot0);
    h.dx1 = NocRotRow(dUVdx, h.rot1); h.dy1 = NocRotRow(dUVdy, h.rot1);
    h.dx2 = NocRotRow(dUVdx, h.rot2); h.dy2 = NocRotRow(dUVdy, h.rot2);
    float3 w = pow(max(bw, 0.0), NC_TexHexGamma);
    w /= max(w.x + w.y + w.z, 1e-7);
    w = max(w - NC_TexHexCut, 0.0);
    h.w = w / max(w.x + w.y + w.z, 1e-7);
    h.fade = fade;
    return h;
}

// Weights from the luminance of the three colour taps: D = lerp(1, Y, beta), col = gain(normalize(D w), 0.70), nrm = normalize(D w).
NocHexW NocHexWeightsFromLum(NocHex h, float3 lum)
{
    NocHexW o;
    float3 d = lerp(1.0, lum, NC_TexHexBeta) * h.w;
    d /= max(d.x + d.y + d.z, 1e-7);
    o.nrm = d;
    o.col = NocHexGain(d, NC_TexHexGainColour);
    return o;
}

// Height-aware weights (moss, ground; shared by every map): w_i (h_i + 0.05)^8, normalised; heights from the three mask taps.
NocHexW NocHexWeightsFromHeight(NocHex h, float3 height)
{
    NocHexW o;
    float3 d = h.w * pow(max(height + NC_TexHeightBias, 0.0), NC_TexHeightPow);
    d /= max(d.x + d.y + d.z, 1e-7);
    o.nrm = d;
    o.col = d;
    return o;
}

// The uniform weights of a faded-out lookup (only the last mip runs; the value is unused).
NocHexW NocHexWeightsFlat(NocHex h) { NocHexW o; o.col = h.w; o.nrm = h.w; return o; }

// Three taps (zero weights skipped) with the given weights.
float4 NocHexTaps(NocHex h, float3 w, TEXTURE2D_PARAM(tex, samp))
{
    float4 c = 0.0;
    [branch] if (w.x > 0.0) c += w.x * SAMPLE_TEXTURE2D_GRAD(tex, samp, h.uv0, h.dx0, h.dy0);
    [branch] if (w.y > 0.0) c += w.y * SAMPLE_TEXTURE2D_GRAD(tex, samp, h.uv1, h.dx1, h.dy1);
    [branch] if (w.z > 0.0) c += w.z * SAMPLE_TEXTURE2D_GRAD(tex, samp, h.uv2, h.dx2, h.dy2);
    return c;
}
float4 NocHexTapsArr(NocHex h, float3 w, TEXTURE2D_ARRAY_PARAM(tex, samp), float slice)
{
    float4 c = 0.0;
    [branch] if (w.x > 0.0) c += w.x * SAMPLE_TEXTURE2D_ARRAY_GRAD(tex, samp, h.uv0, slice, h.dx0, h.dy0);
    [branch] if (w.y > 0.0) c += w.y * SAMPLE_TEXTURE2D_ARRAY_GRAD(tex, samp, h.uv1, slice, h.dx1, h.dy1);
    [branch] if (w.z > 0.0) c += w.z * SAMPLE_TEXTURE2D_ARRAY_GRAD(tex, samp, h.uv2, slice, h.dx2, h.dy2);
    return c;
}

// Colour: samples the three taps, derives the weights from their luminance (out hw, reused by the mask and the slope), blends.
float4 NocHexColour(NocHex h, TEXTURE2D_PARAM(tex, samp), out NocHexW hw)
{
    float4 m = NocLastMip(TEXTURE2D_ARGS(tex, samp));
    hw = NocHexWeightsFlat(h);
    [branch] if (h.fade > 0.0)
    {
        float4 c0 = 0.0, c1 = 0.0, c2 = 0.0;
        [branch] if (h.w.x > 0.0) c0 = SAMPLE_TEXTURE2D_GRAD(tex, samp, h.uv0, h.dx0, h.dy0);
        [branch] if (h.w.y > 0.0) c1 = SAMPLE_TEXTURE2D_GRAD(tex, samp, h.uv1, h.dx1, h.dy1);
        [branch] if (h.w.z > 0.0) c2 = SAMPLE_TEXTURE2D_GRAD(tex, samp, h.uv2, h.dx2, h.dy2);
        const float3 Lw = float3(0.2126, 0.7152, 0.0722);
        hw = NocHexWeightsFromLum(h, float3(dot(c0.rgb, Lw), dot(c1.rgb, Lw), dot(c2.rgb, Lw)));
        float4 c = hw.col.x * c0 + hw.col.y * c1 + hw.col.z * c2;
        m = lerp(m, c, h.fade);
    }
    return m;
}
float4 NocHexColourArr(NocHex h, TEXTURE2D_ARRAY_PARAM(tex, samp), float slice, out NocHexW hw)
{
    float4 m = NocLastMipArr(TEXTURE2D_ARRAY_ARGS(tex, samp), slice);
    hw = NocHexWeightsFlat(h);
    [branch] if (h.fade > 0.0)
    {
        float4 c0 = 0.0, c1 = 0.0, c2 = 0.0;
        [branch] if (h.w.x > 0.0) c0 = SAMPLE_TEXTURE2D_ARRAY_GRAD(tex, samp, h.uv0, slice, h.dx0, h.dy0);
        [branch] if (h.w.y > 0.0) c1 = SAMPLE_TEXTURE2D_ARRAY_GRAD(tex, samp, h.uv1, slice, h.dx1, h.dy1);
        [branch] if (h.w.z > 0.0) c2 = SAMPLE_TEXTURE2D_ARRAY_GRAD(tex, samp, h.uv2, slice, h.dx2, h.dy2);
        const float3 Lw = float3(0.2126, 0.7152, 0.0722);
        hw = NocHexWeightsFromLum(h, float3(dot(c0.rgb, Lw), dot(c1.rgb, Lw), dot(c2.rgb, Lw)));
        float4 c = hw.col.x * c0 + hw.col.y * c1 + hw.col.z * c2;
        m = lerp(m, c, h.fade);
    }
    return m;
}

// Height-aware weights from the mask's B (height) at the three taps; returns the blended mask too (moss, ground).
float4 NocHexMaskHeight(NocHex h, TEXTURE2D_PARAM(tex, samp), out NocHexW hw)
{
    float4 m = NocLastMip(TEXTURE2D_ARGS(tex, samp));
    hw = NocHexWeightsFlat(h);
    [branch] if (h.fade > 0.0)
    {
        float4 c0 = 0.0, c1 = 0.0, c2 = 0.0;
        [branch] if (h.w.x > 0.0) c0 = SAMPLE_TEXTURE2D_GRAD(tex, samp, h.uv0, h.dx0, h.dy0);
        [branch] if (h.w.y > 0.0) c1 = SAMPLE_TEXTURE2D_GRAD(tex, samp, h.uv1, h.dx1, h.dy1);
        [branch] if (h.w.z > 0.0) c2 = SAMPLE_TEXTURE2D_GRAD(tex, samp, h.uv2, h.dx2, h.dy2);
        hw = NocHexWeightsFromHeight(h, float3(c0.b, c1.b, c2.b));
        float4 c = hw.col.x * c0 + hw.col.y * c1 + hw.col.z * c2;
        m = lerp(m, c, h.fade);
    }
    return m;
}

// Any further map (mask, overlay) blended with the colour weights hw.col.
float4 NocHexMask(NocHex h, NocHexW hw, TEXTURE2D_PARAM(tex, samp))
{
    float4 m = NocLastMip(TEXTURE2D_ARGS(tex, samp));
    [branch] if (h.fade > 0.0) m = lerp(m, NocHexTaps(h, hw.col, TEXTURE2D_ARGS(tex, samp)), h.fade);
    return m;
}
float4 NocHexMaskArr(NocHex h, NocHexW hw, TEXTURE2D_ARRAY_PARAM(tex, samp), float slice)
{
    float4 m = NocLastMipArr(TEXTURE2D_ARRAY_ARGS(tex, samp), slice);
    [branch] if (h.fade > 0.0) m = lerp(m, NocHexTapsArr(h, hw.col, TEXTURE2D_ARRAY_ARGS(tex, samp), slice), h.fade);
    return m;
}

// Height slope (dh/du, dh/dv) in the uv frame: each tap's slope rotated back by its vertex rotation, blended with hw.nrm, x fade.
float2 NocHexSlope(NocHex h, NocHexW hw, TEXTURE2D_PARAM(tex, samp), float strength)
{
    float2 g = 0.0;
    [branch] if (h.fade > 0.0)
    {
        [branch] if (hw.nrm.x > 0.0) g += hw.nrm.x * NocRotCol(h.rot0, NocTangentToSlope(SAMPLE_TEXTURE2D_GRAD(tex, samp, h.uv0, h.dx0, h.dy0), strength));
        [branch] if (hw.nrm.y > 0.0) g += hw.nrm.y * NocRotCol(h.rot1, NocTangentToSlope(SAMPLE_TEXTURE2D_GRAD(tex, samp, h.uv1, h.dx1, h.dy1), strength));
        [branch] if (hw.nrm.z > 0.0) g += hw.nrm.z * NocRotCol(h.rot2, NocTangentToSlope(SAMPLE_TEXTURE2D_GRAD(tex, samp, h.uv2, h.dx2, h.dy2), strength));
        g *= h.fade;
    }
    return g;
}
float2 NocHexSlopeArr(NocHex h, NocHexW hw, TEXTURE2D_ARRAY_PARAM(tex, samp), float slice, float strength)
{
    float2 g = 0.0;
    [branch] if (h.fade > 0.0)
    {
        [branch] if (hw.nrm.x > 0.0) g += hw.nrm.x * NocRotCol(h.rot0, NocTangentToSlope(SAMPLE_TEXTURE2D_ARRAY_GRAD(tex, samp, h.uv0, slice, h.dx0, h.dy0), strength));
        [branch] if (hw.nrm.y > 0.0) g += hw.nrm.y * NocRotCol(h.rot1, NocTangentToSlope(SAMPLE_TEXTURE2D_ARRAY_GRAD(tex, samp, h.uv1, slice, h.dx1, h.dy1), strength));
        [branch] if (hw.nrm.z > 0.0) g += hw.nrm.z * NocRotCol(h.rot2, NocTangentToSlope(SAMPLE_TEXTURE2D_ARRAY_GRAD(tex, samp, h.uv2, slice, h.dx2, h.dy2), strength));
        g *= h.fade;
    }
    return g;
}

// ===================================================================================================== world triplanar (mode 0)

// Plane weights |n|^4, normalised, cut at 0.01 (w' = max(w - cut, 0), renormalised: a zero plane is skipped). Plane coordinates
// in metres: X = (P.z, P.y), Y = (P.x, P.z), Z = (P.x, P.y) (the volume gradient of a plane's slope s is X (0, s.y, s.x),
// Y (s.x, 0, s.y), Z (s.x, s.y, 0): the height field h(P) of each plane is consistent on both sides, so no sign flip).
struct NocTri
{
    float3 w;
    float3 P, dPdx, dPdy;
};

// Takes the derivatives of P: call before any divergent branch.
NocTri NocTriSetup(float3 P, float3 n)
{
    NocTri t;
    float3 w = pow(abs(n), NC_TexTriPow);
    w /= max(w.x + w.y + w.z, 1e-7);
    w = max(w - NC_TexTriCut, 0.0);
    t.w = w / max(w.x + w.y + w.z, 1e-7);
    t.P = P;
    t.dPdx = ddx(P);
    t.dPdy = ddy(P);
    return t;
}

// Plane k (0 = X, 1 = Y, 2 = Z): metric coordinate and its gradients.
void NocTriPlane(NocTri t, int k, out float2 c, out float2 dx, out float2 dy)
{
    if (k == 0)      { c = t.P.zy; dx = t.dPdx.zy; dy = t.dPdy.zy; }
    else if (k == 1) { c = t.P.xz; dx = t.dPdx.xz; dy = t.dPdy.xz; }
    else             { c = t.P.xy; dx = t.dPdx.xy; dy = t.dPdy.xy; }
}
float3 NocTriVolume(int k, float2 s) { return k == 0 ? float3(0.0, s.y, s.x) : (k == 1 ? float3(s.x, 0.0, s.y) : float3(s.x, s.y, 0.0)); }

// Result of a full material lookup: albedo variation (rgb), mask, the surface gradient and the fade (mean over the planes).
struct NocTexResult
{
    float3 albedo;
    float4 mask;
    float3 sg;
    float fade;
};

// World triplanar of the three maps, each active plane hex-tiled (rate, rotate, seed + plane), normals as one surface gradient.
// tile in metres. Up to 3 planes x 3 taps per map; a single plane (walls, floors) runs 3 taps per map, often 1-2.
NocTexResult NocTriSample(NocTri t, float3 n, float tile, float rate, float rotate, uint seed, float strength,
                          TEXTURE2D_PARAM(albedoTex, albedoSamp), TEXTURE2D_PARAM(normalTex, normalSamp), TEXTURE2D_PARAM(maskTex, maskSamp))
{
    NocTexResult r;
    r.albedo = 0.0; r.mask = 0.0; float3 vg = 0.0; r.fade = 0.0;
    float inv = 1.0 / max(tile, 1e-4);
    [unroll] for (int k = 0; k < 3; k++)
    {
        float wk = t.w[k];
        [branch] if (wk > 0.0)
        {
            float2 c, dx, dy;
            NocTriPlane(t, k, c, dx, dy);
            float fade = NocTexFade(tile, NocFootprintBox(dx, dy));
            NocHex h = NocHexSetup(c * inv, dx * inv, dy * inv, NC_TexHexRate, rotate, seed + (uint)k * 0x9E3779B9u, fade);
            NocHexW hw;
            r.albedo += wk * NocHexColour(h, TEXTURE2D_ARGS(albedoTex, albedoSamp), hw).rgb;
            r.mask += wk * NocHexMask(h, hw, TEXTURE2D_ARGS(maskTex, maskSamp));
            vg += wk * NocTriVolume(k, NocHexSlope(h, hw, TEXTURE2D_ARGS(normalTex, normalSamp), strength));
            r.fade += wk * fade;
        }
    }
    r.sg = NocSurfGradVolume(n, vg);
    return r;
}

// ===================================================================================================== module shuffle (gridded sets)

// Gridded near sets (granite, pavers, garden tiles, wall tiles, brick): the texture holds counts = tile / module whole modules
// (texprep's lattice registration puts module boundaries in the middle of the joints). Module (i, j) of the world (rows shifted
// by half a module on odd j for running bond) takes a source module hash(i, j, seed) mod counts (same row parity for running
// bond, so the bond stays continuous), uvTex = (uvM - (i - si, j - sj) module) / tile, in [0, 1): one tap, SampleGrad with the
// continuous gradients of uvM / tile, the seam inside the joint. tone and smooth: the per-module offsets (+-6 %, +-0.04), faded
// to 0 below 2-4 module pixels.
struct NocModule
{
    float2 uv, dx, dy;
    float tone;      // albedo factor (1 +- 0.06, faded to 1)
    float smooth;    // smoothness offset (+-0.04, faded to 0)
    uint hash;
    float fade;
};

NocModule NocModuleSetup(float2 uvM, float2 dMdx, float2 dMdy, float2 module, float2 counts, float tile, float runningBond, uint seed)
{
    NocModule o;
    float2 m = max(module, 1e-4);
    float j = floor(uvM.y / m.y);
    float odd = runningBond > 0.5 ? abs(j - 2.0 * floor(0.5 * j)) : 0.0;     // 1 on odd rows of a running bond
    float i = floor((uvM.x - 0.5 * m.x * odd) / m.x);
    uint3 h = NocPcg3d(uint3((uint)(int)i, (uint)(int)j, seed));
    float cx = max(round(counts.x), 1.0), cy = max(round(counts.y), 1.0);
    float si = (float)(h.x % (uint)cx);
    float sj = runningBond > 0.5 ? 2.0 * (float)(h.y % (uint)max(floor(0.5 * cy), 1.0)) + odd : (float)(h.y % (uint)cy);
    float inv = 1.0 / max(tile, 1e-4);
    o.uv = (uvM - float2(i - si, j - sj) * m) * inv;
    o.dx = dMdx * inv;
    o.dy = dMdy * inv;
    float fp = NocFootprintBox(dMdx, dMdy);
    float wm = smoothstep(NC_TexModuleFadePx0, NC_TexModuleFadePx1, min(m.x, m.y) / max(fp, 1e-7));
    o.tone = 1.0 + NC_TexModuleTone * (2.0 * NocU01(h.z) - 1.0) * wm;
    o.smooth = NC_TexModuleSmooth * (2.0 * NocU01(h.x ^ h.y) - 1.0) * wm;
    o.hash = h.z;
    o.fade = NocTexFade(tile, fp);
    return o;
}

// ===================================================================================================== panel shuffle (facade skins)

// Cell-aligned panel shuffle (TEXTURE_PLAN 1.6, 2.5): cell-local metres plus a hashed offset of a whole tile range, one tap with
// the continuous gradients of (u, v) / tile; the seam between two panels lies inside the analytic panel joint.
float2 NocPanelUV(float2 localMetres, uint panelHash, float tile)
{
    uint2 h = NocPcg3d(uint3(panelHash, 0x50414E4Cu, 0x9E3779B9u)).xy;
    return localMetres / max(tile, 1e-4) + float2(NocU01(h.x), NocU01(h.y));
}
// Per-panel tone (+-12 %) faded to 1 by smoothstep(2, 4, panel px).
float NocPanelTone(uint panelHash, float panelPx)
{
    float u = NocU01(NocPcg(panelHash ^ 0x544F4E45u));
    return 1.0 + NC_TexPanelTone * (2.0 * u - 1.0) * smoothstep(NC_TexPanelFadePx0, NC_TexPanelFadePx1, panelPx);
}

// ===================================================================================================== UV0 metres (mode 1)

// Tangent frame of a metric UV0 from screen derivatives (Mikkelsen 2020, GenBasisTB): T along +u, B along +v, both in the
// surface; the mesh needs no tangents. Takes derivatives: call before any divergent branch.
void NocUvFrame(float2 uv, float3 P, float3 n, out float3 T, out float3 B)
{
    float3 dPdx = ddx(P), dPdy = ddy(P);
    float2 dUVdx = ddx(uv), dUVdy = ddy(uv);
    float3 sx = dPdx - n * dot(n, dPdx);
    float3 sy = dPdy - n * dot(n, dPdy);
    float det = dUVdx.x * dUVdy.y - dUVdx.y * dUVdy.x;
    float sd = det < 0.0 ? -1.0 : 1.0;
    float3 t = sx * (sd * dUVdy.y) + sy * (-sd * dUVdx.y);
    float3 b = sx * (-sd * dUVdy.x) + sy * (sd * dUVdx.x);
    float lt = dot(t, t), lb = dot(b, b);
    float3 fb = abs(n.y) < 0.999 ? float3(0.0, 1.0, 0.0) : float3(1.0, 0.0, 0.0);
    T = lt > 1e-20 ? t * rsqrt(lt) : normalize(cross(fb, n));
    B = lb > 1e-20 ? b * rsqrt(lb) : cross(n, T);
}

#endif
