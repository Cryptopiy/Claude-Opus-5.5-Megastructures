// BwSteel - painted structural steel (MASTER_PROMPT 14.2.6 "SteelPaint with ochre bands and edge wear", 8.11.1, 8.11.4):
// doors, gates, gate houses, lamp posts, cart, stair stringers. Weathered industrial paint from the BwTexGen "Steel" set
// (orange peel, mottle, chalking streaks meandering on curl noise; 1 mm texels on a 1.0 m tile, world triplanar, plus a
// rotated 2.83 m copy mixed by a 6 m world mask; weathering smoothness +-0.08 at 6 m),
// SteelPaint (or SafetyOchre for SteelOchre) recoloured from the set; ochre bands where vertex B is set (builders paint
// bands on door leaves, gauge rings) or as world-y stripes (_BandMode 1). Paint and rust are layers, not tints (8.11.1):
// the set's chip potential (Mask R) is thresholded by the edge wear (vertex G on chamfers x _EdgeWear, plus _ChipAmount
// everywhere) with an fwidth-filtered step; inside a chip the surface is bare steel (metallic 1, iron F0 from
// PAL_FoamWhite x 0.62 = 0.51-0.56, smoothness _BareSmoothness), around it a rust halo (dielectric, rough); elsewhere it is
// dielectric paint (metallic 0). Grime collects with the vertex AO.
Shader "Megastructure/BwSteel"
{
    Properties
    {
        _BaseColor ("Paint colour (linear)", Color) = (0.112, 0.144, 0.171, 1)
        _BandColor ("Band colour (linear)", Color) = (0.571, 0.323, 0.033, 1)
        _Smoothness ("Paint smoothness offset (0.52 = texture)", Range(0, 0.9)) = 0.52
        _BareSmoothness ("Bare steel smoothness", Range(0, 0.9)) = 0.5
        _EdgeWear ("Edge wear strength", Range(0, 1)) = 0.8
        _ChipAmount ("Chips everywhere", Range(0, 1)) = 0.12
        _RustAmount ("Rust halo", Range(0, 1)) = 0.6
        _BandMode ("Bands: 0 vertex B, 1 world-y stripes", Range(0, 1)) = 0
        _BandPeriod ("Stripe period (m)", Float) = 0.4
        _BandWidth ("Stripe width (m)", Float) = 0.2
        _AO ("Vertex AO strength", Range(0, 1)) = 1
        _TexScale ("Texture tile (m)", Float) = 1.0
        _NormalStrength ("Normal strength", Range(0, 2)) = 1.0
        _DetailMix ("Second sample mix", Range(0, 1)) = 0.6
        [NoScaleOffset] _AlbedoMap ("Albedo (sRGB, BwTexGen Steel)", 2D) = "grey" {}
        [NoScaleOffset] _NormalMap ("Normal (BC5 RG)", 2D) = "bump" {}
        [NoScaleOffset] _MaskMap ("Mask (chip potential, AO, height, smoothness)", 2D) = "grey" {}
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "Include/BwLitCore.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float4 _BandColor;
        float _Smoothness;
        float _BareSmoothness;
        float _EdgeWear;
        float _ChipAmount;
        float _RustAmount;
        float _BandMode;
        float _BandPeriod;
        float _BandWidth;
        float _AO;
        float _TexScale;
        float _NormalStrength;
        float _DetailMix;
    CBUFFER_END

    TEXTURE2D(_AlbedoMap); SAMPLER(sampler_AlbedoMap);
    TEXTURE2D(_NormalMap);
    TEXTURE2D(_MaskMap);

    void BwSurfaceHook(inout BwFragData f, inout SurfaceData s)
    {
        float3 p = f.positionWS;
        float3 ng = f.normalGeoWS;
        // 1.0 m sample + a rotated 2.83 m copy (wrap-safe BwTriDetune) mixed by a ~6 m world mask: chips and chalking
        // streaks never repeat on a 4-16 m door, gate or gate house (8.8)
        BwTri t0 = BwTriSetup(p, ng, _TexScale, 6.0);
        BwTri t1 = BwTriDetune(t0, 0.3533, float2(0.27, 0.64));
        float mixK = _DetailMix * lerp(0.55, 1.0, smoothstep(0.25, 0.75, BwValueNoise3(p * 0.17 + 2.9)));   // never one pure tile
        float3 texAlb = lerp(BwTriSample(TEXTURE2D_ARGS(_AlbedoMap, sampler_AlbedoMap), t0).rgb,
                             BwTriSample(TEXTURE2D_ARGS(_AlbedoMap, sampler_AlbedoMap), t1).rgb, mixK);
        float4 mask = lerp(BwTriSample(TEXTURE2D_ARGS(_MaskMap, sampler_AlbedoMap), t0),
                           BwTriSample(TEXTURE2D_ARGS(_MaskMap, sampler_AlbedoMap), t1), mixK);
        float3 n = normalize(lerp(BwTriNormal(TEXTURE2D_ARGS(_NormalMap, sampler_AlbedoMap), t0, ng, _NormalStrength),
                                  BwTriNormal(TEXTURE2D_ARGS(_NormalMap, sampler_AlbedoMap), t1, ng, _NormalStrength * 0.8), mixK));
        float macro = BwFbmFiltered3(p / 9.0, f.footprint / 9.0, 3);
        // weathering smoothness +-0.08 at 6 m (decorrelated from the tone; footprint-filtered 3D fBm, 8.11.4)
        float macroS = BwMacroSigned3(BwFbmFiltered3(p / 6.0 + 4.1, f.footprint / 6.0, 3));

        // bands: vertex B (builder-painted) or filtered world-y stripes
        float per = max(_BandPeriod, 0.01);
        float fwy = max(abs(ddx(p.y)) + abs(ddy(p.y)), 1e-5);
        float stripes = BwPulseTri(p.y / per, saturate(_BandWidth / per), fwy / per);
        float band = saturate(max(f.color.b, _BandMode > 0.5 ? stripes : 0.0));

        // wear layers: chip potential thresholded by the wear (filtered step), rust halo just outside the chips
        float wear = saturate(f.color.g * _EdgeWear + _ChipAmount);
        float T = mask.r;
        float thr = 1.0 - 0.7 * wear;
        float fwT = max(1.5 * (abs(ddx(T)) + abs(ddy(T))), 0.04);   // wide AA: a sub-pixel chip never pops to bare metal
        float bare = saturate((T - thr) / fwT + 0.5) * step(0.02, wear);
        float rust = saturate(saturate((T - (thr - 0.07)) / fwT + 0.5) - bare) * _RustAmount * step(0.02, wear);

        float ao = lerp(1.0, f.color.r, _AO);
        float3 paintC = BwRecolour(texAlb, lerp(_BaseColor.rgb, _BandColor.rgb, band), PAL_SteelPaint);
        paintC *= 1.0 + 0.06 * (2.0 * macro - 1.0);
        float3 rustC = lerp(PAL_SafetyOchre, PAL_ConcreteDark, 0.55) * (0.8 + 0.4 * mask.b);
        float3 bareC = PAL_FoamWhite * 0.62;
        float3 alb = lerp(paintC, rustC, rust);
        alb = lerp(alb, bareC, bare);
        s.albedo = alb * lerp(0.75, 1.0, ao);
        s.metallic = bare;                                   // metal only where bare steel shows through (8.11.1)
        float paintS = saturate(mask.a + (_Smoothness - 0.52) + 0.08 * macroS) * lerp(0.85, 1.0, ao);
        s.smoothness = lerp(lerp(paintS, 0.22, rust), _BareSmoothness, bare);
        s.occlusion = ao * mask.g;
        s.emission = 0;
        f.normalWS = n;
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
            #pragma vertex BwVert
            #pragma fragment BwFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #define BW_PASS_FORWARD
            #include "Include/BwLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex BwVert
            #pragma fragment BwFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #define BW_PASS_SHADOW
            #include "Include/BwLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex BwVert
            #pragma fragment BwFrag
            #pragma multi_compile_instancing
            #define BW_PASS_DEPTH
            #include "Include/BwLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex BwVert
            #pragma fragment BwFrag
            #pragma multi_compile_instancing
            #define BW_PASS_DEPTHNORMALS
            #include "Include/BwLitPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
