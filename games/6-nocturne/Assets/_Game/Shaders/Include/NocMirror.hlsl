// NocMirror.hlsl - the analytic wet-floor mirror of the plane y 0 (MASTER_PROMPT 14.7 H4, D-003, R1 section 2). Every walkable
// surface is the plane y 0, so the mirror is exact: no SSR, no probe. For a floor fragment P seen from the eye:
//   lobe   r = reflect(v, n) (r.y >= 0.01), F = refl (0.03 + 0.97 (1 - n.v)^5), G1 Smith-GGX; the lobe is a 2D Gaussian in the
//          frame (X = cross(n, r), Y = cross(r, X)) with in-plane variance sigma2 = alpha^2 + 0.004 and lateral variance
//          sigma2 (n.v)^2 (the anisotropic warp of the microfacet lobe: wet streets streak vertically, D-003), plus the pixel
//          footprint of r (specular AA, Kaplanyan / Tokuyoshi, PIXVAR 0.25, trace clamp 0.36) and a 0.75 px lateral floor;
//   (a) sky   NocSkyColorBlur(r, sqrt(Syy)) (the cloud octaves fade by the lobe footprint);
//   (b) walls up to 4 finite vertical rectangles of the beat (NocWetRegistry), first hit in t, edges softened by the lobe
//          footprint on the wall, macro colour (facade strata or a flat emitter), mirror-consistent fog toward NocSkyColor(r);
//          FX (INT-13, D-032 3): on the floor a facade plane shows its lit windows (NocWallWindows, filtered by the lobe's
//          lateral and vertical footprints apart: vertical streaks) x NC_SurfWallGain; a pixel past the puddle fade mixes
//          the puddle and film lobes (NocMirrorEnvMix / NocMirrorSourcesMix) instead of one lobe of the blended variance;
//   (c) area sources (<= 32, NocWetRegistry): segments or discs with glow half-width rho and L_eff <= 3; Karis eq. 19 gives the
//          representative distance, the segment is clipped to the r hemisphere, the closed-form Gaussian mass (one exp, two
//          erf) integrates the lobe over the source, so a reflection is never brighter than its emitter (no fireflies);
//          reflection fog exp(-sigma^2 t (t + 2 d)) makes the total exactly the fog of the virtual image.
// The C# double mirror is NocMirrorMath (WetFloorTests). No URP point-light specular on NocWet (NOC_SPECULAR_OFF).
#ifndef NOC_MIRROR_INCLUDED
#define NOC_MIRROR_INCLUDED

#include "NocAtmosphere.hlsl"
#include "../Beats/NocScreenWave.hlsl"                 // kind 2: the LED screen wall of beat 6 (SG; _NocScreenT from SignsDriver)
#include "../Generated/NocSurfaceConstantsGen.hlsl"   // Look v2 (SU): NC_Surf*
#include "../Generated/NocFacadeConstantsGen.hlsl"    // NC_FaLit: the lit share of the wall planes' windows (FX, INT-13)

#define NOC_SRC_MAX 32
#define NOC_WALL_MAX 4

float4 _NocSrcP0[NOC_SRC_MAX];   // xyz end 0 (m), w = glow half-width rho (m)
float4 _NocSrcP1[NOC_SRC_MAX];   // xyz end 1 (= end 0 for a disc)
float4 _NocSrcL[NOC_SRC_MAX];    // rgb = L_eff x crossfade weight (linear, <= 3)
float  _NocSrcCount;
float4 _NocWallA[NOC_WALL_MAX];  // centre x, z; outward normal x, z
float4 _NocWallB[NOC_WALL_MAX];  // half length, top y, kind (0 facade macro, 1 flat, 2 LED screen wave), crossfade weight
float4 _NocWallC[NOC_WALL_MAX];  // macro radiance (linear)
float  _NocWallCount;

static const float NOC_INV_2PI = 0.15915494;

// Erf, Winitzki 2008 (a = 0.147, |err| < 1.3e-4): the segment mass (R1 2.3).
float NocErfW(float x)
{
    float x2 = x * x, ax2 = 0.147 * x2;
    float s = x < 0.0 ? -1.0 : 1.0;
    return s * sqrt(max(1.0 - exp(-x2 * (1.2732395 + ax2) / (1.0 + ax2)), 0.0));
}

// Smith-GGX masking G1 (URP Core G_MaskingSmithGGX in float).
float NocG1(float nv, float alpha)
{
    nv = max(nv, 1e-3);
    return 1.0 / (0.5 + 0.5 * sqrt(1.0 + alpha * alpha * (1.0 / (nv * nv) - 1.0)));
}

struct NocLobe
{
    float3 r, X, Y;
    float3 S;       // Sxx, Syy, Sxy (rad^2)
    float F;        // reflectivity x Fresnel
    float FG;       // F x G1
    float nv;
};

// The lobe of a floor fragment. Derivatives are taken here: call it once per pixel, outside any branch.
NocLobe NocMirrorLobe(float3 P, float3 eye, float3 n, float sigma2, float reflectivity)
{
    NocLobe o;
    float3 v = normalize(P - eye);                                       // eye -> floor
    float3 r = reflect(v, n);
    r.y = max(r.y, 0.01);                                                // never below the horizon
    o.r = normalize(r);
    float nv = saturate(dot(n, -v));
    o.nv = nv;
    float3 X = cross(n, o.r);
    float lx = length(X);
    o.X = lx > 1e-4 ? X / lx : normalize(cross(float3(0, 1, 0), v + float3(0, 0, 1e-3)));
    o.Y = cross(o.r, o.X);
    float3 drx = ddx(o.r), dry = ddy(o.r);
    float2 ja = float2(dot(drx, o.X), dot(drx, o.Y)), jb = float2(dot(dry, o.X), dot(dry, o.Y));
    float3 saa = NC_WetPixVar * float3(ja.x * ja.x + jb.x * jb.x, ja.y * ja.y + jb.y * jb.y, ja.x * ja.y + jb.x * jb.y);
    saa *= min(1.0, NC_WetAaClamp / max(saa.x + saa.y, 1e-12));
    float lat = NC_WetLatMinPx * NocPixelAngle();
    o.S = float3(max(sigma2 * nv * nv + saa.x, lat * lat), sigma2 + saa.y, saa.z);
    o.F = reflectivity * (NC_WetF0 + (1.0 - NC_WetF0) * PositivePow(1.0 - nv, 5.0));
    o.FG = o.F * NocG1(nv, sqrt(0.5 * sigma2));
    return o;
}

// Gaussian mass of the lobe over a source: a segment p0 -> p1 (gnomonic coordinates) of angular width w, or a disc.
float NocSegmentMass(float2 p0, float2 p1, float3 S, float w)
{
    float2 dlt = p1 - p0;
    float len = length(dlt);
    bool disc = len < 1e-3 * w;
    float sw = disc ? 0.125 * w * w : NOC_INV_2PI * w * w;               // peak-matched width variance
    float sxx = S.x + sw, syy = S.y + sw, sxy = S.z;
    float det = max(sxx * syy - sxy * sxy, 1e-20);
    float3 Si = float3(syy, sxx, -sxy) / det;
    float norm = NOC_INV_2PI * rsqrt(det);
    if (disc)
    {
        float q = p0.x * p0.x * Si.x + p0.y * p0.y * Si.y + 2.0 * p0.x * p0.y * Si.z;
        return saturate(0.78539816 * w * w * norm * exp(-0.5 * q));
    }
    float2 t = dlt / len;
    float a = max(t.x * t.x * Si.x + t.y * t.y * Si.y + 2.0 * t.x * t.y * Si.z, 1e-12);
    float b = t.x * p0.x * Si.x + t.y * p0.y * Si.y + (t.x * p0.y + t.y * p0.x) * Si.z;
    float2 m = p0 - (b / a) * t;
    float qp = m.x * m.x * Si.x + m.y * m.y * Si.y + 2.0 * m.x * m.y * Si.z;
    float k = rsqrt(2.0 * a);
    float lineInt = exp(-0.5 * qp) * sqrt(1.5707963 / a) * (NocErfW((a * len + b) * k) - NocErfW(b * k));
    return saturate(w * lineInt * norm);
}

// Extinction coefficient for reflected paths near the ground (regional sigma + the alley haze at its ground value).
float NocMirrorSigma() { return _NocFog.x + _NocFog.y; }

// (c) the area sources, already x F G1. A floor pixel holds a share m of puddle (lobe oP) and 1 - m of the rain film (lobe oD):
// its specular is the mixture m oP + (1 - m) oD (FX, INT-13 / D-032 3). Past the puddle fade a pixel covers both; one lobe of
// the blended variance would spread the puddles' sharp streaks into the film's wide lobe (peak x (sP / sMix)^2, 0.15 at m 0.35)
// and lose their G1 at grazing too. Both lobes share the frame (r, X, Y: one normal), so the source's clipped, projected segment
// is computed once and only its Gaussian mass twice. oP = oD with m = 1 is the single lobe.
// rhoMax (FX, C06): NocGlass mirrors the sources as their lamp-sized cores (the registered rho is the wet floor's glow width).
float3 NocMirrorSourcesMixR(float3 P, float dEye, NocLobe oP, NocLobe oD, float m, float rhoMax)
{
    float3 sum = 0;
    float sig2 = NocMirrorSigma() * NocMirrorSigma();
    float wP = m * oP.FG, wD = (1.0 - m) * oD.FG;
    int count = (int)min(_NocSrcCount, (float)NOC_SRC_MAX);
    [loop] for (int i = 0; i < count; i++)
    {
        float3 A = _NocSrcP0[i].xyz - P, B = _NocSrcP1[i].xyz - P, Ld = B - A;
        float rLd = dot(oP.r, Ld);
        float tq = saturate((dot(oP.r, A) * rLd - dot(A, Ld)) / max(dot(Ld, Ld) - rLd * rLd, 1e-6));  // Karis eq. 19
        float dq = max(length(A + tq * Ld), 1e-2);
        float la = dot(A, oP.r), lb = dot(B, oP.r), lc = 0.1 * dq;                                       // clip to the r hemisphere
        if (max(la, lb) <= lc) continue;
        float3 Ac = la < lc ? lerp(A, B, (lc - la) / (lb - la)) : A;
        float3 Bc = lb < lc ? lerp(B, A, (lc - lb) / (la - lb)) : B;
        float2 p0 = float2(dot(Ac, oP.X), dot(Ac, oP.Y)) / dot(Ac, oP.r);                                // gnomonic
        float2 p1 = float2(dot(Bc, oP.X), dot(Bc, oP.Y)) / dot(Bc, oP.r);
        float w = 2.0 * min(_NocSrcP0[i].w, rhoMax) / dq;
        float mass = wP * NocSegmentMass(p0, p1, oP.S, w) + wD * NocSegmentMass(p0, p1, oD.S, w);
        float Tr = exp(-sig2 * dq * (dq + 2.0 * dEye));                                                  // mirror-consistent fog
        sum += _NocSrcL[i].rgb * (mass * Tr);
    }
    return sum * NC_SurfSrcGain;
}

float3 NocMirrorSourcesMix(float3 P, float dEye, NocLobe oP, NocLobe oD, float m) { return NocMirrorSourcesMixR(P, dEye, oP, oD, m, 1e6); }
float3 NocMirrorSources(float3 P, float dEye, NocLobe o) { return NocMirrorSourcesMixR(P, dEye, o, o, 1.0, 1e6); }

// Macro radiance of a reflected wall at hit H: kind 0 = facade (its strata every 24 floors, faded by the footprint), 1 = flat,
// 2 = the LED screen wall (the 14.5.6 colour wave at the hit, scaled to the registered mean; periods 360-700 m need no footprint).
float3 NocWallMacro(float3 H, float kind, float3 colour, float soft)
{
    if (kind > 1.5) return NocScreenMirror(H, colour);
    if (kind > 0.5) return colour;
    float k = 6.2831853 / 86.4;                                                       // 24 floors x 3.6 m
    float amp = 0.25 * exp(-0.5 * (k * soft) * (k * soft));
    return colour * (1.0 + amp * cos(k * H.y));
}

// FX (INT-13, D-032 3): the lit windows of a facade wall plane (kind 0) in the wet mirror, mean 1 (NocSurfaceConstants SurfWall*;
// C# twin NocMirrorMath.WallWindows). u, y: metres on the wall; sU, sV: the lobe's footprint half-widths on it (m), apart, so the
// windows streak vertically; seed: the wall's index.
float NocWallPulseInt(float x, float w)                     // integral of a periodic pulse of duty w centred in each cell
{
    float fl = floor(x);
    return fl * w + clamp(x - fl - (0.5 - 0.5 * w), 0.0, w);
}
float NocWallShare(float c, float g, int seed)             // the lit share of bay c in group g: A + B h^3, mean 1
{
    float h = NocU01(NocPcg2((int)(2.0 * c + 1.0), (int)g + seed * 7919));
    return NC_SurfWallShareA + NC_SurfWallShareB * h * h * h;
}
float NocWallWindows(float u, float y, float sU, float sV, int seed)
{
    float x = u / NC_SurfWallBayU, c = floor(x);
    float gy = y / (NC_SurfWallFloorV * NC_SurfWallGroupFloors) - 0.5, g = floor(gy), fg = gy - g;
    fg = fg * fg * (3.0 - 2.0 * fg);
    float q = lerp(NocWallShare(c, g, seed), NocWallShare(c, g + 1.0, seed), fg);
    float nB = max(1.0, 2.0 * sV / (NC_SurfWallFloorV * NC_SurfWallGroupFloors)) * max(1.0, 2.0 * sU / NC_SurfWallBayU);
    float qB = 1.0 + (q - 1.0) * rsqrt(nB);
    float yv = y / NC_SurfWallFloorV, j = floor(yv);
    float lit = NocU01(NocPcg2((int)(2.0 * c), (int)j + seed * 7919)) < NC_FaLit * q ? 1.0 : 0.0;
    float s = max(sV / NC_SurfWallFloorV, 1e-3), a = 0.5 - 0.5 * NC_SurfWallWinV;
    float covV = (clamp(yv - j + s, a, 1.0 - a) - clamp(yv - j - s, a, 1.0 - a)) / (2.0 * s);
    float V = lerp(qB, lit * covV / (NC_FaLit * NC_SurfWallWinV), saturate(2.0 - 4.0 * sV / NC_SurfWallFloorV));
    float su = max(sU / NC_SurfWallBayU, 1e-3);
    float covU = max(0.0, NocWallPulseInt(x + su, NC_SurfWallWinU) - NocWallPulseInt(x - su, NC_SurfWallWinU)) / (2.0 * su);   // >= 0 (round-off)
    return covU / NC_SurfWallWinU * V;
}

// The first registered wall plane hit by the ray P + t r (t > 0): rgb = its macro radiance fogged toward `sky` along the
// mirror-consistent path (exp(-sigma^2 t (t + 2 dEye))), a = its coverage. softRad is the angular half-width (rad) over which
// the rectangle's ends and top fade (the lobe width). Look v2 (-surf-v 2, TEXTURE_PLAN 3.3, CRIT0 #8): the fade is at least
// NC_SurfWallBlendRad (2.5 deg) wide, so a wall's end blends into the sky (or the next wall) over about 5 deg around its vanishing line
// instead of switching: a macro plane has no real edge to mirror. FA's glass reflects the canyon and Gulf walls through it
// (TEXTURE_PLAN 2.7; read-only include for FA). win 1 (the wet floor only, FX INT-13): a facade plane (kind 0) shows its lit
// windows (NocWallWindows over the lobe's vertical softRad and lateral softLat footprints) x NC_SurfWallGain.
float4 NocWallReflectEx(float3 P, float3 r, float softRad, float softLat, float dEye, float3 sky, float win)
{
    float sig2 = NocMirrorSigma() * NocMirrorSigma();
    float ang = max(softRad, NC_SurfWallBlendRad);                                  // TP-2: the v2 blend is the only one
    float rh = max(length(r.xz), 1e-4);
    // the nearest hit (t1) and, in v2, the one behind it (t2): a near wall's fading end shows the wall behind it, not the sky
    float t1 = 1e9, t2 = 1e9;
    float3 c1 = sky, c2 = sky;
    float a1 = 0, a2 = 0;
    int nw = (int)min(_NocWallCount, (float)NOC_WALL_MAX);
    [loop] for (int i = 0; i < nw; i++)
    {
        float4 A = _NocWallA[i], B = _NocWallB[i];
        float2 nrm = A.zw;
        float denom = r.x * nrm.x + r.z * nrm.y;
        if (denom > -1e-4) continue;                                                  // the ray moves away from this face
        float t = ((A.x - P.x) * nrm.x + (A.y - P.z) * nrm.y) / denom;
        if (t <= 0.0 || t >= t2) continue;
        float3 H = P + r * t;
        float2 U = float2(-nrm.y, nrm.x);
        float u = (H.x - A.x) * U.x + (H.z - A.y) * U.y;
        float soft = t * ang + 1e-3;
        float cov = saturate(0.5 + (B.x - abs(u)) / (2.0 * soft)) * saturate(0.5 + (B.y - H.y) / (2.0 * soft)) * B.w;
        if (cov <= 0.0) continue;
        float Tr = exp(-sig2 * t * (t + 2.0 * dEye));
        float3 mac = NocWallMacro(H, B.z, _NocWallC[i].rgb, t * softRad + 1e-3);
        // the lateral footprint on the wall: t x softLat stretched by the ray's incidence in plan (1 / cos, at most x 10)
        float wsU = t * softLat / max(-denom / rh, 0.1) + 1e-3;
        if (win > 0.5 && B.z < 0.5) mac *= NC_SurfWallGain * NocWallWindows(u, H.y, wsU, t * softRad + 1e-3, i);
        float3 col = lerp(sky, mac, Tr);
        if (t < t1) { t2 = t1; c2 = c1; a2 = a1; t1 = t; c1 = col; a1 = cov; }
        else { t2 = t; c2 = col; a2 = cov; }
    }
    float a = a1 + (1.0 - a1) * a2;                                                   // c1 over c2 over the sky
    return float4((a1 * c1 + (1.0 - a1) * a2 * c2) / max(a, 1e-6), a);
}

float4 NocWallReflect(float3 P, float3 r, float softRad, float dEye, float3 sky) { return NocWallReflectEx(P, r, softRad, softRad, dEye, sky, 0.0); }

// (a) + (b): sky and the first wall the lobe axis hits, not yet x F G1; the facade planes with their lit windows (FX, INT-13).
float3 NocMirrorEnv(float3 P, float dEye, NocLobe o)
{
    float sy = sqrt(o.S.y);
    float3 sky = NocSkyColorBlur(o.r, sy);
    float4 w = NocWallReflectEx(P, o.r, sy, sqrt(o.S.x), dEye, sky, 1.0);
    return lerp(sky, w.rgb, w.a);
}

// (a) + (b) of the puddle / film mixture (see NocMirrorSourcesMix), already x F G1; a lobe of zero weight is skipped (no
// derivatives below this point, so the branches are safe).
float3 NocMirrorEnvMix(float3 P, float dEye, NocLobe oP, NocLobe oD, float m)
{
    float wP = m * oP.FG, wD = (1.0 - m) * oD.FG;
    float3 e = 0;
    [branch] if (wP > 0.0) e += wP * NocMirrorEnv(P, dEye, oP);
    [branch] if (wD > 0.0) e += wD * NocMirrorEnv(P, dEye, oD);
    return e;
}

// ------------------------------------------------------------------ puddles and ripples (R1 2.6)
// Puddle noise: two rotated value-noise octaves (4.0 m, 1.3 m); its 65th percentile is NC_PuddleThreshold (35 % puddles).
float NocPuddleNoise(float2 xz)
{
    float2 a = xz / NC_PuddleScale0;
    float2 b = float2(0.8 * xz.x - 0.6 * xz.y, 0.6 * xz.x + 0.8 * xz.y) / NC_PuddleScale1 + 31.7;
    return 0.62 * NocValueNoise(a) + 0.38 * NocValueNoise(b);
}

// Look v2 puddle noise (TEXTURE_PLAN 3.3): two rotated octaves at NC_SurfPuddleScale0 / 1 (2.0 / 0.6 m; v1 4.0 / 1.3 m read as
// camouflage blots, CRIT0 #8, GB-10). NocWet adds the height bias 0.35 (0.5 - h) of the material's own height map, so joints and
// low spots fill first, and thresholds the sum at its row's measured _PuddleTh (35 % +- 2 %). Twin: NocSurfaceMath.PuddleNoise.
float NocPuddleNoise2(float2 xz)
{
    float2 a = float2(0.96 * xz.x + 0.28 * xz.y, -0.28 * xz.x + 0.96 * xz.y) / NC_SurfPuddleScale0 + 7.3;
    float2 b = float2(0.6 * xz.x - 0.8 * xz.y, 0.8 * xz.x + 0.6 * xz.y) / NC_SurfPuddleScale1 + 31.7;
    return 0.62 * NocValueNoise(a) + 0.38 * NocValueNoise(b);
}

// Look v2 rain ripples (TEXTURE_PLAN 3.3, CRIT0 #29): many small faint wavelets instead of 0.25 m target rings: one ring per
// NC_SurfRippleCell cell every NC_SurfRipplePeriod / rho s, radius NC_SurfRippleR0 + NC_SurfRippleSpeed age over a life of
// NC_SurfRippleLife (<= 0.155 m), wavelength NC_SurfRippleLambda, amplitude x Ref / (Ref + R) (the energy spreads over the
// ring). Every ring centre lies in the middle half of its cell, so the 2 x 2 cells around the nearest corner hold every ring
// that reaches xz (0.75 cell = 0.225 m >= R + 1.5 lambda = 0.185 m). Slope of the height field (xz); 0 when rho is 0. Twin: NocSurfaceMath.Ripple.
float2 NocRippleGrad2(float2 xz, float t, float rho)
{
    float2 g = 0;
    float cell = NC_SurfRippleCell;
    float2 base = floor(xz / cell + 0.5) - 1.0;
    float period = NC_SurfRipplePeriod / max(rho, 0.05);
    [unroll] for (int j = 0; j < 2; j++)
    [unroll] for (int i = 0; i < 2; i++)
    {
        int2 c = int2(base) + int2(i, j);
        uint h = NocPcg2(c.x + 7919, c.y - 104729);
        float2 centre = (float2(c) + 0.25 + 0.5 * float2(NocU01(h), NocU01(NocPcg(h + 11u)))) * cell;
        float age = fmod(t + NocU01(NocPcg(h + 23u)) * period, period);
        float life = saturate(1.0 - age / NC_SurfRippleLife);
        float R = NC_SurfRippleR0 + NC_SurfRippleSpeed * age;
        float2 dv = xz - centre;
        float r = length(dv);
        float x = (r - R) / NC_SurfRippleLambda;
        float s = exp(-1.5 * x * x) * sin(6.2831853 * x) * life * life * (NC_SurfRippleRef / (NC_SurfRippleRef + R));
        g += s * dv / max(r, 1e-4);
    }
    return g;
}

// Rain-ring slope (gradient of the height, xz) from the 2 x 2 cells around the nearest cell corner; 0 when rho is 0.
float2 NocRippleGrad(float2 xz, float t, float rho)
{
    float2 g = 0;
    float cell = NC_RippleCell;
    float2 base = floor(xz / cell + 0.5) - 1.0;
    float period = NC_RipplePeriod / max(rho, 0.05);
    [unroll] for (int j = 0; j < 2; j++)
    [unroll] for (int i = 0; i < 2; i++)
    {
        int2 c = int2(base) + int2(i, j);
        uint h = NocPcg2(c.x, c.y);
        float2 centre = (float2(c) + 0.25 + 0.5 * float2(NocU01(h), NocU01(NocPcg(h + 11u)))) * cell;
        float age = fmod(t + NocU01(NocPcg(h + 23u)) * period, period);
        float life = saturate(1.0 - age / NC_RippleLife);
        float R = NC_RippleR0 + NC_RippleSpeed * age;
        float2 dv = xz - centre;
        float r = length(dv);
        float x = (r - R) / NC_RippleLambda;
        float s = exp(-1.5 * x * x) * sin(6.2831853 * x) * life * life;
        g += s * dv / max(r, 1e-4);
    }
    return g;
}

#endif
