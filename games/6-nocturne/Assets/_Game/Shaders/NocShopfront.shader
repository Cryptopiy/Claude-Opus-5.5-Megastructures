// NocShopfront - lit rooms behind shop glass, alley windows, door transoms and vending fronts (Look v2, TEXTURE_PLAN 3.5 SU-4;
// CRIT0 #24: the flat clipped emissive rectangles). A G1Mesh.Quad panel carries panel-local metric UV0 (u along its bottom edge,
// v up) and its size in UV1 (W, H). The view ray enters a box room W wide, H tall and _RoomDepth deep behind the glass (FA's
// NocInterior: van Dongen's slab form, each surface linear in its exit depth) and picks up structure where it lands: shelf
// boards and product rows on the back and side walls, a ceiling light strip near the glass, a darker floor; every pattern fades
// to its mean below 2-4 px of its own size, so nothing shimmers. The room's light is _EmissionColor x _RoomGain (0.6: the bloom
// carries the brightness) x raw / mean (FA's exact closed-form NocRoomMeanV2 for this view), so the panel's mean stays the
// slot's emission x gain and the room only redistributes it. On top, dark glass (GlassDark, smoothness 0.9) reflects the sky
// through the lit template. No keyword, no texture.
// BV (Phase 4 wave A, INT-22): a per-panel state from the vertex colour, so one slot shows a street of different shops. The
// MeshBuilder default (1, 0, 0, 1) is the lit room above, unchanged (the alley and every other user). R = a per-panel random
// (states 1-3), G = light reduction (room gain x (1 - G)), B = state x 32: 0 the lit room, 1 a closed roller shutter (painted
// corrugated steel, slats 75 mm faded to their mean below 2-4 px, grime toward the bottom; some leak a thin line of room light
// under the bottom bar, some are perforated and glow at 6 % of the room), 2 a half-lowered shutter (its bottom edge at A x H; the
// lit room below), 3 a dark shop (glass over a room at 4 % stand-by light).
Shader "Megastructure/NocShopfront"
{
    Properties
    {
        _BaseColor ("Glass tint (linear)", Color) = (0.0176, 0.0194, 0.0212, 1)
        [HDR] _EmissionColor ("Room light (linear HDR)", Color) = (1, 0.8, 0.6, 1)
        _RoomGain ("Room light gain (mean)", Range(0, 2)) = 0.6
        _RoomDepth ("Room depth (m)", Float) = 4
        _ShelfPitch ("Shelf pitch (m)", Float) = 0.45
        _ItemWidth ("Product width (m)", Float) = 0.12
        _Seed ("Seed", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "Include/NocLitCore.hlsl"
    #include "Include/NocTexturing.hlsl"
    #include "Generated/NocFacadeConstantsGen.hlsl"
    #include "Include/NocInterior.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float4 _EmissionColor;
        float _RoomGain;
        float _RoomDepth;
        float _ShelfPitch;
        float _ItemWidth;
        float _Seed;
        float _Cull;
    CBUFFER_END

    // Structure of a wall at its 2D coordinate q (m): shelf boards (dark, 3 cm) every pitch, product rows between them (hashed
    // brightness 0.55-1.45 per item cell, 12 % gaps, half of the items in a muted packaging tint of luma ~1), faded to the mean 1 below
    // 2-4 px of the item width; fp = metres per pixel there. rgb, mean ~1.
    float3 NocShelves(float2 q, float pitch, float itemW, float fp, uint seed)
    {
        float keep = smoothstep(2.0, 4.0, itemW / max(fp, 1e-5));
        float row = floor(q.y / pitch);
        float shift = NocU01(NocPcg((uint)(int)row * 0x9E3779B9u + seed)) * itemW;
        float2 cell = float2(floor((q.x + shift) / itemW), row);
        uint h = NocPcg2((int)cell.x + (int)seed, (int)cell.y * 7919 + 13);
        float item = NocU01(h) < 0.12 ? 0.35 : lerp(0.55, 1.45, NocU01(NocPcg(h + 17u)));
        uint ht = NocPcg(h + 29u);
        float k = NocU01(ht);
        float3 tint = k < 0.1 ? float3(1.25, 0.85, 0.70) : (k < 0.2 ? float3(1.10, 1.05, 0.70) : (k < 0.3 ? float3(0.80, 1.10, 0.90)
                    : (k < 0.4 ? float3(0.80, 0.95, 1.30) : (k < 0.5 ? float3(1.20, 0.80, 1.00) : float3(1.0, 1.0, 1.0)))));
        float fy = (q.y / pitch) - row;
        float board = NocFilteredGrid1(q.y, pitch, 0.03, fp, 2.0);            // the board line itself, box filtered
        float top = smoothstep(0.85, 1.0, fy) * 0.5;                            // the shadowed gap under the next board
        float3 sc = lerp(tint * (item * (1.0 - top)), 0.25, board);
        float mean = lerp(0.922 * 0.9625, 0.25, 0.03 / pitch);                 // E[item] 0.922 x E[1 - top] 0.9625, blended with the boards
        return lerp(mean, sc, keep) / mean;
    }

    // BV (Phase 4, SG's request): a tall room (H 2.4-2.8 m and up, blended) keeps its shelves only up to a top board at
    // hs = min(H - 0.6, 2.3) m; above it a plain wall at 0.45 x and a 0.25 m lit frieze (x 1.6) under the ceiling, so 3-5 m rooms
    // stop reading as walls of boxes. The profile is divided by its own mean over the wall height (the 4 cm board ignored: 0.3 %),
    // so the room's mean stays the slot's emission x gain. Steps box filtered over one pixel (fp). Windows, transoms and vending
    // fronts (H < 2.4 m) are unchanged. rgb, mean ~1.
    float3 NocShopWall(float2 q, float H, float pitch, float itemW, float fp, uint seed)
    {
        float3 sh = NocShelves(q, pitch, itemW, fp, seed);
        float tall = smoothstep(2.4, 2.8, H);
        float hs = min(H - 0.6, 2.3), hf = H - 0.25;
        float fpc = max(fp, 1e-4);
        float aboveS = smoothstep(hs - 0.5 * fpc, hs + 0.5 * fpc, q.y);
        float inF = smoothstep(hf - 0.5 * fpc, hf + 0.5 * fpc, q.y);
        float bw = 0.5 * max(0.04, fpc);
        float boardT = (1.0 - smoothstep(bw - 0.5 * fpc, bw + 0.5 * fpc, abs(q.y - hs))) * min(1.0, 0.04 / fpc);
        float3 prof = lerp(sh, 0.45, aboveS);
        prof = lerp(prof, 1.6, inF);
        prof = lerp(prof, 0.25, boardT);
        float m = (hs + (hf - hs) * 0.45 + 0.25 * 1.6) / max(H, 0.1);
        return lerp(sh, prof / m, tall);
    }

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        float3 P = f.positionWS;
        float3 n = f.normalWS;
        float2 uvP = f.uv.xy;                                                     // panel-local metres (G1Mesh.Quad)
        float2 WH = max(f.uv.zw, 0.05);                                           // panel size (UV1.xy)
        float3 T, B;
        NocUvFrame(uvP, P, n, T, B);                                              // derivatives: before any branch
        float3 d = -f.viewDirWS;                                                  // eye -> glass
        float dn = max(dot(d, -n), 0.05);                                         // into the room
        float kx = dot(d, T) / dn, ky = dot(d, B) / dn;
        float W = WH.x, H = WH.y, Dr = max(_RoomDepth, 0.05);
        float xg = clamp(uvP.x, 0.0, W), yg = clamp(uvP.y, 0.0, H);

        // FA's room: the exit depth and which surface the ray leaves through
        float tx = (kx >= 0.0 ? W - xg : xg) / max(abs(kx), 1e-5);
        float ty = (ky >= 0.0 ? H - yg : yg) / max(abs(ky), 1e-5);
        float t = min(Dr, min(tx, ty));
        float raw = NocRoomRawV2(xg, yg, kx, ky, W, 0.0, H, Dr);
        float mean = NocRoomMeanV2(0.0, W, 0.0, H, kx, ky, W, 0.0, H, Dr);
        float3 hit = float3(xg + kx * t, yg + ky * t, t);                         // (across, up, depth) in the room
        // metres per pixel at the hit: the glass footprint grown by the depth behind it
        float fp = f.footprint * (1.0 + t / max(f.viewDist, 0.1)) / max(dn, 0.2);
        uint seed = (uint)(_Seed + 0.5) * 0x632BE5ABu + 0x53484F50u;
        float3 st = 1.0;
        [branch] if (t >= Dr - 1e-4) st = NocShopWall(hit.xy, H, _ShelfPitch, _ItemWidth, fp, seed);                       // back wall
        else [branch] if (tx <= ty) st = NocShopWall(float2(hit.z, hit.y), H, _ShelfPitch, _ItemWidth, fp, seed + 101u);  // side wall
        else if (ky >= 0.0)                                                                                             // ceiling strip
            st = 1.0 + 0.8 * (1.0 - smoothstep(0.0, 0.6, abs(hit.x - 0.5 * W) / max(0.5 * W, 0.1))) * (1.0 - smoothstep(0.3, 1.5, hit.z));
        else st = 0.8;                                                                                                  // floor
        float3 room = raw / max(mean, 1e-4) * st;

        s.albedo = _BaseColor.rgb;
        s.metallic = 0.0;
        s.smoothness = 0.9;
        s.occlusion = 1.0;
        // BV: the panel's state (vertex colour; the default is the lit room at full gain)
        float gainV = saturate(1.0 - f.color.g);
        int state = (int)(f.color.b * 255.0 / 32.0 + 0.5);
        float3 roomMean = _EmissionColor.rgb * (_RoomGain * gainV);
        s.emission = roomMean * room;
        [branch] if (state == 3)
        {
            s.emission = roomMean * room * 0.04;                                  // a dark shop: stand-by light deep inside
        }
        else [branch] if (state == 1 || state == 2)
        {
            float rnd = f.color.r;
            float fpS = max(f.footprint, 1e-5);                                   // the shutter lies on the glass plane
            float yS = state == 2 ? H * f.color.a : 0.0;                          // the shutter's bottom edge (panel metres)
            float yy = uvP.y - yS;
            float onS = smoothstep(-0.5 * fpS, 0.5 * fpS, yy);
            // corrugated slats: grooves (filtered lines) and a triangle-wave ridge shading with its normal tilt, faded to the mean
            const float pitchS = 0.075;
            float keepS = smoothstep(2.0, 4.0, pitchS / fpS);
            float groove = NocFilteredGrid1(uvP.y, pitchS, 0.010, fpS, 2.0);
            float ph = frac(uvP.y / pitchS);
            float ridge = keepS * (4.0 * abs(ph - 0.5) - 1.0);                    // -1..1, mean 0
            float slope = keepS * 0.40 * sin(6.28318530718 * ph);                 // mean 0
            // paint by panel: grey, green-grey, blue-grey, warm grey, rust
            float3 paint = rnd < 0.34 ? float3(0.090, 0.092, 0.095) : (rnd < 0.52 ? float3(0.062, 0.078, 0.070)
                         : (rnd < 0.72 ? float3(0.070, 0.074, 0.088) : (rnd < 0.88 ? float3(0.112, 0.104, 0.094) : float3(0.095, 0.064, 0.046))));
            float grime = 0.35 * (1.0 - smoothstep(0.0, 1.4, yy));
            float streakRaw = saturate((NocValueNoise(float2(uvP.x / 0.18 + rnd * 91.0, uvP.y / 1.6)) - 0.5) * 2.2);
            float keepN = smoothstep(2.0, 4.0, 0.18 / fpS);
            float streak = lerp(0.2, streakRaw, keepN);
            float bar = 1.0 - smoothstep(0.06 - 0.5 * fpS, 0.06 + 0.5 * fpS, yy);  // the heavier bottom bar
            float rail = 1.0 - smoothstep(0.05 - 0.5 * fpS, 0.05 + 0.5 * fpS, min(uvP.x, W - uvP.x));   // side guides
            float3 alb = paint * (1.0 + 0.12 * ridge) * (1.0 - 0.55 * groove) * (1.0 - grime) * (1.0 - 0.25 * streak);
            alb = lerp(alb, paint * 0.45, saturate(bar + rail));
            // light through it: a thin leak under the bottom bar (state 1, about 45 % of the shutters) or a perforated glow (20 %)
            float k2 = frac(rnd * 7.31);
            float gap = 0.025, lw = max(gap, fpS);
            float leakBand = (1.0 - smoothstep(-lw - 0.5 * fpS, -lw + 0.5 * fpS, -abs(uvP.y - 0.5 * gap) * 2.0)) * (gap / lw);
            float3 em = 0.0;
            if (state == 1 && k2 < 0.45) em = roomMean * (0.9 * leakBand);
            else if (k2 > 0.80) em = roomMean * (0.06 * (1.0 - groove));
            float3 nS = normalize(n + B * slope);
            s.albedo = lerp(s.albedo, alb, onS);
            s.smoothness = lerp(0.9, 0.5 - 0.2 * grime, onS);
            s.emission = lerp(s.emission, em, onS);
            f.normalWS = normalize(lerp(n, nS, onS));
        }
        [branch] if (NocDebugView() == 8) f.debug = float4(frac(uvP / 0.25), 0.5, 1.0);
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
            #include "Include/NocLitPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
