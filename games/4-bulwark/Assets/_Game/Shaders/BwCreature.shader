// BwCreature - the kraken's skin (Addendum A1, kraken owner; 14.2.6 "BwCreature"). Lit template (BwLitCore + BwLitPass: SRP
// Batcher, the four passes, analytic environment, specular AA and clamps), GPU spline skinning in the vertex hook (all
// passes, BwCreatureSkin.hlsl), its own forward fragment (BwCreatureForward.hlsl: thin-part scattering, the underwater object
// model of 14.7 H3, BwFogApply in air). Surfaces:
//   * body (_KrTube 0): the graded Tripo atlas (Kraken_Albedo / Normal / Mask), blended into the tentacle tile over the last
//     84 m of every arm (normal through a derivative cotangent frame, so the two uv frames never mix);
//   * tubes (_KrTube 1): the tentacle tile (Tentacle_Albedo / Normal / Mask) at (theta, v(u));
//   * on both, in REST space (the pattern sticks to the moving skin): barnacle crust on the back (Worley shells 3.5 m with
//     dark openings, colonies by a 40 m field, faded below 6 px per shell), grey-green moss, long pale scars, slime
//     (smoothness 0.45-0.75), water sheets running down emerged skin (wetness per limb), darker crevices (AO x cavity),
//     the A1 1.3 "passing cloud" chromatophore bands (11.7 m/s, 0.07 Hz: lead decision 2, +-15 % albedo), a sucker wave, and
//     the eyes' light on the inner socket walls (mask A x eye gain; A1 1.2: nothing else glows);
//   * LOD cross-fade: _KrFade (x coverage, y 0 = keep noise < x, 1 = keep noise >= x) on the static blue-noise tile.
// Colours only through the A1 PAL_Kraken* tokens; emission HDR within 14.2.5.
Shader "Megastructure/BwCreature"
{
    Properties
    {
        [NoScaleOffset] _AlbedoMap ("Body albedo", 2D) = "grey" {}
        [NoScaleOffset] _NormalMap ("Body normal", 2D) = "bump" {}
        [NoScaleOffset] _MaskMap ("Body mask (AO, smooth, cavity, socket glow)", 2D) = "white" {}
        [NoScaleOffset] _TentAlbedo ("Tentacle albedo", 2D) = "grey" {}
        [NoScaleOffset] _TentNormal ("Tentacle normal", 2D) = "bump" {}
        [NoScaleOffset] _TentMask ("Tentacle mask (AO, smooth, height, sucker)", 2D) = "white" {}
        _KrTube ("Tube (1) or body (0)", Float) = 0
        _KrFade ("LOD fade (x coverage, y mode, z LOD index)", Vector) = (1, 0, 0, 0)
        _NormalStrength ("Normal strength (BC5 unpack fixed in Wave X: 2.2 x the baked slopes)", Range(0, 3)) = 2.2
        _Barnacles ("Barnacle crust", Range(0, 1)) = 0.85
        _Scars ("Scars", Range(0, 1)) = 0.7
        _Clouds ("Passing-cloud bands (A1 1.3, +-15 %)", Range(0, 1)) = 1.0
        _SocketGlow ("Eye light on the socket walls", Range(0, 3)) = 0.18
        _RimScatter ("Thin-part scattering", Range(0, 2)) = 0.8
        _Slime ("Slime smoothness boost", Range(0, 0.3)) = 0.10
        _Sheets ("Water sheets", Range(0, 1)) = 0.8
        _DamClip ("Dam clip for lee-side cameras (reads the global _BwDamClip)", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "Include/BwLitCore.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _KrTube;
        float4 _KrFade;
        float _NormalStrength;
        float _Barnacles;
        float _Scars;
        float _Clouds;
        float _SocketGlow;
        float _RimScatter;
        float _Slime;
        float _Sheets;
        float _DamClip;
        float _Cull;
    CBUFFER_END

    TEXTURE2D(_AlbedoMap); SAMPLER(sampler_AlbedoMap);
    TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
    TEXTURE2D(_MaskMap); SAMPLER(sampler_MaskMap);
    TEXTURE2D(_TentAlbedo); SAMPLER(sampler_TentAlbedo);
    TEXTURE2D(_TentNormal); SAMPLER(sampler_TentNormal);
    TEXTURE2D(_TentMask); SAMPLER(sampler_TentMask);

    #include "Include/BwCreatureSkin.hlsl"

    #define BW_HAS_VERTEX_HOOK
    #define BW_HAS_CLIP_HOOK

    void BwVertexHook(inout BwVertexData v)
    {
        float3 p = v.positionOS;
        float3x3 w2o = (float3x3)GetWorldToObjectMatrix();
        float3 nOS = normalize(mul(w2o, v.normalWS));
        float3 tOS = mul(w2o, v.tangentWS.xyz);
        tOS = dot(tOS, tOS) > 1e-12 ? normalize(tOS) : float3(1, 0, 0); // pal:ok (fallback axis)
        float w = saturate(v.color.y);
        float3 q = p;
        if (v.color.x < 200.0 && w > 1e-4)
        {
            uint limb = KrLimbIndex(v.color.x);
            // relief tube (LOD0, _KrFade.z = LOD index) fading out: by mid-band its relief folds back to the smooth radius
            // (vertex colour w, KrakenTentacleMesh) and its normals to the smooth radial normal, so the dithered mix with the
            // smooth LOD1 has one silhouette and one shading (kraken-show33 show_k4_s788: a dotted fringe off the K4 arch)
            UNITY_BRANCH
            if (_KrTube > 0.5 && _KrTube < 1.5 && _KrFade.z < 0.5 && _KrFade.y < 0.5 && _KrFade.x < 0.999)
            {
                float3 cs = KrRestCentre(limb, v.uv.z);
                float3 radial = p - cs;
                float rl = length(radial);
                if (rl > 1e-3)
                {
                    float wRel = smoothstep(0.55, 1.0, _KrFade.x);
                    nOS = normalize(lerp(radial / rl, nOS, wRel));
                    p = cs + radial * lerp(max(v.color.w, 0.05) / rl, 1.0, wRel);
                }
            }
            float3 n2 = nOS, t2 = tOS;
            float3 sk = KrSkin(limb, v.uv.z, p, n2, t2);
            q = lerp(p, sk, w);
            nOS = normalize(lerp(nOS, n2, w));
            t2 = lerp(tOS, t2, w);
            tOS = dot(t2, t2) > 1e-12 ? normalize(t2) : tOS;
        }
        q = KrMantle(q, 1.0 - w);
        v.custom = float4(p, w); // pal:ok (rest position, not a colour)
        v.positionWS = TransformObjectToWorld(q);
        v.normalWS = TransformObjectToWorldNormal(nOS);
        v.tangentWS = float4(TransformObjectToWorldDir(tOS), v.tangentWS.w); // pal:ok (not a colour)
    }

    void BwClipHook(BwFragData f)
    {
        clip(KrDamClipKeep(f.positionWS, _DamClip));
        if (_KrFade.x < 0.999 || _KrFade.y > 0.5)
        {
            float n = BwBlueNoiseAt(f.positionCS.xy);
            float keep = _KrFade.y > 0.5 ? (n - _KrFade.x + 1e-4) : (_KrFade.x - n);
            clip(keep);
        }
    }

    // tangent-space normal -> world through the mesh tangent frame
    float3 KrTangentToWorld(BwFragData f, float3 nTS)
    {
        float3 tWS = f.tangentWS.xyz - f.normalWS * dot(f.tangentWS.xyz, f.normalWS);
        if (dot(tWS, tWS) < 1e-10) return f.normalWS;
        tWS = normalize(tWS);
        float3 bWS = cross(f.normalWS, tWS) * (f.tangentWS.w < 0.0 ? -1.0 : 1.0);
        return normalize(nTS.x * tWS + nTS.y * bWS + nTS.z * f.normalWS);
    }

    // tangent-space normal -> world through a cotangent frame of an arbitrary uv (screen derivatives; Schueler 2013)
    float3 KrCotangentToWorld(BwFragData f, float2 uv, float3 nTS)
    {
        float3 dp1 = ddx(f.positionWS), dp2 = ddy(f.positionWS);
        float2 du1 = ddx(uv), du2 = ddy(uv);
        float3 N = f.normalWS;
        float3 dp2perp = cross(dp2, N), dp1perp = cross(N, dp1);
        float3 T = dp2perp * du1.x + dp1perp * du2.x;
        float3 B = dp2perp * du1.y + dp1perp * du2.y;
        float invmax = rsqrt(max(max(dot(T, T), dot(B, B)), 1e-20));
        return normalize(nTS.x * T * invmax + nTS.y * B * invmax + nTS.z * N);
    }

    // the same cotangent frame from precomputed derivatives (usable inside a divergent branch)
    float3 KrCotangentToWorldD(float3 N, float3 dp1, float3 dp2, float2 du1, float2 du2, float3 nTS)
    {
        float3 dp2perp = cross(dp2, N), dp1perp = cross(N, dp1);
        float3 T = dp2perp * du1.x + dp1perp * du2.x;
        float3 B = dp2perp * du1.y + dp1perp * du2.y;
        float invmax = rsqrt(max(max(dot(T, T), dot(B, B)), 1e-20));
        return normalize(nTS.x * T * invmax + nTS.y * B * invmax + nTS.z * N);
    }

    // Both normal maps are BC5 on Standalone (R x, G y, B 0, A 1; Tools/kraken texture_meta). x = r a also reads DXT5nm
    // (A x, R 1) and plain RGB(A 1); z is rebuilt. Wave X fix: the old unpack took B as z (-1 on BC5, clamped to 0.05), which
    // tilted every normal ~20x: the hide read as rippled glass at every distance (show_k5_ks11, eyeair_4k_crust_crop).
    float3 KrUnpack(float4 tex, float strength)
    {
        float2 xy = float2(tex.r * tex.a, tex.g) * 2.0 - 1.0;
        float z = sqrt(saturate(1.0 - dot(xy, xy)));
        return normalize(float3(xy * strength, max(z, 1e-3))); // pal:ok (not a colour)
    }

    // bump from a scalar height through screen derivatives (surface gradient), for the procedural crust and scars
    // (fine derivatives: the coarse ones are constant per 2 x 2 quad and drew the bumps as pixel blocks at 600 m, kraken-19)
    float3 KrBump(BwFragData f, float3 n, float h, float scale)
    {
        float3 dpx = ddx_fine(f.positionWS), dpy = ddy_fine(f.positionWS);
        float dhx = ddx_fine(h) * scale, dhy = ddy_fine(h) * scale;
        float3 r1 = cross(dpy, n), r2 = cross(n, dpx);
        float det = dot(dpx, r1);
        float3 g = (sign(det) * (dhx * r1 + dhy * r2)) / max(abs(det), 1e-12);
        // towards the silhouette the projected area (det) goes to 0 and the screen-space gradient explodes: fade the bump out
        // at grazing angles (kraken-22 kr_islet: sparkling pixels along the sunlit rim of an arm)
        float nv = saturate(dot(f.normalGeoWS, f.viewDirWS));
        return normalize(n - g * smoothstep(0.04, 0.28, nv));
    }

    void BwSurfaceHook(inout BwFragData f, inout SurfaceData s)
    {
        if (_KrTube > 2.5)
        {   // kelp streamers (A1 1.1 rule 2): grey-green fronds, darker and rotting towards the tips, wet
            float along = saturate(f.uv.z / max(_KrLimb[KrLimbIndex(f.color.x)].x, 1.0));
            float grain = KrFbm(f.custom.xyz / 1.7 + f.color.x * 3.1, f.footprint / 1.7, 3);
            s.albedo = (half3)(PAL_KrakenMoss * (0.55 + 0.6 * grain) * (1.0 - 0.45 * along));
            s.smoothness = (half)(0.62 - 0.2 * along);
            s.metallic = 0.0h;
            s.occlusion = 1.0h;
            s.emission = half3(0, 0, 0); // pal:ok
            return;
        }
        if (_KrTube > 1.5)
        {   // the beak: black chitin, worn paler towards the hooked tips, glossy
            float tip = f.uv.y;
            float grain = KrValueNoise3(f.custom.xyz / 0.9);
            s.albedo = (half3)lerp((PAL_KrakenSkinDark * 0.15) * (0.8 + 0.4 * grain), (PAL_KrakenSucker * 0.45) * 0.35, tip * tip * tip);
            s.smoothness = (half)(0.78 - 0.2 * tip);
            s.metallic = 0.0h;
            s.occlusion = (half)lerp(0.6, 1.0, tip);
            s.emission = half3(0, 0, 0); // pal:ok
            return;
        }
        float u = f.uv.z, th = f.uv.w;
        bool isLimb = f.color.x < 200.0;
        uint limb = KrLimbIndex(f.color.x);
        float4 L2 = isLimb ? _KrLimb2[limb] : float4(0, 1, 100, 1); // pal:ok (not a colour)
        float3 rest = f.custom.xyz;
        float fp = f.footprint;
        float2 tuv = float2(th * (0.5 / PI) + 0.5, -KrTileV(u, L2));

        float3 albedo; float3 nWS; float ao, smooth, cav, glow, sucker;
        // the baked relief x2.2 near, x1.3 from ~0.6 m per pixel (600 m at 1080p): mip-level detail at that strength sparkled
        float nStr = _NormalStrength * lerp(1.0, 0.6, smoothstep(0.15, 0.6, fp));
        if (_KrTube > 0.5)
        {
            // the unwrapped Tripo tile carries a regular scale grid on the back that reads as tiling at 80 m (look round
            // 7-11): on the back half (|theta| > 1.2 rad) its albedo is pulled towards a 3-mip blur and its normals flattened
            float backT = smoothstep(1.2, 2.3, abs(th)) * 0.55;
            albedo = SAMPLE_TEXTURE2D(_TentAlbedo, sampler_TentAlbedo, tuv).rgb;
            albedo = lerp(albedo, SAMPLE_TEXTURE2D_BIAS(_TentAlbedo, sampler_TentAlbedo, tuv, 3.0).rgb, backT);
            float4 m = SAMPLE_TEXTURE2D(_TentMask, sampler_TentMask, tuv);
            nWS = KrTangentToWorld(f, KrUnpack(SAMPLE_TEXTURE2D(_TentNormal, sampler_TentNormal, tuv), nStr * (1.0 - backT)));
            ao = m.r; smooth = m.g; cav = saturate(0.5 + (m.b - 0.5) * 0.6); glow = 0.0; sucker = m.a;
        }
        else
        {
            float2 uv = f.uv.xy;
            albedo = SAMPLE_TEXTURE2D(_AlbedoMap, sampler_AlbedoMap, uv).rgb;
            float4 m = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, uv);
            nWS = KrTangentToWorld(f, KrUnpack(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv), nStr));
            ao = m.r; smooth = m.g; cav = m.b; glow = m.a; sucker = 0.0;
            // arm -> tentacle tile over the last 40 m before the cut (the joint carries the tentacle's skin), weighted by b.
            // The derivatives are taken in uniform flow, the three tile samples only where b > 0 (explicit gradients): the
            // head and most of the arms skip them (Wave X: the full-frame head close-up cost 2.2 ms at 4K)
            // the back and flanks blend over 84 m (over 40 m the Tripo arm's baked moss stopped visibly at the joint, look
            // round 11-15); the sucker side keeps 40 m (the tile's sucker rings must not land on the sculpted suckers, round 17)
            float bBack = smoothstep(0.9, 1.6, abs(th));
            float b = (isLimb && limb < 8u) ? lerp(smoothstep(L2.x - 46.0, L2.x - 6.0, u), smoothstep(L2.x - 90.0, L2.x - 6.0, u), bBack) : 0.0;
            float2 dtx = ddx(tuv), dty = ddy(tuv);
            float3 dp1 = ddx(f.positionWS), dp2 = ddy(f.positionWS);
            UNITY_BRANCH
            if (b > 0.0005)
            {
                float3 ta = SAMPLE_TEXTURE2D_GRAD(_TentAlbedo, sampler_TentAlbedo, tuv, dtx, dty).rgb;
                float4 tm = SAMPLE_TEXTURE2D_GRAD(_TentMask, sampler_TentMask, tuv, dtx, dty);
                float3 tnTS = KrUnpack(SAMPLE_TEXTURE2D_GRAD(_TentNormal, sampler_TentNormal, tuv, dtx, dty), nStr);
                float3 tn = KrCotangentToWorldD(f.normalWS, dp1, dp2, dtx, dty, tnTS);
                albedo = lerp(albedo, ta, b);
                nWS = normalize(lerp(nWS, tn, b));
                ao = lerp(ao, tm.r, b); smooth = lerp(smooth, tm.g, b); cav = lerp(cav, 0.5, b); sucker = tm.a * b;
            }
        }
        // the mantle is a dark, mossy dome (A1 1.1 ref. 1/2): the Tripo atlas paints red sucker strips over the rigid head as
        // well, which read as coral at 600 m (look round 4); above the crown they turn into dark skin and moss
        if (!isLimb && _KrTube < 0.5)
        {
            float redness = saturate((albedo.r - max(albedo.g, albedo.b)) * 5.0);
            float upper = smoothstep(18.0, 45.0, rest.y);
            float lum = dot(albedo, float3(0.30, 0.59, 0.11)); // pal:ok (luma weights)
            float3 hide = lerp(PAL_KrakenSkinDark, PAL_KrakenMoss, 0.35) * (0.6 + 1.2 * saturate(lum * 3.0));
            albedo = lerp(albedo, hide, redness * upper * 0.85);
        }
        float3 n = nWS;
        // wet hide, not chrome (look round 2: smoothness 0.65-0.93 over the emerged head mirrored the bright sky at every
        // grazing facet and the dark skin read as silver): the mask's 0.55-0.72 maps to 0.47-0.60 before slime and sheets
        smooth = 0.10 + 0.70 * smooth;
        // the head: crusted, less mirror on the dome; and a rock-like lumpy relief (5-20 m, rest space, footprint-filtered) so
        // the wet film breaks into many small highlights instead of one continuous glassy sheen (show_k5_ks11 at noon). The
        // bump is taken in uniform flow (derivatives) and blended in by the head mask.
        float headMask = (_KrTube < 0.5) ? saturate((f.color.x - 200.0) / 40.0) : 0.0;
        smooth *= lerp(1.0, 0.74, smoothstep(20.0, 80.0, rest.y) * headMask);
        UNITY_BRANCH
        if (_KrTube < 0.5)                                       // per material (body): the tubes skip it
        {
            float lump = KrFbmGrad(rest / 11.0 + 5.3, fp / 11.0, 1);
            n = normalize(lerp(n, KrBump(f, n, lump, 12.0), headMask * smoothstep(-10.0, 25.0, rest.y)));
        }

        // ---- limbs: the albedo follows the arm's own frame (fix wave X2, S2-2 "a red rubber hose": the Tripo tile is red all
        // round, so the K4 arch read saturated red on its dorsal side too). theta = 0 is the sucker side; the dorsal 60 %
        // (|theta| > 0.4 pi) turns to the dark mossy hide (KrakenSkinDark / KrakenMoss, the tile's luminance kept as detail),
        // the blood-red stays on the sucker side, and the tile's sucker rings fade out on the back
        if (isLimb)
        {
            float dorsal = smoothstep(0.55, 1.1, abs(th));   // (1.0, 1.6) left the K4 arch flanks red (kraken-fx2 show_k4_kk2)
            float lumL = dot(albedo, float3(0.30, 0.59, 0.11)); // pal:ok (luma weights)
            float mossN = KrValueNoise3(rest / 23.0 + 2.7);
            float3 hideL = lerp(PAL_KrakenSkinDark, PAL_KrakenMoss, 0.2 + 0.4 * mossN) * (0.55 + 1.3 * saturate(lumL * 3.0));
            albedo = lerp(albedo, hideL, dorsal * 0.9);
            sucker *= 1.0 - dorsal;
        }
        // ---- back mask (rest space): limbs by the angle round the limb (the back is |theta| ~ pi), the head above 25 m
        // (limbs: the back and upper flanks, |theta| > ~1.3 rad, so moss smears break the tile's regular scale grid there)
        float back = isLimb ? smoothstep(1.2, 2.3, abs(th)) : smoothstep(20.0, 60.0, rest.y);
        float underside = isLimb ? 1.0 - smoothstep(0.9, 1.6, abs(th)) : 1.0 - smoothstep(4.0, 30.0, rest.y);

        // ---- barnacle crust: Worley shells (3.5 m cells) in colonies, faded below ~6 px per shell
        float colony = smoothstep(0.56, 0.74, KrFbm(rest / 40.0, fp / 40.0, 3)) * back * _Barnacles * (1.0 - glow);
        float shellFade = 1.0 - smoothstep(0.35, 0.65, fp);      // 3.5 m shells: gone below ~6-10 px
        float crustH = 0.0, crustCover = 0.0;
        if (colony > 0.001)
        {
            float2 wv = KrWorley(rest / 3.5);
            float shell = smoothstep(0.46, 0.22, wv.x);
            float hole = smoothstep(0.13, 0.07, wv.x);
            crustH = (shell * (0.6 + 0.4 * wv.y) - hole * 0.8) * colony * shellFade;
            // dull, algae-stained shells (linear 0.05-0.10): a crust on the dark hide, never a pale stone (look round 1: the
            // 0.14-0.26 shells over 45 % of the back turned the head into a grey boulder at noon)
            float cellTint = lerp(0.5, wv.y, shell);                // the cell hash only inside a shell (KrWorley is exact there)
            float3 shellCol = lerp(PAL_KrakenSucker, PAL_KrakenMoss, 0.35) * (0.075 + 0.07 * cellTint);
            float cover = colony * lerp(0.25, shell, shellFade);
            crustCover = cover;
            albedo = lerp(albedo, shellCol * (1.0 - 0.85 * hole * shellFade), cover);
            smooth = lerp(smooth, 0.34, cover);
            ao *= 1.0 - 0.5 * hole * colony * shellFade;
        }
        n = KrBump(f, n, crustH, 1.6);                          // outside the branch: derivatives in uniform flow
        // ---- warts and wrinkles (Wave X: after the BC5 fix the baked relief alone read as smooth rubber at 60-120 m): a
        // 3 m rest-space creased (wrinkle) gradient-noise bump on the hide, not on the sucker cups; faded with the footprint
        {
            float wart = KrFbmCrease(rest / 3.0 + 17.0, fp / 1.5, 2);     // each octave gone below ~8 px per cell (no sparkle); 2 octaves (4K cost)
            n = normalize(lerp(n, KrBump(f, n, wart, 1.3), 1.0 - saturate(sucker * 1.5)));
        }
        // ---- algae / moss smears on the back between colonies (meso tone)
        float moss = smoothstep(isLimb ? 0.40 : 0.45, 0.62, KrFbm(rest / 18.0 + 3.1, fp / 18.0, 3)) * back * (1.0 - colony);
        albedo = lerp(albedo, PAL_KrakenMoss * (0.7 + 0.6 * KrValueNoise3(rest / 2.3)), moss * 0.55);
        smooth = lerp(smooth, 0.32, moss * 0.6);

        // ---- scars: long pale weals (rest space, warped bands), width ~0.9 m, only some stretches
        {
            float a1 = dot(rest, float3(0.53, 0.21, 0.82)) / 26.0 + 0.45 * sin(dot(rest, float3(-0.7, 0.3, 0.64)) / 37.0); // pal:ok (direction)
            float d = abs(frac(a1) - 0.5) * 26.0;
            float wdt = 0.45 + 0.35 * KrValueNoise3(rest / 9.0);
            float on = smoothstep(0.55, 0.75, KrValueNoise3(rest / 60.0 + 7.7)) * _Scars;
            float px = wdt / max(fp, 1e-4);
            float scar = (1.0 - smoothstep(wdt * 0.5, wdt, d)) * on * smoothstep(0.8, 2.0, px);
            albedo = lerp(albedo, (PAL_KrakenSucker * 0.28) * (0.6 + 0.4 * KrValueNoise3(rest / 1.3)), scar * 0.75);
            smooth = lerp(smooth, 0.35, scar);
            n = KrBump(f, n, scar * 0.6, 1.0);
        }

        // ---- slime and wet film
        float slime = KrFbm(rest / 7.0 + 11.0, fp / 7.0, 3);
        smooth = saturate(smooth + _Slime * (slime - 0.35) * 1.6);

        // ---- water sheets running off emerged skin (world height above the sea, flow along world -y)
        float wet = isLimb ? L2.w : _KrMantle.z;
        float above = f.positionWS.y - TC_SeaY;
        if (above > 0.0 && wet > 0.001)
        {
            float streak = KrFbm(float3(rest.x / 5.0, (f.positionWS.y + _KrPhase.y) / 14.0, rest.z / 5.0), fp / 5.0, 2); // pal:ok (coordinates)
            float sheet = smoothstep(0.55, 0.80, streak) * wet * _Sheets * saturate(above / 3.0);
            albedo *= 1.0 - 0.30 * sheet;
            smooth = lerp(smooth, 0.72, sheet);                 // a thin film on rough hide, not a glaze (was 0.84: glassy at noon)
            // fix wave X2 (S2-2: "no wet sheen", KS11 head p99.9 luma 160.8): emerged wet skin keeps a wet lobe (A1: roughness
            // 0.12 on skin, 0.30 on crust) in a film over the emerged part and in the running sheets; wetness falls after
            // emergence (the director's HeadWetness / limb wetness ramps); the footprint widening below keeps 600 m matte
            float film = wet * saturate(above / 3.0) * (0.45 + 0.55 * smoothstep(0.35, 0.70, streak));
            smooth = lerp(smooth, lerp(0.84, 0.70, crustCover), film * 0.75);
            smooth = lerp(smooth, 0.88, sheet * (1.0 - crustCover));
        }

        // ---- darker crevices (8.11.4 cavity dirt) and the vertex AO of the body
        float crev = saturate((0.5 - cav) * 2.0);
        albedo *= 1.0 - 0.45 * crev;
        ao *= lerp(1.0, saturate(f.color.z), 0.7) * (1.0 - 0.35 * crev);

        // ---- "passing cloud" (A1 1.3): dark chromatophore bands sweep from the mantle over the arms at 11.7 m/s, 0.07 Hz,
        // +-15 % albedo (wavelength 167 m; the band coordinate is the rest distance from the mantle tip, then out along u)
        float dist = isLimb ? (230.0 + u) : max(0.0, 230.0 + rest.z * 0.9);
        float band = 0.5 + 0.5 * sin(6.2831853 * (dist / 166.7 - _KrPhase.x));
        albedo *= 1.0 - _Clouds * 0.30 * (band - 0.5);
        // ---- sucker wave (A1 1.3): rims open and close in a wave travelling along the arm (cups darken, rims blanch)
        float sw = 0.5 + 0.5 * sin(6.2831853 * (u / 38.0 - _KrPhase.z));
        albedo = lerp(albedo, albedo * (0.75 + 0.5 * sw), sucker);
        smooth = lerp(smooth, 0.32, sucker * 0.9);              // wet flesh, not glass rings (look rounds 7-11)
        n = normalize(lerp(n, f.normalWS, saturate(sucker) * 0.45));   // softer rims: fewer grazing sky glints
        // the Tripo tile paints white specular glints into its suckers (a second, fixed highlight on top of the lit one read
        // as glass rings at 80 m): clamp the cups and rims to the bone-pink token
        albedo = lerp(albedo, min(albedo, PAL_KrakenSucker * 0.62), saturate(sucker * 1.5));
        // ---- hooks (A1 1.1 rule 4, the mask form outside beat 6): pale chitin teeth on both edges of every sucker annulus
        // (tile mask A 0.18-0.75), ~100 round the limb, each ~1/10 of a sucker; faded below ~5 px per hook (no sparkle)
        {
            float rLimb = isLimb ? (u < L2.x ? L2.y : max(0.3, L2.y * pow(max(1.0 - (u - L2.x) / max(L2.z, 1.0), 0.0), 1.43))) : 10.0;
            float hookPx = (6.2831853 * rLimb / 100.0) / max(fp, 1e-4);
            float band = smoothstep(0.18, 0.32, sucker) * (1.0 - smoothstep(0.55, 0.75, sucker));
            float hn = BwValueNoise(tuv * float2(100.0, 154.0)); // pal:ok (tile frequencies, not a colour)
            // shown from ~5 px per hook (at 2-4 px the pale teeth were isolated sunlit pixels on the arm rims: kraken-22..24
            // kr_islet NaN-like 4-5 px, a twinkle in motion)
            float hook = smoothstep(0.70, 0.84, hn) * band * smoothstep(4.5, 8.0, hookPx);
            albedo = lerp(albedo, PAL_KrakenSucker * 0.62, hook);
            smooth = lerp(smooth, 0.78, hook);
            n = KrBump(f, n, hook * 0.5, 0.6);
        }
        // ---- emission: only the eyes glow (A1 1.2); their light on the inner socket walls (mask A x eye gain)
        float3 emission = PAL_KrakenEyeGlow * (glow * glow * glow * _SocketGlow * _KrMantle.w);

        // micro-shadowing of the crusted hide at grazing angles: the outline of the dome and the arms keeps a dull rim instead
        // of the bright Fresnel sky ring that made the head read as a glass ball (grazing term smoothness + 0.04 in URP)
        smooth *= lerp(0.70, 1.0, smoothstep(0.05, 0.45, saturate(dot(n, f.viewDirWS))));
        // the relief the footprint filters out of the normal (warts, shells, the faded baked detail) goes into roughness:
        // x0.78 from ~0.7 m per pixel (one isolated sun glint at 600 m in kraken-21 kr_head600, image_metrics NaN-like)
        smooth *= lerp(1.0, 0.78, smoothstep(0.12, 0.7, fp));
        // inside a LOD cross-fade band (both LODs know the coverage x) the dithered mix of two meshes must not show two sets of
        // sharp sun glints: up to x0.8 smoothness at mid-band (kraken-31 kr_islet: 2 isolated glints on a LOD0/1 arm)
        smooth *= 1.0 - 0.8 * _KrFade.x * (1.0 - _KrFade.x) * step(_KrFade.x, 0.999);

        // keep the shading normal facing the camera (tangent-space detail at grazing angles must not turn away from the view)
        float nvS = dot(n, f.viewDirWS);
        n = nvS < 0.06 ? normalize(n + f.viewDirWS * (0.06 - nvS)) : n;

        // specular AA of the procedural and baked relief (Kaplanyan 2016 on the final normal, fine derivatives, kernel
        // roughness^2 <= 0.36): where the normal changes within a pixel the lobe widens, so the sunlit rims and crests no longer
        // sparkle (kraken-22 kr_islet); BwShadeEx's own filter (threshold 0.2) stays on top
        {
            float3 dndx = ddx_fine(n), dndy = ddy_fine(n);
            float kr2 = min(0.5 * (dot(dndx, dndx) + dot(dndy, dndy)), 0.36);
            float pr = 1.0 - saturate(smooth);
            smooth = 1.0 - sqrt(sqrt(saturate(pr * pr * pr * pr + kr2)));
        }

        f.normalWS = n;
        s.albedo = (half3)clamp(albedo, 0.004, 0.75);
        s.metallic = 0.0h;
        s.smoothness = (half)clamp(smooth, 0.2, 0.86);
        s.occlusion = (half)saturate(ao);
        s.emission = (half3)emission;
        s.normalTS = half3(0, 0, 1);
        f.custom.w = sucker;
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
            #pragma fragment KrFrag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #define BW_PASS_FORWARD
            #include "Include/BwLitPass.hlsl"
            #include "Include/BwCreatureForward.hlsl"
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
            #define BW_PASS_DEPTHNORMALS
            #include "Include/BwLitPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
