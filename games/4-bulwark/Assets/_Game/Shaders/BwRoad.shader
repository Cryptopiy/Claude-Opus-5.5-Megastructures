// BwRoad - the crest road deck (MASTER_PROMPT 14.2.6 "BwSteel / BwRoad", 14.5.2, 8.11). Broom-finished concrete pavement
// from the BwTexGen "Road" set (transverse broom grooves, exposed aggregate, wear polish; 1 mm texels on a 1.0 m tile,
// world-space triplanar, a rotated 3.4 m copy mixed in by a macro mask), ConcreteMid -> ConcreteWet by `_Wet` and the
// rain wetness (_BwSurface.x with _WetFollow; the albedo goes _WetAlbedo 0.8 of the way: a broomed deck drains, the film
// fills the grooves), macro tone +-6 % at 40 m, weathering smoothness +-0.08 at 23 m, kerb grime toward the parapet and
// the lee kerb.
// Painted LaneWhite markings, all exact box-filtered coverages (no SDF threshold that could shimmer):
//   * edge lines 0.15 m wide at x = +-_EdgeLineX (solid), centre line at x 0 dashed _CenterDash / _CenterGap (the dash
//     train along z is filtered with a triangle kernel and converges to its mean far away);
//   * chainage numerals: two seven-segment digits 0.7 x 1.2 m, stroke 0.15, upright for a walker heading north (segment
//     a toward +z, tens digit toward -x), every 100 m from z 100 to 900 showing floor(z / 100) as 01..09 in the lee lane
//     (x -3.0) and, offset +50 m (z 150..850, 01..08), in the sea lane (x +3.0). Each segment is an axis-aligned
//     rectangle whose coverage is the product of two exact 1D box filters (footprints fwidth(x), fwidth(z)); the digit
//     layer fades out once a stroke is narrower than 3 px (gone at 1.5 px). C# twin: RoadMarkings (MaterialTests).
// Paint: LaneWhite recoloured by the pavement texture variation, smoother (0.45), flattens the micro normal.
// Deck expansion joints (fix wave X2, lead S4-6: the steel plates read as blue reflective lines alternating bright and dark
// rows): the crest's joints at z = 18 + 24 k across x -5.90..5.49 (CrestProps.ExpansionPhase / Pitch / X0 / X1) are drawn
// here as exact box-filtered coverages along z: two 0.14 m cover plates (SteelPaint albedo, smoothness capped at 0.35, the
// sky reflection cut to half by the specular occlusion) either side of a 0.04 m neoprene seal (ConcreteDark, rough). The
// joint fades into the pavement once a plate is narrower than 3 px along z (gone at 1.5 px), so a far joint is never a
// sub-pixel bright line. (The crest builder's 6 mm steel plate meshes should use this road material, or go: shared request.)
Shader "Megastructure/BwRoad"
{
    Properties
    {
        _BaseColor ("Dry colour (linear)", Color) = (0.212, 0.223, 0.220, 1)
        _WetColor ("Wet colour (linear)", Color) = (0.037, 0.045, 0.053, 1)
        _LineColor ("Marking colour (linear)", Color) = (0.687, 0.665, 0.597, 1)
        _Wet ("Wetness", Range(0, 1)) = 0
        _WetFollow ("Follow rain wetness (_BwSurface.x)", Range(0, 1)) = 1
        _Smoothness ("Smoothness offset dry (0.30 = texture)", Range(0, 0.9)) = 0.30
        _WetSmoothness ("Smoothness wet", Range(0, 0.95)) = 0.72
        _WetAlbedo ("Wet darkening (0..1 of the wet colour)", Range(0, 1)) = 0.8
        _PaintSmoothness ("Paint smoothness", Range(0, 0.9)) = 0.45
        _MacroTone ("Macro tone amplitude", Range(0, 0.2)) = 0.06
        _KerbGrime ("Kerb grime", Range(0, 0.5)) = 0.12
        _AO ("Vertex AO strength", Range(0, 1)) = 1
        _EdgeWear ("Edge wear strength", Range(0, 1)) = 0.5
        _TexScale ("Texture tile (m)", Float) = 1.0
        _NormalStrength ("Normal strength", Range(0, 2)) = 1.0
        _DetailMix ("Second sample mix", Range(0, 1)) = 0.6
        _LineWidth ("Line width (m)", Float) = 0.15
        _EdgeLineX ("Edge line |x| (m)", Float) = 5.0
        _CenterDash ("Centre dash (m)", Float) = 3.0
        _CenterGap ("Centre gap (m)", Float) = 6.0
        _DigitsOn ("Chainage numerals", Range(0, 1)) = 1
        [NoScaleOffset] _AlbedoMap ("Albedo (sRGB, BwTexGen Road)", 2D) = "grey" {}
        [NoScaleOffset] _NormalMap ("Normal (BC5 RG)", 2D) = "bump" {}
        [NoScaleOffset] _MaskMap ("Mask (metal, AO, height, smoothness)", 2D) = "grey" {}
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "Include/BwLitCore.hlsl"
    #include "Generated/BulwarkConstGen.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float4 _WetColor;
        float4 _LineColor;
        float _Wet;
        float _WetFollow;
        float _Smoothness;
        float _WetSmoothness;
        float _WetAlbedo;
        float _PaintSmoothness;
        float _MacroTone;
        float _KerbGrime;
        float _AO;
        float _EdgeWear;
        float _TexScale;
        float _NormalStrength;
        float _DetailMix;
        float _LineWidth;
        float _EdgeLineX;
        float _CenterDash;
        float _CenterGap;
        float _DigitsOn;
    CBUFFER_END

    TEXTURE2D(_AlbedoMap); SAMPLER(sampler_AlbedoMap);
    TEXTURE2D(_NormalMap);
    TEXTURE2D(_MaskMap);

    // Seven-segment masks, bit a = 1, b = 2, c = 4, d = 8, e = 16, f = 32, g = 64 (C# twin RoadMarkings.Segments).
    static const uint BwSevenSeg[10] = { 63u, 6u, 91u, 79u, 102u, 109u, 125u, 7u, 127u, 111u };

    // Exact box-filtered coverage of one digit centred at the origin of q (x right, z up), size W x H, stroke t.
    float BwDigitCover(float2 q, uint digit, float fwx, float fwz)
    {
        const float W = TC_LaneDigitW, H = TC_LaneDigitH, t = TC_LaneDigitStroke;
        uint m = BwSevenSeg[min(digit, 9u)];
        float hx = BwBoxCover(q.x, 0.0, W, fwx);                        // horizontal segments span the full width
        float vl = BwBoxCover(q.x, -0.5 * (W - t), t, fwx);             // left column
        float vr = BwBoxCover(q.x, 0.5 * (W - t), t, fwx);              // right column
        float za = BwBoxCover(q.y, 0.5 * (H - t), t, fwz);              // a (top)
        float zg = BwBoxCover(q.y, 0.0, t, fwz);                        // g (middle)
        float zd = BwBoxCover(q.y, -0.5 * (H - t), t, fwz);             // d (bottom)
        // column halves overlap through the middle stroke (no seam where b meets c or f meets e)
        float zu = BwBoxCover(q.y, 0.25 * (H - t), 0.5 * (H + t), fwz); // upper half (b, f): [-t/2, H/2]
        float zl = BwBoxCover(q.y, -0.25 * (H - t), 0.5 * (H + t), fwz);// lower half (c, e): [-H/2, t/2]
        float c = 0.0;
        if (m & 1u)  c = max(c, hx * za);
        if (m & 2u)  c = max(c, vr * zu);
        if (m & 4u)  c = max(c, vr * zl);
        if (m & 8u)  c = max(c, hx * zd);
        if (m & 16u) c = max(c, vl * zl);
        if (m & 32u) c = max(c, vl * zu);
        if (m & 64u) c = max(c, hx * zg);
        return c;
    }

    // Two-digit chainage number k (1..9 shows "0k") centred at (cx, cz); zero outside its box (margin by the footprint).
    float BwChainage(float2 xz, float cx, float cz, uint k, float fwx, float fwz)
    {
        float2 q = xz - float2(cx, cz);
        float halfW = TC_LaneDigitW + 0.1 + 2.0 * fwx, halfH = 0.5 * TC_LaneDigitH + 2.0 * fwz;
        if (abs(q.x) > halfW || abs(q.y) > halfH) return 0.0;
        float off = 0.5 * TC_LaneDigitW + 0.1;                           // digit centres at -+0.45 m (0.2 m gap)
        float a = BwDigitCover(q + float2(off, 0.0), k / 10u, fwx, fwz);
        float b = BwDigitCover(q - float2(off, 0.0), k % 10u, fwx, fwz);
        return max(a, b);
    }

    // Expansion joint coverages at p (plates, seal; 0..1), faded below 3 px of plate width along z (gone at 1.5 px).
    static const float BwJointPhase = 18.0, BwJointPitch = 24.0, BwJointPlateW = 0.14, BwJointSealW = 0.04;
    static const float BwJointX0 = -5.90, BwJointX1 = 5.49;
    float BwRoadJoint(float3 p, float fwx, float fwz, out float seal)
    {
        float k = round((p.z - BwJointPhase) / BwJointPitch);
        float zc = BwJointPhase + BwJointPitch * k;
        float off = 0.5 * (BwJointSealW + BwJointPlateW);
        float across = BwBoxCover(p.x, 0.5 * (BwJointX0 + BwJointX1), BwJointX1 - BwJointX0, fwx);
        float plates = max(BwBoxCover(p.z, zc - off, BwJointPlateW, fwz), BwBoxCover(p.z, zc + off, BwJointPlateW, fwz));
        seal = BwBoxCover(p.z, zc, BwJointSealW, fwz);
        float platePx = BwJointPlateW / max(fwz, 1e-5);
        float fade = saturate((platePx - 1.5) / 1.5);                      // 3 px and up: full; 1.5 px: gone
        seal *= across * fade;
        return plates * across * fade;
    }

    float BwRoadMarkings(float3 p, out float digitsOut)
    {
        float fwx = max(abs(ddx(p.x)) + abs(ddy(p.x)), 1e-5);
        float fwz = max(abs(ddx(p.z)) + abs(ddy(p.z)), 1e-5);
        float cov = 0.0;
        // edge lines and the dashed centre line
        cov = max(cov, BwBoxCover(p.x, -_EdgeLineX, _LineWidth, fwx));
        cov = max(cov, BwBoxCover(p.x, _EdgeLineX, _LineWidth, fwx));
        float per = max(_CenterDash + _CenterGap, 0.1);
        float dash = BwPulseTri(p.z / per, saturate(_CenterDash / per), fwz / per);
        cov = max(cov, BwBoxCover(p.x, 0.0, _LineWidth, fwx) * dash);
        // chainage numerals
        float digits = 0.0;
        [branch] if (_DigitsOn > 0.5)
        {
            float kl = clamp(round(p.z / TC_LaneDigitPitch), 1.0, 9.0);
            float zl = kl * TC_LaneDigitPitch;
            digits = BwChainage(p.xz, TC_LaneDigitLeeX, zl, (uint)kl, fwx, fwz);
            float ks = clamp(round((p.z - TC_LaneDigitSeaOffset) / TC_LaneDigitPitch), 1.0, 8.0);
            float zs = ks * TC_LaneDigitPitch + TC_LaneDigitSeaOffset;
            digits = max(digits, BwChainage(p.xz, TC_LaneDigitSeaX, zs, (uint)ks, fwx, fwz));
            float strokePx = TC_LaneDigitStroke / max(fwx, fwz);
            digits *= saturate((strokePx - 1.5) / 1.5);                  // 3 px and up: full; 1.5 px: gone
        }
        digitsOut = digits;
        return max(cov, digits);
    }

    void BwSurfaceHook(inout BwFragData f, inout SurfaceData s)
    {
        float3 p = f.positionWS;
        float3 ng = f.normalGeoWS;
        float fp = f.footprint;

        BwTri t0 = BwTriSetup(p, ng, _TexScale, 8.0);
        BwTri t1 = BwTriDetune(t0, 0.2941, float2(0.43, 0.19));
        float mixK = _DetailMix * lerp(0.55, 1.0, smoothstep(0.25, 0.75, BwValueNoise(p.xz * 0.13 + 1.9)));   // never one pure tile
        float3 texAlb = lerp(BwTriSample(TEXTURE2D_ARGS(_AlbedoMap, sampler_AlbedoMap), t0).rgb,
                             BwTriSample(TEXTURE2D_ARGS(_AlbedoMap, sampler_AlbedoMap), t1).rgb, mixK);
        float4 mask = lerp(BwTriSample(TEXTURE2D_ARGS(_MaskMap, sampler_AlbedoMap), t0),
                           BwTriSample(TEXTURE2D_ARGS(_MaskMap, sampler_AlbedoMap), t1), mixK);
        float3 n = normalize(lerp(BwTriNormal(TEXTURE2D_ARGS(_NormalMap, sampler_AlbedoMap), t0, ng, _NormalStrength),
                                  BwTriNormal(TEXTURE2D_ARGS(_NormalMap, sampler_AlbedoMap), t1, ng, _NormalStrength * 0.8), mixK));

        float macro = BwFbmFiltered(p.xz / 40.0, fp / 40.0, 3);
        float tone = 1.0 + _MacroTone * (2.0 * macro - 1.0);
        float macroS = BwMacroSigned(BwFbmFiltered(p.xz / 23.0 + 7.7, fp / 23.0, 3));   // weathering smoothness (8.11.4)
        float kerb = smoothstep(4.4, 5.6, abs(p.x)) * _KerbGrime;          // grime gathers at the parapet and the lee kerb

        float top = smoothstep(0.6, 0.85, ng.y);
        float wetK = saturate(max(_Wet, _WetFollow * saturate(_BwSurface.x) * top));
        float wetA = wetK * _WetAlbedo;                                    // broomed deck drains: darkens less than a soaked wall
        float ao = lerp(1.0, f.color.r, _AO);
        float wear = f.color.g * _EdgeWear;

        float digits;
        float paint = BwRoadMarkings(p, digits) * top;
        paint *= 0.9 + 0.1 * mask.b;                                       // paint thinner in the broom lows
        float jfx = max(abs(ddx(p.x)) + abs(ddy(p.x)), 1e-5), jfz = max(abs(ddx(p.z)) + abs(ddy(p.z)), 1e-5);
        float jSeal;
        float jPlate = BwRoadJoint(p, jfx, jfz, jSeal) * top;
        jSeal *= top;

        float3 dry = BwRecolour(texAlb, _BaseColor.rgb, PAL_ConcreteMid);
        float3 wet = BwRecolour(texAlb, _WetColor.rgb, PAL_ConcreteMid);
        float3 pav = lerp(dry, wet, wetA) * tone * (1.0 - kerb);
        float3 paintAlb = BwRecolour(texAlb, _LineColor.rgb, PAL_ConcreteMid) * lerp(1.0, 0.75, wetK);
        float3 alb = lerp(pav, paintAlb, paint);
        alb = lerp(alb, BwRecolour(texAlb, PAL_SteelPaint, PAL_ConcreteMid), jPlate);   // joint cover plates (painted steel)
        alb = lerp(alb, PAL_ConcreteDark, jSeal);                                        // neoprene seal
        alb = lerp(alb, alb * 1.1, wear);
        s.albedo = alb * lerp(0.82, 1.0, ao);

        float dryS = saturate(mask.a + (_Smoothness - 0.30)) - 0.06 * wear + 0.04 * kerb + 0.08 * macroS;
        float wetS = _WetSmoothness * (0.8 + 0.2 * (1.0 - mask.b));
        float pavS = lerp(dryS, wetS, wetK);
        s.smoothness = saturate(lerp(pavS, max(_PaintSmoothness, pavS), paint));
        s.smoothness = lerp(s.smoothness, min(s.smoothness, 0.35), jPlate);             // no mirror plates (capped sky reflection)
        s.smoothness = lerp(s.smoothness, 0.15, jSeal);
        s.metallic = 0.0;
        s.occlusion = ao * mask.g * (1.0 - 0.5 * saturate(jPlate + jSeal));             // half the sky reflection on the joint
        s.emission = 0;

        f.normalWS = normalize(lerp(n, ng, saturate(0.6 * wetK + 0.5 * paint)));
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
