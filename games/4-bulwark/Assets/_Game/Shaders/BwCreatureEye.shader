// BwCreatureEye - the kraken's eyeballs (Addendum A1 1.1-1.2, kraken owner): per side a main eye (20 m eyeball, iris 9.6 m
// = 2 sin(0.50) R: the carved socket shows only +-0.55 R vertically, so A1's 12 m iris read as a cut band ("lips", look
// round 7-8); slit 6.6 x 1.5 m, round 6 m in the dark) and three lesser eyes (8, 6, 4 m; the 6 m one milky, never glowing), procedural
// spheres sunk in the sockets carved into the Tripo head, turned by KrakenCreature to track the camera.
// Unit sphere, +Z = the pupil axis (uv0 = local x, y; uv1.x = local z). The transform keeps local X level in the world, so
// the horizontal slit stays level whatever the body does (the statocyst: a real, unsettling detail).
// Iris: KrakenEyeGlow (linear 1.0, 0.12, 0.02) x E (4 in air, 6 under water before the water's attenuation), radial fibres
// faded below ~3 px per fibre, a hotter ring at the pupil, black pupil (_Pupil 0 slit .. 1 round); sclera blood-black with
// dark veins; glossy wet cornea (smoothness 0.95 with the template's specular AA); glow +-10 % at 0.2 Hz (_KrMantle.w).
// Lids: in the SOCKET frame (_EyeC centre, _SockN, _SockU, set per frame), an elliptical aperture 16 x 11 m (half-height
// 0.55 R) closing with _Blink: skin-dark folded lids slide over the turning eyeball. Forward fragment shared with
// BwCreature (underwater model, BwFogApply).
Shader "Megastructure/BwCreatureEye"
{
    Properties
    {
        _IrisRadius ("Iris half-angle (rad)", Float) = 0.50
        _SlitWidth ("Slit half-height", Float) = 0.075
        _SlitLength ("Slit half-length", Float) = 0.33
        _RoundRadius ("Round pupil radius", Float) = 0.30
        _Glow ("Iris glow E in air (x palette; fix wave X2: 2.0, a cartoon disc at noon with 5.5)", Range(0, 8)) = 2.0
        _GlowWater ("Iris glow E under water (fix wave X2: 3.5, so the soft emission knee keeps the iris gradation)", Range(0, 8)) = 3.5
        _Milky ("Milky (never glows)", Float) = 0
        _Pupil ("Pupil 0 slit .. 1 round", Range(0, 1)) = 0
        _Blink ("Lids 0 open .. 1 closed", Range(0, 1)) = 0
        _EyeC ("Eye centre (world) and radius", Vector) = (0, 0, 0, 1)
        _SockN ("Socket normal (world)", Vector) = (0, 0, 1, 0)
        _SockU ("Socket up (world)", Vector) = (0, 1, 0, 0)
        _DamClip ("Dam clip for lee-side cameras (reads the global _BwDamClip)", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "Include/BwLitCore.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _IrisRadius;
        float _SlitWidth;
        float _SlitLength;
        float _RoundRadius;
        float _Glow;
        float _GlowWater;
        float _Milky;
        float _Pupil;
        float _Blink;
        float4 _EyeC;
        float4 _SockN;
        float4 _SockU;
        float _RimScatter;
        float _DamClip;
        float _Cull;
    CBUFFER_END

    #include "Include/BwCreatureSkin.hlsl"
    // beat 6 window lamps (Beat06WindowGallery globals, read only; BwWater.hlsl declares them for the water shaders, which
    // also include BwCreatureSkin.hlsl, so they live here): xyz position (world), w gain 0..1; colour x radiance, w range (m)
    float4 _BwWLampA0, _BwWLampB0, _BwWLampCol;

    #define BW_HAS_CLIP_HOOK
    void BwClipHook(BwFragData f) { clip(KrDamClipKeep(f.positionWS, _DamClip)); }

    // value noise on a circle round the pupil axis (periodic in phi: no seam at +-pi), r along the radius
    float KrIrisNoise(float phi, float r, float around, float along, float seed)
    {
        return BwValueNoise3(float3(cos(phi) * around, sin(phi) * around, r * along + seed)); // pal:ok (coordinates)
    }

    // GGX lobe shape normalised to 1 at its peak (a = roughness^2): a^4 / ((NoH^2 (a^2 - 1) + 1)^2)
    float KrLobe(float NoH, float a)
    {
        float a2 = a * a;
        float dn = NoH * NoH * (a2 - 1.0) + 1.0;
        return a2 * a2 / max(dn * dn, 1e-8);
    }

    // one window lamp's catchlight on the cornea (gain w, world position xyz); 0 when the lamp is off
    float KrLampCatch(float4 lamp, float3 pos, float3 n, float3 v)
    {
        float3 L = lamp.xyz - pos;
        float d2 = dot(L, L);
        if (lamp.w < 1e-3 || d2 < 1e-4) return 0.0;
        float3 h = normalize(v + L * rsqrt(d2));
        float nh = saturate(dot(n, h));
        return lamp.w * (KrLobe(nh, 0.07) + 0.02 * KrLobe(nh, 0.28));
    }

    void BwSurfaceHook(inout BwFragData f, inout SurfaceData s)
    {
        float3 p = float3(f.uv.x, f.uv.y, f.uv.z); // pal:ok (local position, not a colour)
        p *= rsqrt(max(dot(p, p), 1e-8));
        float a = acos(clamp(p.z, -1.0, 1.0));                 // angle from the pupil axis
        float phi = atan2(p.y, p.x);
        float fpAng = max(f.footprint / max(length(GetObjectToWorldMatrix()._m00_m10_m20), 1e-3), 1e-4);   // footprint in radians
        float ir = _IrisRadius;
        float rr = a / ir;                                     // 0 at the centre, 1 at the iris edge
        float iris = 1.0 - smoothstep(ir - 0.04, ir + 0.03, a);
        float uw = saturate(_KrUnderwater.x);
        // ---- iris structure (periodic round the axis, faded by the pixel footprint: no shimmer)
        float fibVis = 1.0 - smoothstep(0.004, 0.010, fpAng);  // ~150 irregular fibres: gone below ~3 px each
        float blotVis = 1.0 - smoothstep(0.012, 0.030, fpAng); // ~55 crypts and flecks
        float fib = KrIrisNoise(phi, rr, 24.0, 2.5, 0.0);
        float fib2 = KrIrisNoise(phi + 0.37, rr, 41.0, 4.0, 5.3);
        float blot = KrIrisNoise(phi, rr, 9.0, 6.0, 11.7);
        float fleck = smoothstep(0.76, 0.90, KrIrisNoise(phi, rr, 26.0, 14.0, 23.1)) * blotVis;
        float fibres = lerp(0.8, 0.35 + 0.65 * fib * (0.6 + 0.8 * fib2), fibVis);
        float crypts = lerp(0.85, 0.55 + 0.7 * blot, blotVis);
        float furrow = 1.0 - 0.25 * blotVis * smoothstep(0.6, 0.9, 0.5 + 0.5 * sin(rr * 30.0 + 4.0 * blot));
        // radial profile: a hot collarette round the pupil, embers outward, then (fix wave X2, KS17 "no limbal ring") a distinct
        // dark limbal ring: the outer 10 % of the iris at 0.3x
        float hot = exp(-pow((rr - 0.24) / 0.15, 2.0));
        float limbal = smoothstep(0.84, 0.93, rr);
        float prof = (0.55 + 0.9 * hot) * (1.0 - 0.35 * smoothstep(0.55, 0.85, rr)) * (1.0 - 0.7 * limbal);
        // pupil (fix wave X2, KS17 "glowing lips": the old bar with rounded ends was 2 x 0.53 wide on a 2 x 0.48 iris, so it cut
        // the iris into an upper and a lower lip): a lens with tapered ends, half-length 0.60 sin(iris) (A1 1.2: an 8 m slit on a
        // 12 m iris; 0.67 measured 0.79 of the lit iris in kraken-fx2 KS17: the dark limbal ring is not counted), the iris showing beyond both ends; half-height h(x) = hy (1 - (x / hx)^2)^k, k 1 (lens) .. 0.5 (round in
        // the dark: an ellipse, a circle when hx = hy). Eye-local x is level in the world. Anti-aliased by the footprint.
        float hx = lerp(0.60 * sin(ir), _RoundRadius, _Pupil), hy = lerp(_SlitWidth, _RoundRadius, _Pupil);
        float xs = saturate(abs(p.x) / max(hx, 1e-3));
        float hgt = hy * pow(max(1.0 - xs * xs, 0.0), lerp(1.0, 0.5, _Pupil));
        float slitD = max(abs(p.y) - hgt, abs(p.x) - hx);
        float pupEdge = max(fpAng * 0.75, 0.004);
        float pupil = (1.0 - smoothstep(-pupEdge, pupEdge, slitD)) * step(0.0, p.z);
        // a soft heat round the pupil, fading out within 0.05 R (look round 7: a thin ring hugged the bar like lips)
        float rim = 0.25 * exp(-pow(max(slitD, 0.0) / 0.05, 2.0)) * (1.0 - pupil) * iris;
        float gain = _KrMantle.w * lerp(_Glow, _GlowWater, uw) * (1.0 - _Milky);
        float body = fibres * crypts * furrow * prof * (1.0 - 0.45 * fleck) + rim;
        float3 glow = PAL_KrakenEyeGlow * (gain * body) * iris * (1.0 - pupil);
        // A1 1.2 / critic S3-9: far under water the glow reaches the eye as what survives the water (red removed first): a green point
        float rW = max(length(f.positionWS - _WorldSpaceCameraPos) - _KrUnderwater.w, 0.0);
        float3 cW = float3(TC_BeamCR, TC_BeamCG, TC_BeamCB); // pal:ok (beam attenuation, not a colour)
        float3 tint = PAL_KrakenEyeGlow * exp(-cW * rW);
        tint *= dot(PAL_KrakenEyeGlow, float3(0.2126, 0.7152, 0.0722)) / max(dot(tint, float3(0.2126, 0.7152, 0.0722)), 1e-5); // pal:ok (luma weights)
        glow = lerp(glow, tint * (gain * body) * iris * (1.0 - pupil), saturate(_KrUnderwater.x) * smoothstep(30.0, 50.0, rW));
        // the pupil is a hole into a dark cavity: near-black, a faint blood-red retinal glow, and (KrFrag) only 1 % of the water
        // veil in front of it (fix wave X2: it showed the veil at full strength and read as an open mouth)
        glow += PAL_KrakenUnderRed * (0.004 * gain) * pupil;   // 0.02 lifted the pupil to R 45 (kraken-fx2)
        // sclera (mostly inside the socket): blood-black with darker veins
        float vein = smoothstep(0.62, 0.70, KrIrisNoise(phi, a, 6.0, 4.0, 13.0)) * blotVis;
        float3 sclera = PAL_KrakenUnderRed * (0.16 + 0.12 * vein);
        float3 albedo = lerp(sclera, PAL_KrakenSkinDark * 0.15, iris);
        albedo = lerp(albedo, PAL_KrakenSkinDark * 0.02, pupil);
        // milky eye: a clouded, blind, pale globe with a faint ghost of the iris
        albedo = lerp(albedo, PAL_KrakenEyeMilky * (0.55 + 0.25 * iris), _Milky);

        // ---- lids (socket frame): a heavy hooded upper lid and a lower lid, both closing with _Blink
        float3 d = f.positionWS - _EyeC.xyz;
        d *= rsqrt(max(dot(d, d), 1e-8));
        float3 sn = normalize(_SockN.xyz), su = normalize(_SockU.xyz - sn * dot(_SockU.xyz, sn));
        float yv = dot(d, su), zv = dot(d, sn);
        // an almond aperture (lid lines curve to the corners, the inner corner lower): parallel straight lids read as lips
        float xv = dot(d, normalize(cross(su, sn)));
        float almond = sqrt(saturate(1.0 - pow(abs(xv + 0.06) / 0.98, 2.2)));
        float openUp = (0.46 * almond - 0.04) * (1.0 - _Blink), openDn = (0.52 * almond - 0.03) * (1.0 - _Blink);
        // fix wave X2 (KS17 "a uniform navy lid ring outlines the eye like a lip line"): the lids are soft rolled folds, not a
        // painted line: a rolled margin (the normal turns over the edge), the upper lid's shadow on the eyeball under it,
        // wrinkles parallel to the edge and barnacle crust farther out (A1 1.1.3), lit by the window lamps (KrFrag)
        float lidPx = max(fpAng * 1.5, 0.012);
        float front = smoothstep(-0.1, 0.2, zv);
        float upT = yv - openUp, dnT = -yv - openDn;            // > 0 on the lid: distance past its edge
        float lid = max(smoothstep(-lidPx, lidPx, upT), smoothstep(-lidPx, lidPx, dnT)) * front;
        float onUp = step(0.0, yv);
        float lidT = onUp > 0.5 ? upT : dnT;                     // distance past the nearer lid's edge
        // the upper lid hoods the eyeball under it (up to 0.75 darker over 0.22 R), the lower lid a little (0.35 over 0.10 R)
        float hood = (1.0 - smoothstep(0.0, 0.22, -upT)) * (1.0 - lid) * front;
        float hoodDn = (1.0 - smoothstep(0.0, 0.10, -dnT)) * (1.0 - lid) * front;
        float shade = saturate(1.0 - 0.75 * hood - 0.35 * hoodDn);
        // lid relief: the rolled margin, wrinkles (2 noise-warped frequencies, faded by the footprint), crust shells past 0.08 R
        float wrVis = 1.0 - smoothstep(0.012, 0.03, fpAng);
        float around = atan2(xv, max(zv, 1e-3));
        float warp = KrIrisNoise(around * 3.0, lidT, 3.0, 9.0, 31.0);
        float wA = lidT * 38.0 + 3.0 * warp, wB = lidT * 83.0 + 5.0 * warp + 1.7;
        float dW = (0.30 * cos(wA) + 0.12 * cos(wB)) * wrVis * smoothstep(0.0, 0.03, lidT);
        float roll = 0.9 * exp(-pow(lidT / 0.05, 2.0)) + 0.35 * exp(-pow((lidT - 0.10) / 0.06, 2.0));
        float3 towardOpen = onUp > 0.5 ? -su : su;
        float2 wv = KrWorley(f.positionWS / max(_EyeC.w * 0.16, 0.05));
        float crust = smoothstep(0.08, 0.20, lidT) * smoothstep(0.55, 0.72, KrIrisNoise(around, lidT, 4.0, 7.0, 41.0)) * blotVis;
        float shell = smoothstep(0.46, 0.22, wv.x) * crust;
        float3 nSph = normalize(f.normalWS);
        float3 nLid = normalize(nSph + towardOpen * (roll + dW) - sn * (0.25 * shell * (wv.x - 0.3)));
        float fold = 0.5 + 0.5 * cos(wA);
        float3 lidCol = lerp(PAL_KrakenSkinDark * (0.75 + 0.45 * fold * wrVis),
                             lerp(PAL_KrakenSkinDark, PAL_KrakenMoss, 0.6) * (0.8 + 0.6 * warp), 0.35 * smoothstep(0.05, 0.2, lidT));
        lidCol = lerp(lidCol, lerp(PAL_KrakenSucker, PAL_KrakenMoss, 0.4) * (0.08 + 0.06 * wv.y), shell);
        lidCol *= 1.0 - 0.5 * exp(-pow(lidT / 0.025, 2.0));    // the margin tucks into shadow where it meets the eyeball
        albedo = lerp(albedo * shade, lidCol, lid);
        // ---- cornea (fix wave X2, KS17 "no cornea, no specular"): a clear dome over the iris, proud of the 20 m eyeball (normal
        // tilted x1.9 off the axis), wet (smoothness 0.95), with the catchlights of the window lamps (beat 6) and of the lit
        // gallery behind the viewer, a Fresnel rim, and a wet meniscus where the lids meet the eyeball
        float cornea = (1.0 - smoothstep(ir - 0.02, ir + 0.10, a)) * step(0.0, p.z) * (1.0 - _Milky);
        float3 nOSc = normalize(float3(p.xy * (1.0 + 0.9 * cornea), p.z)); // pal:ok (normal, not a colour)
        float3 nC = normalize(mul((float3x3)GetObjectToWorldMatrix(), nOSc));
        float3 n = normalize(lerp(nSph, nC, cornea));
        n = normalize(lerp(n, nLid, lid));
        float3 v = f.viewDirWS;
        float clamp3 = _KrPhase.w > 0.01 ? _KrPhase.w : 3.0;   // 3 / 2^EV: the post-exposure clamp 3
        float lamps = KrLampCatch(_BwWLampB0, f.positionWS, n, v) + 0.6 * KrLampCatch(_BwWLampA0, f.positionWS, n, v);
        // the lit gallery behind the viewer (a large window of light up and to the side of the camera)
        float3 side = cross(v, float3(0.0, 1.0, 0.0)); // pal:ok (direction)
        side *= rsqrt(max(dot(side, side), 1e-6));
        float3 Lg = normalize(v + float3(0.0, 0.32, 0.0) + side * 0.20); // pal:ok (direction)
        float3 hg = normalize(v + Lg);
        float gal = KrLobe(saturate(dot(n, hg)), 0.09) + 0.015 * KrLobe(saturate(dot(n, hg)), 0.30);   // broad lobes 0.06-0.08 hazed the pupil grey
        float fres = pow(1.0 - saturate(dot(n, v)), 5.0);
        float menis = (0.3 * exp(-pow((upT + 0.012) / 0.010, 2.0)) + exp(-pow((dnT + 0.010) / 0.010, 2.0))) * blotVis * front
                    * (0.5 + 0.5 * KrIrisNoise(phi, a, 11.0, 3.0, 51.0));
        float catchW = (cornea * (0.55 * lamps + 0.35 * gal) * (1.0 - lid) * shade + 0.06 * fres * cornea * (1.0 - lid) + 0.05 * menis) * uw;
        float lampMax = max(_BwWLampCol.r, max(_BwWLampCol.g, _BwWLampCol.b));
        float3 lampCol = _BwWLampCol.rgb / max(lampMax, 1e-4);
        float3 specCol = lerp(PAL_KrakenEyeMilky, lampCol, saturate(_BwWLampB0.w + _BwWLampA0.w) * step(1e-4, lampMax));
        gKrSpec = specCol * (catchW * clamp3 * 0.55);
        gKrVeilKeep = lerp(1.0, 0.01, pupil * (1.0 - lid));   // 0.08 still read (10,16,39) at KS17 (kraken-fx3): sRGB lifts small linear values
        f.normalWS = n;
        s.albedo = (half3)albedo;
        // emission: the iris glow under the hood and outside the lids; KrFrag clamps its hue and level after the water
        // the iris light on the inner lid margin (warm, fading over 0.12 R): the lid ring no longer outlines the eye in navy
        float3 lidGlow = PAL_KrakenEyeGlow * (0.06 * gain * exp(-max(lidT, 0.0) / 0.12)) * lid * front;
        s.emission = (half3)(glow * (1.0 - lid) * shade + lidGlow);
        s.smoothness = (half)lerp(lerp(lerp(0.95, 0.8, _Milky), 0.62, lid), 0.40, shell * lid);
        s.metallic = 0.0h;
        s.occlusion = (half)lerp(lerp(1.0, 0.5, 1.0 - shade), 0.75, lid);
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
