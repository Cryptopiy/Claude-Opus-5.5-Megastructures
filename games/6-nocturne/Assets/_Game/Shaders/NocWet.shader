// NocWet - the wet floors of Nocturne (14.2.5, 14.7 H4, D-003, R1 section 2): every walkable surface is the plane y 0.
// Surface: the albedo token (_BaseColor) x the generated albedo variation, planar xz maps (one tap each, _Tile m), puddle
// mask 35 % (two rotated value-noise octaves, measured threshold, edge widened by fwidth, faded to the 0.35 mean by the
// footprint), roughness (GGX alpha) 0.08 in puddles and 0.35 elsewhere (x the map, blended in variance), rain-ring ripples in
// puddles faded by the footprint with the removed slope variance moved into the lobe (Toksvig-like), dry-area normal detail.
// Lighting: diffuse from the Forward+ lights and the trilight only (no URP point-light specular: NOC_SPECULAR_OFF, D-003),
// weighted by (1 - F); specular = F G1 x (sky + the beat's wall planes + the area sources) of NocMirror.hlsl with specular AA
// and mirror-consistent fog; then NocFog and NocFloor (NocLitPass). The static test (-rain 0 -freeze-clock) is frame-identical:
// the ripples scale with the rain rho and the sources change only through 12 m crossfades.
Shader "Megastructure/NocWet"
{
    Properties
    {
        _BaseColor ("Albedo token (linear)", Color) = (0.0070, 0.0060, 0.0130, 1)
        [NoScaleOffset] _AlbedoTex ("Albedo variation (sRGB, mean 0.5)", 2D) = "grey" {}
        [NoScaleOffset][Normal] _NormalTex ("Normal", 2D) = "bump" {}
        [NoScaleOffset] _MaskTex ("Mask (R metal, G AO, B height, A smoothness)", 2D) = "white" {}
        _Tile ("Tile size (m)", Float) = 2
        _NormalStrength ("Dry normal strength", Range(0, 2)) = 0.6
        _PuddleFraction ("Puddle fraction (0..1, 0.35 = 14.2.5)", Range(0, 1)) = 0.35
        _AlphaPuddle ("GGX alpha in puddles", Range(0.02, 1)) = 0.08
        _AlphaDry ("GGX alpha elsewhere", Range(0.02, 1)) = 0.35
        _ReflPuddle ("Reflectivity in puddles", Range(0, 1)) = 0.95
        _ReflDry ("Reflectivity elsewhere", Range(0, 1)) = 0.5
        _MacroScale ("Macro scale (m)", Float) = 11
        _MacroAlbedo ("Macro albedo +-", Range(0, 0.5)) = 0.15
        _AO ("Vertex AO strength", Range(0, 1)) = 0.5
        // Look v2 (-surf-v 2, TEXTURE_PLAN 3.3; SU): the puddle threshold measured from the row's height histogram, the absolute
        // smoothness range of the mask (photo rows 0..1) and the analytic steel-deck plates (0 = none)
        _PuddleTh ("v2 puddle threshold (noise + height bias)", Float) = 0.5688
        _SmoothMin ("v2 smoothness min (wet)", Range(0, 0.95)) = 0.4
        _SmoothMax ("v2 smoothness max (wet)", Range(0, 0.95)) = 0.6
        _PlateU ("v2 deck plate size u (m, 0 = none)", Float) = 0
        _PlateV ("v2 deck plate size v (m)", Float) = 0
        // v2 photo set (TP-1; the v1 path keeps the generated maps above until SU-5): _Tex2 1 = sample these (hex or module shuffle on
        // planar xz), their own token, the absolute dry smoothness lifted toward the wet gloss by _WetGloss2, the threshold _PuddleTh2
        // measured from their own height histogram
        _Tex2 ("v2 photo set on", Float) = 0
        _BaseColor2 ("v2 albedo token (linear)", Color) = (0.171, 0.171, 0.171, 1)
        [NoScaleOffset] _AlbedoTex2 ("v2 albedo variation (photo)", 2D) = "grey" {}
        [NoScaleOffset][Normal] _NormalTex2 ("v2 normal (photo, GL)", 2D) = "bump" {}
        [NoScaleOffset] _MaskTex2 ("v2 mask (photo)", 2D) = "white" {}
        _Tile2 ("v2 tile (m)", Float) = 2
        _TileV2 ("v2 tile v (m, 0 = _Tile2)", Float) = 0
        _NormalStrength2 ("v2 normal strength", Range(0, 2)) = 1
        _SmoothMin2 ("v2 smoothness min (photo: 0)", Range(0, 1)) = 0
        _SmoothMax2 ("v2 smoothness max (photo: 1)", Range(0, 1)) = 1
        _WetGloss2 ("v2 wet gloss (0 dry .. 1 SurfWetGloss)", Range(0, 1)) = 0.5
        _PuddleTh2 ("v2 puddle threshold of the photo heights", Float) = 0.5688
        _HexRotate ("v2 hex rotation (0 offsets only .. 1 full)", Range(0, 1)) = 1
        _TexSeed ("v2 anti-tiling seed", Float) = 0
        _AntiTiling ("v2 anti-tiling (0 hex, 1 module shuffle)", Float) = 0
        _ModuleU ("v2 module u (m)", Float) = 1
        _ModuleV ("v2 module v (m)", Float) = 1
        _Bond ("v2 running bond", Float) = 0
        _BondAxis ("v2 bond axis (1 = half bond along v)", Float) = 0
        [HDR] _EmissionColor ("Emission (linear HDR)", Color) = (0, 0, 0, 1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "Include/NocLitCore.hlsl"
    #include "Include/NocTexturing.hlsl"
    #include "Include/NocMirror.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float4 _EmissionColor;
        float _Tile;
        float _NormalStrength;
        float _PuddleFraction;
        float _AlphaPuddle;
        float _AlphaDry;
        float _ReflPuddle;
        float _ReflDry;
        float _MacroScale;
        float _MacroAlbedo;
        float _AO;
        float _Cull;
        float _PuddleTh;
        float _SmoothMin;
        float _SmoothMax;
        float _PlateU;
        float _PlateV;
        float4 _AlbedoTex_TexelSize;
        float4 _BaseColor2;
        float4 _MaskTex2_TexelSize;
        float _Tex2;
        float _Tile2;
        float _TileV2;
        float _NormalStrength2;
        float _SmoothMin2;
        float _SmoothMax2;
        float _WetGloss2;
        float _PuddleTh2;
        float _HexRotate;
        float _TexSeed;
        float _AntiTiling;
        float _ModuleU;
        float _ModuleV;
        float _Bond;
        float _BondAxis;
    CBUFFER_END

    TEXTURE2D(_AlbedoTex); SAMPLER(sampler_AlbedoTex);
    TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
    TEXTURE2D(_MaskTex);   SAMPLER(sampler_MaskTex);
    // the v2 photo set, sampled with the v1 maps' samplers (3 samplers in all)
    TEXTURE2D(_AlbedoTex2);
    TEXTURE2D(_NormalTex2);
    TEXTURE2D(_MaskTex2);

    // v2 planar xz lookup of the photo set (TEXTURE_PLAN 3.1, 1.3): the module shuffle of gridded sets (_AntiTiling 1; _BondAxis 1
    // swaps the axes for the half bond along v) or hex tiling (TP's NocHexSetup with the row's rotation); SampleGrad only.
    void NocWetLookup(float2 xz, float2 dx0, float2 dy0, uint seed, out float3 var, out float4 mask, out float2 slope)
    {
        float2 tl = float2(max(_Tile2, 1e-3), _TileV2 > 0.0 ? _TileV2 : max(_Tile2, 1e-3));
        [branch] if (_AntiTiling > 0.5)
        {
            bool sw = _BondAxis > 0.5;
            float2 module = max(float2(_ModuleU, _ModuleV), 1e-3);
            float2 counts = tl / module;
            NocModule mo = NocModuleSetup(sw ? xz.yx : xz, sw ? dx0.yx : dx0, sw ? dy0.yx : dy0, sw ? module.yx : module,
                                          sw ? counts.yx : counts, tl.x, _Bond, seed);
            float2 uv = sw ? mo.uv.yx : mo.uv, gx = sw ? mo.dx.yx : mo.dx, gy = sw ? mo.dy.yx : mo.dy;
            var = NocSampleFade(TEXTURE2D_ARGS(_AlbedoTex2, sampler_AlbedoTex), uv, gx, gy, mo.fade).rgb * mo.tone;
            mask = NocSampleFade(TEXTURE2D_ARGS(_MaskTex2, sampler_MaskTex), uv, gx, gy, mo.fade);
            mask.a = saturate(mask.a + mo.smooth);
            slope = 0.0;
            [branch] if (mo.fade > 0.0) slope = NocTangentToSlope(SAMPLE_TEXTURE2D_GRAD(_NormalTex2, sampler_NormalTex, uv, gx, gy), _NormalStrength2) * mo.fade;
        }
        else
        {
            float2 dx = dx0 / tl, dy = dy0 / tl;
            float fade = NocTexFade(1.0, NocFootprintBox(dx, dy));
            NocHex h = NocHexSetup(xz / tl, dx, dy, NC_TexHexRate, _HexRotate, seed, fade);
            NocHexW hw;
            var = NocHexColour(h, TEXTURE2D_ARGS(_AlbedoTex2, sampler_AlbedoTex), hw).rgb;
            mask = NocHexMask(h, hw, TEXTURE2D_ARGS(_MaskTex2, sampler_MaskTex));
            slope = NocHexSlope(h, hw, TEXTURE2D_ARGS(_NormalTex2, sampler_NormalTex), _NormalStrength2);
        }
    }

    // Debug view 8 (TEXTURE_PLAN 7.1): a 0.25 m checker of the planar xz mapping x the mip level of the maps (texels per pixel:
    // blue magnified (<= 1/4) .. green 1 .. yellow 4 .. red >= 16).
    float3 NocWetDensityCode(float2 xz, float fwm)
    {
        float2 c = floor(xz / 0.25);
        float chk = abs(c.x + c.y - 2.0 * floor(0.5 * (c.x + c.y))) > 0.5 ? 1.0 : 0.6;
        float mip = log2(max(fwm * max(_AlbedoTex_TexelSize.z, 1.0) / max(_Tile, 1e-3), 1e-4));
        float3 blue = float3(0.1, 0.2, 1.0), green = float3(0.1, 0.9, 0.2), yellow = float3(1.0, 0.9, 0.1), red = float3(1.0, 0.1, 0.1);
        float3 ramp = mip < 0.0 ? lerp(blue, green, saturate(0.5 * mip + 1.0))
                    : (mip < 2.0 ? lerp(green, yellow, 0.5 * mip) : lerp(yellow, red, saturate(0.5 * (mip - 2.0))));
        return ramp * chk;
    }

    // Writes f.custom = (puddle mask, lobe variance sigma2, reflectivity, 0) and the floor normal for the forward hook.
    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        float2 xz = f.positionWS.xz;
        // footprints and every derivative before any branch
        float fwm = max(max(length(ddx(xz)), length(ddy(xz))), 1e-5);              // m per pixel, anisotropic (grazing)
        float pn = NocPuddleNoise(xz);
        float fwn = fwidth(pn);
        float pn2 = NocPuddleNoise2(xz);
        float fwn2 = fwidth(pn2);

        float2 uv = xz / max(_Tile, 1e-3) + float2(0.41, 0.17);
        float3 var = SAMPLE_TEXTURE2D(_AlbedoTex, sampler_AlbedoTex, uv).rgb;
        float4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv);
        float3 tn = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, uv), _NormalStrength);
        float fwh = fwidth(mask.b);
        float2 dXZx = ddx(xz), dXZy = ddy(xz);

        float rho = _NocFog.w;
        float macro = NocValueNoise(xz / max(_MacroScale, 0.1)) * 0.65 + NocValueNoise(xz / max(_MacroScale, 0.1) * 3.1 + 9.1) * 0.35;
        float m, sigma2, refl;
        float3 n, albedo;
        // Look v2 (TEXTURE_PLAN 3.3; CRIT0 #8, #29; GB-10): many small irregular puddles from the 2.0 / 0.6 m noise plus the
        // material's own height (joints and low spots fill first), thresholded at the row's measured _PuddleTh (35 % +- 2 %)
        float h = mask.b;
        float th = _PuddleTh, fwhV = fwh;
        float3 tok = _BaseColor.rgb;
        float2 sl = -tn.xy;                                                         // v1 maps: the tangent normal's slope
        float smooth = lerp(_SmoothMin, _SmoothMax, mask.a);
        [branch] if (_Tex2 > 0.5)
        {
            // the photo set: its own height, threshold, token and absolute dry smoothness lifted by the rain film; the height's
            // edge widening estimated from its texel density (a derivative is not available inside the branch)
            uint seed = (uint)(_TexSeed + 0.5) * 0x9E3779B9u + 0x57455431u;
            float4 mk;
            NocWetLookup(xz, dXZx, dXZy, seed, var, mk, sl);
            mask = mk;
            h = mk.b;
            th = _PuddleTh2;
            fwhV = 0.25 * saturate(fwm * max(_MaskTex2_TexelSize.z, 1.0) / max(_Tile2, 1e-3));
            tok = _BaseColor2.rgb;
            smooth = lerp(lerp(_SmoothMin2, _SmoothMax2, mk.a), NC_SurfWetGloss, _WetGloss2);
        }
        // the analytic steel-deck plates (FloorSteelDeck: _PlateU x _PlateV m): each plate sags toward its centre (a zero-mean
        // height term, so the measured threshold holds), puddles collect in the low points, the 6 mm welds stand proud
        float plateOn = _PlateU > 0.0 ? 1.0 : 0.0;
        float2 plate = float2(max(_PlateU, 1e-3), max(_PlateV, 1e-3));
        float2 pr = abs(frac(xz / plate) - 0.5) * 2.0;                             // 0 at the plate centre, 1 at its seam
        float rr = max(pr.x, pr.y);
        h = saturate(h + plateOn * 0.25 * (rr * rr - 0.5));
        float weld = plateOn * (1.0 - (1.0 - NocFilteredGrid1(xz.x, plate.x, 0.006, fwm, 2.0)) * (1.0 - NocFilteredGrid1(xz.y, plate.y, 0.006, fwm, 2.0)));

        float field = pn2 + NC_SurfPuddleHeightBias * (0.5 - h);
        float e = NC_SurfPuddleEdge + fwn2 + NC_SurfPuddleHeightBias * fwhV;
        m = smoothstep(th - e, th + e, field);
        float keep = smoothstep(NC_SurfPuddleFadePx0, NC_SurfPuddleFadePx1, NC_SurfPuddleScale1 / fwm);
        m = lerp(_PuddleFraction, m, keep) * (1.0 - weld);

        // rain rings inside the puddles only, faded by the footprint; the removed slope variance moves into the lobe
        float fade = smoothstep(NC_SurfRippleFadePx0, NC_SurfRippleFadePx1, NC_SurfRippleLambda / fwm);
        float2 grad = 0;
        [branch] if (rho > 0.001 && fade > 0.0 && m > 0.001) grad = NocRippleGrad2(xz, _NocTime.y, rho) * (NC_SurfRippleSlope * fade * m);
        float rippleVar = NC_SurfRippleSlope * NC_SurfRippleSlope * NC_SurfRippleVar * rho * (1.0 - fade * fade);   // in the puddle lobe

        // lobe: the absolute (wet) mask smoothness between the puddles, never sharper than SurfAlphaDryMin; flat water in them
        // FX (INT-13, D-032 3): two lobes, mixed by m in the forward hook (NocMirrorEnvMix / NocMirrorSourcesMix): the puddle lobe
        // (flat water + the rings' moved variance) and the film lobe between them. A pixel past the puddle fade covers both; one
        // lobe of the blended variance spread the puddles' sharp streaks into the film lobe (peak x 0.15 at m 0.35)
        float aDry = max((1.0 - smooth) * (1.0 - smooth), NC_SurfAlphaDryMin);
        float s2Dry = aDry * aDry + NC_WetSigmaFloor;
        sigma2 = NC_SurfAlphaPuddle * NC_SurfAlphaPuddle + NC_WetSigmaFloor + rippleVar;
        refl = lerp(_ReflDry, 0.5 * _ReflDry, weld);   // the film's reflectivity (moss 0.35); the puddles' is NC_SurfReflPuddle

        // puddle normals stay flat: up plus the rings; the texture slope is weighted by (1 - m)
        n = normalize(float3(-sl.x * (1.0 - m) + grad.x, 1.0, -sl.y * (1.0 - m) + grad.y));
        albedo = tok * 2.0 * var * (1.0 + _MacroAlbedo * (2.0 * macro - 1.0)) * lerp(1.0, 0.75, m) * lerp(1.0, 0.6, weld);

        // AO
        float ao = lerp(1.0, f.color.r, _AO) * lerp(1.0, mask.g, 0.8);
        s.albedo = albedo;
        s.metallic = 0.0;
        s.smoothness = 0.5;
        s.occlusion = ao;
        s.emission = _EmissionColor.rgb;
        f.normalWS = n;
        f.custom = float4(m, sigma2, refl, s2Dry);       // puddle share, puddle-lobe variance, film reflectivity, film-lobe variance
        [branch] if (NocDebugView() == 8) f.debug = float4(NocWetDensityCode(xz, fwm), 1.0);
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
            #define NOC_SPECULAR_OFF
            #define NOC_HAS_FORWARD_HOOK
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Include/NocLighting.hlsl"

            half3 NocForwardHook(NocFragData f, SurfaceData s, InputData inputData)
            {
                // the puddle and film lobes share r, X, Y (one normal); both are built here, outside any branch (derivatives)
                NocLobe oP = NocMirrorLobe(f.positionWS, _WorldSpaceCameraPos.xyz, inputData.normalWS, f.custom.y, NC_SurfReflPuddle);
                NocLobe oD = NocMirrorLobe(f.positionWS, _WorldSpaceCameraPos.xyz, inputData.normalWS, f.custom.w, f.custom.z);
                float m = f.custom.x;
                BRDFData brdf;
                InitializeBRDFData(s, brdf);
                half3 diffuse = brdf.diffuse * inputData.bakedGI * s.occlusion + NocDirect(brdf, inputData);
                half3 spec = NocMirrorEnvMix(f.positionWS, f.viewDist, oP, oD, m) + NocMirrorSourcesMix(f.positionWS, f.viewDist, oP, oD, m);
                return min(diffuse * (1.0 - lerp(oD.F, oP.F, m)) + spec + s.emission, 1000.0);
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
