// Megastructure/BwWaterPortal - the sea behind one 20 x 36 m pane of the window gallery (MASTER_PROMPT 14.7 H3 "the window
// is a portal, not a mesh", 14.5.6; Addendum A1 1.8). Beat 6 owner. Drawn on the inner glass face (axis + 6.0) at queue
// Geometry+10 after every interior opaque, in two passes (a stencil mark where the glass is visible, then the water with
// ZTest Always on the marked pixels); per pixel it continues the camera ray through the 2.4 m glass and the conical
// reveal (the opening stays 20 x 36 m through the glass, then flares 45 deg for 3.5 m to the dam's sea face) and shades:
//   * the reveal surface it hits first (analytic: the glass seat inside the glass thickness, the flared concrete beyond),
//     lit by the water's own light (the veil irradiance of the face's normal), the survey lamps and the 14.7 H3 object model
//     L0 exp(-c r) + veil (1 - exp(-c r)) over the water path;
//   * the arm above panes 1-3 (A1 1.8 "the light goes out", s 1072-1212): a round-cone SDF in the water (BwWater.hlsl
//     BwWArmTrace / BwWArmColour), soft-edged, shaded by the same object model; its solid core writes depth;
//   * otherwise the open water: the veil L_c = G S_c exp(-Kd_c z) exp(beta sin e) of the camera depth (BwWVeil, the twin of
//     the creature's KrVeil), with the upper veil dimmed by _BwWState.x while the arm passes above (A1 1.10 simplified tier),
//     2 % noise of 40 m cells and the hard floor (linear 0.0018 = sRGB 6 before the dither, H5).
// Depth: SV_Depth = the reveal hit, or a point 90 m beyond the glass (T_B 1 %; inside every beat-6 far plane): the creature
// (queue Geometry+20) depth-tests against it, every interior surface in front keeps its pixels (stencil), and the far field
// (Geometry+499, depth forced far) never draws over the water. Nothing outside the glass shows inside a pier: the
// piers are real interior geometry. The in-scatter in front of the creature (shafts, lamp cones) is BwWaterPortalScatter's.
Shader "Megastructure/BwWaterPortal"
{
    Properties
    {
        _NoiseAmp ("Cell noise amplitude (2 %)", Range(0, 0.1)) = 0.02
        _RevealAlbedo ("Reveal albedo scale", Range(0, 2)) = 1.0
        _SeatAlbedo ("Glass seat albedo scale", Range(0, 2)) = 0.35
        _FarBeyond ("Depth written beyond the glass (m)", Float) = 90
        _CutY ("Near-surface cut height (world y)", Float) = -30
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Include/BulwarkPalette.hlsl"
    #include "Include/BwCommon.hlsl"
    #include "Include/BulwarkAtmosphere.hlsl"
    #include "Generated/BulwarkConstGen.hlsl"
    #include "Include/BwWater.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _NoiseAmp;
        float _RevealAlbedo;
        float _SeatAlbedo;
        float _FarBeyond;
        float _CutY;
    CBUFFER_END

    struct PortalAttributes { float4 positionOS : POSITION; };
    struct PortalVaryings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };

    PortalVaryings PortalVert(PortalAttributes v)
    {
        PortalVaryings o;
        o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
        o.positionCS = TransformWorldToHClip(o.positionWS);
        return o;
    }

    // First hit of the ray G + d t (t > 0, metres) with the reveal of the pane containing G. Returns t (or -1) and the hit
    // normal (world, facing the opening); seat = 1 when the hit lies inside the glass thickness.
    float PortalReveal(float3 G, float3 d, out float3 nrm, out float seat)
    {
        nrm = float3(0, 1, 0); // pal:ok (not a colour)
        seat = 0.0;
        float3 T = _BwWTan.xyz, R = _BwWPlane.xyz;
        float sg = BwWHallS(G);
        float k = clamp(round((sg - TC_PaneS0) / TC_PanePitch), 0.0, TC_PaneCount - 1.0);
        float sc = TC_PaneS0 + TC_PanePitch * k;
        float du = dot(d, T), dw = max(dot(d, R), 1e-4), dy = d.y;
        float hw = 0.5 * TC_PaneWidth;
        float gIn = TC_GlassThickness, wEnd = TC_GlassThickness + TC_GlassRevealDepth;
        float yt0 = BwWFloorY(sg) + TC_PaneFloorOffset + TC_PaneHeight, yb0 = BwWFloorY(sg) + TC_PaneFloorOffset;
        // the interpolated quad point can lie a hair outside its own pane on the edge pixels; then the seat face it should hit
        // has t <= 0 and the pixel showed open water: a one-pixel blue seam along the frames (gallery-final3 pose_w1085p40).
        // Clamped 2 mm inside, every edge pixel hits its seat.
        float u0 = clamp(sg - sc, -hw + 0.002, hw - 0.002);
        float gy = clamp(G.y, yb0 + 0.002, yt0 - 0.002);
        float e = dy + TC_GalleryRamp * du;          // rate of y relative to the sloping pane edges
        float best = 1e9; float3 bn = nrm; float bs = 0.0;
        float t;
        // side +u: straight (inside the glass), then flared
        if (du > 1e-5) { t = (hw - u0) / du; if (t > 0 && dw * t <= gIn && t < best) { best = t; bn = -T; bs = 1; } }
        if (du - dw > 1e-5) { t = (hw - gIn - u0) / (du - dw); float w = dw * t; if (t > 0 && w > gIn && w <= wEnd && t < best) { best = t; bn = normalize(-T + R); bs = 0; } }
        // side -u
        if (du < -1e-5) { t = (-hw - u0) / du; if (t > 0 && dw * t <= gIn && t < best) { best = t; bn = T; bs = 1; } }
        if (du + dw < -1e-5) { t = (-hw + gIn - u0) / (du + dw); float w = dw * t; if (t > 0 && w > gIn && w <= wEnd && t < best) { best = t; bn = normalize(T + R); bs = 0; } }
        // top
        if (e > 1e-5) { t = (yt0 - gy) / e; if (t > 0 && dw * t <= gIn && t < best) { best = t; bn = float3(0, -1, 0); bs = 1; } } // pal:ok (normal)
        if (e - dw > 1e-5) { t = (-gIn - (gy - yt0)) / (e - dw); float w = dw * t; if (t > 0 && w > gIn && w <= wEnd && t < best) { best = t; bn = normalize(float3(0, -1, 0) + R); bs = 0; } } // pal:ok (normal)
        // bottom
        if (e < -1e-5) { t = (yb0 - gy) / e; if (t > 0 && dw * t <= gIn && t < best) { best = t; bn = float3(0, 1, 0); bs = 1; } } // pal:ok (normal)
        if (e + dw < -1e-5) { t = (gIn - (gy - yb0)) / (e + dw); float w = dw * t; if (t > 0 && w > gIn && w <= wEnd && t < best) { best = t; bn = normalize(float3(0, 1, 0) + R); bs = 0; } } // pal:ok (normal)
        nrm = bn; seat = bs;
        return best < 1e8 ? best : -1.0;
    }

    // Irradiance-like light of a surface facing n under water at point p: half the veil of its normal (the water's own light
    // falls on it from the hemisphere it faces) plus the lamps' Lambert term.
    float3 PortalSurfaceLight(float3 p, float3 n, float depth)
    {
        float3 Lv = BwWVeil(normalize(n + float3(0, 0.35, 0)), depth); // pal:ok (direction, not a colour)
        float3 lamp = 0.0;
        float3 la = _BwWLampA0.xyz - p, lb = _BwWLampB0.xyz - p;
        lamp += _BwWLampCol.rgb * BwWLampAt(p, _BwWLampA0, _BwWLampA1) * saturate(dot(n, normalize(la)) * 0.8 + 0.2);
        lamp += _BwWLampCol.rgb * BwWLampAt(p, _BwWLampB0, _BwWLampB1) * saturate(dot(n, normalize(lb)) * 0.8 + 0.2);
        return 0.5 * Lv + lamp;
    }

    void PortalShade(PortalVaryings i, out float3 col, out float3 Pdepth)
    {
        float3 cam = _WorldSpaceCameraPos;
        float3 G = i.positionWS;
        float3 v = G - cam;
        float dist0 = max(length(v), 1e-3);
        float3 d = v / dist0;
        float camDepth = max(BWW_SEA_Y - cam.y, 0.0);
        float3 veil = BwWVeil(d, camDepth);
        // the arm passing above dims the UPPER veil only (it blocks the downwelling light; A1 1.8 "exposure does not compensate")
        veil *= lerp(1.0, _BwWState.x, saturate(d.y * 1.6 + 0.2));
        float3 nrm; float seat;
        float th = PortalReveal(G, d, nrm, seat);
        // critic S3-10: the reveal's outer edge (where the flare meets the dam's sea face, open water beyond) is found here,
        // not rasterised, so MSAA cannot smooth it (KS16 jambs: 1.6 % intermediate pixels). Four rotated-grid sub-rays (the
        // pixel's own footprint on the pane, ddx / ddy of the quad point) give the reveal's coverage of the pixel; the reveal and
        // the water behind it are mixed by it. The depth stays the centre ray's (the creature tests against it).
        float3 gdx = ddx(G), gdy = ddy(G);
        float cov = 0.0;
        float3 Gs = G, ds = d, nrms = nrm; float ths = th, seats = seat;
        [unroll] for (int q = 0; q < 4; q++)
        {
            float2 o = q == 0 ? float2(0.125, 0.375) : (q == 1 ? float2(-0.375, 0.125) : (q == 2 ? float2(-0.125, -0.375) : float2(0.375, -0.125)));
            float3 Gq = G + gdx * o.x + gdy * o.y;
            float3 dq = normalize(Gq - cam);
            float3 nq; float sq;
            float tq = PortalReveal(Gq, dq, nq, sq);
            if (tq > 0.0)
            {
                cov += 0.25;
                if (ths <= 0.0) { ths = tq; Gs = Gq; ds = dq; nrms = nq; seats = sq; }
            }
        }
        if (th > 0.0) cov = max(cov, 0.25);
        float3 c = veil;
        float3 cRev = veil;
        if (ths > 0.0)
        {
            float3 P = Gs + ds * ths;
            float depthP = max(BWW_SEA_Y - P.y, 0.0);
            float tGlass = TC_GlassThickness / max(dot(ds, _BwWPlane.xyz), 1e-3);
            float rWater = max(ths - tGlass, 0.0);
            float3 cext = float3(TC_BeamCR, TC_BeamCG, TC_BeamCB); // pal:ok (beam attenuation c of 14.7 H3, not a colour)
            float3 Tr = exp(-cext * rWater);
            float3 alb = seats > 0.5 ? PAL_SteelPaint * (0.25 * _SeatAlbedo) : PAL_ConcreteDark * _RevealAlbedo;
            float3 Lsurf = alb * PortalSurfaceLight(P, nrms, depthP);
            // grime streaks on the reveal concrete (rest space: hall s and depth), filtered by the footprint
            float sP = BwWHallS(P);
            float fp = ths * _BwCam.w * 2.0 + 1e-3;
            float streak = BwValueNoise3(float3(sP * 0.7, P.y * 0.06, 0.0)) * saturate(1.0 - fp / 1.5); // pal:ok (noise coordinates)
            Lsurf *= 0.75 + 0.5 * streak;
            cRev = Lsurf * Tr + veil * (1.0 - Tr);
        }
        if (th > 0.0) Pdepth = G + d * th;
        else
        {
            // the water ends what can be seen of anything beyond it: depth at _FarBeyond (90 m: T_B 1 %), and for rays rising
            // toward the surface, at y -30 once beyond 40 m (the creature's near-surface parts are lit as if in air by its own
            // model; the veil hides them as the water would)
            float tCut = _FarBeyond;
            if (d.y > 1e-3) tCut = min(tCut, max(40.0, (_CutY - G.y) / d.y));
            Pdepth = G + d * tCut;
        }
        if (cov < 1.0)
        {
            float tCutW = _FarBeyond;
            if (d.y > 1e-3) tCutW = min(tCutW, max(40.0, (_CutY - G.y) / d.y));
            // the arm above panes 1-3 (A1 1.8, s 1072-1212): soft coverage over the open water; its solid core writes depth
            // (the scatter takes the same coverage, so the in-scatter behind it fades with the edge, never steps)
            float tGlassA = TC_GlassThickness / max(dot(d, _BwWPlane.xyz), 1e-3);
            float tArm;
            float aArm = BwWArmTrace(G, d, tGlassA, tCutW, dist0, tArm);
            if (aArm > 1e-3)
            {
                float3 Pa = G + d * tArm;
                c = lerp(c, BwWArmColour(Pa, veil, tArm - tGlassA), aArm);
                if (aArm > 0.98 && th <= 0.0) Pdepth = Pa;
            }
        }
        c = lerp(c, cRev, cov);
        // 2 % noise of 40 m cells (H3 darkness: breaks the deep gradient into something the encoder keeps)
        float n = BwValueNoise3((cam + d * 40.0) / 40.0 + float3(0.37, 0.11, 0.73)); // pal:ok (noise offset)
        c *= 1.0 + _NoiseAmp * (2.0 * n - 1.0);
        c = max(c, TC_DarkFloorLinear);
        col = BwFogApply(c, G);
    }

    float PortalDepth(float3 P)
    {
        float4 cs = TransformWorldToHClip(P);
        return cs.z / max(cs.w, 1e-6);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+10" "IgnoreProjector" = "True" }

        // Pass 1 (drawn first: URP draws SRPDefaultUnlit before UniversalForward): marks stencil bit 64 where the glass itself
        // is visible (the quad's own depth against the interior: the rail, the frames, a pier in front keep their pixels).
        Pass
        {
            Name "PortalMask"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            ZWrite Off ZTest LEqual Cull Back ColorMask 0
            Stencil { Ref 64 ReadMask 64 WriteMask 64 Comp Always Pass Replace }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex PortalVert
            #pragma fragment PortalMaskFrag
            half4 PortalMaskFrag(PortalVaryings i) : SV_Target { return 0; }
            ENDHLSL
        }
        // Pass 2: the water on exactly those pixels, ZTest Always: whatever another owner left beyond the glass plane is
        // replaced by water (the portal is authoritative, 14.7 H3); the creature is drawn after it (queue Geometry+20, set by
        // Beat06Encounter) and depth-tests against the depth written here. The stencil bit is cleared again.
        Pass
        {
            Name "Portal"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On ZTest Always Cull Back
            Stencil { Ref 64 ReadMask 64 WriteMask 64 Comp Equal Pass Zero Fail Keep }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex PortalVert
            #pragma fragment PortalFrag
            half4 PortalFrag(PortalVaryings i, out float depth : SV_Depth) : SV_Target
            {
                float3 col, P;
                PortalShade(i, col, P);
                depth = PortalDepth(P);
                return half4(col, 1.0);
            }
            ENDHLSL
        }
        // The pane blocks the sun like the dam it stands in (the water would remove almost all of the direct sun at this
        // depth anyway): two-sided, URP shadow bias.
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex PortalShadowVert
            #pragma fragment PortalShadowFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            float3 _LightDirection;
            struct PSA { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            float4 PortalShadowVert(PSA v) : SV_POSITION
            {
                float3 p = TransformObjectToWorld(v.positionOS.xyz);
                float3 n = TransformObjectToWorldNormal(v.normalOS);
                float4 cs = TransformWorldToHClip(ApplyShadowBias(p, n, _LightDirection));
            #if UNITY_REVERSED_Z
                cs.z = min(cs.z, UNITY_NEAR_CLIP_VALUE);
            #else
                cs.z = max(cs.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                return cs;
            }
            half4 PortalShadowFrag() : SV_Target { return 0; }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ZTest LEqual ColorMask R Cull Back
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex PortalVert
            #pragma fragment PortalDepthFrag
            half PortalDepthFrag(PortalVaryings i, out float depth : SV_Depth) : SV_Target
            {
                float3 col, P;
                PortalShade(i, col, P);
                depth = PortalDepth(P);
                return 0;
            }
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On ZTest LEqual Cull Back
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex PortalVert
            #pragma fragment PortalNormalsFrag
            half4 PortalNormalsFrag(PortalVaryings i, out float depth : SV_Depth) : SV_Target
            {
                float3 col, P;
                PortalShade(i, col, P);
                depth = PortalDepth(P);
                return half4(NormalizeNormalPerPixel(-_BwWPlane.xyz), 0.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
