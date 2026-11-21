// BulwarkAtmosphere.hlsl - the one source of truth for sky, fog and curvature in every Bw* shader (MASTER_PROMPT
// 14.2.2, 14.7 H4, Part 10 #13). Every global below is written once per frame by AtmosphereDriver (C#), never by a
// material; global names carry the _Bw prefix and never collide with material properties (lesson 3.2 #5).
// C# twin: BwAtmosphereMath in Runtime/Core/BwCurvature.cs (same operation order; AtmosphereTests / FarFieldTests).
//
// Public API (names and signatures are stable; the lit pass, BwLine, BwBeam and the far field call them):
//   BwSkyColor(dir)              raw sky radiance along a world direction (sky state stops, sun-side glow, aureole; no
//                                disc, so reflections never pick up a firefly). Below the horizon: the horizon colour.
//   BwHorizon(az)                horizon colour toward azimuth az (deg, 0 = +Z, 90 = +X)
//   BwFogApply(color, posWS)     color exp(-tau) + fogColor (1 - exp(-tau)), tau = haze + storm slab (closed form, 14.2.2),
//                                plus the local dust medium of the beat 1 gallery (box, 14.2.4 "dust 6e-3")
//   BwFogTransmittance(posWS)    exp(-tau) only (additive effects: beams, glows)
//   BwSunBreak(posWS)            sun transmittance through the cloud hole (14.7 H2): 1 - 0.85 w (1 - hole)
//   BwDrop(d) / BwHorizonDip(h, R)  curvature (stable forms)
//   Far-field helpers (BwFarField, BwSea beyond the clipmap): BwSkyVisible(dir) = sky as seen through the whole
//   atmosphere, BwAtmoRay / BwFogApplyFar (6 + 2 sphere-exact segments, earth shadow), BwSunDisc, BwRaySphereNear.
//
// Optical depth (14.2.2): haze sigma0 exp(-h/H), H 2000 m, h = 6000 + y; along a segment whose altitude is linear
// tau = sigma0 H / sin(g) (exp(-h_end/H) - exp(-h_eye/H)), written as sigma0 e^(-h0/H) d (e^x - 1)/x, x = (h0 - h1)/H,
// which is the same number and has the |sin g| < 1e-3 limit sigma0 exp(-h/H) d built in (series for |x| < 1e-3).
// Storm slab: sigma_s between 5600 and 6400 m with 150 m linear (C0) edges, integrated exactly. Far rays (BwAtmoRay)
// split the path into 6 + 2 segments whose end altitudes are exact on the sphere, so the 100-276 km lowland and the sky
// near the horizon are integrated correctly (the single closed form misses the curvature: 0.016 instead of 0.0126 at
// 100 km, 14.2.2 check, DEVELOPMENT.md).
// Fog colour: BwSkyColor along the ray with the elevation clamped to [-1, +6] deg, faded to the azimuth mean as the ray
// steepens (no pinwheel at the nadir), desaturated by 0.2 (1 - exp(-tau)). With a low sun (dusk) the in-scatter of rays
// below the VISIBLE horizon fades from the horizon colour to the shadowed-haze colour (1 - exp(-(dep / 1.5 deg)^2)): the haze
// such rays cross lies near the dam, where sunlight reaches it only on a grazing path of optical depth
// s0 sqrt(2 pi R H) e^(-a_t/H) = 12.7 e^(-a_t/H) (s0 4.5e-5; tangent altitude a_t), i.e. below ~4 km it is unlit
// (DEVELOPMENT.md, the atmosphere report). The fade is continuous at the horizon, so horizon and fog still agree.
// The visible sky is the raw sky seen through the same atmosphere, so sky, fog and sea reflection meet at the horizon by
// construction (Part 10 #13).
// Sky stops: horizon (0), mid (12 deg), upper (35 deg), zenith (90 deg) joined by a monotone C1 cubic (Fritsch-Carlson
// slopes from the CPU, _BwSkyTan*), so every 14.2.1 stop is met exactly at its elevation (DuskRose at 12, DuskViolet at 35).
#ifndef BULWARK_ATMOSPHERE_INCLUDED
#define BULWARK_ATMOSPHERE_INCLUDED

float4 _BwCam;          // xyz camera position (world), w = vertical pixel angle (rad / px); per camera (beginCameraRendering)
float4 _BwScreen;       // x = render width, y = render height (px), z = thin-line minimum width (px), w = 1 / height
float4 _BwTime;         // x = ambient clock mod 3600, y = real time mod 3600 (BwClock); the far field uses _BwFlow instead
float4 _BwFlow;         // far-field motion offsets (m) from the UNWRAPPED double clock (no 3600 s seam): x = storm cloud
                        // wind, y = fair-weather cloud wind, z = mist-river flow (4 m/s), w = outlet-jet flow (40 m/s)
float4 _BwSunDir;       // xyz toward the sun (unit; also below the horizon), w = sun disc angular radius (rad)
float4 _BwSunColor;     // rgb key radiance at the eye (linear colour x intensity, LightCal applied; 0 = key off), w = disc clamp (60)
float4 _BwSunDisc;      // rgb sun disc radiance (clamped to 60), w = disc visibility 0..1 (storm deck)
float4 _BwSkyZenith;    // rgb zenith radiance (linear)
float4 _BwSkyUpper;     // rgb radiance at 35 deg elevation (DuskViolet at dusk; lerp(zenith, horizon, 0.225) otherwise)
float4 _BwSkyMid;       // rgb radiance at 12 deg (DuskRose at dusk; the 14.2.1 mid colour lerp(zenith, horizon, 0.45) otherwise)
float4 _BwSkyHorizon;   // rgb horizon radiance away from the sun, w = 14.2.1 mid mix (0.45)
float4 _BwSkyTanH;      // rgb slope d(colour)/d(elevation, rad) of the stop curve at the horizon (CPU Fritsch-Carlson)
float4 _BwSkyTanM;      // ... at 12 deg
float4 _BwSkyTanU;      // ... at 35 deg
float4 _BwSkyTanZ;      // ... at the zenith (0: the dome is smooth through the zenith)
float4 _BwSkyGlow;      // rgb horizon radiance toward the sun azimuth (DuskAmber at dusk), w = azimuth lobe exponent
float4 _BwSkyHalo;      // rgb aureole radiance next to the sun, w = aureole angular scale (rad)
float4 _BwSkyShape;     // x = sun-side glow elevation scale (rad), yzw = azimuth-mean fog colour (rgb) for steep rays
float4 _BwFogLow;       // low-sun in-scatter below the visible horizon: x = depression scale (rad), y = low-sun weight 0..1,
                        // z = sun-side glow fraction kept, w = its depression scale (rad)
float4 _BwAmbSky;       // trilight ambient: sky (keyed to s: exterior / interior blended at the door, portal and mouth)
float4 _BwAmbMid;       // trilight ambient: horizon band
float4 _BwAmbGround;    // trilight ambient: ground
float4 _BwAmbExtSky;    // the exterior trilight alone (far field; region-based ambient BwAmbientAt)
float4 _BwAmbExtMid;
float4 _BwAmbExtGround;
float4 _BwAmbInt;       // rgb interior ambient (lamp colour x 0.06; beat 6 cyan x 0.08), w = soft edge of the interior boxes (m)
float4 _BwIntBox[8];    // interior boxes (world): [2k] = min xyz, [2k+1] = max xyz; gallery, atrium, stairs + hall, gallery 6 + tunnel
float4 _BwFog0;         // x = sigma0 haze (1/m), y = haze scale height H (m), z = sigma_s storm slab (1/m), w = fog desaturation (0.2)
float4 _BwFog1;         // x = storm slab bottom altitude (m), y = slab top (m), z = slab edge width (m), w = altitude of world y 0 (m, 6000)
float4 _BwFogShadow;    // rgb in-scatter of shadowed haze (earth shadow at dusk, under the storm deck), w = storm-deck weight 0..1
float4 _BwDustMin;      // xyz box min of the local dust medium (beat 1 gallery), w = extinction (1/m; 0 = none)
float4 _BwDustMax;      // xyz box max
float4 _BwDustColor;    // rgb in-scatter radiance of the dust (lamp-lit)
float4 _BwSunHole;      // x = sun-break weight 0..1, yz = lit-disc centre on the lowland (world x, z, m), w = disc radius (m)
float4 _BwSea0;         // rgb far-sea body radiance (SeaStorm -> SeaTeal x 0.6), w = slope variance s2 of the smooth sea
float4 _BwFarCam;       // per camera, double on the CPU: x = eye altitude above the lowland sphere (m), y = sin^2(dip) lowland,
                        // z = sin^2(dip) sea sphere (-1: eye below the sea), w = sin^2(dip) crest sphere (-1: eye below the crest)
float4 _BwOpening;      // per camera: x = radiance clamp of the exterior seen from the eye (14.7 H8: 12 inside the dam's interior
                        // boxes, 1e4 outside, 2 m soft edge; 0 = unset = no clamp), y = the eye's interior weight
float4 _BwMouth0;       // beat-7 tunnel mouth (sky light of the threshold zone, BwMouthLight): x = face plane x, y / z = z of the
                        // west / east wall crossings, w = 1 once beat 7 has published it (0 = off)
float4 _BwMouth1;       // x / y = floor / soffit y of the opening, z = reach (m) from the opening centre, w unused

#define BW_EARTH_R 6371000.0                 // BulwarkConst.EarthRadius
#define BW_SEA_OFFSET 5992.0                 // BulwarkConst.SeaSphereOffset (sea sphere radius R + 5992)
#define BW_CREST_OFFSET 6000.0               // BulwarkConst.AltitudeOffset (road y 0 = altitude 6000)
#define BW_ATMO_SEG_NEAR 6                   // far-ray segments up to the horizon distance (BwAtmosphereMath.SegNear)
#define BW_ATMO_SEG_FAR 2                    // segments beyond it (sky rays only; BwAtmosphereMath.SegFar)
#define BW_ATMO_DEG 0.0174532925
#define BW_FOG_EL_MIN (-1.0 * BW_ATMO_DEG)   // BulwarkConst.FogElevMin
#define BW_FOG_EL_MAX (6.0 * BW_ATMO_DEG)    // BulwarkConst.FogElevMax
#define BW_TERMINATOR 300.0                  // soft earth-shadow terminator half-width (m): the sun disc 0.27 deg at ~1.6 km
#define BW_STOP_M (12.0 * BW_ATMO_DEG)        // 14.2.1 sky stops: mid (DuskRose at dusk)
#define BW_STOP_U (35.0 * BW_ATMO_DEG)        // upper (DuskViolet at dusk)
#define BW_STOP_Z (90.0 * BW_ATMO_DEG)        // zenith

float BwLumaA(float3 c) { return dot(c, float3(0.2126, 0.7152, 0.0722)); } // pal:ok (Rec.709 luminance weights)
float BwAtmoSmooth(float a, float b, float x) { float t = saturate((x - a) / (b - a)); return t * t * (3.0 - 2.0 * t); }

// ------------------------------------------------------------------------------------------------ sky
// Azimuth lobe toward the sun: ((1 + cos daz) / 2)^k, from the horizontal parts of dir and the sun direction.
float BwSunAzLobe(float3 d)
{
    float2 a = d.xz, b = _BwSunDir.xz;
    float la = length(a), lb = length(b);
    float c = (la > 1e-5 && lb > 1e-5) ? dot(a, b) / max(la * lb, 1e-10) : 0.0;
    return pow(saturate(0.5 + 0.5 * c), max(_BwSkyGlow.w, 0.0));
}

float3 BwHorizon(float azDeg)
{
    float a = azDeg * BW_ATMO_DEG;
    float3 d = float3(sin(a), 0.0, cos(a));   // pal:ok (not a colour)
    return lerp(_BwSkyHorizon.rgb, _BwSkyGlow.rgb, BwSunAzLobe(d));
}

// Cubic Hermite segment from (x0, y0, slope m0) to (x1, y1, slope m1), evaluated at x (clamped to the segment).
float3 BwHermite(float3 y0, float3 y1, float3 m0, float3 m1, float x0, float x1, float x)
{
    float h = x1 - x0;
    float t = saturate((x - x0) / h);
    float t2 = t * t, t3 = t2 * t;
    return (2.0 * t3 - 3.0 * t2 + 1.0) * y0 + (t3 - 2.0 * t2 + t) * h * m0 + (3.0 * t2 - 2.0 * t3) * y1 + (t3 - t2) * h * m1;
}

// The four 14.2.1 stops at elevation e >= 0 (rad): monotone C1 cubic, each stop met exactly at its elevation.
float3 BwSkyStops(float e)
{
    // segment selection without early returns (one evaluation, no uninitialised paths)
    bool lo = e < BW_STOP_M, mi = e < BW_STOP_U;
    float3 y0 = lo ? _BwSkyHorizon.rgb : (mi ? _BwSkyMid.rgb : _BwSkyUpper.rgb);
    float3 y1 = lo ? _BwSkyMid.rgb : (mi ? _BwSkyUpper.rgb : _BwSkyZenith.rgb);
    float3 m0 = lo ? _BwSkyTanH.rgb : (mi ? _BwSkyTanM.rgb : _BwSkyTanU.rgb);
    float3 m1 = lo ? _BwSkyTanM.rgb : (mi ? _BwSkyTanU.rgb : _BwSkyTanZ.rgb);
    float x0 = lo ? 0.0 : (mi ? BW_STOP_M : BW_STOP_U);
    float x1 = lo ? BW_STOP_M : (mi ? BW_STOP_U : BW_STOP_Z);
    return BwHermite(y0, y1, m0, m1, x0, x1, e);
}

// Vertical profile through the four stops (BwSkyStops); the sun-side glow lerps toward _BwSkyGlow with weight
// glowScale lobe(az) exp(-el / scale). d is unit length.
float3 BwSkyBase(float3 d, float glowScale)
{
    float el = asin(clamp(d.y, -1.0, 1.0));
    float e = max(el, 0.0);
    float3 c = BwSkyStops(e);
    float g = glowScale * BwSunAzLobe(d) * exp(-e / max(_BwSkyShape.x, 1e-3));
    return lerp(c, _BwSkyGlow.rgb, g);
}

// The aureole (forward scattering round the sun): _BwSkyHalo exp(-angle / width), by the TRUE direction (unit d).
float3 BwSkyHaloTerm(float3 d)
{
    float chord = length(d - _BwSunDir.xyz);                   // 2 sin(angle / 2): stable near the sun
    return _BwSkyHalo.rgb * exp(-chord / max(_BwSkyHalo.w, 1e-4));
}

// Raw sky radiance: stops + sun-side glow + aureole. No disc (BwSunDisc).
float3 BwSkyColor(float3 dir)
{
    float3 d = dir * rsqrt(max(dot(dir, dir), 1e-12));
    return max(BwSkyBase(d, 1.0) + BwSkyHaloTerm(d), 0.0);
}

// Dip of the visible lowland horizon below the eye's horizontal (rad), from the CPU sin^2 (0 when the eye is below it).
float BwVisibleDip() { return _BwFarCam.y > 0.0 ? asin(sqrt(saturate(_BwFarCam.y))) : 0.0; }

// Fog colour (in-scatter of lit haze) along dir: the sky with the elevation clamped to [-1, +6] deg. The phase-like
// terms follow the true scattering angle: the aureole uses the true direction and the sun-side glow fades as the ray
// dips below the horizon (6 deg scale), and steep rays fade to the azimuth mean (cos^4 of the elevation), so the haze
// below the sun shows no bright column and looking straight down no azimuth pinwheel. With a low sun (_BwFogLow.y, dusk)
// rays below the VISIBLE horizon fade to the shadowed-haze colour by 1 - exp(-(dep / _BwFogLow.x)^2) (grazing sunlight,
// header), keeping a little sun-side glow. Continuous with zero slope at the visible horizon. Not desaturated.
float3 BwFogColorRaw(float3 dir)
{
    float3 d = dir * rsqrt(max(dot(dir, dir), 1e-12));
    float hz = length(d.xz);
    float elTrue = atan2(d.y, max(hz, 1e-6));
    float el = clamp(elTrue, BW_FOG_EL_MIN, BW_FOG_EL_MAX);
    float2 hd = hz > 1e-6 ? d.xz / hz : float2(0.0, 1.0);
    float3 fd = float3(hd.x * cos(el), sin(el), hd.y * cos(el));   // pal:ok (not a colour)
    float glow = exp(-max(-elTrue, 0.0) / (6.0 * BW_ATMO_DEG));
    float w = hz * hz;
    float3 c = lerp(_BwSkyShape.yzw, BwSkyBase(fd, glow), w * w) + BwSkyHaloTerm(d);
    if (_BwFogLow.y > 0.0)
    {
        float dep = max(-elTrue - BwVisibleDip(), 0.0);
        float x = dep / max(_BwFogLow.x, 1e-4);
        float wLow = (1.0 - exp(-x * x)) * _BwFogLow.y;          // Gaussian onset: C1 at the horizon (no band edge)
        float3 jLow = lerp(_BwFogShadow.rgb, _BwSkyGlow.rgb, _BwFogLow.z * BwSunAzLobe(d) * exp(-dep / max(_BwFogLow.w, 1e-4)));
        c = lerp(c, jLow, wLow);
    }
    return max(c, 0.0);
}

float3 BwFogDesat(float3 c, float T) { return lerp(c, BwLumaA(c).xxx, _BwFog0.w * (1.0 - T)); }

// 14.7 H8: the exterior seen from inside an opening (door, portal, tunnel mouth) is clamped to radiance 12 (the sun disc,
// 60, would clip a large bloom through the mouth); the clamp opens over 2 m as the eye steps out (_BwOpening, per camera).
float3 BwOpeningClamp(float3 c) { return _BwOpening.x > 0.0 ? min(c, _BwOpening.x) : c; }

// Sun disc radiance along dir with analytic coverage over one pixel (pixelAngle rad) and a mild limb darkening.
float3 BwSunDisc(float3 dir, float pixelAngle)
{
    float3 d = dir * rsqrt(max(dot(dir, dir), 1e-12));
    float chord = length(d - _BwSunDir.xyz);
    float r = max(_BwSunDir.w, 1e-5);
    float cov = saturate((r - chord) / max(pixelAngle, 1e-6) + 0.5);
    float u = saturate(chord / r);
    float limb = 0.55 + 0.45 * sqrt(saturate(1.0 - u * u));
    return min(_BwSunDisc.rgb, _BwSunColor.w) * (_BwSunDisc.w * cov * limb);
}

// Per-channel transmittance of the disc for a grey line-of-sight transmittance T: blue is extinguished 1.25x and red
// 0.8x as strongly (the haze reddens the low sun; 14.2.3 keeps the 60 clamp, the Neutral curve keeps a hot core).
float3 BwDiscTransmittance(float T)
{
    float tau = -log(max(T, 1e-6));
    return exp(-tau * float3(0.8, 1.0, 1.25));   // pal:ok (extinction ratios, not a colour)
}

// ------------------------------------------------------------------------------------------------ region ambient
// Interior weight 0..1 of a world point: inside any interior box (gallery, atrium, stairs + exit hall, gallery 6 +
// tunnel), soft over +-_BwAmbInt.w at every face. The exterior seen through the door, the portal or the tunnel mouth
// keeps the exterior ambient (Part 10 #36, 14.6). C# twin: BwAtmosphereMath.InteriorWeightAt.
float BwInteriorWeight(float3 p)
{
    float e = max(_BwAmbInt.w, 1e-3);
    float w = 0.0;
    [unroll] for (int k = 0; k < 4; k++)
    {
        float3 a = saturate((p - _BwIntBox[2 * k].xyz) / (2.0 * e) + 0.5);
        float3 b = saturate((_BwIntBox[2 * k + 1].xyz - p) / (2.0 * e) + 0.5);
        float3 i3 = min(a, b);
        w = max(w, i3.x * i3.y * i3.z);
    }
    return w;
}

// Lambert's polygon formula, one edge: the angle the edge (unit directions a -> b from the shaded point) subtends times
// the unit normal of the plane through the point and the edge.
float3 BwEdgeIrr(float3 a, float3 b)
{
    float3 x = cross(a, b);
    float l = length(x);
    return l > 1e-6 ? x * (acos(clamp(dot(a, b), -1.0, 1.0)) / l) : x;
}

// Sky light through the tunnel mouth (the threshold zone of beat 7): the opening (the rectangle where the tunnel cuts
// the lee face, x = _BwMouth0.x, z0..z1, floor..soffit) is an area light of sky radiance for points inside the dam.
// Form factor F = n . Phi / 2 pi (Lambert: Phi = sum of the edge terms, vertices ordered so Phi points out through the
// opening from inside), radiance = the sky (the lowland's ground ambient below the horizon) along Phi; returns F x L,
// the outgoing-radiance-per-albedo term BwAmbientAt adds to the INTERIOR ambient (so it follows the interior weight and
// joins the exterior trilight continuously at the face). 0 west of the face (rising over 0.25 m inside it), beyond the
// reach (it fades over its last 30 %; F there is below 1e-3) or before beat 7 publishes the mouth.
float3 BwMouthLight(float3 n, float3 p)
{
    float3 c = float3(_BwMouth0.x, 0.5 * (_BwMouth1.x + _BwMouth1.y), 0.5 * (_BwMouth0.y + _BwMouth0.z));   // pal:ok (point)
    float3 dc = c - p;
    float dist = length(dc);
    float r = max(_BwMouth1.z, 1e-3);
    float inside = saturate((p.x - _BwMouth0.x) / 0.25);
    if (_BwMouth0.w < 0.5 || dist > r || inside <= 0.0) return 0.0;
    float3 A = normalize(float3(_BwMouth0.x, _BwMouth1.x, _BwMouth0.y) - p);   // pal:ok (point)
    float3 B = normalize(float3(_BwMouth0.x, _BwMouth1.x, _BwMouth0.z) - p);   // pal:ok (point)
    float3 C = normalize(float3(_BwMouth0.x, _BwMouth1.y, _BwMouth0.z) - p);   // pal:ok (point)
    float3 D = normalize(float3(_BwMouth0.x, _BwMouth1.y, _BwMouth0.y) - p);   // pal:ok (point)
    float3 phi = BwEdgeIrr(A, B) + BwEdgeIrr(B, C) + BwEdgeIrr(C, D) + BwEdgeIrr(D, A);
    float F = saturate(dot(n, phi) * 0.159154943);
    float lp = length(phi);
    float3 d = lp > 1e-6 ? phi / lp : dc / max(dist, 1e-6);
    float3 hd = float3(d.x, max(d.y, 0.0), d.z);                                  // pal:ok (not a colour)
    hd = dot(hd, hd) > 1e-6 ? hd : float3(-1, 0, 0);                              // pal:ok (straight down: out of the face)
    float3 L = lerp(BwSkyColor(hd), _BwAmbExtGround.rgb, smoothstep(0.0, 0.35, -d.y));
    return (F * inside * (1.0 - smoothstep(0.7 * r, r, dist))) * L;
}

// Region-based trilight ambient at a world point (the lit pass may call this instead of the s-keyed _BwAmb* trilight),
// plus the sky light through the tunnel mouth for points of the tunnel's threshold zone (BwMouthLight).
float3 BwAmbientAt(float3 normalWS, float3 posWS)
{
    float u = normalWS.y;
    float sky = saturate(u), ground = saturate(-u), mid = 1.0 - sky - ground;
    float3 ext = _BwAmbExtSky.rgb * sky + _BwAmbExtMid.rgb * mid + _BwAmbExtGround.rgb * ground;
    float3 inn = _BwAmbInt.rgb * (sky + mid + 0.8 * ground);
    return lerp(ext, inn + BwMouthLight(normalWS, posWS), BwInteriorWeight(posWS));
}

// ------------------------------------------------------------------------------------------------ optical depth
// Haze along a segment whose altitude runs linearly from h0 to h1 over length d (14.2.2 closed form).
float BwHazeTau(float h0, float h1, float d)
{
    float H = max(_BwFog0.y, 1.0);
    float x = (h0 - h1) / H;
    float g = abs(x) < 1e-3 ? 1.0 + 0.5 * x : (exp(x) - 1.0) / x;
    return _BwFog0.x * exp(-h0 / H) * d * g;
}

// Storm slab: trapezoid density (linear edges of width w centred on the slab bounds) integrated exactly through the
// ramp antiderivative R(h; a) = 0 | (h - a)^2 / 2w | h - a - w/2. Near-horizontal segments (|dh| < 10 m) use
// Simpson's rule instead of a difference of antiderivatives (lesson 3.2 #12).
float BwRampR(float h, float a, float w)
{
    float t = h - a;
    return t <= 0.0 ? 0.0 : (t < w ? t * t / (2.0 * w) : t - 0.5 * w);
}
float BwSlabDensity(float h, float lo, float hi, float w)
{
    return saturate((h - (lo - 0.5 * w)) / w) - saturate((h - (hi - 0.5 * w)) / w);
}
float BwStormTau(float h0, float h1, float d)
{
    if (_BwFog0.z <= 0.0) return 0.0;
    float lo = _BwFog1.x, hi = _BwFog1.y, w = max(_BwFog1.z, 1.0);
    float dh = h1 - h0;
    float frac;
    if (abs(dh) < 10.0)
        frac = (BwSlabDensity(h0, lo, hi, w) + 4.0 * BwSlabDensity(0.5 * (h0 + h1), lo, hi, w) + BwSlabDensity(h1, lo, hi, w)) / 6.0;
    else
    {
        float a1 = lo - 0.5 * w, a2 = hi - 0.5 * w;
        float i1 = BwRampR(h1, a1, w) - BwRampR(h1, a2, w);
        float i0 = BwRampR(h0, a1, w) - BwRampR(h0, a2, w);
        frac = saturate((i1 - i0) / dh);
    }
    return _BwFog0.z * frac * d;
}

// Local dust medium (beat 1 gallery box): optical depth of the part of the segment a -> b inside the box.
float BwDustTau(float3 a, float3 b)
{
    if (_BwDustMin.w <= 0.0) return 0.0;
    float3 v = b - a;
    float3 inv = 1.0 / (abs(v) > 1e-6 ? v : (v >= 0.0 ? 1e-6 : -1e-6));
    float3 t0 = (_BwDustMin.xyz - a) * inv, t1 = (_BwDustMax.xyz - a) * inv;
    float3 tn = min(t0, t1), tf = max(t0, t1);
    float enter = max(max(tn.x, tn.y), max(tn.z, 0.0));
    float leave = min(min(tf.x, tf.y), min(tf.z, 1.0));
    return _BwDustMin.w * max(leave - enter, 0.0) * length(v);
}

float BwFogTau(float3 posWS)
{
    float3 v = posWS - _BwCam.xyz;
    float d = length(v);
    float h0 = _BwFog1.w + _BwCam.y;
    float h1 = _BwFog1.w + posWS.y;
    return BwHazeTau(h0, h1, d) + BwStormTau(h0, h1, d);
}

float BwFogTransmittance(float3 posWS) { return exp(-(BwFogTau(posWS) + BwDustTau(_BwCam.xyz, posWS))); }

float3 BwDustApply(float3 c, float3 a, float3 b)
{
    float Td = exp(-BwDustTau(a, b));
    return c * Td + _BwDustColor.rgb * (1.0 - Td);
}

// colour = colour exp(-tau) + fogColor (1 - exp(-tau)); fogColor = sky along the same ray with the elevation clamped
// to [-1, +6] deg, desaturated by 0.2 (1 - exp(-tau)). Horizon and fog agree by construction (Part 10 #13).
// Wave X (ending): the true penumbra. The lit fraction is the share of the sun disc (radius 0.265 deg) above the point's own
// horizon: x = (el + dip(h)) / r_sun, f = (acos(-x) + x sqrt(1 - x^2)) / pi, dip(h) = acos(R / (R + h)) in the stable form
// atan(sqrt(h (2R + h)) / R). At el -2.35 the lower limb clears the horizon at 6638 m and the upper limb at 4219 m: the
// 2.4 km alpenglow band of 3.7 (crest fraction 0.81; the crest goes dark at el -2.752); the cabin's sunset fades over the
// limb altitudes 5467 -> 3421 m (tau 26.9 -> 64) instead of over +-300 m. In daylight (x >= 1) it is exactly 1.
#define BW_SUN_RADIUS_RAD 0.0046251             // 0.265 deg
float BwDiscFraction(float x)
{
    x = clamp(x, -1.0, 1.0);
    return saturate((acos(-x) + x * sqrt(saturate(1.0 - x * x))) * 0.31830989);
}
float BwHorizonDipRad(float h)
{
    float hh = max(h, 0.0);
    return atan(sqrt(hh * (2.0 * BW_EARTH_R + hh)) / BW_EARTH_R);
}
// se = sin(sun elevation) above the plane normal to the point's local up.
float BwEarthLitN(float h, float se)
{
    float el = asin(clamp(se, -1.0, 1.0));
    return BwDiscFraction((el + BwHorizonDipRad(h)) / BW_SUN_RADIUS_RAD);
}
// Alpenglow reddening (3.7): the grazing ray to a point at altitude h passes its tangent point at h_t = h - R el^2 / 2; air
// mass M(h_t) = 38 exp(-h_t / 8400), transmittance exp(-k M), k = (0.12, 0.19, 0.32). The key colour is the crest's, so the
// tint is exp(-k (M(h_t) - M(h_t,crest))) <= 1: orange at the crest, deep red at the bottom of the band. 1 in daylight.
float3 BwAlpenTint(float h, float se)
{
    if (se >= 0.0) return float3(1.0, 1.0, 1.0);   // pal:ok (not a colour: a unit transmittance)
    float el = asin(clamp(se, -1.0, 0.0));
    float drop = 0.5 * BW_EARTH_R * el * el;
    float dM = 38.0 * (exp(-(h - drop) / 8400.0) - exp(-(BW_CREST_OFFSET - drop) / 8400.0));
    return exp(-float3(0.12, 0.19, 0.32) * max(dM, 0.0));   // pal:ok (extinction coefficients)
}
float BwEarthLit(float h)
{
    return BwEarthLitN(h, _BwSunDir.y);
}

float3 BwFogApply(float3 color, float3 posWS)
{
    float3 v = posWS - _BwCam.xyz;
    float d = max(length(v), 1e-4);
    float T = exp(-BwFogTau(posWS));
    float3 fogRaw = BwFogColorRaw(v / d);
    // Wave X (ending): haze inside the earth's shadow scatters the shadowed colour, as the far ray does (BwAtmoSegment), so a
    // near object on the dusk lowland is not veiled in the lit rose of the anti-sun horizon while the far face beyond it is
    // veiled in the shadow colour (pink trees against a violet wall, ending-20 KS27: 74/40/57 vs 45/37/72). Weight: the unlit
    // share of the sun disc at the segment's mid altitude. Daylight (sun above the horizon) skips it.
    if (_BwSunDir.y < 0.0)
        fogRaw = lerp(fogRaw, _BwFogShadow.rgb, 1.0 - BwEarthLit(_BwFog1.w + 0.5 * (_BwCam.y + posWS.y)));
    float3 fogc = BwFogDesat(fogRaw, T);
    float3 c = color * T + fogc * (1.0 - T);
    return BwDustApply(c, _BwCam.xyz, posWS);
}

// ------------------------------------------------------------------------------------------------ sun break (14.7 H2)
// The lit disc on the lowland (centre _BwSunHole.yz, radius .w) is where the sun shines through the deck hole; a point
// P at altitude h is in that beam when its sun shadow on the ground, P - sunDir h / sunDir.y, falls in the disc.
// T_sun = 1 - 0.85 w (1 - hole); w = 0 outside the sun-break window (the beat 2 key 0.35 is already the hidden sun).
float BwSunBreakAt(float2 xz, float h)
{
    float w = _BwSunHole.x;
    if (w <= 0.0) return 1.0;
    float3 sd = _BwSunDir.xyz;
    float2 pg = xz - sd.xz * (max(h, 0.0) / max(sd.y, 0.02));
    float r = _BwSunHole.w;
    float hole = saturate((r - length(pg - _BwSunHole.yz)) / 3000.0 + 0.5) * saturate(r / 3000.0);
    return 1.0 - 0.85 * w * (1.0 - hole);
}
// The main light of every lit shader (the earth's shadow is BwEarthLit above: the sun disc's lit share at the altitude): the sun-break hole (14.7 H2) and the earth's shadow at the surface altitude.
float BwSunBreak(float3 posWS) { float h = _BwFog1.w + posWS.y; return BwSunBreakAt(posWS.xz, h) * BwEarthLit(h); }
// Openness 0..1 of the sun-break beam at a point (0 outside the break window).
float BwSunHoleAt(float2 xz, float h)
{
    float w = _BwSunHole.x;
    if (w <= 0.0) return 0.0;
    return saturate((BwSunBreakAt(xz, h) - (1.0 - 0.85 * w)) / max(0.85 * w, 1e-4));
}

// ------------------------------------------------------------------------------------------------ curvature (14.7 H4)
// Drop of the sphere below the tangent plane at horizontal distance d: 2 R sin^2(d / 2R) (stable; never R (1 - cos)).
float BwDrop(float d) { float s = sin(d / (2.0 * BW_EARTH_R)); return 2.0 * BW_EARTH_R * s * s; }
// Horizon dip below horizontal for an eye at height h above a sphere of radius R: sin^2(dip) = h (2R + h) / (R + h)^2.
float BwHorizonDip(float h, float R) { return asin(sqrt(saturate(h * (2.0 * R + h) / ((R + h) * (R + h))))); }

// Near intersection of a ray of elevation sine dy from an eye at distance rEye from the sphere centre, the eye OUTSIDE
// the sphere, sin2Dip = (rEye^2 - r^2) / rEye^2 (CPU, double). No-cancellation forms (14.7 H4):
//   disc' = (-dy - sin dip)(-dy + sin dip),  t = rEye sin^2 dip / (-dy + sqrt(disc'))  (= rEye (-dy - sqrt disc')).
// Returns -1 above the horizon.
float BwRaySphereNear(float rEye, float sin2Dip, float dy)
{
    if (sin2Dip <= 0.0) return -1.0;
    float sd = sqrt(sin2Dip);
    if (-dy <= sd) return -1.0;
    float disc = (-dy - sd) * (-dy + sd);
    return rEye * sin2Dip / (-dy + sqrt(disc));
}

// Far intersection with a sphere of altitude hTop above the lowland for an eye at altitude h0 below it.
float BwRaySphereExit(float h0, float hTop, float dy)
{
    float r0 = BW_EARTH_R + h0;
    float q = (hTop - h0) * (2.0 * BW_EARTH_R + hTop + h0);        // (R+hTop)^2 - (R+h0)^2, stable
    float b = r0 * dy;
    float s = sqrt(max(b * b + q, 0.0));
    return b > 0.0 ? q / (b + s) : s - b;
}

// Altitude above the lowland sphere at distance t along a ray of elevation sine dy from an eye at altitude h0:
// ((R + h0)^2 + 2 (R + h0) t dy + t^2)^(1/2) - R in the stable form N / (sqrt(.) + R).
float BwAltAt(float h0, float dy, float t)
{
    float r0 = BW_EARTH_R + h0;
    float n = h0 * (2.0 * BW_EARTH_R + h0) + t * (2.0 * r0 * dy + t);
    float rr = r0 * r0 + t * (2.0 * r0 * dy + t);
    return n / (sqrt(max(rr, 0.0)) + BW_EARTH_R);
}

// Haze optical depth of the part of a linear-altitude segment that lies above altitude hx.
float BwHazeTauAbove(float hA, float hB, float d, float hx)
{
    if (hA <= hx && hB <= hx) return 0.0;
    if (hA >= hx && hB >= hx) return BwHazeTau(hA, hB, d);
    float f = (hx - hA) / (hB - hA);
    return hA > hx ? BwHazeTau(hA, hx, d * f) : BwHazeTau(hx, hB, d * (1.0 - f));
}

// Signed earth-shadow altitude at a point whose local up is n: R (1/cos el - 1) when the sun is below the local horizon
// (air below it is in the shadow), the negative of it when the sun is above (everything lit). At el 0 it is 0, so the
// soft terminator (+-300 m, the sun disc) is continuous through sunset.
float BwShadowAltitude(float3 n)
{
    float se = dot(_BwSunDir.xyz, n);
    float ce = sqrt(saturate(1.0 - se * se));
    float hs = BW_EARTH_R * se * se / max(ce * (1.0 + ce), 1e-6);
    return se >= 0.0 ? -hs : hs;
}

// The atmosphere along a far ray (camera-relative; the lowland sphere centre is below the eye): 6 uniform segments up to
// min(tEnd, tH) and 2 more from tH to tEnd (tH = (R+h) sin(dip), the distance to the horizon), with exact end altitudes,
// the closed form per segment and the in-scatter J = fogColor, or _BwFogShadow.rgb for the haze inside the earth's shadow
// (soft terminator +-300 m). Rays just above and just below the horizon share their nodes up to tH, so the visible sky
// and the fogged lowland meet without a step. Out: transmittance T and the in-scatter L (not yet desaturated).
void BwAtmoSegment(float3 dir, float3 C, float tA, float hA, float tB, float hB, float3 jLit, inout float T, inout float3 L)
{
    float d = tB - tA;
    float tauH = BwHazeTau(hA, hB, d);
    float tau = tauH + BwStormTau(hA, hB, d);
    float3 J = jLit;
    if (tauH > 1e-7)
    {
        float3 pm = dir * (0.5 * (tA + tB));
        float3 n = normalize(pm - C);
        float hs = BwShadowAltitude(n);
        float wE = 0.5 * (BwHazeTauAbove(hA, hB, d, hs - BW_TERMINATOR) + BwHazeTauAbove(hA, hB, d, hs + BW_TERMINATOR)) / tauH;
        float wD = 1.0;
        if (_BwFogShadow.w > 0.0)
        {
            // haze under the storm deck (below the slab) is lit only diffusely, except in the sun-break beam
            float deck = _BwFog1.x - 0.5 * _BwFog1.z;
            float under = 1.0 - BwHazeTauAbove(hA, hB, d, deck) / tauH;
            float hole = BwSunHoleAt(_BwCam.xz + pm.xz, 0.5 * (hA + hB));
            wD = 1.0 - under * _BwFogShadow.w * (1.0 - hole);
        }
        // the storm slab below the eye is lit from above through the cloud: its in-scatter fades toward the shadowed colour
        // with the depth below the eye (400 m, half the slab) for rays below the VISIBLE horizon (Gaussian onset over 1.5 deg
        // of depression, as the low-sun fade), so a ray looking down through the deck shows the ground (KS05) while the
        // horizon, the sea and the sky (rays at or above the horizon) are unchanged and still meet the fog
        float depS = max(-asin(clamp(dir.y, -1.0, 1.0)) - BwVisibleDip(), 0.0) / (1.5 * BW_ATMO_DEG);
        float sLit = 1.0 - _BwFogShadow.w * saturate((_BwFarCam.x - 0.5 * (hA + hB)) / 400.0) * (1.0 - exp(-depS * depS));
        float w = saturate((min(wE, wD) * tauH + sLit * (tau - tauH)) / max(tau, 1e-9));
        J = lerp(_BwFogShadow.rgb, jLit, w);
    }
    float e = exp(-tau);
    L += T * (1.0 - e) * J;
    T *= e;
}

void BwAtmoRay(float3 dir, float tEnd, float3 jLit, out float T, out float3 L)
{
    float h0 = _BwFarCam.x;
    float dy = dir.y;
    float3 C = float3(0.0, -(BW_EARTH_R + h0), 0.0);   // pal:ok (not a colour)
    float tH = _BwFarCam.y > 0.0 ? (BW_EARTH_R + h0) * sqrt(_BwFarCam.y) : tEnd;
    float tN = min(tEnd, tH);
    T = 1.0; L = 0.0;
    float tA = 0.0, hA = h0;
    [unroll]
    for (int i = 1; i <= BW_ATMO_SEG_NEAR; i++)
    {
        float tB = tN * (float)i / (float)BW_ATMO_SEG_NEAR;
        float hB = BwAltAt(h0, dy, tB);
        BwAtmoSegment(dir, C, tA, hA, tB, hB, jLit, T, L);
        tA = tB; hA = hB;
    }
    if (tEnd > tN)
    {
        [unroll]
        for (int k = 1; k <= BW_ATMO_SEG_FAR; k++)
        {
            float tB = tN + (tEnd - tN) * (float)k / (float)BW_ATMO_SEG_FAR;
            float hB = BwAltAt(h0, dy, tB);
            BwAtmoSegment(dir, C, tA, hA, tB, hB, jLit, T, L);
            tA = tB; hA = hB;
        }
    }
}

// Far-ray fog: color T + desat(L); the same numbers as BwFogApply for short rays (one linear segment there).
float3 BwFogApplyFar(float3 color, float3 dir, float t)
{
    float T; float3 L;
    BwAtmoRay(dir, t, BwFogColorRaw(dir), T, L);
    float3 c = color * T + BwFogDesat(L, T);
    return BwDustApply(c, _BwCam.xyz, _BwCam.xyz + dir * t);
}

// Distance along a ray that misses the ground to where the atmosphere ends (slab top + edge or the eye, + 3 H).
float BwAtmoExit(float3 dir)
{
    float hTop = max(_BwFog1.y + _BwFog1.z, _BwFarCam.x) + 3.0 * max(_BwFog0.y, 1.0);
    return BwRaySphereExit(_BwFarCam.x, hTop, dir.y);
}

// The sky as seen from the eye: the raw sky through the whole atmosphere (no disc). Rays below the visible horizon
// return the horizon colour seen through the atmosphere up to the tangent point (used by sea reflections).
float3 BwSkyVisible(float3 dir)
{
    float3 d = dir * rsqrt(max(dot(dir, dir), 1e-12));
    float tl = BwRaySphereNear(BW_EARTH_R + _BwFarCam.x, _BwFarCam.y, d.y);
    float t = tl > 0.0 ? tl : BwAtmoExit(d);
    return BwFogApplyFar(BwSkyColor(d), d, t);
}

// The smooth sea beyond the clipmap (14.7 H1, H4): Fresnel mix of the body radiance and the sky as it reaches the sea
// (BwSkyVisible of the reflected ray, so the sea horizon equals the fog and the sky: Part 10 #13), plus a clamped Beckmann
// glint of variance s2 (none while the storm deck hides the sun). dir = view ray, n = surface normal (unit). Used by
// BwFarField (sea sphere) and meant for every sea surface that has to meet it (graybox plane, BwSea far tier, T7).
// BwSeaSurfaceS2: the same with the slope variance given: the far field passes the sea owner's BwSeaFarS2() (BwSeaWaves.hlsl, the variance
// the clipmap reaches at 2048 m), so the clipmap edge is invisible (14.7 H1 "beyond the 2048 m clipmap the sea sphere of
// BwFarField uses the same function", T7).
float3 BwSeaSurfaceS2(float3 dir, float3 n, float s2v)
{
    float mu = max(-dot(dir, n), 1e-4);
    float F = 0.02 + 0.98 * pow(1.0 - mu, 5.0);
    float3 r = reflect(dir, n);
    r.y = max(r.y, 1e-3);
    float3 c = lerp(_BwSea0.rgb, BwSkyVisible(r), F);
    float s2 = max(s2v, 1e-3);
    float3 hv = normalize(_BwSunDir.xyz - dir);
    float ch = saturate(dot(n, hv));
    float ch2 = max(ch * ch, 1e-4);
    float D = exp(-(1.0 - ch2) / (ch2 * s2)) / (3.14159265 * s2 * ch2 * ch2);
    float Fh = 0.02 + 0.98 * pow(1.0 - saturate(dot(-dir, hv)), 5.0);
    float g = D * Fh * 0.25 * saturate(dot(n, _BwSunDir.xyz)) / max(mu, 0.05);
    return c + min(_BwSunColor.rgb * g, 8.0) * _BwSunDisc.w;
}
float3 BwSeaSurface(float3 dir, float3 n) { return BwSeaSurfaceS2(dir, n, _BwSea0.w); }

#endif
