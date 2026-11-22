// AlleyLayout: the dimensions of beat 1 (MASTER_PROMPT 14.5.1, 14.4) as named constants, one source for the builders, the
// EditMode tests (Beat01AlleyTests) and the projections of KS00-KS03. The alley runs along the path's first leg (x = 0, z = s,
// heading north) from the dead-end wall at z -2.5 to the mouth at z 120.
//
// Clear width. The visible walls follow the walkable lanes exactly: wall face = lane half-width + 0.35 m (player radius), so the
// clear width is 2 lane + 0.70 = the "collider gap minus 0.70" rule of 14.4 read the other way round, and the invisible PathGuide
// wall (lane + 0.38 m, D-021) sits 3 cm behind the visible one. NocLanes holds the knots (14.4 lane list: 1.85 s 0-26, 2.65 at the
// bar s 36-56, 1.95 s 66-70, 1.85 s 76-112, 2.15 at the mouth s 120; smootherstep between knots), which gives the 14.5.1 profile
// 4.4 / 6.0 / 4.6 / 4.4 / 5.0 m. (14.5.1 says "smoothstep"; the lanes are the one source of the profile, lesson 11.)
//
// Masses (all inside CityField's "alley podia corridor" exclusion x -90..90, z -60..125, D-018):
//   west block x -90..wall, z -60..120, podium 22 m; west tower x -90..-30 (set back 30 m from the axis), z -60..116, y 22..560;
//   east block x wall..90 (z -60..86) and wall..33 (z 86..120), podium 31 m; east tower x 34..90, z -60..86, y 31..560 (it ends
//   where the boulevard's south slabs begin, BoulevardLayout); dead-end block x -2.6..2.6, z -60..-2.5, 26 m (its sides run into
//   both podia), steel door 0.9 x 2.0 m at its centre.
// The KS02 slit (s 88, pitch +66): roof edges x -2.2 at y 22 and x +2.2 at y 31 from the eye (0, 1.7, 88): half-angles
// atan(2.2 / 20.3) = 6.19 deg and atan(2.2 / 29.3) = 4.29 deg (14.5.1); the shafts at -30 / +34 enter the slit above y 278 / 451.
using System;

namespace Megastructure
{
    public static class AlleyLayout
    {
        // ---- profile (14.5.1, 14.4 lanes)
        /// <summary>Wall face beyond the lane half-width: the player radius (14.12), so the capsule just touches the wall.</summary>
        public const double LaneToWall = 0.35;
        public const double S0 = 0.0, S1 = 120.0;
        /// <summary>z of the dead-end wall face (14.5.1) and of the mouth (s 120, the boulevard's south facade line).</summary>
        public const double DeadEndZ = -2.5, MouthZ = 120.0;
        /// <summary>Widest wall half-width of the profile (the bar, 6.0 m clear).</summary>
        public const double MaxHalf = 3.0;

        /// <summary>Wall half-width (m) at z (= s on the alley leg): lane(s) + 0.35, held at s 0 behind the start.</summary>
        public static double HalfWidth(double z)
        {
            double s = Math.Max(S0, Math.Min(S1, z));
            return NocLanes.LaneHalfWidth(s) + LaneToWall;
        }

        /// <summary>Clear width between the walls at s (the 14.5.1 acceptance numbers 4.4 / 6.0 / 4.6 / 5.0).</summary>
        public static double ClearWidth(double s) => 2.0 * HalfWidth(s);

        // ---- podia and towers (14.5.1)
        public const double WestPodiumH = 22.0, EastPodiumH = 31.0, DeadEndH = 26.0;
        public const double WestTowerX = -30.0, EastTowerX = 34.0;
        public const double BlockX0 = -90.0, BlockX1 = 90.0, BlockZ0 = -60.0;
        /// <summary>The west tower runs to the boulevard shaft line (z 116); the east tower ends behind the south slabs (z 86).</summary>
        public const double WestTowerZ1 = 116.0, EastTowerZ1 = 86.0;
        /// <summary>The east block narrows to x 33 north of z 86 (the boulevard podium x 33..215.3 takes over there).</summary>
        public const double EastBlockNarrowZ = 86.0, EastBlockNarrowX = 33.0;
        public const double TowerTop = 560.0;
        public const double WallBottom = -1.0;            // walls reach into the 1 m floor slab (no gap, agreement 1)
        public const double DeadEndHalf = 2.6;            // the dead-end block's sides run 0.4 m into both podia
        public const double DoorW = 0.9, DoorH = 2.0, DoorRecess = 0.08, DoorFrame = 0.06;
        /// <summary>Wall sampling along z (m): fine enough that the chord of a smootherstep flank deviates < 2 mm.</summary>
        public const double ProfileStep = 0.5;

        // ---- noodle bar (14.5.1 NoodleBar; KS01)
        public const double BarZ0 = 40.0, BarZ1 = 52.0;   // the recess (the 6.0 m flat of the profile, wall at x -3.0)
        public const double CounterZ0 = 43.0, CounterZ1 = 47.0, CounterDepth = 0.9, CounterH = 1.0, CounterRecess = 0.03;
        public const double WindowY0 = 1.0, WindowY1 = 2.3;   // window 4.0 x 1.3 m over the counter (the opening z 43..47)
        public const double RoomDepth = 1.2;                   // the lit bar room behind the opening (KS01 sees its back and far side)
        public const double AwningY0 = 2.6, AwningY1 = 3.0, AwningZ0 = 41.5, AwningZ1 = 48.5, AwningOut = 0.8;
        public const double SignY0 = 3.0, SignY1 = 4.4, SignZ0 = 42.0, SignZ1 = 48.0, SignOff = 0.06;
        public const double BarLightX = -2.0, BarLightY = 2.2, BarLightZ = 45.5, BarLightRange = 9.0, BarLightIntensity = 7.0;

        // ---- vending machines (14.5.1: 3 x 1.8 x 0.75 x 0.7 m on the east wall, z 62-67)
        public const int VendingCount = 3;
        public const double VendZ0 = 62.0, VendZ1 = 67.0, VendW = 0.75, VendH = 1.8, VendD = 0.7;
        public const double VendScreenY0 = 0.95, VendScreenY1 = 1.65, VendScreenW = 0.55;

        // ---- catwalk (14.5.1, KS02: x +-0.21, y -0.93..-0.70)
        public const double CatwalkZ0 = 96.0, CatwalkZ1 = 97.6, CatwalkY0 = 8.8, CatwalkY1 = 9.2, CatwalkEmbed = 0.25, CatwalkRail = 1.0;

        // ---- fire escapes, cables, blade signs (14.5.1: 3 + 3 escape modules, 140 cables, 22 blades; AL wave B: the AC units, pipes,
        //      awnings, bins and steam vents are laid out by AlleyFixtures)
        // fire escapes stay south of z 86: from the KS02 eye (z 88) anything north of it at y 4-15 m stands inside the slit
        public const double EscapeLandingD = 0.9, EscapeLandingL = 2.0, EscapeRail = 1.0, EscapeFirstY = 4.2, EscapeRise = 3.4;
        public static readonly double[] EscapeWestZ = { 12.0, 70.0, 84.0 };
        public static readonly double[] EscapeEastZ = { 6.0, 22.0, 80.0 };
        public const int EscapeLevels = 4;
        public const int CableCount = 140;
        public const double CableY0 = 5.0, CableY1 = 19.0, CableSag = 0.35;
        /// <summary>22 blade signs 0.5 x 2.0 m (14.5.1), 11 per wall, clear of the escapes, AC units, bar frontage and niche;
        /// bottoms y 3.8-6.0 by hash; every third one carries a small light (8 lights, 14.2.3).</summary>
        public const int BladeSignCount = 22;
        public const double BladeSignW = 0.5, BladeSignH = 2.0, BladeSignY = 3.8, BladeSignYSpan = 2.2, BladeSignGap = 0.05;
        public static readonly double[] BladeSignZWest = { 4.0, 17.0, 30.0, 35.0, 55.0, 62.0, 76.0, 93.0, 101.0, 108.0, 116.0 };
        public static readonly double[] BladeSignZEast = { 3.0, 12.0, 18.0, 28.0, 42.0, 55.0, 70.0, 86.0, 95.0, 104.0, 114.0 };

        // ---- side doors (14.5.1 scale cue: door 2.0 m): 9 per wall (two on the s 56-66 flank, seen by the mid-beat free looks), clear of the bar, the niche and the
        // width-test stations; 1.0 x 2.1 m openings, steel leaves 12 cm behind the wall plane, a lit transom 2 cm proud above
        // (y 2.2-2.45: above the capsule, 1.8 m, and 0.5 m above the camera sphere's centre)
        public const double SideDoorW = 1.0, SideDoorH = 2.1, SideDoorDepth = 0.12, TransomY0 = 2.2, TransomY1 = 2.45, TransomProud = 0.02;
        public static readonly double[] DoorZWest = { 6.0, 18.0, 54.0, 60.0, 69.0, 79.0, 92.0, 103.0, 108.0 };
        public static readonly double[] DoorZEast = { 9.0, 16.0, 38.5, 47.0, 59.5, 78.0, 88.0, 100.0, 106.0 };

        // ---- lit windows in the podium walls (graybox life: apartment and back-of-house windows above the street)
        public const double WinW = 1.0, WinH = 1.4, WinFloor = 3.2, WinY0 = 3.6, WinPitch = 4.5, WinProud = 0.02, WinLit = 0.45, WinWarmShare = 0.6;

        // ---- light kit (14.2.3 beat 1) and wet-floor registry (14.7 H4, D-003)
        public const double MouthLightY = 6.0, MouthLightZ = 121.0, MouthLightRange = 25.0, MouthLightIntensity = 4.0;
        public const float WetWallX = 2.2f;
    }
}
