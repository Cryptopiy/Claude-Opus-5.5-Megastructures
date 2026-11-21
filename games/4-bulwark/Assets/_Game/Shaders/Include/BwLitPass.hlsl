// BwLitPass.hlsl - the four passes of every lit Bw* shader (ForwardLit, ShadowCaster, DepthOnly, DepthNormals) with
// one shared vertex path: object -> world -> (vertex hook) -> clip. Imported from The Ring's RingLitPass.hlsl (based on
// URP 17.6 Lit: LitForwardPass.hlsl, ShadowCasterPass.hlsl, DepthOnlyPass.hlsl, LitDepthNormalsPass.hlsl) with the bend
// removed, BwFogApply as the fog and GPU instancing added (InstancedBatch draws lit materials with RenderMeshInstanced).
// Wave A (materials owner): BwFragData.normalGeoWS (the unperturbed vertex normal for triplanar weights and horizon
// occlusion), optional tangent-space normals (BW_USE_NORMAL_TS, mesh tangent frame), optional alpha output for
// alpha-to-coverage (BW_ALPHA_OUT), and BwShadeEx (specular AA, analytic environment, clamps: BwLighting.hlsl).
#ifndef BW_LIT_PASS_INCLUDED
#define BW_LIT_PASS_INCLUDED

#if defined(BW_PASS_FORWARD)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "BwLighting.hlsl"
#elif defined(BW_PASS_SHADOW)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
    float3 _LightDirection;
    float3 _LightPosition;
#endif

struct BwAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    float4 tangentOS  : TANGENT;
    float4 color      : COLOR;
    float2 uv0        : TEXCOORD0;
    float2 uv1        : TEXCOORD1;
#if defined(BW_INDIRECT)
    uint instanceID   : SV_InstanceID;
#else
    UNITY_VERTEX_INPUT_INSTANCE_ID
#endif
};

struct BwVaryings
{
    float4 positionCS     : SV_POSITION;
    float3 positionWS     : TEXCOORD0;
    float3 normalWS       : TEXCOORD1;
    float4 tangentWS      : TEXCOORD2;
    float4 color          : TEXCOORD3;
    float4 uv             : TEXCOORD4;
    float4 custom         : TEXCOORD5;
#if !defined(BW_INDIRECT)
    UNITY_VERTEX_INPUT_INSTANCE_ID
#endif
};

BwVaryings BwVert(BwAttributes input)
{
    BwVaryings o = (BwVaryings)0;
#if !defined(BW_INDIRECT)
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, o);
#endif
    BwVertexData v;
    v.positionOS = input.positionOS.xyz;
#if defined(BW_INDIRECT)
    v.instanceID = input.instanceID;
    v.positionWS = input.positionOS.xyz;
    v.normalWS = input.normalOS;
    v.tangentWS = input.tangentOS;
#else
    v.instanceID = 0;
    v.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    v.normalWS = TransformObjectToWorldNormal(input.normalOS);
    v.tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);   // pal:ok (not a colour)
#endif
    v.color = input.color;
    v.uv = float4(input.uv0, input.uv1);   // pal:ok (not a colour)
    v.custom = 0;
#if defined(BW_HAS_VERTEX_HOOK)
    BwVertexHook(v);
#endif
    float3 n = normalize(v.normalWS);

#if defined(BW_PASS_SHADOW)
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

BwFragData BwMakeFrag(BwVaryings i)
{
    BwFragData f;
    f.positionCS = i.positionCS;
    f.positionWS = i.positionWS;
    float nl2 = dot(i.normalWS, i.normalWS);
    f.normalWS = nl2 > 1e-12 ? i.normalWS * rsqrt(nl2) : float3(0, 1, 0);   // pal:ok (not a colour)
    f.normalGeoWS = f.normalWS;
    f.tangentWS = i.tangentWS;
    f.color = i.color;
    f.uv = i.uv;
    f.custom = i.custom;
    f.screenUV = GetNormalizedScreenSpaceUV(i.positionCS);
    float3 v = _BwCam.xyz - i.positionWS;
    f.viewDist = length(v);
    f.viewDirWS = v / max(f.viewDist, 1e-4);
    f.footprint = max(f.viewDist * _BwCam.w, 1e-4);
    return f;
}

#if defined(BW_PASS_FORWARD)
half4 BwFrag(BwVaryings i) : SV_Target0
{
#if !defined(BW_INDIRECT)
    UNITY_SETUP_INSTANCE_ID(i);
#endif
    BwFragData f = BwMakeFrag(i);
#if defined(BW_HAS_CLIP_HOOK)
    BwClipHook(f);
#endif
    SurfaceData s = (SurfaceData)0;
    s.albedo = half3(0.5, 0.5, 0.5); // pal:ok (neutral default before the hook)
    s.alpha = 1.0h;
    s.metallic = 0.0h;
    s.specular = half3(0, 0, 0); // pal:ok
    s.smoothness = 0.5h;
    s.occlusion = 1.0h;
    s.emission = half3(0, 0, 0); // pal:ok
    s.normalTS = half3(0, 0, 1);
    BwSurfaceHook(f, s);

#if defined(BW_USE_NORMAL_TS)
    // tangent-space normal through the mesh tangent frame (meshes with tangents); a zero tangent keeps the hook normal
    float3 tWS = f.tangentWS.xyz;
    if (dot(tWS, tWS) > 1e-8)
    {
        tWS = normalize(tWS - f.normalWS * dot(tWS, f.normalWS));
        float3 bWS = cross(f.normalWS, tWS) * (f.tangentWS.w < 0.0 ? -1.0 : 1.0) * GetOddNegativeScale();
        f.normalWS = normalize(TransformTangentToWorld(s.normalTS, half3x3(tWS, bWS, f.normalWS)));
    }
#endif

    InputData inputData = (InputData)0;
    inputData.positionWS = f.positionWS;
    inputData.positionCS = i.positionCS;
    inputData.normalWS = NormalizeNormalPerPixel(f.normalWS);
    inputData.viewDirectionWS = f.viewDirWS;
    inputData.normalizedScreenSpaceUV = f.screenUV;
#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(f.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);   // pal:ok (not a colour)
#endif
    inputData.shadowMask = half4(1, 1, 1, 1);
    // region ambient (14.6, Part 10 #36): interiors take lamp colour x 0.06 by the interior boxes of the world point, so the
    // exterior seen through the door, the portal or the tunnel mouth keeps the exterior trilight (BulwarkAtmosphere.hlsl)
    // and the sky light of open doors, portals and arches on the interior part (BwRegionAmbient, BwLighting.hlsl)
    inputData.bakedGI = BwRegionAmbient(inputData.normalWS, f.positionWS, f.normalGeoWS);

    half3 c = BwShadeEx(inputData, s, f.positionWS, f.normalGeoWS);
#if !defined(BW_NO_FOG)
    c = BwFogApply(c, f.positionWS);
#endif
#if defined(BW_ALPHA_OUT)
    return half4(c, saturate(s.alpha));
#else
    return half4(c, 1.0h);
#endif
}
#elif defined(BW_PASS_DEPTHNORMALS)
void BwFrag(BwVaryings i, out half4 outNormalWS : SV_Target0)
{
#if !defined(BW_INDIRECT)
    UNITY_SETUP_INSTANCE_ID(i);
#endif
#if defined(BW_HAS_CLIP_HOOK)
    BwClipHook(BwMakeFrag(i));
#endif
    outNormalWS = half4(NormalizeNormalPerPixel(normalize(i.normalWS)), 0.0);
}
#elif defined(BW_PASS_DEPTH)
half BwFrag(BwVaryings i) : SV_Target0
{
#if !defined(BW_INDIRECT)
    UNITY_SETUP_INSTANCE_ID(i);
#endif
#if defined(BW_HAS_CLIP_HOOK)
    BwClipHook(BwMakeFrag(i));
#endif
    return i.positionCS.z;
}
#else // shadow
half4 BwFrag(BwVaryings i) : SV_Target0
{
#if !defined(BW_INDIRECT)
    UNITY_SETUP_INSTANCE_ID(i);
#endif
#if defined(BW_HAS_CLIP_HOOK)
    BwClipHook(BwMakeFrag(i));
#endif
    return 0;
}
#endif

#endif
