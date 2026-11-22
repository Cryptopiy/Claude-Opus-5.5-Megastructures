// NocScreen - the face of the Wall of signs (MASTER_PROMPT 14.2.5 NocScreen, 14.5.6, D-010; SG Phase 4 wave A, INT-02 / CRIT0 #14):
// a 2 km LED screen facade through the Noc lit template (URP BRDFData, analytic sky reflection, Forward+ lights, trilight x
// LightCal, specular AA). The emission is NocScreenShade (Shaders/Beats/NocScreenWave.hlsl, shared with the H4 wall plane):
// the slow colour wave of 14.5.6 on the Look v2 ramp (never violet), the display imagery (slow abstract programmes, no text, no
// people, every frequency <= 0.11 Hz), panels 36 x 20 m with 0.4 m gaps and bezels, per-panel white point / age / failures, 4 m
// modules with joints and calibration steps (faded below 5.5 px), 0.25 m LED pixels near the walker (faded in from 3 to 6 px)
// and the LED view-angle falloff. The black LED face is the albedo; the bezels are painted metal that catches the lights.
// Photosensitivity (14.5.6): every modulation <= 0.11 Hz, no flashing, no pop (failures are static); emission peak below the bloom
// threshold 0.9 (Beat06SignsTests). Time arrives as the global _NocScreenT (SignsDriver, double, wrapped), so no term jumps.
// Fix wave: the eye (D-034 5.3, C28) on a 4 x 5 panel canvas once per 600 s; the face dissolves into the deck colour from y 230 to
// 430 (C17). Then NocFog (cloud mix above y 380, fog toward NocSkyColor) and NocFloor last (NocLitPass).
Shader "Megastructure/NocScreen"
{
    Properties
    {
        _BaseColor ("Screen albedo (linear)", Color) = (0.0125, 0.0130, 0.0140, 1)
        _Smooth ("Screen smoothness", Range(0, 0.95)) = 0.35
        _EmitGain ("Wall emission x panel level (14.5.6: 0.45)", Float) = 0.45
        _Level ("Panel level (14.5.6: 0.55)", Float) = 0.55
        _Breath ("Panel breath amplitude (D-010: 0.05)", Float) = 0.05
        _SeamDark ("Seam darkening (unused since the LED facade: the gap scatters 10 %)", Range(0, 1)) = 0.85
        _Waves ("Waves (1 simplified, 3 full)", Float) = 3
        _Content ("Imagery strength (1 full, 0 waves only)", Range(0, 1)) = 1
        _BezelAlbedo ("Bezel albedo (linear)", Float) = 0.055
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "../Include/NocLitCore.hlsl"
    #include "NocScreenWave.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float _Smooth;
        float _EmitGain;
        float _Level;
        float _Breath;
        float _SeamDark;
        float _Waves;
        float _Content;
        float _BezelAlbedo;
        float _Cull;
    CBUFFER_END

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        float z = f.positionWS.z, y = f.positionWS.y;
        // per-axis metres per pixel (the face is planar, z and y are linear on it): before any branch
        float2 fw = max(float2(length(float2(ddx(z), ddy(z))), length(float2(ddx(y), ddy(y)))), 1e-4);
        float cosV = dot(f.normalWS, f.viewDirWS);
        NocScreenOut o = NocScreenShade(z, y, fw, cosV, _EmitGain * _Level, _Breath / max(_Level, 1e-3), _Content, _Waves);
        s.albedo = lerp(_BaseColor.rgb, _BezelAlbedo.xxx, o.metal);
        s.metallic = 0.0;
        s.smoothness = lerp(_Smooth, 0.55, o.metal);
        s.occlusion = 1.0;
        s.emission = o.emission;
        // fix wave C17 (2): the face dissolves into the deck from y 230 to 430 (wisps +-40 m): the ceiling colour above the point
        // replaces the emission and the albedo fades, so the 2 km facade has no top in the look-ups (the cloud mix of NocFog
        // starts at 380 for everything else)
        [branch] if (y > 190.0)
        {
            float blurM = NocPixelAngle() * length(f.positionWS - _WorldSpaceCameraPos.xyz);
            float fd;
            float3 cc = NocCloudColorN(f.positionWS.xz, blurM, fd);
            float kd = smoothstep(230.0, 430.0, y + 40.0 * fd);
            s.emission = lerp(s.emission, cc, kd);
            s.albedo *= 1.0 - kd;
        }
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On ZTest LEqual Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #define NOC_PASS_FORWARD
            #include "../Include/NocLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #define NOC_PASS_SHADOW
            #include "../Include/NocLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #define NOC_PASS_DEPTH
            #include "../Include/NocLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #define NOC_PASS_DEPTHNORMALS
            #include "../Include/NocLitPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
