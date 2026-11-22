// Beat02BoulevardTests: the acceptance dimensions of beat 2 (MASTER_PROMPT 14.5.2 Acceptance (1) and (6), the KS03-KS06 projection
// numbers of 14.5.2 / 14.10) on the BUILT geometry (Beat02_Boulevard.BuildParts(), ray-cast with G1TestGeo):
//   street 150.0 m between the street faces (rays along z above the arcade), median 24.0 m between the barriers' outer faces,
//   blades 14 x 200 m (+-0.05; the NocNeon face and the slab body), the twelve blade positions and colour cycle, 84 columns,
//   40 lamp posts, the tram sight sector (nothing taller than 0.5 m in z 195-225, x 79-175 except the 0.9 m barrier), the
//   agreements (street floor x -1 500..215.3 with its top at y 0 and no face at the tongue seam, slabs end at x 213 + closure to
//   x 215.25 < 215.3), every mesh valid, chunk origins within 200 m, <= 520 k triangles; KS03 / KS04 / KS05 / KS06 subject
//   rectangles against the 14.5.2 numbers (the spec rounds; tolerance 0.06 NDC, logged); the camera sphere and the player capsule
//   along both lane edges and the centre (s 120-400) never enter a triangle (rails excepted for the capsule: they are 0.07 m and
//   walked over at s 162-164).
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class Beat02BoulevardTests
    {
        static G1Parts s_parts;
        static G1TestGeo s_geo;
        static G1Parts Parts => s_parts ?? (s_parts = Beat02_Boulevard.BuildParts());
        static G1TestGeo Geo => s_geo ?? (s_geo = new G1TestGeo(Parts));

        [Test]
        public void Street_Is150_Median_Is24()
        {
            foreach (float x in new[] { 100f, 180f, 60f })
            {
                var o = new Vector3(x, 20f, 195f);
                Assert.IsTrue(Geo.Ray(o, Vector3.forward, 200f, out float tn, out string pn), "north face");
                Assert.IsTrue(Geo.Ray(o, Vector3.back, 200f, out float ts, out string ps), "south face");
                Debug.Log(FormattableString.Invariant($"[NOC] street at x {x:F0}: {tn + ts:F4} m ({ps} .. {pn})"));
                Assert.AreEqual(150.0, tn + ts, 0.05, "street width at x " + x);
            }
            // the median: from the road on both sides inward at y 0.5, between planters (x 100 lies between 89 and 103)
            Assert.IsTrue(Geo.Ray(new Vector3(100f, 0.5f, 170f), Vector3.forward, 40f, out float a, out string pa));
            Assert.IsTrue(Geo.Ray(new Vector3(100f, 0.5f, 220f), Vector3.back, 40f, out float b, out string pb));
            double z0 = 170.0 + a, z1 = 220.0 - b;
            Debug.Log(FormattableString.Invariant($"[NOC] median at x 100: z {z0:F3}..{z1:F3} = {z1 - z0:F4} m ({pa}, {pb})"));
            Assert.AreEqual(24.0, z1 - z0, 0.05, "median width");
            Assert.AreEqual(BoulevardLayout.MedianZ0, z0, 0.02);
        }

        [Test]
        public void Blades_Are14x200_AtTheSpecPositions()
        {
            int n = 0;
            var pal = new List<int>();
            foreach (var s in Parts.Signs)
            {
                if (s.size.y < 100f) continue;
                n++;
                Assert.AreEqual(14.0, s.size.x, 0.05, "blade width"); Assert.AreEqual(200.0, s.size.y, 0.05, "blade height");
                pal.Add(s.palette);
            }
            Assert.AreEqual(24, n, "twelve blades, glyph faces on both sides");
            for (int i = 0; i < BladeSign.Count; i++)
            {
                BladeSign.Blade(i, out int side, out double x, out double zf, out double y0);
                var part = Parts.Find("blade" + i);
                Assert.IsNotNull(part);
                Bounds b = Bounds(part);
                Assert.AreEqual(14.0, b.size.x, 0.05, "blade body width"); Assert.AreEqual(200.0, b.size.y, 0.05, "blade body height");
                Assert.AreEqual(side > 0 ? 40.0 : 60.0, b.min.y, 0.05);
                Assert.AreEqual(zf, side > 0 ? b.min.z : b.max.z, 0.01, "blade face z (263 / 127)");
                if (side > 0) Assert.AreEqual(18.0 + 36.0 * (i / 2), x, 1e-9); else Assert.AreEqual(6.0 + 36.0 * (i / 2), x, 1e-9);
                // LOOK_V2 10 (PA's REQUEST INT): the v1 4-cycle magenta / cyan / amber / violet is gone; no blade shows magenta (0)
                // or aviation red (6). Beat06SignsTests.Lights_R4_SaturationOfPaLights checks the tokens and light saturations.
                Assert.That(BladeSign.Palette(i), Is.Not.EqualTo(0).And.Not.EqualTo(6),
                    "LOOK_V2 10: no magenta or red blade (6-cycle SignWhite, NeonCyan, WinNeutral, SodiumAmber, SignWhite, WinTungsten)");
            }
        }

        static Bounds Bounds(G1Parts.Part p)
        {
            var b = new Bounds(p.Mb.Positions[0] + p.Origin, Vector3.zero);
            foreach (var q in p.Mb.Positions) b.Encapsulate(q + p.Origin);
            return b;
        }

        [Test]
        public void Counts_Columns84_Lamps40_Bollards()
        {
            int cols = Parts.Find("columnsS").Mb.TriangleCount / 44 + Parts.Find("columnsN").Mb.TriangleCount / 44;
            var bollards = MedianPromenade.BollardPositions();
            var lamps = MedianPromenade.LampPositions(bollards);
            Debug.Log(FormattableString.Invariant($"[NOC] boulevard: {cols} arcade columns (spec 84), {lamps.Count} lamp posts (spec 40), {bollards.Count} bollards (spec 60), lamp x {lamps[lamps.Count - 1].x:F0}..{lamps[0].x:F0}"));
            Assert.AreEqual(84, cols);
            Assert.AreEqual(40, lamps.Count);
            Assert.AreEqual(60, bollards.Count, 8);
            foreach (var l in lamps)
            {
                Assert.GreaterOrEqual(NocLanes.DistanceOutsideLanes(new Vector3(l.x, 0f, l.z), 200.0), MedianPromenade.LampClear, "lamp post in a lane at x " + l.x);
                bool sector = l.z > BoulevardLayout.SectorZ0 && l.z < BoulevardLayout.SectorZ1 && l.x > BoulevardLayout.SectorX0 && l.x < BoulevardLayout.SectorX1;
                Assert.IsFalse(sector, "lamp post in the tram sight sector at x " + l.x);
            }
            foreach (var p in bollards)
            {
                double d = NocLanes.DistanceOutsideLanes(p, 160.0);
                Assert.GreaterOrEqual(d, 0.35 + 0.1 - 0.01, "bollard surface inside the capsule's reach at " + p);
            }
        }

        [Test]
        public void TramSightSector_IsClear()
        {
            int bad = 0; string first = null;
            foreach (var p in Parts.Parts)
            {
                if (p.Name == "medianBarriers") continue;
                foreach (var q0 in p.Mb.Positions)
                {
                    Vector3 q = q0 + p.Origin;
                    if (q.x > BoulevardLayout.SectorX0 && q.x < BoulevardLayout.SectorX1 && q.z > BoulevardLayout.SectorZ0 && q.z < BoulevardLayout.SectorZ1 && q.y > BoulevardLayout.SectorMaxH)
                    { bad++; if (first == null) first = p.Name + " " + q; }
                }
            }
            Assert.AreEqual(0, bad, "geometry above 0.5 m in the tram sight sector: " + first);
            // the barrier itself is 0.9 m
            var bar = Bounds(Parts.Find("medianBarriers"));
            Assert.AreEqual(0.9, bar.max.y, 0.01);
        }

        [Test]
        public void Agreements_FloorSeam_Closure_FaceA()
        {
            float xmax = float.MinValue, xmin = float.MaxValue, ymax = float.MinValue;
            foreach (var p in Parts.Parts)
            {
                if (!p.Name.StartsWith("street") || p.Name.StartsWith("streetEdge") || p.Mb.VertexCount == 0) continue;
                var b = Bounds(p);
                xmax = Mathf.Max(xmax, b.max.x); xmin = Mathf.Min(xmin, b.min.x); ymax = Mathf.Max(ymax, b.max.y);
            }
            Assert.AreEqual(215.3, xmax, 1e-3, "street floor ends at the tongue root x 215.3 (agreement 2)");
            Assert.AreEqual(-1500.0, xmin, 1e-3);
            Assert.AreEqual(0.0, ymax, 1e-6, "walkable top at y 0 (agreement 1)");
            // no east face of the floor at the seam, nothing of G1 beyond x 215.3 except the terminus cornice (2 cm, y 13)
            foreach (var p in Parts.Parts)
            {
                if (p.Mb.VertexCount == 0) continue;
                var b = Bounds(p);
                if (p.Name == "terminusTrim") { Assert.LessOrEqual(b.max.x, 215.28f); continue; }
                Assert.LessOrEqual(b.max.x, 215.3f + 1e-3f, p.Name + " reaches beyond x 215.3");
            }
            // ray from the tongue side (x 230) westward at y 20 inside the flank z 100: hits the closure at 215.25 (behind face A)
            Assert.IsTrue(Geo.Ray(new Vector3(230f, 100f, 100f), Vector3.left, 40f, out float t, out string part));
            Assert.AreEqual(230.0 - BoulevardLayout.ClosureX1, t, 1e-3, part);
        }

        [Test]
        public void Meshes_Valid_ChunkOrigins_UnderBudget()
        {
            long tris = 0;
            foreach (var p in Parts.Parts)
            {
                if (p.Mb.VertexCount == 0) continue;
                var r = MeshValidation.Validate(p.Mb, p.Name);
                Assert.IsTrue(r.Ok, r.ToString());
                tris += p.Mb.TriangleCount;
                foreach (var q in p.Mb.Positions)
                    Assert.LessOrEqual(new Vector2(q.x, q.z).magnitude, 200f, p.Name + ": vertex beyond 200 m of its chunk origin (7.5)");
            }
            Debug.Log(FormattableString.Invariant($"[NOC] beat 2: {Parts.Parts.Count} parts, {tris} triangles (budget 520 000), {Parts.Signs.Count} signs, {Parts.Lights.Count} lights"));
            Assert.LessOrEqual(tris, 520000);
            Assert.LessOrEqual(Parts.Lights.Count, 28, "14.2.3 beat 2: at most 28 realtime local lights");
        }

        [Test]
        public void KeyShots_Subjects()
        {
            // KS03 (s 116): north facade base at y -0.42 on the axis; north blades x 54 and 90, lower 70 m
            var v3 = G1TestGeo.View(G1TestGeo.Shot("KS03_boulevard_mouth"));
            QaProjection.ProjectPoint(v3, new D3(0, 0, BoulevardLayout.NorthZ), out _, out double yb);
            Debug.Log(FormattableString.Invariant($"[NOC] KS03 north facade base y {yb:F3} (spec -0.42)"));
            Assert.AreEqual(-0.42, yb, 0.02);
            G1TestGeo.Inside(Blade(v3, 3, 70), 0.28, 0.41, 0.0, 0.66, 0.06, "KS03 blade x 54 (lower 70 m)");
            G1TestGeo.Inside(Blade(v3, 5, 70), 0.46, 0.62, 0.0, 0.66, 0.06, "KS03 blade x 90 (lower 70 m)");
            // KS04 (s 250): north blade x 198 (y 40-140), south blade x 186 (y 60-160); canyon end (floor end x 215.3). D-040 (C10):
            // the KS04 pitch 22 -> 27 moves them down (14.10's -0.20..0.69 / 0.04..0.91 / -0.64 at pitch 22 become the values below)
            var v4 = G1TestGeo.View(G1TestGeo.Shot("KS04_boulevard_axis"));
            G1TestGeo.Inside(Blade(v4, 11, 100), -0.48, -0.30, -0.34, 0.54, 0.07, "KS04 north blade x 198");
            G1TestGeo.Inside(Blade(v4, 10, 100), 0.31, 0.49, -0.10, 0.74, 0.07, "KS04 south blade x 186");
            QaProjection.ProjectPoint(v4, new D3(BoulevardLayout.FacadeX1, 0, BoulevardLayout.AxisZ), out _, out double yc);
            Debug.Log(FormattableString.Invariant($"[NOC] KS04 canyon end y {yc:F3} (D-040 pitch 27: -0.81; 14.10 at pitch 22: -0.64)"));
            Assert.AreEqual(-0.81, yc, 0.02);
            // KS05 (s 310): blade tops (north x 198, south x 186), blade x 162 mid, facade-cloud lines at y 460 near x -+0.12
            var v5 = G1TestGeo.View(G1TestGeo.Shot("KS05_boulevard_up"));
            G1TestGeo.Inside(Top(v5, 11), -0.27, -0.23, 0.50, 0.59, 0.06, "KS05 north blade x 198 top");
            G1TestGeo.Inside(Top(v5, 10), 0.22, 0.26, 0.62, 0.71, 0.06, "KS05 south blade x 186 top");
            QaProjection.ProjectPoint(v5, new D3(425.0, 460.0, BoulevardLayout.NorthZ), out double lx, out double ly);
            QaProjection.ProjectPoint(v5, new D3(425.0, 460.0, BoulevardLayout.SouthZ), out double rx, out _);
            Debug.Log(FormattableString.Invariant($"[NOC] KS05 facade-cloud lines (y 460, x 425): x {lx:F3} / {rx:F3} at y {ly:F3} (spec -+0.12 at +0.13)"));
            Assert.AreEqual(-0.12, lx, 0.02); Assert.AreEqual(0.12, rx, 0.02);
            // KS06 (s 264): the tram body (x 109.8-169.8, z 227, 3.6 m) and the line of sight over the sector
            var v6 = G1TestGeo.View(G1TestGeo.Shot("KS06_tram"));
            var tram = G1TestGeo.Rect(v6, G1TestGeo.BoxCorners(new Vector3(109.8f, 0.3f, 225.3f), new Vector3(169.8f, 3.9f, 228.7f)));
            G1TestGeo.Inside(tram, -0.24, 0.23, -0.43, -0.29, 0.06, "KS06 tram volume (Phase 4)");
            var eye = new Vector3(79.3f, 1.7f, 195f);
            int blocked = 0, total = 0;
            for (float x = 112f; x <= 168f; x += 4f)
                for (float y = 0.6f; y <= 3.6f; y += 1.0f)
                {
                    Vector3 d = new Vector3(x, y, 226f) - eye; total++;
                    if (Geo.Ray(eye, d.normalized, d.magnitude, out _, out string part) && part != "medianBarriers") blocked++;
                }
            Debug.Log(FormattableString.Invariant($"[NOC] KS06 tram rays: {total - blocked}/{total} unoccluded"));
            Assert.AreEqual(0, blocked, "nothing of G1 between the KS06 eye and the tram");
        }

        static Vector4 Blade(QaView v, int i, double lower)
        {
            BladeSign.Blade(i, out int side, out double x, out double zf, out double y0);
            return G1TestGeo.Rect(v, G1TestGeo.BoxCorners(new Vector3((float)x - 7f, (float)y0, (float)zf), new Vector3((float)x + 7f, (float)(y0 + lower), (float)zf)));
        }

        static Vector4 Top(QaView v, int i)
        {
            BladeSign.Blade(i, out int side, out double x, out double zf, out double y0);
            float yt = (float)(y0 + BoulevardLayout.BladeH);
            return G1TestGeo.Rect(v, new[] { new Vector3((float)x - 7f, yt, (float)zf), new Vector3((float)x + 7f, yt, (float)zf) });
        }

        [Test]
        public void Walkability_CameraAndCapsuleClear()
        {
            Geo.LaneClearance(120.0, 400.0, 0.5, n => n.StartsWith("rails"), out double cam, out double cap, out string worst);
            Debug.Log(FormattableString.Invariant($"[NOC] beat 2 clearance: camera sphere min {cam:F3} m (>= 0.2), capsule min {cap:F3} m (>= 0.35, rails excepted) {worst}"));
            Assert.GreaterOrEqual(cam, NocturneConstants.CameraSphere, worst);
            Assert.GreaterOrEqual(cap, NocturneConstants.PlayerRadius - 2e-3, worst);
        }

        [Test]
        public void CanyonWest_CityTowersAdjoinTheHeroFacades()
        {
            // the canyon west of the hero facades (south x < -90, north x < -60) is CityField's: log the tower footprints that face
            // the street (D-018 exclusions) and the largest gap along each wall (information for the skyline / exclusion review)
            foreach (int side in new[] { -1, 1 })
            {
                var spans = new List<Vector2>();
                double nearZ = side < 0 ? -1e9 : 1e9;
                for (int ci = -5; ci <= 0; ci++)
                    for (int cj = side < 0 ? -1 : 2; cj <= (side < 0 ? 0 : 3); cj++)
                    {
                        if (!CityField.TowerAt(ci, cj, out CityTower t)) continue;
                        double x0 = t.cx - t.radius, x1 = t.cx + t.radius;
                        if (x1 < -700 || x0 > -40) continue;
                        spans.Add(new Vector2((float)x0, (float)x1));
                        double zf = side < 0 ? t.cz + t.radius : t.cz - t.radius;
                        nearZ = side < 0 ? Math.Max(nearZ, zf) : Math.Min(nearZ, zf);
                    }
                spans.Sort((a, b) => b.y.CompareTo(a.y));
                double edge = side < 0 ? BoulevardLayout.SouthX0 : BoulevardLayout.NorthX0, gap = 0, cur = edge;
                foreach (var sp in spans) { if (sp.y < cur) gap = Math.Max(gap, cur - sp.y); cur = Math.Min(cur, sp.x); }
                Debug.Log(FormattableString.Invariant($"[NOC] canyon west, {(side < 0 ? "south" : "north")} wall: {spans.Count} CityField towers in x -700..-40 (bounding circles), first tower face reaches x {(spans.Count > 0 ? spans[0].y : float.NaN):F0} (hero end x {edge:F0}), largest gap {gap:F0} m, nearest street-side extent z {nearZ:F0}"));
            }
        }
    }
}
