// NocDreadVeil - the hull's deck veil (docs/DREAD.md 2.1; owner DR): the analytic occluder of the city-lit cloud base, an
// elongated soft ellipse in the plane y 456 that moves with the hull (DreadDriver writes its globals, NocDread.hlsl). A quad under
// the deck multiplies whatever lies behind it (the sky's deck, the tops of the faces inside the deck) by m = 1 - k shape T: the
// hull's shadow on the cloud base, darkest over the hull, soft over 390 x 1 250 m. Queue Transparent-20 (after every opaque and
// the sky, before the glass and the streaks), Blend DstColor Zero, ZWrite Off, ZTest LEqual (the hull, below the plane, is never
// darkened by it: NocHull darkens its own fog colour by the same factor), Cull Off. No texture, no keyword.
Shader "Megastructure/NocDreadVeil"
{
    Properties
    {
        _VeilGain ("Veil gain (x the global strength)", Float) = 1.0
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "../Include/NocAtmosphere.hlsl"
    #include "NocDread.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _VeilGain;
    CBUFFER_END

    struct Attributes
    {
        float4 positionOS : POSITION;
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS : TEXCOORD0;
    };

    Varyings Vert(Attributes i)
    {
        Varyings o;
        o.positionWS = TransformObjectToWorld(i.positionOS.xyz);
        o.positionCS = TransformWorldToHClip(o.positionWS);
        return o;
    }

    half4 Frag(Varyings i) : SV_Target
    {
        float m = 1.0 - saturate(_NocDreadHull.z * _VeilGain) * NocDreadVeilShape(i.positionWS.xz) * NocFogT(i.positionWS);
        return half4(m, m, m, 1.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-20" "IgnoreProjector" = "True" }
        Pass
        {
            Name "Veil"
            Tags { "LightMode" = "UniversalForward" }
            Blend DstColor Zero
            ZWrite Off ZTest LEqual Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
    Fallback Off
}
