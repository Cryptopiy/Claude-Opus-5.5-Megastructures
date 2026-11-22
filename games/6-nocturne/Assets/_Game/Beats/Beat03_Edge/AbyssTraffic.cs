// AbyssTraffic (AB, Phase 4 wave A, INT-01): the eight H7 traffic layers of MASTER_PROMPT 14.5.3 and the lanes of the six cross-gulf
// bridges as rivers of vehicles (NocTraffic lanes, NocStreak kind 4), replacing the Phase-3 stand-ins (TrafficLayerLines: one white
// NocLine stroke per layer, the "laser beams" of KS09 / KS10 / KS12 / KS13).
//   Layers (x, y) = (300, -225), (720, -240), (640, -340), (360, -440), (560, -520), (470, -880), (650, -1 100), (420, -1 400)
//   (ARK D-043, the user's order: the 14.5.3 layers (540, 30), (380, 180) and (350, -120) passed 28 / 178 / 122 m from the walker on
//   the pier and the skybridge, and seen that close a vehicle's 0.12 s streak sweeps across the frame like a laser bolt; (600, 300)
//   lay inside the Ark's volume x 295-735, y 300-600. Every layer is now at least WalkClearance (200 m) from the walk of beats 3-4
//   (s 400-880: x 215.3-695.3, z 195, eye y 1.7) and from the skybridge (y -14..+16), all of them below the walker: the sky over the
//   rift belongs to the Ark, the abyss to the rivers. The four new ones (0-3) sit in pairs along the faces and step in toward the
//   axis as they go down - (300, -225) / (720, -240) along the walls just above the first haze veil (y -250), (640, -340) /
//   (360, -440) half-way in - so from the bridge the eight rivers read as a funnel of light converging into the deep, the y -520
//   subject river (x 560) and the deep three (x 470 / 650 / 420) at its centre),
//   z -5 500..5 500 (T at the ends <= 0.02 from every walk pose, 14.7 H7). Each layer is a sky road of six lanes: three north-bound
//   (+z) east of the layer line and three south-bound west of it (right-hand traffic seen from above), 5 m apart, 10 m median; mean
//   spacing 32 m per lane (+-30 % jitter, phases phi_i = (i + jitter) / N), speeds per layer 17-26 m/s (+-3 %, quantised: no
//   jump at the time wrap), alpha ramps e 0.06 at both ends (T < 0.2 there from every key pose; AbyssTests checks it).
//   (r02: 4 lanes at 45 m read as two dotted lines in AB_r01 KS10, not a river: 6 lanes at 32 m are 2.1 x denser.)
//   Every vehicle is a lamp pair 1.5 m apart (0.3 m lamps): NocStreak kind 4 shows its cool-white heads to a camera in front of it
//   and its red-amber tails to a camera behind it, so a layer reads as tails one way and heads the other from every view, and the
//   pair resolves into two lamps from 3 px of separation (within ~300 m at 1080p).
//   Density: 14.5.3 names 275 vehicles per layer (2 200); a single file at 40 m reads as a dotted line, not a river, so a layer
//   carries 6 x 344 = 2 064 (16 512 for the eight); they are 2-triangle instanced quads (33 k triangles, about 0.1 ms; proposed
//   decision in docs/logs/AB.md).
//   Cross bridges (14.5.3 "cross-bridge lights", INT-01): four lanes per deck (two per direction, 3.5 m apart, lamps 0.9 m above the
//   deck top), 30 m mean spacing, 14 m/s; each lane runs from inside face A to inside face B (ramp e 0.03 of the lane length, the
//   whole ramp behind the face planes, where the lit portal of CrossBridge closes the deck end).
// Built once at load under BeatRoot(2) (always active, 14.6); the batches are sealed and submitted per camera (NocCameraBatches);
// no per-frame work on the CPU (the shader advances p = frac(phi + v t / L) from _NocTraffic.x).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class AbyssTraffic
    {
        // ------------------------------------------------------------------ layers (14.5.3)
        public static readonly double[] LayerX = { 300.0, 720.0, 640.0, 360.0, 560.0, 470.0, 650.0, 420.0 };
        public static readonly double[] LayerY = { -225.0, -240.0, -340.0, -440.0, -520.0, -880.0, -1100.0, -1400.0 };
        /// <summary>Layer speeds (m/s), one per layer (aerial roads run faster than the boulevard's 14 m/s).</summary>
        public static readonly double[] LayerSpeed = { 24.0, 21.0, 18.0, 17.0, 20.0, 22.0, 26.0, 23.0 };
        public const double LayerZ0 = -5500.0, LayerZ1 = 5500.0;
        public const int LanesPerDirection = 3;
        public const double LaneGap = 5.0, MedianGap = 10.0, LayerSpacing = 24.0;   // r04: 32 -> 24 m (AB_r03 KS10 / KS12: the rivers read as dotted lines)
        public const double LayerRamp = NocturneConstants.TrafficRampLayer;

        // ------------------------------------------------------------------ ARK (D-043): clearances
        /// <summary>No vehicle closer than this (m) to the walk of beats 3-4 or to the skybridge (ARK.md, the user's "lasers").</summary>
        public const double WalkClearance = 200.0;
        /// <summary>The walk of beats 3-4: s 400-880 runs straight along z 195 from x 215.3 to 695.3 (x = s - 184.7), eye y 1.7, the
        /// camera within +-2.5 m of the axis (the 5 m deck).</summary>
        public const double WalkS0 = 400.0, WalkS1 = 880.0, WalkX0 = 215.3, WalkX1 = 695.3, WalkZ = 195.0, WalkEyeY = 1.7, WalkHalfW = 2.5;
        /// <summary>The skybridge as a box (gate frames 16 m tall, the pier bottoms at -14 m, gates A..B, z 175..215).</summary>
        public const double SkyX0 = 371.7, SkyX1 = 695.3, SkyY0 = -14.0, SkyY1 = 16.0, SkyZ0 = 175.0, SkyZ1 = 215.0;
        /// <summary>The Ark's volume (ARK.md: the whole rift length): no layer inside it.</summary>
        public const double ArkX0 = 295.0, ArkX1 = 735.0, ArkY0 = 300.0, ArkY1 = 600.0;

        /// <summary>The boulevard lanes' walk clearance (NocStreak's NocWalkClear, kinds 0 / 1): no vehicle within BoulevardClear0 m
        /// of a camera over the rift, full at BoulevardClear1; the weight eases in over x 191.3-215.3 (s 376-400) and out over
        /// x 695.3-719.3 (s 880-904), and off the z 195 axis over 60-90 m.</summary>
        public const double BoulevardClear0 = 300.0, BoulevardClear1 = 400.0;

        /// <summary>C# twin of NocStreak's NocWalkClear: the factor on a boulevard vehicle at distance d from a camera at (cx, cz).</summary>
        public static double BoulevardClear(double cx, double cz, double d)
        {
            double w = NocMath.Smoothstep(191.3, 215.3, cx) * (1.0 - NocMath.Smoothstep(695.3, 719.3, cx)) * (1.0 - NocMath.Smoothstep(60.0, 90.0, Math.Abs(cz - 195.0)));
            return 1.0 - w * (1.0 - NocMath.Smoothstep(BoulevardClear0, BoulevardClear1, d));
        }

        /// <summary>Distance (m) from a point to an axis-aligned box (0 inside).</summary>
        public static double BoxDistance(double x, double y, double z, double x0, double x1, double y0, double y1, double z0, double z1)
        {
            double dx = Math.Max(0.0, Math.Max(x0 - x, x - x1)), dy = Math.Max(0.0, Math.Max(y0 - y, y - y1)), dz = Math.Max(0.0, Math.Max(z0 - z, z - z1));
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>Distance (m) from a point to the walk of beats 3-4 (the eye's box: x 215.3-695.3, y 1.7, z 195 +- 2.5).</summary>
        public static double WalkDistance(double x, double y, double z)
            => BoxDistance(x, y, z, WalkX0, WalkX1, WalkEyeY, WalkEyeY, WalkZ - WalkHalfW, WalkZ + WalkHalfW);

        /// <summary>The smallest distance (m) of a straight lane A..B to the walk and to the skybridge box (sampled every 1 m or less;
        /// a distance changes by at most 1 m per metre along the lane, so the samples are within 0.5 m of the true minimum).</summary>
        public static double LaneClearance(TrafficLane l)
        {
            double L = l.Length, best = double.MaxValue;
            int n = Math.Max(2, (int)Math.Ceiling(L));
            for (int i = 0; i <= n; i++)
            {
                double p = (double)i / n;
                double x = l.ax + (l.bx - l.ax) * p, y = l.ay + (l.by - l.ay) * p, z = l.az + (l.bz - l.az) * p;
                best = Math.Min(best, Math.Min(WalkDistance(x, y, z), BoxDistance(x, y, z, SkyX0, SkyX1, SkyY0, SkyY1, SkyZ0, SkyZ1)));
            }
            return best;
        }

        // ------------------------------------------------------------------ cross-bridge lanes
        public const double BridgeLaneZ0 = 1.75, BridgeLaneDz = 3.5, BridgeLaneY = 0.9, BridgeSpacing = 30.0, BridgeSpeed = 14.0, BridgeRamp = 0.03;

        // ------------------------------------------------------------------ vehicle lights (NocStreak kind 4)
        /// <summary>Lamp pair separation (m) and lamp width (m); head and tail radiance gains (x SignWhite / x the tail colour).</summary>
        public const double PairSep = 1.5, LampWidth = 0.3;
        public const float HeadGain = 10.0f, TailGain = 6.0f;   // r03: 6 / 4.4 -> 8 / 5, r04: 10 / 6 (D-032 5: the heads are the abyss's highlights)
        /// <summary>Layer glow (NocStreak kind 6, the aggregate in-scatter of a layer's lamps in the haze): Gaussian sigma (m), the
        /// strip's segments, peak radiance gain x the glow colour (lerp(WinNeutral, SodiumAmber, GlowAmber)).
        /// r03 (AB_r02 KS08 / KS09 / KS12: sigma 14 m at gain 0.12 drew the layers as hard searchlight beams fanning from the vanishing
        /// point, +35-60 codes over the haze in KS10): sigma 22 m, gain 0.06, and a per-layer intensity that follows the haze density
        /// (GlowIntensity: 0.15 for the layers in the clear air above the deck, rising to 1 at y -600 and below, where the abyss
        /// haze is dense and the glow is all that is left of a layer).</summary>
        public const double GlowSigma = 22.0, GlowAmber = 0.35;
        public const int GlowSegments = 128;
        public const float GlowGain = 0.06f;
        public const double GlowAbove = 0.15, GlowFullDepth = -600.0;

        /// <summary>The glow intensity of layer k (x GlowGain): GlowAbove at y >= 0, linear to 1 at GlowFullDepth.</summary>
        public static double GlowIntensity(int k)
        {
            double t = Math.Max(0.0, Math.Min(1.0, LayerY[k] / GlowFullDepth));
            return GlowAbove + (1.0 - GlowAbove) * t;
        }
        public const float BridgeHeadGain = 6.0f, BridgeTailGain = 4.0f;   // r06: 2.4 / 1.8 -> 6 / 4 (the deck traffic read dimmer than the deck lamps)

        /// <summary>The lanes of layer k: north-bound first (A at z0), then south-bound (A at z1).</summary>
        public static void LayerLanes(int k, List<TrafficLane> lanes)
        {
            double x = LayerX[k], y = LayerY[k];
            for (int dirn = 0; dirn < 2; dirn++)
                for (int j = 0; j < LanesPerDirection; j++)
                {
                    double off = 0.5 * MedianGap + LaneGap * j;
                    var l = new TrafficLane
                    {
                        name = FormattableString.Invariant($"layer{k}_{(dirn == 0 ? "n" : "s")}{j}"),
                        ax = dirn == 0 ? x + off : x - off, bx = dirn == 0 ? x + off : x - off,
                        ay = y, by = y,
                        az = dirn == 0 ? LayerZ0 : LayerZ1, bz = dirn == 0 ? LayerZ1 : LayerZ0,
                        speed = LayerSpeed[k], spacing = LayerSpacing, ramp = LayerRamp,
                    };
                    lanes.Add(l);
                }
        }

        /// <summary>The four lanes of cross bridge i (east-bound on the south half, west-bound on the north half), running from
        /// inside face A to inside face B so the whole alpha ramp lies behind the face planes.</summary>
        public static void BridgeLanes(int i, List<TrafficLane> lanes)
        {
            double z = EdgeDims.BridgeZ[i], y = EdgeDims.BridgeY[i] + BridgeLaneY;
            for (int dirn = 0; dirn < 2; dirn++)
                for (int j = 0; j < 2; j++)
                {
                    double zz = dirn == 0 ? z - BridgeLaneZ0 - BridgeLaneDz * j : z + BridgeLaneZ0 + BridgeLaneDz * j;
                    // r02 (AB-t01: bridge 3's ramp stood 3 m in front of face A): the face planes at the lane's own z (the faces flare
                    // with |z - 195|, up to 0.65 m of x per m of z); the portal's glow wall (at the bridge axis) hides the rest
                    double xa = EdgeDims.FrontX(0, EdgeDims.BridgeY[i], zz), xb = EdgeDims.FrontX(1, EdgeDims.BridgeY[i], zz);
                    double span = xb - xa, r = BridgeRamp * span / (1.0 - 2.0 * BridgeRamp);
                    var l = new TrafficLane
                    {
                        name = FormattableString.Invariant($"bridge{i}_{(dirn == 0 ? "e" : "w")}{j}"),
                        ax = dirn == 0 ? xa - r : xb + r, bx = dirn == 0 ? xb + r : xa - r,
                        ay = y, by = y, az = zz, bz = zz,
                        speed = BridgeSpeed, spacing = BridgeSpacing, ramp = BridgeRamp,
                    };
                    lanes.Add(l);
                }
        }

        /// <summary>ARK (D-043): fliers on the air crossings (CrossBridge.AirZ / AirY): faster than the deck traffic.</summary>
        public const double AirSpeed = 19.0, AirSpacing = 18.0;   // a03: 26 -> 18 (KS11 p99 116.7)

        /// <summary>The four lanes of air crossing i (CrossBridge.AirZ / AirY): the bridge-lane layout (east-bound south of the
        /// crossing's axis, west-bound north of it), lamps at AirY, each lane from inside face A to inside face B with its whole alpha
        /// ramp behind the face planes (the portals' glow walls close the openings).</summary>
        public static void AirLanes(int i, List<TrafficLane> lanes)
        {
            double z = CrossBridge.AirZ[i], y = CrossBridge.AirY[i], yb = CrossBridge.AirBase(i);
            for (int dirn = 0; dirn < 2; dirn++)
                for (int j = 0; j < 2; j++)
                {
                    double zz = dirn == 0 ? z - BridgeLaneZ0 - BridgeLaneDz * j : z + BridgeLaneZ0 + BridgeLaneDz * j;
                    double xa = EdgeDims.FrontX(0, yb, zz), xb = EdgeDims.FrontX(1, yb, zz);
                    double span = xb - xa, r = BridgeRamp * span / (1.0 - 2.0 * BridgeRamp);
                    lanes.Add(new TrafficLane
                    {
                        name = FormattableString.Invariant($"air{i}_{(dirn == 0 ? "e" : "w")}{j}"),
                        ax = dirn == 0 ? xa - r : xb + r, bx = dirn == 0 ? xb + r : xa - r,
                        ay = y, by = y, az = zz, bz = zz,
                        speed = AirSpeed, spacing = AirSpacing, ramp = BridgeRamp,
                    });
                }
        }

        /// <summary>Every air-crossing lane.</summary>
        public static List<TrafficLane> AllAirLanes()
        {
            var l = new List<TrafficLane>(CrossBridge.AirZ.Length * 4);
            for (int i = 0; i < CrossBridge.AirZ.Length; i++) AirLanes(i, l);
            return l;
        }

        /// <summary>Every layer lane (8 layers x 6 lanes).</summary>
        public static List<TrafficLane> AllLayerLanes()
        {
            var l = new List<TrafficLane>(LayerX.Length * 2 * LanesPerDirection);
            for (int k = 0; k < LayerX.Length; k++) LayerLanes(k, l);
            return l;
        }

        /// <summary>Every cross-bridge lane (6 bridges x 4 lanes).</summary>
        public static List<TrafficLane> AllBridgeLanes()
        {
            var l = new List<TrafficLane>(EdgeDims.BridgeZ.Length * 4);
            for (int i = 0; i < EdgeDims.BridgeZ.Length; i++) BridgeLanes(i, l);
            return l;
        }

        /// <summary>Counts of the last Build (logged).</summary>
        public static int LayerVehicles, BridgeVehicles, AirVehicles;
        /// <summary>ARK: head / tail gains of the fliers (the layers' 10 / 6: the crossing carries the eye-level highlights).</summary>
        public const float AirHeadGain = 14.0f, AirTailGain = 6.0f;   // a02: 10 -> 12, a03: 14

        /// <summary>Builds and registers the two batches (layers, bridges); lib may be null (no scene output).</summary>
        public static void Build(MaterialLibrary lib)
        {
            LayerVehicles = 0; BridgeVehicles = 0; AirVehicles = 0;
            if (lib == null || lib.StreakTail == null) return;
            Mesh quad = NocTraffic.LaneQuad();
            var layerMat = Variant(lib.StreakTail, HeadGain, TailGain, LayerRamp, "AbyssLayerVehicles");
            var bridgeMat = Variant(lib.StreakTail, BridgeHeadGain, BridgeTailGain, BridgeRamp, "AbyssBridgeVehicles");
            LayerVehicles = NocTraffic.Batch("AbyssLayers", AllLayerLanes(), layerMat, quad, 0x4C415952u).Count;
            BridgeVehicles = NocTraffic.Batch("AbyssBridges", AllBridgeLanes(), bridgeMat, quad, 0x42524447u).Count;
            var airMat = Variant(lib.StreakTail, AirHeadGain, AirTailGain, BridgeRamp, "AbyssAirVehicles");
            AirVehicles = NocTraffic.Batch("AbyssAir", AllAirLanes(), airMat, quad, 0x41495221u).Count;
            // the glow of every layer: one strip along the layer's centre line
            var glowMat = new Material(lib.StreakTail) { name = "AbyssLayerGlow" };
            Color g = Color.Lerp(NocturnePalette.Linear(NocPal.WinNeutral), NocturnePalette.Linear(NocPal.SodiumAmber), (float)GlowAmber);
            glowMat.SetFloat(s_kind, 6f);
            glowMat.SetFloat(s_gain, GlowGain);
            glowMat.SetFloat(s_ramp, (float)LayerRamp);
            glowMat.SetVector(s_tint, new Vector4(g.r, g.g, g.b, 1f));
            glowMat.enableInstancing = true;
            var glow = new InstancedBatch("AbyssLayerGlow", NocTraffic.RibbonMesh(GlowSegments), glowMat, chunkSize: 100000f);
            for (int k = 0; k < LayerX.Length; k++)
            {
                var centre = new TrafficLane { ax = LayerX[k], bx = LayerX[k], ay = LayerY[k], by = LayerY[k], az = LayerZ0, bz = LayerZ1 };
                glow.Add(NocTraffic.RibbonMatrix(centre, GlowIntensity(k), GlowSigma));
            }
            if (!NocTraffic.Off) NocCameraBatches.Register(glow);   // QA A/B (NOC_TRAFFIC_OFF)
        }

        static readonly int s_kind = Shader.PropertyToID("_Kind"), s_gain = Shader.PropertyToID("_Gain"), s_ramp = Shader.PropertyToID("_Ramp");
        static readonly int s_size = Shader.PropertyToID("_Size"), s_tint = Shader.PropertyToID("_Tint");

        /// <summary>A kind-4 variant of the library's streak row (agreement 9: derived once at load).</summary>
        static Material Variant(Material baseMat, float head, float tail, double ramp, string name)
        {
            var m = new Material(baseMat) { name = name };
            m.SetFloat(s_kind, 4f);
            m.SetFloat(s_gain, head);
            m.SetFloat(s_ramp, (float)ramp);
            m.SetFloat(s_size, (float)LampWidth);
            m.SetVector(s_tint, new Vector4(tail, 0f, (float)PairSep, (float)LampWidth));
            m.enableInstancing = true;
            return m;
        }
    }
}
