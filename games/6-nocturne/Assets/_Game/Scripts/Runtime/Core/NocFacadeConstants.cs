// NocFacadeConstants: the Look v2 facade (docs/TEXTURE_PLAN.md section 2, docs/LOOK_V2.md section 4, docs/FACADE_V2.md), owner FA.
// Exported as NC_<Name> into Shaders/Generated/NocFacadeConstantsGen.hlsl (gen_hlsl.py / ShaderGen: `public const` literals only;
// every name starts with "Fa" so it never collides with NocturneConstants' v1 Facade* lines, which stay byte-identical for
// CityField and the -facade-v 1 path). Lengths in metres, cell-local coordinates: x along u from the cell's left edge, y up
// from the cell's bottom (the floor line). The C# twin is NocFacadeMath (v2 section), the HLSL Shaders/Include/NocFacadeSkin.hlsl.
namespace Megastructure
{
    public static class NocFacadeConstants
    {
        // ------------------------------------------------------------------ window light (LOOK_V2 4; D-028: the final values)
        /// <summary>Overall lit fraction 0.18 and the strata lit factors (residential, commercial, mechanical, sky lobby; shares
        /// 70/15/10/5 % stay NocturneConstants.FacadeShare*, share x factor sums to 1.00). Section units (mode 1) light 0.13: their glass
        /// covers 0.62 of the face, 1.9 x the residential 0.33, so the plan's 0.21 made the Gulf faces 1.8 x the city mean (CRIT0 #6, TV static).</summary>
        public const double FaLit = 0.18, FaFactorRes = 1.08, FaFactorCom = 0.69, FaFactorMech = 0.23, FaFactorLobby = 2.31, FaSecLit = 0.13;
        /// <summary>Zone level z = FaZoneLo + FaZoneSpan x 0.5 (U_row + U_cell): rows of 2 floors keyed per tower, 24 columns per face (mean 1).</summary>
        public const double FaZoneLo = 0.40, FaZoneSpan = 1.2;
        /// <summary>Tower multiplier m_T and lit-floor-group factor g_G: log-normal, mean 1, clamped to [FaLnMin, FaLnMax]
        /// (the clamp of p at 1 then loses 0.2 % of the residential mean and 3 % of the lobby's: 0.4 % overall).</summary>
        public const double FaTowerSigma = 0.35, FaGroupSigma = 0.45, FaLnMin = 0.3, FaLnMax = 3.0;
        /// <summary>Lit floor groups of 4-12 floors, laid out per (tower key, band); g fades to 1 by smoothstep(3, 6, group px).</summary>
        public const int FaGroupMinRows = 4, FaGroupMaxRows = 12;
        public const double FaGroupFadePx0 = 3.0, FaGroupFadePx1 = 6.0;
        /// <summary>Lit glass radiance E = token x (FaYMed / Y(token)) x b x I / E[I]; b = exp(FaBSigma N) clamped to [FaBMin, FaBMax],
        /// E[b] = FaBMean (Box-Muller from two window hashes; checked by FacadeTests).</summary>
        public const double FaYMed = 0.12, FaBSigma = 0.45, FaBMin = 0.40, FaBMax = 2.40, FaBMean = 1.0954;
        /// <summary>Hue classes on one window hash U (LOOK_V2 4): [0, w_f/3) WinTungsten, [w_f/3, w_f) WinWarm, [w_f, w_f + n_f)
        /// WinNeutral, then WinCold up to FaHueBlue0, WinBlue (x FaBlueGain) up to FaHueCol0, then coloured (x FaColGain, each
        /// lerp(c, WinNeutral, FaColDesat)): NeonCyan 0.015, AviationRed 0.010, LeafGlow 0.010, NeonMagenta 0.005. The moving
        /// boundaries (w_f, w_f + n_f) are soft over FaHueBand; at w_f = 0 no window is warm.</summary>
        public const double FaHueBlue0 = 0.93, FaHueCol0 = 0.96, FaBlueGain = 0.6, FaColGain = 0.8, FaColDesat = 0.4, FaHueBand = 0.03;
        public const double FaColCyan = 0.015, FaColRed = 0.010, FaColLeaf = 0.010, FaColMagenta = 0.005;
        /// <summary>Interior factor I (LOOK_V2 4): ceiling falloff 1.0 at the head to FaFalloffSill at the sill; curtains on
        /// FaCurtainP of lit windows (x FaCurtainGain, tint lerp(hue, WinWarm, FaCurtainTint)), blinds on FaBlindP (slats of
        /// FaBlindPitch, open fraction FaBlindOpen, faded below FaBlindFadePx of slat pitch); curtains and blinds are exclusive.
        /// Only the zero-mean part I / E[I] - 1 fades in, over smoothstep(FaIntFadePx0, FaIntFadePx1) of window height (px).</summary>
        public const double FaFalloffSill = 0.70, FaCurtainP = 0.35, FaCurtainGain = 0.45, FaCurtainTint = 0.25;
        public const double FaBlindP = 0.15, FaBlindPitch = 0.08, FaBlindOpen = 0.45, FaBlindFadePx = 3.0;
        public const double FaIntFadePx0 = 4.0, FaIntFadePx1 = 8.0;
        /// <summary>Unlit glass interior: GlassDark x ambient_eq + WinNeutral x FaStandby x (0.5..1.5 by hash) (LOOK_V2 4).</summary>
        public const double FaStandby = 0.0005;
        /// <summary>Stateless epochs (H2, unchanged): 150 s epochs with a 1.2 s toggle ramp.</summary>
        public const double FaEpoch = 150.0, FaToggle = 1.2;

        // ------------------------------------------------------------------ skins and regions (TEXTURE_PLAN 2.2, 2.3)
        /// <summary>Region OQ (Old Quarter and the Heart ring): a disc about (FaOqX, FaOqZ) of radius FaOqR; D elsewhere. Skin byte
        /// 0x00-0x77 = D (seed % 120), 0x78-0xEF = OQ (0x78 + seed % 120), 0xF0 + k = hero override k.</summary>
        public const double FaOqX = 1250.0, FaOqZ = 388.0, FaOqR = 600.0;
        /// <summary>Region OQ lit-share factor (lead decision 6: the Old Quarter about 4 luma codes darker than region D; its skin
        /// shares alone give 1.9 codes): the lit probability x FaOqLit, never the window radiance (TEXTURE_PLAN 2.3).</summary>
        public const double FaOqLit = 0.78;
        public const int FaRegionSpan = 120, FaOqBase = 120, FaOverrideBase = 240;   // decimal: the HLSL generators parse literals as numbers
        /// <summary>Skin shares (S0 glass curtain wall, S1 precast, S2 metal ribbon, S3 brick, S4 tiled residential) per region,
        /// as cumulative thresholds on U_skin (S4 takes the rest).</summary>
        public const double FaShareD0 = 0.25, FaShareD1 = 0.55, FaShareD2 = 0.70, FaShareD3 = 0.75;
        public const double FaShareQ0 = 0.10, FaShareQ1 = 0.35, FaShareQ2 = 0.40, FaShareQ3 = 0.80;
        /// <summary>Tower variants keyed by K: S1 centre mullion, S2 ribbed panels, core strip (on faces wider than FaCoreMinFace),
        /// setback ledge line, mechanical stripe per residential band.</summary>
        public const double FaS1MullionP = 0.5, FaS2RibbedP = 0.3, FaCoreP = 0.4, FaCoreMinFace = 30.0, FaLedgeP = 0.3, FaMechStripeP = 0.75;

        // ------------------------------------------------------------------ whole bays and bands (TEXTURE_PLAN 2.5)
        /// <summary>Openings lie inside [uMin + FaCornerPier, uMax - FaCornerPier] x [y0 + base, y1 - FaParapet]; base is FaBase on
        /// faces whose y0 is at or below FaBaseY0 (podium faces), else 0. The parapet coping line is FaCoping tall at the top.</summary>
        public const double FaCornerPier = 0.6, FaParapet = 1.6, FaBase = 6.0, FaBaseY0 = 0.0, FaCoping = 0.12;
        /// <summary>Setback ledge line: FaLedgeH tall at y1, drawn at least FaLedgeMinPx (energy x true / drawn), WinNeutral at FaLedgeGain x Y_med.</summary>
        public const double FaLedgeH = 0.3, FaLedgeMinPx = 1.0, FaLedgeGain = 0.6;
        /// <summary>Corner trim (metal flashing) within FaCornerTrim of a face edge.</summary>
        public const double FaCornerTrim = 0.25;

        // ------------------------------------------------------------------ cell geometry (TEXTURE_PLAN 2.3; docs/FACADE_V2.md table)
        /// <summary>Frame and mullion of residential openings (LOOK_V2: 0.12 m MetalPaint inside the opening).</summary>
        public const double FaFrame = 0.12;
        // S0 glass curtain wall: metal slab cap [0, FaS0Cap), spandrel glass [FaS0Cap, FaS0Sp1), transom, vision [FaS0Oy0, FaS0Oy1],
        // mullions 0.12 at the cell edges (outside the opening) and at FaS0Mid; glass nearly flush (recess FaS0D).
        public const double FaS0Cap = 0.18, FaS0Sp1 = 1.89, FaS0Ox0 = 0.06, FaS0Ox1 = 2.34, FaS0Oy0 = 1.95, FaS0Oy1 = 3.54, FaS0Mull = 0.12, FaS0D = 0.05;
        // S1 precast: slab band FaS1Slab, opening 1.6 x 2.2 at sill 0.9 (variant B: 1.72 wide with a centre mullion), recess 0.22.
        public const double FaS1Slab = 0.35, FaS1Ox0 = 0.40, FaS1Ox1 = 2.00, FaS1BOx0 = 0.34, FaS1BOx1 = 2.06, FaS1Oy0 = 0.90, FaS1Oy1 = 3.10, FaS1D = 0.22;
        // S2 metal ribbon: panel band FaS2Slab, ribbon 1.8 tall at sill 0.9 between edge mullions, centre mullion 0.08, recess 0.08.
        public const double FaS2Slab = 0.30, FaS2Ox0 = 0.06, FaS2Ox1 = 2.34, FaS2Oy0 = 0.90, FaS2Oy1 = 2.70, FaS2Frame = 0.05, FaS2Mull = 0.08, FaS2D = 0.08;
        // S3 brick: window 1.2 x 1.6 at sill 0.85, frame 0.08, recess 0.18, stone sill and lintel bands FaS3Stone tall.
        public const double FaS3Slab = 0.12, FaS3Ox0 = 0.60, FaS3Ox1 = 1.80, FaS3Oy0 = 0.85, FaS3Oy1 = 2.45, FaS3Frame = 0.08, FaS3D = 0.18, FaS3Stone = 0.12;
        // S4 tiled residential: window 1.4 x 1.6 at sill 0.9, frame 0.08, slab edge 0.25, recess 0.15.
        public const double FaS4Slab = 0.25, FaS4Ox0 = 0.50, FaS4Ox1 = 1.90, FaS4Oy0 = 0.90, FaS4Oy1 = 2.50, FaS4Frame = 0.08, FaS4D = 0.15;
        // commercial 4.8 x 4.32: opening 4.4 x 3.2 (LOOK_V2 cover 0.679), frame 0.10, mullions 0.08 every 1.1 m, recess 0.10 (0.25 on S1/S3)
        public const double FaComOx0 = 0.20, FaComOx1 = 4.60, FaComOy0 = 0.60, FaComOy1 = 3.80, FaComFrame = 0.10, FaComMull = 0.08, FaComMullP = 1.1, FaComD = 0.10, FaComDDeep = 0.25;
        // mechanical 9 x 6.171: louvre slit 8.4 x 0.8, frame 0.05, louvre blades of FaLouvreW every FaLouvreP (open fraction 0.5)
        public const double FaMechOx0 = 0.30, FaMechOx1 = 8.70, FaMechOy0 = 2.70, FaMechOy1 = 3.50, FaMechFrame = 0.05, FaMechD = 0.15, FaLouvreP = 0.15, FaLouvreW = 0.075;
        // sky lobby 18 x 14.4: opening 17 x 12 in stone portal frames, frame 0.15, mullions 0.12 every 3.4 m, a transom at mid height
        public const double FaLobOx0 = 0.50, FaLobOx1 = 17.50, FaLobOy0 = 1.20, FaLobOy1 = 13.20, FaLobFrame = 0.15, FaLobMull = 0.12, FaLobMullP = 3.4, FaLobD = 0.30;
        // section bays (mode 1) 9 x 4: opening 8.2 x 3.2, frame 0.10, mullions 0.10 every 2.05 m, cut slab 0.5, recess 0.5
        // (Look v2 section cells are half bays: 4.5 x 4 m lit units, so a 9 m bay holds two rooms and the cut face reads as rooms, not
        // as 8 m light blocks; the column line every FaSecColP keeps the 9 m structure)
        public const double FaSecP = 4.5, FaSecColP = 9.0, FaSecColW = 0.45;
        public const double FaSecSlab = 0.50, FaSecOx0 = 0.25, FaSecOx1 = 4.25, FaSecOy0 = 0.50, FaSecOy1 = 3.70, FaSecFrame = 0.10, FaSecMull = 0.10, FaSecMullP = 2.0, FaSecD = 0.50;
        /// <summary>Section macro (mode 1, TEXTURE_PLAN 2.10, CRIT0 #6): districts of FaSecDistU x FaSecDistV (28 bays x 75 floors) with a
        /// log-normal lit-density multiplier (sigma FaSecDistSigma, mean 1); a mechanical band FaSecMechH tall at each district's
        /// bottom (louvre wall, one FaSecSlitH slit per floor lit on FaSecSlitLit of the floors at 0.6 Y_med, amber x k_a or SignWhite);
        /// one glazed atrium FaSecAtriumW x FaSecAtriumH per district, lit on FaSecAtriumLit of them at FaSecAtriumGain x Y_med,
        /// mullions FaSecAtriumMull every FaSecAtriumMullU m and every floor.</summary>
        public const double FaSecDistU = 252.0, FaSecDistV = 300.0, FaSecDistSigma = 0.8, FaSecMechH = 12.0, FaSecSlitH = 0.6, FaSecSlitLit = 0.6;
        public const double FaSecAtriumW = 27.0, FaSecAtriumH = 48.0, FaSecAtriumLit = 0.7, FaSecAtriumGain = 0.8, FaSecAtriumMull = 0.25, FaSecAtriumMullU = 4.5;
        /// <summary>Atrium hall profile: per floor a lit balcony edge FaSecAtriumEdge tall (x (FaSecHallBase + FaSecHallEdge x line)),
        /// brighter toward the hall floor (x (1.3 - 0.6 t), t = height in the hall 0..1); mean factor (0.25 + 1.6 x 0.2) x 1.0 = 0.57
        /// (the halls read as stacked lit balconies, not as a lit board: gain 0.8).
        /// Slits: the mechanical bands' slits are cut into 9 m bay segments (FaSecSlitGap at the columns), each lit by its own hash.</summary>
        public const double FaSecAtriumEdge = 0.8, FaSecHallBase = 0.25, FaSecHallEdge = 1.6, FaSecSlitGap = 1.0;
        /// <summary>Louvre walls (mechanical stripes and section mechanical bands): MetalPaint x FaLouvreAlb under blades of FaLouvreW
        /// every FaLouvreP (each blade darkens by half; mean factor 0.75), AO FaAoLouvre.</summary>
        public const double FaLouvreAlb = 2.0, FaAoLouvre = 0.85;
        /// <summary>S4 loggias (TEXTURE_PLAN 2.3): on FaLoggiaP of the residential bands of an S4 tower every bay is a loggia - the
        /// opening [FaLogX0, FaLogX1] x [slab, P.y] recessed FaLoggiaD, sliding doors [FaLogDoorX0, FaLogDoorX1] x [slab, slab +
        /// FaLogDoorH] at the back with a centre mullion, a top rail FaLogRailW tall at slab + FaLogRailY and balusters FaLogBalW
        /// every FaLogBalP below it; the corner bays and the floor under the parapet stay solid wall in those bands.</summary>
        public const double FaLoggiaP = 0.4, FaLoggiaD = 1.4, FaLogX0 = 0.10, FaLogX1 = 2.30, FaLogDoorX0 = 0.20, FaLogDoorX1 = 2.20, FaLogDoorH = 2.2;
        public const double FaLogRailY = 1.05, FaLogRailW = 0.05, FaLogBalP = 0.12, FaLogBalW = 0.03;
        /// <summary>Hero details (TEXTURE_PLAN 2.2 overrides): the spire's S0 fins (override 0) - anodised blades FaFinW wide and FaFinD
        /// deep at every mullion line, their side widening them by |kx| FaFinD at oblique views, albedo MetalPaint x FaFinAlb; the
        /// screen-wall mass's S2 LED strips (override 1, CRIT0 #22) - one strip FaLedH tall in a FaLedHousing housing in every slab
        /// band, lit on FaLedLit of the floors at FaLedGain x Y_med, SignWhite / WinNeutral / WinCold by hash, drawn >= 1 px.</summary>
        public const double FaFinW = 0.08, FaFinD = 0.5, FaFinAlb = 2.5, FaLedH = 0.06, FaLedHousing = 0.3, FaLedLit = 0.7, FaLedGain = 0.5;
        /// <summary>Photo skins (TEXTURE_PLAN 1.2, 1.6, 2.3; FacadeArr of Tools/texprep/facade.json): the wall, slab, spandrel and reveals
        /// of a tower take one slice (S1 precast 0 or, on FaS1WeatheredP of the towers, weathered concrete 1; S2 metal panel 2 or
        /// ribbed metal 3; S3 brick 5; S4 facade tile 4; S0 caps metal panel 2; section 1; roofs 0), cell-aligned panel shuffle with
        /// the offsets in whole registered courses / lifts / ribs / modules, faded to the last mip by NocTexFade (2-4 px per tile).
        /// Albedo x 2 x variation (mean 1), smoothness = the slice's dry smoothness + FaWetLift (vertical faces wet 0.6, LOOK_V2 5.2),
        /// AO x lerp(1, mask AO, FaTexAo), the normal as a surface gradient in the facade frame.</summary>
        public const double FaS1WeatheredP = 0.15, FaWetLift = 0.15, FaTexAo = 0.5;
        /// <summary>Sill streaks (TEXTURE_PLAN 2.5, LOOK_V2 4: -20 % albedo): under every opening the OverlayArr slice 0 leak decal
        /// (TP), FaStreakLen x a hashed stretch 1..FaStreakStretch long from the sill down, its column offset by the cell hash, the
        /// decal x across the window width; albedo x (1 - FaStreakDark coverage), smoothness + FaStreakSmooth coverage; faded to its
        /// mean profile 2 FaStreakMean (1 - d / len) below 2-4 px of the 2 m tile.</summary>
        public const double FaStreakLen = 1.1, FaStreakStretch = 1.6, FaStreakDark = 0.35, FaStreakSmooth = 0.25, FaStreakMean = 0.12;
        /// <summary>Skin details: S2 ribbed panels (30 % of S2 towers) - ribs FaRibW every FaRibP, albedo +- FaRibTone around the mean;
        /// S3 stone sills and lintels FaS3Stone tall, FaS3StoneX wider than the window on each side (Granite).</summary>
        public const double FaRibP = 0.3, FaRibW = 0.06, FaRibTone = 0.5, FaS3StoneX = 0.1;
        /// <summary>Panel joints: FaJoint wide (darker, rougher), drawn at least 1 px with energy w / drawn, mean below 4 px.</summary>
        public const double FaJoint = 0.015, FaJointDark = 0.45;

        // ------------------------------------------------------------------ materials (tokens x factors; the token is the mean colour)
        /// <summary>Wall token factors: S2 MetalPaint x FaS2Albedo, S4 Plaster x FaS4Albedo; per-panel tone +- FaPanelTone faded by
        /// smoothstep(2, 4) of panel px; per-face albedo +- FaFaceTone.</summary>
        public const double FaS2Albedo = 2.0, FaS4Albedo = 0.75, FaPanelTone = 0.12, FaFaceTone = 0.04;
        /// <summary>Smoothness (perceptual): concrete / render walls, metal walls, frames (anodised constant), reveals, wet sills.</summary>
        public const double FaSmoothWall = 0.30, FaSmoothMetal = 0.45, FaSmoothFrame = 0.55, FaSmoothSill = 0.85;
        /// <summary>Ambient occlusion of slab soffit lines, reveals and recessed frames.</summary>
        public const double FaAoSlab = 0.85, FaAoReveal = 0.60, FaAoFrame = 0.75;
        /// <summary>Frontal mean sky reflection per unit area of wall (LOOK_V2 4: 0.0006) and of glass (F0 0.04 x sky ~0.02), for
        /// the unresolved MeanRadiance only (the shader reflects analytically).</summary>
        public const double FaWallSpec = 0.0006, FaGlassSpec = 0.0008;
        /// <summary>Room-light spill (TEXTURE_PLAN 2.6): lit window radiance x albedo x F on jambs, sill, head and frame edges, and a
        /// halo below the opening E x FaHalo x exp(-d / FaHaloL) for d in [0, FaHaloMax].</summary>
        public const double FaSpillJamb = 0.25, FaSpillSill = 0.35, FaSpillHead = 0.15, FaSpillFrame = 0.20, FaHalo = 0.02, FaHaloL = 0.4, FaHaloMax = 1.2;

        // ------------------------------------------------------------------ glass (TEXTURE_PLAN 2.7, LOOK_V2 4)
        /// <summary>Dielectric F0, perceptual roughness, pane tilt +- deg (folded into roughness below FaTiltFadePx of pane size).</summary>
        public const double FaGlassF0 = 0.04, FaGlassRough = 0.05, FaPaneTilt = 1.5, FaTiltFadePx = 2.0;
        /// <summary>Reflected skyline (TEXTURE_PLAN 2.7, FA-2): azimuth bins of FaSkyBin deg, each cut into 1-3 tower slots; a tower of
        /// hashed width FaSkyW0..FaSkyW1 m fills its slot (so it stands at d = W / slot angle); its body is the unresolved facade
        /// mean with lit glass FaSkyGlass per facade area, speckled on the city's cell grid with one window rectangle
        /// [FaSkyWinX0, FaSkyWinX1] x [FaSkyWinY0, FaSkyWinY1] per 2.4 x 3.6 m cell (area 0.332 = FaSkyGlass: opening cover 0.4385 x
        /// glass 0.757, LOOK_V2 4).</summary>
        public const double FaSkyBin = 4.0, FaSkyW0 = 34.0, FaSkyW1 = 64.0, FaSkyGlass = 0.332;
        public const double FaSkyWinX0 = 0.5, FaSkyWinX1 = 1.9, FaSkyWinY0 = 1.0, FaSkyWinY1 = 3.05;

        // ------------------------------------------------------------------ interiors (TEXTURE_PLAN 2.8, FA-3)
        /// <summary>Box rooms (one per window cell, x 0..P.x, y 0..P.y - FaRoomSoffit, depth by hash): surface radiance relative to the
        /// window's mean, each surface linear in its exit depth w (NocInterior.hlsl) - back wall FaRoomBack (0.85 + 0.3 v), side walls
        /// FaRoomSide (1.3 - 0.6 w), ceiling FaRoomCeil (1.25 - 0.5 w), floor FaRoomFloor (1.2 - 0.4 w); faded in by
        /// smoothstep(FaBoxPx0, FaBoxPx1) of window height in px.</summary>
        public const double FaRoomBack = 1.0, FaRoomSide = 0.6, FaRoomCeil = 0.9, FaRoomFloor = 0.4;
        public const double FaRoomSoffit = 0.3, FaRoomDepthRes0 = 6.0, FaRoomDepthRes1 = 10.0, FaRoomDepthCom0 = 10.0, FaRoomDepthCom1 = 14.0, FaRoomDepthSec0 = 8.0, FaRoomDepthSec1 = 12.0;

        // ------------------------------------------------------------------ distance ladder (TEXTURE_PLAN 2.9)
        /// <summary>Per-axis window fade smoothstep(2.5, 5.5, pitch px) (H2, unchanged); box rooms smoothstep(5, 9, window px);
        /// textured rooms smoothstep(24, 40); photo skins smoothstep(2, 4, tile px); panel tone smoothstep(2, 4, panel px).</summary>
        public const double FaFadePx0 = 2.5, FaFadePx1 = 5.5, FaBoxPx0 = 5.0, FaBoxPx1 = 9.0, FaRoomPx0 = 24.0, FaRoomPx1 = 40.0;
        public const double FaTexPx0 = 2.0, FaTexPx1 = 4.0;
        /// <summary>Visible-glass sliver fade: an axis of the window field falls to its view-dependent mean while the glass left visible
        /// behind the recess is under FaSliverPx0..FaSliverPx1 px wide (oblique views, loggias), so lit slivers never sparkle.</summary>
        public const double FaSliverPx0 = 1.0, FaSliverPx1 = 2.0;
        /// <summary>v2 edge lights: drawn at least FaEdgeMinPx wide (v1: 2) with a FaEdgeBlurPx-wide box (energy kept), so the bright
        /// line at a tower's corner does not shimmer in pans (texpans vs the -capture-ss 2 reference: KS04 / KS05 blocks 5.5 / 7.7 levels).</summary>
        public const double FaEdgeMinPx = 3.0, FaEdgeBlurPx = 1.5, FaEdgeInsetPx = 1.25;
        /// <summary>Display-space coverage of thin HDR bands (INT-05 / FA-D19, Phase 4 FX): a partly covered pixel of an edge band carries
        /// the radiance that the grade's Neutral highlight curve (identity below FaToneStart = NocGradeMath.NeutralStart, exposure
        /// gain 2^EV with |EV| &lt;= 0.2 taken as 1) displays as the coverage times the band's displayed peak, which is what the
        /// -capture-ss 2 reference's box after the grade does; so the partly covered pixels of a band brighter than the curve's start
        /// no longer clip (stair-steps that crawl in pans). Full coverage and bands below the start are unchanged
        /// (NocFacadeMath.DisplayCover; FacadeSkinTests).</summary>
        public const double FaToneStart = 0.76;
        /// <summary>Face extents (uMin, uMax, y0, y1) are snapped to 1 / FaExtentQ m in the v2 shader, so the per-pixel interpolation
        /// noise of these per-face constants (about 1 ulp, 5e-4 m at 4 km) never flips a whole-bay, core-strip or row decision (TQ-09:
        /// columns of a 30 m face flickered between frames). Writers keep u extents off the rounding half-steps by FaExtentGuard m
        /// (NocFacadeMath.SafeExtent: a 1 mm nudge on about 3 % of faces) and write y0 / y1 on the grid (outward).</summary>
        public const double FaExtentQ = 16.0, FaExtentGuard = 0.001;
    }
}
