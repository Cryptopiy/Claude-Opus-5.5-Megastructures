// Beat04Tests: the pure geometry of beat 4 (MASTER_PROMPT 14.5.4 acceptance 1 and 3, 14.10.1 T-SCALE / T-MESH, Addendum A1
// 1.4 reef channel): the Colossopod unit, the belt, the six islets, the channel, the sea-face clearance, the gauges, the
// build-time clipping, the surf density and the crest band's lane clearance. Owner: the reef builder (Wave X).
using System.Collections.Generic;
using Megastructure.Beats;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class Beat04Tests
    {
        [Test]
        public void Unit_IsFortyMetresTipToTipWithTaperedArms()
        {
            float L = ReefLayout.ArmLength;
            for (int a = 0; a < 4; a++)
            {
                Assert.AreEqual(1f, ReefLayout.ArmDir[a].magnitude, 1e-5f, "arm " + a + " unit length");
                for (int b = a + 1; b < 4; b++)
                {
                    float tt = (ReefLayout.ArmDir[a] * L - ReefLayout.ArmDir[b] * L).magnitude;
                    Assert.AreEqual(40.0f, tt, 0.5f, $"tip to tip {a}-{b}");
                    Assert.AreEqual(109.47f, Vector3.Angle(ReefLayout.ArmDir[a], ReefLayout.ArmDir[b]), 0.05f, "tetrahedral angle");
                }
            }
            Assert.AreEqual((float)BulwarkConst.UnitArmRadius0, ReefLayout.ArmRadiusAt(0f), 1e-4f);
            Assert.AreEqual((float)BulwarkConst.UnitArmRadius1, ReefLayout.ArmRadiusAt(L), 1e-4f);
            Assert.AreEqual((float)BulwarkConst.UnitSize, ReefLayout.TipToTip, 1e-4f);
        }

        [Test]
        public void Belt_HasFortyToEightyAwashUnitsWithTipsBelowMinusOnePointFive()
        {
            int belt = 0;
            foreach (var u in ReefLayout.All())
            {
                if (u.kind != ReefLayout.Kind.Belt) continue;
                if (u.cellJ >= ReefLayout.SouthRows) continue;
                belt++;
                Assert.LessOrEqual(u.TopY, (float)BulwarkConst.BeltTipYMax + 1e-3f, "belt tip above -1.5");
                Assert.GreaterOrEqual(u.TopY, (float)BulwarkConst.BeltTipYMin - 1e-3f, "belt tip below -14");
            }
            // every layer-0 cell of the belt (also the ones clipped away as wholly under the troughs)
            int cells = 0;
            for (int j = 0; j < ReefLayout.SouthRows; j++)
                for (int i = 0; i < ReefLayout.Columns; i++)
                    if (ReefLayout.ReefUnitAt(i, j, out var v) && v.kind == ReefLayout.Kind.Belt) cells++;
            Debug.Log($"[BW] Beat04 belt: {cells} units in the belt cells, {belt} built (top above the clip)");
            Assert.That(cells, Is.InRange(BulwarkConst.BeltUnitsMin, BulwarkConst.BeltUnitsMax), "belt units (14.5.4: about 57)");
            Assert.That(belt, Is.InRange(BulwarkConst.BeltUnitsMin, BulwarkConst.BeltUnitsMax), "built belt units");
        }

        [Test]
        public void Islets_AreSixPilesOfWholeUnitsAtTheirCrests()
        {
            Assert.AreEqual(6, ReefLayout.IsletCount);
            for (int k = 0; k < 6; k++)
            {
                var l = ReefLayout.Islet(k);
                Assert.That(l.Count, Is.InRange(BulwarkConst.IsletUnitsMin, BulwarkConst.IsletUnitsMax), "islet " + (k + 1) + " units");
                float top = float.MinValue;
                foreach (var u in l) top = Mathf.Max(top, u.TopY);
                Assert.AreEqual(ReefLayout.IsletCrestY[k], top, 0.05f, "islet " + (k + 1) + " crest");
                Assert.That(top, Is.InRange((float)BulwarkConst.IsletCrestYMin, (float)BulwarkConst.IsletCrestYMax), "crest y +10..+22");
                Assert.That(ReefLayout.IsletRadius[k], Is.InRange((float)BulwarkConst.IsletRadiusMin, (float)BulwarkConst.IsletRadiusMax));
                Vector2 c = ReefLayout.IsletCentre(k);
                Assert.GreaterOrEqual(c.x - (float)BulwarkConst.SeaFaceX, (float)BulwarkConst.IsletMinWallDistance, "islet " + (k + 1) + " 140 m from the wall");
                // whole units: the crest unit stands on three arms with its fourth within 20 deg of vertical
                var crest = l[0];
                Assert.GreaterOrEqual(crest.Dir(crest.topArm).y, Mathf.Cos(20.5f * Mathf.Deg2Rad), "crest unit upright");
                // a pile, not a heap of merged concrete: mean overlap per touching pair below 2.5 m of capsule depth
                float pen = 0f; int pairs = 0;
                for (int a = 0; a < l.Count; a++)
                    for (int b = a + 1; b < l.Count; b++)
                    {
                        float p = ReefLayout.Penetration(l[a], l[b]);
                        if (p > 0f) { pen += p; pairs++; }
                    }
                Debug.Log($"[BW] Beat04 islet {k + 1}: {l.Count} units, crest {top:F2}, touching pairs {pairs}, mean overlap {(pairs > 0 ? pen / pairs : 0f):F2} m");
            }
            Assert.AreEqual(45f, ReefLayout.IsletRadius[0], 1e-4f, "islet 1 radius 45 (Wave A audit 45-50)");
        }

        [Test]
        public void Grid_LeavesTheChannelAndTheIsletsOpenAndIsDeterministic()
        {
            var a = ReefLayout.All();
            int channel = 0;
            foreach (var u in a)
            {
                if (u.kind == ReefLayout.Kind.Islet) continue;
                Vector2 c = ReefLayout.CellCentre(u.cellI, u.cellJ);
                if (c.x > 900f && c.x < 1100f && c.y > 704f && c.y < 1040f) channel++;
                for (int k = 0; k < 6; k++)
                    Assert.GreaterOrEqual((c - ReefLayout.IsletCentre(k)).magnitude, ReefLayout.IsletRadius[k], "grid cell inside islet " + (k + 1));
            }
            Assert.AreEqual(0, channel, "no unit in the reef channel (Addendum A1 1.4)");
            bool h1 = ReefLayout.ReefUnitAt(3, 2, out var u1);
            bool h2 = ReefLayout.ReefUnitAt(3, 2, out var u2);
            Assert.IsTrue(h1 && h2);
            Assert.AreEqual(u1.centre, u2.centre);
            Assert.AreEqual(u1.rot, u2.rot);
        }

        [Test]
        public void Units_StayClearOfTheSeaFaceAndTheLastPier()
        {
            int checkedParts = 0;
            foreach (var u in ReefLayout.All())
                for (int a = 0; a < 4; a++)
                    for (int s = 0; s <= 6; s++)
                    {
                        float t = ReefLayout.ArmLength * s / 6f;
                        Vector3 p = u.centre + u.Dir(a) * t;
                        if (p.y < -14f) continue;
                        float r = ReefLayout.ArmRadiusAt(t);
                        Assert.GreaterOrEqual(p.x - r, ReefLayout.SeaFaceX(p.y) + 0.4f, "unit enters the sea face");
                        if (p.x - r < 46.4f && p.y < 1f)
                            Assert.GreaterOrEqual(p.z - r, 704.4f, "unit enters the last spillway pier");
                        checkedParts++;
                    }
            Assert.Greater(checkedParts, 1000);
        }

        [Test]
        public void Gauges_OneWithin200MetresOfTheKs10Eye()
        {
            var eye = new Vector3(3.0f, 1.7f, 776.0f);
            float best = float.MaxValue;
            int n = 0;
            foreach (var u in ReefLayout.All())
            {
                if (!u.gauge) continue;
                n++;
                Assert.LessOrEqual(u.centre.x, 6f + (float)BulwarkConst.GaugeRange + 1f, "gauges within 300 m");
                best = Mathf.Min(best, (Beat04ArmourSlope.GaugeDeckTop(u) - eye).magnitude);
            }
            Debug.Log($"[BW] Beat04 gauges: {n}, nearest to the KS10 eye {best:F1} m");
            Assert.Greater(n, 2);
            Assert.LessOrEqual(best, 200f, "a gauge platform within 200 m (14.5.4 acceptance 2)");
        }

        [Test]
        public void Mesh_Lod0Lod1BudgetsAndClipping()
        {
            var l0 = ColossopodMesh.Build(0, -1);
            var l1 = ColossopodMesh.Build(1, -1);
            Debug.Log($"[BW] Beat04 unit triangles: LOD0 {l0.TriangleCount}, LOD1 {l1.TriangleCount}");
            Assert.That(l0.TriangleCount, Is.InRange(1300, 1900), "LOD0 about 1600 triangles");
            Assert.That(l1.TriangleCount, Is.InRange(200, 400), "LOD1 about 300 triangles");
            var mesh = l0.ToMesh("b4_test_unit");
            var rep = MeshValidation.Validate(mesh);
            Assert.IsTrue(rep.Ok, rep.Describe());
            Object.DestroyImmediate(mesh);
            // clipping keeps every triangle with a vertex above the clip and nothing else
            var dst = new MeshBuilder();
            var remap = new List<int>();
            int kept = ColossopodMesh.AppendClipped(dst, l0, Matrix4x4.TRS(new Vector3(0f, -20f, 0f), Quaternion.identity, Vector3.one), ReefLayout.ClipY, remap);
            Assert.Greater(kept, 0);
            Assert.Less(kept, l0.TriangleCount / 2, "a unit 20 m down keeps only its upper arm");
            foreach (int i in dst.Indices) Assert.Less(i, dst.VertexCount);
            // a whole islet and a far shoal chunk in world coordinates pass the T-MESH checks (no float-collapsed triangles)
            foreach (int k in new[] { 0, 5 })
            {
                var chunk = new MeshBuilder();
                foreach (var u in ReefLayout.Islet(k))
                    ColossopodMesh.AppendClipped(chunk, l0, Matrix4x4.TRS(u.centre, u.rot, Vector3.one), ReefLayout.ClipY, remap);
                var cm = chunk.ToMesh("b4_test_islet" + k);
                var cr = MeshValidation.Validate(cm);
                Object.DestroyImmediate(cm);
                Assert.IsTrue(cr.Ok, cr.Describe());
            }
            var far = new MeshBuilder();
            for (int i = 50; i < 57; i++)
                for (int j = 40; j < 49; j++)
                    if (ReefLayout.GridUnit(i, j, out var g))
                        ColossopodMesh.AppendClipped(far, l1, Matrix4x4.TRS(g.centre, g.rot, Vector3.one), ReefLayout.ClipY, remap);
            var fm = far.ToMesh("b4_test_far");
            var fr = MeshValidation.Validate(fm);
            Object.DestroyImmediate(fm);
            Assert.IsTrue(fr.Ok, fr.Describe());
        }

        [Test]
        public void Surf_DensityIsSoftAndFadesOut()
        {
            var u = ReefLayout.Islet(0)[0];
            Assert.AreEqual(1f, Beat04ArmourSlope.SurfDensity(u.centre.x, u.centre.z), 0.05f, "inside the crest unit");
            Assert.AreEqual(0f, Beat04ArmourSlope.SurfDensity(820f, 800f), 1e-4f, "faded out beyond x 800");
            // no step: neighbouring samples 0.5 m apart differ by at most 0.3 (the collar's e-fold 1.8 m: slope <= 0.56 / m)
            float prev = Beat04ArmourSlope.SurfDensity(60f, 760f);
            for (int i = 1; i < 200; i++)
            {
                float d = Beat04ArmourSlope.SurfDensity(60f + 0.5f * i, 760f);
                Assert.That(d, Is.InRange(0f, 1f));
                Assert.LessOrEqual(Mathf.Abs(d - prev), 0.3f, "surf density step at x " + (60f + 0.5f * i));
                prev = d;
            }
        }

        [Test]
        public void Crest_PropsKeepOutOfTheCapsuleReach()
        {
            foreach (int k in Beat04ArmourSlope.LampK)
            {
                Beat04ArmourSlope.LampBase(k, out double x, out _, out double z, out bool lee);
                Assert.IsTrue(x <= -5.75 - 0.095 + 1e-6 || x >= 5.45 + 0.095 - 1e-6 || !lee, "lamp post " + k + " in the lane reach");
                Assert.That(z, Is.InRange(Beat04ArmourSlope.BandZ0, Beat04ArmourSlope.BandZ1));
            }
            CollectionAssert.AreEqual(new[] { 750.0, 800.0, 850.0, 900.0 }, BulwarkConst.LampPostZBeat4);
            foreach (double z in Beat04ArmourSlope.BollardZ()) Assert.That(z, Is.InRange(704.0, 910.0));
            Assert.LessOrEqual(Beat04ArmourSlope.LeeKerbX + 0.095, -5.75, "bollards outside the capsule reach");
        }

        [Test]
        public void Tower_TopIsAt49DegreesFromKs12()
        {
            // KS12 (-1.5, 1.7, 852) yaw 0: the crown's top edge (y 80.3, 0.4 m proud of z 920)
            double el = System.Math.Atan2(BulwarkConst.TowerHeight + 0.3 - 1.7, BulwarkConst.TowerZ0 - Beat04ArmourSlope.CrownProud - 852.0) * 180.0 / System.Math.PI;
            Assert.AreEqual(49.0, el, 0.5, "tower top elevation");
        }
        [Test]
        public void Ks10_TipHeightsMatchTheSpec()
        {
            // 14.5.4 KS10: tips 132 +-20 px at 80 m and 54 +-10 px at 200 m (4K, vFOV 64: 33.75 px/deg), measured on the
            // tallest unit within +-10 m of that distance in the frame (bearings 30-126 deg from the eye (3.0, 1.7, 776))
            var eye = new Vector2(3.0f, 776.0f);
            foreach (var (dist, want, tol) in new[] { (80f, 132f, 20f), (200f, 54f, 10f) })
            {
                float best = float.MinValue;
                foreach (var u in ReefLayout.All())
                {
                    if (u.kind == ReefLayout.Kind.Islet) continue;
                    Vector3 tip = u.Tip(u.topArm);
                    var d2 = new Vector2(tip.x, tip.z) - eye;
                    float d = d2.magnitude, brg = Mathf.Atan2(d2.x, d2.y) * Mathf.Rad2Deg;
                    if (Mathf.Abs(d - dist) > 10f || brg < 30f || brg > 126f) continue;
                    best = Mathf.Max(best, u.TopY);
                }
                float h = best - (float)BulwarkConst.SeaY;
                float px = Mathf.Atan2(h, dist) * Mathf.Rad2Deg * 33.75f;
                Debug.Log($"[BW] Beat04 KS10 tip at {dist} m: top y {best:F2} ({h:F2} m above the mean sea) = {px:F0} px at 4K (spec {want} +-{tol})");
                Assert.AreEqual(want, px, tol, "KS10 tip height at " + dist + " m");
            }
        }
        [Test]
        public void Surf_FoamRingMatchesTheHullWithinOneMetre()
        {
            // 14.5.4 acceptance 3: foam rings coincide with the unit footprints (top-down mismatch <= 1.0 m). The overlay carries
            // the density at its grid nodes and interpolates bilinearly; its ring term (SurfCollar; the islet skirts are broad by
            // design) is measured: along rays out of the arms that pierce the waterline the
            // 0.5 contour of the interpolated field lies within 1.0 m of the exact one.
            float sp = Beat04ArmourSlope.SurfSpacing, x0 = Beat04ArmourSlope.SurfX0, z0 = (float)BulwarkConst.ReefZ0 + 0.5f;
            float Bilinear(float x, float z)
            {
                float gx = (x - x0) / sp, gz = (z - z0) / sp;
                int i = Mathf.FloorToInt(gx), j = Mathf.FloorToInt(gz);
                float fx = gx - i, fz = gz - j;
                float a = Beat04ArmourSlope.SurfCollar(x0 + sp * i, z0 + sp * j), b = Beat04ArmourSlope.SurfCollar(x0 + sp * (i + 1), z0 + sp * j);
                float c = Beat04ArmourSlope.SurfCollar(x0 + sp * i, z0 + sp * (j + 1)), d = Beat04ArmourSlope.SurfCollar(x0 + sp * (i + 1), z0 + sp * (j + 1));
                return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fz);
            }
            // start points: where the top arm of a unit pierces the mean waterline (inside the hull, density 1); a ray is
            // measured where the exact field has a clean edge (it stays below 0.5 for 3 m after its first crossing): gaps
            // narrower than a grid cell between two hulls are filled with foam by design
            var starts = new List<Vector2>();
            foreach (var u in ReefLayout.All())
            {
                if (starts.Count >= 24) break;
                // the belt and shoal units: the islet piles carry broad skirts by design (their 0.5 contour is a skirt, not a ring)
                if (u.centre.x > 300f || u.kind == ReefLayout.Kind.Islet) continue;
                if ((new Vector2(u.centre.x, u.centre.z) - ReefLayout.IsletCentre(0)).magnitude < 120f) continue;
                Vector3 tip = u.Tip(u.topArm), d = u.Dir(u.topArm);
                float sea = (float)BulwarkConst.SeaY;
                if (tip.y < sea + 1f || u.centre.y > sea || d.y < 0.2f) continue;
                float t = (sea - u.centre.y) / d.y;
                Vector3 w = u.centre + d * t;
                starts.Add(new Vector2(w.x, w.z));
            }
            Assert.Greater(starts.Count, 6);
            float worst = 0f;
            int rays = 0;
            foreach (var c0 in starts)
                for (int a = 0; a < 8; a++)
                {
                    var dir = new Vector2(Mathf.Cos(a * Mathf.PI / 4f), Mathf.Sin(a * Mathf.PI / 4f));
                    float exact = -1f, interp = -1f;
                    for (float t = 0f; t < 30f; t += 0.1f)
                    {
                        Vector2 p = c0 + dir * t;
                        if (exact < 0f && Beat04ArmourSlope.SurfCollar(p.x, p.y) < 0.5f) exact = t;
                        if (interp < 0f && Bilinear(p.x, p.y) < 0.5f) interp = t;
                        if (exact >= 0f && interp >= 0f) break;
                    }
                    if (exact < 0f || interp < 0f) continue;
                    bool clean = true;
                    for (float t = exact; t < exact + 3f; t += 0.25f)
                    {
                        Vector2 p = c0 + dir * t;
                        if (Beat04ArmourSlope.SurfCollar(p.x, p.y) >= 0.5f) { clean = false; break; }
                    }
                    if (!clean) continue;
                    rays++;
                    // the distance between the two 0.5 contours NORMAL to the exact one (|b(p) - 0.5| / |grad|): along a ray
                    // that grazes a contour (a submerged arm just under the surface) the ray distance is not the mismatch
                    Vector2 q = c0 + dir * exact;
                    float e = 0.1f;
                    var g = new Vector2(Beat04ArmourSlope.SurfCollar(q.x + e, q.y) - Beat04ArmourSlope.SurfCollar(q.x - e, q.y),
                                        Beat04ArmourSlope.SurfCollar(q.x, q.y + e) - Beat04ArmourSlope.SurfCollar(q.x, q.y - e)) / (2f * e);
                    float normal = Mathf.Abs(Bilinear(q.x, q.y) - Beat04ArmourSlope.SurfCollar(q.x, q.y)) / Mathf.Max(g.magnitude, 1e-3f);
                    worst = Mathf.Max(worst, Mathf.Min(normal, Mathf.Abs(exact - interp)));
                }
            Assert.Greater(rays, 40, "clean edges measured");
            Debug.Log($"[BW] Beat04 foam ring: worst top-down mismatch of the 0.5 contour {worst:F2} m ({rays} clean rays from {starts.Count} waterline piercings, grid {sp} m)");
            Assert.LessOrEqual(worst, 1.0f, "foam ring vs hull (14.5.4 acceptance 3)");
        }
    }
}
