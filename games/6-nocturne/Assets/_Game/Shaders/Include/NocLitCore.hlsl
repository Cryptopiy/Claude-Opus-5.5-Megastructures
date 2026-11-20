// NocLitCore.hlsl - included FIRST (in HLSLINCLUDE) by every lit Noc* shader, before the material's UnityPerMaterial
// CBUFFER and its hooks (imported from The Ring's RingLitCore.hlsl, IMPORT_PLAN 7.10). Nocturne is a flat world: the
// render position equals the world position (no bend, theta 0), so RingBend / RingGround are not imported.
//
// A material shader provides, in its HLSLINCLUDE block after including this file:
//   CBUFFER_START(UnityPerMaterial) ... CBUFFER_END          (identical in every pass: SRP Batcher)
//   void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)   (always)
//   optional: #define NOC_HAS_VERTEX_HOOK + void NocVertexHook(inout NocVertexData v)   (world-space displacement, all passes)
//   optional: #define NOC_HAS_CLIP_HOOK + void NocClipHook(NocFragData f)   (clip() in all passes)
//   optional: #define NOC_INDIRECT  (RenderMeshIndirect draws: SV_InstanceID reaches the vertex hook, which writes the world
//             position itself; unity_ObjectToWorld is not applied)
//   optional: #define NOC_NO_FOG (only for content that is fogged inside its own hook)
//   optional: #define NOC_HAS_FORWARD_HOOK + half3 NocForwardHook(NocFragData f, SurfaceData s, InputData inputData)
//             (replaces NocShade in the forward pass; NocFog and NocFloor still follow); #define NOC_SPECULAR_OFF before
//             the forward pass include turns off the GGX point-light specular (NocWet)
// Look v2 (TEXTURE_PLAN 8.3 TP-0; each is off unless the shader defines it, so no existing picture changes):
//   optional: #define NOC_PASS_UV1ZW   the vertex stage copies uv1.zw (the facade's y0, y1) into custom.zw before the vertex hook
//   optional: #define NOC_ALPHA_OUT    the forward pass returns the surface alpha with the opaque colour (alpha to coverage)
//   optional: #define NOC_FRONT_FACE   the forward pass reads SV_IsFrontFace into f.faceSign (+1 front, -1 back; else +1)
//   f.specOcc (default 1)            multiplies the specular environment only (NocShadeOcc); s.occlusion stays on both terms
//   f.debug (default 0)              a hook writes its debug view here: rgb = display codes / 255, a = 1 (TEXTURE_PLAN 7.1);
//                                    the forward pass returns it, raw, while _NocDebugView > 0 (view 7, the world normal, is
//                                    written by the template itself)
//   NocFaceFrame(n, u, P)            the per-pixel facade frame with the measured orientation sign sigma (TEXTURE_PLAN 1.6)
// Then each pass does: #define NOC_PASS_FORWARD | NOC_PASS_SHADOW | NOC_PASS_DEPTH | NOC_PASS_DEPTHNORMALS and
// #include "Include/NocLitPass.hlsl". Every forward pass ends with NocFog, then NocFloor (14.2.2 order).
#ifndef NOC_LIT_CORE_INCLUDED
#define NOC_LIT_CORE_INCLUDED

#define _ENVIRONMENTREFLECTIONS_OFF 1
#define _SCREENSPACEREFLECTIONS_OFF 1

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
#include "NocAtmosphere.hlsl"

struct NocVertexData
{
    float3 positionOS;
    float3 positionWS;   // world position (hook may displace it)
    float3 normalWS;
    float4 tangentWS;    // w = sign
    float4 color;        // vertex colour (R = AO, G = edge wear: the Abyss MeshKit convention)
    float4 uv;           // xy = uv0, zw = uv1
    float4 custom;       // free, interpolated to the fragment
    uint instanceID;
};

struct NocFragData
{
    float4 positionCS;
    float3 positionWS;
    float3 normalWS;        // hook may perturb it (keep it normalised)
    float4 tangentWS;
    float4 color;
    float4 uv;
    float4 custom;
    float2 screenUV;
    float3 viewDirWS;
    float viewDist;
    float footprint;        // approximate pixel footprint (m) at the fragment
    float faceSign;         // +1 front face, -1 back face (NOC_FRONT_FACE; +1 otherwise)
    float specOcc;          // specular-only occlusion (NocShadeOcc), default 1
    float4 debug;           // debug view output of the hook: rgb = display codes / 255, a = 1 when written
};

float3 NocPerturbNormal(NocFragData f, float3 dn) { return normalize(f.normalWS + dn); }

// Per-pixel facade frame (TEXTURE_PLAN 1.6). u is the metric face coordinate (uv0.x) and v = world y. The writers disagree on the
// direction of u (EdgeMesh face A: u grows with z while n = +x; the z 275 notch wall n = -z with u = x; the tongue hull), so the
// sign is measured: Tp is the horizontal tangent projected into the surface, sigma = sign(ddx(u) ddx(P).Tp + ddy(u) ddy(P).Tp)
// (+1 when the sum is under 1e-12: an edge-on face is sub-pixel anyway), T = sigma Tp is the direction in which u grows and
// B = the world up projected into the surface (the direction in which v grows). The frame need not be right-handed: normals
// and shifts are expressed along dP/du and dP/dv. Takes derivatives: call it before any branch. C# twin NocTexturingMath.FaceFrame.
struct NocFrame
{
    float3 T;       // dP/du direction (unit)
    float3 B;       // dP/dv direction (unit, projected world up)
    float sigma;    // +1 or -1
};

NocFrame NocFaceFrame(float3 n, float u, float3 P)
{
    NocFrame fr;
    float3 t0 = float3(n.z, 0.0, -n.x);
    float3 tp = t0 - n * dot(n, t0);
    float lt = dot(tp, tp);
    tp = lt > 1e-12 ? tp * rsqrt(lt) : float3(1.0, 0.0, 0.0);
    float3 dPx = ddx(P), dPy = ddy(P);
    float sg = ddx(u) * dot(dPx, tp) + ddy(u) * dot(dPy, tp);
    fr.sigma = sg < -1e-12 ? -1.0 : 1.0;
    fr.T = fr.sigma * tp;
    float3 bp = float3(0.0, 1.0, 0.0) - n * n.y;
    float lb = dot(bp, bp);
    fr.B = lb > 1e-12 ? bp * rsqrt(lb) : float3(0.0, 0.0, 1.0);
    return fr;
}

#endif
