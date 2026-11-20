// BoulevardLayout: the dimensions of beat 2 (MASTER_PROMPT 14.5.2, 14.4) as named constants, one source for the builders, the
// EditMode tests (Beat02BoulevardTests) and the KS03-KS06 projections. The path crosses the south half of the street northward
// (s 120-171, x = 0), turns right at C1 (0, 195) r 24 and runs east along the median axis z 195 (x = s - 184.7) to the tongue
// root x 215.3 (s 400).
//
// Street facades (agreement 2): south z 120, north z 270 (street 150.0 m), hero extent x -90 (south: the alley's west block) /
// -60 (north: CityField's "boulevard canyon hero" exclusion) to x 215.3 where face A begins. Each side is
//   * a podium 40 m (14.5.2) with the arcade 8 m: columns 0.6 m every 6 m on the facade line (x = 3 + 6k: the alley mouth at
//     x -2.5..2.5 falls between two bays; 41 south + 43 north = 84, 14.5.2), the arcade 4 m deep (back wall at z 116 / 274);
//   * a terrace setback 4 m: the shafts stand back at z 116 / 274;
//   * 7 slabs per side from x -33, 30 m wide, 6 m slots (x -3..3, 33..39, ...), the seventh ends at x 213; slab depth 30 m (the
//     shafts z 86..116 and 274..304; the podium z 86..120 and 270..304), shafts y -3 300..560 (NocFacade, warm fraction 0.10);
//     G1 closes x 213..215.25 (slab 6's closure, 5 cm behind face A's plane x 215.3, agreement 2);
//   * south exception (proposed decision G1-D1): slabs 0 (x -33..-3) and 1 (3..33) stand over the alley's last 30 m, directly
//     north of the KS02 eye (0, 1.7, 88): their slot faces x +-3 would fill the slit below the zenith row (at frame y +0.19 the
//     open slit shrinks from +-0.097 to +-0.027 and the cloud is gone), so the alley blocks keep their podia (22 / 31 m) to the
//     street there and the alley towers (x <= -30, x >= 34, 14.5.1) frame the gap; the south slab rhythm starts at slab 2 (x 39).
// Median z 183-207 (24.0 m) with the 0.9 m edge barrier in its outer 0.3 m (inner face at |dz| 11.70, 0.32 m behind the lane wall
// of 11.0 + 0.38); 40 sodium lamp posts 12 m every 14 m in the barrier lines; tram rails z 163 / 227; terminus x 205-215.3,
// z 212-270. Blades: north x = 18 + 36 k at z 263 (y 40-240), south x = 6 + 36 k at z 127 (y 60-260), 14 x 200 m.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class BoulevardLayout
    {
        // ---- street (14.5.2 acceptance: street 150.0, median 24.0)
        public const double SouthZ = 120.0, NorthZ = 270.0, AxisZ = 195.0;
        public const double FacadeX1 = 215.3, SouthX0 = -90.0, NorthX0 = -60.0;
        /// <summary>The street floor runs west far into the CityField canyon (its edge lies within 0.07 deg of the horizon).</summary>
        public const double StreetFloorX0 = -1500.0, FloorChunk = 200.0, FloorThickness = 1.0;
        public const double PodiumH = 40.0, ArcadeH = 8.0, ArcadeDepth = 4.0, Setback = 4.0;
        public const double ColumnW = 0.6, ColumnPitch = 6.0, ColumnX0 = 3.0, ColumnInset = 0.02;
        public const int ColumnK0 = -7, ColumnK1 = 35;               // x = 3 + 6 k: -39 .. 213
        public const double MouthPierX = 4.5;                        // solid piers x 2.5..4.5 beside the alley mouth (south)
        public const double ShopY0 = 0.35, ShopY1 = 3.6, ShopInset = 0.6, ShopGap = 0.04;

        // ---- slabs (14.5.2)
        public const int SlabCount = 7;
        public const double SlabX0 = -33.0, SlabW = 30.0, SlabPitch = 36.0, SlabDepth = 30.0;
        public const double ShaftY0 = -3300.0, ShaftY1 = 560.0;
        public const double ClosureX1 = 215.25;                      // 5 cm behind face A (x 215.3)
        /// <summary>North slab "-1": the part of the rhythm's x -69..-39 slab inside the hero exclusion (x >= -60).</summary>
        public const double NorthWestSlabX0 = -60.0, NorthWestSlabX1 = -39.0;
        /// <summary>First south slab index (G1-D1: slabs 0 and 1 replaced by the alley blocks, KS02).</summary>
        public const int SouthFirstSlab = 2;
        /// <summary>The boulevard podium on the south side starts east of the alley's east block (x 33).</summary>
        public const double SouthPodiumX0 = 33.0;

        public static double SlabXa(int k) => SlabX0 + SlabPitch * k;
        public static double SlabXb(int k) => SlabXa(k) + SlabW;
        public static double SouthShaftZ0 => SouthZ - Setback - SlabDepth;     // 86
        public static double SouthShaftZ1 => SouthZ - Setback;                 // 116
        public static double NorthShaftZ0 => NorthZ + Setback;                 // 274
        public static double NorthShaftZ1 => NorthZ + Setback + SlabDepth;     // 304

        /// <summary>The x intervals of [x0, x1] not covered by a south slab (the east alley tower's north face shows there).</summary>
        public static List<Vector2> SouthSlabGaps(double x0, double x1)
        {
            var res = new List<Vector2>(4);
            double cur = x0;
            for (int k = SouthFirstSlab; k < SlabCount && cur < x1; k++)
            {
                double a = SlabXa(k), b = SlabXb(k);
                if (b <= cur) continue;
                if (a > cur) res.Add(new Vector2((float)cur, (float)Math.Min(a, x1)));
                cur = Math.Max(cur, b);
            }
            if (cur < x1) res.Add(new Vector2((float)cur, (float)x1));
            return res;
        }

        // ---- blades (14.5.2 BladeSign; acceptance 14 x 200)
        public const int BladesPerSide = 6;
        public const double BladeW = 14.0, BladeH = 200.0, BladeThick = 0.8, BladePitch = 36.0;
        public const double NorthBladeX0 = 18.0, NorthBladeZ = 263.0, NorthBladeY0 = 40.0;
        public const double SouthBladeX0 = 6.0, SouthBladeZ = 127.0, SouthBladeY0 = 60.0;
        public const double BladeGlyphOff = 0.03, BracketW = 0.6;
        public static readonly double[] BracketY = { 0.06, 0.5, 0.94 };   // bracket heights as fractions of the blade

        // ---- median (14.5.2; lanes of 14.4: crossing 10.0, median 11.0)
        public const double MedianZ0 = 183.0, MedianZ1 = 207.0, MedianHalf = 12.0;
        public const double BarrierH = 0.9, BarrierT = 0.3, BarrierX0 = -135.0;
        /// <summary>East end of the median barriers: the lane widens 11 -> 12 over s 396-404 (onto the tongue, NocLanes), and at
        /// s 398.5 (x 213.8) the capsule reaches 11.0 + 0.229 + 0.35 = 11.58 < 11.70 (the barrier's inner face).</summary>
        public const double BarrierX1 = 213.8;
        public const double LampH = 12.0, LampPitch = 14.0, LampArm = 1.6, LampHead = 0.8, LampHeadW = 0.45;
        public const int LampCount = 40;
        public const double BollardH = 0.9, BollardR = 0.1, BollardGap = 0.06, BollardSpacing = 2.9;
        /// <summary>Tram sight sector (14.5.2): nothing taller than 0.5 m in z 195-225, x 79-175 except the 0.9 m barrier.</summary>
        public const double SectorZ0 = 195.0, SectorZ1 = 225.0, SectorX0 = 79.0, SectorX1 = 175.0, SectorMaxH = 0.5;
        public const int Kiosks = 3, Benches = 12;
        public const double KioskW = 3.0, KioskH = 2.6, BenchL = 1.8, BenchD = 0.5, BenchH = 0.45, PlanterH = 1.0;

        // ---- tram (14.5.2: rails 0.07 m at z 163 and 227, terminus x 205-215.3, z 212-270)
        public const double TrackSouthZ = 163.0, TrackNorthZ = 227.0, Gauge = 1.435, TrackX1 = 205.0;
        public const double TerminusX0 = 205.0, TerminusX1 = 215.25, TerminusZ0 = 212.0, TerminusZ1 = 270.0, TerminusH = 14.0;
        public const double PortalW = 5.2, PortalH = 6.4;          // BV: the tram (3.4 x 3.9 m) and its contact wire (y 5.6) pass

        // ---- wet floor (14.7 H4)
        public const double WetS0 = 120.0, WetS1 = 400.0;
        public const float WallMacro = 0.035f;          // facade mean radiance (D-017 ~0.026) with the lit street base
    }
}
