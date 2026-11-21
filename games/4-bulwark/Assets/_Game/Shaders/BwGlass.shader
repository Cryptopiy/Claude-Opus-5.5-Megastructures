// BwGlass.shader - the glass of the elevator cabin (ADDENDUM A1 00 LEAD DECISION 1; ending owner): walls and the glass floor.
// Transparent (queue Transparent, ZWrite off, premultiplied blend One / OneMinusSrcAlpha, Cull Off: one quad = both sides), so
// the far field and everything beyond draw first and show through. Laminated float glass: Schlick Fresnel with F0 0.04 on the
// analytic environment (BwSkyColor above the horizon, the exterior ground trilight below it, as BwEnvironment does), a sun
// glint (GGX, roughness 0.06, the dusk key with the earth's shadow of BulwarkAtmosphere's BwSunBreak at the pane's altitude),
// a faint tint absorption (_Absorb, colour _Tint: SeaTeal, the green edge of thick glass) that also darkens what is behind it,
// and a fine dust film that rises at grazing angles (the pane reads as a surface without hiding the view). The pane fades
// over its last pixel of distance, nothing aliases. Fog: the glass is only ever seen within metres (the cabin), its
// contribution is extinguished with the closed-form haze like any colour pass (BwFogApply of the added light).
Shader "Megastructure/BwGlass"
{
    Properties
    {
        _Tint ("Tint (linear token)", Color) = (0.09, 0.27, 0.27, 1)
        _Absorb ("Absorption", Range(0, 0.4)) = 0.05
        _Dust ("Grazing dust film", Range(0, 0.3)) = 0.06
        _Roughness ("Glint roughness", Range(0.02, 0.5)) = 0.06
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Pass
        {
            Name "BwGlass"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off
            ZTest LEqual
            Cull Off
            Blend One OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex GlassVert
            #pragma fragment GlassFrag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Include/BulwarkPalette.hlsl"
            #include "Include/BwCommon.hlsl"
            #include "Include/BulwarkAtmosphere.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Tint;
                float _Absorb;
                float _Dust;
                float _Roughness;
            CBUFFER_END

            struct GlassAttributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct GlassVaryings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float3 normalWS : TEXCOORD1; };

            GlassVaryings GlassVert(GlassAttributes v)
            {
                GlassVaryings o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            float3 GlassEnv(float3 r)
            {
                float3 hd = float3(r.x, max(r.y, 0.0), r.z);   // pal:ok (direction)
                float3 sky = BwSkyColor(dot(hd, hd) > 1e-6 ? normalize(hd) : float3(0.0, 0.0, 1.0));   // pal:ok (direction)
                return lerp(sky, _BwAmbExtGround.rgb, smoothstep(0.0, 0.35, -r.y));
            }

            half4 GlassFrag(GlassVaryings i, bool front : SV_IsFrontFace) : SV_Target
            {
                float3 p = i.positionWS;
                float3 V = _WorldSpaceCameraPos - p;
                float dist = max(length(V), 1e-4);
                V /= dist;
                float3 N = normalize(i.normalWS);
                N = dot(N, V) < 0.0 ? -N : N;                    // both sides
                float NoV = saturate(dot(N, V));
                float F = 0.04 + 0.96 * pow(1.0 - NoV, 5.0);
                float3 R = reflect(-V, N);
                float3 refl = GlassEnv(R) * F;
                // sun glint (GGX normalised, roughness _Roughness), lit only above the earth's shadow at the pane's altitude
                float3 L = _BwSunDir.xyz;
                float3 H = normalize(L + V);
                float NoH = saturate(dot(N, H)), NoL = saturate(dot(N, L));
                float a2 = _Roughness * _Roughness * _Roughness * _Roughness;
                float d = NoH * NoH * (a2 - 1.0) + 1.0;
                float D = a2 / max(PI * d * d, 1e-6);
                float3 glint = min(_BwSunColor.rgb * (D * F * 0.25 * NoL) * BwSunBreak(p), 8.0);
                // tint absorption and the dust film (scatters the ambient: grazing views see the pane)
                float graze = pow(1.0 - NoV, 3.0);
                float dust = _Dust * (0.25 + 0.75 * graze);
                float3 amb = _BwAmbExtSky.rgb * 0.5 + _BwAmbExtMid.rgb * 0.5;
                float3 film = (PAL_ConcreteMid * amb) * dust + _Tint.rgb * amb * _Absorb * 0.5;
                float alpha = saturate(F + (_Absorb + dust) * (1.0 - F));
                float3 col = refl + glint + film;
                col = BwFogApply(col, p) - BwFogApply(float3(0.0, 0.0, 0.0), p);   // pal:ok (added light extinguished, no in-scatter)
                col = AnyIsNaN(col) ? float3(0.0, 0.0, 0.0) : max(col, 0.0);     // pal:ok (NaN guard)
                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
