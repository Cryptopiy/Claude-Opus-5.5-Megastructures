// NocAlleyRoom - the modelled interiors of beat 1 (MASTER_PROMPT 14.5.1 NoodleBar; Phase 4 wave B, AL: INT-07 the noodle bar,
// docs/DREAD.md 6 the empty rooms near the path). Real geometry (C#, NoodleBar / AlleyRooms) carries its albedo in the vertex
// colour (rgb, linear) and a surface kind in alpha (kind x 32 / 255):
//   0 matte (plaster, painted wood), 1 wall tiles (10 cm square, 3 mm grout, faded to the mean below 3 px), 2 wood (planks 12 cm,
//   grain), 3 self-lit (lamp shades, bulbs, the paper lantern: albedo x _SelfLit), 4 a light-box (menus, the blade signs: a lit
//   board with dark hashed glyph strokes in 0.20 x 0.27 uv cells, stroke 0.03: no real text; faded to the mean below 2-4 px of
//   stroke, the footprint from the uv derivatives), 5 steel (pots, the kitchen line),
//   6 fabric (curtains, the noren: soft folds), 7 glazed ceramic (bowls, cups).
// UV0 is metres (G1Mesh.Quad panel-local or MeshKit planar per facet). The room is lit by the lit template (ambient, the alley's
// realtime lights) and by up to two analytic lamps of its own (material _Lamp0 / _Lamp1: position xyz, intensity w; colours
// _Lamp0Color / _Lamp1Color): E = I max(N.L, 0) / (d^2 + 0.3) on the albedo, added as emission, so a room needs no realtime light
// (the 14.2.3 cap of 20 holds). Analytic, filtered, no keyword; derivatives before any branch.
// Fix wave C37 (KS01 over its luma window: the empty room was the flattest, brightest surface of the frame): an ENCLOSED room
// (_Interior 1, the two empty rooms of DREAD 6) is lit only by its own lamp: the template's sky ambient (x _InteriorAmb, the little
// that comes in through the window) and none of the alley's realtime lights (no shadow maps: they lit the back wall through the
// alley wall, evenly, grey), plus the lamp's GGX highlight on the table, the cup and the floor. Its matte surfaces (kind 0) take
// the CC0 plaster (texprep set Plaster, polyhaven plastered_wall_04: an albedo-variation map of mean 0.5, x 2) and low-frequency
// stains in world space: tide marks under the ceiling, a grime band over the floor, damp blotches (1-3 m, mean-preserving).
// The open rooms (_Interior 0: the noodle bar, the light-box signs) are shaded by the template exactly as before.
Shader "Megastructure/NocAlleyRoom"
{
    Properties
    {
        _Lamp0 ("Lamp 0 (xyz world, w intensity)", Vector) = (0, 2.5, 0, 0)
        [HDR] _Lamp0Color ("Lamp 0 colour (linear)", Color) = (1, 0.7, 0.42, 1)
        _Lamp1 ("Lamp 1 (xyz world, w intensity)", Vector) = (0, 2.5, 0, 0)
        [HDR] _Lamp1Color ("Lamp 1 colour (linear)", Color) = (1, 0.7, 0.42, 1)
        _SelfLit ("Self-lit gain (kind 3, 4)", Float) = 4
        _Fill ("Room fill (x albedo, linear)", Float) = 0.02
        _Interior ("Enclosed room (1: own lamps only)", Float) = 0
        _InteriorAmb ("Enclosed room: share of the sky ambient", Float) = 0.1
        [NoScaleOffset] _WallTex ("Plaster albedo variation (CC0, mean 0.5)", 2D) = "grey" {}
        _WallTexOn ("Plaster on the matte surfaces", Float) = 0
        _WallTile ("Plaster tile (m)", Float) = 3.2
        _Stain ("Stain strength", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "../Include/NocLitCore.hlsl"
    #include "../Include/NocTexturing.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _Lamp0;
        float4 _Lamp0Color;
        float4 _Lamp1;
        float4 _Lamp1Color;
        float _SelfLit;
        float _Fill;
        float _Interior;
        float _InteriorAmb;
        float _WallTexOn;
        float _WallTile;
        float _Stain;
        float _Cull;
    CBUFFER_END
    TEXTURE2D(_WallTex);
    SAMPLER(sampler_WallTex);

    float RmStep(float edge, float x, float fp) { return saturate((x - edge) / max(fp, 1e-5) + 0.5); }

    float3 RmLamp(float4 lamp, float3 col, float3 P, float3 n)
    {
        float3 L = lamp.xyz - P;
        float d2 = dot(L, L);
        float ndl = max(dot(n, L * rsqrt(max(d2, 1e-4))), 0.0);
        return col * (lamp.w * ndl / (d2 + 0.3));
    }

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        float3 P = f.positionWS;
        float3 n = f.normalWS;
        float2 uv = f.uv.xy;
        float fp = max(f.footprint, 1e-5);
        float3 alb = f.color.rgb;
        int kind = (int)(f.color.a * 255.0 / 32.0 + 0.5);
        float smooth = 0.3;
        float metal = 0.0;
        float selfLit = 0.0;
        // patterns (all evaluated without branches on derivatives: fp is the template's footprint)
        float tiles = max(NocFilteredGrid1(uv.x, 0.10, 0.003, fp, 3.0), NocFilteredGrid1(uv.y, 0.10, 0.003, fp, 3.0));
        uint th = NocPcg2((int)floor(uv.x / 0.10), (int)floor(uv.y / 0.10) + 977);
        float tileVar = lerp(1.0, 0.96 + 0.08 * NocU01(th), smoothstep(2.0, 4.0, 0.10 / fp));
        float plank = NocFilteredGrid1(uv.x, 0.12, 0.004, fp, 3.0);
        float grain = lerp(0.5, NocValueNoise(float2(uv.x * 40.0, uv.y * 2.5)), smoothstep(2.0, 4.0, 0.03 / fp));
        float fold = 1.0 + 0.3 * sin(6.28318 * uv.x / 0.09) * smoothstep(2.0, 4.0, 0.09 / fp);
        // light-box glyphs: cells 0.20 x 0.27 (uv units: metres on the menus, scaled to one column on the blade signs), 2-4 strokes on
        // a 3 x 4 lattice, stroke 0.03; the footprint in uv units from the uv derivatives (before any branch)
        float fpUV = max(max(length(ddx(uv)), length(ddy(uv))), 1e-5);
        const float CW = 0.20, CH = 0.27, SW = 0.03;
        float2 cell = float2(uv.x / CW, uv.y / CH);
        float2 ci = floor(cell), cf = frac(cell);
        uint gh = NocPcg2((int)ci.x * 31 + 7, (int)ci.y * 17 + 3);
        float g = 0.0;
        [unroll] for (int k = 0; k < 4; k++)
        {
            uint hk = NocPcg(gh + (uint)k * 0x9E37u);
            float on = step(0.35, NocU01(hk));
            float vert = step(0.5, NocU01(hk + 11u));
            float a = 0.2 + 0.6 * floor(NocU01(hk + 23u) * 3.0) / 2.0;          // lattice line 0.2 / 0.5 / 0.8
            float b0 = 0.15 + 0.35 * NocU01(hk + 37u), b1 = b0 + 0.3 + 0.2 * NocU01(hk + 41u);
            float2 q = vert > 0.5 ? cf.xy : cf.yx;
            float wv = vert > 0.5 ? SW / CW : SW / CH;
            float fpc = vert > 0.5 ? fpUV / CW : fpUV / CH;
            float st = (1.0 - RmStep(wv, abs(q.x - a), fpc)) * RmStep(b0, q.y, fpc) * (1.0 - RmStep(min(b1, 0.9), q.y, fpc));
            g = max(g, st * on);
        }
        float glyphKeep = smoothstep(1.2, 2.5, SW / fpUV);   // the strokes are box filtered (RmStep): kept down to 1.2 px
        float glyph = lerp(0.16, g, glyphKeep);
        // C37: the CC0 plaster (albedo variation, mean 0.5 -> x 2; hardware mips and anisotropy from the uv derivatives, sampled here
        // outside any branch) and the stains of an abandoned room, in world space so they run across the room's faces:
        //   damp blotches  1 - 0.22 smoothstep(0.55, 0.8, noise at 1.6 m) (mean ~0.95),
        //   tide marks     under the ceiling (y > 2.0): a brown band whose lower edge wanders with a 0.9 m noise,
        //   grime          over the floor (y < 0.35): -18 % fading up.
        float3 plaster = 2.0 * SAMPLE_TEXTURE2D(_WallTex, sampler_WallTex, uv / max(_WallTile, 0.1)).rgb;
        float wz = P.x + P.z;
        float blot = smoothstep(0.55, 0.8, NocValueNoise(float2(wz, P.y) / 1.6 + 13.7));
        float tideEdge = 2.05 + 0.35 * NocValueNoise(float2(wz / 0.9, 3.1));
        float tide = smoothstep(tideEdge - 0.06, tideEdge + 0.10, P.y) * step(abs(n.y), 0.5);
        float grime = 1.0 - smoothstep(0.0, 0.35, P.y);
        float3 stainK = (1.0 - 0.22 * blot) * lerp(1.0, float3(0.72, 0.64, 0.52), 0.8 * tide) * (1.0 - 0.18 * grime);

        [branch] if (kind == 0)
        {
            alb *= lerp(1.0, plaster * lerp(1.0, stainK, _Stain), _WallTexOn);
        }
        else [branch] if (kind == 1)
        {
            alb *= tileVar * (1.0 - 0.7 * tiles);
            smooth = lerp(0.75, 0.3, tiles);
        }
        else [branch] if (kind == 2)
        {
            alb *= (0.8 + 0.4 * grain) * (1.0 - 0.5 * plank);
            smooth = 0.45;
        }
        else [branch] if (kind == 3)
        {
            selfLit = 1.0;
            smooth = 0.3;
        }
        else [branch] if (kind == 4)
        {
            selfLit = 1.0 - 0.85 * glyph;
            smooth = 0.6;
        }
        else [branch] if (kind == 5)
        {
            smooth = 0.65;
            metal = 0.6;
        }
        else [branch] if (kind == 6)
        {
            alb *= fold;
            smooth = 0.15;
        }
        else [branch] if (kind == 7)
        {
            smooth = 0.85;
        }
        float3 E = RmLamp(_Lamp0, _Lamp0Color.rgb, P, n) + RmLamp(_Lamp1, _Lamp1Color.rgb, P, n);
        // fabric and paper pass some lamp light through (translucent, both faces lit)
        float3 Eb = RmLamp(_Lamp0, _Lamp0Color.rgb, P, -n) + RmLamp(_Lamp1, _Lamp1Color.rgb, P, -n);
        E += (kind == 6 ? 0.35 : 0.0) * Eb;
        s.albedo = alb;
        s.metallic = metal;
        s.smoothness = smooth;
        s.occlusion = 1.0;
        s.emission = alb * (E + _Fill) * (1.0 - selfLit) + alb * (_SelfLit * selfLit);
        [branch] if (NocDebugView() == 8) f.debug = float4(frac(uv / 0.25), 0.5, 1.0);
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
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #define NOC_PASS_FORWARD
            #define NOC_HAS_FORWARD_HOOK
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Include/NocLighting.hlsl"

            // one lamp's GGX highlight (URP's DirectBRDFSpecular), radiance as RmLamp: I / (d^2 + 0.3)
            half3 RmLampSpec(BRDFData brdf, float4 lamp, float3 col, float3 P, float3 n, float3 V)
            {
                float3 L = lamp.xyz - P;
                float d2 = dot(L, L);
                L *= rsqrt(max(d2, 1e-4));
                float ndl = saturate(dot(n, L));
                return brdf.specular * DirectBRDFSpecular(brdf, n, L, V) * ndl * col * (lamp.w / (d2 + 0.3));
            }

            half3 NocForwardHook(NocFragData f, SurfaceData s, InputData inputData)
            {
                half3 c = 0.0;
                [branch] if (_Interior < 0.5)
                {
                    c = NocShadeOcc(inputData, s, f.specOcc);                  // the bar and the signs: the template, unchanged
                }
                else
                {
                    // the enclosed room: its lamps (emission), a share of the sky ambient through the window, the lamps' highlight;
                    // no alley light reaches inside (the template has no shadow maps, so a wall would not stop it)
                    BRDFData brdf;
                    InitializeBRDFData(s, brdf);
                    c = inputData.bakedGI * brdf.diffuse * (_InteriorAmb * s.occlusion) + s.emission
                      + RmLampSpec(brdf, _Lamp0, _Lamp0Color.rgb, f.positionWS, inputData.normalWS, inputData.viewDirectionWS)
                      + RmLampSpec(brdf, _Lamp1, _Lamp1Color.rgb, f.positionWS, inputData.normalWS, inputData.viewDirectionWS);
                }
                return min(c, 1000.0);
            }

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
            #include "../Include/NocLitPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
