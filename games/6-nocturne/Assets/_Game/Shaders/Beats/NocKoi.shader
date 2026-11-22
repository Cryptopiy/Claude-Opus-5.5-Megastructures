// NocKoi - the 900 m hologram koi of beat 7 (MASTER_PROMPT 14.2.5 NocKoi, 14.5.7, 14.7 H6, D-009; Look v2: LOOK_V2 2.3, 6
// "Climaxes", 8.1 KS17 / KS18, TEXTURE_PLAN 6, CRIT0 #9). An additive, translucent hologram of a real Kohaku koi: a body tube with
// a head (smooth skin, gill cover, two eye spots, four barbels), imbricate scales, vermilion Kohaku patches, two pectoral fins,
// a dorsal fin and the split tail veil (D-009: 120 x 120 m), slow scanlines and a caustic-like inner variation.
// Geometry: the mesh lives in parameter space (KoiBuilder): uv0 = (a = arc length behind the head (m), theta = ring angle),
// uv1 = (radial offset rho, forward offset (caps, fins, barbels), wave envelope env(a), d env / da), uv2 = (0 frame part / 1 veil,
// scale row sv (body) | fin u | veil k | barbel t, fin v | veil half-span, part id: 0 body and caps, 2 / 3 outer / inner pectoral,
// 4 dorsal, 5 barbel). Every frame part (body, caps, fins, barbels) is a rigid point (forward, radial cos theta, radial sin theta)
// in the ring frame of its station a, so one vertex path places all of it. The vertex shader evaluates the spine from two
// uniforms, head azimuth phi_h (rad, wrapped to [0, 2 pi)) and the wave phase frac(t_w / 20), and the reference-spine attributes
// of the station (D-032 2, HT: KoiMath's 900 m plane curve, uv3 = (r_ref, alpha, dr_ref/da, dalpha/da), uv4 = (Dy, dDy/da)):
//   phi = phi_h - alpha(a), r = r_ref(a) + (R(phi) - 260) m(a) + w, m = 1 - smoothstep(0, 110, a), R = 260 + 18 sin(2 phi + 0.7)
//   C(a) = S + r (cos phi, 0, sin phi) + (y(phi) + Dy(a)) Y, y = 300 + 60 sin(phi + 0.4), w = env(a) sin(2 pi (a / 810 - t_w / 20));
//   F = -dC/da (analytic), N = normalize(Y x F), B = F x N (D-009: rings follow the waved spine); the veil spans
//   cos(30 deg) Y + sin(30 deg) N. C# mirror of the vertex and fragment: Beats/Beat07_Heart/KoiShaderMath.cs
// (KoiShaderTests, Beat07HeartTests).
// Look (LOOK_V2 6 ladder, displayed codes): rim KoiGlow x 1.6 (1 - NdotV)^2.2 brighter than the core (210-235); body
// KoiBody x 0.55 x (0.55 + 0.45 vein), vein mean 0.5, both shells (front 1, back 0.35) about 130-175; fins x 0.25 (70-110); veil
// x 0.12 (45-75, the deck shows through between the rays); eye spots <= 0.5 x body; patches KoiPatch on both shells from (a, |theta
// from the top|), 0 on the head (a < 0.12 L), saturation x (1 - 0.75 smoothstep(1 440, 1 490, s)) (KoiDriver). Every pattern is
// filtered by its pixel footprint (fwidth, taken before any branch) and fades to its mean below 2.5-5 px of pitch. Budgets by
// the brightest channel, hue kept: body front 1.8 (the 1.5-2.0 clamp of TEXTURE_PLAN 6), back 0.8, fins 0.6, veil 0.35; radiance
// clamp 3.0; fogged additively as col T (1 - cloudMix) and never floored (D-009). Queue Transparent+30 (3030), Blend One One,
// ZWrite Off, ZTest LEqual, Cull Off. No texture, no sampler, no keyword.
// Fix wave p4_01 C02 (HT, a hologram, not an inflatable; LOOK_V2 reference 4): (1) transmission - the front shell's body term
// thins toward the centre of the silhouette (x 1 - _CoreClear n.v^2; the Fresnel rim keeps its 210-235) and a first pass
// "Ghost" (LightMode SRPDefaultUnlit, which URP draws before UniversalForward for the same renderer; Blend DstColor One:
// dst x (1 + k)) lifts what lies behind the body centre by k = _GhostGain n.v^2 T (1 - cloudMix) (front body shell only, 0 on the
// fins, veil and barbels, 0 at the rim), so the towers' windows and the spire behind ghost through at about 10-20 % contrast;
// (2) structure - interference gaps across the body every KOI_GAPP m along the spine (lines of half-width KOI_GAPW of the pitch,
// -_GapDepth x (line - its mean 2 KOI_GAPW): the body keeps its mean; faded below 6 px of pitch) and a half-cell feather on the
// Kohaku patch edges (the scalloped scale-centre mask is mixed 50 / 50 with the smooth mask).
Shader "Megastructure/NocKoi"
{
    Properties
    {
        _KoiParams ("phi_h (rad, wrapped), frac(t_w / 20), patch saturation, scanline phase", Vector) = (0.1159, 0, 1, 0)
        _KoiTime ("flow phase frac(t_w / 8), caustic phase frac(t_w / 240), 0, 0", Vector) = (0, 0, 0, 0)
        _BodyGain ("Body gain (x KoiBody, LOOK_V2 2.3: 0.55)", Float) = 0.55
        _RimGain ("Rim gain (x KoiGlow, LOOK_V2 2.3: 1.6)", Float) = 1.6
        _FrontBudget ("Body front max-channel budget (1.5-2.0)", Float) = 1.8
        _BackBudget ("Body back max-channel budget", Float) = 0.8
        _BackWeight ("Back-shell weight", Float) = 0.35
        _FinGain ("Fin gain (x body)", Float) = 0.25
        _FinBudget ("Fin max-channel budget", Float) = 0.6
        _VeilGain ("Veil gain (x body)", Float) = 0.12
        _VeilBudget ("Veil max-channel budget", Float) = 0.35
        _KoiPupil ("Pupil offsets in the eyes (outer a, arc; inner a, arc; m): DREAD 5.2 hook, 0 = centred", Vector) = (0, 0, 0, 0)
        _CoreClear ("Front-shell body thinning at the silhouette centre (x 1 - k n.v^2), C02", Float) = 0.8
        _GhostGain ("Background lift behind the body centre (dst x (1 + k n.v^2)), C02", Float) = 1.5
        _GapDepth ("Interference gap depth along the spine, C02", Float) = 0.22
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "../Include/NocAtmosphere.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _KoiParams;
        float4 _KoiTime;
        float _BodyGain;
        float _RimGain;
        float _FrontBudget;
        float _BackBudget;
        float _BackWeight;
        float _FinGain;
        float _FinBudget;
        float _VeilGain;
        float _VeilBudget;
        float4 _KoiPupil;
        float _CoreClear;
        float _GhostGain;
        float _GapDepth;
    CBUFFER_END

    // KoiMath constants (Beat07HeartTests reads these lines and compares them with KoiMath)
    #define KOI_SX 1330.0
    #define KOI_SZ 388.0
    #define KOI_R0 260.0
    #define KOI_RA 18.0
    #define KOI_RP 0.7
    #define KOI_Y0 300.0
    #define KOI_YA 60.0
    #define KOI_YP 0.4
    #define KOI_WL 810.0
    #define KOI_L 900.0
    #define KOI_TAU 6.28318530718
    #define KOI_RMODA 110.0      // KoiMath.RModFade: the orbit's ellipticity fades behind the head
    #define KOI_VROLLC 0.866025403784   // cos / sin of KoiMath.VeilRollDeg (30 deg): the veil axis cos Y + sin N
    #define KOI_VROLLS 0.5
    // Look constants (KoiShaderTests reads these lines and compares them with KoiShaderMath)
    #define KOI_NC 18.0          // scale columns around the body
    #define KOI_KH 0.6           // scale row spacing / column width
    #define KOI_SR 0.62          // scale disc radius (column widths); the hex lattice's covering radius is 0.508
    #define KOI_RHOMIN 14.0      // smallest radius the scale size follows (m)
    #define KOI_HEADA 108.0      // patch head mask, 0.12 L (LOOK_V2 6)
    #define KOI_GILLA 84.0       // gill cover edge on the dorsal and ventral line (m); +KOI_GILLB at the side axes
    #define KOI_GILLB 16.0
    #define KOI_EYEA 36.0        // eye station (m), ring angle above the side axis (rad), radius (m)
    #define KOI_EYET 0.40
    #define KOI_EYER 4.2
    #define KOI_VEILA0 870.0     // KoiMath.VeilA0 = L - 30
    #define KOI_VEILL 140.0
    #define KOI_VEILNOTCH 93.3333333333  // KoiMath.VeilNotch: the split edge's notch, 2/3 of the veil length
    #define KOI_SCALEMEAN 0.6245 // mean of the composited scale shade at footprints <= 0.08 columns (KoiShaderTests)
    #define KOI_SMF0 0.07        // the soft composite darkens at wider footprints: mean(fw) = KOI_SCALEMEAN - KOI_SMA x^2 / (x + KOI_SMB),
    #define KOI_SMA 0.0737       // x = max(fw - KOI_SMF0, 0) (fit to 0.0005 over fw 0-0.45, where the scales fade out); the vein
    #define KOI_SMB 0.08         // subtracts mean(fw), so the displayed body keeps its brightness at every distance
    #define KOI_CAUSTMEAN 0.4392 // mean of the caustic ridge network (KoiShaderTests)
    #define KOI_STRB 0.40        // body structure factor KOI_STRB + KOI_STRK vein (mean 0.775 at a vein mean of 0.5, LOOK_V2 6)
    #define KOI_STRK 0.75
    #define KOI_VEINS 1.6        // vein = 0.5 + KOI_VEINS (scales - mean(fw)) + KOI_CAUSTA (caustic - its mean)
    #define KOI_CAUSTA 0.6
    #define KOI_RIMP 3.0         // Fresnel rim exponent (a thin glowing edge, the core keeps its structure)
    #define KOI_TAILA0 820.0     // the caudal peduncle dissolves into the veil (body x 1 - 0.9 f, rim x 1 - 0.85 f)
    #define KOI_TAILA1 900.0
    #define KOI_GAPP 7.0         // C02: interference gap pitch along the spine (m) and line half-width (fraction of the pitch)
    #define KOI_GAPW 0.15

    struct Attributes
    {
        float4 positionOS : POSITION;
        float2 uv0 : TEXCOORD0;
        float4 uv1 : TEXCOORD1;
        float4 uv2 : TEXCOORD2;
        float4 uv3 : TEXCOORD3;      // reference spine of the station: r_ref, alpha, dr_ref/da, dalpha/da (D-032 2)
        float4 uv4 : TEXCOORD4;      // drop Dy, dDy/da, 0, 0
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS : TEXCOORD0;
        float3 normalWS : TEXCOORD1;
        float4 data0 : TEXCOORD2;    // a_eff = a - forward (m), theta (rad), scale row sv, part id
        float4 data1 : TEXCOORD3;    // fin u | veil k | barbel t, fin v, radial offset (m), 0
        float3 tangentWS : TEXCOORD4; // spine forward F of the station (the back shell traces its view ray to the front shell)
    };

    // the waved spine at station a and its derivative along a (D-032 2): sh3 = (r_ref, alpha, dr_ref/da, dalpha/da), sh4 = (Dy, dDy/da)
    void KoiSpine(float a, float4 sh3, float4 sh4, float env, float envD, float phiH, float twFrac, out float3 C, out float3 dC)
    {
        float phi = phiH - sh3.y, dphi = -sh3.w;
        float sp, cp;
        sincos(phi, sp, cp);
        float s2 = sin(2.0 * phi + KOI_RP), c2 = cos(2.0 * phi + KOI_RP);
        float t = saturate(a / KOI_RMODA);
        float m = 1.0 - t * t * (3.0 - 2.0 * t), dm = -6.0 * t * (1.0 - t) / KOI_RMODA;
        float ph = KOI_TAU * (a / KOI_WL - twFrac);
        float sw, cw;
        sincos(ph, sw, cw);
        float w = env * sw;
        float dw = envD * sw + env * cw * (KOI_TAU / KOI_WL);
        float r = sh3.x + KOI_RA * s2 * m + w;
        float dr = sh3.z + 2.0 * KOI_RA * c2 * dphi * m + KOI_RA * s2 * dm + dw;
        float y = KOI_Y0 + KOI_YA * sin(phi + KOI_YP) + sh4.x;
        float dy = KOI_YA * cos(phi + KOI_YP) * dphi + sh4.y;
        C = float3(KOI_SX + r * cp, y, KOI_SZ + r * sp);
        dC = float3(dr * cp - r * sp * dphi, dy, dr * sp + r * cp * dphi);
    }

    Varyings KoiVert(Attributes v)
    {
        Varyings o = (Varyings)0;
        float a = v.uv0.x, theta = v.uv0.y;
        float3 C, dC;
        KoiSpine(a, v.uv3, v.uv4, v.uv1.z, v.uv1.w, _KoiParams.x, _KoiParams.y, C, dC);
        float3 F = -dC * rsqrt(max(dot(dC, dC), 1e-12));
        float3 N = cross(float3(0.0, 1.0, 0.0), F);
        N *= rsqrt(max(dot(N, N), 1e-12));
        float3 B = cross(F, N);
        float st, ct;
        sincos(theta, st, ct);
        float3 ring = ct * N + st * B;
        float3 p, n;
        if (v.uv2.x < 0.5)
        {
            p = C + F * v.uv1.y + ring * v.uv1.x;
            n = F * v.uv1.y + ring * v.uv1.x;
            n *= rsqrt(max(dot(n, n), 1e-12));
        }
        else
        {
            p = C + (KOI_VROLLC * float3(0.0, 1.0, 0.0) + KOI_VROLLS * N) * (v.uv2.y * v.uv2.z);
            n = normalize(KOI_VROLLS * B - KOI_VROLLC * N);
        }
        o.positionWS = p;
        o.positionCS = TransformWorldToHClip(p);
        o.normalWS = n;
        o.data0 = float4(a - v.uv1.y, theta, v.uv2.y, v.uv2.x > 0.5 ? 1.0 : v.uv2.w);
        o.data1 = float4(v.uv2.y, v.uv2.z, v.uv1.x, 0.0);
        o.tangentWS = F;
        return o;
    }

    // ------------------------------------------------------------------ patterns (C# twin: KoiShaderMath)
    uint KoiHash(int2 c)
    {
        uint h = ((uint)c.x * 0x8DA6B343u) ^ ((uint)c.y * 0xD8163841u);
        h ^= h >> 15; h *= 0x2C1B3C6Du; h ^= h >> 12; h *= 0x297A2D39u; h ^= h >> 15;
        return h;
    }

    float KoiHash01(int2 c) { return (float)(KoiHash(c) & 0xFFFFFFu) * (1.0 / 16777216.0); }

    // value noise on the integer lattice ("cell noise"), quintic fade
    float KoiNoise(float2 p)
    {
        float2 i = floor(p), f = p - i;
        float2 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
        int2 c = (int2)i;
        float n00 = KoiHash01(c), n10 = KoiHash01(c + int2(1, 0)), n01 = KoiHash01(c + int2(0, 1)), n11 = KoiHash01(c + int2(1, 1));
        return n00 + (n10 - n00) * u.x + (n01 - n00) * u.y + (n00 - n10 - n01 + n11) * u.x * u.y;
    }

    // Kohaku patch mask on (a, sin theta): symmetric about the dorsal line, so the front and back shells of a ray agree; four
    // patches (sandan + a tail spot) with organic edges from two noise octaves (80 m, 34 m), wrapping from the dorsal line down to
    // 2.3 +- 0.3 rad from the top (>= 70 % of the circumference), 0 on the head
    float KoiPatch(float a, float st)
    {
        float q = (1.0 - st) * 70.0;
        float n = 0.6 * KoiNoise(float2(a * (1.0 / 80.0) + 3.1, q * (1.0 / 80.0) + 7.3))
                + 0.4 * KoiNoise(float2(a * (1.0 / 34.0) + 9.4, q * (1.0 / 34.0) + 1.7));
        float4 c = float4(194.0, 356.0, 492.0, 618.0), h = float4(48.0, 39.0, 27.0, 13.0);
        float4 x = (a - c) / h;
        float4 e = saturate(1.0 - x * x);
        float4 g = smoothstep(1.6 * h, 1.3 * h, abs(a - c));
        float env = max(max(e.x, e.y), max(e.z, e.w));
        float gate = max(max(g.x, g.y), max(g.z, g.w));
        float band = smoothstep(0.37, 0.43, env + 1.8 * (n - 0.5)) * gate;
        float psi = acos(clamp(st, -1.0, 1.0));
        float pmax = 2.30 + 0.6 * (KoiNoise(float2(a * (1.0 / 60.0) + 5.3, 2.9)) - 0.5);
        return band * smoothstep(pmax + 0.10, pmax - 0.10, psi) * smoothstep(KOI_HEADA - 6.0, KOI_HEADA + 6.0, a);
    }

    // shade of one scale: e = distance inside its disc edge, t = posterior coordinate (-1..1) (column widths); w = rim AA width
    float KoiScaleShade(float e, float t, float w)
    {
        float rim = (1.0 - smoothstep(0.0, 0.11 + w, e)) * (0.11 / (0.11 + w));
        return 0.42 + 0.26 * smoothstep(-0.7, 1.0, t) + 0.42 * rim;
    }

    // imbricate scales on the (X columns, Y rows) lattice, rows offset by half a column (each scale with its own +-12 %
    // brightness): the most anterior disc that contains a point lies on top; candidates composited front to back with soft coverage over the footprint fw (column widths), each
    // darkened by the shadow just behind the edges in front of it. Returns the shade and the coverage-weighted patch mask of the
    // scale centres (the scalloped Kohaku edge); a and rowM give the centres' arc length (row spacing rowM metres).
    float KoiScales(float X, float Y, float fw, float a, float rowM, float patchP, out float patchQ)
    {
        float j0 = floor(Y - KOI_SR / KOI_KH) + 1.0;
        float acc = 0.0, wsum = 0.0, cover = 1.0, shadow = 1.0;
        patchQ = 0.0;
        [unroll] for (int k = 0; k < 3; k++)
        {
            float j = j0 + k;
            float odd = 0.5 * (j - 2.0 * floor(j * 0.5));
            float xc = floor(X - odd + 0.5) + odd;
            float2 d2 = float2(X - xc, (Y - j) * KOI_KH);
            float e = KOI_SR - length(d2);
            float cov = smoothstep(-fw, fw, e);
            float wk = cov * cover;
            // each scale +-12 % brighter or darker (hash of its row and half-column, 36 around: closes at the seam)
            float hc = 2.0 * xc - 36.0 * floor(2.0 * xc / 36.0);
            float jit = KoiHash01(int2((int)j, (int)(hc + 0.5))) - 0.5;
            float s = KoiScaleShade(e, d2.y / KOI_SR, fw) * shadow * (1.0 + 0.24 * jit);
            float ac = a - (Y - j) * rowM;
            patchQ += wk * KoiPatch(ac, sin(xc * (KOI_TAU / KOI_NC)));
            acc += wk * s;
            wsum += wk;
            cover *= 1.0 - cov;
            shadow *= 1.0 - 0.45 * (1.0 - smoothstep(0.0, 0.16 + fw, -e)) * (1.0 - cov);
        }
        float rest = max(0.0, 1.0 - wsum);
        patchQ += rest * patchP;
        return acc + rest * KOI_SCALEMEAN;
    }

    // caustic-like inner light: a warped ridge network at about 80 m drifting on a closed loop (periodic phase), [0, 1].
    // q = (a, 40 theta): p.y advances by pi per ring, so every p.y / r.y frequency is 2 and the network closes at theta = 2 pi
    // (no seam along the outer flank)
    float KoiCaustic(float2 q, float ph)
    {
        float s, c;
        sincos(KOI_TAU * ph, s, c);
        float2 p = q * (1.0 / 80.0) + 0.8 * float2(c, s);
        float2 r = p + 0.55 * float2(sin(p.y * 2.0 + 1.3), sin(p.x * 1.3 - 0.4));
        float v = 1.0 - 0.5 * abs(sin(r.x * 1.9 + sin(r.y * 2.0)) + sin(r.y * 2.0 + sin(r.x * 1.1 + 0.5)));
        return v * v;
    }

    // mean of the composited scale shade at footprint fw (column widths)
    float KoiScaleMeanAt(float fw)
    {
        float x = max(fw - KOI_SMF0, 0.0);
        return KOI_SCALEMEAN - KOI_SMA * x * x / (x + KOI_SMB);
    }

    float KoiLine(float d, float w, float fw) { return (1.0 - smoothstep(w, w + fw, abs(d))) * (w / (w + 0.5 * fw)); }

    float3 KoiDesat(float3 c, float k) { float y = dot(c, float3(0.2126, 0.7152, 0.0722)); return y + k * (c - y); }

    float3 KoiBudget(float3 c, float budget) { return c * min(1.0, budget / max(max(c.r, c.g), max(c.b, 1e-4))); }

    // fan fins (pectoral, pelvic): the fin spans u in [0, KoiFanUMax(v)] of the root chord, so s = u / umax(v) are rays radiating
    // from a short root; the outer edge is v = 1 - 0.3 s^2 (KoiShaderMath.FanUMax / FanVMax)
    float KoiFanUMax(float v) { return 0.35 + 0.65 * smoothstep(0.0, 0.75, v); }

    // The back shell traces its view ray through the tube (the local cylinder of radius rad around the spine tangent F) to the
    // front shell and returns that point's patch mask: light that passes the vermilion pigment of the front shell is tinted by
    // it, and a back-shell patch seen through white skin is dimmed (LOOK_V2 6: patches read vermilion, not salmon, from below).
    float KoiFrontPatch(float a, float rad, float3 nOut, float3 F, float3 vdir)
    {
        float3 v = -vdir;                                                    // camera -> fragment
        float vF = dot(v, F);
        float3 vp = v - vF * F;
        float vp2 = max(dot(vp, vp), 1e-4);
        float3 d = rad * nOut;
        float t = 2.0 * max(dot(d, vp), 0.0) / vp2;
        float3 df = d - t * vp;
        float3 N = cross(float3(0.0, 1.0, 0.0), F);
        N *= rsqrt(max(dot(N, N), 1e-12));
        float stF = dot(df, cross(F, N)) / max(rad, 1e-3);
        return KoiPatch(a + clamp(t * vF, -80.0, 80.0), clamp(stF, -1.0, 1.0));
    }

    half4 KoiFrag(Varyings i, FRONT_FACE_TYPE ff : FRONT_FACE_SEMANTIC) : SV_Target
    {
        // every derivative first (no branch above this line)
        float a = i.data0.x, th = i.data0.y, sv = i.data0.z, part = i.data0.w;
        float fu = i.data1.x, fv = i.data1.y, rad = i.data1.z;
        float X = th * (KOI_NC / KOI_TAU);
        float fwX = fwidth(X), fwY = fwidth(sv);
        float2 q = float2(a, th * 40.0);
        float fwQ = max(fwidth(q.x), fwidth(q.y));
        float fwU = fwidth(fu), fwV = fwidth(fv);
        float fanS = fu / KoiFanUMax(fv);
        float fwS = fwidth(fanS);
        float fwYw = fwidth(i.positionWS.y);
        float3 nGeo = cross(ddx(i.positionWS), ddy(i.positionWS));
        float fwA = fwidth(a);

        float front = IS_FRONT_VFACE(ff, 1.0, 0.0);
        float3 toCam = _WorldSpaceCameraPos.xyz - i.positionWS;
        float3 vdir = toCam * rsqrt(max(dot(toCam, toCam), 1e-6));
        float flow = _KoiTime.x, cph = _KoiTime.y, satK = _KoiParams.z, scan = _KoiParams.w;
        float3 aqua = lerp(PAL_KoiBody, PAL_KoiGlow, 0.35);
        float3 c;
        float budget;

        // slow scanlines in world height: 24 m bands at +-6 % and, on front faces only (the back shell's lines would cross
        // them in a lattice), thin 6 m lines (0.72 m wide, +26 %, their mean 0.12 subtracted so the body keeps its energy),
        // faded below 6 px of pitch and where the skin turns horizontal (|n.y| 0.55 -> 0.85: no contour loops on the belly
        // seen from below)
        float fwL = fwYw * (1.0 / 6.0);
        float lineS = KoiLine(frac(i.positionWS.y * (1.0 / 6.0) + 4.0 * scan) - 0.5, 0.06, fwL);
        float flatK = 1.0 - smoothstep(0.55, 0.85, abs(i.normalWS.y) * rsqrt(max(dot(i.normalWS, i.normalWS), 1e-12)));
        float scanK = 1.0 + 0.06 * cos(KOI_TAU * (i.positionWS.y * (1.0 / 24.0) + scan))
                    + 0.3 * front * flatK * smoothstep(3.0, 6.0, 1.0 / max(fwL, 1e-4)) * (lineS - 0.12);
        float caust = lerp(KOI_CAUSTMEAN, KoiCaustic(q, cph), smoothstep(2.0, 5.0, 10.0 / max(fwQ, 1e-4)));

        if (part < 0.5 || (part > 4.5 && part < 5.5))
        {
            // ---- body tube, head, caps and barbels (closed shells: front 1, back 0.35)
            float3 nO = normalize(i.normalWS);
            float3 n = nO * (front > 0.5 ? 1.0 : -1.0);
            float ndv = saturate(dot(n, vdir));
            float rim = pow(max(1.0 - ndv, 1e-4), KOI_RIMP);
            float st = sin(th), ct = cos(th);
            float x = saturate(a / KOI_L);
            float3 col = lerp(PAL_KoiBody, float3(0.88, 0.93, 0.96), 0.30 * (1.0 - smoothstep(0.02, 0.30, x)));
            col = lerp(col, PAL_KoiGlow, 0.35 * smoothstep(0.55, 1.0, x));
            float3 pc = KoiDesat(PAL_KoiPatch, satK);
            float body, rimK = 1.0, patchM = 0.0, frontP = 0.0;
            if (part > 4.5)
            {
                // barbel: a thin cool-white whisker fading to its tip
                body = 0.6 * (1.0 - 0.65 * saturate(fu)) * scanK;
                rim *= 0.5;
            }
            else
            {
                float rhoC = max(rad, KOI_RHOMIN);
                float rowM = KOI_KH * KOI_TAU * rhoC / KOI_NC;
                float fwE = max(fwX, fwY * KOI_KH);
                float pitchPx = 1.0 / max(max(fwX, fwY), 1e-5);
                float sFade = smoothstep(2.5, 5.0, pitchPx);
                float patchP = KoiPatch(a, st);
                float patchQ;
                float sc = KoiScales(X, sv, fwE, a, rowM, patchP, patchQ);
                patchM = lerp(patchP, patchQ, 0.5 * sFade);                      // C02: half-cell feather on the patch edges
                // head: smooth skin ahead of the gill cover (a bright arc, posterior-most on the side axes, its shadow behind)
                float gill = KOI_GILLA + KOI_GILLB * abs(ct);
                float onBody = smoothstep(gill - 3.0, gill + 5.0, a);
                float sideW = smoothstep(0.15, 0.45, abs(ct));
                float veinS = 0.5 + KOI_VEINS * sFade * (sc - KoiScaleMeanAt(fwE));
                float vein = lerp(0.48, veinS, onBody) + KOI_CAUSTA * (caust - KOI_CAUSTMEAN);
                vein += 0.45 * KoiLine(a - gill, 1.2, fwA) * sideW;
                vein -= 0.30 * smoothstep(gill + 1.0, gill + 3.0 + fwA, a) * (1.0 - smoothstep(gill + 4.0, gill + 12.0 + fwA, a)) * sideW;
                // mouth: a dark lip line across the lower front of the snout cap (a < 0), lowest on the ventral line
                float dV = th - 4.71238898;
                dV -= KOI_TAU * floor(dV / KOI_TAU + 0.5);
                vein -= 0.6 * KoiLine(a - (-7.0 + 3.5 * (1.0 - cos(dV))), 0.7, fwA) * (1.0 - smoothstep(0.9, 1.3, abs(dV)));
                vein = saturate(vein);
                // eye spots on both flanks (outer theta 0.40, inner pi - 0.40): a dark pupil and a bright ring
                float dthO = th - KOI_EYET, dthI = th - (3.14159265 - KOI_EYET);
                dthO -= KOI_TAU * floor(dthO / KOI_TAU + 0.5);
                dthI -= KOI_TAU * floor(dthI / KOI_TAU + 0.5);
                float rr = max(rad, 1.0);
                float dE = min(length(float2(a - KOI_EYEA, dthO * rr)), length(float2(a - KOI_EYEA, dthI * rr)));
                // the pupil may look aside (DREAD 5.2, wave B: _KoiPupil, at most 0.3 R off the centre; 0 = centred, as in wave A)
                float dP = min(length(float2(a - KOI_EYEA - _KoiPupil.x, dthO * rr - _KoiPupil.y)),
                               length(float2(a - KOI_EYEA - _KoiPupil.z, dthI * rr - _KoiPupil.w)));
                float fwD = fwA + 1e-3;
                float eye = lerp(1.0, 0.15, 1.0 - smoothstep(KOI_EYER * 0.62 - fwD, KOI_EYER * 0.62 + fwD, dP));
                eye += 0.6 * KoiLine(dE - KOI_EYER, 0.45, fwD);
                eye = lerp(eye, 1.0, smoothstep(KOI_EYER * 0.8, KOI_EYER * 2.5, fwD));          // gone below about 2.5 px
                body = (KOI_STRB + KOI_STRK * vein) * (0.88 + 0.12 * st) * eye * scanK;
                // C02 interference gaps across the body (mean kept: the line's mean 2 KOI_GAPW is subtracted)
                float fwG = fwA * (1.0 / KOI_GAPP);
                float gapL = KoiLine(frac(a * (1.0 / KOI_GAPP)) - 0.5, KOI_GAPW, fwG);
                body *= 1.0 - _GapDepth * smoothstep(3.0, 6.0, 1.0 / max(fwG, 1e-4)) * (gapL - 2.0 * KOI_GAPW);
                rimK = 0.8 + 0.5 * (vein - 0.5);                                 // the scales carry into the Fresnel edge
                // the back shell takes the colour of the light that reaches the camera through the front shell: vermilion
                // behind a front patch, aqua with a faint warm hint where only the back is patched (no grey stain)
                if (front < 0.5) frontP = KoiFrontPatch(a, rad, nO, normalize(i.tangentWS), vdir);
                patchM = front > 0.5 ? patchM : max(frontP, 0.25 * patchM);
                col = lerp(col, pc, patchM);
            }
            float pk = patchM;
            float3 rimC = lerp(PAL_KoiGlow, pc, pk) * (1.0 - 0.5 * pk);           // a patch glows vermilion at its edge, never pink
            float tailF = smoothstep(KOI_TAILA0, KOI_TAILA1, a);
            // C02: the front shell thins at the centre; the Kohaku pigment stays denser (patch sat >= 0.60, LOOK_V2 6)
            float clearK = 1.0 - _CoreClear * front * ndv * ndv * (1.0 - 0.7 * pk);
            c = col * (_BodyGain * body * clearK * (1.0 - 0.9 * tailF)) + rimC * (_RimGain * rim * rimK * (1.0 - 0.85 * tailF));
            c *= front > 0.5 ? 1.0 : _BackWeight;
            budget = front > 0.5 ? _FrontBudget : _BackBudget;
        }
        else
        {
            // ---- membranes (single sheets): view-dependent thickness boost toward grazing
            float ndvG = abs(dot(nGeo * rsqrt(max(dot(nGeo, nGeo), 1e-20)), vdir));
            float boost = lerp(1.0, 1.4, (1.0 - ndvG) * (1.0 - ndvG));
            float inten;
            if (part < 1.5)
            {
                // tail veil: rays at constant k (19), a faint membrane between, flow bands running back, bright split edge,
                // rounded lobes
                float vv = (a - KOI_VEILA0) / KOI_VEILL;
                float k = fu, ak = abs(k);
                float fwK = fwU * 9.0;
                float ray = KoiLine(frac(k * 9.0 + 0.5) - 0.5, 0.07, fwK) * smoothstep(2.0, 4.5, 1.0 / max(fwK, 1e-5));
                float vmax = (KOI_VEILNOTCH + (KOI_VEILL - KOI_VEILNOTCH) * ak) / KOI_VEILL;
                float fwVV = fwA / KOI_VEILL + 1e-5;
                float edgeD = vmax - vv;
                float mask = smoothstep(0.0, 0.05 + fwVV, edgeD) * smoothstep(0.0, 0.15, vv);
                mask *= 1.0 - 0.85 * smoothstep(0.55, 1.0, ak) * smoothstep(vmax - 0.35, vmax, vv);
                float edge = 1.0 - smoothstep(0.0, 0.06 + fwVV, edgeD);
                float fl = 0.78 + 0.22 * sin(KOI_TAU * (vv * 3.5 - 1.3 * flow) + k * 1.5);
                inten = (0.28 + 0.9 * ray + 1.2 * edge * smoothstep(0.3, 0.6, vv)) * fl * mask;
                c = lerp(PAL_KoiBody, PAL_KoiGlow, 0.5) * (_BodyGain * _VeilGain * inten * boost) * scanK;
                budget = _VeilBudget;
            }
            else if (part < 3.5 || (part > 5.5 && part < 7.5))
            {
                // pectoral and pelvic fins: a fan of 11 jointed rays from a short root, a stiff leading ray, edge glow
                float s = fanS;
                float edgeD = (1.0 - 0.3 * saturate(s) * saturate(s)) - fv;
                float fwR = fwS * 11.0;
                float ray = KoiLine(frac(s * 11.0 + 0.5) - 0.5, 0.08, fwR) * smoothstep(2.0, 4.5, 1.0 / max(fwR, 1e-5));
                float seg = 1.0 - 0.25 * smoothstep(2.0, 4.0, 0.1 / max(fwV, 1e-5)) * (0.5 + 0.5 * cos(KOI_TAU * fv * 10.0));
                float lead = 1.0 - smoothstep(0.0, 0.05 + fwS, s);
                float trail = 1.0 - smoothstep(0.0, 0.05 + fwS, 1.0 - s);
                float edge = 1.0 - smoothstep(0.0, 0.07 + fwV, edgeD);
                float mask = smoothstep(0.0, 0.03 + fwV, edgeD) * smoothstep(0.0, 0.10, fv);
                float fl = 0.85 + 0.15 * sin(KOI_TAU * (fv * 2.5 - flow));
                inten = (0.4 + 0.6 * ray * seg + 0.6 * lead + 0.3 * trail + 0.8 * edge) * fl * mask;
                c = aqua * (_BodyGain * _FinGain * inten * boost) * scanK;
                budget = _FinBudget;
            }
            else
            {
                // dorsal (part 4) and anal (part 8) fins: rays sloping back (5 m apart at the root), a stiff leading ray, edge
                // glow along the free edge
                float anal = step(7.5, part);
                float h0 = lerp(16.0, 10.0, anal), hp = lerp(40.0, 24.0, anal), h1 = lerp(12.0, 8.0, anal);
                float up = lerp(0.25, 0.30, anal), len = lerp(90.0, 40.0, anal);
                float hgt = fu <= up ? lerp(h0, hp, fu / up) : lerp(hp, h1, (fu - up) / (1.0 - up));
                float rc = (fu * len - 0.55 * fv * hgt) * 0.2;
                float fwR = max(fwU * len * 0.2, fwV * hgt * 0.11);
                float ray = KoiLine(frac(rc + 0.5) - 0.5, 0.09, fwR) * smoothstep(2.0, 4.5, 1.0 / max(fwR, 1e-5));
                float lead = 1.0 - smoothstep(0.0, 0.05 + fwU, fu);
                float edge = 1.0 - smoothstep(0.0, 0.10 + fwV, 1.0 - fv);
                float mask = smoothstep(0.0, 0.04 + fwV, 1.0 - fv) * smoothstep(1.0, 0.94 - fwU, fu) * smoothstep(0.0, 0.10, fv);
                float fl = 0.85 + 0.15 * sin(KOI_TAU * (fu * 3.0 - 1.2 * flow));
                inten = (0.5 + 0.5 * ray + 0.6 * lead + 0.7 * edge) * fl * mask;
                c = aqua * (_BodyGain * _FinGain * inten * boost) * scanK;
                budget = _FinBudget;
            }
            c *= 0.85 + 0.3 * caust;
        }
        c = KoiBudget(c, budget);                                            // hue-preserving clamp by the brightest channel
        float T = NocFogT(i.positionWS);
        float cloud = 1.0 - smoothstep(NC_CloudMixY0, NC_CloudMixY1, i.positionWS.y);
        c = min(c * (T * cloud), 3.0);
        return half4(c, 0.0);                                                // additive light: no NocFloor (D-009)
    }

    // C02 transmission: the background behind the front body shell is lifted by dst x (1 + k) (Blend DstColor One), k largest at
    // the silhouette centre and 0 at the Fresnel rim, faded with the fog like the hologram itself; drawn before the additive pass
    half4 KoiGhostFrag(Varyings i, FRONT_FACE_TYPE ff : FRONT_FACE_SEMANTIC) : SV_Target
    {
        float part = i.data0.w;
        float front = IS_FRONT_VFACE(ff, 1.0, 0.0);
        float3 toCam = _WorldSpaceCameraPos.xyz - i.positionWS;
        float3 vdir = toCam * rsqrt(max(dot(toCam, toCam), 1e-6));
        float ndv = saturate(dot(i.normalWS * rsqrt(max(dot(i.normalWS, i.normalWS), 1e-12)), vdir));
        float bodyK = part < 0.5 ? 1.0 : 0.0;
        float tailF = smoothstep(KOI_TAILA0, KOI_TAILA1, i.data0.x);
        float T = NocFogT(i.positionWS);
        float cloud = 1.0 - smoothstep(NC_CloudMixY0, NC_CloudMixY1, i.positionWS.y);
        float k = _GhostGain * front * bodyK * ndv * ndv * (1.0 - tailF) * T * cloud;
        return half4(0.9 * k, k, k, 0.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent+30" "IgnoreProjector" = "True" }
        Pass
        {
            Name "Ghost"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Blend DstColor One
            ZWrite Off ZTest LEqual Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex KoiVert
            #pragma fragment KoiGhostFrag
            ENDHLSL
        }
        Pass
        {
            Name "Koi"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off ZTest LEqual Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex KoiVert
            #pragma fragment KoiFrag
            ENDHLSL
        }
    }
    Fallback Off
}
