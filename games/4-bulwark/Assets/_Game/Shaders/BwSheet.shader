// BwSheet - the moving water of beat 3 (MASTER_PROMPT 14.2.6 "BwSheet", 14.5.3 Flow / Sheets, 14.2.5 photosensitivity,
// Part 10 #3, #19). One lit shader (BwLitCore / BwLitPass: shared lighting, analytic sky reflection, BwFogApply), two modes:
//   _Mode 0  the DRAW-DOWN WATER of the slots (opaque): dark green-teal water, streaks along the flow lines advected in the
//            Lagrangian coordinate (UV0.x = travel time from the mouth; lambda = ambient time x 8.3 - travel x 8.3, so a
//            streak passes a fixed point at the same rate everywhere and stretches as the flow accelerates), foam at the
//            pier noses and their wakes, a turbulent mouth (the funnel), drip foam under the raised gate lips (x 10) and
//            aeration toward the lip; ripple normals by the surface gradient of the streak height (Mikkelsen 2010).
//   _Mode 1  the NEAR FALLING SHEET (first ~100 m, alpha-to-coverage under MSAA 4x): opaque glassy white-green at the lip,
//            white and aerated by ~20 m (Horeni breakup 16-20 m, docs/RESEARCH.md B 3.1), flow streaks in the same
//            Lagrangian coordinate (UV0.y = flight time, continuous with the channel across the lip), translucent glow of
//            the sunlit aerated water, and ligament breakup: elongated holes open over t 2.5..4.55 s and the slot edges
//            fray first; coverage edges are fwidth-wide so the alpha-to-coverage samples never shimmer.
// Photosensitivity: every pattern advects at the flow (the lambda cells are 6-23 m long), so no point flashes: the fastest
// luminance modulation at a fixed point is 8.3 / 6 = 1.4 cells per second on a 3-octave noise whose first octave carries
// the contrast (0.7 Hz), the breakup mask 8.3 / 9 = 0.9 Hz at its finest octave.
// Every procedural octave fades to its mean once its cell is under 2-4 pixel footprints (no moire, no sparkle).
// Colours: palette macros only. No shadow casting (the renderers switch it off; the pass exists for the SRP contract).
Shader "Megastructure/BwSheet"
{
    Properties
    {
        _Mode ("0 channel water, 1 falling sheet", Float) = 0
        _LipTau ("Travel time mouth -> lip (s)", Float) = 9.3
        _Foam ("Foam gain", Range(0, 2)) = 1
        _StreakGain ("Streak contrast", Range(0, 2)) = 1
        _Glow ("Sheet translucency", Range(0, 1)) = 0.12
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        _AlphaToMask ("Alpha to coverage", Float) = 0
    }

    HLSLINCLUDE
    #include "Include/BwLitCore.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _Mode;
        float _LipTau;
        float _Foam;
        float _StreakGain;
        float _Glow;
        float _AlphaToMask;
    CBUFFER_END

    float4 _BwSheetFlow;   // x = ambient time x 8.3 m/s (lip-equivalent metres), written by Beat03SpillwayBridge

    #define SHEET_SPEED 8.3                  // BulwarkConst.LipSpeed
    #define SHEET_BREAK_T0 2.5               // SpillwayGeometry.BreakupT0
    #define SHEET_BREAK_T1 4.55              // SpillwayGeometry.BreakupT1
    #define SHEET_SLOT_Z0 424.0
    #define SHEET_PITCH 20.0
    #define SHEET_SLOT_LEN 16.0
    #define SHEET_MOUTH_X 46.0
    #define SHEET_GATE_LIP_X 10.0            // SpillwayGeometry: the raised gate lip (y -9) at x 10.0

    // Anisotropic streak noise: 2D gradient noise on (across, along) cells, octaves x2 without rotation (the streaks keep
    // their direction), each octave weighted by its footprint in cells. About [-1, 1], mean 0.
    float SheetNoiseW(float2 x, float2 fpCells, int octaves, out float kept)
    {
        float sum = 0.0, amp = 1.0, norm = 0.0, wsum = 0.0;
        float2 p = x, fp = fpCells;
        [unroll(3)] for (int i = 0; i < octaves; i++)
        {
            float w = 1.0 - smoothstep(0.25, 0.5, max(fp.x, fp.y));
            sum += amp * w * BwGradNoise(p) * 1.4;
            norm += amp;
            wsum += amp * w;
            p = p * 2.0 + float2(7.31, 3.17);   // pal:ok (not a colour)
            fp *= 2.0;
            amp *= 0.5;
        }
        kept = wsum / max(norm, 1e-4);          // share of the noise energy still resolved (1 near, 0 far)
        return sum / max(norm, 1e-4);
    }

    float SheetNoise(float2 x, float2 fpCells, int octaves)
    {
        float kept;
        return SheetNoiseW(x, fpCells, octaves, kept);
    }

    float2 SheetFp(float2 q) { return float2(fwidth(q.x), fwidth(q.y)); }   // pal:ok (not a colour)

    // Surface-gradient bump (Mikkelsen 2010, "Bump Mapping Unparametrized Surfaces on the GPU"): perturb n by the screen
    // derivatives of a height field h (metres).
    float3 SheetBump(float3 n, float3 p, float h)
    {
        float3 dpx = ddx(p), dpy = ddy(p);
        float3 sx = dpx - dot(dpx, n) * n, sy = dpy - dot(dpy, n) * n;
        float3 r1 = cross(sy, n), r2 = cross(n, sx);
        float det = dot(sx, r1);
        if (abs(det) < 1e-12) return n;
        float3 g = (ddx(h) * r1 + ddy(h) * r2) / det;
        float3 m = n - g;
        float l2 = dot(m, m);
        return l2 > 1e-8 ? m * rsqrt(l2) : n;
    }

    // ------------------------------------------------------------------------------------------------ channel water
    void ChannelSurface(inout BwFragData f, inout SurfaceData s)
    {
        float tau = f.uv.x;
        float3 p = f.positionWS;
        float lam = _BwSheetFlow.x - tau * SHEET_SPEED;
        float2 q = float2(p.z * (1.0 / 0.55), lam * (1.0 / 7.0));       // pal:ok (not a colour)
        float st = SheetNoise(q, SheetFp(q), 3);
        float2 q2 = float2(p.z * (1.0 / 2.3), lam * (1.0 / 23.0)) + 11.0;   // pal:ok (not a colour)
        float st2 = SheetNoise(q2, SheetFp(q2), 2);
        // distance to the nearest pier face inside the slot (0 over a pier: hidden there anyway)
        float zl = p.z - SHEET_SLOT_Z0;
        float zs = zl - SHEET_PITCH * floor(zl / SHEET_PITCH);
        float dzw = zs < SHEET_SLOT_LEN ? min(zs, SHEET_SLOT_LEN - zs) : 0.0;
        float xd = max(SHEET_MOUTH_X - p.x, 0.0);
        float wake = exp(-dzw / (0.35 + 0.06 * xd)) * exp(-xd / 30.0);
        float mouth = smoothstep(38.0, SHEET_MOUTH_X, p.x) * saturate(0.5 + 0.45 * st2);
        float dx = p.x - (SHEET_GATE_LIP_X - 0.3);
        float drip = exp(-dx * dx * 4.0) * saturate(0.45 + 0.6 * st2) + 0.35 * exp(-max(-dx, 0.0) * 0.25) * step(dx, 0.0) * saturate(st);
        // Wave X: the last 12 m before the lip carry more air (the jets from under the raised gates arrive there broken:
        // 0.42 -> 0.7) and the fast streaks are whiter (0.55 -> 0.75): KS09 looks straight down on this water through the
        // grating (mean luma 94.3 against the beat-3 band 95-150 in mistx-01, the water itself 55-80; 93.4 at 21:9 in mistx-04)
        float lip = (1.0 - smoothstep(-6.0, 6.0, p.x)) * 0.7;
        // aerated streaks along the flow lines where the flow passes 4 m/s (x below ~25): white lines on dark water
        float fast = 1.0 - smoothstep(4.0, 28.0, p.x);
        float streakFoam = fast * smoothstep(0.25, 0.85, st) * (0.55 + 0.45 * saturate(0.5 + st2));
        float foam = saturate((0.9 * wake + mouth + 0.6 * drip + lip + 0.75 * streakFoam) * (0.75 + 0.5 * st) * _Foam);
        float3 body = lerp(PAL_SeaStorm, PAL_SeaTeal, 0.35) * (0.42 + 0.07 * st * _StreakGain + 0.04 * st2);
        s.albedo = lerp(body, PAL_FoamWhite * 0.82, foam);
        s.smoothness = lerp(0.86, 0.35, foam);
        s.metallic = 0.0;
        s.occlusion = 1.0;
        s.emission = 0;
        s.alpha = 1.0;
        // ripples: 3 cm streak ridges and 6 cm long swells, faded with their noise octaves
        float h = 0.03 * st * _StreakGain + 0.06 * st2;
        f.normalWS = SheetBump(f.normalWS, p, h);
    }

    // ------------------------------------------------------------------------------------------------ falling sheet
    float SheetCoverage(BwFragData f, out float lam)
    {
        float u = f.uv.x, t = f.uv.y;
        float3 p = f.positionWS;
        lam = _BwSheetFlow.x - (_LipTau + t) * SHEET_SPEED;
        float2 qb = float2(p.z * (1.0 / 1.3), lam * (1.0 / 9.0)) + 2.1;   // pal:ok (not a colour)
        float kept;
        float br = 0.5 + 0.5 * SheetNoiseW(qb, SheetFp(qb), 3, kept);
        float hole = smoothstep(SHEET_BREAK_T0, SHEET_BREAK_T1, t);
        float edge = min(u, SHEET_SLOT_LEN - u);
        float fray = saturate(1.0 - edge / (0.3 + 1.6 * t)) * smoothstep(0.4, 1.5, t);
        float a = br - saturate(1.15 * hole + 0.8 * fray);
        a = t > SHEET_BREAK_T1 + 0.05 ? -1.0 : a;
        // the filtered-away noise becomes a statistical coverage: far away the sheet thins out smoothly instead of ending
        // on a hard line (the unresolved octaves had a spread of about 0.3 in br)
        return saturate(a / max(max(fwidth(a), 0.3 * (1.0 - kept)), 1e-3) + 0.5);
    }

    void SheetSurface(inout BwFragData f, inout SurfaceData s)
    {
        float t = f.uv.y;
        float3 p = f.positionWS;
        float lam;
        s.alpha = SheetCoverage(f, lam);
        float2 q = float2(p.z * (1.0 / 0.5), lam * (1.0 / 6.0));          // pal:ok (not a colour)
        float st = SheetNoise(q, SheetFp(q), 3);
        float2 q2 = float2(p.z * (1.0 / 1.6), lam * (1.0 / 11.0)) + 5.3;  // pal:ok (not a colour)
        float st2 = SheetNoise(q2, SheetFp(q2), 2);
        float aer = smoothstep(0.08, 1.3, t);                              // glassy over the brow, white by ~8 m of drop
        float white = saturate(aer * (0.85 + 0.5 * st) + 0.3 * aer * st2 + 0.25 * smoothstep(0.2, 0.9, st));
        float3 glass = lerp(PAL_SeaTeal, PAL_SeaStorm, 0.4) * 0.45 + PAL_FoamWhite * 0.07;
        s.albedo = lerp(glass, PAL_FoamWhite * 0.85, white);
        s.smoothness = lerp(0.9, 0.3, white);
        s.metallic = 0.0;
        s.occlusion = 1.0;
        // translucency: the aerated water glows with the sunlight scattered through it (the key, the sun-break beam)
        s.emission = PAL_FoamWhite * _BwSunColor.rgb * (BwSunBreak(p) * _Glow * white);
        f.normalWS = SheetBump(f.normalWS, p, 0.04 * st + 0.08 * st2 * aer);
    }

    void BwSurfaceHook(inout BwFragData f, inout SurfaceData s)
    {
        if (_Mode > 0.5) SheetSurface(f, s);
        else ChannelSurface(f, s);
    }

    // depth and shadow passes: the sheet's coverage at 0.5 (the forward pass resolves it by alpha-to-coverage)
    void BwClipHook(BwFragData f)
    {
        if (_Mode < 0.5) return;
        float lam;
        clip(SheetCoverage(f, lam) - 0.5);
    }

    #define BW_ALPHA_OUT
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
