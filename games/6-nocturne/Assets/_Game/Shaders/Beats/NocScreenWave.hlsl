// NocScreenWave.hlsl - the wall of signs as a 2 km LED screen facade (MASTER_PROMPT 14.5.6, 14.2.5 NocScreen, D-010; SG, Phase 4
// wave A, INT-02 / CRIT0 #14). Included by Shaders/Beats/NocScreen.shader (the face x 874) and, for the H4 wall plane of kind 2,
// by NocMirror.hlsl (FX), so the wet promenade mirrors the same colour wave. C# twin: Beats/Beat06_Signs/NocScreenMath.cs.
//
// Layers (all in metres on the face: zl = z - 275, y):
//   1. the colour wave of 14.5.6 (phi1..phi3, index h) on the Look v2 ramp (teal, ice, amber -> ice over s 1 230-1 290, cyan;
//      desaturated 35 %, mixed 15 % toward ScreenBase, luminance-normalised k 0.8 to a geometric mean of 0.27);
//   2. imagery (v3, SG fifth start): the wall is a mosaic of displays (columns of two panels, 72.8 m, display heights of 3..7
//      panels aligned to the seams), each running a slow programme that changes every 40-75 s with a 16 s crossfade: the bare
//      wave, a large abstract glyph on a dark vignetted field, liquid (a domain-warped flow rising 0.07 m/s), a sun with slow
//      rings, a scrolling column of glyph cells, an abstract skyline with lit window cells, a matrix of glyph cells on plates,
//      bokeh discs, scan lines over a gradient and standby (dark). Each epoch draws a display level (0.9 / 1.1 / 1.35) and a hue
//      offset on the ramp (a quarter turn +-0.04: displays differ in colour, crossfaded the short way round). Each returns a
//      luminance factor m (x 1.85, D-036 (6): its mean x the LED hardware's mean is 1.56, the LOOK_V2 KS16 wall of 60-85 codes
//      (SG r03: 51.5 at 0.97; r07: 61.4 at 1.70); m capped at 4.5 / the LED dot's peak gain, so a lit dot stays below the bloom threshold) and an accent weight a (the ramp one stop on,
//      saturated x 1.3). Sub-pixel structure (window cells, scan lines, ridges, rings, bokeh) fades to its mean before it can
//      alias. No text, no people; every temporal frequency <= 0.05 Hz; wall-pixel luminance rate <= 0.04 /s (Beat06SignsTests);
//   3. hardware: panels 36 x 20 m on a 36.4 x 20.4 m pitch (0.4 m gap + 0.12 m bezels, box-filtered), per-panel brightness,
//      white point and age (18 % aged: dimmer, yellowed, mottled), 1.2 % dead panels, dead modules (8 % of the modules of 1.8 %
//      of the panels) and a stuck module column on 1 % (static: nothing flashes, nothing pops); 4 m modules with calibration
//      steps and 3 cm joints (40 % dark) faded to their mean below 5.5 px of module pitch; 0.25 m LED pixels (a 0.66 dot over a
//      35 % diffuser floor, energy kept) faded in from 10 to 18 px of pitch, and while they are resolved the imagery is sampled at
//      the pixel centres (the staircase of a real LED screen near the walker); LED binning +-3 % per pixel from 3 px of pitch;
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
#define SCR_JOINT 0.03
#define SCR_JDARK 0.40       // joint darkening
#define SCR_LED 0.25
#define SCR_DOT 0.66
#define SCR_DW 72.8
#define SCR_FADE 16.0        // programme crossfade (s)
#define SCR_COLV 0.15        // glyph-column scroll speed (m/s)
#define SCR_MMAX 4.5         // the cap of m where the LED dots are unresolved (NocScreenMath.ContentMaxM)
#define SCR_DOTG 0.842195    // the resolved LED dot peak gain - 1: 0.35 + 0.65 / 0.66^2 - 1 (cap SCR_MMAX / (1 + SCR_DOTG wp))
#define SCR_MNORM 1.85       // imagery normalisation: mean m x the LED hardware mean 1.56 (D-036 (6): the KS16 wall toward the middle of 60-85; 1.70 gave 61.4; NocScreenMath.ContentNorm)
#define SCR_EDGE 0.12        // the softest imagery edge (m)
#define SCR_SATK 1.3         // accent saturation
#define SCR_TAU 6.28318530718

float NocScrLum(float3 c) { return dot(c, float3(0.2126, 0.7152, 0.0722)); }
float3 NocScrMix(float3 c) { c = lerp(c, NocScrLum(c).xxx, 0.35); return lerp(c, PAL_ScreenBase, 0.15); }
// the accent: a colour pushed away from its luminance by k (no negative channel)
float3 NocScrSat(float3 c, float k) { float y = NocScrLum(c); return max(y.xxx + (c - y.xxx) * k, 0.0); }

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

// programme of display id in epoch e: 0 wave 6 %, 1 glyph 10 %, 2 liquid 16 %, 3 sun 10 %, 4 glyph column 8 %, 5 skyline 13 %,
// 6 glyph matrix 13 %, 7 bokeh 11 %, 8 scan lines 8 %, 9 standby 5 %
uint NocScrProgramOf(uint id, uint e)
{
    float u = NocU01(NocPcg3d(uint3(id, e, 0x50524F47u)).x);
    return u < 0.06 ? 0u : (u < 0.16 ? 1u : (u < 0.32 ? 2u : (u < 0.42 ? 3u : (u < 0.50 ? 4u : (u < 0.63 ? 5u : (u < 0.76 ? 6u : (u < 0.87 ? 7u : (u < 0.95 ? 8u : 9u))))))));
}

// display level of epoch e: 0.9 (25 %), 1.1 (50 %), 1.35 (25 %)
float NocScrLevel(uint id, uint e)
{
    float u = NocU01(NocPcg3d(uint3(id, e, 0x4C45564Cu)).x);
    return u < 0.25 ? 0.9 : (u < 0.75 ? 1.1 : 1.35);
}

// hue offset of epoch e on the ramp: a quarter turn (0, 1/4, 1/2, 3/4) +- 0.04
float NocScrHue(uint id, uint e)
{
    uint h = NocPcg3d(uint3(id, e, 0x48554531u)).x;
    return (float)(h % 4u) * 0.25 + 0.08 * (NocU01(NocPcg(h)) - 0.5);
}

// value-noise fbm of 2 or 3 octaves (lacunarity 2.03), normalised to [0, 1]
float NocScrFbm(float2 p, int oct)
{
    float s = 0.0, a = 0.5, f = 1.0, n = 0.0;
    [unroll] for (int o = 0; o < 3; o++)
    {
        float k = o < oct ? 1.0 : 0.0;
        s += k * a * NocValueNoise(p * f + float2(17.3 * o, -5.1 * o));
        n += k * a; a *= 0.5; f *= 2.03;
    }
    return s / n;
}

// one programme: (m, a) at display-local lp (m) of a display sz, programme time tau (s since its epoch began; negative while it
// fades in), soft = the pixel footprint (m, >= 0.1)
float2 NocScrProgram(uint p, uint seed, float2 lp, float2 sz, float tau, float soft)
{
    float u1 = NocU01(NocPcg(seed ^ 0x55310000u)), u2 = NocU01(NocPcg(seed ^ 0x55320000u)), u3 = NocU01(NocPcg(seed ^ 0x55330000u));
    float m = 1.15, a = 0.0;
    [branch] if (p == 0u)
    {
        // the bare wave under a soft band rising through the display (40-70 m, 0.1 m/s: <= 0.0025 Hz): never a flat field (r04)
        float bb = 0.5 + 0.5 * sin(SCR_TAU * ((lp.y + (u2 - 0.5) * 0.6 * lp.x - 0.1 * tau) / (40.0 + 30.0 * u1)));
        m = 0.65 + 1.3 * bb * bb;
    }
    else [branch] if (p == 1u)
    {
        // one large glyph on a dark vignetted field, its glow breathing at 0.025 Hz
        float cw = 0.62 * sz.x, ch = min(0.80 * sz.y, cw * (18.0 / 14.0));
        float d = NocScrGlyph(lp - 0.5 * (sz - float2(cw, ch)), cw, ch, seed);
        float r = 0.05 * cw, e = max(soft, SCR_EDGE);
        float core = 1.0 - smoothstep(r - e, r + e, d);
        float glow = exp(-(d * d) / max(16.0 * r * r, 1e-4)) * (0.75 + 0.25 * sin(SCR_TAU * (tau / 40.0 + u3)));
        float vg = 1.0 - 0.5 * smoothstep(0.2, 0.8, abs(lp.y / sz.y - 0.5) * 2.0);
        float2 rq = (lp - 0.5 * sz) / sz;
        float field = 0.125 + 0.30 * exp(-dot(rq, rq) / 0.12);             // r05: 0.26 + 0.55, f02: 0.14 + 0.40 read as pale flat panels; >= 0.125 keeps m >= 0.10
        m = (field + 1.3 * glow) * vg + 3.4 * core;
        a = core;
    }
    else [branch] if (p == 2u)
    {
        // liquid: domain-warped fbm (features 18-30 m) rising 0.07 m/s, bright pools and a glowing ridge line
        float sc = 1.0 / (18.0 + 12.0 * u1);
        float2 q = float2(lp.x * sc + 31.0 * u2, (lp.y - 0.07 * tau) * sc + 57.0 * u3);
        float wx = NocScrFbm(q * 0.7 + float2(3.1, 0.0), 2) - 0.5, wy = NocScrFbm(q * 0.7 + float2(0.0, 7.7), 2) - 0.5;
        float n = NocScrFbm(q + 2.2 * float2(wx, wy), 3);
        float pool = smoothstep(0.38, 0.78, n);
        float sw = 0.6 * soft * sc;
        float s2 = 0.0018 + sw * sw;                                         // the ridge widened to the footprint, energy kept
        float ridge = exp(-((n - 0.62) * (n - 0.62)) / s2) * sqrt(0.0018 / s2);
        m = 0.26 + 2.6 * pool * pool + 1.0 * ridge;
        a = saturate(0.4 * pool * pool + 0.8 * ridge);
    }
    else [branch] if (p == 3u)
    {
        // sun: a luminous disc on a dark gradient drifting 0.08 m/s, a halo and rings rippling outward (7 m, 0.15 m/s: 0.02 Hz)
        float2 c = float2(sz.x * (0.3 + 0.4 * u1) + 0.08 * (tau - 30.0) * (u3 < 0.5 ? 1.0 : -1.0), sz.y * (0.3 + 0.4 * u2));
        float R = 0.26 * min(sz.x, sz.y), rho = length(lp - c), e = 1.0 + soft;
        float sun = 1.0 - smoothstep(R - e, R + e, rho);
        float halo = exp(-(rho * rho) / (2.6 * R * R));
        float lam = 7.0, dr = abs(frac((rho - 0.15 * tau) / lam) - 0.5) * lam;
        float rr = (rho - R) / (1.6 * R);
        float ring = (1.0 - smoothstep(0.3, 1.0 + soft, dr)) * smoothstep(R, R + 2.0 + soft, rho) * exp(-rr * rr);
        ring *= smoothstep(1.0, 3.0, lam / max(soft, 1e-3));
        m = 0.18 + 0.30 * lp.y / sz.y + 1.1 * halo + 2.6 * sun + 0.9 * ring;
        a = saturate(sun + 0.5 * ring);
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
        m = lerp(1.05, 0.20 + 3.0 * core, inS);
        a = core * inS;
    }
    else [branch] if (p == 5u)
    {
        // skyline: an abstract city on a glowing sky; its window cells (1.2 m, 32 % lit, each fading over 40-80 s) fade to their
        // mean below 2-4 px of cell pitch
        float hz = sz.y * (0.10 + 0.10 * u1), bw = 3.0 + 3.0 * u2;
        float bi = floor(lp.x / bw);
        float hb = NocU01(NocPcg3d(uint3((uint)(int)(bi + 4096.0), seed, 0x534B594Cu)).x);
        float top = hz + sz.y * (0.08 + 0.40 * hb * hb);
        float inB = 1.0 - smoothstep(top - 0.3 - soft, top + 0.3 + soft, lp.y);
        float2 wij = floor(lp / 1.2);
        uint hw = NocPcg3d(uint3((uint)(int)(wij.x + 4096.0), (uint)(int)(wij.y + 4096.0), seed)).x;
        float lit = NocU01(hw) < 0.32 ? 1.0 : 0.0;
        float tw = 40.0 + 40.0 * NocU01(NocPcg(hw ^ 0x77u));
        lit *= 0.6 + 0.4 * sin(SCR_TAU * (tau / tw + NocU01(NocPcg(hw))));
        float2 cc = lp - (wij + 0.5) * 1.2;
        float win = NocCover(cc.x, soft, -0.336, 0.336) * NocCover(cc.y, soft, -0.264, 0.264) * lit;
        win = lerp(0.0473, win, smoothstep(2.0, 4.0, 1.2 / soft));        // mean: 0.246 of the cell x 32 % x 0.6
        float sky = 0.35 + 2.4 * exp(-max(lp.y - hz, 0.0) / (0.45 * sz.y)) * smoothstep(-2.0, 6.0, lp.y - 0.5 * hz);
        m = sky * (1.0 - inB) + inB * (0.10 + 2.2 * win);
        a = (1.0 - inB) * 0.12 + inB * win;
    }
    else [branch] if (p == 6u)
    {
        // a matrix of glyph cells (6, 8 or 10 across) on plates of their own brightness; 80 % carry a glyph that brightens and
        // dims over 36-72 s
        float k = u1 < (1.0 / 3.0) ? 6.0 : (u1 < (2.0 / 3.0) ? 8.0 : 10.0);
        float2 cs = float2(sz.x / k, sz.x / k * (18.0 / 14.0));
        float2 ci = floor(lp / cs), q = lp - ci * cs;
        uint hc = NocPcg3d(uint3((uint)(int)(ci.x + 64.0), (uint)(int)(ci.y + 4096.0), seed)).x;
        float d = NocScrGlyph(q, cs.x, cs.y, hc);
        float r = 0.07 * cs.x, e = max(soft, SCR_EDGE);
        float core = 1.0 - smoothstep(r - e, r + e, d);
        float tt = 36.0 + 36.0 * NocU01(NocPcg(hc)), ph = NocU01(NocPcg(hc ^ 0x9E37u));
        float b = 0.5 + 0.5 * sin(SCR_TAU * (tau / tt + ph));
        b *= b;
        float on = NocU01(NocPcg(hc ^ 0x51u)) < 0.8 ? 1.0 : 0.0;
        float gut = NocCover(q.x, soft, 0.18, cs.x - 0.18) * NocCover(q.y, soft, 0.18, cs.y - 0.18);
        float plate = (0.35 + 0.95 * NocU01(NocPcg(hc ^ 0x504Cu))) * (0.8 + 0.2 * sin(SCR_TAU * (tau / (2.0 * tt) + ph))) * (1.0 - 0.35 * q.y / cs.y);
        m = 0.16 + gut * (plate + core * on * (0.6 + 3.0 * b));
        a = gut * core * on * b;
    }
    else [branch] if (p == 7u)
    {
        // bokeh: two layers of static soft discs (16 m and 10 m cells), each fading on its own 20-40 s phase; a disc becomes its
        // cell's mean once its edge is under a pixel
        m = 0.30 + 0.45 * lp.y / sz.y;
        [unroll] for (uint layer = 0u; layer < 2u; layer++)
        {
            float g = 16.0 - 6.0 * layer;
            float2 gi = floor(lp / g);
            uint3 hb = NocPcg3d(uint3((uint)(int)(gi.x + 4096.0), (uint)(int)(gi.y + 4096.0), seed + layer));
            float2 c = (gi + 0.38 + 0.24 * float2(NocU01(hb.x), NocU01(hb.y))) * g;
            float rr = g * (0.18 + 0.12 * NocU01(hb.z)), dd = length(lp - c), ew = 0.05 * g;
            float disc = 1.0 - smoothstep(rr - ew, rr + ew, dd);
            float rim = exp(-((dd - rr) * (dd - rr)) / (0.0016 * g * g)) * 0.35;
            float fd = smoothstep(0.02 * g, 0.06 * g, soft);
            disc = lerp(disc, 3.14159265 * rr * rr / (g * g), fd);
            rim *= 1.0 - fd;
            float tt = 20.0 + 20.0 * NocU01(NocPcg(hb.y ^ 0x77u));
            float br = (0.8 + 1.6 * NocU01(NocPcg(hb.z))) * (NocU01(NocPcg(hb.x ^ 5u)) < 0.75 ? 1.0 : 0.0) * (0.55 + 0.45 * sin(SCR_TAU * (tau / tt + NocU01(hb.z))));
            m += br * (disc * (0.7 + 0.3 * NocU01(NocPcg(hb.y))) + rim) * (0.9 - 0.3 * layer);
            a = max(a, disc * (NocU01(NocPcg(hb.y ^ 3u)) < 0.4 ? 1.0 : 0.0));
        }
    }
    else [branch] if (p == 8u)
    {
        // scan lines: thin horizontal lines (2-4 m apart, 0.4-0.9 m) across the display over a dark vertical gradient, a slow
        // brightening rising through them (0.028 Hz); box-filtered, faded to their mean below 3-6 px of pitch (r05: a pale
        // gradient with lines on part of the width read as a flat light panel)
        float sp = 2.0 + 2.0 * u1;
        float li = floor(lp.y / sp);
        float wl = 0.2 + 0.25 * NocU01(NocPcg3d(uint3((uint)(int)(li + 4096.0), seed, 0x4C494E45u)).x);
        float ln = NocCover(lp.y - (li + 0.5) * sp, soft, -wl, wl);
        ln = lerp(0.65 / sp, ln, smoothstep(3.0, 6.0, sp / soft));
        float b = 0.5 + 0.5 * sin(SCR_TAU * (li * 0.07 - tau / 36.0));
        b *= b;
        float x0 = sz.x * (0.04 + 0.12 * u2), x1 = sz.x * (0.84 + 0.12 * u3), ew = 0.3 + soft;
        float span = smoothstep(x0 - ew, x0 + ew, lp.x) * (1.0 - smoothstep(x1 - ew, x1 + ew, lp.x));
        m = 0.15 + 0.45 * lp.y / sz.y + ln * span * (1.2 + 2.6 * b);
        a = ln * span * b;
    }
    else [branch] if (p == 9u)
    {
        m = 0.20;                                   // standby
    }
    return float2(m, a);
}

// the imagery at wall point (zl, y): (m, a) of the display's programme x its level x SCR_MNORM (capped at SCR_MMAX / (1 + SCR_DOTG
// wp): a resolved LED dot never passes the bloom threshold), crossfaded over the last 16 s of each epoch; dh = the hue offset
float2 NocScreenContent(float zl, float y, float soft, float wp, out float dh)
{
    uint id; float2 lp, sz;
    NocScrDisplay(zl, y, id, lp, sz);
    float T = NocScrPeriod(id), N = 600.0 / T;
    float tau = _NocScreenT.z + NocU01(NocPcg(id ^ 0x50484153u)) * T;
    float ef = floor(tau / T), x = tau / T - ef;
    uint e0 = (uint)fmod(ef, N), e1 = (uint)fmod(ef + 1.0, N);
    float w = smoothstep(1.0 - SCR_FADE / T, 1.0, x);
    float cap = SCR_MMAX / (1.0 + SCR_DOTG * wp);
    uint s0 = NocPcg3d(uint3(id, e0, 0x53454544u)).x, s1 = NocPcg3d(uint3(id, e1, 0x53454544u)).x;
    float2 r0 = NocScrProgram(NocScrProgramOf(id, e0), s0, lp, sz, x * T, soft);
    r0.x = min(r0.x * NocScrLevel(id, e0) * SCR_MNORM, cap);
    float2 r1 = r0;
    [branch] if (w > 0.0)
    {
        r1 = NocScrProgram(NocScrProgramOf(id, e1), s1, lp, sz, (x - 1.0) * T, soft);
        r1.x = min(r1.x * NocScrLevel(id, e1) * SCR_MNORM, cap);
    }
    float d0 = NocScrHue(id, e0), dd = NocScrHue(id, e1) - d0;
    dh = d0 + (dd - round(dd)) * w;                 // the short way round the ramp
    return lerp(r0, r1, w);
}

// ------------------------------------------------------------------------------------------------ the eye (D-034 5.3, fix wave C28)
// One enormous soft-edged eye on a canvas of 4 x 5 panels (z 311.4 - 457.0, y 20.4 - 122.4, 146 x 102 m: a panel reads as a pixel),
// once per 600 s of the clock, inside KS16's readable window (s 1 134 - 1 196 = t 453.6 - 478.4 at 2.5 m/s; the KS16 still is
// t 460, its 60 s sequence t 460 - 520): the canvas fades in over t 448 - 454 (the closed lid a faint glowing line), the eye opens
// over 454 - 460, holds to 472, closes over 472 - 478, the canvas fades out over 478 - 484. Lids <= 8.5 m/s over 20 m soft edges
// and a static skin fold (a wall point changes <= 0.1 /s, T-WAVE 0.12; no flash in the model), no text, no people. C# twin:
// NocScreenMath.Eye.
#define SCR_EYE_Z0 311.4
#define SCR_EYE_Z1 457.0
#define SCR_EYE_Y0 20.4
#define SCR_EYE_Y1 122.4
#define SCR_EYE_CZ 384.2
#define SCR_EYE_CY 71.4

// canvas weight and lid opening at the wrapped clock t (s)
float NocScrEyeEnv(float t, out float open)
{
    open = NocS01((t - 454.0) / 6.0) * (1.0 - NocS01((t - 472.0) / 6.0));
    return NocS01((t - 448.0) / 6.0) * (1.0 - NocS01((t - 478.0) / 6.0));
}

// the eye canvas at world (z, y): rgb = colour x m in the mosaic's units (x level 1.1 x SCR_MNORM, capped at cap), a = weight
float4 NocScrEye(float z, float y, float soft, float cap)
{
    float open;
    float env = NocScrEyeEnv(_NocScreenT.z, open);
    float w = env * NocCover(z, soft, SCR_EYE_Z0, SCR_EYE_Z1) * NocCover(y, soft, SCR_EYE_Y0, SCR_EYE_Y1);
    float4 r = float4(0.0, 0.0, 0.0, 0.0);
    [branch] if (w > 0.0)
    {
    float2 p = float2(z - SCR_EYE_CZ, y - SCR_EYE_CY);
    float u = saturate(abs(p.x) / 64.0);
    float bow = pow(max(1.0 - u * u, 0.0), 0.85);
    float e = max(10.0, soft);
    float yu = 34.0 * open * bow + 0.8, yl = -0.72 * 34.0 * open * bow - 0.8;
    float eyeA = smoothstep(yl - e, yl + e, p.y) * (1.0 - smoothstep(yu - e, yu + e, p.y)) * (1.0 - smoothstep(64.0 - 2.0 * e, 64.0, abs(p.x)));
    // iris (r 24 m) and pupil (r 9.5 m) looking down toward the promenade; fibres fade to their mean below 2-4 px of spacing
    float2 q = p - float2(-9.0, -4.0);
    float rr = length(q);
    float irisA = 1.0 - smoothstep(21.0, 27.0 + soft, rr);
    float pupilA = 1.0 - smoothstep(7.0, 12.0 + soft, rr);
    float ang = atan2(q.y, q.x);
    float fib = 0.5 + 0.3 * cos(40.0 * ang + 5.0 * sin(3.0 * ang) + 0.35 * rr) + 0.2 * cos(73.0 * ang + 3.0 * cos(5.0 * ang) - 0.21 * rr);
    fib = lerp(0.5, fib, smoothstep(2.0, 4.0, SCR_TAU * rr / 73.0 / max(soft, 0.1)));
    float2 qc = q - float2(-6.0, 8.0);
    float cl = exp(-dot(qc, qc) / 20.0);
    float ms = 1.6 * (1.0 - 0.45 * u * u) * (1.0 - 0.35 * smoothstep(yu - 16.0, yu, p.y));
    float mi = 1.25 * (0.6 + 0.8 * fib) * (0.55 + 0.45 * smoothstep(9.5, 24.0, rr)) * (1.0 - 0.55 * smoothstep(18.0, 24.0, rr));
    float yc = p.y - 34.0 * bow - 10.0;                                       // the skin fold above the open lid: static, fades in
    float crease = exp(-(yc * yc) / 18.0) * bow * open;
    float2 pg = p / float2(84.0, 50.0);
    float mf = 0.20 + 0.34 * exp(-dot(pg, pg)) + 0.45 * crease;          // the field glows softly round the eye (f01: a flat teal field)
    float k = 1.1 * SCR_MNORM;
    float3 cS = NocScreenIce(), cI = NocScrSat(NocScreenRamp(0.52), SCR_SATK), cF = NocScreenRamp(0.0);
    float3 inside = lerp(cS * min(ms * k, cap), cI * min(mi * k, cap), irisA);
    inside = lerp(inside, cS * (0.04 * k), pupilA);
    inside = lerp(inside, cS * min(1.6 * k, cap), cl);
    r.rgb = lerp(cF * min(mf * k, cap), inside, eyeA);
    r.a = w;
    }
    return r;
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

    // LED pixels (0.25 m, a 0.66 dot over a 35 % diffuser floor), faded in from 10 to 18 px of pitch with the energy kept (r03 / r04:
    // from 5 or 8 px the dot lattice read as a wire mesh over the left third of KS16)
    float wp = smoothstep(10.0, 18.0, SCR_LED / fmax);
    float2 cell = pp - floor(pp / SCR_LED) * SCR_LED;
    float d0 = 0.5 * (1.0 - SCR_DOT) * SCR_LED, d1 = 0.5 * (1.0 + SCR_DOT) * SCR_LED;
    float dotc = NocCover(cell.x, fw.x, d0, d1) * NocCover(cell.y, fw.y, d0, d1);
    float led = lerp(1.0, 0.35 + 0.65 * dotc / (SCR_DOT * SCR_DOT), wp);
    // the imagery is sampled at the LED centre while the pixels are resolved
    float2 qc = (floor(pp / SCR_LED) + 0.5) * SCR_LED - pp;
    float zc = z + qc.x * wp, yc = y + qc.y * wp;

    // 4 m modules: 3 cm joints (filtered grid, mean below 5.5 px) and calibration steps faded below 5.5 px of module pitch
    float w4 = smoothstep(2.5, 5.5, SCR_MOD / fmax);
    float jz = NocFilteredGrid1(pp.x, SCR_MOD, SCR_JOINT, fw.x, 5.5), jy = NocFilteredGrid1(pp.y, SCR_MOD, SCR_JOINT, fw.y, 5.5);
    float joint = 1.0 - SCR_JDARK * (1.0 - (1.0 - jz) * (1.0 - jy));

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
    // LED binning: every 0.25 m pixel at its own static brightness (+-3 %, zero mean), faded in from 3 to 6 px of pitch, so no
    // smooth gradient of the imagery stays free of texture near the walker (14.7 H1; SG r06: BandScore 3.0 on KS16's lower left)
    float2 lc = floor(pp / SCR_LED);
    float bin = NocU01(NocPcg3d(uint3((uint)(int)(lc.x + 65536.0), (uint)(int)(lc.y + 65536.0), hp.x)).x) - 0.5;
    float binning = 1.0 + 0.06 * bin * smoothstep(3.0, 6.0, SCR_LED / fmax);

    // colour: the wave (+ the display's hue offset), the imagery, the D-010 breath
    float2 ma = float2(1.0, 0.0);
    float dh = 0.0;
    [branch] if (content > 0.001) ma = NocScreenContent(zc - SCR_Z0, yc, max(fmax, 0.1), wp, dh);
    float h = NocScreenIndex(zc, yc, waves) + dh * content;
    float3 c1 = NocScreenRamp(h);
    float3 c2 = NocScrSat(NocScreenRamp(h + 0.25), SCR_SATK);
    float3 col = lerp(c1, c2, ma.y * content) * lerp(1.0, ma.x, content);
    // the eye (C28): its canvas replaces the mosaic for 36 s of every 600 s
    [branch] if (content > 0.001)
    {
        float4 eye = NocScrEye(zc, yc, max(fmax, 0.1), SCR_MMAX / (1.0 + SCR_DOTG * wp));
        col = lerp(col, eye.rgb, eye.a * content);
    }
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

    o.emission = col * (gain * level * kb * km * mottle * deadPanel * led * joint * seam * binning) * tint * ang;
    return o;
}

#endif
