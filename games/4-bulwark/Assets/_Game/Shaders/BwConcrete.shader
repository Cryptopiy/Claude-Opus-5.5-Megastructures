// BwConcrete - the reference material of the BwLitCore / BwLitPass contract (structure imported from The Ring's
// RingCeramic.shader; recipe MASTER_PROMPT 14.2.6, quality bar 8.11). Colours come from MaterialFactory (palette tokens
// ConcreteMid / ConcreteDark / ConcreteWet, linear); `_Wet` blends dry -> wet, `_WetFollow` adds the rain wetness of the
// atmosphere (_BwSurface.x: the road and exposed concrete are darkened outside the sun-break window).
// Scale ladder (8.11.3): micro + meso from the BwTexGen "Concrete" set (bug holes, aggregate, plywood grain, 1 mm texels on
// a 2.0 m tile) sampled world-space triplanar twice (2.0 m and a rotated 5.37 m copy mixed 33-60 % by a 9 m mask: never one
// pure tile, no visible tiling even at grazing angles); meso formwork panels 3.0 x 1.5 m with 12 mm gaps and tie holes on 0.6 m on cast faces (walls, soffits; not on
// trowelled top faces), box-filtered by the per-axis footprint (abs(ddx) + abs(ddy) of the position on the panel's
// axes: correct at grazing angles) and faded once their spacing drops below 4 px along that axis; per-panel pour tone +-2.5 %; drip
// stains below the panel joints on vertical faces; macro tone +-6 % at 40 m and smoothness +-0.08 at 23 m (footprint-filtered fBm); 2 cm chamfers
// carry vertex G = edge wear (lighter, rougher), vertex R = AO, vertex B = extra drip source. Smoothness varies by about
// 0.2 across a wall (texture meso noise + pores + wear + wetness: 8.11.4 asks for at least 0.1). Sun tint x1.9: the main
// light's chroma is scaled 1.9x around its luminance (luminance and LightCal unchanged), so the gold sun break reads on
// grey concrete (docs/MATERIALS.md, DEVELOPMENT.md).
// Copy this file's structure for new lit materials: one UnityPerMaterial cbuffer identical in all passes,
// `_CLUSTER_LIGHT_LOOP` declared, `_SHADOWS_SOFT` only, `multi_compile_instancing` in every pass.
Shader "Megastructure/BwConcrete"
{
    Properties
    {
        _BaseColor ("Dry colour (linear)", Color) = (0.212, 0.223, 0.220, 1)
        _WetColor ("Wet colour (linear)", Color) = (0.037, 0.045, 0.053, 1)
        _Wet ("Wetness", Range(0, 1)) = 0
        _WetFollow ("Follow rain wetness (_BwSurface.x)", Range(0, 1)) = 0
        _Smoothness ("Smoothness offset dry (0.30 = texture)", Range(0, 0.9)) = 0.30
        _WetSmoothness ("Smoothness wet", Range(0, 0.95)) = 0.72
        _PanelSize ("Formwork panel (m)", Vector) = (3.0, 1.5, 3.0, 0)
        _PanelGap ("Panel gap (m)", Float) = 0.012
        _TieSpacing ("Tie hole spacing (m)", Float) = 0.6
        _TieRadius ("Tie hole radius (m)", Float) = 0.012
        _MacroTone ("Macro tone amplitude", Range(0, 0.2)) = 0.06
        _AO ("Vertex AO strength", Range(0, 1)) = 1
        _EdgeWear ("Edge wear strength", Range(0, 1)) = 0.5
        _TexScale ("Texture tile (m)", Float) = 2.0
        _NormalStrength ("Normal strength", Range(0, 2)) = 1.0
        _DetailMix ("Second sample mix", Range(0, 1)) = 0.6
        _DripStrength ("Drip stains", Range(0, 1)) = 0.6
        _SunTint ("Sun tint (chroma x)", Range(1, 3)) = 1.9
        [NoScaleOffset] _AlbedoMap ("Albedo (sRGB, BwTexGen Concrete)", 2D) = "grey" {}
        [NoScaleOffset] _NormalMap ("Normal (BC5 RG)", 2D) = "bump" {}
        [NoScaleOffset] _MaskMap ("Mask (metal, AO, height, smoothness)", 2D) = "grey" {}
        [HDR] _Emission ("Emission (linear HDR)", Color) = (0, 0, 0, 1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "Include/BwLitCore.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float4 _WetColor;
        float4 _PanelSize;
        float4 _Emission;
        float _Wet;
        float _WetFollow;
        float _Smoothness;
        float _WetSmoothness;
        float _PanelGap;
        float _TieSpacing;
        float _TieRadius;
        float _MacroTone;
        float _AO;
        float _EdgeWear;
        float _TexScale;
        float _NormalStrength;
        float _DetailMix;
        float _DripStrength;
        float _SunTint;
    CBUFFER_END

    TEXTURE2D(_AlbedoMap); SAMPLER(sampler_AlbedoMap);
    TEXTURE2D(_NormalMap);
    TEXTURE2D(_MaskMap);

    // Triplanar dominant-axis projection: q = in-plane coordinates, fq = their per-axis pixel footprints (abs(ddx) +
    // abs(ddy) of the world position, projected on the same axes: on a wall seen at a grazing angle the footprint along
    // the receding axis is many times the isotropic distance footprint), size = panel size along them.
    void BwConcretePlane(float3 p, float3 fw, float3 n, out float2 q, out float2 fq, out float2 size)
    {
        float3 an = abs(n);
        if (an.y >= an.x && an.y >= an.z) { q = p.xz; fq = fw.xz; size = _PanelSize.xz; }
        else if (an.x >= an.z) { q = p.zy; fq = fw.zy; size = _PanelSize.zy; }
        else { q = p.xy; fq = fw.xy; size = _PanelSize.xy; }
    }

    // Filtered formwork gaps (0..1 coverage of the gap colour): each line train is box-filtered by the footprint across
    // its own lines (fq.x for the joints at x = k size.x, fq.y for the ones at y = k size.y) and faded once its spacing
    // drops below 4 px along that axis. Coordinates are wrapped by the period first (precision).
    float BwConcreteGaps(float2 q, float2 size, float2 fq)
    {
        float2 w = q - size * floor(q / size);
        float gx = BwFilteredGrid1(w.x, size.x, _PanelGap, fq.x, 4.0);
        float gy = BwFilteredGrid1(w.y, size.y, _PanelGap, fq.y, 4.0);
        return saturate(gx + gy - gx * gy);
    }

    // Tie holes: discs of radius r on a square grid of spacing sp. Near (footprint under r / 2): the disc with an
    // anisotropic edge width (the directional footprint of the distance, |c.x| fq.x + |c.y| fq.y over |c|); far (footprint
    // over 1.5 r): the equal-area square (side r sqrt(pi)) as the exact product of two 1D box filters with fq.x and fq.y,
    // which converges to the hole's area fraction; faded once the spacing drops below 4 px on the coarser axis.
    float BwConcreteTies(float2 q, float2 fq)
    {
        float sp = max(_TieSpacing, 1e-3);
        float r = max(_TieRadius, 1e-4);
        float2 c = (q / sp - floor(q / sp) - 0.5) * sp;                  // metres from the nearest tie centre
        float d = length(c);
        float fd = (abs(c.x) * fq.x + abs(c.y) * fq.y) / max(d, 1e-6);
        float disc = saturate((r - d) / max(fd, 1e-6) + 0.5);
        float side = 1.7724539 * r;
        float box = BwBoxCover(c.x, 0.0, side, fq.x) * BwBoxCover(c.y, 0.0, side, fq.y);
        float fmax = max(fq.x, fq.y);
        float cov = lerp(disc, box, saturate(fmax / r - 0.5));
        float fade = saturate((sp / max(fmax, 1e-6) - 4.0) / 4.0);
        return cov * fade;
    }

    // Main-light response ("sun tint x1.9", 14.2.6): the key's chroma scaled by _SunTint around its luminance (luminance
    // and LightCal kept), but never past a channel ratio min/max of 0.6 nor below the key's own ratio: a near-white key
    // (noon #FFF3E3) warms the sunlit concrete by up to x1.9, an already saturated key (sun-break gold, storm, dusk) is left
    // as it is. The literal x1.9 on BreakGold turned grey concrete into saturated orange (wa-mat-01, logged decision).
    #define BW_HAS_LIGHT_HOOK
    void BwMainLightHook(inout half3 color, half3 dirWS)
    {
        float y = max(BwLuma(color), 1e-5);
        float3 c = color;
        float cmin = min(c.r, min(c.g, c.b)), cmax = max(c.r, max(c.g, c.b));
        float ratio0 = cmin / max(cmax, 1e-5);
        float floorRatio = min(0.6, ratio0);
        // largest k with (y + k (cmin - y)) / (y + k (cmax - y)) >= floorRatio
        float num = y * (1.0 - floorRatio), den = floorRatio * (cmax - y) - (cmin - y);
        float kmax = den > 1e-6 ? num / den : _SunTint;
        float k = clamp(min(_SunTint, kmax), 1.0, _SunTint);
        color = max(y + (c - y) * k, 0.0);
    }

    void BwSurfaceHook(inout BwFragData f, inout SurfaceData s)
    {
        float3 p = f.positionWS;
        float3 ng = f.normalGeoWS;
        float fp = f.footprint;
        float3 fw = abs(ddx(p)) + abs(ddy(p));                // per-axis footprints (uniform control flow)
        float top = smoothstep(0.6, 0.85, ng.y);            // trowelled top faces: no formwork
        float vertical = 1.0 - saturate(abs(ng.y) * 1.5);    // walls

        // ---- texture set: 2.0 m sample + rotated 5.37 m sample mixed by a ~9 m mask (breaks the tile period)
        BwTri t0 = BwTriSetup(p, ng, _TexScale, 6.0);
        BwTri t1 = BwTriDetune(t0, 0.3727, float2(0.31, 0.57));
        float mixK = _DetailMix * lerp(0.55, 1.0, smoothstep(0.25, 0.75, BwValueNoise3(p * 0.11 + 3.7)));   // never one pure tile
        float4 alb0 = BwTriSample(TEXTURE2D_ARGS(_AlbedoMap, sampler_AlbedoMap), t0);
        float4 alb1 = BwTriSample(TEXTURE2D_ARGS(_AlbedoMap, sampler_AlbedoMap), t1);
        float4 msk0 = BwTriSample(TEXTURE2D_ARGS(_MaskMap, sampler_AlbedoMap), t0);
        float4 msk1 = BwTriSample(TEXTURE2D_ARGS(_MaskMap, sampler_AlbedoMap), t1);
        float3 texAlb = lerp(alb0.rgb, alb1.rgb, mixK);
        float4 mask = lerp(msk0, msk1, mixK);
        float3 n0 = BwTriNormal(TEXTURE2D_ARGS(_NormalMap, sampler_AlbedoMap), t0, ng, _NormalStrength);
        float3 n1 = BwTriNormal(TEXTURE2D_ARGS(_NormalMap, sampler_AlbedoMap), t1, ng, _NormalStrength * 0.8);
        float3 n = normalize(lerp(n0, n1, mixK));

        // ---- macro tone +-6 % at 40 m and weathering smoothness +-0.08 at 23 m (decorrelated; footprint-filtered fBm so
        //      it never aliases; 3D lattices: no streaks on walls of any orientation; BwMacroSigned3 stretches the fBm to
        //      about +-1 at its 5th / 95th percentiles, 8.11.4)
        float macro = BwFbmFiltered3(p / 40.0, fp / 40.0, 3);
        float tone = 1.0 + _MacroTone * (2.0 * macro - 1.0);
        float macroS = BwMacroSigned3(BwFbmFiltered3(p / 23.0 + 7.7, fp / 23.0, 3));

        // ---- formwork on cast faces: gaps, ties, per-panel pour tone
        float2 q, fq, size;
        BwConcretePlane(p, fw, ng, q, fq, size);
        // formwork is flat plywood: it fades on curved surfaces (curvature from the geometric normal's screen derivative,
        // in 1/m: radius under ~4 m has no panels; facet edges of chamfers give 1 px spikes where the gap fades anyway)
        float curv = length(abs(ddx(ng)) + abs(ddy(ng))) / max(fp, 1e-5);
        float cast = (1.0 - top) * (1.0 - smoothstep(0.12, 0.3, curv));
        float gap = BwConcreteGaps(q, size, fq) * cast;
        float tie = BwConcreteTies(q, fq) * cast;
        float panelPx = min(size.x / max(fq.x, 1e-6), size.y / max(fq.y, 1e-6));
        float panelFade = saturate((panelPx - 4.0) / 4.0) * cast;
        float2 cell = floor(q / max(size, 1e-3));
        float pour = (BwHash21(int2(cell)) - 0.5) * 0.05 * panelFade;

        // ---- drip stains on walls: vertical streaks hanging below each horizontal joint, faded by the footprint
        float row = q.y / max(size.y, 1e-3);
        float below = (1.0 - frac(row)) * size.y;                        // metres below the joint above
        bool alongZ = abs(ng.x) >= abs(ng.z);
        float along = alongZ ? p.z : p.x;
        float fAlong = alongZ ? fw.z : fw.x;                              // footprint across the streaks
        float streak = BwValueNoise(float2(along * 2.3, p.y * 0.09 + floor(row) * 7.1));
        float dripFade = BwPxFade(0.45, max(fAlong, fw.y), 2.0, 4.0);
        float drip = _DripStrength * (0.6 + f.color.b) * vertical * dripFade
                   * smoothstep(0.5, 0.85, streak) * exp(-below / 1.1);
        drip = saturate(drip);

        // ---- wetness: material + rain (top faces fully, walls in streaks, soffits stay dry)
        float rain = _WetFollow * saturate(_BwSurface.x) * saturate(top + vertical * (0.35 + 0.65 * drip));
        float wetK = saturate(max(_Wet, rain));

        // ---- albedo
        float ao = lerp(1.0, f.color.r, _AO);
        float wear = f.color.g * _EdgeWear;
        float3 dry = BwRecolour(texAlb, _BaseColor.rgb, PAL_ConcreteMid);
        float3 wet = BwRecolour(texAlb, _WetColor.rgb, PAL_ConcreteMid);
        float3 alb = lerp(dry, wet, wetK) * (tone + pour);
        alb *= (1.0 - 0.45 * gap) * (1.0 - 0.6 * tie) * (1.0 - 0.22 * drip);
        alb = lerp(alb, alb * 1.12, wear);                              // chamfers: fresh, lighter fractures
        s.albedo = alb * lerp(0.82, 1.0, ao);

        // ---- smoothness: texture (meso noise, pores, Toksvig mips) + offset, wet film fills the lows
        float dryS = saturate(mask.a + (_Smoothness - 0.30)) * (1.0 - 0.3 * gap) - 0.08 * wear + 0.04 * drip;
        float wetS = _WetSmoothness * (0.85 + 0.15 * (1.0 - mask.b));
        dryS += 0.08 * macroS;                                            // weathering patches at 23 m (8.11.4)
        s.smoothness = saturate(lerp(dryS, wetS, wetK));
        s.metallic = 0.0;
        s.occlusion = ao * mask.g * (1.0 - 0.5 * tie) * (1.0 - 0.25 * gap);
        s.emission = _Emission.rgb;

        // ---- normal: texture detail, flattened by a water film
        f.normalWS = normalize(lerp(n, ng, 0.6 * wetK));
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
