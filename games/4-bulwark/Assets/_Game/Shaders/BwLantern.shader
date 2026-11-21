// BwLantern - the glass drum of the Keeper's Lantern, the goal object (MASTER_PROMPT 14.5.7, 14.11, 14.2.5): Lantern token
// x6 (TC_LanternEmissive), clamped per channel so the flicker's peak is 6 (TC_ClampLantern: the base clamp is
// 6 / 1.03 = 5.83, then x gain), with an irregular +-3 % flicker that is a sum of
// three sines at 0.31, 0.77 and 0.97 Hz with weights 1 : 0.6 : 0.3 (TC_Lantern*), normalised by 1.9, evaluated on the
// ambient clock _BwTime.x (frozen in capture stills; mod 3600 s is a whole number of periods of all three, so no step).
// Nothing above 1 Hz (photosensitivity rule). C# twin: Megastructure.LanternFlicker.Gain(t) (MaterialLibrary.cs; the
// beat-7 builder drives the 5 m point light with it so glass and light agree; MaterialTests compare them).
// The flame core reads brighter through the middle of the drum (view-facing), the edges fall off like thick glass.
Shader "Megastructure/BwLantern"
{
    Properties
    {
        _EmissionColor ("Emission colour (linear token)", Color) = (1.0, 0.462, 0.122, 1)
        _Intensity ("Intensity (x token)", Float) = 6.0
        _Clamp ("Radiance clamp per channel", Float) = 6.0
        _Flicker ("Flicker amplitude", Range(0, 0.1)) = 0.03
        _GlassFalloff ("Grazing falloff", Range(0, 1)) = 0.45
        _Smoothness ("Glass smoothness", Range(0, 0.95)) = 0.85
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "Include/BwLitCore.hlsl"
    #include "Generated/BulwarkConstGen.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _EmissionColor;
        float _Intensity;
        float _Clamp;
        float _Flicker;
        float _GlassFalloff;
        float _Smoothness;
    CBUFFER_END

    // Phases (rad) mirrored from LanternFlicker.Phase.
    #define BW_LANTERN_PHASE1 0.0
    #define BW_LANTERN_PHASE2 1.7
    #define BW_LANTERN_PHASE3 4.1

    float BwLanternGain(float t)
    {
        float s = TC_LanternW1 * sin(BW_TWO_PI * TC_LanternHz1 * t + BW_LANTERN_PHASE1)
                + TC_LanternW2 * sin(BW_TWO_PI * TC_LanternHz2 * t + BW_LANTERN_PHASE2)
                + TC_LanternW3 * sin(BW_TWO_PI * TC_LanternHz3 * t + BW_LANTERN_PHASE3);
        return 1.0 + _Flicker * s / (TC_LanternW1 + TC_LanternW2 + TC_LanternW3);
    }

    void BwSurfaceHook(inout BwFragData f, inout SurfaceData s)
    {
        float NoV = saturate(dot(f.normalWS, f.viewDirWS));
        // clamp so the PEAK of the flicker stays at _Clamp (6, 14.2.5): clamping after the gain would clip the R channel's
        // upper half-wave (R = 1.0 x 6 sits at the clamp) and add harmonics above 1 Hz; this keeps the flicker sinusoidal
        float3 radiance = min(_EmissionColor.rgb * _Intensity, _Clamp / (1.0 + _Flicker)) * BwLanternGain(_BwTime.x);
        s.albedo = PAL_ConcreteWet * 0.5;                    // glass body (the light is the emission)
        s.metallic = 0.0;
        s.smoothness = _Smoothness;
        s.occlusion = 1.0;
        s.emission = radiance * (1.0 - _GlassFalloff * (1.0 - NoV * NoV));
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
            #pragma vertex BwVert
            #pragma fragment BwFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #define BW_PASS_FORWARD
            #include "Include/BwLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex BwVert
            #pragma fragment BwFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #define BW_PASS_SHADOW
            #include "Include/BwLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex BwVert
            #pragma fragment BwFrag
            #pragma multi_compile_instancing
            #define BW_PASS_DEPTH
            #include "Include/BwLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex BwVert
            #pragma fragment BwFrag
            #pragma multi_compile_instancing
            #define BW_PASS_DEPTHNORMALS
            #include "Include/BwLitPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
