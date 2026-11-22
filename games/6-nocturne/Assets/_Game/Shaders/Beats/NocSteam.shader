// NocSteam - soft steam plumes of beat 1 (MASTER_PROMPT 14.5.1: four steam vents at s 24, 58, 86, 104, cut order item 4; the
// noodle bar's pots, INT-07). Each plume is one quad (G1Mesh.Quad facing -z: UV0 = panel metres, u along +x, v up; UV1 = (W, H))
// turned about its vertical axis toward the camera in the vertex hook (a cylindrical billboard: never seen edge-on). The density
// is a two-octave value noise advected upward at _Rise m/s by the scene clock (_NocTime.x: frame-rate independent, frozen by
// -freeze-clock), in a plume envelope that widens with height and fades at the base, the top and the sides; every noise scale is
// >= 0.25 m and fades to its mean below 4 px (no sparkle). Alpha <= _Opacity; the plume fades out within 2.5 m of the camera, so
// walking through it never fills the screen. Premultiplied over the scene (NOC_TRANSPARENT), fog and floor applied by the template.
// Fix wave C38 (the plumes could not be seen in any frame): steam is a participating medium, not a surface. The template lit the
// billboard by N.L with its normal turned to the camera, so the wall lamps beside and behind a plume gave it nothing and it
// stayed darker than the lit walls behind it. Now its own forward hook in-scatters every light without N.L, through a
// Henyey-Greenstein phase (g = _Phase, relative to isotropic: (1 - g^2) / (1 + g^2 - 2 g cos)^1.5, cos = -L.V; the plume glows
// against a lamp behind it), x _Scatter, plus the sky ambient and _Glow x density (the warm kitchen light inside the bar).
Shader "Megastructure/NocSteam"
{
    Properties
    {
        _BaseColor ("Steam albedo (linear)", Color) = (0.55, 0.55, 0.56, 1)
        [HDR] _Glow ("Self glow (linear HDR)", Color) = (0, 0, 0, 1)
        _Opacity ("Peak opacity", Range(0, 1)) = 0.28
        _Rise ("Rise speed (m/s)", Float) = 0.55
        _Scale ("Noise scale (m)", Float) = 0.6
        _Scatter ("In-scatter gain (x the lights)", Float) = 1.5
        _Phase ("Phase anisotropy g", Range(0, 0.8)) = 0.35
        [HDR] _LampColor ("Baked lamp colour (linear)", Color) = (1, 0.7, 0.45, 1)
        _LampScale ("Baked lamp irradiance per unit of vertex G", Float) = 0.25
    }

    HLSLINCLUDE
    #include "../Include/NocLitCore.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float4 _Glow;
        float _Opacity;
        float _Rise;
        float _Scale;
        float _Scatter;
        float _Phase;
        float4 _LampColor;
        float _LampScale;
    CBUFFER_END

    #define NOC_HAS_VERTEX_HOOK
    void NocVertexHook(inout NocVertexData v)
    {
        float W = max(v.uv.z, 0.05);
        float u = v.uv.x - 0.5 * W;                                // metres from the plume axis along the authored +x
        float3 c = v.positionWS - float3(u, 0.0, 0.0);
        float3 toCam = _WorldSpaceCameraPos.xyz - c;
        toCam.y = 0.0;
        float l = length(toCam);
        float3 fwd = l > 1e-3 ? toCam / l : float3(0.0, 0.0, -1.0);
        float3 right = float3(fwd.z, 0.0, -fwd.x);
        v.positionWS = c + right * u;
        v.normalWS = fwd;
        v.custom.x = u;
    }

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        float W = max(f.uv.z, 0.05), H = max(f.uv.w, 0.05);
        float u = f.custom.x, y = f.uv.y;
        float v = saturate(y / H);
        float fp = max(f.footprint, 1e-5);
        float t = _NocTime.x;
        float seed = frac(f.color.r * 13.7) * 50.0;
        float sc = max(_Scale, 0.25);
        // advected two-octave noise; the second octave is faded below 4 px
        float2 p = float2(u / sc + seed, (y - t * _Rise) / (sc * 1.6));
        float n1 = NocValueNoise(p + float2(0.13 * sin(t * 0.21 + seed), 0.0));
        float n2 = NocValueNoise(p * 2.03 + float2(7.1, -t * 0.35));
        float k2 = smoothstep(2.0, 4.0, 0.5 * sc / fp);
        float n = n1 * 0.7 + lerp(0.5, n2, k2) * 0.3;
        float dens = saturate((n - 0.32) * 2.2);
        // envelope: widening plume, soft base, top and sides
        float halfW = 0.5 * W * lerp(0.45, 1.0, v);
        float side = 1.0 - smoothstep(0.35, 1.0, abs(u) / max(halfW, 0.05));
        float env = smoothstep(0.0, 0.18, v) * (1.0 - smoothstep(0.55, 1.0, v)) * side;
        float nearFade = smoothstep(0.8, 2.5, f.viewDist);
        float a = saturate(_Opacity * dens * env * nearFade);
        s.albedo = _BaseColor.rgb;
        s.metallic = 0.0;
        s.smoothness = 0.0;
        s.occlusion = 1.0;
        s.alpha = a;
        s.emission = _Glow.rgb * (0.5 + 0.5 * dens);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend One OneMinusSrcAlpha
            ZWrite Off ZTest LEqual Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #define NOC_PASS_FORWARD
            #define NOC_TRANSPARENT
            #define NOC_HAS_FORWARD_HOOK
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Include/NocLighting.hlsl"

            // in-scatter of one light: colour x distance attenuation x the HG phase relative to isotropic
            half3 StScatter(Light light, float3 V, float g)
            {
                float c = dot(-light.direction, V);
                float hg = (1.0 - g * g) / pow(max(1.0 + g * g - 2.0 * g * c, 1e-3), 1.5);
                return light.color * (light.distanceAttenuation * light.shadowAttenuation * hg);
            }

            half3 NocForwardHook(NocFragData f, SurfaceData s, InputData inputData)
            {
                float3 V = inputData.viewDirectionWS;
                float g = _Phase;
                const half4 shadowMask = half4(1, 1, 1, 1);
                half3 sc = StScatter(GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask), V, g);
            #if defined(_ADDITIONAL_LIGHTS)
                uint pixelLightCount = GetAdditionalLightsCount();
                #if USE_CLUSTER_LIGHT_LOOP
                [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                {
                    CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
                    sc += StScatter(GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask), V, g);
                }
                #endif
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    sc += StScatter(GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask), V, g);
                LIGHT_LOOP_END
            #endif
                // + the alley lamps' irradiance at the plume baked per plume in vertex G (C# AlleyFixtures.SteamLampByte: no range
                //   window, isotropic): the realtime r 7 window left the s 24 plume nothing (AL_f01: still no lift at KS00 4K x 1995)
                half3 baked = _LampColor.rgb * (f.color.g * _LampScale);
                half3 c = s.albedo * (inputData.bakedGI + (sc + baked) * (_Scatter * NocLightCal())) + s.emission;
                return min(c, 1000.0);
            }

            #include "../Include/NocLitPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
