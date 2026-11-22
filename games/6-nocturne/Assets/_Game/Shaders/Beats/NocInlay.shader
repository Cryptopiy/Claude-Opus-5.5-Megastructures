// NocInlay - the brass inlay of Heart Square's plaza (MASTER_PROMPT 14.5.7 "plaza", INT-12; owner HT, Phase 4 wave A). A
// transparent lit overlay drawn on the plaza slab's own top fan (HeartFloorBuilder.TopFan: the same vertices and triangles as the
// NocWet floor under the same transform, so ZTest LEqual passes exactly on the floor and nowhere else; ZWrite Off; queue 2501:
// after the opaque floor, before every other transparent, so halos and rain stay on top). Between the lines nothing is drawn and
// NocWet's analytic mirror shows unchanged.
//   * ring lines _Ring.y wide every _Ring.x about _Centre, from the radius _Ring.z to _Ring.w (brass);
//   * radial seams every 2 pi / _Seam.x rad, _Seam.y wide, beyond the radius _Seam.z (a darker brass cap: x _Seam.w);
//   * each line is the exact box coverage of its strip by the pixel footprint measured along the line's own normal (radial for the
//     rings, tangential for the seams; NocCover), so a sub-pixel line keeps its energy (w / footprint) and never aliases; each
//     family fades to nothing where its pitch falls below _FadePx.x pixels (full above _FadePx.y): no moire toward the horizon;
//   * aged brass: metallic, F0 _Brass, smoothness _Smooth, alpha = coverage x fade; fragments under 1/512 coverage are discarded
//     before the lighting. HT-04 (fourth start): the template's NocShade mirrored only the dark sky, so the lines vanished into
//     the wet stone; the brass now mirrors the wet floor's own analytic world (14.7 H4, NocMirror.hlsl, read-only include as for
//     NocGlass): the lobe of its (spec-AA filtered) smoothness, the sky and wall planes (NocMirrorEnv) and the H4 sources
//     (NocMirrorSources: lamps, the goal window, and whatever FX adds there), weighted by the brass Fresnel (Schlick, F0 = _Brass)
//     and Smith G1 instead of water's F: lamp and window streaks run along the lines, brighter than on the stone. Energy: F <= 1
//     and every source is clamped to L_eff 3, so a line is never brighter than what it mirrors (no fireflies).
// C# twin of the pattern: HeartFloorBuilder.InlayCoverage (Beat07HeartTests). No keyword, no sampler, derivatives before the clip.
Shader "Megastructure/NocInlay"
{
    Properties
    {
        _Centre ("Centre (world x, -, z)", Vector) = (1330, 0, 388, 0)
        _Ring ("Rings: pitch, width, first r, last r (m)", Vector) = (20, 0.12, 60, 380)
        _Seam ("Seams: count, width (m), from r (m), tint", Vector) = (48, 0.06, 58, 0.6)
        _FadePx ("Pitch fade (px): none below x, full above y", Vector) = (6, 10, 0, 0)
        _Brass ("Brass F0 (linear)", Color) = (0.5, 0.42, 0.3, 1)
        _Smooth ("Smoothness", Range(0, 0.95)) = 0.7
    }

    HLSLINCLUDE
    #include "../Include/NocLitCore.hlsl"
    #include "../Include/NocMirror.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float4 _Centre;
        float4 _Ring;
        float4 _Seam;
        float4 _FadePx;
        float4 _Brass;
        float _Smooth;
    CBUFFER_END

    // Coverage of the ring and seam lines at the world point P (x: total, y: the seams' share). Takes derivatives: call it before
    // any branch. The footprints are box widths (|d.dx| + |d.dy|) along the radial and the tangential direction.
    float2 NocInlayCover(float3 P)
    {
        float2 d = P.xz - _Centre.xz;
        float r = max(length(d), 1e-3);
        float2 rh = d / r;
        float2 th = float2(-rh.y, rh.x);
        float2 dx = ddx(P.xz), dy = ddy(P.xz);
        float fwR = max(abs(dot(dx, rh)) + abs(dot(dy, rh)), 1e-5);
        float fwT = max(abs(dot(dx, th)) + abs(dot(dy, th)), 1e-5);
        // rings: the nearest ring k pitch, inside [first, last]
        float pitch = max(_Ring.x, 1e-3);
        float rk = round(r / pitch) * pitch;
        float inRange = step(_Ring.z - 1e-3, rk) * step(rk, _Ring.w + 1e-3);
        float aRing = NocCover(r - rk, fwR, -0.5 * _Ring.y, 0.5 * _Ring.y) * smoothstep(_FadePx.x, _FadePx.y, pitch / fwR) * inRange;
        // seams: the signed arc to the nearest seam angle (continuous across atan2's cut: 2 pi / count divides 2 pi)
        float dth = 6.28318531 / max(_Seam.x, 1.0);
        float ang = atan2(d.y, d.x);
        float arc = (ang - round(ang / dth) * dth) * r;
        float aSeam = NocCover(arc, fwT, -0.5 * _Seam.y, 0.5 * _Seam.y) * smoothstep(_FadePx.x, _FadePx.y, r * dth / fwT) * step(_Seam.z, r);
        aSeam *= 1.0 - aRing;
        return float2(aRing + aSeam, aSeam);
    }

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        float2 a = NocInlayCover(f.positionWS);
        clip(a.x - 1.0 / 512.0);
        float seamShare = a.y / max(a.x, 1e-4);
        s.albedo = (half3)(_Brass.rgb * lerp(1.0, _Seam.w, seamShare));
        s.metallic = 1.0h;
        s.smoothness = (half)_Smooth;
        s.occlusion = 1.0h;
        s.alpha = (half)saturate(a.x);
        f.normalWS = float3(0.0, 1.0, 0.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-499" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend One OneMinusSrcAlpha
            ZWrite Off ZTest LEqual Cull Back
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #define NOC_PASS_FORWARD
            #define NOC_HAS_FORWARD_HOOK
            #define NOC_TRANSPARENT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Include/NocLighting.hlsl"

            // The brass line's straight radiance (NOC_TRANSPARENT premultiplies by the coverage): the wet floor's analytic mirror
            // through the brass Fresnel. The lobe is built first, outside any branch (derivatives).
            half3 NocForwardHook(NocFragData f, SurfaceData s, InputData inputData)
            {
                float a = (1.0 - s.smoothness) * (1.0 - s.smoothness);              // GGX alpha of the filtered smoothness
                float sigma2 = a * a + NC_WetSigmaFloor;                              // the floor's lobe variance (H4)
                NocLobe o = NocMirrorLobe(f.positionWS, _WorldSpaceCameraPos.xyz, inputData.normalWS, sigma2, 1.0);
                float g1 = NocG1(o.nv, sqrt(0.5 * sigma2));
                o.F = 1.0;
                o.FG = g1;                                                            // NocMirrorSources returns G1 x sources
                float3 F = s.albedo + (1.0 - s.albedo) * PositivePow(1.0 - o.nv, 5.0);   // Schlick, F0 = the brass
                float3 c = F * (g1 * NocMirrorEnv(f.positionWS, f.viewDist, o) + NocMirrorSources(f.positionWS, f.viewDist, o));
                return (half3)min(c, 1000.0);
            }

            #include "../Include/NocLitPass.hlsl"
            ENDHLSL
        }
    }
}
