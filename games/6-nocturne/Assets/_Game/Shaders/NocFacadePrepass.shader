// NocFacadePrepass - the depth-only override pass of NocFacadePrepassFeature (TEXTURE_PLAN 2.12 / 8.3 TP-0: NocFacade overdraw
// measured above 1.5 at KS04, KS09 and KS17, docs/logs/TP.md 23:15). Not a scene material: the feature creates its own material.
//   0 Depth   the facade renderers' geometry into the camera depth before the opaques (ZWrite On, ColorMask 0). The clip position
//             is computed exactly as NocLitPass's NocVert does it (TransformObjectToWorld, then TransformWorldToHClip; NocFacade
//             has no vertex hook), so the depth is bit-identical to NocFacade's forward pass: that pass may keep ZTest LEqual or
//             switch to ZWrite Off / ZTest Equal (FA's _FacadeZWrite / _FacadeZTest), and every facade fragment hidden behind the
//             front surface is rejected before shading.
// No keywords, no samplers.
Shader "Megastructure/NocFacadePrepass"
{
    Properties { }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Depth"
            ZWrite On ZTest LEqual ColorMask 0 Cull Back
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 positionOS : POSITION; };
            float4 Vert(A a) : SV_POSITION { return TransformWorldToHClip(TransformObjectToWorld(a.positionOS.xyz)); }
            half4 Frag() : SV_Target { return 0; }
            ENDHLSL
        }
    }
    Fallback Off
}
