// HeartLayout: every dimension of beat 7, Heart Square (MASTER_PROMPT 14.4 "Heart", 14.5.7 Geometry and Light, 14.2.3 row 7,
// 14.11 goal, 14.12 "Heart"), shared by the builders and Beat07HeartTests. Units metres; x east, z north, y up.
// The walk: C3 (centre (931.74, 357.88), r 30) to s 1 283.56, then the east leg z 387.88, x = s - 351.82, to the goal
// (1 148.18, 387.88) at s 1 500; lane half-width 12 (14.4) to s 1 440, then a funnel to 3.0 at the tenement.
namespace Megastructure
{
    public static class HeartLayout
    {
        // ------------------------------------------------------------------ plaza (14.4: circle R 400 about S)
        public const double SX = NocturneConstants.SpireX, SZ = NocturneConstants.SpireZ, PlazaR = NocturneConstants.PlazaRadius;
        /// <summary>The plaza slab (and the floors between the promenade and the circle) is 2 m thick, top y 0 (agreement 1).</summary>
        public const double FloorThickness = 2.0;
        /// <summary>Segments of the circle outline (the 400 m circle deviates 1.2 cm from its polygon at 1 024 segments).</summary>
        public const int PlazaSegments = 1024;
        /// <summary>The fill between the promenade (x &lt;= 914) and the circle: x 914 to the circle for z 360 (the arcade's north
        /// face) to z 520 (the screen wall's north end), so the C3 corner east of x 914 and the gap x 914 - 931 are floored.</summary>
        public const double FillX0 = 914.0, FillZ0 = 360.0, FillZ1 = 520.0;
        /// <summary>The walk's east-leg offset: x = s - 351.82 for s >= 1 283.56 (14.4: C3 tangent out at (931.74, 387.88)).</summary>
        public const double EastLegOffset = 351.82, EastLegZ = 387.88, EastLegS0 = 1283.56;

        // ------------------------------------------------------------------ spire (14.5.7)
        /// <summary>Shaft y -3 300..560: R 32 at the plaza (y 0) tapering linearly to 20 at y 460, then 20 to the top; below the
        /// plaza R 32. 24 faces (NocFacade cold windows, warm fraction 0 in beat 7).</summary>
        public const double SpireY0 = -3300.0, SpireY1 = 560.0, SpireR0 = 32.0, SpireRTop = 20.0, SpireTaperY = 460.0;
        public const int SpireSides = 24;
        /// <summary>Base drums R 48 / 42 / 36, 6 m each (y 0 - 6, 6 - 12, 12 - 18), each sunk 0.1 m into the one below (no co-planar caps).</summary>
        public static readonly double[] DrumR = { 48.0, 42.0, 36.0 };
        public const double DrumH = 6.0, DrumSink = 0.1;
        /// <summary>12 rib fins (thin boxes) on every second shaft corner from the top drum to the top: 4 m deep, 1.2 m thick,
        /// 0.4 m inside the shaft; cyan-white NocLine strips 0.5 m wide on the outer edges.</summary>
        public const int FinCount = 12;
        public const double FinDepth = 4.0, FinThick = 1.2, FinInset = 0.4, StripWidth = 0.5, StripGain = 1.5;

        /// <summary>The podium (CRIT0 #21, HT): three granite steps around the lowest drum, outer radii PlinthR, each PlinthStep tall.</summary>
        public static readonly double[] PlinthR = { 56.0, 53.0, 50.5 };
        public const double PlinthStep = 0.16;
        /// <summary>Ring girders (hoops) every HoopPitch from HoopPitch to HoopTop (11, below the cloud base 460), HoopH tall, from
        /// R(y) - 0.3 to R(y) + HoopOut (inside the fins' outer edge R + 4, so the 14.5.7 envelope R(y) + 4 holds); on each outer face a
        /// NocLine ring light (StripWidth) that breathes at RingBreathHz (0.1 Hz; 14.10 photosensitivity <= 0.2 Hz) between
        /// RingLightLow and 1 of RingLightGain x NeonCyan (14.5.7), ring k lagging k RingLag rad: a slow pulse rises up the spire
        /// (2 pi / 0.5 = 12.6 rings per period: 503 m per 10 s). HT-06 (fifth start): at the strips' colour and gain (1.5) the
        /// 12 strips and 11 rings read as a cyan wire cage (FX_r03 KS17 / KS18); the rings are now the subordinate accent:
        /// NeonCyan x 0.6 (peak G 0.47, B 0.60 linear against the strips' 1.35 / 1.5), so the verticals carry the spire.</summary>
        public const double HoopPitch = 40.0, HoopTop = 440.0, HoopH = 1.6, HoopOut = 3.6;
        public const double RingBreathHz = 0.1, RingLightLow = 0.55, RingLightGain = 0.6, RingLag = 0.5;
        /// <summary>Buttress flare of the fins: the outer edge stands FinFlare beyond R(y) + FinDepth at y FlareY0 (on the lowest drum,
        /// R 48: 31.84 + 4 + 9.5 = 45.34) and joins R(y) + FinDepth at FlareY1 (smoothstep), in FlarePieces straight pieces; the strip
        /// follows the edge down to the drum. Below FlareY1 = 96 m only: the koi (lowest 184 m) never meets it.</summary>
        public const double FinFlare = 9.5, FlareY0 = 6.0, FlareY1 = 96.0;
        public const int FlarePieces = 12;

        /// <summary>Outer edge radius of a fin at height y (the flare, then R(y) + FinDepth).</summary>
        public static double FinOuter(double y)
        {
            double t = System.Math.Max(0.0, System.Math.Min(1.0, (y - FlareY0) / (FlareY1 - FlareY0)));
            return SpireRadius(y) + FinDepth + FinFlare * (1.0 - t * t * (3.0 - 2.0 * t));
        }

        /// <summary>Spire radius (corner radius of the 24-gon) at height y.</summary>
        public static double SpireRadius(double y)
        {
            if (y <= 0.0) return SpireR0;
            if (y >= SpireTaperY) return SpireRTop;
            return SpireR0 + (SpireRTop - SpireR0) * (y / SpireTaperY);
        }

        // ------------------------------------------------------------------ ring towers (14.5.7: 22 at R 470 - 560, west +-40 deg clear)
        public const int RingTowerCount = 22;
        public const double RingInner = 470.0, RingOuter = 560.0, RingCityExclusion = 456.0;
        /// <summary>Bearings (deg from +X toward +Z) within +-40 deg of west (180) carry no tower: towers on 220..500 deg.</summary>
        public const double RingWestClear = 40.0;
        public const double RingHalfMin = 17.0, RingHalfMax = 24.0;

        // ------------------------------------------------------------------ Old Quarter (14.4, 14.5.7)
        /// <summary>Tenement x 1 156 - 1 170, z 379 - 397, 14.4 m (4 storeys of 3.6 m).</summary>
        public const double TenX0 = 1156.0, TenX1 = 1170.0, TenZ0 = 379.0, TenZ1 = 397.0, TenH = 14.4, Storey = 3.6;
        /// <summary>Neighbours: south z 355 - 377 (11.0 m), north z 399 - 421 (12.5 m), their west faces 1 m behind the tenement's.</summary>
        public const double NbX0 = 1157.0, NbX1 = 1171.0, NbSZ0 = 355.0, NbSZ1 = 377.0, NbSH = 11.0, NbNZ0 = 399.0, NbNZ1 = 421.0, NbNH = 12.5;
        /// <summary>The back row of the quarter (x 1 173 - 1 202, three blocks over z 355 - 421).</summary>
        public const double BackX0 = 1173.0, BackX1 = 1202.0;
        public static readonly double[] BackZ = { 355.0, 377.0, 399.0, 421.0 };
        public static readonly double[] BackH = { 9.6, 13.2, 10.4 };
        /// <summary>Buildings stand 1 m deep in the 2 m plaza slab (no base face on the floor plane).</summary>
        public const double BuildingY0 = -1.0;
        /// <summary>Dark windows 1.2 x 1.5 m (14.5.7), 0.8 m above each storey floor, columns every 3 m about the goal window.</summary>
        public const double WinW = 1.2, WinH = 1.5, WinSill = 0.8, WinPitch = 3.0, WinProud = 0.03;

        // ------------------------------------------------------------------ the goal (14.5.7, 14.11, 14.2.3 row 7)
        public const double GoalX = NocturneConstants.GoalWindowX, GoalY0 = NocturneConstants.GoalWindowY0, GoalY1 = NocturneConstants.GoalWindowY1;
        public const double GoalZ0 = NocturneConstants.GoalWindowZ0, GoalZ1 = NocturneConstants.GoalWindowZ1, GoalZC = 0.5 * (GoalZ0 + GoalZ1);
        /// <summary>Wall thickness at the window (the reveal) and the room behind it (storey 2: floor 3.6, ceiling 7.0).</summary>
        public const double WallT = 0.3, RoomX1 = 1159.6, RoomZ0 = 385.4, RoomZ1 = 390.4, RoomY0 = 3.6, RoomY1 = 7.0;
        /// <summary>Balcony slab 2.6 x 1.4 m, top y 4.2, 0.18 m thick, x 1 154.6 - 1 156, centred on the window.</summary>
        public const double BalX0 = 1154.6, BalX1 = 1156.0, BalW = 2.6, BalTop = 4.2, BalT = 0.18;
        /// <summary>Railing 0.9 m: top rail 0.05 m, rods 0.02 m every 0.11 m (NocLine tubes), on the three open sides.</summary>
        public const double RailH = 0.9, RailTop = 0.05, RodW = 0.02, RodStep = 0.11, RailInset = 0.05;
        /// <summary>Fix wave C35: rod pitch on the two side returns (seen nearly edge-on from KS19 the 0.11 m rods fell to 1-1.5 px and
        /// aliased into a hatch at 1080p): 1.33 m / 4 = 0.33 m, 3 x the front pitch, so the projected pitch stays >= 3 px.</summary>
        public const double RodStepSide = 0.33;
        /// <summary>Fix wave C18: the balcony slab (CC0 concrete skin) glows with the window's bounce, GoalLamp x BalconyBounce with
        /// the emission following the albedo (LOOK_V2 6 / 8.1: the underside lit 50-80 codes; it was 27, sat 0.09).</summary>
        public const double BalconyBounce = 0.06;
        /// <summary>Pot 0.35 m (radius 0.16) with a 0.6 m plant of 12 dark leaf blades, at the balcony's south corner.</summary>
        public const double PotH = 0.35, PotR = 0.16, PlantH = 0.6;
        public const int PlantBlades = 12;
        /// <summary>The table lamp: a 0.2 m sphere at (1 157.2, 5.0, 387.9), GoalLamp x 3.5 (14.2.1: the only core above 3.0).</summary>
        public const double LampX = 1157.2, LampY = 5.0, LampZ = 387.9, LampR = 0.1, LampGain = 3.5;
        /// <summary>The real point light of 14.2.3 row 7 (r 14, i 5) at the window plane.</summary>
        public const double GoalLightX = NocturneConstants.GoalLampX, GoalLightY = NocturneConstants.GoalLampY, GoalLightZ = NocturneConstants.GoalLampZ;
        public const double GoalLightRange = 14.0, GoalLightIntensity = 5.0;
        /// <summary>Lamp-lit room surfaces (GoalLamp x gain): back wall, side walls, ceiling, floor, the half-drawn curtain. HT (INT-08):
        /// the base glow is lowered (was 0.55 / 0.32 / 0.22 / 0.12 / 0.42); the room light adds the falloff. Fifth start: back 0.30 -> 0.20
        /// and ceiling 0.10 -> 0.07 (HT_r01 KS19: the upper back wall above the picture was a flat pale panel; the lamp's own light now
        /// carries the gradient from the globe up the wall).
        /// Fix wave C18 (panel p4_01): the aperture read 117 against LOOK_V2's 165-195 (about 2.2 x in linear light); HT_r04 with the
        /// base x 1.7 and the light x 1.6 read 134, HT_r05 (x 2.75) 148: from KS19 the upper sash looks up at the CEILING (it read as a
        /// grey veil at 0.18): ceiling 0.55 (the lamp's warm pool overhead), back 0.70, sides 0.60, curtain 0.90, the room light
        /// x 2.5 (0.20 / 0.18 / 0.07 / 0.08 / 0.30 and i 4.0 before the fix wave).</summary>
        public const double RoomBack = 0.70, RoomSide = 0.60, RoomCeil = 0.55, RoomFloor = 0.18, CurtainGain = 0.90;
        /// <summary>D-034 6 (lead ruling, fix wave): an empty lit room behind the ground-floor window of column -1 (z 384.3 - 385.5):
        /// a table, a chair pushed back, a cup, a desk lamp on (cold: the goal window stays the only warm window), nobody. Room box
        /// x from the dark reveal's back to EmptyX1, y 0 - EmptyY1, z EmptyZ0 - EmptyZ1 (clear of the goal room above, y >= 3.6,
        /// and of the next openings); walls WinCold x EmptyGlow (emission follows the plaster); the lamp's light WinCold r / i.</summary>
        public const int EmptyStorey = 0, EmptyColumn = -1;
        public const double EmptyX1 = 1160.2, EmptyY1 = 3.2, EmptyZ0 = 382.6, EmptyZ1 = 387.2, EmptyGlow = 0.012, EmptyBulbGain = 2.2;
        public const double EmptyLightRange = 4.0, EmptyLightIntensity = 1.8;
        /// <summary>Faint cold glow of the two curtained dark windows (WinCold x), the entrance lantern's frosted glass (x).</summary>
        public const double DarkCurtainGlow = 0.05, LanternGain = 1.4;

        // ------------------------------------------------------------------ the goal window's joinery, curtain and room (INT-08, HT)
        /// <summary>Box-frame lining (jambs, head, sill board) GoalFrameW wide from 0.04 m behind the face to the reveal's back; sashes
        /// GoalSashW (stiles and rails) in two planes; the upper sash's meeting rail top at GoalMeetY with one vertical glazing bar;
        /// the lower sash raised GoalSashRaise, so its bottom rail (4.775 - 4.82 m) stays above the KS19 ray to the lamp's top (4.70 m
        /// at the frame plane) and the lamp is seen through the open bottom of the window; the outer stone sill GoalSillT thick,
        /// GoalSillProud in front of the face, GoalSillOver past the opening on each side, its top 1 cm under the opening.</summary>
        public const double GoalFrameW = 0.06, GoalSashW = 0.045, GoalMeetY = 5.17, GoalSashRaise = 0.34;
        public const double GoalSillT = 0.06, GoalSillProud = 0.12, GoalSillOver = 0.08;
        /// <summary>The half-drawn curtain: a pleated sheet (pitch, pleat amplitude at the leading edge and at the gathered side) from
        /// the south side of the room to z 387.75 (37 % of the opening), hung from a rod at y CurtainRodY, CurtainX behind the face;
        /// its own deep rust albedo (linear), so the outdoor goal light (0.85 m in front of it) shades the folds instead of blowing
        /// out a pale slab, and its glow (GoalLamp x CurtainGain) is the lamp's light through the cloth.</summary>
        public const double CurtainX = TenX0 + WallT + 0.12, CurtainZ0 = RoomZ0 + 0.3, CurtainZ1 = 387.75, CurtainTop = 5.97, CurtainBottom = 4.0;
        public const double CurtainPitch = 0.11, CurtainAmpEdge = 0.028, CurtainAmpGather = 0.05, CurtainRodY = 6.0;
        public static readonly UnityEngine.Color CurtainAlbedo = new UnityEngine.Color(0.16f, 0.075f, 0.035f, 1f);
        /// <summary>The table lamp's own light inside the room (GoalLamp, r 5.5, i 4.0; LOOK_V2 6 "warm falloff into the room"): the
        /// back wall, the ceiling and the furniture fall off with distance from the lamp (the room's emissive glow is lowered to a
        /// base: RoomBack / RoomSide / RoomCeil / RoomFloor).</summary>
        public const double RoomLightRange = 5.5, RoomLightIntensity = 10.0;
        /// <summary>The back wall's furniture seen through the window from KS19 (the visible band is y 5.64 - 7.0 at z 387.0 - 388.8):
        /// a picture (centre z, y; width, height; frame bar), a shelf (y, z range, depth) with books.</summary>
        public const double PictureZ = 388.25, PictureY = 6.2, PictureW = 0.56, PictureH = 0.42, PictureBar = 0.035;
        public const double ShelfY = 6.52, ShelfZ0 = 386.85, ShelfZ1 = 387.85, ShelfD = 0.22;

        // ------------------------------------------------------------------ the tenement's other windows and its door (INT-08, HT)
        /// <summary>Dark windows of the tenement: openings in the brick (not proud panes), reveal DarkRevealD deep, dark glass at its
        /// back, a frame lining DarkFrameW, a casement mullion and a transom; a stone sill and a stone lintel on the face.</summary>
        public const double DarkRevealD = 0.18, DarkFrameW = 0.06, DarkSillT = 0.06, DarkSillProud = 0.1, DarkSillOver = 0.06;
        public const double LintelH = 0.14, LintelProud = 0.015, LintelOver = 0.08;
        /// <summary>The entrance: a door 1.1 x 2.3 m under the entrance light (it replaces the ground-floor window of column +1),
        /// recessed DoorRevealD, with a stone step.</summary>
        public const double DoorW = 1.1, DoorH = 2.3, DoorRevealD = 0.25;

        // ------------------------------------------------------------------ plaza furniture (14.5.7, 14.2.3 row 7)
        /// <summary>20 plaza lamps 6 m (WinCold, r 12 i 3): two rows 16 m either side of the east leg, every 20 m from x 950.</summary>
        public const int PlazaLampCount = 20;
        public const double PlazaLampX0 = 950.0, PlazaLampStep = 20.0, PlazaLampLateral = 16.0, PlazaLampH = 6.0;
        public const double PlazaLampRange = 12.0, PlazaLampIntensity = 3.0, PlazaLampHeadGain = 2.0;
        /// <summary>The lamp fixture (INT-12, HT): a lathe of a cast plinth (R 0.2, 0.45 m), a pole tapering from R 0.09 to 0.055, a
        /// collar; the luminaire a dark disc housing (R 0.42) over its lens (R LampLensR at LampLensY, facing down, SignWhite x
        /// LampLensGain); the real light LampLightDrop under the lens, the halo and the H4 source at the lens.</summary>
        public const double LampLensY = 5.70, LampLensR = 0.315, LampLensGain = 2.0, LampLightDrop = 0.35;
        /// <summary>HT-05: the lit part is a frosted bowl under the housing (rim R 0.31 at LampLensY down to LampBowlY on the axis),
        /// not a flat disc: a disc seen edge-on from eye height was a 1-2 px sliver at 100-250 m (NaN-like pixels in KS17); the
        /// bowl is 0.22 m tall from the side. Halo and H4 source at its centre LampGlowY.</summary>
        public const double LampBowlY = 5.485, LampGlowY = 5.60;
        /// <summary>Benches (scale cues near the walk): one between every second pair of lamp columns (x BenchX0 + BenchStep k, k 0..4)
        /// on both rows, BenchLateral from the east leg (2.5 m outside the lane), two granite legs and a seat of BenchSlats slats.</summary>
        public const int BenchPerRow = 5, BenchSlats = 5;
        public const double BenchX0 = 960.0, BenchStep = 40.0, BenchL = 2.4, BenchD = 0.56, BenchH = 0.46, BenchLateral = 14.8;

        // ------------------------------------------------------------------ plaza inlay (INT-12, HT; Shaders/Beats/NocInlay.shader)
        /// <summary>Brass ring lines InlayRingW wide every InlayRingPitch about S from InlayRing0 (outside the podium, R 56) to
        /// InlayRing1 (17 rings; the edge at 400 is the floor's outline); radial seams every 360 / InlaySeams deg (7.5), brass-capped
        /// joints InlaySeamW wide beyond InlaySeamR0 (one runs 0.12 m beside the east leg's centre line: the walk's leading line to
        /// the spire); both families fade out where their pitch falls below InlayFadePx0 px (full above InlayFadePx1).</summary>
        public const double InlayRingPitch = 20.0, InlayRingW = 0.12, InlayRing0 = 60.0, InlayRing1 = 380.0;
        public const int InlaySeams = 48;
        public const double InlaySeamW = 0.06, InlaySeamR0 = 58.0, InlayFadePx0 = 6.0, InlayFadePx1 = 10.0;
        /// <summary>Aged brass F0 (linear; HSV saturation 0.40: under the SignWhite lamps its glints stay below the warm-blob rule's
        /// saturation 0.5), smoothness (worn by feet and wet: 0.70, GGX alpha 0.09, a sharper lobe than the wet film's 0.18
        /// floor), the seams' darker cap (x the brass). HT-04: NocInlay mirrors the wet floor's analytic world with this F0.</summary>
        public static readonly UnityEngine.Color InlayBrass = new UnityEngine.Color(0.50f, 0.42f, 0.30f, 1f);
        public const double InlaySmooth = 0.70, InlaySeamTint = 0.6;
        /// <summary>A low planter (0.45 m with a 0.2 m hedge) closes the walk 1.4 - 2.2 m beyond the goal (behind the PathGuide end
        /// cap); r01's 0.8 m box filled the lower third of a pitch-0 view.</summary>
        public const double EndPlanterX0 = 1149.6, EndPlanterX1 = 1150.4, EndPlanterHalfZ = 4.2, EndPlanterH = 0.45, EndHedgeH = 0.2;

        // ------------------------------------------------------------------ the tenement's downpipe (HT-03)
        /// <summary>A cast-iron rainwater downpipe on the pier between the two south window columns (z 382.56 - 384.24 clear of
        /// sills and lintels): axis DownpipeOff in front of the west face, radius DownpipeR, wall clips every DownpipeClipStep, a
        /// hopper head under the roof edge (top DownpipeTop: below the roof's KS19 silhouette, so no koi sample is hidden) and a
        /// shoe at the foot.</summary>
        public const double DownpipeZ = 383.4, DownpipeOff = 0.09, DownpipeR = 0.05, DownpipeClipStep = 1.8, DownpipeTop = 13.85;   // HT-t04: 14.15 rose 0.32 deg over the roof edge from the KS19 eye
    }
}
