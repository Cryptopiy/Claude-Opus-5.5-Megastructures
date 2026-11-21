// BwSurf.shader - the white water of the beat 4 reef (MASTER_PROMPT 14.5.4 "foam skirt", "the swell breaks on the reef",
// 14.2.6 BwArmour "foam at the waterline"; owner: the beat 4 builder). Drawn on Beat04_ArmourSlope's surf grids (2.5 m
// quads over the belt, the shoal to x 800 and the six islets; empty quads are not built), which ride on THE sea: the vertex
// stage displaces every vertex by BwSeaWaves' Gerstner sum at the grid's spacing (components shorter than 8 x 2.5 m are
// left out, as on a clipmap ring of that spacing; the 0.3 m lift keeps the white water above the omitted short waves) plus
// the kraken's disturbance heights, and drops by the curvature like the clipmap.
// Coverage (no hard edges anywhere, Wave A lesson): the baked density (vertex R: exp(-d / 5 m) of the distance d from the
// mean waterline to the nearest unit hull, broad skirts round the islets, faded out over x 600-800) times the surge (the
// wave crest here now or 7 / 14 m to the west: the swell travels west, so the white water trails the crest that just broke),
// thresholded against the same footprint-filtered lacy bubble texture as BwSea's foam (drifting with the surface), so a
// patch is solid at the hull and breaks into lace and holes outward; far away every pattern converges to its mean. Lit like
// BwSea's foam (trilight ambient + key with the cascade shadow), FoamWhite clamped to 4, BwFogApplyFar, alpha blended
// after the sea (Geometry+150, ZWrite off): it never writes depth, so the mist pass and the sea's depth are untouched.
// No polygon offset: the 0.3 m lift separates it from the sea, and a slope-scaled offset pulled the grazing far grid
// through the units in front (reef-01..04: a dotted bright line across the islets at the sea's horizon row).
// Photosensitivity: the coverage moves with the swell (periods >= 3.2 s for wavelengths >= 16 m): < 0.31 Hz.
Shader "Megastructure/BwSurf"
{
    Properties
    {
        _Strength ("Foam strength", Range(0, 2)) = 0.85
        _Lift ("Lift above the sea (m)", Float) = 0.3
        _Spacing ("Grid spacing (m)", Float) = 2.5
        _SurgeGain ("Surge gain", Range(0, 1)) = 0.6
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Include/BulwarkPalette.hlsl"
    #include "Include/BwCommon.hlsl"
    #include "Include/BulwarkAtmosphere.hlsl"
    #include "Include/BwSeaWaves.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _Strength;
        float _Lift;
        float _Spacing;
        float _SurgeGain;
    CBUFFER_END

    #define SURF_SPHERE_R (6371000.0 + 5992.0)

    struct SurfAttributes
    {
        float4 positionOS : POSITION;   // world x, -8, world z (identity transform)
        float4 color : COLOR;           // r = baked density
    };

    struct SurfVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS : TEXCOORD0;
        float2 data : TEXCOORD1;        // x = density, y = surge (m)
    };

    SurfVaryings SurfVert(SurfAttributes v)
    {
        SurfVaryings o = (SurfVaryings)0;
        float3 w = TransformObjectToWorld(v.positionOS.xyz);
        float2 xz = w.xz;
        float imageW = BwSeaImageWeight(xz.x);
        float2 l = xz - _BwSeaOrigin.xz;
        float calm = BwSeaCalm(xz);
        float3 d = BwSeaDisplace(l, _Spacing, imageW);
        d.y += BwSeaDisturbHeight(xz, _Spacing);
        d *= calm;
        float h1 = BwSeaDisplace(l - float2(7.0, 0.0), _Spacing, imageW).y * calm;    // pal:ok (not a colour: 7 m west)
        float h2 = BwSeaDisplace(l - float2(14.0, 0.0), _Spacing, imageW).y * calm;   // pal:ok (not a colour: 14 m west)
        float surge = max(d.y, max(0.75 * h1, 0.5 * h2));
        float2 rel = xz - _WorldSpaceCameraPos.xz;
        float drop = dot(rel, rel) / (2.0 * SURF_SPHERE_R);
        float3 p = float3(xz.x + d.x, _BwSeaOrigin.y + d.y + _Lift - drop, xz.y + d.z);   // pal:ok (not a colour)
        o.positionWS = p;
        o.positionCS = TransformWorldToHClip(p);
        o.data = float2(v.color.r, surge);   // pal:ok (not a colour)
        return o;
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+150" "IgnoreProjector" = "True" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off ZTest LEqual Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex SurfVert
            #pragma fragment SurfFrag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            half4 SurfFrag(SurfVaryings i) : SV_Target
            {
                float3 cam = _WorldSpaceCameraPos;
                float3 v = i.positionWS - cam;
                float dist = max(length(v), 1e-3);
                float3 dir = v / dist;
                float2 xz = i.positionWS.xz;
                float2 dP0 = ddx(xz), dP1 = ddy(xz);
                float fp = max(sqrt(max(dot(dP0, dP0), dot(dP1, dP1))), 1e-4);
                // the lacy bubble texture of BwSea's foam, on the same drifted coordinates (the bubbles move with the water)
                float2 sw = float2(xz.x + _BwSeaFoam.y, xz.y);   // pal:ok (not a colour: drifted foam coordinates)
                float tA = BwFbmFiltered(sw * (1.0 / 0.9), fp / 0.9, 4);
                float tB = BwFbmFiltered(sw * (1.0 / 2.7), fp / 2.7, 3);
                float nf = BwFbmFiltered(sw * float2(1.0 / 9.0, 1.0 / 5.0), fp / 5.0, 4);   // pal:ok (not a colour: patches)
                float lace = saturate((tA + tB - 0.75) * 1.6 + 0.4 * (nf - 0.5) + 0.12);
                // broad, slowly wandering streaks (12-30 m) so the surf field is never a uniform halo round a hull
                float wn = BwFbmFiltered(sw * (1.0 / 24.0), fp / 24.0, 3);
                float dens = saturate(i.data.x) * lerp(0.55, 1.15, wn);
                float surge = saturate(i.data.y / 1.6 + 0.35);
                float cov = dens * lerp(1.0 - _SurgeGain, 1.0, surge);
                float a = saturate((1.2 * cov - 0.75 * (1.0 - lace)) * 1.4) * _Strength;
                a *= 1.0 - smoothstep(1200.0, 1800.0, dist);
                if (a < 1e-3) discard;

                Light ml = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float u = 0.95;
                float3 amb = _BwAmbSky.rgb * u + _BwAmbMid.rgb * (1.0 - u);
                float3 key = _BwSunColor.rgb * saturate(_BwSunDir.y) * ml.shadowAttenuation * BwSunBreak(i.positionWS);
                float3 c = min(PAL_FoamWhite * (0.88 + 0.12 * tA) * (amb + key), 4.0);
                c = BwFogApplyFar(c, dir, dist);
                c = AnyIsNaN(c) ? float3(0.0, 0.0, 0.0) : c;   // pal:ok (NaN guard)
                return half4(min(c, 1000.0), saturate(a));
            }
            ENDHLSL
        }
    }
    Fallback Off
}
