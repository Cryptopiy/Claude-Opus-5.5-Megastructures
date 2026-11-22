// NocSky - the camera-following sky dome (14.2.5: radius 11 000 m, ZWrite off, Background queue; D-008 / R3 5.3: positioned
// around the camera in the vertex shader from the exact object-space direction, so every camera, capture cameras included,
// gets it with no transform lag and no far-plane cut at far 12 000). Colour = NocSkyColorBlur(view direction, pixel angle) of
// NocAtmosphere.hlsl: the LOOK_V2 3.1 gradient (near-neutral, warm only where the city lights it), the cloud deck at y 460
// (tint model, 4-octave billows) fogged like geometry, the abyss with the H3 sheet modulation, the HazeLow share of the alley. It is the same function every fog fades toward, so fog, sky and dome agree at every elevation (H3; a fogged
// ray to the dome at 11 km is the sky colour itself, so NocFog is the identity here). Then NocFloor. No local dither (the one
// dither is in NocGrade, 14.7 H1). The camera clears SolidColor with no skybox (D-008: URP's skybox would paint over it).
// Fix wave (AT): in beat 7 the dome adds the koi's in-scatter (NocKoiAir, the same term NocFog adds to geometry) along the ray to
// the deck plane, so the air around the 900 m koi glows behind and beside the body (C02-air).
// The Ark (D-043, AT): the dome adds NocArkAir (the glow of the Ark's lights and its searchlight cones in the haze, the same term
// NocFog adds to geometry) along the ray to the deck plane, or 8 km below the horizon.
Shader "Megastructure/NocSky"
{
    Properties
    {
        _SkyRadius ("Dome radius (m)", Float) = 11000
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Include/NocAtmosphere.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _SkyRadius;
    CBUFFER_END

    struct SkyAttributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
    struct SkyVaryings { float4 positionCS : SV_POSITION; float3 dirWS : TEXCOORD0; };

    SkyVaryings SkyVert(SkyAttributes v)
    {
        SkyVaryings o;
        UNITY_SETUP_INSTANCE_ID(v);
        float3 dir = v.positionOS.xyz * rsqrt(max(dot(v.positionOS.xyz, v.positionOS.xyz), 1e-12));   // unit-sphere mesh
        float3 posWS = _WorldSpaceCameraPos.xyz + dir * _SkyRadius;                                      // rotation locked
        o.positionCS = TransformWorldToHClip(posWS);
        o.dirWS = dir;
        return o;
    }

    half4 SkyFrag(SkyVaryings i) : SV_Target
    {
        float3 c = NocSkyColorBlur(i.dirWS, NocPixelAngle());
        [branch] if (_NocAir.z > 0.0)                                   // C02-air (AT fix wave): the koi's light in the air, to the deck
        {
            float3 d = normalize(i.dirWS);
            c += NocKoiAir(d, d.y > 1e-3 ? min((NC_CloudBaseY - _WorldSpaceCameraPos.y) / d.y, 8000.0) : 8000.0);
        }
        [branch] if (_NocArkL.x + _NocArkG.z > 0.0)                     // D-043 (AT): the Ark's lights and searchlights in the air
        {
            float3 d = normalize(i.dirWS);
            c += NocArkAir(d, d.y > 1e-3 ? min((NC_CloudBaseY - _WorldSpaceCameraPos.y) / d.y, 8000.0) : 8000.0);
        }
        return half4(NocFloor(c), 1.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Background" "RenderPipeline" = "UniversalPipeline" "Queue" = "Background" "PreviewType" = "Skybox" }
        Pass
        {
            Name "Sky"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off ZTest LEqual Cull Front
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex SkyVert
            #pragma fragment SkyFrag
            #pragma multi_compile_instancing
            ENDHLSL
        }
    }
    Fallback Off
}
