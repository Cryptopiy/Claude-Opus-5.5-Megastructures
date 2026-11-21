// BwLitCore.hlsl - included FIRST (in HLSLINCLUDE) by every lit Bw* shader, before the material's UnityPerMaterial
// CBUFFER and its hooks. Declares the data structs the hooks see and the shared texture-set helpers (world-space
// triplanar sampling of the BwTexGen packed sets, MASTER_PROMPT 8.11.3). Imported from The Ring's RingLitCore.hlsl with
// the bend stripped (Bulwark is flat: no bent space, positions and normals are plain world space).
//
// A material shader provides, in its HLSLINCLUDE block after including this file:
//   CBUFFER_START(UnityPerMaterial) ... CBUFFER_END          (identical in every pass: SRP Batcher)
//   void BwSurfaceHook(inout BwFragData f, inout SurfaceData s)      (always)
//       s.albedo / metallic / smoothness / occlusion / emission / alpha; the world normal is f.normalWS (the hook may
//       perturb it: triplanar normal maps write it directly). Tangent-space normals: #define BW_USE_NORMAL_TS and write
//       s.normalTS; the pass then transforms it with the mesh tangent frame (mesh tangents required).
//   optional: #define BW_HAS_VERTEX_HOOK + void BwVertexHook(inout BwVertexData v)
//             (world-space displacement; runs identically in all four passes)
//   optional: #define BW_HAS_CLIP_HOOK + void BwClipHook(BwFragData f)   (clip() in all passes)
//   optional: #define BW_HAS_LIGHT_HOOK + void BwMainLightHook(inout half3 color, half3 dirWS) (per-material main-light
//             response on the main light's colour, e.g. BwConcrete's sun tint; forward pass only, after the shadow and
//             sun-break terms)
//   optional: #define BW_ALPHA_OUT (the forward pass returns s.alpha: alpha-to-coverage materials, BwGrating)
//   optional: #define BW_INDIRECT  (RenderMeshIndirect draws, InstancedField: SV_InstanceID reaches the vertex hook and
//             the hook must write a world position itself; unity_ObjectToWorld is not applied; no instancing macros)
//   optional: #define BW_NO_FOG (the material applies its own fog)
// Instanced draws (InstancedBatch, RenderMeshInstanced) need `#pragma multi_compile_instancing` in every pass; the
// vertex path carries UNITY_VERTEX_INPUT_INSTANCE_ID and calls UNITY_SETUP_INSTANCE_ID.
// Then each pass does: #define BW_PASS_FORWARD | BW_PASS_SHADOW | BW_PASS_DEPTH | BW_PASS_DEPTHNORMALS and
// #include "Include/BwLitPass.hlsl".
#ifndef BW_LIT_CORE_INCLUDED
#define BW_LIT_CORE_INCLUDED

// URP's probe and SSR paths are off: the reflection source is the analytic environment of BwLighting.hlsl (8.11.5).
#define _ENVIRONMENTREFLECTIONS_OFF 1
#define _SCREENSPACEREFLECTIONS_OFF 1

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
#include "BulwarkPalette.hlsl"
#include "BwCommon.hlsl"
#include "BulwarkAtmosphere.hlsl"

struct BwVertexData
{
    float3 positionOS;
    float3 positionWS;   // world position (hook may displace it)
    float3 normalWS;     // world normal
    float4 tangentWS;    // world tangent (w = sign)
    float4 color;        // vertex colour (R = AO, G = edge wear by convention, MeshBuilder; B = generator mask)
    float4 uv;           // xy = uv0, zw = uv1
    float4 custom;       // free, interpolated to the fragment
    uint instanceID;
};

struct BwFragData
{
    float4 positionCS;
    float3 positionWS;
    float3 normalWS;        // shading normal: hook may perturb it; keep it normalised
    float3 normalGeoWS;     // interpolated vertex normal (never perturbed: triplanar weights, horizon occlusion)
    float4 tangentWS;
    float4 color;
    float4 uv;
    float4 custom;
    float2 screenUV;
    float3 viewDirWS;
    float viewDist;
    float footprint;        // approximate pixel footprint (m) at the fragment (_BwCam.w * distance)
};

// Perturbs the world normal by a world-space offset.
float3 BwPerturbNormal(BwFragData f, float3 dn)
{
    return normalize(f.normalWS + dn);
}

// ------------------------------------------------------------------------------------------------ texture sets
// BwTexGen packed sets (Editor/TextureFactory.cs, Assets/_Game/Textures): Albedo sRGB (BC7), Normal RG (BC5, tangent
// space of the projection plane, z rebuilt), Mask RGBA linear (BC7): R = metallic potential, G = occlusion, B = height,
// A = smoothness (Toksvig-filtered per mip). All tile on [0, 1]; one tile = `scale` metres.

// Triplanar frame: weights and the three plane coordinates (x plane: (z, y), y plane: (x, z), z plane: (x, y)) in tile
// units, kept UNWRAPPED in the frame (so a rotated / rescaled copy, BwTriDetune, is a linear map of a continuous
// coordinate) and wrapped by 64 tiles only at the sample (BwWrap64: exact, the sets tile per unit; keeps the sampler's
// sub-texel precision at z 1800 m). Derivatives come from the unwrapped coordinates, so SampleGrad keeps the right mip
// across every wrap. No seam anywhere, for t0 and for any detuned copy (Part 10 #33; MaterialTests.Triplanar_* check the
// C# twin BwTriTwin across x = 0, y = 0, z = 64 and z = 128).
struct BwTri
{
    float3 w;
    float2 uvX, uvY, uvZ;
    float2 dxX, dyX, dxY, dyY, dxZ, dyZ;
    float3 nSign;
};

float2 BwWrap64(float2 uv) { return uv - 64.0 * floor(uv * (1.0 / 64.0)); }

BwTri BwTriSetup(float3 posWS, float3 nGeo, float scale, float sharpness)
{
    BwTri t;
    float3 p = posWS / max(scale, 1e-4);
    float3 dpx = ddx(p), dpy = ddy(p);
    float3 a = pow(abs(nGeo), sharpness);
    t.w = a / max(a.x + a.y + a.z, 1e-5);
    t.uvX = p.zy; t.uvY = p.xz; t.uvZ = p.xy;
    t.dxX = dpx.zy; t.dyX = dpy.zy;
    t.dxY = dpx.xz; t.dyY = dpy.xz;
    t.dxZ = dpx.xy; t.dyZ = dpy.xy;
    t.nSign = step(0.0, nGeo) * 2.0 - 1.0;
    return t;
}

// Rotated / rescaled copy of a frame (second, decorrelated sample of the same set: breaks visible tiling, 8.8). Applied
// to the unwrapped coordinates, so the copy is continuous for ANY k (the earlier wrap-then-rotate form jumped by
// k R (64, 0) tiles at every wrap line: a straight seam on x = 0 along the whole crest). The copy's tile is scale / k.
BwTri BwTriDetune(BwTri t, float k, float2 offset)
{
    // 2D rotation by ~37 deg (cos 0.8, sin 0.6) and scale k, applied to every plane's coordinates and derivatives
    float2x2 R = float2x2(0.8, -0.6, 0.6, 0.8) * k;
    t.uvX = mul(R, t.uvX) + offset; t.uvY = mul(R, t.uvY) + offset; t.uvZ = mul(R, t.uvZ) + offset;
    t.dxX = mul(R, t.dxX); t.dyX = mul(R, t.dyX);
    t.dxY = mul(R, t.dxY); t.dyY = mul(R, t.dyY);
    t.dxZ = mul(R, t.dxZ); t.dyZ = mul(R, t.dyZ);
    return t;
}

float4 BwTriSample(TEXTURE2D_PARAM(tex, smp), BwTri t)
{
    float4 c = 0;
    [branch] if (t.w.x > 0.01) c += t.w.x * SAMPLE_TEXTURE2D_GRAD(tex, smp, BwWrap64(t.uvX), t.dxX, t.dyX);
    [branch] if (t.w.y > 0.01) c += t.w.y * SAMPLE_TEXTURE2D_GRAD(tex, smp, BwWrap64(t.uvY), t.dxY, t.dyY);
    [branch] if (t.w.z > 0.01) c += t.w.z * SAMPLE_TEXTURE2D_GRAD(tex, smp, BwWrap64(t.uvZ), t.dxZ, t.dyZ);
    return c / max(dot(t.w, step(0.01, t.w)), 1e-4);
}

// BC5 tangent normal (RG in [0, 1]) -> unit tangent-space normal with the xy strength applied.
float3 BwUnpackRG(float2 rg, float strength)
{
    float2 xy = (rg * 2.0 - 1.0);
    float z = sqrt(saturate(1.0 - dot(xy, xy)));
    return normalize(float3(xy * strength, max(z, 1e-3)));   // pal:ok (not a colour)
}

// Triplanar normal map, whiteout blend in world space (Golus 2017): each plane's tangent normal is added to the
// geometric normal's in-plane components and swizzled back to world axes. Returns a unit world normal.
float3 BwTriNormal(TEXTURE2D_PARAM(tex, smp), BwTri t, float3 nGeo, float strength)
{
    float3 nX = 0, nY = 0, nZ = 0;
    [branch] if (t.w.x > 0.01)
    {
        float3 tn = BwUnpackRG(SAMPLE_TEXTURE2D_GRAD(tex, smp, BwWrap64(t.uvX), t.dxX, t.dyX).rg, strength);
        tn = float3(tn.xy + nGeo.zy, abs(tn.z) * nGeo.x);   // pal:ok (normal swizzle)
        nX = tn.zyx;
    }
    [branch] if (t.w.y > 0.01)
    {
        float3 tn = BwUnpackRG(SAMPLE_TEXTURE2D_GRAD(tex, smp, BwWrap64(t.uvY), t.dxY, t.dyY).rg, strength);
        tn = float3(tn.xy + nGeo.xz, abs(tn.z) * nGeo.y);   // pal:ok (normal swizzle)
        nY = tn.xzy;
    }
    [branch] if (t.w.z > 0.01)
    {
        float3 tn = BwUnpackRG(SAMPLE_TEXTURE2D_GRAD(tex, smp, BwWrap64(t.uvZ), t.dxZ, t.dyZ).rg, strength);
        tn = float3(tn.xy + nGeo.xy, abs(tn.z) * nGeo.z);   // pal:ok (normal swizzle)
        nZ = tn.xyz;
    }
    float3 n = nX * t.w.x + nY * t.w.y + nZ * t.w.z;
    float l2 = dot(n, n);
    return l2 > 1e-8 ? n * rsqrt(l2) : nGeo;
}

// Palette-relative recolour of a set's albedo: the sets are generated around a reference token (ConcreteMid for the
// concrete sets); albedo = texture * (target token / reference token), so the palette stays the only colour source.
float3 BwRecolour(float3 texAlbedo, float3 targetLinear, float3 refLinear)
{
    return texAlbedo * (targetLinear / max(refLinear, 1e-4));
}

#endif
