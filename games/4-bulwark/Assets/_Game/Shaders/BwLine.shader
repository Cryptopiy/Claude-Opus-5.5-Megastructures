// Megastructure/BwLine - thin-line material with a screen-space minimum width (MASTER_PROMPT 14.7 H7, D-07).
// Imported from Thread's ThreadLine.shader (the width maths are unchanged); Thread's flat shading, fog, cable shadow,
// below-fade/haze, shade colour and helix logic are stripped; lighting is the BwLit model (UniversalFragmentPBR mirror,
// BwLighting.hlsl) and fog is BwFogApply (BulwarkAtmosphere.hlsl).
// Minimum full width w_min = _MinWidthPx, or the global _BwScreen.z = max(1.5, 1.75 H / 1080) px when 0 (written per
// camera by ProgressDirector). Energy preservation: when the true width is below w_min, alpha = max(_AlphaFloor 0.35,
// true / w_min).
//
// _LINE_TUBE   (rails, cables; D-07: smooth round tubes with a vertex ring at least every 4 m) an n-gon tube mesh with
//              outward smooth normals; each vertex is pushed radially so the projected diameter is >= w_min. Opaque;
//              the energy alpha drives alpha-to-coverage, which needs _AlphaToMask = 1 and MSAA > 1. Back faces are
//              discarded, so _Cull may stay Off. Passes: ForwardLit, DepthOnly, DepthNormals (no ShadowCaster).
// _LINE_RIBBON (very far lines) camera-facing ribbon: centre-line vertices with uv1.y = side (-1 / +1) and the line
//              direction in normal.xyz; the vertex stage expands perpendicular to the projected line to
//              max(true width, w_min) + a 1 px skirt; the fragment computes the exact signed pixel distance to the
//              homogeneous image line, writes coverage saturate(0.5 + halfWidthPx - |d|) and outputs premultiplied
//              colour (Blend One OneMinusSrcAlpha, ZWrite Off, Transparent queue, Cull Off).
// Distance fade (_FadeNear/_FadeFar, off when _FadeFar <= 0): by alpha-to-coverage when available, otherwise by the
// static blue-noise threshold (D-08), never an ordered dither.
Shader "Megastructure/BwLine"
{
    Properties
    {
        _BaseColor ("Base colour (linear albedo)", Color) = (1, 1, 1, 1)
        _Smoothness ("Smoothness", Range(0, 0.9)) = 0.35
        _Metallic ("Metallic", Range(0, 1)) = 0
        _LineWidth ("True width (m)", Float) = 0.05
        _MinWidthPx ("Minimum width (px, 0 = global)", Float) = 0
        _AlphaFloor ("Energy alpha floor", Float) = 0.35
        _FadeNear ("Distance fade near (m, tube)", Float) = 0
        _FadeFar ("Distance fade far (m, 0 = off, tube)", Float) = 0

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst blend", Float) = 0
        [ToggleUI] _ZWrite ("ZWrite", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
        [ToggleUI] _AlphaToMask ("Alpha to coverage (tube with MSAA)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" "IgnoreProjector" = "True" }
        LOD 300

        HLSLINCLUDE
        #define _ENVIRONMENTREFLECTIONS_OFF 1
        #define _SCREENSPACEREFLECTIONS_OFF 1
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float _Smoothness;
            float _Metallic;
            float _LineWidth;
            float _MinWidthPx;
            float _AlphaFloor;
            float _FadeNear;
            float _FadeFar;
        CBUFFER_END

        #include "Include/BwCommon.hlsl"
        #include "Include/BulwarkAtmosphere.hlsl"

        #define BL_REF_HEIGHT 1080.0   // reference render height of the width rule (14.7 H7)
        #define BL_MIN_WIDTH_PX 1.75   // w_min = max(1.5, 1.75 H / 1080)
        #define BL_MIN_WIDTH_FLOOR_PX 1.5
        #define BL_ALPHA_FLOOR 0.35
        #define BL_FADE_A2C_MIN 0.002  // below this keep factor the A2C mask is empty anyway: discard

        bool BwLineFadeOn() { return _FadeFar > 0.0; }
        float BwLineFadeKeep(float3 positionWS)
        {
            float d = distance(positionWS, _WorldSpaceCameraPos);
            return 1.0 - smoothstep(_FadeNear, max(_FadeFar, _FadeNear + 1e-3), d);
        }

        struct BwLineAttributes
        {
            float4 positionOS : POSITION;
            float3 normalOS   : NORMAL;      // tube: outward smooth normal; ribbon: line direction
            float4 color      : COLOR;
            float4 uv1        : TEXCOORD1;   // ribbon: y = side (-1 / +1)
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct BwLineVaryings
        {
            float4 positionCS        : SV_POSITION;
            float3 positionWS        : TEXCOORD0;   // tube: surface point; ribbon: centre-line point
            centroid float3 normalWS : TEXCOORD1;   // tube: surface normal; ribbon: line direction
            float4 lineData          : TEXCOORD2;   // tube: x = energy alpha; ribbon: xyz = image line (pixel units)
            float2 gw                : TEXCOORD3;   // ribbon: (g, w) with d = g / w the signed pixel distance (perspective-correct: exact)
            float4 color             : TEXCOORD4;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        // Minimum full width in pixels: material override, else the global (ProgressDirector), else the rule.
        float BwLineMinWidthPx()
        {
            float m = (_MinWidthPx > 0.0) ? _MinWidthPx : _BwScreen.z;
            if (m < 0.5)
                m = max(BL_MIN_WIDTH_FLOOR_PX, BL_MIN_WIDTH_PX * GetScaledScreenParams().y / BL_REF_HEIGHT);
            return m;
        }

        float BwLineAlphaFloor() { return (_AlphaFloor > 0.0) ? _AlphaFloor : BL_ALPHA_FLOOR; }

        // Pixels per metre at unit clip w (perspective: w = view depth).
        float BwLineFocalPx() { return abs(UNITY_MATRIX_P._m11) * 0.5 * GetScaledScreenParams().y; }

        // Tube: radial push of every vertex about its centre line so the projected diameter is >= w_min.
        void BwLineTubeVertex(BwLineAttributes v, inout BwLineVaryings o)
        {
            float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
            float3 nWS = TransformObjectToWorldNormal(v.normalOS);
            float radius = 0.5 * _LineWidth;
            float3 axisWS = posWS - nWS * radius;                 // the ring vertex lies on radius _LineWidth / 2
            float4 axisCS = TransformWorldToHClip(axisWS);
            float minPx = BwLineMinWidthPx();
            float trueDiamPx = _LineWidth * BwLineFocalPx() / max(axisCS.w, 1e-4);
            float inflate = max(1.0, minPx / max(trueDiamPx, 1e-6));
            posWS = axisWS + nWS * (radius * inflate);
            o.positionCS = TransformWorldToHClip(posWS);
            o.positionWS = posWS;
            o.normalWS = nWS;
            o.lineData = float4(max(BwLineAlphaFloor(), saturate(trueDiamPx / minPx)), 0.0, 0.0, 0.0); // pal:ok (energy alpha)
            o.gw = float2(0.0, 1.0);
        }

        // Ribbon: screen-space expansion perpendicular to the projected line (homogeneous image line, robust behind the
        // camera and against near-plane clipping).
        void BwLineRibbonVertex(BwLineAttributes v, inout BwLineVaryings o)
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
                o.positionCS = c;
                o.positionWS = pWS;
                o.normalWS = dirWS;
                o.lineData = 0.0;
                o.gw = float2(1e6, 1.0);
                return;
            }
            Lpx /= len;

            float trueHalfPx = (c.w > 1e-4) ? 0.5 * _LineWidth * BwLineFocalPx() / c.w : 0.0;
            float extentPx = max(trueHalfPx, 0.5 * BwLineMinWidthPx()) + 1.0;     // + 1 px skirt for the AA ramp
            float side = (v.uv1.y >= 0.0) ? 1.0 : -1.0;
            c.xy += Lpx.xy * (side * extentPx) * (2.0 / S) * c.w;

            o.positionCS = c;
            o.positionWS = pWS;
            o.normalWS = dirWS;
            o.lineData = float4(Lpx, 0.0);                                      // pal:ok (line coefficients)
            o.gw = float2(dot(Lpx, float3(c.xy * (0.5 * S), c.w)), c.w);         // pal:ok (homogeneous point)
        }

        BwLineVaryings BwLineVert(BwLineAttributes v)
        {
            BwLineVaryings o = (BwLineVaryings)0;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        #if defined(_LINE_RIBBON)
            BwLineRibbonVertex(v, o);
        #else
            BwLineTubeVertex(v, o);
        #endif
            o.color = v.color;
            return o;
        }

        void BwLineRibbonCoverage(BwLineVaryings i, out float d, out float halfPx, out float alpha)
        {
            float w = max(i.gw.y, 1e-6);
            d = i.gw.x / w;
            float trueHalf = 0.5 * _LineWidth * BwLineFocalPx() / w;
            float minHalf = 0.5 * BwLineMinWidthPx();
            halfPx = max(trueHalf, minHalf);
            float cov = saturate(0.5 + halfPx - abs(d));
            float energy = max(BwLineAlphaFloor(), saturate(trueHalf / minHalf));
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
            #pragma vertex BwLineVert
            #pragma fragment BwLineForwardFrag
            #pragma shader_feature_local _ _LINE_TUBE _LINE_RIBBON
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Include/BwLighting.hlsl"

            float4 BwLineForwardFrag(BwLineVaryings i, FRONT_FACE_TYPE isFront : FRONT_FACE_SEMANTIC) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float3 positionWS = i.positionWS;
                float3 toCam = SafeNormalize(_WorldSpaceCameraPos - positionWS);
                float3 n;
                float alphaOut;

            #if defined(_LINE_RIBBON)
                float d, halfPx, alpha;
                BwLineRibbonCoverage(i, d, halfPx, alpha);
                clip(alpha - 1e-4);
                // cylinder normal across the line, signed toward +d on screen
                float3 dirWS = SafeNormalize(i.normalWS);
                float3 sideWS = cross(dirWS, toCam);
                float sl2 = dot(sideWS, sideWS);
                sideWS = (sl2 > 1e-10) ? sideWS * rsqrt(sl2) : SafeNormalize(cross(dirWS, float3(dirWS.y, dirWS.z, dirWS.x))); // pal:ok (any perpendicular)
                float4 cs = mul(GetWorldToHClipMatrix(), float4(sideWS, 0.0));      // pal:ok (direction)
                float4 cp = mul(GetWorldToHClipMatrix(), float4(positionWS, 1.0));  // pal:ok (point)
                float2 dq = (cs.xy * cp.w - cp.xy * cs.w) * (0.5 * GetScaledScreenParams().xy);
                sideWS *= (dot(i.lineData.xy, dq) >= 0.0) ? 1.0 : -1.0;
                float u = clamp(d / halfPx, -1.0, 1.0);
                n = SafeNormalize(u * sideWS + sqrt(saturate(1.0 - u * u)) * toCam);
                alphaOut = alpha;
            #else
                if (!isFront) clip(-1.0);   // closed tube: back faces never contribute (keeps A2C coverage exact)
                alphaOut = i.lineData.x;
                if (BwLineFadeOn())
                {
                    float keep = BwLineFadeKeep(positionWS);
                    if (_AlphaToMaskAvailable > 0.5)
                    {
                        clip(keep - BL_FADE_A2C_MIN);
                        alphaOut *= keep;
                    }
                    else
                    {
                        BwTierClip(keep, i.positionCS.xy);
                    }
                }
                n = SafeNormalize(i.normalWS);
            #endif

                SurfaceData s = (SurfaceData)0;
                s.albedo = _BaseColor.rgb;
                s.alpha = 1.0;
                s.metallic = _Metallic;
                s.smoothness = _Smoothness;
                s.occlusion = 1.0;
                s.normalTS = half3(0, 0, 1);

                InputData inputData = (InputData)0;
                inputData.positionWS = positionWS;
                inputData.positionCS = i.positionCS;
                inputData.normalWS = n;
                inputData.viewDirectionWS = toCam;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
            #if defined(MAIN_LIGHT_CALCULATE_SHADOWS) && !defined(_LINE_RIBBON)
                inputData.shadowCoord = TransformWorldToShadowCoord(positionWS);
            #else
                inputData.shadowCoord = float4(0, 0, 0, 0);   // pal:ok (not a colour)
            #endif
                inputData.shadowMask = half4(1, 1, 1, 1);
                inputData.bakedGI = BwAmbient(n, positionWS);   // region ambient (BwAmbientAt)
                float3 color = BwFogApply(BwShade(inputData, s, positionWS), positionWS);
            #if defined(_LINE_RIBBON)
                return float4(color * alphaOut, alphaOut);                             // pal:ok (premultiplied)
            #else
                return float4(color, alphaOut);                                        // pal:ok (A2C energy alpha x fade)
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
            #pragma vertex BwLineVert
            #pragma fragment BwLineDepthFrag
            #pragma shader_feature_local _ _LINE_TUBE _LINE_RIBBON
            #pragma multi_compile_instancing

            half BwLineDepthFrag(BwLineVaryings i, FRONT_FACE_TYPE isFront : FRONT_FACE_SEMANTIC) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #if defined(_LINE_RIBBON)
                clip(-1.0);   // blended ribbons never write depth
            #else
                if (!isFront) clip(-1.0);
                if (BwLineFadeOn()) BwTierClip(BwLineFadeKeep(i.positionWS), i.positionCS.xy);
            #endif
                return i.positionCS.z;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex BwLineVert
            #pragma fragment BwLineDepthNormalsFrag
            #pragma shader_feature_local _ _LINE_TUBE _LINE_RIBBON
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_instancing

            void BwLineDepthNormalsFrag(BwLineVaryings i, FRONT_FACE_TYPE isFront : FRONT_FACE_SEMANTIC
                , out half4 outNormalWS : SV_Target0
            #ifdef _WRITE_RENDERING_LAYERS
                , out uint outRenderingLayers : SV_Target1
            #endif
                )
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #if defined(_LINE_RIBBON)
                clip(-1.0);
            #else
                if (!isFront) clip(-1.0);
                if (BwLineFadeOn()) BwTierClip(BwLineFadeKeep(i.positionWS), i.positionCS.xy);
            #endif
                float3 n = SafeNormalize(i.normalWS);
            #if defined(_GBUFFER_NORMALS_OCT)
                float2 octNormalWS = PackNormalOctQuadEncode(n);
                float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
                outNormalWS = half4(PackFloat2To888(remappedOctNormalWS), 0.0); // pal:ok (packed normal)
            #else
                outNormalWS = half4(n, 0.0);                                    // pal:ok (normal)
            #endif
            #ifdef _WRITE_RENDERING_LAYERS
                outRenderingLayers = EncodeMeshRenderingLayer();
            #endif
            }
            ENDHLSL
        }
    }

    FallBack Off
}
