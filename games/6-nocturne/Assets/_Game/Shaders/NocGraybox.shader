// NocGraybox - the reference material of the Noc lit template (IMPORT_PLAN 7.13: The Ring's RingCeramic structure) used by
// the Phase-2 graybox: dark albedo token, vertex R = AO (Abyss MeshKit convention), a filtered 1 m grid on upward faces so
// motion reads, optional emission (markers). Forward+ (_CLUSTER_LIGHT_LOOP) and _ADDITIONAL_LIGHTS are kept; the shadow
// keywords are dropped (no shadow maps, 14.2.3). NocFog and NocFloor are applied last by NocLitPass.
Shader "Megastructure/NocGraybox"
{
    Properties
    {
        _BaseColor ("Base colour (linear)", Color) = (0.0232, 0.0232, 0.0395, 1)
        _Smoothness ("Smoothness", Range(0, 0.95)) = 0.35
        _GridStrength ("Grid strength", Range(0, 1)) = 0.5
        _AO ("Vertex AO strength", Range(0, 1)) = 1
        [HDR] _EmissionColor ("Emission (linear HDR)", Color) = (0, 0, 0, 1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "Include/NocLitCore.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float4 _EmissionColor;
        float _Smoothness;
        float _GridStrength;
        float _AO;
        float _Cull;
    CBUFFER_END

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        float up = saturate(f.normalWS.y);
        float gx = NocFilteredGrid1(f.positionWS.x, 1.0, 0.03, f.footprint, 4.0);
        float gz = NocFilteredGrid1(f.positionWS.z, 1.0, 0.03, f.footprint, 4.0);
        float grid = saturate(gx + gz - gx * gz) * up * _GridStrength;
        float ao = lerp(1.0, f.color.r, _AO);
        s.albedo = _BaseColor.rgb * (1.0 + 2.0 * grid) * lerp(0.8, 1.0, ao);
        s.smoothness = min(_Smoothness, 0.95);
        s.occlusion = ao;
        s.emission = _EmissionColor.rgb;
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
