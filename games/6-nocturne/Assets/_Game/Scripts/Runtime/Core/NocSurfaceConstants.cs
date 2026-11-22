// NocSurfaceConstants: the Look v2 near surfaces, wet floors, glass and lines (docs/TEXTURE_PLAN.md section 3, 2.7, 2.11;
// docs/LOOK_V2.md 5.1-5.3), owner SU. Created empty by TP-0 and handed over (TEXTURE_PLAN 8.3). Exported as NC_<Name> into
// Shaders/Generated/NocSurfaceConstantsGen.hlsl (gen_hlsl.py / ShaderGen: `public const double|float|int` literals only; a NC_
// name that another constants class already exports is an error). Every name starts with "Surf"; the NocturneConstants puddle,
// ripple and wet lines stay frozen for the -surf-v 1 path. The texture fade and the micro-detail period are TP's
// (NocTexConstants TexFadePx*, TexMicro*). Lengths in metres, angles in radians. C# twins: NocSurfaceMath (puddles, ripples,
// leaks, line paint, wall radiance) and NocMirrorMath (the wall blend).
namespace Megastructure
{
    public static class NocSurfaceConstants
    {
        // ------------------------------------------------------------------ puddles (TEXTURE_PLAN 3.3; CRIT0 #8, GB-10)
        /// <summary>Puddle noise: two rotated value-noise octaves (v1 4.0 / 1.3 m read as camouflage blots) with weights 0.62 / 0.38.</summary>
        public const double SurfPuddleScale0 = 2.0, SurfPuddleScale1 = 0.6;
        /// <summary>Height bias: m = smoothstep(th - e, th + e, pn + SurfPuddleHeightBias (0.5 - h)), so joints and low spots fill first.</summary>
        public const double SurfPuddleHeightBias = 0.35;
        /// <summary>Puddle fraction target and tolerance of every wet row (14.2.5: 35 %), measured over 10^6 samples (SurfaceTests).</summary>
        public const double SurfPuddleFraction = 0.35, SurfPuddleTol = 0.02;
        /// <summary>Edge half-width of the mask (noise units) before the fwidth widening; the mask fades to its fraction below
        /// smoothstep(SurfPuddleFadePx0, SurfPuddleFadePx1) of the fine scale in pixels.</summary>
        public const double SurfPuddleEdge = 0.035, SurfPuddleFadePx0 = 3.0, SurfPuddleFadePx1 = 6.0;
        /// <summary>Threshold of the noise alone (h = 0.5 everywhere): its 65th percentile over 10^6 samples (SurfaceTests measures it).</summary>
        public const double SurfPuddleTh0 = 0.5688;

        // ------------------------------------------------------------------ ripples (TEXTURE_PLAN 3.3; CRIT0 #29)
        /// <summary>Rain rings inside puddles only: one ring per SurfRippleCell cell every SurfRipplePeriod / rho s; radius
        /// SurfRippleR0 + SurfRippleSpeed x age over SurfRippleLife s (up to 0.155 m); profile wavelength SurfRippleLambda; ring
        /// centres lie in the middle half of their cell, so the 2 x 2 cells around the nearest corner hold every ring that reaches a
        /// point (0.75 cell = 0.225 m >= R + 1.5 lambda = 0.185 m). Amplitude x SurfRippleRef / (SurfRippleRef + R) (the energy spreads).</summary>
        public const double SurfRippleCell = 0.3, SurfRippleSpeed = 0.25, SurfRippleR0 = 0.005, SurfRippleLife = 0.6;
        public const double SurfRippleLambda = 0.02, SurfRipplePeriod = 0.6, SurfRippleRef = 0.04;
        /// <summary>Slope gain of the rings (v1 WetRippleSlope 0.12 read as target decals); the part above
        /// smoothstep(SurfRippleFadePx0, SurfRippleFadePx1) of lambda in pixels moves into the lobe variance.</summary>
        public const double SurfRippleSlope = 0.07, SurfRippleFadePx0 = 3.0, SurfRippleFadePx1 = 6.0;
        /// <summary>Mean squared ring slope |g|^2 per unit rain rho (before SurfRippleSlope), measured over 2 x 10^5 random points and
        /// times (SurfaceTests): the lobe adds SurfRippleSlope^2 x SurfRippleVar x rho x (1 - fade^2) x m where the rings fade out.</summary>
        public const double SurfRippleVar = 0.00126;

        // ------------------------------------------------------------------ mirror lobe (TEXTURE_PLAN 3.3)
        /// <summary>GGX alpha in puddles; the dry alpha comes from the absolute mask smoothness, wetted, never under SurfAlphaDryMin.
        /// FX (C04, fix wave p4_01; LOOK_V2 5.2 "up-facing wet 1.0"): 0.18 -> 0.08 (perceptual roughness 0.42 -> 0.28): at 0.18 the
        /// film lobe (sigma 0.19 rad) clamped every wetted row, so the street, pier and plazas read as dry paving (p4_01).</summary>
        public const double SurfAlphaPuddle = 0.08, SurfAlphaDryMin = 0.08;
        /// <summary>Reflectivity in puddles and on the wet film between them (v1 0.95 / 0.5).</summary>
        public const double SurfReflPuddle = 0.95, SurfReflDry = 0.55;

        // ------------------------------------------------------------------ the wall planes of the mirror (TEXTURE_PLAN 3.3, 2.7; CRIT0 #8)
        /// <summary>Half-width of the blend of a registered wall's ends and top (2.5 deg: about 5 deg around the vanishing line).</summary>
        public const double SurfWallBlendRad = 0.0436;
        /// <summary>FX (INT-13, D-032 3): the wet floor's facade wall planes (kind 0) carry the lit windows: bays SurfWallBayU x floors
        /// SurfWallFloorV, window SurfWallWinU x SurfWallWinV of the cell, lit with p = FaLit x the column's share q (q = SurfWallShareA
        /// + SurfWallShareB h^3, mean 1, one value per bay and group of SurfWallGroupFloors floors, smoothstep between groups); box
        /// filtered by the lobe's footprint on the wall (lateral and vertical apart: wet streets streak vertically), the windows fade
        /// to the column share between 1/4 and 1/2 of a floor of vertical footprint, the share to 1 as 1/sqrt(groups x bays covered).
        /// Mean 1 x the registered macro (WetMacroTests unchanged) x the art-directed gain SurfWallGain (D-032 3: <= 2.5).</summary>
        public const double SurfWallBayU = 3.0, SurfWallFloorV = 3.6, SurfWallWinU = 0.55, SurfWallWinV = 0.6;
        public const double SurfWallGroupFloors = 4.0, SurfWallShareA = 0.4, SurfWallShareB = 2.4, SurfWallGain = 2.5;

        // ------------------------------------------------------------------ wetness (LOOK_V2 5.2; TEXTURE_PLAN 3.3)
        /// <summary>wet = saturate(_Wetness (SurfWetBase + SurfWetUp up) + streak SurfWetStreak + up SurfWetPool (0.5 - height)).</summary>
        public const double SurfWetBase = 0.55, SurfWetUp = 0.45, SurfWetStreak = 0.5, SurfWetPool = 0.35;
        /// <summary>Porous darkening lerp(1, SurfWetDarkMin, porosity x wet) (Lagarde: 0.2 at porosity 1) and the wet gloss target.</summary>
        public const double SurfWetDarkMin = 0.2, SurfWetGloss = 0.88;
        /// <summary>Porosity per material class (LOOK_V2 5.2): concrete, render / plaster, brick, granite, bark, moss; paint and
        /// metal 0 (smoothness only); asphalt carries its wetness in its token.</summary>
        public const double SurfPorConcrete = 0.4, SurfPorPlaster = 0.3, SurfPorBrick = 0.5, SurfPorGranite = 0.35, SurfPorBark = 0.45, SurfPorMoss = 0.4;
        /// <summary>Wet level of a vertical face away from the streaks (LOOK_V2 5.2: up 1.0, vertical 0.6, sheltered 0.15).</summary>
        public const double SurfWetVertical = 0.6, SurfWetSheltered = 0.15;

        // ------------------------------------------------------------------ wall radiance of the wet mirror (TEXTURE_PLAN 2.11)
        /// <summary>URP's dielectric diffuse factor (1 - 0.04): radiance = albedo x 0.96 x (ambient + fill N.L).</summary>
        public const double SurfLambert = 0.96;
        /// <summary>WetMacroTests: every registered macro within 5 % (luminance and each channel) of its C# mean.</summary>
        public const double SurfMacroTol = 0.05;

        // ------------------------------------------------------------------ anchored leaks (TEXTURE_PLAN 3.3)
        /// <summary>The leak decal (OverlayArr 0) hangs from the top edge: v_leak = d_top / (SurfLeakLen x stretch), stretch
        /// SurfLeakStretch0..1 by hash per SurfLeakTileU-wide column; the vertex bake stores d_top in 0.1 m steps up to SurfLeakMaxD.</summary>
        public const double SurfLeakLen = 1.0, SurfLeakStretch0 = 1.0, SurfLeakStretch1 = 2.0, SurfLeakTileU = 2.0, SurfLeakMaxD = 25.5;
        /// <summary>Full-strength leak: albedo x SurfLeakDark, smoothness + SurfLeakSmooth, AO x (1 - SurfLeakAo).</summary>
        public const double SurfLeakDark = 0.7, SurfLeakSmooth = 0.15, SurfLeakAo = 0.1;
        /// <summary>Mean coverage of the leak decal over its length (OverlayArr slice 0: texmeta sliceStats coverage 0.121 over the whole
        /// padded slice, the decal fills half of it: 0.242); the far profile is 2 x SurfLeakMean x (1 - d_top / length).</summary>
        public const double SurfLeakMean = 0.242;

        // ------------------------------------------------------------------ NocLine paint (TEXTURE_PLAN 3.5)
        /// <summary>Albedo x (1 + SurfLineAmp n1(a / SurfLineScale)) plus sparse chips (SurfLineChipP of SurfLineChipLen cells,
        /// bare SteelF0), and a darker splash band of SurfLineSplash m at the foot (x SurfLineSplashDark); filtered to the mean below
        /// SurfLineFadePx pixels of the scale.</summary>
        public const double SurfLineAmp = 0.12, SurfLineScale = 0.35, SurfLineChipP = 0.06, SurfLineChipLen = 0.04;
        public const double SurfLineSplash = 0.3, SurfLineSplashDark = 0.65, SurfLineFadePx = 2.0;

        // ------------------------------------------------------------------ glass (TEXTURE_PLAN 2.7, 3.2; CRIT0 #17)
        /// <summary>Grime (OverlayArr 1) raises the perceptual roughness from SurfGlassRough0 to SurfGlassRough1 and adds up to
        /// SurfGlassGrimeAlb albedo; rivulets (OverlayArr 2) only within SurfGlassRivuletD metres.</summary>
        public const double SurfGlassRough0 = 0.05, SurfGlassRough1 = 0.20, SurfGlassGrimeAlb = 0.03, SurfGlassRivuletD = 12.0;
        /// <summary>Tube ribs and frames every SurfGlassRibPitch metres along the tube, SurfGlassRibWidth wide.</summary>
        public const double SurfGlassRibPitch = 6.0, SurfGlassRibWidth = 0.08;
        /// <summary>FX (INT-19, AB's request): the Forward+ lights on the glass x SurfGlassLightK: the specular-AA-filtered GGX lobe of a
        /// lamp on the KS13 glass strip / floor is a soft 30-60 px glow that reads as a light floating over the void. KS13 (FX_r04 / r05
        /// / r07 at 0.35 / 0.25 / 1): floor blob mean 70.1 / 65.6 / 86.9, strip glow 114.6 / 108.7 / 143.9 levels. KS12's blob at
        /// (700, 375) is not this term (unchanged at 0.25). The environment reflection is unchanged.</summary>
        public const double SurfGlassLightK = 0.35;
        /// <summary>FX (C06, fix wave p4_01): the horizontal panes seen from above (the KS10 ledge, the KS13 strip) carry the water
        /// film's impact rings at SurfGlassRingGain x the puddles' slope; their grime scatters 1 + SurfGlassGrimeH times the vertical
        /// panes' albedo; they mirror the beat's registered sources (NocMirrorSourcesMixR) as cores of half-width SurfGlassSrcRho m
        /// (the wet glow rho 0.3-0.45 drew a 60 px orb, FX_r10 KS13) through a lobe of variance floor SurfGlassSigmaFloor (the wet
        /// floor's 0.004 is a 0.063 rad blur), the grime cutting that image by up to SurfGlassGrimeBreak.</summary>
        public const double SurfGlassRingGain = 1.5, SurfGlassGrimeH = 2.0, SurfGlassGrimeBreak = 0.6, SurfGlassSrcRho = 0.15, SurfGlassSigmaFloor = 1e-5;
        /// <summary>FX (C04, fix wave p4_01; D-032 3): the art-directed gain of the wet floors' area sources (lamps, blades, screens,
        /// windows) next to SurfWallGain, at most 2.5. The registry's L_eff cap (3.0) and the closed-form lobe mass stay; the median
        /// lamps register LampWarm x 1.6 (under the cap), so their streaks scale by the full 2.5.</summary>
        public const double SurfSrcGain = 2.5;
    }
}
