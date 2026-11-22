// NocDreadBody - the shadow below (docs/DREAD.md 3.1; owner DR): a vast shape in the haze, a darkening of the haze glow and of the
// sheets behind it that reads as ONE body (fix wave C42: r03's soft volume read as cloud shading). A dark shell (0.004 linear) with a
// faint sheen on its back (the haze light from above, x n.y^2: the spine and the crest lighter than the flanks, a rounded body),
// fogged like geometry (NocFog: the haze and the sheets in front of it stay), alpha-blended with an opacity that rises over the
// outer few metres of its silhouette (a = _Opacity (1 - e^(-k x)) / (1 - e^(-k)), x = |n.v|, k = _EdgeK 6: the edge is soft over
// about 5 % of the half-width, 4-6 px at KS13, never a hard line nor a cloud); _Opacity carries the s fade (DreadDriver). NocFloor
// on the straight colour. Queue
// Transparent-10 (after the opaque world and the sky, before the glass, the streaks and the lights), Blend SrcAlpha
// OneMinusSrcAlpha, ZWrite Off, ZTest LEqual, Cull Back (one layer). No texture, no keyword.
Shader "Megastructure/NocDreadBody"
{
    Properties
    {
        _BodyColor ("Body colour (linear)", Color) = (0.004, 0.0042, 0.0048, 1)
        _Opacity ("Opacity (x fade)", Range(0, 1)) = 0.9
        _EdgeK ("Silhouette sharpness k (opacity 1 - e^(-k |n.v|))", Range(1, 20)) = 6.0
        _Sheen ("Back sheen (raw linear radiance x n.y^2; a Vector: no gamma conversion)", Vector) = (0.006, 0.005, 0.004, 1)
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "../Include/NocAtmosphere.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BodyColor;
        float _Opacity;
        float _EdgeK;
        float4 _Sheen;
    CBUFFER_END

    struct Attributes
    {
        float4 positionOS : POSITION;
        float3 normalOS : NORMAL;
        float4 color : COLOR;          // R = the end fade (ShadowBuilder)
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS : TEXCOORD0;
        float3 normalWS : TEXCOORD1;
        float fade : TEXCOORD2;
    };

    Varyings Vert(Attributes i)
    {
        Varyings o;
        o.positionWS = TransformObjectToWorld(i.positionOS.xyz);
        o.normalWS = TransformObjectToWorldNormal(i.normalOS);
        o.fade = i.color.r;
        o.positionCS = TransformWorldToHClip(o.positionWS);
        return o;
    }

    half4 Frag(Varyings i) : SV_Target
    {
        float3 n = normalize(i.normalWS);
        float3 v = normalize(_WorldSpaceCameraPos.xyz - i.positionWS);
        // r01's smoothstep(0, 0.65) read as a hard oval, r03's (1 - e^(-3 x^2)) over the whole |n.v| range as cloud shading: the
        // opacity now rises over the outer few metres of the silhouette (x 0..0.3 is the outer 5 % of an ellipse's half-width)
        float x = saturate(abs(dot(n, v)));
        float k = max(_EdgeK, 1.0);
        float a = saturate(_Opacity) * (1.0 - exp(-k * x)) / (1.0 - exp(-k)) * saturate(i.fade);
        float up = saturate(n.y);
        float3 c = NocFog(_BodyColor.rgb + _Sheen.rgb * (up * up), i.positionWS);
        c = NocFloor(c);
        return half4(c, a);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-10" "IgnoreProjector" = "True" }
        Pass
        {
            Name "DreadBody"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off ZTest LEqual Cull Back
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
    Fallback Off
}
