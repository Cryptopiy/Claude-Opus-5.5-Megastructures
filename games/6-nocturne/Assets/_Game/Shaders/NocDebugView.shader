// NocDebugView - the passes of NocDebugViewFeature (TEXTURE_PLAN 7.1, TP-0). Not a scene material: the feature creates its own
// material from this shader (Always Included like every Megastructure/* shader).
//   0 Count       override material of the overdraw renderers, drawn in the opaque order (CommonOpaque: quantised front to back)
//                 with ZTest LEqual and ZWrite On into a cleared depth: R += 1 per fragment that passes the depth test, i.e. per
//                 fragment the facade forward pass would shade without a prepass (no depth priming, D-008)
//   1 Complexity  the same renderers with ZTest Always: G += 1 per fragment (depth complexity, an upper bound)
//   2 Copy        full screen: the debug colour saved before the rain and the grade (linear) back into the camera colour
//   3 Overdraw    full screen: codes R = 8 x shaded count, G = 8 x complexity (both capped at 255), B = 255 where a counted
//                 renderer covers the pixel; written as the linear value whose sRGB encode is that code
//   4 Prime       depth only (ZWrite On, ColorMask 0): every other opaque renderer first, so a counted fragment hidden behind a
//                 floor, deck or wall is not counted (the forward pass draws those near surfaces first, front to back)
// No keywords, no instancing; samplers: none (Load only).
Shader "Megastructure/NocDebugView"
{
    Properties { }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Include/NocCommon.hlsl"

    Texture2D<float4> _NocDbgSrc;

    struct RendAttr { float4 positionOS : POSITION; };
    float4 RendVert(RendAttr a) : SV_POSITION { return TransformWorldToHClip(TransformObjectToWorld(a.positionOS.xyz)); }

    float4 FullVert(uint id : SV_VertexID) : SV_POSITION
    {
        float2 uv = float2((id << 1) & 2, id & 2);
        return float4(uv * 2.0 - 1.0, UNITY_NEAR_CLIP_VALUE, 1.0);
    }

    float4 FragCount(float4 pos : SV_POSITION) : SV_Target { return float4(1.0, 0.0, 0.0, 0.0); }
    float4 FragComplexity(float4 pos : SV_POSITION) : SV_Target { return float4(0.0, 1.0, 0.0, 0.0); }
    float4 FragCopy(float4 pos : SV_POSITION) : SV_Target { return float4(_NocDbgSrc.Load(int3(pos.xy, 0)).rgb, 1.0); }
    float4 FragOverdraw(float4 pos : SV_POSITION) : SV_Target
    {
        float2 n = _NocDbgSrc.Load(int3(pos.xy, 0)).rg;
        float3 codes = float3(min(8.0 * round(n.x), 255.0), min(8.0 * round(n.y), 255.0), n.y > 0.5 ? 255.0 : 0.0) / 255.0;
        return float4(NocSrgbToLinear(codes), 1.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Count"
            ZTest LEqual ZWrite On Cull Back Blend One One ColorMask R
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex RendVert
            #pragma fragment FragCount
            ENDHLSL
        }
        Pass
        {
            Name "Complexity"
            ZTest Always ZWrite Off Cull Back Blend One One ColorMask G
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex RendVert
            #pragma fragment FragComplexity
            ENDHLSL
        }
        Pass
        {
            Name "Copy"
            ZTest Always ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex FullVert
            #pragma fragment FragCopy
            ENDHLSL
        }
        Pass
        {
            Name "Overdraw"
            ZTest Always ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex FullVert
            #pragma fragment FragOverdraw
            ENDHLSL
        }
        Pass
        {
            Name "Prime"
            ZTest LEqual ZWrite On Cull Back ColorMask 0
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex RendVert
            #pragma fragment FragCount
            ENDHLSL
        }
    }
    Fallback Off
}
