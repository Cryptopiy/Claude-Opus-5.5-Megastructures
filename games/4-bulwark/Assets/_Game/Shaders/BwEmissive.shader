// BwEmissive - HDR lamp glass and glowing fittings (MASTER_PROMPT 14.2.6 "BwEmissive / BwLantern", 14.2.5 clamps,
// 14.5.1 caged lamps, 14.5.5 pendants). Emission = palette token (_EmissionColor: SodiumWarm, Tungsten) x _Intensity,
// each channel clamped to _Clamp (lamps HDR 3, TC_ClampLamp), then x the lamp gain of the atmosphere (_BwSurface.z, 0
// means 1: the beat-5 portal boost 2^(1.3 (1 - smootherstep((s - 920) / 40))) is applied after the clamp, as 14.2.5
// states it; only materials with _GainFollow 1 follow it: LampTungsten, the atrium pendants and stair cages). Constant in time (14.5.1: "constant emission, only the pool sways"). The glass is a dark dielectric lit by
// the shared model (a thin specular sheen of the room), the emission falls off slightly toward grazing (_GlassFalloff)
// so a bulb reads as a volume, not a flat disc. Fogged like everything else (BwFogApply in the pass).
Shader "Megastructure/BwEmissive"
{
    Properties
    {
        _EmissionColor ("Emission colour (linear token)", Color) = (1.0, 0.434, 0.068, 1)
        _Intensity ("Intensity (x token)", Float) = 2.5
        _Clamp ("Radiance clamp per channel", Float) = 3.0
        _GlassFalloff ("Grazing falloff", Range(0, 1)) = 0.35
        _Smoothness ("Glass smoothness", Range(0, 0.95)) = 0.8
        _GainFollow ("Follow the lamp gain (_BwSurface.z)", Range(0, 1)) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "Include/BwLitCore.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _EmissionColor;
        float _Intensity;
        float _Clamp;
        float _GlassFalloff;
        float _Smoothness;
        float _GainFollow;
    CBUFFER_END

    float BwLampGain() { return lerp(1.0, _BwSurface.z > 0.0 ? _BwSurface.z : 1.0, saturate(_GainFollow)); }

    void BwSurfaceHook(inout BwFragData f, inout SurfaceData s)
    {
        float NoV = saturate(dot(f.normalWS, f.viewDirWS));
        float3 radiance = min(_EmissionColor.rgb * _Intensity, _Clamp) * BwLampGain();
        s.albedo = PAL_ConcreteWet * 0.5;                    // dark glass body
        s.metallic = 0.0;
        s.smoothness = _Smoothness;
        s.occlusion = 1.0;
        s.emission = radiance * (1.0 - _GlassFalloff * (1.0 - NoV));
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
