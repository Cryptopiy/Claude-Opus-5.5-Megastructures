// NocFacadeSkin.hlsl - the Look v2 facade (-facade-v 2; docs/TEXTURE_PLAN.md section 2, docs/LOOK_V2.md section 4,
// docs/FACADE_V2.md), owner FA. The window field stays procedural and analytic (H2, D-004, D-017); on top of it:
//   - skins per tower (the skin byte, vertex colour G: region D / OQ share tables, hero overrides 0xF0 + k), never per face;
//   - strata, zone rows, lit floor groups, the tower multiplier, the core strip and the mechanical stripes keyed by the tower key
//     K = pcg(byte), so floor lines and lit floors continue around corners;
//   - LOOK_V2's window light (lit 0.18 with the strata factors, zone spread 0.40 + 1.2 U, hue classes moving with w_f / n_f,
//     brightness Y_med 0.12 x b, b = exp(0.45 N) clamped to [0.40, 2.40], stand-by x0.0005, curtains / blinds / ceiling falloff
//     as a zero-mean interior factor faded in over 4-8 px of window height);
//   - whole bays only (openings inside [uMin + 0.6, uMax - 0.6] x [y0 + base, y1 - parapet]) and the macro layer (tower
//     multiplier, lit floor groups, core strip, mechanical stripes, setback ledge line, stratum bands);
//   - exact recess rectangles (the opening intersected with its image shifted by the recess depth along the view ray in the
//     per-pixel facade frame of NocFaceFrame), reveals with their own normals, room-light spill and a rain-scatter halo, wet sills;
//   - frames and mullions as exact filtered bars at the glass plane, panel joints, per-panel tone (2 x 2 box-filtered),
//     slab ledges, parapet coping, corner trim, base band, edge lights with LOOK_V2's colours.
// Every element is an interval in cell coordinates whose coverage comes from NocCover over the 2 x 2 cells under the bounding-box
// footprint, blended per axis to its band mean below the H2 fade (smoothstep(2.5, 5.5, pitch px)); band means count the
// suppressed cells, and the parts of a footprint that fall in a neighbour band take that band's mean, so nothing moves the
// linear-light mean (E1-E7) and nothing shimmers (S1-S7). The C# twin is NocFacadeMath (v2 section): EvalV2 = NocFacadeEvalV2.
// Requires NocLitCore.hlsl (NocFrame) before it. Takes no derivatives: the caller passes the footprints (derivatives before
// every branch).
#ifndef NOC_FACADE_SKIN_INCLUDED
#define NOC_FACADE_SKIN_INCLUDED

#include "NocFacade.hlsl"
#include "../Generated/NocFacadeConstantsGen.hlsl"
#include "NocInterior.hlsl"

#define NOC_SK_S0 0
#define NOC_SK_S1 1
#define NOC_SK_S2 2
#define NOC_SK_S3 3
#define NOC_SK_S4 4
#define NOC_SK_SEC 5
#define NOC_ST_RES 0
#define NOC_ST_COM 1
#define NOC_ST_MECH 2
#define NOC_ST_LOBBY 3
#define NOC_ST_SEC 4

// ---------------------------------------------------------------------------------------------------------------- skins
struct NocSkinV2
{
    int id;           // NOC_SK_*
    int region;       // 0 D, 1 OQ
    int overrideK;    // -1 regional skin, else the hero override k
    uint key;         // tower key K = pcg(byte)
    bool s1Mullion, s2Ribbed, core, ledge, fins, led, stoneBase;
};

uint NocH3x(uint a, uint b, uint c) { return NocPcg3d(uint3(a, b, c)).x; }
uint NocH3y(uint a, uint b, uint c) { return NocPcg3d(uint3(a, b, c)).y; }

NocSkinV2 NocSkinOfV2(uint b, int mode)
{
    NocSkinV2 s;
    b = b & 0xFFu;
    s.key = NocPcg(b);
    uint K = s.key;
    s.overrideK = -1;
    s.region = (b >= (uint)NC_FaOqBase && b < (uint)NC_FaOverrideBase) ? 1 : 0;
    s.id = NOC_SK_S1;
    s.s1Mullion = false; s.s2Ribbed = false; s.core = false; s.ledge = false; s.fins = false; s.led = false; s.stoneBase = false;
    if (mode == 1) { s.id = NOC_SK_SEC; return s; }
    if (b >= (uint)NC_FaOverrideBase)
    {
        int k = (int)(b - (uint)NC_FaOverrideBase);
        s.overrideK = k;
        if (k == 0)      { s.id = NOC_SK_S0; s.fins = true; }
        else if (k == 1) { s.id = NOC_SK_S2; s.led = true; }
        else if (k == 3) { s.id = NOC_SK_S4; }
        else if (k == 4) { s.id = NOC_SK_S1; s.stoneBase = true; }
        else             { s.id = NOC_SK_S1; }
    }
    else
    {
        float us = NocU01(NocPcg(K ^ 0x534B4E31u));
        bool q = s.region == 1;
        float t0 = q ? NC_FaShareQ0 : NC_FaShareD0, t1 = q ? NC_FaShareQ1 : NC_FaShareD1;
        float t2 = q ? NC_FaShareQ2 : NC_FaShareD2, t3 = q ? NC_FaShareQ3 : NC_FaShareD3;
        s.id = us < t0 ? NOC_SK_S0 : (us < t1 ? NOC_SK_S1 : (us < t2 ? NOC_SK_S2 : (us < t3 ? NOC_SK_S3 : NOC_SK_S4)));
    }
    s.s1Mullion = NocU01(NocPcg(K ^ 0x4D554C4Cu)) < NC_FaS1MullionP;
    s.s2Ribbed = NocU01(NocPcg(K ^ 0x52494253u)) < NC_FaS2RibbedP;
    s.core = NocU01(NocPcg(K ^ 0x434F5245u)) < NC_FaCoreP;
    s.ledge = NocU01(NocPcg(K ^ 0x4C444745u)) < NC_FaLedgeP;
    return s;
}

int NocStratumTypeV2(uint K, int band, int mode)
{
    if (mode == 1) return NOC_ST_SEC;
    float r = NocU01(NocH3x(K, (uint)(band + 65536), 0x53545232u));
    return r < NC_FacadeShareRes ? NOC_ST_RES : (r < NC_FacadeShareRes + NC_FacadeShareCom ? NOC_ST_COM
         : (r < NC_FacadeShareRes + NC_FacadeShareCom + NC_FacadeShareMech ? NOC_ST_MECH : NOC_ST_LOBBY));
}

float NocFactorV2(int type)
{
    return type == NOC_ST_COM ? NC_FaFactorCom : (type == NOC_ST_MECH ? NC_FaFactorMech
         : (type == NOC_ST_LOBBY ? NC_FaFactorLobby : (type == NOC_ST_SEC ? NC_FaSecLit / NC_FaLit : NC_FaFactorRes)));
}

// ---------------------------------------------------------------------------------------------------------------- cells
struct NocCellV2
{
    float Pu, Pv, bandH;
    float ox0, ox1, oy0, oy1;   // opening (frame outer edge), cell-local metres
    float ff;                   // frame inside the opening (all four sides)
    float mOff, mP, mW;         // x mullions at mOff + k mP strictly inside (ox0, ox1); mW 0 = none
    float tY, tW;               // one y transom (tW 0 = none)
    float slab;                 // slab band [0, slab)
    float sp0, sp1;             // spandrel glass band (S0), sp1 <= sp0: none
    float D;                    // recess depth
    float louvre;               // 1: louvre blades over the opening (mechanical slit)
    float gx0, gx1, gy0, gy1;   // the glass rectangle at the glass plane (the opening inside its frame; a loggia's doors)
    float loggia;               // 1: S4 loggia band (whole-bay recess, doors at the back, rails in front)
};

float NocSlabOfV2(int sk)
{
    return sk == NOC_SK_S0 ? NC_FaS0Cap : (sk == NOC_SK_S2 ? NC_FaS2Slab : (sk == NOC_SK_S3 ? NC_FaS3Slab : (sk == NOC_SK_S4 ? NC_FaS4Slab : NC_FaS1Slab)));
}

NocCellV2 NocCellOfV2(NocSkinV2 s, int type)
{
    NocCellV2 c;
    c.bandH = NC_FacadeBand; c.ff = NC_FaFrame;
    c.mOff = 0.0; c.mP = 100.0; c.mW = 0.0; c.tY = 0.0; c.tW = 0.0; c.sp0 = 0.0; c.sp1 = 0.0; c.louvre = 0.0;
    int sk = s.id;
    c.slab = NocSlabOfV2(sk);
    if (type == NOC_ST_COM)
    {
        c.Pu = NC_FacadeComP; c.Pv = NC_FacadeComH;
        c.ox0 = NC_FaComOx0; c.ox1 = NC_FaComOx1; c.oy0 = NC_FaComOy0; c.oy1 = NC_FaComOy1;
        c.ff = NC_FaComFrame; c.mOff = NC_FaComOx0; c.mP = NC_FaComMullP; c.mW = NC_FaComMull;
        c.D = (sk == NOC_SK_S1 || sk == NOC_SK_S3) ? NC_FaComDDeep : NC_FaComD;
        if (sk == NOC_SK_S0) { c.sp0 = NC_FaS0Cap; c.sp1 = c.oy0 - 0.06; }
    }
    else if (type == NOC_ST_MECH)
    {
        c.Pu = NC_FacadeMechP; c.Pv = NC_FacadeMechH;
        c.ox0 = NC_FaMechOx0; c.ox1 = NC_FaMechOx1; c.oy0 = NC_FaMechOy0; c.oy1 = NC_FaMechOy1;
        c.ff = NC_FaMechFrame; c.D = NC_FaMechD; c.louvre = 1.0;
    }
    else if (type == NOC_ST_LOBBY)
    {
        c.Pu = NC_FacadeLobbyP; c.Pv = NC_FacadeLobbyH;
        c.ox0 = NC_FaLobOx0; c.ox1 = NC_FaLobOx1; c.oy0 = NC_FaLobOy0; c.oy1 = NC_FaLobOy1;
        c.ff = NC_FaLobFrame; c.mOff = NC_FaLobOx0; c.mP = NC_FaLobMullP; c.mW = NC_FaLobMull;
        c.tY = 0.5 * (NC_FaLobOy0 + NC_FaLobOy1); c.tW = NC_FaLobMull; c.D = NC_FaLobD;
    }
    else if (type == NOC_ST_SEC)
    {
        c.Pu = NC_FaSecP; c.Pv = NC_FacadeSecH; c.bandH = NC_FacadeSecBand;
        c.ox0 = NC_FaSecOx0; c.ox1 = NC_FaSecOx1; c.oy0 = NC_FaSecOy0; c.oy1 = NC_FaSecOy1;
        c.ff = NC_FaSecFrame; c.mOff = 0.5 * NC_FaSecP; c.mP = 100.0; c.mW = NC_FaSecMull;   // one centre mullion per lit unit
        c.D = NC_FaSecD; c.slab = NC_FaSecSlab;
    }
    else
    {
        c.Pu = NC_FacadeColumn; c.Pv = NC_FacadeFloor;
        if (sk == NOC_SK_S0)
        {
            c.ox0 = NC_FaS0Ox0; c.ox1 = NC_FaS0Ox1; c.oy0 = NC_FaS0Oy0; c.oy1 = NC_FaS0Oy1;
            c.ff = 0.0; c.mOff = 0.0; c.mP = 0.5 * NC_FacadeColumn; c.mW = NC_FaS0Mull; c.D = NC_FaS0D;
            c.sp0 = NC_FaS0Cap; c.sp1 = NC_FaS0Sp1;
        }
        else if (sk == NOC_SK_S2)
        {
            c.ox0 = NC_FaS2Ox0; c.ox1 = NC_FaS2Ox1; c.oy0 = NC_FaS2Oy0; c.oy1 = NC_FaS2Oy1;
            c.ff = NC_FaS2Frame; c.mOff = 0.0; c.mP = 0.5 * NC_FacadeColumn; c.mW = NC_FaS2Mull; c.D = NC_FaS2D;
        }
        else if (sk == NOC_SK_S3)
        {
            c.ox0 = NC_FaS3Ox0; c.ox1 = NC_FaS3Ox1; c.oy0 = NC_FaS3Oy0; c.oy1 = NC_FaS3Oy1; c.ff = NC_FaS3Frame; c.D = NC_FaS3D;
        }
        else if (sk == NOC_SK_S4)
        {
            c.ox0 = NC_FaS4Ox0; c.ox1 = NC_FaS4Ox1; c.oy0 = NC_FaS4Oy0; c.oy1 = NC_FaS4Oy1; c.ff = NC_FaS4Frame; c.D = NC_FaS4D;
        }
        else
        {
            bool m = s.s1Mullion;
            c.ox0 = m ? NC_FaS1BOx0 : NC_FaS1Ox0; c.ox1 = m ? NC_FaS1BOx1 : NC_FaS1Ox1;
            c.oy0 = NC_FaS1Oy0; c.oy1 = NC_FaS1Oy1; c.D = NC_FaS1D;
            if (m) { c.mOff = 0.5 * NC_FacadeColumn; c.mP = 100.0; c.mW = NC_FaFrame; }
        }
    }
    c.gx0 = c.ox0 + c.ff; c.gx1 = c.ox1 - c.ff; c.gy0 = c.oy0 + c.ff; c.gy1 = c.oy1 - c.ff; c.loggia = 0.0;
    return c;
}

bool NocLoggiaBandV2(uint K, int band) { return NocU01(NocH3x(K, (uint)(band + 65536), 0x4C4F4747u)) < NC_FaLoggiaP; }

// The cell of a band: an S4 tower's loggia bands swap the residential window for a whole-bay loggia (TEXTURE_PLAN 2.3).
NocCellV2 NocCellBandV2(NocSkinV2 s, int type, int band)
{
    NocCellV2 c = NocCellOfV2(s, type);
    if (type == NOC_ST_RES && s.id == NOC_SK_S4 && NocLoggiaBandV2(s.key, band))
    {
        c.ox0 = NC_FaLogX0; c.ox1 = NC_FaLogX1; c.oy0 = c.slab; c.oy1 = c.Pv; c.ff = 0.0;
        c.gx0 = NC_FaLogDoorX0; c.gx1 = NC_FaLogDoorX1; c.gy0 = c.slab; c.gy1 = c.slab + NC_FaLogDoorH;
        c.mOff = 0.5 * c.Pu; c.mP = 100.0; c.mW = 0.5 * NC_FaFrame; c.D = NC_FaLoggiaD; c.loggia = 1.0;
    }
    return c;
}

bool NocKnownY(float y0, float y1) { return y1 - y0 > 2.0; }

// Whole bays (TEXTURE_PLAN 2.5): cells iF..iL have their opening inside [uMin + 0.6, uMax - 0.6]; the core strip is k0..k1.
void NocBaysV2(NocSkinV2 s, NocCellV2 c, int type, float uMin, float uMax, out int iF, out int iL, out int k0, out int k1)
{
    float a = uMin + NC_FaCornerPier, b = uMax - NC_FaCornerPier;
    iF = (int)ceil((a - c.ox0) / c.Pu);
    iL = (int)floor((b - c.ox1) / c.Pu);
    if (c.loggia > 0.5) { iF += 1; iL -= 1; }        // no loggias in the corner bays
    k0 = 0; k1 = -1;
    if (s.core && (type == NOC_ST_RES || type == NOC_ST_COM) && uMax - uMin > NC_FaCoreMinFace)
    {
        float w = NocU01(NocPcg(s.key ^ 0x43575744u)) < 0.5 ? 3.0 : 6.0;
        int n = max(1, (int)round(w / c.Pu));
        float uc = 0.5 * (uMin + uMax);
        k0 = (int)floor(uc / c.Pu - 0.5 * n + 0.5);
        k1 = k0 + n - 1;
    }
}

bool NocBayHasWindow(int i, int iF, int iL, int k0, int k1) { return i >= iF && i <= iL && (i < k0 || i > k1); }

bool NocMechStripeV2(uint K, int band) { return NocU01(NocH3y(K, (uint)(band + 65536), 0x4D454348u)) < NC_FaMechStripeP; }

// Rows with windows: [jF, jL] after the mechanical stripe (top two rows of a keyed residential band), the base band and the parapet.
void NocRowsV2(NocSkinV2 s, NocCellV2 c, int type, int band, float y0, float y1, out int jF, out int jL)
{
    int rows = (int)round(c.bandH / c.Pv);
    jF = 0; jL = rows - 1;
    if (type == NOC_ST_RES && NocMechStripeV2(s.key, band)) jL = rows - 3;
    if (NocKnownY(y0, y1))
    {
        float baseH = y0 <= NC_FaBaseY0 ? NC_FaBase : 0.0;
        float lo = y0 + baseH - band * c.bandH, hi = y1 - NC_FaParapet - (c.loggia > 0.5 ? c.Pv : 0.0) - band * c.bandH;   // none under the parapet
        jF = max(jF, (int)ceil((lo - c.oy0) / c.Pv));
        jL = min(jL, (int)floor((hi - c.oy1) / c.Pv));
    }
}

// ---------------------------------------------------------------------------------------------------------------- lit probability
float NocNormalV2(uint h1, uint h2)
{
    float u1 = 1.0 - NocU01(h1), u2 = NocU01(h2);
    return sqrt(-2.0 * log(u1)) * cos(6.28318530718 * u2);
}
float NocLogNormal1(float n, float sigma) { return clamp(exp(sigma * n - 0.5 * sigma * sigma), NC_FaLnMin, NC_FaLnMax); }

float NocTowerMulV2(uint K)
{
    uint3 h = NocPcg3d(uint3(K, 0x544F5752u, 7u));
    return NocLogNormal1(NocNormalV2(h.x, h.y), NC_FaTowerSigma);
}

// Lit floor groups of tower K in a band (groups of 4..12 rows from the band's bottom): the group factors of rows j0 and j0 + 1
// and, for the filtered mean, the group of row jc with its row interval and its neighbours' factors.
struct NocGroupsV2 { float m0, m1, gC, gLo, gHi; int cS, cE; bool hasLo, hasHi; };

NocGroupsV2 NocGroupsOfV2(uint K, int band, int j0, int jc, int rows)
{
    NocGroupsV2 r;
    r.m0 = 1.0; r.m1 = 1.0; r.gC = 1.0; r.gLo = 1.0; r.gHi = 1.0; r.cS = 0; r.cE = rows; r.hasLo = false; r.hasHi = false;
    int start = 0; float prev = 1.0; bool wantNext = false;
    [loop] for (int g = 0; g < 8; g++)
    {
        uint3 h = NocPcg3d(uint3(K, (uint)(band + 65536), 0x47525000u + (uint)g));
        int len = (int)NC_FaGroupMinRows + (int)floor((NC_FaGroupMaxRows - NC_FaGroupMinRows + 1.0) * NocU01(h.x));
        float mul = NocLogNormal1(NocNormalV2(h.y, h.z), NC_FaGroupSigma);
        int end = start + len;
        if (wantNext) { r.gHi = mul; r.hasHi = true; wantNext = false; }
        // row j belongs to group g when j < end (rows below the band's bottom fall in group 0, as the C# twin's GroupOf)
        bool in0 = j0 < end && (g == 0 || j0 >= start), in1 = j0 + 1 < end && (g == 0 || j0 + 1 >= start);
        if (in0) r.m0 = mul;
        if (in1) r.m1 = mul;
        if (jc < end && (g == 0 || jc >= start))
        {
            r.gC = mul; r.cS = start; r.cE = end; r.gLo = prev; r.hasLo = g > 0;
            wantNext = end < rows;
        }
        prev = mul;
        start = end;
        if (start > max(j0 + 1, jc) && !wantNext) break;
    }
    return r;
}

float NocZoneRowU(uint K, int zrg)              { return NocU01(NocH3x(K, (uint)(zrg + 1048576), 0x5A524F57u)); }
float NocZoneShiftV2(uint face, int zrg)        { return floor(NC_FacadeZoneCols * NocU01(NocH3x(face, (uint)(zrg + 1048576), 0x5A4F4E45u))); }
float NocZoneCellU(uint face, int zrg, int zc)  { return NocU01(NocH3y((uint)(zc + 1048576), (uint)(zrg + 1048576), face)); }
float NocZoneZ(float uRow, float uCell)          { return NC_FaZoneLo + NC_FaZoneSpan * 0.5 * (uRow + uCell); }

// Zone level box-filtered over the 2 x 2 zones under the footprint (rows per tower, columns per face).
float NocZoneFilteredV2(uint K, uint face, int band, float u, float vl, float fwu, float fwv, NocCellV2 c)
{
    float zh = NC_FacadeZoneRows * c.Pv, zw = NC_FacadeZoneCols * c.Pu;
    float r0 = floor((vl - 0.5 * fwv) / zh);
    float acc = 0.0, wsum = 0.0;
    [unroll] for (int a = 0; a < 2; a++)
    {
        float zrl = r0 + a;
        float cvz = NocCover(vl, fwv, zrl * zh, (zrl + 1.0) * zh);
        int zrg = band * 16 + (int)zrl;
        float sh = NocZoneShiftV2(face, zrg), ur = NocZoneRowU(K, zrg);
        float c0 = floor((u - 0.5 * fwu) / zw + sh / NC_FacadeZoneCols);
        [unroll] for (int b = 0; b < 2; b++)
        {
            float cc = c0 + b;
            float cuz = NocCover(u, fwu, (NC_FacadeZoneCols * cc - sh) * c.Pu, (NC_FacadeZoneCols * (cc + 1.0) - sh) * c.Pu);
            float w = cuz * cvz;
            acc += w * NocZoneZ(ur, NocZoneCellU(face, zrg, (int)cc));
            wsum += w;
        }
    }
    return acc / max(wsum, 1e-6);
}

// ---------------------------------------------------------------------------------------------------------------- hue, brightness
float NocY709(float3 c) { return dot(c, float3(0.2126, 0.7152, 0.0722)); }
float3 NocNormY(float3 c, float g) { return c * (NC_FaYMed * g / max(NocY709(c), 1e-6)); }
float3 NocDesatV2(float3 c) { return lerp(c, PAL_WinNeutral, NC_FaColDesat); }

float3 NocClassColourV2(int k)
{
    if (k == 0) return NocNormY(PAL_WinTungsten, 1.0);
    if (k == 1) return NocNormY(PAL_WinWarm, 1.0);
    if (k == 2) return NocNormY(PAL_WinNeutral, 1.0);
    if (k == 3) return NocNormY(PAL_WinCold, 1.0);
    if (k == 4) return NocNormY(PAL_WinBlue, NC_FaBlueGain);
    if (k == 5) return NocNormY(NocDesatV2(PAL_NeonCyan), NC_FaColGain);
    if (k == 6) return NocNormY(NocDesatV2(PAL_AviationRed), NC_FaColGain);
    if (k == 7) return NocNormY(NocDesatV2(PAL_LeafGlow), NC_FaColGain);
    return NocNormY(NocDesatV2(PAL_NeonMagenta), NC_FaColGain);
}

float NocHueStep(float t, float U) { return saturate((t - U) / NC_FaHueBand + 0.5); }

// Lit-window colour of hue hash uh (luminance Y_med for the white classes): the class boundaries w_f / 3, w_f, w_f + n_f move
// with s and are soft over 0.03; at w_f = 0 no window is warm.
float3 NocHueV2(float uh, float wf, float nf)
{
    float gw = saturate(wf / NC_FaHueBand);
    float s1 = NocHueStep(wf / 3.0, uh) * gw, s2 = NocHueStep(wf, uh) * gw, s3 = NocHueStep(wf + nf, uh);
    if (uh < NC_FaHueBlue0)
        return NocClassColourV2(0) * s1 + NocClassColourV2(1) * (s2 - s1) + NocClassColourV2(2) * max(0.0, s3 - s2) + NocClassColourV2(3) * (1.0 - max(s3, s2));
    if (uh < NC_FaHueCol0) return NocClassColourV2(4);
    float x = uh - NC_FaHueCol0;
    return NocClassColourV2(x < NC_FaColCyan ? 5 : (x < NC_FaColCyan + NC_FaColRed ? 6 : (x < NC_FaColCyan + NC_FaColRed + NC_FaColLeaf ? 7 : 8)));
}

float3 NocMeanHueV2(float wf, float nf)
{
    float gw = saturate(wf / NC_FaHueBand);
    float tung = wf / 3.0 * gw, warm = 2.0 * wf / 3.0 * gw, neut = nf + wf * (1.0 - gw);
    float cold = NC_FaHueBlue0 - wf - nf, blue = NC_FaHueCol0 - NC_FaHueBlue0;
    return NocClassColourV2(0) * tung + NocClassColourV2(1) * warm + NocClassColourV2(2) * neut + NocClassColourV2(3) * cold + NocClassColourV2(4) * blue
         + NocClassColourV2(5) * NC_FaColCyan + NocClassColourV2(6) * NC_FaColRed + NocClassColourV2(7) * NC_FaColLeaf + NocClassColourV2(8) * NC_FaColMagenta;
}

// Mechanical slits: SodiumAmber on half of them x k_a (= w_f / 0.32, LOOK_V2 R6), the rest SignWhite, at 0.6 Y_med.
float3 NocMechHueV2(float uh, float wf)
{
    float ka = saturate(wf / 0.32);
    return uh < 0.5 * ka ? NocNormY(PAL_SodiumAmber, 0.6) : NocNormY(PAL_SignWhite, 0.6);
}
float3 NocMeanMechHueV2(float wf)
{
    float ka = saturate(wf / 0.32);
    return NocNormY(PAL_SodiumAmber, 0.6) * (0.5 * ka) + NocNormY(PAL_SignWhite, 0.6) * (1.0 - 0.5 * ka);
}

float NocBrightnessV2(uint hx) { return clamp(exp(NC_FaBSigma * NocNormalV2(hx, NocPcg(hx ^ 0x42524954u))), NC_FaBMin, NC_FaBMax); }

// E[I] of the interior factor (C# NocFacadeMath.InteriorMean): falloff (1 + 0.7) / 2 x curtains (closure 0.4..1.0, x0.45) /
// blinds (x0.45) / bare.
float NocInteriorMeanV2()
{
    float fall = 0.5 * (1.0 + NC_FaFalloffSill);
    float curtain = 0.7 * NC_FaCurtainGain + 0.3;
    return fall * (NC_FaCurtainP * curtain + NC_FaBlindP * NC_FaBlindOpen + (1.0 - NC_FaCurtainP - NC_FaBlindP));
}

// ---------------------------------------------------------------------------------------------------------------- one window
struct NocWinV2
{
    float lit;      // 0..1 with the toggle ramp
    float3 eLit;    // lit glass radiance (hue x b), interior factor excluded
    float3 eUn;     // stand-by glass light
    int kind;       // 0 bare, 1 curtain, 2 blinds
    float cf, side, tiltA, tiltB;
    float depth;    // room depth hash (FA-3 box rooms)
};

NocWinV2 NocWindowV2(int i, int jl, int band, uint face, int type, float p, float tEpoch, float wf, float nf)
{
    NocWinV2 w;
    int jg = band * 32 + jl;
    uint3 h = NocPcg3d(uint3((uint)(i + 1048576), (uint)(jg + 1048576), face));
    float x = tEpoch + NocU01(h.z);
    float e = floor(x);
    uint4 k = uint4((uint)(i + 1048576), (uint)(jg + 1048576), face, (uint)e);
    // FX (INT-14): a section floor lights in runs (NocFacadeConstants FaSecRun*): the run is on with p / q, each of its cells with
    // q = max(FaSecRunFill, p), so E[lit] = p as before; the run toggles on its own epoch (twin NocFacadeMath.WindowV2)
    float pc = p, runLit = 1.0, uRun = -1.0;
    [branch] if (type == NOC_ST_SEC)
    {
        float qf = max(NC_FaSecRunFill, p), pr = p / qf;
        pc = qf;
        uint hr = NocPcg3d(uint3((uint)(jg + 1048576), face, 0x52554E53u)).x;
        float L = NC_FaSecRunMin + floor((NC_FaSecRunMax - NC_FaSecRunMin + 1.0) * NocU01(hr));
        float r = floor(((float)i + floor(L * NocU01(NocPcg(hr)))) / L);
        uint3 hq = NocPcg3d(uint3((uint)(r + 1048576.0), (uint)(jg + 1048576), face ^ 0x52554E31u));
        float xr = tEpoch + NocU01(hq.z), er = floor(xr);
        float rNow = NocU01(NocPcg4d(uint4(hq.x, hq.y, face, (uint)er)).x) <= pr ? 1.0 : 0.0;
        float rPrev = NocU01(NocPcg4d(uint4(hq.x, hq.y, face, (uint)er - 1u)).x) <= pr ? 1.0 : 0.0;
        runLit = lerp(rPrev, rNow, smoothstep(0.0, NC_FaToggle / NC_FaEpoch, xr - er));
        uRun = NocU01(NocPcg(hq.x ^ 0x48554555u));          // one hue per run: a lit floor is one kind of light
    }
    float aNow = NocU01(NocPcg4d(k).x) <= pc ? 1.0 : 0.0;
    float aPrev = NocU01(NocPcg4d(uint4(k.xyz, k.w - 1u)).x) <= pc ? 1.0 : 0.0;
    w.lit = runLit * lerp(aPrev, aNow, smoothstep(0.0, NC_FaToggle / NC_FaEpoch, x - e));
    float uh = uRun >= 0.0 ? uRun : NocU01(h.y);
    float3 hue = type == NOC_ST_MECH ? NocMechHueV2(uh, wf) : NocHueV2(uh, wf, nf);
    w.eLit = hue * NocBrightnessV2(h.x);
    w.eUn = PAL_WinNeutral * (NC_FaStandby * (0.5 + NocU01(NocPcg(h.x ^ h.z))));
    float uk = NocU01(NocPcg(h.y ^ 0x43555254u));
    w.kind = uk < NC_FaCurtainP ? 1 : (uk < NC_FaCurtainP + NC_FaBlindP ? 2 : 0);
    uint hc = NocPcg(h.z ^ 0x43464F4Cu);
    w.cf = 0.4 + 0.6 * NocU01(hc);
    w.side = NocU01(NocPcg(hc));
    w.tiltA = 2.0 * NocU01(NocPcg(h.x ^ 0x54494C54u)) - 1.0;
    w.tiltB = 2.0 * NocU01(NocPcg(h.y ^ 0x54494C54u)) - 1.0;
    w.depth = NocU01(NocPcg(h.y ^ h.z));
    return w;
}

// Room depth of stratum `type` for depth hash u (TEXTURE_PLAN 2.8, 2.10; NocInteriorMath.Depth).
float NocRoomDepthV2(int type, float u)
{
    if (type == NOC_ST_SEC) return lerp(NC_FaRoomDepthSec0, NC_FaRoomDepthSec1, u);
    if (type == NOC_ST_COM || type == NOC_ST_LOBBY) return lerp(NC_FaRoomDepthCom0, NC_FaRoomDepthCom1, u);
    return lerp(NC_FaRoomDepthRes0, NC_FaRoomDepthRes1, u);
}

// ---------------------------------------------------------------------------------------------------------------- coverages
// Clear glass along x of the cell starting at su (world u): the visible glass behind the recess (shifted by -sx) minus the frame
// and up to 4 mullions inside the opening. frac: the cell-mean fraction (x, fw ignored).
float NocGlassX(float x, float fw, float su, NocCellV2 c, float sx, bool frac)
{
    float v0 = max(c.ox0, c.ox0 - sx), v1 = min(c.ox1, c.ox1 - sx);
    float g0 = max(v0, c.gx0 - sx), g1 = min(v1, c.gx1 - sx);
    float r = frac ? max(0.0, g1 - g0) / c.Pu : NocCover(x, fw, su + g0, su + g1);
    [branch] if (c.mW > 0.0)
    {
        float k0 = floor((c.ox0 - c.mOff) / c.mP) + 1.0;
        [loop] for (int k = 0; k < 4; k++)
        {
            float mc = c.mOff + (k0 + k) * c.mP;
            if (mc >= c.ox1) break;
            float a = max(g0, mc - 0.5 * c.mW - sx), b = min(g1, mc + 0.5 * c.mW - sx);
            r -= frac ? max(0.0, b - a) / c.Pu : NocCover(x, fw, su + a, su + b);
        }
    }
    return max(0.0, r);
}

float NocGlassY(float y, float fw, float sv, NocCellV2 c, float sy, bool frac)
{
    float v0 = max(c.oy0, c.oy0 - sy), v1 = min(c.oy1, c.oy1 - sy);
    float g0 = max(v0, c.gy0 - sy), g1 = min(v1, c.gy1 - sy);
    float r = frac ? max(0.0, g1 - g0) / c.Pv : NocCover(y, fw, sv + g0, sv + g1);
    if (c.tW > 0.0)
    {
        float a = max(g0, c.tY - 0.5 * c.tW - sy), b = min(g1, c.tY + 0.5 * c.tW - sy);
        r -= frac ? max(0.0, b - a) / c.Pv : NocCover(y, fw, sv + a, sv + b);
    }
    return max(0.0, r);
}

// Rain-scatter halo under an opening: exp(-d / 0.4) for d = oy0 - y in [0, 1.2], box-integrated (frac: the cell mean).
float NocHaloY(float y, float fw, float sv, NocCellV2 c, bool frac)
{
    float L = NC_FaHaloL, top = sv + c.oy0, lo = top - NC_FaHaloMax;
    if (frac) return L * (1.0 - exp(-NC_FaHaloMax / L)) / c.Pv;
    float a = max(lo, y - 0.5 * fw), b = min(top, y + 0.5 * fw);
    return b <= a ? 0.0 : L * (exp(-(top - b) / L) - exp(-(top - a) / L)) / max(fw, 1e-6);
}

// Room-light spill weights of one cell (TEXTURE_PLAN 2.6): jambs x0.25, sill x0.35 or head x0.15 (by the sign of the vertical
// shift) at the reveal albedo, frame edges x0.20 at MetalPaint, the halo x0.02. Bilinear in the per-axis coverages.
float3 NocSpillCovV2(float ox, float oy, float vx, float vy, float gx, float gy, float hy, float ky, float3 albRev)
{
    float jamb = max(0.0, (ox - vx) * oy), sh = max(0.0, vx * (oy - vy)), fr = max(0.0, vx * vy - gx * gy);
    float fRev = NC_FaSpillJamb * jamb + (ky < 0.0 ? NC_FaSpillSill : NC_FaSpillHead) * sh;
    return albRev * fRev + PAL_MetalPaint * (NC_FaSpillFrame * fr) + NC_FaHalo * ox * hy;
}

float3 NocWallTokenV2(NocSkinV2 s, int type)
{
    if (type == NOC_ST_SEC) return PAL_ConcreteDark;
    if (s.id == NOC_SK_S0) return PAL_MetalPaint;
    if (s.id == NOC_SK_S2) return PAL_MetalPaint * NC_FaS2Albedo;
    if (s.id == NOC_SK_S3) return PAL_Brick;
    if (s.id == NOC_SK_S4) return PAL_Plaster * NC_FaS4Albedo;
    return PAL_ConcreteDark;
}

// ---------------------------------------------------------------------------------------------------------------- section macro
// Mode 1 (the Gulf cut faces and the tongue hull, TEXTURE_PLAN 2.10, CRIT0 #6): the face is cut into districts of FaSecDistU x
// FaSecDistV (28 bays x 75 floors, so every 9 x 4 m cell lies in one district). Each district has
//   - a lit-density multiplier (log-normal FaSecDistSigma, mean 1): dark voids and lit cores, box-filtered over the districts
//     under the footprint for the band means, exact per cell for the resolved windows;
//   - a mechanical band FaSecMechH tall at its bottom: blind louvre wall with one louvre slit per floor (FaSecSlitH, drawn at
//     least 1 px with its energy kept), lit on FaSecSlitLit of the floors, amber x k_a or SignWhite at 0.6 Y_med;
//   - one glazed atrium (FaSecAtriumW x FaSecAtriumH, whole bays and floors, at a hashed place inside the district), lit on
//     FaSecAtriumLit of the districts as a tall hall (FaSecAtriumGain x Y_med, warm to neutral with w_f), with a mullion grid.
// Every part is an exact interval coverage, so it holds at every distance (a 27 x 48 m atrium is 7 x 13 px at 3 km).
float NocSecDistMulV2(uint face, float ub, float vb)
{
    uint3 h = NocPcg3d(uint3((uint)(ub + 1048576.0), (uint)(vb + 1048576.0), face ^ 0x44495354u));
    // FX (INT-14): a share FaSecVoidP of the districts are dark voids, the rest carry the remainder (mean 1)
    return NocU01(h.z) < NC_FaSecVoidP ? NC_FaSecVoidMul
         : NocLogNormal1(NocNormalV2(h.x, h.y), NC_FaSecDistSigma) * ((1.0 - NC_FaSecVoidP * NC_FaSecVoidMul) / (1.0 - NC_FaSecVoidP));
}

// District multiplier box-filtered over the 2 x 2 districts under the footprint (the band-mean regime).
float NocSecDistFilteredV2(uint face, float u, float v, float fwu, float fwv)
{
    float u0 = floor((u - 0.5 * fwu) / NC_FaSecDistU), v0 = floor((v - 0.5 * fwv) / NC_FaSecDistV);
    float acc = 0.0, ws = 0.0;
    [unroll] for (int b = 0; b < 2; b++)
        [unroll] for (int a = 0; a < 2; a++)
        {
            float ub = u0 + a, vb = v0 + b;
            float w = NocCover(u, fwu, ub * NC_FaSecDistU, (ub + 1.0) * NC_FaSecDistU) * NocCover(v, fwv, vb * NC_FaSecDistV, (vb + 1.0) * NC_FaSecDistV);
            acc += w * NocSecDistMulV2(face, ub, vb);
            ws += w;
        }
    return acc / max(ws, 1e-6);
}

struct NocSecMacroOutV2 { float mech, atrium, atGlass; float3 em; };

NocSecMacroOutV2 NocSecMacroV2(uint face, float u, float v, float fwu, float fwv, float wf)
{
    NocSecMacroOutV2 m;
    float ub = floor(u / NC_FaSecDistU), vb = floor(v / NC_FaSecDistV);
    float du = u - ub * NC_FaSecDistU, dv = v - vb * NC_FaSecDistV;
    uint3 h = NocPcg3d(uint3((uint)(ub + 1048576.0), (uint)(vb + 1048576.0), face ^ 0x4154524Du));
    // mechanical band at the district's bottom: louvre wall + one slit per floor
    m.mech = NocCover(dv, fwv, 0.0, NC_FaSecMechH);
    float3 em = 0.0;
    float ka = saturate(wf / 0.32);
    float3 slitE = (NocU01(h.z) < 0.5 * ka ? NocNormY(PAL_SodiumAmber, 0.6) : NocNormY(PAL_SignWhite, 0.6));
    float d = max(NC_FaSecSlitH, fwv);
    // slits in 9 m bay segments (gaps at the columns), each (bay, floor) lit by its own hash: the two bays under the footprint
    float b0 = floor((du - 0.5 * fwu) / NC_FaSecColP);
    [branch] if (m.mech > 0.0)
    [unroll] for (int f = 0; f < 3; f++)
    {
        float c = NC_FacadeSecH * (f + 0.5);
        float cv = NocCover(dv, fwv, c - 0.5 * d, c + 0.5 * d) * (NC_FaSecSlitH / d);
        [unroll] for (int bb = 0; bb < 2; bb++)
        {
            float bay = b0 + bb;
            float lit = NocU01(NocPcg(h.x ^ (uint)(f * 0x9E37 + 0x51) ^ NocPcg((uint)(bay + 4096.0)))) < NC_FaSecSlitLit ? 1.0 : 0.0;
            float cu = NocCover(du, fwu, bay * NC_FaSecColP + 0.5 * NC_FaSecSlitGap, (bay + 1.0) * NC_FaSecColP - 0.5 * NC_FaSecSlitGap);
            em += slitE * (lit * cv * cu);
        }
    }
    // the atrium: whole bays and floors, inside the district above the mechanical band
    float nbu = floor(NC_FaSecDistU / NC_FaSecColP), nbv = floor(NC_FaSecDistV / NC_FacadeSecH);
    float aw = NC_FaSecAtriumW / NC_FaSecColP, ah = NC_FaSecAtriumH / NC_FacadeSecH, mb = NC_FaSecMechH / NC_FacadeSecH;
    float ia = 1.0 + floor(NocU01(h.x) * (nbu - aw - 2.0)), ja = mb + 1.0 + floor(NocU01(h.y) * (nbv - ah - mb - 2.0));
    float ax0 = ia * NC_FaSecColP, ax1 = ax0 + NC_FaSecAtriumW, ay0 = ja * NC_FacadeSecH, ay1 = ay0 + NC_FaSecAtriumH;
    m.atrium = NocCover(du, fwu, ax0, ax1) * NocCover(dv, fwv, ay0, ay1);
    float atLit = NocU01(NocPcg(h.z ^ 0x4C495431u)) < NC_FaSecAtriumLit ? 1.0 : 0.0;
    float3 atHue = lerp(NocNormY(PAL_WinNeutral, 1.0), NocNormY(PAL_WinWarm, 1.0), saturate(wf / 0.32) * NocU01(NocPcg(h.y ^ 0x48554555u)));
    // mullion grid of the hall glazing (filtered bars, energy kept, mean below 4 px)
    float gx = 1.0 - NocFilteredGrid1(du - ax0, NC_FaSecAtriumMullU, NC_FaSecAtriumMull, fwu, 4.0);
    float gy = 1.0 - NocFilteredGrid1(dv - ay0, NC_FacadeSecH, NC_FaSecAtriumMull, fwv, 4.0);
    m.atGlass = gx * gy;
    // the hall behind the glazing: a lit balcony edge at every floor, brighter toward the hall floor (mean factor 0.61)
    float edge = NocFilteredGrid1(dv - ay0, NC_FacadeSecH, NC_FaSecAtriumEdge, fwv, 3.0);
    float tH = saturate((dv - ay0) / NC_FaSecAtriumH);
    float hall = (NC_FaSecHallBase + NC_FaSecHallEdge * edge) * (1.3 - 0.6 * tH);
    em += atHue * (NC_FaSecAtriumGain * atLit * m.atGlass * hall) * m.atrium;
    m.em = em;
    return m;
}

// ---------------------------------------------------------------------------------------------------------------- evaluation
struct NocFaceInV2
{
    float u, v, fwu, fwv;        // face coordinates (u clamped to [uMin, uMax]) and bounding-box footprints (m / px)
    uint face; uint skinByte; int mode;
    float uMin, uMax, y0, y1;    // y0 = y1 = 0: unknown (Gulf writers)
    float kx, ky;                // recess shift per metre of depth: dt / dn and (db / dn) B.y
    float tEpoch, wf, nf;
    float forceW;                // < 0: footprint fades; 0 or 1 forces the window axes (H2 tests)
};

struct NocFaceOutV2
{
    float3 emission;             // lit / stand-by glass, room spill, halo (interior factor included)
    float aGlass, aSpandrel, aFrame, aJamb, aSillHead, aOpen, aSlab, aLedge;
    float wu, wv, pEff;
    NocSkinV2 skin; NocCellV2 cell;
    int type, band, rows;
    float allowMask;             // 1 inside a window bay and row at the pixel centre (debug view 3)
    float cellFx, cellFy;        // frac(x / P.x), frac(y / P.y) at the pixel centre
    float tone;                  // per-panel x per-face albedo factor (mean 1)
    float tiltA, tiltB, tiltW;   // pane tilt of the cell under the pixel centre, and its weight (0 below 2 px panes)
    float winPx;                 // window height in pixels
    float louver;                // section mechanical band (louvre wall) coverage, mode 1
};

// C11 (AT, integration round): strata below the walk go dark with depth, so AT's sink veil lifts a dark wall into the umber
// haze: the lit probability x (1 - 0.6 S01((-40 - y) / 400)) at world height y (1 above y -40, 0.4 below y -440).
// NocS01 is NocAtmosphere.hlsl's (every includer includes NocLitCore first). Mirrored by NocFacadeMath.DepthLitV2.
float NocFaDepthLitV2(float yWS) { return 1.0 - 0.6 * NocS01((-40.0 - yWS) / 400.0); }

// Mean (unresolved) fractions of a band of type `type` at band-local vl (neighbour bands of a footprint straddling a boundary):
// opening, glass, frame, jamb, sill / head, slab, spandrel, and the emission of those parts.
void NocBandMeanPartV2(NocSkinV2 s, int band, int mode, float u, float vlNb, float fwu, float fwv, float uMin, float uMax,
    float y0, float y1, float kx, float ky, float wf, float nf, out float4 aA, out float3 aB, out float3 em)
{
    int type = NocStratumTypeV2(s.key, band, mode);
    NocCellV2 c = NocCellBandV2(s, type, band);
    float sx = c.D * kx, sy = c.D * ky;
    int iF, iL, k0, k1, jF, jL;
    NocBaysV2(s, c, type, uMin, uMax, iF, iL, k0, k1);
    NocRowsV2(s, c, type, band, y0, y1, jF, jL);
    int rows = (int)round(c.bandH / c.Pv);
    float Ax = NocCover(u, fwu, iF * c.Pu, (iL + 1) * c.Pu);
    if (k1 >= k0) Ax = max(0.0, Ax - NocCover(u, fwu, k0 * c.Pu, (k1 + 1) * c.Pu));
    float Ay = NocCover(vlNb, fwv, jF * c.Pv, (jL + 1) * c.Pv), Aband = NocCover(vlNb, fwv, 0.0, rows * c.Pv);
    float mOx = (c.ox1 - c.ox0) / c.Pu, mOy = (c.oy1 - c.oy0) / c.Pv;
    float mVx = max(0.0, min(c.ox1, c.ox1 - sx) - max(c.ox0, c.ox0 - sx)) / c.Pu, mVy = max(0.0, min(c.oy1, c.oy1 - sy) - max(c.oy0, c.oy0 - sy)) / c.Pv;
    float mGx = NocGlassX(0.0, 1.0, 0.0, c, sx, true), mGy = NocGlassY(0.0, 1.0, 0.0, c, sy, true);
    float Ox = mOx * Ax, Oy = mOy * Ay, Vx = mVx * Ax, Vy = mVy * Ay, Gx = mGx * Ax, Gy = mGy * Ay, Hy = NocHaloY(0.0, 1.0, 0.0, c, true) * Ay;
    float aOpen = Ox * Oy, aGlass = Gx * Gy;
    aA = float4(aOpen, aGlass, max(0.0, Vx * Vy - aGlass), max(0.0, (Ox - Vx) * Oy));
    float aSp = c.sp1 > c.sp0 ? NocGlassX(0.0, 1.0, 0.0, c, 0.0, true) * Ax * (c.sp1 - c.sp0) / c.Pv * Ay : 0.0;
    aB = float3(max(0.0, Vx * (Oy - Vy)), c.slab / c.Pv * Aband, aSp);
    float p = NC_FaLit * NocFactorV2(type) * NocTowerMulV2(s.key) * (s.region == 1 ? NC_FaOqLit : 1.0)
            * NocFaDepthLitV2(band * c.bandH + vlNb);                                         // C11 depth fade (world y)
    float3 hue = type == NOC_ST_MECH ? NocMeanMechHueV2(wf) : NocMeanHueV2(wf, nf);
    float3 eLitMean = hue * NC_FaBMean, eUnMean = PAL_WinNeutral * NC_FaStandby;
    float3 albRev = NocWallTokenV2(s, type) * NC_FaAoReveal;
    em = (eLitMean * p + eUnMean * (1.0 - p)) * aGlass + eLitMean * p * NocSpillCovV2(Ox, Oy, Vx, Vy, Gx, Gy, Hy, ky, albRev);
}

NocFaceOutV2 NocFacadeEvalV2(NocFaceInV2 q)
{
    NocFaceOutV2 o = (NocFaceOutV2)0;
    NocSkinV2 s = NocSkinOfV2(q.skinByte, q.mode);
    uint K = s.key;
    float fwu = max(q.fwu, 1e-4), fwv = max(q.fwv, 1e-4);
    float bandH = q.mode == 1 ? NC_FacadeSecBand : NC_FacadeBand;
    float bandF = floor(q.v / bandH);
    int band = (int)bandF;
    float vl = q.v - bandF * bandH;
    int type = NocStratumTypeV2(K, band, q.mode);
    NocCellV2 c = NocCellBandV2(s, type, band);
    o.skin = s; o.cell = c; o.type = type; o.band = band;
    float wu = smoothstep(NC_FaFadePx0, NC_FaFadePx1, c.Pu / fwu);
    float wv = smoothstep(NC_FaFadePx0, NC_FaFadePx1, c.Pv / fwv);
    if (q.forceW >= 0.0) { wu = q.forceW; wv = q.forceW; }
    float sx = c.D * q.kx, sy = c.D * q.ky;
    // the glass visible behind the recess shrinks at oblique views: a lit sliver under 1-2 px sparkles in pans (S1), so the axis
    // falls to its view-dependent mean (m_g(V): the same expectation, E4) while the visible glass is under FaSliverPx0..1 px
    if (q.forceW < 0.0)
    {
        float vgx = max(0.0, min(min(c.ox1, c.ox1 - sx), c.gx1 - sx) - max(max(c.ox0, c.ox0 - sx), c.gx0 - sx));
        float vgy = max(0.0, min(min(c.oy1, c.oy1 - sy), c.gy1 - sy) - max(max(c.oy0, c.oy0 - sy), c.gy0 - sy));
        wu *= smoothstep(NC_FaSliverPx0, NC_FaSliverPx1, vgx / fwu);
        wv *= smoothstep(NC_FaSliverPx0, NC_FaSliverPx1, vgy / fwv);
    }
    o.wu = wu; o.wv = wv;
    int rows = (int)round(c.bandH / c.Pv);
    o.rows = rows;

    // whole bays and rows; the allowed coverage of the footprint (the mean regime counts the suppressed cells)
    int iF, iL, k0, k1, jF, jL;
    NocBaysV2(s, c, type, q.uMin, q.uMax, iF, iL, k0, k1);
    NocRowsV2(s, c, type, band, q.y0, q.y1, jF, jL);
    float Ax = NocCover(q.u, fwu, iF * c.Pu, (iL + 1) * c.Pu);
    if (k1 >= k0) Ax = max(0.0, Ax - NocCover(q.u, fwu, k0 * c.Pu, (k1 + 1) * c.Pu));
    float Ay = NocCover(vl, fwv, jF * c.Pv, (jL + 1) * c.Pv), Aband = NocCover(vl, fwv, 0.0, rows * c.Pv);

    // per-axis mean fractions
    float mOx = (c.ox1 - c.ox0) / c.Pu, mOy = (c.oy1 - c.oy0) / c.Pv;
    float mVx = max(0.0, min(c.ox1, c.ox1 - sx) - max(c.ox0, c.ox0 - sx)) / c.Pu;
    float mVy = max(0.0, min(c.oy1, c.oy1 - sy) - max(c.oy0, c.oy0 - sy)) / c.Pv;
    float mGx = NocGlassX(0.0, 1.0, 0.0, c, sx, true), mGy = NocGlassY(0.0, 1.0, 0.0, c, sy, true), mHy = NocHaloY(0.0, 1.0, 0.0, c, true);
    float mSx = c.sp1 > c.sp0 ? NocGlassX(0.0, 1.0, 0.0, c, 0.0, true) : 0.0, mSlab = c.slab / c.Pv, mSpy = max(0.0, c.sp1 - c.sp0) / c.Pv;

    // the two cells under the footprint on each axis
    int cx0 = (int)floor((q.u - 0.5 * fwu) / c.Pu), cy0 = (int)floor((vl - 0.5 * fwv) / c.Pv);
    float2 XO, XV, XG, YO, YV, YG, YH;
    float spx = 0.0, spy = 0.0, slabY = 0.0, ledgeY = 0.0;
    float2 cellCu, cellCv;   // full-cell coverages (per-panel tone)
    [unroll] for (int a = 0; a < 2; a++)
    {
        int i = cx0 + a; float su = i * c.Pu;
        bool fx = NocBayHasWindow(i, iF, iL, k0, k1);
        XO[a] = fx ? NocCover(q.u, fwu, su + c.ox0, su + c.ox1) : 0.0;
        XV[a] = fx ? NocCover(q.u, fwu, su + max(c.ox0, c.ox0 - sx), su + min(c.ox1, c.ox1 - sx)) : 0.0;
        XG[a] = fx ? NocGlassX(q.u, fwu, su, c, sx, false) : 0.0;
        if (fx && c.sp1 > c.sp0) spx += NocGlassX(q.u, fwu, su, c, 0.0, false);
        cellCu[a] = NocCover(q.u, fwu, su, su + c.Pu);
        int j = cy0 + a; float sv = j * c.Pv;
        bool fy = j >= jF && j <= jL;
        YO[a] = fy ? NocCover(vl, fwv, sv + c.oy0, sv + c.oy1) : 0.0;
        YV[a] = fy ? NocCover(vl, fwv, sv + max(c.oy0, c.oy0 - sy), sv + min(c.oy1, c.oy1 - sy)) : 0.0;
        YG[a] = fy ? NocGlassY(vl, fwv, sv, c, sy, false) : 0.0;
        YH[a] = fy ? NocHaloY(vl, fwv, sv, c, false) : 0.0;
        cellCv[a] = NocCover(vl, fwv, sv, sv + c.Pv);
        if (j >= 0 && j < rows)
        {
            slabY += NocCover(vl, fwv, sv, sv + c.slab);
            ledgeY += NocCover(vl, fwv, sv + c.slab - 0.05, sv + c.slab);
        }
        if (fy && c.sp1 > c.sp0) spy += NocCover(vl, fwv, sv + c.sp0, sv + c.sp1);
    }
    // blended per-axis totals (resolved 2 x 2 coverage -> mean fraction x allowed coverage)
    float OxE = wu * (XO.x + XO.y) + (1.0 - wu) * mOx * Ax, VxE = wu * (XV.x + XV.y) + (1.0 - wu) * mVx * Ax, GxE = wu * (XG.x + XG.y) + (1.0 - wu) * mGx * Ax;
    float OyE = wv * (YO.x + YO.y) + (1.0 - wv) * mOy * Ay, VyE = wv * (YV.x + YV.y) + (1.0 - wv) * mVy * Ay, GyE = wv * (YG.x + YG.y) + (1.0 - wv) * mGy * Ay;
    float HyE = wv * (YH.x + YH.y) + (1.0 - wv) * mHy * Ay;
    o.aOpen = OxE * OyE; o.aGlass = GxE * GyE; o.aFrame = max(0.0, VxE * VyE - o.aGlass);
    o.aJamb = max(0.0, (OxE - VxE) * OyE); o.aSillHead = max(0.0, VxE * (OyE - VyE));
    o.aSlab = wv * slabY + (1.0 - wv) * mSlab * Aband;
    o.aLedge = wv * ledgeY + (1.0 - wv) * (0.05 / c.Pv) * Aband;
    if (c.sp1 > c.sp0) o.aSpandrel = (wu * spx + (1.0 - wu) * mSx * Ax) * (wv * spy + (1.0 - wv) * mSpy * Ay);

    // lit probability at the pixel: tower x lit-floor group x zone, each level blended to its parent by its own fade
    float aT0 = NC_FaLit * NocFactorV2(type) * NocTowerMulV2(K) * (s.region == 1 ? NC_FaOqLit : 1.0);   // the Old Quarter lights fewer windows
    float aT = aT0 * NocFaDepthLitV2(q.v);   // C11 depth fade at the fragment (q.v = world y); resolved cells take their row centre
    int jc = clamp((int)floor(vl / c.Pv), 0, rows - 1);
    NocGroupsV2 G = NocGroupsOfV2(K, band, cy0, jc, rows);
    float wg = smoothstep(NC_FaGroupFadePx0, NC_FaGroupFadePx1, NC_FaGroupMinRows * c.Pv / fwv);
    float gEff = 1.0;
    [branch] if (wg > 0.0)
    {
        float gy0 = G.cS * c.Pv, gy1 = G.cE * c.Pv;
        float cIn = NocCover(vl, fwv, gy0, gy1), cLo = NocCover(vl, fwv, gy0 - 64.0 * c.Pv, gy0), cHi = NocCover(vl, fwv, gy1, gy1 + 64.0 * c.Pv);
        float gLo = G.hasLo ? G.gLo : 1.0, gHi = G.hasHi ? G.gHi : 1.0;
        float gF = cIn * G.gC + cLo * gLo + cHi * gHi + max(0.0, 1.0 - cIn - cLo - cHi);
        gEff = lerp(1.0, gF, wg);
    }
    float wz = smoothstep(NC_FaFadePx0, NC_FaFadePx1, min(NC_FacadeZoneCols * c.Pu / fwu, NC_FacadeZoneRows * c.Pv / fwv));
    float zEff = 1.0;
    [branch] if (wz > 0.0) zEff = lerp(1.0, NocZoneFilteredV2(K, q.face, band, q.u, vl, fwu, fwv, c), wz);
    float pEff = aT * gEff * zEff;
    [branch] if (q.mode == 1) pEff *= NocSecDistFilteredV2(q.face, q.u, q.v, fwu, fwv);   // district density (band-mean regime)
    o.pEff = pEff;
    float3 hueMean = type == NOC_ST_MECH ? NocMeanMechHueV2(q.wf) : NocMeanHueV2(q.wf, q.nf);
    float3 eLitMean = hueMean * NC_FaBMean;
    float3 eUnMean = PAL_WinNeutral * NC_FaStandby;
    float3 eGlassMean = eLitMean * pEff + eUnMean * (1.0 - pEff);
    float3 albRev = NocWallTokenV2(s, type) * NC_FaAoReveal;

    // resolved cells: per-cell glass and spill coverages x the cell state, with the zero-mean interior factor
    float3 R = 0.0, kS = 0.0; float kG = 0.0;
    float wuv = wu * wv;
    float winPx = (c.oy1 - c.oy0) / fwv;
    o.winPx = winPx;
    float wI = smoothstep(NC_FaIntFadePx0, NC_FaIntFadePx1, winPx);
    float wBox = type == NOC_ST_MECH ? 0.0 : smoothstep(NC_FaBoxPx0, NC_FaBoxPx1, winPx);   // box rooms (FA-3), not behind louvres
    float invEI = 1.0 / NocInteriorMeanV2();
    [branch] if (wuv > 0.0)
    {
        float gw = saturate(q.wf / NC_FaHueBand);
        [unroll] for (int b = 0; b < 2; b++)
        {
            int j = cy0 + b;
            int zrg = band * 16 + (int)((uint)max(j, 0) / (uint)NC_FacadeZoneRows);
            float sh = NocZoneShiftV2(q.face, zrg), uRow = NocZoneRowU(K, zrg);
            float gm = b == 0 ? G.m0 : G.m1;
            [unroll] for (int a = 0; a < 2; a++)
            {
                float gx = XG[a], gy = YG[b];
                float cg = gx * gy;
                float3 cs = NocSpillCovV2(XO[a], YO[b], XV[a], YV[b], gx, gy, YH[b], q.ky, albRev);
                [branch] if (cg + cs.x + cs.y + cs.z > 0.0)
                {
                    int i = cx0 + a;
                    int zc = (int)floor((i + sh) / NC_FacadeZoneCols);
                    // mode 1: the district of the cell (cells never straddle a district: 28 bays x 75 floors)
                    float mc = q.mode == 1 ? NocSecDistMulV2(q.face, floor((i + 0.5) * c.Pu / NC_FaSecDistU), floor((band * bandH + (j + 0.5) * c.Pv) / NC_FaSecDistV)) : 1.0;
                    float dRow = NocFaDepthLitV2(band * bandH + (j + 0.5) * c.Pv);   // C11: one state per window (no split across its height)
                    float p = min(1.0, aT0 * dRow * gm * mc * NocZoneZ(uRow, NocZoneCellU(q.face, zrg, zc)));
                    NocWinV2 w = NocWindowV2(i, j, band, q.face, type, p, q.tEpoch, q.wf, q.nf);
                    // interior factor I / E[I] at the pixel's glass-plane point (x, y in the opening), zero-mean part faded in
                    float xg = q.u - i * c.Pu + sx, yg = vl - j * c.Pv + sy;
                    float fallT = saturate((yg - c.oy0) / max(c.oy1 - c.oy0, 1e-3));
                    float I = lerp(NC_FaFalloffSill, 1.0, fallT);
                    // the box room behind the glass replaces the falloff (same mean: the room factor is raw / its exact mean x E[falloff])
                    [branch] if (wBox > 0.0 && w.lit > 0.0)
                    {
                        // the ceiling never below the glass head (S0's vision glass reaches 3.54 m of the 3.6 m floor)
                        float Dr = NocRoomDepthV2(type, w.depth), yc = max(c.Pv - NC_FaRoomSoffit, c.gy1 + 0.02);
                        float rm = NocRoomMeanV2(c.gx0, c.gx1, c.gy0, c.gy1, q.kx, q.ky, c.Pu, 0.0, yc, Dr);
                        float rr = NocRoomRawV2(xg, yg, q.kx, q.ky, c.Pu, 0.0, yc, Dr) / max(rm, 1e-4);
                        I = lerp(I, 0.5 * (1.0 + NC_FaFalloffSill) * rr, wBox);
                    }
                    float3 eL = w.eLit;
                    if (w.kind == 1)
                    {
                        // the closure is a share of the glass inside the frame, so its mean over the glass is E[cf] (I / E[I] exact)
                        float g0 = c.gx0, g1 = c.gx1, cw = w.cf * (g1 - g0);
                        float cov = w.side < 1.0 / 3.0 ? NocCover(xg, fwu, g0, g0 + cw)
                                  : (w.side < 2.0 / 3.0 ? NocCover(xg, fwu, g1 - cw, g1)
                                  : NocCover(xg, fwu, g0, g0 + 0.5 * cw) + NocCover(xg, fwu, g1 - 0.5 * cw, g1));
                        I *= 1.0 - (1.0 - NC_FaCurtainGain) * cov;
                        // the curtain's tint lerp(hue, WinWarm, 0.25) at the window's luminance; none while w_f = 0 (the Heart)
                        float3 warmL = NocNormY(PAL_WinWarm, NocY709(w.eLit) / NC_FaYMed);
                        eL = lerp(w.eLit, warmL, NC_FaCurtainTint * gw * cov * wI);
                    }
                    else if (w.kind == 2)
                    {
                        float slat = NocFilteredGrid1(yg, NC_FaBlindPitch, NC_FaBlindPitch * (1.0 - NC_FaBlindOpen), fwv, NC_FaBlindFadePx);
                        I *= 1.0 - slat;
                    }
                    float Ieff = 1.0 + (I * invEI - 1.0) * wI;
                    R += eL * (w.lit * cg * Ieff) + w.eLit * cs * w.lit + w.eUn * ((1.0 - w.lit) * cg);
                    kG += cg; kS += cs;
                    // the pane under the pixel centre carries its tilt
                    if (a == (int)(floor(q.u / c.Pu) - cx0) && b == (int)(floor(vl / c.Pv) - cy0)) { o.tiltA = w.tiltA; o.tiltB = w.tiltB; }
                }
            }
        }
    }
    float3 spillE = NocSpillCovV2(OxE, OyE, VxE, VyE, GxE, GyE, HyE, q.ky, albRev);
    o.emission = R * wuv + eGlassMean * (o.aGlass - wuv * kG) + eLitMean * (spillE - kS * wuv) * pEff;

    // neighbour bands: the unresolved part of a footprint that straddles a band boundary takes that band's mean
    float lo = NocCover(vl, fwv, -1e6, 0.0), hi = NocCover(vl, fwv, rows * c.Pv, 1e6);
    [branch] if ((lo + hi) * (1.0 - wv) > 0.0)
    {
        float4 aA; float3 aB, em;
        float k = 1.0 - wv;
        [branch] if (lo > 0.0)
        {
            NocBandMeanPartV2(s, band - 1, q.mode, q.u, vl + bandH, fwu, fwv, q.uMin, q.uMax, q.y0, q.y1, q.kx, q.ky, q.wf, q.nf, aA, aB, em);
            o.aOpen += k * aA.x; o.aGlass += k * aA.y; o.aFrame += k * aA.z; o.aJamb += k * aA.w; o.aSillHead += k * aB.x;
            o.aSlab += k * aB.y; o.aSpandrel += k * aB.z; o.emission += k * em;
        }
        [branch] if (hi > 0.0)
        {
            NocBandMeanPartV2(s, band + 1, q.mode, q.u, vl - bandH, fwu, fwv, q.uMin, q.uMax, q.y0, q.y1, q.kx, q.ky, q.wf, q.nf, aA, aB, em);
            o.aOpen += k * aA.x; o.aGlass += k * aA.y; o.aFrame += k * aA.z; o.aJamb += k * aA.w; o.aSillHead += k * aB.x;
            o.aSlab += k * aB.y; o.aSpandrel += k * aB.z; o.emission += k * em;
        }
    }

    // per-panel tone (+-12 % by cell hash, exact 2 x 2 box filter, faded below 2-4 px panels) x per-face tone (+-4 %)
    float wt = smoothstep(NC_FaTexPx0, NC_FaTexPx1, min(c.Pu / fwu, c.Pv / fwv));
    float tone = 1.0;
    [branch] if (wt > 0.0)
    {
        float acc = 0.0, ws = 0.0;
        [unroll] for (int b2 = 0; b2 < 2; b2++)
            [unroll] for (int a2 = 0; a2 < 2; a2++)
            {
                float w = cellCu[a2] * cellCv[b2];
                float h = NocU01(NocPcg3d(uint3((uint)(cx0 + a2 + 1048576), (uint)(band * 32 + cy0 + b2 + 1048576), q.face ^ 0x50414E4Cu)).x);
                acc += w * (2.0 * h - 1.0); ws += w;
            }
        tone = 1.0 + NC_FaPanelTone * wt * acc / max(ws, 1e-6);
    }
    o.tone = tone * (1.0 + NC_FaFaceTone * (2.0 * NocU01(NocPcg(q.face ^ 0x41424344u)) - 1.0));

    // debug views 3 and 6, pane tilt weight
    float xc = q.u - floor(q.u / c.Pu) * c.Pu, yc = vl - floor(vl / c.Pv) * c.Pv;
    o.cellFx = xc / c.Pu; o.cellFy = yc / c.Pv;
    int ic = (int)floor(q.u / c.Pu), jcc = (int)floor(vl / c.Pv);
    o.allowMask = (NocBayHasWindow(ic, iF, iL, k0, k1) && jcc >= jF && jcc <= jL) ? 1.0 : 0.0;
    o.tiltW = smoothstep(NC_FaTiltFadePx, 2.0 * NC_FaTiltFadePx, min((c.ox1 - c.ox0) / fwu, winPx)) * wuv;

    // mode 1: the district's mechanical band and atrium over the window field (exact coverages)
    [branch] if (q.mode == 1)
    {
        NocSecMacroOutV2 m = NocSecMacroV2(q.face, q.u, q.v, fwu, fwv, q.wf);
        float k = saturate(1.0 - m.mech - m.atrium);
        o.emission = o.emission * k + m.em;
        o.aGlass = o.aGlass * k + m.atrium * m.atGlass; o.aOpen = o.aOpen * k + m.atrium; o.aFrame = o.aFrame * k + m.atrium * (1.0 - m.atGlass);
        o.aJamb *= k; o.aSillHead *= k; o.aSlab *= k; o.aLedge *= k; o.aSpandrel *= k;
        o.louver = m.mech;
    }
    return o;
}

// ---------------------------------------------------------------------------------------------------------------- macro extras
// Setback ledge line (30 % of towers by K): a lit line FaLedgeH tall at the top of each box (y1), drawn at least FaLedgeMinPx with
// its energy kept (TEXTURE_PLAN 2.5), WinNeutral at Y_med x FaLedgeGain. Needs y1.
float3 NocLedgeLineV2(NocSkinV2 s, float v, float fwv, float y0, float y1)
{
    if (!s.ledge || !NocKnownY(y0, y1)) return 0.0;
    float h = NC_FaLedgeH, d = max(h, NC_FaLedgeMinPx * fwv);
    return NocNormY(PAL_WinNeutral, NC_FaLedgeGain) * (NocCover(v, fwv, y1 - d, y1) * (h / d));
}

// v2 edge light at the face's vertical edges: at least FaEdgeMinPx wide with its energy kept, box-filtered over FaEdgeBlurPx pixels
// and set in from the corner by FaEdgeInsetPx pixels (a corner is a geometric silhouette: a bright band touching it shimmers with
// MSAA's 4 samples in pans against the supersampled reference; set in, the silhouette pixel keeps the wall).
float NocFacadeEdgeV2(float u, float uMin, float uMax, float fwu)
{
    float h = NC_FacadeEdgeHalf, d = max(h, NC_FaEdgeMinPx * fwu), fb = NC_FaEdgeBlurPx * fwu, e = NC_FaEdgeInsetPx * fwu;
    return (NocCover(u, fb, uMin + e, uMin + e + d) + NocCover(u, fb, uMax - e - d, uMax - e)) * (h / d);
}

// Display-space coverage of a thin HDR band (INT-05 / FA-D19, Phase 4 FX; C# twin NocFacadeMath.DisplayCover). A band of full-coverage
// radiance L covering c of a pixel is written as the radiance that the grade's Neutral highlight curve (identity below
// NC_FaToneStart; the exposure gain 2^EV, |EV| <= 0.2, taken as 1) displays as c x the band's displayed peak: the box of the
// -capture-ss 2 reference after the grade. Partly covered pixels of a band above the curve's start no longer clip to the peak
// (the stair-steps of the 3 px tower-corner lines that crawl in pans); c = 1 and bands below the start are unchanged.
float NocToneFwd(float x)
{
    float d = 1.0 - NC_FaToneStart;
    return x < NC_FaToneStart ? x : 1.0 - d * d / (x + d - NC_FaToneStart);
}
float NocToneInv(float y)
{
    float d = 1.0 - NC_FaToneStart;
    y = min(y, 0.995);
    return y < NC_FaToneStart ? y : NC_FaToneStart - d + d * d / (1.0 - y);
}
float3 NocDisplayCover(float3 L, float c)
{
    float pk = max(L.r, max(L.g, L.b));
    float v = NocToneInv(saturate(c) * NocToneFwd(pk));
    return L * (v / max(pk, 1e-6));
}

// The v2 edge light as radiance: the two corner bands' coverage (they never overlap: the face is wider than 2 (e + d)) at the band
// radiance colour x gain x h / d, written in display space (NocDisplayCover).
float3 NocFacadeEdgeLightV2(float3 colourGain, float u, float uMin, float uMax, float fwu)
{
    float h = NC_FacadeEdgeHalf, d = max(h, NC_FaEdgeMinPx * fwu), fb = NC_FaEdgeBlurPx * fwu, e = NC_FaEdgeInsetPx * fwu;
    float c = NocCover(u, fb, uMin + e, uMin + e + d) + NocCover(u, fb, uMax - e - d, uMax - e);
    return NocDisplayCover(colourGain * (h / d), c);
}

// Edge-light colours of LOOK_V2 4 (CityField's four kinds remapped; no magenta, no violet): 1 SignWhite; 2 NeonCyan on 60 %;
// 3 SodiumAmber x k_a else SignWhite; 4 SodiumAmber on 20 % x k_a else SignWhite.
float3 NocFacadeEdgeColourV2(int kind, uint face, float wf)
{
    float h = NocU01(NocPcg(face ^ 0x45444745u));
    float ka = saturate(wf / 0.32);
    if (kind == 2) return h < 0.6 ? PAL_NeonCyan : PAL_SignWhite;
    if (kind == 3) return h < ka ? PAL_SodiumAmber : PAL_SignWhite;
    if (kind == 4) return h < 0.2 * ka ? PAL_SodiumAmber : PAL_SignWhite;
    return PAL_SignWhite;
}

// ---------------------------------------------------------------------------------------------------------------- reflected skyline
// The open-city reflection of the glass (TEXTURE_PLAN 2.7, FA-2; replaces the city-glint lattice and, on the glass, the D-017 city
// term): the reflected ray at azimuth az / elevation el meets the shaft of a city tower. Azimuth is split into FaSkyBin-degree bins,
// each cut at hashed points into 1-3 towers; a tower of hashed width FaSkyW0..1 m filling its angular slot w stands at d = W / w
// (wide slots are near towers), so every tower's windows keep the city's 2.4 x 3.6 m cells. Every shaft rises into the cloud deck:
// its top is the deck (the body fades into the sky over NC_CloudMixY0..1 as the city's own cloud mix), so the skyline is a comb of
// fogged shafts, never a ridge. Body = the unresolved facade mean (lit glass FaSkyGlass at the LOOK_V2 hue mix, stand-by, dark
// glass and ConcreteDark under the ambient) with window speckle on the tower's own (x, y) cell grid (hashed phase per tower, no
// global lattice), faded to the mean by the H2 fade of the cell in blur footprints (S1, S6); then the fog of that distance toward
// the sky along the ray. Edges: the vertical edge to the neighbour slot and the speckle are box-filtered over the blur.
// Returns the tower coverage of the ray (0: sky) and the tower radiance Lb (fogged).
struct NocSkyBldV2 { float a0, a1, d; uint key; };

NocSkyBldV2 NocSkySlotV2(float bin, int k, int n, float u1, float u2)
{
    NocSkyBldV2 b;
    float c1 = n == 2 ? 0.3 + 0.4 * u1 : 0.2 + 0.25 * u1, c2 = 0.55 + 0.25 * u2;
    float lo = k == 0 ? 0.0 : (k == 1 ? c1 : c2), hi = k == n - 1 ? 1.0 : (k == 0 ? c1 : c2);
    b.a0 = (bin + lo) * NC_FaSkyBin; b.a1 = (bin + hi) * NC_FaSkyBin;
    uint binU = (uint)(bin + 1024.0);
    b.key = NocH3x(binU, (uint)k, 0x534B594Cu);
    float Wm = NC_FaSkyW0 + (NC_FaSkyW1 - NC_FaSkyW0) * NocU01(b.key);
    b.d = Wm / max(radians(b.a1 - b.a0), 1e-4);
    return b;
}

// The slot under azimuth az (deg, any range) and its nearer neighbour across the closer edge.
void NocSkyFindV2(float az, out NocSkyBldV2 b, out NocSkyBldV2 nb)
{
    float bin = floor(az / NC_FaSkyBin);
    float f = az / NC_FaSkyBin - bin;
    uint h = NocPcg((uint)(bin + 1024.0) ^ 0x534B5942u);
    int n = 1 + (int)floor(3.0 * NocU01(h));
    float u1 = NocU01(NocPcg(h ^ 0x31u)), u2 = NocU01(NocPcg(h ^ 0x32u));
    float c1 = n == 2 ? 0.3 + 0.4 * u1 : 0.2 + 0.25 * u1, c2 = 0.55 + 0.25 * u2;
    int k = n == 1 ? 0 : (f < c1 ? 0 : ((n == 3 && f >= c2) ? 2 : 1));
    b = NocSkySlotV2(bin, k, n, u1, u2);
    bool left = az - b.a0 < b.a1 - az;
    if (left && k > 0) nb = NocSkySlotV2(bin, k - 1, n, u1, u2);
    else if (!left && k < n - 1) nb = NocSkySlotV2(bin, k + 1, n, u1, u2);
    else
    {
        float bin2 = left ? bin - 1.0 : bin + 1.0;
        uint h2 = NocPcg((uint)(bin2 + 1024.0) ^ 0x534B5942u);
        int n2 = 1 + (int)floor(3.0 * NocU01(h2));
        float v1 = NocU01(NocPcg(h2 ^ 0x31u)), v2 = NocU01(NocPcg(h2 ^ 0x32u));
        nb = NocSkySlotV2(bin2, left ? n2 - 1 : 0, n2, v1, v2);
    }
}

// Radiance of tower b along the ray (az, el) from height yP, before the cloud fade and fog; blurRad: the reflection's footprint.
float3 NocSkyBodyV2(NocSkyBldV2 b, float az, float el, float yP, float blurRad, float3 amb, float wf, float nf)
{
    float3 eLitMean = NocMeanHueV2(wf, nf) * NC_FaBMean;
    float3 eUn = PAL_WinNeutral * NC_FaStandby + PAL_GlassDark * amb * 0.96;
    float3 wall = PAL_ConcreteDark * amb * 0.96;
    float3 M = (eLitMean * NC_FaLit + eUn * (1.0 - NC_FaLit)) * NC_FaSkyGlass + wall * (1.0 - NC_FaSkyGlass);
    // the tower's cell grid in metres: x along its face from the slot's left edge, y world height
    float ce = max(cos(radians(el)), 0.05);
    float x = b.d * radians(az - b.a0) + 2.4 * NocU01(NocPcg(b.key ^ 0x50484158u));
    float y = yP + b.d * tan(radians(el));
    float fx = b.d * blurRad, fy = b.d * blurRad / (ce * ce);
    float ws = smoothstep(NC_FaFadePx0, NC_FaFadePx1, min(NC_FacadeColumn / max(fx, 1e-4), NC_FacadeFloor / max(fy, 1e-4)));
    float3 L = M;
    [branch] if (ws > 0.0)
    {
        float ix = floor(x / NC_FacadeColumn), iy = floor(y / NC_FacadeFloor);
        float lx = x - ix * NC_FacadeColumn, ly = y - iy * NC_FacadeFloor;
        uint3 hc = NocPcg3d(uint3((uint)(ix + 1048576.0), (uint)(iy + 1048576.0), b.key));
        float lit = NocU01(hc.x) < NC_FaLit ? 1.0 : 0.0;
        float3 Ew = NocHueV2(NocU01(hc.y), wf, nf) * NocBrightnessV2(hc.z);
        float cw = NocCover(lx, fx, NC_FaSkyWinX0, NC_FaSkyWinX1) * NocCover(ly, fy, NC_FaSkyWinY0, NC_FaSkyWinY1);
        float3 Lr = wall * (1.0 - cw) + cw * (lit * Ew + (1.0 - lit) * eUn) * (NC_FaSkyGlass / ((NC_FaSkyWinX1 - NC_FaSkyWinX0) * (NC_FaSkyWinY1 - NC_FaSkyWinY0) / (NC_FacadeColumn * NC_FacadeFloor)));
        L = lerp(M, Lr, ws);
    }
    return L;
}

float NocSkylineV2(float3 r, float3 posWS, float blurRad, float3 amb, float wf, float nf, float3 skyR, out float3 Lb)
{
    float az = degrees(atan2(r.x, r.z)) + 360.0;
    float el = degrees(asin(clamp(r.y, -0.999, 0.999)));
    float bd = max(degrees(blurRad), 1e-3);
    NocSkyBldV2 b, nb;
    NocSkyFindV2(az, b, nb);
    // the coverage of the slot under the centre over the blur, the rest to the neighbour
    float cb = NocCover(az, bd, b.a0, b.a1);
    float3 L = NocSkyBodyV2(b, az, el, posWS.y, blurRad, amb, wf, nf) * cb;
    float dM = b.d * cb;
    [branch] if (cb < 1.0) { L += NocSkyBodyV2(nb, az, el, posWS.y, blurRad, amb, wf, nf) * (1.0 - cb); dM += nb.d * (1.0 - cb); }
    // the shaft enters the cloud deck: the body fades into the sky over the city's cloud-mix band (at the slot's mean distance)
    float yHit = posWS.y + dM * tan(radians(el));
    float wc = smoothstep(NC_CloudMixY0, NC_CloudMixY1, yHit);
    float tau = _NocFog.x * 0.5 * (1.0 + NocDown(min(yHit, 0.0))) * dM;      // the height factor averaged along the ray (NocOpticalDepthS)
    float T = exp(-tau * tau);
    Lb = L * T + skyR * (1.0 - T);
    return (1.0 - wc) * (el < 89.0 ? 1.0 : 0.0);
}

#endif
