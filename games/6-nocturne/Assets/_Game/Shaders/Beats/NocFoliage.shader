// NocFoliage - the crowns of the hanging garden (MASTER_PROMPT 14.2.5, 14.5.5; Look v2: TEXTURE_PLAN 4, LOOK_V2 5.1 and 10) through
// the Noc lit template (trilight x LightCal, Forward+ lights, analytic sky reflection, specular AA). Three modes, chosen per material
// by _LeafMode (a material constant: every branch on it is uniform; no keyword):
// 0, the v1 glowing crown (the graybox; -foliage-v 1, unchanged): dark leaf albedo (Moss token) plus emission:
//   leaf glow   LeafGlow x _GlowGain x (0.55 + 0.9 leaf) x clump: `leaf` = contrast-stretched 3D value noise on 0.9 m world cells
//               (60 %) and on 0.35 m cells (40 %), `clump` = the same noise on 5 m cells (mean 1), each octave faded to its mean (0.5)
//               by the pixel footprint (smoothstep(1.5, 4.0) px per cell), so a far crown is its mean energy (no shimmer, no
//               moire; Part 8.6) and a near crown is a soft glow with leaf-sized variation, not dark blotches;
//   tips        LeafWarm x _TipGain where a third noise exceeds a threshold, on up-facing surfaces, faded to its mean by the same rule;
//   rim         LeafGlow x _RimGain x (1 - n.v)^_RimPower (the silhouette glows against the cloud);
//   underside   x (1 + _UnderGain x max(0, -n.y)) (KS15: the glowing undersides seen from below).
// 1, the v2 CORE (lib.LeafCore, queue 2000): the opaque lobed crown at 0.90 seen through the card gaps = the deep leaves: two
//   layers of the LeafCrown atlas (sprites of 2 m, the second offset by half a sprite, a hashed sprite per 2 m cell; the sprite
//   borders are empty, so the cell edges never show) planar-projected on the axis plane facing the smooth core normal, with
//   gradients from ddx / ddy of the position (continuous across the axis switch); albedo = Leaf x 2 x the leaf colour, x
//   lerp(_CoreGap 0.25, 1, cover) (the gaps between those leaves are deeper still; r03 had 0.45 and a lobe facing the camera
//   still read as a smooth dome), occlusion = _CoreAo x the lobe-valley AO of
//   vertex colour R (GardenTree.CoreV2); no emission. Mips average cover and colour (no sharpening: no shimmer).
// 2, the v2 CARDS (lib.Leaves, queue 2450, AlphaToMask On under MSAA 4x, Cull Off): one sprite of the LeafCrown atlas (a
//   Texture2DArray of 4 cluster sprites, texprep; leaves at true size) per card (GardenLeafCards: uv0 sprite uv, uv1.x slice,
//   uv1.y card hash, uv1.zw the octahedral card normal, normal = the ellipsoid normal, tangent = card u, colour R AO, G glow card,
//   B sky exposure):
//   alpha       Golus 2017 sharpening a' = (a - 0.5) / max(fwidth(a), 1e-4) + 0.5 while a leaf covers 2 px or more (mip <=
//               _SharpMip; a 0.15 m leaf is 75 texels at mip 0), faded to the coverage-preserving mip alpha over 1.5 mips
//               (lerp(a', a, saturate((mip - _SharpMip) / 1.5))): FO lookdev r01 faded it by mip 3 as TEXTURE_PLAN 4 drafted and the
//               KS15 fringe became a grey-green veil of dithered soft alpha around 3 px leaves; _AlphaMipBias is the moire fallback;
//   albedo      Leaf x 2 x variation (mean 0.5: the token is the mean) x a per-card hue (+-8 %) and value (+-6 %) jitter;
//   normal      n = normalize(lerp(faceSign x card normal, ellipsoid normal, _BendNormal 0.7)) + the atlas normal as a surface
//               gradient along the card's u / v (x faceSign: a thin leaf's back is the mirrored front), strength 0.8;
//   occlusion   vertex AO x lerp(1, atlas AO, 0.8); smoothness = atlas smoothness + _WetSmooth (rain sheen), <= 0.75;
//   light       the template's PBR terms plus thin-leaf transmission (8.11 item 7): albedo x _Transmission x (sky exposure x
//               (trilight(-n_view) + fill) + the local lights), each x saturate(L . -n_view) where n_view is the card normal
//               turned to the viewer, x LightCal (the core above shadows the underside cards: exposure, no shadow maps);
//   emission    LeafGlow specks and the LeafGlow rim on the 2.5 % bioluminescent cards only (specks: value noise in sprite space
//               above _SpeckThreshold on the upper leaves, faded to its mean by the footprint; FO lookdev r02: a rim on every card
//               outlined each lobe of the lobed crowns with a jade ring, 21-24 codes against 13-16 inside), LeafWarm tips on all,
//               at the LOOK_V2 10 gains (0.012, 0.015, 0.03 on the v2 materials).
// Derivatives (uv, alpha) and the atlas taps come first, before every branch. NocFog and NocFloor last (NocLitPass). Colours are
// PAL_* tokens (D-014); gains are linear. Every division is guarded; the noise is the PCG twin of GardenTree.Noise3.
Shader "Megastructure/NocFoliage"
{
    Properties
    {
        _AlbedoGain ("Leaf albedo (x Moss token in v1, x Leaf in v2)", Range(0, 2)) = 0.6
        _Smoothness ("Smoothness", Range(0, 0.95)) = 0.3
        _CellSize ("Leaf cell (m)", Float) = 0.9
        _ClumpSize ("Clump cell (m)", Float) = 5.0
        _FineSize ("Fine leaf cell (m)", Float) = 0.35
        _Contrast ("Leaf contrast", Range(0, 4)) = 1.8
        _GlowGain ("Leaf glow gain (x LeafGlow)", Float) = 0.042
        _TipGain ("Tip glow gain (x LeafWarm)", Float) = 0.08
        _TipThreshold ("Tip threshold", Range(0, 1)) = 0.68
        _RimGain ("Rim gain (x LeafGlow)", Float) = 0.06
        _RimPower ("Rim power", Float) = 3
        _UnderGain ("Underside glow gain", Float) = 1.0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        _LeafMode ("Mode (0 v1 crown, 1 v2 core, 2 v2 cards)", Float) = 0
        _AlphaToMask ("Alpha to coverage (0 off, 1 on)", Float) = 0
        _AlbedoTex ("Leaf atlas albedo variation (a = opacity)", 2DArray) = "" {}
        _NormalTex ("Leaf atlas normal (GL)", 2DArray) = "" {}
        _MaskTex ("Leaf atlas mask (G AO, B height, A smoothness)", 2DArray) = "" {}
        _NormalStrength ("Atlas normal strength", Range(0, 2)) = 0.8
        _BendNormal ("Bent normal toward the ellipsoid", Range(0, 1)) = 0.7
        _HueJitter ("Per-card hue jitter", Range(0, 0.2)) = 0.08
        _Transmission ("Thin-leaf transmission (x albedo)", Range(0, 3)) = 1.2
        _WetSmooth ("Rain sheen (smoothness added)", Range(0, 0.5)) = 0.2
        _CoreAo ("Core occlusion (crown interior)", Range(0, 1)) = 0.55
        _CoreContrast ("Core leaf-noise contrast (unused since the atlas core)", Range(0, 2)) = 0.7
        _CoreGap ("Core: brightness of the gaps between its leaves", Range(0, 1)) = 0.25
        _SpeckThreshold ("Speck threshold", Range(0, 1)) = 0.8
        _AlphaMipBias ("Alpha mip bias (moire fallback)", Range(0, 1)) = 0
        _SharpMip ("Alpha sharpening kept up to this mip (leaf >= 2 px)", Range(0, 8)) = 5
    }

    HLSLINCLUDE
    #include "../Include/NocLitCore.hlsl"
    #define NOC_PASS_UV1ZW

    CBUFFER_START(UnityPerMaterial)
        float _AlbedoGain;
        float _Smoothness;
        float _CellSize;
        float _ClumpSize;
        float _FineSize;
        float _Contrast;
        float _GlowGain;
        float _TipGain;
        float _TipThreshold;
        float _RimGain;
        float _RimPower;
        float _UnderGain;
        float _Cull;
        float _LeafMode;
        float _AlphaToMask;
        float _NormalStrength;
        float _BendNormal;
        float _HueJitter;
        float _Transmission;
        float _WetSmooth;
        float _CoreAo;
        float _CoreContrast;
        float _SpeckThreshold;
        float _AlphaMipBias;
        float _SharpMip;
        float _CoreGap;
        float4 _AlbedoTex_TexelSize;
    CBUFFER_END

    TEXTURE2D_ARRAY(_AlbedoTex);
    SAMPLER(sampler_AlbedoTex);
    TEXTURE2D_ARRAY(_NormalTex);
    TEXTURE2D_ARRAY(_MaskTex);

    // 3D value noise in [0, 1], quintic, PCG lattice (twin of GardenTree.Noise3: NocPcg(x + NocPcg(y + NocPcg(z))))
    float NocFoliageHash(int3 i) { return NocU01(NocPcg((uint)i.x + NocPcg((uint)i.y + NocPcg((uint)i.z)))); }
    float NocFoliageNoise(float3 p)
    {
        float3 f = floor(p);
        int3 i = int3(f);
        float3 t = p - f;
        float3 u = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
        float a = lerp(NocFoliageHash(i), NocFoliageHash(i + int3(1, 0, 0)), u.x);
        float b = lerp(NocFoliageHash(i + int3(0, 1, 0)), NocFoliageHash(i + int3(1, 1, 0)), u.x);
        float c = lerp(NocFoliageHash(i + int3(0, 0, 1)), NocFoliageHash(i + int3(1, 0, 1)), u.x);
        float d = lerp(NocFoliageHash(i + int3(0, 1, 1)), NocFoliageHash(i + int3(1, 1, 1)), u.x);
        return lerp(lerp(a, b, u.y), lerp(c, d, u.y), u.z);
    }

    // weight of a pattern of cell size `cell` (m) at footprint fp (m / px): 1 above 4 px per cell, 0 below 1.5 px
    float NocFoliageKeep(float cell, float fp) { return smoothstep(1.5, 4.0, cell / max(fp, 1e-5)); }

    // octahedral decode (twin of GardenLeafCards.OctDecode)
    float3 NocFoliageOctDecode(float2 e)
    {
        float3 n = float3(e.x, e.y, 1.0 - abs(e.x) - abs(e.y));
        float t = saturate(-n.z);
        n.xy += float2(n.x >= 0.0 ? -t : t, n.y >= 0.0 ? -t : t);
        return normalize(n);
    }

    // the v1 crown (mode 0), unchanged from the graybox
    void NocFoliageV1(inout NocFragData f, inout SurfaceData s)
    {
        float3 p = f.positionWS;
        float3 n = f.normalWS;
        float fp = f.footprint;
        float cell = max(_CellSize, 0.05), clumpCell = max(_ClumpSize, 0.1), fine = max(_FineSize, 0.05);

        float wLeaf = NocFoliageKeep(cell, fp);
        float wFine = NocFoliageKeep(fine, fp);
        float wClump = NocFoliageKeep(clumpCell, fp);
        // the noise is only evaluated where it shows (uniform branches would not save work; the weights are pixel-local)
        float nLeaf = wLeaf > 0.0 ? NocFoliageNoise(p / cell) : 0.5;
        float nFine = wFine > 0.0 ? NocFoliageNoise(p / fine + float3(7.7, 31.1, 11.9)) : 0.5;
        float nClump = wClump > 0.0 ? NocFoliageNoise(p / clumpCell + float3(17.3, 5.1, 29.7)) : 0.5;
        float nTip = wLeaf > 0.0 ? NocFoliageNoise(p / cell + float3(41.9, 13.7, 3.3)) : 0.5;

        float leaf = saturate(0.5 + _Contrast * (0.6 * (nLeaf - 0.5) * wLeaf + 0.4 * (nFine - 0.5) * wFine));   // mean 0.5 at every distance
        float clump = 1.0 + 0.8 * (nClump - 0.5) * wClump;                           // mean 1
        const float tipMean = 0.13;                                                  // mean of the step below over the noise (numerical, threshold 0.68)
        float tipRaw = smoothstep(_TipThreshold, _TipThreshold + 0.12, nTip);
        float tip = lerp(tipMean, tipRaw, wLeaf) * saturate(n.y * 1.5 + 0.2);

        float nv = saturate(dot(n, f.viewDirWS));
        float rim = PositivePow(1.0 - nv, max(_RimPower, 0.5));
        float under = 1.0 + _UnderGain * saturate(-n.y);

        float3 E = PAL_LeafGlow * (_GlowGain * (0.55 + 0.9 * leaf) * clump * under + _RimGain * rim)
                 + PAL_LeafWarm * (_TipGain * tip);

        s.albedo = PAL_Moss * _AlbedoGain * (0.7 + 0.6 * leaf);
        s.metallic = 0.0;
        s.smoothness = min(_Smoothness, 0.95);
        s.occlusion = lerp(1.0, f.color.r, 0.5);
        s.emission = E;
    }

    // the v2 core (mode 1): two planar layers of the leaf atlas on the axis plane facing the normal (see the header)
    void NocFoliageCore(inout NocFragData f, inout SurfaceData s, float3 dpx, float3 dpy)
    {
        const float k = 0.5;                                                  // 1 / sprite metres
        float3 p = f.positionWS;
        float3 an = abs(f.normalWS);
        float useY = an.y >= max(an.x, an.z) ? 1.0 : 0.0;
        float useX = (1.0 - useY) * (an.x >= an.z ? 1.0 : 0.0);
        float useZ = 1.0 - useY - useX;
        float2 pc = useY * p.xz + useX * p.zy + useZ * p.xy;
        float2 gx = (useY * dpx.xz + useX * dpx.zy + useZ * dpx.xy) * k;
        float2 gy = (useY * dpy.xz + useX * dpy.zy + useZ * dpy.xy) * k;
        float2 u1 = pc * k, u2 = pc * k + 0.5;
        float s1 = floor(NocFoliageHash(int3(int2(floor(u1)), 17)) * 3.999);
        float s2 = floor(NocFoliageHash(int3(int2(floor(u2)), 29)) * 3.999);
        float4 a1 = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoTex, sampler_AlbedoTex, u1, s1, gx, gy);
        float4 a2 = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoTex, sampler_AlbedoTex, u2, s2, gx, gy);
        float c1 = saturate(a1.a), c2 = saturate(a2.a);
        float cover = 1.0 - (1.0 - c1) * (1.0 - c2);
        float3 col = (saturate(a1.rgb) * c1 + saturate(a2.rgb) * c2 * (1.0 - c1)) / max(cover, 1e-3);
        col = lerp(float3(0.5, 0.5, 0.5), col, saturate(cover * 8.0));     // no leaf: the atlas mean
        s.albedo = PAL_Leaf * _AlbedoGain * 2.0 * col * lerp(_CoreGap, 1.0, cover);
        s.metallic = 0.0;
        s.smoothness = min(_Smoothness, 0.95);
        s.occlusion = _CoreAo * f.color.r;
        s.emission = 0.0;
    }

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        // ---- derivatives and the atlas taps first (before every branch)
        float2 uv = f.uv.xy;
        float slice = floor(f.uv.z + 0.5);
        float2 dx = ddx(uv), dy = ddy(uv);
        float3 dpx = ddx(f.positionWS), dpy = ddy(f.positionWS);
        float aBias = exp2(_AlphaMipBias);
        float4 alb = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoTex, sampler_AlbedoTex, uv, slice, dx * aBias, dy * aBias);
        float aw = max(fwidth(alb.a), 1e-4);
        float res = _AlbedoTex_TexelSize.z > 1.0 ? _AlbedoTex_TexelSize.z : 1024.0;
        float mip = 0.5 * log2(max(max(dot(dx, dx), dot(dy, dy)) * res * res, 1e-8)) + _AlphaMipBias;

        [branch] if (_LeafMode < 0.5) { NocFoliageV1(f, s); return; }
        [branch] if (_LeafMode < 1.5) { NocFoliageCore(f, s, dpx, dpy); return; }

        // ---- cards (mode 2)
        float4 nTex = SAMPLE_TEXTURE2D_ARRAY_GRAD(_NormalTex, sampler_AlbedoTex, uv, slice, dx, dy);
        float4 mask = SAMPLE_TEXTURE2D_ARRAY_GRAD(_MaskTex, sampler_AlbedoTex, uv, slice, dx, dy);

        float aSharp = saturate((alb.a - 0.5) / aw + 0.5);
        s.alpha = (half)lerp(aSharp, saturate(alb.a), saturate((mip - _SharpMip) * (1.0 / 1.5)));

        float hash = f.uv.w;
        float j = (2.0 * hash - 1.0) * _HueJitter;
        float3 tint = float3(1.0 + j, 1.0, 1.0 - 1.5 * j);
        tint /= max(dot(tint, float3(0.2126, 0.7152, 0.0722)), 1e-3);
        float value = 1.0 + 0.12 * (frac(hash * 7.31) - 0.5);
        s.albedo = PAL_Leaf * _AlbedoGain * 2.0 * saturate(alb.rgb) * tint * value;
        s.metallic = 0.0;

        // normals: card normal toward the viewer side, bent toward the ellipsoid normal, then the atlas as a surface gradient
        float3 nCard = NocFoliageOctDecode(f.custom.zw);
        float3 nRad = f.normalWS;
        float3 nView = nCard * f.faceSign;
        float3 T = f.tangentWS.xyz;
        T = normalize(T - nCard * dot(nCard, T) + 1e-6);
        float3 B = cross(nCard, T);
        float3 ts = UnpackNormalScale(nTex, max(_NormalStrength, 0.0));
        float3 grad = (ts.x * T + ts.y * B) / max(ts.z, 0.25);
        float3 nBent = normalize(lerp(nView, nRad, _BendNormal));
        float3 n = normalize(nBent + f.faceSign * grad * (1.0 - _BendNormal * 0.5));
        f.normalWS = n;

        float ao = f.color.r * lerp(1.0, mask.g, 0.8);
        s.occlusion = ao;
        s.smoothness = min(mask.a + _WetSmooth, 0.75);

        // transmission inputs for the forward hook: the viewer-side leaf normal and the weight (sky exposure in custom.w's sign
        // bit is not needed: exposure is the vertex B)
        f.custom = float4(nView, _Transmission * ao);

        // emission: specks on the glow cards, rim, tips (LOOK_V2 10)
        float fp = f.footprint;
        float speck = 0.0;
        [branch] if (f.color.g > 0.5)
        {
            const float speckCells = 40.0;                                   // cells per sprite: ~5 cm on a 2 m card
            float raw = smoothstep(_SpeckThreshold, _SpeckThreshold + 0.08, NocFoliageNoise(float3(uv * speckCells, hash * 61.0)));
            const float speckMean = 0.06;                                    // mean of the step over the noise (threshold 0.80)
            float keep = NocFoliageKeep(2.0 / speckCells, fp);
            speck = lerp(speckMean, raw, keep) * smoothstep(0.35, 0.75, mask.b);
        }
        float glowCard = f.color.g > 0.5 ? 1.0 : 0.0;
        float nv = saturate(dot(nBent, f.viewDirWS));
        float rim = PositivePow(1.0 - nv, max(_RimPower, 0.5)) * glowCard;
        float tip = smoothstep(0.70, 0.95, mask.b) * saturate(n.y * 1.5 + 0.2);
        s.emission = (half3)(PAL_LeafGlow * (_GlowGain * speck + _RimGain * rim) + PAL_LeafWarm * (_TipGain * tip));
    }

    // depth-type passes (never run while depth priming and the depth texture are off; kept correct): the card cut-out at 0.5
    void NocClipHook(NocFragData f)
    {
        [branch] if (_LeafMode > 1.5)
        {
            float a = SAMPLE_TEXTURE2D_ARRAY_LOD(_AlbedoTex, sampler_AlbedoTex, f.uv.xy, floor(f.uv.z + 0.5), 0).a;
            clip(a - 0.5);
        }
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On ZTest LEqual Cull [_Cull] AlphaToMask [_AlphaToMask]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #define NOC_PASS_FORWARD
            #define NOC_ALPHA_OUT
            #define NOC_FRONT_FACE
            #define NOC_HAS_FORWARD_HOOK
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Include/NocLighting.hlsl"

            // thin-leaf transmission of one light toward the viewer side (8.11 item 7)
            half3 NocFoliageTrans(Light light, float3 nView)
            {
                return light.color * (light.distanceAttenuation * light.shadowAttenuation * saturate(dot(light.direction, -nView)));
            }

            // mode 2: the template's PBR terms (NocShadeOcc, written out) plus the transmission through the card in one light loop
            half3 NocFoliageShadeCards(NocFragData f, SurfaceData s, InputData inputData)
            {
                BRDFData brdfData;
                InitializeBRDFData(s, brdfData);
                half3 reflectVector = reflect(-inputData.viewDirectionWS, inputData.normalWS);
                half NoV = saturate(dot(inputData.normalWS, inputData.viewDirectionWS));
                half fresnelTerm = Pow4(1.0h - NoV);
                half3 indirectSpecular = NocReflection(reflectVector, brdfData.perceptualRoughness);
                half3 color = inputData.bakedGI * brdfData.diffuse * s.occlusion
                            + indirectSpecular * EnvironmentBRDFSpecular(brdfData, fresnelTerm) * (s.occlusion * f.specOcc);

                float3 nView = f.custom.xyz;
                half3 tint = (half3)(s.albedo * f.custom.w);
                half exposure = (half)f.color.b;
                const BRDFData noClearCoat = (BRDFData)0;
                const half4 shadowMask = half4(1, 1, 1, 1);
                Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
                half3 direct = LightingPhysicallyBased(brdfData, noClearCoat, mainLight, inputData.normalWS, inputData.viewDirectionWS, 0.0h, false);
                half3 trans = exposure * NocFoliageTrans(mainLight, nView);
            #if defined(_ADDITIONAL_LIGHTS)
                uint pixelLightCount = GetAdditionalLightsCount();
                #if USE_CLUSTER_LIGHT_LOOP
                [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                {
                    CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
                    Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
                    direct += LightingPhysicallyBased(brdfData, noClearCoat, light, inputData.normalWS, inputData.viewDirectionWS, 0.0h, false);
                    trans += exposure * NocFoliageTrans(light, nView);
                }
                #endif
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
                    direct += LightingPhysicallyBased(brdfData, noClearCoat, light, inputData.normalWS, inputData.viewDirectionWS, 0.0h, false);
                    trans += NocFoliageTrans(light, nView);
                LIGHT_LOOP_END
            #endif
                [branch] if (_NocKoiK > 0.0) direct += brdfData.diffuse * NocKoiLight(inputData.positionWS, inputData.normalWS);
                color += (direct + tint * trans) * NocLightCal();
                color += tint * exposure * NocAmbient(-nView);          // the lid's light through the leaf (NocAmbient has LightCal)
                color += s.emission;
                return min(color, 1000.0);
            }

            half3 NocForwardHook(NocFragData f, SurfaceData s, InputData inputData)
            {
                // one return (INT): the early return inside [branch] drew fxc X4000 "potentially uninitialized variable"
                half3 c;
                [branch] if (_LeafMode < 1.5) c = NocShadeOcc(inputData, s, f.specOcc);
                else c = NocFoliageShadeCards(f, s, inputData);
                return c;
            }
            #include "../Include/NocLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #define NOC_PASS_SHADOW
            #define NOC_HAS_CLIP_HOOK
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
            #define NOC_HAS_CLIP_HOOK
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
            #define NOC_HAS_CLIP_HOOK
            #include "../Include/NocLitPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
