// NocNeon - neon signs (MASTER_PROMPT 14.2.5 NocNeon, 14.7 H2, D-008): opaque emissive quads, Abyss InstancedBatch instances
// (<= 511 per draw, explicit bounds, per-camera submission through NocCameraBatches). Instance = TRS(centre, rotation, (W, H, 1))
// of the unit quad (-0.5..0.5, facing local -Z, drawn double-sided); colour, glyph seed and breathing phase come from a hash of
// the position (never UNITY_INSTANCING_BUFFER). Glyphs: the sign is a column of glyph cells (aspect 14 x 18, the blade cells of
// 14.5.2); each cell holds 3-5 hashed capsule strokes on a 4 x 6 lattice, stroke width 0.055 W, anti-aliased with the pixel
// footprint and drawn at least 1.75 px wide with the energy kept (true / drawn). Below 3 px of stroke footprint the cell fades
// (1.5..3 px; 6 px before TQ-08) to its exact mean coverage x the Neutral curve's ratio at the stroke peak (flat colour, no
// shimmer, the tone-mapped mean of the strokes); the whole quad is drawn at least 2.2 px in both axes (clip-space
// inflation, foreshortening included) with energy x (true / drawn) per axis. Breathing <= 0.2 Hz, +-8 % (frequencies on a 1/3600 Hz
// lattice so the wrapped scene clock never jumps). Emission = token x 1.6 (LOOK_V2 2.3 signs); NocFog then NocFloor last.
// Colours (LOOK_V2 R6 / R7, section 10): hash weights SodiumAmber 0.28 x k_a, WinNeutral 0.24, NeonCyan 0.18, AviationRed 0.14,
// SignWhite 0.12 + 0.28 (1 - k_a), NeonMagenta 0.04, NeonViolet 0 (k_a per item through NocAmberMix: no pop); fixed _Palette 0
// magenta, 1 cyan, 2 amber (x k_a), 3 = the retired violet slot -> WinNeutral, 4 SignWhite, 5 WinNeutral, 6 AviationRed,
// 7 WinTungsten. The dark panel face is at most 0.015 x the colour desaturated 60 % (lerp toward WinNeutral).
Shader "Megastructure/NocNeon"
{
    Properties
    {
        _Gain ("Emission gain (x token)", Float) = 1.6
        _Palette ("Colour (-1 hash; 0 magenta, 1 cyan, 2 amber, 3 retired violet = WinNeutral, 4 SignWhite, 5 WinNeutral, 6 red, 7 tungsten)", Float) = -1
        _Panel ("Panel glow (x colour desaturated 60 %, capped 0.015)", Float) = 0.015
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Include/NocAtmosphere.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _Gain;
        float _Palette;
        float _Panel;
    CBUFFER_END

    struct Attributes
    {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 st : TEXCOORD0;          // true sign coordinates in metres (0..W, 0..H)
        float3 posWS : TEXCOORD1;
        nointerpolation float4 sign : TEXCOORD2;    // x W, y H, z energy (true / drawn, both axes), w seed (as float bits)
    };

    // projected pixel length of the world vector axisFull at the centre: exact projection of both ends, the half-length clamped so
    // both ends keep w >= 0.25 w_centre (a sign the camera passes beside never inflates across the screen)
    float NocProjPx(float3 axisFull, float4 cC, float2 scr)
    {
        float4 cD = mul(UNITY_MATRIX_VP, float4(axisFull, 0.0));
        float h = 0.5;
        if (abs(cD.w) > 1e-6) h = min(h, 0.75 * cC.w / abs(cD.w));
        float4 a = cC - cD * h, b = cC + cD * h;
        return length((b.xy / b.w - a.xy / a.w) * 0.5 * scr) * (0.5 / max(h, 1e-6));
    }

    Varyings Vert(Attributes i)
    {
        UNITY_SETUP_INSTANCE_ID(i);
        Varyings o = (Varyings)0;
        float4x4 M = UNITY_MATRIX_M;
        float3 R = float3(M._m00, M._m10, M._m20), U = float3(M._m01, M._m11, M._m21), c = float3(M._m03, M._m13, M._m23);
        float W = max(length(R), 1e-4), H = max(length(U), 1e-4);
        float2 scr = max(_ScreenParams.xy, 1.0);
        float4 cC = TransformWorldToHClip(c);
        cC.w = max(cC.w, 1e-3);
        float wPx = NocProjPx(R, cC, scr), hPx = NocProjPx(U, cC, scr);
        float kw = max(1.0, NC_NeonClampPx / max(wPx, 1e-4)), kh = max(1.0, NC_NeonClampPx / max(hPx, 1e-4));
        float3 p = c + R * (i.positionOS.x * kw) + U * (i.positionOS.y * kh);
        o.positionCS = TransformWorldToHClip(p);
        o.posWS = p;
        o.st = float2((i.positionOS.x * kw + 0.5) * W, (i.positionOS.y * kh + 0.5) * H);
        uint seed = NocPcg3d(asuint(round(c * 10.0))).x;
        o.sign = float4(W, H, 1.0 / (kw * kh), asfloat(seed & 0x7FFFFFu | 0x3F800000u));
        return o;
    }

    float NocCapsule(float2 p, float2 a, float2 b)
    {
        float2 pa = p - a, ba = b - a;
        float h = saturate(dot(pa, ba) / max(dot(ba, ba), 1e-8));
        return length(pa - ba * h);
    }

    float3 NocNeonColour(uint seed)
    {
        float ua = NocU01(NocPcg(seed ^ 0x416D6272u));                  // the item's position in the R6 amber band
        float u = NocU01(NocPcg(seed ^ 0x436F6C72u));
        // the hash slot from the cumulative weights: amber 0.28 (x k_a, the rest SignWhite) -> slot 2, neutral 0.24 -> 5, cyan 0.18 -> 1,
        // red 0.14 -> 6, white 0.12 -> 4, magenta 0.04 -> 0 (one expression and one return: no X4000 on the early-return chain)
        float c1 = NC_NeonWAmber, c2 = c1 + NC_NeonWNeutral, c3 = c2 + NC_NeonWCyan, c4 = c3 + NC_NeonWRed, c5 = c4 + NC_NeonWWhite;
        float hk = u < c1 ? 2.0 : (u < c2 ? 5.0 : (u < c3 ? 1.0 : (u < c4 ? 6.0 : (u < c5 ? 4.0 : 0.0))));
        float k = _Palette >= 0.0 ? _Palette : hk;
        float3 amber = lerp(PAL_SignWhite, PAL_SodiumAmber, NocAmberMix(ua, _NocAmber.x));
        // slots: 0 magenta, 1 cyan, 2 amber (x k_a), 3 the retired violet slot -> WinNeutral (R7), 4 SignWhite, 5 WinNeutral, 6 red, 7 tungsten
        return k < 0.5 ? PAL_NeonMagenta : (k < 1.5 ? PAL_NeonCyan : (k < 2.5 ? amber : (k < 3.5 ? PAL_WinNeutral
             : (k < 4.5 ? PAL_SignWhite : (k < 5.5 ? PAL_WinNeutral : (k < 6.5 ? PAL_AviationRed : PAL_WinTungsten))))));
    }

    half4 Frag(Varyings i) : SV_Target
    {
        float W = i.sign.x, H = i.sign.y, energy = i.sign.z;
        uint seed = asuint(i.sign.w) & 0x7FFFFFu;
        float2 st = i.st;
        float fp = max(max(length(float2(ddx(st.x), ddy(st.x))), length(float2(ddx(st.y), ddy(st.y)))), 1e-5);   // m per px

        // glyph cells: a column of cells of aspect 14 x 18
        float n = max(1.0, round(H / (W * (18.0 / 14.0))));
        float cellH = H / n;
        float ci = clamp(floor(st.y / cellH), 0.0, n - 1.0);
        float2 lp = float2(st.x, st.y - ci * cellH);
        float padX = 0.16 * W, padY = 0.10 * cellH;
        float2 lat = float2((W - 2.0 * padX) / 3.0, (cellH - 2.0 * padY) / 5.0);
        float r = 0.5 * NC_NeonStroke * W;
        float rD = max(r, 0.5 * NC_NeonMinPx * fp);
        uint hc = NocPcg3d(uint3((uint)ci, seed, 0x476C7068u)).x;
        int count = 3 + (int)(hc % 3u);
        float dmin = 1e9, area = 0.0;
        [loop] for (int k = 0; k < count; k++)
        {
            uint hs = NocPcg3d(uint3((uint)ci, seed, (uint)k + 17u)).y;
            int a0 = (int)(hs & 3u), b0 = (int)((hs >> 2) % 6u);
            int dirK = (int)((hs >> 5) & 3u), len = 1 + (int)((hs >> 7) % 3u);
            int2 dv = dirK == 0 ? int2(1, 0) : (dirK == 1 ? int2(0, 1) : (dirK == 2 ? int2(1, 1) : int2(1, -1)));
            int2 e = clamp(int2(a0, b0) + dv * len, int2(0, 0), int2(3, 5));
            float2 pa = float2(padX, padY) + float2(a0, b0) * lat, pb = float2(padX, padY) + float2(e) * lat;
            dmin = min(dmin, NocCapsule(lp, pa, pb));
            area += 2.0 * r * length(pb - pa) + 3.14159265 * r * r;
        }
        float flatCov = saturate(area / max(W * cellH, 1e-4));
        float cov = saturate((rD - dmin) / fp + 0.5) * (r / rD);
        float w = smoothstep(0.5 * NC_NeonFlatPx, NC_NeonFlatPx, 2.0 * r / fp);

        float u1 = NocU01(NocPcg(seed ^ 0x42726561u)), u2 = NocU01(NocPcg(seed ^ 0x50686173u));
        float f = round(lerp(NC_NeonBreathHz0, NC_NeonBreathHz1, u1) * 3600.0) / 3600.0;     // whole cycles per wrapped hour
        float breath = 1.0 + NC_NeonBreathAmp * sin(6.28318530718 * (f * _NocTime.x + u2));
        float3 colour = NocNeonColour(seed);
        // the flat regime stands for strokes that the grade's Neutral curve compresses (start 0.76): it carries their tone-mapped
        // mean, not the mean of their HDR value (TQ-08: flat blades read 1.8-2.0 levels above the -capture-ss 2 reference)
        float pk = max(colour.r, max(colour.g, colour.b)) * _Gain * breath;
        float d0 = 1.0 - NC_NeonToneStart;
        float tr = pk > NC_NeonToneStart ? (1.0 - d0 * d0 / (pk + d0 - NC_NeonToneStart)) / pk : 1.0;
        float glyph = lerp(flatCov * tr, cov, w);
        if (energy < 0.999) glyph = flatCov * tr;           // drawn larger than true: unresolved, flat colour
        float3 panel = lerp(colour, PAL_WinNeutral, NC_NeonPanelDesat) * (min(_Panel, NC_NeonPanel) * energy);   // dark panel face
        float3 c = colour * (_Gain * energy * breath * glyph) + panel;
        c = NocFog(c, i.posWS);
        return half4(NocFloor(c), 1.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Pass
        {
            Name "Neon"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On ZTest LEqual Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment FragDepth
            #pragma multi_compile_instancing
            half FragDepth(Varyings i) : SV_Target { return i.positionCS.z; }
            ENDHLSL
        }
    }
    Fallback Off
}
