// BwGrade.shader - the one post stack: bloom, grade and the one final dither (MASTER_PROMPT 3.1 rule 6, 8.2, 14.2.5,
// 14.7 H5). Imported from Thread's ThreadGrade.shader; extended with the D-10 colour matrix (white balance + saturation)
// and the 0.18-pivot luminance contrast before the curve. Drawn by Megastructure.Rendering.BwGradeFeature (Render Graph,
// AfterRenderingPostProcessing, URP post-processing OFF on the camera). Full-screen triangles via Core Blit.hlsl.
//
// Passes (the C# side finds them by name):
//   BwBloomPrefilter  camera colour (full res, RGBA16F, MSAA resolved) -> half res: 13-tap downsample with a partial
//                     Karis average (firefly-safe), exposure, colour matrix, HDR clamp (40), URP soft-knee threshold
//   BwBloomDown       13-tap downsample (Jimenez 2014)
//   BwBloomUp         3x3 tent of the coarser level lerped with this level by scatter (URP-style scatter)
//   BwGrade           exposure, colour matrix (WB + saturation), linear contrast (pivot 0.18, toe-protected), bloom,
//                     Neutral highlight curve, vignette x fade x pause dim, sRGB encode + static TPDF blue-noise dither,
//                     written to an R8G8B8A8_SRGB target
// Every intermediate stays in floating point; the only quantisation is the final 8-bit write, and the dither is the
// last operation before it (exactly one display-referred dither). CPU mirror: BwGradeMath.cs (tests: GradeTests.cs).
// Numeric safety (Part 8.6): NaN -> 0 and an HDR clamp on every scene fetch, guarded divisions, saturated sqrt/pow inputs.
Shader "Megastructure/BwGrade"
{
    HLSLINCLUDE
    // Include order as URP's own Bloom.shader.
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
    // Blit.hlsl provides: TEXTURE2D_X(_BlitTexture), float4 _BlitScaleBias, Attributes, Varyings
    // { positionCS : SV_POSITION, texcoord : TEXCOORD0 }, Varyings Vert(Attributes) (full-screen triangle,
    // texcoord = uv * _BlitScaleBias.xy + _BlitScaleBias.zw), sampler_LinearClamp / sampler_PointClamp.

    TEXTURE2D_X(_BwGradeLow);     // up: the coarser bloom level | grade: the bloom result
    TEXTURE2D(_BwGradeNoise);     // 64 x 64 blue-noise rank map, (rank + 0.5) / 4096, read with Load

    CBUFFER_START(UnityPerMaterial)
        float4 _BwGradeSrcSize;   // _BlitTexture: w, h, 1/w, 1/h
        float4 _BwGradeLowSize;   // _BwGradeLow:  w, h, 1/w, 1/h
        float4 _BwGradeP0;        // prefilter: threshold, knee, clamp, exposure gain | up: scatter | grade: exposure gain, bloom intensity, contrast pivot, contrast k
        float4 _BwGradeP1;        // grade: vignette intensity, aspect (w / h), fade multiplier (1 - FadeBlack)(1 - PauseDim/2), dither peak (8-bit codes)
        float4 _BwGradeP2;        // grade: dither tile offset x, y (integer texels; 0 when static), Neutral start, Neutral desaturation
        float4 _BwGradeM0;        // colour matrix M = S * W, row 0 (xyz)  (BwGradeMath.GradeMatrix)
        float4 _BwGradeM1;        // row 1
        float4 _BwGradeM2;        // row 2
    CBUFFER_END

    static const float3 kBwGradeLuma = float3(0.2126729, 0.7151522, 0.0721749); // pal:ok (Rec.709 luminance weights, linear, sum 1; = BwGradeMath.Lum*)
    static const float kBwGradeHdrMax = 65000.0;          // below the half-float maximum 65504
    static const float kBwGradeVigRef = 1.0 + (16.0 / 9.0) * (16.0 / 9.0);   // squared half-diagonal of a 16:9 frame (half-height units)
    static const float kBwGradeVigShape = 1.5;            // BwGradeMath.VignetteShape
    #define BW_DITHER_TILE 64                             // FrameworkConst.DitherTile (14.12), asserted by GradeTests
    static const uint kBwGradeTileMask = (uint)BW_DITHER_TILE - 1u;
    static const float kBwGradeToe0 = 0.0025;              // BwGradeMath.ContrastToe0
    static const float kBwGradeToe1 = 0.0125;              // BwGradeMath.ContrastToe1

    // ------------------------------------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------------------------------------
    float3 BwGradeSanitize(float3 c)
    {
        c = AnyIsNaN(c) ? (0.0).xxx : c;
        return clamp(c, 0.0, kBwGradeHdrMax);   // +Inf -> max, -Inf and negatives -> 0
    }

    float3 BwGradeFetch(float2 uv)
    {
        return BwGradeSanitize(SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb);
    }

    float BwGradeLuma(float3 c) { return dot(c, kBwGradeLuma); }
    float BwGradeKarisW(float3 c) { return 1.0 / (1.0 + BwGradeLuma(c)); }

    // Exact piecewise sRGB (IEC 61966-2-1), as the GPU's sRGB encode; BwGradeMath mirrors it.
    float3 BwGradeLinearToSrgb(float3 c)
    {
        c = saturate(c);
        float3 lo = c * 12.92;
        float3 hi = 1.055 * pow(max(c, 1e-10), 1.0 / 2.4) - 0.055;
        return (c <= 0.0031308) ? lo : hi;
    }
    float3 BwGradeSrgbToLinear(float3 s)
    {
        s = saturate(s);
        float3 lo = s / 12.92;
        float3 hi = pow((s + 0.055) / 1.055, 2.4);
        return (s <= 0.04045) ? lo : hi;
    }

    // 13-tap 2x downsample (Jimenez 2014). t = SOURCE texel size; the taps cover 6 x 6 source texels.
    // karis: partial Karis average of the five 2x2 groups (first downsample only; suppresses fireflies from
    // the sun disc (radiance 60), clamped sea glints, lamp and lantern cores and city-light blobs, so the bloom
    // never flickers).
    float3 BwGradeDown13(float2 uv, float2 t, bool karis)
    {
        float3 a = BwGradeFetch(uv + t * float2(-2.0,  2.0));
        float3 b = BwGradeFetch(uv + t * float2( 0.0,  2.0));
        float3 c = BwGradeFetch(uv + t * float2( 2.0,  2.0));
        float3 d = BwGradeFetch(uv + t * float2(-2.0,  0.0));
        float3 e = BwGradeFetch(uv);
        float3 f = BwGradeFetch(uv + t * float2( 2.0,  0.0));
        float3 g = BwGradeFetch(uv + t * float2(-2.0, -2.0));
        float3 h = BwGradeFetch(uv + t * float2( 0.0, -2.0));
        float3 i = BwGradeFetch(uv + t * float2( 2.0, -2.0));
        float3 j = BwGradeFetch(uv + t * float2(-1.0,  1.0));
        float3 k = BwGradeFetch(uv + t * float2( 1.0,  1.0));
        float3 l = BwGradeFetch(uv + t * float2(-1.0, -1.0));
        float3 m = BwGradeFetch(uv + t * float2( 1.0, -1.0));
        if (!karis)
            return e * 0.125 + (a + c + g + i) * 0.03125 + (b + d + f + h) * 0.0625 + (j + k + l + m) * 0.125;
        float3 b0 = (j + k + l + m) * 0.25;
        float3 b1 = (a + b + d + e) * 0.25;
        float3 b2 = (b + c + e + f) * 0.25;
        float3 b3 = (d + e + g + h) * 0.25;
        float3 b4 = (e + f + h + i) * 0.25;
        float w0 = 0.5 * BwGradeKarisW(b0);
        float w1 = 0.125 * BwGradeKarisW(b1);
        float w2 = 0.125 * BwGradeKarisW(b2);
        float w3 = 0.125 * BwGradeKarisW(b3);
        float w4 = 0.125 * BwGradeKarisW(b4);
        return (b0 * w0 + b1 * w1 + b2 * w2 + b3 * w3 + b4 * w4) / max(w0 + w1 + w2 + w3 + w4, 1e-6);
    }

    float3 BwGradeLowTap(float2 uv)
    {
        return SAMPLE_TEXTURE2D_X_LOD(_BwGradeLow, sampler_LinearClamp, uv, 0).rgb;
    }

    // Khronos PBR Neutral highlight compression WITHOUT the 0.04 toe offset (BwGradeMath decision B03-1):
    // identity below `start` (0.76, BwGradeMath.NeutralStart, passed in _BwGradeP2.z so C# is the single
    // source), then one common scale (hue and saturation kept) and a mix toward white of speed `desat`.
    float3 BwGradeNeutral(float3 c, float start, float desat)
    {
        c = max(c, 0.0);
        start = clamp(start, 0.01, 0.99);
        float peak = max(c.r, max(c.g, c.b));
        if (peak < start) return c;
        float d = 1.0 - start;
        float newPeak = 1.0 - d * d / (peak + d - start);   // denominator >= d > 0
        c *= newPeak / peak;                                 // peak >= start > 0
        float g = 1.0 - 1.0 / (max(desat, 0.0) * (peak - newPeak) + 1.0);
        return lerp(c, newPeak.xxx, g);
    }

    // Colour matrix M = S * W (white balance as LMS von Kries with white luminance kept, then saturation around luminance).
    float3 BwGradeMatrix(float3 c)
    {
        return float3(dot(_BwGradeM0.xyz, c), dot(_BwGradeM1.xyz, c), dot(_BwGradeM2.xyz, c));   // pal:ok (not a colour)
    }

    // Linear contrast (D-10): Y' = pivot (Y / pivot)^(1 + k), hue-preserving scale, faded to the identity below
    // Y ~ 0.0125 with a C1 smoothstep so the sRGB-6 floor (linear 0.0018) is never crushed (lesson 3.2 #6).
    float3 BwGradeContrast(float3 c, float pivot, float k)
    {
        float y = BwGradeLuma(c);
        if (y <= 1e-8) return c;
        float p = pivot * pow(max(y / pivot, 1e-8), 1.0 + k);
        float w = smoothstep(kBwGradeToe0, kBwGradeToe1, y);
        return c * ((y + (p - y) * w) / y);
    }

    // Subtle radial vignette, circular in pixels, radius in half-height units (Hor+); 1 - intensity at a 16:9 corner.
    float BwGradeVignette(float2 uv, float aspect, float intensity)
    {
        float2 p = (uv - 0.5) * 2.0;
        p.x *= aspect;
        float r2 = dot(p, p) / kBwGradeVigRef;
        float shape = (1.0 - exp(-kBwGradeVigShape * r2)) / (1.0 - exp(-kBwGradeVigShape));
        return 1.0 - intensity * shape;
    }

    // Blue-noise TPDF on [-1, 1]: tile value (uniform) -> monotonic triangular remap (URP ApplyDithering form),
    // static by default (offset 0); with -dither temporal the tile moves by the integer R2 offset each frame. One value for R, G and B
    // (monochrome: luma is what survives 4:2:0 video, research R6 0.4).
    float BwGradeTpdf(float2 positionSS)
    {
        uint2 p = (uint2(positionSS) + uint2(_BwGradeP2.xy)) & kBwGradeTileMask;
        float u = LOAD_TEXTURE2D(_BwGradeNoise, int2(p)).r;
        float t = u * 2.0 - 1.0;
        return FastSign(t) * (1.0 - sqrt(saturate(1.0 - abs(t))));
    }

    // ------------------------------------------------------------------------------------------------
    // Fragment programs
    // ------------------------------------------------------------------------------------------------
    float4 FragPrefilter(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float3 c = BwGradeDown13(input.texcoord, _BwGradeSrcSize.zw, true);
        c = min(max(BwGradeMatrix(c * _BwGradeP0.w), 0.0), _BwGradeP0.z);   // exposure + matrix (threshold acts on graded values), clamp 40
        // URP soft-knee threshold (URP Bloom.shader FragPrefilter)
        float br = max(c.r, max(c.g, c.b));
        float knee = max(_BwGradeP0.y, 1e-4);
        float soft = clamp(br - _BwGradeP0.x + knee, 0.0, 2.0 * knee);
        soft = soft * soft / (4.0 * knee);
        c *= max(br - _BwGradeP0.x, soft) / max(br, 1e-4);
        return float4(max(c, 0.0), 1.0); // pal:ok
    }

    float4 FragDown(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        return float4(BwGradeDown13(input.texcoord, _BwGradeSrcSize.zw, false), 1.0); // pal:ok
    }

    float4 FragUp(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = input.texcoord;
        float2 t = _BwGradeLowSize.zw;
        float3 lo = BwGradeLowTap(uv) * 4.0
                  + (BwGradeLowTap(uv + float2(-t.x, 0.0)) + BwGradeLowTap(uv + float2(t.x, 0.0))
                   + BwGradeLowTap(uv + float2(0.0, -t.y)) + BwGradeLowTap(uv + float2(0.0, t.y))) * 2.0
                  + (BwGradeLowTap(uv + float2(-t.x, -t.y)) + BwGradeLowTap(uv + float2(t.x, -t.y))
                   + BwGradeLowTap(uv + float2(-t.x, t.y)) + BwGradeLowTap(uv + float2(t.x, t.y)));
        lo *= 1.0 / 16.0;   // 3x3 tent
        float3 hi = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb;
        return float4(lerp(hi, lo, _BwGradeP0.x), 1.0); // pal:ok
    }

    float4 FragGrade(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = input.texcoord;
        // unit screen uv (scale and any y-flip removed) for the bloom texture and the vignette
        float2 sbScale = float2(abs(_BlitScaleBias.x) > 1e-6 ? _BlitScaleBias.x : 1.0, abs(_BlitScaleBias.y) > 1e-6 ? _BlitScaleBias.y : 1.0);
        float2 uvUnit = (uv - _BlitScaleBias.zw) / sbScale;

        // 1. exposure and the colour matrix (white balance + saturation), negatives clamped
        float3 c = BwGradeSanitize(SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv, 0).rgb);
        c = max(BwGradeMatrix(c * _BwGradeP0.x), 0.0);

        // 2. contrast in linear HDR around the 0.18 pivot, before the curve (toe-protected)
        c = BwGradeContrast(c, _BwGradeP0.z, _BwGradeP0.w);

        // 3. bloom (prefiltered from the exposed, graded scene, so it is added here)
        float3 bloom = SampleTexture2DBicubic(TEXTURE2D_X_ARGS(_BwGradeLow, sampler_LinearClamp), uvUnit,
                                              _BwGradeLowSize, (1.0).xx, 0u).rgb;   // bicubic: no blocky half-res diamonds
        c += max(bloom, 0.0) * _BwGradeP0.y;

        // 4. tone curve (identity below 0.76: palette colours display as authored)
        c = BwGradeNeutral(c, _BwGradeP2.z, _BwGradeP2.w);

        // 5. vignette, the title / ending fade and the pause dim, in float, before the dither
        c *= BwGradeVignette(uvUnit, _BwGradeP1.y, _BwGradeP1.x) * _BwGradeP1.z;

        // 6. sRGB code space + TPDF blue-noise dither of peak A codes (static by default), faded inside the last half LSB at both
        //    ends so pure black (fades) and pure white stay exact (research R6 2.2). The target is R8G8B8A8_SRGB:
        //    we return the LINEAR value of the dithered code, the GPU re-encodes it and rounds (one quantiser).
        float3 s = BwGradeLinearToSrgb(c);
        float n = BwGradeTpdf(input.positionCS.xy) * (_BwGradeP1.w / 255.0);
        float3 edge = saturate(min(s, 1.0 - s) * 510.0);
        s = saturate(s + n * edge);
        return float4(BwGradeSrgbToLinear(s), 1.0); // pal:ok
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off Blend Off

        Pass
        {
            Name "BwBloomPrefilter"
            HLSLPROGRAM
            #pragma target 4.5
            #pragma editor_sync_compilation
            #pragma vertex Vert
            #pragma fragment FragPrefilter
            ENDHLSL
        }

        Pass
        {
            Name "BwBloomDown"
            HLSLPROGRAM
            #pragma target 4.5
            #pragma editor_sync_compilation
            #pragma vertex Vert
            #pragma fragment FragDown
            ENDHLSL
        }

        Pass
        {
            Name "BwBloomUp"
            HLSLPROGRAM
            #pragma target 4.5
            #pragma editor_sync_compilation
            #pragma vertex Vert
            #pragma fragment FragUp
            ENDHLSL
        }

        Pass
        {
            Name "BwGrade"
            HLSLPROGRAM
            #pragma target 4.5
            #pragma editor_sync_compilation
            #pragma vertex Vert
            #pragma fragment FragGrade
            ENDHLSL
        }
    }
    Fallback Off
}
