// BwLighting.hlsl - shared lit model of every lit Bw* material (MASTER_PROMPT 8.6, 8.11.1, 8.11.5, 8.11.6; Part 10 #15,
// #38). Imported from The Ring's RingLighting.hlsl; rewritten by the materials owner (Wave A) after reading URP 17.6
// (Library/PackageCache/com.unity.render-pipelines.universal@8457e85b8184, package.json 17.6.0):
//   * BRDF: URP's own BRDFData (InitializeBRDFData: metallic workflow, kDielectricSpec F0 0.04, perceptual roughness =
//     1 - smoothness, roughness = max(pr^2, HALF_MIN_SQRT)) and DirectBRDFSpecular (GGX D x the V*F approximation of
//     "Optimizing PBR for Mobile"), exactly as UniversalFragmentPBR -> LightingPhysicallyBased (Lighting.hlsl 48-96), but
//     the specular term and the per-light specular radiance are clamped (8.11.6) and the light loop is our own.
//   * Indirect: EnvironmentBRDF / EnvironmentBRDFSpecular (BRDF.hlsl 157-172: surfaceReduction 1/(roughness^2 + 1),
//     lerp(F0, grazingTerm, Fresnel)) with URP's Fresnel term Pow4(1 - NoV) (GlobalIllumination.hlsl 543). URP's
//     GlossyEnvironmentReflection samples unity_SpecCube0 (here _ENVIRONMENTREFLECTIONS_OFF); our reflection source is an
//     ANALYTIC environment from BwSkyColor (BulwarkAtmosphere.hlsl, the same function that draws the sky and the fog, so
//     reflections can never lag or mismatch the sky: Part 10 #38) along the GGX dominant direction (Lagarde & de Rousiers
//     2014), mixed toward the trilight ambient as roughness rises and indoors. Ambient is our trilight from globals (never
//     SH; never write unity_Ambient* or _GlossyEnvironmentColor, URP overwrites them per camera), chosen by REGION
//     (Wave B): BwAmbientAt(n, posWS) of BulwarkAtmosphere.hlsl blends the exterior trilight with the interior ambient
//     (lamp colour x 0.06; beat 6 cyan x 0.08) by the interior boxes of the shaded point, so the exterior seen through the
//     door, portal or tunnel mouth keeps the exterior ambient and the interiors their lamp colour (Part 10 #36, 14.6);
//     the smooth-lobe mirror is the sky outdoors and the interior ambient indoors, by the same region weight.
//   * Occlusion: diffuse x AO; specular x Lagarde's specular occlusion from AO x a horizon term on the geometric normal
//     (URP multiplies all GI by AO, GlobalIllumination.hlsl 566).
//   * Specular AA (8.11.6): smoothness capped at 0.95, then URP core ProjectedSpaceGeometricNormalFiltering
//     (CommonMaterial.hlsl 192-227, Tokuyoshi & Kaplanyan 2021) on the shading normal; textures add their own Toksvig
//     variance in the Mask mips (BwTexGen).
//   * The main light is attenuated by BwSunBreak (14.7 H2); BwFogApply is applied last by the pass.
// Requires Core.hlsl and Lighting.hlsl first and a local variable named `inputData` (LIGHT_LOOP_BEGIN reads it).
#ifndef BW_LIGHTING_INCLUDED
#define BW_LIGHTING_INCLUDED

#include "BulwarkAtmosphere.hlsl"

// _BwSurface (rain wetness, interior factor, lamp gain) is declared in BwCommon.hlsl.

#define BW_SMOOTHNESS_MAX 0.95          // 8.11.6 roughness floor (water and glass are analytic elsewhere)
#define BW_SAA_SCREEN_VARIANCE 0.15     // pixel kernel variance of the geometric specular AA (URP core: at most 0.25)
#define BW_SAA_THRESHOLD 0.2            // largest added roughness (squared: 0.04)
#define BW_SPEC_TERM_MAX 100.0          // clamp of the D*V*F/4 term per light (URP mobile uses 1000 for half overflow)
#define BW_SPEC_RADIANCE_MAX 16.0       // clamp of the specular radiance per light (linear HDR, before exposure)

// Trilight ambient around world up.
half3 BwAmbient(float3 normalWS)
{
    float u = normalWS.y;
    float sky = saturate(u), ground = saturate(-u);
    float mid = 1.0 - sky - ground;
    return _BwAmbSky.rgb * sky + _BwAmbMid.rgb * mid + _BwAmbGround.rgb * ground;
}

// Probe refinement inside an interior box. The interior boxes (_BwIntBox, atmosphere owner) end at the shells' OUTER
// faces with a symmetric soft edge of +-e (_BwAmbInt.w, 0.75 m), so a probe in a room corner can lie within e of ANOTHER
// face of its box: in the 4 m gallery the walls are 0.6 m thick and the floor 0.5 m above the box bottom, so the wall
// foot (y 0..0.25) sampled up to 1 - (0.5 / 1.5 + 0.5) = 17 % and the floor edge up to 7 % of the exterior trilight and
// sky mirror (a blue-grey band along both junctions in KS01, int-03: blue 7 -> 34 levels). For a probe inside a box (that
// box's weight wk > 0.5; each axis factor is then > 0.5) the probe moves toward the box shrunk by e (where wk = 1) by
// t = smoothstep(0.5, 0.75, wk): t = 0 on every box face (wk = 0.5), so the weight stays continuous through the door,
// the portal and the tunnel mouth, and the exterior (wk <= 0.5) is exactly the atmosphere owner's BwAmbientAt.
float3 BwInteriorProbe(float3 q)
{
    float e = max(_BwAmbInt.w, 1e-3);
    float best = 0.0;
    float3 lo = q, hi = q;
    [unroll] for (int k = 0; k < 4; k++)
    {
        float3 mn = _BwIntBox[2 * k].xyz, mx = _BwIntBox[2 * k + 1].xyz;
        float3 i3 = min(saturate((q - mn) / (2.0 * e) + 0.5), saturate((mx - q) / (2.0 * e) + 0.5));
        float wk = i3.x * i3.y * i3.z;
        if (wk > best) { best = wk; lo = mn + e; hi = max(mn + e, mx - e); }
    }
    return lerp(q, clamp(q, lo, hi), smoothstep(0.5, 0.75, best));
}

// Sky light through the rectangular openings of the interiors (doors, portals, arches; Wave B; published by
// LocalLightPool as _BwOpen, its only writer): [2k] = (xMin, xMax, yMin, yMax) in the plane z = [2k+1].x, [2k+1].y = +1
// when the exterior lies at z > plane (-1: at z < plane), .z = reach (m), .w = on. Each opening is an area light of
// exterior radiance for the points on its interior side: Lambert's polygon formula (BwEdgeIrr of BulwarkAtmosphere.hlsl,
// as BwMouthLight for the tunnel mouth), F = n . Phi / 2 pi, radiance = the sky along Phi (the ground ambient below the
// horizon). Returns F x L (outgoing radiance per albedo), 0 on the exterior side (rising over 0.25 m inside the plane)
// and more than 1 m below the sill, fading over the last 30 % of the reach. C# twin: LocalLightPool.OpeningFactor.
float4 _BwOpen[8];

float3 BwOpeningLight(float3 n, float3 p)
{
    float3 sum = 0.0;
    [unroll] for (int k = 0; k < 4; k++)
    {
        float4 r = _BwOpen[2 * k], c = _BwOpen[2 * k + 1];
        // interior side of the plane (rising over 0.25 m), and not below the sill: points more than 1 m under it (the
        // stairwell under the atrium floor) see the opening only through the floor, which this unshadowed term ignores
        float inside = saturate((c.x - p.z) * c.y / 0.25) * saturate(1.0 + p.y - r.z);
        float3 ctr = float3(0.5 * (r.x + r.y), 0.5 * (r.z + r.w), c.x);   // pal:ok (point)
        float dist = length(ctr - p);
        float reach = max(c.z, 1e-3);
        if (c.w < 0.5 || inside <= 0.0 || dist > reach) continue;
        float3 A = normalize(float3(r.x, r.z, c.x) - p);   // pal:ok (point)
        float3 B = normalize(float3(r.y, r.z, c.x) - p);   // pal:ok (point)
        float3 C = normalize(float3(r.y, r.w, c.x) - p);   // pal:ok (point)
        float3 D = normalize(float3(r.x, r.w, c.x) - p);   // pal:ok (point)
        float3 phi = BwEdgeIrr(A, B) + BwEdgeIrr(B, C) + BwEdgeIrr(C, D) + BwEdgeIrr(D, A);
        phi *= (phi.z * (c.x - p.z) < 0.0) ? -1.0 : 1.0;  // the vector irradiance points through the opening
        float F = saturate(dot(n, phi) * 0.159154943);
        float lp = length(phi);
        float3 d = lp > 1e-6 ? phi / lp : (ctr - p) / max(dist, 1e-6);
        float3 hd = float3(d.x, max(d.y, 0.0), d.z);                                  // pal:ok (not a colour)
        hd = dot(hd, hd) > 1e-6 ? hd : float3(0, 0, 1);                               // pal:ok (straight down: any horizontal direction)
        float3 L = lerp(BwSkyColor(hd), _BwAmbExtGround.rgb, smoothstep(0.0, 0.35, -d.y));
        sum += (F * inside * (1.0 - smoothstep(0.7 * reach, reach, dist))) * L;
    }
    return sum;
}

// Where a surface point samples the region ambient: one soft-edge width (_BwAmbInt.w, 0.75 m) along its GEOMETRIC normal,
// i.e. inside the space the surface faces, refined by BwInteriorProbe. A floor, an inner wall and their junctions then
// take the full interior ambient (no exterior bleed along the floor-wall junctions of the 4 m gallery), the outer faces
// of the same shell and the exterior seen through the door keep the exterior ambient (Wave B, Part 10 #14 / #36).
float3 BwAmbientProbePos(float3 positionWS, float3 normalGeoWS)
{
    return BwInteriorProbe(positionWS + normalGeoWS * max(_BwAmbInt.w, 0.0));
}

// The region ambient of a surface point: BwAmbientAt (exterior trilight / interior ambient by the interior boxes) at
// BwAmbientProbePos, plus the sky light of the open doors, portals and arches (BwOpeningLight at the point itself) on
// the interior part (it follows the same interior weight, so it joins the exterior trilight continuously at the plane).
float3 BwRegionAmbient(float3 normalWS, float3 positionWS, float3 normalGeoWS)
{
    float3 q = BwAmbientProbePos(positionWS, normalGeoWS);
    return BwAmbientAt(normalWS, q) + BwInteriorWeight(q) * BwOpeningLight(normalWS, positionWS);
}

// Trilight ambient around world up at a world point (BwRegionAmbient with the shading normal as geometric normal).
half3 BwAmbient(float3 normalWS, float3 positionWS)
{
    return (half3)BwRegionAmbient(normalWS, positionWS, normalWS);
}

// Analytic environment radiance along a direction: the sky above the horizon (BwSkyColor), the fog-coloured horizon
// sinking into the ground ambient below it (the lowland and the sea are seen through kilometres of haze).
float3 BwEnvSky(float3 d)
{
    float3 hd = float3(d.x, max(d.y, 0.0), d.z);   // pal:ok (not a colour)
    hd = dot(hd, hd) > 1e-6 ? hd : float3(0, 0, 1); // pal:ok (straight down: any horizontal direction)
    float3 sky = BwSkyColor(hd);
    return lerp(sky, _BwAmbGround.rgb, smoothstep(0.0, 0.35, -d.y));
}

// GGX dominant direction (Lagarde & de Rousiers 2014).
float3 BwDominantDir(float3 R, float3 N, float perceptualRoughness)
{
    float a = perceptualRoughness * perceptualRoughness;
    float3 d = lerp(N, R, (1.0 - a) * (sqrt(saturate(1.0 - a)) + a));
    float dl = dot(d, d);
    return dl > 1e-8 ? d * rsqrt(dl) : R;
}

// Environment radiance of a GGX lobe at a world point: dominant direction, mirror image of the sky outdoors (of the
// interior ambient indoors, by the region weight of the point) for smooth lobes, the region trilight ambient (the
// irradiance the diffuse term uses) for rough lobes.
float3 BwEnvironment(float3 R, float3 N, float perceptualRoughness, float3 positionWS)
{
    float3 d = BwDominantDir(R, N, perceptualRoughness);
    float w = BwInteriorWeight(positionWS);
    float3 amb = BwAmbientAt(d, positionWS) + w * BwOpeningLight(d, positionWS);
    float3 mirror = lerp(BwEnvSky(d), amb, w);
    return lerp(mirror, amb, smoothstep(0.2, 0.8, perceptualRoughness));
}

// The position-free variant (callers without a world point): the s-keyed trilight and interior factor (_BwSurface.y).
float3 BwEnvironment(float3 R, float3 N, float perceptualRoughness)
{
    float3 d = BwDominantDir(R, N, perceptualRoughness);
    float3 amb = BwAmbient(d);
    float3 mirror = lerp(BwEnvSky(d), amb, saturate(_BwSurface.y));
    return lerp(mirror, amb, smoothstep(0.2, 0.8, perceptualRoughness));
}

// Kept for callers of the imported API (reflection along R only).
half3 BwReflection(float3 reflectWS, float perceptualRoughness)
{
    float3 r = normalize(reflectWS);
    return BwEnvironment(r, r, perceptualRoughness);
}

// One light, URP LightingPhysicallyBased without clear coat, with the specular clamps.
half3 BwDirectPBR(BRDFData brdfData, half3 lightColor, half3 lightDir, float atten, float3 N, float3 V)
{
    half NdotL = saturate(dot(N, lightDir));
    half3 radiance = lightColor * (atten * NdotL);
    half spec = min(DirectBRDFSpecular(brdfData, N, lightDir, V), BW_SPEC_TERM_MAX);
    half3 specRad = min(brdfData.specular * spec * radiance, BW_SPEC_RADIANCE_MAX);
    return brdfData.diffuse * radiance + specRad;
}

half3 BwShadeEx(InputData inputData, SurfaceData surfaceData, float3 positionWS, float3 normalGeoWS)
{
    // specular AA: roughness floor, then geometric normal filtering of the shading normal (screen-space derivatives)
    float smoothness = min((float)surfaceData.smoothness, BW_SMOOTHNESS_MAX);
    surfaceData.smoothness = (half)ProjectedSpaceGeometricNormalFiltering(smoothness, inputData.normalWS, BW_SAA_SCREEN_VARIANCE, BW_SAA_THRESHOLD);

    BRDFData brdfData;
    InitializeBRDFData(surfaceData, brdfData);
    const half4 shadowMask = half4(1, 1, 1, 1);
    float3 N = inputData.normalWS, V = inputData.viewDirectionWS;

    // GetMainLight(shadowCoord, positionWS, shadowMask) applies the shadow-distance fade
    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
    mainLight.shadowAttenuation *= BwSunBreak(positionWS);
    half3 mainColor = mainLight.color;
#if defined(BW_HAS_LIGHT_HOOK)
    BwMainLightHook(mainColor, mainLight.direction);
#endif

    // indirect: trilight diffuse x AO, analytic environment x specular occlusion x horizon occlusion
    float3 R = reflect(-V, N);
    half NoV = saturate(dot(N, V));
    half fresnelTerm = Pow4(1.0h - NoV);
    float ao = saturate(surfaceData.occlusion);
    float specOcc = saturate(pow(abs(NoV + ao), exp2(-16.0 * brdfData.roughness - 1.0)) - 1.0 + ao);
    float horizon = saturate(1.0 + dot(R, normalGeoWS));
    horizon *= horizon;
    float3 env = BwEnvironment(R, N, brdfData.perceptualRoughness, BwAmbientProbePos(positionWS, normalGeoWS));
    half3 color = inputData.bakedGI * brdfData.diffuse * ao
                + env * EnvironmentBRDFSpecular(brdfData, fresnelTerm) * (specOcc * horizon);

    color += BwDirectPBR(brdfData, mainColor, mainLight.direction, mainLight.distanceAttenuation * mainLight.shadowAttenuation, N, V);

#if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();
    #if USE_CLUSTER_LIGHT_LOOP
    // non-main directional lights are not in the clusters
    [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
        color += BwDirectPBR(brdfData, light.color, light.direction, light.distanceAttenuation * light.shadowAttenuation, N, V);
    }
    #endif
    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
        color += BwDirectPBR(brdfData, light.color, light.direction, light.distanceAttenuation * light.shadowAttenuation, N, V);
    LIGHT_LOOP_END
#endif

    color += surfaceData.emission;
    color = (AnyIsNaN(color)) ? half3(0, 0, 0) : color; // pal:ok (NaN guard)
    return min(color, 1000.0);
}

// The imported 3-argument API (BwLine and other callers): geometric normal = shading normal.
half3 BwShade(InputData inputData, SurfaceData surfaceData, float3 positionWS)
{
    return BwShadeEx(inputData, surfaceData, positionWS, inputData.normalWS);
}

#endif
