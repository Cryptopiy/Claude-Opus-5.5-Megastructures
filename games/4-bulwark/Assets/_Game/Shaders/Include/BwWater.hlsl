// BwWater.hlsl - the sea seen through the window gallery's glass (MASTER_PROMPT 14.7 H3, 14.5.6; Addendum A1 1.8). Beat 6
// owner. Shared by BwWaterPortal (the veil behind each pane, the analytic reveals, depth for the creature), BwWaterPortalScatter
// (additive in-scatter in front of whatever lies beyond the glass: caustic shafts, the survey-lamp cones, the lamp lenses,
// the 4 % glass reflection) and BwSnow (marine snow). Include after Core.hlsl, BulwarkPalette.hlsl, BwCommon.hlsl,
// BulwarkAtmosphere.hlsl and Generated/BulwarkConstGen.hlsl.
//
// Frame: the gallery leg is straight (heading 0.86 deg, s 1035.4-1370), so the hall is an affine frame: s(p) = _BwWTan.w +
// dot(p, tangent), lateral(p) = dot(p, right) - _BwWPlane.w (axis + lateral; the inner glass face at lateral 6.0, the outer at
// 8.4), floor y(s) = -54 - 0.12 (s - 1070). Panes k = 0..9 centred at s 1085 + 30 k, 20 m wide, from floor - 8 to floor + 28.
// Globals (Beat06WindowGallery publishes them every frame; a pure function of s and the ambient clock):
//   _BwWPlane  xyz right (unit, horizontal, toward the sea), w = dot(axis point, right)
//   _BwWTan    xyz tangent (unit, along +s), w = s of the world origin's projection (s = w + dot(p, tangent))
//   _BwWState  x upper-veil gain (the arm crossing: 1 -> 0.6 and back; A1 1.10 simplified tier, keyed by s), y amber shaft
//              tint 0..1 (s 1340-1370), z glass reflection sway (rad), w caustic clock (s, unwrapped mod 2400)
//   _BwWLamp{A,B}0  xyz lamp position (world), w = gain 0..1 (log-linear ramps of A1 1.8)
//   _BwWLamp{A,B}1  xyz aim (unit), w = cos(cone half-angle)
//   _BwWLampCol     rgb lamp radiance scale (linear Tungsten x intensity), w = range (m)
//   _BwWEye    xyz the main eye's centre (world), w = glow strength (0 = none): its amber light on the snow within 7 m
//   _BwWSnow   xyz bulk flow offset of the snow (m: drift down, the displacement along +s), w = snow visibility 0..1
#ifndef BW_WATER_INCLUDED
#define BW_WATER_INCLUDED

float4 _BwWPlane;
float4 _BwWTan;
float4 _BwWState;
float4 _BwWLampA0, _BwWLampA1, _BwWLampB0, _BwWLampB1;
float4 _BwWLampCol;
float4 _BwWEye;
float4 _BwWSnow;

#define BWW_SEA_Y (-8.0)
#define BWW_GLASS_IN 6.0
#define BWW_GLASS_OUT 8.4

float BwWHallS(float3 p) { return _BwWTan.w + dot(p, _BwWTan.xyz); }
float BwWLateral(float3 p) { return dot(p, _BwWPlane.xyz) - _BwWPlane.w; }
float BwWFloorY(float s) { return -54.0 - 0.12 * (s - 1070.0); }

// Veil radiance (14.7 H3): L_c = G S_c exp(-Kd_c z) exp(beta sin e), red floor 6 % of G, hard floor (the twin of KrVeil, so
// the creature's object model and the portal agree to the bit). depth = metres below the sea surface.
float3 BwWVeil(float3 dirWS, float depth)
{
    float3 Kd = float3(TC_KdR, TC_KdG, TC_KdB); // pal:ok (attenuation coefficients, not a colour)
    float3 S = float3(TC_VeilSR, TC_VeilSG, TC_VeilSB); // pal:ok (spectral weights, not a colour)
    float G = TC_VeilG0 / (TC_VeilSG * exp(-TC_KdG * TC_VeilG0Depth));
    float e = clamp(dirWS.y, -1.0, 1.0);
    float3 L = G * S * exp(-Kd * max(depth, 0.0)) * exp(TC_VeilBeta * e);
    L.r = max(L.r, TC_VeilRedFloor * L.g);
    return max(L, TC_DarkFloorLinear);
}

// Refracted sun inside the water (unit, pointing DOWN along the light's travel), Snell n 1.33 at the flat surface.
float3 BwWSunInWater()
{
    float3 toSun = normalize(_BwSunDir.xyz + float3(0, 1e-4, 0)); // pal:ok (not a colour)
    float sinA = sqrt(saturate(1.0 - toSun.y * toSun.y));        // sine of the zenith angle in air
    float sinW = sinA / TC_Refraction;
    float cosW = sqrt(saturate(1.0 - sinW * sinW));
    float2 h = toSun.xz / max(sinA, 1e-4);
    return -normalize(float3(h.x * sinW, cosW, h.y * sinW)); // pal:ok (direction)
}

// Surface caustic C(u, v): normalised sum of 3 rotated (0.5 + 0.5 sin(k_i . x + w_i t))^6 (14.7 H3), wavelengths 5-14 m,
// each faded to its mean by the footprint (fp, m) so it never aliases. Mean of (0.5 + 0.5 sin)^6 = 0.2256.
float BwWCaustic(float2 xz, float t, float fp)
{
    const float2 d0 = float2(0.8660, 0.5000), d1 = float2(-0.6428, 0.7660), d2 = float2(0.1736, -0.9848);
    const float L0 = 5.3, L1 = 8.9, L2 = 13.7;
    float c = 0.0;
    float a0 = pow(0.5 + 0.5 * sin(dot(d0, xz) * (6.2831853 / L0) + 0.61 * t), 6.0);
    float a1 = pow(0.5 + 0.5 * sin(dot(d1, xz) * (6.2831853 / L1) + 0.43 * t + 1.7), 6.0);
    float a2 = pow(0.5 + 0.5 * sin(dot(d2, xz) * (6.2831853 / L2) + 0.29 * t + 4.1), 6.0);
    float f0 = saturate(1.0 - fp / (0.5 * L0)), f1 = saturate(1.0 - fp / (0.5 * L1)), f2 = saturate(1.0 - fp / (0.5 * L2));
    c += lerp(0.2256, a0, f0) + lerp(0.2256, a1, f1) + lerp(0.2256, a2, f2);
    return c / (3.0 * 0.2256);   // mean 1
}

// One survey lamp's irradiance factor at p: cone (smooth edge over the outer 30 % of the half-angle), inverse square with a
// 1 m core, the range window, and the water's beam attenuation along the lamp ray (c of 14.7 H3, per channel).
float3 BwWLampAt(float3 p, float4 L0, float4 L1)
{
    if (L0.w <= 1e-4) return 0.0;
    float3 v = p - L0.xyz;
    float d = length(v);
    float3 dir = v / max(d, 1e-4);
    float cosA = dot(dir, L1.xyz);
    float cosEdge = L1.w;
    float cosIn = lerp(1.0, cosEdge, 0.35);
    float cone = smoothstep(cosEdge, cosIn, cosA);
    cone *= cone;                                  // a soft beam edge (no visible cone outline)
    float range = max(_BwWLampCol.w, 1.0);
    float win = 1.0 - smoothstep(0.3 * range, range, d);   // a long, smooth fade: no visible range sphere
    float3 c = float3(TC_BeamCR, TC_BeamCG, TC_BeamCB); // pal:ok (beam attenuation, not a colour)
    return L0.w * cone * win / (1.0 + d * d) * exp(-c * d);
}

float3 BwWLamps(float3 p)
{
    return _BwWLampCol.rgb * (BwWLampAt(p, _BwWLampA0, _BwWLampA1) + BwWLampAt(p, _BwWLampB0, _BwWLampB1));
}

// The eye's own glow on nearby particles (amber, within 7 m of its surface; A1 1.8 "its glow lights the snow amber").
float3 BwWEyeGlow(float3 p)
{
    if (_BwWEye.w <= 1e-4) return 0.0;
    float d = max(length(p - _BwWEye.xyz) - 10.0, 0.0);
    return PAL_KrakenEyeGlow * (_BwWEye.w * exp(-d / 3.0) * saturate(1.0 - d / 7.0));
}

// ---------------------------------------------------------------------------------------------------------------------
// The arm above panes 1-3 (A1 1.8 "the light goes out", s 1072-1212; Beat06Layout.CrossArmAt): a round-cone SDF of three
// segments in the water (the drooping free end tip -> mid, mid -> knee along the dam face, knee -> root out into the black),
// with a row of sucker swellings on the faces looking down. Never a mesh: the portal shades it with the 14.7 H3 object model (its silhouette contrast is the
// transmittance exp(-c r): G 0.08-0.12, B 0.22-0.29 at 25-30 m, as A1 asks), the scatter cuts the in-scatter behind it and
// its shadow along the refracted sun puts the shafts out pane by pane.
//   _BwWArm0  xyz tip (world), w tip radius (<= 0: no arm this frame)
//   _BwWArm1  xyz mid (the drooping, curling free end), w radius
//   _BwWArm2  xyz knee, w radius
//   _BwWArm3  xyz root, w radius
float4 _BwWArm0, _BwWArm1, _BwWArm2, _BwWArm3;

// distance between the segments p1-q1 and p2-q2 (Ericson 5.1.9); s, t = the closest points' parameters
float BwWSegSegDist(float3 p1, float3 q1, float3 p2, float3 q2, out float s, out float t)
{
    float3 d1 = q1 - p1, d2 = q2 - p2, r = p1 - p2;
    float a = max(dot(d1, d1), 1e-8), e = max(dot(d2, d2), 1e-8), f = dot(d2, r);
    float c = dot(d1, r), b = dot(d1, d2);
    float denom = a * e - b * b;
    s = denom > 1e-8 ? saturate((b * f - c * e) / denom) : 0.0;
    t = (b * s + f) / e;
    if (t < 0.0) { t = 0.0; s = saturate(-c / a); }
    else if (t > 1.0) { t = 1.0; s = saturate((b - c) / a); }
    return length(p1 + d1 * s - (p2 + d2 * t));
}

// round cone a(r1) - b(r2), exact distance (Quilez)
float BwWRoundCone(float3 p, float3 a, float3 b, float r1, float r2)
{
    float3 ba = b - a;
    float l2 = dot(ba, ba);
    float rr = r1 - r2;
    float a2 = l2 - rr * rr;
    float il2 = 1.0 / l2;
    float3 pa = p - a;
    float y = dot(pa, ba);
    float z = y - l2;
    float3 xv = pa * l2 - ba * y;
    float x2 = dot(xv, xv);
    float y2 = y * y * l2;
    float z2 = z * z * l2;
    float k = sign(rr) * rr * rr * x2;
    if (sign(z) * a2 * z2 > k) return sqrt(x2 + z2) * il2 - r2;
    if (sign(y) * a2 * y2 < k) return sqrt(x2 + y2) * il2 - r1;
    return (sqrt(x2 * a2 * il2) + y * rr) * il2 - r1;
}

// sucker swellings, 3.5 m apart along a free segment a(ra) - b(rb), on the faces looking down (8 % of the radius: the edge
// reads as a row of bumps at 25-30 m, the SDF stays 0.6-Lipschitz for the march); u0 = arc length at a
float BwWArmBumps(float3 p, float3 a, float3 b, float ra, float rb, float u0)
{
    float3 ba = b - a;
    float l2 = max(dot(ba, ba), 1e-4);
    float u = saturate(dot(p - a, ba) / l2);
    float3 rv = p - (a + ba * u);
    float down = smoothstep(0.25, 0.8, -rv.y / max(length(rv), 1e-3));
    float La = u0 + u * sqrt(l2);
    float r = lerp(ra, rb, u);
    return pow(0.5 + 0.5 * cos(6.2831853 * La / 3.5), 3.0) * 0.08 * r * down * smoothstep(4.0, 12.0, La);
}

float BwWArmSdf(float3 p)
{
    float dA = BwWRoundCone(p, _BwWArm0.xyz, _BwWArm1.xyz, _BwWArm0.w, _BwWArm1.w);
    float dB = BwWRoundCone(p, _BwWArm1.xyz, _BwWArm2.xyz, _BwWArm1.w, _BwWArm2.w);
    float dC = BwWRoundCone(p, _BwWArm2.xyz, _BwWArm3.xyz, _BwWArm2.w, _BwWArm3.w);
    float lA = length(_BwWArm1.xyz - _BwWArm0.xyz);
    dA -= BwWArmBumps(p, _BwWArm0.xyz, _BwWArm1.xyz, _BwWArm0.w, _BwWArm1.w, 0.0);
    dB -= BwWArmBumps(p, _BwWArm1.xyz, _BwWArm2.xyz, _BwWArm1.w, _BwWArm2.w, lA);
    return min(min(dA, dB), dC);
}

// Coverage 0..1 of the arm along the ray G + d t for t in [t0, t1] (metres from the glass's inner face), and where it is
// (tHit). A sphere march (<= 48 steps) skipped unless the ray passes within the arm's radius of its axis; the coverage is
// the march's least distance against a soft width that grows with the water path (forward scattering) and the pixel
// footprint, so the silhouette is soft and never aliases.
float BwWArmTrace(float3 G, float3 d, float t0, float t1, float dist0, out float tHit)
{
    tHit = t1;
    if (_BwWArm0.w <= 0.0 || t1 <= t0 + 0.1) return 0.0;
    float sa, sb;
    float3 ra = G + d * t0, rb = G + d * t1;
    float bA = BwWSegSegDist(ra, rb, _BwWArm0.xyz, _BwWArm1.xyz, sa, sb);
    float bB = BwWSegSegDist(ra, rb, _BwWArm1.xyz, _BwWArm2.xyz, sa, sb);
    float bC = BwWSegSegDist(ra, rb, _BwWArm2.xyz, _BwWArm3.xyz, sa, sb);
    float rMax = max(max(_BwWArm1.w, _BwWArm2.w), _BwWArm3.w);
    if (min(min(bA, bB), bC) > rMax + 6.0) return 0.0;
    float tt = t0, dmin = 1e9, tAt = t1;
    [loop] for (int i = 0; i < 48; i++)
    {
        float ds = BwWArmSdf(G + d * tt);
        if (ds < dmin) { dmin = ds; tAt = tt; }
        if (ds < 0.02) break;
        tt += max(ds * 0.6, 0.05 + 0.004 * tt);
        if (tt > t1) break;
    }
    tHit = tAt;
    float w = 0.35 + 0.015 * (tAt - t0) + 1.5 * (dist0 + tAt) * _BwCam.w;
    return 1.0 - smoothstep(0.0, w, dmin);
}

// The arm as seen through rWater metres of water: its underside in the upwelling light (pale sucker skin x 0.25, half the
// lower hemisphere) under the 14.7 H3 object model L0 exp(-c r) + veil (1 - exp(-c r)).
float3 BwWArmColour(float3 P, float3 veilAlong, float rWater)
{
    float depthP = max(BWW_SEA_Y - P.y, 0.0);
    float3 L0 = PAL_KrakenSucker * (0.25 * 0.5) * BwWVeil(float3(0, -1, 0), depthP); // pal:ok (direction, not a colour)
    float3 cext = float3(TC_BeamCR, TC_BeamCG, TC_BeamCB); // pal:ok (beam attenuation c of 14.7 H3, not a colour)
    float3 T = exp(-min(cext * max(rWater, 0.0), float3(4.0, 1.6, 1.4))); // pal:ok (optical depths; the creature's KR_TAU_MAX)
    return L0 * T + veilAlong * (1.0 - T);
}

// The arm's shadow on the downwelling light at p (Lw: the refracted sun, pointing down): 0 inside, 1 clear, a penumbra of
// 5 m (the water's forward scattering softens it).
float BwWArmShadow(float3 p, float3 Lw)
{
    if (_BwWArm0.w <= 0.0) return 1.0;
    float k = max(BWW_SEA_Y - p.y, 0.0) / max(-Lw.y, 0.2);
    float3 q = p - Lw * k;
    float sa, ta, sb, tb, sc, tcc;
    float dA = BwWSegSegDist(p, q, _BwWArm0.xyz, _BwWArm1.xyz, sa, ta) - lerp(_BwWArm0.w, _BwWArm1.w, ta);
    float dB = BwWSegSegDist(p, q, _BwWArm1.xyz, _BwWArm2.xyz, sb, tb) - lerp(_BwWArm1.w, _BwWArm2.w, tb);
    float dC = BwWSegSegDist(p, q, _BwWArm2.xyz, _BwWArm3.xyz, sc, tcc) - lerp(_BwWArm2.w, _BwWArm3.w, tcc);
    return smoothstep(-1.0, 4.0, min(min(dA, dB), dC));
}

// Caustic shaft light at a water point p (single scattering of the downwelling caustic field): C at the surface point along
// the refracted sun, attenuated over the slant depth by Kd (half rate for the blue-green shaft channel, 14.7 H3).
float3 BwWShaftAt(float3 p, float3 Lw, float t, float fp)
{
    float k = (BWW_SEA_Y - p.y) / max(-Lw.y, 0.2);
    float2 q = p.xz - Lw.xz * k;
    float C = BwWCaustic(q, t, fp);
    float3 Kd = float3(TC_KdR, TC_KdG, TC_KdB); // pal:ok (attenuation, not a colour)
    float3 att = exp(-0.5 * Kd * max(BWW_SEA_Y - p.y, 0.0));
    return att * (max(C - 0.75, 0.0) * _BwWState.x);   // the bright ridges only: light added, never removed
}

#endif
