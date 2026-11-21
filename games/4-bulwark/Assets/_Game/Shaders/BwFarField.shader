// BwFarField.shader - the camera-following far-field dome (MASTER_PROMPT 14.7 H4, 14.2.2, 14.2.6, Part 10 #13 / #30).
// One cube around the eye, drawn after every opaque (queue Geometry+499) at the far plane (depth forced to far, ZWrite
// off, ZTest LEqual: only pixels no geometry covered; it ignores the camera far plane, 14.6). Every pixel ray is
// intersected analytically, camera-relative, with
//   the lowland sphere (R 6371000, hit x < -6), the sea sphere (R + 5992, hit x > 6, east only), the crest sphere
//   (R + 6000, |x| <= 6: the 12 m dam strip beyond the real road), the lee-face plane x = -6 (between the lowland and
//   the crest) and the sea-face plane x = +6 (between the sea and the crest),
// with sin^2(dip) of each sphere computed in double on the CPU (_BwFarCam) and the no-cancellation hit
// t = (R+h) sin^2(dip) / (-dy + sqrt((-dy - sin dip)(-dy + sin dip))). Ground pattern coordinates are arc lengths
// R phi along the heading from the eye's nadir. Layers: sky (BwSkyColor) + cloud deck (2D fbm at 6.8 km) + sun disc;
// lowland fields (300-1500 m warped grid, hedges, macro variation), three meandering rivers (3-sine SDFs, 80-400 m,
// box-filtered coverage), city lights (density of hashed centres x settlement fbm, never points), three mist rivers
// (slab 900-2200 m, amber where sun_el + dip(h_mist) > 0); the smooth far sea (variance s2, Fresnel reflection of the
// sky, clamped Beckmann glint); the lee face (ConcreteDark, lift lines, earth shadow) with five outlet jets (parabolic
// threads, min 1.5 px, energy-preserving); the crest strip. Every octave and line is weighted by the pixel footprint
// (no shimmer). The atmosphere is BulwarkAtmosphere.hlsl's 8-segment far ray (haze, storm slab, earth shadow), so the
// horizon, the fog and the sky agree by construction. Colours: palette tokens and AtmosphereDriver globals only.
// Motion (clouds, mist rivers, jets) reads _BwFlow, offsets computed in double from the unwrapped ambient clock
// (AtmosphereDriver.FlowOffsets): nothing jumps when _BwTime wraps at 3600 s (Part 10 #33).
Shader "Megastructure/BwFarField"
{
    Properties
    {
        _FarDetail ("Detail scale (1 = spec)", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+499" "IgnoreProjector" = "True" }
        Pass
        {
            Name "BwFarField"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex FarVert
            #pragma fragment FarFrag
            #pragma editor_sync_compilation
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Include/BulwarkPalette.hlsl"
            #include "Include/BwCommon.hlsl"
            #include "Include/BulwarkAtmosphere.hlsl"
            #include "Include/BwSeaWaves.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _FarDetail;
            CBUFFER_END

            float4 _BwFar0;     // x = city lights N(s), y = mist-river strength, z = fair-weather cloud field weight 0..1, w = u_sun
            float4 _BwCloud0;   // x = coverage threshold lo, y = hi, z = alpha max, w = texture contrast
            float4 _BwCloud1;   // rgb cloud colour, w = colour mix (0: the sky modulated, 1: the cloud colour)
            float4 _BwEndRide;  // the ending (Beat08Ending): xy = motion-filter length (m) along world x / y = the cabin speed x 1/120 s; z = ending weight 0..1
            float4 _BwEndLamps[10];   // the lantern platform's lanterns (Beat08Ending): xyz world position, w intensity (0 = unused)
            float4 _BwEndGlow;        // the lit plaza seen from afar (Beat08Ending): xyz centre, w radius (m); faded in over 250-400 m
            float4 _BwEndCar;         // the empty car returning up the rails (Beat08Ending): xyz world centre, w = 1 when climbing

            #define FF_LEE_X (-6.0)                 // BulwarkConst.LeeFaceX
            #define FF_SEA_X 6.0                    // BulwarkConst.SeaFaceX
            #define FF_CLOUD_ALT 6800.0             // cloud deck altitude (14.7 H4 "2D fbm at 6.8 km")
            #define FF_MIST_LO 900.0                // mist-river slab (14.7 H4)
            #define FF_MIST_HI 2200.0
            #define FF_JET_Y0 (-90.0)               // BulwarkConst.JetY0
            #define FF_JET_K 0.0030625              // 4.9 / 40^2: y = -90 - k (x + 6)^2
            #define FF_JET_FALL 1000.0              // turns to mist at 1 km (BulwarkConst.JetMistDistance)
            #define FF_JET_HALF 1.6                 // half thickness of a jet thread in its plane (m)
            #define FF_CITY_CELL 75000.0            // city centres every 60-90 km
            #define FF_TOWN_CELL 12000.0            // market towns every 9-15 km
            #define FF_CITY_K 2.5                   // city-light peak radiance scale (blob peaks luma > 120 at 20 km, KS20 at s 1470)
            #define FF_ANCHOR_X (-15330.0)          // the city below the ledge: 20 km from (-9.2, 1452) at az 310
            #define FF_ANCHOR_Z 14310.0
            #define FF_ANCHOR_R 10000.0
            #define FF_MIN_PX 1.5                   // thin-line minimum width (px, H7)
            #define FF_RAIL_Z 1497.114              // the elevator rail line: z of the walker spot in the cabin (BulwarkConst.EndZ)
            #define FF_VH_H 700.0                   // the ending's valley haze: scale height (m)
            #define FF_VH_S 3.2e-4                  // its extinction at the ground (1/m; fx02 2.4e-4 left a 9.5-level foot step)
            #define FF_VH_J 0.75                    // its in-scatter: the shadowed haze colour x 0.75 (the antisolar earth-shadow band;
                                                    // x 1.6 lifted the foot 8 levels above mid-wall, fx01)
            #define FF_JET_SPEED 40.0               // BulwarkConst.JetSpeed (m/s at the face)
            // the ending (ADDENDUM A1 00; Beat08Ending): the shuttle guideway from the base station to the platform buffer, its
            // magnet rails and marker lights; the inspection-gallery rows of the lee face (3.11)
            #define FF_GW_X0 (-3514.5)
            #define FF_GW_X1 (-6.5)
            #define FF_GW_Y0 (-6000.6)
            #define FF_GW_Y1 (-5999.5)
            #define FF_GW_Z0 1495.4
            #define FF_GW_Z1 1499.4
            #define FF_GW_JOINT 25.0                // beam joints (m)
            #define FF_GW_MARK 50.0                 // marker lights on both top edges (m)
            #define FF_GALLERY_DH 250.0             // a gallery row every 250 m of altitude
            #define FF_GALLERY_DZ 60.0              // its lamps every 60 m along the face

            struct FarAttributes { float4 positionOS : POSITION; };
            struct FarVaryings { float4 positionCS : SV_POSITION; float3 dirWS : TEXCOORD0; };

            FarVaryings FarVert(FarAttributes v)
            {
                FarVaryings o;
                float3 d = v.positionOS.xyz;                       // unit cube corners; world axes (no rotation)
                o.positionCS = TransformWorldToHClip(_WorldSpaceCameraPos + d);
            #if UNITY_REVERSED_Z
                o.positionCS.z = 1e-6 * o.positionCS.w;            // just in front of the far plane
            #else
                o.positionCS.z = (1.0 - 1e-6) * o.positionCS.w;
            #endif
                o.dirWS = d;
                return o;
            }

            // ---------------------------------------------------------------- noise (PCG hash, quintic value noise)
            uint FFPcg(uint v)
            {
                uint state = v * 747796405u + 2891336453u;
                uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
                return (word >> 22u) ^ word;
            }
            float FFHash(int2 p) { return (FFPcg(asuint(p.x) + FFPcg(asuint(p.y))) >> 8u) * (1.0 / 16777216.0); }
            float FFHash3(int2 p, uint k) { return (FFPcg(asuint(p.x) + FFPcg(asuint(p.y) + FFPcg(k))) >> 8u) * (1.0 / 16777216.0); }
            float FFNoise(float2 x)
            {
                float2 f = floor(x);
                int2 i = (int2)f;
                float2 t = x - f;
                float2 u = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
                float a = FFHash(i), b = FFHash(i + int2(1, 0)), c = FFHash(i + int2(0, 1)), d = FFHash(i + int2(1, 1));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }
            // fbm of value noise, wavelength 1 at octave 0; fp = pixel footprint in the same units. Unresolved octaves are
            // replaced by their mean 0.5 (Nyquist fade 0.25 -> 0.5 footprints per wavelength): no shimmer.
            float FFFbm(float2 p, float fp, int oct)
            {
                float a = 0.5, s = 0.0, n = 0.0, f = 1.0;
                [loop] for (int i = 0; i < oct; i++)
                {
                    float w = 1.0 - smoothstep(0.25, 0.5, fp * f);
                    s += a * lerp(0.5, FFNoise(p * f + float2(17.3 * i, 9.1 * i)), w);
                    n += a; a *= 0.5; f *= 2.03;
                }
                return s / n;
            }
            // Box-filtered band of width w centred at distance 0 for a footprint fp (same units), widened to at least
            // FF_MIN_PX footprints with its alpha scaled down so the energy is kept (H7).
            // An anisotropic glow of a small light (rest radii r0 along u / v) seen with the footprints fu / fv: energy kept
            // (peak x r0^2 / (ru rv)), radius never below 0.9 x 1.5 px, so a far lamp is a soft dot and a near one a disc.
            float FFGlow(float du, float dv, float fu, float fv, float r0)
            {
                // >= 1.8 px (fix wave S3-12: 473 of 3215 KS28 lamp blobs were single pixels at 1.35 px): never a lone bright pixel
                float ru = max(r0, 1.2 * FF_MIN_PX * fu), rv = max(r0, 1.2 * FF_MIN_PX * fv);
                return exp(-(du * du) / (ru * ru) - (dv * dv) / (rv * rv)) * (r0 * r0) / (ru * rv);
            }

            float FFBand(float dist, float w, float fp)
            {
                float we = max(w, FF_MIN_PX * fp);
                float h = 0.5 * max(fp, 1e-4);
                float a = min(dist + h, 0.5 * we), b = max(dist - h, -0.5 * we);
                return saturate(max(a - b, 0.0) / (2.0 * h)) * (w / we);
            }

            // ---------------------------------------------------------------- rivers (shared by water and mist)
            // River k leaves the dam foot at z0 (the spillway z 562, the outlets z 2700 and 5400) and runs away from the
            // dam fanning out W-NW (heading az 284, 309, 328: drift 0.25, 0.8, 1.6 m of z per m of x), so the three never
            // cross; centre line z(x) = z0 + drift (-x) + three sines (3-sine meander SDF), meanders growing over 6 km.
            void FFRiverParams(int k, out float z0, out float dr, out float ph)
            {
                z0 = k == 0 ? 562.0 : (k == 1 ? 2700.0 : 5400.0);
                dr = k == 0 ? 0.25 : (k == 1 ? 0.8 : 1.6);
                ph = 1.7 * k;
            }
            float FFRiverCentre(int k, float x, out float slope)
            {
                float z0, dr, ph;
                FFRiverParams(k, z0, dr, ph);
                float a1 = 2200.0, l1 = 19000.0 + 2500.0 * k;
                float a2 = 700.0, l2 = 6700.0 - 600.0 * k;
                float a3 = 220.0, l3 = 2100.0 + 300.0 * k;
                float w1 = 6.2831853 / l1, w2 = 6.2831853 / l2, w3 = 6.2831853 / l3;
                float xx = x - FF_LEE_X;
                float grow = saturate(-xx / 6000.0);
                float m = a1 * sin(w1 * xx + ph) + a2 * sin(w2 * xx + 2.1 * ph + 1.0) + a3 * sin(w3 * xx + 0.7 * ph + 2.0);
                float dm = a1 * w1 * cos(w1 * xx + ph) + a2 * w2 * cos(w2 * xx + 2.1 * ph + 1.0) + a3 * w3 * cos(w3 * xx + 0.7 * ph + 2.0);
                slope = -dr + dm * grow;
                return z0 - dr * xx + m * grow;
            }
            float FFRiverCentreLow(int k, float x)
            {
                float z0, dr, ph;
                FFRiverParams(k, z0, dr, ph);
                float xx = x - FF_LEE_X;
                return z0 - dr * xx + 2200.0 * sin(6.2831853 / (19000.0 + 2500.0 * k) * xx + ph) * saturate(-xx / 6000.0);
            }
            float FFRiverDist(int k, float2 g, out float x01)
            {
                float sl;
                float zc = FFRiverCentre(k, g.x, sl);
                x01 = saturate((FF_LEE_X - g.x) / 120000.0);
                return abs(g.y - zc) * rsqrt(1.0 + sl * sl);
            }

            // ---------------------------------------------------------------- lowland
            float3 FFFieldColour(float h)
            {
                // palette mixes only (no literal colours): olive, green-grey, ploughed brown, pale stubble
                float3 c0 = lerp(PAL_ConcreteWet, PAL_SafetyOchre, 0.28);
                float3 c1 = lerp(PAL_SeaTeal, PAL_SafetyOchre, 0.36);
                float3 c2 = lerp(PAL_ConcreteDark, PAL_SafetyOchre, 0.32);
                float3 c3 = lerp(PAL_StormHorizon, PAL_SafetyOchre, 0.30) * 0.75;
                float3 c = h < 0.32 ? c0 : (h < 0.66 ? c1 : (h < 0.9 ? c2 : c3));
                return c * 0.5;
            }
            float3 FFFieldMean()
            {
                return (0.32 * FFFieldColour(0.1) + 0.34 * FFFieldColour(0.5) + 0.24 * FFFieldColour(0.8) + 0.1 * FFFieldColour(0.95));
            }

            // Farmland: regions (nearest of jittered points every 5 km) each with a parcel orientation, a parcel size
            // (strips 220-650 m x 500-1500 m) and a crop mix; the whole grid is domain-warped (+-350 m over 2.6 km: rows bend
            // like real field systems that follow the terrain), every row has its own strip width (x0.65-1.35) and offset
            // (bricks), a third of the parcels are split along their length, hedgerows (7 m) stand on about 60 % of the edges
            // and are broken by gaps, and woods (fbm at 900 m, thresholded softly) cover the field pattern in irregular
            // patches: no lattice, no graph paper from the nadir. Every edge is box-filtered by the footprint and the parcel
            // colour fades to the mean once a parcel spans fewer than ~4 footprints (no shimmer, no moire).
            float3 FFLowlandAlbedo(float2 g0, float fp)
            {
                float m1 = FFNoise(g0 / 9000.0), m2 = FFNoise(g0 / 23000.0 + 7.0);
                float macro = 0.8 + 0.4 * (0.6 * m1 + 0.4 * m2);
                float2 wv = float2(FFNoise(g0 / 2600.0 + 3.1), FFNoise(g0 / 2600.0 + 11.7)) - 0.5;
                float2 g = g0 + wv * 700.0;
                const float RC = 5000.0;
                float2 rc = floor(g / RC);
                float d1 = 1e9, d2 = 1e9;
                int2 best = int2(0, 0);
                [unroll] for (int j = -1; j <= 1; j++)
                [unroll] for (int i = -1; i <= 1; i++)
                {
                    int2 c = (int2)(rc + float2(i, j));
                    float2 pc = (float2(c) + 0.5 + (float2(FFHash3(c, 21u), FFHash3(c, 22u)) - 0.5) * 0.8) * RC;
                    float d = length(g - pc);
                    if (d < d1) { d2 = d1; d1 = d; best = c; }
                    else if (d < d2) d2 = d;
                }
                float border = 0.5 * (d2 - d1);                                  // distance to the region border (m)
                float ang = FFHash3(best, 23u) * 3.14159265;
                float ca = cos(ang), sa = sin(ang);
                float2 q = float2(ca * g.x + sa * g.y, -sa * g.x + ca * g.y);
                float2 size = float2(lerp(220.0, 650.0, FFHash3(best, 24u)), lerp(500.0, 1500.0, FFHash3(best, 25u)));
                float row = floor(q.y / size.y);
                int2 rowId = int2(best.x * 131 + (int)row, best.y);
                size.x *= 0.65 + 0.7 * FFHash3(rowId, 28u);                     // each row its own strip width
                float qx = q.x / size.x + FFHash3(rowId, 26u) * 3.0;
                float2 pg = float2(qx, q.y / size.y);
                float2 cell = floor(pg);
                float2 fr = pg - cell;
                int2 ci = int2((int)cell.x + best.x * 7919, (int)cell.y + best.y * 104729);
                // a third of the parcels split in two along their length
                float split = step(FFHash3(ci, 29u), 0.33);
                float sideB = step(0.5, fr.x) * split;
                int2 cj = ci + int2(0, (int)(sideB * 50000.0));
                float hv = frac(FFHash(cj) * 0.7 + FFHash3(best, 27u) * 0.6);   // the region biases the crop mix
                float3 mean = FFFieldMean();
                float3 alb = FFFieldColour(hv) * (0.85 + 0.3 * FFHash3(cj, 7u));
                alb = mean + (alb - mean) * 0.75;
                float cs = min(size.x, size.y);
                float wField = 1.0 - smoothstep(0.12, 0.35, fp / cs);
                // edges: parcel sides (the split line counts as one), the region border; hedges only on some, with gaps
                float ex = min(fr.x, 1.0 - fr.x);
                ex = split > 0.5 ? min(ex, abs(fr.x - 0.5)) : ex;
                float2 e2 = float2(ex * size.x, min(fr.y, 1.0 - fr.y) * size.y);
                float e = min(min(e2.x, e2.y), border);
                float hedgeHere = step(FFHash3(ci + int2(e2.x < e2.y ? 1 : 0, 0), 30u), 0.6);
                float gaps = smoothstep(0.38, 0.48, FFNoise(g / 85.0 + 2.3));
                float hedge = FFBand(e, 7.0, fp) * (1.0 - smoothstep(0.12, 0.35, fp / 900.0)) * hedgeHere * gaps;
                float soft = saturate(e / max(fp, 1e-3));
                alb = lerp(mean, alb, wField * soft);
                alb *= 1.0 - 0.4 * hedge;
                alb *= 0.9 + 0.2 * FFFbm(g / 140.0, fp / 140.0, 3);
                // woods: irregular patches over the fields (soft edge, widened by the footprint)
                float wf = FFFbm(g0 / 900.0 + 5.7, fp / 900.0, 4);
                float wedge = max(0.025, 0.6 * fp / 900.0);
                float woods = smoothstep(0.6 - wedge, 0.6 + wedge, wf);
                float3 woodC = lerp(PAL_SeaTeal, PAL_ConcreteWet, 0.55) * 0.42 * (0.85 + 0.3 * FFFbm(g0 / 60.0, fp / 60.0, 2));
                alb = lerp(alb, woodC, woods);
                return alb * macro;
            }

            // ---------------------------------------------------------------- ground lighting
            // Key on a surface with normal n at altitude h (sun-break hole, earth shadow); ambient from the EXTERIOR trilight
            // (the far field is never inside: the s-keyed _BwAmb* blend of the tunnel mouth must not tint the lowland).
            float3 FFLight(float3 n, float h, float2 xz, float3 up)
            {
                float nl = saturate(dot(n, _BwSunDir.xyz));
                float seL = dot(_BwSunDir.xyz, up);
                float lit = BwEarthLitN(h, seL);   // the penumbra (3.7); the reddening is the face's (FFFaceShade)
                float brk = BwSunBreakAt(xz, h);
                float3 amb = _BwAmbExtSky.rgb * saturate(n.y) + _BwAmbExtMid.rgb * (1.0 - abs(n.y)) + _BwAmbExtGround.rgb * saturate(-n.y);
                return amb + _BwSunColor.rgb * (nl * lit * brk);
            }

            // ---------------------------------------------------------------- city lights (density field, never points)
            // Urban density (14.7 H4): rho = [sum over hashed city centres (spacing 60-90 km, radius 3-9 km)
            // exp(-(r/Rc)^1.5) + 0.6 x the same for market towns (spacing 9-15 km, radius 0.8-2 km, 70 % of cells)]
            // x (0.6 + 0.4 fbm(p / 400 m)). Each density is summed over the 2 x 2 nearest cells (the jitter keeps every
            // other centre beyond twice its radius). Settlements: one candidate per 0.6 km cell (jittered), present where
            // 1.25 rho + 0.025 exceeds its hash (towns are dense, the countryside keeps a 2.5 % scatter of farms: dark
            // between the towns), a Gaussian pool of sigma 15-50 m (2-8 px blobs at 7-40 km), warm or cool by hash, radiance
            // N(s) (0.35 + 0.65 rho). Each pool is convolved with the pixel footprint (anisotropic: across and along the
            // view on the ground) and its peak scaled so its energy is kept: a far town is a dim smear, a near one a
            // 2-8 px blob; never a point, nothing shimmers.
            float FFDensityLayer(float2 g, float cell, float jitter, float r0, float r1, float keep, uint seed)
            {
                float2 c0 = floor(g / cell - 0.5);
                float rho = 0.0;
                [unroll] for (int j = 0; j <= 1; j++)
                [unroll] for (int i = 0; i <= 1; i++)
                {
                    int2 c = (int2)(c0 + float2(i, j));
                    if (FFHash3(c, seed + 4u) > keep) continue;
                    float2 ctr = (float2(c) + 0.5 + (float2(FFHash3(c, seed), FFHash3(c, seed + 1u)) - 0.5) * jitter) * cell;
                    float rc = lerp(r0, r1, FFHash3(c, seed + 2u));
                    float u = length(g - ctr) / rc;
                    rho += exp(-u * sqrt(u));
                }
                return rho;
            }

            float FFCityDensity(float2 g, float fp)
            {
                // the hashed cities and towns, plus the city that lies 20 km west-north-west of the ledge (az 310 from
                // the KS20 eye, radius 10 km): the near lowland of the dusk views (14.5.7 acceptance (2))
                float2 dc = (g - float2(FF_ANCHOR_X, FF_ANCHOR_Z)) / FF_ANCHOR_R;
                float ua = length(dc);
                // the town round the lantern platform (Wave X ending: "a small lit plaza at the edge of the town lights"): centre
                // 800 m west of the plaza, radius 900 m (rho 0.43 at the plaza, 0.1 at 1.9 km east of it)
                float ut = length(g - float2(-4300.0, 1450.0)) / 900.0;
                float rho = FFDensityLayer(g, FF_CITY_CELL, 0.6, 3000.0, 9000.0, 1.0, 1u)
                          + 0.6 * FFDensityLayer(g, FF_TOWN_CELL, 0.7, 800.0, 2000.0, 0.7, 11u)
                          + exp(-ua * sqrt(ua)) + exp(-ut * sqrt(ut));
                return saturate(rho) * (0.6 + 0.4 * FFFbm(g / 400.0, fp / 400.0, 2));
            }

            // City lights (14.7 H4): radiance N(s) x rho(p) x smoothstep(0.55, 0.8, F(p / 90 m)), F = a 3-octave value fBm.
            // A light is a SOURCE, not a texture: the ground holds one candidate light per jittered 120 m cell; its strength is
            // the H4 product sampled at its own position (rho of the town density, the 90 m urban pattern, a 650 m district
            // mask that leaves parks and fields dark), and it is drawn as a Gaussian pool of sigma 12-30 m widened by the pixel
            // footprint across and along the view (energy kept): a near light is a 2-8 px blob, never a point, and its image is
            // round however grazing the view (a thresholded TEXTURE of the same field aliased into square speckle at 20 km:
            // value-noise isocontours follow the lattice and the along-view footprint is 3-4x the across one). Where the
            // footprint along the view passes 0.5 .. 1.0 cell, the pool sum blends into its expectation (the same pools
            // spread evenly: K rho E[ss] E[district] 2 pi E[sigma^2] / cell^2, E[ss] 0.143 from the fBm's distribution): far
            // towns are a warm glow of the same energy. Warm CityWarm-led; a fifth of the lights cool. None while N(s) = 0.
            float3 FFCity(float2 g, float fpAcross, float fpAlong, float2 viewXZ)
            {
                if (_BwFar0.x <= 0.0) return 0.0;
                float rho = FFCityDensity(g, fpAlong);
                if (rho < 0.02) return 0.0;
                const float CC = 120.0;
                float farW = smoothstep(0.5, 1.0, fpAlong / CC);
                float2 va = normalize(viewXZ + 1e-6);
                float2 vc = float2(-va.y, va.x);
                float3 acc = 0.0;
                if (farW < 1.0)
                {
                    float2 c0 = floor(g / CC);
                    [unroll] for (int j = -1; j <= 1; j++)
                    [unroll] for (int i = -1; i <= 1; i++)
                    {
                        int2 c = (int2)(c0 + float2(i, j));
                        float2 ctr = (float2(c) + 0.5 + (float2(FFHash3(c, 32u), FFHash3(c, 33u)) - 0.5) * 0.8) * CC;
                        float b = smoothstep(0.55, 0.8, FFFbm(ctr / 90.0, 0.0, 3));
                        if (b <= 0.0) continue;
                        b *= smoothstep(0.42, 0.62, FFFbm(ctr / 650.0 + 3.7, 0.0, 2));
                        if (b <= 0.0) continue;
                        float sig = lerp(12.0, 30.0, FFHash3(c, 34u));
                        float2 d = g - ctr;
                        float sa = sqrt(sig * sig + 0.25 * fpAlong * fpAlong), sx = sqrt(sig * sig + 0.25 * fpAcross * fpAcross);
                        float da = dot(d, va) / sa, dx = dot(d, vc) / sx;
                        float k = exp(-0.5 * (da * da + dx * dx)) * (sig * sig / (sa * sx));
                        float3 tint = lerp(PAL_CityWarm, PAL_CityCool, step(0.8, FFHash3(c, 35u)));
                        acc += tint * (k * b * (0.6 + 0.8 * FFHash3(c, 36u)));
                    }
                }
                float3 meanTint = lerp(PAL_CityWarm, PAL_CityCool, 0.2);
                float3 mean = meanTint * (0.143 * 0.5 * 6.2832 * 468.0 / (CC * CC));
                // the town glow: the streets between the pools and the light scattered in the haze over a dense centre, a
                // warm km-scale gradient (x rho^2 overall: the cores of the towns read as warm patches, the countryside stays
                // dark; no texture, so nothing can shimmer)
                float3 glow = PAL_CityWarm * (0.143 * 0.5 * 6.2832 * 468.0 / (CC * CC)) * 0.6 * rho;
                return (lerp(acc, mean, farW) + glow) * (FF_CITY_K * _BwFar0.x * rho);
            }

            // ---------------------------------------------------------------- mist rivers (cloud banks in the slab 900-2200 m)
            // The mist river is a bank of cold cloud flowing down its valley (the gravity current of 14.7 H2). Across the river
            // a flat-topped profile c = smoothstep(0.12, 0.6, m) of m = exp(-1.6 u^2) (u = distance / half width 1.5 -> 4 km:
            // 3-8 km wide), the width breathing along the river (x 0.55-1 at 7 km); its TOP at LO + (HI - LO) c (0.5 + 0.5 b)
            // (b: a 3-octave fbm of billows 1.1 km across, flowing at 4 m/s with the unwrapped clock _BwFlow.z). The ray is
            // tested at 11 fixed nodes between the slab entry and exit (equal altitude steps of 130 m at any angle, no jitter);
            // F = max(h - top, LO - h) changes sign where it enters / leaves the bank and every crossing is placed by the secant,
            // so the entry point and the length inside are continuous in the pixel direction (no terraces of sample planes).
            // Opacity 1 - exp(-sigma L_inside); shading at the entry point: the sun through two taps toward it (400 and 1500 m,
            // descending with the local sun elevation: billows shade each other, the sun-facing upper edges glow), the earth's
            // shadow (soft +-300 m: near the dam only tops above ~1.6 km are lit: AMBER UPPER EDGES and violet-grey lower
            // parts, 14.5.7), a phase of half isotropic, half Henyey-Greenstein g 0.55 (silver lining toward the low sun), and
            // the exterior sky light. Off-river nodes (m < 0.02) skip the noise.
            #define FF_MIST_SIGMA 1.0e-3           // fix wave S2-2: 4.5e-3 made opaque dunes (alpha ~1 within 100-200 m)
            #define FF_MIST_RAMP 250.0             // the density ramps from 0 at the bank surface over 250 m (soft tops)
            #define FF_MIST_N 10
            float FFMistM(float2 g)
            {
                float m = 0.0;
                [unroll] for (int k = 0; k < 3; k++)
                {
                    float z0, dr, ph;
                    FFRiverParams(k, z0, dr, ph);
                    float dd = abs(g.y - FFRiverCentreLow(k, g.x)) * rsqrt(1.0 + dr * dr);
                    float halfW = lerp(1500.0, 4000.0, saturate((FF_LEE_X - g.x) / 60000.0));
                    float u = dd / halfW;
                    m = max(m, exp(-1.6 * u * u));
                }
                return m * (0.55 + 0.45 * FFNoise(g / 7000.0 + 2.9)) * smoothstep(0.0, 2500.0, FF_LEE_X - g.x);
            }
            float FFMistTop(float2 g, float m, float fp)
            {
                float c = smoothstep(0.12, 0.6, m);
                if (c <= 0.0) return FF_MIST_LO - 400.0;
                // billows of ~1100 / 540 / 270 / 130 m (4 octaves: the near banks keep their small billows, the far ones fade to
                // the mean by their footprint)
                float b = FFFbm(float2(g.x + _BwFlow.z, g.y) / 1100.0, fp / 1100.0, 4);
                return min(FF_MIST_LO + (FF_MIST_HI - FF_MIST_LO) * c * (0.5 + 0.5 * b), FF_MIST_HI);
            }
            // signed distance-like test: <= 0 inside the bank (below its top and above LO)
            float FFMistF(float2 g, float h, float fp)
            {
                float m = FFMistM(g);
                float top = m < 0.02 ? FF_MIST_LO - 400.0 : FFMistTop(g, m, fp);
                return max(h - top, (FF_MIST_LO - 50.0) - h);
            }
            float4 FFMist(float3 dir, float tGround, out float tEntryOut)
            {
                tEntryOut = 0.0;
                if (_BwFar0.y <= 0.0) return 0.0;
                float h0 = _BwFarCam.x;
                float r0 = BW_EARTH_R + h0;
                float tIn = BwRaySphereNear(r0, (h0 - FF_MIST_HI) * (2.0 * BW_EARTH_R + h0 + FF_MIST_HI) / (r0 * r0), dir.y);
                if (tIn <= 0.0) return 0.0;
                float tOut = BwRaySphereNear(r0, (h0 - FF_MIST_LO) * (2.0 * BW_EARTH_R + h0 + FF_MIST_LO) / (r0 * r0), dir.y);
                tOut = tOut > 0.0 ? min(tOut, tGround) : min(tIn + 60000.0, tGround);
                if (tOut <= tIn) return 0.0;
                float dt = (tOut - tIn) / FF_MIST_N;
                float fpk = _BwCam.w / max(-dir.y, 0.03);
                // billows smaller than twice the horizontal node spacing are faded to their mean (they would be sampled as
                // stair steps along a grazing ray): the far banks get smooth tops, the near ones keep their billows
                float fpNode = 0.5 * dt * length(dir.xz);
                // ---- crossings of the bank surface along the ray
                // density field (fix wave S2-2): 0 at the bank surface, ramping in over FF_MIST_RAMP of depth (-F = distance to
                // the nearer of the top and the floor), integrated per segment with the trapezoid rule on the crossing part
                float tEntry = -1.0, inside = 0.0;
                float Fa = FFMistF(_BwCam.xz + dir.xz * tIn, BwAltAt(h0, dir.y, tIn), max(tIn * fpk, fpNode));
                if (Fa <= 0.0) tEntry = tIn;
                [loop] for (int i = 1; i <= FF_MIST_N; i++)
                {
                    float t = tIn + i * dt;
                    float Fb = FFMistF(_BwCam.xz + dir.xz * t, BwAltAt(h0, dir.y, t), max(t * fpk, fpNode));
                    float da = smoothstep(0.0, FF_MIST_RAMP, -Fa), db = smoothstep(0.0, FF_MIST_RAMP, -Fb);
                    if (Fa <= 0.0 && Fb <= 0.0) inside += dt * 0.5 * (da + db);
                    else if (Fa <= 0.0 || Fb <= 0.0)
                    {
                        float f = Fa / (Fa - Fb);                       // secant: the crossing between the two nodes
                        inside += Fa <= 0.0 ? f * dt * 0.5 * da : (1.0 - f) * dt * 0.5 * db;
                        if (Fa > 0.0 && tEntry < 0.0) tEntry = t - dt + f * dt;
                    }
                    Fa = Fb;
                }
                if (tEntry < 0.0 || inside <= 0.0) return 0.0;
                tEntryOut = tEntry;
                float alpha = 1.0 - exp(-FF_MIST_SIGMA * _BwFar0.y * inside);
                // ---- light at the entry point
                float3 p = dir * tEntry;
                float2 g = _BwCam.xz + p.xz;
                float h = BwAltAt(h0, dir.y, tEntry);
                float3 up = normalize(p - float3(0.0, -r0, 0.0));   // pal:ok (not a colour)
                float se = dot(_BwSunDir.xyz, up);                    // sine of the local sun elevation
                float2 sxz = _BwSunDir.xz * rsqrt(max(dot(_BwSunDir.xz, _BwSunDir.xz), 1e-6));
                float fpm = max(tEntry * fpk, fpNode);
                float o1 = saturate(1.0 - FFMistF(g + sxz * 400.0, h + 400.0 * se, fpm) / 120.0);
                float o2 = saturate(1.0 - FFMistF(g + sxz * 1500.0, h + 1500.0 * se, fpm) / 120.0);
                float sunT = exp(-4.0e-3 * (400.0 * o1 + 1100.0 * o2));
                float lit = BwEarthLitN(h, se);   // the penumbra; no alpenglow tint: the mist's amber tops are tuned on the key (14.5.7 KS20)
                float cs = dot(dir, _BwSunDir.xyz);
                float hg = (1.0 - 0.3025) / pow(max(1.3025 - 1.1 * cs, 1e-3), 1.5);
                float phase = 0.5 + 0.5 * hg;
                float lowSun = _BwFogLow.y;
                float kSun = lerp(0.42, 0.22, lowSun), kSky = lerp(0.5, 0.24, lowSun);
                // the top of a bank is lit by the whole sky, its flanks and the hollows between billows less
                float topness = saturate(1.0 - FFMistF(g, h + 120.0, fpm) / 240.0);   // 1 near the top, 0 deep in a hollow
                // multiple scattering lifts the hollows and undersides (fix wave S2-2: never darker than the haze around them)
                float3 J = _BwSunColor.rgb * (kSun * phase * sunT * lit * BwSunBreakAt(g, h))
                         + _BwAmbExtSky.rgb * (kSky * (0.8 + 0.2 * topness)) + _BwAmbExtMid.rgb * (0.12 * kSky);
                return float4(J * alpha, alpha);   // pal:ok (rgb in-scatter, a = opacity)
            }

            // ---------------------------------------------------------------- outlet jets (five falling jets of water)
            // Each jet leaves its outlet in the lee face (z_j, y -90) at 40 m/s as a ribbon 12 m wide (along z) and ~2.5 m thick,
            // on the parabola y = -90 - k u^2 (u = metres west of the face, 14.5.7), and turns to mist at 1 km of fall. The ray
            // meets the jet's centre plane z = z_j; the 12 m width projects into that plane along the ray, so the band's half
            // width across the centre line is 0.5 thickness + 6 |dir.xy . n_c| / |dir.z| (n_c the in-plane normal): the true
            // outline of the ribbon from any viewpoint. Inside: a dense aerated core (FFBand: box-filtered, at least 1.5 px,
            // energy kept), clumps of water advected with the parcels (value noise in (u - 40 t) / 15 m, the unwrapped clock
            // _BwFlow.w: no 3600 s seam) that break the core apart more and more with the fall (opacity 0.95 -> 0.4, breakup
            // 0.3 -> 0.85), and a translucent spray envelope widening with the fall (half width + 0.06 fall). Shading of a
            // volume of foam: a cylinder normal across the core (lit side toward the low sun, sky-lit side away), the key times
            // (0.35 + 0.65 n.l) and the earth shadow, the exterior sky light, and a forward-scatter glow of the envelope toward
            // the sun (Henyey-Greenstein g 0.6). Fogged with the closed form (within 7 km).
            float4 FFJets(float3 dir, float tMax)
            {
                float4 acc = 0.0;
                if (abs(dir.z) < 1e-4) return acc;
                float3 Lsun = _BwSunDir.xyz;
                float cs = dot(dir, Lsun);
                float hg = (1.0 - 0.36) / pow(max(1.0 + 0.36 - 1.2 * cs, 1e-3), 1.5);
                [loop] for (int j = 0; j < 5; j++)
                {
                    float zj = j == 0 ? 1900.0 : (j == 1 ? 2700.0 : (j == 2 ? 3900.0 : (j == 3 ? 5400.0 : 7200.0)));   // BulwarkConst.JetZ
                    float t = (zj - _BwCam.z) / dir.z;
                    if (t <= 0.0 || t >= tMax) continue;
                    float3 Q = _BwCam.xyz + dir * t;
                    float u = FF_LEE_X - Q.x;                       // metres west of the face
                    if (u < -1.0 || u > 700.0) continue;
                    float uu = max(u, 0.0);
                    float yp = FF_JET_Y0 - FF_JET_K * uu * uu;
                    float sl = 2.0 * FF_JET_K * uu;
                    float rs = rsqrt(1.0 + sl * sl);
                    float2 nc = float2(-sl, 1.0) * rs;              // in-plane normal (x, y): up, and west on the falling part
                    float across = (Q.y - yp) * rs;                 // signed distance across the centre line (m)
                    float fall = FF_JET_Y0 - yp;
                    float thick = 2.5 + 0.08 * fall;
                    // clumps of water along the jet (value noise fixed to the parcels, advected at 40 m/s with the unwrapped
                    // clock) swell and thin the core: no uniform ribbon
                    float nW = FFFbm(float2((u - _BwFlow.w) / 22.0, 3.1 * j), 0.0, 2);
                    thick *= 0.75 + 0.5 * nW;
                    float halfB = 0.5 * thick + 6.0 * abs(dot(dir.xy, nc)) / max(abs(dir.z), 0.05);
                    float halfS = halfB + 2.0 + 0.06 * fall;
                    float fp = t * _BwCam.w / max(abs(dir.z), 0.15);
                    float dist = abs(across);
                    float start = smoothstep(-1.0, 2.0, u);
                    float cov = FFBand(dist, 2.0 * halfB, fp) * start;
                    float env = exp(-(dist * dist) / max(halfS * halfS, 1e-3)) * start;
                    if (cov <= 0.0 && env < 1e-3) continue;
                    float resolved = saturate(halfB / max(fp, 1e-3) - 1.0);       // 0 for a 1.5-2 px jet
                    float a = clamp(across / max(halfB, 1e-3), -1.0, 1.0);
                    // striations ALONG the fall (white aerated streaks 40 m long, 0.3 of the width across), advected with the
                    // water; the edges fray with the fall (no holes through the core: an aerated jet stays a jet)
                    float n = FFFbm(float2((u - _BwFlow.w) / 40.0, a * 3.0 + 7.3 * j), fp / 12.0, 3);
                    float m = smoothstep(0.3, 0.7, n);
                    float breakup = lerp(0.2, 0.8, smoothstep(30.0, 700.0, fall));
                    float fray = FFNoise(float2((u - _BwFlow.w) / 18.0 + 5.1 * j, sign(across) * 3.7));
                    float edge = 1.0 - smoothstep(1.0 - 0.45 * breakup * (0.4 + 0.6 * fray), 1.0, abs(a));
                    float mist = smoothstep(450.0, FF_JET_FALL, fall);
                    float op = lerp(0.97, 0.7, smoothstep(0.0, 900.0, fall)) * (1.0 - mist);
                    op *= (0.8 + 0.2 * m) * lerp(1.0, edge, resolved);
                    float envA = (0.05 + 0.32 * smoothstep(60.0, 600.0, fall)) * (1.0 - 0.6 * mist) * (0.6 + 0.8 * n);
                    // cylinder normal of the core: in-plane normal across, toward the eye along the line of sight
                    float3 n3 = float3(nc.x, nc.y, 0.0);                                  // pal:ok (normal)
                    float3 tv = normalize(float3(-rs, -sl * rs, 0.0));                    // pal:ok (tangent)
                    float3 tw = -dir - tv * dot(-dir, tv);
                    tw *= rsqrt(max(dot(tw, tw), 1e-6));
                    float3 nJ = normalize(n3 * a + tw * sqrt(saturate(1.0 - a * a)));
                    float hq = BwAltAt(_BwFarCam.x, dir.y, t);
                    float3 lit = BwEarthLitN(hq, _BwSunDir.y) * BwAlpenTint(hq, _BwSunDir.y);
                    float3 amb = _BwAmbExtSky.rgb * saturate(nJ.y) + _BwAmbExtMid.rgb * (1.0 - abs(nJ.y)) + _BwAmbExtGround.rgb * saturate(-nJ.y);
                    float3 key = _BwSunColor.rgb * (lit * BwSunBreakAt(Q.xz, hq));
                    // aerated water is a bright, translucent scatterer: wrapped key on the sun side (gold), the sky light on the
                    // far side (violet), and light carried THROUGH the foam toward the eye when it looks toward the low sun
                    // (forward lobe): a white-gold jet against the dark lowland, never a flat-coloured ribbon
                    float bright = 0.6 + 0.8 * m;
                    float3 core = PAL_FoamWhite * (amb * 1.15 + key * (0.18 + 0.82 * saturate(dot(nJ, Lsun)) + 0.22 * hg)) * bright;
                    float3 spray = PAL_FoamWhite * (amb * 0.9 + key * (0.25 + 0.4 * hg));
                    float3 jc = BwFogApply(lerp(spray, core, saturate(cov * op / max(cov * op + envA * env, 1e-4))), Q);
                    float c = saturate(cov * op + envA * env * (1.0 - cov * op));
                    acc.rgb = lerp(acc.rgb, jc, c * (1.0 - acc.a));
                    acc.a = acc.a + c * (1.0 - acc.a);
                }
                return acc;
            }

            // ---------------------------------------------------------------- the jets' falling veils (Wave X ending)
            // At 1 km of fall a jet is spray, and the spray keeps falling the remaining 4.9 km to the lowland: a veil hanging
            // 600 m out from the face (the plane x = -606, about where the arcs turn to mist), one soft curtain per jet (half
            // width 22 m at the mist point, widening by a twentieth of the extra fall), density 0.34 fading in over the first 150 m
            // (it continues the arcs' own mist fade) and out over 1.6 km of fall (it evaporates into the haze), torn by falling striations (fbm advected down
            // with the jets' unwrapped flow, 40 m/s: no seam). From the lantern platform it turns the arcs over the crest into five
            // waterfalls 5 km tall. Lit like the jets' spray (exterior sky light, the earth-shadowed key with its alpenglow tint,
            // a forward lobe), fogged with the closed form. Ending only (_BwEndRide.z), so beat 7's key shots keep their look.
            float4 FFJetVeils(float3 dir, float tMax)
            {
                float4 acc = 0.0;
                // seen from afar only (the shuttle's second half and the platform): from the cabin the veil 600 m out stood as a
                // sunlit pillar in front of the sunset (ending-16 KS23); it fades in while the shuttle runs from x -700 to -1800
                float wEnd = saturate(_BwEndRide.z) * smoothstep(-700.0, -1800.0, _BwCam.x);
                if (wEnd <= 0.0 || abs(dir.x) < 1e-5) return acc;
                const float XV = FF_LEE_X - 600.0;
                float t = (XV - _BwCam.x) / dir.x;
                if (t <= 0.0 || t >= tMax) return acc;
                float3 Q = _BwCam.xyz + dir * t;
                float fall = (FF_JET_Y0 - FF_JET_FALL) - Q.y;          // metres below the mist point (y -1090)
                if (fall <= 0.0 || Q.y < -6000.0) return acc;
                float fpv = t * _BwCam.w / max(abs(dir.x), 0.1);
                float3 Lsun = _BwSunDir.xyz;
                float cs = dot(dir, Lsun);
                float hg = (1.0 - 0.36) / pow(max(1.0 + 0.36 - 1.2 * cs, 1e-3), 1.5);
                float hq = BwAltAt(_BwFarCam.x, dir.y, t);
                float3 lit = BwEarthLitN(hq, Lsun.y) * BwAlpenTint(hq, Lsun.y);
                float3 key = _BwSunColor.rgb * lit * BwSunBreakAt(Q.xz, hq);
                // sky light of a white scatterer facing the western twilight (the face's afterglow term, 2 taps west at el 10 / 40 deg
                // and the zenith, calibrated on the trilight like FFFaceShade): the spray reads white against the dark concrete
                float3 Wd = float3(-0.985, 0.174, 0.0), Wu = float3(-0.766, 0.643, 0.0), Up = float3(0.0, 1.0, 0.0);   // pal:ok (directions)
                float kV = BwLumaA(_BwAmbExtSky.rgb) / max(BwLumaA(BwSkyColor(Up)), 1e-5);
                float3 ambV = max(_BwAmbExtMid.rgb, 0.5 * kV * (0.5 * BwSkyColor(Wd) + 0.3 * BwSkyColor(Wu) + 0.2 * BwSkyColor(Up)) + 0.5 * _BwAmbExtGround.rgb);
                float3 sc = BwFogApply(PAL_FoamWhite * (ambV * 0.9 + key * (0.25 + 0.4 * hg)), Q);
                float hw = 22.0 + 0.05 * fall;
                float dens = 0.42 * wEnd * smoothstep(0.0, 150.0, fall) * exp(-fall / 1600.0);
                [loop] for (int j = 0; j < 5; j++)
                {
                    float zj = j == 0 ? 1900.0 : (j == 1 ? 2700.0 : (j == 2 ? 3900.0 : (j == 3 ? 5400.0 : 7200.0)));   // BulwarkConst.JetZ
                    float dz = Q.z - zj;
                    if (abs(dz) > 3.0 * hw) continue;
                    float n = FFFbm(float2(dz / (0.5 * hw) + 5.3 * j, (Q.y + _BwFlow.w) / 250.0), max(fpv / (0.5 * hw), fpv / 250.0), 3);
                    float a = saturate(dens * exp(-(dz * dz) / (hw * hw)) * (0.2 + 1.6 * smoothstep(0.32, 0.68, n)));
                    acc.a = acc.a + a * (1.0 - acc.a);
                }
                return float4(sc, acc.a);   // pal:ok (one colour for every veil: straight, not premultiplied, for the lerp in FarFrag)
            }

            // ---------------------------------------------------------------- clouds (deck at 6.8 km)
            float3 FFClouds(float3 dir, float3 sky, out float alpha)
            {
                alpha = 0.0;
                float h0 = _BwFarCam.x;
                if (h0 >= FF_CLOUD_ALT || _BwCloud0.z <= 0.0) return sky;
                float tc = BwRaySphereExit(h0, FF_CLOUD_ALT, dir.y);
                // two cloud fields, each moved by its own wind x the UNWRAPPED clock (_BwFlow.x storm 12 m/s, .y fair
                // 4 m/s; no 3600 s seam and no slide when the wind speed changes): the storm deck dissolves into the
                // fair-weather field over the sky window (_BwFar0.z = fair weight); both are sampled only inside it
                const float2 windDir = float2(-0.94, 0.34);
                float2 p0 = _BwCam.xz + dir.xz * tc;
                float mu = max(dir.y + tc / (2.0 * BW_EARTH_R), 0.02);
                float fp = tc * _BwCam.w / mu;
                float wf = saturate(_BwFar0.z);
                float n = 0.0;
                if (wf < 1.0) n += (1.0 - wf) * FFFbm((p0 + windDir * _BwFlow.x) / 3200.0, fp / 3200.0, 5);
                if (wf > 0.0) n += wf * FFFbm((p0 + windDir * _BwFlow.y) / 3200.0 + 41.7, fp / 3200.0, 5);
                float a = _BwCloud0.z * smoothstep(_BwCloud0.x, _BwCloud0.y, n);
                // the sun-break hole in the deck (the deck point's sun shadow on the ground in the lit disc)
                float brk = BwSunBreakAt(p0, FF_CLOUD_ALT);
                a *= 1.0 - saturate((brk - (1.0 - 0.85 * _BwSunHole.x)) / max(0.85 * _BwSunHole.x, 1e-3)) * step(1e-3, _BwSunHole.x);
                float tex = 1.0 + _BwCloud0.w * (n - 0.5);
                float3 cc = lerp(sky * tex, _BwCloud1.rgb * tex, _BwCloud1.w);
                alpha = a;
                return lerp(sky, cc, a);
            }

            // ---------------------------------------------------------------- the far sea (smooth sea of variance s2)
            // the far sea: BwSeaSurfaceS2 of BulwarkAtmosphere.hlsl (shared with the graybox sea plane and the BwSea builder)
            // the slope variance is the sea owner's fully filtered spectrum (BwSeaFarS2, what the clipmap reaches at 2048 m),
            // so the clipmap edge does not show (14.7 H1, T7); the schedule's 0.0133 until SeaState publishes it
            float3 FFSea(float3 dir, float3 n)
            {
                float s2 = BwSeaFarS2();
                return BwSeaSurfaceS2(dir, n, s2 > 1e-4 ? s2 : _BwSea0.w);
            }

            // ---------------------------------------------------------------- the lee face beyond the near meshes
            // The analytic face continues the real face meshes (the beat-7 builder's z 1000..1800 x y -120..0 patch, the crest
            // builders' faces) with the same look as BwConcrete in ConcreteDark (Part 10 #34 tier crossover): formwork panels
            // 3.0 x 1.5 m with 12 mm gaps box-filtered by the per-axis footprint and faded below 4 px of their spacing, the
            // per-panel pour tone +-2.5 % (the same BwHash21 of the panel cell), the macro tone +-10 % at 40 m (the same 3D fBm
            // of the world position), the texture set's mean albedo (= the token) and occlusion (0.9); lit like the lit
            // template: diffuse x (exterior trilight + key x n.l, earth shadow, sun break), GGX key specular (F0 0.04,
            // roughness 0.49 = smoothness 0.30) and the grazing environment term of URP's EnvironmentBRDFSpecular (which makes
            // the far, grazing part of a rough wall brighter: dropping it would show a step at the mesh edge).
            float FFGgx(float3 N, float3 L, float3 V, float a)
            {
                float3 H = normalize(L + V);
                float NoH = saturate(dot(N, H)), LoH = saturate(dot(L, H));
                float a2 = a * a;
                float d = NoH * NoH * (a2 - 1.0) + 1.00001;
                return min(a2 / ((d * d) * max(0.1, LoH * LoH) * (4.0 * a + 2.0)), 100.0);
            }

            float3 FFFaceShade(float3 fpos, float3 dir, float3 N, float hp, float3 up, float fpi, float3 dF)
            {
                float2 q = fpos.zy;
                float2 fq = max(dF.zy, 1e-4);
                // the descent's motion filter: the face slides past the cabin at up to 110 m/s; every pattern is box-filtered
                // over the distance it moves in half a frame (a 180-degree shutter), so it blurs along y instead of strobing
                fq.y = max(fq.y, _BwEndRide.y);
                fpi = max(fpi, _BwEndRide.y);
                const float2 size = float2(3.0, 1.5);
                float2 w = q - size * floor(q / size);
                float gx = BwFilteredGrid1(w.x, size.x, 0.012, fq.x, 4.0), gy = BwFilteredGrid1(w.y, size.y, 0.012, fq.y, 4.0);
                float gap = saturate(gx + gy - gx * gy);
                float panelPx = min(size.x / fq.x, size.y / fq.y);
                float panelFade = saturate((panelPx - 4.0) / 4.0);
                float pour = (BwHash21(int2(floor(q / size))) - 0.5) * 0.05 * panelFade;
                float macro = BwFbmFiltered3(fpos / 40.0, fpi / 40.0, 3);
                float tone = 1.0 + 0.10 * (2.0 * macro - 1.0);    // _MacroTone 0.10 of the beat-7 face (Beat07SluiceLedge.FaceMacroTone)
                // a 6 km wall reads by its rhythm (ending, ADDENDUM A1 00): weathering streaks down the face (fbm stretched 30:1
                // along y) and the monolith joints every 18 m (0.6 m dark grooves; box-filtered, they stay as a faint vertical
                // rhythm of 12-15 px at 3.5 km and fade below 4 px of spacing)
                float streak = FFFbm(float2(q.x / 9.0, q.y / 280.0), max(fq.x / 9.0, fq.y / 280.0), 3);
                tone *= 0.88 + 0.24 * streak;
                float jz = q.x - 18.0 * floor(q.x / 18.0 + 0.5);
                float joint = BwFilteredLine(jz, 0.6, fq.x) * saturate((18.0 / max(fq.x, 1e-3) - 4.0) / 4.0);
                tone *= 1.0 - 0.35 * joint;
                // the far rhythm (Wave X, the money shot at 3.5-7 km where every pattern above is under a pixel: the face read as
                // one flat panel): each 18 m monolith column has its own pour tone per 250 m lift (+-6 %, faded to the mean below
                // 3 px), groups of five (90 m: 24 px at 3.5 km) a second tone (+-5 %); weathering over 120 m x 900 m (+-14 %); and
                // seepage under every gallery row: a dark streak 3-8 m wide at each drain (every 60 m, between the lamps), 40-160 m
                // long, fading down (box-filtered across: a 1.5 px line at 3.5 km keeps its energy)
                float liftI = floor((hp - 0.8) / FF_GALLERY_DH);
                float wA = saturate((18.0 / max(fq.x, 1e-3) - 3.0) / 3.0);
                float wB = saturate((90.0 / max(fq.x, 1e-3) - 3.0) / 3.0);
                // the lift tones blend over +-20 m round each gallery row (a hard step swept the near face in one frame at
                // 110 m/s: a diff spike every 250 m in seq_ride_descent, ending-seq24 frames 69 / 137 / 205)
                float rowN = round((hp - 0.8) / FF_GALLERY_DH);
                float wL = smoothstep(-20.0, 20.0, (hp - 0.8) - rowN * FF_GALLERY_DH);
                int cA = (int)floor(q.x / 18.0), cB = (int)floor(q.x / 90.0);
                // the ending (seen from the shuttle and the platform through 3.5-7 km of haze): the far amplitudes x2 so they
                // survive the haze (fix wave S2-1 c: 3-5 levels), and a perspective rhythm of monolith blocks 150 m wide (a soft
                // buttress-like shading across each and a dark 1.5 m joint between them, box-filtered: 36 px apart at 3.5 km)
                float wFar = saturate(_BwEndRide.z) * smoothstep(-700.0, -1800.0, _BwCam.x);
                float amp = 1.0 + wFar;
                float tA = (lerp(FFHash3(int2(cA, (int)rowN - 1), 51u), FFHash3(int2(cA, (int)rowN), 51u), wL) - 0.5) * 0.16 * wA * amp;
                float tB = (lerp(FFHash3(int2(cB, (int)rowN - 1), 52u), FFHash3(int2(cB, (int)rowN), 52u), wL) - 0.5) * 0.14 * wB * amp;
                float wth = FFFbm(float2(q.x / 120.0, q.y / 900.0) + 17.3, max(fq.x / 120.0, fq.y / 900.0), 4);
                tone *= (1.0 + tA + tB) * (1.0 + 0.2 * amp * (2.0 * wth - 1.0));
                if (wFar > 0.0)
                {
                    float wM = saturate((150.0 / max(fq.x, 1e-3) - 4.0) / 4.0);
                    float uM = q.x / 150.0 - floor(q.x / 150.0);
                    float jM = q.x - 150.0 * floor(q.x / 150.0 + 0.5);
                    float blockT = FFHash3(int2((int)floor(q.x / 150.0), (int)floor(hp / 1500.0)), 55u) - 0.5;
                    tone *= 1.0 + wFar * wM * (0.10 * cos(6.2831853 * (uM - 0.5)) + 0.22 * blockT);
                    tone *= 1.0 - 0.6 * wFar * BwFilteredLine(jM, 1.5, fq.x) * saturate((150.0 / max(fq.x, 1e-3) - 4.0) / 4.0);
                    tone *= 1.0 + 0.2 * wFar * (2.0 * macro - 1.0);    // the macro tone 0.10 -> 0.30 in the far view
                }
                float rowUp = (liftI + 1.0) * FF_GALLERY_DH;
                float below = rowUp + 0.8 - hp;                                   // metres under the row above
                float drainI = round(fpos.z / FF_GALLERY_DZ);
                float dzD = fpos.z - FF_GALLERY_DZ * drainI;
                int2 dId = int2((int)drainI, (int)liftI);
                float sLen = 40.0 + 120.0 * FFHash3(dId, 53u);
                float sW = 3.0 + 5.0 * FFHash3(dId, 54u);
                float seep = FFBand(dzD, sW, fq.x) * exp(-below / sLen) * smoothstep(0.0, 8.0, below) * (rowUp < BW_CREST_OFFSET - 100.0 ? 1.0 : 0.0)
                           * (0.6 + 0.4 * FFFbm(float2(fpos.z / 7.0, hp / 30.0), max(fq.x / 7.0, fq.y / 30.0), 2));
                tone *= 1.0 - 0.32 * seep;
                float3 alb = PAL_ConcreteDark * (tone + pour) * (1.0 - 0.45 * gap);
                // inspection galleries (3.11): a 1.6 m window slot every 250 m of altitude (23 rows), lamps every 60 m along it
                float hRow = round(hp / FF_GALLERY_DH) * FF_GALLERY_DH;
                float rowOn = (hRow > 1.0 && hRow < BW_CREST_OFFSET - 100.0) ? 1.0 : 0.0;
                float dyRow = hp - hRow - 0.8;
                float slot = BwFilteredLine(dyRow, 1.6, fq.y) * rowOn;
                alb *= 1.0 - 0.6 * slot;
                float cellL = floor(fpos.z / FF_GALLERY_DZ);
                float dzL = fpos.z - FF_GALLERY_DZ * (cellL + 0.5);
                // a fifth of the bays dark; in the far ending view (fix wave S2-1 d) clusters of 1.2 km x 3 rows from almost all lit
                // to mostly dark, and each lamp its own brightness (0.3-1.0): no identical dashes at every altitude
                int rowI = (int)(hRow / FF_GALLERY_DH);
                float thr = lerp(0.22, lerp(0.08, 0.7, FFHash3(int2((int)floor(fpos.z / 1200.0), (int)floor(hRow / (3.0 * FF_GALLERY_DH))), 44u)), wFar);
                float lampOn = step(thr, FFHash3(int2((int)cellL, rowI), 41u));
                float lampB = lerp(1.0, 0.3 + 0.7 * FFHash3(int2((int)cellL, rowI), 45u), wFar);
                float lampG = FFGlow(dzL, dyRow, fq.x, fq.y, 2.2) * rowOn * lampOn * lampB * _BwFar0.x;
                // the apron floodlights along the foot of the wall: a glow every 40 m at 10 m (the string of lights under 6 km)
                float dzB = fpos.z - 40.0 * (floor(fpos.z / 40.0) + 0.5);
                lampG += FFGlow(dzB, hp - 10.0, fq.x, fq.y, 1.4) * _BwFar0.x;
                // the elevator's guide rail as a lit ruler of the 6 km (fix wave S2-1 e; far ending view only, so nothing passes the
                // cabin at 110 m/s): a SodiumWarm guide light every 100 m from the foot to the ledge on the rail line (z of the
                // walker's spot in the cabin), energy-preserving glows of at least 1.8 px
                if (wFar > 0.0)
                {
                    float dyG = hp - 100.0 * (floor(hp / 100.0) + 0.5);
                    lampG += FFGlow(fpos.z - FF_RAIL_Z, dyG, fq.x, fq.y, 0.8) * (5.0 * wFar * step(hp, BW_CREST_OFFSET - 100.0));
                }
                float occ = 0.9 * (1.0 - 0.25 * gap);
                float3 V = -dir;
                float3 L = _BwSunDir.xyz;
                float NoV = saturate(dot(N, V)), NoL = saturate(dot(N, L));
                float seF = dot(_BwSunDir.xyz, up);
                float3 lit = BwEarthLitN(hp, seF) * BwAlpenTint(hp, seF);   // the 2.4 km penumbra, orange at the crest, red below
                // the twilight arch (fix wave S2-1 b): under the direct-lit band the face still sees the sunlit upper atmosphere
                // to the west; that light fades with the depth below the shadow line over 2.4 km (amber at the line, rose, then
                // violet), so the top 15-20 % of the wall goes amber -> rose -> violet instead of one hard lit strip
                float hSh = BwShadowAltitude(up);
                float wTw = wFar * smoothstep(hSh - 2400.0, hSh + 200.0, hp);
                float3 twTint = lerp(lerp(PAL_DuskViolet / max(BwLumaA(PAL_DuskViolet), 1e-4), PAL_DuskRose / max(BwLumaA(PAL_DuskRose), 1e-4), saturate(2.0 * wTw)),
                                     PAL_DuskAmber / max(BwLumaA(PAL_DuskAmber), 1e-4), saturate(2.0 * wTw - 1.0));
                float3 sun = _BwSunColor.rgb * lit * (BwSunBreakAt(fpos.xz, hp) * NoL);
                const float rough = 0.49;
                float3 amb = _BwAmbExtSky.rgb * saturate(N.y) + _BwAmbExtMid.rgb * (1.0 - abs(N.y)) + _BwAmbExtGround.rgb * saturate(-N.y);
                // the afterglow (Wave X ending): the lee face looks WEST into the twilight, so its sky light is the bright western
                // half of the dome, not the trilight's azimuth-mean side term. E/pi = 0.5 x (cosine-weighted mean of 5 sky taps
                // round N: el 8 / 30 / 60 deg on N, el 15 deg at +-50 deg azimuth) x k + 0.5 x the ground term, with k the
                // trilight's own calibration (luma of _BwAmbExtSky over the mean of 5 upper-dome taps), so the scale stays the
                // schedule's. Ending only (_BwEndRide.z) and faded out within 900 m of the crest, where beat 7's near face meshes
                // (trilight-lit) meet the far face: no seam at the ledge; the alpenglow key dominates there anyway.
                float wAG = saturate(_BwEndRide.z) * smoothstep(300.0, 900.0, BW_CREST_OFFSET - hp);
                if (wAG > 0.0)
                {
                    float3 tz = float3(0.0, 0.0, 1.0), U = float3(0.0, 1.0, 0.0);   // pal:ok (directions)
                    float3 sF = 0.98 * BwSkyColor(N * 0.990 + U * 0.139) + 0.87 * BwSkyColor(N * 0.866 + U * 0.5)
                              + 0.5 * BwSkyColor(N * 0.5 + U * 0.866)
                              + 0.62 * (BwSkyColor(N * 0.621 + tz * 0.740 + U * 0.259) + BwSkyColor(N * 0.621 - tz * 0.740 + U * 0.259));
                    sF /= 0.98 + 0.87 + 0.5 + 1.24;
                    float3 sU = BwSkyColor(U) + BwSkyColor(float3(0.643, 0.766, 0.0)) + BwSkyColor(float3(-0.643, 0.766, 0.0))   // pal:ok
                              + BwSkyColor(float3(0.0, 0.766, 0.643)) + BwSkyColor(float3(0.0, 0.766, -0.643));                      // pal:ok
                    float k = BwLumaA(_BwAmbExtSky.rgb) / max(BwLumaA(sU * 0.2), 1e-5);
                    float3 ambAG = 0.5 * k * sF + 0.5 * _BwAmbExtGround.rgb;
                    amb = lerp(amb, max(amb, ambAG), wAG);
                }
                amb += BwLumaA(amb) * twTint * (2.0 * wTw * sqrt(wTw));
                float3 R = reflect(dir, N);
                float3 d = normalize(lerp(N, R, 0.614));
                float3 ambD = _BwAmbExtSky.rgb * saturate(d.y) + _BwAmbExtMid.rgb * (1.0 - abs(d.y)) + _BwAmbExtGround.rgb * saturate(-d.y);
                float3 hd = float3(d.x, max(d.y, 0.0), d.z);                               // pal:ok (direction)
                float3 skyD = lerp(BwSkyColor(dot(hd, hd) > 1e-6 ? hd : float3(0.0, 0.0, 1.0)), _BwAmbExtGround.rgb, smoothstep(0.0, 0.35, -d.y));   // pal:ok
                float3 env = lerp(skyD, ambD, 0.84);
                float fres = pow(1.0 - NoV, 4.0);
                float envK = (1.0 / (rough * rough + 1.0)) * lerp(0.04, 0.34, fres);
                // masked at grazing view (fix wave S3-12: looking straight up the face from its foot, KS26a, the lit crest band
                // glinted as one NaN-like pixel at (919, 161))
                float3 spec = min(0.04 * FFGgx(N, L, V, rough) * sun, 16.0) * smoothstep(0.0, 0.15, NoV);
                return alb * 0.96 * (amb * occ + sun) + spec + env * envK * occ + PAL_SodiumWarm * (3.0 * lampG);
            }

            // ---------------------------------------------------------------- the shuttle guideway (kind 6)
            // A concrete beam 4 m wide, 1.1 m tall, on the lowland from the base station to the platform; the magnet rails are
            // two dark bands along x (constant along the motion: nothing strobes under the shuttle at 152 m/s), joints every
            // 25 m and CityCool marker lights every 50 m on both top edges are filtered by the footprint plus the motion filter.
            float3 FFGuideway(float3 wp, float3 n, float fp, float hp, float3 up)
            {
                float fx = max(fp, _BwEndRide.x);
                float3 alb = PAL_ConcreteMid * 0.82;
                float3 e = 0.0;
                if (n.y > 0.5)
                {
                    float rails = saturate(BwFilteredLine(wp.z - 1496.4, 0.32, fp) + BwFilteredLine(wp.z - 1498.4, 0.32, fp));
                    alb = lerp(alb, PAL_SteelPaint * 0.35, rails);
                    float jx = wp.x - FF_GW_JOINT * floor(wp.x / FF_GW_JOINT + 0.5);
                    alb *= 1.0 - 0.35 * BwFilteredLine(jx, 0.05, fx);
                    float mx = wp.x - FF_GW_MARK * floor(wp.x / FF_GW_MARK + 0.5);
                    float g = FFGlow(mx, wp.z - (FF_GW_Z0 + 0.15), fx, fp, 0.12) + FFGlow(mx, wp.z - (FF_GW_Z1 - 0.15), fx, fp, 0.12);
                    e = PAL_CityCool * (4.0 * g * _BwFar0.x);
                }
                else alb *= 0.8;   // the sides: rougher, darker
                return alb * FFLight(n, hp, wp.xz, up) + e;
            }

            // ---------------------------------------------------------------- the lamp roads of the lowland foot (Wave X ending)
            // From the lantern platform (2.7 m eye height) the whole lowland between 1 km and the foot is a 1-2 px sliver: the
            // land reads only through its lights. Three roads carry sodium street lamps (heads 8 m up, so they stand ABOVE the
            // horizon from the platform and are drawn here as 3D glows, not on the ground): the platform road beside the
            // guideway (z 1452, from the plaza to the foot, every 60 m: a converging string of lights 3.4 km long that measures
            // the distance to the wall), the foot road under the wall (x -320, every 50 m) and the town road to the north-east
            // (from (-3300, 1900) to (-600, 3900), every 70 m). The ray is solved against each string's line (closest approach),
            // then the 3 nearest heads are drawn as energy-preserving glows (radius 0.45 m, never under 1.35 px) dimmed by the
            // haze to the lamp; a head behind the hit (the wall, the plaza) is skipped. Their light pools on the ground are in
            // FFRoadPools (the ground branch, anisotropic footprints: a pool at grazing is a thin warm line).
            #define FF_RD_H 8.0
            #define FF_RD_N 3
            float3 FFRoadP0(int r) { return r == 0 ? float3(-3200.0, -6000.0 + FF_RD_H, 1452.0) : (r == 1 ? float3(-320.0, -6000.0 + FF_RD_H, -6000.0) : float3(-3300.0, -6000.0 + FF_RD_H, 1900.0)); }   // pal:ok
            float3 FFRoadP1(int r) { return r == 0 ? float3(-330.0, -6000.0 + FF_RD_H, 1452.0) : (r == 1 ? float3(-320.0, -6000.0 + FF_RD_H, 9000.0) : float3(-600.0, -6000.0 + FF_RD_H, 3900.0)); }     // pal:ok
            float FFRoadPeriod(int r) { return r == 0 ? 60.0 : (r == 1 ? 50.0 : 70.0); }

            float3 FFLampHeads(float3 dir, float tMax, float pix)
            {
                float3 acc = 0.0;
                if (_BwFar0.x <= 0.0) return acc;   // lit with the city lights N(s)
                [unroll] for (int r = 0; r < FF_RD_N; r++)
                {
                    float3 a = FFRoadP0(r) - _BwCam.xyz;
                    float3 u = FFRoadP1(r) - FFRoadP0(r);
                    float len = length(u);
                    u /= len;
                    float per = FFRoadPeriod(r);
                    float b = dot(dir, u), d = dot(dir, a), e = dot(u, a);
                    float t = (d * b - e) / max(1.0 - b * b, 1e-5);
                    float kc = clamp(round(t / per), 0.0, floor(len / per));
                    [unroll] for (int k = -1; k <= 1; k++)
                    {
                        float kk = kc + k;
                        if (kk < 0.0 || kk * per > len) continue;
                        float3 L = a + u * (kk * per);
                        float cd = length(L);
                        if (cd < 25.0 || cd > tMax + 30.0) continue;
                        float ca = sqrt(max(2.0 * (1.0 - dot(dir, L / cd)), 0.0));
                        float r0 = 0.45 / cd, ra = max(r0, 1.2 * FF_MIN_PX * pix);   // >= 1.8 px (fix wave S3-12)
                        float on = step(0.08, FFHash3(int2((int)kk, r), 61u));   // one lamp in twelve is out
                        acc += PAL_SodiumWarm * (9.0 * on * smoothstep(25.0, 60.0, cd) * exp(-(ca * ca) / (ra * ra)) * (r0 * r0) / (ra * ra)
                               * BwDiscTransmittance(exp(-_BwFog0.x * cd)));
                    }
                }
                return acc * _BwFar0.x;
            }

            // The pools of the same lamps on the ground at g (world xz near the foot): E = I h / d^3 from a head 8 m up, I chosen so
            // the pool under a lamp is ~6x the dusk ambient on the meadow; summed over the 3 nearest heads of each road; the
            // footprints along / across the view widen each pool so its energy is kept (no shimmer at grazing views).
            float3 FFRoadPools(float2 g, float fpAcross, float fpAlong, float2 va)
            {
                float3 acc = 0.0;
                if (_BwFar0.x <= 0.0) return acc;
                float2 vc = float2(-va.y, va.x);
                [unroll] for (int r = 0; r < FF_RD_N; r++)
                {
                    float2 p0 = FFRoadP0(r).xz, p1 = FFRoadP1(r).xz;
                    float2 u = p1 - p0;
                    float len = length(u);
                    u /= len;
                    float per = FFRoadPeriod(r);
                    float t = dot(g - p0, u);
                    if (t < -60.0 || t > len + 60.0) continue;
                    float kc = clamp(round(t / per), 0.0, floor(len / per));
                    [unroll] for (int k = -1; k <= 1; k++)
                    {
                        float kk = kc + k;
                        if (kk < 0.0 || kk * per > len) continue;
                        float2 dl = g - (p0 + u * (kk * per));
                        float sig = 7.0;
                        float sa = sqrt(sig * sig + 0.25 * fpAlong * fpAlong), sx = sqrt(sig * sig + 0.25 * fpAcross * fpAcross);
                        float da = dot(dl, va) / sa, dx = dot(dl, vc) / sx;
                        float on = step(0.08, FFHash3(int2((int)kk, r), 61u));
                        acc += PAL_SodiumWarm * (on * exp(-0.5 * (da * da + dx * dx)) * (sig * sig / (sa * sx)));
                    }
                }
                return acc * (0.8 * _BwFar0.x);
            }

            // ---------------------------------------------------------------- fragment
            half4 FarFrag(FarVaryings i) : SV_Target
            {
                float3 dir = normalize(i.dirWS);
                float3 eye = _BwCam.xyz;
                float pix = max(_BwCam.w, 1e-6);
                float h0 = _BwFarCam.x;
                float rEye = BW_EARTH_R + h0;
                float dy = dir.y;
                float3 C = float3(0.0, -rEye, 0.0);   // pal:ok (not a colour)

                // the eye inside the dam body (the sluice tunnel, east of the lee face and below the crest): the face plane is
                // seen only through openings, which the real geometry frames, so westward rays through the tunnel mouth go on
                // to the lowland and the sky instead of hitting the analytic face (the 46-level step at s 1422.85, Wave A)
                bool eyeInDam = eye.x > FF_LEE_X && h0 < BW_CREST_OFFSET;
                // face position and its screen derivatives in uniform control flow (formwork filtering of kind 3)
                float tFu = abs(dir.x) > 1e-6 ? max((FF_LEE_X - eye.x) / dir.x, 0.0) : 0.0;
                float3 fposF = eye + dir * tFu;
                float3 dF = abs(ddx(fposF)) + abs(ddy(fposF));

                // ---- analytic hits
                float tHit = 1e30;
                int kind = 0;   // 0 sky, 1 lowland, 2 sea, 3 lee face, 4 crest strip, 5 sea face
                float tL = BwRaySphereNear(rEye, _BwFarCam.y, dy);
                if (tL > 0.0 && eye.x + dir.x * tL < FF_LEE_X) { tHit = tL; kind = 1; }
                float tS = BwRaySphereNear(rEye, _BwFarCam.z, dy);
                if (tS > 0.0 && tS < tHit && eye.x + dir.x * tS > FF_SEA_X) { tHit = tS; kind = 2; }
                float tC = BwRaySphereNear(rEye, _BwFarCam.w, dy);
                float xC = eye.x + dir.x * tC;
                if (tC > 0.0 && tC < tHit && abs(xC) <= FF_SEA_X) { tHit = tC; kind = 4; }
                float hF = -1.0, tF = -1.0;
                if (abs(dir.x) > 1e-6)
                {
                    tF = (FF_LEE_X - eye.x) / dir.x;
                    if (tF > 0.0)
                    {
                        hF = BwAltAt(h0, dy, tF);
                        if (!eyeInDam && tF < tHit && hF > 0.0 && hF < BW_CREST_OFFSET) { tHit = tF; kind = 3; }
                    }
                    float tE = (FF_SEA_X - eye.x) / dir.x;
                    if (tE > 0.0 && tE < tHit)
                    {
                        float hE = BwAltAt(h0, dy, tE);
                        if (hE > BW_SEA_OFFSET && hE < BW_CREST_OFFSET) { tHit = tE; kind = 5; }
                    }
                }
                // the shuttle guideway: slab test of the beam box, camera-relative (eye within 4 km of it: no precision issue)
                float3 gwN = float3(0.0, 1.0, 0.0);   // pal:ok (not a colour)
                {
                    float3 b0 = float3(FF_GW_X0, FF_GW_Y0, FF_GW_Z0) - eye, b1 = float3(FF_GW_X1, FF_GW_Y1, FF_GW_Z1) - eye;   // pal:ok (box corners)
                    float3 sd = float3(abs(dir.x) > 1e-7 ? dir.x : 1e-7, abs(dir.y) > 1e-7 ? dir.y : 1e-7, abs(dir.z) > 1e-7 ? dir.z : 1e-7);   // pal:ok
                    float3 t0 = b0 / sd, t1 = b1 / sd;
                    float3 tmn = min(t0, t1), tmx = max(t0, t1);
                    float tN = max(max(tmn.x, tmn.y), tmn.z), tX = min(min(tmx.x, tmx.y), tmx.z);
                    if (tN > 0.0 && tX > tN && tN < tHit)
                    {
                        tHit = tN; kind = 6;
                        gwN = tmn.y >= tmn.x && tmn.y >= tmn.z ? float3(0.0, -sign(dir.y), 0.0)   // pal:ok (normal)
                            : (tmn.x >= tmn.z ? float3(-sign(dir.x), 0.0, 0.0) : float3(0.0, 0.0, -sign(dir.z)));   // pal:ok
                    }
                }

                float3 col;
                float4 mistK = 0.0;
                float mistT = 0.0;
                if (kind == 0)
                {
                    // ---- sky: raw sky + cloud deck, through the whole atmosphere, + the disc
                    float3 raw = BwSkyColor(dir);
                    float ca;
                    raw = FFClouds(dir, raw, ca);
                    float T; float3 L;
                    BwAtmoRay(dir, BwAtmoExit(dir), BwFogColorRaw(dir), T, L);
                    col = raw * T + BwFogDesat(L, T);
                    // the disc is clipped by the true horizon (the lowland sphere above) only: its transmittance is floored at 0.15
                    // so the grazing haze path of a sun on the horizon (KS23 at 4389 m: tau > 5) reddens it but never erases it
                    // (fix wave S2-3: KS23 had no disc)
                    col += BwSunDisc(dir, pix) * BwDiscTransmittance(max(T, 0.15)) * (1.0 - ca);
                    col = BwDustApply(col, eye, eye + dir * 1000.0);
                }
                else
                {
                    float3 p = dir * tHit;                 // camera-relative hit
                    float3 q = p - C;
                    float3 up = normalize(q);
                    // altitude of the hit: the stable form (length(q) - R would be quantised to 0.5 m at R = 6.4e6)
                    float hp = kind == 1 ? 0.0 : (kind == 3 ? hF : BwAltAt(h0, dy, tHit));
                    float3 surf;
                    if (kind == 1)
                    {
                        // arc-length ground coordinates along the heading from the nadir (14.7 H4)
                        float hz = length(q.xz);
                        float phi = atan2(hz, q.y);
                        float2 g = eye.xz + (hz > 1e-3 ? q.xz / hz : float2(0.0, 0.0)) * (BW_EARTH_R * phi);
                        float mu = max(-dot(dir, up), 0.02);
                        float fp = tHit * pix / mu * _FarDetail;
                        // the ride's motion filter on the ground: the shuttle's run (along x) and the descent's last metres
                        // (a point at horizontal distance dh moves dh / h0 x the vertical step)
                        float dh = tHit * length(dir.xz);
                        fp = max(fp, max(_BwEndRide.x, _BwEndRide.y * dh / max(h0, 1.0)));
                        float3 alb = FFLowlandAlbedo(g, fp);
                        // near ground (the platform at 2.7 m eye height): meadow tone and tussocks 6 m down to 9 cm, each
                        // octave faded by the footprint (unresolved octaves are their mean: the far look is unchanged)
                        // (Wave X: the 7-octave sum has a std of ~0.07, so the meadow read flat (+-1 level at KS27); its contrast is
                        // expanded x2.2 about the mean 0.5, which keeps the mean, hence the far look, unchanged)
                        alb *= 0.7 + 0.6 * saturate((FFFbm(g / 6.0 + 31.7, fp / 6.0, 7) - 0.5) * 2.2 + 0.5);
                        float3 light = FFLight(up, 0.0, g, up);
                        // the lanterns of the platform light the meadow round it (point lights, E = I cos / d^2, 1 m soft core)
                        float3 gp = eye + p;
                        [unroll] for (int li = 0; li < 10; li++)
                        {
                            float4 lp = _BwEndLamps[li];
                            float3 lv = lp.xyz - gp;
                            float d2 = dot(lv, lv);
                            light += PAL_Lantern * (lp.w * saturate(dot(up, lv * rsqrt(max(d2, 1e-4)))) / (d2 + 1.0));
                        }
                        // the street lamps of the foot roads (FFRoadPools)
                        light += FFRoadPools(g, tHit * pix * _FarDetail, fp, normalize(dir.xz + 1e-6));
                        surf = alb * light;
                        // rivers: box-filtered coverage, water = Fresnel sky reflection over dark water
                        float wcov = 0.0;
                        [unroll] for (int k = 0; k < 3; k++)
                        {
                            float x01;
                            float dd = FFRiverDist(k, g, x01);
                            wcov = max(wcov, FFBand(dd, lerp(80.0, 400.0, x01), fp));
                        }
                        float F = 0.02 + 0.98 * pow(1.0 - mu, 5.0);
                        float3 water = lerp(PAL_SeaStorm * 0.12 * light, BwSkyColor(reflect(dir, up)), F);
                        surf = lerp(surf, water, wcov);
                        surf += FFCity(g, tHit * pix * _FarDetail, fp, dir.xz);
                        // mist rivers between the eye and the ground: composited AFTER the haze (fix wave S2-2): the bank gets the
                        // haze of the eye-to-entry path only, the ground behind it the whole path (below, after BwFogApplyFar)
                        mistK = FFMist(dir, tHit, mistT);
                    }
                    else if (kind == 2)
                    {
                        surf = FFSea(dir, up);
                    }
                    else if (kind == 3)
                    {
                        // lee face: the near meshes' BwConcrete look (FFFaceShade)
                        float3 n = float3(1.0, 0.0, 0.0) * (eye.x > FF_LEE_X ? 1.0 : -1.0);   // pal:ok (not a colour)
                        surf = FFFaceShade(eye + p, dir, n, hp, up, tHit * pix, dF);
                    }
                    else if (kind == 4)
                    {
                        surf = PAL_ConcreteMid * FFLight(up, hp, (eye + p).xz, up);
                    }
                    else if (kind == 6)
                    {
                        float muG = max(abs(dot(dir, gwN)), 0.05);
                        surf = FFGuideway(eye + p, gwN, tHit * pix / muG * _FarDetail, max(hp, 0.0), up);
                    }
                    else
                    {
                        surf = PAL_ConcreteWet * FFLight(float3(1.0, 0.0, 0.0), hp, (eye + p).xz, up);   // pal:ok (not a colour)
                    }
                    col = BwFogApplyFar(surf, dir, tHit);
                    if (mistK.a > 0.0)
                        col = lerp(col, BwFogApplyFar(mistK.rgb / max(mistK.a, 1e-4), dir, lerp(mistT, tHit, 0.3)), mistK.a);   // 0.3: the haze behind a translucent bank
                    // the valley haze of the ending (fix wave S2-1 a): at the foot (eye below ~800 m) the lowest air is denser
                    // (an extra layer, scale height 700 m, 3.2e-4 /m at the ground, in the earth's shadow: its in-scatter is the
                    // shadowed haze), integrated in closed form along the same ray for the face, the lowland and the guideway, so
                    // the wall's foot and the ground at the same range get the same in-scatter, the lower 1.5 km of the wall
                    // sits closer to the haze than mid-wall (T 0.33 / 0.61 / 0.71 / 0.77 at the foot / 1.5 km / 3 km / crest from
                    // the platform 3.48 km away) and the far lowland fades into it instead of a black floor under a backdrop
                    // the lowland toward the visible horizon fades into the sky's own horizon colour (fix wave S3-9: a hard dark
                    // rim of ~55 levels within 20-60 px under the horizon from the crest, the ledge and the descent): beyond 12 % of
                    // the horizon distance (33 km from the crest) the ground blends to BwSkyVisible just above the horizon at the
                    // same azimuth (90 % at 70 %), so sky and ground meet in one colour; not from the foot (eye below 300 m)
                    if (kind == 1 && _BwFarCam.y > 0.0)
                    {
                        float tHz = rEye * sqrt(_BwFarCam.y);
                        float wHz = 0.9 * smoothstep(0.12, 0.7, tHit / max(tHz, 1.0)) * smoothstep(300.0, 1500.0, h0);
                        if (wHz > 0.0)
                        {
                            float dip = BwVisibleDip();
                            float2 hz2 = dir.xz * rsqrt(max(dot(dir.xz, dir.xz), 1e-12));
                            float3 dH = float3(hz2.x * cos(dip), -sin(dip) + 4e-4, hz2.y * cos(dip));   // pal:ok (direction)
                            col = lerp(col, BwFogApplyFar(BwSkyColor(dH), dH, BwAtmoExit(dH)), wHz);
                        }
                    }
                    float wVH = saturate(_BwEndRide.z) * (1.0 - smoothstep(150.0, 800.0, h0));
                    if (wVH > 0.0 && kind != 2)
                    {
                        float hEnd = kind == 1 ? 0.0 : max(hp, 0.0);
                        float xV = (max(h0, 0.0) - hEnd) / FF_VH_H;
                        float gV = abs(xV) < 1e-3 ? 1.0 + 0.5 * xV : (exp(xV) - 1.0) / xV;
                        float TV = exp(-FF_VH_S * wVH * exp(-max(h0, 0.0) / FF_VH_H) * tHit * gV);
                        col = col * TV + BwFogDesat(_BwFogShadow.rgb * FF_VH_J, TV) * (1.0 - TV);
                    }
                }

                // ---- the lee-face top edge against what lies beyond (analytic coverage, no stair steps)
                if (kind == 3 && hF > BW_CREST_OFFSET - 4.0 * tF * pix)
                {
                    float covF = saturate((BW_CREST_OFFSET - hF) / max(tF * pix, 1e-3) + 0.5);
                    float3 beyond = BwSkyVisible(dir);
                    col = lerp(beyond, col, covF);
                }

                // ---- the lit plaza of the lantern platform from afar (the descent, the shuttle's start): one energy-preserving
                // glow (radius 16 m of radiance ~0.25, never under 1.35 px) where the lantern meshes are switched off (> 300 m)
                if (_BwEndGlow.w > 0.0)
                {
                    float3 gv = _BwEndGlow.xyz - eye;
                    float gd = length(gv);
                    float gfar = smoothstep(250.0, 400.0, gd);
                    if (gfar > 0.0 && gd < tHit + 50.0)
                    {
                        float ga = sqrt(max(2.0 * (1.0 - dot(dir, gv / gd)), 0.0));
                        float r0 = _BwEndGlow.w / gd, ra = max(r0, 1.2 * FF_MIN_PX * pix);
                        col += PAL_Lantern * (0.25 * gfar * exp(-(ga * ga) / (ra * ra)) * (r0 * r0) / (ra * ra) * BwDiscTransmittance(exp(-_BwFog0.x * gd)));
                    }
                }

                // ---- the returning car: its lit interior (3 m, Tungsten x6) climbing the rail line, a glow of at least 1.35 px
                if (_BwEndCar.w > 0.0)
                {
                    float3 cv = _BwEndCar.xyz - eye;
                    float cd = length(cv);
                    if (cd > 30.0 && cd < tHit + 20.0)
                    {
                        float ca2 = sqrt(max(2.0 * (1.0 - dot(dir, cv / cd)), 0.0));
                        float r0 = 3.0 / cd, ra = max(r0, 1.6 * FF_MIN_PX * pix);   // a 3-4 px glow from the platform (fix wave S2-1 e)
                        float hC = _BwFog1.w + _BwEndCar.y;
                        col += PAL_Tungsten * (6.0 * smoothstep(30.0, 60.0, cd) * exp(-(ca2 * ca2) / (ra * ra)) * (r0 * r0) / (ra * ra)
                                * BwDiscTransmittance(exp(-_BwFog0.x * cd * exp(-0.5 * (max(h0, 0.0) + max(hC, 0.0)) / max(_BwFog0.y, 1.0)))));   // haze at the mid altitude
                    }
                }

                // ---- the street-lamp heads of the foot roads (3D glows above the horizon from the platform)
                col += FFLampHeads(dir, tHit, pix);

                // ---- the jets' falling veils, then the outlet jets in front of everything beyond them
                float4 veils = FFJetVeils(dir, tHit);
                col = lerp(col, veils.rgb, veils.a);
                float4 jets = FFJets(dir, tHit);
                // from the shuttle and the platform the arcs (thrown 700 m out from outlets 90 m under the crest) project above
                // the crest silhouette as orange hooks detached from their veils (fix wave S2-1 f): there they are cut at the
                // crest line (the face's own top-edge coverage), so each jet hangs from the wall into its veil
                float wJ = saturate(_BwEndRide.z) * smoothstep(-700.0, -1800.0, _BwCam.x);
                if (wJ > 0.0 && tF > 0.0)
                    jets.a *= lerp(1.0, saturate((BW_CREST_OFFSET - 60.0 - hF) / max(tF * pix, 1e-3) + 0.5), wJ);
                col = lerp(col, jets.rgb, jets.a);

                col = BwOpeningClamp(col);                         // 14.7 H8: exterior seen from inside an opening <= 12
                col = AnyIsNaN(col) ? float3(0.0, 0.0, 0.0) : col;   // pal:ok (NaN guard)
                return half4(min(col, 1000.0), 1.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
