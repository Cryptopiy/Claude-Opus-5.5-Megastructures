// NocAlleyWindow - the residential windows of the alley podia (MASTER_PROMPT 14.5.1; Phase 4 wave B, AL, INT-06: the flat white
// window cards). A G1Mesh.Quad panel over the opening (panel-local metric UV0: u along the bottom edge, v up; UV1 = (W, H)) draws,
// by analytic ray casting in the panel frame (no texture, no keyword):
//   * the reveal: the opening is _Reveal (0.14 m) deep; a ray that meets a jamb, the head soffit or the sill before the glass plane
//     shades that concrete face (its own normal through the lit template, plus the room's spill near the glass);
//   * the window frame on the glass plane: a 5 cm frame, a mullion and a transom (painted, by seed), box filtered over the pixel;
//   * the room behind the glass (interior mapping, van Dongen): a box wider than the opening (0.9 m beyond each jamb, the floor
//     0.95 m under the sill, the ceiling 0.35 m over the head) and _RoomDepth x 0.8-1.3 deep, its walls, floor and ceiling lit
//     analytically by one lamp (warm pendant, or a cold ceiling panel for the cold rooms: N.L / (d^2 + 0.25) plus a 0.12 fill),
//     a furniture silhouette plane (a tall cabinet, a low sofa or table, by seed) at 0.6 of the depth, the pendant's shade where
//     the ray passes it;
//   * the covering, by panel state: none, open curtains, a sheer closed curtain, a half-lowered blind, one curtain half drawn;
//     fabric folds and blind slats are lit from the room side (translucent), and fade to their means below 3-6 px.
// Fix wave C14 (a lit transom crawled at s 105): the per-panel seed is decoded by rounding (the truncated byte x 257 flipped per
// pixel: see the seed line); and every pattern is filtered by the screen derivative (fwidth, before any branch) of
// its own coordinate, bars / furniture / shade by an exact box coverage (AlBox), the reveal edge by coverage: no crawl at grazing views.
// Per-panel state in the vertex colour: R = random, G = lit (1) or stand-by dark (0, 3 % light), B = covering x 32 / 255,
// A = cold share (0 warm WinWarm room, 1 cold WinCold room). The room light is _EmissionColor / _ColdColor x _RoomGain x the
// analytic radiance (mean about 1 over a lit window), so the windows keep the old flat cards' mean (shop factor x 0.6).
// Glass on top: GlassDark tint, smoothness 0.9 (the lit template reflects the sky). Every pattern is filtered by the pixel footprint
// grown with the ray depth; derivatives (NocUvFrame) are taken before any branch.
Shader "Megastructure/NocAlleyWindow"
{
    Properties
    {
        _BaseColor ("Glass tint (linear)", Color) = (0.0176, 0.0194, 0.0212, 1)
        [HDR] _EmissionColor ("Warm room light (linear HDR)", Color) = (0.9, 0.55, 0.27, 1)
        [HDR] _ColdColor ("Cold room light (linear HDR)", Color) = (0.33, 0.43, 0.6, 1)
        _RoomGain ("Room light gain (mean)", Range(0, 2)) = 0.6
        _RoomDepth ("Room depth (m)", Float) = 3.5
        _Reveal ("Reveal depth (m)", Float) = 0.14
        _RevealColor ("Reveal concrete albedo (linear)", Color) = (0.055, 0.052, 0.05, 1)
        _Seed ("Seed", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "../Include/NocLitCore.hlsl"
    #include "../Include/NocTexturing.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float4 _EmissionColor;
        float4 _ColdColor;
        float _RoomGain;
        float _RoomDepth;
        float _Reveal;
        float4 _RevealColor;
        float _Seed;
        float _Cull;
    CBUFFER_END

    // Box-filtered step: 0 below edge, 1 above, over one pixel footprint fp.
    float AlStep(float edge, float x, float fp) { return saturate((x - edge) / max(fp, 1e-5) + 0.5); }
    // Exact box filter of the interval [a, b] over the footprint fp centred at x: energy-conserving for thin bars (a bar narrower
    // than the pixel keeps its true coverage, width / fp, at every sub-pixel offset).
    float AlBox(float x, float a, float b, float fp)
    {
        float h = 0.5 * max(fp, 1e-5);
        return saturate((min(x + h, b) - max(x - h, a)) / (2.0 * h));
    }

    // Wall albedo palette of the rooms (linear): warm plaster, pale green-grey, blue-grey, off-white, faded rose-beige.
    float3 AlWallAlbedo(float r)
    {
        return r < 0.2 ? float3(0.42, 0.36, 0.28) : (r < 0.4 ? float3(0.30, 0.34, 0.30) : (r < 0.6 ? float3(0.28, 0.31, 0.36)
             : (r < 0.8 ? float3(0.46, 0.44, 0.40) : float3(0.42, 0.33, 0.30))));
    }

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        float3 P = f.positionWS;
        float3 n = f.normalWS;
        float2 uvP = f.uv.xy;                                                     // panel-local metres (G1Mesh.Quad)
        float2 WH = max(f.uv.zw, 0.05);                                           // opening size (UV1.xy)
        float3 T, B;
        NocUvFrame(uvP, P, n, T, B);                                              // derivatives: before any branch
        float3 d = -f.viewDirWS;                                                  // eye -> glass
        float dn = max(dot(d, -n), 0.04);                                         // into the wall
        float kx = dot(d, T) / dn, ky = dot(d, B) / dn;                           // panel metres per metre of depth
        float W = WH.x, H = WH.y;
        float xg = clamp(uvP.x, 0.0, W), yg = clamp(uvP.y, 0.0, H);
        float fp0 = f.footprint / max(dn, 0.2);

        float rnd = f.color.r;
        float lit = f.color.g;
        int cover = (int)(f.color.b * 255.0 / 32.0 + 0.5);
        float cold = f.color.a;
        // C14 ROOT CAUSE: rnd is a byte / 255, so rnd x 65535 = byte x 257 EXACTLY, and the interpolated colour wanders by an ulp
        // per pixel: the truncating cast flipped between byte x 257 and byte x 257 - 1 in scattered pixel columns, so those pixels
        // drew another room (depth, lamp, wallpaper, mullion, frame colour): the 1 px stripes that crawled frame to frame (seq_b12
        // frames 1-5, the same with -rain 0). The byte is now rounded (the seeds of the intended rooms are unchanged).
        uint seed = NocPcg((uint)(rnd * 255.0 + 0.5) * 257u + (uint)(_Seed + 0.5) * 0x9E3779B9u);
        float u1 = NocU01(NocPcg(seed + 1u)), u2 = NocU01(NocPcg(seed + 2u)), u3 = NocU01(NocPcg(seed + 3u)), u4 = NocU01(NocPcg(seed + 4u));
        float3 lightCol = lerp(_EmissionColor.rgb, _ColdColor.rgb, cold) * _RoomGain;
        float level = lerp(0.03, 1.0, lit);                                       // stand-by light in the dark rooms

        // ---- the reveal: jambs (x), head soffit / sill (y) before the glass plane at depth R
        float R = max(_Reveal, 0.01);
        float tJx = (kx >= 0.0 ? W - xg : xg) / max(abs(kx), 1e-5);
        float tJy = (ky >= 0.0 ? H - yg : yg) / max(abs(ky), 1e-5);
        // ---- the glass plane point
        float xg2 = xg + kx * R, yg2 = yg + ky * R;
        float fpG = fp0 * (1.0 + R / max(f.viewDist, 0.1));
        // fix wave C14 (the crawling transom): every pattern takes the TRUE footprint of its own coordinate, the screen derivative
        // (fwidth) of that coordinate taken here, before any branch. At a grazing view the old estimate fp0 = footprint / cos (cos
        // clamped at 0.2) undershot by 5x and more on the glass, and inside the room the hit point moves with d(tan)/d(angle) =
        // 1 / cos^2 per pixel: the folds, the wallpaper stripe and the furniture edges aliased into 1-4 px stripes that crawled.
        float2 fwG = max(fwidth(float2(xg2, yg2)), fpG);

        // ---- the room behind the glass (box wider than the opening)
        const float mx = 0.9, fy = 0.95, cy = 0.35;
        float D = max(_RoomDepth, 0.5) * lerp(0.8, 1.3, u1);
        float tx = (kx >= 0.0 ? W + mx - xg2 : xg2 + mx) / max(abs(kx), 1e-5);
        float ty = (ky >= 0.0 ? H + cy - yg2 : yg2 + fy) / max(abs(ky), 1e-5);
        float t = min(D, min(tx, ty));
        float3 hit = float3(xg2 + kx * t, yg2 + ky * t, t);
        float3 nR = t >= D - 1e-4 ? float3(0.0, 0.0, -1.0) : (tx <= ty ? float3(kx >= 0.0 ? -1.0 : 1.0, 0.0, 0.0) : float3(0.0, ky >= 0.0 ? -1.0 : 1.0, 0.0));
        bool isCeil = t < D - 1e-4 && tx > ty && ky >= 0.0;
        bool isFloor = t < D - 1e-4 && tx > ty && ky < 0.0;
        float fpR = fp0 * (1.0 + (R + t) / max(f.viewDist, 0.1));
        // the room's pattern coordinates and their true footprints (a face change inside the 2 x 2 quad gives a large fwidth: the
        // pattern fades to its mean along the room's corner lines, never a sparkle)
        float sc = nR.z < -0.5 ? hit.x : hit.z;                                   // the wallpaper stripe coordinate
        float fwS = max(fwidth(sc), fpR);
        float zf = D * 0.6;                                                       // the furniture plane
        float fx = xg2 + kx * zf, fyv = yg2 + ky * zf + fy;                       // across the room, height above the room floor
        float2 fwF = max(fwidth(float2(fx, fyv)), fpR);

        // the lamp: a warm pendant near the room's middle, or a cold ceiling panel
        float3 Lp = float3(0.5 * W + (u2 - 0.5) * 0.9, H + cy - lerp(0.45, 0.12, cold), D * lerp(0.35, 0.6, u3));
        float2 q = float2(xg2 + kx * Lp.z, yg2 + ky * Lp.z) - Lp.xy;              // the pendant's plane
        float2 fwQ = max(fwidth(q), fpR);
        float2 qc = hit.xz - Lp.xz;                                               // the cold panel on the ceiling
        float2 fwC = max(fwidth(qc), fpR);

        float3 albW = AlWallAlbedo(u4);
        float3 alb = isCeil ? float3(0.55, 0.53, 0.50) : (isFloor ? float3(0.16, 0.11, 0.08) : albW);
        // wallpaper: a faint vertical stripe on the walls (mean-preserving, faded to the mean below 4 px of pitch)
        float stripe = NocFilteredGrid1(sc, 0.24, 0.06, fwS, 4.0);
        alb *= (isCeil || isFloor) ? 1.0 : (1.0 - 0.18 * stripe * step(0.5, u2));
        float3 Lv = Lp - hit;
        float dl2 = dot(Lv, Lv);
        float E = max(dot(nR, Lv * rsqrt(max(dl2, 1e-4))), 0.0) * 1.6 / (dl2 + 0.25) + 0.12;
        // the cold panel spreads its light flatter and wider
        E = lerp(E, 0.55 * max(dot(nR, normalize(Lv)), 0.0) + 0.25, 0.5 * cold);
        float3 room = alb * E * 3.0;

        // furniture silhouette plane at 0.6 of the depth: a tall cabinet and a low sofa / table, by seed (exact box coverage)
        [branch] if (zf < t)
        {
            float c0 = lerp(-mx + 0.1, W + mx - 1.0, u3), c1 = lerp(-mx + 0.2, W + mx - 1.8, frac(u3 * 7.3 + 0.37));
            float tall = AlBox(fx, c0, c0 + 0.85, fwF.x) * AlBox(fyv, 0.0, 1.95, fwF.y);
            float low = AlBox(fx, c1, c1 + 1.7, fwF.x) * AlBox(fyv, 0.0, 0.78 + 0.1 * u4, fwF.y);
            float cover0 = saturate(tall + low);
            float3 Lf = Lp - float3(fx, fyv - fy, zf);
            float Ef = max(-Lf.z * rsqrt(max(dot(Lf, Lf), 1e-4)), 0.0) * 1.6 / (dot(Lf, Lf) + 0.25) + 0.10;
            float3 furn = lerp(float3(0.10, 0.07, 0.05), float3(0.09, 0.09, 0.10), frac(u4 * 3.7)) * Ef * 3.0;
            room = lerp(room, furn, cover0);
        }
        // the pendant shade (warm rooms) where the ray passes its plane in front of the hit
        [branch] if (Lp.z < t && cold < 0.5)
        {
            float wq = lerp(0.20, 0.10, saturate((q.y + 0.08) / 0.18));          // a cone shade, wide at the bottom
            float shade = AlBox(q.x, -wq, wq, fwQ.x) * AlBox(q.y, -0.10, 0.10, fwQ.y);
            float cord = AlBox(q.x, -0.006, 0.006, fwQ.x) * AlStep(0.10, q.y, fwQ.y);
            room = lerp(room, float3(2.4, 2.2, 1.9), shade) * (1.0 - 0.8 * cord);
        }
        // the cold ceiling panel
        [branch] if (isCeil && cold >= 0.5)
        {
            room += AlBox(qc.x, -0.6, 0.6, fwC.x) * AlBox(qc.y, -0.15, 0.15, fwC.y) * 3.0;
        }
        room *= level;

        // ---- the covering on the glass plane (lit from the room side); every pattern mean-preserving, faded below 3-6 px
        float3 em = room;
        float fold = 1.0 + 0.35 * sin(6.28318 * xg2 / 0.11 + u2 * 6.0) * smoothstep(3.0, 6.0, 0.11 / fwG.x);
        float3 fabric = lerp(float3(0.75, 0.62, 0.48), float3(0.55, 0.60, 0.62), frac(u1 * 5.1)) * fold * 0.45 * level * (0.6 + 0.4 * lit);
        [branch] if (cover == 1)
        {
            float cw = W * lerp(0.15, 0.28, u3);
            float c = saturate((1.0 - AlStep(cw, xg2, fwG.x)) + AlStep(W - cw, xg2, fwG.x));
            em = lerp(em, fabric, c);
        }
        else [branch] if (cover == 2)
        {
            em = lerp(em, fabric * 1.4, 0.72);                                    // a sheer: the room shows through it
        }
        else [branch] if (cover == 3)
        {
            float yb = H * lerp(0.35, 0.75, u3);                                  // the blind's bottom bar
            float on = AlStep(yb, yg2, fwG.y);
            const float pitch = 0.05;
            float keep = smoothstep(3.0, 6.0, pitch / fwG.y);
            // 1 on the slat, 0 in the gap; the pattern's mean is 0.5 and it fades to exactly that (C14: it faded to 0.8)
            float slat = lerp(0.5, smoothstep(0.15, 0.35, abs(frac(yg2 / pitch) - 0.5)), keep);
            float3 blind = lerp(room * 0.9, float3(0.80, 0.78, 0.72) * 0.40 * level * (0.6 + 0.4 * lit), slat);
            em = lerp(em, blind, on);
        }
        else [branch] if (cover == 4)
        {
            float cw = W * lerp(0.45, 0.62, u3);
            em = lerp(em, fabric, 1.0 - AlStep(cw, xg2, fwG.x));
        }

        // ---- the window frame on the glass plane: 5 cm frame, mullion, transom (exact box coverage per axis)
        float fr = 0.05;
        float frame = 1.0 - AlBox(xg2, fr, W - fr, fwG.x) * AlBox(yg2, fr, H - fr, fwG.y);
        frame = max(frame, AlBox(xg2, 0.5 * W - 0.022, 0.5 * W + 0.022, fwG.x) * step(0.5, u4));
        frame = max(frame, AlBox(yg2, 0.68 * H - 0.022, 0.68 * H + 0.022, fwG.y));
        float3 frameAlb = u2 < 0.5 ? float3(0.30, 0.29, 0.27) : float3(0.07, 0.07, 0.075);   // painted white-grey or dark metal

        // ---- compose: reveal, frame, glass. The reveal is where the glass-plane point leaves the opening (tRev < R <=> xg2 or yg2
        // outside [0, W] x [0, H]); its edge is box filtered (coverage inG), not a per-pixel switch
        float inG = AlBox(xg2, 0.0, W, fwG.x) * AlBox(yg2, 0.0, H, fwG.y);
        // reveal face normal (world): the jamb faces the opening's centre, the head faces down, the sill up
        float3 nRev = tJx <= tJy ? (kx >= 0.0 ? -T : T) : (ky >= 0.0 ? -B : B);
        float spill = saturate(min(tJx, tJy) / R);                                 // the room light reaches the inner reveal
        float3 revEm = lightCol * level * 0.06 * spill;

        s.metallic = 0.0;
        s.occlusion = 1.0;
        float3 glassAlb = lerp(_BaseColor.rgb, frameAlb, frame);
        float glassSm = lerp(0.9, 0.45, frame);
        float3 glassEm = lerp(lightCol * em, lightCol * level * 0.05, frame);
        s.albedo = lerp(_RevealColor.rgb, glassAlb, inG);
        s.smoothness = lerp(0.25, glassSm, inG);
        s.emission = lerp(revEm, glassEm, inG);
        f.normalWS = normalize(lerp(nRev, n, inG));
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
