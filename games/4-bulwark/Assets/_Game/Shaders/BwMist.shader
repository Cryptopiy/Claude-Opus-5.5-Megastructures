// BwMist.shader - the full-screen mist pass of MistVolumeFeature (MASTER_PROMPT 14.7 H2, 14.2.6 "BwMist", Part 8.6 MSAA rule).
// Drawn after the opaques and the far-field dome (RenderPassEvent.AfterRenderingSkybox), before the transparents and BwGrade,
// into the MSAA camera colour with Blend One SrcAlpha (rgb = in-scattered radiance, a = transmittance of the scene).
// The maths is Include/BwMistVolume.hlsl; this file only decides where it runs:
//   BwMistMark    per pixel, own MSAA stencil (MistVolumePass creates a transient D24S8): loads the 4 MSAA depths
//                 (Texture2DMS bound through UseTexture, the path URP's CopyDepth takes on D3D11) and marks the pixel
//                 (stencil 1) when they disagree by more than 2 % and its ray touches a mist volume; no colour.
//   BwMistPixel   per pixel where stencil != 1: one integration to the (agreeing) depth, written to all 4 samples.
//   BwMistSample  per sample where stencil == 1 (SV_SampleIndex): the same 24-sample grid (bounded by the farthest of the
//                 pixel's samples) read at the sample's own depth, so a rail sample gets almost no mist and a plume sample
//                 all of it: no halo on thin foreground (14.7 H2 MSAA rule).
//   BwMistSingle  the logged fallback when the depth is not MSAA-bindable (or MSAA is off): one pass, the depth of a
//                 single-sample texture; with _BwMistNearest3x3 = 1 the nearest depth of the 3 x 3 neighbourhood of the
//                 resolved _CameraDepthTexture (max raw under reversed Z; RESEARCH B 7.5).
// Every pass: ZTest Always, ZWrite Off, Cull Off. No material properties (all inputs are _BwMist* globals, MistVolumeState).
Shader "Megastructure/BwMist"
{
    Properties { }

    HLSLINCLUDE
    #include "Include/BwMistVolume.hlsl"

    Texture2DMS<float, 4> _BwMistDepthMS;     // the camera depth attachment (bindTextureMS, 4 samples)
    Texture2D<float> _BwMistDepth;             // fallback: a single-sample depth
    float _BwMistNearest3x3;                   // fallback: 1 = nearest of the 3 x 3 neighbourhood

    struct MistV { float4 positionCS : SV_POSITION; };

    MistV MistVert(uint vid : SV_VertexID)
    {
        MistV o;
        o.positionCS = GetFullScreenTriangleVertexPosition(vid);
        return o;
    }

    // World ray through a sub-pixel position (SV_Position units), camera-relative: inverse projection then the view
    // rotation (no world-space reconstruction: no float precision loss at z 600 m).
    float3 MistRayDir(float2 pixel)
    {
        float2 uv = pixel * _BwMistScreen.zw;
        float4 cs = ComputeClipSpacePosition(uv, 0.5);
        float4 vs = mul(UNITY_MATRIX_I_P, cs);
        float3 v = vs.xyz / (abs(vs.w) > 1e-8 ? vs.w : 1e-8);
        return normalize(mul((float3x3)UNITY_MATRIX_I_V, v));
    }

    // Distance along the ray d to the surface of a raw depth (1e7 for the cleared far value: sky / far-field dome).
    float MistRawToDist(float raw, float3 d)
    {
    #if UNITY_REVERSED_Z
        if (raw <= 1e-7) return 1e7;
    #else
        if (raw >= 1.0 - 1e-7) return 1e7;
    #endif
        float eye = LinearEyeDepth(raw, _ZBufferParams);
        float3 fwd = -UNITY_MATRIX_V[2].xyz;
        return eye / max(dot(d, fwd), 1e-4);
    }

    // Eye depths of the 4 samples (1e7 for the far value) and whether they agree within 2 %.
    void MistLoad4(int2 px, out float4 raw, out float4 eye)
    {
        raw = float4(_BwMistDepthMS.Load(px, 0), _BwMistDepthMS.Load(px, 1), _BwMistDepthMS.Load(px, 2), _BwMistDepthMS.Load(px, 3));   // pal:ok (depths)
        [unroll] for (int k = 0; k < 4; k++)
        {
        #if UNITY_REVERSED_Z
            eye[k] = raw[k] <= 1e-7 ? 1e7 : LinearEyeDepth(raw[k], _ZBufferParams);
        #else
            eye[k] = raw[k] >= 1.0 - 1e-7 ? 1e7 : LinearEyeDepth(raw[k], _ZBufferParams);
        #endif
        }
    }

    bool MistIsEdge(float4 eye)
    {
        float zmin = min(min(eye.x, eye.y), min(eye.z, eye.w));
        float zmax = max(max(eye.x, eye.y), max(eye.z, eye.w));
        return zmax > 1.02 * zmin;
    }

    float MistEyeToDist(float eye, float3 d)
    {
        if (eye >= 1e6) return 1e7;
        float3 fwd = -UNITY_MATRIX_V[2].xyz;
        return eye / max(dot(d, fwd), 1e-4);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            Name "BwMistMark"
            ColorMask 0
            Stencil { Ref 1 Comp Always Pass Replace }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex MistVert
            #pragma fragment MistMarkFrag
            void MistMarkFrag(MistV i)
            {
                int2 px = int2(i.positionCS.xy);
                float4 raw, eye;
                MistLoad4(px, raw, eye);
                if (!MistIsEdge(eye)) discard;
                float3 d = MistRayDir(i.positionCS.xy);
                float far = MistEyeToDist(max(max(eye.x, eye.y), max(eye.z, eye.w)), d);
                if (!MistRayTouches(_WorldSpaceCameraPos.xyz, d, far)) discard;
            }
            ENDHLSL
        }

        Pass
        {
            Name "BwMistPixel"
            Blend One SrcAlpha
            Stencil { Ref 1 Comp NotEqual }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex MistVert
            #pragma fragment MistPixelFrag
            float4 MistPixelFrag(MistV i) : SV_Target
            {
                int2 px = int2(i.positionCS.xy);
                float4 raw, eye;
                MistLoad4(px, raw, eye);
                float3 d = MistRayDir(i.positionCS.xy);
                float near = MistEyeToDist(min(min(eye.x, eye.y), min(eye.z, eye.w)), d);
                float far = MistEyeToDist(max(max(eye.x, eye.y), max(eye.z, eye.w)), d);
                float4 m = MistIntegrate(_WorldSpaceCameraPos.xyz, d, near, far);
                if (m.a >= 0.99999 && dot(m.rgb, m.rgb) <= 0.0) discard;
                return m;
            }
            ENDHLSL
        }

        Pass
        {
            Name "BwMistSample"
            Blend One SrcAlpha
            Stencil { Ref 1 Comp Equal }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex MistVert
            #pragma fragment MistSampleFrag
            float4 MistSampleFrag(MistV i, uint s : SV_SampleIndex) : SV_Target
            {
                int2 px = int2(floor(i.positionCS.xy));
                float4 raw, eye;
                MistLoad4(px, raw, eye);
                float2 sp = floor(i.positionCS.xy) + 0.5 + _BwMistDepthMS.GetSamplePosition(s);
                float3 d = MistRayDir(sp);
                float own = MistEyeToDist(eye[min(s, 3u)], d);
                float far = MistEyeToDist(max(max(eye.x, eye.y), max(eye.z, eye.w)), d);
                return MistIntegrate(_WorldSpaceCameraPos.xyz, d, own, far);
            }
            ENDHLSL
        }

        Pass
        {
            Name "BwMistSingle"
            Blend One SrcAlpha
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex MistVert
            #pragma fragment MistSingleFrag
            float4 MistSingleFrag(MistV i) : SV_Target
            {
                int2 px = int2(i.positionCS.xy);
                float raw = _BwMistDepth.Load(int3(px, 0));
                if (_BwMistNearest3x3 > 0.5)
                {
                    int2 hi = int2(_BwMistScreen.xy) - 1;
                    [unroll] for (int y = -1; y <= 1; y++)
                        [unroll] for (int x = -1; x <= 1; x++)
                        {
                            float r = _BwMistDepth.Load(int3(clamp(px + int2(x, y), int2(0, 0), hi), 0));
                        #if UNITY_REVERSED_Z
                            raw = max(raw, r);
                        #else
                            raw = min(raw, r);
                        #endif
                        }
                }
                float3 d = MistRayDir(i.positionCS.xy);
                float t = MistRawToDist(raw, d);
                float4 m = MistIntegrate(_WorldSpaceCameraPos.xyz, d, t, t);
                if (m.a >= 0.99999 && dot(m.rgb, m.rgb) <= 0.0) discard;
                return m;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
