// BwB5Porthole - the sea seen through the three stair portholes of beat 5 (MASTER_PROMPT 14.5.5: cyan discs at eye depths
// 8.3, 19.1, 29.9 m, luma 161 / 125 / 98, "caustic flicker in the porthole water"; Addendum A1 K6 the porthole darkens).
// A stand-in for BwWaterPortal on a 0.6 m disc: UV0 = the disc's local coordinates (-1..1, +y = up). Water radiance =
// SeaTeal x _Intensity, brighter toward the top (the surface light comes from above: x 1.0 -> 0.72 over the disc) and in a
// soft centre bloom, with slow caustic ripples: two drifting sine lattices, contrast +-7 %, drift 0.06 m/s on a 0.18 m
// pattern (0.33 Hz at a point, below the 1 Hz photosensitivity limit of 14.2.5), angular rates in whole periods of the
// 3600 s wrap of _BwTime.x (Part 10 #33: no jump). _Shade (0..1) darkens the water to 15 % (a kraken arm sliding past the
// glass, K6: luma 98 -> <= 20; written by the kraken choreography through Beat05GateTower.PortholeShade). A thin
// glass sheen keeps the disc from reading as a flat sticker. Fogged by BwFogApply like every colour pass.
Shader "Megastructure/BwB5Porthole"
{
    Properties
    {
        _Color ("Water colour (linear token)", Color) = (0.0137, 0.168, 0.202, 1)
        _Intensity ("Intensity (x token)", Float) = 3.0
        _Shade ("Kraken shade (0..1)", Range(0, 1)) = 0
        _Phase ("Ripple phase offset", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float _Intensity;
            float _Shade;
            float _Phase;
        CBUFFER_END

        #include "Include/BwCommon.hlsl"
        #include "Include/BulwarkAtmosphere.hlsl"

        struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
        struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 positionWS : TEXCOORD1; };

        Varyings Vert(Attributes v)
        {
            Varyings o;
            o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
            o.positionCS = TransformWorldToHClip(o.positionWS);
            o.uv = v.uv;
            return o;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On ZTest LEqual Cull Back
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            half4 Frag(Varyings i) : SV_Target
            {
                const float kW = 6.2831853 / 3600.0;
                float t = _BwTime.x;
                float2 p = i.uv * 0.3;                                  // metres on the 0.6 m disc
                float r = saturate(length(i.uv));
                // caustic lattices drifting at about 0.06 m/s (angular rates 2 pi k / 3600, k integer)
                float a1 = sin((p.x * 34.9 + p.y * 12.0) + kW * 1200.0 * t + _Phase);
                float a2 = sin((p.x * -14.0 + p.y * 33.0) + kW * 1500.0 * t + 1.7 + _Phase);
                float a3 = sin((p.x * 22.0 - p.y * 27.0) - kW * 900.0 * t + 4.1);
                float c = (a1 * a2 + 0.5 * a3) / 1.5;                    // -1 .. 1
                float caustic = 1.0 + 0.07 * c * (1.0 - r * r);
                float topLight = lerp(0.72, 1.0, saturate(0.5 + 0.5 * i.uv.y));
                float bloom = 1.0 + 0.18 * (1.0 - r * r);
                float3 water = _Color.rgb * _Intensity * topLight * bloom * caustic;
                water *= lerp(1.0, 0.15, saturate(_Shade));
                // glass sheen: a faint rim of the stairwell's tungsten (the glass edge, not a reflection probe)
                float rim = smoothstep(0.82, 1.0, r);
                float3 col = lerp(water, water * 0.55, rim);
                col = BwFogApply(col, i.positionWS);
                return half4(col, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R Cull Back
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment FragDepth
            half FragDepth(Varyings i) : SV_Target { return i.positionCS.z; }
            ENDHLSL
        }
    }
    FallBack Off
}
