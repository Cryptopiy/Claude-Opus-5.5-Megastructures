// EdgeDims: the numbers and the pure plan geometry of beat 3, The Edge (MASTER_PROMPT 14.4, 14.5.3, 14.12, 14.2.3) and the
// shared geometry agreements of Phase 3 (tongue / boulevard / bridge / garden seams). Pure C# in double, no UnityEngine
// dependency, so the EditMode tests (Beat03EdgeTests) and the other builders (G3's tubes and gate, the visibility audit) read the
// same functions the meshes are built from.
//   Tongue A: x 215.3..375.3, half-width w(x) = 75 to x 225.3, smoothstep to 3.2 over x 225.3-305.3, 3.2 to x 350, smoothstep to
//   2.5 over x 350-375.3 (a bow); deck top y 0, 1 m thick (agreement 1); hull skirt to y -120 at the root.
//   Gulf faces: ruled surfaces x_A(z) = 215.3 - F(|z - 195|), x_B = 815.3 + F (z < 195) or + F2 (z > 195), F(d) = 700
//   smoothstep(300, 1 800, d), F2(d) = 700 smoothstep(805, 2 305, d); z -6 500..6 900, y -3 300..560. Terraces: the face steps
//   back every 300 m of height by 30 + 90 hash(k, 400 m z cell) m (cumulative). First step at y -360 (decision G2-D1 in
//   docs/logs/G2.md: with a step at -60 the lower tube portal (y -142, 14.12 "x 215.3-815.3"), T6's launch from x 215.3 (14.5.4)
//   and the spans of the cross bridges at y -180 and -300 (699 and 1 980 m) would all meet a receded face).
using System;

namespace Megastructure
{
    public static class EdgeDims
    {
        // ------------------------------------------------------------------ tongue A (14.4, 14.5.3, 14.12)
        public const double TongueX0 = 215.3, TongueX1 = 375.3, TongueZ = 195.0;
        public const double TongueW0 = 75.0, TongueW1 = 3.2, TongueW2 = 2.5;
        public const double TongueFunnelX0 = 225.3, TongueFunnelX1 = 305.3, TongueBowX0 = 350.0;
        /// <summary>Deck slab: top y 0 (the H4 mirror plane), 1 m thick (agreement 1), 2 cm chamfer on the walk edge (8.7).</summary>
        public const double DeckThickness = 1.0, DeckBevel = 0.02;
        /// <summary>Deck sampling step along x (m): the w(x) bow and funnel are smooth at 1 m.</summary>
        public const double DeckStep = 1.0;

        /// <summary>Hull skirt (14.5.3 "a hull skirt (ruled surface) to y -120 under the tongue"): keel depth 120 m at the root
        /// falling to 10 m at the tip (the bridge truss below the deck is 10 m deep, 14.5.4); keel half-width 0.4 w(x); the hull
        /// reaches 0.8 m into face A's mass at the root (no crack at the junction).</summary>
        public const double HullDepthRoot = 120.0, HullDepthTip = 10.0, HullKeelFraction = 0.4, HullRootInset = 0.8, HullDepthPower = 1.5;

        /// <summary>Glass balustrade (14.5.3: 1.1 m on both edges; 14.4 "railed cliff edges", so it runs along the whole tongue
        /// edge): centre line 5 cm inside the deck edge, top rail tube 0.06 m (NocLine, top at 1.10), steel shoe 0.09 x 0.11 m,
        /// glass from the shoe into the rail. It ends at x 371.95: x >= 372 above the deck is gate block A (G3, agreement 3).</summary>
        public const double RailHeight = 1.10, RailInset = 0.05, RailTube = 0.06, ShoeWidth = 0.09, ShoeHeight = 0.11, ShoeSink = 0.01;
        public const double GlassBottom = 0.08, GlassTop = 1.075, RailEndX = 371.95, RailSegment = 2.0;

        /// <summary>AB (Phase 4 wave A, D-032 1 / INT-09 / INT-01 "no vertigo"): the glass edge of the pier, x 338..371.95 on both
        /// sides: from lateral LedgeIn (1.75) to the shoe the deck is a walk-on glass ledge (NocGlass pane 3 cm below the top, like the
        /// KS13 strip; PathGuide's floor collider carries the walker) on a steel frame (an edge girder under the shoe, cross beams
        /// every 2.5 m, a nosing on the stone edge) with the slab and the hull pulled in to LedgeIn, so the eye looks through the
        /// floor past the hull into the rift (KS10: the deck 50 % -> about 33 % of the frame, the drop about 46 %; AB_ks10_search).
        /// The deck plan (Acceptance (1), the half-widths) is unchanged.</summary>
        public const double LedgeX0 = 338.0, LedgeX1 = 371.95, LedgeIn = 1.75, LedgeGlassY = -0.03, LedgeNosing = 0.06;
        /// <summary>The ledge's steel, slim (third start: the AB-t02 built KS10 showed the r02 frame (girder 0.30 x 0.42, beams 0.08 x
        /// 0.22) taking 12 % of the frame through the glass, drop 36 %; a ray model of this frame gives 5 %, drop about 43 %): edge
        /// girder under the shoe, cantilever ribs every LedgeBeamPitch, the pane's lip under the girder.</summary>
        public const double LedgeGirderW = 0.14, LedgeGirderD = 0.14, LedgeBeamPitch = 2.5, LedgeBeamW = 0.05, LedgeBeamD = 0.16, LedgeGlassLip = 0.04;
        /// <summary>AB fix wave (C05, p4_01 KS10: the 0.14 m girder plus the 0.11 m shoe drew a 110-170 px dark bar across the drop at
        /// frame centre, built near structure 15.4 %): the edge is frameless. The girder becomes a stainless channel that clamps the
        /// pane's edge under the balustrade shoe, LedgeGirderW wide (the pane's lip and InGlassLedge unchanged) and LedgeChannelD
        /// deep (<= 0.12 m; the pane at -0.03 sits in its slot), and the cantilever ribs become structural glass fins LedgeFinW thick
        /// (<= 0.08 m) under the pane (NocGlass: no opaque bar crosses the drop below the shoe).</summary>
        public const double LedgeChannelD = 0.06, LedgeFinW = 0.04;
        /// <summary>True when (x, lateral |z - 195|) lies over the glass ledge's open area (between the nosing and the girder; 1 cm of
        /// slack under the girder's inner edge, which follows w(x) linearly between the 1 m deck samples).</summary>
        public static bool InGlassLedge(double x, double lateral) =>
            x > LedgeX0 && x < LedgeX1 && lateral > LedgeIn + LedgeNosing && lateral < TongueHalfWidth(x) - LedgeGirderW + 0.01;

        /// <summary>Plaza barrier (14.5.3: planters 1.0 m at the lane edge +-12.7 on x 225-290): inner face at lane + 0.7 (14.4
        /// "collider gap minus 0.70"), from x 214 to x 290: the boulevard median barrier ends at x 213.8 (BoulevardLayout.BarrierX1),
        /// so the first planter (x 214.3-216.7) closes the visible barrier line to 0.5 m (GE, D-029 GB-17: from x 216 it left 2.2 m).</summary>
        public const double BarrierX0 = 214.0, BarrierX1 = 290.0, BarrierGap = 0.7, PlanterWidth = 0.8, PlanterLength = 2.4, PlanterHeight = 1.0, PlanterPitch = 3.0;

        /// <summary>14 sodium lamps 5 m (14.5.3, scale cue 'lamps 5 m'), 7 per side every 10 m, posts 0.9 m outside the planters;
        /// 12 benches 0.45 m (6 per side) outside the barrier, facing the drop.</summary>
        public const int LampCount = 14, BenchCount = 12;
        public const double LampHeight = 5.0, LampX0 = 222.0, LampDx = 10.0, LampLateral = 13.9, LampArm = 0.9;
        public const double BenchX0 = 227.0, BenchDx = 10.0, BenchLateral = 16.0, BenchSeat = 0.45, BenchLength = 1.8, BenchDepth = 0.5;

        /// <summary>Light kit of beat 3 (14.2.3 row 3: at most 24 realtime local lights; 14 balustrade WinCold r 10 i 2.5, tongue
        /// lamps r 14 i 4): 14 balustrade lights on the pier and the 10 sodium lamps at x >= 242 carry a realtime light (14 + 10 = 24);
        /// all 14 lamps and 14 balustrade lights are wet sources (H4).</summary>
        public const int BalustradeLights = 14, RealtimeLampFrom = 4;
        public const double BalustradeLightX0 = 296.0, BalustradeLightDx = 12.0, BalustradeLightY = 0.16;
        /// <summary>AB fix wave (C13, p4_01 b23 s 392.6: the first step-light pair popped to clipped white bloom orbs for 6-8 frames
        /// at a time on the approach): the balustrade light was a point light 0.15 m from the balustrade glass and 6 cm from its own
        /// housing, so the glass's GGX image and the housing's lit chamfer were millimetre-sized spots of extreme radiance that a
        /// far camera's MSAA samples caught or missed as the walker moved. The light is a spot aimed inward and down at
        /// BalSpotPitch below the horizontal (the step light's real throw onto the walk): its outer cone (BalSpotAngle) leaves the
        /// glass, the shoe and the housing behind it unlit (they lie >= 90 deg off its axis), and the floor straight under it sits
        /// in the cone's soft edge.</summary>
        public const double BalSpotPitch = 45.0, BalSpotAngle = 100.0, BalSpotInner = 60.0;
        /// <summary>AB fix wave (C13, AB_c13b): the plaza lamps' realtime lights are spots aimed down (outer LampSpotAngle, inner
        /// LampSpotInner, like the boulevard's lamps): the emissive head 7 cm above the light (>= 100 deg off the axis) is no
        /// longer lit by it, which drew the same sub-pixel pops at s 391-394.</summary>
        public const double LampSpotAngle = 130.0, LampSpotInner = 80.0;
        public const double LampRange = 14.0, LampIntensity = 4.0, BalLightRange = 10.0, BalLightIntensity = 2.5;

        // ------------------------------------------------------------------ Gulf faces (14.4, 14.5.3)
        public const double FaceXA = 215.3, FaceXB = 815.3, FaceZ0 = -6500.0, FaceZ1 = 6900.0, FaceY0 = -3300.0, FaceTop = 560.0;
        public const double FlareD0 = 300.0, FlareD1 = 1800.0, Flare2D0 = 805.0, Flare2D1 = 2305.0, FlareWidth = 700.0;
        /// <summary>Terraces (14.5.3): first step y -360 (decision G2-D1), then every 300 m; step 30 + 90 hash(k, cell) m; cells 400 m in z.</summary>
        public const double TerraceFirstY = -360.0, TerracePitch = 300.0, TerraceStepMin = 30.0, TerraceStepRange = 90.0, TerraceCell = 400.0;
        public const int TerraceLevels = 11;    // level 0 (y -360..560) + 10 receded levels down to y -3 300
        /// <summary>Mass thicknesses (agreement 7: >= 60 m, back face with NocFacade, tops at y 560). Face B is 98.7 m thick so the
        /// garden notch's south wall spans x_B..914 exactly at the neck; north of the notch its back stays inside the screen-wall slab
        /// (x 850, agreement 6: x 834..874 is G4's) to z 700 and steps out to x_B + 98.7 over z 700..1 000 (GB-15, below).</summary>
        public const double ThickA = 60.0, ThickB = 98.7, BackBNorthX = 850.0, BackBBlendZ0 = 700.0, BackBBlendZ1 = 1000.0;
        /// <summary>GE, D-029 GB-15: between z 700 and 1 000 the back steps out in whole setbacks (x 850 -> 871.3 -> 892.7 -> 914 at
        /// z 750 / 850 / 950: three planar facades and three south-facing jog walls) instead of the smooth blend that read as a
        /// windowless twisted surface in KS16 and the beat-6 stills; each back facade is planar, so its u is its own length.</summary>
        public const int BackBSteps = 3;
        public static double BackBStepZ(int k) => BackBBlendZ0 + (BackBBlendZ1 - BackBBlendZ0) * (k + 0.5) / BackBSteps;
        public static double BackBStepX(int k) => BackBNorthX + (XB(BackBBlendZ1) + ThickB - BackBNorthX) * k / BackBSteps;
        /// <summary>Face A above y 0 only outside the canyon z 120..270 (agreement 2).</summary>
        public const double CanyonZ0 = 120.0, CanyonZ1 = 270.0;
        /// <summary>Garden notch (agreement 5): face B above y 0 cut away for z 115..275 between x_B and 914; the notch floor of face
        /// B's mass at y -0.5 lies inside G3's terrace slab; the north wall ends at x 834 where the screen wall (G4) begins, so the
        /// promenade x 874..914 stays open (decision G2-D3).</summary>
        public const double NotchZ0 = 115.0, NotchZ1 = 275.0, NotchX1 = 914.0, NotchNorthX1 = 834.0, NotchFloorY = -0.5;

        // ------------------------------------------------------------------ portals (14.5.4, 14.12; agreement 4)
        /// <summary>Lower tube (y -142, z 209, r 5) portals on both faces; collar r 5.3..6.3 (0.3 m clear of G3's r 5 tube, no coplanar
        /// cylinders), 1.0 m proud of the face, tunnel 30 m deep into the mass, a 16 m square panel around it.</summary>
        public const double LowerTubeY = -142.0, LowerTubeZ = 209.0, TubeRadius = 5.0, PortalInner = 5.3, PortalOuter = 6.3, PortalLip = 1.0, PortalDepth = 30.0, PortalPanelHalf = 8.0;
        public const int PortalSegments = 32;

        // ------------------------------------------------------------------ cross bridges and traffic layers (14.5.3)
        /// <summary>The six cross-gulf bridges (z, y) of 14.5.3 (deck 14 m, a Warren truss 8 m deep in 25 m bays, a light strip on
        /// both deck edges); each spans face to face at its own height (terraces included) and reaches 2 m into both faces.</summary>
        public static readonly double[] BridgeZ = { 850.0, 1380.0, 2300.0, -640.0, -1500.0, -2600.0 };
        public static readonly double[] BridgeY = { -180.0, 60.0, -600.0, 90.0, -300.0, -40.0 };
        public const double BridgeWidth = 14.0, BridgeDeck = 1.5, BridgeTruss = 8.0, BridgeBay = 25.0, BridgeMember = 0.8, BridgeEmbed = 2.0, BridgeLightR = 0.3;
        // The H7 traffic layers live in AbyssTraffic (AB, Phase 4: vehicle rivers instead of the Phase-3 static strokes).

        // ------------------------------------------------------------------ tongue functions
        /// <summary>Tongue half-width w(x) of 14.4 (0 outside x 215.3..375.3 is not defined: clamped).</summary>
        public static double TongueHalfWidth(double x)
        {
            double w = TongueW0 + (TongueW1 - TongueW0) * NocMath.Smoothstep(TongueFunnelX0, TongueFunnelX1, x);
            return w + (TongueW2 - TongueW1) * NocMath.Smoothstep(TongueBowX0, TongueX1, x);
        }

        /// <summary>Path progress s of a point on the tongue's centre line (x = s - 184.70 on the east leg, 14.12).</summary>
        public static double SOfX(double x) => x + NocturneConstants.EastLegOffset;

        /// <summary>Keel depth (positive, m) of the hull skirt at x: 120 at the root, 10 at the tip, (1 - smoothstep)^1.5 in between.</summary>
        public static double HullDepth(double x)
        {
            double t = 1.0 - NocMath.Smoothstep(TongueX0, TongueX1, x);
            return HullDepthTip + (HullDepthRoot - HullDepthTip) * Math.Pow(Math.Max(0.0, t), HullDepthPower);
        }

        public static double KeelHalfWidth(double x) => HullKeelFraction * TongueHalfWidth(Math.Max(x, TongueX0));

        /// <summary>Inner face of the plaza barrier (lateral, m): the lane edge + 0.7 (the lane comes from NocLanes, 14.4).</summary>
        public static double BarrierInner(double x) => NocLanes.LaneHalfWidth(SOfX(x)) + BarrierGap;

        // ------------------------------------------------------------------ Gulf face functions
        public static double F(double d) => FlareWidth * NocMath.Smoothstep(FlareD0, FlareD1, d);
        public static double F2(double d) => FlareWidth * NocMath.Smoothstep(Flare2D0, Flare2D1, d);
        public static double XA(double z) => FaceXA - F(Math.Abs(z - TongueZ));
        public static double XB(double z) => FaceXB + (z < TongueZ ? F(Math.Abs(z - TongueZ)) : F2(Math.Abs(z - TongueZ)));

        /// <summary>dx/dz of the face plan curve (face 0 = A, 1 = B), analytic (smoothstep' = 6 t (1 - t) / W).</summary>
        public static double DXdZ(int face, double z)
        {
            double d = z - TongueZ, ad = Math.Abs(d), sg = d < 0 ? -1.0 : 1.0;
            double d0 = FlareD0, d1 = FlareD1;
            if (face == 1 && d > 0) { d0 = Flare2D0; d1 = Flare2D1; }
            double t = (ad - d0) / (d1 - d0);
            if (t <= 0.0 || t >= 1.0) return 0.0;
            double dF = FlareWidth * 6.0 * t * (1.0 - t) / (d1 - d0);
            return face == 0 ? -dF * sg : dF * sg;
        }

        /// <summary>Plan x of face `face` (0 = A, 1 = B) at z (the unreceded top level).</summary>
        public static double FaceX(int face, double z) => face == 0 ? XA(z) : XB(z);

        /// <summary>+1 for face B (it recedes toward +x), -1 for face A (toward -x).</summary>
        public static double RecedeSign(int face) => face == 0 ? -1.0 : 1.0;

        /// <summary>400 m terrace cell index of z.</summary>
        public static int CellOf(double z) => (int)Math.Floor(z / TerraceCell);

        /// <summary>Terrace level of height y: 0 above the first step (y -360), then 1..10 every 300 m.</summary>
        public static int LevelOf(double y)
        {
            if (y >= TerraceFirstY) return 0;
            int k = (int)Math.Ceiling((TerraceFirstY - y) / TerracePitch - 1e-9);
            return Math.Min(TerraceLevels - 1, Math.Max(1, k));
        }

        /// <summary>Top y of level k (k >= 1: the ledge at the step), bottom y of level k.</summary>
        public static double LevelTop(int k) => k == 0 ? FaceTop : TerraceFirstY - TerracePitch * (k - 1);
        public static double LevelBottom(int k) => k >= TerraceLevels - 1 ? FaceY0 : TerraceFirstY - TerracePitch * k;

        /// <summary>Step of terrace k (>= 1) in cell c of face `face`: 30 + 90 hash(k, cell) m (14.5.3).</summary>
        public static double TerraceStep(int face, int k, int cell)
        {
            uint h = NocHash.H3(unchecked((uint)(k + 4096)), unchecked((uint)(cell + 1048576)), face == 0 ? 0x47554C41u : 0x47554C42u, 0);
            return TerraceStepMin + TerraceStepRange * NocHash.U01(h);
        }

        /// <summary>Cumulative recession X_k (m) of level k in cell c (0 for level 0).</summary>
        public static double Recession(int face, int k, int cell)
        {
            double x = 0.0;
            for (int j = 1; j <= k; j++) x += TerraceStep(face, j, cell);
            return x;
        }

        /// <summary>World x of the front surface of face `face` at height y and z (terraces included): the plane the tubes and the
        /// cross bridges meet.</summary>
        public static double FrontX(int face, double y, double z) => FaceX(face, z) + RecedeSign(face) * Recession(face, LevelOf(y), CellOf(z));

        /// <summary>How far the face profile at (y, z) differs from the plane of the top level (14.5.3 Acceptance (4)).</summary>
        public static double ProfileOffset(int face, double y, double z) => Math.Abs(FrontX(face, y, z) - FaceX(face, z));

        /// <summary>True where the level-0 face exists above y 0 (face A outside the canyon, face B outside the garden notch).</summary>
        public static bool AboveGround(int face, double z) => face == 0 ? (z <= CanyonZ0 || z >= CanyonZ1) : (z <= NotchZ0 || z >= NotchZ1);

        /// <summary>Back face x of the face mass at z (level 0 only; agreement 7).</summary>
        public static double BackX(int face, double z)
        {
            if (face == 0) return XA(z) - ThickA;
            double far = XB(z) + ThickB;
            if (z <= NotchZ1) return far;
            if (z <= BackBBlendZ0) return BackBNorthX;
            if (z >= BackBBlendZ1) return far;
            int k = 0;
            while (k < BackBSteps && z >= BackBStepZ(k)) k++;
            return BackBStepX(k);
        }

        /// <summary>Arc length along the plan curve of face `face` from z 195 to z (signed, m; Simpson with 2 m steps): the facade u.</summary>
        public static double ArcLength(int face, double z)
        {
            double a = TongueZ, b = z;
            int n = Math.Max(2, (int)Math.Ceiling(Math.Abs(b - a) / 2.0));
            if ((n & 1) == 1) n++;
            double h = (b - a) / n, sum = 0.0;
            for (int i = 0; i <= n; i++)
            {
                double zz = a + h * i, d = DXdZ(face, zz), f = Math.Sqrt(1.0 + d * d);
                sum += (i == 0 || i == n) ? f : ((i & 1) == 1 ? 4.0 * f : 2.0 * f);
            }
            return sum * h / 3.0;
        }

        /// <summary>Span (m) of cross bridge i between the two faces at its height (terraces included), without the embeds.</summary>
        public static double BridgeSpan(int i) => FrontX(1, BridgeY[i], BridgeZ[i]) - FrontX(0, BridgeY[i], BridgeZ[i]);
    }
}
