// Beat07HeartTests: EditMode acceptance of beat 7, Heart Square (MASTER_PROMPT 14.5.7 Geometry and Acceptance, 14.10 KS19 and
// T-KOI clearances, 14.11 goal, 14.4 lanes; geometry agreements 1 and 6; D-009; Part 10 items 8, 27, 29):
//   plaza circle R 400.0 about S and the floors cover the lane + 1 m from C3 to the goal; tenement 14.4 m at x 1 156 - 1 170,
//   z 379 - 397, the window opening 1.2 x 1.5 m at x 1 156, y 4.4 - 5.9, z 387.3 - 388.5; KS19 window at x -0.15..-0.03,
//   y -0.26..+0.02 and 138 x 172 px at 1080p; the lamp sphere visible from the KS19 eye over the balcony slab; spire radii 32 / 20,
//   drums 48 / 42 / 36, 12 fins; 22 ring towers outside R 456 (and inside 470 - 560), the west sector +-40 deg clear, no overlap,
//   koi clearance to the tower boxes >= 100 m over 20 wave phases; the NocKoi vertex formulas (KoiShaderMath, the shader's #defines)
//   reproduce KoiMath within 5 cm for every vertex of the mesh at many poses and every pose stays inside the mesh bounds; the driver's
//   head azimuth follows the 14.5.7 table; nothing of beat 7 stands in the lane; every mesh validates; lights and triangles within
//   the 14.2.3 / 14.5.7 budgets; no amber-hued emitter but the window.
// HT (Phase 4 wave A): the plaza inlay overlay is the slab's own top fan, its pattern (17 rings, 48 seams, the walk's leading
// seam, energy-kept sub-pixel lines, the 6 px pitch fade); the spire's 11 hoops, ring-light breathing (0.1 Hz, ring sum
// within +-4.2 %), the buttress flare landing on the lowest drum, the podium steps and the envelope R(y) + 4 above 96 m.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class Beat07HeartTests
    {
        static KeyShot Shot(string id)
        {
            foreach (var k in NocturneConstants.KeyShots) if (k.tag.StartsWith(id, StringComparison.Ordinal)) return k;
            throw new ArgumentException(id);
        }

        static Bounds WorldBounds(MeshBuilder mb, Vector3 origin)
        {
            var b = new Bounds(mb.Positions[0] + origin, Vector3.zero);
            foreach (var p in mb.Positions) b.Encapsulate(p + origin);
            return b;
        }

        [Test]
        public void Plaza_Radius400_AndFloorsCoverTheLane()
        {
            var outline = HeartFloorBuilder.Outline();
            double rMin = double.MaxValue, rMax = 0.0;
            int onCircle = 0;
            foreach (var p in outline)
            {
                double r = Math.Sqrt((p.x - HeartLayout.SX) * (p.x - HeartLayout.SX) + (p.y - HeartLayout.SZ) * (p.y - HeartLayout.SZ));
                if (Math.Abs(r - 400.0) < 0.5) { onCircle++; rMin = Math.Min(rMin, r); rMax = Math.Max(rMax, r); }
            }
            Debug.Log(FormattableString.Invariant($"[NOC] plaza outline: {outline.Count} points, {onCircle} on the circle at R {rMin:F4}..{rMax:F4}"));
            Assert.AreEqual(400.0, rMin, 0.02); Assert.AreEqual(400.0, rMax, 0.02);
            Mesh m = HeartFloorBuilder.Slab().ToMesh("heartFloor");
            Assert.IsTrue(MeshValidation.Validate(m).Ok, MeshValidation.Validate(m).ToString());
            Bounds b = m.bounds;
            Assert.AreEqual(0.0, b.max.y, 1e-4, "walk plane y 0");
            Assert.GreaterOrEqual(-b.min.y, 1.0, "slab at least 1 m thick (agreement 1)");
            UnityEngine.Object.DestroyImmediate(m);

            int prom = 0, heart = 0;
            for (double s = NocturneConstants.BeatBoundaries[6]; s <= NocturneConstants.GoalS; s += 0.5)
            {
                double h = NocLanes.LaneHalfWidth(s) + 1.0;
                for (double l = -h; l <= h + 1e-9; l += 0.5)
                {
                    Vector3 p = NocLanes.CorridorToWorld(s, l, 0.0);
                    if (p.x >= SignsLayout.PromX0 && p.x <= SignsLayout.PromX1 && p.z >= SignsLayout.PromZ0 && p.z <= SignsLayout.PromZ1) { prom++; continue; }
                    Assert.IsTrue(HeartFloorBuilder.Covers(p.x, p.z), FormattableString.Invariant($"lane point ({p.x:F2}, {p.z:F2}) at s {s} not floored"));
                    heart++;
                }
            }
            Debug.Log(FormattableString.Invariant($"[NOC] Beat07 lane + 1 m: {prom} points on the promenade, {heart} on the heart floor"));
        }

        [Test]
        public void Tenement_14p4_AndTheWindowOpening()
        {
            var t = OldQuarterBuilder.Tenement();
            Bounds b = WorldBounds(t, OldQuarterBuilder.Origin);
            Debug.Log(FormattableString.Invariant($"[NOC] tenement x {b.min.x:F3}-{b.max.x:F3}, z {b.min.z:F3}-{b.max.z:F3}, top {b.max.y:F3}"));
            Assert.AreEqual(14.4, b.max.y, 0.02); Assert.AreEqual(1156.0, b.min.x, 0.02); Assert.AreEqual(1170.0, b.max.x, 0.02);
            Assert.AreEqual(379.0, b.min.z, 0.02); Assert.AreEqual(397.0, b.max.z, 0.02);
            Assert.AreEqual(4, (int)Math.Round(HeartLayout.TenH / HeartLayout.Storey), "4 storeys of 3.6 m");
            // the opening: no tenement face covers the window rectangle (ray from the goal side through every point of it)
            var mesh = t.ToMesh("tenement");
            var verts = new List<Vector3>(); mesh.GetVertices(verts);
            var idx = new List<int>(); mesh.GetTriangles(idx, 0);
            int blocked = 0, samples = 0;
            for (double y = HeartLayout.GoalY0 + 0.02; y < HeartLayout.GoalY1; y += 0.1)
                for (double z = HeartLayout.GoalZ0 + 0.02; z < HeartLayout.GoalZ1; z += 0.1)
                {
                    samples++;
                    var o = new Vector3(1150f, (float)y, (float)z) - OldQuarterBuilder.Origin;
                    for (int i = 0; i < idx.Count; i += 3)
                        if (RayHits(o, Vector3.right, verts[idx[i]], verts[idx[i + 1]], verts[idx[i + 2]], out float d) && d < 9.0f) { blocked++; break; }
                }
            // the window edges: the reveal quads span exactly x 1156 - 1156.3, y 4.4 - 5.9, z 387.3 - 388.5
            double y0 = double.MaxValue, y1 = double.MinValue, z0 = double.MaxValue, z1 = double.MinValue;
            foreach (var q in verts)
            {
                Vector3 p = q + OldQuarterBuilder.Origin;
                if (p.x <= HeartLayout.GoalX + 0.01 || p.x > HeartLayout.GoalX + HeartLayout.WallT + 0.01) continue;
                y0 = Math.Min(y0, p.y); y1 = Math.Max(y1, p.y); z0 = Math.Min(z0, p.z); z1 = Math.Max(z1, p.z);
            }
            Debug.Log(FormattableString.Invariant($"[NOC] goal window opening y {y0:F3}-{y1:F3} ({y1 - y0:F3} m), z {z0:F3}-{z1:F3} ({z1 - z0:F3} m); {blocked}/{samples} rays through it blocked"));
            Assert.AreEqual(0, blocked);
            Assert.AreEqual(1.5, y1 - y0, 0.01); Assert.AreEqual(1.2, z1 - z0, 0.01);
            Assert.AreEqual(4.4, y0, 0.01); Assert.AreEqual(387.3, z0, 0.01);
            UnityEngine.Object.DestroyImmediate(mesh);
        }

        // Moller-Trumbore, both sides
        static bool RayHits(Vector3 o, Vector3 d, Vector3 a, Vector3 b, Vector3 c, out float t)
        {
            t = 0f;
            Vector3 e1 = b - a, e2 = c - a, p = Vector3.Cross(d, e2);
            float det = Vector3.Dot(e1, p);
            if (Mathf.Abs(det) < 1e-9f) return false;
            float inv = 1f / det;
            Vector3 s = o - a;
            float u = Vector3.Dot(s, p) * inv;
            if (u < 0f || u > 1f) return false;
            Vector3 q = Vector3.Cross(s, e1);
            float v = Vector3.Dot(d, q) * inv;
            if (v < 0f || u + v > 1f) return false;
            t = Vector3.Dot(e2, q) * inv;
            return t > 0f;
        }

        [Test]
        public void KS19_WindowFraming_AndLampOverTheBalcony()
        {
            var k = Shot("KS19");
            var v = new QaView(k.x, k.y, k.z, k.yaw, k.pitch, k.fov, 1920, 1080);
            double x0 = 9, x1 = -9, y0 = 9, y1 = -9;
            foreach (double y in new[] { HeartLayout.GoalY0, HeartLayout.GoalY1 })
                foreach (double z in new[] { HeartLayout.GoalZ0, HeartLayout.GoalZ1 })
                {
                    Assert.IsTrue(QaProjection.ProjectPoint(v, new D3(HeartLayout.GoalX, y, z), out double nx, out double ny));
                    x0 = Math.Min(x0, nx); x1 = Math.Max(x1, nx); y0 = Math.Min(y0, ny); y1 = Math.Max(y1, ny);
                }
            double wPx = (x1 - x0) * 0.5 * 1920, hPx = (y1 - y0) * 0.5 * 1080;
            Debug.Log(FormattableString.Invariant($"[NOC] KS19 window x {x0:F3}..{x1:F3}, y {y0:F3}..{y1:F3}, {wPx:F1} x {hPx:F1} px at 1080p (14.5.7: -0.15..-0.03, -0.26..+0.02, 138 x 172)"));
            Assert.AreEqual(-0.15, x0, 0.02); Assert.AreEqual(-0.03, x1, 0.02);
            Assert.AreEqual(-0.26, y0, 0.02); Assert.AreEqual(0.02, y1, 0.02);
            // 14.5.7's 138 x 172 px is f x size / 7.82 m (the horizontal distance, f = 898.7 px at vFOV 62): the exact projection of the
            // stated NDC range gives the same width and a height of 0.28 x 540 = 151 px (the window is seen 24 deg above the axis)
            Assert.AreEqual(138.0, wPx, 5.0);
            Assert.AreEqual(0.28 * 540.0, hPx, 5.0);
            // the lamp sphere (the x 3.5 core) clears the balcony slab's front edge as seen from the KS19 eye
            double ex = k.x, ey = k.y;
            double lowest = OldQuarterBuilder.LampCentreY - HeartLayout.LampR;
            double t = (HeartLayout.BalX0 - ex) / (HeartLayout.LampX - ex);
            double atEdge = ey + (lowest - ey) * t;
            Debug.Log(FormattableString.Invariant($"[NOC] KS19 ray to the lamp's lowest point passes the balcony edge at y {atEdge:F3} (slab top {OldQuarterBuilder.BalconyTop})"));
            Assert.Greater(atEdge, OldQuarterBuilder.BalconyTop + 0.05);
            // GB-18 (D-029): the sight line to the window's bottom edge clears the slab's front edge, so no part of the window hides
            double tb = (HeartLayout.BalX0 - ex) / (HeartLayout.GoalX - ex);
            double sill = ey + (HeartLayout.GoalY0 - ey) * tb;
            Debug.Log(FormattableString.Invariant($"[NOC] KS19 ray to the window's bottom edge passes the balcony edge at y {sill:F3} (slab top {OldQuarterBuilder.BalconyTop})"));
            Assert.Greater(sill, OldQuarterBuilder.BalconyTop + 0.05, "the balcony slab hides the bottom of the goal window (GB-18)");
            // ... and enters the room through the opening
            double tw = (HeartLayout.GoalX - ex) / (HeartLayout.LampX - ex);
            double yw = ey + (OldQuarterBuilder.LampCentreY - ey) * tw;
            Assert.That(yw, Is.InRange(HeartLayout.GoalY0 + 0.1, HeartLayout.GoalY1 - 0.1));
        }

        /// <summary>INT-08 (HT): the goal window's joinery and sill keep the lamp in view from the KS19 eye (a grid of rays over the
        /// sphere's disc), leave most of the opening open (>= 60 % of a grid of rays through it), and the curtain is pleated (its
        /// depth range and the spread of its normals).</summary>
        [Test]
        public void GoalWindow_JoineryKeepsTheLampInView_CurtainIsPleated()
        {
            var mb = new MeshBuilder();
            OldQuarterBuilder.GoalJoinery(mb);
            OldQuarterBuilder.GoalSill(mb);
            var mesh = mb.ToMesh("goalWindow");
            Assert.IsTrue(MeshValidation.Validate(mesh).Ok);
            var verts = new List<Vector3>(); mesh.GetVertices(verts);
            var idx = new List<int>(); mesh.GetTriangles(idx, 0);
            var k = Shot("KS19");
            Vector3 eye = new Vector3((float)k.x, (float)k.y, (float)k.z) - OldQuarterBuilder.Origin;
            bool Blocked(Vector3 target)
            {
                Vector3 d = target - eye; float dist = d.magnitude; d /= dist;
                for (int i = 0; i < idx.Count; i += 3)
                    if (RayHits(eye, d, verts[idx[i]], verts[idx[i + 1]], verts[idx[i + 2]], out float t) && t < dist) return true;
                return false;
            }
            Vector3 lamp = new Vector3((float)HeartLayout.LampX, (float)OldQuarterBuilder.LampCentreY, (float)HeartLayout.LampZ) - OldQuarterBuilder.Origin;
            Vector3 f = (lamp - eye).normalized, r = Vector3.Cross(Vector3.up, f).normalized, u = Vector3.Cross(f, r);
            int lampRays = 0, lampBlocked = 0;
            for (int i = -3; i <= 3; i++)
                for (int j = -3; j <= 3; j++)
                {
                    if (i * i + j * j > 9) continue;
                    lampRays++;
                    if (Blocked(lamp + r * (0.095f * i / 3f) + u * (0.095f * j / 3f))) lampBlocked++;
                }
            int winRays = 0, winBlocked = 0;
            for (double y = HeartLayout.GoalY0 + 0.025; y < HeartLayout.GoalY1; y += 0.05)
                for (double z = HeartLayout.GoalZ0 + 0.025; z < HeartLayout.GoalZ1; z += 0.05)
                {
                    winRays++;
                    // a point 0.5 m behind the opening along the view ray
                    Vector3 w = new Vector3((float)HeartLayout.GoalX, (float)y, (float)z) - OldQuarterBuilder.Origin;
                    Vector3 target = w + (w - eye).normalized * 0.5f;
                    if (Blocked(target)) winBlocked++;
                }
            double open = 1.0 - winBlocked / (double)winRays;
            // the curtain: pleat depth and normal spread
            var cur = new MeshBuilder();
            OldQuarterBuilder.Curtain(cur);
            float xMin = float.MaxValue, xMax = float.MinValue, nzMin = 1f, nzMax = -1f;
            foreach (var p in cur.Positions) { xMin = Mathf.Min(xMin, p.x); xMax = Mathf.Max(xMax, p.x); }
            foreach (var n in cur.Normals) { nzMin = Mathf.Min(nzMin, n.z); nzMax = Mathf.Max(nzMax, n.z); Assert.Less(n.x, 0f, "the curtain faces the window (-x)"); }
            Debug.Log(FormattableString.Invariant($"[NOC] goal window: lamp rays blocked {lampBlocked}/{lampRays}; opening {open:P1} open from the KS19 eye ({winBlocked}/{winRays} rays on the joinery); curtain depth {xMax - xMin:F3} m, normal z {nzMin:F2}..{nzMax:F2}, {cur.TriangleCount} triangles"));
            Assert.AreEqual(0, lampBlocked, "the joinery and the sill hide part of the lamp");
            Assert.GreaterOrEqual(open, 0.6, "the joinery covers too much of the window");
            Assert.GreaterOrEqual(xMax - xMin, 2.0 * HeartLayout.CurtainAmpEdge, "pleat depth");
            Assert.Greater(nzMax - nzMin, 1.2, "pleated normals");
            UnityEngine.Object.DestroyImmediate(mesh);
        }

        /// <summary>INT-08 (HT): the tenement's openings (18 dark windows, the goal window, the door) cut the west face exactly: the
        /// face's area is the facade minus the openings, no face quad covers an opening, and the reveals close every opening.</summary>
        [Test]
        public void Tenement_OpeningsCutTheFace()
        {
            var open = OldQuarterBuilder.Openings();
            int windows = 0, goal = 0, doors = 0;
            double holes = 0.0;
            foreach (var o in open) { if (o.kind == 0) goal++; else if (o.kind == 1) windows++; else doors++; holes += (o.y1 - Math.Max(o.y0, HeartLayout.BuildingY0)) * (o.z1 - o.z0); }
            Assert.AreEqual(1, goal); Assert.AreEqual(1, doors); Assert.AreEqual(18, windows);
            var t = OldQuarterBuilder.Tenement();
            double west = 0.0;
            for (int i = 0; i < t.Indices.Count; i += 3)
            {
                Vector3 a = t.Positions[t.Indices[i]], b = t.Positions[t.Indices[i + 1]], c = t.Positions[t.Indices[i + 2]];
                Vector3 n = Vector3.Cross(b - a, c - a);
                if (n.x < -1e-6f && Mathf.Abs(a.x + OldQuarterBuilder.Origin.x - (float)HeartLayout.TenX0) < 1e-3f && Mathf.Abs(b.x - a.x) < 1e-4f && Mathf.Abs(c.x - a.x) < 1e-4f)
                    west += 0.5 * n.magnitude;
            }
            double full = (HeartLayout.TenH - HeartLayout.BuildingY0) * (HeartLayout.TenZ1 - HeartLayout.TenZ0);
            Debug.Log(FormattableString.Invariant($"[NOC] tenement west face {west:F3} m2 = {full:F3} - openings {holes:F3} ({windows} windows, goal, door)"));
            Assert.AreEqual(full - holes, west, 1e-2);
            var rev = OldQuarterBuilder.TenementReveals();
            Assert.AreEqual(4 * windows + 3 * doors, rev.TriangleCount / 2, "four reveal quads per window, three at the door");
        }

        [Test]
        public void Spire_RadiiDrumsFins()
        {
            var shaft = SpireBuilder.Shaft();
            double r0 = 0, r460 = 0, rTop = 0, yMin = double.MaxValue, yMax = double.MinValue;
            foreach (var q in shaft.Positions)
            {
                double r = Math.Sqrt(q.x * q.x + q.z * q.z);
                if (Math.Abs(q.y) < 1e-3) r0 = Math.Max(r0, r);
                if (Math.Abs(q.y - 460f) < 1e-3) r460 = Math.Max(r460, r);
                if (Math.Abs(q.y - 560f) < 1e-3) rTop = Math.Max(rTop, r);
                yMin = Math.Min(yMin, q.y); yMax = Math.Max(yMax, q.y);
            }
            Debug.Log(FormattableString.Invariant($"[NOC] spire corner radius {r0:F3} at y 0, {r460:F3} at 460, {rTop:F3} at 560; y {yMin:F0}..{yMax:F0}"));
            Assert.AreEqual(32.0, r0, 0.02); Assert.AreEqual(20.0, r460, 0.02); Assert.AreEqual(20.0, rTop, 0.02);
            Assert.AreEqual(-3300.0, yMin, 0.1); Assert.AreEqual(560.0, yMax, 0.1);
            Bounds d = WorldBounds(SpireBuilder.Drums(), Vector3.zero);
            Assert.AreEqual(48.0, 0.5 * d.size.x, 0.05, "lowest drum R 48");
            Assert.AreEqual(18.0, d.max.y, 0.01, "three drums of 6 m");
            for (int i = 0; i < 3; i++) Assert.AreEqual(48.0 - 6.0 * i, HeartLayout.DrumR[i], 1e-9);
            var fins = new MeshBuilder(); var strips = new MeshBuilder();
            for (int j = 0; j < HeartLayout.FinCount; j++) { SpireBuilder.Fin(fins, j); SpireBuilder.Strip(strips, j); }
            Assert.AreEqual(12, HeartLayout.FinCount);
            foreach (var mb in new[] { shaft, SpireBuilder.Drums(), fins, strips })
            {
                Mesh m = mb.ToMesh("spire");
                Assert.IsTrue(MeshValidation.Validate(m).Ok, MeshValidation.Validate(m).ToString());
                UnityEngine.Object.DestroyImmediate(m);
            }
        }

        static void TowerCorners(CityTower t, List<Vector2> into)
        {
            into.Clear();
            for (int b = 0; b < 3; b++)
                for (int sx = -1; sx <= 1; sx += 2)
                    for (int sz = -1; sz <= 1; sz += 2) { t.Corner(b, sx, sz, out double x, out double z); into.Add(new Vector2((float)x, (float)z)); }
        }

        [Test]
        public void RingTowers_OutsideR456_WestClear_KoiClearance()
        {
            var towers = RingTowersBuilder.Towers();
            Assert.AreEqual(22, towers.Count);
            var corners = new List<Vector2>();
            double inMin = double.MaxValue, outMax = 0.0, westMin = double.MaxValue;
            foreach (var t in towers)
            {
                RingTowersBuilder.RadialExtent(t, out double rIn, out double rOut);
                inMin = Math.Min(inMin, rIn); outMax = Math.Max(outMax, rOut);
                Assert.Greater(rIn, HeartLayout.RingCityExclusion, "outside R 456");
                Assert.GreaterOrEqual(rIn, HeartLayout.RingInner - 0.05);
                Assert.LessOrEqual(rOut, HeartLayout.RingOuter + 0.05);
                TowerCorners(t, corners);
                foreach (var c in corners)
                {
                    double bearing = Math.Atan2(c.y - HeartLayout.SZ, c.x - HeartLayout.SX) / NocMath.Deg;
                    double fromWest = Math.Abs(NocMath.WrapDeg(bearing - 180.0));
                    westMin = Math.Min(westMin, fromWest);
                    Assert.Greater(fromWest, HeartLayout.RingWestClear, "the west sector +-40 deg carries no tower");
                }
                // inside the city's Heart exclusion (no CityField tower can collide with it)
                Assert.Less(rOut, NocturneConstants.CityHeartRadius);
            }
            for (int i = 0; i < towers.Count; i++)
                for (int j = i + 1; j < towers.Count; j++)
                {
                    double d = Math.Sqrt((towers[i].cx - towers[j].cx) * (towers[i].cx - towers[j].cx) + (towers[i].cz - towers[j].cz) * (towers[i].cz - towers[j].cz));
                    Assert.Greater(d, towers[i].radius + towers[j].radius + 2.0, "towers do not touch");
                }
            // koi body and veil samples against the tower boxes (base box, the widest), 36 head azimuths x 20 wave phases
            var buf = new D3[KoiMath.SampleCount];
            double clear = double.MaxValue;
            for (int ia = 0; ia < 36; ia++)
                for (int k = 0; k < KoiMath.WavePhases; k++)
                {
                    KoiMath.Samples(ia * 10.0 * NocMath.Deg, KoiMath.PhaseWaveTime(k), buf);
                    for (int i = 0; i < buf.Length; i++)
                    {
                        var part = KoiMath.SampleKind(i);
                        if (part == KoiPart.Pectoral || part == KoiPart.Dorsal) continue;   // fins are Phase 4
                        foreach (var t in towers) clear = Math.Min(clear, BoxDistance(t, buf[i]));
                    }
                }
            Debug.Log(FormattableString.Invariant($"[NOC] ring towers: inner {inMin:F2} m (>= 470, city exclusion 456), outer {outMax:F2} m (<= 560), nearest corner {westMin:F1} deg from west (> 40); koi body/veil to tower boxes {clear:F1} m (>= 100)"));
            Assert.GreaterOrEqual(clear, 100.0);
        }

        static double BoxDistance(CityTower t, D3 p)
        {
            double r = t.yaw * NocMath.Deg, c = Math.Cos(r), s = Math.Sin(r);
            double dx = p.x - t.cx, dz = p.z - t.cz;
            double lx = dx * c - dz * s, lz = dx * s + dz * c;      // inverse of Corner's rotation
            var q = t.boxes[0];
            double ex = Math.Max(0.0, Math.Abs(lx) - q.hx), ez = Math.Max(0.0, Math.Abs(lz) - q.hz);
            double ey = Math.Max(0.0, p.y - NocturneConstants.CityShaftY1);
            return Math.Sqrt(ex * ex + ez * ez + ey * ey);
        }

        [Test]
        public void KoiShader_MatchesKoiMath_AndStaysInBounds()
        {
            // the shader's constants are KoiMath's
            string path = Path.Combine(Application.dataPath, "_Game/Shaders/Beats/NocKoi.shader");
            string src = File.ReadAllText(path);
            double Def(string n) => double.Parse(Regex.Match(src, @"#define\s+" + n + @"\s+([-0-9.]+)").Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
            Assert.AreEqual(KoiMath.CentreX, Def("KOI_SX"), 1e-9); Assert.AreEqual(KoiMath.CentreZ, Def("KOI_SZ"), 1e-9);
            Assert.AreEqual(KoiMath.OrbitR0, Def("KOI_R0"), 1e-9); Assert.AreEqual(KoiMath.OrbitRAmp, Def("KOI_RA"), 1e-9);
            Assert.AreEqual(KoiMath.OrbitRPhase, Def("KOI_RP"), 1e-9); Assert.AreEqual(KoiMath.OrbitY0, Def("KOI_Y0"), 1e-9);
            Assert.AreEqual(KoiMath.OrbitYAmp, Def("KOI_YA"), 1e-9); Assert.AreEqual(KoiMath.OrbitYPhase, Def("KOI_YP"), 1e-9);
            Assert.AreEqual(KoiMath.WaveLength, Def("KOI_WL"), 1e-9); Assert.AreEqual(KoiMath.Length, Def("KOI_L"), 1e-9);

            var mb = KoiBuilder.Body();
            Mesh mesh = KoiBuilder.BuildMesh();
            var r = MeshValidation.Validate(mesh);
            Assert.IsTrue(r.Ok, r.ToString());
            long tris = WorldContext.TriangleCount(mesh);
            Assert.LessOrEqual(tris, 30000, "14.5.7: koi <= 30 k triangles");
            int bodyVerts = (KoiMath.BodySegments + 1) * KoiMath.RingVertices;
            double worst = 0.0, worstVeil = 0.0;
            Bounds ob = KoiBuilder.OrbitBounds;
            for (int ia = 0; ia < 12; ia++)
                for (int k = 0; k < 5; k++)
                {
                    double phi = (6.64 + 30.0 * ia + 3.3) * NocMath.Deg, tw = 4.0 * k + 0.37;
                    Vector4 prm = KoiShaderMath.Params(phi, tw);
                    for (int i = 0; i < mb.VertexCount; i++)
                    {
                        Vector2 uv0 = mb.UV0[i]; Vector4 uv1 = mb.UV1[i], uv2 = mb.UV2[i];
                        KoiShaderMath.Vertex(uv0, uv1, uv2, prm.x, prm.y, out Vector3 p, out _);
                        Assert.IsTrue(ob.Contains(p), FormattableString.Invariant($"vertex {i} at {p} leaves the koi bounds"));
                        if (i < bodyVerts)
                        {
                            D3 refP = KoiMath.RingPoint(uv0.x, uv0.y, phi, tw);
                            worst = Math.Max(worst, (KoiMath.ToVector3(refP) - p).magnitude);
                        }
                        else if (uv2.x > 0.5f)
                        {
                            // the veil vertex is KoiMath.VeilPoint(v, k): v from its arc length, k from uv2.y
                            double v = (uv0.x - KoiMath.VeilA0) / KoiMath.VeilLength;
                            D3 refV = KoiMath.VeilPoint(v, uv2.y, phi, tw);
                            worstVeil = Math.Max(worstVeil, (KoiMath.ToVector3(refV) - p).magnitude);
                        }
                    }
                }
            Debug.Log(FormattableString.Invariant($"[NOC] koi mesh {mesh.vertexCount} vertices, {tris} triangles; shader formulas vs KoiMath: body {worst * 100:F2} cm, veil {worstVeil * 100:F2} cm (60 poses)"));
            Assert.Less(worst, 0.05);
            Assert.Less(worstVeil, 0.05);
            UnityEngine.Object.DestroyImmediate(mesh);
        }

        [Test]
        public void KoiDriver_FollowsTheHeadTable_InCaptureMode()
        {
            bool cap = NocState.Capture;
            try
            {
                NocState.Capture = true;
                var d = new KoiDriver(null);
                for (int i = 0; i < KoiMath.TableS.Length; i++)
                {
                    var f = new FrameState { s = KoiMath.TableS[i] };
                    d.OnFrame(f);
                    double deg = d.HeadAzimuth / NocMath.Deg;
                    Assert.AreEqual(KoiMath.TableDeg[i], deg, 0.5, FormattableString.Invariant($"head azimuth at s {KoiMath.TableS[i]}"));
                    Assert.AreEqual((KoiMath.TableS[i] - 1260.0) / 1.5, d.WaveTime, 1e-9, "capture wave time (s - 1 260) / 1.5 (D-026)");
                }
                Assert.AreEqual(0.0, KoiDriver.LightRamp(1247.0), 1e-12);
                Assert.AreEqual(1.0, KoiDriver.LightRamp(1273.0), 1e-12);
            }
            finally { NocState.Capture = cap; }
        }

        [Test]
        public void Furniture_StaysOutOfTheLane_AndMeshesValidate()
        {
            var meshes = new List<KeyValuePair<string, KeyValuePair<MeshBuilder, Vector3>>>();
            void Add(string n, MeshBuilder mb, Vector3 o) => meshes.Add(new KeyValuePair<string, KeyValuePair<MeshBuilder, Vector3>>(n, new KeyValuePair<MeshBuilder, Vector3>(mb, o)));
            Add("tenement", OldQuarterBuilder.Tenement(), OldQuarterBuilder.Origin);
            Add("neighbours", OldQuarterBuilder.Neighbours(), OldQuarterBuilder.Origin);
            for (int p = 0; p < 4; p++) Add("room" + p, OldQuarterBuilder.Room(p), OldQuarterBuilder.Origin);
            Add("reveals", OldQuarterBuilder.TenementReveals(), OldQuarterBuilder.Origin);
            var props = new MeshBuilder(); var canvas = new MeshBuilder(); OldQuarterBuilder.RoomProps(props, canvas);
            Add("props", props, OldQuarterBuilder.Origin); Add("canvas", canvas, OldQuarterBuilder.Origin);
            var gj = new MeshBuilder(); OldQuarterBuilder.GoalJoinery(gj); Add("goalJoinery", gj, OldQuarterBuilder.Origin);
            var gs = new MeshBuilder(); OldQuarterBuilder.GoalSill(gs); Add("goalSill", gs, OldQuarterBuilder.Origin);
            var tp = new MeshBuilder(); var tg = new MeshBuilder(); OldQuarterBuilder.TenementPanes(tp, tg);
            Add("tenementPanes", tp, OldQuarterBuilder.Origin); Add("curtainedPanes", tg, OldQuarterBuilder.Origin);
            var tj = new MeshBuilder(); var ts = new MeshBuilder(); var td = new MeshBuilder(); OldQuarterBuilder.TenementTrim(tj, ts, td); OldQuarterBuilder.NeighbourTrim(tj, ts);
            Add("door", td, OldQuarterBuilder.Origin);
            Add("joinery", tj, OldQuarterBuilder.Origin); Add("stoneTrim", ts, OldQuarterBuilder.Origin);
            var lh = new MeshBuilder(); var lg = new MeshBuilder(); OldQuarterBuilder.Lantern(lh, lg);
            Add("lantern", lh, OldQuarterBuilder.Origin); Add("lanternGlass", lg, OldQuarterBuilder.Origin);
            var dp = new MeshBuilder(); OldQuarterBuilder.Downpipe(dp); Add("downpipe", dp, OldQuarterBuilder.Origin);
            var dark = new MeshBuilder(); var curtain = new MeshBuilder(); var lamp = new MeshBuilder();
            OldQuarterBuilder.Interior(dark, curtain, lamp);
            Add("furniture", dark, OldQuarterBuilder.Origin); Add("curtain", curtain, OldQuarterBuilder.Origin); Add("lamp", lamp, OldQuarterBuilder.Origin);
            Add("balcony", OldQuarterBuilder.BalconySlab(), OldQuarterBuilder.Origin);
            var rods = new MeshBuilder(); var rails = new MeshBuilder(); OldQuarterBuilder.Railing(rods, rails);
            Add("rods", rods, OldQuarterBuilder.Origin); Add("rails", rails, OldQuarterBuilder.Origin);
            var pot = new MeshBuilder(); var leaves = new MeshBuilder(); OldQuarterBuilder.Plant(pot, leaves);
            Add("pot", pot, OldQuarterBuilder.Origin); Add("leaves", leaves, OldQuarterBuilder.Origin);
            var panes = new MeshBuilder();
            OldQuarterBuilder.Windows(panes, HeartLayout.TenX0, HeartLayout.TenZ0, HeartLayout.TenZ1, 4);
            Add("panes", panes, OldQuarterBuilder.Origin);
            var pts = new List<Vector3>(); var yaws = new List<float>();
            PlazaFurnitureBuilder.BollardPoints(pts, yaws);
            var bollards = new MeshBuilder(); PromenadeBuilder.Bollards(bollards, pts, yaws, PlazaFurnitureBuilder.Origin);
            Add("plazaBollards", bollards, PlazaFurnitureBuilder.Origin);
            var lampPosts = new MeshBuilder(); var lampLenses = new MeshBuilder();
            for (int i = 0; i < HeartLayout.PlazaLampCount; i++)
                PlazaFurnitureBuilder.LampFixture(lampPosts, lampLenses, PlazaFurnitureBuilder.LampPosition(i) - PlazaFurnitureBuilder.Origin);
            Add("plazaLampPosts", lampPosts, PlazaFurnitureBuilder.Origin); Add("plazaLampLenses", lampLenses, PlazaFurnitureBuilder.Origin);
            var benchStone = new MeshBuilder(); var benchWood = new MeshBuilder(); PlazaFurnitureBuilder.Benches(benchStone, benchWood);
            Add("plazaBenches", benchStone, PlazaFurnitureBuilder.Origin); Add("plazaBenchSeats", benchWood, PlazaFurnitureBuilder.Origin);
            Assert.AreEqual(6.0, WorldBounds(lampPosts, PlazaFurnitureBuilder.Origin).max.y, 0.05, "lamps 6 m (14.5.7)");
            // HT-05: the lit bowl is 0.22 m tall from the side (a flat lens disc was a 1-2 px sliver from afar)
            Bounds lensB = WorldBounds(lampLenses, PlazaFurnitureBuilder.Origin);
            Assert.GreaterOrEqual(lensB.size.y, 0.2, "lamp bowl height");
            Assert.AreEqual(HeartLayout.LampBowlY, lensB.min.y, 0.01);
            long tris = 0;
            foreach (var kv in meshes)
            {
                Beat06SignsTests.AssertClearOfLane(kv.Key, kv.Value.Key, kv.Value.Value, 1400);
                Mesh m = kv.Value.Key.ToMesh(kv.Key);
                var r = MeshValidation.Validate(m);
                Assert.IsTrue(r.Ok, r.ToString());
                tris += WorldContext.TriangleCount(m);
                UnityEngine.Object.DestroyImmediate(m);
            }
            // the rest of the beat: floor, spire, towers, koi
            tris += HeartFloorBuilder.Slab().TriangleCount + HeartFloorBuilder.InlayFan().TriangleCount + SpireBuilder.Shaft().TriangleCount
                + SpireBuilder.Drums().TriangleCount + SpireBuilder.Plinth().TriangleCount + SpireBuilder.Hoops().TriangleCount;
            var finsT = new MeshBuilder(); var stripsT = new MeshBuilder();
            for (int j = 0; j < HeartLayout.FinCount; j++) { SpireBuilder.Fin(finsT, j); SpireBuilder.Strip(stripsT, j); }
            tris += finsT.TriangleCount + stripsT.TriangleCount;
            foreach (double y in SpireBuilder.HoopHeights()) tris += SpireBuilder.RingLight(y).TriangleCount;
            int culled = 0; var mbT = new MeshBuilder();
            tris += 2 * CityMesher.Append(mbT, RingTowersBuilder.Towers(), 0, 0, ref culled);
            tris += KoiBuilder.Body().TriangleCount;
            int lights = HeartLayout.PlazaLampCount + 4;   // the goal light, the room light (INT-08, HT), the entrance light, the empty room's lamp (D-034 6)
            var eS = new MeshBuilder(); var eW = new MeshBuilder(); var eT = new MeshBuilder(); var eB = new MeshBuilder();
            OldQuarterBuilder.EmptyRoom(eS, eW, eT, eB);
            tris += eS.TriangleCount + eW.TriangleCount + eT.TriangleCount + eB.TriangleCount;
            Debug.Log(FormattableString.Invariant($"[NOC] Beat07: about {tris} triangles (budget 560 k), {lights} real lights (14.2.3 row 7: 32), {pts.Count} bollards"));
            Assert.LessOrEqual(tris, 560000);
            Assert.LessOrEqual(lights, 32);
        }

        [Test]
        public void PlazaInlay_SameTopFan_RingsSeamsFade()
        {
            // the overlay is the slab's top fan: identical vertices and triangles (ZTest LEqual passes exactly on the floor)
            var slab = HeartFloorBuilder.Slab();
            var fan = HeartFloorBuilder.InlayFan();
            Assert.Greater(fan.VertexCount, 900, "a fine fan (the outline's 961 points and the centre)");
            for (int i = 0; i < fan.VertexCount; i++) Assert.AreEqual(slab.Positions[i], fan.Positions[i], "fan vertex " + i);
            Mesh ms = slab.ToMesh("slab"), mf = fan.ToMesh("fan");
            int[] ts = ms.triangles, tf = mf.triangles;
            for (int i = 0; i < tf.Length; i++) Assert.AreEqual(ts[i], tf[i], "fan index " + i);
            Assert.IsTrue(MeshValidation.Validate(mf).Ok, MeshValidation.Validate(mf).ToString());
            foreach (var v in mf.vertices) Assert.AreEqual(0f, v.y, 0f);
            UnityEngine.Object.DestroyImmediate(ms); UnityEngine.Object.DestroyImmediate(mf);

            double fine = 0.002;   // 2 mm per pixel: every line resolved
            Vector2 C(double r, double degFromEast, double fwR, double fwT)
            {
                double a = degFromEast * Math.PI / 180.0;
                return HeartFloorBuilder.InlayCoverage(HeartLayout.SX + r * Math.Cos(a), HeartLayout.SZ + r * Math.Sin(a), fwR, fwT);
            }
            int rings = 0;
            for (double r = 20.0; r <= 400.0; r += 20.0) if (C(r, 3.75, fine, fine).x > 0.99f) rings++;
            Assert.AreEqual(17, rings, "17 brass rings 60 .. 380 m");
            Assert.AreEqual(0f, C(40.0, 3.75, fine, fine).x, 1e-6, "no ring under the podium");
            Assert.AreEqual(0f, C(390.0, 3.75, fine, fine).x, 1e-6, "between the rings: the floor shows");
            Assert.AreEqual(1f, C(300.0 + 0.05, 3.75, fine, fine).x, 1e-6, "inside the 0.12 m line");
            Assert.AreEqual(0f, C(300.0 + 0.07, 3.75, fine, fine).x, 1e-6, "outside the 0.12 m line");
            // a sub-pixel ring keeps its energy (w / footprint) while its pitch is still >= 10 px
            Assert.AreEqual(0.12 / 1.2, C(300.0, 3.75, 1.2, fine).x, 1e-4, "energy-kept ring at 1.2 m per pixel");
            // the pitch fade: 20 m / fwR px; none below 6 px, full above 10 px
            Assert.AreEqual(0f, C(300.0, 3.75, 20.0 / 5.9, fine).x, 1e-6);
            Assert.Greater(C(300.0, 3.75, 20.0 / 8.0, fine).x, 0f);
            // 48 seams every 7.5 deg from R 58; one runs along the walk (bearing 180 deg, 0.12 m north of the east leg)
            int seams = 0;
            for (int k = 0; k < 96; k++) if (C(250.0, k * 3.75, fine, fine).y > 0.99f) seams++;
            Assert.AreEqual(48, seams, "seams every 7.5 deg");
            Assert.AreEqual(1f, C(250.0, 180.0, fine, fine).y, 1e-6, "the leading seam under the walk");
            Assert.AreEqual(1f, C(250.0, -180.0, fine, fine).y, 1e-6, "atan2's cut is a seam too");
            Assert.AreEqual(0f, C(50.0, 180.0, fine, fine).x, 1e-6, "no seam on the podium");
            double zSeam = HeartLayout.SZ - HeartLayout.EastLegZ;
            Debug.Log(FormattableString.Invariant($"[NOC] inlay: {rings} rings, {seams} seams, the leading seam {zSeam:F2} m north of the walk's centre line"));
            Assert.Less(Math.Abs(zSeam), 0.5);
            // the aged brass under the SignWhite lamps stays below the warm-blob saturation 0.5
            Color lamp = NocturnePalette.Linear(NocPal.SignWhite), b = HeartLayout.InlayBrass;
            Color glint = new Color(Mathf.LinearToGammaSpace(b.r * lamp.r), Mathf.LinearToGammaSpace(b.g * lamp.g), Mathf.LinearToGammaSpace(b.b * lamp.b));
            Color.RGBToHSV(glint, out float gh, out float gs, out _);
            Debug.Log(FormattableString.Invariant($"[NOC] brass glint under SignWhite: hue {gh * 360f:F1} sat {gs:F3}"));
            Assert.Less(gs, 0.5f);
        }

        [Test]
        public void Tenement_DownpipeClearOfOpeningsAndBelowTheRoof()
        {
            // HT-03: the downpipe (clips +-(R + 0.012), hopper +-0.14) stays clear of every opening with its sills and lintels
            double z = HeartLayout.DownpipeZ, half = 0.14;
            foreach (var o in OldQuarterBuilder.Openings())
            {
                double over = Math.Max(HeartLayout.LintelOver, HeartLayout.DarkSillOver);
                bool clear = z + half < o.z0 - over || z - half > o.z1 + over;
                Assert.IsTrue(clear, FormattableString.Invariant($"downpipe z {z} vs opening z {o.z0:F2}-{o.z1:F2} (storey {o.storey})"));
            }
            var mb = new MeshBuilder();
            OldQuarterBuilder.Downpipe(mb);
            Bounds b = WorldBounds(mb, OldQuarterBuilder.Origin);
            Assert.LessOrEqual(b.max.y, HeartLayout.TenH - 0.2, "the hopper stays under the roof edge (KS19 silhouette unchanged)");
            Assert.LessOrEqual(b.max.x, HeartLayout.TenX0 + 0.02, "no part inside the wall beyond the outlet");
            Assert.GreaterOrEqual(b.min.x, HeartLayout.TenX0 - 0.35, "stands off the face by centimetres");
            Assert.GreaterOrEqual(b.min.y, -1e-3, "the shoe stays above the plaza");
            // below the KS19 roof silhouette: the hopper's front top edge seen from the KS19 eye lies under the roof edge
            double ex = 1148.18, ey = 1.7;
            double eRoof = Math.Atan2(HeartLayout.TenH - ey, HeartLayout.TenX0 - ex), eHop = Math.Atan2(b.max.y - ey, b.min.x - ex);
            Debug.Log(FormattableString.Invariant($"[NOC] downpipe z {z}, y {b.min.y:F2}..{b.max.y:F2}, x {b.min.x:F3}..{b.max.x:F3}; KS19 elevation roof {eRoof * 180 / Math.PI:F2} deg, hopper {eHop * 180 / Math.PI:F2} deg"));
            Assert.Less(eHop, eRoof);
            Mesh m = mb.ToMesh("downpipe");
            Assert.IsTrue(MeshValidation.Validate(m).Ok, MeshValidation.Validate(m).ToString());
            UnityEngine.Object.DestroyImmediate(m);
        }

        [Test]
        public void SpireStrips_MirrorSources_SteadyAcrossThePlaza()
        {
            NocWetRegistry.Clear();
            try
            {
                SpireBuilder.AddStripSources(new Color(1.0f, 1.4f, 1.5f));
                int nf = SpireBuilder.MirrorFins.Length;
                Assert.AreEqual(nf * SpireBuilder.MirrorS.Length, NocWetRegistry.TotalSources);
                double minW = double.MaxValue, maxW = 0.0;
                int maxN = 0;
                for (double s = 1240.0; s <= 1500.0; s += 0.25)
                {
                    int n = NocWetRegistry.Collect(s);
                    maxN = Math.Max(maxN, n);
                    double sum = 0.0;
                    for (int i = 0; i < n; i++) { NocWetRegistry.GetSource(i, out _, out _, out Vector4 l); sum += l.y; }
                    double w = sum / (1.4 * nf);
                    minW = Math.Min(minW, w); maxW = Math.Max(maxW, w);
                    Assert.AreEqual(0, NocWetRegistry.Dropped);
                }
                Debug.Log(FormattableString.Invariant($"[NOC] spire strip mirror: weight {minW:F6}..{maxW:F6} over s 1240-1500, at most {maxN} sources"));
                Assert.AreEqual(1.0, minW, 1e-5, "the copies' crossfades sum to 1 (steady streaks)");
                Assert.AreEqual(1.0, maxW, 1e-5);
                Assert.LessOrEqual(maxN, 2 * nf);
                // the chords start on the strip at the drum and stay within 6 m of it up to MirrorTop
                double y0 = HeartLayout.FlareY0 + 0.3, r0 = HeartLayout.FinOuter(y0), r1 = HeartLayout.FinOuter(SpireBuilder.MirrorTop);
                for (double y = y0; y <= SpireBuilder.MirrorTop; y += 1.0)
                {
                    double rc = r0 + (r1 - r0) * (y - y0) / (SpireBuilder.MirrorTop - y0);
                    Assert.Less(Math.Abs(rc - HeartLayout.FinOuter(y)), 6.0, FormattableString.Invariant($"chord vs strip at y {y:F0}"));
                }
            }
            finally { NocWetRegistry.Clear(); }
        }

        [Test]
        public void Spire_HoopsRingLightsFlarePodium()
        {
            var hy = SpireBuilder.HoopHeights();
            Assert.AreEqual(11, hy.Count, "ring lights every 40 m, 40 .. 440");
            for (int k = 0; k < hy.Count; k++) Assert.AreEqual(40.0 * (k + 1), hy[k], 1e-9);
            // hoops and ring lights inside the envelope R(y) + 4 (the fins' outer edge above the flare)
            foreach (var mb in new[] { SpireBuilder.Hoops(), SpireBuilder.RingLight(40.0), SpireBuilder.RingLight(440.0) })
            {
                foreach (var q in mb.Positions)
                {
                    double r = Math.Sqrt(q.x * q.x + q.z * q.z);
                    Assert.LessOrEqual(r, HeartLayout.SpireRadius(q.y) + HeartLayout.FinDepth + 0.05, FormattableString.Invariant($"hoop vertex r {r:F2} at y {q.y:F1}"));
                }
                Mesh m = mb.ToMesh("hoop");
                Assert.IsTrue(MeshValidation.Validate(m).Ok, MeshValidation.Validate(m).ToString());
                UnityEngine.Object.DestroyImmediate(m);
            }
            // the buttress flare lands on the lowest drum and is gone at 96 m; above it the fins keep R(y) + 4
            Assert.AreEqual(HeartLayout.SpireRadius(6.0) + 13.5, HeartLayout.FinOuter(6.0), 1e-9);
            Assert.Less(HeartLayout.FinOuter(5.9), HeartLayout.DrumR[0], "the fin's foot stands on the R 48 drum");
            for (double y = 96.0; y <= 560.0; y += 4.0) Assert.AreEqual(HeartLayout.SpireRadius(y) + 4.0, HeartLayout.FinOuter(y), 1e-9);
            for (double y = 6.0; y < 96.0; y += 1.0) Assert.GreaterOrEqual(HeartLayout.FinOuter(y), HeartLayout.FinOuter(y + 1.0), "monotone flare");
            var fins = new MeshBuilder();
            for (int j = 0; j < HeartLayout.FinCount; j++) SpireBuilder.Fin(fins, j);
            double yLow = double.MaxValue;
            foreach (var q in fins.Positions)
            {
                yLow = Math.Min(yLow, q.y);
                double r = Math.Sqrt(q.x * q.x + q.z * q.z);
                if (q.y >= 96.0) Assert.LessOrEqual(r, HeartLayout.SpireRadius(q.y) + 4.0 + 0.61, "fin within the envelope above the flare");
            }
            Assert.AreEqual(5.9, yLow, 1e-4, "fins start on the lowest drum (y 6, sunk 0.1)");
            // the podium: three steps R 56 / 53 / 50.5 of 0.16 m around the drums
            Bounds pb = WorldBounds(SpireBuilder.Plinth(), Vector3.zero);
            Assert.AreEqual(56.0, 0.5 * pb.size.x, 0.05);
            Assert.AreEqual(0.48, pb.max.y, 1e-4);
            Mesh pm = SpireBuilder.Plinth().ToMesh("plinth");
            Assert.IsTrue(MeshValidation.Validate(pm).Ok, MeshValidation.Validate(pm).ToString());
            UnityEngine.Object.DestroyImmediate(pm);
            // breathing: 0.55 .. 1 at 0.1 Hz (period 10 s), ring k lagging 0.5 k rad; the eleven rings' sum within +-4.2 %
            Assert.AreEqual(1.0, SpireDriver.Breath(0, 0.0), 1e-9);
            Assert.AreEqual(0.55, SpireDriver.Breath(0, 5.0), 1e-9);
            Assert.AreEqual(SpireDriver.Breath(3, 1.7), SpireDriver.Breath(3, 11.7), 1e-9, "period 10 s");
            double sMin = double.MaxValue, sMax = 0.0, slope = 0.0;
            for (double t = 0.0; t < 10.0; t += 0.01)
            {
                double sum = 0.0;
                for (int k = 0; k < hy.Count; k++)
                {
                    sum += SpireDriver.Breath(k, t);
                    slope = Math.Max(slope, Math.Abs(SpireDriver.Breath(k, t + 0.01) - SpireDriver.Breath(k, t)) / 0.01);
                }
                sMin = Math.Min(sMin, sum); sMax = Math.Max(sMax, sum);
            }
            double mean = 0.5 * (sMin + sMax), dev = 0.5 * (sMax - sMin) / mean;
            Debug.Log(FormattableString.Invariant($"[NOC] ring lights: sum {sMin:F3}..{sMax:F3} (+-{100 * dev:F2} %), steepest {slope:F3} /s"));
            Assert.Less(dev, 0.042);
            Assert.Less(slope, 0.15, "no ring changes by more than 15 % of its peak per second");
        }

        [Test]
        public void NoAmber_ButTheWindow()
        {
            // hue (deg) of every emitter colour of beat 7 except the goal: lamp heads WinCold, spire strips cyan-white, the koi
            double Hue(Color c)
            {
                Color.RGBToHSV(new Color(Mathf.LinearToGammaSpace(c.r), Mathf.LinearToGammaSpace(c.g), Mathf.LinearToGammaSpace(c.b)), out float h, out float s, out _);
                return h * 360.0;
            }
            var emitters = new[]
            {
                // Look v2 (PA, LOOK_V2 6 / 10): the plaza lamps are SignWhite; the koi patches are vermilion (hue 18, outside 20-55)
                NocturnePalette.Linear(NocPal.SignWhite), G4Kit.Mix(NocturnePalette.Linear(NocPal.NeonCyan), NocturnePalette.Linear(NocPal.SignWhite), 0.5),
                NocturnePalette.Linear(NocPal.KoiBody), NocturnePalette.Linear(NocPal.KoiGlow), NocturnePalette.Linear(NocPal.KoiPatch),
            };
            foreach (var c in emitters)
            {
                double h = Hue(c);
                Assert.IsFalse(h >= 20.0 && h <= 55.0, FormattableString.Invariant($"emitter hue {h:F1} deg is amber"));
            }
            double g = Hue(NocturnePalette.Linear(NocPal.GoalLamp));
            Debug.Log(FormattableString.Invariant($"[NOC] goal lamp hue {g:F1} deg (14.5.7: 28 - 48)"));
            Assert.That(g, Is.InRange(28.0, 48.0));
        }
    }
}
