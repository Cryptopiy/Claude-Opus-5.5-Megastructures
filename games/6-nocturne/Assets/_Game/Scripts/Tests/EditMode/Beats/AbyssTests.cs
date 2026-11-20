// AbyssTests (AB, Phase 4 wave A): EditMode checks of the abyss content of beat 3 (MASTER_PROMPT 14.5.3, 14.7 H7, INT-01):
//   the eight traffic layers at the spec's (x, y), z -5 500..5 500, six lanes each, inside the rift at every z and clear of the
//   cross bridges and the tubes; the H7 ramp rule for every layer and cross-bridge lane (a vehicle with alpha in (0.02, 0.98) is in
//   haze, T < 0.2, from every key pose and from the corners of the walk's camera box, or behind a face plane / inside a portal);
//   no jump at the time wrap; the cross-bridge deck lamps stand on their decks between the portals; the Gulf faces carry light
//   content (galleries, atria, terrace lip lights) down to level 5 on both faces; KS10's drop share >= 40 % on the built world.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class AbyssTests
    {
        static void Report(string s) => Debug.Log("[ABYSS] " + s);

        [Test]
        public void Layers_SpecPlacementAndCounts()
        {
            double[] x = { 600, 380, 540, 350, 560, 470, 650, 420 }, y = { 300, 180, 30, -120, -520, -880, -1100, -1400 };
            Assert.AreEqual(8, AbyssTraffic.LayerX.Length);
            for (int k = 0; k < 8; k++) { Assert.AreEqual(x[k], AbyssTraffic.LayerX[k], 1e-9); Assert.AreEqual(y[k], AbyssTraffic.LayerY[k], 1e-9); }
            var lanes = AbyssTraffic.AllLayerLanes();
            Assert.AreEqual(8 * 2 * AbyssTraffic.LanesPerDirection, lanes.Count, "8 layers x 6 lanes");
            int north = 0, total = 0;
            var phis = new List<double>(); var vs = new List<double>();
            foreach (var l in lanes)
            {
                Assert.AreEqual(Math.Min(l.az, l.bz), -5500.0, 1e-9, l.name); Assert.AreEqual(Math.Max(l.az, l.bz), 5500.0, 1e-9, l.name);
                Assert.AreEqual(l.ax, l.bx, 1e-9, l.name + " runs along z");
                if (l.bz > l.az) north++;
                phis.Clear(); vs.Clear();
                NocTraffic.Vehicles(l, 1u, phis, vs);
                total += phis.Count;
                double L = l.Length;
                for (int i = 0; i < phis.Count; i++)
                    Assert.AreEqual(NocTraffic.Phase(phis[i], vs[i], L, 0.0), NocTraffic.Phase(phis[i], vs[i], L, NocturneConstants.TrafficTimeWrap), 1e-7, "no jump at the time wrap");
            }
            Assert.AreEqual(8 * AbyssTraffic.LanesPerDirection, north, "three north-bound lanes per layer");
            Report(FormattableString.Invariant($"layer vehicles {total} ({total / 8} per layer, 14.5.3 names 275)"));
            Assert.GreaterOrEqual(total / 8, 275, "at least the spec's 275 vehicles per layer");
        }

        [Test]
        public void Layers_InsideTheRiftAndClear()
        {
            foreach (var l in AbyssTraffic.AllLayerLanes())
            {
                for (double z = -5500; z <= 5500; z += 50)
                {
                    Assert.Greater(l.ax, EdgeDims.FrontX(0, l.ay, z) + 60.0, FormattableString.Invariant($"{l.name} 60 m clear of face A at z {z}"));
                    Assert.Less(l.ax, EdgeDims.FrontX(1, l.ay, z) - 60.0, FormattableString.Invariant($"{l.name} 60 m clear of face B at z {z}"));
                }
                // the cross bridges: deck top + lamp posts above, the truss below (14 m wide at z_i)
                for (int i = 0; i < EdgeDims.BridgeZ.Length; i++)
                {
                    double top = EdgeDims.BridgeY[i] + CrossBridge.LampPostH + 0.5, bot = EdgeDims.BridgeY[i] - EdgeDims.BridgeDeck - EdgeDims.BridgeTruss - 1.0;
                    Assert.IsTrue(l.ay > top + 15.0 || l.ay < bot - 15.0, FormattableString.Invariant($"{l.name} (y {l.ay}) 15 m clear of bridge {i} (y {bot:F0}..{top:F0})"));
                }
                // tubes along x: the upper (y 118, z 195), the lower (y -142, z 209), the deck tubes (y 2, z 183 / 207), r <= 5
                foreach (var t in new[] { (118.0, 195.0), (-142.0, 209.0), (2.0, 183.0), (2.0, 207.0) })
                    Assert.Greater(Math.Abs(l.ay - t.Item1), 15.0, l.name + " clear of the tube at y " + t.Item1);
                // the tongue's hull (keel to y -120 at the root, about -12 at x 350) and the bridge truss (10 m)
                if (l.ay < 0) Assert.Less(l.ay, -EdgeDims.HullDepth(Math.Max(EdgeDims.TongueX0, Math.Min(EdgeDims.TongueX1, l.ax))) - 15.0, l.name + " below the hull");
                if (l.ay > 0) Assert.Greater(l.ay, 16.0 + 10.0, l.name + " above the gate frames (16 m)");
            }
        }

        static IEnumerable<(string, double, double, double, double)> Eyes()
        {
            foreach (var k in NocturneConstants.KeyShots) yield return (k.tag, k.s, k.x, k.y, k.z);
            // the corners and edge midpoints of the walk's camera box (14.4; CityCamX/Z), at the eye height, with the s of the nearest beat
            double[] xs = { NocturneConstants.CityCamX0, 0.5 * (NocturneConstants.CityCamX0 + NocturneConstants.CityCamX1), NocturneConstants.CityCamX1 };
            double[] zs = { NocturneConstants.CityCamZ0, NocturneConstants.CityCamZ1 };
            foreach (double xx in xs) foreach (double zz in zs) yield return ("box", 480.0, xx, 1.7, zz);
        }

        static void CheckRamps(List<TrafficLane> lanes, Func<TrafficLane, double, double, double, bool> portal)
        {
            var st = new NocAtmoState();
            int checkedPts = 0;
            foreach (var e in Eyes())
            {
                NocFogMath.StateAt(st, e.Item2, 0.0, 1.0, true);
                st.CamX = e.Item3; st.CamY = e.Item4; st.CamZ = e.Item5;
                foreach (var l in lanes)
                    for (double p = 0.0005; p < 1.0; p += 0.0005)
                    {
                        double a = NocTraffic.Alpha(p, l.ramp);
                        if (a <= 0.02 || a >= 0.98) continue;
                        double x = l.ax + (l.bx - l.ax) * p, y = l.ay + (l.by - l.ay) * p, z = l.az + (l.bz - l.az) * p;
                        checkedPts++;
                        if (portal(l, x, y, z)) continue;
                        double T = NocFogMath.FogT(st, x, y, z);
                        Assert.Less(T, 0.2, FormattableString.Invariant($"{l.name} ramp at ({x:F0}, {y:F0}, {z:F0}) visible from {e.Item1}: T {T:F3}"));
                    }
            }
            Report(FormattableString.Invariant($"ramp points checked {checkedPts}"));
        }

        [Test]
        public void Layers_RampsInHaze_H7()
        {
            CheckRamps(AbyssTraffic.AllLayerLanes(), (l, x, y, z) => false);
        }

        [Test]
        public void BridgeLanes_RampsBehindTheFaces_H7()
        {
            var lanes = AbyssTraffic.AllBridgeLanes();
            Assert.AreEqual(24, lanes.Count, "6 bridges x 4 lanes");
            // the ramp of a bridge lane lies behind the face planes (inside the portal: the glow wall hides it)
            CheckRamps(lanes, (l, x, y, z) =>
            {
                double yDeck = y - AbyssTraffic.BridgeLaneY;
                return x <= EdgeDims.FrontX(0, yDeck, z) + 1e-6 || x >= EdgeDims.FrontX(1, yDeck, z) - 1e-6;
            });
            foreach (var l in lanes)
            {
                double alphaAtFace = NocTraffic.Alpha(Math.Abs((EdgeDims.FrontX(0, l.ay - AbyssTraffic.BridgeLaneY, l.az) - l.ax) / (l.bx - l.ax)), l.ramp);
                Assert.Greater(alphaAtFace, 0.98, l.name + ": fully opaque where it leaves face A");
            }
        }

        [Test]
        public void CrossBridges_LampsOnTheirDecks()
        {
            var parts = new List<EdgePart>();
            CrossBridge.Build(parts);
            Assert.Greater(CrossBridge.Lamps.Count, 300, "lamp rows on six bridges");
            foreach (var p in CrossBridge.Lamps)
            {
                int i = -1;
                for (int k = 0; k < EdgeDims.BridgeZ.Length; k++) if (Math.Abs(p.z - EdgeDims.BridgeZ[k]) < 8.0) i = k;
                Assert.GreaterOrEqual(i, 0, "a lamp belongs to a bridge: " + p);
                Assert.LessOrEqual(Math.Abs(p.z - EdgeDims.BridgeZ[i]), 0.5 * EdgeDims.BridgeWidth, "over the deck");
                Assert.AreEqual(EdgeDims.BridgeY[i] + CrossBridge.LampPostH + 0.15, p.y, 0.01, "on its 6 m post");
                Assert.Greater(p.x, EdgeDims.FrontX(0, EdgeDims.BridgeY[i], p.z) + CrossBridge.PortalDepth, "between the portals");
                Assert.Less(p.x, EdgeDims.FrontX(1, EdgeDims.BridgeY[i], p.z) - CrossBridge.PortalDepth, "between the portals");
            }
            Report(FormattableString.Invariant($"bridge lamps {CrossBridge.Lamps.Count}"));
        }

        /// <summary>Shares (sky, deck, rail, near structure, drop) of a pose on a scene, the VisibilityAudit.DropShares rule (a hit within
        /// 60 m horizontally is deck / rail / near structure by its renderer path, farther hits and misses below the horizon are the
        /// drop), on a gx x gy grid at 16:9; near = counts of the near-structure labels.</summary>
        public static double[] Shares(IVisRayScene scene, D3 eye, double yaw, double pitch, double fov, int gx, int gy, Dictionary<string, int> near)
        {
            int sky = 0, deck = 0, rail = 0, nr = 0, drop = 0;
            QaProjection.Basis(yaw, pitch, out D3 f, out D3 r, out D3 u);
            double tv = Math.Tan(fov * 0.5 * NocMath.Deg), asp = 16.0 / 9.0;
            for (int j = 0; j < gy; j++)
                for (int i = 0; i < gx; i++)
                {
                    double nx = -1.0 + (2.0 * i + 1.0) / gx, ny = -1.0 + (2.0 * j + 1.0) / gy;
                    D3 d = (f + r * (nx * tv * asp) + u * (ny * tv)).Normalized;
                    if (d.y > 0.0) { sky++; continue; }
                    if (scene.Raycast(eye, d, 20000.0, out double t, out VisClass c, out string lab))
                    {
                        D3 h = eye + d * t;
                        double hd = Math.Sqrt((h.x - eye.x) * (h.x - eye.x) + (h.z - eye.z) * (h.z - eye.z));
                        if (c == VisClass.Built && hd < VisibilityAudit.NearStructure)
                        {
                            switch (VisibilityAudit.BuiltClass(lab))
                            {
                                case VisClass.Deck: deck++; break;
                                case VisClass.Rail: rail++; break;
                                default: nr++; if (near != null) { near.TryGetValue(lab, out int q); near[lab] = q + 1; } break;
                            }
                            continue;
                        }
                    }
                    drop++;
                }
            double n = gx * gy;
            return new[] { sky / n, deck / n, rail / n, nr / n, drop / n };
        }

        /// <summary>Tool (run explicitly): the KS10 composition on the built world for a grid of gaze poses at s 548 (D-032 1: the drop
        /// share >= 40 % by composition). Writes QA/reports/AB_ks10_search.md.</summary>
        [Test, Explicit]
        public void KS10_CompositionSearch()
        {
            var built = VisibilityWorld.Build(out VisibilityWorld.Info info);
            Report("world: " + info);
            var eye = new D3(363.3, 1.7, 195.0);
            var sb = new System.Text.StringBuilder("# AB KS10 composition search (built world, s 548, eye (363.3, 1.7, 195), vFOV 68, 128 x 72 rays)\n\n| yaw | pitch | sky | deck | rail | near | drop | near labels |\n|---|---|---|---|---|---|---|---|\n");
            foreach (double yaw in new[] { 10.0, 5.0, 0.0, -5.0, -10.0, -15.0, -20.0, -25.0, -30.0 })
                foreach (double pitch in new[] { -24.0, -26.0, -28.0, -30.0, -32.0, -34.0 })
                {
                    var near = new Dictionary<string, int>();
                    var sh = Shares(built, eye, yaw, pitch, 68.0, 128, 72, near);
                    var top = new List<KeyValuePair<string, int>>(near);
                    top.Sort((a, b) => b.Value.CompareTo(a.Value));
                    string labs = string.Join(", ", top.ConvertAll(kv => kv.Key + " x" + kv.Value).GetRange(0, Math.Min(3, top.Count)));
                    sb.Append(FormattableString.Invariant($"| {yaw} | {pitch} | {100 * sh[0]:F1} | {100 * sh[1]:F1} | {100 * sh[2]:F1} | {100 * sh[3]:F1} | {100 * sh[4]:F1} | {labs} |")).Append('\n');
                }
            string path = System.IO.Path.Combine(System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..")), "QA", "reports", "AB_ks10_search.md");
            System.IO.File.WriteAllText(path, sb.ToString());
            Report("KS10 search written to " + path);
        }

        /// <summary>14.5.3 Acceptance (2) / D-032 1: KS10's drop share on the built world at the key-shot pose (the VisibilityAudit
        /// DropShares rule, 128 x 72 rays) is at least 40 %; the pier's glass edge (EdgeDims.Ledge*) is the composition that reaches it.</summary>
        [Test]
        public void KS10_DropShare_AtLeast40_OnTheBuiltWorld()
        {
            var built = VisibilityWorld.Build(out VisibilityWorld.Info info);
            KeyShot k = default;
            foreach (var q in NocturneConstants.KeyShots) if (q.tag.StartsWith("KS10", StringComparison.Ordinal)) k = q;
            var near = new Dictionary<string, int>();
            var sh = Shares(built, new D3(k.x, k.y, k.z), k.yaw, k.pitch, k.fov, 128, 72, near);
            var top = new List<KeyValuePair<string, int>>(near);
            top.Sort((a, b) => b.Value.CompareTo(a.Value));
            string labs = string.Join(", ", top.ConvertAll(kv => kv.Key + " x" + kv.Value).GetRange(0, Math.Min(4, top.Count)));
            Report(FormattableString.Invariant($"KS10 (yaw {k.yaw}, pitch {k.pitch}, vFOV {k.fov}) sky / deck / rail / near / drop {100 * sh[0]:F1} / {100 * sh[1]:F1} / {100 * sh[2]:F1} / {100 * sh[3]:F1} / {100 * sh[4]:F1} %; near {labs}"));
            Assert.GreaterOrEqual(sh[4], 0.40, "KS10 drop share (14.5.3 Acceptance (2), D-032 1)");
        }

        [Test]
        public void GulfFaces_CarryLightDownTheRift()
        {
            ReliefBuilder.BuildParts(true);
            Assert.Greater(GulfRibBuilder.Galleries, 100, "galleries");
            Assert.Greater(GulfRibBuilder.Atria, 10, "atria");
            int deepA = 0, deepB = 0, deepLight = 0;
            foreach (var p in GulfRibBuilder.Pieces)
            {
                if (p.level >= 4) { if (p.face == 0) deepA++; else deepB++; }
                if (p.level >= 3 && (p.kind == 'g' || p.kind == 'a')) deepLight++;
            }
            Report(FormattableString.Invariant($"pods {GulfRibBuilder.Pods}, towers {GulfRibBuilder.Towers}, decks {GulfRibBuilder.Decks}, risers {GulfRibBuilder.Pipes}, galleries {GulfRibBuilder.Galleries}, atria {GulfRibBuilder.Atria}, edge runs {GulfRibBuilder.Edges}; pieces on levels 4-5: A {deepA}, B {deepB}; lights on levels 3-5 {deepLight}"));
            Assert.Greater(deepA, 50, "face A carries pieces below y -1 260");
            Assert.Greater(deepB, 50, "face B carries pieces below y -1 260");
            Assert.Greater(deepLight, 30, "galleries and atria below y -960");
        }
    }
}
