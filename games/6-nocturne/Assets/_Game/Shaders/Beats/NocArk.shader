// NocArk - THE ARK's hull (D-043, Logs/ark/ARK.md; owner DR), on the Noc lit template (NocLitCore / NocLitPass), NocHull's
// lighting and fog with the ship's own lights drawn procedurally (no texture). Mesh (HullBuilder): uv0 = (local z from the bow,
// local y = world y), uv1.x = local x, vertex R = AO, vertex B = kind (0 hull plate, 100/255 spine and pods, 200/255 gondola).
//   - plate: dark painted steel (DreadBuilder's linear 0.05) lit by the three ambient fields it hangs in (the lit haze and city
//     below, the faces' windows beside, the deck above), seams every 12 m along and 16 m across with a tone per plate;
//   - decks: on every face that is not a floor or a ceiling (|n.y| < 0.55), windows on the 4 m floor pitch (60 decks from the keel
//     to the back) every 3.2 m along the body, lit by sectors (16 windows x 6 decks: activity 0.02..0.40, a few sectors bright, most
//     nearly dark: decks nobody is on), warm / cold / white; on the down-facing treads and the keel plate, portholes in rows along
//     the body (4.5 m along, 9 m across); gondolas carry lit cabins (activity 0.35..0.85);
//   - filtering (Part 10: no shimmer, no pops): each pulse is box-filtered over its own pixel footprint (fwidth of its coordinate),
//     the per-window lit state fades to its sector's mean once a window is under ~3 px and the sectors to the global mean once a
//     sector is under ~4 px, so the far ship is a smooth mean glow of exactly the near pattern's energy;
//   - keel hangar bays: lit 18 x 80 m bays every 400 m on the keel plate (warm, mullioned); the bow's command band (a lit glass
//     ribbon across the stem wall at y 437-447);
//   - then the cloud mix (the upper steps melt into the deck from 425 m to 560 m, billows +-45 m) and the fog toward NocSkyColor(ray),
//     then AT's NocArkAirTo (the air glow and the searchlight cones in front of the hull).
// Opaque, ZWrite On, Cull Off, forward + DepthOnly + DepthNormals. Steady: nothing blinks (D-010).
Shader "Megastructure/NocArk"
{
    Properties
    {
        _BaseColor ("Hull paint (sRGB colour; DreadBuilder sets the linear 0.05)", Color) = (0.25, 0.25, 0.26, 1)
        _BellyLight ("Light from below: the city and the lit haze (linear radiance)", Vector) = (0.15, 0.112, 0.075, 1)
        _SideLight ("Light from the sides: the faces' windows (linear radiance)", Vector) = (0.06, 0.058, 0.055, 1)
        _TopLight ("Light from above: the deck (linear radiance)", Vector) = (0.04, 0.038, 0.034, 1)
        _SeamDepth ("Plate seam darkening", Range(0, 1)) = 0.35
        _WinGain ("Deck window radiance (x palette, linear)", Float) = 0.6
        _PortGain ("Porthole radiance (x palette, linear)", Float) = 0.7
        _CabGain ("Gondola cabin radiance (x palette, linear)", Float) = 0.9
        _BayGain ("Keel hangar bay radiance (x LampWarm, linear)", Float) = 0.16
        _BandGain ("Bow command band radiance (linear)", Float) = 0.26
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
    }

    HLSLINCLUDE
    #define NOC_HAS_FORWARD_HOOK
    #define NOC_NO_FOG
    #include "../Include/NocLitCore.hlsl"
    #include "NocDread.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float4 _BellyLight;
        float4 _SideLight;
        float4 _TopLight;
        float _SeamDepth;
        float _WinGain;
        float _PortGain;
        float _CabGain;
        float _BayGain;
        float _BandGain;
        float _Cull;
    CBUFFER_END

    // integral of a pulse train (opening w centred in each period p) from 0 to x
    float ArkPulseInt(float x, float p, float w)
    {
        float c = floor(x / p);
        return c * w + clamp(x - c * p - 0.5 * (p - w), 0.0, w);
    }
    // the pulse train box-filtered over the footprint fw (exact mean of the pulse over the pixel)
    float ArkPulse(float x, float fw, float p, float w)
    {
        fw = max(fw, 1e-3);
        return (ArkPulseInt(x + 0.5 * fw, p, w) - ArkPulseInt(x - 0.5 * fw, p, w)) / fw;
    }

    static const float3 ArkMeanCol = 0.68 * PAL_WinWarm + 0.22 * PAL_WinCold + 0.10 * PAL_SignWhite;

    // windows on a (pu x pv) grid with openings (wu x wv), sectors of (16 x 6) cells with activity lo..hi (hash^4: cycle a02, most
    // sectors nearly dark and a few lit districts, so the belly reads as a built structure, not a starfield); seed per family
    float3 ArkWindows(float u, float v, float fu, float fv, float pu, float pv, float wu, float wv, int seed, float lo, float hi, int run)
    {
        float cov = ArkPulse(u, fu, pu, wu) * ArkPulse(v, fv, pv, wv);
        int2 cell = int2(floor(u / pu), floor(v / pv));
        int2 sec = int2(floor(u / (16.0 * pu)), floor(v / (6.0 * pv)));
        float hs = NocU01(NocPcg2(sec.x + 7919 * seed, sec.y + 104729));
        float h2 = hs * hs;
        float act = lo + (hi - lo) * h2 * h2;
        // lit in runs of 'run' cells along u (rooms, corridors: cycle a01's single dots read as a starfield)
        int rc = (int)floor((float)cell.x / (float)run);
        float hc = NocU01(NocPcg2(rc + 31337 * seed, cell.y + 4093));
        float hk = NocU01(NocPcg2(rc - 911, cell.y + 77 + 613 * seed));
        float lit = hc < act ? 1.0 : 0.0;
        float3 col = hk < 0.68 ? PAL_WinWarm : (hk < 0.90 ? PAL_WinCold : PAL_SignWhite);
        float bright = 0.55 + 0.45 * frac(hk * 7.13);
        float3 cellE = lit * col * bright;
        float3 secE = act * ArkMeanCol * 0.775;
        float m = smoothstep(0.25, 0.6, max(fu / pu, fv / pv));
        float3 E = lerp(cellE, secE, m);
        float ms = smoothstep(0.12, 0.3, max(fu / (16.0 * pu), fv / (6.0 * pv)));
        E = lerp(E, (lo + (hi - lo) / 5.0) * ArkMeanCol * 0.775, ms);
        return E * cov;
    }

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        float lx = f.uv.z;
        float seamZ = NocFilteredGrid1(f.uv.x, 12.0, 0.35, f.footprint, 4.0);
        float seamX = NocFilteredGrid1(lx, 16.0, 0.35, f.footprint, 4.0);
        float h = NocU01(NocPcg2((int)floor(f.uv.x / 12.0), (int)floor(lx / 16.0) + 4096));
        float tone = 1.0 + 0.36 * (h - 0.5) * saturate(1.0 - f.footprint * 4.0 / 12.0);
        s.albedo = _BaseColor.rgb * (1.0 - _SeamDepth * max(seamZ, seamX)) * tone;
        s.smoothness = 0.25;
        s.occlusion = lerp(0.55, 1.0, f.color.r);
        s.emission = 0.0;
    }

    half3 NocForwardHook(NocFragData f, SurfaceData s, InputData inputData)
    {
        float3 n = f.normalWS;
        // derivatives first (before any branch)
        float fz = fwidth(f.uv.x), fy = fwidth(f.uv.y), fx = fwidth(f.uv.z);
        float down = saturate(-n.y), up = saturate(n.y), side = 1.0 - abs(n.y);
        float3 E = _BellyLight.rgb * down + _TopLight.rgb * up + _SideLight.rgb * side;
        float3 c = s.albedo * E * s.occlusion;
        float kind = f.color.b;
        float3 em = 0.0;
        [branch] if (kind < 0.2)
        {
            if (abs(n.y) < 0.55)
            {
                bool across = abs(n.z) > abs(n.x);
                float u = across ? f.uv.z : f.uv.x, fu = across ? fx : fz;
                em = _WinGain * ArkWindows(u, f.uv.y, fu, fy, 3.2, 4.0, 1.9, 1.8, across ? 3 : 1, 0.02, 0.60, 3);
                // the bow's command band (cycle a04): one continuous lit ribbon of glass across the stem wall at y 437-447, mullions
                // every 3 m: the ship's signature line, readable from 3 km out
                [branch] if (n.z < -0.9 && f.uv.x < 1.0)
                {
                    float band = ArkPulse(f.uv.y - 442.0 + 50.0, fy, 100.0, 10.0);
                    float mull = 1.0 - 0.5 * NocFilteredGrid1(f.uv.z, 3.0, 0.35, f.footprint, 4.0);
                    // cycle a05: one even bar read flat; a tone per 12 m bay (0.55..1), faded to its mean 0.775 once a bay is < 4 px
                    float pb = NocU01(NocPcg2((int)floor(f.uv.z / 12.0) + 811, 4471));
                    float tone = lerp(0.775, 0.55 + 0.45 * pb, saturate(1.0 - fx * 4.0 / 12.0));
                    em += _BandGain * lerp(PAL_LampWarm, PAL_SignWhite, 0.35) * (band * mull * tone);
                }
            }
            else if (n.y < -0.55)
            {
                em = _PortGain * ArkWindows(f.uv.x, f.uv.z, fz, fx, 4.5, 9.0, 2.2, 1.4, 2, 0.005, 0.50, 7);
                // the keel plate's hangar bays (cycle a02): two 18 x 80 m lit bays every 400 m between the spine and the pods (z 740,
                // 1 140, ... 4 340), 70 % lit, warm, a 4 m mullion grid inside: big rhythmic glows that read from 3 km and from below
                float ax = abs(f.uv.z);
                [branch] if (n.y < -0.9 && ax < 66.0 && f.uv.y < 331.5 && f.uv.x > 600.0 && f.uv.x < 4500.0)
                {
                    float bz = ArkPulse(f.uv.x - 540.0, fz, 400.0, 80.0);
                    float bx = ArkPulse(ax - 31.0 + 50.0, fx, 100.0, 18.0);
                    int bi = (int)floor((f.uv.x - 540.0) / 400.0);
                    float on = NocU01(NocPcg2(bi * 2 + (f.uv.z > 0.0 ? 1 : 0), 7717)) < 0.7 ? 1.0 : 0.0;
                    float mull = 1.0 - 0.45 * max(NocFilteredGrid1(f.uv.x, 4.0, 0.4, f.footprint, 4.0), NocFilteredGrid1(ax, 4.5, 0.4, f.footprint, 4.0));
                    em += _BayGain * PAL_LampWarm * (bz * bx * on * mull);
                }
            }
        }
        else if (kind > 0.6)
        {
            if (abs(n.y) < 0.55)
            {
                bool across = abs(n.z) > abs(n.x);
                float u = across ? f.uv.z : f.uv.x, fu = across ? fx : fz;
                em = _CabGain * ArkWindows(u, f.uv.y, fu, fy, 3.0, 3.6, 2.2, 2.0, 5, 0.35, 0.85, 2);
            }
        }
        c += em;
        float3 P = f.positionWS;
        float3 V = P - _WorldSpaceCameraPos.xyz;
        float T = NocFogT(P);
        // perf (DR_ark_perf, s 580-600 +2.3 ms with AT's air): where the haze in front of the hull is thin (T > 0.95) and the melt
        // cannot reach (y < 380), its colour comes from the sky blurred over 0.02 rad (the deck's 110 / 40 m octaves skipped: an error
        // of (1 - T) x the fine billows' share, under one code); blended to the sharp sky over T 0.95..0.85 (no contour), sharp beyond
        float wS = P.y > 380.0 ? 1.0 : saturate((0.95 - T) / 0.1);
        float3 sky;
        [branch] if (wS >= 1.0) sky = NocSkyColor(V);
        else
        {
            sky = NocSkyColorBlur(V, 0.02);
            [branch] if (wS > 0.0) sky = lerp(sky, NocSkyColor(V), wS);
        }
        sky *= NocDreadVeilRay(P);
        [branch] if (P.y > 425.0 - 45.0)      // perf (DR_ark_perf): the cloud noise only where the melt can reach (y > 380)
        {
            float fm = NocDeckForm(NocCloudNoise(P.xz, f.footprint));
            // cycle a02: a flat top edge at 455..530 made the Ark a closed object hanging in the rift; it now melts into the deck
            // from 425 m to 560 m with +-45 m billows, so its upper steps go on up into the cloud and its height is never seen whole
            c = lerp(c, sky, smoothstep(425.0, 560.0, P.y + 45.0 * fm));
        }
        [branch] if (T < 0.99995) c = lerp(sky, c, T);
        // AT (D-043): the air glow round the Ark and its searchlight cones in front of the hull (0 while no Ark is lit)
        c += NocArkAirTo(P);
        return (half3)c;
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On ZTest LEqual Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #define NOC_PASS_FORWARD
            #include "../Include/NocLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #define NOC_PASS_DEPTH
            #include "../Include/NocLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #define NOC_PASS_DEPTHNORMALS
            #include "../Include/NocLitPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
