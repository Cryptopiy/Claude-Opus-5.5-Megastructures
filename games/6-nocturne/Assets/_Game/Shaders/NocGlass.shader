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
    }

    HLSLINCLUDE
    #include "Include/NocLitCore.hlsl"
    #include "Include/NocTexturing.hlsl"
    #include "Include/NocMirror.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float _Smoothness;
        float _AlphaBase;
        float _AlphaFresnel;
        float _Cull;
        float _GrimeGain;
        float _RivuletGain;
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
        // v2 overlay coordinates (m): along the pane's run (the rib axis) and up; derivatives before the branch
        float2 g = float2(abs(n.x) <= abs(n.z) + 1e-3 ? f.positionWS.x : f.positionWS.z, f.positionWS.y);
        float2 gdx = ddx(g), gdy = ddy(g);
        float rib = NocGlassRibs(f.positionWS, n, f.footprint);
        // grime raises the roughness 0.05 -> 0.20 and scatters a little light (alpha and albedo); rivulets bend the reflection
        // near the eye; both fade to their means by the footprint, no tap at gain 0
        float grime = 0.0;
        float2 slope = 0.0;
        [branch] if (_GrimeGain > 0.0)
        {
            float2 uv = g / 1.5, dx = gdx / 1.5, dy = gdy / 1.5;
            float fade = NocTexFade(1.0, NocFootprintBox(dx, dy));
            grime = lerp(0.136, 0.0, fade);                                         // texmeta coverage mean of slice 1
            [branch] if (fade > 0.0) grime += fade * SAMPLE_TEXTURE2D_ARRAY_GRAD(_OverlayMask, sampler_OverlayMask, uv, 1, dx, dy).r;
            grime *= _GrimeGain;
        }
        [branch] if (_RivuletGain > 0.0 && f.viewDist < NC_SurfGlassRivuletD)
        {
            float near = 1.0 - smoothstep(0.6 * NC_SurfGlassRivuletD, NC_SurfGlassRivuletD, f.viewDist);
            float2 uv = g + float2(0.37, 0.0), dx = gdx, dy = gdy;
            float fade = NocTexFade(1.0, NocFootprintBox(dx, dy)) * near * _RivuletGain;
            [branch] if (fade > 0.0) slope = NocTangentToSlope(SAMPLE_TEXTURE2D_ARRAY_GRAD(_OverlayNormal, sampler_OverlayMask, uv, 2, dx, dy), 1.0) * fade;
        }
        float2 tt = float2(-n.z, n.x);
        float3 Tg = dot(tt, tt) > 1e-6 ? normalize(float3(tt.x, 0.0, tt.y)) : float3(1.0, 0.0, 0.0);
        f.normalWS = NocResolveNormal(n, NocSurfGradTB(slope, Tg, float3(0.0, 1.0, 0.0)));
        float aGlass = saturate(F + (1.0 - F) * 0.25 * grime);
        s.albedo = PAL_GlassDark + NC_SurfGlassGrimeAlb * grime;
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
                float F = f.custom.x;
                float3 r = reflect(-inputData.viewDirectionWS, inputData.normalWS);
                half3 premul;
                [branch] if (f.custom.w > 0.5)
                {
                    float rib = f.custom.y, aGlass = f.custom.z;
                    half3 sky = NocReflection(r, brdf.perceptualRoughness);
                    float4 w = NocWallReflect(f.positionWS, r, max(brdf.perceptualRoughness * brdf.perceptualRoughness, NocPixelAngle()), f.viewDist, sky);
                    half3 env = lerp(sky, w.rgb, w.a);
                    half3 glass = brdf.diffuse * inputData.bakedGI * aGlass + env * F + NocDirect(brdf, inputData);
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
