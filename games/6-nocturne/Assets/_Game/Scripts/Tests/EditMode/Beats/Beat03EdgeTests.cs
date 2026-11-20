// Beat03EdgeTests: EditMode acceptance checks of beat 3, The Edge (MASTER_PROMPT 14.5.3 Acceptance (1), (4), (6); 14.10 T-PLAN,
// the KS09 / KS10 frame shares of the deck plan; the Phase-3 geometry agreements of docs/logs/G2.md):
//   T-PLAN half-widths at x 235..370; the rift widths of 14.5.3; the cross-bridge spans; the face profile differs from a plane by
//   >= 30 m at three heights and is the plain plane above the first step (tube portals, bridges at -180 / -300); every part passes
//   MeshValidation; <= 520 k triangles; deck top exactly at y 0 from x 215.3 to 375.3 (5.0 m at the tip); nothing of the beat
//   stands inside the lane + 0.2 m camera sphere; face A has nothing above y 0 inside the canyon z 120..270, face B nothing above
//   y -0.5 inside the garden notch and nothing on the promenade x 874..914; x >= 372 above the deck is free for gate block A;
//   24 realtime lights and 28 wet sources (<= 32 active), 4 wet walls; the portal panel hole meets the collar.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class Beat03EdgeTests
    {
        static List<EdgePart> s_parts;
        static List<EdgeLight> s_lights;

        static List<EdgePart> Parts()
        {
            if (s_parts == null) { s_lights = new List<EdgeLight>(); s_parts = Beat03_Edge.BuildParts(s_lights); }
            return s_parts;
        }

        static IEnumerable<Vector3> World(EdgePart p) { foreach (var v in p.Mb.Positions) yield return v + p.Origin; }

        [Test]
        public void TPlan_HalfWidths()
        {
            double[] x = { 235, 265, 295, 315, 340, 355, 363.3, 370 };
            double[] w = { 72.1, 39.5, 6.5, 3.2, 3.2, 3.1, 2.8, 2.6 };
            var sb = new System.Text.StringBuilder("T-PLAN w(x):");
            for (int i = 0; i < x.Length; i++)
            {
                double v = EdgeDims.TongueHalfWidth(x[i]);
                sb.Append(FormattableString.Invariant($" {x[i]}:{v:F3}"));
                Assert.AreEqual(w[i], v, 0.1, FormattableString.Invariant($"w({x[i]})"));
                Assert.AreEqual(NocLanes.TongueHalfWidth(x[i]), v, 1e-9, "EdgeDims and NocLanes share the 14.4 formula");
            }
            Assert.AreEqual(75.0, EdgeDims.TongueHalfWidth(215.3), 1e-9);
            Assert.AreEqual(75.0, EdgeDims.TongueHalfWidth(225.3), 1e-9);
            Assert.AreEqual(2.5, EdgeDims.TongueHalfWidth(375.3), 1e-9);
            Debug.Log(sb.ToString());
        }

        [Test]
        public void Rift_WidthsAndBridgeSpans()
        {
            for (double z = -105; z <= 495; z += 15) Assert.AreEqual(600.0, EdgeDims.XB(z) - EdgeDims.XA(z), 1e-6, "neck 600 m");
            Assert.AreEqual(1007.0, EdgeDims.XB(-640) - EdgeDims.XA(-640), 1.0, "1 007 m at z -640");
            Assert.AreEqual(2000.0, EdgeDims.XB(-1700) - EdgeDims.XA(-1700), 1e-6, "2 km beyond z -1 605");
            Assert.AreEqual(2000.0, EdgeDims.XB(2600) - EdgeDims.XA(2600), 1e-6, "2 km beyond z 2 500");
            Assert.AreEqual((double)CityField.GulfXA(777), EdgeDims.XA(777), 1e-9, "the city's Gulf exclusion uses the same faces");
            Assert.AreEqual((double)CityField.GulfXB(1777), EdgeDims.XB(1777), 1e-9);
            double[] spec = { 699, 1155, 1966, 1007, 1980, 2000 };
            var sb = new System.Text.StringBuilder("bridge spans (plain / at height):");
            for (int i = 0; i < spec.Length; i++)
            {
                double z = EdgeDims.BridgeZ[i], plain = EdgeDims.XB(z) - EdgeDims.XA(z);
                Assert.AreEqual(spec[i], plain, 1.0, "14.5.3 span " + i);
                double real = EdgeDims.BridgeSpan(i);
                sb.Append(FormattableString.Invariant($" {spec[i]}: {plain:F1} / {real:F1};"));
                if (EdgeDims.LevelOf(EdgeDims.BridgeY[i]) == 0) Assert.AreEqual(plain, real, 1e-6, "above the first step the bridge meets the plain faces");
            }
            Debug.Log(sb.ToString());
        }

        [Test]
        public void FaceProfile_DiffersFromPlane()
        {
            // 14.5.3 Acceptance (4): >= 30 m at three heights; the plain plane above the first step (y -360, G2-D1)
            foreach (int face in new[] { 0, 1 })
                foreach (double z in new[] { -2600.0, -640.0, 0.0, 195.0, 400.0, 1380.0, 2300.0 })
                {
                    Assert.AreEqual(0.0, EdgeDims.ProfileOffset(face, -142.0, z), 1e-9, "lower tube height on the plain face");
                    Assert.AreEqual(0.0, EdgeDims.ProfileOffset(face, -300.0, z), 1e-9);
                    double a = EdgeDims.ProfileOffset(face, -400.0, z), b = EdgeDims.ProfileOffset(face, -800.0, z), c = EdgeDims.ProfileOffset(face, -1200.0, z);
                    Assert.GreaterOrEqual(a, 30.0); Assert.GreaterOrEqual(b, 60.0); Assert.GreaterOrEqual(c, 90.0);
                    Assert.LessOrEqual(a, 120.0 + 1e-9);
                }
            for (int k = 1; k < EdgeDims.TerraceLevels; k++)
            {
                double st = EdgeDims.TerraceStep(0, k, 0);
                Assert.That(st >= 30.0 && st <= 120.0, "step 30 + 90 hash");
            }
            Assert.AreEqual(EdgeDims.FaceY0, EdgeDims.LevelBottom(EdgeDims.TerraceLevels - 1), 1e-9);
            Assert.AreEqual(1, EdgeDims.LevelOf(-361.0));
            Assert.AreEqual(2, EdgeDims.LevelOf(-661.0));
            Assert.AreEqual(EdgeDims.TerraceLevels - 1, EdgeDims.LevelOf(-3299.0));
        }

        [Test]
        public void Meshes_ValidAndWithinBudget()
        {
            long tris = 0;
            int bad = 0;
            var sb = new System.Text.StringBuilder();
            var byMat = new Dictionary<EdgeMat, long>();
            foreach (var p in Parts())
            {
                if (p.Mb.VertexCount == 0) continue;
                Mesh m = p.Mb.ToMesh(p.Name);
                var r = MeshValidation.Validate(m);
                if (!r.Ok) { bad++; sb.AppendLine(r.Describe()); }
                long t = p.Mb.TriangleCount * (p.Mat == EdgeMat.Glass ? 2 : 1);
                tris += t;
                byMat.TryGetValue(p.Mat, out long acc); byMat[p.Mat] = acc + t;
                UnityEngine.Object.DestroyImmediate(m);
            }
            var line = new System.Text.StringBuilder(FormattableString.Invariant($"Beat 3 triangles {tris}:"));
            foreach (var kv in byMat) line.Append(FormattableString.Invariant($" {kv.Key} {kv.Value};"));
            Debug.Log(line.ToString());
            Assert.AreEqual(0, bad, sb.ToString());
            Assert.LessOrEqual(tris, 520000, "14.5.3 Acceptance (6)");
        }

        [Test]
        public void Deck_TopAtZeroAndSeams()
        {
            double xmin = double.MaxValue, xmax = double.MinValue, tipHalf = 0, rootHalf = 0;
            foreach (var p in Parts())
            {
                if (p.Mat != EdgeMat.Floor) continue;
                foreach (var v in World(p))
                {
                    Assert.AreEqual(0f, v.y, 1e-5f, "walkable top at y 0 (agreement 1)");
                    xmin = Math.Min(xmin, v.x); xmax = Math.Max(xmax, v.x);
                    if (Math.Abs(v.x - 375.3) < 1e-3) tipHalf = Math.Max(tipHalf, Math.Abs(v.z - 195.0) + EdgeDims.DeckBevel);
                    if (Math.Abs(v.x - 215.3) < 1e-3) rootHalf = Math.Max(rootHalf, Math.Abs(v.z - 195.0) + EdgeDims.DeckBevel);
                }
            }
            Assert.AreEqual(215.3, xmin, 1e-3, "tongue starts where G1's street floor ends (agreement 2)");
            Assert.AreEqual(375.3, xmax, 1e-3, "tongue ends where G3's bridge deck starts (agreement 3)");
            Assert.AreEqual(2.5, tipHalf, 0.01, "5.0 m at the tip");
            Assert.AreEqual(75.0, rootHalf, 0.01, "150 m at the root (the boulevard street)");
            Assert.AreEqual(1.0, EdgeDims.DeckThickness, 1e-9, "at least 1 m thick");
        }

        [Test]
        public void Lane_NeverBlocked()
        {
            // every vertex of the near furniture and rails lies outside lane + camera sphere (0.2) + 3 cm, and outside the collider
            double worst = double.MaxValue; string where = "";
            foreach (var p in Parts())
            {
                if (p.Mat == EdgeMat.Floor || p.Mat == EdgeMat.Facade || p.Mat == EdgeMat.Stone || p.Name.StartsWith("Cross") || p.Name.StartsWith("Traffic") || p.Name.StartsWith("Portal")) continue;
                foreach (var v in World(p))
                {
                    if (v.x < 215.3f || v.x > 375.3f || v.y > 2.0f || v.y < 0.001f) continue;   // AB: at or below the walk plane nothing blocks (the glass edge's pane and frame)
                    double s = EdgeDims.SOfX(v.x), lane = NocLanes.LaneHalfWidth(s);
                    double m = Math.Abs(v.z - 195.0) - (lane + NocturneConstants.CameraSphere);
                    if (m < worst) { worst = m; where = FormattableString.Invariant($"{p.Name} at ({v.x:F2}, {v.y:F2}, {v.z:F2}), lane {lane:F3}"); }
                }
            }
            Debug.Log(FormattableString.Invariant($"lane clearance: worst {worst:F3} m beyond lane + 0.2 ({where})"));
            Assert.GreaterOrEqual(worst, 0.03, where);
        }

        [Test]
        public void Agreements_CanyonNotchPromenadeGate()
        {
            foreach (var p in Parts())
            {
                bool faceA = p.Name.StartsWith("FaceA"), faceB = p.Name.StartsWith("FaceB");
                foreach (var v in World(p))
                {
                    if (faceA && v.y > 0.01f) Assert.That(v.z <= 120.01f || v.z >= 269.99f, "face A above y 0 only outside z 120..270: " + v);
                    if (faceB && v.y > -0.49f && v.x > 815.31f && v.x < 913.99f) Assert.That(v.z <= 115.01f || v.z >= 274.99f, "garden notch open: " + v);
                    if (faceB && v.y > 0.01f && v.z >= 275f && v.z <= 520f) Assert.That(v.x <= 874.0f, "promenade x 874..914, z 275..520 (G4) free: " + v);
                    if (faceA || faceB) { Assert.LessOrEqual(v.y, 560.001f); Assert.GreaterOrEqual(v.y, -3300.001f); }
                    if (!faceA && !faceB && v.y > 0.01f && v.x > 372f && Math.Abs(v.z - 195f) < 20f)
                        Assert.Fail("x >= 372 above the deck belongs to gate block A (G3): " + p.Name + " " + v);
                }
            }
            Assert.AreEqual(EdgeDims.FaceXA, EdgeDims.FrontX(0, 100, 0), 1e-9);
            Assert.AreEqual(EdgeDims.FaceXB, EdgeDims.FrontX(1, -142, 209), 1e-9);
        }

        [Test]
        public void Lights_KitAndWetRegistry()
        {
            Parts();
            int realtime = 0;
            foreach (var l in s_lights) if (l.Realtime) realtime++;
            Assert.AreEqual(24, realtime, "14.2.3 row 3: at most 24 realtime local lights");
            Assert.AreEqual(EdgeDims.LampCount + EdgeDims.BalustradeLights, s_lights.Count);
            NocWetRegistry.Clear();
            foreach (var l in s_lights) NocWetRegistry.AddSource(l.Pos, l.Pos, l.Rho, l.Linear * l.Radiance);
            Beat03_Edge.RegisterWalls(null);
            int maxActive = NocWetRegistry.MaxActive();
            NocWetRegistry.Collect(480.0);
            int walls = NocWetRegistry.ActiveWalls;
            NocWetRegistry.Collect(405.0);
            int wallsAtBoundary = NocWetRegistry.ActiveWalls;
            NocWetRegistry.Clear();
            Debug.Log(FormattableString.Invariant($"beat 3 wet sources {s_lights.Count}, max active {maxActive}, walls at s 480 {walls}, at s 405 {wallsAtBoundary}"));
            Assert.LessOrEqual(maxActive, 32);
            Assert.AreEqual(4, walls);
            Assert.LessOrEqual(wallsAtBoundary, 2, "face B walls start after G1's boulevard walls have faded (<= 4 in all)");
        }

        [Test]
        public void Portal_PanelMeetsCollar()
        {
            foreach (int face in new[] { 0, 1 })
            {
                double xf = EdgeDims.FaceX(face, EdgeDims.LowerTubeZ);
                int onRing = 0;
                foreach (var p in Parts())
                {
                    if (!p.Name.StartsWith(face == 0 ? "FaceA" : "FaceB")) continue;
                    foreach (var v in World(p))
                    {
                        if (Math.Abs(v.x - xf) > 1e-3) continue;
                        double r = Math.Sqrt(Math.Pow(v.y - EdgeDims.LowerTubeY, 2) + Math.Pow(v.z - EdgeDims.LowerTubeZ, 2));
                        if (Math.Abs(r - EdgeDims.PortalOuter) < 1e-3) onRing++;
                    }
                }
                Assert.AreEqual(EdgeDims.PortalSegments, onRing, "panel hole r 6.3 on face " + face);
                Assert.Greater(EdgeDims.PortalInner, EdgeDims.TubeRadius + 0.2, "tunnel clear of G3's r 5 tube");
            }
        }

        // ------------------------------------------------------------------ KS09 / KS10 frame shares (deck plan, glass transparent)
        static void Shares(double ex, double ez, double yaw, double pitch, double vfov, out double sky, out double pier, out double drop)
        {
            const int W = 192, H = 108;
            double th = Math.Tan(vfov * 0.5 * Math.PI / 180.0), tw = th * 16.0 / 9.0;
            double cy = Math.Cos(yaw * Math.PI / 180.0), sy = Math.Sin(yaw * Math.PI / 180.0), cp = Math.Cos(pitch * Math.PI / 180.0), sp = Math.Sin(pitch * Math.PI / 180.0);
            double[] f = { sy * cp, sp, cy * cp }, r = { cy, 0, -sy }, u = { -sy * sp, cp, -cy * sp };
            int a = 0, b = 0, c = 0;
            for (int j = 0; j < H; j++)
                for (int i = 0; i < W; i++)
                {
                    double vv = (1.0 - 2.0 * (j + 0.5) / H) * th, uu = (2.0 * (i + 0.5) / W - 1.0) * tw;
                    double dx = f[0] + uu * r[0] + vv * u[0], dy = f[1] + uu * r[1] + vv * u[1], dz = f[2] + uu * r[2] + vv * u[2];
                    if (dy >= 0) { a++; continue; }
                    double t = -NocturneConstants.EyeHeight / dy, x = ex + dx * t, z = ez + dz * t;
                    bool deck = (x >= EdgeDims.TongueX0 && x <= EdgeDims.TongueX1 && Math.Abs(z - 195.0) <= EdgeDims.TongueHalfWidth(x))
                             || (x > EdgeDims.TongueX1 && x <= 695.3 && Math.Abs(z - 195.0) <= 2.5);
                    if (deck) b++; else c++;
                }
            double n = W * H;
            sky = 100.0 * a / n; pier = 100.0 * b / n; drop = 100.0 * c / n;
        }

        [Test]
        public void KeyShots_DeckPlanShares()
        {
            Shares(335.3, 195.0, 20, -18, 68, out double s9, out double p9, out double d9);
            Shares(363.3, 195.0, 20, -30, 68, out double s10, out double p10, out double d10);
            Debug.Log(FormattableString.Invariant($"KS09 sky/pier/drop {s9:F1}/{p9:F1}/{d9:F1} % (spec 26/39/36); KS10 {s10:F1}/{p10:F1}/{d10:F1} % (spec 7/50/43)"));
            Assert.AreEqual(26, s9, 2.0); Assert.AreEqual(39, p9, 3.0); Assert.AreEqual(36, d9, 3.0);
            Assert.AreEqual(7, s10, 2.0); Assert.AreEqual(50, p10, 3.0);
            Assert.GreaterOrEqual(d10, 40.0, "14.5.3 Acceptance (2): KS10 drop >= 40 %");
        }
    }
}
