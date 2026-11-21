// Megastructure/BwSnow - marine snow outside the window gallery's glass (MASTER_PROMPT 14.7 H3 "Snow", 14.5.6; Addendum A1
// 1.8 bulk drift). Beat 6 owner. One static mesh of 6000 camera-facing quads (Beat06Snow): POSITION = the flake's seed in
// [0, 1)^3, uv0 = the corner (+-1, +-1), uv1 = (size jitter, brightness 0.3-1.0, phase, 0). The vertex shader places each
// flake in a box of 40 x 60 x 60 m (lateral from the outer glass + 0.3 m, height round the eye, along s round the camera) with
// an exact per-axis wrap: position = B0 + frac((seed L + drift - B0) / L) L, so a flake leaving one face re-enters at the
// opposite one, never jumps inside the box, and the 8 m edge fade hides the wrap. Drift: 0.03 m/s down, 0.1 m/s turbulence
// (sines per flake), the bulk flow along +s of A1 1.8 (_BwWSnow.xyz, keyed to s by the builder). Size 3 cm but at least 2 px
// (alpha x (true / drawn)^2: energy preserving, no sparkle). Brightness 0.3-1.0 of (veil + caustic shaft + lamps + the eye's
// amber glow) x the water's beam attenuation from the glass. Additive (Blend One One): no sorting. Depth-tested against the
// interior (piers hide them) and the portal's depth (the creature hides the flakes behind it).
Shader "Megastructure/BwSnow"
{
    Properties
    {
        _SnowGain ("Flake radiance scale", Float) = 4.0
        _LampGain ("Lamp light on the flakes", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Snow"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off ZTest LEqual Cull Off
            Blend One One

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex SnowVert
            #pragma fragment SnowFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Include/BulwarkPalette.hlsl"
            #include "Include/BwCommon.hlsl"
            #include "Include/BulwarkAtmosphere.hlsl"
            #include "Generated/BulwarkConstGen.hlsl"
            #include "Include/BwWater.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _SnowGain;
                float _LampGain;
            CBUFFER_END

            struct SnA { float4 positionOS : POSITION; float2 corner : TEXCOORD0; float4 info : TEXCOORD1; };
            struct SnV { float4 positionCS : SV_POSITION; float2 corner : TEXCOORD0; float3 color : TEXCOORD1; };

            SnV SnowVert(SnA v)
            {
                SnV o;
                o.corner = v.corner;
                o.color = 0.0;
                float3 cam = _WorldSpaceCameraPos;
                float t = _BwWState.w;                    // the builder's clock (unwrapped, mod 2400 s)
                float vis = _BwWSnow.w;
                // box in hall coordinates (lateral, y, s)
                float3 Lbox = float3(TC_SnowBoxX, TC_SnowBoxY, TC_SnowBoxZ); // pal:ok (box size, not a colour)
                float camS = BwWHallS(cam);
                float3 B0 = float3(BWW_GLASS_OUT + 0.3, cam.y - 0.5 * TC_SnowBoxY, camS - 0.5 * TC_SnowBoxZ); // pal:ok (box corner)
                float ph = v.info.z * 6.2831853;
                float3 turb = float3(sin(0.31 * t + ph), sin(0.23 * t + 2.1 * ph), sin(0.27 * t + 1.3 * ph)) * (TC_SnowTurbulence / 0.27); // pal:ok (offset)
                float3 q = v.positionOS.xyz * Lbox + turb + _BwWSnow.xyz;   // drift down and the bulk flow from the builder (double)
                float3 c = B0 + frac((q - B0) / Lbox) * Lbox;
                float3 p;
                p.xz = _BwWPlane.xz * (c.x + _BwWPlane.w) + _BwWTan.xz * (c.z - _BwWTan.w);
                p.y = c.y;
                // edge fade (8 m) on the five open faces, 1 m at the glass side
                float3 e0 = c - B0, e1 = B0 + Lbox - c;
                float fade = saturate(e0.x / 1.0) * saturate(e1.x / TC_SnowEdgeFade) * saturate(e0.y / TC_SnowEdgeFade) * saturate(e1.y / TC_SnowEdgeFade)
                           * saturate(e0.z / TC_SnowEdgeFade) * saturate(e1.z / TC_SnowEdgeFade);
                float3 toP = p - cam;
                float dist = max(length(toP), 1e-3);
                float trueSize = TC_SnowSize * v.info.x;
                float px = dist * _BwCam.w;
                float drawSize = max(trueSize, 2.0 * px);
                float keep = (trueSize * trueSize) / (drawSize * drawSize);
                // water path from the outer glass face to the flake along the view ray
                float lc = BwWLateral(cam), lp = c.x;
                float tw = dist * saturate((lp - BWW_GLASS_OUT) / max(lp - lc, 1e-3));
                float3 cext = float3(TC_BeamCR, TC_BeamCG, TC_BeamCB); // pal:ok (beam attenuation, not a colour)
                float3 Tr = exp(-cext * tw);
                float depthP = max(BWW_SEA_Y - p.y, 0.0);
                float3 ambient = BwWVeil(float3(0, 0.3, 0.954), depthP) * 1.4; // pal:ok (direction, not a colour)
                ambient *= lerp(1.0, _BwWState.x, 0.6);
                float3 Lw = BwWSunInWater();
                float3 shaft = BwWVeil(float3(0, 1, 0), depthP) * 0.0743 * BwWShaftAt(p, Lw, _BwWState.w, 0.0) * 2.0; // pal:ok (direction)
                float3 lamp = BwWLamps(p) * _LampGain;
                float3 glow = BwWEyeGlow(p);
                // peak capped at 1.5 x the horizontal veil of the flake's depth: a flake adds at most 150 % over open water and
                // stays under 8 x the 5x5 median even over the dark creature (no fireflies, 14.10.3), lamp-lit ones included
                float3 veilH = BwWVeil(float3(0, 0, 1), depthP); // pal:ok (direction, not a colour)
                float3 flake = min((ambient + shaft + lamp + glow) * (v.info.y * _SnowGain), 1.5 * veilH);
                o.color = flake * Tr * (keep * fade * vis);
                float3 right = UNITY_MATRIX_V[0].xyz, up = UNITY_MATRIX_V[1].xyz;
                float3 wp = p + (right * v.corner.x + up * v.corner.y) * (0.5 * drawSize * (vis > 1e-3 ? 1.0 : 0.0));
                o.positionCS = TransformWorldToHClip(wp);
                return o;
            }

            half4 SnowFrag(SnV i) : SV_Target
            {
                float r2 = dot(i.corner, i.corner);
                float a = saturate(1.0 - r2);
                a *= a;
                return half4(i.color * a, 0.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
