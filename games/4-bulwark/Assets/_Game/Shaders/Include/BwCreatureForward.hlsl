// BwCreatureForward.hlsl - the kraken's forward fragment (Addendum A1, kraken owner). BwLitPass's BwFrag plus:
//   * emission kept out of the lit sum so the underwater model can treat it on its own;
//   * thin-part scattering: blood-red light through thin tentacles against the sun (radius from the limb profile);
//   * underwater (_KrUnderwater.x > 0; the window-portal builder sets it): downwelling attenuation exp(-Kd depth) of the lit
//     part, then the 14.7 H3 object model L = L0 exp(-c r) + L_veil (1 - exp(-c r)) with c = (0.34, 0.085, 0.050) /m over
//     the water path r (view distance minus the in-air path), the veil of the camera depth (KrVeil); _KrUnderwater.z keeps
//     part of the emission from the extinction (the eyes read a little farther; an art choice, default 0.35);
//   * in air: BwFogApply (the shared fog), as every Bw* shader.
// Fix wave X2: under water the local lamps (beat 6 window lamps) skip the downwelling loss (they are not sunlight from the
// surface: the eye had no catchlight and the socket no lamp light); the optical depth is clamped (KR_TAU_MAX: KS16 flat
// cut-out); the eye hook's pupil keeps 8 % of the veil and its cornea catchlight (gKrSpec) skips the emission clamp; the
// emission is clamped in hue (G <= 0.14 R, B <= 0.08 R) and level (soft knee to 0.53 x the post-exposure clamp 3) after the
// water (the KS17 core turned yellow-green, R/G 0.81); lee-side cameras beyond 3 km see the stinger as a dark silhouette.
// Included after BwLitPass.hlsl in the ForwardLit pass only (Lighting.hlsl is available).
#ifndef BW_CREATURE_FORWARD_INCLUDED
#define BW_CREATURE_FORWARD_INCLUDED

float KrRadiusFromU(BwFragData f)
{
    if (f.color.x > 200.0) return 80.0;
    float4 L2 = _KrLimb2[KrLimbIndex(f.color.x)];
    float u = f.uv.z;
    return u < L2.x ? max(L2.y, 12.0) * (1.0 + (L2.x - u) / 150.0) : max(0.3, L2.y * pow(max(1.0 - (u - L2.x) / max(L2.z, 1.0), 0.0), 1.43));
}

// The local lights' share of BwShadeEx (the same BRDF, smoothness pre-filtered by the caller): under water it skips the
// downwelling attenuation exp(-Kd depth) that belongs to the sun and sky light from the surface.
float3 KrAdditionalLit(InputData inputData, SurfaceData s, float smoothF)
{
    float3 sum = float3(0.0, 0.0, 0.0); // pal:ok (zero)
#if defined(_ADDITIONAL_LIGHTS)
    s.smoothness = (half)smoothF;
    BRDFData brdfData;
    InitializeBRDFData(s, brdfData);
    const half4 shadowMask = half4(1, 1, 1, 1);
    float3 N = inputData.normalWS, V = inputData.viewDirectionWS;
    uint pixelLightCount = GetAdditionalLightsCount();
    #if USE_CLUSTER_LIGHT_LOOP
    [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
        sum += BwDirectPBR(brdfData, light.color, light.direction, light.distanceAttenuation * light.shadowAttenuation, N, V);
    }
    #endif
    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
        sum += BwDirectPBR(brdfData, light.color, light.direction, light.distanceAttenuation * light.shadowAttenuation, N, V);
    LIGHT_LOOP_END
#endif
    return sum;
}

// Emission hue and level clamp (fix wave X2): the eye glow keeps its red (G <= 0.14 R, B <= 0.08 R: the palette's 0.12 /
// 0.02 with a little room) whatever the water did to it, and rolls off softly towards 0.53 x the clamp 3 / 2^EV (1.6 after
// exposure: the core stays red-orange through the tonemapper instead of clipping to yellow-white, KS17 R/G 0.81 -> >= 1.8).
float3 KrEmissionClamp(float3 em)
{
    em.g = min(em.g, 0.14 * em.r);
    em.b = min(em.b, 0.08 * em.r);
    float cap = 0.53 * (_KrPhase.w > 0.01 ? _KrPhase.w : 3.0);
    float m = max(em.r, 1e-5);
    return em * (cap * (1.0 - exp(-m / cap)) / m);
}

half4 KrFrag(BwVaryings i) : SV_Target0
{
    UNITY_SETUP_INSTANCE_ID(i);
    BwFragData f = BwMakeFrag(i);
    BwClipHook(f);
    SurfaceData s = (SurfaceData)0;
    s.albedo = half3(0.5, 0.5, 0.5); // pal:ok (neutral default before the hook)
    s.alpha = 1.0h;
    s.smoothness = 0.5h;
    s.occlusion = 1.0h;
    s.normalTS = half3(0, 0, 1);
    BwSurfaceHook(f, s);
    half3 emission = s.emission;
    s.emission = half3(0, 0, 0); // pal:ok

    // lee-side cameras beyond 3 km (the stinger from the lantern platform, R3: a dark silhouette against the dusk sky): the
    // low-sun wash and the red rim scatter are cut, the hide darkened; the fog (true slant range) does the rest
    float farLee = (_BwDamClip.x > 0.5) ? smoothstep(2500.0, 3500.0, f.viewDist) : 0.0;
    s.albedo = (half3)((float3)s.albedo * (1.0 - 0.7 * farLee));
    s.smoothness = (half)((float)s.smoothness * (1.0 - 0.5 * farLee));

    InputData inputData = (InputData)0;
    inputData.positionWS = f.positionWS;
    inputData.positionCS = i.positionCS;
    inputData.normalWS = NormalizeNormalPerPixel(f.normalWS);
    inputData.viewDirectionWS = f.viewDirWS;
    inputData.normalizedScreenSpaceUV = f.screenUV;
#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(f.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0); // pal:ok (not a colour)
#endif
    inputData.shadowMask = half4(1, 1, 1, 1);
    inputData.bakedGI = BwRegionAmbient(inputData.normalWS, f.positionWS, f.normalGeoWS);
    // the smoothness BwShadeEx will use (its specular AA), for the local-light share under water (derivatives in uniform flow)
    float smoothF = ProjectedSpaceGeometricNormalFiltering(min((float)s.smoothness, BW_SMOOTHNESS_MAX), inputData.normalWS, BW_SAA_SCREEN_VARIANCE, BW_SAA_THRESHOLD);
    half3 c = BwShadeEx(inputData, s, f.positionWS, f.normalGeoWS);
    c = (half3)((float3)c * (1.0 - 0.55 * farLee));

    // thin-part scattering (against the sun): tentacles thinner than ~16 m glow blood-red at the rim
    Light ml = GetMainLight(inputData.shadowCoord, f.positionWS, half4(1, 1, 1, 1));
    float thin = saturate(1.0 - KrRadiusFromU(f) / 16.0) * (1.0 - farLee);
    float NoV = saturate(dot(f.normalGeoWS, f.viewDirWS));   // the smooth geometric rim (the bumped normal speckled it)
    float back = pow(saturate(dot(f.viewDirWS, -ml.direction)), 3.0) * (1.0 - NoV) * (1.0 - NoV);
    float3 scatterCol = (f.color.x > 9.5 && f.color.x < 200.0) ? PAL_KrakenMoss * 1.6 : PAL_KrakenUnderRed;   // kelp: green light through the fronds
    // arms: the blood-red glow lives on the undersides (|theta| < ~1.2 rad); the dark backs keep 30 % of it (A1 1.1: dark on
    // top, blood-red / rust below; a back-lit K4 arch read red all over, kraken-show34n show_k4_kk2)
    thin *= (f.color.x < 9.5) ? 1.0 - 0.7 * smoothstep(0.9, 1.6, abs(f.uv.w)) : 1.0;
    c += (half3)(scatterCol * (2.2 * _RimScatter * thin * back) * ml.color * (0.35 + 0.65 * ml.shadowAttenuation) * BwSunBreak(f.positionWS));

    // a camera inside the dam below the sea (the window gallery) sees anything below the sea surface through water, even when
    // no window encounter set _KrUnderwater (KS16 still: the arm in the in-air fog read as a flat cut-out brighter than the
    // water, fix wave X2 S2-5)
    float uwAuto = (_BwDamClip.x < 0.5 && _BwCam.x > TC_LeeFaceX + 1.0 && _BwCam.y < TC_SeaY - 2.0) ? 1.0 - smoothstep(TC_SeaY - 2.0, TC_SeaY, f.positionWS.y) : 0.0;
    float uw = max(_KrUnderwater.x, uwAuto);
    float3 emAir = KrEmissionClamp((float3)emission);
    half3 cAir = (half3)BwFogApply((float3)c + emAir + gKrSpec, f.positionWS);
    UNITY_BRANCH
    if (uw > 0.001)
    {
        float3 Kd = float3(TC_KdR, TC_KdG, TC_KdB); // pal:ok (attenuation, not a colour)
        float3 cext = float3(TC_BeamCR, TC_BeamCG, TC_BeamCB); // pal:ok (beam attenuation c of 14.7 H3)
        float seaY = _KrUnderwater.x > 0.001 ? _KrUnderwater.y : TC_SeaY;
        float depth = max(seaY - f.positionWS.y, 0.0);
        float3 cAdd = KrAdditionalLit(inputData, s, smoothF);
        float3 cSun = max((float3)c - cAdd, 0.0);
        float3 L0 = cSun * exp(-Kd * depth) + cAdd;
        float r = max(f.viewDist - _KrUnderwater.w, 0.0);
        float3 T = exp(-min(cext * r, KR_TAU_MAX));
        float3 veil = KrVeil(-f.viewDirWS, max(seaY - _BwCam.y, 0.0));
        float3 em = KrEmissionClamp((float3)emission * (T + _KrUnderwater.z * (1.0 - T)));
        float3 cw = L0 * T + em + gKrSpec * (T + 0.5 * (1.0 - T)) + veil * (1.0 - T) * gKrVeilKeep;
        c = (half3)lerp((float3)cAir, cw, uw);
    }
    else c = cAir;
    return half4(c, 1.0h);
}

#endif
