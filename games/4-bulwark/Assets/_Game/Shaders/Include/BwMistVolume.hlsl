// BwMistVolume.hlsl - the analytic mist volume of MistVolumeFeature (MASTER_PROMPT 14.7 H2, 14.5.3, 14.2.5).
// One ray per pixel (or per MSAA sample on edge pixels) through at most 8 emitters, no particles, no sorting:
//   * the PLUME   rho0 exp(-(z-562)^2 / (2 150^2)) exp(-max(y, 0) / 130) over x -6 .. -2500 (faded over 40 m next to the
//                 lee face and over the last 200 m), peak horizontal optical depth 1.0; plus the spray VEIL (optical depth
//                 0.02 below y 400 within 1.5 km of the face, s 560-640, weight from the CPU) in the same samples;
//   * the CURTAIN the 14 sheets beyond 90 m: the 14.5.3 parabola y = -11.5 - k (x + 6)^2 as a surface, intersected
//                 exactly (one quadratic), with the across-slot profile blurred by the Gaussian radius 3 m (90 m) .. 60 m
//                 (1 km), the water fraction falling to 0 at 1000 m (white water -> mist), advected streaks;
//   * up to 6 WALL SPRAYS from WaveImpacts (ellipsoids 8 x 12 x 8 m, 0.6 s in, 2.4 s out) and the beat 7 CONDUIT spray,
//                 all analytic chords (unit profile (1 - r^2)^2: closed-form optical depth, clipped at the sample depth).
// Integration (H2): 24 FIXED samples between the analytic entry and exit of the plume box (clipped by the farthest of
// the pixel's 4 depths, so the grid is the same for every sample of a pixel), no jitter; density x (0.65 + 0.35 fbm3(p /
// 60 m + wind t)) from a periodic 3D noise texture (MistVolumeNoise, offset wrapped on whole tiles in double on the CPU);
// transmittance exp(-sum 0.02 rho dt); single scattering with the Henyey-Greenstein phase (g 0.55, plus two softer
// multiple-scattering octaves) x sun visibility (BwSunBreak at every sample, the direct-sun share _BwSunDisc.w) and sky
// ambient; the rainbow phase term of H2 (primary and secondary per channel, dark band 0.6, gain K, bow radiance clamped
// at 2.0). Each sample's in-scatter is attenuated by the haze in front of it (BwFogTransmittance) and the haze in front
// of the mist is added back (BwFogApply of black at the mist's mean distance), so mist and fog agree.
// Per MSAA sample: integrating to a depth d is the cumulative table of the 24 samples read at d (linear inside the step
// that contains d); the per-pixel and the per-sample paths share this code and the grid, so they are identical where
// the depths agree (MistVolumePass: pixel pass for agreeing pixels, SV_SampleIndex pass for stencil-marked edges).
// C# twin: Runtime/Rendering/MistVolumeMath.cs (every MIST_* constant; MistTests compares them).
#ifndef BW_MIST_VOLUME_INCLUDED
#define BW_MIST_VOLUME_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "BulwarkPalette.hlsl"
#include "BwCommon.hlsl"
#include "BulwarkAtmosphere.hlsl"

// ------------------------------------------------------------------------------------------------ constants (twins)
#define MIST_SIGMA 0.02
#define MIST_SAMPLES 24
#define MIST_SPACING 1.6
#define MIST_FBM_BASE 0.65
#define MIST_FBM_AMP 0.35
#define MIST_FBM_TILE 240.0              // MistVolumeNoise: 4 cells of 60 m per tile (H2 "p / 60 m")
#define MIST_PLUME_Z 562.0
#define MIST_PLUME_SIGMA_Z 150.0
#define MIST_PLUME_H 130.0
#define MIST_PLUME_X_FACE (-6.0)
#define MIST_PLUME_X_FAR (-2500.0)
#define MIST_PLUME_FACE_FADE 40.0
#define MIST_PLUME_FAR_FADE 200.0
#define MIST_PLUME_X_SCALE 600.0         // densest where the mist climbs the lee face (MistVolumeMath.PlumeXScale)
#define MIST_PLUME_Y_MIN (-6000.0)
#define MIST_PLUME_Y_MAX 700.0
#define MIST_PLUME_Z_HALF 600.0
#define MIST_VEIL_WIDTH 1500.0
#define MIST_VEIL_TOP 400.0
#define MIST_VEIL_TOP_FADE 100.0
#define MIST_VEIL_Z_HALF 1500.0
#define MIST_VEIL_Z_FADE 300.0
#define MIST_VEIL_X_FADE 150.0
#define MIST_VEIL_FACE_FADE 150.0        // soft start at the lee face (MistVolumeMath.VeilFaceFade: no seam along the face plane)
#define MIST_LIP_X (-6.0)
#define MIST_LIP_Y (-11.5)
#define MIST_LIP_SPEED 8.3
#define MIST_GRAVITY_HALF 4.9
#define MIST_CURTAIN_D0 90.0
#define MIST_CURTAIN_FADE_IN0 70.0
#define MIST_CURTAIN_FADE_IN1 95.0
#define MIST_CURTAIN_R0 3.0
#define MIST_CURTAIN_D1 1000.0
#define MIST_CURTAIN_R_EXP 1.244103        // ln(60 / 3) / ln(1000 / 90)
#define MIST_CURTAIN_WATER_TAU 5.0
#define MIST_CURTAIN_MIST_TAU 0.8
#define MIST_CURTAIN_END0 900.0
#define MIST_CURTAIN_END1 1100.0
#define MIST_SLOT_Z0 424.0
#define MIST_SLOT_PITCH 20.0
#define MIST_SLOT_LEN 16.0
#define MIST_SLOT_COUNT 14
#define MIST_HG_G 0.55
#define MIST_PHASE_CLAMP 1.5
#define MIST_GOLD_SUN 1.0                // sun-break gold (MistVolumeMath.Gold*): share of the sun light turned BreakGold
#define MIST_GOLD_AMB 1.0                // ... of the dome ambient
#define MIST_GOLD_LOBE 0.35              // forward lobe toward the sun: 0.35 x pi x HG(c, 0.75), at most 0.75
#define MIST_GOLD_LOBE_G 0.75
#define MIST_GOLD_LOBE_CLAMP 0.75
#define MIST_BOW_WIDTH 0.9
#define MIST_BOW_SECONDARY 0.25
#define MIST_BOW_CLAMP 2.0
#define MIST_BAND 0.6
#define MIST_BAND_IN 42.5
#define MIST_BAND_OUT 50.0
#define MIST_BAND_EDGE 0.5
#define MIST_SPRAYS 7                    // 6 wall sprays (+ one spare slot)
#define MIST_SPRAY_BASE 0.35             // spray billows: density x (0.35 + 0.65 smoothstep(noise))
#define MIST_ALBEDO 0.92                 // single-scattering albedo of water droplets (no absorption in the visible, minus loss)
#define MIST_SPRAY_ALBEDO 1.0            // white-water spray ambient albedo (MistVolumeMath.SprayAlbedo)
#define MIST_WATER_ALBEDO 0.85           // the white aerated water of the curtain (Lambert-like, FoamWhite)
#define MIST_SPRAY_WHITE 0.85            // white-water share of a thick spray's radiance (MistVolumeMath.SprayWhite)
#define MIST_SPRAY_WHITE_TAU 0.8         // ... reached as 1 - exp(-tau / 0.8) along the ray (MistVolumeMath.SprayWhiteTau)
#define MIST_SPRAY_NUP 1.2               // up tilt of the white water's facing normal (-d.x, 1.2, -d.z) (MistVolumeMath.SprayNormalUp)
#define MIST_SPRAY_ERODE 0.55            // outline erosion: density x smoothstep(0, 0.35, noise - 0.55 - 0.2 yN + 0.9 sqrt(q)) (MistVolumeMath.SprayErode*)
#define MIST_SPRAY_ERODE_IN 0.9
#define MIST_SPRAY_ERODE_SOFT 0.35
#define MIST_SPRAY_ERODE_TOP 0.2
#define MIST_SPRAY_NEAR0 3.0              // sprays fade out toward the eye: x smoothstep(3, 12, chord midpoint distance) (MistVolumeMath.SprayNear0/1)
#define MIST_SPRAY_NEAR1 12.0
#define MIST_STORM_KEY 0.0               // share of the hidden storm key that lights the mist (the dome carries it)
#define MIST_DAM_CREST_Y 0.5             // top of the dam's shadow edge (road y 0, parapet 1.1: their mean)
#define MIST_DAM_SOFT0 0.3               // shadow edge half width at the face (m)
#define MIST_SUN_PENUMBRA 0.0047         // + tan of the sun's angular radius (0.267 deg) x the distance to the face

// ------------------------------------------------------------------------------------------------ globals (MistVolumeState)
float4 _BwMist0;              // x = plume rho0, y = veil rho x weight(s), z = bow K (0 = bow off), w = sun gate (0 = mist sun off)
float4 _BwMist1;              // x = number of ellipsoid slots in use (0..7), y = mist enabled (0/1), z = curtain enabled, w = curtain flow offset (tiles, wrapped)
float4 _BwMistWind;           // xyz fbm offset of the plume (tiles, wrapped on whole tiles in double), w = spray fbm offset (tiles)
float4 _BwMistSprayC[MIST_SPRAYS];   // xyz centre (world), w = peak density x envelope
float4 _BwMistSprayR[MIST_SPRAYS];   // xyz semi-axes (m, local right / up / forward), w = yaw (rad)
float4 _BwMistSprayL[MIST_SPRAYS];   // rgb extra ambient (interior cold light), w = sun weight (0 conduit, 1 exterior)
float4 _BwMistSprayS[MIST_SPRAYS];   // xy = lean of the axis (world x, z metres per metre of height: downwind shear), z = 1 / noise tile (m), w = 1 / vertical noise tile
float4 _BwMistSprayN[MIST_SPRAYS];   // xyz = noise offset (tiles, wrapped in double on the CPU: evolution, vertical scroll, slot decorrelation)
float4 _BwMistSprayW[MIST_SPRAYS];   // x = white-water share (0.85 kraken sprays, 0.5 wall sprays: MistVolumeMath.SprayWhite / SprayWhiteWall)
float4 _BwMistScreen;         // x = width, y = height (px), z = 1 / width, w = 1 / height
float4 _BwMistAmb;            // rgb isotropic sky-dome in-scatter (x albedo, MistVolumeState.DomeAmbient), w = 1 when valid
float4 _BwMistWarp;           // xyz breathing-warp offset (1.6 km tiles, wrapped in double on the CPU)
// the beat 7 conduit spray (Beat07 ConduitSpray contract): a box in the conduit frame (u along the flow, v across, y)
float4 _BwMistBox0;           // xyz frame origin (world: the panel's south end on the tunnel axis, floor y), w = sigma0 (1/m) at the water
float4 _BwMistBox1;           // xy = axis U (world x, z), zw = axis V (world x, z)
float4 _BwMistBox2;           // x = u min, y = v min, z = water y, w = scale height (m)
float4 _BwMistBox3;           // x = u max, y = v max, z = ceiling y, w = u where the drop shaft starts (3 x sigma0 to the ceiling)
float4 _BwMistBoxL;           // rgb in-scatter radiance of the cold wall lights, w = 1 when the box is drawn
float4 _BwMistBoxF;           // x = air drift offset along u (tiles, wrapped)
float4 _BwMistGold;           // x = sun-break gold weight (s 560-640, MistVolumeMath.GoldWeight)
TEXTURE3D(_BwMistNoise); SAMPLER(sampler_BwMistNoise);

// ------------------------------------------------------------------------------------------------ helpers
float MistSmooth(float x) { x = saturate(x); return x * x * (3.0 - 2.0 * x); }

// erf (Winitzki 2008, max error 1.3e-4), twin MistVolumeMath.Erf
float MistErf(float x)
{
    float x2 = x * x;
    float v = sqrt(saturate(1.0 - exp(-x2 * (1.2732395 + 0.147 * x2) / (1.0 + 0.147 * x2))));
    return x >= 0.0 ? v : -v;
}

// Slab test of a ray against an axis-aligned box: entry / exit parameters (exit < entry = miss).
float2 MistBox(float3 o, float3 invD, float3 bmin, float3 bmax)
{
    float3 t0 = (bmin - o) * invD, t1 = (bmax - o) * invD;
    float3 tn = min(t0, t1), tf = max(t0, t1);
    return float2(max(max(tn.x, tn.y), tn.z), min(min(tf.x, tf.y), tf.z));
}

float MistFbm(float3 p, float3 offsetTiles)
{
    return SAMPLE_TEXTURE3D_LOD(_BwMistNoise, sampler_BwMistNoise, p * (1.0 / MIST_FBM_TILE) + offsetTiles, 0).r;
}

// ------------------------------------------------------------------------------------------------ densities
float MistPlumeBase(float3 p)
{
    float dz = p.z - MIST_PLUME_Z;
    float g = exp(-dz * dz * (0.5 / (MIST_PLUME_SIGMA_Z * MIST_PLUME_SIGMA_Z)));
    float v = exp(-max(p.y, 0.0) * (1.0 / MIST_PLUME_H));
    float f = MistSmooth((MIST_PLUME_X_FACE - p.x) * (1.0 / MIST_PLUME_FACE_FADE)) * MistSmooth((p.x - MIST_PLUME_X_FAR) * (1.0 / MIST_PLUME_FAR_FADE))
            * exp(-(MIST_PLUME_X_FACE - p.x) * (1.0 / MIST_PLUME_X_SCALE));
    return g * v * f;
}

float MistVeilBase(float3 p)
{
    float fx = MistSmooth((p.x - (MIST_PLUME_X_FACE - MIST_VEIL_WIDTH)) * (1.0 / MIST_VEIL_X_FADE))
             * MistSmooth((MIST_PLUME_X_FACE - p.x) * (1.0 / MIST_VEIL_FACE_FADE));
    float fy = 1.0 - MistSmooth((p.y - (MIST_VEIL_TOP - MIST_VEIL_TOP_FADE)) * (1.0 / MIST_VEIL_TOP_FADE));
    float fz = 1.0 - MistSmooth((abs(p.z - MIST_PLUME_Z) - (MIST_VEIL_Z_HALF - MIST_VEIL_Z_FADE)) * (1.0 / MIST_VEIL_Z_FADE));
    return fx * fy * fz;
}

// The plume breathing (14.5.3 "plume breathing (5 m/s)"): the formula of H2 is evaluated at a coordinate displaced by a
// large-scale noise (1.6 km tiles, 400 m cells), +-80 m in y and +-60 m in z, advected upward with the 60 m fbm, so the
// soft top and the Gaussian flanks billow in lobes of hundreds of metres. Integrated along km-long rays the 60 m fbm
// averages out (round 4: a flat wall); lobes of this size survive the integral and give the plume its silhouette.
#define MIST_WARP_TILE 1600.0
#define MIST_WARP_Y 160.0
#define MIST_WARP_Z 120.0
// The face onset (round 6): the plume starts at x -6 - dx with dx = (12 + 140 smoothstep(-60, 220, y)) x n(p) >= 0 (n a
// third fetch of the 400 m noise): below the crest the mist hugs the lee face (at most 12 m off it), above the crest its
// lobes lean off the face by up to 152 m, as an updraft separates at a crest edge. Without it the plume's inner boundary is
// the face plane itself, which an eye on the crest sees edge-on: a straight vertical edge through the vanishing point of
// the dam, from the deck to the zenith (wb-mist-05 pose_plume_b4). Never east of the face (dx >= 0).
#define MIST_ONSET_BASE 12.0
#define MIST_ONSET_TOP 140.0
#define MIST_ONSET_Y0 (-60.0)
#define MIST_ONSET_Y1 220.0
float3 MistWarp(float3 p)
{
    float3 q = p * (1.0 / MIST_WARP_TILE) + _BwMistWarp.xyz;   // own offset, wrapped on whole 1.6 km tiles (no seam)
    float wy = SAMPLE_TEXTURE3D_LOD(_BwMistNoise, sampler_BwMistNoise, q, 0).r - 0.5;
    float wz = SAMPLE_TEXTURE3D_LOD(_BwMistNoise, sampler_BwMistNoise, q.zxy + 0.41, 0).r - 0.5;
    // onset noise: the 400 m cells and one 100 m octave (a lumpy shoulder, not a uniformly shifted straight edge)
    float wx = 0.6 * SAMPLE_TEXTURE3D_LOD(_BwMistNoise, sampler_BwMistNoise, q.yzx + 0.73, 0).r
             + 0.4 * SAMPLE_TEXTURE3D_LOD(_BwMistNoise, sampler_BwMistNoise, q.zyx * 4.0 + 0.19, 0).r;
    float rise = smoothstep(MIST_ONSET_Y0, MIST_ONSET_Y1, p.y);
    float reach = MIST_ONSET_BASE + MIST_ONSET_TOP * rise;
    // above the crest the shoulder always leans off the face (at least 35 % of the reach), the noise adds the rest
    float dx = reach * lerp(saturate(1.6 * wx - 0.3), 0.35 + 0.65 * saturate(1.6 * wx - 0.3), rise);
    return float3(p.x + dx, p.y + MIST_WARP_Y * wy, p.z + MIST_WARP_Z * wz);   // pal:ok (not a colour)
}

// ------------------------------------------------------------------------------------------------ the dam's shadow
// The lee face is an opaque wall 6 km high at x -6 whose top is the crest: with the sun in the east (l.x > 0) a point west of
// it is in the dam's shadow when its ray toward the sun meets the face plane below the crest. Along a view ray the height
// where that sun ray crosses the face is LINEAR in the ray parameter (yHit = A + B t), so the lit fraction of a whole march
// segment is integrated exactly (the average of a clamped ramp: no banding where the shadow plane cuts the 24 samples).
// The soft edge is the sun's penumbra (0.0047 x the distance to the face, plus 0.3 m). Only the DIRECT share of the key
// (_BwSunDisc.w) is shadowed: the storm's diffuse key has no dam shadow. Seen from the crest the shadow is the plane through
// the crest edge and the sun: a great circle through the antisolar point (KS08: the bow lives in the lit mist above it).
float MistRampInt(float u) { return u <= 0.0 ? 0.0 : (u >= 1.0 ? u - 0.5 : 0.5 * u * u); }

float MistDamLit(float3 o, float3 d, float ta, float tb)
{
    float3 l = _BwSunDir.xyz;
    if (l.x < 0.02) return 1.0;                                             // the sun is not behind the dam
    float il = 1.0 / l.x;
    float A = o.y + l.y * (MIST_PLUME_X_FACE - o.x) * il - MIST_DAM_CREST_Y;
    float B = d.y - l.y * d.x * il;
    float tm = 0.5 * (ta + tb);
    float dist = max((MIST_PLUME_X_FACE - (o.x + d.x * tm)) * il, 0.0);
    float inv = 1.0 / (2.0 * (MIST_DAM_SOFT0 + MIST_SUN_PENUMBRA * dist));
    float ua = (A + B * ta) * inv + 0.5, ub = (A + B * tb) * inv + 0.5;
    if (abs(ub - ua) < 1e-4) return saturate(0.5 * (ua + ub));
    return saturate((MistRampInt(ub) - MistRampInt(ua)) / (ub - ua));
}

float MistDamLitAt(float3 p)
{
    float3 l = _BwSunDir.xyz;
    if (l.x < 0.02 || p.x >= MIST_PLUME_X_FACE) return 1.0;
    float dist = (MIST_PLUME_X_FACE - p.x) / l.x;
    float yHit = p.y + l.y * dist - MIST_DAM_CREST_Y;
    return saturate(yHit / (2.0 * (MIST_DAM_SOFT0 + MIST_SUN_PENUMBRA * dist)) + 0.5);
}

// Plume + veil density at p (with the breathing warp and the fbm modulation).
float MistGroupDensity(float3 p)
{
    float base = _BwMist0.x * MistPlumeBase(MistWarp(p)) + _BwMist0.y * MistVeilBase(p);
    if (base <= 1e-7) return 0.0;
    return base * (MIST_FBM_BASE + MIST_FBM_AMP * MistFbm(p, _BwMistWind.xyz));
}

// Self-shadowing of the plume (the light that reaches a sample through the plume itself; without it a 2.5 km plume is a
// flat wall of one brightness). Toward the sun: a probe 60 m along the sun direction (the fbm billows shade each other:
// form without a light march) plus the analytic path of the base density to where the light leaves the plume (the lee
// face for the eastern sun of beats 2-4, or the soft top); toward the sky: the analytic vertical column above the sample.
// Beer's law with the extinction halved (multiple scattering carries light deeper: Wrenninge's octave softening), and an
// ambient floor (light from below and the sides).
#define MIST_SHADOW_PROBE 60.0
#define MIST_SHADOW_SOFT 0.5
#define MIST_SHADOW_LARGE 0.6
#define MIST_AMB_FLOOR 0.35

float MistPlumeColumnUp(float3 p)
{
    float dz = p.z - MIST_PLUME_Z;
    float g = exp(-dz * dz * (0.5 / (MIST_PLUME_SIGMA_Z * MIST_PLUME_SIGMA_Z)));
    float f = MistSmooth((MIST_PLUME_X_FACE - p.x) * (1.0 / MIST_PLUME_FACE_FADE)) * MistSmooth((p.x - MIST_PLUME_X_FAR) * (1.0 / MIST_PLUME_FAR_FADE))
            * exp(-(MIST_PLUME_X_FACE - p.x) * (1.0 / MIST_PLUME_X_SCALE));
    float col = p.y >= 0.0 ? MIST_PLUME_H * exp(-p.y * (1.0 / MIST_PLUME_H)) : (MIST_PLUME_H - p.y);
    return _BwMist0.x * g * f * col;
}

// The probe's density: the plume formula with the fbm billows but without the breathing warp (4 texture fetches fewer per
// sample; the warp moves lobes by tens of metres, the probe only needs the local billow 60 m toward the sun).
float MistProbeDensity(float3 p)
{
    float base = _BwMist0.x * MistPlumeBase(p) + _BwMist0.y * MistVeilBase(p);
    if (base <= 1e-7) return 0.0;
    return base * (MIST_FBM_BASE + MIST_FBM_AMP * MistFbm(p, _BwMistWind.xyz));
}

void MistShadow(float3 p, float rho, out float tsun, out float tamb)
{
    float3 l = _BwSunDir.xyz;
    float rq = MistProbeDensity(p + l * MIST_SHADOW_PROBE);
    float tau = MIST_SIGMA * 0.5 * (rho + rq) * MIST_SHADOW_PROBE;
    float lFace = l.x > 0.05 ? (MIST_PLUME_X_FACE - p.x) / l.x : 1e5;
    float lTop = (max(-p.y, 0.0) + MIST_PLUME_H) / max(l.y, 0.05);
    float base = _BwMist0.x * MistPlumeBase(p);
    tau += MIST_SIGMA * MIST_SHADOW_LARGE * base * max(min(lFace, lTop) - MIST_SHADOW_PROBE, 0.0);
    tsun = exp(-MIST_SHADOW_SOFT * tau);
    tamb = MIST_AMB_FLOOR + (1.0 - MIST_AMB_FLOOR) * exp(-MIST_SHADOW_SOFT * MIST_SIGMA * MistPlumeColumnUp(p));
}

// ------------------------------------------------------------------------------------------------ phase and bow
// Henyey-Greenstein (1/sr)
float MistHg(float c, float g)
{
    float den = max(1.0 + g * g - 2.0 * g * c, 1e-4);
    return (1.0 - g * g) / (4.0 * BW_PI * den * sqrt(den));
}

struct MistLight
{
    float3 sun;        // key radiance (_BwSunColor) x mist sun gate
    float3 psun;       // pi x phase (multiple-scattering octaves, dark band) of this ray, per channel
    float3 bow;        // K x bow(theta) per channel x direct-sun share
    float3 amb;        // exterior ambient in-scatter radiance (isotropic sky trilight x albedo)
    float3 fwd;        // the haze-band radiance along the ray (BwFogColorRaw x albedo): what a forward-scattering spray passes on
    float vis;         // direct share of the key (_BwSunDisc.w): the part the dam can shadow
};

MistLight MistLightFor(float3 d)
{
    MistLight l;
    float3 sd = _BwSunDir.xyz;
    float c = dot(d, sd);
    float vis = saturate(_BwSunDisc.w);                                       // direct sun (the storm deck hides it)
    float pms = MistHg(c, MIST_HG_G) + 0.5 * MistHg(c, 0.5 * MIST_HG_G) + 0.25 * MistHg(c, 0.25 * MIST_HG_G);
    float piso = 1.75 / (4.0 * BW_PI);                                         // the same energy, isotropic (diffuse storm light)
    float theta = acos(clamp(-c, -1.0, 1.0)) * BW_RAD2DEG;                     // angle from the antisolar point
    float bandA = MistSmooth((theta - (MIST_BAND_IN - MIST_BAND_EDGE)) / (2.0 * MIST_BAND_EDGE));
    float bandB = MistSmooth((theta - (MIST_BAND_OUT - MIST_BAND_EDGE)) / (2.0 * MIST_BAND_EDGE));
    float band = 1.0 - (1.0 - MIST_BAND) * bandA * (1.0 - bandB);
    float bandOn = _BwMist0.z > 0.0 ? band : 1.0;                              // the bow-off render has no dark band either
    float p = min(BW_PI * lerp(piso, pms * bandOn, vis), MIST_PHASE_CLAMP);
    l.psun = p.xxx;
    float3 tp = (theta.xxx - float3(42.0, 41.0, 40.3)) * (1.0 / MIST_BOW_WIDTH);   // H2 primary (R, G, B)   pal:ok (angles)
    float3 ts = (theta.xxx - float3(50.8, 52.0, 53.5)) * (1.0 / MIST_BOW_WIDTH);   // H2 secondary            pal:ok (angles)
    float3 bow = exp(-tp * tp) + MIST_BOW_SECONDARY * exp(-ts * ts);
    l.bow = bow * (_BwMist0.z * vis);
    // the storm's hidden key (14.2.3 row 2, x0.35 'hidden') is the SURFACES' stand-in for the glow of the storm deck; the
    // mist already takes that glow from the sky dome itself (_BwMistAmb), so only the direct sun lights it (vis: 0 in the
    // storm, 1 from s 590): the plume under the storm is the dome average, a grey mass darker than the bright horizon and
    // lighter than the lowland (round 6: with the key added it matched the horizon and vanished, KS06 +2 levels)
    l.sun = _BwSunColor.rgb * (_BwMist0.w * lerp(MIST_STORM_KEY, 1.0, vis));
    l.vis = vis;
    l.amb = _BwMistAmb.w > 0.5 ? _BwMistAmb.rgb
          : (_BwAmbExtSky.rgb + 2.0 * _BwAmbExtMid.rgb + _BwAmbExtGround.rgb) * (0.25 * MIST_ALBEDO);   // fallback: the trilight
    l.fwd = BwFogColorRaw(d);
    // sun-break gold (fix wave X2 S3-1, beat 3 'awe, relief, gold'): through s 560-640 the mist's sun light turns toward
    // BreakGold at the same luminance, the dome ambient part of the way, and a clamped forward lobe toward the sun is added,
    // so the veil inside the bow and the plume against the sun read gold instead of khaki (x2 KS08: saturation 0.08)
    float gw = saturate(_BwMistGold.x);
    if (gw > 0.0)
    {
        float3 gN = PAL_BreakGold / max(BwLuma(PAL_BreakGold), 1e-4);
        l.sun = lerp(l.sun, BwLuma(l.sun) * gN, gw * MIST_GOLD_SUN);
        l.amb = lerp(l.amb, BwLuma(l.amb) * gN, gw * MIST_GOLD_AMB);
        l.psun += min(MIST_GOLD_LOBE * BW_PI * MistHg(c, MIST_GOLD_LOBE_G), MIST_GOLD_LOBE_CLAMP) * (gw * vis);
    }
    return l;
}

// ------------------------------------------------------------------------------------------------ accumulation
struct MistAcc
{
    float3 L;        // in-scattered radiance (premultiplied)
    float T;         // transmittance
    float B;         // sum T a Tsun Tfog: the bow integral (bow colour applied at the end, clamped)
    float tw;        // absorption-weighted distance (sum T a t)
    float w;         // sum T a
};

MistAcc MistAccInit() { MistAcc a; a.L = 0; a.T = 1.0; a.B = 0; a.tw = 0; a.w = 0; return a; }

// One homogeneous segment: optical depth tau, sun visibility ts, haze transmittance tf, distance t, own radiance per unit
// absorbed light: sunScale x (sun psun) + ambient.
void MistAddSegmentS(inout MistAcc acc, MistLight l, float tau, float ts, float tf, float t, float sunW, float3 extraAmb, float shSun, float shAmb)
{
    if (tau <= 0.0) return;
    float a = 1.0 - exp(-tau);
    float k = acc.T * a;
    acc.L += k * tf * (l.sun * l.psun * (ts * sunW * shSun) + (sunW > 0.0 ? l.amb * shAmb : 0.0) + extraAmb);
    acc.B += k * tf * ts * sunW * shSun;
    acc.tw += k * t;
    acc.w += k;
    acc.T *= 1.0 - a;
}

void MistAddSegment(inout MistAcc acc, MistLight l, float tau, float ts, float tf, float t, float sunW, float3 extraAmb)
{
    MistAddSegmentS(acc, l, tau, ts, tf, t, sunW, extraAmb, 1.0, 1.0);
}

// ------------------------------------------------------------------------------------------------ ellipsoid sprays
// Closed-form chord of the unit profile (1 - r^2)^2 along t in [ta, tb] (o2, d2 in the ellipsoid's unit frame, d2 per world
// metre): returns A^2 (F(u1) - F(u0)) (x sigma rho = optical depth), the half chord sqrt(q) of the unit ellipsoid and the
// clipped chord's midpoint t.
float MistUnitChord(float3 o2, float3 d2, float ta, float tb, out float sq, out float tm)
{
    sq = 0.0; tm = 0.5 * (ta + tb);
    float A = dot(d2, d2);
    float Bh = dot(o2, d2);
    float sm = -Bh / max(A, 1e-12);                                            // closest approach
    float q = 1.0 - (dot(o2, o2) - Bh * Bh / max(A, 1e-12));                   // 1 - r_min^2
    if (q <= 0.0 || tb <= ta) return 0.0;
    float h = sqrt(q / max(A, 1e-12));
    float u0 = max(-h, ta - sm), u1 = min(h, tb - sm);
    if (u1 <= u0) return 0.0;
    float h2 = h * h, h4 = h2 * h2;
    float F1 = h4 * u1 - (2.0 / 3.0) * h2 * u1 * u1 * u1 + u1 * u1 * u1 * u1 * u1 * 0.2;
    float F0 = h4 * u0 - (2.0 / 3.0) * h2 * u0 * u0 * u0 + u0 * u0 * u0 * u0 * u0 * 0.2;
    sq = sqrt(q);
    tm = sm + 0.5 * (u0 + u1);
    return A * A * max(F1 - F0, 0.0);
}

// Closed-form chord through the unit-profile ellipsoid of slot i, clipped to [tNear, tDepth]. Returns tau and the chord
// midpoint distance. (Wave X: wall sprays are now one ellipsoid from the water to the top of the thrown water, the slap a
// raised column; MistVolumeMath.WallColumn / SlapCentreK. A split ellipsoid with a longer lower half showed a seam at its
// centre plane, mistx-14 KS04.)
float MistSprayTau(int i, float3 o, float3 d, float tNear, float tDepth, out float tMid)
{
    tMid = 0.0;
    float4 C = _BwMistSprayC[i], R = _BwMistSprayR[i], S = _BwMistSprayS[i];
    if (C.w <= 0.0) return 0.0;
    float cy = cos(R.w), sy = sin(R.w);
    float3 fwd = float3(sy, 0.0, cy), right = float3(cy, 0.0, -sy);           // pal:ok (not a colour); yaw about +Y (0 = +Z)
    // the downwind lean is a shear of the axis (x, z offset S.xy per metre of height): undo it on the ray (linear, so the
    // chord stays closed-form)
    float3 rel = o - C.xyz;
    rel.xz -= S.xy * rel.y;
    float3 dd = d;
    dd.xz -= S.xy * d.y;
    float3 inv = 1.0 / max(R.xyz, 1e-3);
    float3 o2 = float3(dot(rel, right), rel.y, dot(rel, fwd)) * inv;   // pal:ok (not a colour)
    float3 d2 = float3(dot(dd, right), dd.y, dot(dd, fwd)) * inv;   // pal:ok (not a colour)
    float sqm;
    float ch = MistUnitChord(o2, d2, tNear, tDepth, sqm, tMid);
    if (ch <= 0.0) return 0.0;
    float q = sqm * sqm;
    float3 pm = o + d * tMid;
    // billows: two fetches of the noise in spray-local coordinates (the texture rides with the spray), a deeper
    // modulation than the plume's (an 8 m burst of spray is lumpy; the 60 m fbm of H2 would leave it a smooth blob);
    // wall sprays: 16 m tiles evolving at 0.5 m/s; kraken sprays: tiles scaled with the spray, taller than wide for
    // cascading water, scrolled down (falling) or up (thrown) by the CPU offset
    float3 pr = pm - C.xyz;
    pr.xz -= S.xy * pr.y;
    float3 nq = pr * S.zwz + _BwMistSprayN[i].xyz;
    float nb = 0.6 * SAMPLE_TEXTURE3D_LOD(_BwMistNoise, sampler_BwMistNoise, nq, 0).r
             + 0.4 * SAMPLE_TEXTURE3D_LOD(_BwMistNoise, sampler_BwMistNoise, nq * 2.7 + 0.53, 0).r;
    float fb = MIST_SPRAY_BASE + (1.0 - MIST_SPRAY_BASE) * smoothstep(0.15, 0.85, nb);
    // ragged outline (Wave X): toward the silhouette (q -> 0, the ray grazes the ellipsoid) only the noise peaks keep their
    // density, so a spray is a lumpy burst of water and never a smooth oval (mistx-02 KS11: the white wall plume read as a
    // cotton ball with an elliptic edge). sqrt(q) is the half chord of the unit ellipsoid (1 on the axis, 0 at the edge).
    // the upper part is eroded harder (thrown water thins and breaks up as it rises; the KS11 wall plume was a smooth puff
    // at noon): + MIST_SPRAY_ERODE_TOP x the height of the chord midpoint over the vertical semi-axis
    float yN = saturate(pr.y / max(R.y, 1e-3));
    float erode = smoothstep(0.0, MIST_SPRAY_ERODE_SOFT, nb - MIST_SPRAY_ERODE - MIST_SPRAY_ERODE_TOP * yN + MIST_SPRAY_ERODE_IN * sqrt(q));
    // near the eye (Wave X): a chord whose midpoint is under 6 m away fades to nothing at 1.5 m, so a wall spray that the
    // reef wind carries over the road does not white out the frame when it reaches the camera (mistx-05/06-seqk5 at KS11:
    // frame mean 130 -> 159 at up to 6.8 levels per frame while the spray swept over the eye; 14.10.3 allows 1.2-2.0)
    float nearW = smoothstep(MIST_SPRAY_NEAR0, MIST_SPRAY_NEAR1, tMid);
    return MIST_SIGMA * C.w * fb * erode * nearW * ch;
}

// ------------------------------------------------------------------------------------------------ conduit spray (beat 7)
// Closed-form optical depth through the conduit box: extinction sigma0 exp(-(y - water) / H) (a ray's y is linear in t,
// so the integral is an exponential difference), plus 3 sigma0 from the drop shaft (u >= shaftU) to the ceiling; one fbm
// fetch at the chord midpoint in drift-advected conduit coordinates (64 m tiles, 4 m/s air drift: below 1 Hz at a point).
float MistConduitTau(float3 o, float3 d, float tNear, float tDepth, out float tMid)
{
    tMid = 0.0;
    if (_BwMistBoxL.w <= 0.0) return 0.0;
    float2 rel = o.xz - _BwMistBox0.xz;
    float3 lo = float3(dot(rel, _BwMistBox1.xy), dot(rel, _BwMistBox1.zw), o.y);      // pal:ok (not a colour)
    float3 ld = float3(dot(d.xz, _BwMistBox1.xy), dot(d.xz, _BwMistBox1.zw), d.y);    // pal:ok (not a colour)
    float3 bmin = float3(_BwMistBox2.x, _BwMistBox2.y, _BwMistBox2.z);                 // pal:ok (not a colour)
    float3 bmax = float3(_BwMistBox3.x, _BwMistBox3.y, _BwMistBox3.z);                 // pal:ok (not a colour)
    float3 inv = 1.0 / (abs(ld) > 1e-6 ? ld : (ld >= 0.0 ? 1e-6 : -1e-6));
    float2 te = MistBox(lo, inv, bmin, bmax);
    float t0 = max(te.x, tNear), t1 = min(te.y, tDepth);
    if (t1 <= t0) return 0.0;
    float H = max(_BwMistBox2.w, 0.05), wy = _BwMistBox2.z, s0 = _BwMistBox0.w;
    float y0 = lo.z + ld.z * t0, y1 = lo.z + ld.z * t1;
    float tau = abs(ld.z) > 1e-4
        ? s0 * H / ld.z * (exp(-(y0 - wy) / H) - exp(-(y1 - wy) / H))
        : s0 * exp(-(y0 - wy) / H) * (t1 - t0);
    // the drop shaft: 3 sigma0 for u >= shaftU
    float us = _BwMistBox3.w;
    float ua = lo.x + ld.x * t0, ub = lo.x + ld.x * t1;
    float fa = abs(ub - ua) > 1e-5 ? saturate((max(ua, ub) - us) / abs(ub - ua)) : (ua >= us ? 1.0 : 0.0);
    tau += 3.0 * s0 * fa * (t1 - t0);
    tMid = 0.5 * (t0 + t1);
    float3 pm = o + d * tMid;
    float2 rm = pm.xz - _BwMistBox0.xz;
    float3 nc = float3(dot(rm, _BwMistBox1.xy) * (1.0 / 64.0) - _BwMistBoxF.x, pm.y * (1.0 / 16.0), dot(rm, _BwMistBox1.zw) * (1.0 / 16.0));   // pal:ok (not a colour)
    float fb = MIST_FBM_BASE + MIST_FBM_AMP * SAMPLE_TEXTURE3D_LOD(_BwMistNoise, sampler_BwMistNoise, nc, 0).r;
    return max(tau, 0.0) * fb;
}

// ------------------------------------------------------------------------------------------------ curtain
struct MistCurtain
{
    float s;       // ray parameter of the hit (-1: none)
    float tau;     // optical depth along the ray
    float water;   // water share of tau (0..1)
    float3 p;      // hit point (world)
    float3 n;      // surface normal facing the eye
};

// The two intersections of a ray with the sheet surface y = -11.5 - k (x + 6)^2, k = 4.9 / 8.3^2 (one stable quadratic):
// the region under the surface is convex, so a ray from the crest enters it at the near root (the top of the sheet, the
// brow within 9.85 m of drop as seen from the KS09 eye) and leaves it at the far root (deep, seen from below). Both are
// curtain layers where they fall in its drop range (rays through a pier gap meet the deep curtain at their far root).
void MistCurtainRoots(float3 o, float3 d, out float s0, out float s1)
{
    s0 = -1.0; s1 = -1.0;
    if (_BwMist1.z <= 0.0) return;
    float k = MIST_GRAVITY_HALF / (MIST_LIP_SPEED * MIST_LIP_SPEED);
    float ox = o.x - MIST_LIP_X;
    float a = k * d.x * d.x, b = 2.0 * k * d.x * ox + d.y, cc = k * ox * ox + (o.y - MIST_LIP_Y);
    if (a < 1e-9)
    {
        if (abs(b) > 1e-9) s0 = -cc / b;
        return;
    }
    float disc = b * b - 4.0 * a * cc;
    if (disc < 0.0) return;
    float sq = sqrt(disc);
    float q = -0.5 * (b + (b >= 0.0 ? sq : -sq));
    float r0 = q / a, r1 = abs(q) > 1e-9 ? cc / q : -1.0;
    s0 = min(r0, r1); s1 = max(r0, r1);
}

// The curtain layer at ray parameter s (tau 0 when the point is not on the curtain: east of the lip, outside the drop
// range, between the slots before they merge): the across-slot profile, the water fraction and the advected streaks.
MistCurtain MistCurtainAt(float3 o, float3 d, float s)
{
    MistCurtain c;
    c.s = s; c.tau = 0.0; c.water = 0.0; c.p = o + d * s; c.n = float3(0, 1, 0);   // pal:ok (normal)
    if (s <= 0.0) return c;
    float3 p = c.p;
    if (p.x > MIST_LIP_X) return c;
    float drop = MIST_LIP_Y - p.y;
    if (drop < MIST_CURTAIN_FADE_IN0 || drop > MIST_CURTAIN_END1) return c;
    float k = MIST_GRAVITY_HALF / (MIST_LIP_SPEED * MIST_LIP_SPEED);
    float radius = MIST_CURTAIN_R0 * pow(max(drop, MIST_CURTAIN_D0) * (1.0 / MIST_CURTAIN_D0), MIST_CURTAIN_R_EXP);
    // across-slot profile: the 14 slots blurred by the radius (the piers' 4 m gaps fill in as the curtain spreads)
    float inv = 1.0 / (radius * 1.41421356);
    float fz = 0.0;
    [loop] for (int i = 0; i < MIST_SLOT_COUNT; i++)
    {
        float z0 = MIST_SLOT_Z0 + MIST_SLOT_PITCH * i;
        fz += 0.5 * (MistErf((p.z - z0) * inv) - MistErf((p.z - z0 - MIST_SLOT_LEN) * inv));
    }
    fz = saturate(fz);
    if (fz <= 1e-4) return c;
    float u = saturate((drop - MIST_CURTAIN_D0) / (MIST_CURTAIN_D1 - MIST_CURTAIN_D0));
    float w = (1.0 - u) * (1.0 - u);                                            // water fraction 1 at 90 m, 0 at 1000 m
    float fin = MistSmooth((drop - MIST_CURTAIN_FADE_IN0) / (MIST_CURTAIN_FADE_IN1 - MIST_CURTAIN_FADE_IN0));
    float fout = 1.0 - MistSmooth((drop - MIST_CURTAIN_END0) / (MIST_CURTAIN_END1 - MIST_CURTAIN_END0));
    float tw = MIST_CURTAIN_WATER_TAU * w, tm = MIST_CURTAIN_MIST_TAU * (1.0 - w);
    // advected streaks: the Lagrangian coordinate (departure time x 8.3 m/s) in 40 m cells, so a feature passes a fixed
    // point in at least 4.8 s (0.21 Hz, 14.2.5 "sheets advect, never flash"); narrow across the fall (9 m cells in z)
    float tflight = sqrt(drop / MIST_GRAVITY_HALF);
    float3 nc = float3(p.z * (1.0 / 9.0), _BwMist1.w - tflight * (MIST_LIP_SPEED / 40.0), drop * (1.0 / 160.0));   // pal:ok (not a colour)
    float streak = 0.55 + 0.45 * SAMPLE_TEXTURE3D_LOD(_BwMistNoise, sampler_BwMistNoise, nc * 0.25, 0).r;
    float3 n = normalize(float3(2.0 * k * (p.x - MIST_LIP_X), 1.0, 0.0));         // pal:ok (normal)
    float cosv = abs(dot(n, d));
    c.tau = min((tw + tm) * fz * fin * fout * streak / max(cosv, 0.2), 12.0);
    c.water = (tw + tm) > 0.0 ? tw / (tw + tm) : 0.0;
    c.n = dot(n, d) < 0.0 ? n : -n;
    return c;
}

// The curtain as one thin layer of the march: white water (Lambert-like, wrapped: aerated water glows through) mixed with
// mist (the ray's phase and bow) by the water share.
void MistAddCurtain(inout MistAcc acc, MistLight l, MistCurtain c)
{
    if (c.tau <= 0.0) return;
    float ts = BwSunBreak(c.p) * lerp(1.0, MistDamLitAt(c.p), l.vis);
    float tf = BwFogTransmittance(c.p);
    float a = 1.0 - exp(-c.tau);
    float k = acc.T * a;
    float wrap = 0.3 + 0.7 * saturate(dot(c.n, _BwSunDir.xyz));
    float3 lw = MIST_WATER_ALBEDO * PAL_FoamWhite * (l.sun * (ts * wrap) + BwAmbientAt(c.n, c.p));
    float3 lm = l.sun * l.psun * ts + l.amb;
    acc.L += k * tf * lerp(lm, lw, c.water);
    acc.B += k * tf * ts * (1.0 - c.water);
    acc.tw += k * c.s;
    acc.w += k;
    acc.T *= 1.0 - a;
}

// ------------------------------------------------------------------------------------------------ the ray
// rgb = in-scattered radiance (premultiplied, haze in front included), a = transmittance of the scene behind.
// tDepth: distance to the surface of this sample (1e7 = sky); tFar: the farthest of the pixel's samples (the grid).
float4 MistIntegrate(float3 o, float3 d, float tDepth, float tFar)
{
    MistAcc acc = MistAccInit();
    if (_BwMist1.y <= 0.0) return float4(0, 0, 0, 1);   // pal:ok (no mist)
    MistLight l = MistLightFor(d);
    float3 invD = 1.0 / (abs(d) > 1e-6 ? d : (d >= 0.0 ? 1e-6 : -1e-6));
    const float tNear = 0.05;

    // ---- sprays (nearer than the plume for every eye on the crest: wall sprays are east of x -6, the plume west of it)
    // White-water spray is made of large drops (0.1-1 mm, Mie asymmetry about 0.85): most of the sky light it scatters
    // toward the eye comes from just behind it, the haze band along the ray. Its ambient in-scatter is therefore the dome
    // average blended toward that band radiance by the slot's forward weight (_BwMistSprayN.w, 0.85), with the albedo of
    // water (MIST_SPRAY_ALBEDO 1.0: a 1 mm drop absorbs under 0.04 %): without it a thick spray under a storm sky had the
    // dome average x 0.92 (0.55 of the horizon) and read as dark smoke against the horizon (round 8: KS06, pose_spray175,
    // the kraken sprays). Against the darker upper sky and the sea the spray now reads lighter, as white water does.
    int ns = (int)_BwMist1.x;
    [loop] for (int i = 0; i < ns; i++)
    {
        float tm;
        float tau = MistSprayTau(i, o, d, tNear, tDepth, tm);
        if (tau <= 1e-5) continue;
        float3 pm = o + d * tm;
        float ts = BwSunBreak(pm), tf = BwFogTransmittance(pm), sunW = _BwMistSprayL[i].w;
        float3 ambS = lerp(l.amb * (MIST_SPRAY_ALBEDO / MIST_ALBEDO), l.fwd * MIST_SPRAY_ALBEDO, _BwMistSprayN[i].w);
        float3 rm = l.sun * l.psun * (ts * sunW) + (sunW > 0.5 ? ambS : l.amb * step(1e-4, sunW)) + _BwMistSprayL[i].rgb;
        // Wave X: a THICK exterior spray is white water, lit like the foam at its foot and the curtain's aerated water
        // (Lambert-like with a wrapped key, BwAmbientAt of a normal facing the eye and tilted up): the forward-scatter
        // term alone left every kraken and wall spray at 0.93 of the horizon behind it, a dark smoke (wb-mist-09-kq,
        // pose_spray175). The white share grows with the optical depth along this ray (1 - exp(-tau / 0.8), at most
        // the slot's share: 0.85 for kraken sprays, 0.5 for the wall sprays, which must not strobe near the eye), so the thin
        // edges stay mist and blend into the haze without an outline.
        float wW = sunW > 0.5 ? saturate(_BwMistSprayW[i].x) * (1.0 - exp(-tau * (1.0 / MIST_SPRAY_WHITE_TAU))) : 0.0;
        float3 nS = normalize(float3(-d.x, MIST_SPRAY_NUP, -d.z));   // pal:ok (normal)
        float wrap = 0.3 + 0.7 * saturate(dot(nS, _BwSunDir.xyz));
        float3 rw = MIST_WATER_ALBEDO * PAL_FoamWhite * (l.sun * (ts * wrap) + BwAmbientAt(nS, pm));
        float a = 1.0 - exp(-tau);
        float k = acc.T * a;
        acc.L += k * tf * lerp(rm, rw, wW);
        acc.B += k * tf * ts * sunW * (1.0 - wW);
        acc.tw += k * tm;
        acc.w += k;
        acc.T *= 1.0 - a;
    }

    // ---- the beat 7 conduit spray (in the tunnel: nearer than anything else of the mist)
    {
        float tm;
        float tau = MistConduitTau(o, d, tNear, tDepth, tm);
        if (tau > 1e-5)
        {
            float3 pm = o + d * tm;
            MistAddSegment(acc, l, tau, 1.0, BwFogTransmittance(pm), tm, 0.0, _BwMistBoxL.rgb + 0.5 * MIST_ALBEDO * BwAmbientAt(float3(0, 1, 0), pm));   // pal:ok (normal)
        }
    }

    // ---- plume + veil (24 fixed samples between the box entry and exit) with the curtain layer inside
    float3 bmin = float3(MIST_PLUME_X_FAR, MIST_PLUME_Y_MIN, MIST_PLUME_Z - MIST_PLUME_Z_HALF);   // pal:ok (not a colour)
    float3 bmax = float3(MIST_PLUME_X_FACE, MIST_PLUME_Y_MAX, MIST_PLUME_Z + MIST_PLUME_Z_HALF);   // pal:ok (not a colour)
    if (_BwMist0.y > 0.0)
    {
        bmin.z = min(bmin.z, MIST_PLUME_Z - MIST_VEIL_Z_HALF);
        bmax.z = max(bmax.z, MIST_PLUME_Z + MIST_VEIL_Z_HALF);
    }
    float2 te = MistBox(o, invD, bmin, bmax);
    float t0 = max(te.x, tNear), t1 = min(te.y, tFar);
    if (t1 > t0 && t0 < tDepth)
    {
        float cs0, cs1;
        MistCurtainRoots(o, d, cs0, cs1);
        MistCurtain cur = MistCurtainAt(o, d, cs0);
        MistCurtain cur1 = MistCurtainAt(o, d, cs1);
        bool curPending = cur.s > 0.0 && cur.s <= tDepth && cur.tau > 0.0;
        bool cur1Pending = cur1.s > 0.0 && cur1.s <= tDepth && cur1.tau > 0.0;
        if (!curPending && cur1Pending) { cur = cur1; curPending = true; cur1Pending = false; }
        float span = t1 - t0;
        // haze optical depth at the start, middle and end of the grid, interpolated piecewise linearly in between
        float f0 = -log(max(BwFogTransmittance(o + d * t0), 1e-6));
        float fm = -log(max(BwFogTransmittance(o + d * (t0 + 0.5 * span)), 1e-6));
        float f1 = -log(max(BwFogTransmittance(o + d * t1), 1e-6));
        float ua = 0.0;
        [loop] for (int j = 0; j < MIST_SAMPLES; j++)
        {
            float ub = pow(max((j + 1) * (1.0 / MIST_SAMPLES), 1e-6), MIST_SPACING);
            float ta = t0 + span * ua, tb = t0 + span * ub;
            ua = ub;
            if (ta >= tDepth) break;
            float tbc = min(tb, tDepth);
            float tm = 0.5 * (ta + tbc);
            bool curHere = curPending && cur.s <= tb;
            if (curHere && cur.s < tm) { MistAddCurtain(acc, l, cur); curPending = false; }
            float3 pm = o + d * tm;
            float rho = MistGroupDensity(pm);
            if (rho > 0.0)
            {
                float um = (tm - t0) / span;
                float ftau = um < 0.5 ? lerp(f0, fm, 2.0 * um) : lerp(fm, f1, 2.0 * um - 1.0);
                float shSun, shAmb;
                MistShadow(pm, rho, shSun, shAmb);
                float tsun = BwSunBreak(pm) * lerp(1.0, MistDamLit(o, d, ta, tbc), l.vis);
                MistAddSegmentS(acc, l, MIST_SIGMA * rho * (tbc - ta), tsun, exp(-ftau), tm, 1.0, 0.0, shSun, shAmb);
            }
            if (curHere && curPending) { MistAddCurtain(acc, l, cur); curPending = false; }
            if (!curPending && cur1Pending) { cur = cur1; curPending = true; cur1Pending = false; }
            if (acc.T < 1e-3) break;
        }
        if (curPending) MistAddCurtain(acc, l, cur);
        if (cur1Pending) MistAddCurtain(acc, l, cur1);
    }

    if (acc.w <= 0.0) return float4(0, 0, 0, 1);   // pal:ok (no mist on this ray)
    // the rainbow (H2): in-scatter += sun radiance x rho dt x bow x K, clamped at 2.0 per channel
    acc.L += min(l.sun * l.bow * acc.B, MIST_BOW_CLAMP);
    // the haze between the eye and the mist: the scene colour already carries it, but the mist's transmittance removed
    // that part with the rest; add it back at the mist's absorption-weighted mean distance
    float tbar = acc.tw / acc.w;
    acc.L += BwFogApply(float3(0, 0, 0), o + d * tbar) * (1.0 - acc.T);   // pal:ok (black: fog in-scatter only)
    return float4(acc.L, acc.T);   // pal:ok (not a colour)
}

// Does the ray touch any mist volume before tFar? (mark pass: only such edge pixels need the per-sample pass)
bool MistRayTouches(float3 o, float3 d, float tFar)
{
    if (_BwMist1.y <= 0.0) return false;
    float3 invD = 1.0 / (abs(d) > 1e-6 ? d : (d >= 0.0 ? 1e-6 : -1e-6));
    float3 bmin = float3(MIST_PLUME_X_FAR, MIST_PLUME_Y_MIN, MIST_PLUME_Z - (_BwMist0.y > 0.0 ? MIST_VEIL_Z_HALF : MIST_PLUME_Z_HALF));   // pal:ok (not a colour)
    float3 bmax = float3(MIST_PLUME_X_FACE, MIST_PLUME_Y_MAX, MIST_PLUME_Z + (_BwMist0.y > 0.0 ? MIST_VEIL_Z_HALF : MIST_PLUME_Z_HALF));   // pal:ok (not a colour)
    float2 te = MistBox(o, invD, bmin, bmax);
    if (min(te.y, tFar) > max(te.x, 0.05)) return true;
    float tc;
    if (MistConduitTau(o, d, 0.05, tFar, tc) > 1e-5) return true;
    int ns = (int)_BwMist1.x;
    [loop] for (int i = 0; i < ns; i++)
    {
        float4 C = _BwMistSprayC[i], R = _BwMistSprayR[i];
        if (C.w <= 0.0) continue;
        float r = max(R.x, max(R.y, R.z)) * (1.0 + length(_BwMistSprayS[i].xy));   // the lean moves the top by up to |lean| x height
        float3 rel = o - C.xyz;
        float b = dot(rel, d), c = dot(rel, rel) - r * r;
        if (b * b - c > 0.0 && -b + sqrt(max(b * b - c, 0.0)) > 0.0) return true;
    }
    return false;
}

#endif
