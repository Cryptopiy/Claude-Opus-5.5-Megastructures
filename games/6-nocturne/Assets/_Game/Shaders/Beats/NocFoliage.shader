// NocFoliage - the crowns of the hanging garden (MASTER_PROMPT 14.2.5, 14.5.5; Look v2: TEXTURE_PLAN 4, LOOK_V2 5.1 and 10) through
// the Noc lit template (trilight x LightCal, Forward+ lights, analytic sky reflection, specular AA). Three modes, chosen per material
// by _LeafMode (a material constant: every branch on it is uniform; no keyword). GD (Phase 4) deleted the v1 glowing crown
// (mode 0 and its noise cells; mode 0 now draws as the core) and added the lanterns, the fireflies, the leaf sway and the lantern
// field (GardenLanterns.cs):
//   vertex hook  cards (2): leaf sway, a horizontal wind displacement of _SwayAmp (0.25 m at the crown tips: the cards are the
//               crown's outer leaves) with period _SwayPeriod (7 s) plus a 0.618x slower partial (both <= 0.2 Hz), phase from the
//               world position (gust fronts about 30 m long) and the card hash; the core, trunks and limbs stay still.
//               Lanterns (3) and fireflies (4): the ball centre is position - normal x radius (uv0.x; radial normals); the ball
//               grows about it to a 2.2 px diameter (_MinPx) where it would be smaller, and custom.x = (r / r')^2 keeps its
//               energy (no sparkle, Part 8.6); the post lanterns swing with half the sway; fireflies wander 1.5 m (0.05 m/s) and
//               orbit 0.3 m on the scene clock (_NocTime.x, so -freeze-clock freezes them). Integration round (QA-F13 / SG, G5 on
//               the KS16 hold): the TREE lanterns (crown in uv1) hang still. Their 6 cm swing is 1-3 px at 25-40 m; a 2.2-8 px ball
//               moving by a pixel or two re-rasterises at 0.6-1.2 Hz (amplitude 0.023 in 5 px block means at KS16's top left),
//               and it moved them across the fringe-fade bands (the f928 block pop); still, their only modulation is the 0.1 Hz
//               pulse, and a fade changes only with the eye.
//               GD fix wave (C32, panel p4_01): the floor holds for the ball's INSCRIBED sphere (an icosahedron's inradius is
//               0.795 of the circumradius r: at the old circumradius floor the silhouette was 1.75-2.2 px) and a ball drawn under
//               _FlatPx px is shaded flat (custom.z = its drawn diameter in px): the hot face-on profile 0.55 + 0.9 (n.v)^2 on a
//               2-px disc put the centre pixel 100-137 codes over all 24 neighbours (image_metrics NaN-like; it twinkles as the
//               disc crosses pixel centres). A flat disc of >= 2.2 px under MSAA 4x: centre <= 69 codes over the best neighbour at
//               any sub-pixel offset and a constant peak (GD sim, QA/GD_lantern_sim.py), so no isolated maxima and no twinkle.
//               Tree lanterns behind their own crown (GD r06: the 13 KS15 maxima left were whole lanterns seen through 1-px gaps
//               of the fringe cards, (233, 166, 87) among leaves at 50): uv1 = (crown centre x, z, y, horizontal radius Rh),
//               colour A = Ry / (2 Rh); in the crown's ellipsoid space the eye ray to the lantern centre comes within q of the
//               crown centre; when it passes deeper than the lantern itself (|l| - q > 0) and inside the leaf fringe (q < 1.18:
//               lobes to 1.031, card shell 1.02, cards 2 m on a 20 m crown) the lantern is hidden by leaves: its emission fades
//               (1 - max(smoothstep(0, 0.05, |l| - q), smoothstep(0.05, 0.12, (1 - t*) |l - o|)) x (1 - smoothstep(1.08, 1.18, q)):
//               the second term catches a low lantern whose ray grazes the fringe 1-2.4 m before reaching it; t* = the parameter of
//               the closest approach on the eye-lantern segment) and a faded ball shrinks to its centre over fade 0.02..0 (no
//               raster: neither a peek nor a dark speck; integration round: the old switch at fade 0.01 was a one-frame step). The same "grazes the fringe before it" test runs against the other 16 crowns
//               (_NocGardenCrowns, a global array set once at build): far lanterns behind the near crowns' fringes.
//   lantern field  modes 1 and 2: colour A = the lanterns' irradiance / _FieldScale (GardenLanterns.ApplyField); added as the
//               light scattered through the canopy: lum(albedo) x E x _LanternLightGain x lerp(Lantern, WinNeutral, 0.4) x the
//               leaf chroma raised to a power that goes from 1 next to a strand (E >= _NearWarm: the light has crossed one leaf,
//               the leaves beside a strand turn LeafWarm) to _ScatterPow deep in the crown (the light has crossed many leaves);
//               plus the faint glow of the down-facing leaves, lerp(LeafGlow, LeafWarm, _UnderWarm) x _UnderGlow x albedo / Leaf
//               (GD fix wave C33: pure LeafGlow pulled the canopy to a mint hue 138; LOOK_V2 2.1's Leaf is yellow-green, hue 93).
// 3, LANTERNS: emission Lantern x _LanternGain (2.0) x (1 +- _LanternPulse 6 % at _LanternHz 0.1, phase = lantern hash) x energy
//   x (0.35 + 1.3 (n.v)^2: the hot face-on paper, disc mean 1.0) x the per-lantern warmth (vertex colour); no albedo.
//   Under _FlatPx (custom.z) the profile is flat 1.0 (same disc mean: no energy change; C32).
// 4, FIREFLIES: emission lerp(LeafWarm, LeafGlow, hash) x _FireflyGain x a slow glow (0.55 +- 0.45 at _FireflyHz <= 0.2 Hz) x
//   energy, faded out nearer than 1.5-4 m.
// 5, BARK (GD fix wave C33: the limbs under the crowns read as smooth black tubes): the CC0 bark photo of lib.Bark (trident maple,
//   texprep: _BarkTex / _BarkNormalTex, the variation around 0.5) on the limbs' UV0 metres / _BarkTile, albedo _BarkColor x 2 x the
//   variation, normals from the photo through a derivative tangent frame, the template's PBR terms, plus the lantern field (colour A)
//   as the direct light of the lanterns around the limb: albedo x E x _LanternLightGain x lerp(Lantern, WinNeutral, 0.4).
// 1, the v2 CORE (lib.LeafCore, queue 2000): the opaque lobed crown at 0.90 seen through the card gaps = the deep leaves: two
//   layers of the LeafCrown atlas (sprites of 2 m, the second offset by half a sprite, a hashed sprite per 2 m cell; the sprite
//   borders are empty, so the cell edges never show) planar-projected on the axis plane facing the smooth core normal, with
//   gradients from ddx / ddy of the position (continuous across the axis switch); albedo = Leaf x 2 x the leaf colour, x
//   lerp(_CoreGap 0.25, 1, cover) (the gaps between those leaves are deeper still; r03 had 0.45 and a lobe facing the camera
//   still read as a smooth dome), occlusion = _CoreAo x the lobe-valley AO of
//   vertex colour R (GardenTree.CoreV2); no emission. Mips average cover and colour (no sharpening: no shimmer).
// 2, the v2 CARDS (lib.Leaves, queue 2450, AlphaToMask On under MSAA 4x, Cull Off): one sprite of the LeafCrown atlas (a
//   Texture2DArray of 4 cluster sprites, texprep; leaves at true size) per card (GardenLeafCards: uv0 sprite uv, uv1.x slice,
//   uv1.y card hash, uv1.zw the octahedral card normal, normal = the ellipsoid normal, tangent = card u, colour R AO, G glow card,
//   B sky exposure):
//   alpha       Golus 2017 sharpening a' = (a - 0.5) / max(fwidth(a), 1e-4) + 0.5 while a leaf covers 2 px or more (mip <=
//               _SharpMip; a 0.15 m leaf is 75 texels at mip 0), faded to the coverage-preserving mip alpha over 1.5 mips
//               (lerp(a', a, saturate((mip - _SharpMip) / 1.5))): FO lookdev r01 faded it by mip 3 as TEXTURE_PLAN 4 drafted and the
//               KS15 fringe became a grey-green veil of dithered soft alpha around 3 px leaves; _AlphaMipBias is the moire fallback;
//   albedo      Leaf x 2 x variation (mean 0.5: the token is the mean) x a per-card hue (+-8 %) and value (+-6 %) jitter;
//   normal      n = normalize(lerp(faceSign x card normal, ellipsoid normal, _BendNormal 0.7)) + the atlas normal as a surface
//               gradient along the card's u / v (x faceSign: a thin leaf's back is the mirrored front), strength 0.8;
//   occlusion   vertex AO x lerp(1, atlas AO, 0.8); smoothness = atlas smoothness + _WetSmooth (rain sheen), <= 0.75;
//   light       the template's PBR terms plus thin-leaf transmission (8.11 item 7): albedo x _Transmission x (sky exposure x
//               (trilight(-n_view) + fill) + the local lights), each x saturate(L . -n_view) where n_view is the card normal
//               turned to the viewer, x LightCal (the core above shadows the underside cards: exposure, no shadow maps);
//   emission    LeafGlow specks and the LeafGlow rim on the 2.5 % bioluminescent cards only (specks: value noise in sprite space
//               above _SpeckThreshold on the upper leaves, faded to its mean by the footprint; FO lookdev r02: a rim on every card
//               outlined each lobe of the lobed crowns with a jade ring, 21-24 codes against 13-16 inside), LeafWarm tips on all,
//               at the LOOK_V2 10 gains (0.012, 0.015, 0.03 on the v2 materials).
// Derivatives (uv, alpha) and the atlas taps come first, before every branch. NocFog and NocFloor last (NocLitPass). Colours are
// PAL_* tokens (D-014); gains are linear. Every division is guarded; the noise is the PCG twin of GardenTree.Noise3.
Shader "Megastructure/NocFoliage"
{
    Properties
    {
        _AlbedoGain ("Leaf albedo (x Moss token in v1, x Leaf in v2)", Range(0, 2)) = 0.6
        _Smoothness ("Smoothness", Range(0, 0.95)) = 0.3
        _GlowGain ("Leaf glow gain (x LeafGlow)", Float) = 0.042
        _TipGain ("Tip glow gain (x LeafWarm)", Float) = 0.08
        _RimGain ("Rim gain (x LeafGlow)", Float) = 0.06
        _RimPower ("Rim power", Float) = 3
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        _LeafMode ("Mode (1 core, 2 cards, 3 lanterns, 4 fireflies, 5 bark; 0 = core)", Float) = 1
        _AlphaToMask ("Alpha to coverage (0 off, 1 on)", Float) = 0
        _AlbedoTex ("Leaf atlas albedo variation (a = opacity)", 2DArray) = "" {}
        _NormalTex ("Leaf atlas normal (GL)", 2DArray) = "" {}
        _MaskTex ("Leaf atlas mask (G AO, B height, A smoothness)", 2DArray) = "" {}
        _NormalStrength ("Atlas normal strength", Range(0, 2)) = 0.8
        _BendNormal ("Bent normal toward the ellipsoid", Range(0, 1)) = 0.7
        _HueJitter ("Per-card hue jitter", Range(0, 0.2)) = 0.08
        _Transmission ("Thin-leaf transmission (x albedo)", Range(0, 3)) = 1.2
        _WetSmooth ("Rain sheen (smoothness added)", Range(0, 0.5)) = 0.2
        _CoreAo ("Core occlusion (crown interior)", Range(0, 1)) = 0.55
        _CoreGap ("Core: brightness of the gaps between its leaves", Range(0, 1)) = 0.25
        _SpeckThreshold ("Speck threshold", Range(0, 1)) = 0.8
        _AlphaMipBias ("Alpha mip bias (moire fallback)", Range(0, 1)) = 0
        _SharpMip ("Alpha sharpening kept up to this mip (leaf >= 2 px)", Range(0, 8)) = 5
        _SwayAmp ("Leaf sway at the crown tips (m)", Range(0, 1)) = 0.25
        _SwayPeriod ("Leaf sway period (s)", Range(1, 20)) = 7
        _LanternGain ("Lantern emission (x Lantern)", Float) = 2.0
        _LanternPulse ("Lantern pulse depth", Range(0, 0.2)) = 0.06
        _LanternHz ("Lantern pulse (Hz)", Range(0, 0.2)) = 0.1
        _MinPx ("Lantern / firefly minimum diameter (px)", Range(0, 6)) = 2.2
        _LanternLightGain ("Lantern field light on the leaves", Range(0, 4)) = 1.2
        _ScatterPow ("Leaf chroma power of the scattered lantern light", Range(1, 5)) = 2.5
        _FieldScale ("Lantern field scale (E = A x scale)", Float) = 4
        _UnderGlow ("Jade glow of the down-facing leaves (x LeafGlow)", Range(0, 0.2)) = 0.045
        _FireflyGain ("Firefly emission", Float) = 8
        _FireflyHz ("Firefly glow (Hz)", Range(0, 0.2)) = 0.12
        _FlatPx ("Lantern: flat profile below this drawn diameter (px)", Range(0, 16)) = 3
        _UnderWarm ("Under-glow hue: lerp(LeafGlow, LeafWarm, x)", Range(0, 1)) = 0.65
        _NearWarm ("Field: E at which the scattered light keeps the lantern's warmth", Range(0.1, 8)) = 1.6
        [NoScaleOffset] _BarkTex ("Bark albedo variation (photo, mode 5)", 2D) = "grey" {}
        [NoScaleOffset] _BarkNormalTex ("Bark normal (GL, mode 5)", 2D) = "bump" {}
        _BarkColor ("Bark token (linear, mode 5)", Color) = (0.071, 0.054, 0.045, 1)
        _BarkTile ("Bark tile (m, mode 5)", Float) = 1.8
        _BarkNormalStrength ("Bark normal strength (mode 5)", Range(0, 2)) = 1
    }

    HLSLINCLUDE
    #include "../Include/NocLitCore.hlsl"
    #define NOC_PASS_UV1ZW
    #define NOC_HAS_VERTEX_HOOK

    CBUFFER_START(UnityPerMaterial)
        float _AlbedoGain;
        float _Smoothness;
        float _GlowGain;
        float _TipGain;
        float _RimGain;
        float _RimPower;
        float _Cull;
        float _LeafMode;
        float _AlphaToMask;
        float _NormalStrength;
        float _BendNormal;
        float _HueJitter;
        float _Transmission;
        float _WetSmooth;
        float _CoreAo;
        float _SpeckThreshold;
        float _AlphaMipBias;
        float _SharpMip;
        float _CoreGap;
        float _SwayAmp;
        float _SwayPeriod;
        float _LanternGain;
        float _LanternPulse;
        float _LanternHz;
        float _MinPx;
        float _LanternLightGain;
        float _ScatterPow;
        float _FieldScale;
        float _UnderGlow;
        float _FireflyGain;
        float _FireflyHz;
        float _FlatPx;
        float _UnderWarm;
        float _NearWarm;
        float _BarkTile;
        float _BarkNormalStrength;
        float4 _BarkColor;
        float4 _AlbedoTex_TexelSize;
    CBUFFER_END

    TEXTURE2D_ARRAY(_AlbedoTex);
    SAMPLER(sampler_AlbedoTex);
    TEXTURE2D_ARRAY(_NormalTex);
    TEXTURE2D_ARRAY(_MaskTex);
    // the 17 crowns of the garden (GardenLanterns.SetCrownGlobals at build: even = centre xyz, odd = radii xyz; count in
    // _NocGardenCrownCount, 0 = none): a tree lantern behind ANOTHER crown's leaves fades like one behind its own (C32)
    float4 _NocGardenCrowns[34];
    float _NocGardenCrownCount;
    TEXTURE2D(_BarkTex);                                                      // mode 5; sampled with sampler_AlbedoTex (Repeat)
    TEXTURE2D(_BarkNormalTex);

    // 3D value noise in [0, 1], quintic, PCG lattice (twin of GardenTree.Noise3: NocPcg(x + NocPcg(y + NocPcg(z))))
    float NocFoliageHash(int3 i) { return NocU01(NocPcg((uint)i.x + NocPcg((uint)i.y + NocPcg((uint)i.z)))); }
    float NocFoliageNoise(float3 p)
    {
        float3 f = floor(p);
        int3 i = int3(f);
        float3 t = p - f;
        float3 u = t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
        float a = lerp(NocFoliageHash(i), NocFoliageHash(i + int3(1, 0, 0)), u.x);
        float b = lerp(NocFoliageHash(i + int3(0, 1, 0)), NocFoliageHash(i + int3(1, 1, 0)), u.x);
        float c = lerp(NocFoliageHash(i + int3(0, 0, 1)), NocFoliageHash(i + int3(1, 0, 1)), u.x);
        float d = lerp(NocFoliageHash(i + int3(0, 1, 1)), NocFoliageHash(i + int3(1, 1, 1)), u.x);
        return lerp(lerp(a, b, u.y), lerp(c, d, u.y), u.z);
    }

    // weight of a pattern of cell size `cell` (m) at footprint fp (m / px): 1 above 4 px per cell, 0 below 1.5 px
    float NocFoliageKeep(float cell, float fp) { return smoothstep(1.5, 4.0, cell / max(fp, 1e-5)); }

    // octahedral decode (twin of GardenLeafCards.OctDecode)
    float3 NocFoliageOctDecode(float2 e)
    {
        float3 n = float3(e.x, e.y, 1.0 - abs(e.x) - abs(e.y));
        float t = saturate(-n.z);
        n.xy += float2(n.x >= 0.0 ? -t : t, n.y >= 0.0 ? -t : t);
        return normalize(n);
    }

    // leaf sway (vertex; see the header): metres of displacement at world position p for a card of hash h
    float3 NocFoliageSway(float3 p, float h)
    {
        float t = _NocTime.x;
        float w = 6.2831853 / max(_SwayPeriod, 0.5);
        float ph = dot(p.xz, float2(0.17, 0.11)) + 1.5 * h;
        float s1 = sin(w * t + ph), s2 = sin(0.618 * w * t + 1.7 * ph + 2.0);
        float a = _SwayAmp * (0.75 * s1 + 0.25 * s2);
        return float3(0.8 * a, 0.15 * _SwayAmp * s2, 0.6 * a);
    }

    // the light under the crowns (modes 1 and 2; see the header): the lantern field's warm light scattered through the canopy
    // (E = colour A x _FieldScale) plus the faint jade glow of the leaves facing down (LeafGlow x _UnderGlow, the garden's
    // bioluminescence, LOOK_V2 beat 5), both x the leaf albedo relative to the Leaf token (the leaf texture carries through)
    float3 NocFoliageFieldLight(float3 albedo, float a, float ny)
    {
        const float3 lumW = float3(0.2126, 0.7152, 0.0722);
        float lum = max(dot(albedo, lumW), 1e-5);
        float E = a * _FieldScale;
        // next to a strand (E >= _NearWarm) the light has crossed one leaf: chroma^1 (warm, LeafWarm); deep in the crown _ScatterPow
        float pw = lerp(max(_ScatterPow, 1.0), 1.0, smoothstep(0.25 * _NearWarm, _NearWarm, E));
        float3 sc = pow(max(albedo / lum, 1e-3), pw);
        sc /= max(dot(sc, lumW), 1e-4);
        float3 L = lerp(PAL_Lantern, PAL_WinNeutral, 0.4);
        float rel = lum / max(dot(PAL_Leaf, lumW), 1e-4);
        float under = saturate(0.2 - 0.8 * ny);
        float3 glow = lerp(PAL_LeafGlow, PAL_LeafWarm, _UnderWarm);
        glow *= dot(PAL_LeafGlow, lumW) / max(dot(glow, lumW), 1e-4);        // the hue moves, the luminance stays LeafGlow's
        return L * sc * (lum * E * _LanternLightGain) + glow * (_UnderGlow * rel * under);
    }

    void NocVertexHook(inout NocVertexData v)
    {
        [branch] if (_LeafMode > 2.5 && _LeafMode < 4.5)
        {
            float r = max(v.uv.x, 1e-4);
            float h = v.uv.y;
            float3 n = v.normalWS;
            float3 c = v.positionWS - n * r;
            float t = _NocTime.x;
            [branch] if (_LeafMode > 3.5)
            {
                // fireflies: a slow wander (1.5 m at 0.033 rad/s = 0.05 m/s) and a 0.3 m orbit (period 6-12 s)
                float h2 = frac(h * 13.37), h3 = frac(h * 71.93);
                float w1 = 0.033 * (0.8 + 0.4 * h2);
                c += 1.5 * float3(sin(w1 * t + 6.2831853 * h), 0.35 * sin(0.7 * w1 * t + 6.2831853 * h2), cos(0.9 * w1 * t + 6.2831853 * h3));
                float wo = 6.2831853 / (6.0 + 6.0 * h2);
                c += 0.3 * float3(cos(wo * t + 6.2831853 * h3), 0.3 * sin(2.0 * wo * t), sin(wo * t + 6.2831853 * h3));
            }
            else if (v.custom.w <= 2.0) c += 0.5 * NocFoliageSway(c, h);    // post lanterns swing; tree lanterns hang still (QA-F13)
            float d = length(_WorldSpaceCameraPos.xyz - c);
            float fp = d * 2.0 / max(abs(UNITY_MATRIX_P[1][1]) * max(_NocScreen.x, 1.0), 1e-4);
            // C32: the floor on the inscribed sphere (icosahedron inradius 0.795 r), so the silhouette is never under _MinPx
            const float inK = 0.795;
            float k = max(1.0, 0.5 * _MinPx * fp / (inK * r));
            float fade = 1.0;
            [branch] if (_LeafMode < 3.5 && v.custom.w > 2.0)
            {
                // tree lanterns: hidden behind the leaf fringe of their own crown (see the header)
                float3 R = float3(v.custom.w, max(2.0 * v.color.a * v.custom.w, 0.5), v.custom.w);
                float3 C = float3(v.uv.z, v.custom.z, v.uv.w);
                float3 o = (_WorldSpaceCameraPos.xyz - C) / R, l = (c - C) / R;
                float3 dir = l - o;
                float tq = saturate(-dot(o, dir) / max(dot(dir, dir), 1e-6));
                float q = length(o + tq * dir);
                float deeper = smoothstep(0.0, 0.05, length(l) - q);                       // the ray goes deeper than the lantern
                float before = smoothstep(0.05, 0.12, (1.0 - tq) * length(dir));          // or grazes the fringe 1-2.4 m before it
                fade = 1.0 - max(deeper, before) * (1.0 - smoothstep(1.08, 1.18, q));
                // the other crowns (GD r09: the KS15 / KS14 peeks left were far lanterns behind the near crowns' fringes): the eye
                // ray comes within q_j of crown j's centre 1-2.4 m before the lantern -> that crown is in front of it
                int nC = (int)min(_NocGardenCrownCount, 17.0);
                [loop] for (int j = 0; j < nC; j++)
                {
                    float3 Cj = _NocGardenCrowns[2 * j].xyz, Rj = max(_NocGardenCrowns[2 * j + 1].xyz, 0.5);
                    float3 oj = (_WorldSpaceCameraPos.xyz - Cj) / Rj, lj = (c - Cj) / Rj;
                    float3 dj = lj - oj;
                    float tj = saturate(-dot(oj, dj) / max(dot(dj, dj), 1e-6));
                    float qj = length(oj + tj * dj);
                    float other = step(1.69, dot(lj, lj));                 // not its own crown (|l| >= 1.3; the own test is above)
                    fade *= 1.0 - other * smoothstep(0.05, 0.12, (1.0 - tj) * length(dj)) * (1.0 - smoothstep(1.08, 1.18, qj));
                }
            }
            v.positionWS = c + n * (r * k * saturate(50.0 * fade));          // shrinks over fade 0.02..0 (was a switch at 0.01)
            v.custom.x = fade / (k * k);
            v.custom.y = d;
            v.custom.z = 2.0 * inK * r * k / max(fp, 1e-6);                   // drawn (inscribed) diameter in px
        }
        else
        {
            [branch] if (_LeafMode > 1.5 && _LeafMode < 2.5) v.positionWS += NocFoliageSway(v.positionWS, v.uv.w);
        }
    }

    // mode 3: a lantern ball (emission only)
    void NocFoliageLantern(inout NocFragData f, inout SurfaceData s)
    {
        float h = f.uv.y;
        float pulse = 1.0 + _LanternPulse * sin(6.2831853 * (_LanternHz * _NocTime.x + h));
        float nv = saturate(dot(f.normalWS, f.viewDirWS));
        s.albedo = 0.0;
        s.metallic = 0.0;
        s.smoothness = 0.0;
        s.occlusion = 1.0;
        // a paper lantern with a lamp inside: the light crossing the paper face-on is the brightest (GD r01: a flat 0.8 + 0.2 n.v
        // ball peaked at luma 195 in KS15, 14.5.5 asks maxima > 200); mean over the disc 0.35 + 1.3 / 2 = 1.0 (fix wave r07: the
        // 0.55 + 0.9 (n.v)^2 core left the KS15 lanterns at 190-202, 18 blobs over 200). A ball drawn under _FlatPx is shaded
        // flat (mean 1.0 too): a profile on a 2-4 px disc makes one hot pixel (C32)
        float hot = lerp(1.0, 0.35 + 1.3 * nv * nv, smoothstep(0.5 * _FlatPx, 1.5 * _FlatPx, f.custom.z));
        s.emission = (half3)(PAL_Lantern * saturate(f.color.rgb) * (_LanternGain * pulse * f.custom.x * hot));
    }

    // mode 4: a firefly mote (emission only)
    void NocFoliageFirefly(inout NocFragData f, inout SurfaceData s)
    {
        float h = f.uv.y;
        float glow = 0.55 + 0.45 * sin(6.2831853 * (_FireflyHz * _NocTime.x + h));
        float nearFade = smoothstep(1.5, 4.0, f.custom.y);
        s.albedo = 0.0;
        s.metallic = 0.0;
        s.smoothness = 0.0;
        s.occlusion = 1.0;
        s.emission = (half3)(lerp(PAL_LeafWarm, PAL_LeafGlow, frac(h * 3.7)) * (_FireflyGain * glow * f.custom.x * nearFade));
    }

    // the v2 core (mode 1): two planar layers of the leaf atlas on the axis plane facing the normal (see the header)
    void NocFoliageCore(inout NocFragData f, inout SurfaceData s, float3 dpx, float3 dpy)
    {
        const float k = 0.5;                                                  // 1 / sprite metres
        float3 p = f.positionWS;
        float3 an = abs(f.normalWS);
        float useY = an.y >= max(an.x, an.z) ? 1.0 : 0.0;
        float useX = (1.0 - useY) * (an.x >= an.z ? 1.0 : 0.0);
        float useZ = 1.0 - useY - useX;
        float2 pc = useY * p.xz + useX * p.zy + useZ * p.xy;
        float2 gx = (useY * dpx.xz + useX * dpx.zy + useZ * dpx.xy) * k;
        float2 gy = (useY * dpy.xz + useX * dpy.zy + useZ * dpy.xy) * k;
        float2 u1 = pc * k, u2 = pc * k + 0.5;
        float s1 = floor(NocFoliageHash(int3(int2(floor(u1)), 17)) * 3.999);
        float s2 = floor(NocFoliageHash(int3(int2(floor(u2)), 29)) * 3.999);
        float4 a1 = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoTex, sampler_AlbedoTex, u1, s1, gx, gy);
        float4 a2 = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoTex, sampler_AlbedoTex, u2, s2, gx, gy);
        float c1 = saturate(a1.a), c2 = saturate(a2.a);
        float cover = 1.0 - (1.0 - c1) * (1.0 - c2);
        float3 col = (saturate(a1.rgb) * c1 + saturate(a2.rgb) * c2 * (1.0 - c1)) / max(cover, 1e-3);
        col = lerp(float3(0.5, 0.5, 0.5), col, saturate(cover * 8.0));     // no leaf: the atlas mean
        s.albedo = PAL_Leaf * _AlbedoGain * 2.0 * col * lerp(_CoreGap, 1.0, cover);
        s.metallic = 0.0;
        s.smoothness = min(_Smoothness, 0.95);
        s.occlusion = _CoreAo * f.color.r;
        s.emission = (half3)(NocFoliageFieldLight(s.albedo, f.color.a, f.normalWS.y) * sqrt(saturate(s.occlusion)));
    }

    // mode 5: the limbs' bark lit by the lanterns around them (see the header); uvB / gB* = the bark uv and its gradients
    void NocFoliageBark(inout NocFragData f, inout SurfaceData s, float2 uvB, float2 gBx, float2 gBy, float3 dpx, float3 dpy)
    {
        float4 a = SAMPLE_TEXTURE2D_GRAD(_BarkTex, sampler_AlbedoTex, uvB, gBx, gBy);
        float4 nt = SAMPLE_TEXTURE2D_GRAD(_BarkNormalTex, sampler_AlbedoTex, uvB, gBx, gBy);
        s.albedo = _BarkColor.rgb * 2.0 * saturate(a.rgb);
        s.metallic = 0.0;
        s.smoothness = min(_Smoothness, 0.95);
        s.occlusion = 1.0;
        // derivative tangent frame (cotangent frame of the uv over the triangle), guarded
        float3 N = normalize(f.normalWS);
        float3 d2p = cross(dpy, N), d1p = cross(N, dpx);
        float3 T = d2p * gBx.x + d1p * gBy.x;
        float3 B = d2p * gBx.y + d1p * gBy.y;
        float inv = rsqrt(max(max(dot(T, T), dot(B, B)), 1e-20));
        float3 ts = UnpackNormalScale(nt, max(_BarkNormalStrength, 0.0));
        f.normalWS = normalize(T * (inv * ts.x) + B * (inv * ts.y) + N * max(ts.z, 0.05));
        float3 L = lerp(PAL_Lantern, PAL_WinNeutral, 0.4);
        s.emission = (half3)(L * s.albedo * (f.color.a * _FieldScale * _LanternLightGain));
    }

    void NocSurfaceHook(inout NocFragData f, inout SurfaceData s)
    {
        // ---- derivatives and the atlas taps first (before every branch)
        float2 uv = f.uv.xy;
        float slice = floor(f.uv.z + 0.5);
        float2 dx = ddx(uv), dy = ddy(uv);
        float3 dpx = ddx(f.positionWS), dpy = ddy(f.positionWS);
        float aBias = exp2(_AlphaMipBias);
        float4 alb = SAMPLE_TEXTURE2D_ARRAY_GRAD(_AlbedoTex, sampler_AlbedoTex, uv, slice, dx * aBias, dy * aBias);
        float aw = max(fwidth(alb.a), 1e-4);
        float res = _AlbedoTex_TexelSize.z > 1.0 ? _AlbedoTex_TexelSize.z : 1024.0;
        float mip = 0.5 * log2(max(max(dot(dx, dx), dot(dy, dy)) * res * res, 1e-8)) + _AlphaMipBias;
        float bInv = 1.0 / max(_BarkTile, 0.05);
        float2 uvB = uv * bInv, gBx = dx * bInv, gBy = dy * bInv;

        [branch] if (_LeafMode > 4.5) { NocFoliageBark(f, s, uvB, gBx, gBy, dpx, dpy); return; }
        [branch] if (_LeafMode < 1.5) { NocFoliageCore(f, s, dpx, dpy); return; }
        [branch] if (_LeafMode > 3.5) { NocFoliageFirefly(f, s); return; }
        [branch] if (_LeafMode > 2.5) { NocFoliageLantern(f, s); return; }

        // ---- cards (mode 2)
        float4 nTex = SAMPLE_TEXTURE2D_ARRAY_GRAD(_NormalTex, sampler_AlbedoTex, uv, slice, dx, dy);
        float4 mask = SAMPLE_TEXTURE2D_ARRAY_GRAD(_MaskTex, sampler_AlbedoTex, uv, slice, dx, dy);

        float aSharp = saturate((alb.a - 0.5) / aw + 0.5);
        s.alpha = (half)lerp(aSharp, saturate(alb.a), saturate((mip - _SharpMip) * (1.0 / 1.5)));

        float hash = f.uv.w;
        float j = (2.0 * hash - 1.0) * _HueJitter;
        float3 tint = float3(1.0 + j, 1.0, 1.0 - 1.5 * j);
        tint /= max(dot(tint, float3(0.2126, 0.7152, 0.0722)), 1e-3);
        float value = 1.0 + 0.12 * (frac(hash * 7.31) - 0.5);
        s.albedo = PAL_Leaf * _AlbedoGain * 2.0 * saturate(alb.rgb) * tint * value;
        s.metallic = 0.0;

        // normals: card normal toward the viewer side, bent toward the ellipsoid normal, then the atlas as a surface gradient
        float3 nCard = NocFoliageOctDecode(f.custom.zw);
        float3 nRad = f.normalWS;
        float3 nView = nCard * f.faceSign;
        float3 T = f.tangentWS.xyz;
        T = normalize(T - nCard * dot(nCard, T) + 1e-6);
        float3 B = cross(nCard, T);
        float3 ts = UnpackNormalScale(nTex, max(_NormalStrength, 0.0));
        float3 grad = (ts.x * T + ts.y * B) / max(ts.z, 0.25);
        float3 nBent = normalize(lerp(nView, nRad, _BendNormal));
        float3 n = normalize(nBent + f.faceSign * grad * (1.0 - _BendNormal * 0.5));
        f.normalWS = n;

        float ao = f.color.r * lerp(1.0, mask.g, 0.8);
        s.occlusion = ao;
        s.smoothness = min(mask.a + _WetSmooth, 0.75);

        // transmission inputs for the forward hook: the viewer-side leaf normal and the weight (sky exposure in custom.w's sign
        // bit is not needed: exposure is the vertex B)
        f.custom = float4(nView, _Transmission * ao);

        // emission: specks on the glow cards, rim, tips (LOOK_V2 10)
        float fp = f.footprint;
        float speck = 0.0;
        [branch] if (f.color.g > 0.5)
        {
            const float speckCells = 40.0;                                   // cells per sprite: ~5 cm on a 2 m card
            float raw = smoothstep(_SpeckThreshold, _SpeckThreshold + 0.08, NocFoliageNoise(float3(uv * speckCells, hash * 61.0)));
            const float speckMean = 0.06;                                    // mean of the step over the noise (threshold 0.80)
            float keep = NocFoliageKeep(2.0 / speckCells, fp);
            speck = lerp(speckMean, raw, keep) * smoothstep(0.35, 0.75, mask.b);
        }
        float glowCard = f.color.g > 0.5 ? 1.0 : 0.0;
        float nv = saturate(dot(nBent, f.viewDirWS));
        float rim = PositivePow(1.0 - nv, max(_RimPower, 0.5)) * glowCard;
        float tip = smoothstep(0.70, 0.95, mask.b) * saturate(n.y * 1.5 + 0.2);
        s.emission = (half3)(PAL_LeafGlow * (_GlowGain * speck + _RimGain * rim) + PAL_LeafWarm * (_TipGain * tip)
                   + NocFoliageFieldLight(s.albedo, f.color.a, nBent.y) * lerp(1.0, ao, 0.5));
    }

    // depth-type passes (never run while depth priming and the depth texture are off; kept correct): the card cut-out at 0.5
    void NocClipHook(NocFragData f)
    {
        [branch] if (_LeafMode > 1.5 && _LeafMode < 2.5)
        {
            float a = SAMPLE_TEXTURE2D_ARRAY_LOD(_AlbedoTex, sampler_AlbedoTex, f.uv.xy, floor(f.uv.z + 0.5), 0).a;
            clip(a - 0.5);
        }
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On ZTest LEqual Cull [_Cull] AlphaToMask [_AlphaToMask]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #define NOC_PASS_FORWARD
            #define NOC_ALPHA_OUT
            #define NOC_FRONT_FACE
            #define NOC_HAS_FORWARD_HOOK
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Include/NocLighting.hlsl"

            // thin-leaf transmission of one light toward the viewer side (8.11 item 7)
            half3 NocFoliageTrans(Light light, float3 nView)
            {
                return light.color * (light.distanceAttenuation * light.shadowAttenuation * saturate(dot(light.direction, -nView)));
            }

            // mode 2: the template's PBR terms (NocShadeOcc, written out) plus the transmission through the card in one light loop
            half3 NocFoliageShadeCards(NocFragData f, SurfaceData s, InputData inputData)
            {
                BRDFData brdfData;
                InitializeBRDFData(s, brdfData);
                half3 reflectVector = reflect(-inputData.viewDirectionWS, inputData.normalWS);
                half NoV = saturate(dot(inputData.normalWS, inputData.viewDirectionWS));
                half fresnelTerm = Pow4(1.0h - NoV);
                half3 indirectSpecular = NocReflection(reflectVector, brdfData.perceptualRoughness);
                half3 color = inputData.bakedGI * brdfData.diffuse * s.occlusion
                            + indirectSpecular * EnvironmentBRDFSpecular(brdfData, fresnelTerm) * (s.occlusion * f.specOcc);

                float3 nView = f.custom.xyz;
                half3 tint = (half3)(s.albedo * f.custom.w);
                half exposure = (half)f.color.b;
                const BRDFData noClearCoat = (BRDFData)0;
                const half4 shadowMask = half4(1, 1, 1, 1);
                Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
                half3 direct = LightingPhysicallyBased(brdfData, noClearCoat, mainLight, inputData.normalWS, inputData.viewDirectionWS, 0.0h, false);
                half3 trans = exposure * NocFoliageTrans(mainLight, nView);
            #if defined(_ADDITIONAL_LIGHTS)
                uint pixelLightCount = GetAdditionalLightsCount();
                #if USE_CLUSTER_LIGHT_LOOP
                [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                {
                    CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
                    Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
                    direct += LightingPhysicallyBased(brdfData, noClearCoat, light, inputData.normalWS, inputData.viewDirectionWS, 0.0h, false);
                    trans += exposure * NocFoliageTrans(light, nView);
                }
                #endif
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
                    direct += LightingPhysicallyBased(brdfData, noClearCoat, light, inputData.normalWS, inputData.viewDirectionWS, 0.0h, false);
                    trans += NocFoliageTrans(light, nView);
                LIGHT_LOOP_END
            #endif
                [branch] if (_NocKoiK > 0.0) direct += brdfData.diffuse * NocKoiLight(inputData.positionWS, inputData.normalWS);
                color += (direct + tint * trans) * NocLightCal();
                color += tint * exposure * NocAmbient(-nView);          // the lid's light through the leaf (NocAmbient has LightCal)
                color += s.emission;
                return min(color, 1000.0);
            }

            half3 NocForwardHook(NocFragData f, SurfaceData s, InputData inputData)
            {
                // one return (INT): the early return inside [branch] drew fxc X4000 "potentially uninitialized variable"
                half3 c;
                [branch] if (_LeafMode < 1.5 || _LeafMode > 4.5) c = NocShadeOcc(inputData, s, f.specOcc);
                else c = NocFoliageShadeCards(f, s, inputData);
                return c;
            }
            #include "../Include/NocLitPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull [_Cull]
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NocVert
            #pragma fragment NocFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #define NOC_PASS_SHADOW
            #define NOC_HAS_CLIP_HOOK
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
            #define NOC_HAS_CLIP_HOOK
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
            #define NOC_HAS_CLIP_HOOK
            #include "../Include/NocLitPass.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
