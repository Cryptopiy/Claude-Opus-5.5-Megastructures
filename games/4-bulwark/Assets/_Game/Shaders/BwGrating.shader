// BwGrating - see-through bar grating on a flat panel (MASTER_PROMPT 14.2.6, 14.5.3 slots, 14.5.7 tunnel panel, 14.7 H7;
// Part 10 #3). Bearing bars 40 mm wide at 80 mm pitch (TC_GratingBar / TC_GratingPitch), drawn as ANALYTIC coverage on
// a quad and resolved by alpha-to-coverage under MSAA 4x (opaque queue, depth written per sample, no sorting):
//   * coverage across the bars = a periodic pulse train filtered with a triangle kernel of the pixel footprint along the
//     bar-normal axis (Quilez "more checker filtering": steadier than the box under motion), exact at any pitch;
//   * the bars have depth (_BarDepth 30 mm): seen at an angle their flank widens the apparent bar by
//     depth * |v_c| / |v_n| (clamped to the pitch), shaded darker, so the grating closes up physically toward grazing;
//   * below _FadePx (at least BW_GRATING_FADE_MIN = 6 px per period, fix wave X2 S3-3) the pattern fades to its mean
//     coverage (50 % at normal view, the "50 % grey" of 14.2.6), so no moire can form at any pitch (gone completely at
//     3 px; the material's 3 px left a beat band and a crawl at 3-6 px per period on the spillway deck, x2-seq s 520-640);
//   * the triangle kernel is 1.25 x the footprint (BW_GRATING_FILTER_K): each bar edge spans more of the 4 A2C levels, so
//     a walked-over bar slides instead of stepping;
// Bars run along world z on horizontal panels (_BarAxis 0; 1 = along x) and vertically on wall panels (tunnel). Painted
// steel from the BwTexGen "Steel" set; walked tops worn and polished (lighter, smoother paint; dielectric: no metal term),
// weathering smoothness +-0.08 at 6 m. Double sided; no shadow casting (bars thinner than a
// shadow texel at 150 m cascades would alias). Depth passes clip at coverage 0.5.
Shader "Megastructure/BwGrating"
{
    Properties
    {
        _BaseColor ("Paint colour (linear)", Color) = (0.112, 0.144, 0.171, 1)
        _BarWidth ("Bar width (m)", Float) = 0.04
        _BarPitch ("Bar pitch (m)", Float) = 0.08
        _BarDepth ("Bar depth (m)", Float) = 0.03
        _BarAxis ("Horizontal panels: 0 bars along z, 1 along x", Range(0, 1)) = 0
        _FadePx ("Fade below (px per period)", Float) = 3
        _Smoothness ("Paint smoothness offset (0.52 = texture)", Range(0, 0.9)) = 0.42
        _TopWear ("Polished tops", Range(0, 1)) = 0.35
        _TexScale ("Texture tile (m)", Float) = 1.0
        [NoScaleOffset] _AlbedoMap ("Albedo (sRGB, BwTexGen Steel)", 2D) = "grey" {}
        [NoScaleOffset] _NormalMap ("Normal (BC5 RG)", 2D) = "bump" {}
        [NoScaleOffset] _MaskMap ("Mask", 2D) = "grey" {}
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
        _AlphaToMask ("Alpha to coverage", Float) = 1
    }

    HLSLINCLUDE
    #include "Include/BwLitCore.hlsl"

    #define BW_GRATING_FADE_MIN 6.0      // px per period where the fade to the mean starts (fully mean at half of it)
    #define BW_GRATING_FILTER_K 1.25     // triangle kernel half-width in footprints

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float _BarWidth;
        float _BarPitch;
        float _BarDepth;
        float _BarAxis;
        float _FadePx;
        float _Smoothness;
        float _TopWear;
        float _TexScale;
        float _AlphaToMask;
    CBUFFER_END

    TEXTURE2D(_AlbedoMap); SAMPLER(sampler_AlbedoMap);
    TEXTURE2D(_NormalMap);
    TEXTURE2D(_MaskMap);

    // Axis across the bars (unit, world) for a panel with geometric normal n.
    float3 BwGratingAcross(float3 n)
    {
        float3 an = abs(n);
        if (an.y >= an.x && an.y >= an.z) return _BarAxis > 0.5 ? float3(0, 0, 1) : float3(1, 0, 0);   // pal:ok (axis)
        return an.x >= an.z ? float3(0, 0, 1) : float3(1, 0, 0);                                        // pal:ok (axis)
    }

    // Coverage of the bars (top + flank) and of the tops alone at this fragment; k = how much of the pattern is drawn (1 =
    // resolved, 0 = faded to the mean).
    void BwGratingCover(BwFragData f, out float cov, out float covTop, out float k)
    {
        float3 ng = f.normalGeoWS;
        float3 a = BwGratingAcross(ng);
        float c = dot(f.positionWS, a);
        float pitch = max(_BarPitch, 1e-3);
        float fwc = max(abs(ddx(c)) + abs(ddy(c)), 1e-6);
        float h = fwc / pitch;                                              // footprint in periods
        float vn = max(abs(dot(f.viewDirWS, ng)), 0.05);
        float vc = dot(f.viewDirWS, a);
        float side = _BarDepth * abs(vc) / vn;
        float wTop = saturate(_BarWidth / pitch);
        float wEff = saturate((_BarWidth + side) / pitch);
        float start = vc > 0.0 ? 0.0 : (wTop - wEff);                       // flank on the far side of the bar
        float x = c / pitch;
        float fadePx = max(_FadePx, BW_GRATING_FADE_MIN);
        k = saturate((1.0 / h - 0.5 * fadePx) / (0.5 * fadePx));          // 1 at fadePx px per period, 0 at half
        float hf = BW_GRATING_FILTER_K * h;
        covTop = lerp(wTop, BwPulseTri(x, wTop, hf), k);
        cov = lerp(wEff, BwPulseTri(x - start, wEff, hf), k);
        cov = max(cov, covTop);
    }

    // Depth and shadow passes only (each of those passes defines BW_HAS_CLIP_HOOK itself; the forward pass resolves the
    // partial coverage by alpha-to-coverage instead of clipping). The threshold follows the fade (0.5 k): where the pattern
    // is faded the panel is solid in the depth texture too, so no binary moire of the bars is left there.
    void BwClipHook(BwFragData f)
    {
        float cov, covTop, k;
        BwGratingCover(f, cov, covTop, k);
        clip(cov - 0.5 * k);
    }

    #define BW_ALPHA_OUT
    void BwSurfaceHook(inout BwFragData f, inout SurfaceData s)
    {
        // double sided: shade the face toward the viewer
        float facing = dot(f.normalGeoWS, f.viewDirWS) >= 0.0 ? 1.0 : -1.0;
        f.normalGeoWS *= facing;
        f.normalWS *= facing;
        float cov, covTop, kPat;
        BwGratingCover(f, cov, covTop, kPat);
        float flank = saturate(cov - covTop) / max(cov, 1e-3);

        BwTri t0 = BwTriSetup(f.positionWS, f.normalGeoWS, _TexScale, 6.0);
        float3 texAlb = BwTriSample(TEXTURE2D_ARGS(_AlbedoMap, sampler_AlbedoMap), t0).rgb;
        float4 mask = BwTriSample(TEXTURE2D_ARGS(_MaskMap, sampler_AlbedoMap), t0);
        float3 alb = BwRecolour(texAlb, _BaseColor.rgb, PAL_SteelPaint);
        // walked tops: paint worn thin and polished (lighter, smoother), still a dielectric (8.11.1: metallic is 0 or 1 by a
        // mask; bare metal on 40 mm bars is sub-pixel beyond a few metres, so the grating keeps no metal term)
        float wornTop = _TopWear * smoothstep(0.6, 0.9, f.normalGeoWS.y) * (1.0 - flank);
        alb = lerp(alb, alb * 1.6, wornTop);
        s.albedo = alb * lerp(1.0, 0.5, flank);
        s.metallic = 0.0;
        // weathering smoothness +-0.08 at 6 m (footprint-filtered, 8.11.4)
        float macroS = BwMacroSigned3(BwFbmFiltered3(f.positionWS / 6.0 + 4.1, f.footprint / 6.0, 3));
        float paintS = saturate(mask.a + (_Smoothness - 0.52) + 0.08 * macroS);
        s.smoothness = lerp(paintS, max(paintS, 0.55), wornTop) * lerp(1.0, 0.8, flank);
        s.occlusion = mask.g * lerp(1.0, 0.6, flank);
        s.emission = 0;
        s.alpha = cov;
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "Queue" = "AlphaTest" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On ZTest LEqual Cull [_Cull] AlphaToMask [_AlphaToMask]
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
            #define BW_HAS_CLIP_HOOK
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
            #define BW_HAS_CLIP_HOOK
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
            #define BW_HAS_CLIP_HOOK
            #define BW_PASS_DEPTHNORMALS
            #include "Include/BwLitPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
