// BwArmour - the 40 m "Colossopod" armour units of the reef and the islets (MASTER_PROMPT 14.2.6, 14.5.4, 8.11).
// Zoned by the height above the mean sea (TC_SeaY, -8.0 m), every boundary broken by world noise:
//   * below the wet line (_WetHeight above the sea): ConcreteWet, smoothness up to _WetSmoothness (spray-wet);
//   * the swash / splash band from 1 m below the sea up to _SwashHeight above it: barnacle crust from the BwTexGen
//     "Crust" set (two generations of shells with craters on a slimy film, 0.5 mm texels on a uniform 0.5 m tile), two
//     decorrelated samples merged by height, thinned by a world-space colony density, faded to its mean below 4 px per
//     shell, patchy;
//     ("barnacle crust above the swash line": the crust starts at the swash line and climbs the splash zone);
//   * a broken foam line in FoamWhite at the DYNAMIC waterline (Wave X, beat 4 owner): the sea height at the fragment from
//     BwSeaWaves (Gerstner sum at a 1 m spacing + the kraken's disturbance heights, the same globals as BwSea), so the
//     white swash film rides up and down the hull with the swell (centred 0.25 m above the instantaneous waterline,
//     half-width 2 x _FoamWidth, thicker on a rising crest); evaluated only within 6 m of the mean sea ([branch]);
//   * above: dry ConcreteDark basalt-coloured concrete from the "Concrete" set (2.0 m + 5.37 m samples), macro tone +-6 %.
// Ochre gauge bands (SafetyOchre, alternating with the concrete every _GaugePitch in y, filtered) where vertex B is set
// or _GaugeBands is 1: the gauge platforms on one tip in five within 300 m. Vertex R = AO, G = edge wear, A = per-unit tone.
Shader "Megastructure/BwArmour"
{
    Properties
    {
        _BaseColor ("Dry colour (linear)", Color) = (0.070, 0.078, 0.084, 1)
        _WetColor ("Wet colour (linear)", Color) = (0.037, 0.045, 0.053, 1)
        _FoamColor ("Foam colour (linear)", Color) = (0.823, 0.880, 0.896, 1)
        _BandColor ("Gauge band colour (linear)", Color) = (0.571, 0.323, 0.033, 1)
        _SeaLevel ("Mean sea level (y)", Float) = -8.0
        _WetHeight ("Wet up to (m above the sea)", Float) = 5.0
        _SwashHeight ("Crust up to (m above the sea)", Float) = 3.0
        _FoamWidth ("Foam line half-width (m)", Float) = 0.35
        _Smoothness ("Smoothness offset dry (0.30 = texture)", Range(0, 0.9)) = 0.26
        _WetSmoothness ("Smoothness wet", Range(0, 0.95)) = 0.45
        _MacroTone ("Macro tone amplitude", Range(0, 0.2)) = 0.06
        _AO ("Vertex AO strength", Range(0, 1)) = 1
        _EdgeWear ("Edge wear strength", Range(0, 1)) = 0.4
        _TexScale ("Concrete tile (m)", Float) = 2.0
        _CrustScale ("Crust tile (m)", Float) = 0.5
        _NormalStrength ("Normal strength", Range(0, 2)) = 1.0
        _CrustStrength ("Crust amount", Range(0, 1)) = 1.0
        _GaugeBands ("Gauge bands everywhere", Range(0, 1)) = 0
        _GaugePitch ("Gauge band height (m)", Float) = 0.5
        [NoScaleOffset] _AlbedoMap ("Concrete albedo", 2D) = "grey" {}
        [NoScaleOffset] _NormalMap ("Concrete normal", 2D) = "bump" {}
        [NoScaleOffset] _MaskMap ("Concrete mask", 2D) = "grey" {}
        [NoScaleOffset] _CrustAlbedo ("Crust albedo", 2D) = "grey" {}
        [NoScaleOffset] _CrustNormal ("Crust normal", 2D) = "bump" {}
        [NoScaleOffset] _CrustMask ("Crust mask", 2D) = "grey" {}
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "Include/BwLitCore.hlsl"
    #include "Include/BwSeaWaves.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float4 _WetColor;
        float4 _FoamColor;
        float4 _BandColor;
        float _SeaLevel;
        float _WetHeight;
        float _SwashHeight;
        float _FoamWidth;
        float _Smoothness;
        float _WetSmoothness;
        float _MacroTone;
        float _AO;
        float _EdgeWear;
        float _TexScale;
        float _CrustScale;
        float _NormalStrength;
        float _CrustStrength;
        float _GaugeBands;
        float _GaugePitch;
    CBUFFER_END

    TEXTURE2D(_AlbedoMap); SAMPLER(sampler_AlbedoMap);
    TEXTURE2D(_NormalMap);
    TEXTURE2D(_MaskMap);
    TEXTURE2D(_CrustAlbedo);
    TEXTURE2D(_CrustNormal);
    TEXTURE2D(_CrustMask);
    // sampler_PointRepeat: URP core's global point / repeat sampler (GlobalSamplers.hlsl)

    // Shown fraction of the fragment's shells for a colony density: the shell ranks of the dominant triplanar plane are
    // POINT-sampled at mip 0 of the uncompressed (RGBA32) Crust mask (exact ranks of whole shells: trilinear mips and BC7
    // endpoints both blended two ranks at the rims and the threshold left isolated pixels) at 4 sub-pixel positions (+-1/4 px along the screen derivatives), so the decision itself is
    // anti-aliased: a pixel on the border of a shown and a hidden shell gets a partial value, never a lone speck.
    float BwCrustShow(BwTri t, float dens)
    {
        bool px = t.w.x >= t.w.y && t.w.x >= t.w.z, py = !px && t.w.y >= t.w.z;
        float2 uv = px ? t.uvX : (py ? t.uvY : t.uvZ);
        float2 dx = 0.25 * (px ? t.dxX : (py ? t.dxY : t.dxZ)), dy = 0.25 * (px ? t.dyX : (py ? t.dyY : t.dyZ));
        float r0 = SAMPLE_TEXTURE2D_LOD(_CrustMask, sampler_PointRepeat, BwWrap64(uv + dx + dy), 0.0).r;
        float r1 = SAMPLE_TEXTURE2D_LOD(_CrustMask, sampler_PointRepeat, BwWrap64(uv + dx - dy), 0.0).r;
        float r2 = SAMPLE_TEXTURE2D_LOD(_CrustMask, sampler_PointRepeat, BwWrap64(uv - dx + dy), 0.0).r;
        float r3 = SAMPLE_TEXTURE2D_LOD(_CrustMask, sampler_PointRepeat, BwWrap64(uv - dx - dy), 0.0).r;
        return 0.25 * (step(r0, dens) + step(r1, dens) + step(r2, dens) + step(r3, dens));
    }

    void BwSurfaceHook(inout BwFragData f, inout SurfaceData s)
    {
        float3 p = f.positionWS;
        float3 ng = f.normalGeoWS;
        float fp = f.footprint;

        // concrete: two decorrelated samples
        BwTri t0 = BwTriSetup(p, ng, _TexScale, 6.0);
        BwTri t1 = BwTriDetune(t0, 0.3727, float2(0.71, 0.23));
        float mixK = 0.6 * lerp(0.55, 1.0, smoothstep(0.25, 0.75, BwValueNoise3(p * 0.09 + 5.3)));   // never one pure tile
        float3 cAlb = lerp(BwTriSample(TEXTURE2D_ARGS(_AlbedoMap, sampler_AlbedoMap), t0).rgb,
                           BwTriSample(TEXTURE2D_ARGS(_AlbedoMap, sampler_AlbedoMap), t1).rgb, mixK);
        float4 cMask = lerp(BwTriSample(TEXTURE2D_ARGS(_MaskMap, sampler_AlbedoMap), t0),
                            BwTriSample(TEXTURE2D_ARGS(_MaskMap, sampler_AlbedoMap), t1), mixK);
        float3 cN = normalize(lerp(BwTriNormal(TEXTURE2D_ARGS(_NormalMap, sampler_AlbedoMap), t0, ng, _NormalStrength),
                                   BwTriNormal(TEXTURE2D_ARGS(_NormalMap, sampler_AlbedoMap), t1, ng, _NormalStrength * 0.8), mixK));

        // zones by height above the sea, edges broken by noise (0.6-1.5 m wobble)
        float hs = p.y - _SeaLevel;
        float wob = (BwFbmGrad3(p * 0.35, fp * 0.35, 3) - 0.5) * 3.0;
        float wetK = 1.0 - smoothstep(_WetHeight - 1.0, _WetHeight + 0.5, hs + wob);
        float patch = smoothstep(0.3, 0.5, BwFbmGrad3(p * 0.6 + 11.0, fp * 0.6, 3));
        float crustK = _CrustStrength * patch * smoothstep(-1.6, -0.8, hs + 0.5 * wob)
                     * (1.0 - smoothstep(_SwashHeight - 0.8, _SwashHeight + 0.4, hs + wob));
        // foam residue: horizontal streaks along the waterline (3D fBm stretched 4:1 along the horizontal, so it never
        // shows the lattice's axis-aligned blocks), broken, soft-edged
        float foamN = BwFbmGrad3(p * float3(1.1, 4.4, 1.1), fp * 4.4, 4);   // pal:ok (anisotropic scale, not a colour)
        float lace = smoothstep(0.35, 0.65, BwFbmGrad3(p * 9.0 + 2.3, fp * 9.0, 2));   // 11 cm lace inside the streaks
        // the instantaneous waterline (THE sea's height here; 0 where SeaState has not published, e.g. material sheets)
        float hsDyn = hs, rise = 0.0;
        [branch] if (abs(hs) < 6.0)
        {
            float2 lw = BwSeaLocal(p);
            float imW = BwSeaImageWeight(p.x);
            float calmW = BwSeaCalm(p.xz);
            float hW = (BwSeaDisplace(lw, 1.0, imW).y + BwSeaDisturbHeight(p.xz, 1.0)) * calmW;
            float hE = BwSeaDisplace(lw + float2(6.0, 0.0), 1.0, imW).y * calmW;   // pal:ok (not a colour: 6 m east, the crest to come)
            hsDyn = hs - hW;
            rise = saturate((hE - hW) * 0.5 + 0.5);                            // a crest arriving from the east: the swash climbs
        }
        float fw = 2.0 * _FoamWidth * lerp(0.8, 1.6, rise);
        // the band is a thin horizontal stripe far away: its edges widen by the vertical pixel footprint and its amplitude
        // keeps the band's coverage (fw / (fw + 2 fy)), so a sub-pixel band greys out instead of breaking into isolated
        // bright pixels (reef-01..03: NaN-like specks along the waterline at 150-700 m)
        float fy = abs(ddx(p.y)) + abs(ddy(p.y)) + 1e-4;
        float bandCov = fw / (fw + 2.0 * fy);
        float foamK = (1.0 - smoothstep(0.3 * fw - fy, fw + fy, abs(hsDyn - 0.25 + 0.3 * wob))) * bandCov
                    * lerp(smoothstep(0.42, 0.7, foamN), 0.5, 1.0 - bandCov) * lerp(0.35, 1.0, lerp(lace, 0.5, 1.0 - bandCov));

        // ---- barnacle crust (only where it shows). Two decorrelated samples of the uniform "Crust" tile (0.5 m, and a
        //      rotated 0.65 m copy through the wrap-safe BwTriDetune: shells 47 / 18 mm), merged by HEIGHT (the taller shell wins: the two shell
        //      sets interpenetrate like a crowded crust, never a ghosted 50 % blend), the layer bias wandering on a 1.4 m world
        //      field; triplanar sharpness 12 (no doubled shells on curved faces). Colony density is a world-space,
        //      footprint-filtered 3D gradient fBm (0.5 / 0.25 / 0.12 m; round colonies, no lattice blocks): every texel carries the random rank of its nearest shell
        //      (Mask R, constant over the shell and its surrounding film, so the mips never mix a shell's rim with another
        //      rank) and a shell shows only where the density exceeds it, else the slimy film (whole shells vanish, no
        //      tile-periodic colonies). Once a 27 mm shell is under 4 px the detail fades to the tile's mean (its 1 x 1 mip) mixed with the film by the
        //      density, gone at 2 px: no 0.5 m lattice at 45-140 m (the islets, the belt).
        float3 filmC = lerp(PAL_ConcreteWet, PAL_SeaTeal, 0.12);
        float densN = BwFbmGrad3(p * 2.0 + 4.2, fp * 2.0, 3);
        float dens = saturate((densN - 0.5) * 2.4 + 0.62);                 // shown fraction of the shells, p5 0.10 .. p95 1
        float detailK = BwPxFade(0.027, fp, 2.0, 4.0);
        float3 crAlb = filmC, crN = ng;
        float4 crMask = float4(0.0, 1.0, 0.2, 0.36);                        // film: AO 1, low, smoothness 0.36 (pal:ok)
        BwTri tc0 = BwTriSetup(p, ng, _CrustScale, 12.0);                   // derivatives outside the branch
        BwTri tc1 = BwTriDetune(tc0, 0.77, float2(0.37, 0.11));
        [branch] if (crustK > 0.002)
        {
            // each layer is thinned first (a shell shows where the colony density exceeds its rank), THEN the two are merged
            // by height: a hidden shell has the film's height, so the other layer never leaves a crescent of it
            float4 filmM = float4(0.0, 1.0, 0.2, 0.36);                     // pal:ok (mask, not a colour)
            float show0 = BwCrustShow(tc0, dens), show1 = BwCrustShow(tc1, dens);
            float4 m0 = lerp(filmM, BwTriSample(TEXTURE2D_ARGS(_CrustMask, sampler_AlbedoMap), tc0), show0);
            float4 m1 = lerp(filmM, BwTriSample(TEXTURE2D_ARGS(_CrustMask, sampler_AlbedoMap), tc1), show1);
            float bias = 0.3 * (2.0 * BwValueNoise3(p * 0.7 + 8.3) - 1.0);
            float sel = smoothstep(-0.06, 0.06, m1.b - m0.b + bias);
            float3 a0 = lerp(filmC, BwTriSample(TEXTURE2D_ARGS(_CrustAlbedo, sampler_AlbedoMap), tc0).rgb, show0);
            float3 a1 = lerp(filmC, BwTriSample(TEXTURE2D_ARGS(_CrustAlbedo, sampler_AlbedoMap), tc1).rgb, show1);
            float3 n0 = normalize(lerp(ng, BwTriNormal(TEXTURE2D_ARGS(_CrustNormal, sampler_AlbedoMap), tc0, ng, _NormalStrength), show0));
            float3 n1 = normalize(lerp(ng, BwTriNormal(TEXTURE2D_ARGS(_CrustNormal, sampler_AlbedoMap), tc1, ng, _NormalStrength), show1));
            float3 dAlb = lerp(a0, a1, sel);
            float4 dMask = lerp(m0, m1, sel);
            float3 dN = normalize(lerp(n0, n1, sel));
            // far: the tile mean (1 x 1 mip), thinned toward the film by the density
            float3 meanAlb = SAMPLE_TEXTURE2D_LOD(_CrustAlbedo, sampler_AlbedoMap, float2(0.5, 0.5), 12.0).rgb;
            float4 meanMask = SAMPLE_TEXTURE2D_LOD(_CrustMask, sampler_AlbedoMap, float2(0.5, 0.5), 12.0);
            float3 farAlb = lerp(filmC, meanAlb, dens);
            float4 farMask = lerp(filmM, meanMask, dens);
            crAlb = lerp(farAlb, dAlb, detailK);
            crMask = lerp(farMask, dMask, detailK);
            crN = normalize(lerp(ng, dN, detailK));
        }

        float macro = BwFbmFiltered3(p / 40.0, fp / 40.0, 3);
        float tone = 1.0 + _MacroTone * (2.0 * macro - 1.0);
        float macroS = BwMacroSigned3(BwFbmFiltered3(p / 23.0 + 7.7, fp / 23.0, 3));   // weathering (8.11.4)
        float ao = lerp(1.0, f.color.r, _AO);
        float wear = f.color.g * _EdgeWear;

        // per-unit casting tone (vertex A: 1 = neutral; the beat 4 reef writes a hash per unit, 0.84-1.0: no two pours alike)
        float unitTone = 0.84 + 0.16 * f.color.a;
        // weathering at the scale of metres (8.11.4, megalophobia: the eye reads the surface at the size of the unit):
        // vertical run-off streaks (0.7 m wide, about 15 m long, darker) and a patchy pale salt / guano crust on the
        // up-facing dry tops; both only above the wet zone
        float streakN = BwFbmGrad3(p * float3(0.7, 0.05, 0.7) + 3.1, fp * 0.7, 3);   // pal:ok (anisotropic scale, not a colour)
        float streak = smoothstep(0.52, 0.78, streakN) * (1.0 - wetK);
        float upK = smoothstep(0.55, 0.9, ng.y) * (1.0 - wetK);
        float salt = upK * smoothstep(0.5, 0.75, BwFbmGrad3(p * 0.45 + 9.1, fp * 0.45, 3));

        float3 dry = BwRecolour(cAlb, _BaseColor.rgb, PAL_ConcreteMid);
        float3 wet = BwRecolour(cAlb, _WetColor.rgb, PAL_ConcreteMid);
        // mottling of the pour at 3-8 m (+-12 %) and the TIDE MARK: a pale band of dried salt 0.5-0.9 m above the wet line,
        // the same height on every unit of the reef (a datum line the eye follows across the field: scale), broken by noise
        float mottle = 1.0 + 0.2 * (BwFbmFiltered3(p / 5.0 + 1.7, fp / 5.0, 3) - 0.5);
        float hTide = hs - _WetHeight;
        float tideN = BwFbmGrad3(p * float3(0.9, 2.5, 0.9) + 6.6, fp * 2.5, 3);   // pal:ok (anisotropic scale, not a colour)
        float tideCov = 0.6 / (0.6 + 2.0 * fy);
        float tide = smoothstep(-0.15 - fy, 0.25 + fy, hTide) * (1.0 - smoothstep(0.55 - fy, 1.1 + fy, hTide + 0.3 * (tideN - 0.5)))
                   * lerp(smoothstep(0.35, 0.6, tideN), 0.5, 1.0 - tideCov) * tideCov;
        float3 alb = lerp(dry, wet, wetK) * tone * unitTone * mottle * (1.0 - 0.38 * streak);
        alb = lerp(alb, PAL_LaneWhite * 0.42, saturate(0.42 * salt + 0.45 * tide));
        alb = lerp(alb, alb * 1.15, wear);
        float3 crust = crAlb * lerp(1.0, 0.7, wetK);                         // shells darken when wet
        alb = lerp(alb, crust, crustK);
        alb = lerp(alb, _FoamColor.rgb * (0.85 + 0.15 * foamN), foamK * 0.6);

        // gauge bands (filtered alternation in y)
        // vertex B marks the tide-gauge strip; it is interpolated across one 18 deg facet of the arm, so it is sharpened
        // (0.3-0.7) into a crisp-edged painted strip instead of a band that fades over a metre
        float gauge = saturate(max(smoothstep(0.3, 0.7, f.color.b), _GaugeBands));
        float per = max(2.0 * _GaugePitch, 0.02);
        float fwy = max(abs(ddx(p.y)) + abs(ddy(p.y)), 1e-5);
        float bands = gauge * BwPulseTri(p.y / per, 0.5, fwy / per);
        alb = lerp(alb, BwRecolour(cAlb, _BandColor.rgb, PAL_ConcreteMid), bands);
        s.albedo = alb * lerp(0.8, 1.0, ao);

        float dryS = saturate(cMask.a + (_Smoothness - 0.30)) - 0.06 * wear + 0.08 * macroS;
        float conS = lerp(dryS, _WetSmoothness * (0.85 + 0.15 * (1.0 - cMask.b)), wetK);
        float crS = min(crMask.a + 0.15 * wetK, lerp(0.35, 0.5, wetK));     // shells are chalky: 0.35 dry, 0.5 wet at most
        s.smoothness = saturate(lerp(lerp(conS, crS, crustK), 0.3, foamK) + 0.1 * bands);
        s.metallic = 0.0;
        s.occlusion = ao * lerp(cMask.g, crMask.g, crustK);
        s.emission = 0;
        float3 n = normalize(lerp(cN, crN, crustK));
        f.normalWS = normalize(lerp(n, ng, saturate(0.25 * wetK * (1.0 - crustK) + 0.6 * foamK + 0.6 * bands)));
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
            #pragma fragment BwFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #define BW_PASS_FORWARD
            #include "Include/BwLitPass.hlsl"
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
            #pragma multi_compile_instancing
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
            #pragma multi_compile_instancing
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
            #pragma multi_compile_instancing
            #define BW_PASS_DEPTHNORMALS
            #include "Include/BwLitPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
