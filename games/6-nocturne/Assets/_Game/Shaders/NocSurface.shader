// NocSurface - the shared physically based material of Nocturne (MASTER_PROMPT 8.11 items 1-6, docs/MATERIALS.md): concrete,
// wet stone, painted and bare steel, brass, moss, bark, asphalt. Metallic workflow through the Noc lit template (URP BRDFData,
// analytic sky reflection, Forward+ lights, trilight x LightCal, specular AA, NocFog and NocFloor last).
// Maps (NocTextureGen, one tile = _Tile metres): _AlbedoTex = albedo VARIATION (sRGB, mean 0.5 linear; the albedo token in
// _BaseColor is the mean, so palette tokens stay the single colour source), _NormalTex = tangent normal (mipmapped, filtered),
// _MaskTex = R metallic, G occlusion, B height, A smoothness (0..1 mapped to _SmoothMin.._SmoothMax).
// World-space triplanar projection (weights |n|^4, whiteout normal blend, per-plane offsets so the planes never repeat
// together) plus macro variation from value noise (m scale, two octaves): albedo +-_MacroAlbedo, smoothness +-_MacroSmooth
// (>= 0.1 peak to peak on every large surface, 8.11 item 4). Context layers (8.11 item 4): vertex AO (R) as occlusion,
// edge wear (G) exposing bare metal (_WearColor, metallic 1) on painted steel, rain wetness (darker porous albedo, smoother,
// more on up faces) and drip streaks below edges on vertical faces. Dielectrics have metallic 0; metals 1 (8.11 item 1).
// Look v2 (-surf-v 2; TEXTURE_PLAN 3.1-3.4, LOOK_V2 5; SU): the maps through TP's NocTexturing library - hex-tiled world
// triplanar (_Mapping 0, per-row rotation _HexRotate 0 for directional sets) or UV0 metres (_Mapping 1, bark), every tap
// SampleGrad and faded to the last mip by the footprint, normals as one surface gradient; leaks anchored to the face top
// (vertex B = d_top baked for _LeakAnchor rows; the analytic stand-in until TP-1's OverlayArr decal); wetness after Lagarde with
// the row's porosity (darkening lerp(1, 0.2, porosity x wet), gloss toward 0.88; metals, paint and tiles: gloss only); debug view
// 8. TP-2 (FX, Phase 4 wave A): the v1 path (-surf-v 1: NocTriplanar taps, the old wetness) is deleted; the v1 properties stay
// (the material layout does not move) and the generated maps remain the _Tex2 0 set.
Shader "Megastructure/NocSurface"
{
    Properties
    {
        _BaseColor ("Albedo token (linear)", Color) = (0.0232, 0.0232, 0.0395, 1)
        [NoScaleOffset] _AlbedoTex ("Albedo variation (sRGB, mean 0.5)", 2D) = "grey" {}
        [NoScaleOffset][Normal] _NormalTex ("Normal", 2D) = "bump" {}
        [NoScaleOffset] _MaskTex ("Mask (R metal, G AO, B height, A smoothness)", 2D) = "white" {}
        _Tile ("Tile size (m)", Float) = 2
        _Metallic ("Metallic (0 dielectric, 1 metal)", Range(0, 1)) = 0
        _SmoothMin ("Smoothness min", Range(0, 0.95)) = 0.2
        _SmoothMax ("Smoothness max", Range(0, 0.95)) = 0.5
        _NormalStrength ("Normal strength", Range(0, 2)) = 1
        _MacroScale ("Macro scale (m)", Float) = 9
        _MacroAlbedo ("Macro albedo +-", Range(0, 0.5)) = 0.15
        _MacroSmooth ("Macro smoothness +-", Range(0, 0.3)) = 0.08
        _AO ("Vertex AO strength", Range(0, 1)) = 1
        _EdgeWear ("Edge wear (vertex G)", Range(0, 1)) = 0
        _WearColor ("Bare metal F0 (linear)", Color) = (0.44, 0.48, 0.55, 1)
        _Wetness ("Rain wetness", Range(0, 1)) = 0.5
        _Streaks ("Drip streaks", Range(0, 1)) = 0.3
        [HDR] _EmissionColor ("Emission (linear HDR)", Color) = (0, 0, 0, 1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        // Look v2 (-surf-v 2, TEXTURE_PLAN 3.1-3.4; SU). Floats only, no keyword; the v1 path reads none of them.
        _TileV ("v2 tile v (m, 0 = _Tile)", Float) = 0
        _Mapping ("v2 mapping (0 world triplanar, 1 UV0 metres)", Float) = 0
        _HexRotate ("v2 hex rotation (0 offsets only .. 1 full)", Range(0, 1)) = 1
        _TexSeed ("v2 anti-tiling seed", Float) = 0
        _Porosity ("v2 porosity (LOOK_V2 5.2)", Range(0, 1)) = 0.4
        _WetLevel ("v2 wet level (exposed 1, sheltered 0.15)", Range(0, 1)) = 1
        _LeakAnchor ("v2 leaks anchored to the face top (vertex B)", Float) = 0
        _LeakGain ("v2 leak strength", Range(0, 1)) = 0.5
        _LeakDecal ("v2 leak decal from OverlayArr slice 0 (0 = the analytic stand-in)", Float) = 0
        [NoScaleOffset] _OverlayMask ("v2 OverlayArr mask (R coverage, B height, A smoothness)", 2DArray) = "" {}
        [NoScaleOffset] _OverlayNormal ("v2 OverlayArr normal (GL)", 2DArray) = "" {}
        _MicroGain ("v2 micro detail gain (0 off)", Range(0, 1)) = 0
        _MicroSlice ("v2 MicroArr slice (0 concrete, 1 stone, 2 metal, 3 asphalt)", Float) = 0
        [NoScaleOffset] _MicroAlbedo ("v2 MicroArr albedo modifier (mean 0.5)", 2DArray) = "" {}
        [NoScaleOffset] _MicroNormal ("v2 MicroArr normal (GL, high-passed)", 2DArray) = "" {}
        _EmitByAlbedo ("v2 emission x the albedo variation (0 flat, 1 textured glow: the goal curtain)", Range(0, 1)) = 0
        // v2 photo set (TP-1's processed maps; the v1 path keeps the generated maps above, so the default picture never moves before
        // SU-5): _Tex2 1 = sample these with their own tile, token and absolute-smoothness range
        _Tex2 ("v2 photo set on", Float) = 0
        _BaseColor2 ("v2 albedo token (linear, LOOK_V2 5.1)", Color) = (0.171, 0.171, 0.171, 1)
        [NoScaleOffset] _AlbedoTex2 ("v2 albedo variation (photo)", 2D) = "grey" {}
        [NoScaleOffset][Normal] _NormalTex2 ("v2 normal (photo, GL)", 2D) = "bump" {}
        [NoScaleOffset] _MaskTex2 ("v2 mask (photo: R metal, G AO, B height, A absolute smoothness)", 2D) = "white" {}
        _Tile2 ("v2 tile (m)", Float) = 2
        _TileV2 ("v2 tile v (m, 0 = _Tile2)", Float) = 0
        _SmoothMin2 ("v2 smoothness min", Range(0, 1)) = 0
        _SmoothMax2 ("v2 smoothness max", Range(0, 1)) = 1
        _NormalStrength2 ("v2 normal strength", Range(0, 2)) = 1
        _AntiTiling ("v2 anti-tiling (0 hex, 1 module shuffle)", Float) = 0
        _ModuleU ("v2 module u (m)", Float) = 1
        _ModuleV ("v2 module v (m)", Float) = 1
        _Bond ("v2 running bond (0 stack, 1 running)", Float) = 0
        _BondAxis ("v2 bond axis (0 rows shift along u, 1 columns shift along v)", Float) = 0
    }

    HLSLINCLUDE
    #include "Include/NocLitCore.hlsl"
    #include "Include/NocTexturing.hlsl"
    #include "Generated/NocSurfaceConstantsGen.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float4 _WearColor;
        float4 _EmissionColor;
        float _Tile;
        float _Metallic;
        float _SmoothMin;
        float _SmoothMax;
        float _NormalStrength;
        float _MacroScale;
        float _MacroAlbedo;
        float _MacroSmooth;
        float _AO;
        float _EdgeWear;
        float _Wetness;
        float _Streaks;
        float _Cull;
        float _TileV;
        float _Mapping;
        float _HexRotate;
        float _TexSeed;
        float _Porosity;
        float _WetLevel;
        float _LeakAnchor;
        float _LeakGain;
        float _LeakDecal;
        float _MicroGain;
        float _MicroSlice;
        float _EmitByAlbedo;
        float4 _BaseColor2;
        float _Tex2;
        float _Tile2;
        float _TileV2;
        float _SmoothMin2;
        float _SmoothMax2;
        float _NormalStrength2;
        float _AntiTiling;
        float _ModuleU;
        float _ModuleV;
        float _Bond;
        float _BondAxis;
        float4 _AlbedoTex_TexelSize;
    CBUFFER_END

    TEXTURE2D(_AlbedoTex); SAMPLER(sampler_AlbedoTex);
    TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
    TEXTURE2D(_MaskTex);   SAMPLER(sampler_MaskTex);
    // Look v2 arrays (TP's OverlayArr / MicroArr; one sampler per array: 5 samplers in all)
    TEXTURE2D_ARRAY(_OverlayMask); SAMPLER(sampler_OverlayMask);
    TEXTURE2D_ARRAY(_OverlayNormal);
    TEXTURE2D_ARRAY(_MicroAlbedo); SAMPLER(sampler_MicroAlbedo);
    TEXTURE2D_ARRAY(_MicroNormal);
    // the v2 photo set, sampled with the v1 maps' samplers (same wrap / filter class: 5 samplers in all)
    TEXTURE2D(_AlbedoTex2);
    TEXTURE2D(_NormalTex2);
    TEXTURE2D(_MaskTex2);

    // v2 lookup of one set (TEXTURE_PLAN 3.1, 1.3): UV0 metres (_Mapping 1, hex), world triplanar with the module shuffle of gridded
    // sets (_AntiTiling 1: one tap per map and plane, the seam inside the joint; _BondAxis 1 swaps the axes: half bond along v) or
    // world triplanar hex tiling (TP's NocTriSample). Every tap SampleGrad: safe inside the uniform v2 branch.
    NocTexResult NocSurfLookup(NocTri tri, float3 n0, float2 uvM, float2 dUVdx, float2 dUVdy, float3 T, float3 B, uint seed,
                               float tile, float tileV, float nStrength,
                               TEXTURE2D_PARAM(albTex, albSamp), TEXTURE2D_PARAM(nrmTex, nrmSamp), TEXTURE2D_PARAM(mskTex, mskSamp))
    {
        NocTexResult r;
        float2 tl = float2(max(tile, 1e-3), tileV > 0.0 ? tileV : max(tile, 1e-3));
        [branch] if (_Mapping > 0.5)
        {
            float2 dx = dUVdx / tl, dy = dUVdy / tl;
            float fade = NocTexFade(1.0, NocFootprintBox(dx, dy));
            NocHex h = NocHexSetup(uvM / tl, dx, dy, NC_TexHexRate, _HexRotate, seed, fade);
            NocHexW hw;
            r.albedo = NocHexColour(h, TEXTURE2D_ARGS(albTex, albSamp), hw).rgb;
            r.mask = NocHexMask(h, hw, TEXTURE2D_ARGS(mskTex, mskSamp));
            r.sg = NocSurfGradTB(NocHexSlope(h, hw, TEXTURE2D_ARGS(nrmTex, nrmSamp), nStrength), T, B);
            r.fade = fade;
        }
        else
        {
            [branch] if (_AntiTiling > 0.5)
            {
                r.albedo = 0.0; r.mask = 0.0; r.fade = 0.0;
                float3 vg = 0.0;
                bool sw = _BondAxis > 0.5;
                bool courses = _ModuleU <= 0.0;                                         // whole courses (brick): hashed per course only
                float2 module = max(float2(courses ? 1024.0 * tl.x : _ModuleU, _ModuleV), 1e-3);
                float2 counts = courses ? float2(1.0, tl.y / module.y) : tl / module;
                [unroll] for (int k = 0; k < 3; k++)
                {
                    float wk = tri.w[k];
                    [branch] if (wk > 0.0)
                    {
                        float2 c, dx, dy;
                        NocTriPlane(tri, k, c, dx, dy);
                        NocModule mo = NocModuleSetup(sw ? c.yx : c, sw ? dx.yx : dx, sw ? dy.yx : dy, sw ? module.yx : module,
                                                      sw ? counts.yx : counts, tl.x, _Bond, seed + (uint)k * 0x9E3779B9u);
                        float2 uv = sw ? mo.uv.yx : mo.uv, gx = sw ? mo.dx.yx : mo.dx, gy = sw ? mo.dy.yx : mo.dy;
                        [branch] if (courses) uv.x += NocU01(NocPcg((uint)(int)floor(c.y / module.y) * 0x9E3779B9u + seed));   // per-course u offset
                        float4 a = NocSampleFade(TEXTURE2D_ARGS(albTex, albSamp), uv, gx, gy, mo.fade);
                        float4 m = NocSampleFade(TEXTURE2D_ARGS(mskTex, mskSamp), uv, gx, gy, mo.fade);
                        float2 sl = 0.0;
                        [branch] if (mo.fade > 0.0) sl = NocTangentToSlope(SAMPLE_TEXTURE2D_GRAD(nrmTex, nrmSamp, uv, gx, gy), nStrength) * mo.fade;
                        m.a = saturate(m.a + mo.smooth);
                        r.albedo += wk * a.rgb * mo.tone;
                        r.mask += wk * m;
                        vg += wk * NocTriVolume(k, sl);
                        r.fade += wk * mo.fade;
                    }
                }
                r.sg = NocSurfGradVolume(n0, vg);
            }
            else
            {
                r = NocTriSample(tri, n0, tl.x, NC_TexHexRate, _HexRotate, seed, nStrength,
                                 TEXTURE2D_ARGS(albTex, albSamp), TEXTURE2D_ARGS(nrmTex, nrmSamp), TEXTURE2D_ARGS(mskTex, mskSamp));
            }
        }
        return r;
    }

    // Triplanar sample of the three maps; returns the blended albedo variation, mask and the world normal.
    // v1 lookup. Explicit gradients (the screen derivatives of P passed in, taken before the uniform v2 branch): the same mip
    // selection as the implicit taps, and the v2 path no longer pays for these nine taps.
    void NocTriplanar(float3 p, float3 n, float3 dpx, float3 dpy, out float3 albedoVar, out float4 mask, out float3 nWS)
    {
        float3 w = pow(abs(n), 4.0);
        w /= max(w.x + w.y + w.z, 1e-5);
        float inv = 1.0 / max(_Tile, 1e-3);
        float2 uvX = p.zy * inv + float2(0.37, 0.11);     // per-plane offsets: the three planes never repeat together
        float2 uvY = p.xz * inv + float2(0.73, 0.29);
        float2 uvZ = p.xy * inv + float2(0.19, 0.61);
        float2 xdx = dpx.zy * inv, xdy = dpy.zy * inv, ydx = dpx.xz * inv, ydy = dpy.xz * inv, zdx = dpx.xy * inv, zdy = dpy.xy * inv;
        albedoVar = SAMPLE_TEXTURE2D_GRAD(_AlbedoTex, sampler_AlbedoTex, uvX, xdx, xdy).rgb * w.x
                  + SAMPLE_TEXTURE2D_GRAD(_AlbedoTex, sampler_AlbedoTex, uvY, ydx, ydy).rgb * w.y
                  + SAMPLE_TEXTURE2D_GRAD(_AlbedoTex, sampler_AlbedoTex, uvZ, zdx, zdy).rgb * w.z;
        mask = SAMPLE_TEXTURE2D_GRAD(_MaskTex, sampler_MaskTex, uvX, xdx, xdy) * w.x
             + SAMPLE_TEXTURE2D_GRAD(_MaskTex, sampler_MaskTex, uvY, ydx, ydy) * w.y
             + SAMPLE_TEXTURE2D_GRAD(_MaskTex, sampler_MaskTex, uvZ, zdx, zdy) * w.z;
        // whiteout-blended triplanar normals (Golus 2017), tangent x flipped by the axis sign
        float3 sgn = float3(n.x < 0.0 ? -1.0 : 1.0, n.y < 0.0 ? -1.0 : 1.0, n.z < 0.0 ? -1.0 : 1.0);
        float3 tX = UnpackNormalScale(SAMPLE_TEXTURE2D_GRAD(_NormalTex, sampler_NormalTex, uvX, xdx, xdy), _NormalStrength);
        float3 tY = UnpackNormalScale(SAMPLE_TEXTURE2D_GRAD(_NormalTex, sampler_NormalTex, uvY, ydx, ydy), _NormalStrength);
        float3 tZ = UnpackNormalScale(SAMPLE_TEXTURE2D_GRAD(_NormalTex, sampler_NormalTex, uvZ, zdx, zdy), _NormalStrength);
        tX.x *= sgn.x; tY.x *= sgn.y; tZ.x *= -sgn.z;
        float3 nX = float3(tX.xy + n.zy, abs(tX.z) * n.x);
        float3 nY = float3(tY.xy + n.xz, abs(tY.z) * n.y);
        float3 nZ = float3(tZ.xy + n.xy, abs(tZ.z) * n.z);
        nWS = normalize(nX.zyx * w.x + nY.xzy * w.y + nZ.xyz * w.z);
    }

    // ------------------------------------------------------------------ Look v2 (-surf-v 2; TEXTURE_PLAN 3.1-3.3, LOOK_V2 5; SU)
    // Debug view 8 (TEXTURE_PLAN 7.1): a 0.25 m checker of the row's mapping x the mip level of its maps (texels per pixel: blue
    // magnified (<= 1/4) .. green 1 .. yellow 4 .. red >= 16).
    float3 NocSurfDensityCode(float2 c2, float fp)
    {
        float2 c = floor(c2 / 0.25);
        float chk = abs(c.x + c.y - 2.0 * floor(0.5 * (c.x + c.y))) > 0.5 ? 1.0 : 0.6;
        float mip = log2(max(fp * max(_AlbedoTex_TexelSize.z, 1.0) / max(_Tile, 1e-3), 1e-4));
        float3 blue = float3(0.1, 0.2, 1.0), green = float3(0.1, 0.9, 0.2), yellow = float3(1.0, 0.9, 0.1), red = float3(1.0, 0.1, 0.1);
        float3 ramp = mip < 0.0 ? lerp(blue, green, saturate(0.5 * mip + 1.0))
                    : (mip < 2.0 ? lerp(green, yellow, 0.5 * mip) : lerp(yellow, red, saturate(0.5 * (mip - 2.0))));
        return ramp * chk;
    }

    // Leaks anchored to the face top (TEXTURE_PLAN 3.3): vertex B = d_top in 0.1 m steps (baked by WorldContext.AddMesh for
    // _LeakAnchor rows on -surf-v 2). The face is cut into SurfLeakTileU (2 m) columns, each with a hashed stretch 1-2 (length
    // 1 m x stretch) and u offset; the column of u and its nearest neighbour are blended over the 0.15 m next to their border, so
    // the leaks never end in a vertical seam. A column is either the OverlayArr slice 0 decal (_LeakDecal 1: v = 1 at the edge,
    // 0.5 one length below, the opacity-0 pad under it, clamped at 0.25; faded to its linear coverage profile below 2-4 px of
    // the 2 m tile) or the analytic stand-in (a stretched value noise, 0.35 m across). Returns the coverage; slope = the decal's
    // height slope along (u, up) where it covers.
    // The horizontal face coordinate (m) of a vertical face along its horizontal tangent (x where the face is level).
    float NocFaceU(float3 P, float3 n)
    {
        float2 t = float2(-n.z, n.x);
        float lt = dot(t, t);
        return lt > 1e-6 ? dot(P.xz, t * rsqrt(lt)) : P.x;
    }

    float NocLeakColumn(float k, out float off)
    {
        uint hh = NocPcg2((int)k, 0x4C45414B);
        off = NocU01(NocPcg(hh + 0x9E3779B9u));
        return lerp(NC_SurfLeakStretch0, NC_SurfLeakStretch1, NocU01(hh));
    }

    float NocLeakOne(float k, float u, float dTop, float2 dU, float2 dD, float fp, out float2 slope)
    {
        float off;
        float len = NC_SurfLeakLen * NocLeakColumn(k, off);
        float hang = smoothstep(0.0, 0.08, dTop);
        slope = 0.0;
        float cov;
        [branch] if (_LeakDecal > 0.5)
        {
            float2 uv = float2(u / NC_SurfLeakTileU + off, max(0.25, 1.0 - 0.5 * dTop / len));
            float2 dx = float2(dU.x / NC_SurfLeakTileU, -0.5 * dD.x / len), dy = float2(dU.y / NC_SurfLeakTileU, -0.5 * dD.y / len);
            float fade = NocTexFade(1.0, NocFootprintBox(dx, dy));
            cov = 2.0 * NC_SurfLeakMean * saturate(1.0 - dTop / len);              // the decal's mean profile (same mean over its length)
            [branch] if (fade > 0.0)
            {
                float m = SAMPLE_TEXTURE2D_ARRAY_GRAD(_OverlayMask, sampler_OverlayMask, uv, 0, dx, dy).r;
                cov = lerp(cov, m, fade);
                slope = NocTangentToSlope(SAMPLE_TEXTURE2D_ARRAY_GRAD(_OverlayNormal, sampler_OverlayMask, uv, 0, dx, dy), 1.0) * fade;
            }
        }
        else
        {
            float raw = saturate((NocValueNoise(float2(u / 0.35 + 17.0 * off, dTop / len)) - 0.45) * 2.5);
            float keep = smoothstep(2.0, 4.0, 0.35 / max(fp, 1e-5));
            cov = lerp(NC_SurfLeakMean, raw, keep) * exp(-dTop / len);
        }
        return cov * hang;
    }

    float NocLeak(float u, float3 n, float dTop, float2 dU, float2 dD, float fp, out float2 slope)
    {
        float x = u / NC_SurfLeakTileU;
        float c = floor(x), fr = x - c;
        float side = fr < 0.5 ? -1.0 : 1.0;
        float dB = (fr < 0.5 ? fr : 1.0 - fr) * NC_SurfLeakTileU;               // distance to the nearer column border (m)
        float wN = 0.5 * (1.0 - smoothstep(0.0, 0.15, dB));
        float2 s0, s1 = 0.0;
        float l0 = NocLeakOne(c, u, dTop, dU, dD, fp, s0), l1 = 0.0;
        [branch] if (wN > 0.0) l1 = NocLeakOne(c + side, u, dTop, dU, dD, fp, s1);
        float vertical = saturate(1.0 - 4.0 * abs(n.y));
        slope = lerp(s0, s1, wN) * vertical;
        return lerp(l0, l1, wN) * vertical;
    }

    // Micro detail (TEXTURE_PLAN 3.3, MicroArr 0.5 m): every active triplanar plane with its weight (one plane on walls and floors,
    // two on chamfers: no seam where the dominant plane changes), faded in by w_det = smoothstep(4, 8, 0.5 m / fp) x _MicroGain; albedo
    // x 2 x the modifier (mean 1), the high-passed slope as a volume gradient. No tap where w_det is 0.
    float3 NocMicro(NocTri t, float wDet, inout float3 albedo)
    {
        float3 vg = 0.0;
        [branch] if (wDet > 0.0)
        {
            float inv = 1.0 / NC_TexMicroTile;
            float mm = 0.0;
            [unroll] for (int k = 0; k < 3; k++)
            {
                float wk = t.w[k];
                [branch] if (wk > 0.0)
                {
                    float2 c, dx, dy;
                    NocTriPlane(t, k, c, dx, dy);
                    float2 uv = c * inv + 0.37 + 0.29 * k;
                    mm += wk * SAMPLE_TEXTURE2D_ARRAY_GRAD(_MicroAlbedo, sampler_MicroAlbedo, uv, _MicroSlice, dx * inv, dy * inv).r;
                    float2 sl = NocTangentToSlope(SAMPLE_TEXTURE2D_ARRAY_GRAD(_MicroNormal, sampler_MicroAlbedo, uv, _MicroSlice, dx * inv, dy * inv), 1.0);
                    vg += wk * NocTriVolume(k, sl * wDet);
                }
            }
            albedo *= lerp(1.0, 2.0 * mm, wDet);
        }
        return vg;
    }

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        float3 p = f.positionWS;
        float3 n0 = f.normalWS;
        // every derivative before any branch
        NocTri tri = NocTriSetup(p, n0);
        float2 uvM = f.uv.xy;
        float2 dUVdx = ddx(uvM), dUVdy = ddy(uvM);
        float3 T, B;
        NocUvFrame(uvM, p, n0, T, B);

        // macro variation (two value-noise octaves; the projection mixes the height so walls vary too)
        float2 mp = float2(p.x + 0.71 * p.y, p.z - 0.53 * p.y) / max(_MacroScale, 0.1);
        float m1 = NocValueNoise(mp), m2 = NocValueNoise(mp * 3.1 + 17.3);
        float macro = 0.65 * m1 + 0.35 * m2;
        float up = saturate(n0.y);
        float vert = 1.0 - up - saturate(-n0.y);
        // the leak decal's face coordinates (u along the face, d_top from vertex B) and their derivatives
        float uFace = NocFaceU(p, n0), dTop = f.color.b * NC_SurfLeakMaxD;
        float2 dU = float2(ddx(uFace), ddy(uFace)), dD = float2(ddx(dTop), ddy(dTop));

        // ---- maps: hex-tiled world triplanar (mode 0) or UV0 metres (mode 1), surface-gradient normals (NocTexturing, TP)
        uint seed = (uint)(_TexSeed + 0.5) * 0x9E3779B9u + 0x53555246u;
        NocTexResult r;
        float3 token = _BaseColor.rgb;
        float sMin = _SmoothMin, sMax = _SmoothMax;
        [branch] if (_Tex2 > 0.5)
        {
            r = NocSurfLookup(tri, n0, uvM, dUVdx, dUVdy, T, B, seed, _Tile2, _TileV2, _NormalStrength2,
                              TEXTURE2D_ARGS(_AlbedoTex2, sampler_AlbedoTex), TEXTURE2D_ARGS(_NormalTex2, sampler_NormalTex), TEXTURE2D_ARGS(_MaskTex2, sampler_MaskTex));
            token = _BaseColor2.rgb;
            sMin = _SmoothMin2; sMax = _SmoothMax2;
        }
        else
        {
            r = NocSurfLookup(tri, n0, uvM, dUVdx, dUVdy, T, B, seed, _Tile, _TileV, _NormalStrength,
                              TEXTURE2D_ARGS(_AlbedoTex, sampler_AlbedoTex), TEXTURE2D_ARGS(_NormalTex, sampler_NormalTex), TEXTURE2D_ARGS(_MaskTex, sampler_MaskTex));
        }
        float3 n2 = NocResolveNormal(n0, r.sg);
        float h = r.mask.b;

        float ao = lerp(1.0, f.color.r, _AO) * lerp(1.0, r.mask.g, 0.85);
        float3 albedo = token * 2.0 * r.albedo * (1.0 + _MacroAlbedo * (2.0 * macro - 1.0));
        float metal = saturate(_Metallic * r.mask.r);
        float smooth = lerp(sMin, sMax, r.mask.a) + _MacroSmooth * (2.0 * NocValueNoise(mp * 1.7 + 41.0) - 1.0);

        // edge wear (vertex G): bare metal on painted steel
        float wear = saturate(_EdgeWear * f.color.g * (0.6 + 0.8 * h));
        albedo = lerp(albedo, _WearColor.rgb, wear);
        metal = lerp(metal, 1.0, wear);
        smooth = lerp(smooth, 0.55, wear);

        // leaks anchored to the face top (vertex B) on _LeakAnchor rows; the v1 drip streaks stay on every vertical face (wetness)
        float leak = 0.0;
        float2 leakSlope = 0.0;
        [branch] if (_LeakAnchor > 0.5)
        {
            leak = _LeakGain * NocLeak(uFace, n0, dTop, dU, dD, f.footprint, leakSlope);
            // the decal's relief along (u, up) as a surface gradient of the face
            float2 tt = float2(-n0.z, n0.x);
            float3 Tf = dot(tt, tt) > 1e-6 ? normalize(float3(tt.x, 0.0, tt.y)) : float3(1.0, 0.0, 0.0);
            n2 = NocResolveNormal(n2, NocSurfGradTB(leakSlope * _LeakGain, Tf, float3(0.0, 1.0, 0.0)));
        }
        // micro detail within 1-2 m (MicroArr)
        float wDet = _MicroGain * smoothstep(NC_TexMicroPx0, NC_TexMicroPx1, NC_TexMicroTile / max(f.footprint, 1e-5));
        float3 mvg = NocMicro(tri, wDet, albedo);
        n2 = NocResolveNormal(n2, NocSurfGradVolume(n2, mvg));
        float raw = saturate((NocValueNoise(float2((p.x + p.z) / 0.35, p.y / 7.0)) - 0.55) * 3.0);
        float keep = 1.0 - smoothstep(0.25, 0.5, f.footprint / 0.35);
        float streak = max(leak, lerp(0.12, raw, keep) * vert * _Streaks);       // the rain's drip streaks on every vertical face
        albedo *= lerp(1.0, NC_SurfLeakDark, leak);
        smooth += NC_SurfLeakSmooth * leak;
        ao *= 1.0 - NC_SurfLeakAo * leak;

        // wetness (LOOK_V2 5.2, Lagarde): up 1, vertical 0.55 + streaks, pooling in low height on up faces; porous rows darken
        // by lerp(1, 0.2, porosity x wet), every row gains gloss toward 0.88 (metals, paint, tiles: smoothness only)
        float wet = saturate(_WetLevel * (NC_SurfWetBase + NC_SurfWetUp * up) + streak * NC_SurfWetStreak + up * NC_SurfWetPool * (0.5 - h));
        albedo *= lerp(1.0, NC_SurfWetDarkMin, saturate(_Porosity * wet) * (1.0 - metal));
        smooth = lerp(smooth, NC_SurfWetGloss, wet * lerp(0.6, 0.45, _Porosity));

        s.albedo = max(albedo, 0.0);
        s.metallic = metal;
        s.smoothness = saturate(smooth);
        s.occlusion = ao;
        s.emission = _EmissionColor.rgb * lerp(1.0, 2.0 * r.albedo, _EmitByAlbedo);   // the weave glows through (mean 1)
        f.normalWS = n2;
        [branch] if (NocDebugView() == 8)
        {
            float2 c2 = _Mapping > 0.5 ? uvM : (abs(n0.y) > 0.7 ? p.xz : (abs(n0.x) > abs(n0.z) ? p.zy : p.xy));
            f.debug = float4(NocSurfDensityCode(c2, f.footprint), 1.0);
        }
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On ZTest LEqual Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #define NOC_PASS_FORWARD
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
