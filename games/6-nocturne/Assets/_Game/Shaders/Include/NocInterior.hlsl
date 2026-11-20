// NocInterior.hlsl - box rooms behind the lit windows of the Look v2 facade (docs/TEXTURE_PLAN.md 2.8, FA-3), owner FA.
// One room per window cell: x 0..W (the bay), y yf..yc (floor line to the slab soffit), depth 0..Dr behind the glass plane. The ray
// from the glass-plane point (xg, yg) moves (kx, ky) cell metres per metre of depth (the recess shift of NocFaceInV2), so it leaves
// the room through the back wall, a side wall, the ceiling or the floor at the depth min(Dr, A, B), with A and B the exit depths
// toward the side and toward the ceiling / floor (van Dongen's slab form). Analytic shading, each surface linear in its exit depth
// w = t / Dr: back wall FaRoomBack (0.85 + 0.3 v), lighter toward the ceiling; side walls FaRoomSide (1.3 - 0.6 w); ceiling
// FaRoomCeil (1.25 - 0.5 w), lit by the lamp near the window; floor FaRoomFloor (1.2 - 0.4 w). Because A and B are linear in the
// glass coordinates, the share of each surface over the glass and its mean exit depth are closed-form integrals (NocRoomPartV2),
// so NocRoomMeanV2 is the exact expectation for the view (within 1 % of a 48 x 48 integral up to 60 deg, InteriorTests) and the
// room factor raw / mean keeps the lit glass mean (TEXTURE_PLAN 2.9 E3). C# twin: NocInteriorMath.
#ifndef NOC_INTERIOR_INCLUDED
#define NOC_INTERIOR_INCLUDED

float NocRoomRawV2(float xg, float yg, float kx, float ky, float W, float yf, float yc, float Dr)
{
    // the glass-plane point inside the room's aperture (the neighbour cell of a 2 x 2 footprint evaluates a point outside it)
    xg = clamp(xg, 0.0, W); yg = clamp(yg, yf, yc);
    float tx = (kx >= 0.0 ? W - xg : xg) / max(abs(kx), 1e-5);
    float ty = (ky >= 0.0 ? yc - yg : yg - yf) / max(abs(ky), 1e-5);
    float t = min(Dr, min(tx, ty));
    float w = saturate(t / max(Dr, 1e-3));
    float v = saturate((clamp(yg + ky * t, yf, yc) - yf) / max(yc - yf, 1e-3));
    float back = NC_FaRoomBack * (0.85 + 0.3 * v);
    float side = NC_FaRoomSide * (1.3 - 0.6 * w);
    float cf = ky >= 0.0 ? NC_FaRoomCeil * (1.25 - 0.5 * w) : NC_FaRoomFloor * (1.2 - 0.4 * w);
    return t >= Dr - 1e-4 ? back : (tx <= ty ? side : cf);
}

// For exit depths A uniform on [A0, A1] and B uniform on [B0, B1]: the integrals over A in [A0, min(A1, Dr)] of the B-length above A
// (x: zeroth moment, y: first moment in A). The side wall's share is x / area, its mean depth y / x.
float2 NocRoomPartV2(float A0, float A1, float B0, float B1, float Dr)
{
    float Ae = min(A1, Dr), L = B1 - B0;
    float2 I = 0.0;
    float a = A0, b = min(Ae, B0);
    if (b > a) I += float2(L * (b - a), 0.5 * L * (b * b - a * a));
    float p = max(A0, B0), q = min(Ae, B1);
    if (q > p) I += float2(B1 * (q - p) - 0.5 * (q * q - p * p), 0.5 * B1 * (q * q - p * p) - (q * q * q - p * p * p) / 3.0);
    return I;
}

// Exact expectation of NocRoomRawV2 over the glass rectangle [g0, g1] x [h0, h1] for the shift (kx, ky).
float NocRoomMeanV2(float g0, float g1, float h0, float h1, float kx, float ky, float W, float yf, float yc, float Dr)
{
    const float BIG = 1e6;
    float ax = abs(kx), ay = abs(ky);
    float A0 = ax > 1e-5 ? (kx > 0.0 ? W - g1 : g0) / ax : BIG, A1 = ax > 1e-5 ? (kx > 0.0 ? W - g0 : g1) / ax : BIG + 1.0;
    float B0 = ay > 1e-5 ? (ky > 0.0 ? yc - h1 : h0 - yf) / ay : BIG, B1 = ay > 1e-5 ? (ky > 0.0 ? yc - h0 : h1 - yf) / ay : BIG + 1.0;
    float area = max((A1 - A0) * (B1 - B0), 1e-9);
    float2 S = NocRoomPartV2(A0, A1, B0, B1, Dr), C = NocRoomPartV2(B0, B1, A0, A1, Dr);
    float fS = S.x / area, fC = C.x / area, fB = max(0.0, 1.0 - fS - fC);
    float wS = S.x > 0.0 ? S.y / (S.x * Dr) : 0.0, wC = C.x > 0.0 ? C.y / (C.x * Dr) : 0.0;
    float sy = ky * Dr, yb0 = max(h0, yf - sy), yb1 = min(h1, yc - sy);
    float vb = saturate((0.5 * (yb0 + yb1) + sy - yf) / max(yc - yf, 1e-3));
    float mB = NC_FaRoomBack * (0.85 + 0.3 * vb), mS = NC_FaRoomSide * (1.3 - 0.6 * wS);
    float mC = ky >= 0.0 ? NC_FaRoomCeil * (1.25 - 0.5 * wC) : NC_FaRoomFloor * (1.2 - 0.4 * wC);
    return fB * mB + fS * mS + fC * mC;
}

#endif
