// NocScreenWave.hlsl - the wall of signs as a 2 km LED screen facade (MASTER_PROMPT 14.5.6, 14.2.5 NocScreen, D-010; SG, Phase 4
// wave A, INT-02 / CRIT0 #14). Included by Shaders/Beats/NocScreen.shader (the face x 874) and, for the H4 wall plane of kind 2,
// by NocMirror.hlsl (FX), so the wet promenade mirrors the same colour wave. C# twin: Beats/Beat06_Signs/NocScreenMath.cs.
//
// Layers (all in metres on the face: zl = z - 275, y):
//   1. the colour wave of 14.5.6 (phi1..phi3, index h) on the Look v2 ramp (teal, ice, amber -> ice over s 1 230-1 290, cyan;
//      desaturated 35 %, mixed 15 % toward ScreenBase, luminance-normalised k 0.8 to a geometric mean of 0.27);
//   2. imagery: the wall is a mosaic of displays (columns of two panels, 72.8 m, display heights of 3..7 panels aligned to the
//      seams), each running a slow programme that changes every 40-75 s with a 12 s crossfade: the bare wave, a large abstract
//      glyph on a dark field, aurora bands, a luminous disc (a "sun") on a dark gradient, a scrolling column of glyph cells, a
//      two-tone field split by a glowing horizon, a tiled picture (every panel at its own breathing level) and standby (dark);
//      each epoch also draws a display level (0.95 / 1.2 / 1.5, 25 / 50 / 25 %) and m is capped at 2.42 (the bloom check).
//      Each returns a luminance factor m (mean about 1.09: it makes up the LED hardware's losses, seams, joints and panel
//      brightness 0.88, so the wall keeps the 14.5.6 mean) and an accent weight a (the ramp one stop on, mixed 35 % toward ice).
//      No text, no people; every temporal frequency <= 0.11 Hz; wall-pixel luminance rate <= 0.04 /s (Beat06SignsTests);
//   3. hardware: panels 36 x 20 m on a 36.4 x 20.4 m pitch (0.4 m gap + 0.12 m bezels, box-filtered), per-panel brightness,
//      white point and age (18 % aged: dimmer, yellowed, mottled), 1.2 % dead panels, dead modules (8 % of the modules of 1.8 %
//      of the panels) and a stuck module column on 1 % (static: nothing flashes, nothing pops); 4 m modules with calibration
//      steps and 5 cm joints faded to their mean below 5.5 px of module pitch; 0.25 m LED pixels (a 0.66 dot over a 15 % diffuser
//      floor, energy kept) faded in from 5 to 10 px of pitch, and while they are resolved the imagery is sampled at the pixel
//      centres (the staircase of a real LED screen near the walker);
//   4. emissive depth: the LED view-angle falloff (0.70 at grazing, cos^0.4, a slight cool shift), so the screen faces the street.
// Time arrives as the global _NocScreenT = (frac(t / 58), frac(t / 60), t mod 600, waves) from SignsDriver in double (Part 10
// item 33: the 600 s wrap is a whole number of every display period, so no term jumps). Every function here is derivative-free;
// the caller passes the per-axis footprint fw (m per pixel), taken before any branch.
#ifndef NOC_SCREEN_WAVE_INCLUDED
#define NOC_SCREEN_WAVE_INCLUDED

#include "../Include/NocAtmosphere.hlsl"

float4 _NocScreenT;     // x frac(t / 58), y frac(t / 60), z t mod 600, w waves (1 or 3); 0 when nobody wrote it

#define SCR_Z0 275.0
#define SCR_PZ 36.4
#define SCR_PY 20.4
#define SCR_GAP 0.2          // half gap (the 0.4 m seam)
#define SCR_BEZ 0.32         // half gap + bezel 0.12
#define SCR_MOD 4.0
#define SCR_JOINT 0.05
#define SCR_LED 0.25
#define SCR_DOT 0.66
#define SCR_DW 72.8
#define SCR_FADE 12.0        // programme crossfade (s)
#define SCR_COLV 0.15        // glyph-column scroll speed (m/s)
#define SCR_MMAX 2.42        // the cap of the imagery factor m (NocScreenMath.ContentMaxM)
#define SCR_TAU 6.28318530718

float NocScrLum(float3 c) { return dot(c, float3(0.2126, 0.7152, 0.0722)); }
float3 NocScrMix(float3 c) { c = lerp(c, NocScrLum(c).xxx, 0.35); return lerp(c, PAL_ScreenBase, 0.15); }

// the Look v2 ramp with the D-010 normalisation (constant-folded): teal, ice, amber (-> ice by s 1 290), cyan, periodic in h
float3 NocScreenRamp(float h)
{
    float3 t = NocScrMix(PAL_ScreenTeal), w = NocScrMix(PAL_SignWhite), a = NocScrMix(PAL_SodiumAmber), c = NocScrMix(PAL_NeonCyan);
    float yt0 = NocScrLum(t), yw = NocScrLum(w), ya = NocScrLum(a), yc = NocScrLum(c);
    float yg = pow(yt0 * yw * ya * yc, 0.25);
    float ky = NC_ScreenRampY / yg;
    t *= ky * pow(yg / yt0, 0.8);
    w *= ky * pow(yg / yw, 0.8);
    a *= ky * pow(yg / ya, 0.8);
    c *= ky * pow(yg / yc, 0.8);
    a = lerp(w, a, saturate(_NocAmber.y));
    float x = frac(h) * 4.0;
    return x < 1.0 ? lerp(t, w, x) : (x < 2.0 ? lerp(w, a, x - 1.0) : (x < 3.0 ? lerp(a, c, x - 2.0) : lerp(c, t, x - 3.0)));
}

// the ice stop (the accent target)
float3 NocScreenIce() { return NocScreenRamp(0.25); }

// colour index of 14.5.6 at a wall point (world z, y); waves 1 (phi1) or 3
float NocScreenIndex(float z, float y, float waves)
{
    float h = 0.5 + 0.28 * sin(SCR_TAU * ((0.85 * z + 0.53 * y) / 520.0 - _NocScreenT.x));
    [branch] if (waves > 1.5)
    {
        h += 0.18 * sin(SCR_TAU * ((-0.60 * z + 0.80 * y) / 360.0 + _NocScreenT.y));
        h += 0.12 * sin(SCR_TAU * ((0.30 * z - 0.95 * y) / 700.0 - _NocScreenT.x));
    }
    return frac(h);
}

// H4 wall plane of kind 2 (NocMirror): the wave at the hit, scaled so its mean luminance is that of the registered macro colour
float3 NocScreenMirror(float3 H, float3 meanColour)
{
    float waves = _NocScreenT.w > 1.5 ? 3.0 : 1.0;
    return NocScreenRamp(NocScreenIndex(H.z, H.y, waves)) * (NocScrLum(meanColour) / NC_ScreenRampY);
}

// ------------------------------------------------------------------------------------------------ imagery
float NocScrCapsule(float2 p, float2 a, float2 b)
{
    float2 pa = p - a, ba = b - a;
    float h = saturate(dot(pa, ba) / max(dot(ba, ba), 1e-8));
    return length(pa - ba * h);
}

// distance (m) to the 3-5 strokes of glyph `seed` in a cell cw x ch (m) at the cell-local point lp: NocNeon's 4 x 6 lattice
float NocScrGlyph(float2 lp, float cw, float ch, uint seed)
{
    float padX = 0.16 * cw, padY = 0.10 * ch;
    float2 lat = float2((cw - 2.0 * padX) / 3.0, (ch - 2.0 * padY) / 5.0);
    uint count = 3u + seed % 3u;
    float d = 1e6;
    [unroll] for (uint k = 0u; k < 5u; k++)
    {
        uint hs = NocPcg3d(uint3(seed, k + 17u, 0x53435247u)).y;
        int a0 = (int)(hs & 3u), b0 = (int)((hs >> 2) % 6u);
        uint dirK = (hs >> 5) & 3u;
        int len = 1 + (int)((hs >> 7) % 3u);
        int2 dv = dirK == 0u ? int2(1, 0) : (dirK == 1u ? int2(0, 1) : (dirK == 2u ? int2(1, 1) : int2(1, -1)));
        int2 e = clamp(int2(a0, b0) + dv * len, int2(0, 0), int2(3, 5));
        float2 pa = float2(padX, padY) + float2(a0, b0) * lat, pb = float2(padX, padY) + float2(e) * lat;
        float dk = NocScrCapsule(lp, pa, pb);
        d = k < count ? min(d, dk) : d;
    }
    return d;
}

// the display of a wall point: id, local position (m from its lower left corner) and size (m); columns of two panels, heights
// of 3..7 panels with a per-column offset, so every display edge lies on a seam
void NocScrDisplay(float zl, float y, out uint id, out float2 lp, out float2 sz)
{
    float c = floor(zl / SCR_DW);
    uint hc = NocPcg3d(uint3((uint)(int)(c + 64.0), 0x44495350u, 7u)).x;
    uint nhu = 3u + hc % 5u;
    float nh = (float)nhu;
    float off = (float)((hc >> 4) % nhu);
    float rowf = (y / SCR_PY + off) / nh;
    float r = floor(rowf);
    id = NocPcg3d(uint3((uint)(int)(c + 64.0), (uint)(int)(r + 4096.0), 0x53435244u)).x;
    sz = float2(SCR_DW, nh * SCR_PY);
    lp = float2(zl - c * SCR_DW, (rowf - r) * sz.y);
}

// display period (s): 40, 50, 60 or 75 (each divides 600, so the 600 s clock wraps on an epoch boundary) and its phase
float NocScrPeriod(uint id) { uint k = (id >> 3) % 4u; return k == 0u ? 40.0 : (k == 1u ? 50.0 : (k == 2u ? 60.0 : 75.0)); }

// programme of display id in epoch e: 0 wave 14 %, 1 glyph 18 %, 2 aurora 10 %, 3 sun 16 %, 4 glyph column 10 %, 5 horizon 16 %,
// 6 tiles 8 %, 7 standby 8 %
uint NocScrProgramOf(uint id, uint e)
{
    float u = NocU01(NocPcg3d(uint3(id, e, 0x50524F47u)).x);
    return u < 0.14 ? 0u : (u < 0.32 ? 1u : (u < 0.42 ? 2u : (u < 0.58 ? 3u : (u < 0.68 ? 4u : (u < 0.84 ? 5u : (u < 0.92 ? 6u : 7u))))));
}

// display level of epoch e: 0.95 (25 %), 1.2 (50 %), 1.5 (25 %)
float NocScrLevel(uint id, uint e)
{
    float u = NocU01(NocPcg3d(uint3(id, e, 0x4C45564Cu)).x);
    return u < 0.25 ? 0.95 : (u < 0.75 ? 1.2 : 1.5);
}

// one programme: (m, a) at display-local lp (m) of a display sz, programme time tau (s since its epoch began; negative while it
// fades in), soft = edge softness (m), panel = the panel's lattice index (tiles)
float2 NocScrProgram(uint p, uint seed, float2 lp, float2 sz, float tau, float soft, int2 panel)
{
    float u1 = NocU01(NocPcg(seed ^ 0x55310000u)), u2 = NocU01(NocPcg(seed ^ 0x55320000u)), u3 = NocU01(NocPcg(seed ^ 0x55330000u));
    float m = 1.15, a = 0.0;
    [branch] if (p == 1u)
    {
        // one large glyph on a dark field with a soft glow, rising 0.12 m/s; the field darkens toward the top and bottom
        float cw = 0.62 * sz.x, ch = min(0.80 * sz.y, cw * (18.0 / 14.0));
        float2 o = 0.5 * (sz - float2(cw, ch)) + float2(0.0, 0.12 * (tau - 30.0));
        float d = NocScrGlyph(lp - o, cw, ch, seed);
        float r = 0.05 * cw, e = max(soft, 0.6);
        float core = 1.0 - smoothstep(r - e, r + e, d);
        float glow = exp(-(d * d) / max(9.0 * r * r, 1e-4));
        float vg = 1.0 - 0.35 * smoothstep(0.2, 0.75, abs(lp.y / sz.y - 0.5) * 2.0);
        m = (0.55 + 0.75 * glow) * vg + 1.7 * core;
        a = core;
    }
    else [branch] if (p == 2u)
    {
        // aurora: two soft band systems, 30-60 m, rising 0.4-0.6 m/s (<= 0.02 Hz)
        float lam = 30.0 + 15.0 * floor(3.0 * u1), v = 0.4 + 0.2 * u2;
        float b = 0.5 + 0.5 * sin(SCR_TAU * ((lp.y + (u3 - 0.5) * 0.4 * lp.x - v * tau) / lam));
        float b2 = 0.5 + 0.5 * sin(SCR_TAU * ((0.7 * lp.y - (u3 - 0.5) * 0.6 * lp.x + 0.7 * v * tau) / (0.62 * lam)));
        m = 0.45 + 1.1 * b * b + 0.5 * b2 * b;
        a = b * b2;
    }
    else [branch] if (p == 3u)
    {
        // sun: a large luminous disc with a halo on a dark vertical gradient, drifting 0.08 m/s
        float2 c = sz * float2(0.3 + 0.4 * u1, 0.3 + 0.4 * u2) + float2(0.08 * (tau - 30.0) * (u3 < 0.5 ? 1.0 : -1.0), 0.0);
        float R = 0.30 * min(sz.x, sz.y), rho = length(lp - c), e = 1.5 + soft;
        float sun = 1.0 - smoothstep(R - e, R + e, rho);
        float halo = exp(-(rho * rho) / (3.24 * R * R));
        m = 0.45 + 0.25 * lp.y / sz.y + 0.9 * halo + 1.0 * sun;
        a = sun;
    }
    else [branch] if (p == 4u)
    {
        // a column of glyph cells on a dark strip scrolling up at 0.15 m/s (strokes 2.6 m with 2 m soft edges: <= 0.06 Hz)
        float cw = 0.24 * sz.x, ch = cw * (18.0 / 14.0);
        float x0 = sz.x * (0.25 + 0.5 * u1) - 0.5 * cw;
        float ys = lp.y - SCR_COLV * tau;
        float j = floor(ys / ch);
        float2 q = float2(lp.x - x0, ys - j * ch);
        float d = NocScrGlyph(q, cw, ch, NocPcg3d(uint3(seed, (uint)(int)(j + 4096.0), 0x434F4C4Du)).x);
        float r = 0.075 * cw, e = max(soft, 1.0);
        float core = 1.0 - smoothstep(r - e, r + e, d);
        float inS = (1.0 - smoothstep(-1.0, 1.0, -q.x)) * (1.0 - smoothstep(cw - 1.0, cw + 1.0, q.x));
        m = lerp(1.25, 0.35 + 2.0 * core, inS);
        a = core * inS;
    }
    else [branch] if (p == 5u)
    {
        // horizon: two tones split by a line drifting 0.10 m/s, the line glowing
        float ang = (u1 - 0.5) * 0.5;
        float hy = sz.y * (0.3 + 0.4 * u2) + 0.10 * (tau - 30.0) * (u3 < 0.5 ? 1.0 : -1.0);
        float dd = (lp.y - hy) * cos(ang) - (lp.x - 0.5 * sz.x) * sin(ang);
        float up = smoothstep(-2.0 - soft, 2.0 + soft, dd);
        float ln = exp(-(dd * dd) / (3.0 + soft * soft));
        m = 0.5 + 0.9 * up + 1.0 * ln;
        a = saturate(up + 0.5 * ln);
    }
    else [branch] if (p == 6u)
    {
        // tiles: every panel of the display at its own breathing level, periods 30-60 s (<= 0.034 Hz)
        uint ht = NocPcg3d(uint3((uint)(panel.x + 4096), (uint)(panel.y + 4096), seed)).x;
        float tt = 30.0 + 30.0 * NocU01(ht), ph = NocU01(NocPcg(ht));
        float b = 0.5 + 0.5 * sin(SCR_TAU * (tau / tt + ph));
        m = 0.45 + 1.4 * b;
        a = 0.6 * b;
    }
    else [branch] if (p == 7u)
    {
        m = 0.30;                                   // standby
    }
    return float2(m, a);
}

// the imagery at wall point (zl, y): (m, a) of the display's programme x its level (capped at SCR_MMAX), crossfaded over the
// last 12 s of each epoch
float2 NocScreenContent(float zl, float y, float soft, int2 panel)
{
    uint id; float2 lp, sz;
    NocScrDisplay(zl, y, id, lp, sz);
    float T = NocScrPeriod(id), N = 600.0 / T;
    float tau = _NocScreenT.z + NocU01(NocPcg(id ^ 0x50484153u)) * T;
    float ef = floor(tau / T), x = tau / T - ef;
    uint e0 = (uint)fmod(ef, N), e1 = (uint)fmod(ef + 1.0, N);
    float w = smoothstep(1.0 - SCR_FADE / T, 1.0, x);
    uint s0 = NocPcg3d(uint3(id, e0, 0x53454544u)).x, s1 = NocPcg3d(uint3(id, e1, 0x53454544u)).x;
    float2 r0 = NocScrProgram(NocScrProgramOf(id, e0), s0, lp, sz, x * T, soft, panel);
    r0.x = min(r0.x * NocScrLevel(id, e0), SCR_MMAX);
    float2 r1 = r0;
    [branch] if (w > 0.0)
    {
        r1 = NocScrProgram(NocScrProgramOf(id, e1), s1, lp, sz, (x - 1.0) * T, soft, panel);
        r1.x = min(r1.x * NocScrLevel(id, e1), SCR_MMAX);
    }
    return lerp(r0, r1, w);
}

// ------------------------------------------------------------------------------------------------ hardware
struct NocScreenOut
{
    float3 emission;
    float metal;       // coverage of the bezels and gaps (albedo / smoothness of the cabinet frames)
};

// The emission of the face at world (z, y). fw = per-axis footprint (m / px of z and y), cosV = dot(n, toEye), gain = wall
// emission x level (14.5.6: 0.45 x 0.55), breath = the D-010 panel breath amplitude, content = imagery strength (1 full, 0 waves
// only: the Simplified tier), waves 1 or 3.
NocScreenOut NocScreenShade(float z, float y, float2 fw, float cosV, float gain, float breathAmp, float content, float waves)
{
    NocScreenOut o;
    float zl = z - SCR_Z0;
    float fmax = max(fw.x, fw.y);
    int iz = (int)floor(zl / SCR_PZ), iy = (int)floor(y / SCR_PY);
    float2 pp = float2(zl - iz * SCR_PZ, y - iy * SCR_PY) - SCR_GAP;           // panel-local metres from the active area's corner

    // seams and bezels (box-filtered per axis): the LED field covers 1 - bezel; the gap scatters 10 % of the light
    float2 sd = (frac(float2(zl / SCR_PZ, y / SCR_PY) + 0.5) - 0.5) * float2(SCR_PZ, SCR_PY);
    float bz = NocCover(sd.x, fw.x, -SCR_BEZ, SCR_BEZ), by = NocCover(sd.y, fw.y, -SCR_BEZ, SCR_BEZ);
    float gz = NocCover(sd.x, fw.x, -SCR_GAP, SCR_GAP), gy = NocCover(sd.y, fw.y, -SCR_GAP, SCR_GAP);
    float seam = ((1.0 - bz) + 0.10 * gz) * ((1.0 - by) + 0.10 * gy);
    o.metal = 1.0 - (1.0 - bz) * (1.0 - by);

    // LED pixels (0.25 m, a 0.66 dot over a 15 % diffuser floor), faded in from 5 to 10 px of pitch with the energy kept
    float wp = smoothstep(5.0, 10.0, SCR_LED / fmax);
    float2 cell = pp - floor(pp / SCR_LED) * SCR_LED;
    float d0 = 0.5 * (1.0 - SCR_DOT) * SCR_LED, d1 = 0.5 * (1.0 + SCR_DOT) * SCR_LED;
    float dotc = NocCover(cell.x, fw.x, d0, d1) * NocCover(cell.y, fw.y, d0, d1);
    float led = lerp(1.0, 0.15 + 0.85 * dotc / (SCR_DOT * SCR_DOT), wp);
    // the imagery is sampled at the LED centre while the pixels are resolved
    float2 qc = (floor(pp / SCR_LED) + 0.5) * SCR_LED - pp;
    float zc = z + qc.x * wp, yc = y + qc.y * wp;

    // 4 m modules: 5 cm joints (filtered grid, mean below 5.5 px) and calibration steps faded below 5.5 px of module pitch
    float w4 = smoothstep(2.5, 5.5, SCR_MOD / fmax);
    float jz = NocFilteredGrid1(pp.x, SCR_MOD, SCR_JOINT, fw.x, 5.5), jy = NocFilteredGrid1(pp.y, SCR_MOD, SCR_JOINT, fw.y, 5.5);
    float joint = 1.0 - 0.85 * (1.0 - (1.0 - jz) * (1.0 - jy));

    // per panel: brightness, white point, age, failures (static hashes)
    uint3 hp = NocPcg3d(uint3((uint)(iz + 4096), (uint)(iy + 4096), 0x50414E4Cu));
    float u1 = NocU01(hp.x), u2 = NocU01(hp.y), u3 = NocU01(hp.z);
    float u4 = NocU01(NocPcg(hp.x ^ 0x41474544u)), u5 = NocU01(NocPcg(hp.y ^ 0x44454144u));
    float aged = u4 < 0.18 ? 1.0 : 0.0;
    float kb = (0.92 + 0.12 * u1) * lerp(1.0, 0.86, aged);
    float3 tint = float3(1.0 + 0.10 * (u2 - 0.5), 1.0 + 0.02 * (u3 - 0.5), 1.0 - 0.10 * (u2 - 0.5)) * lerp(float3(1, 1, 1), float3(1.04, 1.0, 0.86), aged);
    int2 mi = (int2)floor(max(pp, 0.0) / SCR_MOD);
    uint hm = NocPcg3d(uint3(hp.x, (uint)(mi.x + 16), (uint)(mi.y + 16))).x;
    float km = 1.0 + lerp(0.06, 0.16, aged) * (NocU01(hm) - 0.5);
    float deadMod = (u5 >= 0.012 && u5 < 0.030 && NocU01(NocPcg(hm)) < 0.08) ? 1.0 : 0.0;
    float deadMean = (u5 >= 0.012 && u5 < 0.030) ? 1.0 - 0.08 * 0.96 : 1.0;
    km = lerp(deadMean, km * lerp(1.0, 0.04, deadMod), w4);
    float mottle = 1.0 + aged * 0.10 * (NocValueNoise(float2(zl, y) / 6.0) - 0.5) * w4;
    float deadPanel = u5 < 0.012 ? 0.04 : 1.0;
    float stuck = (u5 >= 0.030 && u5 < 0.040 && mi.x == (int)(hp.z % 9u)) ? w4 : 0.0;

    // colour: the wave, the imagery, the D-010 breath
    float h = NocScreenIndex(zc, yc, waves);
    float3 c1 = NocScreenRamp(h);
    float3 c2 = lerp(NocScreenRamp(h + 0.25), NocScreenIce(), 0.35);
    float2 ma = float2(1.0, 0.0);
    [branch] if (content > 0.001) ma = NocScreenContent(zc - SCR_Z0, yc, max(fmax, 0.5), int2(iz, iy));
    float3 col = lerp(c1, c2, ma.y * content) * lerp(1.0, ma.x, content);
    float3 stuckCol = NocScreenRamp((float)(hp.z % 4u) * 0.25 + 0.125) * 0.55;
    col = lerp(col, stuckCol, stuck);

    uint hb = NocPcg3d(uint3((uint)(iz + 4096), (uint)(iy + 4096), 0x5343524Eu)).x;
    float n = 30.0 + floor(31.0 * NocU01(hb));
    float ph = NocU01(NocPcg(hb));
    float breath = sin(SCR_TAU * frac(n * _NocScreenT.z / 600.0 + ph));
    breath *= smoothstep(2.0, 6.0, min(SCR_PZ / fw.x, SCR_PY / fw.y));
    float level = 1.0 + breathAmp * breath;

    // emissive depth: the LED view-angle falloff (cos^0.4, 0.70 at grazing) and its slight cool shift
    float sc = pow(saturate(cosV) + 1e-4, 0.4);
    float3 ang = lerp(float3(0.67, 0.71, 0.74), float3(1.0, 1.0, 1.0), sc);

    o.emission = col * (gain * level * kb * km * mottle * deadPanel * led * joint * seam) * tint * ang;
    return o;
}

#endif
