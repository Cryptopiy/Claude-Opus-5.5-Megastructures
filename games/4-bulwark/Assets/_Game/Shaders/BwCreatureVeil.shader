// BwCreatureVeil - QA only (Assets/_Game/Beats/QA_KrakenRig, kraken owner): the inside of a 140 m sphere drawn with the
// 14.7 H3 underwater veil radiance KrVeil(view direction, camera depth) so the kraken's underwater object model
// (BwCreatureForward.hlsl: L0 exp(-c r) + veil (1 - exp(-c r))) can be judged against the background it fades into. Unlit,
// opaque, depth written (hides the far field and the sea surface above the booth). Never used in the game.
Shader "Megastructure/BwCreatureVeil"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 1
    }
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Include/BulwarkPalette.hlsl"
    #include "Include/BwCommon.hlsl"
    #include "Generated/BulwarkConstGen.hlsl"
    CBUFFER_START(UnityPerMaterial)
        float _Cull;
    CBUFFER_END
    float4 _KrUnderwater;
    float4 _BwCam;
    #define BW_CREATURE_SKIN_INCLUDED
    float3 KrVeilLocal(float3 dirWS, float depth)
    {
        float3 Kd = float3(TC_KdR, TC_KdG, TC_KdB); // pal:ok (attenuation coefficients)
        float3 S = float3(TC_VeilSR, TC_VeilSG, TC_VeilSB); // pal:ok (spectral weights)
        float G = TC_VeilG0 / (TC_VeilSG * exp(-TC_KdG * TC_VeilG0Depth));
        float3 L = G * S * exp(-Kd * max(depth, 0.0)) * exp(TC_VeilBeta * clamp(dirWS.y, -1.0, 1.0));
        L.r = max(L.r, TC_VeilRedFloor * L.g);
        return max(L, TC_DarkFloorLinear);
    }
    struct A { float4 positionOS : POSITION; };
    struct V { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };
    V Vert(A i) { V o; o.positionWS = TransformObjectToWorld(i.positionOS.xyz); o.positionCS = TransformWorldToHClip(o.positionWS); return o; }
    half4 Frag(V i) : SV_Target0
    {
        float3 d = i.positionWS - _BwCam.xyz;
        d *= rsqrt(max(dot(d, d), 1e-8));
        return half4(KrVeilLocal(d, max(_KrUnderwater.y - _BwCam.y, 0.0)), 1.0h);
    }
    half DepthFrag(V i) : SV_Target0 { return i.positionCS.z; }
    ENDHLSL
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On ZTest LEqual Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment DepthFrag
            ENDHLSL
        }
    }
    Fallback Off
}
