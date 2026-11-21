// BulwarkConst: the game table of MASTER_PROMPT Part 14 - every constant of 14.12, the key positions of 14.4 and the
// geometry numbers of 14.5 (plus the per-beat look tables of 14.2.3-14.2.5 and the key shots of 14.10.2).
// Pure C#, no UnityEngine, so EditMode tests, the offline audio renderer and Tools/gen_hlsl.py can read it.
// Rules: every constant is a plain literal (Tools/gen_hlsl.py turns each double, float or int constant into a
// TC_ define); values that feed path maths are double; colours live in BulwarkPalette; the path functions live in BwPath.
// Conventions (14.1, 14.4): Unity left-handed, metres, seconds, degrees; Y up; +Z north (walk direction), +X east
// (sea); yaw 0 = +Z, 90 = +X; s = horizontal distance along the filleted path; lateral = signed distance to the right
// of the path heading. Beats are 1-based in names (B1..B7), 0-based in arrays (index 0 = beat 1).
namespace Megastructure
{
    public static class BulwarkConst
    {
        // ================================================================ player (14.12 row 1, 14.1, Part 9)
        public const double Eye = 1.7;                 // EYE: eye height above the walking surface (m)
        public const double Walk = 1.5;                // WALK (m/s)
        public const double Brisk = 2.5;               // BRISK = Shift (m/s)
        // CINE: cinematic progress speed (m/s), constant, no holds. ONE source: the cinematic walks at the Shift speed (Part 9,
        // 3.1 a), so Cine is an alias of Brisk, not a literal of its own (static readonly: gen_hlsl.py and ShaderGen export only
        // literal consts, so there is no TC_Cine; FrameworkConst.CinematicSpeed takes Brisk directly).
        public static readonly double Cine = Brisk;
        public const double Accel = 0.2;               // ACCEL: critically damped manual acceleration (s)
        public const double PlayerR = 0.35;            // PLAYER_R: capsule radius (m)
        public const double StartRamp = 0.5;           // smootherstep start ramp of walker and test walks (s); loses 0.25 s
        public const double GroundSpeedPeak = 2.597;   // m/s, on the lateral keys at s 141 (progress speed stays 2.5)
        public const double GroundSpeedPeakS = 141.0;
        public const double LateralSpeedPeak = 0.70;   // m/s at 2.5 m/s
        public const double HeadBob = 0.008;           // m, no roll (14.1)
        public const double CcSlopeLimit = 45.0;       // CharacterController (14.4)
        public const double CcStepOffset = 0.3;
        public const double CameraProbe = 0.2;         // camera probe sphere radius (m)
        public const double StrideWalk = 0.75;         // footstep stride at 1.5 m/s (14.3)
        public const double StrideBrisk = 0.85;        // at 2.5 m/s
        public const double StrideStairs = 0.9;        // on the stairs at any speed

        // ================================================================ path, beats, timeline (14.12 row 2, 14.1, 14.4)
        public const double PathLength = 1500.0;       // PATH_LENGTH (filleted plan distance, exact)
        public const double GoalTriggerS = 1498.5;     // lantern trigger (14.11)
        public const double GoalRestS = 1500.0;        // the walker comes to rest 1.9 m short of the lantern
        public const int BeatCount = 7;
        public static readonly double[] BeatS = { 0.0, 120.0, 420.0, 700.0, 920.0, 1070.0, 1370.0, 1500.0 };   // BEAT_S
        public static readonly double[] BeatLength = { 120.0, 300.0, 280.0, 220.0, 150.0, 300.0, 130.0 };
        public static readonly string[] BeatName = { "Wave gallery", "Crest", "Spillway bridge", "Armour slope", "Gate tower and descent", "Window gallery", "Sluice tunnel and lee ledge" };
        public const double TimeAtWalk = 1000.0;       // s, PATH_LENGTH / WALK
        public const double TimeAtCine = 600.0;        // s, PATH_LENGTH / CINE
        public static readonly double[] CumTimeAtWalk = { 80.0, 280.0, 466.666667, 613.333333, 713.333333, 913.333333, 1000.0 };
        public static readonly double[] CumTimeAtCine = { 48.0, 168.0, 280.0, 368.0, 428.0, 548.0, 600.0 };
        public const double TransitionWindow = 30.0;   // Rule P crossfade width centred on a boundary (14.6)
        public const double HysteresisForward = 20.0;  // tier/activation hysteresis (14.6)
        public const double HysteresisBack = 30.0;

        // ================================================================ path geometry (14.4 table, 14.12 row 3)
        // The filleted polyline is built by BwPath from these numbers: start, leg lengths, headings and arc radii.
        public const double StartX = -1.5;             // origin O: floor at the south end of the gallery, centre line x -1.5
        public const double StartZ = 0.0;
        public const double L1EndS = 120.0;            // L1 gallery, door plane at s 120
        public const double L2EndS = 934.0;            // L2 beats 2-4 to the tower atrium (heading 0)
        public const double A1Radius = 40.0;           // A1 r 40, heading 0 -> 3.0
        public const double A1HeadingDeg = 3.0;
        public const double StairsHeadingDeg = 3.0;
        public const double StairsLength = 97.8;       // 15 x 5.4 + 14 x 1.2
        public const double A2Radius = 40.0;           // A2 r 40, heading 3.0 -> 0.86
        public const double HallHeadingDeg = 0.86;     // exit hall + window gallery
        public const double GalleryEndS = 1370.0;      // the hall/gallery leg ends exactly at the beat 7 boundary
        public const double A3Radius = 30.0;           // A3 r 30, heading 0.86 -> -18
        public const double TunnelHeadingDeg = -18.0;
        // 14.4 lists 50.163 (rounded). Exactly 50.163 puts the tunnel end at z 1427.25502, 1.02 mm off the table's
        // 1427.254; 50.1626 keeps every listed end point within 0.65 mm and still rounds to 50.163 / ledge 63.679.
        public const double TunnelLength = 50.1626;
        public const double A4Radius = 20.0;           // A4 r 20, heading -18 -> 0
        public const double LedgeHeadingDeg = 0.0;
        // Table values (3 decimals) the tests check the built path against (T-PATH, 1e-3).
        public const double A1EndS = 936.094, StairsEndS = 1033.894, A2EndS = 1035.388, A3EndS = 1379.875, TunnelEndS = 1430.038, A4EndS = 1436.321;
        public const double A1ArcLength = 2.094, A2ArcLength = 1.494, A3ArcLength = 9.875, A4ArcLength = 6.283;
        public const double HallLength = 334.612, LedgeLength = 63.679;
        public const double L2EndX = -1.5, L2EndZ = 934.0;
        public const double A1EndX = -1.445, A1EndZ = 936.093;
        public const double StairsEndX = 3.673, StairsEndZ = 1033.759;
        public const double A2EndX = 3.724, A2EndZ = 1035.252;
        public const double GalleryEndX = 8.746, GalleryEndZ = 1369.826;
        public const double A3EndX = 7.281, A3EndZ = 1379.547;
        public const double TunnelEndX = -8.220, TunnelEndZ = 1427.254;
        public const double A4EndX = -9.199, A4EndZ = 1433.435;
        public const double EndX = -9.199, EndZ = 1497.114;
        public const double LeeFaceCrossS = 1422.85;   // the tunnel centre line crosses the lee face x -6.0 here
        public const double LeeFaceCrossZ = 1420.4;

        // ================================================================ floor profile and eye (14.4, 14.12 tower/stairs)
        public const int StairFlights = 15;
        public const int StairStepsPerFlight = 18;
        public const int StairSteps = 270;
        public const double StairRiser = 0.2;
        public const double StairTread = 0.3;
        public const double StairLanding = 1.2;
        public const double StairFlightRun = 5.4;      // 18 x 0.3
        public const double StairFlightPitch = 6.6;    // flight k starts at s = stairs start + 6.6 k
        public const double StairFlightDrop = 3.6;
        public const double StairDrop = 54.0;
        // The 18 risers of a flight are centred on its 5.4 m run (first riser 0.15 m after the flight start), so the
        // mean of the steps is the pitch line from (start, top) to (start + 5.4, top - 3.6): KS14's eye y -25.3 at s 985.
        public const double StairRiserPhase = 0.15;
        public const double StairWidth = 3.6;
        public const double StairHeadroom = 2.6;
        public const double StairHeadroomMin = 2.4;    // acceptance
        public const double StairHandrailHeight = 1.0;
        public const double StairHandrailDiameter = 0.05;
        public const double StairSoffitClearance = 0.62;   // eye to soffit
        public const double EyeKernelHalfWidth = 2.5;  // "y kernel 2.5 m": triangular kernel, half-width (support 5 m)
        public const double EyeAboveTreadLineMax = 0.28;
        public const double GalleryStartS = 1070.0;    // beat 6 start: the ramp starts here
        public const double GalleryRamp = 0.12;        // floor y = -54 - 0.12 (s - 1070) to -90 at s 1370
        public const double FloorY1 = 0.0;             // road / gallery / atrium
        public const double FloorYHall = -54.0;        // stairs bottom, exit hall
        public const double FloorYTunnel = -90.0;      // gallery end, tunnel, ledge
        public const double SeaY = -8.0;               // mean sea level
        public const double EyeDepthAt1070 = 44.3;     // eye depth below the sea
        public const double EyeDepthAt1370 = 80.3;

        // ================================================================ lanes (14.4 "Lanes": collider gap minus 0.70)
        // World-x lanes are given on legs heading north (lateral = x - path x). BwPath.Lane turns them into
        // capsule-centre lateral limits and blends every change (edge slope at most LaneMaxSlope).
        public const double LaneCorridorXMin = -3.15, LaneCorridorXMax = 0.15;   // beat 1 corridor and door opening
        public const double LaneRoadXMin = -5.4, LaneRoadXMax = 5.1;             // crest road (beats 2-4)
        public const double LaneFunnelS0 = 898.0, LaneFunnelS1 = 919.65;         // funnel to the portal (z 920 - PLAYER_R)
        public const double LanePortalWidth = 2.9;     // portal 3.6 - 0.7 (x -2.95..-0.05; see BwPath.Lane)
        public const double LaneStairsWidth = 2.9;
        public const double LaneExitHallWidth = 3.7;
        public const double LaneGalleryLeft = -5.0;    // axis -5.0 .. axis +2.65
        public const double LaneGalleryRight = 2.65;
        public const double LaneTunnelWidth = 2.5;
        public const double LaneLedgeXMin = -11.8, LaneLedgeXMax = -6.4;
        public const double LaneMaxSlope = 0.6;        // max |d edge / ds| of any lane edge (no clamp jumps either way)
        public const double LaneKeyMargin = 0.15;      // lateral keys stay this far inside the lane
        // Lane transition windows (s). Derivations in BwPath.Lane.
        public const double LaneDoorClearS = 121.05;   // door wall north face z 120.7 + PLAYER_R
        public const double LaneDoorOpenS = 137.05;    // corridor -> road widening (16 m smootherstep)
        public const double LaneHallOpenS0 = 1035.74;  // stairwell end (A2 end 1035.388) + PLAYER_R
        public const double LaneHallOpenS1 = 1037.74;
        public const double LaneGalleryOpenS0 = 1070.95;   // gallery doorway plane 1070 + assumed wall 0.6 + PLAYER_R
        public const double LaneGalleryOpenS1 = 1082.95;
        public const double ExitDoorWall = 0.6;        // assumed thickness of the 4.4 x 4.0 doorway wall at s 1070
        public const double LaneTunnelFunnelS0 = 1355.0;   // gallery -> tunnel door funnel
        public const double LaneTunnelFunnelS1 = 1372.65;  // hall end wall s 1373 - PLAYER_R
        public const double LaneLedgeOpenS0 = 1428.0;  // tunnel band -> ledge lane (ends at the A4 end)

        // ================================================================ lateral keys (14.12, 14.5.2-14.5.4)
        // (s, world x) on L2 (centre x -1.5, heading 0): lateral = x + 1.5; smootherstep between keys; 0 elsewhere.
        public static readonly double[] LateralKeyS = { 122.0, 160.0, 166.0, 226.0, 242.0, 282.0, 420.0, 466.0, 502.0, 544.0, 612.0, 660.0, 700.0, 740.0, 800.0, 850.0 };
        public static readonly double[] LateralKeyX = { -1.5, 4.2, 4.2, -4.5, -4.5, -1.5, -1.5, 1.0, 1.0, -4.0, -4.0, -1.5, -1.5, 3.0, 3.0, -1.5 };

        // ================================================================ dam (14.12 row 4, 14.4)
        public const double RoadY = 0.0;
        public const double RoadXMin = -6.0, RoadXMax = 6.0;   // road deck 12 m
        public const double RoadMeshZMin = -300.0, RoadMeshZMax = 1800.0;
        public const double LeeFaceX = -6.0;           // lee face plane, sheer to the lowland
        public const double LowlandY = -6000.0;
        public const double LeeRailX = -6.0;           // lee rail axis
        public const double ParapetXMin = 5.5, ParapetXMax = 6.0, ParapetTop = 1.1, ParapetThickness = 0.5;
        public const double SeaFaceX = 6.0;            // sea face x 6.0 down to y -20, then batter 1:8
        public const double SeaFaceBatterY = -20.0;
        public const double SeaFaceBatter = 8.0;       // x = 6 + (-20 - y) / 8
        public const double GlassAxisOffset = 6.0;     // the glass stays vertical at axis + 6.0
        public const double GlassRevealDepth = 3.5;
        public const double GateTowerZ0 = 920.0;       // towers at z 920 + 2400 k (k 0 walkable)
        public const double GateTowerSpacing = 2400.0;
        public const double FixedGeometryZRange = 1800.0;   // geometry within 1800 m of O in Z, 120 m in Y (Part 7.5 deviation)
        public const double FixedGeometryYRange = 120.0;
        public const double EarthRadius = 6371000.0;   // lowland sphere
        public const double SeaSphereOffset = 5992.0;  // sea sphere radius R + 5992
        public const double CrestDipDeg = 2.486;       // horizon dip from the crest eye
        public const double LedgeDipDeg = 2.467;       // from the ledge eye

        // ================================================================ beat 1: wave gallery (14.12 row 5, 14.4, 14.5.1)
        public const double GalleryClearWidth = 4.0, GalleryClearHeight = 3.6, GalleryLength = 120.0;
        public const double GalleryCornerRadius = 0.3;
        public const double GalleryWall = 0.6;
        public const double GalleryWallXMin = -3.5, GalleryWallXMax = 0.5;   // inner faces
        public const double ShellXMin = -4.1, ShellXMax = 1.1, ShellYMax = 4.2, ShellZMin = -0.6, ShellZMax = 120.7;
        public const double RoadCutZMin = -0.6, RoadCutZMax = 121.0;
        public const double RibWidth = 0.4, RibProud = 0.15, RibZ0 = 3.0, RibPitch = 6.0;
        public const double PropMaxProtrusion = 0.25;
        public const double GalleryLampPitch = 12.0;   // caged lamps at z 12 k, k 1-9
        public const int GalleryLampCount = 9;
        public const double GalleryLampW = 0.45, GalleryLampH = 0.2;
        public const double SlitX = 0.5, SlitWidth = 0.3, SlitY0 = 1.0, SlitY1 = 2.8, SlitZ0 = 6.0, SlitPitch = 12.0, SlitDepth = 1.2;
        public const int SlitCount = 10;
        public const double ValveWheelDiameter = 0.5, ValveWheelZ1 = 30.0, ValveWheelZ2 = 78.0;
        public const double NicheW = 1.0, NicheH = 2.1, NicheD = 0.8, NichePitch = 24.0;
        public const double OchreBandY = 1.1;
        public const double DoorPlaneZ = 120.0;        // closed door face; the walker meets it at s 119.65
        public const double DoorWallNorthZ = 120.7;    // the shell end wall / leaf pockets end here
        public const double DoorOpeningXMin = -3.5, DoorOpeningXMax = 0.5, DoorOpeningHeight = 3.4, DoorWidth = 4.0;
        public const double DoorLeafW = 2.0, DoorLeafH = 3.5, DoorLeafT = 0.7;
        public const double DoorHandWheel = 0.6;
        public const double DoorTriggerS = 104.0;      // gap(t) = 4.0 smootherstep(t / 3.0)
        public const double DoorOpenTime = 3.0;
        public const double DoorLeafCollidersOffGap = 0.5;
        public const double DoorGapCheckTime = 4.0, DoorGapCheckMin = 1.3;
        public const double GalleryDustMotesPerLamp = 300.0;
        public const int GalleryDustMotesMax = 3000;
        public const double LampSwayDeg = 1.2, LampSwayHz = 0.8, LampSwayDecay = 3.0;

        // ================================================================ beat 2: crest (14.12 row 5, 14.5.2)
        public const double LampPostHeight = 9.0, LampPostZ0 = 150.0, LampPostPitch = 50.0;
        public static readonly int[] LampPostK = { 0, 1, 2, 3, 4, 5, 12, 13, 14, 15 };   // none on the spillway
        public static readonly double[] LampPostZBeat4 = { 750.0, 800.0, 850.0, 900.0 };
        public const double LampPostLeeX = -5.6, LampPostSeaX = 5.75;   // even k lee, odd k sea side (on the parapet)
        public const double BollardHeight = 0.9, BollardPitch = 20.0;
        public const double CartX = 2.0, CartY = 0.0, CartZ = 330.0, CartL = 2.8, CartW = 1.3, CartH = 1.9;
        public const double LeeRailPostSize = 0.12, LeeRailPostPitch = 2.0, LeeRailDiameter = 0.10, LeeRailTop = 1.1, LeeRailMid = 0.55;
        public const double LaneDigitW = 0.7, LaneDigitH = 1.2, LaneDigitStroke = 0.15;
        public const double LaneDigitZ0 = 100.0, LaneDigitZ1 = 900.0, LaneDigitPitch = 100.0;
        public const double LaneDigitLeeX = -3.0, LaneDigitSeaX = 3.0, LaneDigitSeaOffset = 50.0;
        public const double FarTowerW = 36.0, FarTowerD = 12.0, FarTowerH = 80.0;
        public static readonly double[] FarTowerZ = { -1480.0, 3320.0, 5720.0 };
        public const int SeaClipLevels = 7, SeaClipQuads = 128;
        public const double SeaClipSpacing0 = 0.5, SeaClipHalfExtentMax = 2048.0, SeaClipGeomorph = 0.15;
        public const double PlumeHeightMin = 6.0, PlumeHeightMax = 14.0, PlumeDuration = 3.0;
        public const double ImpactIntervalCrest = 9.0, ImpactIntervalReef = 6.0;
        public const double ImpactStrengthMin = 0.3, ImpactStrengthMax = 1.0;
        public const double StormCloudSpeed = 12.0;

        // ================================================================ beat 3: spillway (14.12 row 7, 14.4, 14.5.3)
        public const double SpillwayZ0 = 420.0, SpillwayZ1 = 704.0;
        public const int PierCount = 15;               // k 0-14 at z 420 + 20 k to 424 + 20 k
        public const double PierZ0 = 420.0, PierPitch = 20.0, PierLength = 4.0, PierEastX = 46.0, PierTopY = 0.5;
        public const int SlotCount = 14;               // k 0-13 at z 424 + 20 k to 440 + 20 k
        public const double SlotZ0 = 424.0, SlotLength = 16.0;
        public const double SillY = -14.0;
        public const double RadialGateX0 = 9.0, RadialGateX1 = 12.0, RadialGateRadius = 14.0, RadialGateWidth = 16.0, RadialGateRaise = 5.0;
        public const double GateHouseX0 = 9.0, GateHouseX1 = 14.0, GateHouseW = 5.0, GateHouseD = 4.0, GateHouseH = 5.0;
        public const double LipX = -6.0, LipY = -11.5;
        public const double DrawdownY0 = -8.0;         // at x 46
        public const double FlowSpeed0 = 0.8, LipSpeed = 8.3;   // Bernoulli for the 3.5 m drop
        public const double SheetGravityHalf = 4.9;    // sheet x = -6 - 8.3 t, y = -11.5 - 4.9 t^2
        public const double SheetT1 = 4.7;             // t 0-4.7 s (x -45, y -120)
        public const double SheetWidth = 16.0, SheetLipThickness = 2.5, SheetNearLength = 90.0;
        public const double PlumeZ = 562.0, PlumeSigmaZ = 150.0, PlumeTopY = 260.0, PlumeXMin = -2500.0, PlumeXMax = -6.0;
        public const double PlumeScaleHeight = 130.0, PlumePeakOpticalDepth = 1.0;
        public const double GratingBar = 0.04, GratingPitch = 0.08;
        public const double SpillwayDeckWidth = 12.0;
        public const double RainbowPrimaryDeg = 42.0, RainbowSecondaryTopDeg = 34.4, RainbowTopDeg = 25.4;

        // ================================================================ beat 4: reef and tower (14.12 rows 10-11, 14.5.4)
        public const double ReefZ0 = 704.0, ReefZ1 = 1040.0, ReefNorthZ0 = 1560.0, ReefX0 = 6.0, ReefX1 = 1600.0;
        public const double CoveZ0 = 1040.0, CoveZ1 = 1560.0;
        public const double UnitSize = 40.0;           // "Colossopod", tip to tip
        public const double UnitArmRadius0 = 6.0, UnitArmRadius1 = 3.6;
        public const double ReefGrid = 28.0, ReefJitter = 8.0;
        public const int ReefLayers = 3;
        public const double BeltX0 = 6.0, BeltX1 = 140.0, BeltTipYMin = -14.0, BeltTipYMax = -1.5;
        public const int BeltUnitsMin = 40, BeltUnitsMax = 80;
        public const double ParapetHidesReef = 45.0;
        public static readonly double[] IsletX = { 150.0, 260.0, 180.0, 420.0, 520.0, 700.0 };
        public static readonly double[] IsletZ = { 745.0, 830.0, 905.0, 740.0, 950.0, 985.0 };
        public const double IsletCrestYMin = 10.0, IsletCrestYMax = 22.0, IsletRadiusMin = 45.0, IsletRadiusMax = 70.0;
        public const int IsletUnitsMin = 7, IsletUnitsMax = 19;
        public const double IsletMinWallDistance = 140.0;
        public const double UnitLod0Range = 250.0, HornRange = 1600.0, TierCrossover = 400.0;
        public const double GaugePlatform = 1.0, GaugeRail = 1.1, GaugeRange = 300.0;
        public const int GaugeOneIn = 5;
        public const double ReefPlumeHeightMin = 10.0, ReefPlumeHeightMax = 22.0;
        public const int ReefPlumesMax = 8;
        public const double ReefWind = 9.0, ReefMistOpticalDepthMax = 0.5;
        public const double TowerXMin = -6.0, TowerXMax = 6.0, TowerZ0 = 920.0, TowerZ1 = 956.0, TowerHeight = 80.0, TowerCrown = 12.0;
        public const double PortalXMin = -3.3, PortalXMax = 0.3, PortalWidth = 3.6, PortalHeight = 3.4;
        public const double ArchXMin = 0.8, ArchXMax = 5.5, ArchWidth = 4.7, ArchHeight = 5.5, ArchZ = 956.0;
        public const double ParapetRestoredZ = 704.0;

        // ================================================================ beat 5: atrium, stairs, exit hall (14.5.5)
        public const double AtriumZ0 = 920.0, AtriumZ1 = 934.0, AtriumWidth = 12.0, AtriumHeight = 9.0;
        public const double PendantSize = 0.4, PendantY = 5.8, PendantX = 3.0;   // KS13 margin (Wave X2 interiors): z 931 pendant top at row 46 of 1080 (3 % = 32)
        public static readonly double[] PendantZ = { 923.0, 927.0, 931.0 };
        public const double GuardRailHeight = 1.1;
        public const double StairMouthXMin = -3.3, StairMouthXMax = 0.3, StairMouthZ = 934.0;
        public static readonly double[] PortholeS = { 969.0, 989.0, 1008.0 };   // landings 5, 8, 11
        public static readonly double[] PortholeEyeDepth = { 8.3, 19.1, 29.9 };
        public const double PortholeDiameter = 0.6, PortholeDepth = 2.1;
        public const double ExitHallS0 = 1035.388, ExitHallS1 = 1070.0, ExitHallWidth = 4.4, ExitHallHeight = 4.0, ExitHallLampPitch = 8.0;
        public const double ExitDoorWidth = 4.4, ExitDoorHeight = 4.0;
        public const double PressureHumS = 1000.0;

        // ================================================================ beat 6: window gallery (14.12 row 12, 14.4, 14.5.6)
        public const double HallHeight = 28.0, HallWestWall = -5.5, HallRail = 3.0, HallRailHeight = 1.1;
        public const double HallTrenchWidth = 3.0, HallTrenchDepth = 8.0;
        public const double HallAxisX0 = 4.243, HallAxisSlope = 0.015009;   // axis x(s) = 4.243 + 0.015009 (s - 1070)
        public const int PaneCount = 10;
        public const double PaneS0 = 1085.0, PanePitch = 30.0, PaneWidth = 20.0, PaneHeight = 36.0, PaneFloorOffset = -8.0;
        public const double PierWidth6 = 10.0, GlassThickness = 2.4, GlassReflection = 0.04;
        public const double HatchPitch = 30.0, BollardLampHeight = 0.9, BollardLampPitch = 20.0;
        public static readonly double[] SurveyLampS = { 1220.0, 1250.0 };
        public const double SurveyLampHalfAngle = 28.0, SurveyLampRange = 45.0, SurveyLampYaw = 70.0;
        public const int SnowQuads = 6000;
        public const double SnowBoxX = 40.0, SnowBoxY = 60.0, SnowBoxZ = 60.0, SnowSize = 0.03, SnowDrift = 0.03, SnowTurbulence = 0.1, SnowEdgeFade = 8.0;
        public const double HallEndWallS = 1373.0;
        // whale (14.12 row 14, 14.5.6, 14.7 H6)
        public const double WhaleLength = 78.0, WhaleDiameter = 14.0, WhaleFlukes = 22.0, WhaleFins = 9.0;
        public const double WhaleRho0 = 160.0, WhaleRhoSlope = 2.667, WhaleRhoS0 = 1170.0, WhaleLateral = 44.0;
        public const double WhaleSpeedFactor = 1.667, WhaleGait = 32.0;
        public const double WhaleZ0 = 1330.0, WhaleTimeSpeed = 2.5;   // z_c = smin(1330 - 2.5 (t - t0), 1330 - 1.667 (s - 1170))
        public const double WhaleCheckS = 1230.0, WhaleCheckTol = 1.0;
        public const double WhaleCallS = 1222.0;
        public const double LampAOn0 = 1219.0, LampAOn1 = 1220.5, LampAFull1 = 1224.5, LampAOff1 = 1227.5;
        public const double LampBOn0 = 1226.0, LampBOn1 = 1227.5, LampBFull1 = 1231.5, LampBOff1 = 1234.5;

        // ================================================================ beat 7: tunnel, ledge, lantern (14.12 row 15, 14.5.7)
        public const double TunnelWidth = 3.2, TunnelHeight = 3.4;
        public const double TunnelMouthS = 1422.85, TunnelMouthCut = 10.4;
        public const double TunnelLampPitch = 6.0;
        public const double GratingPanelHeight = 2.8, GratingPanelLength = 43.0;
        public const double ConduitWidth = 8.0, ConduitHeight = 6.0, ConduitSpeed = 14.0;
        public const double LedgeXMin = -12.4, LedgeXMax = -6.0, LedgeZ0 = 1420.0, LedgeZ1 = 1503.0, LedgeThickness = 0.8;
        public const double LedgeRail = 1.1, LedgeRailPost = 0.12, LedgeRailPostPitch = 2.0, LedgeRailDiameter = 0.10;
        public const double MooringRing = 0.3, MooringRingPitch = 6.0;
        public const double LeeFaceAboveLedge = 90.0;
        public const double LanternX = -9.2, LanternY = -90.0, LanternZ = 1499.0;
        public const double LanternPost = 1.4, LanternBase = 0.4, LanternGlass = 0.30, LanternEmissive = 6.0;
        public const double LanternLightRange = 5.0, LanternLightIntensity = 2.0, LanternFlicker = 0.03;
        public const double LanternHz1 = 0.31, LanternHz2 = 0.77, LanternHz3 = 0.97, LanternW1 = 1.0, LanternW2 = 0.6, LanternW3 = 0.3;
        public static readonly double[] JetZ = { 1900.0, 2700.0, 3900.0, 5400.0, 7200.0 };
        public const double JetSpeed = 40.0, JetWidth = 12.0, JetMistDistance = 1000.0, JetY0 = -90.0;
        public const double CityLightS0 = 1425.0, CityLightWindow = 50.0;   // N(s) = smoothstep((s - 1425) / 50)

        // ================================================================ the ending (ADDENDUM A1 00 LEAD DECISION 1; section 3.7 sun)
        // One coordinate: s 0-1500 is the walked path (unchanged); s > 1500 is the ending timeline (EndingPath in BwPath.cs):
        // the ride legs advance s at RideSRate per ride second at EVERY walking speed (doors, 75 s descent, transfer, 55 s
        // shuttle, doors), then the 40 m walk on the lantern platform (1 m of s per m walked) to the Keeper's Lantern.
        public const double RideSRate = 2.5;           // s per ride second (= CINE)
        public const double RideBoardTime = 4.0;       // doors close 0.5-3.0, seal 3.0-4.0
        public const double RideDescentTime = 75.0;    // the vertical descent down the lee face (cosine S-curve)
        public const double RideTransferTime = 6.0;    // at the foot: the cabin settles, unlatches, couples to the shuttle bogie
        public const double RideShuttleTime = 55.0;    // the maglev run west (cosine S-curve; fix wave R1: 35 -> 55 s, a longer look back)
        public const double RideDoorsTime = 3.0;       // arrival: the north door opens
        public const double DescentVMax = 110.0;       // m/s, peak of the descent (lead decision "about 110")
        public const double CabinFloorFootY = -5999.0; // shuttle deck and the lantern platform: 1.0 m above the lowland
        public const double ShuttleDistance = 3500.0;  // m west (lead decision)
        public const double ShuttleRampTime = 20.0;    // s of each cosine ramp: v_max = 3500 / (55 - 20) = 100 m/s; ramps 1000 m each,
                                                       // cruise 15 s x 100 = 1500 m; mean accel 100 / 20 = 5.0 m/s^2 (0.51 g),
                                                       // peak pi / 2 x 5.0 = 7.85 m/s^2 (lead ruling R1)
        // cabin (interior box; glass west, north, south; east spandrel 1.1 m + glass; glass floor strip along the west wall)
        public const double CabinX0 = -12.0, CabinX1 = -7.0, CabinZ0 = 1494.4, CabinZ1 = 1500.4, CabinHeight = 3.0;
        public const double CabinFrame = 0.12, CabinFloorDepth = 0.45, CabinRoofDepth = 0.5, CabinGlassFloorX1 = -9.75;
        public const double CabinDoorHalf = 0.8;       // 1.6 m doors (south: boarding; north: exit), centred on the walker's x
        public const double LaneCabinFunnelS0 = 1488.5, LaneCabinFunnelS1 = 1496.9;   // ledge lane -> the door lane (z 1494.4 - 0.35 = s 1496.94)
        public const double LedgeEndZ = 1494.0;        // the ledge slab now ends at the top station (the shaft beyond)
        // the walk on the lantern platform (a FilletPath from the walker's spot in the cabin): north, a right arc to east, east
        public const double FootNorth = 12.0, FootArcR = 8.0, FootEast = 15.4336294;   // 12 + 4 pi + 15.4336294 = 40.0000000 m
        public const double FootDoorHalfLane = 0.45;   // capsule-centre lane through the 1.6 m door (0.8 - PLAYER_R)
        public const double FootPlazaHalfLane = 3.0;   // on the plaza
        public const double KeeperAhead = 1.9;         // the Keeper's Lantern 1.9 m beyond the rest point (14.11)
        public const double KeeperRight = 0.4;         // and 0.4 m to the right (south): the lantern in the lower middle of KS27
        public const double KeeperPost = 0.9;          // a low post: drum centre -17 deg, roof top 1.5 m (-5.9 deg) from the eye 1.94 m away (KS27 lower middle;
                                                       // below the KS28 frame bottom at 29 - 33 = -4 deg)
        public const double EndAlpenglowGain = 1.0;    // the key x (1 + 1.0) on the upper wall after the cabin passed the shadow line
        public const double EndEvGain = 0.8, EndEvS0 = 1560.0, EndEvS1 = 1700.0;   // EV +0.8 into the earth's shadow (the blue hour)
        public const double EndGoalBeforeRest = 1.5;   // goal trigger 1.5 m before the rest (as 1498.5 / 1500)
        public const double EndLookPitch = 29.0, EndLookYaw = 90.0;   // the final look back: crest 59.9 deg at row 43 of 1080 (4 %), vFOV 66
        public const double EndTiltStart = 1.5, EndTiltTime = 7.0;    // t_g of the tilt from pitch 0 to EndLookPitch (3.10)
        /// <summary>The slow tilt-up after the KS28 hold (lead, Wave X: the show's stinger rises 1500-2000 m behind the crest at
        /// t_g 5-29, up to 6 deg above it): pitch EndLookPitch -> EndLookPitch + EndLift over t_g 18-28 (smootherstep, peak
        /// 1.875 x 7 / 10 = 1.3 deg/s). At pitch 36 the frame spans 3-69 deg: the crest (59.7) 9.3 deg under the top edge.</summary>
        public const double EndLiftStart = 18.0, EndLiftTime = 10.0, EndLift = 7.0;
        /// <summary>After-goal sun sink el -2.35 -> -2.82 (3.7) over t_g 9-23: the crest goes dark (el -2.752, 85.5 % of the
        /// sink, smootherstep u 0.715) at t_g 9 + 0.715 x 14 = 19.0 instead of 14.0, because the look now holds to t_g 40.</summary>
        public const double EndSunSinkStart = 9.0, EndSunSinkTime = 14.0, EndSunSink = 0.47;
        public const double SunRideEl0 = -2.10, SunRideElDescent = 0.10;   // el = -2.10 - 0.10 F (F = descended fraction)
        public const double SunFootEl0 = -2.20, SunFootElDrop = 0.15;      // shuttle + walk: -2.20 -> -2.35 at the goal
        public const double HazeFoot = 2.0e-4;         // haze s0 at the foot (3.7): 4.5e-5 to F 0.5, smootherstep to 2e-4 at F 1

        // ================================================================ sun and sky (14.12 row 8, 14.2.3)
        public const double SunB2Az = 100.0, SunB2El = 6.0;
        public const double SunB3Az0 = 98.0, SunB3AzRate = 0.05, SunB3El0 = 12.0, SunB3ElRate = 0.025, SunB3S0 = 420.0, SunB3S1 = 640.0;
        public const double SunB3AzAt640 = 109.0, SunB3ElAt640 = 17.5;
        public const double SunB4Az760 = 150.0, SunB4El760 = 48.0, SunB4Az920 = 176.0, SunB4El920 = 56.0;
        public const double SunB6Az0 = 215.0, SunB6AzRate = 0.255, SunB6El0 = 48.0, SunB6El1 = 20.0;
        public const double SunB7Az0 = 291.5, SunB7Az1 = 305.0, SunB7AzS0 = 1370.0, SunB7AzS1 = 1420.0;
        public const double SunB7El0 = -0.9, SunB7ElDrop = 1.2, SunB7ElS0 = 1420.0, SunB7ElWindow = 80.0;   // el(s)
        public const double SunBreakS0 = 520.0, SunBreakW0 = 55.0, SunBreakS1 = 640.0, SunBreakW1 = 60.0, SunBreakDim = 0.35;   // u_sun(s)
        public const double CloudHoleDistance = 22000.0, CloudHoleAz = 285.0, CloudHoleRadius = 12000.0, CloudHoleS0 = 530.0, CloudHoleWindow = 70.0;
        public const double SunB4KeyS0 = 690.0, SunB4KeyWindow = 50.0;
        public const double KeyB2 = 0.35, KeyB3Peak = 3.4, KeyB3End = 2.6, KeyB4 = 4.2, KeyB5Tungsten = 3.0, KeyB7 = 0.9;
        public const double KeyB1Sodium = 2.5;
        public const double SunDiscRadiance = 60.0;
        public const double InteriorAmbient = 0.06;    // interior ambient = lamp colour x 0.06
        public const double SkyMidLerp = 0.45;         // mid colour lerp(zenith, horizon, 0.45), linear
        public const double DuskVioletElev = 35.0, DuskRoseElev = 12.0, DuskSunAz = 305.0;
        public const double FogElevMin = -1.0, FogElevMax = 6.0, FogDesaturate = 0.2;

        // ================================================================ atmosphere and sea (14.12 row 9, 14.2.2, 14.2.4, 14.7 H1)
        public const double HazeScaleHeight = 2000.0, AltitudeOffset = 6000.0;
        public const double StormSlabY0 = 5600.0, StormSlabY1 = 6400.0, StormSlabEdge = 150.0;
        public static readonly double[] HazeSigma = { 6e-3, 1.3e-4, 9e-5, 6e-5, 0.0, 0.0, 4.5e-5 };   // beat 1 dust, 5 none, 6 water
        public static readonly double[] StormSigma = { 0.0, 6.5e-4, 6.5e-4, 1.0e-4, 0.0, 0.0, 0.0 };
        public const double StormSigmaB3End = 2.0e-4, StormSigmaB3S0 = 520.0, StormSigmaB3S1 = 640.0;
        public static readonly double[] SeaHs = { 2.8, 2.8, 2.0, 3.2, 3.2, 3.2, 3.2 };   // beats 2-4 (others carry the neighbour)
        public static readonly double[] SeaTp = { 9.0, 9.0, 8.0, 11.0, 11.0, 11.0, 11.0 };
        public static readonly double[] SeaPeakWavelength = { 126.0, 100.0, 189.0 };     // beats 2, 3, 4
        public const int SeaComponents = 32;
        public const double SeaMinWavelength = 0.5, SeaSteepness = 0.6, SeaDirSpreadDeg = 35.0;
        public const double SeaSlopeVarFloor = 0.008, GlintClamp = 8.0, FoamClamp = 4.0, FoamJacobian = 0.15;
        public const double ClapotisFade0 = 150.0, ClapotisFade1 = 400.0;

        // ================================================================ water model (14.12 row 13, 14.7 H3)
        public const double KdR = 0.28, KdG = 0.050, KdB = 0.048;
        public const double BeamCR = 0.34, BeamCG = 0.085, BeamCB = 0.050;
        public const double VeilSR = 0.30, VeilSG = 0.85, VeilSB = 0.80;
        public const double VeilBeta = 2.6, VeilRedFloor = 0.045, VeilG0 = 0.061, VeilG0Depth = 44.3;   // X2 S2-5: veil G/B 0.97 / 0.94 / 0.90 at 46 / 64 / 82 m (was 0.38 / 0.29 / 0.22)
        public const double DarkFloorLinear = 0.0018;  // sRGB 6 before the dither
        public const double Refraction = 1.33;
        public const double ShaftReach = 60.0;
        public const int ShaftSamples = 12;

        // ================================================================ camera, vFOV, exposure, grade (14.12 rows 16-17, 14.2.5)
        public const double CameraNear = 0.05;
        public const double FarOutdoor = 12000.0, FarBeat1 = 150.0, FarBeat5 = 300.0, FarBeat6 = 600.0;
        public const double FarSwitch1 = 100.0, FarSwitch2 = 934.0, FarSwitch3 = 1030.0, FarSwitch4 = 1373.0;
        public const int Msaa = 4;
        public static readonly double[] VFov = { 62.0, 64.0, 66.0, 64.0, 60.0, 64.0, 66.0 };
        public const double VFovB5S0 = 890.0, VFovB5S1 = 920.0;   // beat 5 entry window
        public static readonly double[] ExposureEv = { 0.3, 0.0, -0.3, -0.9, 0.7, 0.4, 0.8 };
        public static readonly double[] ExposureEvEnd = { 0.3, 0.0, -0.3, -0.9, 0.7, 1.0, 0.3 };
        public const double EvB3S0 = 400.0, EvB3S1 = 440.0, EvB4S0 = 690.0, EvB4S1 = 740.0, EvB5S0 = 912.0, EvB5S1 = 952.0;
        public const double EvB6S0 = 1070.0, EvB6S1 = 1370.0, EvB7S0 = 1415.0, EvB7S1 = 1445.0;
        public const double EvSlew = 0.30, WbSlew = 6.0, BloomSlew = 0.1;
        public const double PortalLampBoostEv = 1.3, PortalLampBoostWindow = 40.0;   // 2^(1.3 (1 - smootherstep((s-920)/40)))
        public static readonly double[] BloomThreshold = { 1.0, 1.0, 1.0, 1.0, 0.9, 0.8, 0.9 };
        public static readonly double[] BloomIntensity = { 0.20, 0.15, 0.25, 0.20, 0.30, 0.20, 0.30 };
        public static readonly double[] BloomScatter = { 0.65, 0.70, 0.75, 0.70, 0.70, 0.80, 0.75 };
        public static readonly double[] Saturation = { 0.0, -8.0, 6.0, 4.0, 4.0, 0.0, 8.0 };
        public static readonly double[] Contrast = { 0.0, 4.0, 4.0, 4.0, 0.0, 0.0, 0.0 };
        public static readonly double[] WhiteBalance = { 6.0, -4.0, 8.0, -8.0, 10.0, -6.0, 4.0 };
        public static readonly double[] Vignette = { 0.22, 0.14, 0.12, 0.10, 0.20, 0.16, 0.14 };
        public static readonly double[] DitherPeak = { 2.0, 1.5, 1.5, 1.5, 1.5, 2.0, 2.0 };
        public const double ContrastPivot = 0.18;
        public const double ClampLamp = 3.0, ClampLantern = 6.0, ClampSunDisc = 60.0, ClampBloom = 40.0, ClampOpening = 12.0;
        public const double OpeningClippedMax = 0.03;
        public const double PhotosensitiveHz = 1.0;

        // ================================================================ audio (14.12 row 18, 14.3)
        public const double AudioSampleRate = 48000.0;
        public static readonly double[] TargetRmsDb = { -34.0, -24.0, -18.0, -23.0, -32.0, -38.0, -25.0 };
        public const double TargetRmsB7Ledge = -35.0, TargetRmsB7LedgeS = 1470.0, TargetRmsTol = 1.5;
        public const double TruePeakDb = -1.0, LimiterAsymptoteDb = -1.42, DcMax = 0.001, SampleStepMax = 0.25;
        public static readonly double[] ReverbDecay = { 1.4, 0.7, 0.9, 0.8, 2.2, 5.5, 2.0 };
        public const double ReverbStairWell = 3.0, ReverbLedge = 0.5, ReverbHallHfRatio = 0.35;
        public const double SilenceS1S0 = 96.0, SilenceS1S1 = 108.0, SwellS2S = 104.0, FallRoarS3S0 = 380.0, FallRoarS3S1 = 520.0;
        public const double PortalHushS = 920.0, HallHushS = 1070.0, HallHushTime = 8.0, MouthDuckS = 1423.0;
        public const double ConduitBellS0 = 1380.0, ConduitBellS1 = 1423.0;

        // ================================================================ key shots (14.10.2; luma as a fraction of 255)
        static double L(double level) => level / 255.0;

        // ---- key-shot expectations at 4K (px) with the 14.10.5 rulings of Wave B (DEVELOPMENT.md "Blockers and decisions";
        //      read by QA.VisibilityAudit). The spec value is kept in the comment; the number is the computed one.
        /// <summary>KS03 own mean-luma band (levels): the exterior through the 59 deg open door (spec: beat 1 band 28-70).</summary>
        public const double KS03LumaMin = 70.0, KS03LumaMax = 120.0;
        /// <summary>KS02 slit width: 277 px at the rows farthest from the centre (spec 270 = 0.3 / 2.0 x 1797 on axis).</summary>
        public const double KS02SlitWidth4K = 277.0, KS02SlitTol4K = 6.0;
        /// <summary>KS05 deck strip row: 1628 along the centre ray (the edge 1.5 / sin 75 = 1.553 m away, el -47.59 deg;
        /// spec 1661 used the -48.6 deg ray 1.5 m perpendicular).</summary>
        public const double KS05DeckStripRow4K = 1628.0, KS05DeckStripTol4K = 10.0;
        /// <summary>KS06 cart: 115 px (near face at 28.6 m; spec 109 = 1.9 / 30 x 1728).</summary>
        public const double KS06CartHeight4K = 115.0, KS06CartTol4K = 5.0;
        /// <summary>KS13 stair mouth 917 px (camera depth 8 cos 14 - 1.7 sin 14 = 7.35 m; spec 842) and north arch 319 px (spec 293).</summary>
        public const double KS13MouthWidth4K = 917.0, KS13MouthTol4K = 10.0, KS13ArchWidth4K = 319.0, KS13ArchTol4K = 6.0;
        /// <summary>KS14 from the 14.4 stair layout (eye s 985.07, y -25.3; landing 7 at s 987.694-988.894, y -28.8): landing
        /// rows 1704-2122 (spec 1573-1778), following flight top row 1521 (spec 1497), the riser 3 m ahead 57 px (spec 117:
        /// the risers face away from a viewer looking down the stairs; 117 px would need a face-on 0.2 m at 3 m).</summary>
        public const double KS14LandingRow0_4K = 1704.0, KS14LandingRow1_4K = 2122.0, KS14FlightTopRow4K = 1521.0, KS14Riser3mHeight4K = 57.0;
        /// <summary>KS20 lantern post 53 px (27.5 deg off axis; spec 47 on axis) and the north vanishing column 2792 (spec 2783).</summary>
        public const double KS20LanternHeight4K = 53.0, KS20LanternTol4K = 3.0, KS20VanishingCol4K = 2792.0, KS20VanishingTol4K = 10.0;
        /// <summary>KS15 at 21:9: the top-corner rays meet the glass 6 m ahead at lateral +-10.48 m (pane half-width 10), so a
        /// pier sliver shows in both top corners; accepted (the piers frame the glass): the fill rule holds at 16:9 and 16:10.</summary>
        public const string KS15FillExemptAspect = "21:9";

        /// <summary>14.10.2 (capture poses: this table only).</summary>
        public static readonly KeyShot[] KeyShots =
        {
            new KeyShot("KS01_gal_far",     8.0,    -1.5,  1.7,    8.0,     0.0,   0.0, 62.0,  L(28),  L(70)),
            new KeyShot("KS02_gal_slit",    66.0,   -1.5,  1.7,    66.0,    90.0,  4.0, 62.0,  L(28),  L(70)),
            new KeyShot("KS03_door_open",   116.5,  -1.5,  1.7,    116.5,   0.0,   1.0, 62.58, L(KS03LumaMin), L(KS03LumaMax)),
            new KeyShot("KS04_crest_sea",   160.0,  4.2,   1.7,    160.0,   80.0, -8.0, 64.0,  L(85),  L(150)),
            new KeyShot("KS05_crest_drop",  230.0,  -4.5,  1.7,    230.0,  -75.0, -30.0, 64.0, L(85),  L(150)),
            new KeyShot("KS06_crest_axis",  300.0,  -1.5,  1.7,    300.0,   0.0,   2.0, 64.0,  L(85),  L(150)),
            new KeyShot("KS07_spill_gates", 492.0,  1.0,   1.7,    492.0,   70.0, -6.0, 66.0,  L(95),  L(150)),
            new KeyShot("KS08_spill_bow",   604.0,  -4.0,  1.7,    604.0,  -75.0, -2.0, 66.0,  L(95),  L(150)),
            new KeyShot("KS09_spill_grate", 572.0,  -4.0,  1.7,    572.0,  -90.0, -58.0, 66.0, L(95),  L(150)),
            new KeyShot("KS10_reef_wide",   776.0,  3.0,   1.7,    776.0,   78.0, -3.0, 64.0,  L(100), L(165)),
            new KeyShot("KS11_reef_spray",  824.0,  0.9,   1.7,    824.0,   25.0,  6.0, 64.0,  L(100), L(165)),
            new KeyShot("KS12_tower_up",    852.0,  -1.5,  1.7,    852.0,   0.0,  24.0, 64.0,  L(100), L(165)),
            new KeyShot("KS13_atrium",      926.0,  -1.5,  1.7,    926.0,   0.0,  14.0, 60.0,  L(28),  L(80)),
            new KeyShot("KS14_stairs_down", 985.0,  1.11,  -25.3,  985.0,   3.0, -24.0, 60.0,  L(28),  L(80)),
            new KeyShot("KS15_win_first",   1115.0, 4.92,  -57.70, 1115.0,  90.0, 12.0, 64.0,  L(14),  L(85)),
            new KeyShot("KS16_win_up",      1166.0, 5.68,  -63.82, 1166.0,  70.0, 40.0, 64.0,  L(14),  L(85)),
            new KeyShot("KS17_win_eye",     1268.0, 7.215, -76.06, 1267.838, 90.0, 2.0, 64.0,  L(14),  L(85)),   // A1 4.9: the kraken eye replaces the whale
            new KeyShot("KS18_win_down",    1320.0, 8.00,  -82.30, 1320.0,  60.0, -24.0, 64.0, L(14),  L(85)),
            new KeyShot("KS19_sluice",      1405.0, -0.48, -88.3,  1403.44, 35.0,  0.0, 66.0,  L(20),  L(90)),
            new KeyShot("KS20_ledge_world", 1452.0, -9.2,  -88.3,  1449.1, -27.5, -7.0, 66.0,  L(30),  L(90)),
            new KeyShot("KS21_beacon",      1490.0, -9.2,  -88.3,  1487.2,  0.0,  -1.0, 66.0,  L(20),  L(90)),
            // the ending (fix wave R5; the ending builder's own poses, Beat08Ending s_shots): eye = EndingPath (cabin spot -9.199,
            // 1497.114, eye 1.7 m above the cabin floor; the platform walk after s 1857.5). Ride shots by s; KS27 / KS28 / KS28b /
            // KS29 are at the rest (s 1897.5) at the end-camera times t_g 1.5 / 8.5 / 20 / 30 (pitch 0 / 29 / 29 / 36; the
            // capture needs the end clock, the lead QA builder's variant). Beat 8 luma 20-80, beat 9 20-70 (A1 4.10), KS28 25-70.
            new KeyShot("KS22_ride_drop",       1530.0,  -9.199,  -183.674, 1497.114, 270.0, -30.0, 66.0, L(20), L(80)),
            new KeyShot("KS23_ride_sunset",     1571.2,  -9.199, -1610.600, 1497.114, 300.0,  -3.0, 66.0, L(20), L(80)),
            new KeyShot("KS24_ride_lookup",     1616.0,  -9.199, -3581.800, 1497.114,  25.0,  52.0, 66.0, L(20), L(80)),
            new KeyShot("KS25_ride_lanterns",   1680.0,  -9.199, -5932.351, 1497.114, 262.0, -12.0, 66.0, L(20), L(80)),
            new KeyShot("KS26a_transfer",       1705.0,  -9.199, -5997.300, 1497.114,  80.0,  65.0, 66.0, L(20), L(80)),
            new KeyShot("KS26_shuttle_back",    1822.0, -3261.871, -5997.300, 1497.114, 90.0, 32.0, 66.0, L(20), L(80)),
            new KeyShot("KS26b_platform_walk",  1865.0, -3509.199, -5997.300, 1504.614, 330.0, 3.0, 66.0, L(20), L(70)),
            new KeyShot("KS27_keeper",          1897.5, -3485.765, -5997.300, 1517.114, 90.0,  0.0, 66.0, L(20), L(70), 1.5),
            new KeyShot("KS28_dam_reveal",      1897.5, -3485.765, -5997.300, 1517.114, 90.0, 29.0, 66.0, L(25), L(70), 8.5),
            new KeyShot("KS28b_crest_dark",     1897.5, -3485.765, -5997.300, 1517.114, 90.0, 29.0, 66.0, L(20), L(70), 20.0),
            new KeyShot("KS29_lift",            1897.5, -3485.765, -5997.300, 1517.114, 90.0, 36.0, 66.0, L(20), L(70), 30.0),
            // the kraken key shots of A1 4.9 (show / gallery requests; appended so the indices of KS01-KS21 stay)
            new KeyShot("KK1_arm_tower",        378.0,   -1.5,    1.7,      378.0,     22.0,   4.0, 64.0, L(85),  L(150)),
            new KeyShot("KK2_head_eyes",        806.0,    2.936,  1.7,      806.0,     72.0,  -2.0, 64.0, L(100), L(165)),
            new KeyShot("KK4_arm_glass",        1232.0,   6.674, -71.74,    1231.842,  66.0,   4.0, 64.0, L(14),  L(85)),
        };
    }

    /// <summary>One key shot of 14.10.2: progress s, eye position, yaw / pitch (deg), vFOV (deg), expected mean luma
    /// range as a fraction of 255 (14.10.3 or the shot's own acceptance). Thread's shape without `wide`.</summary>
    public struct KeyShot
    {
        public string tag;
        public double s, x, y, z, yaw, pitch, fov, lumaMin, lumaMax;
        /// <summary>End-camera time t_g (s after the goal trigger) for the shots at the lantern (KS27-KS29), NaN otherwise: the
        /// capture sets the end clock (sun sink, stinger) with it (fix wave R5).</summary>
        public double tg;
        public KeyShot(string tag, double s, double x, double y, double z, double yaw, double pitch, double fov, double lumaMin, double lumaMax, double tg = double.NaN)
        { this.tag = tag; this.s = s; this.x = x; this.y = y; this.z = z; this.yaw = yaw; this.pitch = pitch; this.fov = fov; this.lumaMin = lumaMin; this.lumaMax = lumaMax; this.tg = tg; }
    }
}
