// BwGrayboxSky - the graybox stand-in for the sky dome and far field (MASTER_PROMPT 14.5.2 / 14.5.7 graybox "sky",
// "sky gradient"). Drawn ONLY when the atmosphere owner's Megastructure.Beats.WorldSky does not exist (GrayboxPath checks the
// type), so the two never draw together. An inward sphere of radius 9 km round the path (inside the 12 km far plane):
//   above the visible horizon: BwSkyColor(view ray) (the one sky model of 14.2.2, so horizon and fog agree, Part 10 #13);
//   below it, west of the dam: a stand-in lowland at y -6000 (ConcreteDark x 0.5 with 1.2 km field cells, footprint-faded),
//   east of it: a stand-in sea at y -8 (SeaStorm); both through BwFogApply at their virtual hit point.
// Unlit; no fog on the sky itself (it IS the fog colour). No literal colours (T-PAL).
Shader "Megastructure/BwGrayboxSky"
{
    Properties
    {
        _GroundScale ("Lowland albedo scale", Float) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+450" "IgnoreProjector" = "True" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On ZTest LEqual Cull Back

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Include/BulwarkPalette.hlsl"
            #include "Include/BwCommon.hlsl"
            #include "Include/BulwarkAtmosphere.hlsl"
            #include "Generated/BulwarkConstGen.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _GroundScale;
            CBUFFER_END

            struct A { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
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

            // sine of the dip of the horizon of a sphere (radius R) seen from height h above it (stable form, 14.7 H4)
            float GbSinDip(float h)
            {
                float R = TC_EarthRadius;
                h = max(h, 0.0);
                return sqrt(saturate(h * (2.0 * R + h) / ((R + h) * (R + h))));
            }

            half4 Frag(V i) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 cam = _WorldSpaceCameraPos.xyz;
                float3 v = i.positionWS - cam;
                float3 d = v / max(length(v), 1e-4);
                float3 c = BwSkyColor(d);
                // lowland plane hit (computed for every pixel so the footprint derivatives stay in uniform control flow)
                float hl = max(cam.y - TC_LowlandY, 1.0);
                float tl = min(hl / max(-d.y, 1e-5), 400000.0);
                float3 pl = cam + d * tl;
                float2 q = pl.xz / 1200.0;
                float fw = max(fwidth(q.x), fwidth(q.y));
                float amp = 0.25 * saturate(1.0 - fw * 4.0);           // 1.2 km field cells fade before they alias
                float n = BwHash21((int2)floor(q));
                bool east = d.x > 0.0 && cam.x > TC_SeaFaceX - 0.5;
                if (east)
                {
                    float h = max(cam.y - TC_SeaY, 0.1);
                    if (d.y < -GbSinDip(h))
                    {
                        float t = min(h / max(-d.y, 1e-5), 12000.0);
                        c = BwFogApply(PAL_SeaStorm * 0.6, cam + d * t);
                    }
                }
                else if (d.y < -GbSinDip(hl))
                {
                    float3 ground = PAL_ConcreteDark * _GroundScale * (1.0 + amp * (2.0 * n - 1.0));
                    c = BwFogApply(ground, pl);
                }
                return half4(max(c, 0.0), 1.0h);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
