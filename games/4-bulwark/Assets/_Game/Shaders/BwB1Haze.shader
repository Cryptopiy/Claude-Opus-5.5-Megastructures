// BwB1Haze - the beat-1 gallery air (MASTER_PROMPT 14.5.1 "Dust: motes per lamp cone", KS02 "a mist breath at z 66"):
// additive camera-facing soft quads, ZWrite Off, Transparent queue, built once by Beat01WaveGallery.
//   _Mode 0 (motes): POSITION = mote centre, TEXCOORD0 = (corner x, corner y, seed, cone weight), TEXCOORD1 = its lamp.
//     A slow drift keyed to the ambient clock with angular rates 2 pi k / 3600 (whole periods in the 3600 s wrap of
//     _BwTime.x: no jump, Part 10 #33). World radius _Size; below 2.5 px the radius is held at 2.5 px and the
//     brightness scaled by the area ratio (energy kept, no sub-pixel sparkle and no isolated pixel, 14.5.1 "min 2 px"); faded out within 0.6 m
//     of the eye (no blobs) and beyond _FadeFar. Forward scattering toward the lamp: Henyey-Greenstein g 0.55.
//   _Mode 2 (grains): dust shaken off the soffit by an impact (14.5.1 "a falling grain sheet at each impact"): POSITION =
//     the start on the ceiling, TEXCOORD0 = (corner, seed, slit index), TEXCOORD1 = (fall speed, delay, drift x, size);
//     driven by the same per-slit (env, age) as the breath; 2.5:1 streaks, faded in over 0.15 s and out above the floor.
//   _Mode 1 (breath): POSITION = the slit mouth, TEXCOORD0 = (corner x, corner y, puff seed, slit index), TEXCOORD1 =
//     the puff's direction (unit, into the corridor) and its spread. Each slit's envelope and age come from C#
//     (_B0.._B4 = (env, age) pairs for slits 0..9; env 0 = nothing drawn): the puff expands from 0.2 to 0.8 m radius
//     and drifts 0.4 + 1.3 smootherstep(age / 3) m into the gallery; soft against the scene depth over 0.35 m (no
//     hard intersection with the pilaster, the floor or the walls).
//   _Mode 3 (slit burst sheet, Wave X2 fix, sea S3-3): a world-fixed quad on the baffle face of slit k: POSITION = world
//     vertex, TEXCOORD0 = (dz from the slit centre (m), dy above the sill (m), seed, slit index). The spray of an impact
//     rushing up the sea face past the baffle gap: a whitewater front rises through the 1.8 m slit in 0.45 s, the sheet
//     scrolls up and falls back ballistically (6 m/s, g), its streaky foam (5 x 22 cm cells, footprint-filtered fBm) thins
//     as it falls; driven by the slit's (env, age) of _B0.._B4 (0.6 s in, 2.4 s out: no flash above 1 Hz). TEXCOORD1.x = 1
//     marks a quad on a channel cheek (TEXCOORD0.x = depth from the baffle): the sheet's light on the cheek, 40 % at the
//     baffle, e-folding over 0.6 m toward the mouth (visible on the walk, where the slit hides the baffle face).
//   _Mode 4 (slit spray droplets): billboards like the grains: POSITION = the start on the baffle, TEXCOORD0 = (corner,
//     seed, slit index), TEXCOORD1 = (velocity (m/s), delay): ballistic clumps thrown through the channel toward the
//     mouth (_Size = the mouth plane x), faded on the sill (_FloorY) and 0.3 m past the mouth.
//   _Mode 5 (wet stain, beat 5 drip): a world-fixed floor quad, TEXCOORD0 = (local x, local y in -1..1, seed, 0): blended
//     premultiplied (_SrcBlend One, _DstBlend OneMinusSrcAlpha): alpha darkens the floor (wet concrete about 0.55 of dry
//     at the centre, a faint tide line at the drying edge, a soft irregular fBm rim, no facets), rgb adds the GGX sheen
//     of the landing lamp (_B0 = lamp position, w = its live intensity x pool weight; F0 0.02, smoothness 0.55-0.9 varied
//     by noise).
// Colour: _Color (a palette token, linear, set by the builder) x _Intensity; fogged by the transmittance of BwFogApply
// (additive light is attenuated, never fog-coloured). Constant in luminance at a fixed pose and time (no flicker).
Shader "Megastructure/BwB1Haze"
{
    Properties
    {
        _Color ("Colour (linear token)", Color) = (1.0, 0.434, 0.068, 1)
        _Intensity ("Intensity", Float) = 0.02
        _Mode ("Mode (0 motes, 1 breath, 2 grains, 3 slit sheet, 4 slit spray, 5 wet stain)", Float) = 0
        _Size ("Mote radius (m)", Float) = 0.0035
        _FadeFar ("Far fade (m)", Float) = 28
        _FloorY ("Floor y for mode 2 (m)", Float) = 0
        _B0 ("Breath slits 0-1 (env, age)", Vector) = (0, 0, 0, 0)
        _B1 ("Breath slits 2-3", Vector) = (0, 0, 0, 0)
        _B2 ("Breath slits 4-5", Vector) = (0, 0, 0, 0)
        _B3 ("Breath slits 6-7", Vector) = (0, 0, 0, 0)
        _B4 ("Breath slits 8-9", Vector) = (0, 0, 0, 0)
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst blend", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float4 _B0, _B1, _B2, _B3, _B4;
            float _Intensity;
            float _Mode;
            float _Size;
            float _FadeFar;
            float _FloorY;
        CBUFFER_END

        #include "Include/BwCommon.hlsl"
        #include "Include/BulwarkAtmosphere.hlsl"
        ENDHLSL

        Pass
        {
            Name "ForwardHaze"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off
            ZTest LEqual
            Cull Off
            Blend [_SrcBlend] [_DstBlend]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 corner : TEXCOORD0;
                float3 color : TEXCOORD1;
                float4 screen : TEXCOORD2;   // xy/w = screen uv, z = view depth (m), w = clip w
                float soft : TEXCOORD3;      // soft-depth distance (m), 0 = off
                float3 posWS : TEXCOORD4;    // modes 3, 5: world position
                float4 prm : TEXCOORD5;      // modes 3, 5: (env, age, seed, -)
            };

            static const float kTwoPi = 6.2831853;
            static const float kWrap = 3600.0;

            float BreathEnv(int k, out float age)
            {
                float4 b = k < 2 ? _B0 : (k < 4 ? _B1 : (k < 6 ? _B2 : (k < 8 ? _B3 : _B4)));
                bool hi = (k & 1) == 1;
                age = hi ? b.w : b.y;
                return hi ? b.z : b.x;
            }

            Varyings Vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                if ((_Mode > 2.5 && _Mode < 3.5) || _Mode > 4.5)
                {
                    // ---- world-fixed quads: the slit burst sheet (3) and the wet stain (5)
                    float3 pw = TransformObjectToWorld(v.positionOS.xyz);
                    float env = 1.0, age = 0.0;
                    if (_Mode < 3.5) env = BreathEnv((int)round(v.uv0.w), age);
                    o.positionCS = env > 1e-4 ? TransformWorldToHClip(pw) : float4(0.0, 0.0, -1.0, 1.0);   // pal:ok (nothing to draw: a degenerate clip position)
                    o.corner = v.uv0.xy;
                    o.posWS = pw;
                    o.prm = float4(env, age, v.uv0.z, v.uv0.w);   // pal:ok (mode 5: w = the quad's half length in local units)
                    float3 Tw = BwFogApply(float3(1, 1, 1), pw) - BwFogApply(float3(0, 0, 0), pw);   // pal:ok (transmittance probe)
                    o.color = _Color.rgb * _Intensity * saturate(Tw);
                    o.screen = ComputeScreenPos(o.positionCS);
                    o.screen.z = 0.0;
                    o.soft = v.uv1.x;   // mode 3: 1 = a channel cheek (corner.x = distance from the baffle)
                    return o;
                }
                float3 centre = TransformObjectToWorld(v.positionOS.xyz);
                float t = _BwTime.x;
                float radius, gain;
                float3 col = _Color.rgb * _Intensity;
                float soft = 0.0;
                float stretch = 1.0;
                if (_Mode < 0.5)
                {
                    // ---- motes: slow drift (whole periods in 3600 s), a 4 cm slow fall-and-rise
                    float sd = v.uv0.z;
                    float3 w = kTwoPi / kWrap * float3(47.0 + floor(sd * 7.0), 61.0 + floor(sd * 5.0), 73.0 + floor(sd * 3.0));   // pal:ok (angular rates, not a colour)
                    float3 ph = sd * float3(41.3, 17.9, 23.1);                                                          // pal:ok (phases)
                    centre += float3(0.06 * sin(w.x * t + ph.x), 0.04 * sin(w.y * t + ph.y), 0.06 * cos(w.z * t + ph.z));    // pal:ok (offsets, m)
                    radius = _Size;
                    gain = v.uv0.w;
                    // forward scattering toward the lamp (Henyey-Greenstein, g 0.55), normalised to 1 at 90 deg
                    float3 toCam = GetCameraPositionWS() - centre;
                    float3 fromLamp = centre - v.uv1.xyz;
                    float cosT = dot(normalize(toCam + 1e-5), normalize(fromLamp + 1e-5));
                    const float g = 0.55;
                    float hg = (1.0 - g * g) / pow(max(1.0 + g * g - 2.0 * g * cosT, 1e-3), 1.5);
                    float hg90 = (1.0 - g * g) / pow(1.0 + g * g, 1.5);
                    gain *= min(hg / hg90, 4.0);   // capped: no glints on the line of sight to the lamp
                }
                else if (_Mode > 3.5)
                {
                    // ---- slit spray droplets: ballistic clumps from the baffle toward the mouth (uv1 = velocity, delay)
                    int k = (int)round(v.uv0.w);
                    float age;
                    float env = BreathEnv(k, age);
                    float a = max(age - v.uv1.w, 0.0);
                    centre += v.uv1.xyz * a + float3(0.0, -4.9 * a * a, 0.0);   // pal:ok (ballistic, m)
                    radius = 0.006 + 0.008 * v.uv0.z;
                    float sill = saturate((centre.y - _FloorY) / 0.05);
                    float mouth = 1.0 - saturate((_Size - centre.x) / 0.3);
                    gain = (age > v.uv1.w ? saturate(a / 0.05) : 0.0) * (1.0 - smoothstep(0.55, 1.0, a)) * sill * mouth * saturate(env * 2.2);
                    stretch = 1.8;
                }
                else if (_Mode > 1.5)
                {
                    // ---- grains: dust shaken off the soffit by an impact near slit k, falling 0.6-1.4 m/s (uv1 = speed,
                    // delay, drift x, size), fading in over 0.15 s and out above the floor; streaks 2.5x taller than wide
                    int k = (int)round(v.uv0.w);
                    float age;
                    float env = BreathEnv(k, age);
                    float a = age - v.uv1.y;
                    float fall = v.uv1.x * max(a, 0.0);
                    centre += float3(v.uv1.z * max(a, 0.0), -fall, 0.0);   // pal:ok (fall, m)
                    radius = v.uv1.w;
                    gain = (a > 0.0 ? saturate(a / 0.15) : 0.0) * saturate((centre.y - _FloorY - 0.05) / 0.4) * saturate(env * 1.5);
                    stretch = 2.5;
                }
                else
                {
                    // ---- breath: one puff of a slit's spray drifting into the gallery
                    int k = (int)round(v.uv0.w);
                    float age;
                    float env = BreathEnv(k, age);
                    float u = saturate(age / 3.0);
                    float s5 = u * u * u * (u * (u * 6.0 - 15.0) + 10.0);
                    float3 dir = v.uv1.xyz;
                    float spread = v.uv1.w;
                    centre += dir * (0.4 + 1.3 * s5) * spread + float3(0.0, 0.25 * s5, 0.0);   // pal:ok (rise, m)
                    radius = (0.2 + 0.6 * s5) * lerp(0.7, 1.15, v.uv0.z);
                    gain = env;
                    soft = 0.25;
                }
                // camera-facing quad; hold the radius at 2.5 px and scale the brightness by the area ratio below it
                float3 viewC = TransformWorldToView(centre);
                float depth = max(-viewC.z, 1e-3);
                float pxPerM = _ScreenParams.y * 0.5 * abs(UNITY_MATRIX_P[1][1]) / depth;   // abs: the projection is Y-flipped into render textures
                float rPx = radius * pxPerM;
                const float kMinPx = 2.5;   // a soft blob 5 px across: never an isolated pixel (image_metrics NaN-like / firefly)
                float rEff = max(radius, kMinPx / max(pxPerM, 1e-5));
                gain *= (rPx < kMinPx) ? (rPx * rPx) / (kMinPx * kMinPx) : 1.0;
                // near and far fades (no blob in front of the lens, no distant speckle)
                float nearF = _Mode < 0.5 ? 0.6 : 0.3;
                gain *= saturate((depth - nearF) / nearF) * (1.0 - saturate((depth - _FadeFar) / (0.25 * _FadeFar)));
                float3 viewP = viewC + float3(v.uv0.x * rEff, v.uv0.y * rEff * stretch, 0.0);   // pal:ok (view-space offset)
                o.positionCS = TransformWViewToHClip(viewP);
                o.corner = v.uv0.xy;
                // additive light through the fog: the transmittance of BwFogApply at the centre
                float3 T = BwFogApply(float3(1, 1, 1), centre) - BwFogApply(float3(0, 0, 0), centre);   // pal:ok (transmittance probe)
                o.color = col * gain * saturate(T);
                o.screen = ComputeScreenPos(o.positionCS);
                o.screen.z = depth;
                o.soft = soft;
                return o;
            }

            // slit burst sheet: whitewater of the spray rushing up past the baffle gap (see the header, mode 3)
            half4 FragSheet(Varyings i)
            {
                float env = saturate(i.prm.x * 2.2), age = i.prm.y, sd = i.prm.z;
                float dz = i.corner.x, y = i.corner.y;
                bool cheekQuad = i.soft > 0.5;
                if (cheekQuad) dz = 0.0;   // the cheeks: corner.x is the depth from the baffle, the front is the slit's centre line
                const float kH = 1.8;   // slit height (14.5.1: y 1.0-2.8)
                // the leading edge rises from under the sill to above the head in 0.45 s, jittered by a coarse noise
                float edgeN = BwValueNoise(float2(dz / 0.06 + sd * 31.0, age * 1.7));
                float yf = -0.3 + (kH + 0.9) * BwSmootherstep01(age / 0.45) + 0.25 * (edgeN - 0.5);
                float below = saturate((yf - y) / 0.22);
                // ballistic scroll of the sheet: up at 6 m/s, back down after 0.61 s
                float scroll = 6.0 * age - 4.9 * age * age;
                // vertical streaks (4 x 45 cm cells), sideways warped by a coarse noise so they curl like a rising sheet
                float ys = y - scroll;
                float warp = BwValueNoise(float2(dz / 0.12, ys / 0.35) + sd * 9.1) - 0.5;
                float2 q = float2((dz + 0.06 * warp) / 0.04, ys / 0.45) + sd * float2(13.7, 7.3);   // pal:ok (cells: 4 x 45 cm)
                float fq = max(fwidth(dz) / 0.04, fwidth(y) / 0.45);
                float n = BwFbmFiltered(q, fq, 3);
                float fine = BwFbmFiltered(q * float2(2.3, 1.6) + 5.1, 2.3 * fq, 2);   // pal:ok (octave scales)
                float thin = saturate((age - 0.6) / 2.4);
                float thr = lerp(0.38, 0.6, thin);
                float foam = smoothstep(thr, thr + 0.22, n) * (0.65 + 0.35 * fine);
                // the channel cheeks shade the sheet's edges a little
                float cheek = 1.0 - 0.35 * smoothstep(0.09, 0.15, abs(dz));
                float glow = 0.22 * below;   // light scattered by the spray body
                float crest = 0.6 * exp(-((yf - y) * (yf - y)) / (2.0 * 0.1 * 0.1)) * (1.0 - thin);   // the bright leading edge while it rises
                // the sheet's light on the channel cheeks (seen on the walk, where the 0.3 m slit hides the baffle face): 40 % at
                // the baffle, e-folding over 0.6 m toward the mouth, streaked by the same moving foam (branch-free: the
                // derivatives stay in uniform flow)
                float d = i.corner.x;
                float2 qc = float2(d / 0.3, ys / 0.45) + sd * float2(3.1, 7.3);   // pal:ok (cells)
                float nc = BwFbmFiltered(qc, max(fwidth(d) / 0.3, fwidth(y) / 0.45), 2);
                float Ic = 0.4 * exp(-d / 0.6) * (below * (0.3 + 0.7 * smoothstep(0.35, 0.7, nc)) + 0.5 * crest);
                float Is = cheek * (below * (foam + glow) + crest * (0.5 + 0.5 * n));
                float I = env * (cheekQuad ? Ic : Is);
                return half4(i.color * I, 0.0);
            }

            // wet stain under the beat-5 drip (see the header, mode 5): premultiplied darkening + the lamp's sheen
            half4 FragStain(Varyings i)
            {
                float2 p = i.corner;
                float sd = i.prm.z;
                float fp = max(fwidth(p.x), fwidth(p.y));
                float n1 = BwFbmFiltered(p * 2.2 + sd * 17.0, fp * 2.2, 3);
                float n2 = BwFbmFiltered(p * 7.0 + sd * 5.0 + 3.3, fp * 7.0, 3);
                float r = length(p) + 0.26 * (n1 - 0.5) + 0.06 * (n2 - 0.5);
                // window: zero by the quad's edges whatever the noise (no straight cut)
                float win = (1.0 - smoothstep(0.78, 0.98, abs(p.x))) * (1.0 - smoothstep(0.78, 0.98, abs(p.y) / max(i.prm.w, 1.0)));
                float wet = (1.0 - smoothstep(0.38, 0.74, r)) * win;
                float tide = exp(-((r - 0.64) * (r - 0.64)) / (2.0 * 0.045 * 0.045)) * (1.0 - smoothstep(0.74, 0.84, r)) * win;
                float dark = saturate(0.4 * wet * (0.75 + 0.5 * n2) + 0.12 * tide);
                // sheen: GGX (URP's minimal form) of the landing lamp, smoothness 0.55-0.9 by noise, water F0 0.02
                float smoothK = lerp(0.55, 0.9, saturate(wet * 1.3) * saturate(0.35 + n2));
                float pr = 1.0 - smoothK, rr = max(pr * pr, 0.002), rr2 = rr * rr;
                float3 Lv = _B0.xyz - i.posWS;
                float d2 = max(dot(Lv, Lv), 0.01);
                float3 L = Lv * rsqrt(d2);
                float3 V = normalize(GetCameraPositionWS() - i.posWS);
                float3 H = normalize(L + V);
                float NoH = saturate(H.y), LoH = saturate(dot(L, H)), NoL = saturate(L.y);
                float dd = NoH * NoH * (rr2 - 1.0) + 1.00001;
                float spec = rr2 / ((dd * dd) * max(0.1, LoH * LoH) * (rr * 4.0 + 2.0));
                float F = 0.02 + 0.98 * pow(max(1.0 - LoH, 0.0), 5.0);
                float rangeW = saturate(1.0 - d2 / 64.0);   // 8 m range window
                float3 sheen = i.color * min(spec * F, 6.0) * NoL * max(_B0.w, 0.0) * rangeW * rangeW / d2 * wet;
                return half4(sheen, dark);
            }

            half4 Frag(Varyings i) : SV_Target
            {
                if (_Mode > 2.5 && _Mode < 3.5) return FragSheet(i);
                if (_Mode > 4.5) return FragStain(i);
                float r2 = dot(i.corner, i.corner);
                if (r2 >= 1.0) discard;
                float a = exp(-4.0 * r2) - 0.0183;   // gaussian, zero at the quad edge
                if (i.soft > 0.0)
                {
                    float2 uv = i.screen.xy / max(i.screen.w, 1e-5);
                    float sceneZ = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
                    a *= saturate((sceneZ - i.screen.z) / i.soft);
                }
                return half4(i.color * max(a, 0.0), 0.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
