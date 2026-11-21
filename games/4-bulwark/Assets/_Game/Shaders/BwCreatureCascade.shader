// BwCreatureCascade - the water pouring off the emerged kraken (fix wave X2, S2-3; kraken owner; KrakenCascade.cs builds the
// strands and feeds the emitters). Each strand is a camera-facing ribbon along the ballistic path of the water leaving an
// emitter on the head rim or an arm's underside: P(tau) = E + O v0 tau + wind tau (0.25 + 0.75 a) - g tau^2 / 2 (y), tau in
// [0, fall time to the sea] (an 80 m drop takes 4 s: it reads as size). The strand is torn into clumps that travel with the
// water (noise of t - tau), widens and frays into a spray veil that drifts with the 9 m/s wind towards its foot, and fades
// into the sea. Few thin strands = drip strings (low rate, after the emergence). Widths never fall below 2 px (the alpha takes
// the rest: no sub-pixel sparkle). Lit: white-green water, sun and ambient with a forward-scatter lobe; BwFogApply; the
// lee-side dam clip of the creature. Premultiplied alpha, transparent queue, no depth write, no shadows.
Shader "Megastructure/BwCreatureCascade"
{
    Properties
    {
        _Opacity ("Opacity", Range(0, 1)) = 0.65
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
    #include "Include/BulwarkPalette.hlsl"
    #include "Include/BwCommon.hlsl"
    #include "Include/BulwarkAtmosphere.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Include/BwLighting.hlsl"
    #include "Generated/BulwarkConstGen.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _Opacity;
    CBUFFER_END

    #define KR_CASC_SLOTS 64
    float4 _KrCascE[KR_CASC_SLOTS];   // xyz emitter (world m), w rate 0..1 (1 torn ribbons .. 0.1 drip strings, 0 off)
    float4 _KrCascO[KR_CASC_SLOTS];   // xyz outward direction (world, unit), w fall time to the sea (s)
    float4 _KrCascT;                  // x time (s, wrapped at 1000), yzw wind velocity (world m/s)
    float4 _KrCascW[KR_CASC_SLOTS];   // x sheet width scale of the slot (head rim 3.2, arms 0.8-2.4)
    float4 _BwDamClip;                // the creature's lee-side dam clip (KrakenDirector; see BwCreatureSkin.hlsl)

    // the creature's dam clip (BwCreatureSkin.hlsl KrDamClipKeep, crest parts included)
    float KrCascDamKeep(float3 positionWS)
    {
        if (_BwDamClip.x < 0.5) return 1.0;
        float cx = _BwDamClip.y, cy = _BwDamClip.z;
        float dx = positionWS.x - cx;
        if (positionWS.x <= TC_LeeFaceX || dx <= 1e-3) return 1.0;
        float yLee = cy + (positionWS.y - cy) * saturate((TC_LeeFaceX - cx) / dx);
        float ySea = cy + (positionWS.y - cy) * saturate((TC_SeaFaceX - cx) / dx);
        return min(yLee, ySea) - (TC_ParapetTop + 0.5);
    }

    struct A
    {
        float3 positionOS : POSITION;
        float2 uv : TEXCOORD0;        // x side -1..1, y along 0 (the emitter) .. 1 (the sea)
        float4 d : TEXCOORD1;         // x slot, y seed 0..1, z width scale, w lateral offset -1..1
    };
    struct V
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS : TEXCOORD0;
        float4 a : TEXCOORD1;         // x side, y along, z coverage (alpha scale), w seed
        float2 n : TEXCOORD2;         // x flow coordinate (the water parcel's release time), y width (m)
    };

    V Vert(A i)
    {
        V o = (V)0;
        uint slot = min((uint)(i.d.x + 0.5), (uint)(KR_CASC_SLOTS - 1));
        float4 E = _KrCascE[slot], O = _KrCascO[slot];
        float seed = i.d.y, along = i.uv.y;
        // strands of a slot switch on with the rate: all three at 1, one in three at the drip end
        bool on = E.w > 0.001 && seed < 0.3 + 0.7 * E.w;
        if (!on)
        {
            o.positionCS = float4(0.0, 0.0, 0.0, 1.0); // pal:ok (a degenerate vertex: not drawn)
            return o;
        }
        float g = 9.81;
        float tau = along * O.w;
        float3 up = float3(0.0, 1.0, 0.0); // pal:ok (direction)
        float3 outward = O.xyz;
        float3 lat = cross(up, outward);
        lat *= rsqrt(max(dot(lat, lat), 1e-6));
        float v0 = 1.2 + 2.2 * seed;                            // the sheet leaving the rim (m/s)
        float drift = 0.08 + 0.92 * along * along;               // the frayed spray veil takes the wind, the core falls straight (kraken-fx2 KS11: 25 % drift curved the cores into wires)
        float3 E0 = E.xyz + lat * (i.d.w * 6.0);
        float3 P = E0 + outward * (v0 * tau) + _KrCascT.yzw * (tau * drift) - up * (0.5 * g * tau * tau);
        float3 Tg = outward * v0 + _KrCascT.yzw * drift - up * (g * tau);
        Tg *= rsqrt(max(dot(Tg, Tg), 1e-6));
        float3 toCam = _WorldSpaceCameraPos - P;
        float dist = max(length(toCam), 1e-3);
        float3 side = cross(Tg, toCam / dist);
        side *= rsqrt(max(dot(side, side), 1e-6));
        // width: torn ribbon 1.5-5 m (x the slot's scale), thinner drips, fraying x3 towards the foot
        float w = (1.5 + 3.5 * seed) * i.d.z * _KrCascW[slot].x * lerp(0.3, 1.0, saturate(E.w)) * (1.0 + 2.0 * along * along);
        float wMin = 2.0 * _BwCam.w * dist;                      // never under 2 px: the coverage goes into the alpha
        float cov = saturate(w / max(wMin, 1e-4));
        w = max(w, wMin);
        P += side * (i.uv.x * 0.5 * w);
        o.positionWS = P;
        o.positionCS = TransformWorldToHClip(P);
        o.a = float4(i.uv.x, along, cov * saturate(E.w * 1.5 + 0.2), seed); // pal:ok (interpolants, not a colour)
        o.n = float2((_KrCascT.x - tau) * 1.3 + seed * 37.0, w);
        return o;
    }

    half4 Frag(V i) : SV_Target0
    {
        clip(KrCascDamKeep(i.positionWS));
        float side = i.a.x, along = i.a.y, seed = i.a.w;
        // torn: clumps that travel with the water (release time) and tears across, soft frayed edges
        // (kraken-fx4 kk2: one coarse octave drew smooth glass-like strips) clumps along the flow, tears across, and fine
        // fibres along the sheet (the water rips into strings as it falls), each faded by the strand's pixel width
        float clump = BwValueNoise(float2(i.n.x * 2.2, seed * 31.0)); // pal:ok (noise coordinates)
        float tear = BwValueNoise(float2(i.n.x * 5.0, side * 2.6 + seed * 7.0)); // pal:ok (noise coordinates)
        float fibre = BwValueNoise(float2(side * 7.0 + seed * 13.0, i.n.x * 0.6)); // pal:ok (noise coordinates)
        float edge = 1.0 - smoothstep(0.15, 1.0, abs(side));   // soft frayed edges (kraken-fx5: hard edges read as glass slivers)
        // a dense torn core near the rim (kraken-fx3 KS kk2: 0.22-0.55 read as faint glass streaks), thinning into the veil
        float body = smoothstep(0.30, 0.62, 0.5 * clump + 0.35 * tear + 0.35 * fibre + 0.1 * (1.0 - along) - 0.2 * along);
        float alpha = _Opacity * edge * body * i.a.z * (1.0 - 0.5 * along) * smoothstep(0.0, 0.04 + 0.22 * seed, along) * (1.0 - smoothstep(0.85, 1.0, along));
        clip(alpha - 0.004);
        // white-green water turning to white spray towards the foot, lit by the sun (forward scatter when back-lit) and the sky
        float3 V3 = normalize(_WorldSpaceCameraPos - i.positionWS);
        Light ml = GetMainLight();
        float fwd = pow(saturate(dot(-V3, ml.direction)), 4.0);
        float3 albedo = lerp(lerp(PAL_SeaTeal, PAL_FoamWhite, 0.7), PAL_FoamWhite, saturate(along * 1.4)) * 0.9;
        float3 amb = (float3)BwAmbient(float3(0.0, 1.0, 0.0)); // pal:ok (direction)
        float3 c = albedo * (amb + ml.color * (0.30 + 1.4 * fwd) * saturate(ml.direction.y + 0.2) * BwSunBreak(i.positionWS));
        c = BwFogApply(c, i.positionWS);
        return half4((half3)(c * alpha), (half)alpha);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }
            Blend One OneMinusSrcAlpha
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
