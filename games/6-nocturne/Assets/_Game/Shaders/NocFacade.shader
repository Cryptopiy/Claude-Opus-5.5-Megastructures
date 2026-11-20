// NocFacade - every tower shaft of the city (MASTER_PROMPT 14.2.5, 14.7 H2, D-004): ConcreteDark wall through the Noc lit
// template (URP BRDFData, analytic sky reflection, Forward+ lights, trilight x LightCal, specular AA), analytic windows as emission
// (Include/NocFacade.hlsl: strata, zone level, stateless epochs, per-axis 2.5-5.5 px footprint fade with the exact 2 x 2 box
// filter), dark glass where the window shape covers the pixel (smoother: it reflects the sky), vertical edge lights at the face
// edges (>= 2 px, energy kept), then NocFog (the cloud mix at the shaft tops, fog toward NocSkyColor) and NocFloor last.
// Vertex contract (CityMesher, any builder): uv0 = (u metres along the face, 16-bit face id as float: it survives perspective interpolation), uv1 = (uMin, uMax) of
// the face, colour R = AO, G = tower seed, B = mode (0 facade, 0.5 section, 1 roof), A = edge-light kind x 63/255 (0 none,
// 1 cyan, 2 magenta, 3 white, 4 violet). Globals: _NocFacade (epoch phase, warm fraction) from the AtmosphereDriver.
// Look v2 (TEXTURE_PLAN 2, LOOK_V2 4, docs/FACADE_V2.md; FA, then FX): Include/NocFacadeSkin.hlsl, the only path since TP-2
// (FX, Phase 4 wave A: the v1 window model, its flag -facade-v and the v1 properties' use are deleted; the properties stay so
// the material layout does not move).
// Vertex contract v2: uv1 = (uMin, uMax, y0, y1) (NOC_PASS_UV1ZW: y0, y1 in custom.zw; y0 = y1 = 0 unknown), colour G = the skin
// byte (NocFacadeMath.SkinByte). The surface hook decomposes the pixel into wall, slab, ledge, frame, jamb, sill / head, spandrel
// glass and clear glass by exact filtered coverage; the opaque part goes through the template (its specular x the city term in
// f.specOcc, D-017, so far walls keep their ambient), the glass part's reflection (F0 0.04, roughness 0.05, a +-1.5 deg tilt per
// pane folded into roughness below 2 px) is added by the forward hook.
Shader "Megastructure/NocFacade"
{
    Properties
    {
        _BaseColor ("Wall albedo (linear)", Color) = (0.0232, 0.0232, 0.0395, 1)
        _WindowGain ("Lit window gain (x hue token)", Float) = 0.3
        _EdgeGain ("Edge light gain", Float) = 1.0
        _WallSmooth ("Wall smoothness", Range(0, 0.95)) = 0.35
        _GlassSmooth ("Glass smoothness", Range(0, 0.95)) = 0.85
        _GlassDark ("Glass albedo factor", Range(0, 1)) = 0.5
        _MacroAlbedo ("Per-face albedo +-", Range(0, 0.5)) = 0.15
        _WinGainV2 ("Look v2: lit window gain (x LOOK_V2 4 radiance)", Float) = 1.0
        // Look v2 photo skins (FA-4; MaterialFactory.Facade wires FacadeArr and the slice tiles from its texmeta; _FaTexOn 0 = analytic)
        [NoScaleOffset] _FacadeAlb ("v2 skins albedo variation (array, sRGB, mean 0.5)", 2DArray) = "" {}
        [NoScaleOffset] _FacadeNrm ("v2 skins normal (array, BC5)", 2DArray) = "" {}
        [NoScaleOffset] _FacadeMask ("v2 skins mask (array: R metal, G AO, B height, A smoothness)", 2DArray) = "" {}
        _FaTexOn ("v2 skins bound (1) or analytic (0)", Float) = 0
        _FaTile0 ("slice 0 tile (m)", Float) = 3.0
        _FaTile1 ("slice 1 tile (m)", Float) = 1.543
        _FaTile2 ("slice 2 tile (m)", Float) = 2.0
        _FaTile3 ("slice 3 tile (m)", Float) = 2.8
        _FaTile4 ("slice 4 tile (m)", Float) = 1.95
        _FaTile5 ("slice 5 tile (m)", Float) = 1.846
        _FaTile6 ("slice 6 tile (m)", Float) = 3.2
        _FaTile7 ("slice 7 tile (m)", Float) = 2.4
        [NoScaleOffset] _OverlayMask ("v2 OverlayArr mask (TP: R coverage, A smoothness)", 2DArray) = "" {}
        _FaStreakOn ("v2 sill streaks bound (1)", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        // TEXTURE_PLAN 2.12: with TP's NocFacadePrepassFeature the forward pass runs ZTest Equal, ZWrite Off (set on the Facade row)
        [Enum(Off, 0, On, 1)] _FacadeZWrite ("Forward ZWrite (prepass: Off)", Float) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _FacadeZTest ("Forward ZTest (prepass: Equal)", Float) = 4
    }

    HLSLINCLUDE
    #define NOC_PASS_UV1ZW
    #include "Include/NocLitCore.hlsl"
    #include "Include/NocMirror.hlsl"          // read-only (SU): NocWallReflect, the canyon and Gulf walls in the glass (FA-2)
    #include "Include/NocFacadeSkin.hlsl"
    #include "Include/NocTexturing.hlsl"          // read-only (TP): fade, last mip, slope / surface-gradient helpers

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float _WindowGain;
        float _EdgeGain;
        float _WallSmooth;
        float _GlassSmooth;
        float _GlassDark;
        float _MacroAlbedo;
        float _WinGainV2;
        float _Cull;
        float _FacadeZWrite;
        float _FacadeZTest;
        float _FaTexOn;
        float _FaTile0, _FaTile1, _FaTile2, _FaTile3, _FaTile4, _FaTile5, _FaTile6, _FaTile7;
        float _FaStreakOn;
    CBUFFER_END

    // one sampler for the three skin arrays (aniso 16 from the albedo's import settings): 1 of the 8 per stage
    TEXTURE2D_ARRAY(_FacadeAlb); SAMPLER(sampler_FacadeAlb);
    TEXTURE2D_ARRAY(_FacadeNrm);
    TEXTURE2D_ARRAY(_FacadeMask);
    TEXTURE2D_ARRAY(_OverlayMask); SAMPLER(sampler_OverlayMask);   // TP's OverlayArr (slice 0 leaks): 2 of the 8 samplers

    // Sill streak coverage under the opening of the pixel's cell (cell-local x, y in metres; dx / dy the screen derivatives of the
    // face coordinates): the leak decal from the sill down, faded to its mean profile below 2-4 px of the 2 m decal tile.
    float NocFaStreak(float cxl, float cyl, float2 du, float2 dv, float fwu, NocCellV2 c, uint ph, float allow)
    {
        float dTop = c.oy0 - cyl;                                     // metres below the sill
        float len = NC_FaStreakLen * lerp(1.0, NC_FaStreakStretch, NocU01(NocPcg(ph ^ 0x5354524Bu)));
        // one return (INT): the early return inside [branch] drew fxc X4000 "potentially uninitialized variable (NocFaStreak)"
        float res = 0.0;
        [branch] if (!(allow < 0.5 || dTop < 0.0 || dTop > len))
        {
            float across = NocCover(cxl, fwu, c.ox0, c.ox1);
            float off = NocU01(NocPcg(ph ^ 0x4C45414Bu));
            float2 uv = float2((cxl - c.ox0) / 2.0 + off, max(0.25, 1.0 - 0.5 * dTop / len));
            float2 gx = float2(du.x / 2.0, -0.5 * dv.x / len), gy = float2(du.y / 2.0, -0.5 * dv.y / len);
            float fade = NocTexFade(1.0, NocFootprintBox(gx, gy));
            float cov = 2.0 * NC_FaStreakMean * saturate(1.0 - dTop / len);
            [branch] if (fade > 0.0) cov = lerp(cov, SAMPLE_TEXTURE2D_ARRAY_GRAD(_OverlayMask, sampler_OverlayMask, uv, 0, gx, gy).r, fade);
            res = cov * across;
        }
        return res;
    }

    float NocFaSliceTile(float k)
    {
        return k < 0.5 ? _FaTile0 : (k < 1.5 ? _FaTile1 : (k < 2.5 ? _FaTile2 : (k < 3.5 ? _FaTile3
             : (k < 4.5 ? _FaTile4 : (k < 5.5 ? _FaTile5 : (k < 6.5 ? _FaTile6 : _FaTile7))))));
    }

    // The tower's photo slice (constants: FaS1WeatheredP).
    float NocFaSkinSlice(NocSkinV2 sk, int mode)
    {
        if (mode == 2) return 0.0;
        if (mode == 1) return 1.0;
        if (sk.id == NOC_SK_S0) return 2.0;
        if (sk.id == NOC_SK_S2) return sk.s2Ribbed ? 3.0 : 2.0;
        if (sk.id == NOC_SK_S3) return 5.0;
        if (sk.id == NOC_SK_S4) return 4.0;
        return NocU01(NocPcg(sk.key ^ 0x57454154u)) < NC_FaS1WeatheredP ? 1.0 : 0.0;
    }

    // Panel offset (in tiles) of cell (i, j): whole registered periods per axis (courses, lifts, ribs, modules: the seam between two
    // panels falls between whole courses), free where the slice has no registration, none along u for brick / tile courses (their
    // units do not divide the 2.4 m bay: the pattern runs on across the bays).
    float2 NocFaPanelOffset(float k, uint ph)
    {
        uint2 h = NocPcg3d(uint3(ph, 0x50414E4Cu, 0x9E3779B9u)).xy;
        float ux = NocU01(h.x), uy = NocU01(h.y);
        float qx = k == 3.0 ? 28.0 : (k == 7.0 ? 5.0 : 0.0);
        float qy = k == 1.0 ? 6.0 : (k == 4.0 ? 13.0 : (k == 5.0 ? 20.0 : (k == 7.0 ? 5.0 : 0.0)));
        bool freeU = k == 0.0 || k == 1.0 || k == 2.0 || k == 6.0;
        float ox = qx > 0.0 ? floor(ux * qx) / qx : (freeU ? ux : 0.0);
        float oy = qy > 0.0 ? floor(uy * qy) / qy : uy;
        return float2(ox, oy);
    }

    // ---------------------------------------------------------------------------------------------------- Look v2 surface
    void NocFacadeSurfaceV2(inout NocFragData f, inout SurfaceData s, NocFrame fr, float2 du, float2 dv)
    {
        // per-face extents snapped to 1 / 16 m: interpolation noise of these constants never flips a bay / row decision (TQ-09)
        float uMin = round(f.uv.z * NC_FaExtentQ) / NC_FaExtentQ, uMax = round(f.uv.w * NC_FaExtentQ) / NC_FaExtentQ;
        NocFaceInV2 q;
        q.u = clamp(f.uv.x, uMin, uMax);                 // a pixel centre past a sliver never lights a window past the corner
        q.v = f.positionWS.y;
        q.fwu = abs(du.x) + abs(du.y);                   // bounding-box footprints (TEXTURE_PLAN 2.9; CRIT0 #19)
        q.fwv = abs(dv.x) + abs(dv.y);
        q.face = (uint)(f.uv.y + 0.5);
        q.skinByte = (uint)round(saturate(f.color.g) * 255.0);
        q.mode = (int)round(f.color.b * 2.0);
        q.uMin = uMin; q.uMax = uMax; q.y0 = round(f.custom.z * NC_FaExtentQ) / NC_FaExtentQ; q.y1 = round(f.custom.w * NC_FaExtentQ) / NC_FaExtentQ;
        float3 n = f.normalWS;
        // the eye-to-surface ray in the facade frame (T, B, n), dn > 0 into the wall: the recess shift per metre of depth
        float3 d = -f.viewDirWS;
        float dn = max(dot(d, -n), 1e-3);
        q.kx = dot(d, fr.T) / dn;
        q.ky = dot(d, fr.B) / dn * fr.B.y;
        q.tEpoch = _NocFacade.x; q.wf = _NocFacade.y; q.nf = _NocFacade.w;
        q.forceW = -1.0;
        float fwu = max(q.fwu, 1e-4), fwv = max(q.fwv, 1e-4);
        bool known = NocKnownY(q.y0, q.y1);
        int edgeKind = (int)round(f.color.a * 255.0 / 63.0);

        NocFaceOutV2 o = (NocFaceOutV2)0;
        o.skin = NocSkinOfV2(q.skinByte, q.mode);
        o.cell = NocCellOfV2(o.skin, 0);
        o.tone = 1.0;
        [branch] if (q.mode != 2) o = NocFacadeEvalV2(q);
        NocSkinV2 sk = o.skin;
        NocCellV2 c = o.cell;
        float3 wallTok = NocWallTokenV2(sk, o.type);

        // macro and trim elements on the wall share (exact interval coverages)
        float bandH = q.mode == 1 ? NC_FacadeSecBand : NC_FacadeBand;
        float vl = q.v - floor(q.v / bandH) * bandH;
        float louv = (q.mode == 0 && o.type == NOC_ST_RES && NocMechStripeV2(sk.key, o.band)) ? NocCover(vl, fwv, (o.rows - 2) * c.Pv, o.rows * c.Pv) : 0.0;
        float baseB = (known && q.y0 <= NC_FaBaseY0) ? NocCover(q.v, fwv, q.y0, q.y0 + NC_FaBase) : 0.0;
        float coping = known ? NocCover(q.v, fwv, q.y1 - NC_FaCoping, q.y1) : 0.0;
        float trim = q.mode == 0 ? NocCover(q.u, fwu, uMin, uMin + NC_FaCornerTrim) + NocCover(q.u, fwu, uMax - NC_FaCornerTrim, uMax) : 0.0;
        float joint = 0.0;
        if (sk.id == NOC_SK_S1 || q.mode == 1)
            joint = max(NocFilteredGrid1(q.u, c.Pu, NC_FaJoint, fwu, 4.0), NocFilteredGrid1(vl, c.Pv, NC_FaJoint, fwv, 4.0));
        else if (sk.id == NOC_SK_S2)
            joint = NocFilteredGrid1(q.u, 0.5 * c.Pu, NC_FaJoint, fwu, 4.0);

        float aRefl = o.aGlass + o.aSpandrel;
        // mechanical strata: louvre blades fill the slit (filtered bars, mean below 4 px); the slit light shows between them
        float slitBl = (o.type == NOC_ST_MECH && q.mode == 0) ? NocFilteredGrid1(q.v, NC_FaLouvreP, NC_FaLouvreW, fwv, 4.0) * o.aGlass : 0.0;
        aRefl -= slitBl;
        o.emission *= 1.0 - (o.type == NOC_ST_MECH && q.mode == 0 ? NocFilteredGrid1(q.v, NC_FaLouvreP, NC_FaLouvreW, fwv, 4.0) : 0.0);
        float wr = max(0.0, 1.0 - o.aOpen - o.aSpandrel);                 // the wall share of the pixel
        louv = saturate(louv + o.louver / max(wr, 1e-4));                   // + the section mechanical band (mode 1)
        float blades = NocFilteredGrid1(q.v, NC_FaLouvreP, NC_FaLouvreW, fwv, 4.0);   // louvre blades, mean below 4 px
        float fSlab = saturate(o.aSlab / max(wr, 1e-4)), fLedge = saturate(o.aLedge / max(wr, 1e-4));
        // photo skin (TEXTURE_PLAN 1.6 / 2.3): the tower's slice, cell-aligned panel shuffle, SampleGrad with the true gradients of
        // (u, v) / tile, every map faded to its last mip by w_tex = smoothstep(2, 4, tile px); none when the arrays are not bound
        float3 texAlb = 1.0; float texSm = -1.0, texAo = 1.0; float2 texSlope = 0.0;
        [branch] if (_FaTexOn > 0.5)
        {
            float k = NocFaSkinSlice(sk, q.mode), tile = NocFaSliceTile(k);
            float ci = floor(q.u / c.Pu), cj = floor(q.v / c.Pv);
            float2 cl = float2(q.u - ci * c.Pu, q.v - cj * c.Pv);
            uint ph = NocPcg3d(uint3((uint)(ci + 1048576.0), (uint)(cj + 1048576.0), sk.key ^ q.face)).x;
            float2 uvT = cl / tile + NocFaPanelOffset(k, ph);
            float2 gx = float2(du.x, dv.x) / tile, gy = float2(du.y, dv.y) / tile;
            float wt = NocTexFade(tile, max(fwu, fwv));
            float4 a4 = NocSampleFadeArr(TEXTURE2D_ARRAY_ARGS(_FacadeAlb, sampler_FacadeAlb), uvT, k, gx, gy, wt);
            float4 m4 = NocSampleFadeArr(TEXTURE2D_ARRAY_ARGS(_FacadeMask, sampler_FacadeAlb), uvT, k, gx, gy, wt);
            texAlb = 2.0 * a4.rgb;
            texSm = saturate(m4.a + NC_FaWetLift);
            texAo = lerp(1.0, m4.g, NC_FaTexAo);
            [branch] if (wt > 0.0)
                texSlope = NocTangentToSlope(SAMPLE_TEXTURE2D_ARRAY_GRAD(_FacadeNrm, sampler_FacadeAlb, uvT, k, gx, gy), 1.0) * wt;
        }
        // sill streaks under the openings (residential / commercial window cells of the pixel's row; mean profile at distance)
        float streak = 0.0;
        [branch] if (_FaStreakOn > 0.5 && q.mode == 0 && (o.type == NOC_ST_RES || o.type == NOC_ST_COM))
        {
            float cxl = q.u - floor(q.u / c.Pu) * c.Pu, cyl = vl - floor(vl / c.Pv) * c.Pv;
            uint phs = NocPcg3d(uint3((uint)(floor(q.u / c.Pu) + 1048576.0), (uint)(floor(q.v / c.Pv) + 1048576.0), q.face)).y;
            streak = NocFaStreak(cxl, cyl, du, dv, fwu, c, phs, o.allowMask) * saturate(o.wu * o.wv);   // resolved windows only (the mean darkening at distance is under 0.5 %)
        }
        // wall albedo: the token x the photo skin x per-panel / per-face tone x the joints x the streaks, then the slab band, louvres,
        // base band, trim, coping
        float3 albW = wallTok * texAlb * o.tone * (1.0 - (1.0 - NC_FaJointDark) * joint) * (1.0 - NC_FaStreakDark * streak);
        albW = lerp(albW, wallTok * texAlb * 1.1, fSlab);
        albW = lerp(albW, PAL_MetalPaint * (NC_FaLouvreAlb * (1.0 - 0.5 * blades)), louv);
        albW = lerp(albW, PAL_Granite, baseB * ((sk.stoneBase || sk.id == NOC_SK_S1) ? 1.0 : 0.6));
        albW = lerp(albW, PAL_MetalPaint, saturate(trim));
        albW = lerp(albW, wallTok * 1.15, coping);
        // skin details (TEXTURE_PLAN 2.3): S2 ribbed panels (zero-mean albedo ribs), S3 stone sills and lintels around the windows
        // (cell-local bands, blended to their mean area below the window fade)
        [branch] if (sk.id == NOC_SK_S2 && sk.s2Ribbed && q.mode == 0 && _FaTexOn < 0.5)     // the ribbed slice carries real ribs
            albW *= 1.0 + NC_FaRibTone * (NocFilteredGrid1(q.u, NC_FaRibP, NC_FaRibW, fwu, 4.0) - NC_FaRibW / NC_FaRibP);
        [branch] if (sk.id == NOC_SK_S3 && o.type == NOC_ST_RES && q.mode == 0)
        {
            float cxl = q.u - floor(q.u / c.Pu) * c.Pu, cyl = vl - floor(vl / c.Pv) * c.Pv;
            float sx0 = c.ox0 - NC_FaS3StoneX, sx1 = c.ox1 + NC_FaS3StoneX;
            float stoneR = NocCover(cxl, fwu, sx0, sx1) * (NocCover(cyl, fwv, c.oy0 - NC_FaS3Stone, c.oy0) + NocCover(cyl, fwv, c.oy1, c.oy1 + NC_FaS3Stone)) * o.allowMask;
            float stoneM = (sx1 - sx0) * 2.0 * NC_FaS3Stone / (c.Pu * c.Pv);
            albW = lerp(albW, PAL_Granite, saturate(lerp(stoneM, stoneR, saturate(o.wu * o.wv)) / max(wr, 1e-4)));
        }
        float smW = texSm >= 0.0 ? texSm : ((sk.id == NOC_SK_S0 || sk.id == NOC_SK_S2) ? NC_FaSmoothMetal : NC_FaSmoothWall);
        smW = saturate(smW + NC_FaStreakSmooth * streak);                   // the streaks are the wettest part of the wall
        smW = lerp(smW, NC_FaSmoothSill, saturate(fLedge + coping));      // wet up-facing ledges and the coping
        smW = lerp(smW, NC_FaSmoothFrame, saturate(trim));
        float sill = q.ky < 0.0 ? 1.0 : 0.0;                                // the sill top is the visible reveal when looking down
        float3 alb = wr * albW + (o.aJamb + o.aSillHead) * wallTok * texAlb + (o.aFrame + slitBl) * PAL_MetalPaint + aRefl * PAL_GlassDark;
        float smO = wr * smW + o.aJamb * NC_FaSmoothWall + o.aSillHead * lerp(NC_FaSmoothWall, NC_FaSmoothSill, sill) + (o.aFrame + slitBl) * NC_FaSmoothFrame;
        float aO = max(1.0 - aRefl, 1e-3);
        float ao = wr * texAo * lerp(1.0, NC_FaAoSlab, fSlab) * lerp(1.0, NC_FaAoLouvre, louv) + (o.aJamb + o.aSillHead) * NC_FaAoReveal + (o.aFrame + slitBl) * NC_FaAoFrame + aRefl;

        // shading normal: reveals face the opening centre (+-T jambs, +B sill, -B head), ledges and the coping face up
        float jambSign = q.kx > 0.0 ? -1.0 : 1.0;
        float3 nW = NocResolveNormal(n, NocSurfGradTB(texSlope * saturate(wr), fr.T, fr.B));   // the skin's relief on the wall share
        float3 nS = nW * max(0.0, 1.0 - o.aJamb - o.aSillHead - wr * fLedge - coping)
                  + fr.T * (jambSign * o.aJamb) + fr.B * ((q.ky < 0.0 ? 1.0 : -1.0) * o.aSillHead + wr * fLedge + coping);
        f.normalWS = normalize(nS + n * 1e-3);

        // emission: windows, room spill and halo, edge lights, the setback ledge line
        float3 emit = o.emission * _WinGainV2;
        [branch] if (edgeKind > 0 && q.mode == 0)
            emit += NocFacadeEdgeLightV2(NocFacadeEdgeColourV2(edgeKind, q.face, q.wf) * (NC_NeonGain * _EdgeGain), f.uv.x, uMin, uMax, fwu);   // display-space coverage (INT-05)
        emit += NocLedgeLineV2(sk, q.v, fwv, q.y0, q.y1);

        // hero details: the spire's S0 fins in front of everything (filtered bars, energy kept); the screen-wall mass's LED strips
        [branch] if (sk.fins && q.mode == 0)
        {
            float fin = NocFilteredGrid1(q.u, c.mP, NC_FaFinW + abs(q.kx) * NC_FaFinD, fwu, 4.0);
            alb = alb * (1.0 - fin) + PAL_MetalPaint * (NC_FaFinAlb * fin);
            smO = smO * (1.0 - fin) + NC_FaSmoothFrame * fin;
            ao = ao * (1.0 - fin) + fin;
            emit *= 1.0 - fin;
            aRefl *= 1.0 - fin;
            aO = max(1.0 - aRefl, 1e-3);
        }
        [branch] if (sk.led && q.mode == 0)
        {
            float dl = max(NC_FaLedH, fwv), r0 = floor((q.v - 0.5 * fwv) / c.Pv);
            [unroll] for (int t = 0; t < 2; t++)
            {
                float rA = r0 + t, yl = rA * c.Pv + 0.5 * c.slab;
                uint3 hl = NocPcg3d(uint3((uint)(rA + 1048576.0), q.face, 0x4C454453u));
                float lit = NocU01(hl.x) < NC_FaLedLit ? 1.0 : 0.0;
                float ut = NocU01(hl.y);
                float3 tok = ut < 0.5 ? PAL_SignWhite : (ut < 0.8 ? PAL_WinNeutral : PAL_WinCold);
                emit += NocNormY(tok, NC_FaLedGain) * (lit * NocCover(q.v, fwv, yl - 0.5 * dl, yl + 0.5 * dl) * (NC_FaLedH / dl));
                float house = NocCover(q.v, fwv, yl - 0.5 * NC_FaLedHousing, yl + 0.5 * NC_FaLedHousing) * wr;
                alb = lerp(alb, PAL_MetalPaint * 0.5, house);
            }
        }

        // S4 loggias: the top rail and the balusters stand at the facade plane, in front of the loggia (exact coverages, blended to
        // their mean area below the window fade)
        [branch] if (c.loggia > 0.5)
        {
            float cxl = q.u - floor(q.u / c.Pu) * c.Pu, cyl = vl - floor(vl / c.Pv) * c.Pv;
            float inX = NocCover(cxl, fwu, c.ox0, c.ox1) * o.allowMask;
            float yr = c.slab + NC_FaLogRailY;
            float railR = inX * (NocCover(cyl, fwv, yr - 0.5 * NC_FaLogRailW, yr + 0.5 * NC_FaLogRailW)
                        + NocCover(cyl, fwv, c.slab, yr - 0.5 * NC_FaLogRailW) * NocFilteredGrid1(q.u, NC_FaLogBalP, NC_FaLogBalW, fwu, 4.0));
            float railM = (c.ox1 - c.ox0) / c.Pu * (NC_FaLogRailW + (NC_FaLogRailY - 0.5 * NC_FaLogRailW) * NC_FaLogBalW / NC_FaLogBalP) / c.Pv;
            float rail = saturate(lerp(railM, railR, saturate(o.wu * o.wv)));
            alb = alb * (1.0 - rail) + PAL_MetalPaint * rail;
            smO = smO * (1.0 - rail) + NC_FaSmoothFrame * rail;
            ao = ao * (1.0 - rail) + NC_FaAoFrame * rail;
            emit *= 1.0 - rail;
            aRefl *= 1.0 - rail;
            aO = max(1.0 - aRefl, 1e-3);
        }

        s.albedo = alb;
        s.metallic = 0.0;
        s.smoothness = smO / aO;
        s.occlusion = f.color.r * ao;
        s.emission = emit;
        // the city term (D-017) on the opaque specular only; the glass reflection is the forward hook's (custom.x / y, tangentWS)
        float elev = -f.viewDirWS.y;
        f.specOcc = lerp(NC_FacadeOccMin, 1.0, smoothstep(NC_FacadeOccSin0, NC_FacadeOccSin1, elev)) * aO;
        float tilt = radians(NC_FaPaneTilt) * o.tiltW;
        f.tangentWS = float4(normalize(n + (fr.T * o.tiltA + fr.B * o.tiltB) * tilt), 0.0);
        float aG = NC_FaGlassRough * NC_FaGlassRough;
        float aG2 = sqrt(aG * aG + (1.0 - o.tiltW) * (radians(NC_FaPaneTilt) * radians(NC_FaPaneTilt) / 3.0));
        f.custom.x = aRefl;
        f.custom.y = 1.0 - sqrt(aG2);

        // debug views (TEXTURE_PLAN 7.1): display codes / 255
        int dvw = NocDebugView();
        [branch] if (dvw > 0)
        {
            float3 dc = 0.0;
            float Wo = max(c.ox1 - c.ox0, 1e-3), Ho = max(c.oy1 - c.oy0, 1e-3);
            if (dvw == 1) dc = float3(sk.id * 40.0, sk.region * 128.0, sk.overrideK >= 0 ? 16.0 * (sk.overrideK + 1) : 0.0) / 255.0;
            else if (dvw == 2) dc = float3(o.wu * o.wv, 0.0, 0.0);
            else if (dvw == 3) dc = float3(o.cellFx, o.cellFy, o.allowMask);
            else if (dvw == 4) dc = fr.sigma < 0.0 ? float3(1.0, 0.0, 0.0) : float3(0.0, 1.0, 0.0);
            else if (dvw == 5) dc = float3(32.0 * (log2(c.Pu / fwu) + 2.0), 32.0 * (log2(c.Pv / fwv) + 2.0), 0.0) / 255.0;
            else if (dvw == 6) dc = float3(128.0 + 64.0 * c.D * q.kx / Wo, 128.0 + 64.0 * c.D * q.ky / Ho, 0.0) / 255.0;
            else if (dvw == 10) dc = float3(saturate(o.pEff), (float)(sk.key & 0xFFu) / 255.0, 0.0);
            f.debug = float4(saturate(dc), (dvw == 7 || dvw == 8 || dvw == 9) ? 0.0 : 1.0);
        }
    }

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        float uRaw = f.uv.x;
        float v = f.positionWS.y;
        // derivatives first, outside any branch (both paths): face coordinates and the per-pixel facade frame (sigma)
        float2 du = float2(ddx(uRaw), ddy(uRaw)), dv = float2(ddx(v), ddy(v));
        NocFrame fr = NocFaceFrame(f.normalWS, uRaw, f.positionWS);
        NocFacadeSurfaceV2(f, s, fr, du, dv);                     // TP-2: Look v2 is the only path
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite [_FacadeZWrite] ZTest [_FacadeZTest] Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #define NOC_PASS_FORWARD
            #define NOC_HAS_FORWARD_HOOK
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Include/NocLighting.hlsl"
            // The opaque part with the city term in f.specOcc, plus the glass
            // reflection of the glass share (custom.x) along the pane-tilted normal (tangentWS.xyz) at smoothness custom.y: the sky
            // (NocReflection) behind the reflected city skyline (NocSkylineV2: fogged shafts with window speckle, rising into the
            // deck) behind the registered canyon / Gulf walls (SU's NocWallReflect); this replaces the D-017 city term on the glass
            // (TEXTURE_PLAN 2.7, FA-2).
            half3 NocForwardHook(NocFragData f, SurfaceData s, InputData inputData)
            {
                half3 c = NocShadeOcc(inputData, s, (half)f.specOcc);
                [branch] if (f.custom.x > 0.0)
                {
                    float3 ng = normalize(f.tangentWS.xyz);
                    SurfaceData g = s;
                    g.albedo = PAL_GlassDark; g.metallic = 0.0h; g.specular = half3(0, 0, 0); g.smoothness = (half)f.custom.y;
                    BRDFData bg;
                    InitializeBRDFData(g, bg);
                    float3 V = inputData.viewDirectionWS;
                    half NoV = saturate(dot(ng, V));
                    half fres = Pow4(1.0h - NoV);
                    float3 r = reflect(-V, ng);
                    half3 sky = NocReflection(r, bg.perceptualRoughness);
                    float blur = max(bg.perceptualRoughness * bg.perceptualRoughness, NocPixelAngle());
                    float2 rh = r.xz / max(length(r.xz), 1e-4);
                    float3 Lb;
                    float cov = NocSkylineV2(r, f.positionWS, blur, NocAmbient(float3(-rh.x, 0.0, -rh.y)), _NocFacade.y, _NocFacade.w, sky, Lb);
                    float3 bgC = lerp(sky, Lb, cov);
                    // a registered canyon / Gulf wall plane in front of the skyline (its macro radiance, mirror-consistent fog)
                    float4 wl = NocWallReflect(f.positionWS, r, blur, length(_WorldSpaceCameraPos.xyz - f.positionWS), sky);
                    half3 env = (half3)lerp(bgC, wl.rgb, wl.a) * EnvironmentBRDFSpecular(bg, fres);
                    c += env * (half)(f.custom.x * f.color.r);
                }
                return c;
            }
            #include "Include/NocLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #define NOC_PASS_SHADOW
            #include "Include/NocLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #define NOC_PASS_DEPTH
            #include "Include/NocLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #define NOC_PASS_DEPTHNORMALS
            #include "Include/NocLitPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
