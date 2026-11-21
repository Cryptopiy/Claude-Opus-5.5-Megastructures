// Megastructure/BwWaterPortalScatter - the light IN the water between the glass and whatever lies beyond it (MASTER_PROMPT
// 14.7 H3 shafts; Addendum A1 1.8 lamps). Beat 6 owner. The same pane quads as BwWaterPortal, drawn additively (Blend One
// One, ZWrite Off, Transparent queue) so the in-scatter also lies over the creature and the reveals:
//   * caustic shafts: 12 fixed Gauss-Legendre samples over [glass, min(scene depth, 60 m)] of the surface caustic C at the
//     point the refracted sun passes through (BwWShaftAt), attenuated by the beam c along the view ray, a forward-scattering
//     lobe toward the sun, footprint-filtered caustics (no shimmer), never jittered;
//   * the survey-lamp cones (A1 1.8: the lamps are spots outside the glass): the same 12 samples of the lamps' in-scatter
//     (Henyey-Greenstein g 0.7), and each lamp's lens and its halo in the water (an energy-preserving Gaussian, >= 2.5 px);
//   * the arm above panes 1-3 (A1 1.8, BwWArmTrace): the in-scatter beyond it fades with its coverage, and its shadow along
//     the refracted sun (BwWArmShadow) puts the shafts out pane by pane while it passes;
//   * the 4 % analytic reflection of the hall on the glass: the dim west wall and the bollard lamp tops (their mirror
//     images), with the A1 1.8 sway (+-0.05 deg at 0.6 Hz while something presses on the glass, _BwWState.z).
// Reads the resolved _CameraDepthTexture (BwWaterPortal wrote the reveal hit or glass + 200 m there).
Shader "Megastructure/BwWaterPortalScatter"
{
    Properties
    {
        _ShaftGain ("Shaft in-scatter per metre", Float) = 0.05
        _LampScatter ("Lamp in-scatter per metre", Float) = 0.12
        _LensRadius ("Lamp lens radius (m)", Float) = 0.45
        _LensGain ("Lamp lens radiance", Float) = 0.25
        _ReflectGain ("Glass reflection", Range(0, 0.1)) = 0.04
        _BollardGlint ("Bollard reflection radiance", Float) = 1.5
        _ShaftSat ("Shaft in-scatter saturation kept (critic S2-5)", Range(0, 1)) = 0.72
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-10" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Scatter"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off ZTest LEqual Cull Back
            Blend One One

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex ScatterVert
            #pragma fragment ScatterFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Include/BulwarkPalette.hlsl"
            #include "Include/BwCommon.hlsl"
            #include "Include/BulwarkAtmosphere.hlsl"
            #include "Generated/BulwarkConstGen.hlsl"
            #include "Include/BwWater.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _ShaftGain;
                float _LampScatter;
                float _LensRadius;
                float _LensGain;
                float _ReflectGain;
                float _BollardGlint;
                float _ShaftSat;
            CBUFFER_END

            static const float GL_X[6] = { 0.12523340851146891, 0.36783149899818018, 0.58731795428661748,
                                           0.76990267419430469, 0.90411725637047491, 0.98156063424671924 };
            static const float GL_W[6] = { 0.24914704581340277, 0.23349253653835481, 0.20316742672306592,
                                           0.16007832854334622, 0.10693932599531843, 0.04717533638651183 };

            struct SA { float4 positionOS : POSITION; };
            struct SV { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float4 screen : TEXCOORD1; };

            SV ScatterVert(SA v)
            {
                SV o;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.screen = ComputeScreenPos(o.positionCS);
                return o;
            }

            float ScatterHG(float c, float g)
            {
                float g2 = g * g;
                return (1.0 - g2) / (4.0 * PI * pow(max(1.0 + g2 - 2.0 * g * c, 1e-4), 1.5));
            }

            // lens of one lamp seen along the ray: an energy-preserving Gaussian of at least 2.5 px and a small glow round it
            // (the beam itself is the in-scatter integral). Every cut is soft: the lens fades in over the first metre beyond
            // the glass and out where the scene (a reveal, the creature) lies in front of it, so no edge can ever show.
            float3 ScatterLens(float3 cam, float3 d, float tMin, float tMax, float4 L0, float4 L1)
            {
                if (L0.w <= 1e-4) return 0.0;
                float ts = dot(L0.xyz - cam, d);
                float vis = smoothstep(tMin, tMin + 1.0, ts) * (1.0 - smoothstep(tMax - 0.5, tMax + 0.5, ts));
                if (vis <= 1e-4) return 0.0;
                float perp = length(cam + d * ts - L0.xyz);
                float pxm = ts * _BwCam.w;
                float r = max(_LensRadius, 2.5 * pxm);
                float keep = (_LensRadius * _LensRadius) / (r * r);
                float facing = 0.12 + 0.88 * pow(saturate(dot(-d, L1.xyz)), 2.0);
                float3 c = float3(TC_BeamCR, TC_BeamCG, TC_BeamCB); // pal:ok (beam attenuation, not a colour)
                float3 Tr = exp(-c * max(ts - tMin, 0.0));
                float core = exp(-(perp * perp) / (r * r)) * keep * facing;
                float glow = 0.02 * facing * exp(-(perp * perp) / (4.0 * r * r + 0.5));
                return _BwWLampCol.rgb * (L0.w * _LensGain * vis) * (core + glow) * Tr;
            }

            half4 ScatterFrag(SV i) : SV_Target
            {
                float3 cam = _WorldSpaceCameraPos;
                float3 G = i.positionWS;
                float3 v = G - cam;
                float dist0 = max(length(v), 1e-3);
                float3 d = v / dist0;
                float dw = max(dot(d, _BwWPlane.xyz), 1e-3);
                float2 uv = i.screen.xy / max(i.screen.w, 1e-6);
                float raw = SampleSceneDepth(uv);
                float eye = LinearEyeDepth(raw, _ZBufferParams);
                float3 fwd = -UNITY_MATRIX_V[2].xyz;
                float sceneDist = eye / max(dot(d, fwd), 1e-3);
                float tGlass = TC_GlassThickness / dw;            // metres of the ray inside the glass
                float t0 = tGlass;
                float t1 = min(sceneDist - dist0, TC_ShaftReach);
                float3 col = 0.0;
                // the arm above panes 1-3: the in-scatter beyond it is hidden by its coverage (the same trace as the portal)
                float tArm;
                float aArm = BwWArmTrace(G, d, t0, min(sceneDist - dist0, 90.0), dist0, tArm);
                if (t1 > t0 + 0.05)
                {
                    float3 Lw = BwWSunInWater();
                    float cosSun = dot(d, -Lw);
                    float lobe = 0.6 + 2.4 * pow(saturate(cosSun), 6.0) + 0.4 * saturate(cosSun);
                    float3 cext = float3(TC_BeamCR, TC_BeamCG, TC_BeamCB); // pal:ok (beam attenuation, not a colour)
                    float tc = _BwWState.w;
                    float half_ = 0.5 * (t1 - t0), mid = 0.5 * (t1 + t0);
                    float3 shaft = 0.0, lamp = 0.0;
                    [unroll] for (int k = 0; k < 6; k++)
                    {
                        [unroll] for (int sgn = 0; sgn < 2; sgn++)
                        {
                            float t = mid + (sgn == 0 ? -1.0 : 1.0) * GL_X[k] * half_;
                            float w = GL_W[k] * half_;
                            float3 p = G + d * t;
                            float3 Tr = exp(-cext * (t - t0)) * (t > tArm ? 1.0 - aArm : 1.0);
                            float depthP = max(BWW_SEA_Y - p.y, 0.0);
                            float slant = depthP / max(-Lw.y, 0.2);
                            float fp = (dist0 + t + slant) * _BwCam.w * 1.5 + 0.02;
                            float3 down = BwWVeil(float3(0, 1, 0), depthP) * 0.0743; // pal:ok (direction; e^-2.6 brings the zenith veil to the horizontal level)
                            shaft += w * Tr * down * BwWShaftAt(p, Lw, tc, fp) / max(exp(-0.5 * TC_KdG * depthP), 1e-3) * BwWArmShadow(p, Lw);
                            float3 la = BwWLampAt(p, _BwWLampA0, _BwWLampA1) * (4.0 * PI) * ScatterHG(dot(d, normalize(p - _BwWLampA0.xyz)), 0.7);
                            float3 lb = BwWLampAt(p, _BwWLampB0, _BwWLampB1) * (4.0 * PI) * ScatterHG(dot(d, normalize(p - _BwWLampB0.xyz)), 0.7);
                            lamp += w * Tr * (la + lb);
                        }
                    }
                    // critic S2-5 (aquarium blue): the shafts' in-scatter is desaturated 28 % toward its own luminance (the
                    // particles scatter all wavelengths alike; the veil's blue comes from the long water path, not from them),
                    // luma unchanged; the veil's spectral weights themselves are the shared 14.7 H3 constants (request)
                    float3 shaftL = shaft * (_ShaftGain * lobe);
                    col += lerp(dot(shaftL, float3(0.2126, 0.7152, 0.0722)).xxx, shaftL, _ShaftSat); // pal:ok (Rec.709 luma weights, not a colour)
                    // the shaft tops turn amber near the end of the hall (14.5.6, s 1340-1370)
                    col *= lerp(float3(1, 1, 1), PAL_DuskAmber / max(PAL_DuskAmber.g, 1e-3) * 0.8, _BwWState.y); // pal:ok (neutral factor)
                    col += lamp * _LampScatter * _BwWLampCol.rgb;
                }
                float tMax = sceneDist - dist0;
                col += ScatterLens(cam, d, dist0 + tGlass, dist0 + tMax, _BwWLampA0, _BwWLampA1);
                col += ScatterLens(cam, d, dist0 + tGlass, dist0 + tMax, _BwWLampB0, _BwWLampB1);

                // 4 % reflection of the hall: the ray mirrored in the glass plane runs back to the west wall (lateral -5.5)
                float3 Rn = _BwWPlane.xyz;
                float3 r = reflect(d, -Rn);
                float sw = _BwWState.z;
                r = normalize(r + _BwWTan.xyz * sw);
                float rl = dot(r, Rn);
                float3 refl = 0.0;
                if (rl < -1e-3)
                {
                    float tw = (BwWLateral(G) - TC_HallWestWall) / -rl;
                    float3 H = G + r * tw;
                    float sh = BwWHallS(H);
                    refl += PAL_ConcreteMid * _BwAmbInt.rgb * 0.6;
                    // the nearest bollard lamp top (0.9 m, every 20 m from s 1080): a soft spot of >= 2.5 px
                    float j = clamp(round((sh - 1080.0) / TC_BollardLampPitch), 0.0, 14.0);
                    float sj = 1080.0 + TC_BollardLampPitch * j;
                    float yj = BwWFloorY(sj) + TC_BollardLampHeight - 0.05;
                    float pxm = (dist0 + tw) * _BwCam.w;
                    float rad = max(0.10, 2.5 * pxm);
                    float keep = 0.01 / (rad * rad);
                    float2 dd = float2(sh - sj, H.y - yj);
                    refl += PAL_Tungsten * (_BollardGlint * keep * exp(-dot(dd, dd) / (rad * rad)));
                }
                col += refl * _ReflectGain;
                return half4(max(col, 0.0), 0.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
