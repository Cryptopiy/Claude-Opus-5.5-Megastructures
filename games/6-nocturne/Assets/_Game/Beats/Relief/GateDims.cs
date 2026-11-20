// GateDims: the numbers of the Q1-gate structures of GE (D-029: GB-02, GB-03, GB-06, GB-14; docs/logs/GE.md "Gate design"), pure C#
// in double so ReliefTests, Q1IntegrationTests and the visibility audit read the same values the meshes are built from.
//
// The problem (critique-graybox01 GB-02 / GB-03 / GB-06, G2-D4): agreement 2 leaves the canyon end open (no face A above y 0 for
// z 120..270) and agreement 5 cuts face B away above the garden (z 115..275, x 815.3..914), so the upper tube (y 118, z 195) ended in
// mid-air at both faces, the ten boulevard lanes ended over the garden, and the koi, the spire and the Heart sky showed through the
// notch from the boulevard, the Edge, the bridge and the garden long before the KS17 reveal. The fix is three pieces of architecture:
//   1. the canyon bridge: a six-storey skybridge across the canyon mouth at the face-A plane (x 212.25..232.25, y 108..132), keyed
//      into the last boulevard shafts and face A's flank masses (z 98..292); it carries the upper tube's west portal on its east
//      face. It stands east of x 212.05 because the last north blade (x 198..212, z 263..270, y 40..240) projects there. Every lane
//      passes 6.5 m or more above its roof (lowest lane y 140).
//   2. the notch gate: face B closed again above the garden by a 24 m deep lintel (x 815.9..839.3, soffit y 129, up to the face top
//      560, the notch walls as its ends), so the notch becomes a 160 m wide, 129 m tall gateway into face B (the spec's own KS07 note
//      "face B at y 293" at z 195 assumes a face there). A concrete head beam marks the opening; transfer ribs cross the soffit; the
//      upper tube's east portal sits in a keystone block hanging under the head (x 813.5..818, z 186..204, y 106..129.6). Every
//      lane enters a portal in the lintel, runs through it, crosses the open courtyard above the crowns and ends in the east block.
//      Clearances against FO's crown envelopes (GardenLeafCards: 1.10 x the ellipsoids) are asserted >= 3 m by ReliefTests: soffit
//      ribs 128.0 vs 124.6 (T4 at x 839.3), keystone 106.0 vs 102.5 (T4 at x 818, z 204), lanes vs 135.4 (B4).
//   3. the east block: the notch's east end closed by a building from the garden's east edge (x 914) to an arc of R 366 about the
//      spire (the koi's parts stay inside R 356, 14.5.7 / KoiMath), z 115..275, y -360..560 (face B's level-0 depth; above the arcade only, y >= 38, for
//      z 236..275). It is the garden courtyard's east wall, fills the open void between the garden and the plaza (GB-14) and hides
//      the Heart from every pose before the reveal. The lanes end in its tunnels: the D-019 alpha ramp starts at face B and is
//      e L / (1 - e) = 200.6 m long (x 815.3..1 015.9); it runs through the lintel (alpha 1.00 -> 0.96), across the 74.7 m
//      courtyard (0.96 -> 0.52) and inside the block; beyond the arc it is below 0.04 (north lanes) or 0.001 (south lanes).
// Lanes (NocTraffic.BoulevardLanes, GE lane edit): the four spec lanes below y 135 ((-48, 60), (-42, 95), (+44, 70), (+50, 112))
// cannot reach face B without entering the garden crowns, which reach y 98-125 at the face-B plane and 135 inside the notch, and a
// lane cannot end in the canyon (its 170-200 m ramp would fade in the open): they rise to (-48, 150), (-42, 175), (+44, 145),
// (+50, 185), above the canyon bridge and the crowns. The other six keep their spec heights.
using System;
using System.Collections.Generic;

namespace Megastructure
{
    public static class GateDims
    {
        // ------------------------------------------------------------------ 1. canyon bridge (beat 2/3 boundary)
        /// <summary>Body x 212.25..232.25 (20 m), y 108..132 (6 storeys of 4 m), z 98..292 (its ends inside the slab-6 shafts and face A's
        /// flank masses; x 215.3..232.25 cantilevers from face A's front plane over the tongue root).</summary>
        public const double CbX0 = 212.25, CbX1 = 232.25, CbY0 = 108.0, CbY1 = 132.0, CbZ0 = 98.0, CbZ1 = 292.0;
        /// <summary>Fascia bands (concrete, chamfered) at the soffit and roof edges of the long faces: proud 0.5 m.</summary>
        public const double CbFasciaLow = 2.4, CbFasciaTop = 1.0, CbFasciaProud = 0.5, CbParapet = 1.2, CbParapetT = 0.4;
        /// <summary>Soffit ribs across the bridge every 8 m (0.9 x 1.4 m) between the fascias.</summary>
        public const double CbRibPitch = 8.0, CbRibW = 0.9, CbRibD = 1.4;
        /// <summary>The upper tube's west portal on the east face: square panel half 7.0 around the r 6.3 collar hole; tunnel 16 m.</summary>
        public const double CbPanelHalf = 7.0, CbTunnelDepth = 16.0;

        // ------------------------------------------------------------------ 2. notch gate (face B above the garden)
        /// <summary>Lintel west face x 815.9 (0.6 m behind face B's plane: no edge shared with face B's columns), east face x 839.3,
        /// soffit y 129, top 560; ends = the notch walls z 115 / 275.</summary>
        public const double NgX0 = 815.9, NgX1 = 839.3, NgY0 = 129.0;
        /// <summary>Head beam (concrete) along the opening: x 813.8..816.2 (1.5 m proud of face B), y 124.8..129.4 (0.4 m into the
        /// lintel), z 114..276 (1 m into face B's mass past each notch wall).</summary>
        public const double HeadX0 = 813.8, HeadX1 = 816.2, HeadY0 = 124.8, HeadY1 = 129.4, HeadZ0 = 114.0, HeadZ1 = 276.0;
        /// <summary>Transfer ribs under the soffit along x, every 16 m in z, 1.2 m wide, 1.0 m deep (bottom y 128.0).</summary>
        public const double RibPitch = 16.0, RibW = 1.2, RibY0 = 128.0;
        /// <summary>Keystone block of the upper tube's east portal (concrete): x 813.5..818.0, z 186..204, y 106.0..129.6 (T4's crown
        /// envelope reaches 102.5 under it: 3.5 m).</summary>
        public const double KeyX0 = 813.5, KeyX1 = 818.0, KeyY0 = 106.0, KeyY1 = 129.6, KeyZ0 = 186.0, KeyZ1 = 204.0;

        // ------------------------------------------------------------------ 3. east block (the courtyard's east wall)
        /// <summary>West face x 914 (the garden terrace's east edge), east face the arc R 366 about S, z 115..275, y -360..560 (face B's
        /// level 0 reaches y -360 below the notch; its receded levels lie further down); the
        /// lower part (y &lt; 38) only for z 115..236 (the arcade x 914..990, z 236..360, 38 m, stands there).</summary>
        public const double EbX0 = 914.0, EbR = 366.0, EbZ0 = 115.0, EbZ1 = 275.0, EbZArcade = 236.0, EbArcadeTop = 38.0, EbY0 = -360.0;
        /// <summary>Plan sampling of the arc (m along z).</summary>
        public const double EbArcStep = 8.0;
        /// <summary>Ledges (concrete, 0.8 m proud, 0.6 m thick) on the west and south faces; heights clear of every lane portal.</summary>
        public static readonly double[] EbLedgeY = { 38.0, 104.0, 262.0, 360.0, 456.0 };
        public const double EbLedgeProud = 0.8, EbLedgeT = 0.6;

        // ------------------------------------------------------------------ 4. promenade lintel (the courtyard's north side)
        /// <summary>From the screen-wall slab (x 874; 0.5 m into it) to the east block's line x 914, z 280..286 (B8's crown envelope
        /// reaches z 276.7: 3.3 m clear), y 40..560. GE-t03: from the garden (KS15, the beat-5 mid still and look_up60) the koi
        /// showed through the promenade gap and over the 38 m arcade (sight lines at y 58-78 over z 275); the lintel closes the gap
        /// above y 40, the walker passes under it onto the promenade (KS16's sight lines to the wall cross z 286 below y 31).</summary>
        public const double PlX0 = 873.5, PlX1 = 914.0, PlZ0 = 280.0, PlZ1 = 286.0, PlY0 = 40.0;

        // ------------------------------------------------------------------ lane portals
        /// <summary>Lane tunnel opening 7 x 5 m about the lane (z, y); lit mouth 4 m; tunnels in the block 30 m deep.</summary>
        public const double PortalHalfW = 3.5, PortalHalfH = 2.5, PortalLit = 4.0, PortalBlockDepth = 30.0, PortalFrame = 0.35;

        /// <summary>Koi clearance: every koi part stays inside R 356 about S (KoiMath); the block keeps 10 m more.</summary>
        public const double KoiMaxR = 356.0;

        /// <summary>Crown clearance asserted by ReliefTests (m).</summary>
        public const double CrownClear = 3.0;

        public static double SX => NocturneConstants.SpireX;
        public static double SZ => NocturneConstants.SpireZ;

        /// <summary>x of the east block's arc face at z (R 366 about S, west side).</summary>
        public static double ArcX(double z)
        {
            double dz = z - SZ;
            return SX - Math.Sqrt(Math.Max(0.0, EbR * EbR - dz * dz));
        }

        /// <summary>The plan samples of the arc face for z in [z0, z1] (both ends included, about EbArcStep apart).</summary>
        public static List<double> ArcZs(double z0, double z1)
        {
            int n = Math.Max(1, (int)Math.Ceiling((z1 - z0) / EbArcStep - 1e-9));
            var l = new List<double>(n + 1);
            for (int i = 0; i <= n; i++) l.Add(z0 + (z1 - z0) * i / n);
            return l;
        }

        /// <summary>True when (x, y, z) lies inside the east block's volume.</summary>
        public static bool InEastBlock(double x, double y, double z)
        {
            if (z < EbZ0 || z > EbZ1 || y < EbY0 || y > EdgeDims.FaceTop || x < EbX0 || x > ArcX(z)) return false;
            return z <= EbZArcade || y >= EbArcadeTop;
        }

        /// <summary>The (y, z) of every boulevard lane (NocTraffic.BoulevardLanes order: east-bound, then west-bound).</summary>
        public static List<(double y, double z)> Lanes()
        {
            var e = new List<TrafficLane>(8); var w = new List<TrafficLane>(8);
            NocTraffic.BoulevardLanes(e, w);
            var l = new List<(double, double)>(e.Count + w.Count);
            foreach (var t in e) l.Add((t.ay, t.az));
            foreach (var t in w) l.Add((t.ay, t.az));
            return l;
        }
    }
}
