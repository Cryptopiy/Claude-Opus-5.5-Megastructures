// NocLighting.hlsl - shared lit model of every Noc* material (imported from The Ring's RingLighting.hlsl, IMPORT_PLAN 7.12;
// upgraded for MASTER_PROMPT 8.6, 8.11 and 14.2.3). It is URP 17.6's UniversalFragmentPBR (Lighting.hlsl) made of URP's own
// BRDF pieces - InitializeBRDFData (metallic workflow, GGX, Fresnel), EnvironmentBRDF (split-sum Fresnel), LightingPhysicallyBased
// (GGX direct specular + Lambert) - with these Nocturne deltas:
//   - ambient is the 14.2.3 trilight from the _NocAmb* globals (x the per-beat multiplier, written by the AtmosphereDriver),
//     never SH or probes;
//   - the environment reflection is analytic: NocSkyColorBlur (the same sky and fog colour the dome shows, 8.11 item 5, Part 10
//     item 38) along r, widened by roughness (three taps across elevation, blur matched to the GGX lobe), x the specular
//     occlusion of the vertex AO;
//   - no shadow maps (14.2.3): the main light is the shadowless sky fill;
//   - one global LightCal (_NocFloor.w) multiplies every light, the ambient and the koi term (never the sky reflection or the
//     emission: they are appearances);
//   - the analytic koi light (14.2.3) behind _NocKoiK: K(P) = sum_i I max(0, n.L_i) / (1 + (d_i/120)^2) over 12 spine points,
//     I = KoiGlow x 0.9 (0 outside beat 7, one uniform branch);
//   - NOC_SPECULAR_OFF (before the include) passes specularHighlightsOff = true to the 9-argument LightingPhysicallyBased
//     (NocWet takes its specular from the analytic mirror, D-003).
// Forward+ (_CLUSTER_LIGHT_LOOP defines USE_CLUSTER_LIGHT_LOOP and _ADDITIONAL_LIGHTS in URP 17.6 Core.hlsl): the non-main
// directional lights are looped explicitly, then LIGHT_LOOP_BEGIN covers the clustered local lights.
// Requires: Core.hlsl and Lighting.hlsl included first, a local variable named `inputData` (LIGHT_LOOP_BEGIN reads it).
#ifndef NOC_LIGHTING_INCLUDED
#define NOC_LIGHTING_INCLUDED

#include "NocAtmosphere.hlsl"

float4 _NocAmbSky;
float4 _NocAmbEquator;
float4 _NocAmbGround;
#define NOC_KOI_POINTS 12   // NC_KoiSpinePoints (an array size must be an integer literal)
float4 _NocKoiSpine[NOC_KOI_POINTS];

#if defined(NOC_SPECULAR_OFF)
    #define NOC_SPEC_OFF true
#else
    #define NOC_SPEC_OFF false
#endif

// The one global light calibration (14.2.3); 1 when no driver ran (editor previews).
float NocLightCal() { return _NocFloor.w > 0.0 ? _NocFloor.w : 1.0; }

// Trilight ambient around world up (x LightCal).
half3 NocAmbient(float3 normalWS)
{
    float u = normalWS.y;
    float sky = saturate(u), ground = saturate(-u);
    float eq = 1.0 - sky - ground;
    return (_NocAmbSky.rgb * sky + _NocAmbEquator.rgb * eq + _NocAmbGround.rgb * ground) * NocLightCal();
}

// Analytic environment reflection: the sky function along r, prefiltered by roughness. The GGX lobe half-width is about
// alpha rad; the sky varies mostly with elevation, so two extra taps across elevation (+-0.9 alpha) stand in for the lobe and
// the cloud noise is blurred by the same width (no sparkle from the cloud octaves in glossy reflections).
half3 NocReflection(float3 reflectWS, float perceptualRoughness)
{
    float3 r = normalize(reflectWS);
    float a = perceptualRoughness * perceptualRoughness;
    float blur = max(a, NocPixelAngle());
    float3 c = NocSkyColorBlur(r, blur);
    float spread = 0.9 * a;
    [branch] if (spread > 0.02)
    {
        float3 up = float3(0.0, 1.0, 0.0);
        float3 ru = normalize(r + up * spread), rd = normalize(r - up * spread);
        c = 0.5 * c + 0.25 * (NocSkyColorBlur(ru, blur) + NocSkyColorBlur(rd, blur));
    }
    return c;
}

// Analytic koi light (14.2.3), diffuse only.
half3 NocKoiLight(float3 positionWS, float3 normalWS)
{
    float k = 0.0;
    [loop] for (int i = 0; i < NOC_KOI_POINTS; i++)
    {
        float3 L = _NocKoiSpine[i].xyz - positionWS;
        float d = length(L);
        float q = d / NC_KoiLightRange;
        k += saturate(dot(normalWS, L / max(d, 1e-3))) / (1.0 + q * q);
    }
    return PAL_KoiGlow * (NC_KoiLightGain * _NocKoiK * k);
}

// Specular anti-aliasing (8.11 item 6): widen roughness by the screen-space variance of the shading normal (Tokuyoshi &
// Kaplanyan 2021 projected-space filtering, URP Core's ProjectedSpaceGeometricNormalFiltering), then cap smoothness at 0.95.
half NocFilterSmoothness(half smoothness, float3 normalWS)
{
    float sm = ProjectedSpaceGeometricNormalFiltering(smoothness, normalWS, NC_SpecAaVariance, NC_SpecAaThreshold);
    return (half)min(sm, NC_SmoothnessMax);
}

// Every direct light (main sky fill, extra directionals, Forward+ cluster, koi), x LightCal. Needs a local `inputData`.
half3 NocDirect(BRDFData brdfData, InputData inputData)
{
    const BRDFData noClearCoat = (BRDFData)0;
    const half4 shadowMask = half4(1, 1, 1, 1);
    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
    half3 direct = LightingPhysicallyBased(brdfData, noClearCoat, mainLight, inputData.normalWS, inputData.viewDirectionWS, 0.0h, NOC_SPEC_OFF);

#if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();
    #if USE_CLUSTER_LIGHT_LOOP
    [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
        direct += LightingPhysicallyBased(brdfData, noClearCoat, light, inputData.normalWS, inputData.viewDirectionWS, 0.0h, NOC_SPEC_OFF);
    }
    #endif
    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
        direct += LightingPhysicallyBased(brdfData, noClearCoat, light, inputData.normalWS, inputData.viewDirectionWS, 0.0h, NOC_SPEC_OFF);
    LIGHT_LOOP_END
#endif

    [branch] if (_NocKoiK > 0.0) direct += brdfData.diffuse * NocKoiLight(inputData.positionWS, inputData.normalWS);
    return direct * NocLightCal();
}

// specOcc (TEXTURE_PLAN 8.3 TP-0) multiplies the specular environment only: the diffuse environment takes s.occlusion, the
// specular s.occlusion x specOcc (URP's EnvironmentBRDF, split; specOcc = 1 is the old NocShade). NocFacade moves its city term
// (D-017) from s.occlusion to f.specOcc in FA-1, so far walls keep their ambient.
half3 NocShadeOcc(InputData inputData, SurfaceData surfaceData, half specOcc)
{
    BRDFData brdfData;
    InitializeBRDFData(surfaceData, brdfData);

    half3 reflectVector = reflect(-inputData.viewDirectionWS, inputData.normalWS);
    half NoV = saturate(dot(inputData.normalWS, inputData.viewDirectionWS));
    half fresnelTerm = Pow4(1.0h - NoV);
    half3 indirectSpecular = NocReflection(reflectVector, brdfData.perceptualRoughness);
    half3 color = inputData.bakedGI * brdfData.diffuse * surfaceData.occlusion
                + indirectSpecular * EnvironmentBRDFSpecular(brdfData, fresnelTerm) * (surfaceData.occlusion * specOcc);

    color += NocDirect(brdfData, inputData);
    color += surfaceData.emission;
    return min(color, 1000.0);
}

half3 NocShade(InputData inputData, SurfaceData surfaceData) { return NocShadeOcc(inputData, surfaceData, 1.0h); }

#endif
