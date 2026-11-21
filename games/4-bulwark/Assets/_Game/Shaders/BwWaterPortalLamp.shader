// Megastructure/BwWaterPortalLamp - the two survey lamps' light on the kraken behind the window gallery's glass (Addendum A1
// 1.8: lamp A "lights the arm with grazing light along the glass: the red underside, pale hooked suckers"; lamp B lights the
// eye's socket and brow). Beat 6 owner.
// Why a pass of its own: BwCreature's underwater object model multiplies its whole lit sum by the downwelling attenuation
// exp(-Kd depth) of 14.7 H3. That is right for the sun and the sky (they come down through 60-80 m of water), but it also
// removes the survey lamps a few metres away (red e^-19, green e^-3.4 at y -76), so the arm stays a black silhouette under a
// full lamp. This pass draws the creature's visible meshes once more (Beat06Encounter, Graphics.RenderMesh, only while a lamp
// is lit, queue Geometry+21 right after the creature's Geometry+20) with the creature's own vertex skinning (the vertex hook
// below is BwCreature's, line for line: KrSkin / KrMantle of BwCreatureSkin.hlsl) and its own textures (the material is a copy
// of the creature's, so every map and _KrFade carry over), and ADDS the lamps' light:
//   * the same lamp function as the reveal concrete and the water's in-scatter (BwWLampAt: cone, inverse square with a 1 m
//     core, range window, the beam attenuation c along the lamp ray), so the skin, the reveals and the lit water agree;
//   * a wrapped Lambert (grazing light: the sucker rims facing the lamp catch it, the flat skin a little) and a slime
//     highlight (normalised Blinn-Phong, exponent widened by the normal's screen variance: no sparkle);
//   * the 14.7 H3 object model's transmittance exp(-c r) over the water path to the camera (r = view distance minus the in-air
//     path, _KrUnderwater.w), times the underwater weight (0 outside the hall: nothing is ever added in air).
// The maps are read at least at mip _LampMinLod: the arm is seen from 8-15 m, where the Tripo tile is magnified 3-6x and a
// grazing lamp would draw its texels as blocks; the slime highlight is kept low for the same reason.
// Additive (Blend One One), no depth write, ZTest LEqual with a -1 offset on the creature's own depth (the same vertex code,
// so only the visible front surface passes); the LOD cross-fade clips by the creature's _KrFade like the creature itself.
Shader "Megastructure/BwWaterPortalLamp"
{
    Properties
    {
        [NoScaleOffset] _AlbedoMap ("Body albedo", 2D) = "grey" {}
        [NoScaleOffset] _NormalMap ("Body normal", 2D) = "bump" {}
        [NoScaleOffset] _MaskMap ("Body mask (AO, smooth, cavity, socket glow)", 2D) = "white" {}
        [NoScaleOffset] _TentAlbedo ("Tentacle albedo", 2D) = "grey" {}
        [NoScaleOffset] _TentNormal ("Tentacle normal", 2D) = "bump" {}
        [NoScaleOffset] _TentMask ("Tentacle mask (AO, smooth, height, sucker)", 2D) = "white" {}
        _KrTube ("Tube (1) or body (0)", Float) = 0
        _KrFade ("LOD fade (x coverage, y mode)", Vector) = (1, 0, 0, 0)
        _NormalStrength ("Normal strength", Range(0, 2)) = 1.0
        _LampGain ("Lamp light on the skin", Float) = 1.0
        _LampGainB ("Lamp B (the eye hold, 25 m out) relative to lamp A", Float) = 1.0
        _LampGainA ("Lamp A (low mount at the pressed arm, 2-8 m) relative to _LampGain", Float) = 1.0
        _LampKneeA ("Lamp A effective water path: knee (m) of the lamp and view paths", Float) = 3.0
        _LampSlopeA ("Lamp A effective water path: slope beyond the knee", Range(0, 1)) = 0.25
        _LampWrap ("Lambert wrap (grazing light)", Range(0, 0.5)) = 0.2
        _SpecGain ("Slime highlight", Range(0, 2)) = 0.35
        _LampNormal ("Normal-map strength under the lamp", Range(0, 1)) = 0.6
        _LampMinLod ("Least mip of the maps (no magnified texels)", Range(0, 4)) = 1.5
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    HLSLINCLUDE
    #include "Include/BwLitCore.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _KrTube;
        float4 _KrFade;
        float _NormalStrength;
        float _LampGain;
        float _LampGainB;
        float _LampGainA;
        float _LampKneeA;
        float _LampSlopeA;
        float _LampWrap;
        float _SpecGain;
        float _LampNormal;
        float _LampMinLod;
        float _Cull;
    CBUFFER_END

    TEXTURE2D(_AlbedoMap); SAMPLER(sampler_AlbedoMap);
    TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
    TEXTURE2D(_MaskMap); SAMPLER(sampler_MaskMap);
    TEXTURE2D(_TentAlbedo); SAMPLER(sampler_TentAlbedo);
    TEXTURE2D(_TentNormal); SAMPLER(sampler_TentNormal);
    TEXTURE2D(_TentMask); SAMPLER(sampler_TentMask);

    #include "Include/BwCreatureSkin.hlsl"

    #define BW_HAS_VERTEX_HOOK
    #define BW_HAS_CLIP_HOOK

    // BwCreature's vertex hook, unchanged (the positions must match the creature's depth bit for bit)
    void BwVertexHook(inout BwVertexData v)
    {
        float3 p = v.positionOS;
        float3x3 w2o = (float3x3)GetWorldToObjectMatrix();
        float3 nOS = normalize(mul(w2o, v.normalWS));
        float3 tOS = mul(w2o, v.tangentWS.xyz);
        tOS = dot(tOS, tOS) > 1e-12 ? normalize(tOS) : float3(1, 0, 0); // pal:ok (fallback axis)
        float w = saturate(v.color.y);
        float3 q = p;
        if (v.color.x < 200.0 && w > 1e-4)
        {
            uint limb = KrLimbIndex(v.color.x);
            float3 n2 = nOS, t2 = tOS;
            float3 sk = KrSkin(limb, v.uv.z, p, n2, t2);
            q = lerp(p, sk, w);
            nOS = normalize(lerp(nOS, n2, w));
            t2 = lerp(tOS, t2, w);
            tOS = dot(t2, t2) > 1e-12 ? normalize(t2) : tOS;
        }
        q = KrMantle(q, 1.0 - w);
        v.custom = float4(p, w); // pal:ok (rest position, not a colour)
        v.positionWS = TransformObjectToWorld(q);
        v.normalWS = TransformObjectToWorldNormal(nOS);
        v.tangentWS = float4(TransformObjectToWorldDir(tOS), v.tangentWS.w); // pal:ok (not a colour)
    }

    // BwCreature's LOD cross-fade clip, unchanged
    void BwClipHook(BwFragData f)
    {
        if (_KrFade.x < 0.999 || _KrFade.y > 0.5)
        {
            float n = BwBlueNoiseAt(f.positionCS.xy);
            float keep = _KrFade.y > 0.5 ? (n - _KrFade.x + 1e-4) : (_KrFade.x - n);
            clip(keep);
        }
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+21" "IgnoreProjector" = "True" }

        Pass
        {
            Name "LampOnSkin"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off ZTest LEqual Cull [_Cull]
            Offset -1, -1
            Blend One One

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex BwVert
            #pragma fragment LampFrag
            #include "Include/BwLitPass.hlsl"
            #include "Generated/BulwarkConstGen.hlsl"
            #include "Include/BwWater.hlsl"

            float3 LampTangentToWorld(BwFragData f, float3 nTS)
            {
                float3 tWS = f.tangentWS.xyz - f.normalWS * dot(f.tangentWS.xyz, f.normalWS);
                if (dot(tWS, tWS) < 1e-10) return f.normalWS;
                tWS = normalize(tWS);
                float3 bWS = cross(f.normalWS, tWS) * (f.tangentWS.w < 0.0 ? -1.0 : 1.0);
                return normalize(nTS.x * tWS + nTS.y * bWS + nTS.z * f.normalWS);
            }

            float3 LampCotangentToWorld(BwFragData f, float2 uv, float3 nTS)
            {
                float3 dp1 = ddx(f.positionWS), dp2 = ddy(f.positionWS);
                float2 du1 = ddx(uv), du2 = ddy(uv);
                float3 N = f.normalWS;
                float3 dp2perp = cross(dp2, N), dp1perp = cross(N, dp1);
                float3 T = dp2perp * du1.x + dp1perp * du2.x;
                float3 B = dp2perp * du1.y + dp1perp * du2.y;
                float invmax = rsqrt(max(max(dot(T, T), dot(B, B)), 1e-20));
                return normalize(nTS.x * T * invmax + nTS.y * B * invmax + nTS.z * N);
            }

            // a map sample never finer than mip _LampMinLod (soft under magnification, unchanged in the distance)
            #define LAMP_SAMPLE(tex, smp, uv) SAMPLE_TEXTURE2D_LOD(tex, smp, uv, max(CALCULATE_TEXTURE2D_LOD(tex, smp, uv), _LampMinLod))

            float3 LampUnpack(float4 tex, float strength)
            {
                float3 n = tex.rgb * 2.0 - 1.0;
                n.xy *= strength;
                return normalize(float3(n.xy, max(n.z, 0.05))); // pal:ok (not a colour)
            }

            // The creature's maps (the procedural crust, scars and moss of BwCreature are left out: under one warm lamp the maps
            // carry the form; a crust shell is at most 6 % of the lit area).
            void LampSurface(BwFragData f, out float3 albedo, out float3 n, out float ao, out float smooth)
            {
                n = f.normalWS; ao = 1.0; smooth = 0.6;
                if (_KrTube > 2.5) { albedo = PAL_KrakenMoss * 0.8; smooth = 0.55; return; }          // kelp
                if (_KrTube > 1.5) { albedo = PAL_KrakenSkinDark * 0.15; smooth = 0.75; return; }     // beak
                float u = f.uv.z, th = f.uv.w;
                bool isLimb = f.color.x < 200.0;
                uint limb = KrLimbIndex(f.color.x);
                float4 L2 = isLimb ? _KrLimb2[limb] : float4(0, 1, 100, 1); // pal:ok (not a colour)
                float2 tuv = float2(th * (0.5 / PI) + 0.5, -KrTileV(u, L2));
                float cav;
                if (_KrTube > 0.5)
                {
                    albedo = LAMP_SAMPLE(_TentAlbedo, sampler_TentAlbedo, tuv).rgb;
                    float4 m = LAMP_SAMPLE(_TentMask, sampler_TentMask, tuv);
                    n = LampTangentToWorld(f, LampUnpack(LAMP_SAMPLE(_TentNormal, sampler_TentNormal, tuv), _NormalStrength * _LampNormal));
                    ao = m.r; smooth = m.g; cav = saturate(0.5 + (m.b - 0.5) * 0.6);
                }
                else
                {
                    float2 uv = f.uv.xy;
                    albedo = LAMP_SAMPLE(_AlbedoMap, sampler_AlbedoMap, uv).rgb;
                    float4 m = LAMP_SAMPLE(_MaskMap, sampler_MaskMap, uv);
                    n = LampTangentToWorld(f, LampUnpack(LAMP_SAMPLE(_NormalMap, sampler_NormalMap, uv), _NormalStrength * _LampNormal));
                    ao = m.r; smooth = m.g; cav = m.b;
                    float b = (isLimb && limb < 8u) ? smoothstep(L2.x - 46.0, L2.x - 6.0, u) : 0.0;
                    float3 ta = LAMP_SAMPLE(_TentAlbedo, sampler_TentAlbedo, tuv).rgb;
                    float4 tm = LAMP_SAMPLE(_TentMask, sampler_TentMask, tuv);
                    float3 tn = LampCotangentToWorld(f, tuv, LampUnpack(LAMP_SAMPLE(_TentNormal, sampler_TentNormal, tuv), _NormalStrength * _LampNormal));
                    albedo = lerp(albedo, ta, b);
                    n = normalize(lerp(n, tn, b));
                    ao = lerp(ao, tm.r, b); smooth = lerp(smooth, tm.g, b); cav = lerp(cav, 0.5, b);
                }
                float crev = saturate((0.5 - cav) * 2.0);
                albedo *= 1.0 - 0.45 * crev;
                ao *= lerp(1.0, saturate(f.color.z), 0.7) * (1.0 - 0.35 * crev);
                albedo = clamp(albedo, 0.004, 0.75);
                smooth = clamp(smooth, 0.2, 0.9);
            }

            // one lamp on the skin: the reveals' irradiance scale (_BwWLampCol x BwWLampAt), wrapped Lambert, slime highlight
            float3 LampOnSkin(float3 P, float3 N, float3 V, float3 albedo, float ao, float smooth, float nVar, float4 L0, float4 L1)
            {
                if (L0.w <= 1e-4) return 0.0;
                float3 E = _BwWLampCol.rgb * BwWLampAt(P, L0, L1);
                float3 Ld = normalize(L0.xyz - P);
                float nl = dot(N, Ld);
                float diff = saturate((nl + _LampWrap) / (1.0 + _LampWrap));
                float3 H = normalize(Ld + V);
                float e = exp2(1.0 + 6.0 * smooth);                 // 2 .. 128
                e = e / (1.0 + e * nVar);                           // widened by the normal's screen variance (no sparkle)
                float spec = 0.04 * (e + 8.0) / (8.0 * PI) * pow(saturate(dot(N, H)), e) * saturate(nl);
                return E * (albedo * diff * ao + _SpecGain * spec);
            }

            half4 LampFrag(BwVaryings i) : SV_Target0
            {
                UNITY_SETUP_INSTANCE_ID(i);
                BwFragData f = BwMakeFrag(i);
                BwClipHook(f);
                float uw = _KrUnderwater.x;
                float3 albedo, N; float ao, smooth;
                LampSurface(f, albedo, N, ao, smooth);
                float3 dn = fwidth(N);
                float nVar = saturate(dot(dn, dn) * 4.0);
                float3 V = f.viewDirWS;
                float3 P = f.positionWS;
                float3 cext = float3(TC_BeamCR, TC_BeamCG, TC_BeamCB); // pal:ok (beam attenuation c of 14.7 H3, not a colour)
                float r = max(f.viewDist - _KrUnderwater.w, 0.0);
                float3 T = exp(-cext * r);
                // Lamp A (critic S2-1, A1 1.8 "the red underside"): the EFFECTIVE source sits at the pane. Its water paths (lamp
                // to skin d, skin to glass r) count in full up to a knee of _LampKneeA m and at _LampSlopeA beyond it, so the
                // underside a few metres from the lamp keeps its red (c_R 0.34 /m would leave e^-6 of it over the real 8 + 10 m)
                // while the arm farther along the beam still turns teal (the A1 water colour). Lamp B (the eye hold) is physical.
                float dA = length(P - _BwWLampA0.xyz);
                float dAe = min(dA, _LampKneeA) + _LampSlopeA * max(dA - _LampKneeA, 0.0);
                float rAe = min(r, _LampKneeA) + _LampSlopeA * max(r - _LampKneeA, 0.0);
                float3 TA = exp(-cext * (rAe + dAe - dA));          // the lamp function itself applies exp(-c dA)
                float3 c = _LampGainA * TA * LampOnSkin(P, N, V, albedo, ao, smooth, nVar, _BwWLampA0, _BwWLampA1)
                         + _LampGainB * T * LampOnSkin(P, N, V, albedo, ao, smooth, nVar, _BwWLampB0, _BwWLampB1);
                return half4(max(c * (uw * _LampGain), 0.0), 0.0h);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
