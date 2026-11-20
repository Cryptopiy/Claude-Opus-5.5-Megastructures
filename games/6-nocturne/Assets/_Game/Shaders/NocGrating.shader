// NocGrating - open steel walkway grating (Look v2, TEXTURE_PLAN 3.5 SU-3; CRIT0 #5 / #26, KS02's catwalk and the fire-escape
// landings): a thin two-sided plate (Cull Off) whose open mesh lets the sky and the walls show through, by alpha to coverage
// (AlphaToMask On under MSAA 4x, ZWrite On, AlphaTest queue). The mesh is analytic and exactly filtered, so it neither shimmers
// nor moires: two families of bars in planar world xz (pitch _BarPitch = 62.5 mm both ways, widths _BarW0 along x lines and
// _BarW1 along z lines: mean coverage 1 - (1 - 0.16)(1 - 0.224) = 0.35), each the box-filtered coverage of NocFilteredGrid1 that
// blends to its mean once the pitch falls under 3 px (beyond about 17 m at 1080p); the bars' depth _BarDepth closes the gaps at
// grazing view (projected width w + depth tan(theta) across each family), as a real grating reads solid from a low angle.
// Shading: the lit template (MetalPaint token x a two-octave value-noise macro +-15 % and rust-dark speckle, wet-film smoothness),
// the normal faces the viewer's side (NOC_FRONT_FACE). Depth-type passes cut the plate at coverage 0.5. No keyword, no texture.
Shader "Megastructure/NocGrating"
{
    Properties
    {
        _BaseColor ("Albedo token (linear)", Color) = (0.0497, 0.0513, 0.0545, 1)
        _Smoothness ("Smoothness (wet film)", Range(0, 0.95)) = 0.5
        _MacroAlbedo ("Macro albedo +-", Range(0, 0.5)) = 0.15
        _BarPitch ("Bar pitch (m)", Float) = 0.0625
        _BarW0 ("Bar width, lines across x (m)", Float) = 0.010
        _BarW1 ("Bar width, lines across z (m)", Float) = 0.014
        _BarDepth ("Bar depth (m)", Float) = 0.03
        _AO ("Vertex AO strength", Range(0, 1)) = 0.5
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
    }

    HLSLINCLUDE
    #include "Include/NocLitCore.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float _Smoothness;
        float _MacroAlbedo;
        float _BarPitch;
        float _BarW0;
        float _BarW1;
        float _BarDepth;
        float _AO;
        float _Cull;
    CBUFFER_END

    // Coverage of the bar mesh at P for the view direction v (unit, toward the eye) and pixel footprint fp (m).
    float NocGratingCover(float3 P, float3 v, float fp)
    {
        float vy = max(abs(v.y), 0.05);
        float p = max(_BarPitch, 1e-3);
        float w0 = min(_BarW0 + _BarDepth * abs(v.x) / vy, p);     // lines at x = k p (bars run along z): projected across x
        float w1 = min(_BarW1 + _BarDepth * abs(v.z) / vy, p);
        float gx = NocFilteredGrid1(P.x, p, w0, fp, 3.0);
        float gz = NocFilteredGrid1(P.z, p, w1, fp, 3.0);
        return saturate(1.0 - (1.0 - gx) * (1.0 - gz));
    }

    // the plate is horizontal: the planar footprint of the fragment (anisotropic at grazing view) from its view geometry
    float NocGratingFootprint(NocFragData f)
    {
        return f.footprint / max(abs(f.viewDirWS.y), 0.2);
    }

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        float3 P = f.positionWS;
        float3 n = f.normalWS * f.faceSign;                                   // the side facing the viewer
        if (dot(n, f.viewDirWS) < 0.0) n = -n;
        f.normalWS = n;
        float cover = NocGratingCover(P, f.viewDirWS, NocGratingFootprint(f));
        float2 mp = P.xz / 3.0;
        float macro = 0.65 * NocValueNoise(mp) + 0.35 * NocValueNoise(mp * 3.1 + 17.3);
        // rust-dark speckle at 0.4 m, faded to its mean below 2 px of its scale (no sparkle)
        float keep = smoothstep(2.0, 4.0, 0.4 / max(NocGratingFootprint(f), 1e-5));
        float rust = lerp(0.5, NocValueNoise(P.xz / 0.4 + 5.3), keep);
        float ao = lerp(1.0, f.color.r, _AO);
        s.albedo = _BaseColor.rgb * (1.0 + _MacroAlbedo * (2.0 * macro - 1.0)) * lerp(1.0, 0.7, smoothstep(0.55, 0.85, rust));
        s.metallic = 0.0;
        s.smoothness = min(_Smoothness * lerp(1.0, 0.75, smoothstep(0.55, 0.85, rust)), 0.95);
        s.occlusion = ao;
        s.alpha = cover;
        s.emission = 0;
        [branch] if (NocDebugView() == 8)
        {
            float2 c = floor(P.xz / 0.25);
            float chk = abs(c.x + c.y - 2.0 * floor(0.5 * (c.x + c.y))) > 0.5 ? 1.0 : 0.6;
            f.debug = float4(float3(0.1, 0.9, 0.2) * chk * lerp(0.4, 1.0, cover), 1.0);
        }
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "Queue" = "AlphaTest" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On ZTest LEqual Cull [_Cull] AlphaToMask On
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #define NOC_PASS_FORWARD
            #define NOC_ALPHA_OUT
            #define NOC_FRONT_FACE
            #include "Include/NocLitPass.hlsl"
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
            #define NOC_HAS_CLIP_HOOK
            void NocClipHook(NocFragData f) { clip(NocGratingCover(f.positionWS, f.viewDirWS, NocGratingFootprint(f)) - 0.5); }
            #include "Include/NocLitPass.hlsl"
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
            #define NOC_HAS_CLIP_HOOK
            void NocClipHook(NocFragData f) { clip(NocGratingCover(f.positionWS, f.viewDirWS, NocGratingFootprint(f)) - 0.5); }
            #include "Include/NocLitPass.hlsl"
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
            #define NOC_HAS_CLIP_HOOK
            void NocClipHook(NocFragData f) { clip(NocGratingCover(f.positionWS, f.viewDirWS, NocGratingFootprint(f)) - 0.5); }
            #include "Include/NocLitPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
