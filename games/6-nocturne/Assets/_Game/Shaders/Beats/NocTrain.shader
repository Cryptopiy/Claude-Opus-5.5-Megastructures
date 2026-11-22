// NocTrain - the trains T1-T7 of beat 4 and everything drawn with them in their tubes (MASTER_PROMPT 14.5.4, 14.7 H7; owner TR,
// Phase 4 wave B; derived from BV's NocTram). One material per tube (TrainBuilder): the moving trains, the guideway beam and the
// tunnel linings behind the mouths. The zone of a face comes from its vertex colour R (zone x 25): 0 paint, 1 window strip,
// 2 headlight lens, 3 tail-light lens, 4 dark running gear / bellows / skirt, 5 windscreen, 6 bare steel (roof gear, magnet
// frames), 7 tunnel lining (dark concrete), 8 guideway (concrete beam, steel running faces), 9 roof glazing; G = edge wear (ChamferBox),
// B = a per-car seed. UV0 = metres on the car (u along it from its rear end, v up from the body bottom) on the side faces, planar
// metres elsewhere.
//   * paint: MetalPaint (TP's processed CC0 set, the Tram row's textures) as an albedo variation around the graphite token, its
//     normal and smoothness, faded to the mean below 2-4 px of the 2 m tile (NocTexturing); a darker skirt, a lighter roof
//     shoulder, one cool pinstripe under the windows, panel seams every 2.4 m and door outlines as filtered grooves
//     (NocFilteredGrid1), grime toward the sill, rain streaks, wet gloss;
//   * window strip (TR fix wave, C40): the saloon behind the side glass by interior mapping, a lit box 3.2 m deep (floor v 0.85,
//     ceiling v 3.0 above the body bottom) with a ceiling luminaire, luggage racks on both walls, the far wall with its own
//     windows (night, a quarter with lit blinds) between lit pillars, transverse seat rows (0.95 m pitch, 2 + 2 with an aisle,
//     white headrest covers on the fronts, none at the doors) and the light falloff of 2.4 m ceiling panels; four rotated-grid
//     taps on the pixel's glass footprint (the uv derivatives, taken before every branch) and a fade to a far profile below 10 cm of
//     glass per pixel; transmitted x (1 - Fresnel), the glass reflection is the lit model's (sky x Fresnel, the GGX of the lamps);
//   * headlights SignWhite x _HeadGain (bloom core), tail lights AviationRed x _TailGain;
//   * the tunnel darkness (NOC_HAS_FORWARD_HOOK): the shaded colour x (1 - smoothstep(0, _Fade.x, d)), d = the depth behind the
//     nearer mouth plane x = _Section.y / _Section.z, so a mouth reads as a black hole and a train emerges from it; the clip hook
//     discards x < _Section.x and x > _Section.w (3 cm behind the tunnel end discs, inside the sealed masses), every pass.
// No keyword, three samplers, derivatives before every branch, NocFog and NocFloor last (the template).
Shader "Megastructure/NocTrain"
{
    Properties
    {
        _BaseColor ("Paint albedo token (linear)", Color) = (0.0497, 0.0513, 0.0545, 1)
        [NoScaleOffset] _AlbedoTex ("Paint albedo variation (sRGB, mean 0.5)", 2D) = "grey" {}
        [NoScaleOffset][Normal] _NormalTex ("Paint normal", 2D) = "bump" {}
        [NoScaleOffset] _MaskTex ("Paint mask (R metal, G AO, B height, A smoothness)", 2D) = "white" {}
        _Tile ("Paint tile (m)", Float) = 2
        _StripeColor ("Pinstripe albedo (linear)", Color) = (0.36, 0.40, 0.44, 1)
        [HDR] _WinLight ("Saloon light (linear)", Color) = (1, 0.9216, 0.8308, 1)
        _WinGain ("Saloon light mean", Float) = 0.30
        [HDR] _HeadLight ("Headlight (linear)", Color) = (0.7084, 0.8228, 1, 1)
        _HeadGain ("Headlight gain", Float) = 10.0
        [HDR] _TailLight ("Tail light (linear)", Color) = (1, 0.0232, 0.016, 1)
        _TailGain ("Tail light gain", Float) = 1.2
        _Section ("Tube section (clip lo, mouth lo, mouth hi, clip hi) in world x", Vector) = (372.07, 384, 683, 694.93)
        _Fade ("Mouth darkness (fade length m, -, -, -)", Vector) = (2.5, 0, 0, 0)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "../Include/NocLitCore.hlsl"
    #include "../Include/NocTexturing.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float _Tile;
        float4 _StripeColor;
        float4 _WinLight;
        float _WinGain;
        float4 _HeadLight;
        float _HeadGain;
        float4 _TailLight;
        float _TailGain;
        float4 _Section;
        float4 _Fade;
        float _Cull;
    CBUFFER_END

    TEXTURE2D(_AlbedoTex); SAMPLER(sampler_AlbedoTex);
    TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
    TEXTURE2D(_MaskTex);   SAMPLER(sampler_MaskTex);

    #define NOC_HAS_CLIP_HOOK
    void NocClipHook(NocFragData f)
    {
        float x = f.positionWS.x;
        clip(min(x - _Section.x, _Section.w - x));
    }

    // Car-local layout (TrainBuilder, metres from the body bottom / the car's rear end): window strip v 1.65..2.65; doors (two per
    // side and car) at u 3.6..5.0 and 18.4..19.8.
    #define TRAIN_WIN_V0 1.65
    #define TRAIN_WIN_V1 2.65
    #define TRAIN_DOOR_A0 3.6
    #define TRAIN_DOOR_A1 5.0
    #define TRAIN_DOOR_B0 18.4
    #define TRAIN_DOOR_B1 19.8

    float TrainInDoor(float u)
    {
        return saturate(step(TRAIN_DOOR_A0, u) * step(u, TRAIN_DOOR_A1) + step(TRAIN_DOOR_B0, u) * step(u, TRAIN_DOOR_B1));
    }

    float TrainBand(float t, float a, float b, float w) { return smoothstep(a - w, a + w, t) * (1.0 - smoothstep(b - w, b + w, t)); }

    // ---- saloon interior (TR fix wave, C40): the car seen through its side glass as a lit box, interior mapping. Coordinates in
    // metres: u along the car (from its rear end), v up from the body bottom, d the depth behind the glass. The view ray enters the
    // glass at (u0, v0) and moves (ku, kv) per metre of depth. Values are saloon-light multiples (the luminaire 2.6, lit walls
    // 0.2-0.9, the night through the far windows 0.08); the python twin (KS12 simulation) is logged in docs/logs/TR.md.
    #define SAL_D      3.20
    #define SAL_VF     0.85
    #define SAL_VC     3.00
    #define SAL_PITCH  0.95
    #define SAL_BACK   0.22
    #define SAL_TOP    2.10
    #define SAL_HEAD   0.24
    #define SAL_RACKV  2.78
    #define SAL_RACKD  0.40
    #define SAL_GAIN   1.30

    bool SalDoor(float u)
    {
        return (u > TRAIN_DOOR_A0 - 0.35 && u < TRAIN_DOOR_A1 + 0.35) || (u > TRAIN_DOOR_B0 - 0.35 && u < TRAIN_DOOR_B1 + 0.35);
    }

    // One seat zone (depth za..zb; the seats face +u): the window-facing side of a seat at the zone's entry, else the first two seat
    // backs the ray crosses inside the zone (backs going +u, fronts with the white headrest covers going -u).
    void SalSeats(float u0, float v0, float ku, float kv, float za, float zb, float phase, inout float bestD, inout float bestL)
    {
        float ua = u0 + ku * za, va = v0 + kv * za;
        float m = frac((ua - phase) / SAL_PITCH) * SAL_PITCH;
        if (za < bestD && m < SAL_BACK && va < SAL_TOP && va > SAL_VF && !SalDoor(ua))
        {
            bestD = za;
            bestL = va > SAL_TOP - SAL_HEAD ? 0.45 : 0.18;
            return;
        }
        float aku = abs(ku);
        if (aku < 1e-3 || m < SAL_BACK) return;
        float du = ku > 0.0 ? (SAL_PITCH - m) : (m - SAL_BACK);
        bool front = ku < 0.0;
        [unroll] for (int k = 0; k < 2; k++)
        {
            float dk = za + (du + k * SAL_PITCH) / aku;
            float vk = v0 + kv * dk, uk = u0 + ku * dk;
            if (dk < zb && dk < bestD && vk < SAL_TOP && vk > SAL_VF && !SalDoor(uk))
            {
                bestD = dk;
                bestL = vk > SAL_TOP - SAL_HEAD ? (front ? 0.70 : 0.30) : (front ? 0.22 : 0.16);
                return;
            }
        }
    }

    // The saloon value along one ray (no filtering).
    float SalRaw(float u0, float v0, float ku, float kv, float seed)
    {
        float dC = kv > 1e-4 ? (SAL_VC - v0) / kv : 1e4;
        float dF = kv < -1e-4 ? (SAL_VF - v0) / kv : 1e4;
        float bestD = min(SAL_D, min(dC, dF));
        float bestL = -1.0;
        float phase = seed * 0.41;
        SalSeats(u0, v0, ku, kv, 0.10, 1.18, phase, bestD, bestL);
        SalSeats(u0, v0, ku, kv, 2.02, 3.10, phase, bestD, bestL);
        if (kv > 1e-4)
        {
            float dR = (SAL_RACKV - v0) / kv;                                   // rack undersides (near wall, far wall)
            if (dR > 0.0 && dR < SAL_RACKD && dR < bestD) { bestD = dR; bestL = 0.16; }
            else if (dR > SAL_D - SAL_RACKD && dR < bestD) { bestD = dR; bestL = 0.20; }
        }
        float ue = u0 + ku * bestD, ve = v0 + kv * bestD;
        float rhythm = 0.85 + 0.15 * cos(6.2831853 * (ue - 1.2) / 2.4);       // the 2.4 m ceiling panels' light pools
        if (bestL >= 0.0) return bestL * rhythm;
        if (bestD >= SAL_D - 1e-4)
        {
            // the far wall: its window band (night, a quarter with lit blinds) between lit pillars, the lit upper wall, the
            // shadowed wall behind the seats
            if (ve > TRAIN_WIN_V0 && ve < TRAIN_WIN_V1)
            {
                float mp = abs(frac((ue - 0.55) / 1.6 + 0.5) - 0.5) * 1.6;
                float tb = (ve - TRAIN_WIN_V0) / (TRAIN_WIN_V1 - TRAIN_WIN_V0);
                float cell = floor((ue - 0.55) / 1.6);
                bool blind = NocU01(NocPcg((uint)(int)(cell + 4096.0) * 747796405u + (uint)(seed * 255.0 + 0.5) + 0x2545F491u)) < 0.25;
                return (mp < 0.17 ? 0.55 * (0.85 + 0.3 * tb) : (blind ? 0.62 : 0.08)) * rhythm;
            }
            return (ve >= TRAIN_WIN_V1 ? 0.55 + 0.35 * saturate((ve - TRAIN_WIN_V1) / (SAL_VC - TRAIN_WIN_V1)) : 0.22) * rhythm;
        }
        if (bestD >= dC - 1e-4)
        {
            float c = abs(bestD - 0.5 * SAL_D);                                  // the ceiling: luminaire strip, falloff to the walls
            return c < 0.2 ? 2.6 : 0.85 - 0.4 * c / (0.5 * SAL_D);
        }
        return (bestD > 1.18 && bestD < 2.02 ? 0.26 : 0.18) * rhythm;          // the floor (aisle lighter)
    }

    // The window strip's emission factor at (u, v): the interior over the pixel's glass footprint (four rotated-grid taps along the
    // screen derivatives duvx / duvy of uv), faded below 10 cm of glass per pixel to the band's far profile (the saloon seen from
    // afar: a bright ceiling line in the top 19 %, the lit middle, the darker seat zone; mean 0.76, so a distant train keeps the
    // bright top line of the wave-B band: KS11 p99 >= 120 at 16:10 / 21:9, TR_r13), a per-window variation and the near window
    // frames every 1.6 m (filtered).
    float TrainSaloonView(float u, float v, float2 duvx, float2 duvy, float ku, float kv, float fp, float seed)
    {
        float fw = max(max(length(duvx), length(duvy)), fp);
        float keep = 1.0 - smoothstep(0.035, 0.10, fw);
        float t = saturate((v - TRAIN_WIN_V0) / (TRAIN_WIN_V1 - TRAIN_WIN_V0));
        float L = 0.33 + 0.29 * smoothstep(0.30, 0.45, t) + 1.30 * smoothstep(0.76, 0.86, t);
        [branch] if (keep > 0.0)
        {
            float a = SalRaw(u + 0.125 * duvx.x + 0.375 * duvy.x, v + 0.125 * duvx.y + 0.375 * duvy.y, ku, kv, seed);
            float b = SalRaw(u + 0.375 * duvx.x - 0.125 * duvy.x, v + 0.375 * duvx.y - 0.125 * duvy.y, ku, kv, seed);
            float c = SalRaw(u - 0.125 * duvx.x - 0.375 * duvy.x, v - 0.125 * duvx.y - 0.375 * duvy.y, ku, kv, seed);
            float d = SalRaw(u - 0.375 * duvx.x + 0.125 * duvy.x, v - 0.375 * duvx.y + 0.125 * duvy.y, ku, kv, seed);
            L = lerp(L, 0.25 * (a + b + c + d), keep);
        }
        float cell = floor((u - 0.55) / 1.6);
        // the seed byte rounded, never truncated: the interpolated b x 255 lands on either side of the integer per pixel, and the
        // truncation drew the 8 % row stripes of p4_01 KS12 (C40)
        float var = 0.85 + 0.3 * NocU01(NocPcg((uint)(int)(cell + 4096.0) * 0x9E3779B9u + (uint)(seed * 255.0 + 0.5)));
        L *= lerp(1.0, var, smoothstep(2.0, 3.0, 1.6 / max(fw, 1e-5)));
        float pill = NocFilteredGrid1(u - 0.55, 1.6, 0.09, fw, 2.5);
        return SAL_GAIN * L * (1.0 - 0.85 * pill);
    }

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        float3 P = f.positionWS;
        float3 n0 = f.normalWS;
        float2 uv = f.uv.xy;
        // ---- derivatives first (every branch below is divergent by zone)
        float3 T, B;
        NocUvFrame(uv, P, n0, T, B);
        float2 tuv = uv / max(_Tile, 0.05);
        float2 dx = ddx(tuv), dy = ddy(tuv);
        float2 duvx = dx * max(_Tile, 0.05), duvy = dy * max(_Tile, 0.05);     // metres of uv per pixel (the saloon taps)
        float fade = NocTexFade(1.0, NocFootprintBox(dx, dy));
        float fp = f.footprint;
        int zone = (int)(f.color.r * 255.0 / 25.0 + 0.5);
        float seed = f.color.b;
        float4 alb = NocSampleFade(TEXTURE2D_ARGS(_AlbedoTex, sampler_AlbedoTex), tuv, dx, dy, fade);
        float4 nrm = NocSampleFade(TEXTURE2D_ARGS(_NormalTex, sampler_NormalTex), tuv, dx, dy, fade);
        float4 msk = NocSampleFade(TEXTURE2D_ARGS(_MaskTex, sampler_MaskTex), tuv, dx, dy, fade);

        s.metallic = 0.0;
        s.occlusion = 1.0;
        s.emission = 0.0;
        float side = 1.0 - saturate(abs(n0.y) * 1.5);
        float wear = saturate(f.color.g * 0.35);
        [branch] if (zone == 0)
        {
            float2 slope = NocTangentToSlope(nrm, 0.6) * fade;
            float3 n1 = NocResolveNormal(n0, NocSurfGradTB(slope, T, B));
            float v = uv.y, u = uv.x;
            float3 a = _BaseColor.rgb * lerp(1.0, 2.0 * alb.rgb, 0.45);
            a *= lerp(1.0, 0.70, side * (1.0 - smoothstep(0.62, 0.70, v)));               // darker skirt below 0.7 m
            a *= lerp(1.0, 1.30, side * smoothstep(2.82, 2.90, v));                       // lighter roof shoulder
            float stripe = side * NocFilteredGrid1(v - 1.45, 100.0, 0.045, fp, 2.0);       // the pinstripe under the strip
            a = lerp(a, _StripeColor.rgb, stripe);
            float inDoor = TrainInDoor(u) * step(0.55, v) * step(v, 2.85);
            float seamH = max(NocFilteredGrid1(v - 0.70, 100.0, 0.012, fp, 2.0), NocFilteredGrid1(v - 2.86, 100.0, 0.012, fp, 2.0));
            float seamV = NocFilteredGrid1(u - 1.2, 2.4, 0.012, fp, 2.0) * (1.0 - inDoor);
            float doorEdge = max(max(NocFilteredGrid1(u - TRAIN_DOOR_A0, 100.0, 0.018, fp, 2.0), NocFilteredGrid1(u - TRAIN_DOOR_A1, 100.0, 0.018, fp, 2.0)),
                                 max(NocFilteredGrid1(u - TRAIN_DOOR_B0, 100.0, 0.018, fp, 2.0), NocFilteredGrid1(u - TRAIN_DOOR_B1, 100.0, 0.018, fp, 2.0)));
            float doorBand = step(0.55, v) * step(v, 2.85);
            float seam = side * saturate(max(seamH, max(seamV, doorBand * doorEdge)));
            a *= 1.0 - 0.6 * seam;
            float grime = side * (1.0 - smoothstep(0.2, 0.9, v)) * 0.35;
            float streakRaw = saturate((NocValueNoise(float2(u / 0.22 + seed * 17.0, v / 2.5)) - 0.55) * 2.5) * side * (1.0 - smoothstep(0.9, 1.6, v));
            float keep = smoothstep(2.0, 4.0, 0.22 / max(fp, 1e-5));
            a *= (1.0 - grime) * (1.0 - 0.25 * lerp(0.15, streakRaw, keep));
            float up = saturate(n0.y);
            s.albedo = lerp(a, PAL_SteelF0 * 0.55, wear);
            s.metallic = wear;
            s.smoothness = saturate(lerp(0.55, 0.78, msk.a) + 0.12 * up - 0.18 * grime);
            s.occlusion = lerp(1.0, msk.g, 0.6);
            f.normalWS = n1;
        }
        else [branch] if (zone == 1)
        {
            // the view ray in the car's frame: u along the car (the object's +x), v up, depth inward (-n0)
            float3 rd = -f.viewDirWS;
            float3 tw = normalize(mul((float3x3)GetObjectToWorldMatrix(), float3(1.0, 0.0, 0.0)));
            float dn = max(dot(rd, -n0), 0.03);
            float ku = dot(rd, tw) / dn, kv = rd.y / dn;
            float L = TrainSaloonView(uv.x, uv.y, duvx, duvy, ku, kv, fp, seed);
            float Fw = 0.04 + 0.96 * pow(1.0 - saturate(dot(n0, f.viewDirWS)), 5.0);
            s.albedo = PAL_GlassDark * 0.6;
            s.smoothness = 0.92;
            s.emission = _WinLight.rgb * (_WinGain * L * (1.0 - Fw));
        }
        else if (zone == 2)
        {
            s.albedo = 0.05;
            s.smoothness = 0.9;
            s.emission = _HeadLight.rgb * _HeadGain;
        }
        else if (zone == 3)
        {
            s.albedo = 0.03;
            s.smoothness = 0.9;
            s.emission = _TailLight.rgb * _TailGain;
        }
        else if (zone == 4)
        {
            s.albedo = 0.014 * lerp(1.0, 2.0 * alb.rgb, 0.3);
            s.smoothness = 0.35;
        }
        else if (zone == 5)
        {
            s.albedo = PAL_GlassDark * 0.5;
            s.smoothness = 0.95;
            s.emission = _WinLight.rgb * 0.015;                                    // the dim cab behind the windscreen
        }
        else if (zone == 6)
        {
            s.albedo = lerp(PAL_SteelF0 * 0.35, PAL_SteelF0 * 0.6, wear);
            s.metallic = 1.0;
            s.smoothness = 0.5 + 0.1 * msk.a;
        }
        else if (zone == 9)
        {
            // roof glazing (panorama strips between the roof gear): the saloon's ceiling seen from above, window frames every 1.6 m
            float pillR = NocFilteredGrid1(uv.x - 0.55, 1.6, 0.12, fp, 2.5);
            s.albedo = PAL_GlassDark * 0.6;
            s.smoothness = 0.92;
            s.emission = _WinLight.rgb * (_WinGain * 0.9 * (1.0 - 0.85 * pillR));
        }
        else if (zone == 7)
        {
            s.albedo = 0.022 * lerp(1.0, 2.0 * alb.rgb, 0.5);                       // the tunnel lining (dark concrete)
            s.smoothness = 0.3;
        }
        else if (zone == 8)
        {
            // guideway: a concrete beam (grey, wet sheen) with steel running faces on top
            float topF = smoothstep(0.7, 0.9, n0.y);
            float3 conc = float3(0.060, 0.059, 0.057) * lerp(1.0, 2.0 * alb.rgb, 0.6);
            s.albedo = lerp(conc, PAL_SteelF0 * 0.45, topF);
            s.metallic = topF;
            s.smoothness = lerp(0.45, 0.6, topF);
        }
        else
        {
            s.albedo = 0.02;
            s.smoothness = 0.3;
        }
        [branch] if (NocDebugView() == 8) f.debug = float4(frac(uv / 1.0), (float)zone / 8.0, 1.0);
    }

    // Darkness behind the mouth planes (1 in the open section).
    float TrainLit(float x)
    {
        float inside = max(_Section.y - x, x - _Section.z);
        return 1.0 - smoothstep(0.0, max(_Fade.x, 1e-3), inside);
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
            #define NOC_HAS_FORWARD_HOOK
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Include/NocLighting.hlsl"

            half3 NocForwardHook(NocFragData f, SurfaceData s, InputData inputData)
            {
                return NocShadeOcc(inputData, s, f.specOcc) * (half)TrainLit(f.positionWS.x);
            }

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
