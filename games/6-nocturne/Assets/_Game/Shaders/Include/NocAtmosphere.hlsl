// NocAtmosphere.hlsl - the one source of truth for atmosphere (MASTER_PROMPT 14.2.2, 14.7 H1 / H3, D-006, R2 section 2),
// included by every custom shader. Interface: NocSkyColor(dir), NocSkyColorBlur(dir, blurRad), NocFog(col, posWS),
// NocFloor(col); helpers NocOpticalDepth, NocCloudColor, NocCloudMix. The C# double mirror is NocFogMath (NocSchedule.cs);
// AtmosphereTests checks both against 14.2.2 / H3. Order in every forward pass: shade, NocFog (cloud mix, then fog toward
// NocSkyColor(ray)), NocFloor last. Unity fog is off; no FOG_* keywords anywhere.
// Globals (written once per frame from s by the AtmosphereDriver, never by a material):
//   _NocFog        x = sigma_r (regional sigma_beat, 1/m), y = alley haze extra sigma_a - sigma_r (D-006), z = sheet strength
//                  s_w (0.5, 0 when cut, 14.8 item 9), w = rain rho (the rain pass and the ripples)
//   _NocFogSheets  xyz = haze-sheet heights y_k (m, with the +-15 m / 120 s drift), w = sheet width (45 m)
//   _NocSheetE     xyz = sheet elevations e_k (deg): the centres of the art-directed sky bands of the three sheets (H3: where a ray
//                  crosses a sheet 2.5 km out, -5.7 / -15.6 / -31 undrifted), w = sheet scale (1, 0 when the sheets are cut)
//   _NocAbyss      xy = sheet billow scroll (m, wrapped to NA_SheetScrollPeriod), zw = deep-glow scroll (wrapped to NA_DeepScrollPeriod);
//                  both are added in the rotated frame of every octave (exact wraps)
//   _NocCloud      xy = octave-1 scroll (m, wrapped to 64 x 900 m), zw = octave-2 scroll (wrapped to 64 x 300 m)
//   _NocCloud2     xy = octave-3 scroll (wrapped to 64 x 110 m), zw = octave-4 scroll (wrapped to 64 x 40 m)
//   _NocGlowSeg[8] (ax, az, bx, bz) glow centre segments (m); _NocGlowPar[8] (amplitude, sigma m, ring radius m, tint: 0 CloudHot,
//                  1 CloudCool, 2 CloudBase, 3 = the Gulf shadow multiplier of the base deck);
//   _NocGlowCount  number of active glows (boulevard axis, sign wall, spire, koi ring; beats may feed their own centres)
//   _NocAmber      x = k_a(s) = w_f(s) / 0.32 (LOOK_V2 R6: every city-wide SodiumAmber use x k_a, the remainder to SignWhite;
//                  1 to s 400, 0.625 at s 880, 0 from s 1 250), y = the screen wall's amber-stop weight (1 -> 0 over s 1 230-1 290)
//   _NocDeckOcc    D-034 hook (DREAD 2.1, wave B): the hull in the deck plane, xy centre x / z (m), z half-length along z (m),
//                  w strength; a soft ellipse (beam 0.15 x length) that darkens the city glow of the deck. Zero = identity.
//   _NocHazeOcc    D-034 hook (DREAD 3.1, wave B): the shadow in the abyss haze (plane y -650), same layout; it darkens the deep
//                  glow and the sheets seen against it. Zero = identity.
//   _NocAir        fix wave (AT, p4_01): x = pool amplitude A of the under-city glow pools (C11), y = the sink veil V of surfaces
//                  below the walk (C11; 0 when the sheets are cut), z = koi air gain x KoiK (C02-air; 0 outside beat 7), w = the
//                  alley lift of the deck over the KS02 slit (C44). The values are C# constants (NocAir in NocSchedule.cs) so
//                  they tune without a shader recompile.
//   _NocFlash      D-034 5.4 sheet lightning (C26): xy centre in the deck (m), z sigma (m), w amplitude (<= 0.15 x the envelope; 0 off)
//   _NocKoiAir[12] the koi's 12 spine points (xyz, m) for the in-scatter of its light in the air (C02-air; HT's KoiDriver writes them
//                  into AtmosphereDriver.KoiSpine, uploaded here under AT's own name: NocLighting.hlsl declares _NocKoiSpine)
//   The Ark (D-043, AT: the 5 km ship of DR's course in the haze; written by the AtmosphereDriver from DR's course, the values are
//   NocArkAir constants in NocSchedule.cs, so they tune without a shader recompile; all zero = identity: no Ark before its launch):
//   _NocArk        x axis x (m), y centre z (m), z half-length (m), w deck-occluder strength (0 = no Ark)
//   _NocArkB       x half-beam (m), y penumbra (m), z soft end length (m), w the base deck's darkening share
//   _NocArkG       glow of its lights in the haze: x tube axis y (m), y tube radius (m), z gain (radiance across the tube on a ray
//                  through its axis; 0 off), w the sheet-lightning stretch along z (1 = round)
//   _NocArkL       searchlights: x on (> 0), y pool gain on the haze sheets, z fall-off length along a beam (m), w apex radius r0 (m)
//   _NocArkS       x the lit-spot gain where a beam meets a surface, y the spot's reference radius (m)
//   _NocArkBeamP[6] apex (xyz, m), w gain (radiance across the beam at the apex; 0 = off); _NocArkBeamD[6] unit axis (xyz, down),
//                  w tan of the half-angle (DR's emitters: HullCourse.Searchlight)
// The camera is at y ~1.7, so everything is a function of direction and distance. Colours are linear PAL_* tokens.
#ifndef NOC_ATMOSPHERE_INCLUDED
#define NOC_ATMOSPHERE_INCLUDED

#include "NocCommon.hlsl"

#define NOC_GLOW_MAX 8

// ------------------------------------------------------------------ AT constants (Phase 4: the abyss of H3 and the deck form)
// C# twin: NocAbyss in NocSchedule.cs (AtmosphereTests.AbyssConstants_HlslMatchesCSharp parses every "#define NA_" line here).
// Base gradient below the horizon: lerp(AbyssNear, AbyssGlow, 1 - (1 - |e| / 90)^1.3) (monotone, flat at the nadir; the glow rises
// faster than v1's smoothstep, so the shallow drop of KS09 / KS10 / KS12 reads 45-58 instead of 40-47: LOOK_V2 8.1 drop 50-66).
// Haze sheets as geometry fog: sigma x s_w x the Gaussian sheet layers (H3; s_w 0.5 -> 1.5 so a face visibly loses contrast under
// each sheet). Sky bands: opacity a = coverage(n) x (A_k band(e - e_k) + top_k(e)), band = exp(-(x / 1.6)^2) above e_k (the far
// edge of the layer), exp(-(x / 5.5)^2) below; top_k = (1 - exp(-K_k / sin|e|)) x V_k x smoothstep over e_k .. e_k - 12 deg (the
// layer seen from above, V_k = exp(-(sigma Down(y_k) t_k)^2) its fog visibility); coverage = smoothstep(0.5 - h, 0.5 + h, n),
// h = max(0.10, 10 g), g = blurM sum(w_o fade_o / L_o) (the threshold filtered by the noise change per pixel: no aliased cloud
// edge at grazing, < 1 code per row through the KS09 horizon), n = a 4-octave billow noise on the sheet plane (1200 / 400 /
// 133.3 / 44.4 m, weights 0.30 / 0.30 / 0.25 / 0.15, each faded to its mean below 6..12 px); composited deep to near over the
// base gradient and the deep glow of the lit city (plane y -2000, octaves 675 / 225 m, +-50 % where e < -10).
// Fifth start (AT_r01: the sheets were brighter veils over a base that brightens with depth, so every hole showed an equally
// bright deeper layer and the billows cancelled to +-2-3 codes): the sheets are now DARK VEILS over GLOWING DEPTHS. Under the
// sheets the depths are lifted, c x (1 + 0.7 L(e)), L = smoothstep(3, 30, -e) (1 - smoothstep(30, 80, -e)) (the lit city shows
// most at -30..-50 deg, the nadir keeps its 60-66 of LOOK_V2 8.1); each sheet multiplies what lies under it by its veil colour
// (0.66 / 0.55 / 0.55, the near one lighter and a little cooler: it lies along the horizon band) x shade: c = lerp(c, c x veil_k x
// shade, a_k), band opacities 0.75 / 0.85 / 0.95. Thick cores are darker (0.35), the 44 m octave adds +-20 % detail; holes in a
// layer show the brighter layers and the glow under it, so the strata read by occlusion (near dark billows over a warm deep).
// AT_r02 (lift 1.2, veils 0.45 / 0.55 / 0.65) put the band under the horizon at 36-40 and the nadir at 72: retuned (AT-t03).
#define NA_SheetFogStrength 1.5
#define NA_BandA0 0.75
#define NA_BandA1 0.85
#define NA_BandA2 0.95
#define NA_BandUp 1.6
#define NA_BandDn 5.5
#define NA_TopK0 0.5
#define NA_TopK1 0.5
#define NA_TopK2 0.5
#define NA_TopRamp 12.0
#define NA_CovHalf 0.10
#define NA_CovAA 10.0
#define NA_SheetFadePx0 6.0
#define NA_SheetFadePx1 12.0
#define NA_SheetMaxA 0.95
#define NA_SheetDetail 0.20
#define NA_SheetThick 0.35
#define NA_SheetL0 1200.0
#define NA_SheetL1 400.0
#define NA_SheetL2 133.333333
#define NA_SheetL3 44.444444
#define NA_SheetW0 0.30
#define NA_SheetW1 0.30
#define NA_SheetW2 0.25
#define NA_SheetW3 0.15
#define NA_Veil0R 0.65
#define NA_Veil0G 0.66
#define NA_Veil0B 0.68
#define NA_Veil1R 0.54
#define NA_Veil1G 0.55
#define NA_Veil1B 0.56
#define NA_Veil2R 0.54
#define NA_Veil2G 0.55
#define NA_Veil2B 0.55
#define NA_DeepLift 0.7
#define NA_LiftE0 3.0
#define NA_LiftE1 30.0
#define NA_LiftE2 80.0
#define NA_LiftNadir 1.0
#define NA_AbyssExp 1.3
#define NA_DeepY -2000.0
#define NA_DeepAmp 0.5
#define NA_DeepL0 675.0
#define NA_DeepL1 225.0
#define NA_DeepW0 0.55
#define NA_DeepW1 0.45
#define NA_DeepE0 10.0
#define NA_DeepE1 70.0
#define NA_SheetScrollPeriod 76800.0
#define NA_DeepScrollPeriod 43200.0
// D-034 occluders (identity while _NocDeckOcc.w / _NocHazeOcc.w are 0): beam / length 0.15, soft edge from 0.6 of the radius.
#define NA_OccBeam 0.15
#define NA_OccEdge 0.6
#define NA_HazeOccY -650.0
// Deck form (INT-11): f = x / sqrt(1 + x^2), x = 4.5 (n - 0.5) (never flat, no plateau), base x (1 + 0.5 f), tint x (1 + 0.7 f):
// +-22 % at one std of n over the base deck, +-30 % under the city glows (fifth start: AT_r01's KS05 / KS04 lids showed +-2-3
// codes of billows at 0.5; 0.8 / 0.95 (AT_r02 / r03) put KS02's slit, which lies on a dark billow at its pose, at 50.3 / 48.5,
// under its LOOK_V2 8.2 window of 50.4-70.4), and the cloud mix of geometry starts 70 m x f higher or lower (tops dissolve).
#define NA_DeckK 4.5
#define NA_DeckBaseAmp 0.5
#define NA_DeckTintAmp 0.7
#define NA_CloudWisp 70.0
// ------------------------------------------------------------------ fix wave (panel p4_01, 2026-11-21)
// C11 under-city glow pools: the lit city 2 km down shows through the haze as soft lobes on the plane y -2 100 (value noise
// 337.5 / 112.5 m, weights 0.7 / 0.3, rotations 1 / 3, seeds 905 / 906; pool = smoothstep(0.45, 0.85, n): lobes 150-300 m, so a
// 140 px column of the KS10 drop (650 m wide 4 km out) crosses two or three), amber or neutral by a third lattice (337.5 m,
// rotation 4, seed 907), drifting with the deep glow (_NocAbyss.zw; 43 200 m = 2 / 6 lattices: exact wrap): c x (1 + A f0 (pool
// tint - 0.24)) (A = _NocAir.x = NocAir.PoolAmp; mean about 1), from under the near veil down (weight smoothstep(12, 22, -e): at
// grazing the lattice changes > 1 code per row, the KS09 horizon keeps < 1 code per row) and faded with the 337.5 m octave.
// They sit under the sheets: the veils and their holes layer them. (AT_r01f, 675 / 225 m and smoothstep(0.55, 0.80): one lobe
// edge per column at most, KS10 drop row std still 1-4 codes; the lobes read as hard-edged blotches.)
#define NA_PoolY -2100.0
#define NA_PoolL0 337.5
#define NA_PoolL1 112.5
#define NA_PoolW0 0.7
#define NA_PoolW1 0.3
#define NA_PoolLo 0.45
#define NA_PoolHi 0.85
#define NA_PoolMean 0.24
#define NA_PoolE0 12.0
#define NA_PoolE1 22.0
// AT-t01: at the nadir the pools stacked on the deep glow (KS13 twin mean 65.5 -> 69.4, p5..p95 53-88, LOOK_V2 8.1 60-66): their
// weight falls by 0.8 over -e 45..80 (the deep glow keeps the nadir's structure).
#define NA_PoolNadir 0.8
#define NA_PoolE2 45.0
#define NA_PoolE3 80.0
#define NA_PoolAmbR 1.20
#define NA_PoolAmbG 0.98
#define NA_PoolAmbB 0.70
// C11 sink: surfaces below the walk sink into the abyss haze. NocFog veils them toward the sky colour of their ray by V S01((-40 -
// y) / 360) (V = _NocAir.y = NocAir.SinkV 0.5: 0 above y -40, half below y -400): T' = T (1 - veil). A depth veil, not an extra
// density: AT_r01f's density form (sigma 2.5 S01((-60 - y) / 340)) took T of AB's y -520 traffic river 800 m out from 0.54 to
// 0.15 and of the y -880 river to 0.03 while a face at y -400 only went 0.87 -> 0.69. NocStreak's own NocFogT is unchanged (the
// rivers' lights punch through); geometry through NocFog (faces, terraces, DR's shadow body) sinks. Monotone along every
// downward ray (T falls, the veil rises), identity above y -40 and with the sheets cut.
#define NA_SinkY0 -40.0
#define NA_SinkH 360.0
// C02-air: the koi's light in the air. Each of the 12 spine points glows in the haze as a Gaussian density of radius R 70 m; its
// in-scatter along the view segment 0..L is closed form: G sqrt(pi / 2) R (erf((L - t0) / (sqrt 2 R)) + erf(t0 / (sqrt 2 R)))
// exp(-h^2 / (2 R^2)) (t0 = the ray parameter of the closest approach, h its distance), x KoiGlow; gain = _NocAir.z (KoiGain x
// KoiK, 0 outside beat 7) is the radiance added by one point on a ray through its centre. Added after the fog in NocFog and in
// the sky dome (to the deck plane), so towers and sky behind the body agree.
#define NA_KoiAirR 70.0
// AT_r01f (KoiGlow, no fade): the body's own fragments took the in-scatter of their spine point (KS17 / KS18 orange patches hue
// 14.9 -> 8.0, sat 0.74 -> 0.51: HT's patch discipline failed) and the dark towers took a teal cast (KS18 |R - G| 2.8 -> 4.4).
// So a point adds nothing to fragments within 60 m of it (smoothstep 60..110 m: the body, radius <= 48 m, stays HT's colour; the
// sky and towers 110 m or more from the spine glow in full), and the air colour is KoiGlow half-desaturated toward its luminance
// (lerp(0.607, KoiGlow, 0.55): rain haze scatters a whiter light than the emitter's own saturated cyan).
#define NA_KoiAirIn 60.0
#define NA_KoiAirOut 110.0
#define NA_KoiAirSat 0.55
// AT_r02f: the koi is additive (NocKoi, Blend One One), so the glowing air BEHIND its body (a ray through the spine: h about 0, the
// strongest in-scatter) still tinted the orange patches (hue 9-10, sat 0.52-0.55; HT's window 10-22 / >= 0.60). A point's glow is
// weighted by smoothstep(30, 70, h) (h = its distance from the ray): rays through the body see the hologram, not the air behind
// it; the glow peaks just outside the silhouette (h about 70 m) and falls off from there (AT_r03f with 10..50: KS17 patches pass
// at hue 11.2 / sat 0.60, KS18 still 9.9 / 0.56).
#define NA_KoiAirH0 30.0
#define NA_KoiAirH1 70.0
// C26 sheet lightning (DREAD 5.4): the deck brightens by up to 15 % (x (0.75 + 0.25 f): the thick billows flash more) in a
// Gaussian of sigma 450 m (about 20 % of a frame looking up), rise 0.4 s, decay 1.5 s (AtmosphereDriver, at DreadScore.LightningS);
// the glow centre lies 550 m ahead of the walker on the path heading, or on the hull (DR's _NocDeckOcc) when the launched hull is
// within 900 m: the hull's ellipse keeps 0.1 of the flash, so the flash outlines its silhouette.
#define NA_FlashHull 0.9
// C44: the alley's light on the deck above it (the KS02 slit sees the deck at x 0, z 32-126 from s 88): base and tint x (1 + 0.25
// exp(-r^2 / (2 70^2))) around (0, 80); < 0.2 % at the KS04 / KS05 lid centres, 1.7 % at the boulevard's hot core (115, 195).
#define NA_SlitX 0.0
#define NA_SlitZ 80.0
#define NA_SlitSigma 70.0
#define NOC_KOI_AIR_N 12
#define NOC_ARK_BEAMS 6

float4 _NocFog;
float4 _NocFogSheets;
float4 _NocSheetE;
float4 _NocAbyss;
float4 _NocCloud;
float4 _NocCloud2;
float4 _NocGlowSeg[NOC_GLOW_MAX];
float4 _NocGlowPar[NOC_GLOW_MAX];
float  _NocGlowCount;
float4 _NocAmber;
float4 _NocDeckOcc;
float4 _NocHazeOcc;
float4 _NocAir;
float4 _NocFlash;
float4 _NocKoiAir[NOC_KOI_AIR_N];
float4 _NocArk;
float4 _NocArkB;
float4 _NocArkG;
float4 _NocArkL;
float4 _NocArkS;
float4 _NocArkBeamP[NOC_ARK_BEAMS];
float4 _NocArkBeamD[NOC_ARK_BEAMS];

// ------------------------------------------------------------------ small helpers
float NocS01(float t) { t = saturate(t); return t * t * (3.0 - 2.0 * t); }
float NocSmoother01(float t) { t = saturate(t); return t * t * t * (t * (t * 6.0 - 15.0) + 10.0); }

// Radians per pixel (vertical) of the current camera: 2 / (P11 H). Used as the default blur of the sky noise.
float NocPixelAngle()
{
    return 2.0 / max(abs(UNITY_MATRIX_P[1][1]) * max(_NocScreen.x, 1.0), 1e-4);
}

// Erf, Abramowitz-Stegun 7.1.26 (|err| < 1.5e-7): the monotone haze-sheet path integral (R2 2.2).
float NocErfAS(float x)
{
    float s = x < 0.0 ? -1.0 : 1.0;
    x = abs(x);
    float t = 1.0 / (1.0 + 0.3275911 * x);
    float p = ((((1.061405429 * t - 1.453152027) * t + 1.421413741) * t - 0.284496736) * t + 0.254829592) * t;
    return s * (1.0 - p * exp(-x * x));
}

// Value noise with a lattice periodic in `period` cells (the cloud scroll wraps exactly; twin of NocFogMath.ValueNoiseP).
float NocValueNoiseP(float2 p, float period)
{
    p -= period * floor(p / period);                       // [0, period]: float wrap (no integer modulus)
    float2 f = floor(p);
    float2 t = p - f;
    f = f >= period ? f - period : f;                      // rounding can land p exactly on the period (a 1 px seam line)
    float2 g = f + 1.0;
    g = g >= period ? g - period : g;
    int2 i = int2(f), j = int2(g);
    float2 u = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    float a = NocU01(NocPcg2(i.x, i.y)), b = NocU01(NocPcg2(j.x, i.y));
    float c = NocU01(NocPcg2(i.x, j.y)), d = NocU01(NocPcg2(j.x, j.y));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

// The same lattice with an integer seed added after the wrap (a different pattern per sheet and octave, still periodic).
float NocValueNoisePS(float2 p, float period, int2 seed)
{
    p -= period * floor(p / period);
    float2 f = floor(p);
    float2 t = p - f;
    f = f >= period ? f - period : f;
    float2 g = f + 1.0;
    g = g >= period ? g - period : g;
    int2 i = int2(f) + seed, j = int2(g) + seed;
    float2 u = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
    float a = NocU01(NocPcg2(i.x, i.y)), b = NocU01(NocPcg2(j.x, i.y));
    float c = NocU01(NocPcg2(i.x, j.y)), d = NocU01(NocPcg2(j.x, j.y));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

// Rotation i (0..5) of the abyss noise lattices: +17 / -53 / +38 / -71 / +64 / -29 deg (no lattice aligns with the rift, the city
// grid, another octave or the deck's +23 / -41 / +71 / -59). Tables for the [loop]s of NocAbyss (small code, branchy execution).
static const float2 NocRot6CS[6] = { float2(0.95630476, 0.29237170), float2(0.60181502, -0.79863551), float2(0.78801075, 0.61566148),
                                     float2(0.32556815, -0.94551858), float2(0.43837115, 0.89879405), float2(0.87461971, -0.48480962) };
float2 NocRot6(float2 p, int i) { float2 cs = NocRot6CS[i]; return float2(cs.x * p.x - cs.y * p.y, cs.y * p.x + cs.x * p.y); }
static const float NocSheetLs[4] = { NA_SheetL0, NA_SheetL1, NA_SheetL2, NA_SheetL3 };
static const float NocSheetWs[4] = { NA_SheetW0, NA_SheetW1, NA_SheetW2, NA_SheetW3 };
static const float NocSheetBandA[3] = { NA_BandA0, NA_BandA1, NA_BandA2 };
static const float NocSheetTopK[3] = { NA_TopK0, NA_TopK1, NA_TopK2 };
static const float3 NocSheetVeils[3] = { float3(NA_Veil0R, NA_Veil0G, NA_Veil0B), float3(NA_Veil1R, NA_Veil1G, NA_Veil1B),
                                         float3(NA_Veil2R, NA_Veil2G, NA_Veil2B) };

// D-034 analytic occluder (DREAD 2.1 / 3.1): a soft ellipse in a horizontal plane, o = (centre x, centre z, half-length along z,
// strength): strength x (1 - smoothstep(0.6, 1, r)), r = the ellipse radius (beam 0.15 x length). 0 wherever o.w is 0.
float NocOccluder(float2 xz, float4 o)
{
    float2 hl = max(float2(NA_OccBeam, 1.0) * o.z, 1.0);
    float r = length((xz - o.xy) / hl);
    return saturate(o.w) * (1.0 - smoothstep(NA_OccEdge, 1.0, r));
}

// D-043 the Ark between the lit city and the cloud base (its back in the deck): a soft stadium along z, unit strength (twin
// NocFogMath.ArkShape). Its half-beam _NocArkB.x narrows to 0.35 of it over the soft ends (_NocArkB.z m at each end, S01 along),
// with a penumbra of _NocArkB.y m (the city's light comes from a wide cone below, so the deck's shadow is wider than the hull).
float NocArkShape(float2 xz)
{
    float along = NocS01((_NocArk.z - abs(xz.y - _NocArk.y)) / max(_NocArkB.z, 1.0));
    float hb = _NocArkB.x * (0.35 + 0.65 * sqrt(along));
    float dx = abs(xz.x - _NocArk.x);
    return along * (1.0 - smoothstep(hb - 0.5 * _NocArkB.y, hb + _NocArkB.y, dx));
}

// R6 per-item amber band: the weight of SodiumAmber (vs SignWhite) for an item with hash u in [0, 1) at k_a. Each item switches
// over a k_a band of 0.06 (about 14 s at 2.5 m/s in beats 5-6), so nothing pops; the mean over items is k_a; 0 at k_a 0, 1 at 1.
float NocAmberMix(float u, float ka) { return saturate((ka - (NC_AmberBand + u * (1.0 - NC_AmberBand))) / NC_AmberBand + 1.0); }

// ------------------------------------------------------------------ fog (14.2.2, H3 with D-006 / R2 2.1-2.3)
// Height factor of sigma_eff: 1 + 1.8 smoothstep(0, 1400, -y) (the exact optical depth of a real density along any ray from y 0).
float NocDown(float y) { return 1.0 + NC_FogDownFactor * NocS01(-y / NC_FogDownDepth); }

// Mean of one Gaussian haze sheet (centre yk, width w) along the segment y0 -> y1: closed-form erf integral (monotone).
float NocSheetMean(float y0, float y1, float yk, float w)
{
    float dy = y1 - y0;
    if (abs(dy) < 1.0) { float m = (0.5 * (y0 + y1) - yk) / w; return exp(-m * m); }
    return 0.88622692545 * w * (NocErfAS((y1 - yk) / w) - NocErfAS((y0 - yk) / w)) / dy;
}

// Alley haze weight 1 - smoothstep(18, 30, y): exact mean along y0 -> y1 through its antiderivative Q.
float NocCapQ(float y)
{
    float h = NC_AlleyHazeY1 - NC_AlleyHazeY0;
    float t = (y - NC_AlleyHazeY0) / h;
    if (t <= 0.0) return 0.0;
    if (t >= 1.0) return h * (t - 0.5);
    return h * (t * t * t - 0.5 * t * t * t * t);
}
float NocCapMean(float y0, float y1)
{
    float dy = y1 - y0;
    if (abs(dy) < 0.01) return 1.0 - NocS01((y0 - NC_AlleyHazeY0) / (NC_AlleyHazeY1 - NC_AlleyHazeY0));
    return 1.0 - (NocCapQ(y1) - NocCapQ(y0)) / dy;
}

// Optical depth tau from the eye to posWS; T = exp(-tau^2). sigmaBase replaces _NocFog.x (the sky's cloud uses min(sigma, 6e-4)).
float NocOpticalDepthS(float3 posWS, float sigmaBase)
{
    float3 v = posWS - _WorldSpaceCameraPos.xyz;
    float d = length(v);
    float y0 = _WorldSpaceCameraPos.y, y1 = posWS.y;
    float tau = sigmaBase * NocDown(y1) * d;                                           // exact height term (R2 2.1)
    [branch] if (_NocFog.z > 0.0 && min(y0, y1) < NC_SheetSkipY)
    {
        float w = _NocFogSheets.w;
        float sh = NocDown(_NocFogSheets.x) * NocSheetMean(y0, y1, _NocFogSheets.x, w)
                 + NocDown(_NocFogSheets.y) * NocSheetMean(y0, y1, _NocFogSheets.y, w)
                 + NocDown(_NocFogSheets.z) * NocSheetMean(y0, y1, _NocFogSheets.z, w);
        tau += sigmaBase * _NocFog.z * sh * d;                                           // monotone sheets (R2 2.2)
    }
    [branch] if (_NocFog.y > 0.0) tau += _NocFog.y * NocCapMean(y0, y1) * d;            // alley haze layer (D-006)
    return tau;
}
float NocOpticalDepth(float3 posWS) { return NocOpticalDepthS(posWS, _NocFog.x); }
float NocFogT(float3 posWS) { float tau = NocOpticalDepth(posWS); return exp(-tau * tau); }

// ------------------------------------------------------------------ cloud deck (plane y 460, LOOK_V2 3.1 tint model)
// Glow union W in [0, 1] (soft union of Gaussians, sigma 220 m, over segments / points / rings in xz), the g-weighted tint of
// the glows (the deck colour over each district) and the Gulf shadow union (tint 3: a multiplier of the base, never a glow).
float NocCloudGlowT(float2 xz, out float3 tint, out float gulf)
{
    float keep = 1.0, keepG = 1.0, gsum = 0.0;
    float3 tsum = 0.0;
    int n = (int)min(_NocGlowCount, (float)NOC_GLOW_MAX);
    [loop] for (int i = 0; i < n; i++)
    {
        float4 sg = _NocGlowSeg[i], pr = _NocGlowPar[i];
        float2 a = sg.xy, ab = sg.zw - sg.xy;
        float h = saturate(dot(xz - a, ab) / max(dot(ab, ab), 1e-6));
        float dist = abs(length(xz - a - ab * h) - pr.z);
        float q = dist / max(pr.y, 1.0);
        float g = saturate(pr.x * exp(-0.5 * q * q));
        float sh = step(2.5, pr.w);                                                  // 1 = Gulf shadow
        float3 tc = pr.w < 0.5 ? PAL_CloudHot : (pr.w < 1.5 ? PAL_CloudCool : PAL_CloudBase);
        keepG *= 1.0 - g * sh;
        float gg = g * (1.0 - sh);
        keep *= 1.0 - gg;
        gsum += gg;
        tsum += gg * tc;
    }
    tint = tsum / max(gsum, 1e-4);
    gulf = 1.0 - keepG;
    return 1.0 - keep;
}
float NocCloudGlowG(float2 xz) { float3 t; float g; return NocCloudGlowT(xz, t, g); }

// One octave's fade at the pixel footprint blurM (m): smoothstep(4, 8, L / blurM) (no octave under 4 px of wavelength).
float NocOctaveFade(float L, float blurM) { return smoothstep(NC_CloudFadePx0, NC_CloudFadePx1, L / max(blurM, 1e-6)); }

// Deck noise n in 0..1, mean 0.5: octaves 900 / 300 / 110 / 40 m (weights 0.36 / 0.19 / 0.30 / 0.15), each rotated (+23 / -41 /
// +71 / -59 deg, so no lattice aligns with the city grid or another octave) and scrolled 3 m/s; the 110 and 40 m octaves are
// domain-warped by 60 m x (2 n900 - 1, 2 n300 - 1); faded octaves sit at their mean (no shimmer below 8 px of wavelength).
float NocCloudNoise(float2 xz, float blurM)
{
    float k1 = NocOctaveFade(NC_CloudNoise1, blurM), k2 = NocOctaveFade(NC_CloudNoise2, blurM);
    float k3 = NocOctaveFade(NC_CloudNoise3, blurM), k4 = NocOctaveFade(NC_CloudNoise4, blurM);
    float2 r1 = float2(0.92050485 * xz.x - 0.39073113 * xz.y, 0.39073113 * xz.x + 0.92050485 * xz.y);
    float2 r2 = float2(0.75470958 * xz.x + 0.65605903 * xz.y, -0.65605903 * xz.x + 0.75470958 * xz.y);
    float n1 = lerp(0.5, NocValueNoiseP((r1 + _NocCloud.xy) / NC_CloudNoise1, NC_CloudNoisePeriod), k1);
    float n2 = lerp(0.5, NocValueNoiseP((r2 + _NocCloud.zw) / NC_CloudNoise2, NC_CloudNoisePeriod), k2);
    float n3 = 0.5, n4 = 0.5;
    [branch] if (k3 > 0.0)
    {
        float2 w = xz + NC_CloudWarp * float2(2.0 * n1 - 1.0, 2.0 * n2 - 1.0);
        float2 r3 = float2(0.32556815 * w.x - 0.94551858 * w.y, 0.94551858 * w.x + 0.32556815 * w.y);
        n3 = lerp(0.5, NocValueNoiseP((r3 + _NocCloud2.xy) / NC_CloudNoise3, NC_CloudNoisePeriod), k3);
        [branch] if (k4 > 0.0)
        {
            float2 r4 = float2(0.51503807 * w.x + 0.85716730 * w.y, -0.85716730 * w.x + 0.51503807 * w.y);
            n4 = lerp(0.5, NocValueNoiseP((r4 + _NocCloud2.zw) / NC_CloudNoise4, NC_CloudNoisePeriod), k4);
        }
    }
    return NC_CloudNoiseW1 * n1 + NC_CloudNoiseW2 * n2 + NC_CloudNoiseW3 * n3 + NC_CloudNoiseW4 * n4;
}

// Deck billow form f in (-1, 1), mean 0 at n 0.5: x / sqrt(1 + x^2), x = 4.5 (n - 0.5) (a soft S that never flattens: no H1 plateau).
float NocDeckForm(float n) { float x = NA_DeckK * (n - 0.5); return x * rsqrt(1.0 + x * x); }

// Ceiling colour at (x, z) (LOOK_V2 3.1 tint model, AT form): base = CloudBase (1 + 0.5 f) (1 - 0.35 g_gulf), cloud = lerp(base,
// tint (1 + 0.7 f), W): warm grey over the boulevard, cool over the sign wall, spire and koi; +-22 % billows at one std of n over
// the base, +-30 % under the glows. D-034: the hull occluder scales W by (1 - occ) and the base by (1 - 0.5 occ) (identity at 0).
float3 NocCloudColorN(float2 xz, float blurM, out float f)
{
    f = NocDeckForm(NocCloudNoise(xz, blurM));
    float3 tint; float gulf;
    float W = NocCloudGlowT(xz, tint, gulf);
    float3 base = PAL_CloudBase * ((1.0 + NA_DeckBaseAmp * f) * (1.0 - NC_CloudGulfShade * gulf));
    [branch] if (_NocDeckOcc.w > 0.0)                                                      // D-034 hull: a silhouette in the glow
    {
        float occ = NocOccluder(xz, _NocDeckOcc) * step(_NocArk.w, 0.0);                  // (the Ark's own shape replaces it)
        W *= 1.0 - occ;
        base *= 1.0 - 0.5 * occ;
    }
    float ark = 0.0;
    [branch] if (_NocArk.w > 0.0)                                                          // D-043: the Ark shadows the city glow
    {
        ark = NocArkShape(xz);
        float occ = saturate(_NocArk.w) * ark;
        W *= 1.0 - occ;
        base *= 1.0 - _NocArkB.w * occ;
    }
    float3 col = lerp(base, tint * (1.0 + NA_DeckTintAmp * f), W);
    [branch] if (_NocAir.w > 0.0)                                                          // C44: the alley's light on the deck
    {
        float2 q = (xz - float2(NA_SlitX, NA_SlitZ)) / NA_SlitSigma;
        col *= 1.0 + _NocAir.w * exp(-0.5 * dot(q, q));
    }
    [branch] if (_NocFlash.w > 0.0)                                                        // C26: sheet lightning (DREAD 5.4)
    {
        float2 q = (xz - _NocFlash.xy) / (max(_NocFlash.z, 1.0) * float2(1.0, max(_NocArkG.w, 1.0)));   // D-043: along the Ark
        float fl = _NocFlash.w * exp(-0.5 * dot(q, q)) * (0.75 + 0.25 * f);
        [branch] if (_NocDeckOcc.w > 0.0) fl *= 1.0 - NA_FlashHull * NocOccluder(xz, float4(_NocDeckOcc.xyz, 1.0)) * step(_NocArk.w, 0.0);
        fl *= 1.0 - NA_FlashHull * ark;                                                    // the flash outlines the Ark
        col *= 1.0 + fl;
    }
    return col;
}
float3 NocCloudColor(float2 xz, float blurM) { float f; return NocCloudColorN(xz, blurM, f); }

// ------------------------------------------------------------------ sky colour (14.2.2 + D-006 + H3 sheet modulation)
// Gradient part for elevation e (deg), without the cloud deck (the colour the cloud and every fog fades toward).
float3 NocSkyGradient(float e)
{
    float3 c;
    if (e >= 0.0)
    {
        c = lerp(PAL_HazeHorizon, PAL_NightMid, smoothstep(0.0, 35.0, e));
        c = lerp(c, PAL_NightZenith, smoothstep(35.0, 90.0, e));
    }
    else
    {
        float x = 1.0 - saturate(-e / 90.0);
        float3 a = lerp(PAL_AbyssNear, PAL_AbyssGlow, 1.0 - pow(x, NA_AbyssExp));          // monotone, no dip (14.2.2), flat at -90
        float wb = NocSmoother01(-e / NC_HorizonBlendDeg);                                 // C1 at the horizon (D-006)
        c = lerp(PAL_HazeHorizon, a, wb);                                                  // the sheets: NocAbyss (needs the ray)
    }
    return c;
}

// ------------------------------------------------------------------ the Ark in the haze (D-043, AT): its light in the air
// Closest approach of the ray (eye o, unit rd) to the line (point a, unit b), w = o - a: ray parameter t0, axial position u0,
// squared distance h2, sn = |rd x b| (twin NocFogMath.LineApproach).
void NocLineApproach(float3 w, float3 rd, float3 b, out float t0, out float u0, out float h2, out float sn)
{
    float c = dot(rd, b);
    float s2 = max(1.0 - c * c, 1e-4);
    float dd = dot(rd, w), e = dot(b, w);
    t0 = (e * c - dd) / s2;
    u0 = e + t0 * c;
    float3 q = w + t0 * rd - u0 * b;
    h2 = dot(q, q);
    sn = sqrt(s2);
}

// One closed form for a Gaussian tube of radius r about a line: the density along the ray is exp(-(h^2 + sn^2 (t - t0)^2) / r^2),
// so with G = I / (sqrt(pi) r) (I = the radiance across the tube on a perpendicular ray through its axis) the in-scatter over
// 0..len is I exp(-h^2 / r^2) (erf(sn (len - t0) / r) + erf(sn t0 / r)) / (2 sn), fogged by T = exp(-(sigma Down(y) t0)^2) at the
// closest approach (r, I and the windows are taken there: the tubes are long and vary slowly). sn >= 0.1: the eye is never inside
// a tube (the driver keeps every beam 3 radii + 150 m from the walker; the glow tube's axis is >= 330 m from any walk point).
// Perf (AT_ark_perf): the window (1 - smoothstep(4, 6, h^2 / r^2)) ends the skirt at 2.45 r (the density there is 0.25 % of the
// axis: no edge), and the erf pair is skipped (= 2) when the closest approach lies 2.5 widths inside 0..len (erf 2.5 = 0.9996).
float NocTubeIn(float I, float r, float h2, float t0, float sn, float len, float3 o, float3 rd)
{
    float k = sn / r;
    float tf = max(t0, 0.0);
    float ft = _NocFog.x * NocDown(o.y + tf * rd.y) * tf;
    float q = h2 / (r * r);
    float x0 = t0 * k, x1 = (len - t0) * k;
    float seg = 2.0;
    [branch] if (min(x0, x1) < 2.5) seg = NocErfAS(x1) + NocErfAS(x0);
    return I * exp(-q - ft * ft) * (1.0 - smoothstep(4.0, 6.0, q)) * seg / (2.0 * max(sn, 0.1));
}

// D-043: the Ark's light in the haze along the unit ray rd from the eye over 0..len (m), added after the fog (NocFog, the dome,
// DR's own-fog Ark shader through NocArkAirTo). (1) The glow of its thousands of lit portholes: a tube of radius _NocArkG.y about
// its axis (x, _NocArkG.x, z), windowed to its length (the stadium's soft ends), cold window light. (2) The searchlights: cones from
// their apexes (r = r0 + u tan a), I = gain (r0 / r) exp(-u / L) (a beam's flux spreads: a ray across it sees flux / r), cut
// above the apex (smoothstep(-r0, 2 r0, u0)), SignWhite. Twin NocFogMath.ArkAir.
float3 NocArkAir(float3 rd, float len)
{
    float3 o = _WorldSpaceCameraPos.xyz;
    float3 acc = 0.0;
    float t0, u0, h2, sn;
    [branch] if (_NocArkG.z > 0.0)                                                         // (1) the glow of its lights
    {
        NocLineApproach(o - float3(_NocArk.x, _NocArkG.x, _NocArk.y), rd, float3(0.0, 0.0, 1.0), t0, u0, h2, sn);
        float r = max(_NocArkG.y, 1.0);
        [branch] if (h2 < 6.0 * r * r)
        {
            float win = NocS01((_NocArk.z - abs(u0)) / max(_NocArkB.z, 1.0));
            acc += lerp(PAL_WinNeutral, PAL_WinCold, 0.6) * NocTubeIn(_NocArkG.z * win, r, h2, t0, sn, len, o, rd);
        }
    }
    [branch] if (_NocArkL.x > 0.0)                                                         // (2) the searchlights
    {
        float r0 = max(_NocArkL.w, 1.0), sum = 0.0;
        [loop] for (int i = 0; i < NOC_ARK_BEAMS; i++)
        {
            float4 P = _NocArkBeamP[i], D = _NocArkBeamD[i];
            [branch] if (P.w > 0.0)
            {
                NocLineApproach(o - P.xyz, rd, D.xyz, t0, u0, h2, sn);
                float r = r0 + max(u0, 0.0) * D.w;
                [branch] if (h2 < 6.0 * r * r)
                {
                    float I = P.w * (r0 / r) * exp(-max(u0, 0.0) / max(_NocArkL.z, 1.0)) * smoothstep(-r0, 2.0 * r0, u0);
                    sum += NocTubeIn(I, r, h2, t0, sn, len, o, rd);
                }
            }
        }
        acc += PAL_SignWhite * sum;
    }
    return acc;
}
// The same for a fragment at posWS (DR's Ark shader fogs itself: it adds this after its fog; zero when no Ark is lit).
float3 NocArkAirTo(float3 posWS)
{
    float3 v = posWS - _WorldSpaceCameraPos.xyz;
    float l = length(v);
    float3 a = 0.0;
    [branch] if (_NocArkL.x + _NocArkG.z > 0.0) a = NocArkAir(v / max(l, 1e-3), l);
    return a;
}

// D-043: where a searchlight meets a surface (a face it sweeps, a terrace, a cross bridge deep down): the lit spot, + SignWhite x
// spot gain x beam gain x min(1, (R / r)^2) exp(-u / L) exp(-d^2 / r^2) (u the fragment's depth along the beam, d its distance from
// the axis), added before the fog (NocFog), so the spot is fogged like the surface. Twin NocFogMath.ArkSpot.
float3 NocArkSpot(float3 p)
{
    float sum = 0.0, r0 = max(_NocArkL.w, 1.0), R2 = _NocArkS.y * _NocArkS.y;
    [loop] for (int i = 0; i < NOC_ARK_BEAMS; i++)
    {
        float4 P = _NocArkBeamP[i], D = _NocArkBeamD[i];
        [branch] if (P.w > 0.0)
        {
            float3 w = p - P.xyz;
            float u = dot(w, D.xyz);
            float r = r0 + max(u, 0.0) * D.w;
            float3 q = w - u * D.xyz;
            float d2 = dot(q, q);
            [branch] if (u > 0.0 && d2 < 9.0 * r * r)
                sum += P.w * min(1.0, R2 / (r * r)) * exp(-u / max(_NocArkL.z, 1.0)) * exp(-d2 / (r * r));
        }
    }
    return PAL_SignWhite * (_NocArkS.x * sum);
}

// (AT_ark perf, 21:05: the pools run inside NocSkyColor, i.e. in the fog colour of every fogged fragment below the horizon, 18
// beam-sheet pairs each; at the shipped gains they showed in no capture: off by NocArkAir.PoolGain 0, the uniform branch skips
// them. The spots likewise: NocArkAir.SpotGain 0.)
// D-043: the searchlights' pools on a haze sheet (plane y yk, plane hit h): each beam's footprint (its axis at that depth, radius
// r = r0 + u tan a, stretched along its lean by 1 / |b.y|) lights the billows by pool gain x gain x (r0 / r) exp(-u / L) x
// exp(-d^2 / r^2) (the caller weights it by the sheet's opacity, fog visibility and fine billows). Twin NocFogMath.ArkPool.
float3 NocArkPool(float2 h, float yk)
{
    float sum = 0.0, r0 = max(_NocArkL.w, 1.0);
    [loop] for (int i = 0; i < NOC_ARK_BEAMS; i++)
    {
        float4 P = _NocArkBeamP[i], D = _NocArkBeamD[i];
        [branch] if (P.w > 0.0 && D.y < -0.2 && P.y > yk)
        {
            float u = (P.y - yk) / -D.y;
            float r = r0 + u * D.w;
            float2 dq = h - (P.xz + D.xz * u);
            float2 lean = D.xz * rsqrt(max(dot(D.xz, D.xz), 1e-8));
            float al = dot(dq, lean), ac = dq.x * lean.y - dq.y * lean.x;
            float q2 = (al * al * D.y * D.y + ac * ac) / (r * r);
            sum += P.w * (r0 / r) * exp(-u / max(_NocArkL.z, 1.0)) * exp(-q2);
        }
    }
    return PAL_SignWhite * (_NocArkL.y * sum);
}

// ------------------------------------------------------------------ the abyss (H3, AT): deep glow of the lit city + three haze sheets
// One sheet's billow noise at plane point p (m), footprint blurM (m): 4 octaves, lattice seeds per sheet / octave; the scroll s (m,
// wrapped to NA_SheetScrollPeriod, a whole number of every octave's 64-cell lattice) is added in each octave's rotated frame, so
// the wrap is exact (each octave drifts along its own direction: the billows evolve rather than slide as one sheet).
float NocSheetNoise(float2 p, float2 s, float blurM, int k, out float nFine, out float g)
{
    float n = 0.0;
    nFine = 0.5;
    g = 0.0;
    [loop] for (int o = 0; o < 4; o++)
    {
        float L = NocSheetLs[o];
        float fd = smoothstep(NA_SheetFadePx0, NA_SheetFadePx1, L / max(blurM, 1e-6));
        float v = 0.5;
        [branch] if (fd > 0.0)
            v = lerp(0.5, NocValueNoisePS((NocRot6(p, o + k) + s) / L, NC_CloudNoisePeriod, int2(37 * (k + 1) + 11 * o, 53 * (k + 1) + 7 * o)), fd);
        n += NocSheetWs[o] * v;
        g += NocSheetWs[o] * fd / L;
        nFine = v;                                                                         // the last (44 m) octave
    }
    g *= blurM;                                                                            // noise change per pixel (scale)
    return n;
}

// Band profile of a sheet in elevation: the far edge of the layer above e_k (1.6 deg), a long soft tail below (5.5 deg).
float NocSheetBand(float x) { float q = x > 0.0 ? x / NA_BandUp : x / NA_BandDn; return exp(-q * q); }

// Lift of the depths under the sheets (AT, fifth start): L(e) = smoothstep(3, 30, -e) (1 - smoothstep(30, 80, -e)).
float NocAbyssLift(float e) { return smoothstep(NA_LiftE0, NA_LiftE1, -e) * (1.0 - NA_LiftNadir * smoothstep(NA_LiftE1, NA_LiftE2, -e)); }

// c = the base gradient of a downward ray d (unit, d.y < 0) at elevation e (deg); blurRad = pixel angle (rad). The depths first
// (the lift, the deep glow of the lit city on the plane y -2000, the D-034 shadow), then the sheets deep to near as dark veils:
// c = lerp(c, c x veil_k x shade, a_k).
float3 NocAbyss(float3 c, float3 d, float e, float blurRad)
{
    float sy = max(-d.y, 1e-4);
    float3 cam = _WorldSpaceCameraPos.xyz;
    c *= 1.0 + NA_DeepLift * NocAbyssLift(e);
    float dw = smoothstep(NA_DeepE0, NA_DeepE1, -e);
    [branch] if (dw > 0.0)
    {
        float t = (cam.y - NA_DeepY) / sy;
        float2 h = cam.xz + d.xz * t;
        float blurM = blurRad * t / max(sy, 0.05);
        float f0 = NocOctaveFade(NA_DeepL0, blurM), f1 = NocOctaveFade(NA_DeepL1, blurM);
        float n = NA_DeepW0 * lerp(0.5, NocValueNoisePS((NocRot6(h, 2) + _NocAbyss.zw) / NA_DeepL0, NC_CloudNoisePeriod, int2(900, 300)), f0)
                + NA_DeepW1 * lerp(0.5, NocValueNoisePS((NocRot6(h, 5) + _NocAbyss.zw) / NA_DeepL1, NC_CloudNoisePeriod, int2(901, 301)), f1);
        c *= 1.0 + NA_DeepAmp * 2.0 * (n - 0.5) * dw;
    }
    float pw = smoothstep(NA_PoolE0, NA_PoolE1, -e) * (1.0 - NA_PoolNadir * smoothstep(NA_PoolE2, NA_PoolE3, -e)) * _NocAir.x;   // C11 pools
    [branch] if (pw > 0.0)
    {
        float t = (cam.y - NA_PoolY) / sy;
        float2 h = cam.xz + d.xz * t;
        float blurM = blurRad * t / max(sy, 0.05);
        float f0 = NocOctaveFade(NA_PoolL0, blurM), f1 = NocOctaveFade(NA_PoolL1, blurM);
        [branch] if (f0 > 0.0)
        {
            float n = NA_PoolW0 * NocValueNoisePS((NocRot6(h, 1) + _NocAbyss.zw) / NA_PoolL0, NC_CloudNoisePeriod, int2(905, 305))
                    + NA_PoolW1 * lerp(0.5, NocValueNoisePS((NocRot6(h, 3) + _NocAbyss.zw) / NA_PoolL1, NC_CloudNoisePeriod, int2(906, 306)), f1);
            float sel = NocValueNoisePS((NocRot6(h, 4) + _NocAbyss.zw) / NA_PoolL0, NC_CloudNoisePeriod, int2(907, 307));
            float3 tint = lerp(float3(1.0, 1.0, 1.0), float3(NA_PoolAmbR, NA_PoolAmbG, NA_PoolAmbB), smoothstep(0.35, 0.65, sel));
            float pool = smoothstep(NA_PoolLo, NA_PoolHi, n);
            c *= 1.0 + pw * f0 * (pool * tint - NA_PoolMean);
        }
    }
    [branch] if (_NocHazeOcc.w > 0.0)                                                      // D-034 shadow (DREAD 3.1)
    {
        float2 ho = cam.xz + d.xz * ((cam.y - NA_HazeOccY) / sy);
        c *= 1.0 - NocOccluder(ho, _NocHazeOcc);
    }
    [loop] for (int k = 2; k >= 0; k--)
    {
        float yk = _NocFogSheets[k], ek = _NocSheetE[k];
        float t = (cam.y - yk) / sy;
        float tau = _NocFog.x * NocDown(yk) * t;
        float V = exp(-tau * tau);
        float aE = (NocSheetBandA[k] * NocSheetBand(e - ek) + (1.0 - exp(-NocSheetTopK[k] / sy)) * V * NocS01((ek - e) / NA_TopRamp)) * _NocSheetE.w;
        [branch] if (aE > 0.002)
        {
            float2 h = cam.xz + d.xz * t;
            float nFine, g;
            float n = NocSheetNoise(h, _NocAbyss.xy, blurRad * t / max(sy, 0.05), k, nFine, g);
            float hw = max(NA_CovHalf, NA_CovAA * g);
            float a = min(smoothstep(0.5 - hw, 0.5 + hw, n) * aE, NA_SheetMaxA);
            float shade = 1.0 + NA_SheetDetail * 2.0 * (nFine - 0.5) - NA_SheetThick * saturate((n - 0.5 - hw) * 4.0);
            c = lerp(c, c * NocSheetVeils[k] * shade, a);
            [branch] if (_NocArkL.y > 0.0) c += (a * V * (0.8 + 0.4 * nFine)) * NocArkPool(h, yk);   // D-043: searchlight pools
        }
    }
    return c;
}

// Sky colour (linear) of a direction (any length); blurRad = angular footprint of the pixel (rad) for the cloud noise.
float3 NocSkyColorBlur(float3 dir, float blurRad)
{
    float3 d = dir * rsqrt(max(dot(dir, dir), 1e-12));
    float e = degrees(asin(clamp(d.y, -1.0, 1.0)));
    float3 c = NocSkyGradient(e);
    [branch] if (e < 0.0) c = NocAbyss(c, d, e, blurRad);                                 // H3 (AT): deep glow, three sheets
    [branch] if (e > NC_CloudFadeE0)
    {
        float t = (NC_CloudBaseY - _WorldSpaceCameraPos.y) / max(d.y, 1e-3);
        float3 hit = _WorldSpaceCameraPos.xyz + d * t;
        float blurM = blurRad * t / max(d.y, 0.05);                                        // footprint along the plane (grazing)
        float3 cc = NocCloudColor(hit.xz, blurM);
        float tau = NocOpticalDepthS(hit, min(_NocFog.x, (float)NC_CloudSkySigmaMax));     // fogged like geometry (D-006 cap)
        float T = exp(-tau * tau);
        c = lerp(c, cc, T * smoothstep(NC_CloudFadeE0, NC_CloudFadeE1, e));
    }
    // LOOK_V2 3.1: the warm low haze of the alley layer (D-006) lives in the sky function, so the dome and every fog colour carry
    // it: c = lerp(c, HazeLow, 0.6 sa m / (sr + sa m)), m = the layer's mean along the ray to min(6 500 m, the cloud hit); the
    // path length cancels (continuous across the cloud fade). 0.55 at eye level, 0.20 toward the zenith; gone from s 124.
    [branch] if (_NocFog.y > 0.0)
    {
        float yc = _WorldSpaceCameraPos.y;
        float sam = _NocFog.y * NocCapMean(yc, yc + min(NC_HazeLowPath * d.y, NC_CloudBaseY - yc));
        c = lerp(c, PAL_HazeLow, NC_HazeLowShare * sam / max(_NocFog.x + sam, 1e-9));
    }
    return max(c, 0.0);
}
float3 NocSkyColor(float3 dir) { return NocSkyColorBlur(dir, NocPixelAngle()); }

// ------------------------------------------------------------------ geometry: cloud mix, fog, floor
// H3: geometry mixes toward the ceiling colour above it (smoothstep(380, 560, y + 70 f)), so shaft tops vanish into the deck; the
// billow form f of the deck at the fragment moves the mix 70 m up or down, so a tower dissolves into hanging bellies and gaps (AT).
float3 NocCloudMix(float3 col, float3 posWS)
{
    [branch] if (posWS.y > NC_CloudMixY0 - NA_CloudWisp)
    {
        float blurM = NocPixelAngle() * length(posWS - _WorldSpaceCameraPos.xyz);
        float f;
        float3 cc = NocCloudColorN(posWS.xz, blurM, f);
        col = lerp(col, cc, smoothstep(NC_CloudMixY0, NC_CloudMixY1, posWS.y + NA_CloudWisp * f));
    }
    return col;
}

// C02-air: in-scatter of the koi's light along the unit ray rd from the eye over 0..len (m): sum over the 12 spine points of the
// closed-form Gaussian line integral (radius NA_KoiAirR), x KoiGlow x _NocAir.z / 2 (z = gain x KoiK). Callers branch on _NocAir.z.
float3 NocKoiAir(float3 rd, float len)
{
    float3 ro = _WorldSpaceCameraPos.xyz;
    const float k = 0.70710678 / NA_KoiAirR;
    float3 fe = rd * len;                                                                  // the fragment, relative to the eye
    float g = 0.0;
    [loop] for (int i = 0; i < NOC_KOI_AIR_N; i++)
    {
        float3 p = _NocKoiAir[i].xyz - ro;
        float t0 = dot(p, rd);
        float h2 = max(dot(p, p) - t0 * t0, 0.0);
        float own = smoothstep(NA_KoiAirIn, NA_KoiAirOut, length(p - fe))                   // the body's own fragments: no glow
                  * smoothstep(NA_KoiAirH0, NA_KoiAirH1, sqrt(h2));                          // nor the air seen through its core
        g += own * exp(-h2 * k * k) * (NocErfAS((len - t0) * k) + NocErfAS(t0 * k));
    }
    float3 air = lerp(dot(PAL_KoiGlow, float3(0.2126, 0.7152, 0.0722)).xxx, PAL_KoiGlow, NA_KoiAirSat);
    return air * (0.5 * _NocAir.z * g);
}

// col T + fogColour (1 - T), fogColour = NocSkyColor(direction of the fragment): the horizon always agrees (8.4, 14.2.2).
float3 NocFogT3(float3 col, float3 posWS, float T)
{
    [branch] if (T < 0.99995) col = lerp(NocSkyColor(posWS - _WorldSpaceCameraPos.xyz), col, T);
    return col;
}
float3 NocFog(float3 col, float3 posWS)
{
    [branch] if (_NocArkL.x * _NocArkS.x > 0.0) col += NocArkSpot(posWS);                 // D-043: a searchlight's lit spot (off: 0)
    col = NocCloudMix(col, posWS);
    float T = NocFogT(posWS) * (1.0 - _NocAir.y * NocS01((NA_SinkY0 - posWS.y) / NA_SinkH));   // C11 sink veil below y -40
    col = NocFogT3(col, posWS, T);
    [branch] if (_NocAir.z > 0.0)                                                          // C02-air: the koi's light in the air
    {
        float3 v = posWS - _WorldSpaceCameraPos.xyz;
        float l = length(v);
        col += NocKoiAir(v / max(l, 1e-3), l);
    }
    [branch] if (_NocArkL.x + _NocArkG.z > 0.0)                                            // D-043: the Ark's light in the air
    {
        float3 v = posWS - _WorldSpaceCameraPos.xyz;
        float l = length(v);
        col += NocArkAir(v / max(l, 1e-3), l);
    }
    return col;
}

// The last operation of every shader and of the sky: FloorLin + col (1 - FloorLin).
float3 NocFloor(float3 col)
{
    float3 f = float3(NC_FloorLinR, NC_FloorLinG, NC_FloorLinB);
    return f + max(col, 0.0) * (1.0 - f);
}

#endif
