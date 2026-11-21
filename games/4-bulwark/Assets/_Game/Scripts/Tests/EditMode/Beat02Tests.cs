// Beat02Tests: EditMode tests of the crest builder (MASTER_PROMPT 14.5.2 acceptance (1): road x -6.0..6.0 +-0.02, parapet
// top 1.10, lamp posts 9.0 every 50 m; 14.12 rows "crest" and "Dam"; 14.2.7 scale cues; the capsule-reach rule of the lane
// (nothing visible inside x -5.75..5.45 at body height except the spec's cart, a PathGuide obstacle); the triangle budget of
// 14.5.2 (about 520 k with the sea); the graybox parts it replaces). Builds Beat02Crest alone with the material library.
using System;
using System.Collections.Generic;
using Megastructure.Beats;
using Megastructure.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Megastructure.Tests
{
    public class Beat02Tests
    {
        const string LibraryAsset = "Assets/_Game/Materials/MaterialLibrary.asset";

        sealed class Built : IDisposable
        {
            public GameObject Root;
            public Beat02Crest B;
            public WorldContext Ctx;
            public Built()
            {
                Root = new GameObject("Beat02TestWorld");
                Ctx = new WorldContext { Root = Root.transform, Materials = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(LibraryAsset) };
                B = new Beat02Crest();
                B.Build(Ctx);
            }
            public MeshFilter Find(string name)
            {
                foreach (var mf in Root.GetComponentsInChildren<MeshFilter>(true)) if (mf.gameObject.name == name) return mf;
                return null;
            }
            public void Dispose()
            {
                foreach (var mf in Root.GetComponentsInChildren<MeshFilter>(true)) if (mf.sharedMesh != null) UnityEngine.Object.DestroyImmediate(mf.sharedMesh);
                UnityEngine.Object.DestroyImmediate(Root);
                foreach (var m in B.RuntimeMaterials) if (m != null) UnityEngine.Object.DestroyImmediate(m);
                PathGuide.RemoveObstacle("crest_cart");
            }
        }

        static Bounds MeshBounds(MeshFilter mf, Func<Vector3, bool> keep = null)
        {
            var v = mf.sharedMesh.vertices;
            bool any = false;
            var b = new Bounds();
            foreach (var p in v)
            {
                if (keep != null && !keep(p)) continue;
                if (!any) { b = new Bounds(p, Vector3.zero); any = true; }
                else b.Encapsulate(p);
            }
            return b;
        }

        [Test]
        public void Crest_DimensionsOfTheAcceptance()
        {
            using (var w = new Built())
            {
                Assert.IsEmpty(w.Ctx.MeshProblems, "mesh validation: " + string.Join("; ", w.Ctx.MeshProblems));
                var deck = w.Find("crest_deck");
                Assert.IsNotNull(deck, "deck");
                var db = MeshBounds(deck);
                Assert.AreEqual(-6.0, db.min.x, 0.02, "road x -6.0");
                Assert.AreEqual(6.0, db.max.x, 0.02, "road x +6.0 (under the parapet)");
                Assert.AreEqual(0.0, db.max.y, 1e-4, "road y 0");
                Assert.AreEqual(BulwarkConst.RoadMeshZMin, db.min.z, 0.01, "road mesh from z -300");
                Assert.AreEqual(BulwarkConst.RoadMeshZMax, db.max.z, 0.01, "road mesh to z 1800");
                // no deck inside the spillway, the beat 4 crest or the tower
                Assert.AreEqual(0, CountVerts(deck, p => p.z > BulwarkConst.SpillwayZ0 + 0.01 && p.z < BulwarkConst.TowerZ1 - 0.01), "deck vertices in z 420-956 (other beats)");

                var par = w.Find("crest_parapet");
                var pb = MeshBounds(par);
                Assert.AreEqual(BulwarkConst.ParapetTop, pb.max.y, 1e-4, "parapet top 1.10");
                Assert.AreEqual(BulwarkConst.ParapetXMin, pb.min.x, 1e-4, "parapet x 5.5");
                Assert.AreEqual(BulwarkConst.ParapetXMax, pb.max.x, 1e-4, "parapet x 6.0");

                var sea = MeshBounds(w.Find("crest_sea_face"));
                Assert.AreEqual(BulwarkConst.SeaFaceX, sea.min.x, 1e-4, "sea face x 6.0");
                var lee = MeshBounds(w.Find("crest_lee_face"));
                Assert.AreEqual(BulwarkConst.LeeFaceX, lee.min.x, 1e-4, "lee face x -6.0");
                Assert.AreEqual(BulwarkConst.LeeFaceX, lee.max.x, 1e-4, "lee face is the plane x -6.0");
                Assert.LessOrEqual(lee.min.y, -1000.0, "the lee face drops toward the lowland");
            }
        }

        static int CountVerts(MeshFilter mf, Func<Vector3, bool> pred)
        {
            int n = 0;
            foreach (var p in mf.sharedMesh.vertices) if (pred(p)) n++;
            return n;
        }

        [Test]
        public void Crest_LampPostsRailsBollardsCart()
        {
            using (var w = new Built())
            {
                var lamps = w.Find("crest_lamp_posts");
                foreach (int k in Beat02Crest.LampK)
                {
                    Beat02Crest.LampBase(k, out double x, out double yb, out double z, out bool lee);
                    Assert.AreEqual(150.0 + 50.0 * k, z, 1e-9, "z 150 + 50 k");
                    Assert.AreEqual((k & 1) == 0, lee, "even k on the lee side");
                    var b = MeshBounds(lamps, p => Math.Abs(p.z - z) < 0.6);
                    Assert.AreEqual(yb + BulwarkConst.LampPostHeight, b.max.y, 0.02, "lamp post " + k + " 9.0 m tall");
                    Assert.AreEqual(yb, b.min.y, 0.01, "stands on its base");
                }
                // nothing on the parapet top straddles a parapet joint (sea-side posts' 0.32 m plates, lifebuoy holders)
                var onParapet = new List<double>();
                foreach (int k in Beat02Crest.LampK)
                {
                    Beat02Crest.LampBase(k, out _, out _, out double z, out bool lee);
                    if (!lee) onParapet.Add(z);
                }
                onParapet.AddRange(Beat02Crest.LifebuoyZ);
                foreach (double z in onParapet)
                {
                    double za = BulwarkConst.RoadMeshZMin, nearest = double.MaxValue;
                    while (za < Beat02Crest.BandZ1[0] - 1e-6) { za = Beat02Crest.NextParapetJoint(za, Beat02Crest.BandZ1[0]); nearest = Math.Min(nearest, Math.Abs(za - z)); }
                    Assert.GreaterOrEqual(nearest, 1.0, "parapet prop at z " + z + " is " + nearest + " m from a joint");
                }
                // two rails 0.10 m, tops 1.1 and 0.55
                var rails = MeshBounds(w.Find("crest_rails"));
                Assert.AreEqual(BulwarkConst.LeeRailTop, rails.max.y, 0.002, "top rail top 1.1");
                Assert.AreEqual(BulwarkConst.LeeRailMid - BulwarkConst.LeeRailDiameter, rails.min.y, 0.002, "lower rail bottom (top 0.55, 0.10 m)");
                Assert.AreEqual(BulwarkConst.LeeRailX - 0.05, rails.min.x, 0.002, "rail axis x -6.0, radius 0.05");
                // posts every 2 m at odd z (the visibility audit's KS05 post phase)
                var posts = w.Find("crest_rail_posts").sharedMesh.vertices;
                var zs = new HashSet<int>();
                foreach (var p in posts) zs.Add((int)Math.Round(p.z * 10.0));
                Assert.IsTrue(zs.Contains(2310) && zs.Contains(2290), "posts at z 229 and 231 (KS05: 1.5 m from the eye)");
                Assert.IsFalse(zs.Contains(2300), "no post at even z");
                // bollards: 0.9 m on the lee kerb every 20 m from z 140
                var bol = w.Find("crest_bollards");
                var bb = MeshBounds(bol, p => Math.Abs(p.z - 160.0) < 0.5);
                Assert.AreEqual(BulwarkConst.BollardHeight, bb.max.y, 0.01, "bollard 0.9 m");
                Assert.AreEqual(Beat02Crest.LeeKerbX, bb.center.x, 0.01);
                Assert.GreaterOrEqual(Beat02Crest.BollardCount, 13);
                // the cart: inside the spec box (2.8 x 1.3 x 1.9 at (2.0, 0, 330)) and registered as an obstacle
                var cb = Beat02Crest.CartBounds;
                foreach (var name in new[] { "crest_cart_body", "crest_cart_frame" })
                {
                    var m = MeshBounds(w.Find(name));
                    Assert.GreaterOrEqual(m.min.x, cb.min.x - 0.005f, name); Assert.LessOrEqual(m.max.x, cb.max.x + 0.005f, name);
                    Assert.GreaterOrEqual(m.min.y, cb.min.y - 0.005f, name); Assert.LessOrEqual(m.max.y, cb.max.y + 0.005f, name);
                    Assert.GreaterOrEqual(m.min.z, cb.min.z - 0.005f, name); Assert.LessOrEqual(m.max.z, cb.max.z + 0.005f, name);
                }
                Assert.AreEqual(BulwarkConst.CartH, MeshBounds(w.Find("crest_cart_body")).max.y, 0.02, "cart 1.9 m tall");
                Assert.IsTrue(PathGuide.ObstacleAt(BwPath.Project(BulwarkConst.CartX, BulwarkConst.CartZ, BulwarkConst.CartZ), BulwarkConst.CartX - BulwarkConst.StartX, 0f, out _),
                    "the cart is a PathGuide obstacle");
                Assert.IsFalse(PathGuide.ObstacleAt(BulwarkConst.CartZ, BwPath.CinematicLateral(BulwarkConst.CartZ), 0f, out _), "the cinematic path passes the cart");
            }
        }

        [Test]
        public void Crest_ExpansionJointsAndScuppers()
        {
            using (var w = new Built())
            {
                // expansion joints every 24 m at z 18 + 24 k: none under the beat-1 shell, clear of lamp posts, bollards and rail posts
                var zs = Beat02Crest.ExpansionJointZ();
                Assert.Greater(zs.Count, 40, "both bands");
                int inBeat2 = 0;
                foreach (double z in zs)
                {
                    Assert.AreEqual(0.0, Math.IEEERemainder(z - Beat02Crest.ExpansionPhase, Beat02Crest.ExpansionPitch), 1e-9);
                    Assert.IsFalse(z > BulwarkConst.ShellZMin - 0.5 && z < BulwarkConst.ShellZMax + 0.5, "no joint under the gallery shell");
                    foreach (int k in Beat02Crest.LampK)
                    {
                        Beat02Crest.LampBase(k, out _, out _, out double lz, out _);
                        Assert.Greater(Math.Abs(lz - z), 1.0, "joint at z " + z + " clear of lamp post " + k);
                    }
                    for (int i = 0; i < Beat02Crest.BollardCount; i++) Assert.GreaterOrEqual(Math.Abs(Beat02Crest.BollardZ(i) - z), 2.0 - 1e-9, "bollard");
                    Assert.AreEqual(0.0, Math.IEEERemainder(z, 2.0), 1e-9, "even z: between the rail posts (odd z)");
                    if (z > 120.0 && z < 420.0) inBeat2++;
                }
                Assert.AreEqual(12, inBeat2, "12 joints over beat 2 (138 .. 402)");
                // fix wave X2 S4-6: no geometric joints any more (the 6 mm plates aliased beyond ~9 m); BwRoad draws them filtered
                Assert.IsNull(w.Find("crest_expansion_plates"), "the joints are BwRoad's (no plate meshes)");
                Assert.IsNull(w.Find("crest_expansion_seals"), "the joints are BwRoad's (no seal meshes)");
                // scuppers: the parapet is open from the deck to y 0.15 over 0.40 m at 3 and 9 m into every segment
                var par = w.Find("crest_parapet");
                int ports = 0;
                foreach (double zg in new[] { 126.0, 210.0, 318.0, 1002.0 })
                    foreach (double off in Beat02Crest.ScupperOffsets)
                    {
                        double zp = zg + off;
                        ports++;
                        int inside = CountVerts(par, p => p.x > BulwarkConst.ParapetXMin + 0.01 && p.x < BulwarkConst.ParapetXMax - 0.01
                                                          && p.y > 0.0 && p.y < Beat02Crest.ScupperTop - 0.005
                                                          && Math.Abs(p.z - zp) < 0.5 * Beat02Crest.ScupperWidth - 0.03);
                        Assert.AreEqual(0, inside, "scupper at z " + zp + " is open");
                        // and the port is really cut: kicker vertices bound it at zp +- 0.20
                        Assert.Greater(CountVerts(par, p => Math.Abs(Math.Abs(p.z - zp) - 0.5 * Beat02Crest.ScupperWidth) < 0.025 && p.y < Beat02Crest.ScupperTop + 0.03), 0, "port edges at z " + zp);
                    }
                Assert.AreEqual(8, ports);
            }
        }

        [Test]
        public void Crest_NoParapetPropHidesTheK2Arm()
        {
            // Addendum A1 1.6 K2: the arm leaves the water at (330, 690) and stands over s 356-408; the walker is at x -1.5
            // (lateral keys) with the eye at 1.7. No lifebuoy station (ring R + tube + 0.1 m) may cover the arm's bearing.
            double xb = 0.5 * (BulwarkConst.ParapetXMin + BulwarkConst.ParapetXMax);
            for (double s = 356.0; s <= 408.0; s += 0.5)
            {
                double arm = Math.Atan2(330.0 + 1.5, 690.0 - s) * 180.0 / Math.PI;
                foreach (double z in Beat02Crest.LifebuoyZ)
                {
                    double dx = xb + 1.5, dz = z - s, d = Math.Sqrt(dx * dx + dz * dz);
                    double brg = Math.Atan2(dx, dz) * 180.0 / Math.PI;
                    double half = Math.Atan((Beat02Crest.LifebuoyR + Beat02Crest.LifebuoyTube + 0.1) / d) * 180.0 / Math.PI;
                    Assert.Greater(Math.Abs(brg - arm), half + 1.0, $"lifebuoy at z {z} covers the K2 arm at s {s}");
                }
            }
        }

        [Test]
        public void Crest_NothingInsideTheCapsuleReachButTheCart()
        {
            // the road lane is x -5.4..5.1 for capsule centres: a 0.35 m capsule reaches x -5.75..5.45 between 0.30 and 1.90 m
            using (var w = new Built())
            {
                var bad = new List<string>();
                foreach (var mf in w.Root.GetComponentsInChildren<MeshFilter>(true))
                {
                    string n = mf.gameObject.name;
                    if (n == "crest_deck" || n.StartsWith("crest_sea", StringComparison.Ordinal) || n.StartsWith("crest_cart", StringComparison.Ordinal)
                        || n == "crest_far_towers") continue;
                    int c = 0;
                    foreach (var p in mf.sharedMesh.vertices)
                    {
                        bool inBand = (p.z > BulwarkConst.ShellZMax + 0.5 && p.z < BulwarkConst.SpillwayZ0) || (p.z > BulwarkConst.TowerZ1 + 1.0);
                        if (inBand && p.y > 0.30f && p.y < 1.90f && p.x > -5.75f && p.x < 5.45f) c++;
                    }
                    if (c > 0) bad.Add(n + " (" + c + " vertices)");
                }
                Assert.IsEmpty(bad, "visible geometry inside the capsule reach of the road lane: " + string.Join(", ", bad));
            }
        }

        [Test]
        public void Crest_BudgetsAndReplacements()
        {
            using (var w = new Built())
            {
                Debug.Log($"[BW] Beat02_Crest: crest {w.B.CrestTriangles} triangles, sea {w.B.SeaTriangles}");
                Assert.Less(w.B.CrestTriangles + w.B.SeaTriangles, 600000, "14.5.2: about 520 k triangles");
                Assert.Less(w.B.SeaTriangles, 230000, "sea clipmap 229 k");
                Assert.IsNotNull(w.Find("crest_sea_clipmap"), "the sea is built (BwSea found)");
            }
            var attrs = typeof(Beat02Crest).GetCustomAttributes(typeof(ReplacesGrayboxAttribute), false);
            var parts = new HashSet<string>();
            foreach (ReplacesGrayboxAttribute a in attrs) parts.Add(a.Part);
            Assert.IsTrue(parts.Contains("beat2") && parts.Contains("sea"), "replaces the graybox beat2 and sea parts");
            // the bands it builds are exactly the graybox's beat2 crest bands
            foreach (double z in new[] { -299.0, 0.0, 119.0, 121.0, 300.0, 419.9, 957.0, 1500.0, 1799.0 })
                Assert.AreEqual(1, GrayboxPath.CrestKey(z), "crest key of z " + z + " is beat2");
        }
    }
}
