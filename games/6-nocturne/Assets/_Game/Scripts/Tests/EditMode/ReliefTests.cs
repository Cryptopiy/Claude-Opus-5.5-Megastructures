// ReliefTests: EditMode checks of GE's builders (TEXTURE_PLAN 5 "Tests"; D-029 gate structures, GateDims):
//   every part passes MeshValidation; the relief budget (400 k triangles, 40 draws: TEXTURE_PLAN 9);
//   crown clearance >= 3 m between FO's crown envelopes (GardenLeafCards.Envelope, 1.10 x the ellipsoids) and the notch gate's
//   soffit / ribs / head beam / keystone and every lane over the garden (GB-06: no lane through a crown);
//   every lane: its alpha ramp starts at face B and lies inside structure (lintel, courtyard, east block: alpha past the block's arc
//   face below 0.05), 6 m or more above the canyon bridge's parapet, the four raised lanes above 140;
//   the upper tube's two ends lie inside the canyon bridge's body and the keystone (GB-02);
//   the east block keeps the koi's 356 m radius plus 10 m, closes the notch's east end (x 914) over z 115..275 and reaches the plaza
//   disc (GB-14), and stands clear of the walk lane.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class ReliefTests
    {
        static List<ReliefPart> s_parts;
        static List<ReliefPart> Parts() => s_parts ?? (s_parts = ReliefBuilder.BuildParts(true));

        static void Report(string s) => Debug.Log("[RELIEF] " + s);

        [Test]
        public void PartsValidate_AndBudget()
        {
            long tris = 0; int draws = 0;
            foreach (var p in Parts())
            {
                if (p.Mb.VertexCount == 0) continue;
                var r = MeshValidation.Validate(p.Mb, p.Name);
                Assert.IsTrue(r.Ok, r.Describe());
                tris += p.Mb.TriangleCount; draws++;
            }
            Report(FormattableString.Invariant($"{draws} parts, {tris} triangles"));
            Assert.LessOrEqual(tris, 400000, "GE budget 400 k triangles (TEXTURE_PLAN 9)");
            Assert.LessOrEqual(draws, 40, "GE budget 40 draws");
        }

        /// <summary>Highest point of any crown envelope over the plan rectangle (x0..x1, z0..z1), 0.5 m grid.</summary>
        static double CrownTop(double x0, double x1, double z0, double z1, out string who)
        {
            double best = double.NegativeInfinity; who = "";
            foreach (var t in GardenDims.Trees)
            {
                GardenLeafCards.Envelope(t, out Vector3 c, out Vector3 r);
                if (x1 < c.x - r.x || x0 > c.x + r.x || z1 < c.z - r.z || z0 > c.z + r.z) continue;
                for (double x = x0; x <= x1 + 1e-9; x += 0.5)
                    for (double z = z0; z <= z1 + 1e-9; z += 0.5)
                    {
                        double a = ((x - c.x) / r.x) * ((x - c.x) / r.x) + ((z - c.z) / r.z) * ((z - c.z) / r.z);
                        if (a >= 1.0) continue;
                        double y = c.y + r.y * Math.Sqrt(1.0 - a);
                        if (y > best) { best = y; who = t.Name; }
                    }
            }
            return best;
        }

        [Test]
        public void CrownClearance_NotchGateAndLanes()
        {
            double clear = GateDims.CrownClear;
            var checks = new List<(string, double, double, double, double, double)>
            {
                ("lintel soffit ribs", GateDims.NgX0, GateDims.NgX1 + 0.3, EdgeDims.NotchZ0, EdgeDims.NotchZ1, GateDims.RibY0),
                ("head beam", GateDims.HeadX0, GateDims.HeadX1, EdgeDims.NotchZ0, EdgeDims.NotchZ1, GateDims.HeadY0),
                ("keystone", GateDims.KeyX0, GateDims.KeyX1, GateDims.KeyZ0, GateDims.KeyZ1, GateDims.KeyY0),
            };
            foreach (var (name, x0, x1, z0, z1, yb) in checks)
            {
                double top = CrownTop(x0, x1, z0, z1, out string who);
                Report(FormattableString.Invariant($"{name}: bottom y {yb:F1}, highest crown {top:F1} ({who}), clearance {yb - top:F1} m"));
                Assert.GreaterOrEqual(yb - top, clear, name + " clears the crowns by 3 m");
            }
            foreach (var (y, z) in GateDims.Lanes())
            {
                double top = CrownTop(EdgeDims.FaceXB - 15.0, GateDims.EbX0, z - 1.0, z + 1.0, out string who);
                Report(FormattableString.Invariant($"lane (z {z:F0}, y {y:F0}): highest crown under it {top:F1} ({who}), clearance {y - top:F1} m"));
                Assert.GreaterOrEqual(y - top, clear, FormattableString.Invariant($"lane z {z:F0} y {y:F0} clears the crowns by 3 m (GB-06)"));
            }
        }

        [Test]
        public void Lanes_EndInsideStructure_AboveTheCanyonBridge()
        {
            var e = new List<TrafficLane>(); var w = new List<TrafficLane>();
            NocTraffic.BoulevardLanes(e, w);
            var all = new List<TrafficLane>(e); all.AddRange(w);
            Assert.AreEqual(10, all.Count);
            double parapet = GateDims.CbY1 + 0.04 + GateDims.CbParapet;
            foreach (var l in all)
            {
                double y = l.ay, z = l.az;
                Assert.AreEqual(l.ay, l.by, 1e-9); Assert.AreEqual(l.az, l.bz, 1e-9);
                Assert.GreaterOrEqual(y, 140.0 - 1e-9, l.name + ": every lane above the crowns and the canyon bridge");
                Assert.GreaterOrEqual(y - GateDims.PortalHalfH - parapet, 3.0, l.name + ": 3 m over the canyon bridge's parapet");
                Assert.GreaterOrEqual(y - GateDims.PortalHalfH - GateDims.NgY0, 3.0, l.name + ": its portal opening lies in the lintel above the soffit");
                // the east end of the lane (where its alpha ramp runs, east of face B)
                double xEnd = Math.Max(l.ax, l.bx), L = l.Length, xB = CityField.GulfXB(z);
                double pAt(double x) => (x - Math.Min(l.ax, l.bx)) / L;                    // phase along the lane from its west end
                // east-bound lanes start west (p = 0 at x -4 000), west-bound lanes start east (p = 0 at xEnd): the ramp at the east end
                bool eastBound = l.bx > l.ax;
                double AlphaAt(double x) => NocTraffic.Alpha(eastBound ? pAt(x) : 1.0 - pAt(x), l.ramp);
                Assert.Greater(AlphaAt(xB - 0.5), 0.999, l.name + ": full brightness up to face B (the ramp starts in the portal)");
                double xArc = GateDims.ArcX(z);
                // the lane ends inside the block (south lanes: the arc lies east of x 1 015.9) or is faded out where it leaves it
                double aOut = xEnd <= xArc ? 0.0 : AlphaAt(xArc);
                Report(FormattableString.Invariant($"{l.name} (z {z:F0}, y {y:F0}): ramp x {xB:F1}..{xEnd:F1}, alpha {AlphaAt(GateDims.NgX1):F2} at the lintel's east face, {AlphaAt(GateDims.EbX0):F2} at the block, {aOut:F3} at the arc face x {xArc:F1}"));
                Assert.Less(aOut, 0.05, l.name + ": past the block's arc face the vehicles are faded out");
                Assert.IsTrue(GateDims.InEastBlock(GateDims.EbX0 + 1.0, y, z), l.name + ": its tunnel enters the east block");
            }
        }

        [Test]
        public void GulfRibs_OnTheSectionGrid()
        {
            // TEXTURE_PLAN 5 "Relief edges lie on cell boundaries within 1 cm (C# twin)": FA's section cell (NocFacadeMath.CellOf)
            var sk = NocFacadeMath.SkinOf(0, 1);
            var c = NocFacadeMath.CellOf(sk, NocFacadeMath.StratumSection);
            Assert.AreEqual(NocFacadeConstants.FaSecColP, GulfRibBuilder.ColP, 1e-9, "column pitch = the section's structural column line (FaSecColP)");
            Assert.AreEqual(0.0, Frac(GulfRibBuilder.ColP / c.Pu), 1e-9, "a column line is a cell boundary (whole cells per column)");
            Assert.AreEqual(c.Pv, GulfRibBuilder.FloorH, 1e-9, "floor = the section cell's v pitch");
            Assert.AreEqual(c.bandH, GulfRibBuilder.BandH, 1e-9, "band = the section band");
            Assert.AreEqual(c.slab, GulfRibBuilder.SlabT, 1e-9, "a slab cantilever is the cell's slab band");
            Parts();
            Assert.Greater(GulfRibBuilder.Pieces.Count, 300, "pieces placed");
            int perFace0 = 0, perFace1 = 0;
            foreach (var p in GulfRibBuilder.Pieces)
            {
                if (p.face == 0) perFace0++; else perFace1++;
                Assert.AreEqual(0.0, Frac(p.u0 / c.Pu), 0.01 / c.Pu, "u0 on a column line");
                Assert.AreEqual(0.0, Frac(p.u1 / c.Pu), 0.01 / c.Pu, "u1 on a column line");
                if (p.kind == 's') Assert.AreEqual(0.0, Frac(p.y0 / c.Pv), 0.01 / c.Pv, "slab on a floor line");
                else { Assert.AreEqual(0.0, Frac(p.y0 / c.Pv), 0.01 / c.Pv, "riser / tower from a floor line"); }
            }
            Report(FormattableString.Invariant($"Gulf ribs: {GulfRibBuilder.Pods} pods, {GulfRibBuilder.Towers} towers, {GulfRibBuilder.Decks} decks, {GulfRibBuilder.Pipes} riser bundles, {GulfRibBuilder.Edges} terrace edge runs; face A {perFace0}, face B {perFace1} pieces"));
        }

        static double Frac(double v) { double f = v - Math.Round(v); return Math.Abs(f); }

        [Test]
        public void BoulevardRelief_OnFAsCells()
        {
            // TEXTURE_PLAN 5 item 1: balconies (S4), fins (S0), AC units on whole window bays of the hero slabs' street faces,
            // within 150 m of the walk, recomputed here from FA's cell API (NocFacadeMath) for every element
            Parts();
            Assert.Greater(BoulevardRelief.Faces, 10, "the slab street faces");
            Assert.Greater(BoulevardRelief.Elements.Count, 50, "relief elements");
            foreach (var e in BoulevardRelief.Elements)
            {
                var face = BoulevardRelief.SlabFaces().Find(q => q.seed == e.seed);
                var skin = NocFacadeMath.SkinOf(NocFacadeMath.SkinByte(face.seed, face.region), 0);
                int band = (int)Math.Floor(e.y0 / NocturneConstants.FacadeBand);
                int type = NocFacadeMath.StratumV2(skin.key, band);
                var c = NocFacadeMath.CellOf(skin, type);
                NocFacadeMath.Bays(skin, c, type, e.uMin, e.uMax, out int iF, out int iL, out int c0, out int c1);
                int i = (int)Math.Floor((e.kind == 'f' ? e.u0 + 1e-6 : 0.5 * (e.u0 + e.u1)) / c.Pu);
                Assert.IsTrue(NocFacadeMath.BayHasWindow(i, iF, iL, c0, c1), FormattableString.Invariant($"{e.kind} at u {e.u0:F2} on a window bay"));
                double cellY = band * c.bandH + Math.Floor((e.y0 - band * c.bandH) / c.Pv + 1e-6) * c.Pv;
                if (e.kind == 'b')
                {
                    Assert.AreEqual(skin.id, NocFacadeMath.SkinS4, "balconies on S4");
                    Assert.AreEqual(cellY, e.y0, 0.01, "balcony slab = the cell's slab band");
                    Assert.AreEqual(c.slab, e.y1 - e.y0, 0.01);
                    Assert.AreEqual(i * c.Pu + c.ox0 - BoulevardRelief.BalSide, e.u0, 0.01, "balcony on the opening");
                }
                else if (e.kind == 'f')
                {
                    Assert.AreEqual(skin.id, NocFacadeMath.SkinS0, "fins on S0");
                    Assert.AreEqual(0.0, Frac(e.u0 / c.Pu), 0.01 / c.Pu, "fin on a cell edge");
                }
                Assert.LessOrEqual(e.y1, BoulevardRelief.YMax + 1e-6);
                Assert.GreaterOrEqual(e.y0, BoulevardRelief.YMin - 1e-6);
            }
            Report(FormattableString.Invariant($"boulevard relief: {BoulevardRelief.Faces} faces, {BoulevardRelief.Balconies} balconies, {BoulevardRelief.Fins} fins, {BoulevardRelief.AcUnits} AC units"));
        }

        [Test]
        public void UpperTube_EndsInsidePortals()
        {
            double xa = SkybridgeDims.FaceAX, xb = SkybridgeDims.FaceBX, y = SkybridgeDims.UpperY, z = SkybridgeDims.UpperZ, r = SkybridgeDims.FaceTubeR;
            Assert.That(xa, Is.InRange(GateDims.CbX0, GateDims.CbX1), "west end inside the canyon bridge's body");
            Assert.That(y - EdgeDims.PortalOuter, Is.GreaterThan(GateDims.CbY0 + 2.0), "collar inside the bridge's height (bottom)");
            Assert.That(y + EdgeDims.PortalOuter, Is.LessThan(GateDims.CbY1 - 2.0), "collar inside the bridge's height (top)");
            Assert.That(xb, Is.InRange(GateDims.KeyX0, GateDims.KeyX1), "east end inside the keystone");
            Assert.That(y - EdgeDims.PortalOuter, Is.GreaterThan(GateDims.KeyY0 + 2.0), "collar inside the keystone (bottom)");
            Assert.That(z - EdgeDims.PortalOuter, Is.GreaterThan(GateDims.KeyZ0 + 2.0), "collar inside the keystone (south)");
            Assert.That(z + EdgeDims.PortalOuter, Is.LessThan(GateDims.KeyZ1 - 2.0), "collar inside the keystone (north)");
            Assert.Less(r, GulfPortal.RingInner, "the tube runs inside the portal tunnel");
        }

        [Test]
        public void EastBlock_KoiClearance_ClosesTheNotch_ReachesThePlaza()
        {
            double minR = double.MaxValue;
            foreach (var p in Parts())
            {
                if (!p.Name.StartsWith("NotchEastBlock", StringComparison.Ordinal)) continue;
                foreach (var v in p.Mb.Positions)
                {
                    Vector3 wv = v + p.Origin;
                    double dx = wv.x - GateDims.SX, dz = wv.z - GateDims.SZ;
                    minR = Math.Min(minR, Math.Sqrt(dx * dx + dz * dz));
                }
            }
            Report(FormattableString.Invariant($"east block: nearest vertex {minR:F2} m from the spire axis (koi parts <= {GateDims.KoiMaxR})"));
            Assert.GreaterOrEqual(minR, GateDims.KoiMaxR + 10.0 - 0.05, "the koi keeps 10 m or more from the block");
            // the notch's east end x 914 is closed for z 115..275 above y 38 and for z 115..236 below (the arcade closes the rest)
            for (double z = EdgeDims.NotchZ0 + 0.5; z < EdgeDims.NotchZ1; z += 5.0)
                for (double y = 0.5; y < EdgeDims.FaceTop; y += 10.0)
                {
                    bool arcade = z >= SignsLayout.ArcZ0 && y <= SignsLayout.ArcH;
                    Assert.IsTrue(arcade || GateDims.InEastBlock(GateDims.EbX0 + 0.5, y, z), FormattableString.Invariant($"x 914 closed at y {y:F1}, z {z:F1}"));
                }
            // the block reaches the plaza disc (R 400) across z 115..236 (GB-14: no void between the garden and the plaza)
            for (double z = EdgeDims.NotchZ0; z <= SignsLayout.ArcZ0; z += 5.0)
            {
                double dz = z - GateDims.SZ, xCircle = GateDims.SX - Math.Sqrt(400.0 * 400.0 - dz * dz);
                Assert.Greater(GateDims.ArcX(z), xCircle, FormattableString.Invariant($"block reaches the plaza disc at z {z:F0}"));
            }
            // the walk lane: x <= 911.74 (+0.38 PathGuide wall) on the straight leg and the C2 arc; nothing of the block below y 4 west of x 914
            Assert.Greater(GateDims.EbX0, SignsLayout.LaneCentreX + SignsLayout.LaneHalf + 0.38, "the block's west face stands beyond the lane wall");
        }

        /// <summary>AB fix wave C03: the upper tube's keystone portal (and the canyon bridge's) carries separate collar lamps, not a
        /// continuous ring, and the keystone's west face is face B's skin (NocFacade), not a dark concrete square around the ring.</summary>
        [Test]
        public void TubePortals_SegmentedLamps_KeystoneInFaceBSkin()
        {
            int rings = 0;
            foreach (var p in Parts())
            {
                if (p.Mat != ReliefMat.PortalRing) continue;
                rings++;
                Assert.AreEqual(2 * GulfPortal.LampCount, p.Mb.Indices.Count / 3, p.Name + ": one quad per lamp");
            }
            Assert.AreEqual(2, rings, "notch gate and canyon bridge portals");
            int keyFacade = 0;
            foreach (var p in Parts())
            {
                if (p.Name != "NotchGate_facade") continue;
                foreach (var v in p.Mb.Positions)
                    if (Math.Abs(v.x + p.Origin.x - GateDims.KeyX0) < 1e-3) keyFacade++;
            }
            Assert.Greater(keyFacade, EdgeDims.PortalSegments, "the keystone's west face is written in the facade part");
        }
    }
}
