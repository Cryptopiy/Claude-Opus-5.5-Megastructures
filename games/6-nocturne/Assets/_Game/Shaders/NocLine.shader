// Megastructure/NocLine - thin lines with a screen-space minimum width (14.2.5 NocLine: cables, rails, rods), imported from
// Thread's ThreadLine.shader (IMPORT_PLAN 7.14). Kept: the tube vertex push (ThLineTubeVertex), the camera-facing ribbon with
// the exact homogeneous image line (ThLineRibbonVertex, ThLineRibbonCoverage), the alpha-to-coverage energy path and the
// minimum width max(1.5, 1.75 H / 1080) px (_NocScreen.y, written by ProgressDirector for every camera) with an energy floor
// of 0.35 (alpha = max(0.35, true / drawn)). Stripped (Thread content): ThreadCommon / ThreadPatterns / ThreadFlatPasses,
// ThreadShadeFlat, the analytic cable shadow, the far-below fades and haze, the main-light shadow keywords (no shadow maps).
// Colour: the albedo is lit by NocLighting (trilight + sky reflection + Forward+ lights) plus emission, then NocFog and NocFloor.
// _LINE_TUBE (default): opaque 8-gon tube, A2C (needs _AlphaToMask = 1 and MSAA). _LINE_RIBBON: premultiplied ribbon
// (Blend One OneMinusSrcAlpha, ZWrite Off, Transparent queue, Cull Off); the mesh gives centre-line vertices with uv1.y = side
// (-1 / +1) and the line direction in the normal.
Shader "Megastructure/NocLine"
{
    Properties
    {
        _BaseColor ("Base colour (linear albedo)", Color) = (0.0232, 0.0232, 0.0395, 1)
        [HDR] _EmissionColor ("Emission (linear HDR)", Color) = (0, 0, 0, 1)
        _Smoothness ("Smoothness", Range(0, 0.95)) = 0.4
        _LineWidth ("True width (m)", Float) = 0.03
        _MinWidthPx ("Minimum width (px, 0 = global)", Float) = 0
        _AlphaFloor ("Energy alpha floor", Float) = 0.35
        _EmissionDown ("Emission on the underside only (0 all round, 1 the lower facets: an LED channel)", Float) = 0

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst blend", Float) = 0
        [ToggleUI] _ZWrite ("ZWrite", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
        [ToggleUI] _AlphaToMask ("Alpha to coverage (tube with MSAA)", Float) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" "IgnoreProjector" = "True" }

        HLSLINCLUDE
        #define _ENVIRONMENTREFLECTIONS_OFF 1
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _EmissionColor;
            float _Smoothness;
            float _LineWidth;
            float _MinWidthPx;
            float _AlphaFloor;
            float _EmissionDown;
        CBUFFER_END

        #include "Include/NocAtmosphere.hlsl"
        #include "Generated/NocSurfaceConstantsGen.hlsl"   // Look v2 (SU): NC_SurfLine*

        #define NL_REF_HEIGHT 1080.0

        struct NocLineAttributes
        {
            float4 positionOS : POSITION;
            float3 normalOS   : NORMAL;      // tube: outward smooth normal; ribbon: line direction
            float4 color      : COLOR;
            float4 uv1        : TEXCOORD1;   // ribbon: y = side (-1 / +1)
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct NocLineVaryings
        {
            float4 positionCS        : SV_POSITION;
            float3 positionWS        : TEXCOORD0;
            centroid float3 normalWS : TEXCOORD1;
            float4 lineData          : TEXCOORD2;   // tube: x = energy alpha; ribbon: xyz = image line (pixel units)
            float2 gw                : TEXCOORD3;   // ribbon: (g, w) with d = g / w the signed pixel distance
            float4 color             : TEXCOORD4;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        float NocLineMinWidthPx()
        {
            float m = (_MinWidthPx > 0.0) ? _MinWidthPx : _NocScreen.y;
            if (m < 0.5) m = max(NC_LineMinWidthFloorPx, NC_LineMinWidthPx * GetScaledScreenParams().y / NL_REF_HEIGHT);
            return m;
        }

        float NocLineAlphaFloor() { return (_AlphaFloor > 0.0) ? _AlphaFloor : NC_LineEnergyFloor; }

        // Pixels per metre at unit clip w.
        float NocLineFocalPx() { return abs(UNITY_MATRIX_P._m11) * 0.5 * GetScaledScreenParams().y; }

        // Tube: radial push of every vertex about its centre line so the projected diameter is >= w_min.
        void NocLineTubeVertex(NocLineAttributes v, inout NocLineVaryings o)
        {
            float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
            float3 nWS = TransformObjectToWorldNormal(v.normalOS);
            float radius = 0.5 * _LineWidth;
            float3 axisWS = posWS - nWS * radius;
            float4 axisCS = TransformWorldToHClip(axisWS);
            float minPx = NocLineMinWidthPx();
            float trueDiamPx = _LineWidth * NocLineFocalPx() / max(axisCS.w, 1e-4);
            float inflate = max(1.0, minPx / max(trueDiamPx, 1e-6));
            posWS = axisWS + nWS * (radius * inflate);
            o.positionCS = TransformWorldToHClip(posWS);
            o.positionWS = posWS;
            o.normalWS = nWS;
            o.lineData = float4(max(NocLineAlphaFloor(), saturate(trueDiamPx / minPx)), 0.0, 0.0, 0.0); // pal:ok (energy alpha)
            o.gw = float2(0.0, 1.0);
        }

        // Ribbon: screen-space expansion perpendicular to the projected line (exact homogeneous image line).
        void NocLineRibbonVertex(NocLineAttributes v, inout NocLineVaryings o)
        {
            float3 pWS = TransformObjectToWorld(v.positionOS.xyz);
            float3 dirWS = TransformObjectToWorldDir(v.normalOS);
            float2 S = GetScaledScreenParams().xy;
            float3 cam = _WorldSpaceCameraPos;
            float3 pRef = pWS - dirWS * dot(pWS - cam, dirWS);
            float4 c0 = TransformWorldToHClip(pRef);
            float4 dc = mul(GetWorldToHClipMatrix(), float4(dirWS, 0.0));  // pal:ok (direction, w = 0)
            float3 L = cross(c0.xyw, dc.xyw);
            float3 Lpx = float3(L.x * 2.0 / S.x, L.y * 2.0 / S.y, L.z);     // pal:ok (line coefficients)
            float len = length(Lpx.xy);
            float4 c = TransformWorldToHClip(pWS);
            if (len < 1e-10)
            {
                o.positionCS = c; o.positionWS = pWS; o.normalWS = dirWS; o.lineData = 0.0; o.gw = float2(1e6, 1.0);
                return;
            }
            Lpx /= len;
            float trueHalfPx = (c.w > 1e-4) ? 0.5 * _LineWidth * NocLineFocalPx() / c.w : 0.0;
            float extentPx = max(trueHalfPx, 0.5 * NocLineMinWidthPx()) + 1.0;     // + 1 px skirt for the AA ramp
            float side = (v.uv1.y >= 0.0) ? 1.0 : -1.0;
            c.xy += Lpx.xy * (side * extentPx) * (2.0 / S) * c.w;
            o.positionCS = c;
            o.positionWS = pWS;
            o.normalWS = dirWS;
            o.lineData = float4(Lpx, 0.0);                                      // pal:ok (line coefficients)
            o.gw = float2(dot(Lpx, float3(c.xy * (0.5 * S), c.w)), c.w);         // pal:ok (homogeneous point)
        }

        NocLineVaryings NocLineVert(NocLineAttributes v)
        {
            NocLineVaryings o = (NocLineVaryings)0;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        #if defined(_LINE_RIBBON)
            NocLineRibbonVertex(v, o);
        #else
            NocLineTubeVertex(v, o);
        #endif
            o.color = v.color;
            return o;
        }

        void NocLineRibbonCoverage(NocLineVaryings i, out float d, out float halfPx, out float alpha)
        {
            float w = max(i.gw.y, 1e-6);
            d = i.gw.x / w;
            float trueHalf = 0.5 * _LineWidth * NocLineFocalPx() / w;
            float minHalf = 0.5 * NocLineMinWidthPx();
            halfPx = max(trueHalf, minHalf);
            float cov = saturate(0.5 + halfPx - abs(d));
            float energy = max(NocLineAlphaFloor(), saturate(trueHalf / minHalf));
            alpha = cov * energy;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_Cull]
            AlphaToMask [_AlphaToMask]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocLineVert
            #pragma fragment NocLineForwardFrag
            #pragma shader_feature_local _ _LINE_TUBE _LINE_RIBBON
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Include/NocLighting.hlsl"

            // Look v2 paint variation (-surf-v 2; TEXTURE_PLAN 3.5, SU-3): posts and rails of 0.04-0.20 m are seen from 1-3 m, so the
            // flat token reads as plastic. Two value-noise octaves at SurfLineScale (0.35 m) on a 2D projection of the world position
            // that varies along any axis-aligned tube (vertical posts: y; rails along x or z), albedo x (1 +- SurfLineAmp); sparse lighter
            // chips (SurfLineChipP of SurfLineChipLen cells, worn primer x1.6, smoother); a darker wet splash band over the lowest
            // SurfLineSplash m above the walk plane y 0 (every walkable surface, 14.4). Each detail fades to its mean (x1) below
            // SurfLineFadePx..2x pixels of its own scale, so nothing shimmers. Twin: NocSurfaceMath.LinePaint.
            float3 NocLinePaint(float3 albedo, float3 P, float fp, inout float smooth)
            {
                float2 q = float2(P.y + 0.37 * P.x, P.z - 0.61 * P.x);
                float wN = smoothstep(NC_SurfLineFadePx, 2.0 * NC_SurfLineFadePx, NC_SurfLineScale / fp);
                float n = 0.65 * NocValueNoise(q / NC_SurfLineScale + 3.7) + 0.35 * NocValueNoise(q * (3.1 / NC_SurfLineScale) + 11.3);
                float k = 1.0 + NC_SurfLineAmp * (2.0 * n - 1.0) * wN;
                float wC = smoothstep(NC_SurfLineFadePx, 2.0 * NC_SurfLineFadePx, NC_SurfLineChipLen / fp);
                int2 c = int2(floor(q / NC_SurfLineChipLen));
                float chip = NocU01(NocPcg2(c.x + 4099, c.y - 7919)) < NC_SurfLineChipP ? 1.0 : 0.0;
                float chipW = lerp(NC_SurfLineChipP, chip, wC);                           // the chip's mean coverage below its fade
                k *= 1.0 + 0.6 * chipW;
                smooth = lerp(smooth, min(smooth + 0.15, 0.95), chipW);
                float splash = 1.0 - smoothstep(0.0, NC_SurfLineSplash, P.y);
                k *= lerp(1.0, NC_SurfLineSplashDark, splash);
                return albedo * k;
            }

            float3 NocLineShade(float3 positionWS, float3 normalWS, float4 positionCS)
            {
                SurfaceData s = (SurfaceData)0;
                s.albedo = _BaseColor.rgb;
                float smooth = min(_Smoothness, 0.95);
                // FX TP-2 (REQUEST AB, done by AB): the v2 surface paint is the only path (the NocSurfV2() test is gone)
                float fp = max(length(positionWS - _WorldSpaceCameraPos) * 2.0 / max(abs(UNITY_MATRIX_P._m11) * GetScaledScreenParams().y, 1e-4), 1e-5);
                s.albedo = NocLinePaint(s.albedo, positionWS, fp, smooth);
                s.alpha = 1.0;
                s.smoothness = smooth;
                s.occlusion = 1.0;
                // AB (KS09 / KS10 r02: the lit handrail read as a 50 px flat clipped bar across the frame): with _EmissionDown 1 only the
                // lower half glows (n.y below -0.05, full below -0.55: the LED channel in a handrail's underside, aimed down and inward),
                // so the walker sees a dark rail with a lit sliver along its lower edge (about a third of the tube's width from 2-3 m);
                // 0 keeps the all-round emission of every other line
                float down = 1.0 - smoothstep(-0.55, -0.05, normalWS.y);
                s.emission = _EmissionColor.rgb * lerp(1.0, down, saturate(_EmissionDown));
                s.normalTS = half3(0, 0, 1);
                InputData inputData = (InputData)0;
                inputData.positionWS = positionWS;
                inputData.positionCS = positionCS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = SafeNormalize(_WorldSpaceCameraPos - positionWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);
                inputData.bakedGI = NocAmbient(normalWS);
                float3 c = NocShade(inputData, s);
                return NocFloor(NocFog(c, positionWS));
            }

            float4 NocLineForwardFrag(NocLineVaryings i, FRONT_FACE_TYPE isFront : FRONT_FACE_SEMANTIC) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #if defined(_LINE_RIBBON)
                float d, halfPx, alpha;
                NocLineRibbonCoverage(i, d, halfPx, alpha);
                clip(alpha - 1e-4);
                // cylinder normal across the line (side vector perpendicular to the line and the view ray)
                float3 dirWS = SafeNormalize(i.normalWS);
                float3 toCam = SafeNormalize(_WorldSpaceCameraPos - i.positionWS);
                float3 sideWS = cross(dirWS, toCam);
                float sl2 = dot(sideWS, sideWS);
                sideWS = (sl2 > 1e-10) ? sideWS * rsqrt(sl2) : SafeNormalize(cross(dirWS, float3(dirWS.y, dirWS.z, dirWS.x))); // pal:ok
                float4 cs = mul(GetWorldToHClipMatrix(), float4(sideWS, 0.0));      // pal:ok (direction)
                float4 cp = mul(GetWorldToHClipMatrix(), float4(i.positionWS, 1.0)); // pal:ok (point)
                float2 dq = (cs.xy * cp.w - cp.xy * cs.w) * (0.5 * GetScaledScreenParams().xy);
                sideWS *= (dot(i.lineData.xy, dq) >= 0.0) ? 1.0 : -1.0;
                float u = clamp(d / halfPx, -1.0, 1.0);
                float3 n = SafeNormalize(u * sideWS + sqrt(saturate(1.0 - u * u)) * toCam);
                float3 color = NocLineShade(i.positionWS, n, i.positionCS);
                return float4(color * alpha, alpha);                                 // pal:ok (premultiplied)
            #else
                if (!isFront) clip(-1.0);   // closed tube: back faces never contribute (keeps A2C coverage exact)
                float3 color = NocLineShade(i.positionWS, SafeNormalize(i.normalWS), i.positionCS);
                return float4(color, i.lineData.x);                                  // pal:ok (A2C energy alpha)
            #endif
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocLineVert
            #pragma fragment NocLineDepthFrag
            #pragma shader_feature_local _ _LINE_TUBE _LINE_RIBBON
            #pragma multi_compile_instancing

            half NocLineDepthFrag(NocLineVaryings i, FRONT_FACE_TYPE isFront : FRONT_FACE_SEMANTIC) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
            #if defined(_LINE_RIBBON)
                clip(-1.0);   // blended ribbons never write depth
            #else
                if (!isFront) clip(-1.0);
            #endif
                return i.positionCS.z;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
