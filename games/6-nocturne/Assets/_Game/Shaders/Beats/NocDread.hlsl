// NocDread.hlsl - shared helpers of package DR (docs/DREAD.md 2.1; owner DR): the hull's deck veil, the analytic occluder of the
// city-lit cloud base. Included by NocHull (its fog colour) and NocDreadVeil (the veil quad), so both darken the deck alike.
// Globals (written every frame by DreadDriver; all zero = no hull: every factor is exactly 1):
//   _NocDreadHull   x = hull axis x (m), y = veil centre z (m), z = veil strength k (0..1), w = veil plane y (m, 456)
//   _NocDreadHullR  x = veil half-width (m, across), y = half-length (m, along z), z = inner radius r0 of the soft edge, w = 0
// Factor along a ray: m = 1 - k shape(V.xz) T(V), V = the ray's crossing of the veil plane, shape = 1 - smoothstep(r0, 1, r),
// r = |((x - X) / hw, (z - Z) / hl)|, T = the fog transmittance to V (the deck seen through the haze darkens by its own share only).
#ifndef NOC_DREAD_INCLUDED
#define NOC_DREAD_INCLUDED

float4 _NocDreadHull;
float4 _NocDreadHullR;

float NocDreadVeilShape(float2 xz)
{
    float2 h = max(_NocDreadHullR.xy, 1.0);
    float r = length((xz - _NocDreadHull.xy) / h);
    return 1.0 - smoothstep(_NocDreadHullR.z, 1.0, r);
}

// The veil factor at a point V on the veil plane.
float NocDreadVeilAt(float3 V)
{
    return 1.0 - saturate(_NocDreadHull.z) * NocDreadVeilShape(V.xz) * NocFogT(V);
}

// The veil factor of the deck seen along the ray from the camera through P (1 when the ray does not climb to the plane).
float NocDreadVeilRay(float3 P)
{
    float3 cam = _WorldSpaceCameraPos.xyz;
    float3 d = P - cam;
    float m = 1.0;
    [branch] if (_NocDreadHull.z > 0.0 && d.y > 1e-3)
    {
        float t = (_NocDreadHull.w - cam.y) / d.y;
        m = NocDreadVeilAt(cam + d * t);
    }
    return m;
}

#endif
