// AbyssTraffic (AB, Phase 4 wave A, INT-01): the eight H7 traffic layers of MASTER_PROMPT 14.5.3 and the lanes of the six cross-gulf
// bridges as rivers of vehicles (NocTraffic lanes, NocStreak kind 4), replacing the Phase-3 stand-ins (TrafficLayerLines: one white
// NocLine stroke per layer, the "laser beams" of KS09 / KS10 / KS12 / KS13).
//   Layers (x, y) = (600, 300), (380, 180), (540, 30), (350, -120), (560, -520), (470, -880), (650, -1 100), (420, -1 400),
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
        public static readonly double[] LayerX = { 600.0, 380.0, 540.0, 350.0, 560.0, 470.0, 650.0, 420.0 };
        public static readonly double[] LayerY = { 300.0, 180.0, 30.0, -120.0, -520.0, -880.0, -1100.0, -1400.0 };
        /// <summary>Layer speeds (m/s), one per layer (aerial roads run faster than the boulevard's 14 m/s).</summary>
        public static readonly double[] LayerSpeed = { 24.0, 21.0, 18.0, 17.0, 20.0, 22.0, 26.0, 23.0 };
        public const double LayerZ0 = -5500.0, LayerZ1 = 5500.0;
        public const int LanesPerDirection = 3;
        public const double LaneGap = 5.0, MedianGap = 10.0, LayerSpacing = 32.0;
        public const double LayerRamp = NocturneConstants.TrafficRampLayer;

        // ------------------------------------------------------------------ cross-bridge lanes
        public const double BridgeLaneZ0 = 1.75, BridgeLaneDz = 3.5, BridgeLaneY = 0.9, BridgeSpacing = 30.0, BridgeSpeed = 14.0, BridgeRamp = 0.03;

        // ------------------------------------------------------------------ vehicle lights (NocStreak kind 4)
        /// <summary>Lamp pair separation (m) and lamp width (m); head and tail radiance gains (x SignWhite / x the tail colour).</summary>
        public const double PairSep = 1.5, LampWidth = 0.3;
        public const float HeadGain = 6.0f, TailGain = 4.4f;
        /// <summary>Layer glow (NocStreak kind 6, the aggregate in-scatter of a layer's lamps in the haze): Gaussian sigma (m), the
        /// strip's segments, peak radiance gain x the glow colour (lerp(WinNeutral, SodiumAmber, GlowAmber)).</summary>
        public const double GlowSigma = 14.0, GlowAmber = 0.35;
        public const int GlowSegments = 128;
        public const float GlowGain = 0.12f;
        public const float BridgeHeadGain = 2.4f, BridgeTailGain = 1.8f;

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
        public static int LayerVehicles, BridgeVehicles;

        /// <summary>Builds and registers the two batches (layers, bridges); lib may be null (no scene output).</summary>
        public static void Build(MaterialLibrary lib)
        {
            LayerVehicles = 0; BridgeVehicles = 0;
            if (lib == null || lib.StreakTail == null) return;
            Mesh quad = NocTraffic.LaneQuad();
            var layerMat = Variant(lib.StreakTail, HeadGain, TailGain, LayerRamp, "AbyssLayerVehicles");
            var bridgeMat = Variant(lib.StreakTail, BridgeHeadGain, BridgeTailGain, BridgeRamp, "AbyssBridgeVehicles");
            LayerVehicles = NocTraffic.Batch("AbyssLayers", AllLayerLanes(), layerMat, quad, 0x4C415952u).Count;
            BridgeVehicles = NocTraffic.Batch("AbyssBridges", AllBridgeLanes(), bridgeMat, quad, 0x42524447u).Count;
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
                glow.Add(NocTraffic.RibbonMatrix(centre, 1.0, GlowSigma));
            }
            NocCameraBatches.Register(glow);
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
