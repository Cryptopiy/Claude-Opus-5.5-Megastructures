// NocAlleyPool - light pools of beat 1's lamps on the alley walls (Phase 4 wave B, AL, INT-21: walls lit only by the ambient fall
// under LOOK_V2 check 7, and the 14.2.3 cap of 20 realtime lights is spent). A pool is a quad 1 cm off a wall (UV0 panel metres; UV1 =
// (W, H, su, sv): the panel size and the foot of the lamp on the wall, in panel metres); its vertex colour carries the light (rgb =
// colour x intensity / 4, so up to x 4) and A = d / 4, the lamp's distance from that wall (m). The irradiance of a point source d off
// a plane, along the plane at distance r from its foot, relative to the foot: (d^2 / (d^2 + r^2))^1.5 (inverse square x cosine).
// The pass multiplies what is already drawn there: Blend DstColor One -> dst x (1 + 4 rgb E T), with T the fog transmittance at the
// wall (the pool is light on the surface, so it fades with the surface): the wall's own texture and colour carry the pool, like a
// real light's. Smooth, no pattern, no time, no texture: nothing can shimmer; the panel rim fades to zero. ZWrite Off, drawn after
// the opaque walls (queue Transparent-50: never an occluder of the visibility audit), Offset -1 -1 against the wall.
// Spot mode (_Spot 1; fix wave, D-039 (6): KS02's walls at 4-10 m beside the eye had no source and a 21st realtime light is not
// allowed): the two work floodlights under the catwalk, each cross-aimed down the alley at the opposite wall. The material holds
// both lamps (_SpotA / _SpotB: world position xyz, intensity w; _SpotADir / _SpotBDir: aim xyz, cos of the outer cone w; _SpotInner:
// cos of the inner cone) and the light colour; per pixel E = sum I max(N.L, 0) smoothstep(cos_out, cos_in, -L.aim) / (d^2 + 0.25)
// with the wall normal N = (-sign x, 0, 0) (the alley walls face the axis), the panel rim faded over 0.6 m, x the fog
// transmittance; the pass multiplies the wall by 1 + colour E, like the pools.
Shader "Megastructure/NocAlleyPool"
{
    Properties
    {
        _Offset ("Lamp distance scale", Float) = 1
        _Gain ("Gain", Float) = 1
        _Spot ("Spot mode (work lamps)", Float) = 0
        _SpotA ("Lamp A (xyz, w intensity)", Vector) = (0, -50, 0, 0)
        _SpotADir ("Lamp A aim (xyz, w cos outer)", Vector) = (0, -1, 0, 0.7)
        _SpotB ("Lamp B (xyz, w intensity)", Vector) = (0, -50, 0, 0)
        _SpotBDir ("Lamp B aim (xyz, w cos outer)", Vector) = (0, -1, 0, 0.7)
        _SpotInner ("Cos of the inner cone", Float) = 0.95
        _SpotColor ("Lamp colour (linear)", Color) = (1, 0.8, 0.6, 1)
    }

    HLSLINCLUDE
    #include "../Include/NocLitCore.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _Offset;
        float _Gain;
        float _Spot;
        float4 _SpotA;
        float4 _SpotADir;
        float4 _SpotB;
        float4 _SpotBDir;
        float _SpotInner;
        float4 _SpotColor;
    CBUFFER_END

    float PoolSpot(float4 lamp, float4 aim, float3 P, float3 n)
    {
        float3 L = lamp.xyz - P;
        float d2 = dot(L, L);
        L *= rsqrt(max(d2, 1e-4));
        float cone = smoothstep(aim.w, max(_SpotInner, aim.w + 1e-3), dot(-L, aim.xyz));
        return lamp.w * saturate(dot(n, L)) * cone / (d2 + 0.25);
    }

    struct PoolA { float4 positionOS : POSITION; float4 color : COLOR; float2 uv0 : TEXCOORD0; float4 uv1 : TEXCOORD1; UNITY_VERTEX_INPUT_INSTANCE_ID };
    struct PoolV { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float4 color : TEXCOORD1; float4 uv : TEXCOORD2; float2 src : TEXCOORD3; };

    PoolV PoolVert(PoolA i)
    {
        PoolV o = (PoolV)0;
        UNITY_SETUP_INSTANCE_ID(i);
        o.positionWS = TransformObjectToWorld(i.positionOS.xyz);
        o.positionCS = TransformWorldToHClip(o.positionWS);
        o.color = i.color;
        o.uv = float4(i.uv0, i.uv1.xy);
        o.src = i.uv1.zw;
        return o;
    }

    half4 PoolFrag(PoolV i) : SV_Target0
    {
        float2 WH = max(i.uv.zw, 0.05);
        [branch] if (_Spot > 0.5)
        {
            float3 nW = float3(i.positionWS.x > 0.0 ? -1.0 : 1.0, 0.0, 0.0);
            float Es = PoolSpot(_SpotA, _SpotADir, i.positionWS, nW) + PoolSpot(_SpotB, _SpotBDir, i.positionWS, nW);
            float2 er = min(i.uv.xy, WH - i.uv.xy) / 0.6;
            Es *= smoothstep(0.0, 1.0, saturate(min(er.x, er.y)));
            return half4(_SpotColor.rgb * (_Gain * Es * NocFogT(i.positionWS)), 1.0);
        }
        float2 r = i.uv.xy - i.src.xy;
        float d = max(i.color.a * 4.0 * _Offset, 0.05);
        float q = d * d / max(dot(r, r) + d * d, 1e-6);
        float E = q * sqrt(q);                                               // (d^2 / (d^2 + r^2))^1.5: 1 at the foot
        // fade to zero toward the panel rim (the pool never shows the quad's edge)
        float2 e = min(i.uv.xy, WH - i.uv.xy) / max(0.2 * WH, 0.05);
        E *= smoothstep(0.0, 1.0, saturate(min(e.x, e.y)));
        float T = NocFogT(i.positionWS);
        return half4(i.color.rgb * (4.0 * _Gain * E * T), 1.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-50" "IgnoreProjector" = "True" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend DstColor One
            ZWrite Off ZTest LEqual Cull Back
            Offset -1, -1
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex PoolVert
            #pragma fragment PoolFrag
            #pragma multi_compile_instancing
            ENDHLSL
        }
    }
    Fallback Off
}
