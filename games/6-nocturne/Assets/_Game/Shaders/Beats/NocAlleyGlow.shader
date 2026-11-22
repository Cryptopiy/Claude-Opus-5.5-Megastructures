// NocAlleyGlow - the lit haze of the boulevard seen through the alley mouth (Phase 4 wave B, AL, task 6: KS00, the title frame,
// "the 120 m alley converging on the glowing mouth"; the boulevard's air is lit by its lamps, blades and shops, and that in-scatter
// is what makes a street mouth glow at night). One G1Mesh.Quad just beyond the mouth facing the alley (UV0 panel metres, UV1 =
// (W, H)), Cull Back (invisible from the boulevard), additive (Blend One One): radiance = vertex colour x 4 x _Gain x a smooth
// profile (brightest over the street, falling with height as exp(-y / _FallH), soft at both sides) x the fog transmittance to the
// card (NocFogT: it is light in the air at the card, dimmed like a surface there) x a distance fade smoothstep(_Near0, _Near1,
// d): it dissolves smoothly while the walker approaches (gone well before the mouth, so nothing pops). No time, no pattern, no
// texture: nothing can shimmer or band beyond the scene's own dither.
Shader "Megastructure/NocAlleyGlow"
{
    Properties
    {
        _Gain ("Gain", Float) = 1
        _FallH ("Height fall-off (m)", Float) = 7
        _Near0 ("Distance fade start (m)", Float) = 14
        _Near1 ("Distance fade full (m)", Float) = 60
    }

    HLSLINCLUDE
    #include "../Include/NocLitCore.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _Gain;
        float _FallH;
        float _Near0;
        float _Near1;
    CBUFFER_END

    struct GlowA { float4 positionOS : POSITION; float4 color : COLOR; float2 uv0 : TEXCOORD0; float4 uv1 : TEXCOORD1; UNITY_VERTEX_INPUT_INSTANCE_ID };
    struct GlowV { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float4 color : TEXCOORD1; float4 uv : TEXCOORD2; };

    GlowV GlowVert(GlowA i)
    {
        GlowV o = (GlowV)0;
        UNITY_SETUP_INSTANCE_ID(i);
        o.positionWS = TransformObjectToWorld(i.positionOS.xyz);
        o.positionCS = TransformWorldToHClip(o.positionWS);
        o.color = i.color;
        o.uv = float4(i.uv0, i.uv1.xy);
        return o;
    }

    half4 GlowFrag(GlowV i) : SV_Target0
    {
        float2 WH = max(i.uv.zw, 0.05);
        float x = i.uv.x / WH.x;                                            // 0..1 across
        float y = i.uv.y;                                                    // metres up
        float side = smoothstep(0.0, 0.35, x) * smoothstep(0.0, 0.35, 1.0 - x);
        float prof = exp(-y / max(_FallH, 0.5)) * smoothstep(0.0, 0.6, y) * (1.0 - smoothstep(0.75, 1.0, y / WH.y));
        float d = length(_WorldSpaceCameraPos.xyz - i.positionWS);
        float fade = smoothstep(_Near0, _Near1, d);
        float T = NocFogT(i.positionWS);
        return half4(i.color.rgb * (4.0 * _Gain * side * prof * fade * T), 0.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off ZTest LEqual Cull Back
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex GlowVert
            #pragma fragment GlowFrag
            #pragma multi_compile_instancing
            ENDHLSL
        }
    }
    Fallback Off
}
