// BwConduit - the beat-7 conduit behind the tunnel's grating panel (MASTER_PROMPT 14.5.7, 14.2.6 "BwSheet / BwConduit":
// conduit water in beat 7, advected flow noise, spray). One shader, two modes on the same conduit frame (u along the flow
// from the panel's south end, v across toward the far wall, y world; set by Beat07SluiceLedge):
//   _Mode 0  WATER: the sea at 14 m/s, supercritical (Froude 3.7 at 1.45 m depth): a STATIONARY diamond pattern of oblique
//            cross waves (shock lines at asin(1/Fr) = 15 deg to the flow, as fast channel flow shows them) and rooster tails
//            along both walls, travelling roll waves (9 m, 15.5 m/s), and turbulent streaks advected with the flow (value
//            noise stretched 3 m along x 0.35 m across, two layers at 14 and 11 m/s). Aerated white-green: foam from the crests,
//            the streaks, the wall boundary layers and the plunge into the drop shaft (from u 37.5). The surface mesh is
//            displaced in the vertex stage by the smooth parts only (cross waves, roll waves, the plunge).
//   _Mode 1  WALLS: wet concrete (the library's Concrete texture set, triplanar, recoloured to ConcreteWet) with a green
//            water line; the cold wall lights are painted on the far wall (1.2 m bars, CityCool x3, clamp 3 as lamps).
// Light (both modes, analytic, no real lights): the cold bars as line lights (Lambert + GGX with the representative point
// of Karis 2013 for the specular), the tunnel's sodium cages spilling through the 2.8 m panel slot (visibility by the slot,
// 50 % grating transmission), a dim cold ambient. Spray: an in-shader veil along the view ray from the panel plane to the
// surface (4 samples of an advected density exp(-(y - water) / 1.4 m) plus the shaft plume, single scattering of the
// cold bars with a Henyey-Greenstein phase g 0.5): the conduit reads as roaring water in its own spray. _Veil scales it
// (0 when the MistVolumeFeature integrates the same emitter, ConduitSpray hook).
// Stability (Part 10 #3, #15; 14.2.5 photosensitivity): every noise octave fades by the pixel footprint across it, the
// moving streaks only tilt the normal and modulate foam (block means stay constant), the specular is clamped (6), roughness
// floor 0.12 plus the variance of the faded octaves. Flow offsets come from the C# updater in double, wrapped on exact
// periods of the noise lattice (64 cells of 3 m) and of the roll waves: nothing jumps at the 3600 s clock wrap.
// Fog: BwFogApply last. Colours: palette tokens set by the builder (_ColdColor CityCool, _SodiumColor, _WaterColor SeaTeal,
// _FoamColor FoamWhite, _WallColor ConcreteWet, _RefColor ConcreteMid).
Shader "Megastructure/BwConduit"
{
    Properties
    {
        _Mode ("Mode: 0 water, 1 walls", Float) = 0
        _Frame0 ("Frame origin xyz (world), w = conduit length (m)", Vector) = (0, 0, 0, 43)
        _Frame1 ("Frame axes: xy = tangent (x, z), zw = right (x, z)", Vector) = (0, 1, 1, 0)
        _Dims0 ("v panel, v far wall, water y, floor y", Vector) = (1.6, 9.9, -90.5, -91.8)
        _Dims1 ("ceiling y, shaft u, lamp pitch, first lamp u", Vector) = (-85.8, 37.5, 6, 2)
        _Dims2 ("lamp y, lamp length, culvert u, panel top y", Vector) = (-87.5, 1.2, -4, -87.2)
        _Spill ("tunnel cages: first u, pitch, lateral, y", Vector) = (-3.875, 6, -0.55, -86.75)
        _ColdColor ("Cold light (linear token)", Color) = (0.515, 0.761, 1, 1)
        _SodiumColor ("Sodium (linear token)", Color) = (1, 0.434, 0.068, 1)
        _WaterColor ("Water body (linear token)", Color) = (0.014, 0.168, 0.202, 1)
        _FoamColor ("Foam (linear token)", Color) = (0.823, 0.880, 0.896, 1)
        _WallColor ("Wet wall (linear token)", Color) = (0.037, 0.045, 0.053, 1)
        _RefColor ("Texture reference token", Color) = (0.212, 0.223, 0.220, 1)
        _ColdRadiance ("Cold bar radiance (x token)", Float) = 3.0
        _ColdLight ("Cold bar intensity (point-light units)", Float) = 12.0
        _SpillLight ("Sodium spill intensity", Float) = 0.9
        _Veil ("Spray veil strength", Float) = 1
        _VeilSigma ("Spray extinction at the surface (1/m)", Float) = 0.16
        _FlowOffset ("Flow offsets (m): x 14 m/s, y 11 m/s, z roll phase, w slow", Vector) = (0, 0, 0, 0)
        _TexScale ("Wall texture tile (m)", Float) = 2.0
        [NoScaleOffset] _AlbedoMap ("Albedo (BwTexGen Concrete)", 2D) = "grey" {}
        [NoScaleOffset] _NormalMap ("Normal (BC5 RG)", 2D) = "bump" {}
        [NoScaleOffset] _MaskMap ("Mask", 2D) = "grey" {}
    }

    HLSLINCLUDE
    #include "Include/BwLitCore.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _Frame0;
        float4 _Frame1;
        float4 _Dims0;
        float4 _Dims1;
        float4 _Dims2;
        float4 _Spill;
        float4 _ColdColor;
        float4 _SodiumColor;
        float4 _WaterColor;
        float4 _FoamColor;
        float4 _WallColor;
        float4 _RefColor;
        float4 _FlowOffset;
        float _Mode;
        float _ColdRadiance;
        float _ColdLight;
        float _SpillLight;
        float _Veil;
        float _VeilSigma;
        float _TexScale;
    CBUFFER_END

    TEXTURE2D(_AlbedoMap); SAMPLER(sampler_AlbedoMap);
    TEXTURE2D(_NormalMap);
    TEXTURE2D(_MaskMap);

    #define CD_PERIOD 64            // noise lattice period along the flow (cells; the C# offsets wrap on 64 x 3 m)
    #define CD_CELL_U 3.0           // streak cell along the flow (m)
    #define CD_CELL_V 0.6           // streak cell across (m)
    #define CD_ROLL 9.0             // roll-wave length (m)
    #define CD_SIN_B 0.2588         // shock angle 15 deg
    #define CD_COS_B 0.9659
    #define CD_SPEC_MAX 6.0

    // ---------------------------------------------------------------- frame
    float3 CdLocal(float3 p)
    {
        float2 d = p.xz - _Frame0.xz;
        return float3(dot(d, _Frame1.xy), dot(d, _Frame1.zw), p.y);   // pal:ok (u, v, y)
    }
    float3 CdWorldDir(float3 l) { return float3(_Frame1.x * l.x + _Frame1.z * l.y, l.z, _Frame1.y * l.x + _Frame1.w * l.y); }   // pal:ok (not a colour)

    // ---------------------------------------------------------------- periodic value noise (period 64 cells in x)
    float CdHash(int2 i) { return BwHash21(int2(i.x & (CD_PERIOD - 1), i.y)); }
    float CdNoise(float2 x)
    {
        float2 f = floor(x);
        int2 i = (int2)f;
        float2 t = x - f;
        float2 w = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
        float a = CdHash(i), b = CdHash(i + int2(1, 0)), c = CdHash(i + int2(0, 1)), d = CdHash(i + int2(1, 1));
        return lerp(lerp(a, b, w.x), lerp(c, d, w.x), w.y);
    }
    // three octaves (cells 3 / 1.5 / 0.75 m along, 0.35 / 0.175 / 0.0875 m across), each faded by the footprint across it;
    // returns the value and the variance of what was faded (it goes into the roughness)
    float CdStreak(float u, float v, float off, float fp, float seed, out float lost)
    {
        float s = 0.0, n = 0.0, a = 0.5, k = 1.0;
        lost = 0.0;
        [unroll] for (int o = 0; o < 3; o++)
        {
            float w = 1.0 - smoothstep(0.25, 0.5, fp * k / CD_CELL_V);
            float vw = v + 0.35 * sin(6.2831853 * (u - off) / 12.0 + seed) + 0.2 * sin(6.2831853 * (u - off) / 6.0 + 1.7 * seed);
            float2 x = float2((u - off) * k / CD_CELL_U + 13.0 * o, vw * k / CD_CELL_V + seed + 7.1 * o);
            s += a * lerp(0.5, CdNoise(x), w);
            lost += a * a * (1.0 - w * w) * 0.083;
            n += a; a *= 0.5; k *= 2.0;
        }
        return s / n;
    }

    // ---------------------------------------------------------------- water height (smooth parts) and its gradient
    // cross waves (stationary diamonds) + rooster tails at both walls + roll waves (travelling), times an envelope that
    // builds up from the culvert; minus the plunge into the drop shaft.
    float CdPlunge(float u) { float k = saturate((u - (_Dims1.y - 1.5)) / (_Frame0.w - 0.5 - (_Dims1.y - 1.5))); return 5.5 * k * k; }
    float CdHeight(float u, float v, out float2 grad, out float crest)
    {
        float v0 = _Dims0.x + 0.15, v1 = _Dims0.y;
        float k = 6.2831853 / 4.2;
        float env = smoothstep(_Dims2.z, _Dims2.z + 10.0, u) * (1.0 - smoothstep(_Dims1.y - 4.0, _Dims1.y, u));
        float vv = v - v0;
        float p1 = k * (-u * CD_SIN_B + vv * CD_COS_B), p2 = k * (u * CD_SIN_B + vv * CD_COS_B) + 1.3;
        float e0w = exp(-max(v - v0, 0.0) / 0.45);
        float As = 0.12 * env * (1.0 - 0.6 * e0w);                       // calmer against the panel wall (stays under the floor)
        float hs = As * (cos(p1) + cos(p2));
        float2 gs = -As * k * (sin(p1) * float2(-CD_SIN_B, CD_COS_B) + sin(p2) * float2(CD_SIN_B, CD_COS_B));
        float ph = 6.2831853 * (u - _FlowOffset.z) / CD_ROLL + 0.6 * sin(6.2831853 * v / 7.0);
        float Ar = 0.09 * smoothstep(_Dims2.z, _Dims2.z + 6.0, u);
        float hr = Ar * sin(ph);
        float2 gr = Ar * cos(ph) * float2(6.2831853 / CD_ROLL, 0.6 * 6.2831853 / 7.0 * cos(6.2831853 * v / 7.0));
        float e0 = exp(-max(v - v0, 0.0) / 0.45), e1 = exp(-max(v1 - v, 0.0) / 0.45);
        // rooster tails: 0.12 m against the tunnel-side wall (the crest there peaks 0.04 m under the tunnel floor), 0.38 m on
        // the far wall
        float hb = (0.12 * e0 + 0.38 * e1) * env;
        float2 gb = env * float2(0.0, -0.12 * e0 / 0.45 + 0.38 * e1 / 0.45);
        grad = gs + gr + gb;
        crest = saturate(0.5 + hs / max(2.0 * As, 1e-3) * 0.5) * env;
        return hs + hr + hb;
    }

    // ---------------------------------------------------------------- light
    // the cold bars: line lights along u at (u_k +- L/2, v far wall - 0.08, lamp y)
    int CdLampCount() { return (int)floor((_Dims1.y + 1.0 - _Dims1.w) / _Dims1.z) + 1; }

    // GGX (URP "Optimizing PBR" form) for roughness a (perceptual^2), clamped
    float CdSpec(float3 N, float3 L, float3 V, float a)
    {
        float3 H = normalize(L + V);
        float NoH = saturate(dot(N, H)), LoH = saturate(dot(L, H));
        float a2 = a * a;
        float d = NoH * NoH * (a2 - 1.0) + 1.00001;
        return a2 / ((d * d) * max(0.1, LoH * LoH) * (4.0 * a + 2.0));
    }

    // diffuse irradiance and specular radiance at P (local) with normal N (world), view V (world, toward the eye)
    void CdLights(float3 Pl, float3 Pw, float3 N, float3 V, float a, float F0, out float3 diff, out float3 spec)
    {
        diff = 0.0; spec = 0.0;
        int nL = CdLampCount();
        float idx = clamp(round((Pl.x - _Dims1.w) / _Dims1.z), 0.0, nL - 1.0);
        float3 R = reflect(-V, N);
        float3 Du = CdWorldDir(float3(1.0, 0.0, 0.0));   // pal:ok (direction)
        // the culvert soffit hides the bars from the culvert; the shaft walls from the shaft
        float occ = lerp(smoothstep(_Dims2.z - 1.5, _Dims2.z + 0.5, Pl.x), 1.0, step(_Dims0.z + 1.4, Pl.z)) * exp(min(Pl.z - _Dims0.w, 0.0) / 2.0);
        [unroll] for (int j = -1; j <= 1; j++)
        {
            float k = idx + j;
            if (k < 0.0 || k > nL - 1.0) continue;
            float uk = _Dims1.w + _Dims1.z * k;
            float3 A = CdWorldDir(float3(uk, _Dims0.y - 0.08, 0.0)) + float3(_Frame0.x, _Dims2.x, _Frame0.z);   // pal:ok (position)
            float hl = 0.5 * _Dims2.y;
            // diffuse: the closest point of the bar
            float sD = clamp(dot(Pw - A, Du), -hl, hl);
            float3 Ld = A + Du * sD - Pw;
            float d2 = max(dot(Ld, Ld), 0.5);
            float3 l = Ld * rsqrt(max(dot(Ld, Ld), 1e-6));
            float win = saturate(1.0 - d2 / 400.0); win *= win;
            float E = _ColdLight / d2 * win * occ;
            diff += _ColdColor.rgb * (E * saturate(dot(N, l)));
            // specular: the representative point closest to the reflection ray (Karis 2013)
            float3 w0 = A - Pw;
            float b = dot(Du, R);
            float sS = clamp((b * dot(R, w0) - dot(Du, w0)) / max(1.0 - b * b, 1e-3), -hl, hl);
            float3 Ls = A + Du * sS - Pw;
            float ds2 = max(dot(Ls, Ls), 0.5);
            float3 ls = Ls * rsqrt(max(dot(Ls, Ls), 1e-6));
            float NoL = saturate(dot(N, ls));
            float Fv = F0 + (1.0 - F0) * pow(1.0 - saturate(dot(ls, normalize(ls + V))), 5.0);
            spec += _ColdColor.rgb * (CdSpec(N, ls, V, a) * Fv * NoL * _ColdLight / ds2 * win * occ);
        }
        // sodium cages of the tunnel through the panel slot (F .. panel top at v = panel plane), 50 % grating transmission
        float nS = 8.0;
        float ks = clamp(round((Pl.x - _Spill.x) / _Spill.y), 0.0, nS - 1.0);
        [unroll] for (int m = -1; m <= 1; m++)
        {
            float k = ks + m;
            if (k < 0.0 || k > nS - 1.0) continue;
            float3 Sl = float3(_Spill.x + _Spill.y * k, _Spill.z, _Spill.w);   // pal:ok (position u, v, y)
            float3 dl = Pl - Sl;
            if (dl.y <= 0.05) continue;
            float tc = (_Dims0.x - Sl.y) / dl.y;
            float yc = Sl.z + dl.z * tc;
            float vis = smoothstep(_Frame0.y - 0.05, _Frame0.y + 0.1, yc) * (1.0 - smoothstep(_Dims2.w - 0.1, _Dims2.w + 0.05, yc));
            float d2 = max(dot(dl, dl), 0.25);
            float3 l = -CdWorldDir(float3(dl.x, dl.y, 0.0)) - float3(0.0, dl.z, 0.0);   // pal:ok (direction)
            l *= rsqrt(max(dot(l, l), 1e-6));
            diff += _SodiumColor.rgb * (_SpillLight * 0.5 * vis / d2 * saturate(dot(N, l)));
        }
        // ambient: the tunnel's interior ambient through the panel, and the conduit's own bounce
        diff += _BwAmbMid.rgb * 0.6 + _ColdColor.rgb * 0.012 * occ;
        spec = min(spec, CD_SPEC_MAX);
    }

    // in-scatter of the spray veil along the eye ray from the panel plane to Pw (4 fixed samples, no jitter)
    float3 CdVeil(float3 color, float3 Pw)
    {
        if (_Veil <= 0.0) return color;
        float3 E = _BwCam.xyz;
        float3 El = CdLocal(E), Pl = CdLocal(Pw);
        float3 dlo = Pl - El;
        float tEnter = dlo.y > 1e-4 ? saturate((_Dims0.x - El.y) / dlo.y) : 0.0;
        float len = length(Pw - E);
        float seg = len * (1.0 - tEnter);
        if (seg <= 1e-3) return color;
        float3 Vw = (Pw - E) / max(len, 1e-4);
        float T = 1.0;
        float3 L = 0.0;
        float dt = seg / 4.0;
        int nL = CdLampCount();
        [unroll] for (int i = 0; i < 4; i++)
        {
            float t = tEnter + (1.0 - tEnter) * (i + 0.5) / 4.0;
            float3 q = lerp(El, Pl, t);
            float3 qw = lerp(E, Pw, t);
            float h = max(q.z - _Dims0.z, 0.0);
            // spray streaks flying with the water (3 m along, 0.4 m across, 0.6 m up; the lattice period divides the offset wrap)
            float n = 0.6 * CdNoise(float2((q.x - _FlowOffset.x) / CD_CELL_U, q.y / 0.4 + q.z * 1.6)) + 0.4 * CdNoise(float2((q.x - _FlowOffset.y) / (0.5 * CD_CELL_U), q.y / 0.25 + q.z * 2.3 + 9.1));
            float shaft = 3.0 * smoothstep(_Dims1.y - 4.0, _Dims1.y + 2.0, q.x) * exp(-h / 3.5);
            // (fix wave S2-4: the spray a little thicker and taller, 1.2 x over a 2.4 m scale height)
            float rho = 1.2 * _VeilSigma * (exp(-h / 2.4) * (0.3 + 1.4 * n) + shaft) * _Veil;
            rho *= step(_Dims2.z, q.x) * step(q.x, _Frame0.w);
            // light at the sample: the nearest bar (isotropic + forward lobe toward the eye)
            float idx = clamp(round((q.x - _Dims1.w) / _Dims1.z), 0.0, nL - 1.0);
            float3 J = 0.0;
            [unroll] for (int j = -1; j <= 1; j++)
            {
                float k = idx + j;
                if (k < 0.0 || k > nL - 1.0) continue;
                float3 A = CdWorldDir(float3(_Dims1.w + _Dims1.z * k, _Dims0.y - 0.08, 0.0)) + float3(_Frame0.x, _Dims2.x, _Frame0.z);   // pal:ok
                float3 ld = A - qw;
                float d2 = max(dot(ld, ld), 0.25);
                float c = dot(ld * rsqrt(d2), Vw);   // cos of the scattering angle: light lamp -> sample -> eye
                float g = 0.5;
                float hg = (1.0 - g * g) / pow(max(1.0 + g * g - 2.0 * g * c, 1e-3), 1.5);
                J += _ColdColor.rgb * (_ColdLight / d2 * (0.3 + 0.7 * hg) * 0.25);
            }
            J += _BwAmbMid.rgb * 0.5;
            float a = 1.0 - exp(-rho * dt);
            L += T * a * J;
            T *= 1.0 - a;
        }
        return color * T + L;
    }

    // ---------------------------------------------------------------- vertex
    struct CdAttributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
    struct CdVaryings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float3 normalWS : TEXCOORD1; UNITY_VERTEX_INPUT_INSTANCE_ID };

    CdVaryings CdVert(CdAttributes v)
    {
        CdVaryings o = (CdVaryings)0;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_TRANSFER_INSTANCE_ID(v, o);
        float3 p = TransformObjectToWorld(v.positionOS.xyz);
        if (_Mode < 0.5)
        {
            float3 l = CdLocal(p);
            float2 g; float crest;
            p.y = _Dims0.z + CdHeight(l.x, l.y, g, crest) - CdPlunge(l.x);
        }
        o.positionWS = p;
        o.normalWS = TransformObjectToWorldNormal(v.normalOS);
        o.positionCS = TransformWorldToHClip(p);
        return o;
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On ZTest LEqual Cull Back
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex CdVert
            #pragma fragment CdFrag
            #pragma multi_compile_instancing

            half4 CdFrag(CdVaryings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 Pw = i.positionWS;
                float3 Pl = CdLocal(Pw);
                float3 Vv = _BwCam.xyz - Pw;
                float dist = length(Vv);
                float3 V = Vv / max(dist, 1e-4);
                float fp = max(dist * _BwCam.w, 1e-4);
                float3 col;
                if (_Mode < 0.5)
                {
                    // ---- water
                    float2 g; float crest;
                    float h = CdHeight(Pl.x, Pl.y, g, crest);
                    float plunge = saturate((Pl.x - (_Dims1.y - 1.5)) / 3.0);
                    float lost1, lost2;
                    float s1 = CdStreak(Pl.x, Pl.y, _FlowOffset.x, fp, 0.0, lost1);
                    float s2 = CdStreak(Pl.x, Pl.y, _FlowOffset.y, fp, 31.7, lost2);
                    // streak slope: finite difference across the flow (the streaks are long along it)
                    float lostD;
                    float dv = max(0.06, 0.5 * fp);
                    float s1v = CdStreak(Pl.x, Pl.y + dv, _FlowOffset.x, fp, 0.0, lostD);
                    float slopeV = (s1v - s1) / dv * 0.35 * (1.0 - smoothstep(0.1, 0.3, fp));
                    float slopeU = (s2 - 0.5) * 0.12;
                    float2 gt = g + float2(slopeU, slopeV);
                    float3 nl = normalize(float3(-gt.x, 1.0, -gt.y));   // pal:ok (local normal, (u, y, v) order)
                    float3 N = normalize(CdWorldDir(float3(nl.x, nl.z, 0.0)) + float3(0.0, nl.y, 0.0));   // pal:ok (normal)
                    // foam: crests, streaks, the wall boundary layers, the plunge
                    float v0 = _Dims0.x + 0.15, v1 = _Dims0.y;
                    float bnd = exp(-max(Pl.y - v0, 0.0) / 0.6) + exp(-max(v1 - Pl.y, 0.0) / 0.6);
                    // foam rafts (1.5 x 0.9 m cells, advected with the 14 m/s layer; the lattice period divides the offset wrap)
                    // (two octaves, the second 0.75 x 0.45 m, warped by the cross-wave field so rafts gather on the crests)
                    float2 rq = float2((Pl.x - _FlowOffset.x) / 1.5 + 5.1, Pl.y / 0.9 + 2.3 + 0.8 * crest);
                    float raft = 0.65 * CdNoise(rq) + 0.35 * CdNoise(rq * 2.0 + 11.3);
                    raft = lerp(0.5, raft, 1.0 - smoothstep(0.2, 0.45, fp / 0.9));
                    // clear, dark-green water between the white: the stationary shock diamonds and the rafts carry the foam
                    // self-aerated supercritical flow (the boundary layer has reached the surface 40 m from the culvert): mostly
                    // white, torn by dark-green streaks of clear water between the rafts
                    // (bimodal: white rafts and crests over about half the surface, dark-green clear water between them)
                    float fq = 0.5 + 0.95 * (raft - 0.5) + 0.6 * (s1 - 0.5) + 0.4 * (s2 - 0.5) + 0.45 * (crest - 0.5);
                    float fw = max(0.08, 0.6 * fp / 0.9);                    // transition widened with the footprint (no aliasing)
                    // (fix wave S2-4: KS19 read as a silvery mirror floor; the sea at 14 m/s is white water: the foam threshold
                    // lowered so about 65 % of the surface is aerated)
                    float foam = saturate(smoothstep(0.38 - fw, 0.38 + fw, fq) * 0.95 + 0.5 * bnd + plunge);
                    float a = 0.12 + sqrt(lost1 + lost2) * 0.6 + 0.12 * foam;   // roughness floor + filtered streak variance
                    float F0 = 0.02;
                    float3 diff, spec;
                    CdLights(Pl, Pw, N, V, saturate(a), F0, diff, spec);
                    // aerated colour: FoamWhite on the crests to SeaTeal in the clear runs (the body brighter: bubbly water)
                    float3 body = _WaterColor.rgb * 0.45;
                    float3 alb = lerp(body, _FoamColor.rgb * 0.8, foam);
                    // light entering the clear water scatters back green (subsurface), less where it is foam already
                    float3 sss = _WaterColor.rgb * 1.6 * (1.0 - foam);
                    float NoV = saturate(dot(N, V));
                    float Fr = F0 + (1.0 - F0) * pow(1.0 - NoV, 5.0);
                    col = diff * (alb + sss) * (1.0 - Fr) + spec * (1.0 - 0.6 * foam);
                    col += Fr * (1.0 - 0.75 * foam) * (_ColdColor.rgb * 0.02 + _BwAmbMid.rgb * 0.5);   // foam does not mirror
                }
                else
                {
                    // ---- walls: wet concrete, water line, the painted cold bars on the far wall
                    float3 ng = normalize(i.normalWS);
                    BwTri t0 = BwTriSetup(Pw, ng, _TexScale, 6.0);
                    float3 texAlb = BwTriSample(TEXTURE2D_ARGS(_AlbedoMap, sampler_AlbedoMap), t0).rgb;
                    float4 mask = BwTriSample(TEXTURE2D_ARGS(_MaskMap, sampler_AlbedoMap), t0);
                    float3 N = BwTriNormal(TEXTURE2D_ARGS(_NormalMap, sampler_AlbedoMap), t0, ng, 1.0);
                    N = normalize(lerp(N, ng, 0.5));                                       // water film
                    float3 alb = BwRecolour(texAlb, _WallColor.rgb, _RefColor.rgb) * 1.35;
                    // green-black water line band up to 0.9 m above the mean surface, streaked downward
                    float wl = 1.0 - smoothstep(0.2, 0.9 + 0.3 * BwValueNoise(float2(Pl.x * 1.7, 3.1)), Pw.y - _Dims0.z);
                    alb = lerp(alb, _WaterColor.rgb * 0.18, 0.7 * wl);
                    float3 diff, spec;
                    CdLights(Pl, Pw, N, normalize(_BwCam.xyz - Pw), 0.2, 0.04, diff, spec);
                    col = (diff * alb * mask.g + spec * 0.6) * 0.6;   // fix wave S2-4: the wall dimmed (the water is the subject)
                    // cold bars on the far wall: housing (dark steel) and the lit bar, box-filtered by the footprint
                    if (Pl.y > _Dims0.y - 0.06)
                    {
                        float idx = clamp(round((Pl.x - _Dims1.w) / _Dims1.z), 0.0, CdLampCount() - 1.0);
                        float du = abs(Pl.x - (_Dims1.w + _Dims1.z * idx)), dy = abs(Pw.y - _Dims2.x);
                        float fu = max(fp, 1e-4);
                        float housing = BwFilteredLine(du, _Dims2.y + 0.16, fu) * BwFilteredLine(dy, 0.22, fu);
                        float bar = BwFilteredLine(du, _Dims2.y, fu) * BwFilteredLine(dy, 0.1, fu);
                        col = lerp(col, PAL_ConcreteWet * 0.4 * diff, housing);
                        col += min(_ColdColor.rgb * _ColdRadiance, 3.0) * bar;
                    }
                }
                col = CdVeil(col, Pw);
                col = BwFogApply(col, Pw);
                col = AnyIsNaN(col) ? float3(0.0, 0.0, 0.0) : col;   // pal:ok (NaN guard)
                return half4(min(col, 1000.0), 1.0);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull Back
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex CdVert
            #pragma fragment CdDepthFrag
            #pragma multi_compile_instancing
            half4 CdDepthFrag(CdVaryings i) : SV_Target { return 0; }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R Cull Back
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex CdVert
            #pragma fragment CdDepthFrag
            #pragma multi_compile_instancing
            half CdDepthFrag(CdVaryings i) : SV_Target { return i.positionCS.z; }
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On Cull Back
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex CdVert
            #pragma fragment CdNormalFrag
            #pragma multi_compile_instancing
            void CdNormalFrag(CdVaryings i, out half4 outNormalWS : SV_Target0) { outNormalWS = half4(NormalizeNormalPerPixel(normalize(i.normalWS)), 0.0); }
            ENDHLSL
        }
    }
    Fallback Off
}
