// NocGlass - tubes and the glass strip (14.2.5): Fresnel F0 0.04, alpha 0.12 + 0.6 Fresnel, ZWrite off, Transparent,
// premultiplied (Blend One OneMinusSrcAlpha). Back faces first (R3 5.4, D-008): two materials of this shader, back faces
// (Cull Front, queue 3000) and front faces (Cull Back, queue 3001), so every back face is drawn before every front face.
// Shading: the Noc lit template's lights (GGX highlights of the Forward+ lights x LightCal), the analytic sky reflection x
// Fresnel (8.11 item 5), a faint tint from the ambient; the straight colour is fogged (NocFog), floored (NocFloor) and
// premultiplied by alpha, so a fogged pane fades to the sky colour like everything else.
// Look v2 (-surf-v 2, TEXTURE_PLAN 2.7 / 3.2 / 3.5, CRIT0 #17; SU): neutral, dark and reflection-dominated. The alpha is the Fresnel
// term only (a uniform tint drew the hard band along the tube top), the tint is the GlassDark token, the reflection is the sky
// plus the beat's registered wall planes (NocWallReflect of NocMirror.hlsl, mirror-consistent fog), and opaque painted frames
// (MetalPaint, SurfGlassRibWidth wide every SurfGlassRibPitch m along the dominant horizontal axis of the pane: the tube axis or
// the balustrade run) give the tubes their structure; each frame fades to its mean coverage below 2 px. Grime and rivulets
// (OverlayArr 1 / 2) follow with TP-1's arrays.
Shader "Megastructure/NocGlass"
{
    Properties
    {
        _BaseColor ("Tint (linear)", Color) = (0.0232, 0.0232, 0.0395, 1)
        _Smoothness ("Smoothness", Range(0, 0.95)) = 0.95
        _AlphaBase ("Alpha at normal incidence", Range(0, 1)) = 0.12
        _AlphaFresnel ("Alpha x Fresnel", Range(0, 1)) = 0.6
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull (Front = back faces, Back = front faces)", Float) = 2
        // Look v2 (-surf-v 2; TEXTURE_PLAN 3.2, CRIT0 #17): grime (OverlayArr slice 1, 1.5 m) and rain rivulets (slice 2, 1.0 m, within
        // SurfGlassRivuletD of the eye) on the pane's (run, height) coordinates; 0 = off (no tap)
        [NoScaleOffset] _OverlayMask ("v2 OverlayArr mask", 2DArray) = "" {}
        [NoScaleOffset] _OverlayNormal ("v2 OverlayArr normal", 2DArray) = "" {}
        _GrimeGain ("v2 grime gain", Range(0, 1)) = 0
        _RivuletGain ("v2 rivulet gain", Range(0, 1)) = 0
        // TR (INT-40 / INT-16): a per-material cap of the Fresnel term (alpha and the sky / wall reflection); 1 = no cap
        _FresnelMax ("Fresnel / reflection cap", Range(0, 1)) = 1
    }

    HLSLINCLUDE
    #include "Include/NocLitCore.hlsl"
    #include "Include/NocTexturing.hlsl"
    #include "Include/NocMirror.hlsl"
    #include "Include/NocFacadeSkin.hlsl"     // FX (INT-40 / INT-16): NocSkylineV2, the reflected city skyline of the facade glass
    // FX (C06): the horizontal-pane constants NC_SurfGlassRingGain / GrimeH / GrimeBreak / SrcRho / SigmaFloor (NocSurfaceConstants)

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float _Smoothness;
        float _AlphaBase;
        float _AlphaFresnel;
        float _Cull;
        float _GrimeGain;
        float _RivuletGain;
        float _FresnelMax;
    CBUFFER_END

    TEXTURE2D_ARRAY(_OverlayMask); SAMPLER(sampler_OverlayMask);
    TEXTURE2D_ARRAY(_OverlayNormal);

    float NocGlassFresnel(float nv) { return 0.04 + 0.96 * PositivePow(1.0 - saturate(nv), 5.0); }

    // v2 frame coverage: lines across the dominant horizontal axis of the pane (x unless the pane runs along z), filtered.
    float NocGlassRibs(float3 P, float3 n, float fp)
    {
        float a = abs(n.x) <= abs(n.z) + 1e-3 ? P.x : P.z;
        return NocFilteredGrid1(a, NC_SurfGlassRibPitch, NC_SurfGlassRibWidth, fp, 2.0);
    }

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        float3 n = f.normalWS;
        if (dot(n, f.viewDirWS) < 0.0) n = -n;                       // back faces (Cull Front material) shade their inner side
        f.normalWS = n;
        float F = NocGlassFresnel(dot(n, f.viewDirWS));
        s.metallic = 0.0;
        s.occlusion = 1.0;
        // v2 overlay coordinates (m): along the pane's run (the rib axis) and up; derivatives before the branch.
        // FX (INT-17): (run, y) is degenerate where the pane turns horizontal (the KS13 glass strip / floor, a tube's top and
        // bottom: y constant across them), so the grime map was sampled along one line and drew bands across everything seen
        // through or mirrored in the pane. Those parts take (x, z), blended by wf over |n.y| 0.55..0.85 (no seam on a tube).
        float wf = smoothstep(0.55, 0.85, abs(n.y));
        float2 g = float2(abs(n.x) <= abs(n.z) + 1e-3 ? f.positionWS.x : f.positionWS.z, f.positionWS.y);
        float2 gF = f.positionWS.xz;
        float2 gdx = ddx(g), gdy = ddy(g), gFdx = ddx(gF), gFdy = ddy(gF);
        float fwm = max(max(length(gFdx), length(gFdy)), 1e-5);               // m per pixel on (x, z) (the horizontal panes)
        float rib = NocGlassRibs(f.positionWS, n, f.footprint);
        // grime raises the roughness 0.05 -> 0.20 and scatters a little light (alpha and albedo); rivulets bend the reflection
        // near the eye; both fade to their means by the footprint, no tap at gain 0
        float grime = 0.0;
        float2 slope = 0.0;
        [branch] if (_GrimeGain > 0.0)
        {
            float gv = 0.0, gh = 0.0;
            [branch] if (wf < 1.0)
            {
                float2 uv = g / 1.5, dx = gdx / 1.5, dy = gdy / 1.5;
                float fade = NocTexFade(1.0, NocFootprintBox(dx, dy));
                gv = lerp(0.136, 0.0, fade);                                        // texmeta coverage mean of slice 1
                [branch] if (fade > 0.0) gv += fade * SAMPLE_TEXTURE2D_ARRAY_GRAD(_OverlayMask, sampler_OverlayMask, uv, 1, dx, dy).r;
            }
            [branch] if (wf > 0.0)
            {
                float2 uv = gF / 1.5, dx = gFdx / 1.5, dy = gFdy / 1.5;
                float fade = NocTexFade(1.0, NocFootprintBox(dx, dy));
                gh = lerp(0.136, 0.0, fade);
                [branch] if (fade > 0.0) gh += fade * SAMPLE_TEXTURE2D_ARRAY_GRAD(_OverlayMask, sampler_OverlayMask, uv, 1, dx, dy).r;
            }
            grime = lerp(gv, gh, wf) * _GrimeGain;
        }
        [branch] if (_RivuletGain > 0.0 && f.viewDist < NC_SurfGlassRivuletD && wf < 1.0)   // rivulets run down the steep panes only
        {
            float near = 1.0 - smoothstep(0.6 * NC_SurfGlassRivuletD, NC_SurfGlassRivuletD, f.viewDist);
            float2 uv = g + float2(0.37, 0.0), dx = gdx, dy = gdy;
            float fade = NocTexFade(1.0, NocFootprintBox(dx, dy)) * near * _RivuletGain * (1.0 - wf);
            [branch] if (fade > 0.0) slope = NocTangentToSlope(SAMPLE_TEXTURE2D_ARRAY_GRAD(_OverlayNormal, sampler_OverlayMask, uv, 2, dx, dy), 1.0) * fade;
        }
        // FX (C06, fix wave p4_01): the horizontal panes (the KS10 ledge, the KS13 strip) carry the rain: the water film's impact
        // rings (NocWet's NocRippleGrad2, faded by the footprint like the puddles; 0 at rain 0, so the static test is unchanged)
        float rho = _NocFog.w;
        float rfade = wf * smoothstep(NC_SurfRippleFadePx0, NC_SurfRippleFadePx1, NC_SurfRippleLambda / fwm);
        float2 ring = 0.0;
        [branch] if (rho > 0.001 && rfade > 0.0) ring = NocRippleGrad2(gF, _NocTime.y, rho) * (NC_SurfRippleSlope * NC_SurfGlassRingGain * rfade);
        float2 tt = float2(-n.z, n.x);
        float3 Tg = dot(tt, tt) > 1e-6 ? normalize(float3(tt.x, 0.0, tt.y)) : float3(1.0, 0.0, 0.0);
        f.normalWS = normalize(NocResolveNormal(n, NocSurfGradTB(slope, Tg, float3(0.0, 1.0, 0.0))) + float3(ring.x, 0.0, ring.y));
        float Fc = min(F, _FresnelMax);                                       // TR's cap (1 = none)
        // the grime's occlusion of what lies behind: 0.25 on the steep panes; up to 0.75 on the horizontal ones, where the dirt is seen
        // against the bright abyss (at 0.25 its +-0.1 coverage moved the haze by +-1.6 codes: invisible in the KS13 4K crop, FX_4k13)
        float aGlass = saturate(Fc + (1.0 - F) * (0.25 + 0.5 * wf) * grime);
        // grime on the horizontal panes scatters more (dust and dried spots seen from above catch the deck light)
        s.albedo = PAL_GlassDark + NC_SurfGlassGrimeAlb * grime * (1.0 + NC_SurfGlassGrimeH * wf);
        float rough = lerp(NC_SurfGlassRough0, NC_SurfGlassRough1, saturate(grime));
        s.smoothness = 1.0 - rough;
        s.alpha = rib + (1.0 - rib) * aGlass;
        f.custom = float4(F, rib, aGlass, 1.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend One OneMinusSrcAlpha
            ZWrite Off ZTest LEqual Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #define NOC_PASS_FORWARD
            #define NOC_HAS_FORWARD_HOOK
            #define NOC_TRANSPARENT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Include/NocLighting.hlsl"

            // Straight colour C with C x alpha = tint x ambient x alpha + Fresnel x sky + light highlights.
            // v2: C x alpha = frame x (shaded MetalPaint) + (1 - frame) x (tint x ambient x F + F x (sky over the wall planes) + highlights).
            half3 NocForwardHook(NocFragData f, SurfaceData s, InputData inputData)
            {
                BRDFData brdf;
                InitializeBRDFData(s, brdf);
                float F = f.custom.x, Fc = min(F, _FresnelMax);
                float3 r = reflect(-inputData.viewDirectionWS, inputData.normalWS);
                // FX (C06): the registered light sources (lamps, LED lines: NocWetRegistry) mirrored in the horizontal panes seen
                // from above, through the wet floor's closed-form lobe mass (never brighter than the emitter; the lobe is built here,
                // outside any branch, for its derivatives); the grime breaks the image up
                float wUp = smoothstep(0.55, 0.85, abs(inputData.normalWS.y)) * (inputData.normalWS.y > 0.0 ? 1.0 : 0.0);
                float grimeR = saturate((f.custom.z - Fc) / max((1.0 - F) * (0.25 + 0.5 * smoothstep(0.55, 0.85, abs(inputData.normalWS.y))), 1e-4));
                NocLobe oL = NocMirrorLobe(f.positionWS, _WorldSpaceCameraPos.xyz, inputData.normalWS, brdf.roughness * brdf.roughness + NC_SurfGlassSigmaFloor, 1.0);
                // INT-19 / C06: on a horizontal pane seen from above the Forward+ lights keep their diffuse (the grime lit by the deck
                // lamps) but not their GGX highlight: a point light's peak D(0) = 1 / (pi alpha^2) at the glass alpha 0.0025 blooms into
                // a 75-110 px orb (KS13 (1240, 730): p4_01 75 px, AB's lamp-sized lobe with its 0.02 alpha floor 109 px, FX_r10); the
                // mirrored source core (srcs) is the lamp's image there, bounded by its radiance x F. Vertical panes keep x K.
                BRDFData brdfL = brdf;
                brdfL.specular *= (half)(1.0 - wUp);
                half3 premul;
                [branch] if (f.custom.w > 0.5)
                {
                    float rib = f.custom.y, aGlass = f.custom.z;
                    half3 sky = NocReflection(r, brdf.perceptualRoughness);
                    float blur = max(brdf.perceptualRoughness * brdf.perceptualRoughness, NocPixelAngle());
                    // FX (INT-40 / INT-16): the glass reflects the city skyline in front of the sky, as the facade glass does
                    // (NocSkylineV2: fogged dark shafts with window speckle rising into the deck), so a tube at grazing mirrors the
                    // dark city around it instead of a milky band of the horizon glow; the wall planes stay in front of both
                    float2 rh = r.xz / max(length(r.xz), 1e-4);
                    float3 Lb;
                    float cov = NocSkylineV2(r, f.positionWS, blur, NocAmbient(float3(-rh.x, 0.0, -rh.y)), _NocFacade.y, _NocFacade.w, sky, Lb);
                    half3 bgC = lerp(sky, (half3)Lb, (half)cov);
                    float4 w = NocWallReflect(f.positionWS, r, blur, f.viewDist, sky);
                    half3 env = lerp(bgC, w.rgb, w.a);
                    half3 srcs = 0;
                    [branch] if (wUp > 0.0) srcs = (half3)(NocMirrorSourcesMixR(f.positionWS, f.viewDist, oL, oL, 1.0, NC_SurfGlassSrcRho) * (wUp * (1.0 - NC_SurfGlassGrimeBreak * grimeR) * Fc / max(F, 1e-4)));
                    // FX (INT-19): the lamps' highlights x NC_SurfGlassLightK (none on the horizontal panes, see above)
                    half3 glass = brdf.diffuse * inputData.bakedGI * aGlass + env * Fc + srcs + NocDirect(brdfL, inputData) * NC_SurfGlassLightK;
                    half3 frame = 0;
                    [branch] if (rib > 0.0)
                    {
                        SurfaceData m = s;
                        m.albedo = PAL_MetalPaint;
                        m.smoothness = 0.45;
                        m.alpha = 1.0;
                        frame = NocShade(inputData, m);
                    }
                    premul = rib * frame + (1.0 - rib) * glass;
                }
                else
                {
                    premul = brdf.diffuse * inputData.bakedGI * s.alpha + NocReflection(r, brdf.perceptualRoughness) * F
                           + NocDirect(brdf, inputData);
                }
                return premul / max(s.alpha, 1e-3);
            }

            #include "Include/NocLitPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
