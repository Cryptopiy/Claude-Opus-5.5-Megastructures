// NocRain - the rain pass of MASTER_PROMPT 14.7 H5 with D-005 (R2 section 1, R3 section 1), drawn by NocRainFeature at
// BeforeRenderingPostProcessing (after every transparent queue, before NocGrade's bloom and the one dither).
//   NocRainLight  1/8 resolution local light: 16 bilinear taps = the exact 8 x 8 box of the resolved HDR scene (RGBA16F)
//   NocRainBlur   3 x 3 bilinear tent at 1/8 (about a 1.5 deg radius: one lamp halo)
//   NocRainLayer  one full-screen triangle per camera-attached cylinder layer (radius 3, 6, 12, 20 m), additive (Blend One One)
//                 into the still-multisampled camera colour; the shader writes SV_Depth = the depth of the layer point and the
//                 hardware depth-tests it against every MSAA sample of the read-only camera depth (per-sample occlusion: no rain
//                 over the edge pixels of rails and wires; never _CameraDepthTexture, which holds the farthest sample).
// Layer maths (D-005 fixes of the H5 formulas): 21 whole cells around the circle (width 2 pi z / 21, height 0.8 z, no seam at
// south), drops fixed in a frame that falls at 9 m/s and drifts by X(t) = integral of 9 slope dt (integrated on the CPU in
// double: the spec's arc += tilt q slid drops sideways at up to 27 m/s), cell indices and scroll phases passed as integer + fraction,
// a per-column row stagger, 6 slots per cell with soft presence saturate((p_cell - r) / 0.06), p_cell = 0.9 rho (0.55 + 0.45
// vnoise) with a value noise periodic in azimuth (11 lattice cells per turn). Each drop is a capsule 0.30 m long along the fall
// direction (slope, -1), 0.012 m wide, drawn at least 2.2 px (1080p-relative) with the energy x true / drawn width; the pixel
// distance uses the analytic Jacobian of (arc, height) per pixel from the derivatives of the continuous ray (no fwidth across the
// atan2 cut or cell borders). Zenith: each layer fades by its projected streak length smoothstep(3, 8 px at 1080p).
// Brightness (exposed units): 0.35 (0.04 + 0.9 clamp(Lloc / 0.8, 0, 1.5)) x smoothstep(0, 0.15, Lloc), tinted 30 % toward the
// local light's hue, clamped at 0.8 (below the bloom threshold 0.9), divided by the exposure gain before it is added.
Shader "Megastructure/NocRain"
{
    Properties { }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Include/NocCommon.hlsl"

    TEXTURE2D(_NocRainSrc);
    TEXTURE2D(_NocRainLight);
    // sampler_LinearClamp comes from URP's GlobalSamplers.hlsl (Core.hlsl)
    float4 _NocRainSrcSize;      // light passes: source w, h, 1/w, 1/h; layer pass: zw = 1 / (8 x light texture size)
    float4 _NocRainScreen;       // camera colour w, h, 1/w, 1/h
    float4x4 _NocRainIVP;        // camera-relative inverse view-projection (GPU projection into a texture)
    float4x4 _NocRainVP;         // camera-relative view-projection
    float4 _NocRainA;            // x = 0.9 rho, y = streak slope, z = H / 1080, w = noise time x (t / 9 wrapped mod 11)
    float4 _NocRainB;            // x = noise time y (t / 13), y = fade lo px, z = fade hi px, w = exposure gain
    float4 _NocRainL;            // x = uFrac, y = vFrac, z = layer radius (m), w = iBase (0..20)
    float4 _NocRainM;            // x = jBase (mod 2^20), y = layer index, z = density noise on (1) / off (0), w = layer weight

    struct V2F { float4 positionCS : SV_POSITION; float2 ndc : TEXCOORD0; };

    V2F VertFull(uint id : SV_VertexID)
    {
        float2 p = float2((id << 1) & 2, id & 2) * 2.0 - 1.0;   // (-1,-1) (3,-1) (-1,3)
        V2F o;
        o.positionCS = float4(p, 0.5, 1.0);
        o.ndc = p;
        return o;
    }

    // ---------------------------------------------------------------- local light
    float4 FragLight(V2F i) : SV_Target
    {
        float2 base = floor(i.positionCS.xy) * NC_RainLightDiv;
        float3 acc = 0.0;
        [unroll] for (int b = 0; b < 4; b++)
            [unroll] for (int a = 0; a < 4; a++)
                acc += SAMPLE_TEXTURE2D_LOD(_NocRainSrc, sampler_LinearClamp, (base + float2(2 * a + 1, 2 * b + 1)) * _NocRainSrcSize.zw, 0).rgb;
        return float4(max(acc * (1.0 / 16.0), 0.0), 1.0);
    }

    float4 FragBlur(V2F i) : SV_Target
    {
        float2 uv = i.positionCS.xy * _NocRainSrcSize.zw;
        float3 acc = 0.0;
        [unroll] for (int b = -1; b <= 1; b++)
            [unroll] for (int a = -1; a <= 1; a++)
                acc += SAMPLE_TEXTURE2D_LOD(_NocRainSrc, sampler_LinearClamp, uv + float2(a, b) * _NocRainSrcSize.zw, 0).rgb;
        return float4(acc * (1.0 / 9.0), 1.0);
    }

    // ---------------------------------------------------------------- layers
    float3 NocRainHash(uint3 v) { return float3(NocPcg3d(v)) * (1.0 / 4294967296.0); }

    // value noise with period 11 in x (seam-free around the circle), y unbounded
    float NocRainNoise(float x, float y)
    {
        float2 f = floor(float2(x, y));
        float2 t = float2(x, y) - f;
        t = t * t * (3.0 - 2.0 * t);
        float xm = f.x - NC_RainNoiseCells * floor(f.x / NC_RainNoiseCells);
        uint x0 = (uint)xm, x1 = (x0 + 1u) % (uint)NC_RainNoiseCells;
        uint y0 = (uint)max(f.y, 0.0), y1 = y0 + 1u;
        float a = NocRainHash(uint3(x0, y0, 977u)).x, b = NocRainHash(uint3(x1, y0, 977u)).x;
        float c = NocRainHash(uint3(x0, y1, 977u)).x, d = NocRainHash(uint3(x1, y1, 977u)).x;
        return lerp(lerp(a, b, t.x), lerp(c, d, t.x), t.y);
    }

    float4 FragLayer(V2F i, out float depthOut : SV_Depth) : SV_Target
    {
        float4 hp = mul(_NocRainIVP, float4(i.ndc, 0.5, 1.0));
        float3 d = normalize(hp.xyz / hp.w);
        float3 dX = ddx(d), dY = ddy(d);
        float h2 = max(dot(d.xz, d.xz), 1e-8), h = sqrt(h2);

        // local light first: dark pixels exit before any drop maths (R2 1.4)
        float3 env = SAMPLE_TEXTURE2D_LOD(_NocRainLight, sampler_LinearClamp, i.positionCS.xy * _NocRainSrcSize.zw, 0).rgb * _NocRainB.w;
        float Lloc = dot(env, float3(0.2126, 0.7152, 0.0722));
        float B = NC_RainB0 * (NC_RainB1 + NC_RainB2 * clamp(Lloc / NC_RainLlocRef, 0.0, NC_RainLlocMax)) * smoothstep(0.0, NC_RainLumaGate, Lloc);
        clip(B * _NocRainA.x - 1e-5);

        float z = _NocRainL.z;
        float cw = z * (6.28318530718 / NC_RainCells), ch = z * NC_RainCellH;
        float theta = atan2(d.x, d.z), tanEl = d.y / max(h, 1e-4);
        float2 gTh = float2(d.z * dX.x - d.x * dX.z, d.z * dY.x - d.x * dY.z) / h2;    // d theta / d pixel, seam-free
        float2 gH = float2(dot(d.xz, dX.xz), dot(d.xz, dY.xz)) / h;
        float2 gTe = (float2(dX.y, dY.y) * h - d.y * gH) / h2;                          // d tan(el) / d pixel
        float2 gA = gTh * z, gB = gTe * z;                                               // metres per pixel of (arc, height)
        float det = gA.x * gB.y - gA.y * gB.x;
        det = abs(det) < 1e-12 ? 1e-12 : det;
        float2x2 Ji = float2x2(gB.y, -gA.y, -gB.x, gA.x) / det;                         // pixels per metre
        float2 ax = normalize(float2(_NocRainA.y, -1.0)), nr = float2(-ax.y, ax.x);
        float kLen = smoothstep(_NocRainB.y, _NocRainB.z, NC_RainDropLen * length(mul(Ji, ax)) / _NocRainA.z);
        float halfTrue = 0.5 * NC_RainDropWidth * length(mul(Ji, nr));
        float halfDraw = max(halfTrue, 0.5 * NC_RainMinPx * _NocRainA.z);
        float kE = halfTrue / halfDraw;
        clip(kLen - 1e-4);

        float U = theta * (NC_RainCells / 6.28318530718) - _NocRainL.x;
        float iF = floor(U), fu = U - iF;
        float im = iF - _NocRainL.w;
        uint ic = (uint)(im - NC_RainCells * floor(im / NC_RainCells));
        uint li = (uint)_NocRainM.y;
        float stagger = NocRainHash(uint3(ic, 41u, li)).x;
        float V = tanEl / NC_RainCellH + _NocRainL.y + stagger;
        float jF = floor(V), fv = V - jF;
        uint jc = ((uint)(int)(jF + _NocRainM.x)) & 0xFFFFFu;
        float nz = NocRainNoise((iF + 0.5 + _NocRainL.x) * (NC_RainNoiseCells / (float)NC_RainCells) + _NocRainA.w, _NocRainB.x);
        float pCell = _NocRainA.x * (_NocRainM.z > 0.5 ? NC_RainNoiseBase + NC_RainNoiseAmp * nz : 1.0);

        float2 p = float2(fu * cw, fv * ch);
        float acc = 0.0;
        [unroll] for (uint k = 0; k < (uint)NC_RainSlots; k++)
        {
            float3 r = NocRainHash(uint3(ic, jc, 8u * li + k));
            float ak = saturate((pCell - r.x) / NC_RainPresenceBand);
            float2 c = float2(NC_RainMarginH + (cw - 2.0 * NC_RainMarginH) * r.y, NC_RainMarginV + (ch - 2.0 * NC_RainMarginV) * r.z);
            float2 dl = p - c;
            float al = dot(dl, ax), ac = dot(dl, nr);
            float alc = sign(al) * max(abs(al) - 0.5 * NC_RainDropLen, 0.0);
            float dist = length(mul(Ji, ac * nr + alc * ax));
            acc += saturate(halfDraw + 0.5 - dist) * ak * (0.7 + 0.3 * frac(r.x * 7.31));
        }
        float cov = acc * kE * kLen * _NocRainM.w;
        clip(cov - 1e-4);

        float3 tint = min(lerp(float3(1.0, 1.0, 1.0), env / max(Lloc, 1e-4), NC_RainTint), (float3)NC_RainTintMax);
        float3 add = min(B * tint * cov, (float3)NC_RainClamp) / max(_NocRainB.w, 1e-4);     // back to pre-exposure units

        float D = z / max(h, 1e-4);
        float4 cs = mul(_NocRainVP, float4(d * D, 1.0));
        depthOut = cs.z / cs.w;
        return float4(add, 0.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "NocRainLight"
            ZTest Always ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex VertFull
            #pragma fragment FragLight
            ENDHLSL
        }
        Pass
        {
            Name "NocRainBlur"
            ZTest Always ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex VertFull
            #pragma fragment FragBlur
            ENDHLSL
        }
        Pass
        {
            Name "NocRainLayer"
            ZTest LEqual ZWrite Off Cull Off
            Blend One One
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex VertFull
            #pragma fragment FragLayer
            ENDHLSL
        }
    }
    Fallback Off
}
