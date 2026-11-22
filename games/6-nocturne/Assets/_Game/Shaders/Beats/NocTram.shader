// NocTram - the tram T_A of the boulevard (MASTER_PROMPT 14.5.2 "Moving", 14.7 H7; owner BV, Phase 4 wave A; LOOK_V2 6 "required
// subjects": a continuous WinNeutral window band at linear 0.25-0.32, SignWhite headlights with a core >= 2.0, MetalPaint body, wet).
// One material for the whole vehicle; the zone of a face comes from its vertex colour R (zone x 32: 0 paint, 1 window band,
// 2 headlight lens, 3 tail-light lens, 4 dark running gear and bellows, 5 windscreen, 6 bare steel (pantograph, roof gear),
// 7 destination band), G = edge wear (ChamferBox), B = a per-car seed. UV0 = metres on the car (u along it from its rear end, v up)
// on the side faces, planar metres elsewhere.
//   * paint: MetalPaint (TP's processed CC0 set) as an albedo variation around the token, its normal and smoothness, faded to the
//     mean below 2-4 px of the 2 m tile (NocTexturing); livery = a darker skirt, an 18 cm light stripe over it, a light
//     shoulder; panel seams and door outlines as filtered 12 mm grooves (NocFilteredGrid1); grime toward the sill, rain streaks
//     under the window band, wet gloss (rain everywhere: LOOK_V2 5.2);
//   * window band: dark glass (sky sheen through the lit template) over the lit saloon: a ceiling light strip, a handrail line,
//     seat backs in the lower third, slim pillars every 1.95 m (no grid, no people), every pattern faded to its mean below 2-3 px;
//     the mean emission is _WinLight x _WinGain (the band's mean radiance; 0.28 linear);
//   * headlights SignWhite x _HeadGain (core >= 2.0), tail lights AviationRed x _TailGain;
//   * clip planes (NOC_HAS_CLIP_HOOK, every pass): fragments with dot(P, _ClipA.xyz) + _ClipA.w > 0 or likewise _ClipB are
//     discarded: the 50 m that stick out behind the 10 m terminus hall at launch and the cars inside the depot tunnel are never
//     drawn (both planes lie inside sealed meshes: TramSchedule).
// No keyword, three samplers, derivatives before every branch, NocFog and NocFloor last (the template).
Shader "Megastructure/NocTram"
{
    Properties
    {
        _BaseColor ("Paint albedo token (linear)", Color) = (0.0497, 0.0513, 0.0545, 1)
        [NoScaleOffset] _AlbedoTex ("Paint albedo variation (sRGB, mean 0.5)", 2D) = "grey" {}
        [NoScaleOffset][Normal] _NormalTex ("Paint normal", 2D) = "bump" {}
        [NoScaleOffset] _MaskTex ("Paint mask (R metal, G AO, B height, A smoothness)", 2D) = "white" {}
        _Tile ("Paint tile (m)", Float) = 2
        _StripeColor ("Pinstripe albedo (linear)", Color) = (0.42, 0.44, 0.46, 1)
        [HDR] _WinLight ("Saloon light (linear)", Color) = (1, 0.9216, 0.8308, 1)
        _WinGain ("Saloon light mean", Float) = 0.30
        [HDR] _HeadLight ("Headlight (linear)", Color) = (0.7157, 0.8228, 1, 1)
        _HeadGain ("Headlight gain", Float) = 3.0
        [HDR] _TailLight ("Tail light (linear)", Color) = (1, 0.0232, 0.016, 1)
        _TailGain ("Tail light gain", Float) = 1.2
        [HDR] _Display ("Destination band (linear)", Color) = (1, 0.352, 0.0262, 1)
        _DisplayGain ("Destination band gain", Float) = 0.35
        _ClipA ("Clip plane A (n.xyz, d): discard where n.P + d > 0", Vector) = (1, 0, 0, -214.47)
        _ClipB ("Clip plane B", Vector) = (0, 0, 1, -278)
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
        float4 _Display;
        float _DisplayGain;
        float4 _ClipA;
        float4 _ClipB;
        float _Cull;
    CBUFFER_END

    TEXTURE2D(_AlbedoTex); SAMPLER(sampler_AlbedoTex);
    TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
    TEXTURE2D(_MaskTex);   SAMPLER(sampler_MaskTex);

    #define NOC_HAS_CLIP_HOOK
    void NocClipHook(NocFragData f)
    {
        float4 P = float4(f.positionWS, 1.0);
        clip(min(-dot(P, _ClipA), -dot(P, _ClipB)));
    }

    // Window band geometry (car-local metres, TramBuilder): sill 1.05, head 2.75; door leaves at u 2.35..3.75 and 7.95..9.35.
    #define TRAM_WIN_Y0 1.05
    #define TRAM_WIN_Y1 2.75
    #define TRAM_DOOR_A0 2.35
    #define TRAM_DOOR_A1 3.75
    #define TRAM_DOOR_B0 7.95
    #define TRAM_DOOR_B1 9.35

    // Saloon light seen through the glass at (u, v), mean ~1: ceiling strip, handrail, seat backs, pillars (filtered by fp, m / px).
    float TramSaloon(float u, float v, float fp, float seed)
    {
        float t = saturate((v - TRAM_WIN_Y0) / (TRAM_WIN_Y1 - TRAM_WIN_Y0));
        float L = 0.78 + 1.35 * smoothstep(0.78, 0.97, t);                               // the ceiling light strip (bright top)
        float rail = NocFilteredGrid1(v - (TRAM_WIN_Y0 + 1.22), 100.0, 0.035, fp, 2.0);   // handrail, one dark line
        L *= 1.0 - 0.55 * rail;
        // seat backs: dark rounded blocks below t 0.34, every 0.86 m (gaps over the doors), faded to their mean below 3 px
        float seatW = 0.86;
        float su = frac((u + seed * 0.37) / seatW);
        float back = smoothstep(0.08, 0.16, su) * (1.0 - smoothstep(0.78, 0.86, su)) * (1.0 - smoothstep(0.26, 0.34, t));
        float door = step(TRAM_DOOR_A0, u) * step(u, TRAM_DOOR_A1) + step(TRAM_DOOR_B0, u) * step(u, TRAM_DOOR_B1);
        back *= 1.0 - saturate(door);
        float keepSeat = smoothstep(2.0, 3.0, seatW / max(fp, 1e-5));
        float seatMean = 0.6 * 0.30;                                                       // coverage x height share
        L *= 1.0 - 0.62 * lerp(seatMean, back, keepSeat);
        // pillars every 1.95 m (glass frames), plus the door edges
        float pill = NocFilteredGrid1(u - 0.4, 1.95, 0.10, fp, 2.5);
        L *= 1.0 - 0.85 * pill;
        return L / 0.97;
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
        float fade = NocTexFade(1.0, NocFootprintBox(dx, dy));
        float fp = f.footprint;
        int zone = (int)(f.color.r * 255.0 / 32.0 + 0.5);
        float seed = f.color.b;

        s.metallic = 0.0;
        s.occlusion = 1.0;
        s.emission = 0.0;
        [branch] if (zone == 0)
        {
            // MetalPaint: albedo variation (subtle on a painted vehicle), normal, smoothness
            float4 alb = NocSampleFade(TEXTURE2D_ARGS(_AlbedoTex, sampler_AlbedoTex), tuv, dx, dy, fade);
            float4 nrm = NocSampleFade(TEXTURE2D_ARGS(_NormalTex, sampler_NormalTex), tuv, dx, dy, fade);
            float4 msk = NocSampleFade(TEXTURE2D_ARGS(_MaskTex, sampler_MaskTex), tuv, dx, dy, fade);
            float2 slope = NocTangentToSlope(nrm, 0.6) * fade;
            float3 n1 = NocResolveNormal(n0, NocSurfGradTB(slope, T, B));
            float side = 1.0 - saturate(abs(n0.y) * 1.5);                         // side faces carry the livery and the seams
            float v = uv.y, u = uv.x;
            float3 a = _BaseColor.rgb * lerp(1.0, 2.0 * alb.rgb, 0.45);
            a *= lerp(1.0, 0.72, side * (1.0 - smoothstep(0.96, 1.0, v)));         // darker skirt below 1.0 m
            // two-tone livery (fifth start, BV_r01 KS06: the graphite body vanished into the dark podium and the 60 m tram read as a
            // floating window strip): a light warm-grey shoulder between the window head and the roof (v 2.86..3.45) and an 18 cm
            // light stripe over the skirt (v 0.78..0.96), _StripeColor x 2.8 (Granite x 0.75 x 2.8 = albedo ~0.44, a light paint;
            // BV_r02 KS06 at x 2.1: the bands at 33 luma on a 20 luma body),
            // each band box-filtered over one pixel (fp m / px), with the same CC0 albedo variation as the graphite
            float hb = max(0.5 * fp, 1e-4);
            float shoulder = saturate((min(3.45, v + hb) - max(2.86, v - hb)) / (2.0 * hb));
            float lowStripe = saturate((min(0.96, v + hb) - max(0.78, v - hb)) / (2.0 * hb));
            a = lerp(a, _StripeColor.rgb * 2.8 * lerp(1.0, 2.0 * alb.rgb, 0.45), side * max(shoulder, lowStripe));
            // seams: horizontal at 1.0 / 2.86 / 3.45 m, vertical every 2.92 m, door outlines (grooves 12 mm)
            float seamH = max(NocFilteredGrid1(v - 1.0, 100.0, 0.012, fp, 2.0), max(NocFilteredGrid1(v - 2.86, 100.0, 0.012, fp, 2.0), NocFilteredGrid1(v - 3.45, 100.0, 0.012, fp, 2.0)));
            float seamV = NocFilteredGrid1(u - 0.18, 2.92, 0.012, fp, 2.0);
            float inDoor = (step(TRAM_DOOR_A0, u) * step(u, TRAM_DOOR_A1) + step(TRAM_DOOR_B0, u) * step(u, TRAM_DOOR_B1)) * step(0.32, v) * step(v, 2.86);
            float doorEdge = max(NocFilteredGrid1(u - TRAM_DOOR_A0, 100.0, 0.016, fp, 2.0), max(NocFilteredGrid1(u - TRAM_DOOR_A1, 100.0, 0.016, fp, 2.0),
                             max(NocFilteredGrid1(u - TRAM_DOOR_B0, 100.0, 0.016, fp, 2.0), NocFilteredGrid1(u - TRAM_DOOR_B1, 100.0, 0.016, fp, 2.0))));
            float doorMid = NocFilteredGrid1(u - 0.5 * (TRAM_DOOR_A0 + TRAM_DOOR_A1), 100.0, 0.012, fp, 2.0) + NocFilteredGrid1(u - 0.5 * (TRAM_DOOR_B0 + TRAM_DOOR_B1), 100.0, 0.012, fp, 2.0);
            float doorBand = step(0.32, v) * step(v, 2.86);
            float seam = side * saturate(max(seamH, max(seamV * (1.0 - inDoor), doorBand * max(doorEdge, saturate(doorMid)))));
            a *= 1.0 - 0.6 * seam;
            // grime toward the sill and rain streaks under the window band (vertical value-noise streaks, faded below 2 px)
            float grime = side * (1.0 - smoothstep(0.3, 1.1, v)) * 0.35;
            float streakRaw = saturate((NocValueNoise(float2(u / 0.22 + seed * 17.0, v / 2.5)) - 0.55) * 2.5) * side * (1.0 - smoothstep(0.6, 1.0, v / TRAM_WIN_Y0));
            float keep = smoothstep(2.0, 4.0, 0.22 / max(fp, 1e-5));
            a *= (1.0 - grime) * (1.0 - 0.25 * lerp(0.15, streakRaw, keep));
            float up = saturate(n0.y);
            float smooth = lerp(0.55, 0.78, msk.a) + 0.12 * up - 0.18 * grime;
            float wear = saturate(f.color.g * 0.35);
            s.albedo = lerp(a, PAL_SteelF0 * 0.55, wear);
            s.metallic = wear;
            s.smoothness = saturate(smooth);
            s.occlusion = lerp(1.0, msk.g, 0.6);
            f.normalWS = n1;
        }
        else [branch] if (zone == 1)
        {
            float L = TramSaloon(uv.x, uv.y, fp, seed);
            s.albedo = PAL_GlassDark * 0.6;
            s.smoothness = 0.92;
            s.emission = _WinLight.rgb * (_WinGain * L);
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
            s.albedo = 0.012;
            s.smoothness = 0.35;
        }
        else if (zone == 5)
        {
            s.albedo = PAL_GlassDark * 0.5;
            s.smoothness = 0.95;
            s.emission = _WinLight.rgb * 0.012;                                  // the dim cab behind the windscreen
        }
        else if (zone == 6)
        {
            s.albedo = PAL_SteelF0 * 0.5;
            s.metallic = 1.0;
            s.smoothness = 0.55;
        }
        else
        {
            // destination band: a soft amber glow with a few dark dashes (no real text), its mean kept below 3 px
            float cell = floor(uv.x / 0.16);
            float on = NocU01(NocPcg((uint)(int)(cell + 4096.0) * 0x9E3779B9u + (uint)(seed * 255.0))) < 0.62 ? 1.0 : 0.25;
            float keepD = smoothstep(2.0, 3.0, 0.16 / max(fp, 1e-5));
            s.albedo = 0.02;
            s.smoothness = 0.9;
            s.emission = _Display.rgb * (_DisplayGain * lerp(0.715, on, keepD));
        }
        [branch] if (NocDebugView() == 8) f.debug = float4(frac(uv / 1.0), (float)zone / 7.0, 1.0);
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
