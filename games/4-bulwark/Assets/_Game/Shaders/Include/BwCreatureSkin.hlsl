// BwCreatureSkin.hlsl - the kraken's GPU spline skinning and skin (Addendum A1, kraken owner; used by BwCreature.shader).
//
// Skinning. Every limb (8 Tripo arms continued by procedural tentacles, 2 feeding tentacles) is a chain of K control points
// that KrakenRig evaluates on the CPU each frame (no per-frame mesh rebuild). One global StructuredBuffer _KrCtrl holds, per
// limb and point, three float4: the rest position (xyz), the current position (xyz) and the rotation q = q_cur * q_rest^-1
// (xyzw). A vertex stores its rest position (POSITION, kraken-local metres), its limb (COLOR.x; 255 = rigid head), its skin
// weight (COLOR.y) and its arc length u along the limb (TEXCOORD1.x); then
//     p' = lerp(p, Pcur(u) + q(u) (p - Prest(u)), w)            (Catmull-Rom positions, nlerp rotations)
// The control points are spaced non-uniformly (denser towards the thin tip): index(u) = (K - 1) (a u / L + (1 - a)
// (exp(u / lambda) - 1) / (exp(L / lambda) - 1)) with the per-limb (L, lambda, a, K) of _KrLimb (KrakenMath.ControlIndex is
// the C# twin; KrakenTests checks them against each other).
// Mantle: head vertices contract radially about the mantle axis by _KrMantle.x (the jet pulse) and breathe by _KrMantle.y.
// Everything is in the kraken's local space; unity_ObjectToWorld (the kraken root) places it in the world.
//
// Skin (fragment). Body: the graded Tripo atlas (albedo, tangent-space normal, mask R AO / G smoothness / B cavity / A socket
// glow); arms blend into the tentacle tile (the unwrapped Tripo tentacle) over the last 40 m before the cut, so the joint to
// the procedural tentacle carries the same skin. Tubes: the tile at (theta, v(u)), v(u) = lambda_r / (r0 TILE) (exp((u -
// u0) / lambda_r) - 1): suckers shrink with the radius. On both: barnacle crust (Worley cells in rest space, pale shells on
// the upper surfaces), long pale scars, wet slime (smoothness varied by noise), water sheets running down emerged parts,
// darker crevices, slow sickly bioluminescent spots, the eye-socket glow. Patterns use the REST position, so they stick to
// the skin while it moves, and fade by the pixel footprint (no shimmer).
#ifndef BW_CREATURE_SKIN_INCLUDED
#define BW_CREATURE_SKIN_INCLUDED

#include "../Generated/BulwarkConstGen.hlsl"

#define KR_MAX_LIMBS 32          // 10 limbs + 16 kelp streamers (KrakenCreature.MaxChains)
StructuredBuffer<float4> _KrCtrl;
float4 _KrLimb[KR_MAX_LIMBS];    // x length L (m), y map lambda (m), z map blend a, w control count K
float4 _KrLimb2[KR_MAX_LIMBS];   // x u0 (tube start / arm cut, m), y r0 (radius there, m), z taper span L - u0 (m), w wetness 0..1
float4 _KrMantle;                // x contraction 0..1 (x amplitude), y breathing -1..1, z head wetness, w socket glow gain
float4 _KrHead;                  // xyz mantle centre (local m), w mantle radius (m)
float4 _KrHeadAxis;              // xyz mantle axis (local, unit), w eye-line z (local m): contraction only behind it
float4 _KrPhase;                 // x cloud-band phase 0..1 (14.3 s), y water-sheet flow (m, unwrapped mod 512), z sucker-wave phase, w eye clamp (3 / 2^EV)
float4 _KrUnderwater;            // x weight 0..1, y sea surface y (world m), z emission kept (0..1), w in-air path length (m)
float4 _KrTileInfo;              // x tentacle tile length in radii, y relief scale, z unused, w unused

float4 _BwDamClip;               // global (KrakenDirector): x 1 = the camera is on the lee side (west of the dam), y camera x, z camera y

// Per-fragment outputs of the surface hooks that KrFrag reads (static: private to one fragment invocation; the body hook
// leaves the defaults). Fix wave X2: the pupil showed the water veil at full strength (9,43,80: "an open mouth", KS17).
static float gKrVeilKeep = 1.0;                 // share of the underwater veil kept in front of this fragment
static float3 gKrSpec = float3(0.0, 0.0, 0.0);  // pal:ok (zero) extra specular radiance (the cornea catchlight), outside the emission hue clamp
// Underwater optical-depth clamp per channel (fix wave X2, KS16: at r > ~30 m T -> 0 and the arm became the veil, a flat
// fog-coloured cut-out a little brighter than the water): T >= exp(-1.6) = 0.20 (G) and exp(-1.4) = 0.25 (B) keep the shading
// and a silhouette >= 6 levels darker than the water; red stays physical (exp(-4) = 0.018: no red at range under water)
#define KR_TAU_MAX float3(4.0, 1.6, 1.4) // pal:ok (optical depths, not a colour)

uint KrLimbIndex(float c) { return (uint)min(floor(c + 0.5), (float)(KR_MAX_LIMBS - 1)); }

// Dam clip for lee-side cameras (the stinger seen from the lantern platform 3.5 km west, A1 00 decision 1): the far field draws
// the lee face at the far plane without depth, so the creature (east of the dam) must hide itself wherever the dam stands
// between it and the camera. The dam is the slab TC_LeeFaceX .. TC_SeaFaceX for every z, its top the parapet line
// TC_ParapetTop: a fragment east of the sea face is hidden when the camera -> fragment line passes either face edge below the
// parapet top (for a camera below the crest the lee edge binds). Returns < 0 to clip. enable = the material's _DamClip.
float KrDamClipKeep(float3 positionWS, float enable)
{
    if (enable < 0.5 || _BwDamClip.x < 0.5) return 1.0;
    float cx = _BwDamClip.y, cy = _BwDamClip.z;
    float dx = positionWS.x - cx;
    // fix wave X2: parts lying ON the crest (lee face < x <= sea face) are tested too, against the lee edge (a dark ladder-like
    // sliver drew over the far lee face at the crest line, show_stinger_t28 x 975-1030, y 175-182); 0.5 m margin for the
    // far-field's crest line
    if (positionWS.x <= TC_LeeFaceX || dx <= 1e-3) return 1.0;
    float yLee = cy + (positionWS.y - cy) * saturate((TC_LeeFaceX - cx) / dx);
    float ySea = cy + (positionWS.y - cy) * saturate((TC_SeaFaceX - cx) / dx);
    return min(yLee, ySea) - (TC_ParapetTop + 0.5);
}

float KrControlIndex(float u, float4 L)
{
    float len = max(L.x, 1.0);
    float lam = max(L.y, 1.0);
    float x = saturate(u / len);
    float e = (exp(min(u, len) / lam) - 1.0) / max(exp(len / lam) - 1.0, 1e-6);
    return (L.w - 1.0) * (L.z * x + (1.0 - L.z) * saturate(e));
}

float4 KrFetch(uint limb, uint i, uint k, uint K)
{
    return _KrCtrl[(limb * K + min(i, K - 1)) * 3 + k];
}

float3 KrCatmull(float3 p0, float3 p1, float3 p2, float3 p3, float t)
{
    float t2 = t * t, t3 = t2 * t;
    return 0.5 * ((2.0 * p1) + (-p0 + p2) * t + (2.0 * p0 - 5.0 * p1 + 4.0 * p2 - p3) * t2 + (-p0 + 3.0 * p1 - 3.0 * p2 + p3) * t3);
}

float3 KrRotate(float4 q, float3 v)
{
    float3 t = 2.0 * cross(q.xyz, v);
    return v + q.w * t + cross(q.xyz, t);
}

// Skins a rest point / normal / tangent of limb `limb` at arc u: returns the skinned point (local).
float3 KrSkin(uint limb, float u, float3 p, inout float3 n, inout float3 t)
{
    float4 L = _KrLimb[limb];
    uint K = (uint)max(L.w, 4.0);
    float x = clamp(KrControlIndex(max(u, 0.0), L), 0.0, L.w - 1.0001);
    uint i = (uint)floor(x);
    float f = x - (float)i;
    uint im = i > 0 ? i - 1 : 0;
    uint i1 = min(i + 1, K - 1), i2 = min(i + 2, K - 1);
    float3 r0 = KrFetch(limb, im, 0, K).xyz, r1 = KrFetch(limb, i, 0, K).xyz, r2 = KrFetch(limb, i1, 0, K).xyz, r3 = KrFetch(limb, i2, 0, K).xyz;
    float3 c0 = KrFetch(limb, im, 1, K).xyz, c1 = KrFetch(limb, i, 1, K).xyz, c2 = KrFetch(limb, i1, 1, K).xyz, c3 = KrFetch(limb, i2, 1, K).xyz;
    float4 qa = KrFetch(limb, i, 2, K), qb = KrFetch(limb, i1, 2, K);
    if (dot(qa, qb) < 0.0) qb = -qb;
    float4 q = lerp(qa, qb, f);
    q *= rsqrt(max(dot(q, q), 1e-12));
    float3 pr = KrCatmull(r0, r1, r2, r3, f);
    float3 pc = KrCatmull(c0, c1, c2, c3, f);
    n = KrRotate(q, n);
    t = KrRotate(q, t);
    return pc + KrRotate(q, p - pr);
}

// The rest centre line of limb `limb` at arc u (kraken local), the Prest(u) of KrSkin. The relief tube (LOD0) uses it while it
// cross-fades out: its displaced-relief normals morph to the smooth radial normal, so the dithered mix with the smooth LOD1
// shades alike (kraken-25 kr_islet: 4 isolated bright pixels on a sunlit rim inside the LOD0/1 band; none with -kraken-lod 0).
float3 KrRestCentre(uint limb, float u)
{
    float4 L = _KrLimb[limb];
    uint K = (uint)max(L.w, 4.0);
    float x = clamp(KrControlIndex(max(u, 0.0), L), 0.0, L.w - 1.0001);
    uint i = (uint)floor(x);
    float f = x - (float)i;
    uint im = i > 0 ? i - 1 : 0;
    uint i1 = min(i + 1, K - 1), i2 = min(i + 2, K - 1);
    return KrCatmull(KrFetch(limb, im, 0, K).xyz, KrFetch(limb, i, 0, K).xyz, KrFetch(limb, i1, 0, K).xyz, KrFetch(limb, i2, 0, K).xyz, f);
}

// One PCG round per lattice point (BwPcg3 takes three): the hash of the skin's noises and the crust cells.
uint KrHash3(int3 c) { return BwPcg((asuint(c.x) * 0x8DA6B343u) ^ (asuint(c.y) * 0xD8163841u) ^ (asuint(c.z) * 0xCB1AB31Fu)); }

// Cheap gradient noise for the skin bumps: the 8 cube-diagonal gradients from the hash bits (BwGradNoise3 indexes a static
// table, which D3D11 compiles to an indexable temp: the 3-octave wart bump cost ~1 ms at 4K, kraken27 perf), and an fBm
// that skips the octaves the footprint has faded (their mean is 0); one PCG round per corner (BwPcg3 takes three).
// Range about [0.1, 0.9] around 0.5.
float KrGradCorner(int3 c, float3 d)
{
    uint h = KrHash3(c);   // one PCG round
    float3 g = float3((h & 1u) != 0u ? 1.0 : -1.0, (h & 2u) != 0u ? 1.0 : -1.0, (h & 4u) != 0u ? 1.0 : -1.0); // pal:ok (gradient)
    return dot(g, d);
}
float KrGradNoise3(float3 x)
{
    float3 fl = floor(x);
    int3 i = (int3)fl;
    float3 t = x - fl;
    float3 w = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    float a = KrGradCorner(i, t), b = KrGradCorner(i + int3(1, 0, 0), t - float3(1, 0, 0));   // pal:ok (lattice offsets)
    float c = KrGradCorner(i + int3(0, 1, 0), t - float3(0, 1, 0)), d = KrGradCorner(i + int3(1, 1, 0), t - float3(1, 1, 0));   // pal:ok
    float e = KrGradCorner(i + int3(0, 0, 1), t - float3(0, 0, 1)), g = KrGradCorner(i + int3(1, 0, 1), t - float3(1, 0, 1));   // pal:ok
    float h = KrGradCorner(i + int3(0, 1, 1), t - float3(0, 1, 1)), k = KrGradCorner(i + int3(1, 1, 1), t - float3(1, 1, 1));   // pal:ok
    return 0.5 * lerp(lerp(lerp(a, b, w.x), lerp(c, d, w.x), w.y), lerp(lerp(e, g, w.x), lerp(h, k, w.x), w.y), w.z);
}
float KrFbmGrad(float3 x, float fpCells, int octaves)
{
    float sum = 0.0, amp = 0.5, norm = 0.0, scale = 1.0;
    [unroll(4)] for (int o = 0; o < octaves; o++)
    {
        float wt = 1.0 - smoothstep(0.25, 0.5, fpCells * scale);
        float nv = 0.0;
        UNITY_BRANCH if (wt > 0.0) nv = wt * KrGradNoise3(x * scale + o * 17.31);
        sum += amp * nv;
        norm += amp;
        amp *= 0.5;
        scale *= 2.03;
    }
    return 0.5 + 0.75 * sum / max(norm, 1e-4);
}

// The skin's value noise and footprint-filtered fBm with the same semantics as BwValueNoise3 / BwFbmFiltered3 (a faded
// octave contributes its mean 0.5) but one PCG round per lattice corner and no work for faded octaves (Wave X 4K cost: the
// skin evaluates ~15 noises per pixel; BwHash31 takes three PCG rounds per corner).
float KrValueNoise3(float3 x)
{
    float3 fl = floor(x);
    int3 i = (int3)fl;
    float3 t = x - fl;
    float3 w = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    float a = BwU01(KrHash3(i)), b = BwU01(KrHash3(i + int3(1, 0, 0))), c = BwU01(KrHash3(i + int3(0, 1, 0))), d = BwU01(KrHash3(i + int3(1, 1, 0)));
    float e = BwU01(KrHash3(i + int3(0, 0, 1))), g = BwU01(KrHash3(i + int3(1, 0, 1))), h = BwU01(KrHash3(i + int3(0, 1, 1))), k = BwU01(KrHash3(i + int3(1, 1, 1)));
    return lerp(lerp(lerp(a, b, w.x), lerp(c, d, w.x), w.y), lerp(lerp(e, g, w.x), lerp(h, k, w.x), w.y), w.z);
}
float KrFbm(float3 x, float fpCells, int octaves)
{
    float sum = 0.0, amp = 0.5, norm = 0.0, scale = 1.0;
    [unroll(5)] for (int o = 0; o < octaves; o++)
    {
        float wt = 1.0 - smoothstep(0.25, 0.5, fpCells * scale);
        float nv = 0.5;
        UNITY_BRANCH if (wt > 0.0) nv = lerp(0.5, KrValueNoise3(x * scale + o * 17.31), wt);
        sum += amp * nv;
        norm += amp;
        amp *= 0.5;
        scale *= 2.03;
    }
    return sum / max(norm, 1e-4);
}

// Creased fBm for the hide's wrinkles: soft V-grooves 2|n| (2 sqrt(n^2 + 0.005), ~0.15 cell wide: no 1-px crease) of the cheap
// gradient noise, octaves skipped once faded (a faded octave contributes its mean). Range [0, ~1].
float KrFbmCrease(float3 x, float fpCells, int octaves)
{
    float sum = 0.0, amp = 0.5, norm = 0.0, scale = 1.0;
    [unroll(4)] for (int o = 0; o < octaves; o++)
    {
        float wt = 1.0 - smoothstep(0.25, 0.5, fpCells * scale);
        float nv = 0.45;
        UNITY_BRANCH if (wt > 0.0)
        {
            float g = KrGradNoise3(x * scale + o * 17.31);
            nv = lerp(0.45, 2.0 * sqrt(g * g + 0.005), wt);
        }
        sum += amp * nv;
        norm += amp;
        amp *= 0.5;
        scale *= 2.03;
    }
    return sum / max(norm, 1e-4);
}

// Mantle jet pulse and breathing: radial contraction about the mantle axis, behind the eye line only.
float3 KrMantle(float3 p, float rigid)
{
    float3 c = _KrHead.xyz;
    float3 ax = _KrHeadAxis.xyz;
    float3 d = p - c;
    float along = dot(d, ax);
    float3 radial = d - along * ax;
    float behind = smoothstep(_KrHeadAxis.w, _KrHeadAxis.w - 60.0, p.z);       // the mantle (towards -z), not the face
    float above = smoothstep(10.0, 60.0, p.y);
    float m = behind * above * rigid;
    float k = 0.11 * _KrMantle.x + 0.025 * _KrMantle.y;
    return p - radial * (k * m) + ax * (0.04 * _KrMantle.x * m * max(along, 0.0));
}

// ------------------------------------------------------------------------------------------------ skin helpers
float KrHash13(float3 p) { return BwHash31(int3(floor(p))); }

// Worley distance to the nearest of one jittered point per cell, returns (d1, cell hash). The points are jittered within
// [0.2, 0.8] of their cell, so only the 2 x 2 x 2 cells of the point's nearest octant can hold a point closer than 0.7: the
// result is exact wherever the crust uses it (shells and holes at d1 < 0.46) with 8 cells instead of 27 (Wave X: the 4K
// close-up of the head cost 2.2 ms).
float2 KrWorley(float3 x)
{
    float3 i = floor(x), fr = x - i;
    int3 o0 = int3(step(0.5, fr)) - 1;
    float best = 9.0, hid = 0.0;
    [unroll] for (int dz = 0; dz <= 1; dz++)
    [unroll] for (int dy = 0; dy <= 1; dy++)
    [unroll] for (int dx = 0; dx <= 1; dx++)
    {
        int3 k = o0 + int3(dx, dy, dz);
        int3 c = int3(i) + k;
        uint h = KrHash3(c);
        float3 o = 0.2 + 0.6 * float3(BwU01(h), BwU01(BwPcg(h)), BwU01(BwPcg(h + 7u))); // pal:ok (cell jitter, not a colour)
        float3 d = float3(k) + o - fr; // pal:ok (offset)
        float dd = dot(d, d);
        if (dd < best) { best = dd; hid = BwU01(BwPcg(h + 13u)); }
    }
    return float2(sqrt(best), hid);
}

// v coordinate of the tentacle tile at arc u of a limb: the integral of du / (TILE r(u)) of the power-law taper
// r = r0 (1 - x)^1.43, x = (u - u0) / span (A1 1.2); KrakenMath.TileV is the C# twin
#define KR_TAPER_P 1.43
float KrTileV(float u, float4 L2)
{
    float sp = max(L2.z, 1.0);
    float r0 = max(L2.y, 0.05);
    float x = min((u - L2.x) / sp, 0.985);
    return sp / (r0 * max(_KrTileInfo.x, 1.0)) * (pow(max(1.0 - x, 1e-3), 1.0 - KR_TAPER_P) - 1.0) / (KR_TAPER_P - 1.0);
}

// Underwater veil radiance (14.7 H3): L_c = G S_c exp(-Kd_c z) exp(beta sin e), red floor 6 % of G, hard floor.
float3 KrVeil(float3 dirWS, float depth)
{
    float3 Kd = float3(TC_KdR, TC_KdG, TC_KdB); // pal:ok (attenuation coefficients, not a colour)
    float3 S = float3(TC_VeilSR, TC_VeilSG, TC_VeilSB); // pal:ok (spectral weights, not a colour)
    float G = TC_VeilG0 / (TC_VeilSG * exp(-TC_KdG * TC_VeilG0Depth));
    float e = clamp(dirWS.y, -1.0, 1.0);
    float3 L = G * S * exp(-Kd * max(depth, 0.0)) * exp(TC_VeilBeta * e);
    L.r = max(L.r, TC_VeilRedFloor * L.g);
    return max(L, TC_DarkFloorLinear);
}

#endif
