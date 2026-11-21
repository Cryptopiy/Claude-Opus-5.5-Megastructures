// BwGrayboxMist - the graybox "fogged cone plume" of beat 3 (MASTER_PROMPT 14.5.3 graybox line; the real plume is the
// analytic MistVolumeFeature of 14.7 H2). Drawn on the front faces of a tilted cone frustum (base _ConeBase.xyz radius
// _ConeBase.w, apex _ConeApex.xyz radius _ConeApex.w); the alpha is NOT a facing ramp but the optical depth of an analytic
// density along the view ray:
//   density = (1 - r^2 / R^2) x G(z) x H(y)   inside the cone (r = distance to the axis in the horizontal cross-section)
//   tau     = _TauPeak x (1 - b^2 / R^2)^1.5 x G(z_mid) x H(y) / max(|d_xz|, 0.5), capped at _TauPeak
// where b is the distance from the cone axis to the ray's horizontal line (the parabolic profile integrates to
// (4/3) h^3 / R^2 along a chord of half-length h = sqrt(R^2 - b^2), normalised to 1 on the axis), z_mid the chord midpoint,
// G the 14.5.3 Gaussian along the dam (centre _PlumeZ, sigma _SigmaZ = 150 m) and H the height law (scale _FadeH = 130 m
// above the deck, 0 at _TopY = +260). The peak horizontal optical depth is 1.0 (14.5.3 / H2), so alpha <= 1 - e^-1 = 0.632,
// and alpha falls to 0 smoothly at the silhouette (b -> R): no wall-like band, no hard vertical edge, and the lowland stays
// visible through it. Colour: FoamWhite lit by the sky model (BwSkyColor), then BwFogApply. ZWrite off, Transparent queue,
// static in time (no flicker), no particles. No literal colours (T-PAL).
Shader "Megastructure/BwGrayboxMist"
{
    Properties
    {
        _TauPeak ("Peak horizontal optical depth", Range(0, 2)) = 1
        _TopY ("Top (world y, m)", Float) = 260
        _FadeH ("Height scale (m)", Float) = 130
        _Brightness ("Brightness (x sky)", Float) = 1.6
        _PlumeZ ("Gaussian centre z (m)", Float) = 562
        _SigmaZ ("Gaussian sigma along z (m)", Float) = 150
        _ConeBase ("Cone base (xyz, radius)", Vector) = (-400, -900, 562, 420)
        _ConeApex ("Cone apex (xyz, radius)", Vector) = (-70, 255, 562, 30)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off ZTest LEqual Cull Back

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Include/BulwarkPalette.hlsl"
            #include "Include/BwCommon.hlsl"
            #include "Include/BulwarkAtmosphere.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _TauPeak;
                float _TopY;
                float _FadeH;
                float _Brightness;
                float _PlumeZ;
                float _SigmaZ;
                float4 _ConeBase;
                float4 _ConeApex;
            CBUFFER_END

            struct A { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };

            V Vert(A i)
            {
                V o = (V)0;
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_TRANSFER_INSTANCE_ID(i, o);
                o.positionWS = TransformObjectToWorld(i.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                return o;
            }

            // Optical depth of the analytic plume along the ray from the eye through p (p on the cone's front surface).
            float PlumeTau(float3 p, float3 eye)
            {
                float3 d = p - eye;
                float len = max(length(d), 1e-4);
                d /= len;
                float hy = _ConeApex.y - _ConeBase.y;
                float t = saturate((p.y - _ConeBase.y) / (abs(hy) > 1e-3 ? hy : 1e-3));
                float3 c = lerp(_ConeBase.xyz, _ConeApex.xyz, t);
                float r = max(lerp(_ConeBase.w, _ConeApex.w, t), 1e-3);
                float lh = length(d.xz);
                float2 u = lh > 1e-4 ? d.xz / lh : float2(1.0, 0.0);
                float2 rel = c.xz - p.xz;
                float along = dot(rel, u);
                float b = abs(rel.x * u.y - rel.y * u.x);       // distance from the axis to the ray's horizontal line
                float q = saturate(1.0 - (b * b) / (r * r));
                float profile = q * sqrt(q);                     // (1 - b^2/R^2)^1.5: 0 at the silhouette
                float zMid = p.z + u.y * along;                  // chord midpoint (closest approach to the axis)
                float gz = (zMid - _PlumeZ) / max(_SigmaZ, 1.0);
                float g = exp(-0.5 * gz * gz);
                float h = exp(-max(p.y, 0.0) / max(_FadeH, 1.0)) * (1.0 - smoothstep(_TopY - 60.0, _TopY, p.y));
                float slant = 1.0 / max(lh, 0.5);                // inclined rays cross more mist (at most x2)
                return min(_TauPeak * profile * g * h * slant, _TauPeak);
            }

            half4 Frag(V i) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 eye = _WorldSpaceCameraPos.xyz;
                float tau = PlumeTau(i.positionWS, eye);
                float alpha = 1.0 - exp(-tau);
                float3 v = normalize(eye - i.positionWS);
                float3 up = float3(0.0, 1.0, 0.0);   // pal:ok (direction, not a colour)
                float3 sky = 0.6 * BwSkyColor(up) + 0.4 * BwSkyColor(-v);
                float3 c = PAL_FoamWhite * sky * _Brightness;
                c = BwFogApply(c, i.positionWS);
                return half4(max(c, 0.0), saturate(alpha));
            }
            ENDHLSL
        }
    }
    Fallback Off
}
