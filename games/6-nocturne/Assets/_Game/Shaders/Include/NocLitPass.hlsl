// NocLitPass.hlsl - the four passes of every lit Noc* shader (ForwardLit, ShadowCaster, DepthOnly, DepthNormals) with one
// shared vertex path: object -> world -> (vertex hook) -> clip (imported from The Ring's RingLitPass.hlsl, flat world).
// Forward order (14.2.2): c = NocShade(...) (or the material's NocForwardHook), then NocFog(c, posWS) (cloud mix + fog), then
// NocFloor(c) last; specular AA on the final normal first. NOC_TRANSPARENT returns premultiplied (c alpha, alpha). No shadow maps are rendered in
// Nocturne (14.2.3); the ShadowCaster pass is kept so the template stays URP-Lit compatible.
// Look v2 (TEXTURE_PLAN 8.3 TP-0): uv1 is a float4 (the facade writers' uMin, uMax, y0, y1; xy as before); NOC_PASS_UV1ZW,
// NOC_ALPHA_OUT, NOC_FRONT_FACE, f.specOcc and f.debug as listed in NocLitCore.hlsl; _NocDebugView > 0 returns the raw debug
// codes before fog, floor and lighting (7.1); _NocParity skips NocFog and NocFloor (ShaderParityTests, 7.4).
#ifndef NOC_LIT_PASS_INCLUDED
#define NOC_LIT_PASS_INCLUDED

#if defined(NOC_PASS_FORWARD)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "NocLighting.hlsl"
#elif defined(NOC_PASS_SHADOW)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
    float3 _LightDirection;
    float3 _LightPosition;
#endif

struct NocAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    float4 tangentOS  : TANGENT;
    float4 color      : COLOR;
    float2 uv0        : TEXCOORD0;
    float4 uv1        : TEXCOORD1;   // xy as before; zw: the facade's y0, y1 (NOC_PASS_UV1ZW), (0, 1) when the mesh has 2 components
#if defined(NOC_INDIRECT)
    uint instanceID   : SV_InstanceID;
#else
    UNITY_VERTEX_INPUT_INSTANCE_ID
#endif
};

struct NocVaryings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
    float3 normalWS   : TEXCOORD1;
    float4 tangentWS  : TEXCOORD2;
    float4 color      : TEXCOORD3;
    float4 uv         : TEXCOORD4;
    float4 custom     : TEXCOORD5;
};

NocVaryings NocVert(NocAttributes input)
{
    NocVaryings o = (NocVaryings)0;
#if !defined(NOC_INDIRECT)
    UNITY_SETUP_INSTANCE_ID(input);
#endif
    NocVertexData v;
    v.positionOS = input.positionOS.xyz;
#if defined(NOC_INDIRECT)
    v.instanceID = input.instanceID;
    v.positionWS = input.positionOS.xyz;
    v.normalWS = input.normalOS;
    v.tangentWS = input.tangentOS;
#else
    v.instanceID = 0;
    v.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    v.normalWS = TransformObjectToWorldNormal(input.normalOS);
    v.tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);
#endif
    v.color = input.color;
    v.uv = float4(input.uv0, input.uv1.xy);
    v.custom = 0;
#if defined(NOC_PASS_UV1ZW)
    v.custom.zw = input.uv1.zw;
#endif
#if defined(NOC_HAS_VERTEX_HOOK)
    NocVertexHook(v);
#endif
    float3 n = normalize(v.normalWS);
#if defined(NOC_PASS_SHADOW)
    #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
        float3 lightDirectionWS = normalize(_LightPosition - v.positionWS);
    #else
        float3 lightDirectionWS = _LightDirection;
    #endif
    o.positionCS = ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(v.positionWS, n, lightDirectionWS)));
#else
    o.positionCS = TransformWorldToHClip(v.positionWS);
#endif
    o.positionWS = v.positionWS;
    o.normalWS = n;
    o.tangentWS = v.tangentWS;
    o.color = v.color;
    o.uv = v.uv;
    o.custom = v.custom;
    return o;
}

NocFragData NocMakeFrag(NocVaryings i)
{
    NocFragData f;
    f.positionCS = i.positionCS;
    f.positionWS = i.positionWS;
    f.normalWS = normalize(i.normalWS);
    f.tangentWS = i.tangentWS;
    f.color = i.color;
    f.uv = i.uv;
    f.custom = i.custom;
    f.screenUV = GetNormalizedScreenSpaceUV(i.positionCS);
    float3 v = _WorldSpaceCameraPos.xyz - i.positionWS;
    f.viewDist = length(v);
    f.viewDirWS = v / max(f.viewDist, 1e-4);
    // metres per pixel at the fragment: 2 d tan(vFOV / 2) / H = 2 d / (P[1][1] H)
    f.footprint = max(f.viewDist * 2.0 / max(abs(UNITY_MATRIX_P[1][1]) * max(_NocScreen.x, 1.0), 1e-4), 1e-5);
    f.faceSign = 1.0;
    f.specOcc = 1.0;
    f.debug = float4(0.0, 0.0, 0.0, 0.0);
    return f;
}

// Debug-view output (TEXTURE_PLAN 7.1): display codes / 255 -> the linear value that the sRGB encode of the capture turns back
// into exactly those codes. View 7 is the world shading normal (128 + 127 n); every other view is whatever the hook wrote into
// f.debug (0 where it wrote nothing).
half3 NocDebugOut(NocFragData f, float3 normalWS)
{
    float3 codes = NocDebugView() == 7 ? (128.0 + 127.0 * normalWS) / 255.0 : (f.debug.a > 0.5 ? f.debug.rgb : float3(0.0, 0.0, 0.0));
    return (half3)NocSrgbToLinear(saturate(codes));
}

#if defined(NOC_PASS_FORWARD)
#if defined(NOC_FRONT_FACE)
half4 NocFrag(NocVaryings i, FRONT_FACE_TYPE isFrontFace : FRONT_FACE_SEMANTIC) : SV_Target0
#else
half4 NocFrag(NocVaryings i) : SV_Target0
#endif
{
    NocFragData f = NocMakeFrag(i);
#if defined(NOC_FRONT_FACE)
    f.faceSign = IS_FRONT_VFACE(isFrontFace, 1.0, -1.0);
#endif
#if defined(NOC_HAS_CLIP_HOOK)
    NocClipHook(f);
#endif
    SurfaceData s = (SurfaceData)0;
    s.albedo = half3(0.5, 0.5, 0.5);
    s.alpha = 1.0h;
    s.metallic = 0.0h;
    s.specular = half3(0, 0, 0);
    s.smoothness = 0.5h;
    s.occlusion = 1.0h;
    s.emission = half3(0, 0, 0);
    s.normalTS = half3(0, 0, 1);
    NocSurfaceHook(f, s);

    InputData inputData = (InputData)0;
    inputData.positionWS = f.positionWS;
    inputData.positionCS = i.positionCS;
    inputData.normalWS = NormalizeNormalPerPixel(f.normalWS);
    inputData.viewDirectionWS = f.viewDirWS;
    inputData.normalizedScreenSpaceUV = f.screenUV;
    inputData.shadowCoord = float4(0, 0, 0, 0);
    inputData.shadowMask = half4(1, 1, 1, 1);
    inputData.bakedGI = NocAmbient(inputData.normalWS);
    // specular AA from the screen-space variance of the final shading normal (8.11 item 6), smoothness <= 0.95; derivatives
    // are taken here, outside any branch
    s.smoothness = NocFilterSmoothness(s.smoothness, inputData.normalWS);

#if defined(NOC_HAS_FORWARD_HOOK)
    half3 c = NocForwardHook(f, s, inputData);      // the material composes its own lighting (NocWet: diffuse + analytic mirror)
#else
    half3 c = NocShadeOcc(inputData, s, f.specOcc);
#endif
    // debug views (7.1): raw codes, no fog, floor or grade; a uniform branch after every derivative (the hooks' included)
    [branch] if (_NocDebugView > 0.5) return half4(NocDebugOut(f, inputData.normalWS), 1.0h);
    [branch] if (_NocParity < 0.5)
    {
#if !defined(NOC_NO_FOG)
        c = NocFog(c, f.positionWS);               // cloud mix, then fog toward NocSkyColor(ray) (14.2.2, H3)
#endif
        c = NocFloor(c);                           // last (14.2.2)
    }
#if defined(NOC_TRANSPARENT)
    return half4(c * s.alpha, s.alpha);            // premultiplied (Blend One OneMinusSrcAlpha): the straight colour is floored
#elif defined(NOC_ALPHA_OUT)
    return half4(c, s.alpha);                      // alpha to coverage (AlphaToMask On): opaque colour, straight alpha
#else
    return half4(c, 1.0h);
#endif
}
#elif defined(NOC_PASS_DEPTHNORMALS)
void NocFrag(NocVaryings i, out half4 outNormalWS : SV_Target0)
{
#if defined(NOC_HAS_CLIP_HOOK)
    NocClipHook(NocMakeFrag(i));
#endif
    outNormalWS = half4(NormalizeNormalPerPixel(normalize(i.normalWS)), 0.0);
}
#elif defined(NOC_PASS_DEPTH)
half NocFrag(NocVaryings i) : SV_Target0
{
#if defined(NOC_HAS_CLIP_HOOK)
    NocClipHook(NocMakeFrag(i));
#endif
    return i.positionCS.z;
}
#else // shadow
half4 NocFrag(NocVaryings i) : SV_Target0
{
#if defined(NOC_HAS_CLIP_HOOK)
    NocClipHook(NocMakeFrag(i));
#endif
    return 0;
}
#endif

#endif
