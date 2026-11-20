// NocStreak - additive light streaks: traffic, aviation beacons, fireflies (MASTER_PROMPT 14.2.5 NocStreak, 14.7 H7, D-008).
// Abyss InstancedBatch instances (<= 511 per draw, explicit bounds), submitted per camera (NocCameraBatches); every per-instance
// value rides in the instance matrix or comes from a hash of the position (never UNITY_INSTANCING_BUFFER, D-008).
//   _Kind 0 / 1 (lane vehicles, red tails / white heads; LOOK_V2 10: tails lerp(AviationRed, SodiumAmber, 0.25 h k_a), heads
//            lerp(SignWhite, WinNeutral, 0.6 h), no magenta, no cyan): column 0 = lane vector B - A, column 1 = (0, v, 0), column 2 =
//            lateral x (1 + phi), column 3 = A; p = frac(phi + v t / L) with t = _NocTraffic.x (double-wrapped, speeds quantised),
//            alpha ramps smoothstep(0, e, p) (1 - smoothstep(1 - e, 1, p)), streak length v x 0.12 s along the lane.
//            AB (INT-42): x264 4:2:0 smeared 1-2 px saturated red tails into dark-red blobs (KS04 sky clip), so tails are drawn at
//            least TAIL_MIN_PX (3.0) wide and long and their hue moves a further TAIL_AMBER (0.18) toward SodiumAmber.
//   _Kind 2 (static points: beacons): TRS(p, size) with column 2 scaled by (1 + phase); 0.2 Hz pulse (smooth, never off).
//   _Kind 3 (in-air halos, LOOK_V2 6, NocHalos): TRS(p, sigma): a Gaussian of sigma 0.5 / 0.8 m on a quad of +-3 sigma, peak
//            _Tint (the head radiance) x 0.05 x rho_eff, rho_eff = min(2, (sigma_r + sigma_a) / 4.4e-4) x (0.6 + 0.4 rho_rain)
//            from _NocFog, faded out below 3 px of sigma; fogged like the head.
//   _Kind 4 (AB: the H7 traffic layers and the cross-bridge lanes of 14.5.3): the lane matrix of kinds 0 / 1; every vehicle shows
//            its white head pair to a camera in front of it and its red tail pair to a camera behind it (w_head = smoothstep(-0.2,
//            0.2, dot(to camera, motion))), so a layer reads as red tails one way and cool-white heads the other from every view.
//            _Gain = head gain, _Tint = (tail gain, -, lamp pair separation m, lamp width m). The two lamps of a pair are resolved
//            as two lobes from 3 px of separation on (smoothstep(3, 6, sep px)), one merged lobe below; the drawn quad carries the
//            pair's emitting area exactly (energy = emitting area / drawn area, profile mean 1).
//   _Kind 5 (AB: static lights on structure: cross-bridge deck lamps, mast lamps): TRS(p, size), colour _Tint.rgb x _Gain, a
//            steady level 0.8 + 0.2 phase (no pulse, no flicker), pulled toward the camera like kind 2.
//   _Kind 6 (AB: the glow of a traffic layer, the aggregate in-scatter of its lamps in the haze): a strip mesh along the lane
//            (uv.x 0..1 along, uv.y -1..1 across; NocTraffic.RibbonMesh), column 0 = B - A, column 1 = (intensity, sigma m, 0),
//            column 3 = A; the strip turns about the lane axis to face the camera, spans +-3 sigma (sigma drawn at least 1 px), carries
//            a Gaussian of peak _Tint x _Gain x intensity (x true / drawn sigma when widened), fades over the lane's ramps
//            (e = _Ramp at both ends), fades in with the distance (150 -> 600 m: near a layer its vehicles and their halos carry the
//            light) and is fogged per vertex like every light (x T x (1 - cloud mix)).
// Size: the quad is built in clip space, at least 2.2 px in both axes, energy x (true / drawn) per axis (the (true/drawn)^2 rule),
// a soft profile normalised to mean 1 (energy preserving at every distance). Queue Transparent+10 (3010), Blend One One, ZWrite
// off: fogged as light (x T, x (1 - cloud mix): additive light never fades toward the fog colour), no NocFloor (the opaque floor
// is already below it).
Shader "Megastructure/NocStreak"
{
    Properties
    {
        _Kind ("Kind (0 tail lanes, 1 head lanes, 2 points, 3 halos, 4 layer vehicles, 5 static lights, 6 layer glow)", Float) = 0
        _Gain ("Radiance gain (x token)", Float) = 2.0
        _Ramp ("Lane alpha ramp e", Float) = 0.04
        _Size ("Light size (m, lanes: width)", Float) = 0.35
        _Tint ("Halo head radiance (kind 3, linear) / kind 4 (tail gain, -, pair m, lamp m) / kind 5 colour", Vector) = (0, 0, 0, 0)
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Include/NocAtmosphere.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _Kind;
        float _Gain;
        float _Ramp;
        float _Size;
        float4 _Tint;
    CBUFFER_END

    float4 _NocTraffic;   // x = traffic time wrapped mod 65 536 s (double modulo), y = scene time wrapped mod 3 600 s, zw = 0
    // D-034 hook (docs/DREAD.md 3.1, wave B, owner DR with AB): the shadow below the bridge. xy = its centre (world x, z), z = its
    // half-length along z (m; half-width 0.18 of it), w = strength 0..1. Layer vehicles (kind 4) and layer glows (kind 6) below
    // y -450 inside its outline go dark by w; unset (all zero) it is exactly 1 (nothing changes before wave B sets it).
    float4 _NocAbyssOccluder;
    float NocAbyssOcclusion(float3 P)
    {
        float2 hl = max(float2(0.18, 1.0) * _NocAbyssOccluder.z, 1.0);
        float r = length((P.xz - _NocAbyssOccluder.xy) / hl);
        return 1.0 - saturate(_NocAbyssOccluder.w) * (1.0 - smoothstep(0.7, 1.0, r)) * (1.0 - smoothstep(-550.0, -450.0, P.y));
    }

    // AB (INT-42): tails at least 3 px in both axes (2.2 px for every other light), hue a further 18 % toward SodiumAmber
    #define TAIL_MIN_PX 3.0
    #define TAIL_AMBER 0.18
    // AB: the layer glow reads only where many lamp halos overlap on screen: it fades in from RIBBON_NEAR0 to RIBBON_NEAR1 m
    #define RIBBON_NEAR0 150.0
    #define RIBBON_NEAR1 600.0

    // lights closer than this along the view axis are dropped (no quad straddles the camera plane)
    float NocNear() { return 0.1; }

    struct Attributes
    {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 q : TEXCOORD0;        // -1..1 across the quad
        float4 col : TEXCOORD1;      // rgb radiance x energy x alpha x T x (1 - cloud mix)
        float4 shape : TEXCOORD2;    // kind 4: x = pair resolve 0..1, y = lobe centre (q units), z = lobe half-width (q units)
    };

    // Exact screen end points (ndc) of the world segment centre +- dir len / 2, its half-length clamped so both ends keep
    // w >= 0.25 w_centre: the quad spans exactly the projected segment (never a symmetric screen-space extension, which for a light
    // just in front of the camera plane would run through the vanishing point across the whole frame).
    void NocProjectEnds(float3 dir, float len, float4 cC, out float2 na, out float2 nb)
    {
        float4 cD = mul(UNITY_MATRIX_VP, float4(dir, 0.0));
        float h = 0.5 * len;
        if (abs(cD.w) > 1e-6) h = min(h, 0.75 * cC.w / abs(cD.w));
        float4 a = cC - cD * h, b = cC + cD * h;
        na = a.xy / a.w;
        nb = b.xy / b.w;
    }

    // tail colour of kinds 0 and 4 (LOOK_V2 10 base mix, then INT-42's shift toward amber)
    float3 NocTailColour(float h)
    {
        float3 tail = lerp(PAL_AviationRed, PAL_SodiumAmber, 0.25 * h * _NocAmber.x);
        return lerp(tail, PAL_SodiumAmber, TAIL_AMBER);
    }

    Varyings Vert(Attributes i)
    {
        UNITY_SETUP_INSTANCE_ID(i);
        Varyings o = (Varyings)0;
        float4x4 M = UNITY_MATRIX_M;
        float3 c0 = float3(M._m00, M._m10, M._m20), c1 = float3(M._m01, M._m11, M._m21), c2 = float3(M._m02, M._m12, M._m22);
        float3 c3 = float3(M._m03, M._m13, M._m23);
        float2 scr = max(_ScreenParams.xy, 1.0);

        if (_Kind > 5.5)
        {
            // kind 6: the layer glow strip (no per-vehicle state)
            float Lr = max(length(c0), 1e-3);
            float3 dirR = c0 / Lr;
            float3 P = c3 + c0 * i.uv.x;
            float3 toCamR = _WorldSpaceCameraPos.xyz - P;
            float dR = max(length(toCamR), 1e-3);
            float3 side = cross(dirR, toCamR / dR);
            float sl = length(side);
            side = sl > 1e-3 ? side / sl : float3(0.0, 1.0, 0.0);
            float fpR = dR * 2.0 / max(abs(UNITY_MATRIX_P[1][1]) * scr.y, 1e-4);
            float sigma = max(c1.y, 0.01), sigD = max(sigma, fpR), halfW = 3.0 * sigD;     // drawn sigma >= 1 px
            float qy = i.uv.y * 2.0 - 1.0;
            P += side * (qy * halfW);
            o.positionCS = TransformWorldToHClip(P);
            o.q = float2(0.0, qy);
            o.shape = float4(0.0, 1.0, 0.0, 0.0);                                  // the Gaussian's +-3 sigma = the strip
            float ramp = smoothstep(0.0, _Ramp, i.uv.x) * (1.0 - smoothstep(1.0 - _Ramp, 1.0, i.uv.x)) * smoothstep(RIBBON_NEAR0, RIBBON_NEAR1, dR);
            float TR = NocFogT(P);
            float cloudR = 1.0 - smoothstep(NC_CloudMixY0, NC_CloudMixY1, P.y);
            o.col = float4(_Tint.rgb * (_Gain * c1.x * (sigma / sigD) * ramp * TR * cloudR * NocAbyssOcclusion(P)), 1.0);   // energy x true / drawn
            return o;
        }
        bool layer = _Kind > 3.5 && _Kind < 4.5;
        bool lane = _Kind < 1.5 || layer;
        bool halo = _Kind > 2.5 && _Kind < 3.5;
        float3 centre, dir;
        float trueLen, trueW, alpha;
        float3 colour;
        float minPx = NC_StreakMinPx;
        float emit = 1.0;                 // emitting fraction of the true quad (kind 4: two lamps across the pair width)
        float headW = 0.0;                // kind 4: weight of the head pair (0 = tails)
        uint seed;
        if (lane)
        {
            float L = max(length(c0), 1e-3);
            dir = c0 / L;
            float v = c1.y;
            float phi = length(c2) - 1.0;
            float p = frac(phi + v * _NocTraffic.x / L);
            centre = c3 + c0 * p;
            alpha = smoothstep(0.0, _Ramp, p) * (1.0 - smoothstep(1.0 - _Ramp, 1.0, p));
            trueLen = v * NC_StreakSeconds;
            trueW = _Size;
            seed = NocPcg3d(asuint(float3(c3.x, c3.z, phi))).x;
            float h = NocU01(seed);
            float3 head = lerp(PAL_SignWhite, PAL_WinNeutral, 0.6 * h);
            if (layer)
            {
                // the camera sees the front of the vehicle (heads) or its back (tails)
                float3 toCam = normalize(_WorldSpaceCameraPos.xyz - centre);
                headW = smoothstep(-0.2, 0.2, dot(toCam, dir));
                colour = lerp(NocTailColour(h) * _Tint.x, head * _Gain, headW);
                float lamp = max(_Tint.w, 0.05), pair = max(_Tint.z, 0.0);
                trueW = pair + lamp;
                trueLen = max(trueLen, lamp);
                emit = 2.0 * lamp / trueW;
                minPx = lerp(TAIL_MIN_PX, NC_StreakMinPx, headW);
            }
            else
            {
                colour = _Kind < 0.5 ? NocTailColour(h) : head;
                if (_Kind < 0.5) minPx = TAIL_MIN_PX;
                colour *= _Gain;
            }
        }
        else
        {
            float size = max(length(c0), 1e-3);
            float phase = length(c2) / size - 1.0;
            centre = c3;
            dir = float3(1.0, 0.0, 0.0);
            if (_Kind < 2.5)
            {
                trueLen = size; trueW = size;
                float pulse = 0.5 + 0.5 * sin(6.28318530718 * (NC_CityBeaconHz * _NocTraffic.y + phase));
                alpha = 0.3 + 0.7 * pulse * pulse;                               // 0.2 Hz, smooth, never dark
                colour = PAL_AviationRed * _Gain;
            }
            else if (halo)
            {
                // halo: size = sigma (m); the quad spans +-3 sigma; rho_eff from the local fog (regional + alley haze) and rain
                trueLen = 2.0 * NC_HaloExtent * size; trueW = trueLen;
                float rhoEff = min(2.0, (_NocFog.x + _NocFog.y) / NC_HaloSigmaRef) * (0.6 + 0.4 * _NocFog.w);
                alpha = NC_HaloPeak * rhoEff;
                colour = _Tint.rgb * _Gain;
            }
            else
            {
                // kind 5: a steady light on a structure
                trueLen = size; trueW = size;
                alpha = 0.8 + 0.2 * saturate(phase);
                colour = _Tint.rgb * _Gain;
            }
        }

        // points sit on facade corners: pull them toward the camera by their drawn radius so the wall never clips them
        float3 toCam = _WorldSpaceCameraPos.xyz - centre;
        float dist = max(length(toCam), 1e-3);
        float fp = dist * 2.0 / max(abs(UNITY_MATRIX_P[1][1]) * scr.y, 1e-4);   // metres per pixel at the light
        if (!lane) centre += toCam / dist * min(0.5 * max(trueW, NC_StreakMinPx * fp), 0.5 * dist);
        if (halo) alpha *= smoothstep(NC_HaloFadePx0, NC_HaloFadePx1, trueW / (2.0 * NC_HaloExtent * fp));   // sigma in px

        float4 cC = TransformWorldToHClip(centre);
        float w = max(cC.w, 1e-4);
        float2 nc = cC.xy / w, na = nc, nb = nc;
        if (lane) NocProjectEnds(dir, trueLen, cC, na, nb);
        float2 pa = na * 0.5 * scr, pb = nb * 0.5 * scr;                        // px
        float2 seg = pb - pa;
        float lenPx = lane ? length(seg) : trueLen / fp;
        float2 ax = (lane && lenPx > 1e-4) ? seg / lenPx : float2(1.0, 0.0);
        float wPx = trueW / fp;
        float drawLen = max(lenPx, minPx), drawW = max(wPx, minPx);
        if (layer)
        {
            // the lamp pair: two lobes of half-width max(lamp / 2, 0.75 px) at +-sep / 2, resolved from 3 px of separation
            float sepPx = max(_Tint.z, 0.0) / fp, lobe = max(0.5 * max(_Tint.w, 0.05) / fp, 0.75);
            drawW = max(drawW, sepPx + 2.0 * lobe);
            o.shape = float4(smoothstep(3.0, 6.0, sepPx), sepPx / drawW, 2.0 * lobe / drawW, 0.0);
        }
        float energy = (lenPx / drawLen) * (wPx / drawW) * emit;
        float2 nr = float2(-ax.y, ax.x);
        float2 q = i.uv * 2.0 - 1.0;
        float2 mid = 0.5 * (pa + pb);                                          // grows symmetrically to the 2.2 px minimum
        float2 px = mid + ax * (0.5 * q.x * drawLen) + nr * (0.5 * q.y * drawW);
        o.positionCS = float4(px * (2.0 / scr) * w, cC.z, w);                  // the centre's depth for the whole quad
        if (cC.w < NocNear()) o.positionCS = float4(0.0, 0.0, -1.0, 1.0);          // behind or at the camera: clipped away
        o.q = q;

        float T = NocFogT(centre);
        float cloud = 1.0 - smoothstep(NC_CloudMixY0, NC_CloudMixY1, centre.y);
        float occ = layer ? NocAbyssOcclusion(centre) : 1.0;                     // D-034 hook (1 until wave B sets it)
        o.col = float4(colour * (energy * alpha * T * cloud * occ), 1.0);
        return o;
    }

    half4 Frag(Varyings i) : SV_Target
    {
        // soft profile (1 - x^2)(1 - y^2) normalised to mean 1 (9 / 4): the drawn quad carries exactly the energy above;
        // halos (kind 3): a Gaussian of peak 1 over +-3 sigma, lowered by its edge value per axis so the quad has no rim;
        // layer vehicles (kind 4): across the quad one merged lobe or the two lamp lobes (each (1 - t^2) of half-width z, the pair
        // normalised to mean 1 over the quad: two lobes integrate to 2 x (4/3) z over a width of 2)
        float2 a = saturate(1.0 - i.q * i.q);
        float prof;
        if (_Kind > 5.5)
        {
            // kind 6: a Gaussian across the strip, +-3 sigma = +-shape.y, lowered by its edge value (no rim)
            float t6 = i.q.y / max(i.shape.y, 1e-3);
            float e6 = exp(-4.5);
            prof = saturate((exp(-4.5 * t6 * t6) - e6) / (1.0 - e6));
        }
        else if (_Kind > 2.5 && _Kind < 3.5)
        {
            float e0 = exp(-0.5 * NC_HaloExtent * NC_HaloExtent);
            float2 g = saturate((exp(-0.5 * NC_HaloExtent * NC_HaloExtent * i.q * i.q) - e0) / (1.0 - e0));
            prof = g.x * g.y;
        }
        else if (_Kind > 3.5 && _Kind < 4.5)
        {
            float hz = max(i.shape.z, 1e-3);
            float t1 = (i.q.y - i.shape.y) / hz, t2 = (i.q.y + i.shape.y) / hz;
            float pairProf = (saturate(1.0 - t1 * t1) + saturate(1.0 - t2 * t2)) / ((4.0 / 3.0) * hz);
            prof = 1.5 * a.x * lerp(1.5 * a.y, pairProf, i.shape.x);
        }
        else prof = a.x * a.y * 2.25;
        return half4(i.col.rgb * prof, 0.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent+10" "IgnoreProjector" = "True" }
        Pass
        {
            Name "Streak"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off ZTest LEqual Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            ENDHLSL
        }
    }
    Fallback Off
}
