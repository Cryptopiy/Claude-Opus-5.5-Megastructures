// BwSea.shader - the near sea (MASTER_PROMPT 14.7 H1, 14.2.6 BwSea, 14.5.2 SeaClipmap). Owner: the sea owner.
// Geometry: the camera-centred clipmap of Beat02_Crest/SeaClipmap.cs: 7 levels of 128 x 128 quads, spacing 0.5 x 2^k
// (0.5-32 m), half-extent 32 x 2^k (32-2048 m). Each mesh vertex carries its integer grid coordinates (i, j) in level
// units and (level, group) in uv0; the vertex stage snaps level k to 2 x its spacing around the camera, switches the one
// cell wide trim strips of each ring on or off by the parity of the finer level (no gap, no overlap), geomorphs the outer
// 15 % of every level onto the next level's sampling (odd vertices -> the average of their even neighbours, weights ->
// the coarser ring's), fades the last level flat at 2048 m and adds the curvature drop d^2 / 2 (R + 5992) so the edge
// meets the far-field sea sphere. Waves: BwSeaWaves.hlsl (32 components per sea state, Gerstner steepness 0.6, per-ring
// weights at the vertex, all components with footprint weights at the pixel, unresolved variance into roughness, wall
// clapotis image wave blended out over 150-400 m).
// Shading (no depth reads): Fresnel (Schlick, F0 0.02) mix of the body radiance (_BwSea0: SeaStorm -> SeaTeal x 0.6, the
// far field's own value, so the 2048 m crossover matches) and BwSkyVisible(reflect) (the sky through the whole
// atmosphere: the sea horizon equals the fog and the sky, Part 10 #13); clamped Beckmann glint (peak 1 / (pi s2),
// sun x D x Fresnel clamped to 8, only while the storm deck lets the disc through); foam from the Jacobian (filtered
// whitecap probability), the wall and the pier noses, the wave impacts and broad old-foam bands, FoamWhite lit by the trilight
// ambient and the key, clamped to 4; BwFogApplyFar last (the far field's own fog for every sea pixel).
// The kraken (Addendum A1): the SeaDisturbances bus (BwSeaWaves.hlsl BwSeaDisturbAt / BwSeaDisturbHeight) darkens the body
// colour over a submerged mass, calms the short waves and the whitecaps (slick, wake scar), adds its white water and foam
// (not faded with distance like the wind foam: its patterns are footprint-filtered, large and steady), tilts the normal
// by its mounds, collars, rings and wake crests and displaces the clipmap (morphed like the waves, flattened in the last
// 7 % of the 2048 m level so the far-field sphere meets it). Beyond 2048 m the disturbances continue on the clipmap's
// disturbance ring (level 7: 64 m cells, half-extent 4096 m, flat on the sea sphere): it draws ONLY inside the support of
// an active entry (discard elsewhere, in every pass, so the far field still fills the rest) with the same fragment code as
// the last wave level, whose plain sea equals the far field's (T7); a K1 breach at 1464 m keeps its 1.1 km ring field and a
// 2 km entry is whole. The disturbances fade over Chebyshev 3700-3950 m (the ring reaches at least 3968 m).
// Precision: every phase is camera-relative (origin O snapped to 64 m) with the time term from the double clock.
Shader "Megastructure/BwSea"
{
    Properties
    {
        _FoamStrength ("Foam strength", Range(0, 2)) = 1.0
        _StreakCover ("Old-foam band threshold", Range(0.4, 0.9)) = 0.72
        _Subsurface ("Crest translucency", Range(0, 2)) = 0.6
        _WallFoam ("Wall foam", Range(0, 2)) = 1.0
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Include/BulwarkPalette.hlsl"
    #include "Include/BwCommon.hlsl"
    #include "Include/BulwarkAtmosphere.hlsl"
    #include "Include/BwSeaWaves.hlsl"

    CBUFFER_START(UnityPerMaterial)
        float _FoamStrength;
        float _StreakCover;
        float _Subsurface;
        float _WallFoam;
    CBUFFER_END

    float4 _BwSeaImpact[6];       // xy = (x, z) of an impact, z = age (s), w = strength (SeaState foam patches)

    #define SEA_LEVELS 7           // wave levels 0-6 (14.5.2)
    #define SEA_DIST_LEVEL 7       // the disturbance-only ring beyond 2048 m (SeaClipmap.DisturbLevels)
    #define SEA_HALF 64.0          // half the grid of a level in its own cells (128 x 128 quads)
    #define SEA_MORPH 0.15         // geomorph over the outer 15 % of a level (14.5.2)
    #define SEA_SPACING0 0.5
    #define SEA_SPHERE_R (6371000.0 + 5992.0)   // the sea sphere of BwFarField (BulwarkConst.EarthRadius + SeaSphereOffset)

    struct SeaAttributes
    {
        float4 positionOS : POSITION;   // x = i, z = j (integer grid coordinates of the level)
        float2 uv0 : TEXCOORD0;         // x = level, y = group (0 fixed, 1..4 trim strips, 5..8 trim corners)
    };

    struct SeaVaryings
    {
        float4 positionCS : SV_POSITION;
        float3 positionWS : TEXCOORD0;
        float4 local : TEXCOORD1;       // xy = undisplaced local coordinates l (m), z = image-wave weight, w = height
        nointerpolation float lev : TEXCOORD2;   // clipmap level (7 = the disturbance ring)
    };

    // Is trim group grp of level k drawn for the finer level's offset o (0 or 1 per axis, in level-k cells)?
    bool SeaTrimActive(int grp, float2 o)
    {
        bool xl = o.x > 0.5, zl = o.y > 0.5;                     // the finer level spans [-31, 33] (o = 1) or [-32, 32] (o = 0)
        if (grp == 1) return xl;                                  // column x -32..-31
        if (grp == 2) return !xl;                                 // column x 32..33
        if (grp == 3) return zl;                                  // row z -32..-31
        if (grp == 4) return !zl;                                 // row z 32..33
        if (grp == 5) return xl || zl;                            // corner (x low, z low)
        if (grp == 6) return !xl || zl;                           // corner (x high, z low)
        if (grp == 7) return xl || !zl;                           // corner (x low, z high)
        return !xl || !zl;                                        // corner (x high, z high)
    }

    // Displacement of a clipmap vertex: the ring's own weights, morphed at the level's outer edge.
    float3 SeaVertexDisplace(float2 g, float2 world, int lev, float sp, out float imageW)
    {
        float r = max(abs(g.x), abs(g.y)) / SEA_HALF;
        float m = saturate((r - (1.0 - SEA_MORPH)) / SEA_MORPH);
        bool last = lev >= SEA_LEVELS - 1;
        float spEff = last ? sp : lerp(sp, 2.0 * sp, m);
        float2 l = world - _BwSeaOrigin.xz;
        imageW = BwSeaImageWeight(world.x);
        float3 d = BwSeaDisplace(l, spEff, imageW);
        float hd = BwSeaDisturbHeight(world, spEff);              // the kraken's mounds, collars, rings, wake crests
        if (!last && m > 0.0)
        {
            bool oddX = fmod(abs(g.x), 2.0) > 0.5, oddZ = fmod(abs(g.y), 2.0) > 0.5;
            if (oddX || oddZ)
            {
                float3 a;
                float ad;
                if (oddX && oddZ)
                {
                    a = 0.25 * (BwSeaDisplace(l + float2(-sp, -sp), spEff, imageW) + BwSeaDisplace(l + float2(sp, -sp), spEff, imageW)   // pal:ok (not a colour)
                              + BwSeaDisplace(l + float2(-sp, sp), spEff, imageW) + BwSeaDisplace(l + float2(sp, sp), spEff, imageW));   // pal:ok (not a colour)
                    ad = 0.25 * (BwSeaDisturbHeight(world + float2(-sp, -sp), spEff) + BwSeaDisturbHeight(world + float2(sp, -sp), spEff)
                               + BwSeaDisturbHeight(world + float2(-sp, sp), spEff) + BwSeaDisturbHeight(world + float2(sp, sp), spEff));
                }
                else
                {
                    float2 e = oddX ? float2(sp, 0.0) : float2(0.0, sp);   // pal:ok (not a colour)
                    a = 0.5 * (BwSeaDisplace(l - e, spEff, imageW) + BwSeaDisplace(l + e, spEff, imageW));
                    ad = 0.5 * (BwSeaDisturbHeight(world - e, spEff) + BwSeaDisturbHeight(world + e, spEff));
                }
                d = lerp(d, a, m);
                hd = lerp(hd, ad, m);
            }
        }
        if (last)
        {
            d *= 1.0 - m;                                         // the outermost level fades flat into the far-field sphere
            hd *= 1.0 - BwSeaSmooth(0.93, 1.0, r);                // the disturbances keep their height to 1905 m
        }
        d.y += hd;
        return d * BwSeaCalm(world);
    }

    SeaVaryings SeaVert(SeaAttributes v)
    {
        SeaVaryings o = (SeaVaryings)0;
        float3 cam = _WorldSpaceCameraPos;
        int lev = (int)(v.uv0.x + 0.5);
        int grp = (int)(v.uv0.y + 0.5);
        float sp = SEA_SPACING0 * exp2((float)lev);
        float2 ck = floor(cam.xz / (2.0 * sp)) * (2.0 * sp);       // level centre, snapped to 2 x its spacing
        if (grp > 0)
        {
            float2 cin = floor(cam.xz / sp) * sp;                  // the finer level's centre (snapped to its 2 x spacing)
            float2 off = round((cin - ck) / sp);
            if (!SeaTrimActive(grp, off))
            {
                o.positionCS = float4(0.0, 0.0, -2.0, 1.0);        // pal:ok (outside the depth range: the triangle is clipped)
                return o;
            }
        }
        float2 g = float2(v.positionOS.x, v.positionOS.z);         // pal:ok (not a colour)
        float2 world = ck + g * sp;
        float imageW = 0.0;
        float3 d = 0.0;                                            // the disturbance ring lies flat on the sea sphere
        if (lev < SEA_DIST_LEVEL) d = SeaVertexDisplace(g, world, lev, sp, imageW);
        float2 rel = world - cam.xz;
        float drop = dot(rel, rel) / (2.0 * SEA_SPHERE_R);         // curvature: meets the far-field sea sphere at 2048 m
        float3 p = float3(world.x + d.x, _BwSeaOrigin.y + d.y - drop, world.y + d.z);   // pal:ok (not a colour)
        o.positionWS = p;
        o.positionCS = TransformWorldToHClip(p);
        o.local = float4(world - _BwSeaOrigin.xz, imageW, d.y);   // pal:ok (not a colour)
        o.lev = (float)lev;
        return o;
    }

    // West boundary of the open sea: the sea face (x 6) everywhere but the spillway, whose channels between the piers
    // (z 420-704) hold the beat 3 draw-down water from the pier ends (x 46) westward. Behind the sea face the sea runs on
    // 0.3 m (hidden by the face); at the spillway it stops exactly at x 46 (the draw-down water starts there, flat).
    // The disturbance ring draws only inside an active entry's support (the far field draws the rest).
    void SeaClip(float3 posWS, float lev)
    {
        bool spill = posWS.z > _BwSeaSpill.x && posWS.z < _BwSeaSpill.y;
        float xmin = spill ? _BwSeaSpill.z : _BwSeaWall.x - 0.3;
        clip(posWS.x - xmin);
        if (lev > SEA_DIST_LEVEL - 0.5 && !BwSeaDistCovers(posWS.xz)) discard;
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" "IgnoreProjector" = "True" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On ZTest LEqual Cull Back
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex SeaVert
            #pragma fragment SeaFrag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Foam of the wave impacts (SeaState foam patches): a wall slam leaves a patch along the wall (2.2 x longer in z
            // than it reaches out, its centre moving seaward with the reflected crest), a collision jet a rounder one. The
            // outline is broken by a shared 5 m fbm (warp, -1..1: never a disc) and kept fairly crisp (the last 30 % of the
            // radius, 3.6 m at R 12: a ragged white-water rim, not a smudge; seax-03s KS04 4K); fresh white water is solid for
            // its first second, then breaks up into the mottled lace of aging foam (holes, streaks), its tone churned like
            // the kraken's white water; the patch spreads to R 12 m over 3 s and thins out over 9 s (no step: smooth in age).
            float SeaImpactFoam(float2 xz, float mottle, float warp, float tN, float tW, float nearF)
            {
                float f = 0.0;
                [unroll] for (int i = 0; i < 6; i++)
                {
                    float4 im = _BwSeaImpact[i];
                    float age = im.z;
                    if (age < 0.0 || age > 9.0) continue;
                    bool wallJet = im.x < 6.5;
                    float R = 3.0 + 9.0 * saturate(age / 3.0);
                    float2 d = xz - im.xy;
                    if (wallJet) d = float2((d.x - 0.35 * R) * 1.6, d.y * 0.72);   // pal:ok (not a colour: along the wall)
                    // X2 S3-2 (soft blobs): the rim is torn by the wind-streaked texture (+-0.1 R); near the camera the
                    // patch's coverage is thresholded against that texture (solid fresh core, lace on the aging rim)
                    float rr = length(d) / R + 0.42 * warp + 0.2 * (0.5 - tN);
                    float edge = 1.0 - smoothstep(0.75, 1.0, rr);
                    float life = smoothstep(0.0, 0.5, age) * exp(-age / 3.2);
                    float youth = 1.0 - smoothstep(0.6, 2.4, age);
                    float cov = edge * life * (0.5 + 0.5 * im.w);
                    float fFar = cov * lerp(mottle, 1.0, youth * edge);
                    float e = cov * (1.0 + 0.4 * youth) + tN - 1.0;
                    float fNear = saturate(e / tW) * lerp(0.65, 1.0, saturate(e / 0.45));
                    f = max(f, lerp(fFar, fNear, nearF));
                }
                return f;
            }

            half4 SeaFrag(SeaVaryings i) : SV_Target
            {
                SeaClip(i.positionWS, i.lev);
                float3 cam = _WorldSpaceCameraPos;
                float3 v = i.positionWS - cam;
                float dist = max(length(v), 1e-3);
                float3 dir = v / dist;
                float2 l = i.local.xy;
                float imageW = i.local.z;
                // pixel footprint along the view (the long axis of the pixel's ellipse on the sea)
                float2 dl0 = ddx(l), dl1 = ddy(l);
                float fp = max(sqrt(max(dot(dl0, dl0), dot(dl1, dl1))), 1e-4);
                float2 dP0 = ddx(i.positionWS.xz), dP1 = ddy(i.positionWS.xz);
                // the kraken's disturbances (Addendum A1), faded out with the disturbance ring (Chebyshev 3700-3950 m)
                BwSeaDist kd = BwSeaDisturbAt(i.positionWS.xz, dP0, dP1);
                float2 cr = abs(i.positionWS.xz - cam.xz);
                float distFade = 1.0 - smoothstep(3700.0, 3950.0, max(cr.x, cr.y));
                kd.dark *= distFade; kd.calm *= distFade; kd.foam *= distFade; kd.white *= distFade;
                kd.grad *= distFade; kd.height *= distFade;
                // gusts: cat's-paw patches (120-300 m, drifting west with the foam) roughen and calm the short waves 0.6-1.4;
                // a slick over a submerged body calms them further (up to 85 %)
                float gn = BwFbmFiltered(float2(i.positionWS.x + 1.6 * _BwSeaFoam.y, i.positionWS.z) * (1.0 / 140.0), fp / 140.0, 3);   // pal:ok (not a colour)
                float gust = (saturate(0.5 + 2.2 * (gn - 0.5)) * 0.8 + 0.6) * (1.0 - 0.85 * kd.calm);
                // anisotropic band limit (critic X2 S3-2): each component filtered by the footprint along its own direction,
                // so the waves crossing the view keep their detail in the mid field instead of averaging into streaks
                BwSeaSample sm = BwSeaEvaluateAnisoGC(l, dl0, dl1, imageW, gust, kd.calm);
                sm.s2 = lerp(sm.s2, _BwSeaOrigin.w + 0.3 * (sm.s2 - _BwSeaOrigin.w), kd.calm);   // the unresolved roughness too
                float calm = BwSeaCalm(i.positionWS.xz);
                sm.normal = normalize(lerp(float3(0.0, 1.0, 0.0), sm.normal, calm));   // pal:ok (not a colour)
                sm.jac = 1.0 + (sm.jac - 1.0) * calm;
                sm.jacSigma *= calm;
                sm.height *= calm;
                // tilt by the disturbance heights (mounds, collars, ring waves, wake crests): n + (-dh/dx, 0, -dh/dz) n.y
                sm.normal = normalize(sm.normal + float3(-kd.grad.x, 0.0, -kd.grad.y) * sm.normal.y);   // pal:ok (not a colour)
                sm.height += kd.height;
                float3 n = sm.normal;

                // ---- reflection and body
                float mu = max(-dot(dir, n), 1e-4);
                float F = 0.02 + 0.98 * pow(1.0 - mu, 5.0);
                float3 r = reflect(dir, n);
                r.y = max(r.y, 1e-3);
                r = normalize(r);
                float3 sky = BwSkyVisible(r);
                float nearW = 1.0 - smoothstep(0.05, 0.6, fp);            // translucent crests only where they are resolved
                float crest = saturate(sm.height / 1.6) * nearW;
                float3 body = _BwSea0.rgb * (1.0 + _Subsurface * crest * crest);
                body *= 1.0 - kd.dark;                                  // a mass below the surface swallows the light
                // ... and dims the sky's reflection over it (up to 64 %: a dark hole in the reflection, so the mass reads
                // from the crest where the Fresnel term is 0.45-0.85; DEVELOPMENT.md decision, Addendum A1 readability:
                // the 42 % of the first version left the K3 band at 230 m only about 20 levels darker than its
                // surroundings, inside the wave texture's own +-15 (seax-02k k3w))
                // the dimmed reflection keeps the water's own hue (the sea body token's chroma at luma 1): deep blue-green water
                // over the mass, not a neutral grey smear (seax-06n dshadow measured RGB 64/72/78 against 53/71/80 around it)
                float3 seaHue = _BwSea0.rgb / max(dot(_BwSea0.rgb, float3(0.2126, 0.7152, 0.0722)), 1e-4);   // pal:ok (luma weights)
                float3 skyD = sky * lerp(1.0, clamp(seaHue, 0.4, 1.6), 0.6 * kd.dark);
                float3 c = lerp(body, skyD, F * (1.0 - 0.7 * kd.dark));

                // ---- clamped Beckmann glint (14.7 H1), only through the storm deck's disc visibility
                Light ml = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float sunVis = _BwSunDisc.w * ml.shadowAttenuation * BwSunBreak(i.positionWS);
                if (sunVis > 1e-4)
                {
                    float s2 = max(sm.s2, 1e-3);
                    float3 hv = normalize(_BwSunDir.xyz - dir);
                    float ch = saturate(dot(n, hv));
                    float ch2 = max(ch * ch, 1e-4);
                    float D = exp(-(1.0 - ch2) / (ch2 * s2)) / (3.14159265 * s2 * ch2 * ch2);
                    float Fh = 0.02 + 0.98 * pow(1.0 - saturate(dot(-dir, hv)), 5.0);
                    float g = D * Fh * 0.25 * saturate(dot(n, _BwSunDir.xyz)) / max(mu, 0.05);
                    c += min(_BwSunColor.rgb * g, 8.0) * sunVis;
                }

                // ---- foam: whitecaps (filtered Jacobian), wall and pier noses, impacts, old foam; 0 beyond 1.8 km.
                // Every pattern is footprint-filtered and enters LINEARLY in its coverage, so far away the foam converges to
                // its mean (a faint, steady whitening), never to sparkle. Whitecap / old-foam opacity = coverage x a lacy
                // bubble texture. The foam textures ride on the surface drift (the wind from the east carries the foam west
                // at 0.35 m/s, _BwSeaFoam.y in double), so the bubbles move with the water instead of standing in the world.
                float2 xz = i.positionWS.xz;
                float2 sw = float2(xz.x + _BwSeaFoam.y, xz.y);                                 // pal:ok (not a colour: drifted foam coordinates)
                float tA = BwFbmFiltered(sw * (1.0 / 0.9), fp / 0.9, 4);                        // bubbles (0.9 m)
                float tB = BwFbmFiltered(sw * (1.0 / 2.7), fp / 2.7, 3);                        // lace (2.7 m)
                float nf = BwFbmFiltered(sw * float2(1.0 / 9.0, 1.0 / 5.0), fp / 5.0, 4);   // pal:ok (not a colour: patches)
                // far lace: a soft texture with holes (a 1.6 gain); far away (fbm -> 0.5) it converges to 0.52 (the coverage
                // then carries the mean)
                float laceFar = saturate((tA + tB - 0.75) * 1.6 + 0.4 * (nf - 0.5) + 0.12);
                // near foam texture (critic X2 S3-2: the foam read as soft blobs): the wind (from the east, along x) tears
                // foam into lace STREAKED ALONG IT: 4.5 x 1.2 m streaks, a web of 1.1 x 0.3 m holes (the 0.5 contours of a
                // hole field, mean 0.76) and the 0.9 m bubbles; each filtered by the footprint along its own axes (a view
                // down the wind keeps the across-wind detail). tN is continuous (0..1, mean 0.5); the foam COVERAGE is
                // thresholded against it (foam where tN > 1 - coverage: dense cores, lace filaments on the dissolving rims),
                // crisply within 60 m (softness 0.15 of tN, or the pixel derivative where larger: analytic AA, no sparkle;
                // 0.07 stippled 2 x 2 quads where tiny holes opened, sea-02) and soft from 200 m; past the threshold the foam
                // thickens from 0.65 to 1 over 0.45 of tN (thin translucent lace at the rims and holes, dense cores); over
                // 150-400 m the old linear form takes over (far away the foam stays its mean).
                float2 fpAx = float2(abs(dP0.x) + abs(dP1.x), abs(dP0.y) + abs(dP1.y));   // pal:ok (not a colour: footprint along x / z)
                float fpN1 = max(fpAx.x / 4.5, fpAx.y / 1.2), fpN2 = max(fpAx.x / 1.1, fpAx.y / 0.3);
                float n1 = BwFbmFiltered(float2(sw.x / 4.5, sw.y / 1.2) + 3.7, fpN1, 4);   // pal:ok (not a colour)
                float n2 = BwFbmFiltered(float2(sw.x / 1.1, sw.y / 0.3) - 9.1, fpN2, 3);   // pal:ok (not a colour)
                float rid = lerp(0.76, 1.0 - 2.0 * abs(n2 - 0.5), 1.0 - smoothstep(0.25, 0.5, fpN2));
                float lv = 0.8 * (n1 - 0.5) + 0.7 * (rid - 0.76) + 0.35 * (tA - 0.5);
                float tN = saturate(0.5 + 1.7 * lv);
                float tW = max(lerp(0.15, 0.35, smoothstep(60.0, 200.0, dist)), 1.7 * (abs(ddx(lv)) + abs(ddy(lv))));
                float nearF = 1.0 - smoothstep(150.0, 400.0, dist);
                float lace = lerp(laceFar, tN, nearF);
                float fpJ = abs(ddx(sm.jac)) + abs(ddy(sm.jac)) + 0.03;                        // a 0.03 feather on every whitecap
                float cap = BwSeaFoamCoverAt(sm, fpJ, imageW);
                float capE = cap + tN - 1.0;
                float capNear = 0.9 * saturate(capE / tW) * lerp(0.65, 1.0, saturate(capE / 0.45));
                cap = lerp(0.85 * saturate(cap * 1.8 * laceFar), capNear, nearF) * (1.0 - kd.calm);   // no whitecaps on a slick
                // old foam: the decaying whitecaps of the last minute, drawn by the wind into broad, broken, wandering
                // bands (14 m along the wind, at least 3 m across: never a thin line, so never a scratch at any distance),
                // translucent (at most 0.16) and marbled by the bubbles; the unresolved bands fade to their mean coverage
                float warp = BwFbmFiltered(sw * (1.0 / 30.0), fp / 30.0, 3) - 0.5;
                float2 sq = float2(sw.x / 14.0, (sw.y + 12.0 * warp) / 3.0);                   // pal:ok (not a colour)
                float sn = BwFbmFiltered(sq, fp / 3.0, 3);
                float oldRes = 1.0 - smoothstep(0.25, 0.8, fp / 3.0);
                float oldBand = smoothstep(_StreakCover - 0.14, _StreakCover + 0.06, sn) * smoothstep(0.42, 0.68, nf) * (0.35 + 0.65 * smoothstep(0.3, 0.7, tA));
                float streak = lerp(0.02, oldBand, oldRes) * 0.16;
                // the wall (x 6) and the spillway block (piers x 6-46, z 420-704: noses and outer side faces): standing-wave
                // wash by the distance to the nearest face (continuous: the old switch x 6 -> x 46 at z 420 / 704 drew a
                // straight foam seam along those lines east of the pier ends, seax-06k k3c)
                float2 spC = float2(0.5 * (_BwSeaWall.x + _BwSeaSpill.z), 0.5 * (_BwSeaSpill.x + _BwSeaSpill.y));   // pal:ok (not a colour)
                float2 spH = float2(0.5 * (_BwSeaSpill.z - _BwSeaWall.x), 0.5 * (_BwSeaSpill.y - _BwSeaSpill.x));   // pal:ok (not a colour)
                float dw = min(i.positionWS.x - _BwSeaWall.x, length(max(abs(xz - spC) - spH, 0.0)));
                float wall = _WallFoam * exp(-max(dw, 0.0) / 3.5) * saturate(0.55 + 0.5 * sm.height) * smoothstep(0.2, 0.6, lace);
                float impWarp = (BwFbmFiltered(sw * (1.0 / 5.0), fp / 5.0, 3) - 0.5) * 2.4;
                float mottle = smoothstep(0.18, 0.72, lace + 0.3 * (tA - 0.5));                  // aging foam: holes and lace
                float imp = SeaImpactFoam(xz, mottle, impWarp, tN, tW, nearF);
                float reef = BwSeaReefOccupancy(xz) * lace;
                float foam = saturate(max(max(cap, streak), max(max(wall, imp), reef)) * _FoamStrength);
                foam *= 1.0 - smoothstep(1000.0, 1800.0, dist);           // none at the far-field crossover (2048 m)
                // the kraken's foam: lacy (converging to 0.75 of its coverage far away) and its solid white water; large,
                // filtered patterns, so they are not faded with distance (only with the clipmap's last level, above)
                // the coverage is thresholded against the lace, so its edges break into blobs and holes (no sticker
                // outline) while its solid core stays solid; far away the lace tends to 0.52 and the mean stays close
                float kFoam = saturate((1.3 * kd.foam - 0.6 * (1.0 - lace)) * 1.8);
                // white water churns: 7 m and 2.5 m boils break the sheet into brighter heads and greyer, thinner water
                // (filtered: far away churn -> 0.5 and the sheet keeps its mean), so a breach never reads as a flat plate
                float churn = BwFbmFiltered(sw * (1.0 / 7.0) + 17.3, fp / 7.0, 3);
                float boil = BwFbmFiltered(sw * (1.0 / 2.5) - 5.1, fp / 2.5, 3);
                float kWhite = saturate((kd.white - 0.55 * (1.0 - lace) - 1.1 * (0.5 - churn)) * 2.2);
                float churnTone = 0.5 + 0.9 * churn + 0.35 * (boil - 0.5);     // mean 0.95, 0.4-1.4 near, 0.95 far
                float toneW = max(kWhite * saturate(kd.white * 1.5), 0.7 * saturate(imp * 1.5));   // white water only
                foam = max(foam, saturate(max(kFoam, kWhite)));
                if (foam > 1e-4)
                {
                    float u = n.y;
                    float3 amb = _BwAmbSky.rgb * saturate(u) + _BwAmbMid.rgb * (1.0 - saturate(u));
                    float3 key = _BwSunColor.rgb * saturate(dot(n, _BwSunDir.xyz)) * ml.shadowAttenuation * BwSunBreak(i.positionWS);
                    float bub = lerp(1.0, 0.8 + 0.4 * tA, nearF);            // bubble density: no flat white sheet up close
                    float3 foamRad = min(PAL_FoamWhite * (amb + key) * lerp(1.0, churnTone, toneW) * bub, 4.0);
                    c = lerp(c, foamRad, foam);
                }

                c = BwFogApplyFar(c, dir, dist);
                c = AnyIsNaN(c) ? float3(0.0, 0.0, 0.0) : c;   // pal:ok (NaN guard)
                return half4(min(c, 1000.0), 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R Cull Back
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex SeaVert
            #pragma fragment SeaDepthFrag
            half SeaDepthFrag(SeaVaryings i) : SV_Target0
            {
                SeaClip(i.positionWS, i.lev);
                return i.positionCS.z;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On Cull Back
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex SeaVert
            #pragma fragment SeaNormalsFrag
            void SeaNormalsFrag(SeaVaryings i, out half4 outNormalWS : SV_Target0)
            {
                SeaClip(i.positionWS, i.lev);
                outNormalWS = half4(0.0, 1.0, 0.0, 0.0);   // pal:ok (not a colour: the mean sea normal)
            }
            ENDHLSL
        }
    }
    Fallback Off
}
