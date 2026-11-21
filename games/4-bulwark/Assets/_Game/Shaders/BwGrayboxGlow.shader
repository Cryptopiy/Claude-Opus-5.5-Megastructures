// BwGrayboxGlow - the one lit stand-in material of the graybox (MASTER_PROMPT Part 13 graybox gate, 14.5 "Graybox:" lines).
// Built on the BwLitCore / BwLitPass contract (four passes, SRP Batcher, BwFogApply through the shared pass). Colours only
// through palette macros (PAL_<Token>, selected by _Token) or the 14.7 H3 water model (TC_* constants); GrayboxPath
// creates the runtime materials (Shader.Find), one per use:
//   _Mode 0  emissive token x _Intensity (lamps x2.5 / x3, lantern x6: the 14.2.5 clamps are respected by the caller); with
//            _ObjSize > 0 the emission fades out below 3.5 px of projected size (0 at 1.5 px), so a distant lamp never becomes
//            an isolated bright pixel (14.10.3, Part 10 #4 / #15)
//   _Mode 1  water veil behind the beat 6 glass: L_c = G S_c exp(-Kd_c z) exp(beta sin e) (14.7 H3), z = eye depth below the
//            sea, e = view elevation; red floor 6 % of G, hard floor 0.0018 (sRGB 6); the "vertical gradient" of the graybox
//   _Mode 2  matte palette albedo x _Intensity (sea plane, foam strip, gates, cart, steel), lit by the shared lighting
//   _Mode 3  bright opening: BwSkyColor along the view ray x _Intensity, clamped to 12 (14.7 H8) (tunnel mouth, conduit)
//   _Mode 4  surf foam (beat 4 graybox "flat surf strip with foam"): SeaStorm water with FoamWhite x _Intensity patches; the
//            mesh gives the foam density in uv0.x (1 at the wall and on the islet skirts, 0 at the strip's outer edge), a
//            footprint-filtered value-noise fBm (cells _FoamCell m) thresholds it, and the patches fade to their expected
//            coverage before they alias (no shimmer); smoothness follows the coverage
// Replaced beat by beat by the real materials of Phase 4 (delete the graybox part with it).
Shader "Megastructure/BwGrayboxGlow"
{
    Properties
    {
        _Mode ("Mode (0 emissive, 1 veil, 2 matte, 3 opening, 4 foam)", Float) = 0
        _Token ("Palette token (GbToken index)", Float) = 0
        _Intensity ("Emission / albedo scale", Float) = 1
        _Smoothness ("Smoothness", Range(0, 0.95)) = 0.3
        _ObjSize ("Mode 0: object size for the min-pixel fade (m, 0 = off)", Float) = 0
        _FoamCell ("Mode 4: foam noise cell (m)", Float) = 9
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "Include/BwLitCore.hlsl"
    #include "Generated/BulwarkConstGen.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _Mode;
        float _Token;
        float _Intensity;
        float _Smoothness;
        float _ObjSize;
        float _FoamCell;
    CBUFFER_END

    // Palette token by index (C# twin: GrayboxPath.GbToken). No literal colours (T-PAL).
    float3 GbToken(int t)
    {
        if (t == 0) return PAL_SodiumWarm;
        if (t == 1) return PAL_Tungsten;
        if (t == 2) return PAL_Lantern;
        if (t == 3) return PAL_FoamWhite;
        if (t == 4) return PAL_SeaStorm;
        if (t == 5) return PAL_SteelPaint;
        if (t == 6) return PAL_ConcreteDark;
        if (t == 7) return PAL_SafetyOchre;
        if (t == 8) return PAL_SeaTeal;
        if (t == 9) return PAL_StormHorizon;
        if (t == 10) return PAL_LaneWhite;
        return PAL_ConcreteMid;
    }

    // 14.7 H3 veil radiance seen from an eye at depth z looking along dir (unit). G is set so linear G = 0.050 at z 44.3, e 0.
    float3 GbVeil(float3 dir)
    {
        float z = max(TC_SeaY - _WorldSpaceCameraPos.y, 0.0);
        float se = clamp(dir.y, -1.0, 1.0);
        float g = TC_VeilG0 / max(TC_VeilSG * exp(-TC_KdG * TC_VeilG0Depth), 1e-6);
        float3 kd = float3(TC_KdR, TC_KdG, TC_KdB);            // pal:ok (diffuse attenuation 1/m, not a colour)
        float3 sc = float3(TC_VeilSR, TC_VeilSG, TC_VeilSB);   // pal:ok (veil spectral scale of 14.7 H3, not a palette colour)
        float3 l = g * sc * exp(-kd * z) * exp(TC_VeilBeta * se);
        l.r = max(l.r, TC_VeilRedFloor * l.g);
        return max(l, TC_DarkFloorLinear);
    }

    // Foam coverage in [0, 1] at world xz for a mesh density d (0..1) and pixel footprint fp (m): a 3-octave fBm thresholded
    // at lerp(0.85, 0.30, d); the thresholded pattern fades to its expected coverage (fraction of the fBm above the
    // threshold, fBm p5..p95 about 0.26..0.74) once the first octave's cell is under 4 footprints.
    float GbFoamCover(float2 xz, float d, float fp)
    {
        float cell = max(_FoamCell, 0.5);
        float fpCells = fp / cell;
        float n = BwFbmFiltered(xz / cell, fpCells, 3);
        float t = lerp(0.85, 0.30, d);
        float sharp = smoothstep(t - 0.08, t + 0.08, n);
        float expect = saturate((0.74 - t) / 0.48) * step(0.001, d);
        float detail = 1.0 - smoothstep(0.25, 0.5, fpCells);
        return lerp(expect, sharp, detail);
    }

    void BwSurfaceHook(inout BwFragData f, inout SurfaceData s)
    {
        int mode = (int)round(_Mode);
        float3 tok = GbToken((int)round(_Token));
        float3 ray = -f.viewDirWS;   // from the eye toward the fragment
        s.smoothness = _Smoothness;
        s.albedo = 0;
        s.emission = 0;
        if (mode == 0)
        {
            float px = _ObjSize > 0.0 ? BwPxFade(_ObjSize, f.footprint, 1.5, 3.5) : 1.0;
            s.emission = tok * max(_Intensity, 0.0) * px;
            s.smoothness = 0.0;
        }
        else if (mode == 4)
        {
            // the footprint ACROSS the pattern: at grazing views over the sea the screen derivatives of xz are many times the
            // isotropic distance footprint (Wave B: the materials pans measured 2.16 levels of shimmer on the patch edges)
            float2 dxz = ddx(f.positionWS.xz), dyz = ddy(f.positionWS.xz);
            float fpA = max(f.footprint, max(length(dxz), length(dyz)));
            float cov = GbFoamCover(f.positionWS.xz, saturate(f.uv.x), fpA);
            s.albedo = saturate(lerp(PAL_SeaStorm, PAL_FoamWhite * max(_Intensity, 0.0), cov));
            s.smoothness = lerp(_Smoothness, 0.1, cov);
        }
        else if (mode == 1)
        {
            s.emission = GbVeil(ray);
            s.smoothness = 0.0;
        }
        else if (mode == 3)
        {
            s.emission = min(BwSkyColor(ray) * max(_Intensity, 0.0), TC_ClampOpening);
            s.smoothness = 0.0;
        }
        else
        {
            s.albedo = saturate(tok * max(_Intensity, 0.0));
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
