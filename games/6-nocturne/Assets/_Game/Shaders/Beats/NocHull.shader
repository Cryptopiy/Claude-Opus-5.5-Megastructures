// NocHull - the hull over the rift (docs/DREAD.md 2.1; owner DR), on the Noc lit template (NocLitCore / NocLitPass). It reads only
// as a silhouette against the city-lit deck (its own cloud mix starts at the deck base, y 455, not at the towers' 380): a dark painted-steel albedo (0.035) lit by the three ambient fields it hangs in - the
// lit haze of the abyss under its belly (n.y < 0), the faces' windows on its flanks and the deck glow above - with no point lights
// and no specular (a 1 800 m body 1-3 km away), plate seams every 12 m along the body (uv0.x = local z, footprint-filtered) and the
// vertex AO of the rib grooves and engine throats (vertex R); vertex G masks _EmissionColor (the falling panel's backlit face, 3.2;
// 0 on the hull). Own fog (NOC_NO_FOG + NOC_HAS_FORWARD_HOOK): the cloud mix and then the fog toward NocSkyColor(ray) (the deck
// where the ray meets it, darkened by AT's hull occluder; x DR's veil factor, off), so the hull stays darker than the shadowed deck
// around it at every distance and, parked inside the deck, is exactly the sky behind it (fix wave C01); NocFloor
// last (the template). Opaque, ZWrite On, Cull Off (a closed body; one-sided safety), forward + DepthOnly + DepthNormals.
Shader "Megastructure/NocHull"
{
    Properties
    {
        _BaseColor ("Hull paint (sRGB colour; DreadBuilder sets the hull's linear 0.05)", Color) = (0.035, 0.036, 0.038, 1)
        // the ambient fields are Vectors (raw linear: a Color property is gamma-converted in this linear project, r01-r06 drew
        // the hull's 0.06 as 0.005); the defaults are those converted values, so the panel and the gantries keep their r03 look
        _BellyLight ("Light from below: the city and the lit haze (linear radiance)", Vector) = (0.0049, 0.0035, 0.0023, 1)
        _SideLight ("Light from the sides: the faces' windows (linear radiance)", Vector) = (0.0023, 0.0022, 0.0022, 1)
        _TopLight ("Light from above: the deck (linear radiance)", Vector) = (0.0015, 0.0015, 0.0013, 1)
        _SeamDepth ("Plate seam darkening", Range(0, 1)) = 0.35
        _PlateGrid ("Plate seams across, every m of the local x (0 = off; the hull 16)", Float) = 0
        _PlateOrigin ("World x of the local x origin (the hull's axis 515.3)", Float) = 0
        _EmissionColor ("Emission where vertex G = 1 (linear HDR; the falling panel's backlit face)", Vector) = (0, 0, 0, 0)
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
        float4 _EmissionColor;
        float _SeamDepth;
        float _PlateGrid;
        float _PlateOrigin;
        float _Cull;
    CBUFFER_END

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        float seam = NocFilteredGrid1(f.uv.x, 12.0, 0.35, f.footprint, 4.0);
        float ao = lerp(0.55, 1.0, f.color.r);
        // fix wave C01: right under the belly (430 m) r04 read as a black void with star points (the y 300-400 traffic in front of
        // it). Plates: seams across every _PlateGrid m of the local x too, and a tone per 12 x 16 m plate (+-18 %, faded out once a
        // plate is under ~4 px), so the lights sit on a built surface; the belly's light from the city below is 2.5 x r03's
        float lx = f.positionWS.x - _PlateOrigin;
        float seamX = NocFilteredGrid1(lx, max(_PlateGrid, 1.0), 0.35, f.footprint, 4.0);
        float tone = 1.0;
        [branch] if (_PlateGrid > 0.0)
        {
            float h = NocU01(NocPcg2((int)floor(f.uv.x / 12.0), (int)floor(lx / _PlateGrid) + 4096));
            tone = 1.0 + 0.36 * (h - 0.5) * saturate(1.0 - f.footprint * 4.0 / 12.0);
        }
        else seamX = 0.0;
        s.albedo = _BaseColor.rgb * (1.0 - _SeamDepth * max(seam, seamX)) * tone;
        s.smoothness = 0.25;
        s.occlusion = ao;
        s.emission = 0.0;
    }

    half3 NocForwardHook(NocFragData f, SurfaceData s, InputData inputData)
    {
        float3 n = f.normalWS;
        float down = saturate(-n.y), up = saturate(n.y), side = 1.0 - abs(n.y);
        float3 E = _BellyLight.rgb * down + _TopLight.rgb * up + _SideLight.rgb * side;
        float3 c = s.albedo * E * s.occlusion + _EmissionColor.rgb * f.color.g;
        float3 P = f.positionWS;
        // the sky along the ray (the deck where the ray meets it, with AT's hull occluder): both the cloud mix and the fog fade to it
        float3 sky = NocSkyColor(P - _WorldSpaceCameraPos.xyz) * NocDreadVeilRay(P);
        [branch] if (P.y > NC_CloudMixY0 - NA_CloudWisp)
        {
            float fm = NocDeckForm(NocCloudNoise(P.xz, f.footprint));
            // r01: the towers' mix (380..560) made the belly 21 % and the back 56 % cloud: no silhouette. The hull dissolves only from
            // the deck base up (455..530, billows +-30 m): the belly stays dark, the back sinks into the deck. Fix wave: toward the sky
            // along the ray (r03: the deck colour at the fragment's xz), so the hull parked inside the deck (y >= 575) IS the sky
            // behind it, with no outline, and condenses out of it as it sinks after the launch
            c = lerp(c, sky, smoothstep(455.0, 530.0, P.y + 30.0 * fm));
        }
        float T = NocFogT(P);
        [branch] if (T < 0.99995) c = lerp(sky, c, T);
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
