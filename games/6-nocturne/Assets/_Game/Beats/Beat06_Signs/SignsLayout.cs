// SignsLayout: every dimension of beat 6, the Wall of signs (MASTER_PROMPT 14.4 hero volumes, 14.5.6 geometry, 14.2.3 row 6),
// as named constants shared by the builders and Beat06SignsTests. Units metres; x east, z north, y up; s = path progress.
// The walk: C2 arc (centre (871.74, 225), r 30) to s 1 103.56, the straight leg x 901.74 (z 225 - 357.88, s 1 103.56 - 1 236.44),
// then C3 (centre (931.74, 357.88), r 30) to the beat-7 boundary at s 1 260. Lane half-width 10 on the promenade (14.4; NocLanes),
// so the lane is x 891.74 - 911.74 on the straight leg; the PathGuide wall faces sit 0.38 m outside it (D-021).
// Floors: G3's garden terrace ends at z 275 (geometry agreement 6); this beat floors the promenade x 874 - 914 from z 275 north.
namespace Megastructure
{
    public static class SignsLayout
    {
        // ------------------------------------------------------------------ screen wall slab (14.4, 14.5.6 Acceptance (1))
        /// <summary>Slab x 834 - 874 (40.0 m), z 275 - 520 (245 m), y -1 500..560; the screen face is x 874.00.</summary>
        public const double WallX0 = 834.0, WallX1 = 874.0, WallZ0 = 275.0, WallZ1 = 520.0, WallY0 = -1500.0, WallY1 = 560.0;
        /// <summary>Panels 36 x 20 m with 0.4 m seams (14.5.6): pitch 36.4 x 20.4, seam centres at z 275 + 36.4 k and y 20.4 k
        /// (the promenade floor y 0 meets a seam centre, so every visible panel starts at the floor).</summary>
        public const double PanelW = 36.0, PanelH = 20.0, Seam = 0.4, PanelPitchZ = PanelW + Seam, PanelPitchY = PanelH + Seam;
        /// <summary>Wall emission = ramp x 0.45 x panel level, level 0.55 + breath (14.5.6; D-010 breath +-0.05 at 0.05-0.10 Hz).</summary>
        public const double EmitGain = 0.45, PanelLevel = 0.55, BreathAmp = 0.05, BreathHz0 = 0.05, BreathHz1 = 0.10;
        /// <summary>Seam darkening of the v1 face (kept as a material property; the LED facade's gap scatters 10 %).</summary>
        public const double SeamDark = 0.85;
        /// <summary>Screen surface smoothness (0.55 in r01 gave sharp highlights of the wall lights).</summary>
        public const double ScreenSmooth = 0.35;
        /// <summary>Waves of 14.5.6 in use: 1 in the Simplified tier (phi1 only), 3 in Full.</summary>
        public const int WavesSimplified = 1, WavesFull = 3;
        /// <summary>Imagery strength (NocScreen _Content): Full 1, Simplified 0 (the waves only, 14.5.6 Fallbacks).</summary>
        public static int Waves(int tier) => tier == 0 ? WavesFull : WavesSimplified;
        public static double Content(int tier) => tier == 0 ? 1.0 : 0.0;

        // ------------------------------------------------------------------ the nine glyph signs (14.5.6: 60-120 m at y +20..+420)
        /// <summary>A giant glyph sign on the face: a blade (perpendicular to the face, projecting `w` m east from x 874.5, NocNeon
        /// faces on both sides) or a board (flat on the face, 2.4 m proud, NocNeon face toward +x, `w` m along z). Palette: the
        /// NocNeon slot (1 NeonCyan, 2 SodiumAmber x k_a, 4 SignWhite, 5 WinNeutral).</summary>
        public struct GlyphSign
        {
            public bool blade;
            public double z, y0, h, w;
            public int palette;
            public GlyphSign(bool blade, double z, double y0, double h, double w, int palette)
            { this.blade = blade; this.z = z; this.y0 = y0; this.h = h; this.w = w; this.palette = palette; }
        }

        /// <summary>The nine signs, south to north (heights 70-120 m, y 22-420). Placed off the sight lines of KS16's three wall
        /// points (Beat06SignsTests) and off the lane (blades end at x 890.5, the lane edge is x 891.74). Fix wave (C16, panel
        /// p4_01: the wall covered 43 % of KS16 behind two hanging signs): the blades of z 292 and z 366 hang above KS16's frame
        /// (y 200-290 and 230-330, into the deck mist), the z 438, 490 and 505 blades became boards, and every board is mounted flush
        /// on the face (BoardX 874.35: the screen field reads to its edge; the audit counts a flush board as wall). GE's promenade
        /// lintel (z 280-286, y 40+) takes 2.9 % of KS16 in front of the wall. The boards z 405 / 438 sit above the eye's canvas
        /// (z 311.4 - 457.0, y 20.4 - 122.4; SG_f02: the z 405 board covered the iris).</summary>
        public static readonly GlyphSign[] GlyphSigns =
        {
            new GlyphSign(true, 292.0, 200.0, 90.0, 14.0, 1),
            new GlyphSign(false, 298.0, 330.0, 90.0, 16.0, 4),
            new GlyphSign(false, 329.0, 175.0, 120.0, 22.0, 4),
            new GlyphSign(true, 366.0, 230.0, 100.0, 16.0, 4),
            new GlyphSign(false, 405.0, 150.0, 80.0, 20.0, 5),
            new GlyphSign(false, 438.0, 126.0, 120.0, 16.0, 1),
            new GlyphSign(false, 469.0, 300.0, 80.0, 18.0, 4),
            new GlyphSign(false, 490.0, 30.0, 70.0, 14.0, 2),
            new GlyphSign(false, 505.0, 200.0, 120.0, 15.0, 4),
        };
        /// <summary>Blade housing: from x 873.9 (0.1 m into the slab) to 874.5 + w + 0.3, 0.9 m thick, 0.8 m above and below the sign
        /// faces; the NocNeon faces 0.02 m outside the housing. Board housing: x 873.9 - 874.35 (flush, C16; 876.4 before), 0.6 m
        /// around the face.</summary>
        public const double BladeX0 = 874.5, BladeT = 0.9, BladeRim = 0.8, BoardX = 874.35, BoardRim = 0.6, NeonProud = 0.02;

        // ------------------------------------------------------------------ scale markers on the face (fix wave C17)
        /// <summary>Two service ledges on the row seams y 81.6 and 204.0 (20.4 x 4 and x 10): painted steel x 874.0 - 874.45 (flush:
        /// inside the audit's 0.5 m), 0.3 m thick, z 275.3 - 519.7, with a lamp row under the lip: 0.3 x 0.2 m lenses (SignWhite x
        /// 1.6, NocHalos) every 6 m from z 278. A lamp every 6 m and the 1.2 m-scale ledge give a 36 m panel its size.</summary>
        public static readonly double[] LedgeY = { 81.6, 204.0 };
        public const double LedgeX1 = 874.45, LedgeT = 0.30, LedgeLampPitch = 6.0, LedgeLampZ0 = 278.0, LedgeLampGain = 1.6;
        /// <summary>The lens itself glows at 0.35 (the halo carries the 1.6): at 90-200 m a 0.3 m lens is under a pixel, and at 1.6 it
        /// read as isolated hot pixels (SG_f04 image_metrics: 10 bright NaN-like pixels on the ledge lines) that twinkle in motion.</summary>
        public const double LedgeLensGain = 0.35;
        /// <summary>Four window-cleaning cradles (lit cabins 3.0 x 1.0 x 1.5 m: 1/12 of a panel width) at x 874.5 - 875.5, each
        /// riding two fixed cables (NocLine, 0.04 m) from the wall top y 560 down to 2 m below its lowest position. Height
        /// y(t) = Yc + A sin(2 pi (t600 / 600 + phase)): a pure function of the 600 s clock (exact wrap), speed <= 2 pi A / 600
        /// = 0.188 m/s at A 18 (C17: <= 0.2 m/s). The ranges stay clear of the ledges (cabin 1.5 m + hoists 0.5 m).</summary>
        public static readonly double[] CradleZ = { 296.0, 352.0, 412.0, 470.0 };
        public static readonly double[] CradleYc = { 46.0, 106.0, 160.0, 52.0 };
        public static readonly double[] CradleA = { 12.0, 16.0, 18.0, 13.0 };
        public static readonly double[] CradlePhase = { 0.10, 0.62, 0.35, 0.81 };
        public const double CradleW = 3.0, CradleD = 1.0, CradleH = 1.5, CradleX0 = 874.5, CableR = 0.02, CradleGain = 0.9;
        public static double CradleY(int i, double t600) => CradleYc[i] + CradleA[i] * System.Math.Sin(2.0 * System.Math.PI * (t600 / 600.0 + CradlePhase[i]));


        // ------------------------------------------------------------------ wall lights (14.2.3 row 6: 12 wall lights, wave colour, r 45, i 5)
        public const int WallLightCount = 12;
        /// <summary>The 12 lights hang 6 m in front of the face at y 14 (at 2 m their specular highlights read as dots on the screen, r01), every 20 m from z 285 (they stand in for the wall's glow).</summary>
        public const double WallLightX = 880.0, WallLightY = 14.0, WallLightZ0 = 285.0, WallLightStep = 20.0;
        public const double WallLightRange = 45.0, WallLightIntensity = 5.0;

        // ------------------------------------------------------------------ promenade (14.4, 14.5.6)
        /// <summary>Promenade floor x 874 - 914 from the terrace end z 275 to the wall end z 520; 1 m thick (agreement 1). The slab
        /// reaches 1 m into the screen-wall mass (its top never meets the screen face edge-on); its east face x 914 is back to back
        /// with the arcade's plinth (z 275 - 360) and the heart floor's west side (z 360 - 520), never facing the same way.</summary>
        public const double PromX0 = 874.0, PromX1 = 914.0, PromZ0 = 275.0, PromZ1 = 520.0;
        public const double PromSlabX0 = 873.0, PromSlabX1 = 914.0, FloorThickness = 1.0;
        /// <summary>Straight-leg lane edges (x 901.74 +- 10) and the walk centre x.</summary>
        public const double LaneCentreX = 901.74, LaneHalf = 10.0;
        /// <summary>Planter row (the visible barrier of the lane x 891.7 - 911.7): x 912.2 - 913.6 (behind the invisible wall face
        /// 912.12), 0.75 m tall with a 0.3 m hedge, 5.4 m long every 6 m from z 276 (0.6 m gaps at z 276 + 6 k) to z 354.</summary>
        public const double PlanterX0 = 912.2, PlanterX1 = 913.6, PlanterH = 0.75, HedgeH = 0.30, HedgeInset = 0.10;
        public const double PlanterGap = 0.6, PlanterPitch = 6.0, PlanterGapZ0 = 276.0, PlanterGapZ1 = 354.0;
        /// <summary>Planter coping: a 6 cm granite lip 4 cm proud all round the planter top.</summary>
        public const double CopingH = 0.06, CopingProud = 0.04;
        /// <summary>Lamp posts 6 m (SignWhite heads) at x 912.9 in planter gaps; 14.5.6's x 911 is 0.74 m inside the lane (log).</summary>
        public const double LampX = 912.9, LampH = 6.0, LampPostWidth = 0.16, LampHeadW = 0.6, LampHeadH = 0.25;
        public static readonly double[] LampZ = { 282, 300, 318, 336, 354, 408, 426, 444, 462, 480, 498, 516 };
        /// <summary>The fixture (SG): base plate 0.36 m, a post tapering 0.16 -> 0.11 m, an arm 0.55 m toward the walk at y 5.9 and a
        /// luminaire 0.62 x 0.11 x 0.30 m whose lens (0.5 x 0.22 m) faces down; the light hangs 0.12 m under the lens.</summary>
        public const double LampBase = 0.36, LampBaseH = 0.12, LampTop = 0.11, LampArm = 0.55, LampArmY = 5.9;
        public const double LumW = 0.62, LumH = 0.11, LumD = 0.30, LensW = 0.50, LensD = 0.22;
        /// <summary>14.2.3 row 6: promenade lamps SignWhite r 12 i 3; head emission SignWhite x 2 (H4 source, L_eff &lt;= 3).</summary>
        public const double LampRange = 12.0, LampIntensity = 3.0, LampHeadGain = 2.0, LampSourceRho = 0.3;
        /// <summary>Bollards 0.9 m every 6 m on the outer (west / north) lane edge, centred 0.65 m outside the lane edge (the wall
        /// face is at +0.38, the bollard half-width 0.11); round steel, a domed cap, every third one with a dim cool light ring.</summary>
        public const double BollardH = 0.9, BollardW = 0.22, BollardStep = 6.0, BollardOffset = 0.65;
        public const double BollardRingY0 = 0.70, BollardRingY1 = 0.76, BollardRingGain = 0.9;
        public const int BollardRingEvery = 3;
        public const double Bevel = 0.02;

        // ------------------------------------------------------------------ arcade (14.4: x 914 - 990, z 236 - 360, 38 m high)
        public const double ArcX0 = 914.0, ArcX1 = 990.0, ArcZ0 = 236.0, ArcZ1 = 360.0, ArcH = 38.0;
        /// <summary>The plinth reaches down to y -40 (it stands on the promenade and the plaza disc; no floor edge is co-planar).</summary>
        public const double ArcY0 = -40.0;
        /// <summary>Ground floor: a colonnade on the west (promenade), north (plaza approach) and east (plaza) faces; the shop glass
        /// line stands 4 m behind the face line under the soffit of the upper floors (y 6.6), columns 0.8 m square centred 0.4 m
        /// inside the face line, one bay per column pitch (14 bays west and east, 9 north).</summary>
        public const double ArcRecess = 4.0, ArcSoffit = 6.6, ArcCol = 0.8, ArcColInset = 0.4;
        public const int BaysWest = 14, BaysNorth = 9, BaysEast = 14;
        /// <summary>Shop glass y 0.3 - 4.4 between piers 0.7 m wide, steel frames 0.15 m, a centre mullion; the sign band of each bay
        /// is a small NocNeon blade (0.6 x 1.9 m) hung from the soffit at the colonnade line.</summary>
        public const double ShopY0 = 0.3, ShopY1 = 4.4, ShopPier = 0.7, ShopFrame = 0.15, ShopSignW = 0.6, ShopSignH = 1.9, ShopSignY1 = 6.45;
        /// <summary>Fascia band (steel) around the soffit edge y 6.6 - 7.4, 0.12 m proud; soffit downlights every 4.5 m, 1.6 m in
        /// from the face line (0.3 m square lenses, WinCold x 1.2).</summary>
        public const double FasciaY1 = 7.4, FasciaProud = 0.12, DownlightPitch = 4.5, DownlightIn = 1.6, DownlightSize = 0.3, DownlightGain = 1.2;
        /// <summary>The back side (seen from the Heart): two service catwalks (grating 1.2 m deep, rails 1.1 m, posts every 2 m) on the
        /// north and east faces, maintenance lights under them every 9 m (SignWhite x 1.4, NocHalos), cable risers (0.6 x 0.3 m
        /// trays) on the west, north and east faces. Everything below the 38 m roof line (KS17's koi composition lists the roof).</summary>
        public static readonly double[] CatwalkY = { 14.6, 26.6 };
        public const double CatwalkD = 1.2, RailH = 1.1, RailPost = 2.0, MaintPitch = 9.0, MaintGain = 1.4;
        public static readonly double[] RiserWestZ = { 262.0, 318.0 }, RiserNorthX = { 931.0, 965.0 }, RiserEastZ = { 292.0, 340.0 };
        public const double RiserW = 0.6, RiserD = 0.3;
        /// <summary>Shop room light (x token, NocShopfront room gain 0.6 and the panel state's gain 0.16-0.85 on top: G4Kit.PickShop):
        /// cool rooms visible from the Heart (LOOK_V2 R6: no warm light in beat 7), a few neutral ones on the promenade face; closed
        /// bays are roller shutters. ShopClosed: the dark-room level of the -relief-off fallback.</summary>
        public const double ShopLight = 0.26, ShopClosed = 0.08;   // 0.34 in r03: white boxes

        // ------------------------------------------------------------------ s ranges (wet walls and sources, events)
        /// <summary>The straight leg starts at s 1 103.56 (C2 tangent out); z = s - 878.56 on it.</summary>
        public const double StraightS0 = 1103.56, StraightS1 = 1236.44, StraightZ0 = 225.0;
        public static double StraightZ(double s) => StraightZ0 + (s - StraightS0);
        public static double StraightS(double z) => StraightS0 + (z - StraightZ0);
    }
}
