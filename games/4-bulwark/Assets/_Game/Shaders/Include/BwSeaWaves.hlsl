// BwSeaWaves.hlsl - THE sea wave function of Bulwark (MASTER_PROMPT 14.7 H1, 14.2.6 BwSea). Owner: the sea owner.
// Every global below is written once per frame (and per rendering camera) by SeaState.cs, in double on the CPU; this file
// only reads them. Included by BwSea.shader (the near-sea clipmap) and, read-only, by BwFarField.shader (the sea sphere
// beyond the 2048 m clipmap), so both tiers use one spectrum and one roughness (Part 10 #34, T7).
//
// API (names and signatures are stable; no other include is required, everything is prefixed BwSea / BWSEA_):
//   float2 BwSeaLocal(float3 posWS)                  undisplaced plane coordinates l = posWS.xz - O (O = _BwSeaOrigin.xz)
//   float  BwSeaImageWeight(float worldX)             weight of the wall clapotis image wave (1 within 150 m, 0 at 400 m)
//   float3 BwSeaDisplace(float2 l, float sp, float imageW)
//                                                     Gerstner displacement (world offset, y relative to the mean level
//                                                     -8, mean-level corrected) of the point O + l for a vertex ring of
//                                                     spacing sp (per-ring weights 1 - smoothstep(0.17, 0.3, sp/lambda))
//   BwSeaSample BwSeaEvaluate(float2 l, float fp, float imageW)   (BwSeaEvaluateG: + a gust factor on the short waves;
//                                                     BwSeaEvaluateGC: + a slick factor on the 8-30 m wind sea)
//                                                     pixel stage: every component weighted by the footprint fp (m along
//                                                     the view) w = 1 - smoothstep(0.5, 1, 3 fp / lambda); returns the
//                                                     normal, the Jacobian (resolved and its unresolved std), the
//                                                     unresolved slope variance s2 = 0.008 + sum (1 - w^2)(a k)^2 / 2 and
//                                                     the resolved height
//   BwSeaSample BwSeaEvaluateAnisoGC(float2 l, float2 dl0, float2 dl1, float imageW, float gust, float slick)
//                                                     the same with ANISOTROPIC footprints: each component is band-limited
//                                                     by the pixel footprint projected on its own direction (dl0 / dl1 =
//                                                     ddx / ddy of l), at least 1/8 of the long axis (8:1, like 8x
//                                                     anisotropic texture filtering). The isotropic long-axis filter
//                                                     averaged every wave crossing the view away with the long ones and
//                                                     left brushed-metal streaks in the mid field (critic panel X2 S3-2);
//                                                     BwSea uses this one.
//   float  BwSeaFarS2()                               roughness of the fully filtered sea (all components unresolved):
//                                                     what the clipmap reaches at 2048 m; the far-field sea uses it
//   float  BwSeaFoamThreshold()                       Jacobian whitecap threshold J_t (2.0 % tail, see SeaState)
//   float  BwSeaFoamCover(BwSeaSample s, float fpJ)   (and ...At(s, fpJ, imageW) in the clapotis zone)
//                                                     whitecap coverage 0..1 from the Jacobian, filtered: the probability
//                                                     that the true (resolved + unresolved) J is below J_t
//   float  BwSeaReefOccupancy(float2 xz)              reef occupancy 0..1 for foam (beat 4 owner; 0 until it lands)
//   float  BwSeaCalm(float2 worldXZ)                  0..1 amplitude factor: 0 at the spillway mouths (x 46, z 424-700),
//                                                     1 beyond 40 m from them (the beat 3 draw-down water starts flat at
//                                                     y -8.0 at x 46; the sea meets it without a step)
// Phases: theta_i = k_i d_i . l + Phi_i with Phi_i = k_i d_i . O - omega_i t + phi0_i computed in double (no float jitter
// at z 1500 and none after an hour of ambient clock; _BwTime is not used). Components are sorted long to short inside each
// set, so a loop stops at the first fully filtered one. Two sets are present only inside the 30 m Rule-P windows.
#ifndef BW_SEA_WAVES_INCLUDED
#define BW_SEA_WAVES_INCLUDED

#define BWSEA_MAX 64              // SeaState.MaxComponents (two sets of 32)
#define BWSEA_SET 32              // SeaSpectrum.N
#define BWSEA_TWO_PI 6.2831853
#define BWSEA_FOAM_LMIN 3.0       // whitecaps form on the crests of the wind sea, 3-40 m (SeaSpectrum.FoamMinWavelength)
#define BWSEA_FOAM_LMAX 40.0      // (the long swell is not steep: it carries the whitecaps, it does not break; FoamMaxWavelength)
// Band limits (H1; rebalanced in the X2 fix wave, critic S3-2; twins SeaSpectrum.RingWeight / FootprintWeight):
// vertex ring weight 1 - smoothstep(0.17, 0.3, sp / lambda): whole above 5.9 vertices per wavelength, none below 3.3 (was
// 8 / 4: the 117-173 m swell round the spectral peak now stays in the 16 m and 32 m rings, so its crests stand up as
// silhouettes out to 2 km); pixel weight 1 - smoothstep(0.5, 1, 3 f / lambda): whole above 6 px per wavelength, zero at
// 3 px (was 8 / 4).
#define BWSEA_RING_R0 0.17
#define BWSEA_RING_R1 0.3
#define BWSEA_PIX_K 3.0
#define BWSEA_ANISO_MAX 8.0       // the footprint along a component is at least 1/8 of the pixel's long axis

float4 _BwSeaK[BWSEA_MAX];   // xy = k d (kx, kz), z = k, w = a (m, x sqrt of the set's energy weight)
float4 _BwSeaP[BWSEA_MAX];   // x = Phi (rad, phase at the origin), y = Phi' (wall image wave), z = a k, w = wavelength (m)
float4 _BwSeaInfo;           // x = components of set A, y = total components, z = Gerstner steepness Q, w = whitecap J_t
float4 _BwSeaOrigin;         // xyz = origin O (world; y = mean sea level -8), w = roughness floor 0.008
float4 _BwSeaWall;           // x = wall x (6), y = image fade start (150 m), z = fade end (400 m), w = full variance s2
float4 _BwSeaFoam;           // x = sigma of the whitecap Jacobian (3-40 m band), y = foam-streak drift (m), z = tail (2.05 sigma), w = spare
float4 _BwSeaSpill;          // x = spillway z0 (420), y = z1 (704), z = pier east end x (46), w = mean-level drop (m)

struct BwSeaSample
{
    float3 normal;     // unit, from the footprint-weighted slopes of the Gerstner surface
    float jac;         // resolved Jacobian of the horizontal map (1 = flat), components of 3 m and longer (whitecaps)
    float jacSigma;    // std of the unresolved part of that Jacobian
    float s2;          // slope variance for the Beckmann lobe (floor + unresolved)
    float height;      // resolved height above the mean level (m)
};

float2 BwSeaLocal(float3 posWS) { return posWS.xz - _BwSeaOrigin.xz; }

float BwSeaSmooth(float a, float b, float x) { float t = saturate((x - a) / (b - a)); return t * t * (3.0 - 2.0 * t); }

float BwSeaImageWeight(float worldX)
{
    return 1.0 - BwSeaSmooth(_BwSeaWall.y, _BwSeaWall.z, max(worldX - _BwSeaWall.x, 0.0));
}

// World x of the undisplaced local point (for the image weight).
float BwSeaWorldX(float2 l) { return l.x + _BwSeaOrigin.x; }

float3 BwSeaDisplace(float2 l, float sp, float imageW)
{
    float3 d = 0.0;
    float drop = 0.0;
    float Q = _BwSeaInfo.z;
    int nA = (int)_BwSeaInfo.x, nT = (int)_BwSeaInfo.y;
    [loop] for (int set = 0; set < 2; set++)
    {
        int i0 = set == 0 ? 0 : nA;
        int i1 = set == 0 ? nA : nT;
        [loop] for (int i = i0; i < i1; i++)
        {
            float4 K = _BwSeaK[i];
            float4 P = _BwSeaP[i];
            float r = sp / max(P.w, 1e-4);
            if (r >= BWSEA_RING_R1) break;                           // sorted long -> short: the rest are filtered too
            float w = 1.0 - BwSeaSmooth(BWSEA_RING_R0, BWSEA_RING_R1, r);
            float a = K.w * w;
            float2 dir = K.xy / max(K.z, 1e-6);
            float s, c;
            sincos(dot(K.xy, l) + P.x, s, c);
            d.y += a * c;
            d.xz -= (Q * a * s) * dir;
            drop += 0.5 * Q * a * a * K.z;
            if (imageW > 0.0)
            {
                // the mirror image about the wall: eta(2 Xw - x, z) (14.7 H1); horizontal displacement mirrored in x
                float ai = a * imageW;
                float s2, c2;
                sincos(dot(float2(-K.x, K.y), l) + P.y, s2, c2);   // pal:ok (not a colour)
                d.y += ai * c2;
                d.x += Q * ai * s2 * dir.x;
                d.z -= Q * ai * s2 * dir.y;
                drop += 0.5 * Q * ai * ai * K.z;
            }
        }
    }
    d.y += drop;                                                     // Eulerian mean level stays at -8 (SeaTests)
    return d;
}

// gust: 0..2 factor on the resolved slope of the short wind waves (wavelength below BWSEA_GUST_LMAX), the cat's-paw
// patches of a gusty wind; the unresolved variance is not modulated (the far tier and the roughness stay exact).
#define BWSEA_GUST_LMAX 8.0
#define BWSEA_SLICK_LMAX 30.0     // a slick over a submerged body also flattens the wind sea up to 30 m (BwSeaEvaluateGC)
// slick: 0..1 damping of the resolved wind sea of 8-30 m (x (1 - 0.75 slick)): over the kraken's mass the water turns glassy
// while the long swell runs through (the unresolved variance is again not modulated).
// The pixel-stage core: fpIso > 0 = one isotropic footprint (m) for every component; fpIso <= 0 = anisotropic, each
// component (and its wall image, which runs mirrored in x) filtered by the footprint projected on its own direction,
// sqrt((d.dl0)^2 + (d.dl1)^2), at least 1/BWSEA_ANISO_MAX of the long axis.
BwSeaSample BwSeaEvaluateCore(float2 l, float2 dl0, float2 dl1, float fpIso, float imageW, float gust, float slick)
{
    float Q = _BwSeaInfo.z;
    float sx = 0.0, sz = 0.0, jxx = 0.0, jzz = 0.0, jxz = 0.0, h = 0.0, unres = 0.0;
    float fxx = 0.0, fzz = 0.0, fxz = 0.0, unresF = 0.0;   // the whitecap Jacobian (lambda >= 3 m)
    int nA = (int)_BwSeaInfo.x, nT = (int)_BwSeaInfo.y;
    bool iso = fpIso > 0.0;
    float fpLong = iso ? fpIso : max(sqrt(max(dot(dl0, dl0), dot(dl1, dl1))), 1e-4);
    float fpMin = fpLong / BWSEA_ANISO_MAX;
    bool img = imageW > 0.0;
    [loop] for (int set = 0; set < 2; set++)
    {
        int i0 = set == 0 ? 0 : nA;
        int i1 = set == 0 ? nA : nT;
        [loop] for (int i = i0; i < i1; i++)
        {
            float4 K = _BwSeaK[i];
            float4 P = _BwSeaP[i];
            float ak = P.z;
            float am2 = 0.5 * ak * ak, ai2 = am2 * imageW * imageW;
            bool foamBand = P.w >= BWSEA_FOAM_LMIN && P.w <= BWSEA_FOAM_LMAX;
            float2 dir = K.xy / max(K.z, 1e-6);
            float fpk = fpLong, fpkI = fpLong;
            if (!iso)
            {
                float2 dirI = float2(-dir.x, dir.y);                  // pal:ok (not a colour: the image wave's direction)
                float p0 = dot(dir, dl0), p1 = dot(dir, dl1);
                float q0 = dot(dirI, dl0), q1 = dot(dirI, dl1);
                fpk = max(sqrt(p0 * p0 + p1 * p1), fpMin);
                fpkI = max(sqrt(q0 * q0 + q1 * q1), fpMin);
            }
            float invL = 1.0 / max(P.w, 1e-4);
            float xm = BWSEA_PIX_K * fpk * invL, xi = BWSEA_PIX_K * fpkI * invL;
            if (xm >= 1.0 && (!img || xi >= 1.0))                     // filtered: only its variance
            {
                unres += am2 + ai2;
                if (foamBand) unresF += am2 + ai2;
                continue;
            }
            float w = 1.0 - BwSeaSmooth(0.5, 1.0, xm);
            float wI = img ? 1.0 - BwSeaSmooth(0.5, 1.0, xi) : 0.0;
            float un = (1.0 - w * w) * am2 + (1.0 - wI * wI) * ai2;
            unres += un;
            if (foamBand) unresF += un;
            float s, c;
            sincos(dot(K.xy, l) + P.x, s, c);
            float g = P.w < BWSEA_GUST_LMAX ? gust : (P.w < BWSEA_SLICK_LMAX ? 1.0 - 0.75 * slick : 1.0);
            float a = K.w * w * g, akw = ak * w * g;
            h += a * c;
            sx += akw * s * dir.x;                                    // -d(eta)/dx0 = sum a k dx sin(theta)
            sz += akw * s * dir.y;
            float kc = Q * akw * c;
            jxx += kc * dir.x * dir.x;
            jzz += kc * dir.y * dir.y;
            jxz += kc * dir.x * dir.y;
            float fb = foamBand ? 1.0 : 0.0;
            fxx += fb * kc * dir.x * dir.x;
            fzz += fb * kc * dir.y * dir.y;
            fxz += fb * kc * dir.x * dir.y;
            if (img && wI > 0.0)
            {
                float s2, c2;
                sincos(dot(float2(-K.x, K.y), l) + P.y, s2, c2);   // pal:ok (not a colour)
                float aki = ak * wI * g * imageW;
                h += K.w * wI * g * imageW * c2;
                sx -= aki * s2 * dir.x;                               // the image wave runs with -dx
                sz += aki * s2 * dir.y;
                float kci = Q * aki * c2;
                jxx += kci * dir.x * dir.x;
                jzz += kci * dir.y * dir.y;
                jxz -= kci * dir.x * dir.y;
                fxx += fb * kci * dir.x * dir.x;
                fzz += fb * kci * dir.y * dir.y;
                fxz -= fb * kci * dir.x * dir.y;
            }
        }
    }
    BwSeaSample o;
    // Gerstner tangents dP/dx0 = (1 - jxx, -sx, -jxz), dP/dz0 = (-jxz, -sz, 1 - jzz); normal = cross(dP/dz0, dP/dx0)
    float3 tx = float3(1.0 - jxx, -sx, -jxz);   // pal:ok (not a colour)
    float3 tz = float3(-jxz, -sz, 1.0 - jzz);   // pal:ok (not a colour)
    float3 n = cross(tz, tx);
    float nl = dot(n, n);
    o.normal = nl > 1e-10 ? n * rsqrt(nl) : float3(0.0, 1.0, 0.0);   // pal:ok (not a colour)
    o.jac = (1.0 - fxx) * (1.0 - fzz) - fxz * fxz;
    o.jacSigma = Q * sqrt(max(unresF, 0.0));
    o.s2 = _BwSeaOrigin.w + unres;
    o.height = h;
    return o;
}

BwSeaSample BwSeaEvaluateGC(float2 l, float fp, float imageW, float gust, float slick)
{
    return BwSeaEvaluateCore(l, 0.0, 0.0, max(fp, 1e-4), imageW, gust, slick);
}

BwSeaSample BwSeaEvaluateAnisoGC(float2 l, float2 dl0, float2 dl1, float imageW, float gust, float slick)
{
    return BwSeaEvaluateCore(l, dl0, dl1, -1.0, imageW, gust, slick);
}

BwSeaSample BwSeaEvaluateG(float2 l, float fp, float imageW, float gust) { return BwSeaEvaluateGC(l, fp, imageW, gust, 0.0); }
BwSeaSample BwSeaEvaluate(float2 l, float fp, float imageW) { return BwSeaEvaluateG(l, fp, imageW, 1.0); }

float BwSeaFarS2() { return max(_BwSeaWall.w, _BwSeaOrigin.w); }

float BwSeaFoamThreshold() { return _BwSeaInfo.w; }

// Whitecap threshold where the wall image wave adds its own variance (weight w): J_t = 1 - 2.05 sigma sqrt(1 + w^2).
float BwSeaFoamThresholdAt(float imageW) { return 1.0 - _BwSeaFoam.z * _BwSeaFoam.x * sqrt(1.0 + imageW * imageW); }

// Probability that the true Jacobian (resolved value + unresolved Gaussian part) lies below J_t; the minimum softness
// fpJ (the pixel derivative of J) anti-aliases the threshold where everything is resolved. Logistic form of the normal
// CDF (1.702 x). At distance the coverage converges to the mean whitecap fraction (2 %), never to sparkle.
float BwSeaFoamCoverAt(BwSeaSample s, float fpJ, float imageW)
{
    float sig = sqrt(s.jacSigma * s.jacSigma + fpJ * fpJ + 1e-6);
    float x = (BwSeaFoamThresholdAt(imageW) - s.jac) / sig;
    return 1.0 / (1.0 + exp(-1.702 * clamp(x, -20.0, 20.0)));
}
float BwSeaFoamCover(BwSeaSample s, float fpJ) { return BwSeaFoamCoverAt(s, fpJ, 0.0); }

// Calm near the spillway mouths: the waves die out over the last 40 m before the pier ends (x 46, z 424-700), so the
// sea reaches the draw-down water of the slots exactly at its start level (y -8.0) and slope. Continuous everywhere.
float BwSeaCalm(float2 worldXZ)
{
    float2 q = float2(worldXZ.x - _BwSeaSpill.z, worldXZ.y - clamp(worldXZ.y, _BwSeaSpill.x + 4.0, _BwSeaSpill.y - 4.0));   // pal:ok (not a colour)
    q.x = max(q.x, 0.0);
    return BwSeaSmooth(0.0, 40.0, length(q));
}

// Reef occupancy for foam (14.7 H1 "foam from the Jacobian, wall distance and reef occupancy"): the beat 4 owner's
// ReefUnitAt field. Returns 0 until that lands (no reef in beats 2-3).
float BwSeaReefOccupancy(float2 xz) { return 0.0; }

// ============================================================================ SeaDisturbances (Addendum A1: the kraken)
// The kraken touching the sea. Runtime/Core/SeaDisturbances.cs uploads up to 8 entries per frame (one writer: the kraken
// choreography; for QA the sea owner's Beats/Beat02_Crest/SeaDisturbanceQa.cs behind the flag -sea-dist-qa). The sea
// clock _BwSeaDistClock (ambient clock mod 600 s, from the double clock in SeaState) drives every travelling pattern, so
// frozen-clock stills reproduce; every period below divides 600 s (no jump at the wrap). C# twin: SeaDisturbField.cs.
// API (stable; pure maths, no other include needed):
//   BwSeaDist BwSeaDisturbAt(float2 xz, float2 dP0, float2 dP1)   pixel stage: xz world, dP0 / dP1 = ddx / ddy of xz
//   float     BwSeaDisturbHeight(float2 xz, float sp)             vertex stage: height (m) for a ring of spacing sp
//   bool      BwSeaDistCovers(float2 xz)                          an active entry's support covers xz (the clipmap's
//                                                                 disturbance ring beyond 2048 m draws only there)
// Kinds (local frame: u along the heading, v to its right; A = halfAlong, B = halfAcross, S = strength, Bm = sqrt(A B)):
//   0 Shadow        an enormous mass just below the surface: the body colour darkened by up to 92 % (and the sky
//                   reflection dimmed by up to 64 % in BwSea: a dark hole in the reflection that reads from the crest) in a
//                   mantle with a flat core and a soft heaving outline (0.62 A x 0.62 B, centre 0.25 A ahead) and eight
//                   tapering arms trailing 1.4-2.8 A behind, slowly writhing (6 s); a glassy slick over it (short waves and
//                   whitecaps damped up to 85 %); a Gaussian swell of 0.03 B (0.2-0.8 m). NO foam.
//   1 Breach        the body through the surface: solid white water to a ragged waterline (ellipse + 0.25 Bm, three
//                   angular harmonics), a waterline foam ring, lacy foam to about 2 Bm, ring waves leaving at 4.5 m/s every
//                   2.5 s, a mound 0.15 Bm (1.5-7 m).
//   2 TentacleRise  a tentacle of waterline radius A through the surface: a white collar, ring waves (3 m/s every 1.5 s),
//                   a water collar 0.4 A (0.3-2 m) and a foam stream downwind (west, the storm wind) widening 0.12 m
//                   per m over 18 A + 25 m.
//   3 Slap          a burst: a white core (S^2), a foam ring of radius A (1 + 4 (1 - S)) with a crest under it and a
//                   crater at the centre. S is read as the event envelope: with a fast attack and a slow decay (the
//                   writer's contract for Slap) the ring runs outward while it fades.
//   4 Wake          a body moving just below: a weaker Shadow (55 %) and slick, a bow foam line, churned white water
//                   behind it (3 A + 20 m), the Kelvin wedge (arms at 19.47 deg from 0.8 B beside the stern) with foam
//                   lines and a raised crest, and a calm scar along the track, all fading over 10 A + 40 m.
// Filtering (no sparkle, 14.7 H1): every profile is a Gaussian in a line coordinate q (m) whose pixel footprint
// fq = |grad q . dP0| + |grad q . dP1| widens it energy-preservingly (BwSeaGaussF): far away a ring or a wake arm becomes
// a faint steady band, never a flickering line; travelling crests converge to their mean; the vertex stage weights each
// pattern by the ring spacing like the waves (1 - smoothstep(0.125, 0.25, sp / feature)).
#define BWSEA_DIST_MAX 8
#define BWSEA_KELVIN_TAN 0.353553     // tan(19.47 deg): the Kelvin wedge half-angle
#define BWSEA_DIST_FILTER 0.1         // variance of the pixel footprint kernel per fq^2 (a box is 1/12)

float  _BwSeaDistCount;                       // entries in use (int as float)
float4 _BwSeaDist0[BWSEA_DIST_MAX];           // x, z (world m), halfAlong, halfAcross (m)
float4 _BwSeaDist1[BWSEA_DIST_MAX];           // heading (rad, 0 = +z, clockwise to +x), strength 0..1, kind, height above sea (m)
float  _BwSeaDistClock;                       // ambient clock mod 600 s

struct BwSeaDist
{
    float dark;     // 0..1 darkening of the body (subsurface) colour: a mass below the surface
    float calm;     // 0..1 damping of the short waves, the unresolved roughness and the whitecaps (slick, wake scar)
    float foam;     // 0..1 foam coverage (the caller modulates it by its lace)
    float white;    // 0..1 solid white water (breaking, cascading: not lace-modulated)
    float2 grad;    // d(height)/dx, d(height)/dz (footprint-filtered), for the normal
    float height;   // height of the disturbance (m), for the crest translucency
};

// Energy-preserving footprint filter of exp(-x^2 / (2 sigma^2)): sigma' = sqrt(sigma^2 + 0.1 fq^2), peak x sigma / sigma'.
float BwSeaGaussF(float x, float sigma, float fq)
{
    float s2 = sigma * sigma + BWSEA_DIST_FILTER * fq * fq;
    return sigma * rsqrt(max(s2, 1e-8)) * exp(-0.5 * x * x / max(s2, 1e-8));
}

// Footprint (m) of a line coordinate with gradient g for the pixel derivatives dP0, dP1 of world xz.
float BwSeaLineFp(float2 g, float2 dP0, float2 dP1) { return abs(dot(g, dP0)) + abs(dot(g, dP1)) + 1e-4; }

// Gaussian crests (width sigma) of a ring train leaving its source: crest k at e = c (T - k P), spacing c P; filtered by
// the footprint fq and converging to the mean sigma sqrt(2 pi) / (c P) once a crest spacing is under about 2 footprints.
float BwSeaRingTrain(float e, float c, float P, float sigma, float fq)
{
    float lam = c * P;
    float ph = (e - c * _BwSeaDistClock) / lam;
    float dq = (ph - round(ph)) * lam;
    float g = BwSeaGaussF(dq, sigma, fq);
    float mean = sigma * 2.5066283 / lam;
    return lerp(g, mean, BwSeaSmooth(0.25, 0.6, fq / lam));
}

// Support radius (m) of one entry: beyond it every field is below 1e-3 (the loops skip it; SeaDisturbField twin tests it).
float BwSeaDistSupport(int kind, float A, float B)
{
    float Bm = sqrt(A * B);
    if (kind == 0) return 3.4 * A + 1.2 * B;
    if (kind == 1) return 1.25 * max(A, B) * (1.0 + 6.0 * (1.5 * Bm + 20.0) / Bm);   // e = (rho - 1) Bm reaches 6 decay lengths
    if (kind == 2) return 4.0 * (18.0 * A + 25.0) + 6.0 * A + 30.0;
    if (kind == 3) return 6.0 * A + 30.0;
    return 1.6 * A + 6.0 * (10.0 * A + 40.0) + 3.0 * B;
}

// True where an active entry's support covers world xz (every field of every entry is 0 elsewhere): the disturbance ring
// of the clipmap (level 7, beyond 2048 m) draws only there.
bool BwSeaDistCovers(float2 xz)
{
    int n = (int)min(_BwSeaDistCount + 0.5, (float)BWSEA_DIST_MAX);
    bool hit = false;
    [loop] for (int i = 0; i < n; i++)
    {
        float4 D0 = _BwSeaDist0[i], D1 = _BwSeaDist1[i];
        if (saturate(D1.y) <= 1e-4) continue;
        float sup = BwSeaDistSupport((int)(D1.z + 0.5), max(D0.z, 0.5), max(D0.w, 0.5));
        float2 d = xz - D0.xy;
        if (dot(d, d) < sup * sup) hit = true;
    }
    return hit;
}

// Mantle and tendrils of a submerged body (Shadow, and Wake at 45 %): returns the darkness 0..1 (unscaled by S), the
// slick 0..1 and the swell shape 0..1 with its gradient in (u, v).
void BwSeaMantle(float u, float v, float A, float B, float fqU, float fqV, out float dark, out float slick,
                 out float swell, out float2 gradUV)
{
    float su = 0.5 * A, sv = 0.55 * B, uc = 0.25 * A;
    float su2 = su * su + BWSEA_DIST_FILTER * fqU * fqU, sv2 = sv * sv + BWSEA_DIST_FILTER * fqV * fqV;
    float amp = su * sv * rsqrt(su2 * sv2);
    float du = u - uc;
    swell = amp * exp(-0.5 * (du * du / su2 + v * v / sv2));
    gradUV = -swell * float2(du / su2, v / sv2);
    // the mantle: a flat dark core (q < 0.45) with a soft, slowly heaving outline (q = 1 at 0.62 A x 0.62 B), the edge
    // widened by the footprint (kq) so far away it converges to its mean, never to a flickering rim
    float ca = 0.62 * A, cb = 0.62 * B;
    float qa = du / ca, qb = v / cb;
    float th = atan2(qb, qa);
    float wob = 1.0 + 0.08 * sin(3.0 * th + 0.7) + 0.05 * sin(5.0 * th + 2.1 + 6.2831853 * _BwSeaDistClock / 12.0);
    float q = sqrt(qa * qa + qb * qb) / wob;
    float kq = 0.32 * max(fqU / ca, fqV / cb);
    float body = 1.0 - BwSeaSmooth(0.45 - kq, 1.0 + kq, q);
    // the slick reaches a little beyond the body (x 1.25)
    slick = 1.0 - BwSeaSmooth(0.5 - kq, 1.25 + kq, q);
    // eight arms trailing behind (u from -0.2 A back 1.4-2.8 A), spreading out, tapering, writhing with a 6 s period
    float tend = 0.0;
    float T = _BwSeaDistClock;
    [unroll] for (int j = 0; j < 8; j++)
    {
        float fj = (float)j;
        float len = A * (1.4 + 1.4 * frac(0.618034 * fj + 0.31));
        float t = (-0.2 * A - u) / len;
        if (t <= -0.05 || t >= 1.0) continue;
        float tc = saturate(t);
        float vj = B * 0.8 * (-0.77 + 0.22 * fj);
        float vc = vj * (1.0 + 0.8 * tc) + 0.22 * B * tc * sin(6.2831853 * (1.3 * tc - T / 6.0 + 0.37 * fj));
        float w = B * (0.16 * (1.0 - 0.7 * tc)) + 0.3;
        float env = BwSeaSmooth(-0.05, 0.08, t) * sqrt(max(1.0 - tc, 0.0));
        tend = max(tend, env * BwSeaGaussF(v - vc, w, fqV));
    }
    dark = max(body, 0.9 * tend);
}

BwSeaDist BwSeaDisturbAt(float2 xz, float2 dP0, float2 dP1)
{
    BwSeaDist o;
    o.dark = 0.0; o.calm = 0.0; o.foam = 0.0; o.white = 0.0; o.grad = 0.0; o.height = 0.0;
    int n = (int)min(_BwSeaDistCount + 0.5, (float)BWSEA_DIST_MAX);
    float fqAll = length(dP0) + length(dP1);                  // the footprint in any direction (support margin)
    [loop] for (int i = 0; i < n; i++)
    {
        float4 D0 = _BwSeaDist0[i], D1 = _BwSeaDist1[i];
        float S = saturate(D1.y);
        if (S <= 1e-4) continue;
        int kind = (int)(D1.z + 0.5);
        float A = max(D0.z, 0.5), B = max(D0.w, 0.5);
        float2 d = xz - D0.xy;
        float sup = BwSeaDistSupport(kind, A, B) + 1.2 * fqAll;
        float r2 = dot(d, d);
        if (r2 > sup * sup) continue;
        S *= 1.0 - BwSeaSmooth(0.8 * sup, sup, sqrt(r2));   // continuous at the support radius
        float2 fw = float2(sin(D1.x), cos(D1.x));       // heading
        float2 rt = float2(fw.y, -fw.x);                 // its right
        float u = dot(d, fw), v = dot(d, rt);
        float fqU = BwSeaLineFp(fw, dP0, dP1), fqV = BwSeaLineFp(rt, dP0, dP1);
        float Bm = sqrt(A * B);
        if (kind == 0 || kind == 4)
        {
            float dk, sl, sw; float2 gUV;
            BwSeaMantle(u, v, A, B, fqU, fqV, dk, sl, sw, gUV);
            float k = kind == 0 ? 1.0 : 0.55;
            float hS = clamp(0.03 * B, 0.2, 0.8) * S * k;
            o.dark = max(o.dark, 0.92 * S * k * dk);
            o.calm = max(o.calm, 0.85 * S * k * sl);
            o.height += hS * sw;
            o.grad += hS * (gUV.x * fw + gUV.y * rt);
            if (kind == 4)
            {
                float bh = -u - 0.6 * A;                                  // distance behind the stern
                float fade = exp(-max(bh, 0.0) / (10.0 * A + 40.0));
                float on = BwSeaSmooth(-0.2 * A, 0.3 * A, bh);
                // Kelvin arms |v| = 0.8 B + tan(19.47) bh
                float sv = v >= 0.0 ? 1.0 : -1.0;
                float dv = abs(v) - (0.8 * B + BWSEA_KELVIN_TAN * max(bh, 0.0));
                float fqA = BwSeaLineFp(sv * rt + BWSEA_KELVIN_TAN * fw, dP0, dP1);
                float sa = 0.12 * B + 0.5 + 0.02 * max(bh, 0.0);
                float arm = on * fade * BwSeaGaussF(dv, sa, fqA);
                o.foam = max(o.foam, 0.7 * S * arm);
                float hA = clamp(0.04 * B + 0.15, 0.2, 0.8) * S;
                float armH = on * fade * BwSeaGaussF(dv, 1.5 * sa, fqA);
                o.height += hA * armH;
                o.grad += hA * armH * (-dv / (2.25 * sa * sa + BWSEA_DIST_FILTER * fqA * fqA)) * (sv * rt + BWSEA_KELVIN_TAN * fw);
                // the turbulent scar along the track: calm water with faint streaks of foam
                float scar = on * exp(-max(bh, 0.0) / (8.0 * A + 30.0)) * BwSeaGaussF(v, 0.7 * B + 0.05 * max(bh, 0.0), fqV);
                o.calm = max(o.calm, 0.6 * S * scar);
                // churned white water right behind the body (3 A + 20 m), then the scar's faint streaks
                float churn = on * exp(-max(bh, 0.0) / (3.0 * A + 20.0)) * BwSeaGaussF(v, 0.6 * B + 0.03 * max(bh, 0.0), fqV);
                o.foam = max(o.foam, max(0.15 * S * scar, 0.5 * S * churn));
                // bow wave: a foam line round the front of the body
                float rho = sqrt((u * u) / (A * A) + (v * v) / (B * B)) + 1e-5;
                float2 gR = (u / (A * A) * fw + v / (B * B) * rt) / rho * Bm;
                float bow = BwSeaGaussF((rho - 1.08) * Bm, 0.08 * Bm + 0.4, BwSeaLineFp(gR, dP0, dP1)) * BwSeaSmooth(-0.2, 0.6, u / A);
                o.foam = max(o.foam, 0.6 * S * bow);
            }
        }
        else if (kind == 1)
        {
            float rho = sqrt((u * u) / (A * A) + (v * v) / (B * B)) + 1e-5;
            float e = (rho - 1.0) * Bm;                                 // metres outside the waterline (approx.)
            float2 gE = (u / (A * A) * fw + v / (B * B) * rt) / rho * Bm;
            float fqE = BwSeaLineFp(gE, dP0, dP1);
            // a ragged waterline: three angular harmonics (the 5th turning once in 30 s), +-0.31 Bm at most
            float thB = atan2(v / B, u / A);
            float eW = e + Bm * 0.14 * (sin(3.0 * thB + 1.1) + 0.7 * sin(5.0 * thB + 2.3 + 6.2831853 * _BwSeaDistClock / 30.0) + 0.5 * sin(8.0 * thB - 0.7));
            float we = sqrt(0.0625 * Bm * Bm + BWSEA_DIST_FILTER * fqE * fqE);
            o.white = max(o.white, S * (1.0 - BwSeaSmooth(-we, we, eW - 0.25 * Bm)));
            float ringW = BwSeaGaussF(eW - 0.35 * Bm, 0.2 * Bm + 1.0, fqE);
            float out1 = BwSeaSmooth(0.0, 4.0, e);
            float rings = BwSeaRingTrain(e, 4.5, 2.5, 1.2, fqE) * exp(-max(e, 0.0) / (1.5 * Bm + 20.0)) * out1;
            float pool = exp(-max(eW, 0.0) / (1.2 * Bm));
            o.foam = max(o.foam, S * max(max(ringW, 0.3 * rings), 0.45 * pool));
            // mound (Gaussian over the body) and the outgoing ring waves (cosine, filtered)
            float hB = clamp(0.15 * Bm, 1.5, 7.0) * S;
            float m2 = rho * rho / 0.81;
            float mound = exp(-0.5 * m2);
            o.height += hB * mound;
            o.grad += hB * mound * (-rho / 0.81) * (gE / Bm);
            float lam = 11.25;
            float ph = 6.2831853 * (e - 4.5 * _BwSeaDistClock) / lam;
            float att = exp(-2.0 * (3.14159265 * fqE / lam) * (3.14159265 * fqE / lam));
            float hR = 0.6 * S * exp(-max(e, 0.0) / (Bm + 25.0)) * out1 * att;
            o.height += hR * cos(ph);
            o.grad += -hR * sin(ph) * (6.2831853 / lam) * gE;
        }
        else if (kind == 2)
        {
            float r = length(d) + 1e-5;
            float2 gr = d / r;
            float e = r - A;
            float fqE = BwSeaLineFp(gr, dP0, dP1);
            float wc = 0.35 * A + 0.5;
            o.white = max(o.white, S * BwSeaGaussF(max(e, 0.0), wc, fqE));
            float out1 = BwSeaSmooth(0.0, 1.0, e);
            float rings = BwSeaRingTrain(e, 3.0, 1.5, 0.5 + 0.1 * A, fqE) * exp(-max(e, 0.0) / (2.0 * A + 5.0)) * out1;
            // the foam stream downwind (west): along a = -x, across b = z
            float a = -d.x, b = d.y;
            float sb = A + 0.12 * max(a, 0.0);
            float L = 18.0 * A + 25.0;
            float trail = BwSeaSmooth(-A, A, a) * exp(-max(a, 0.0) / L) * BwSeaGaussF(b, sb, BwSeaLineFp(float2(0.0, 1.0), dP0, dP1));
            o.foam = max(o.foam, S * max(0.25 * rings, 0.8 * trail));
            float hC = clamp(0.4 * A, 0.3, 2.0) * S;
            float sc = 0.6 * A + 0.5;
            float sc2 = sc * sc + BWSEA_DIST_FILTER * fqE * fqE;      // filtered: no thin bright rim far away
            float coll = BwSeaGaussF(e + 0.1 * A, sc, fqE);
            o.height += hC * coll;
            o.grad += hC * coll * (-(e + 0.1 * A) / sc2) * gr;
        }
        else
        {
            // Slap: the ring runs outward as the envelope S decays
            float r = length(d) + 1e-5;
            float2 gr = d / r;
            float fqR = BwSeaLineFp(gr, dP0, dP1);
            float Rr = A * (1.0 + 4.0 * (1.0 - S));
            float sr = 0.18 * A + 0.8 + 0.1 * (Rr - A);
            float ring = BwSeaGaussF(r - Rr, sr, fqR);
            float sc = 0.7 * A;
            float core = BwSeaGaussF(r, sc, fqR);
            float inside = 1.0 - BwSeaSmooth(0.4 * Rr, 1.05 * Rr, r);
            o.foam = max(o.foam, max(sqrt(S) * ring, 0.4 * S * inside));
            o.white = max(o.white, S * S * core);
            float hS = clamp(0.25 * A, 0.4, 2.5);
            float sr2 = sr * sr + BWSEA_DIST_FILTER * fqR * fqR, sk2 = 0.36 * A * A + BWSEA_DIST_FILTER * fqR * fqR;
            float crest = S * hS * BwSeaGaussF(r - Rr, sr, fqR);
            float crater = 0.6 * hS * S * S * BwSeaGaussF(r, 0.6 * A, fqR);
            o.height += crest - crater;
            o.grad += (crest * (-(r - Rr) / sr2) - crater * (-r / sk2)) * gr;
        }
    }
    o.dark = saturate(o.dark); o.calm = saturate(o.calm); o.foam = saturate(o.foam); o.white = saturate(o.white);
    return o;
}

// Vertex stage: height (m) of the disturbances at world xz for a clipmap ring of spacing sp; each pattern carries the
// ring weight of its feature size (the mound and swell resolve on every ring; ring waves and crests only where sampled
// with 8 vertices per wavelength / 4 per crest width).
float BwSeaDisturbHeight(float2 xz, float sp)
{
    float h = 0.0;
    int n = (int)min(_BwSeaDistCount + 0.5, (float)BWSEA_DIST_MAX);
    [loop] for (int i = 0; i < n; i++)
    {
        float4 D0 = _BwSeaDist0[i], D1 = _BwSeaDist1[i];
        float S = saturate(D1.y);
        if (S <= 1e-4) continue;
        int kind = (int)(D1.z + 0.5);
        float A = max(D0.z, 0.5), B = max(D0.w, 0.5);
        float2 d = xz - D0.xy;
        float sup = BwSeaDistSupport(kind, A, B);
        float r2 = dot(d, d);
        if (r2 > sup * sup) continue;
        float fade = 1.0 - BwSeaSmooth(0.8 * sup, sup, sqrt(r2));
        float2 fw = float2(sin(D1.x), cos(D1.x));
        float2 rt = float2(fw.y, -fw.x);
        float u = dot(d, fw), v = dot(d, rt);
        float Bm = sqrt(A * B);
        float h0 = h;
        if (kind == 0 || kind == 4)
        {
            float k = kind == 0 ? 1.0 : 0.55;
            float su = 0.5 * A, sv = 0.55 * B, du = u - 0.25 * A;
            float wRing = 1.0 - BwSeaSmooth(0.125, 0.25, sp / (4.0 * min(su, sv)));
            h += clamp(0.03 * B, 0.2, 0.8) * S * k * wRing * exp(-0.5 * (du * du / (su * su) + v * v / (sv * sv)));
            if (kind == 4)
            {
                float bh = -u - 0.6 * A;
                float sa = 0.12 * B + 0.5 + 0.02 * max(bh, 0.0);
                float dv = abs(v) - (0.8 * B + BWSEA_KELVIN_TAN * max(bh, 0.0));
                float wA = 1.0 - BwSeaSmooth(0.125, 0.25, sp / (6.0 * sa));
                float on = BwSeaSmooth(-0.2 * A, 0.3 * A, bh) * exp(-max(bh, 0.0) / (10.0 * A + 40.0));
                h += clamp(0.04 * B + 0.15, 0.2, 0.8) * S * wA * on * exp(-0.5 * dv * dv / (2.25 * sa * sa));
            }
        }
        else if (kind == 1)
        {
            float rho = sqrt((u * u) / (A * A) + (v * v) / (B * B));
            float e = (rho - 1.0) * Bm;
            float wM = 1.0 - BwSeaSmooth(0.125, 0.25, sp / (3.6 * Bm));
            h += clamp(0.15 * Bm, 1.5, 7.0) * S * wM * exp(-0.5 * rho * rho / 0.81);
            float lam = 11.25;
            float wR = 1.0 - BwSeaSmooth(0.125, 0.25, sp / lam);
            h += 0.6 * S * wR * exp(-max(e, 0.0) / (Bm + 25.0)) * BwSeaSmooth(0.0, 4.0, e) * cos(6.2831853 * (e - 4.5 * _BwSeaDistClock) / lam);
        }
        else if (kind == 2)
        {
            float e = length(d) - A;
            float sc = 0.6 * A + 0.5;
            float wC = 1.0 - BwSeaSmooth(0.125, 0.25, sp / (4.0 * sc));
            h += clamp(0.4 * A, 0.3, 2.0) * S * wC * exp(-0.5 * (e + 0.1 * A) * (e + 0.1 * A) / (sc * sc));
        }
        else
        {
            float r = length(d);
            float Rr = A * (1.0 + 4.0 * (1.0 - S));
            float sr = 0.18 * A + 0.8 + 0.1 * (Rr - A);
            float hS = clamp(0.25 * A, 0.4, 2.5);
            float wS = 1.0 - BwSeaSmooth(0.125, 0.25, sp / (4.0 * sr));
            float wK = 1.0 - BwSeaSmooth(0.125, 0.25, sp / (2.8 * A));
            h += S * hS * wS * exp(-0.5 * (r - Rr) * (r - Rr) / (sr * sr)) - 0.6 * hS * S * S * wK * exp(-0.5 * r * r / (0.36 * A * A));
        }
        h = h0 + (h - h0) * fade;
    }
    return h;
}

#endif
