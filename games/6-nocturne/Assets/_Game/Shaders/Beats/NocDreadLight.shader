// NocDreadLight - the point lights of package DR (docs/DREAD.md 2.1 the hull's belly rows and engine glows, 3.1 the cold lights on
// the shadow's back; owner DR). A mesh of quads whose four vertices sit at the light's centre (object space, DreadLights.cs): it
// moves with its object's transform, one draw per object, no instancing buffer. uv1 = (size m: lamp diameter or halo sigma, gain,
// colour 0 AviationRed / 1 SignWhite / 2 WinCold / 3 LampWarm, profile 0 lamp / 1 Gaussian halo / 2 lit cab: a 3 x 3.5 box, a dim
// frame and a ceiling-lit glass, edges filtered by fwidth (fix wave DR-07)). The quad is built in clip space
// exactly like NocStreak kinds 3 / 5 (14.2.5): at least 2.2 px (NC_StreakMinPx), energy x (true / drawn)^2, the lamp profile
// (1 - x^2)(1 - y^2) x 9/4 (mean 1), the halo a Gaussian of peak 1 over +-3 sigma lowered by its edge value (no rim); the centre is
// pulled toward the camera by its drawn radius (never clipped by the hull it sits on). Fogged as light: x T x (1 - cloud mix), no
// floor. Steady: no pulse, no flash (D-010). _Gain (material) is the object's fade (DreadDriver). Movers (DREAD 2.2, the lift
// cabins): uv2 = (speed m/s, shaft length L m, phase, direction +1 up / -1 down; 0 = static); the light rides y0 + L p (down:
// y0 + L (1 - p)), p = frac(phase + v t / L), t = the traffic clock _NocTraffic.x (wrapped mod 65 536 s, v quantised: exact wraps),
// with its radiance ramped to 0 over the last 2 % at both ends (the wrap is hidden in the deck and the deep haze). Queue
// Transparent+10, Blend One One, ZWrite Off, ZTest LEqual, Cull Off. No texture, no keyword.
Shader "Megastructure/NocDreadLight"
{
    Properties
    {
        _Gain ("Radiance gain (fade x 1)", Float) = 1.0
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "../Include/NocAtmosphere.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _Gain;
    CBUFFER_END

    float4 _NocTraffic;   // x = traffic time wrapped mod 65 536 s (NocTraffic), the movers' clock

    struct Attributes
    {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
        float4 uv1 : TEXCOORD1;
        float4 uv2 : TEXCOORD2;
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 q : TEXCOORD0;
        float4 col : TEXCOORD1;      // rgb radiance x energy x T x (1 - cloud mix), a = the profile (0 lamp, 1 halo, 2 cab)
    };

    float3 DreadColour(float k)
    {
        return k < 0.5 ? PAL_AviationRed : (k < 1.5 ? PAL_SignWhite : (k < 2.5 ? PAL_WinCold : (k < 3.5 ? PAL_LampWarm : PAL_WinWarm)));
    }

    Varyings Vert(Attributes i)
    {
        Varyings o = (Varyings)0;
        float2 scr = max(_ScreenParams.xy, 1.0);
        float3 pos = i.positionOS.xyz;
        float ramp = 1.0;
        if (abs(i.uv2.w) > 0.5)
        {
            float L = max(i.uv2.y, 1.0);
            float p = frac(i.uv2.z + i.uv2.x * _NocTraffic.x / L);
            ramp = smoothstep(0.0, 0.02, p) * (1.0 - smoothstep(0.98, 1.0, p));
            pos.y += L * (i.uv2.w > 0.0 ? p : 1.0 - p);
        }
        float3 centre = TransformObjectToWorld(pos);
        bool halo = i.uv1.w > 0.5 && i.uv1.w < 1.5;
        float size = max(i.uv1.x, 1e-3);
        float trueW = halo ? 2.0 * NC_HaloExtent * size : size;
        float3 toCam = _WorldSpaceCameraPos.xyz - centre;
        float dist = max(length(toCam), 1e-3);
        float fp = dist * 2.0 / max(abs(UNITY_MATRIX_P[1][1]) * scr.y, 1e-4);      // metres per pixel at the light
        float wPx = trueW / fp;
        float drawW = max(wPx, NC_StreakMinPx);
        centre += toCam / dist * min(0.5 * max(trueW, NC_StreakMinPx * fp), 0.5 * dist);
        float4 cC = TransformWorldToHClip(centre);
        float w = max(cC.w, 1e-4);
        float2 pc = cC.xy / w * 0.5 * scr;
        float2 q = i.uv * 2.0 - 1.0;
        float2 px = pc + q * (0.5 * drawW);
        o.positionCS = float4(px * (2.0 / scr) * w, cC.z, w);
        if (cC.w < 0.1) o.positionCS = float4(0.0, 0.0, -1.0, 1.0);                  // behind or at the camera: clipped away
        o.q = q;
        float energy = (wPx / drawW) * (wPx / drawW);
        float T = NocFogT(centre);
        float cloud = 1.0 - smoothstep(NC_CloudMixY0, NC_CloudMixY1, centre.y);
        o.col = float4(DreadColour(i.uv1.z) * (max(_Gain, 0.0) * i.uv1.y * energy * T * cloud * ramp), i.uv1.w);
        return o;
    }

    half4 Frag(Varyings i) : SV_Target
    {
        float prof;
        float2 fw = max(fwidth(i.q), 1e-4);      // derivatives before the branches
        if (i.col.a > 1.5)
        {
            // the lit cab (3 x 3.5 m: half-extents 0.79 x 0.92 of the quad), its dim frame and the glass (0.62 x 0.78), brighter
            // toward the ceiling; edges filtered over one pixel; x 2.1: mean ~1 over the quad (the 14.2.5 energy rule)
            float2 aq = abs(i.q);
            float box = (1.0 - smoothstep(0.79 - fw.x, 0.79 + fw.x, aq.x)) * (1.0 - smoothstep(0.92 - fw.y, 0.92 + fw.y, aq.y));
            float glass = (1.0 - smoothstep(0.62 - fw.x, 0.62 + fw.x, aq.x)) * (1.0 - smoothstep(0.78 - fw.y, 0.78 + fw.y, aq.y));
            float ceil = 0.75 + 0.5 * saturate(0.5 + 0.5 * i.q.y);
            prof = 2.1 * box * lerp(0.22, ceil, glass);
        }
        else if (i.col.a > 0.5)
        {
            float e0 = exp(-0.5 * NC_HaloExtent * NC_HaloExtent);
            float2 g = saturate((exp(-0.5 * NC_HaloExtent * NC_HaloExtent * i.q * i.q) - e0) / (1.0 - e0));
            prof = g.x * g.y;
        }
        else
        {
            float2 a = saturate(1.0 - i.q * i.q);
            prof = a.x * a.y * 2.25;
        }
        return half4(i.col.rgb * prof, 0.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent+10" "IgnoreProjector" = "True" }
        Pass
        {
            Name "DreadLight"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off ZTest LEqual Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
    Fallback Off
}
