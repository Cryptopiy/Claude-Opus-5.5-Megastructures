// VisibilityAuditTests: the 11.13 visibility and reachability audit as an EditMode test (MASTER_PROMPT 11.13, 14.10.2,
// Part 10 items 35 and 39). Builds the real world of the beat builders (Graybox_Path and every Beat* builder; the sky dome,
// sea, water, mist, glass and lamp stand-ins are not occluders), adds MeshColliders to it and runs QA.VisibilityAudit:
// every key shot (target and AutoWalker follower pose) and every named subject of Part 14 at 16:9, 16:10 and 21:9 with the
// 3 % margin, px sizes at 1080p and 4K, occlusion against the built geometry and the analytic floor rule, the pose against
// the lane and the cinematic path, and the reachability sweep (occlusion per sweep pose). Writes QA/reports/visibility.json.
// Spec-level impossibilities are findings in the report (with arithmetic) for the lead, not test failures; the test fails
// only on a broken audit (no report, NaN poses, a key-shot pose outside the walkable lane) or on spec arithmetic the
// table itself states (horizon rows of KS04, KS05, KS10, KS20).
// The graybox guards live here as well (same built world): the capsule clearance of every visible graybox piece (nothing
// inside lane edge + PLAYER_R, Part 10 / 11.8), the ReplacesGraybox keys (a replaced part's pieces are not built, every other
// piece is unchanged; FRAMEWORK.md 4.0) and the LocalLightPool continuity along the walk and under a whip pan (no pool pops,
// Part 10 #6, 14.10.3 frame-step limit).
using System;
using System.Collections.Generic;
using System.IO;
using Megastructure.Beats;
using Megastructure.QA;
using Megastructure.Rendering;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Megastructure.Tests
{
    public class VisibilityAuditTests
    {
        const string LibraryAsset = "Assets/_Game/Materials/MaterialLibrary.asset";
        const string ReportRel = "QA/reports/visibility.json";

        static string Root => Path.GetDirectoryName(Application.dataPath);

        static VisibilityAudit.Pose PoseOf(KeyShot k) => new VisibilityAudit.Pose(k.x, k.y, k.z, k.yaw, k.pitch, k.fov);

        static KeyShot Shot(string id)
        {
            foreach (var k in BulwarkConst.KeyShots) if (k.tag.StartsWith(id, StringComparison.Ordinal)) return k;
            throw new ArgumentException(id);
        }

        // ------------------------------------------------------------------ the built world (shared by the tests below)

        /// <summary>The beat builders' world with MeshColliders on the renderers the predicate accepts; Dispose destroys it.</summary>
        sealed class BuiltWorld : IDisposable
        {
            public GameObject RootGo;
            public GrayboxPath Graybox;
            public readonly List<string> Built = new List<string>();
            public readonly List<Material> RuntimeMaterials = new List<Material>();
            public int Colliders, Skipped;
            readonly bool m_backfaces;

            public BuiltWorld(bool grayboxOnly, Func<MeshRenderer, bool> collide)
            {
                m_backfaces = Physics.queriesHitBackfaces;
                var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(LibraryAsset);
                RootGo = new GameObject("VisibilityAuditWorld");
                var ctx = new WorldContext { Root = RootGo.transform, Materials = lib };
                foreach (var b in GameBootstrap.DiscoverBuilders())
                {
                    bool beat = b.Name.StartsWith("Graybox", StringComparison.Ordinal) || (!grayboxOnly && b.Name.StartsWith("Beat", StringComparison.Ordinal));
                    if (!beat) continue;
                    try { b.Build(ctx); Built.Add(b.Name); }
                    catch (Exception e) { Debug.LogWarning("[BW] visibility audit: builder " + b.Name + " threw " + e.GetType().Name + ": " + e.Message); }
                    if (b is GrayboxPath g) { Graybox = g; RuntimeMaterials.AddRange(g.RuntimeMaterials); }
                }
                foreach (var mr in RootGo.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var mf = mr.GetComponent<MeshFilter>();
                    if (mf == null || mf.sharedMesh == null || !collide(mr)) { Skipped++; continue; }
                    var mc = mr.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                    Colliders++;
                }
                Physics.queriesHitBackfaces = true;
                Physics.SyncTransforms();
            }

            public void Dispose()
            {
                Physics.queriesHitBackfaces = m_backfaces;
                foreach (var mf in RootGo.GetComponentsInChildren<MeshFilter>(true)) if (mf.sharedMesh != null) UnityEngine.Object.DestroyImmediate(mf.sharedMesh);
                UnityEngine.Object.DestroyImmediate(RootGo);
                foreach (var m in RuntimeMaterials) if (m != null) UnityEngine.Object.DestroyImmediate(m);
            }
        }

        // ------------------------------------------------------------------ pure checks

        [Test]
        public void HorizonRows_OfTheKeyShotTableHold()
        {
            // 14.10.2 / 14.5 acceptances: KS04 sea horizon row 840 +-8, KS10 992 +-8, KS05 lowland 180 +-12, KS20 948 +-8 (4K)
            var cases = new (string id, bool sea, double row, double tol)[] { ("KS04", true, 840, 8), ("KS10", true, 992, 8), ("KS05", false, 180, 12), ("KS20", false, 948, 8) };
            foreach (var c in cases)
            {
                var k = Shot(c.id);
                double dip = c.sea ? VisibilityAudit.SeaDipDeg(k.y) : VisibilityAudit.LowlandDipDeg(k.y);
                double row = VisibilityAudit.RowOf(PoseOf(k), VisibilityAudit.AzEl(k.yaw, -dip), 2160);
                Assert.AreEqual(c.row, row, c.tol, c.id + " horizon row (dip " + dip.ToString("0.000") + " deg)");
            }
            Assert.AreEqual(2.486, VisibilityAudit.LowlandDipDeg(1.7), 0.05, "crest dip");
            Assert.AreEqual(2.467, VisibilityAudit.LowlandDipDeg(-88.3), 0.05, "ledge dip");
            Assert.AreEqual(0.100, VisibilityAudit.SeaDipDeg(1.7), 0.05, "sea dip from the crest eye");
        }

        [Test]
        public void Bow_TopIsAt25p4AndThe141PointsLieAboveTheHorizon()
        {
            VisibilityAudit.SunAt(604.0, out double az, out double el);
            Assert.AreEqual(107.2, az, 1e-6);
            Assert.AreEqual(16.6, el, 1e-6);
            var pts = VisibilityAudit.ArcAboveElevation(VisibilityAudit.AzEl(az + 180, -el), 42.0, -VisibilityAudit.LowlandDipDeg(1.7), 141);
            Assert.AreEqual(141, pts.Length);
            double top = double.MinValue;
            foreach (var d in pts)
            {
                double e = Math.Asin(d.y) / BwMath.Deg;
                Assert.GreaterOrEqual(e, -VisibilityAudit.LowlandDipDeg(1.7) - 1e-6);
                top = Math.Max(top, e);
            }
            Assert.AreEqual(25.4, top, 0.5, "bow top (14.10.1 T-NUM)");
        }

        [Test]
        public void Follower_IsFiniteAndEndsOnTheKeyShotS()
        {
            foreach (var k in BulwarkConst.KeyShots)
            {
                var f = VisibilityAudit.SimulateFollower(k.s, PoseOf(k));
                Assert.AreEqual(k.s, f.s, 1e-6, k.tag + " follower s");
                Assert.IsFalse(double.IsNaN(f.yaw) || double.IsNaN(f.pitch) || double.IsNaN(f.lagDeg), k.tag + " follower NaN");
                BwPath.Lane(f.s, out double lo, out double hi);
                Assert.That(f.lateral, Is.InRange(lo - 1e-6, hi + 1e-6), k.tag + " follower lateral inside the lane");
            }
        }

        [Test]
        public void AnalyticFloorRule_DeckEdgeAndTrench()
        {
            // KS05: eye 1.7 m above the deck, lee edge 1.5 m perpendicular: a ray at -45 deg straight west clears the edge
            // (1.7 / tan 45 = 1.7 > 1.5), a ray at -50 deg hits the deck (1.7 / tan 50 = 1.43 < 1.5)
            var eye = new D3(-4.5, 1.7, 230);
            Assert.IsFalse(VisibilityAudit.FloorBlocks(230, eye, VisibilityAudit.AzEl(-90, -45), true, out double dEdge, out _));
            Assert.AreEqual(1.5, dEdge, 0.03);
            Assert.IsTrue(VisibilityAudit.FloorBlocks(230, eye, VisibilityAudit.AzEl(-90, -50), false, out _, out _));
            // KS18: from the gallery axis the trench edge is 3.0 m away (perpendicular): at -30 deg the floor blocks
            var k = Shot("KS18");
            Assert.IsTrue(VisibilityAudit.FloorBlocks(k.s, new D3(k.x, k.y, k.z), VisibilityAudit.AzEl(90, -30), false, out _, out _));
        }

        // ------------------------------------------------------------------ the audit against the built world

        [Test]
        public void Audit_WritesVisibilityJson()
        {
            using (var w = new BuiltWorld(false, Solid))
            {
                string desc = "Physics ray casts against " + w.Colliders + " MeshColliders of the built world (" + string.Join(", ", w.Built) + "; " + w.Skipped
                              + " renderers skipped as non-occluders: sky, sea, water, surf, mist, glass, lamps, transparent)";
                VisibilityAudit.OcclusionTester occ = (D3 o, D3 d, double max, out string blocker) =>
                {
                    blocker = null;
                    if (max <= 0 || !Physics.Raycast(o.ToVector3(), d.ToVector3(), out RaycastHit hit, (float)max, ~0, QueryTriggerInteraction.Ignore)) return false;
                    blocker = hit.collider.gameObject.name + "@" + hit.distance.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "m";
                    return true;
                };
                var graybox = w.Graybox;
                Action<double> pose = s => { graybox?.SetDoorForS(s); Physics.SyncTransforms(); };

                var rep = VisibilityAudit.Run(occ, pose, desc);
                string json = VisibilityAudit.ToJson(rep);
                string path = Path.Combine(Root, ReportRel.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, json);

                int subjects = 0, pass = 0;
                foreach (var sr in rep.Shots) foreach (var r in sr.Target) { subjects++; if (r.Pass) pass++; }
                Debug.Log("[BW] visibility audit: " + rep.Shots.Count + " key shots, " + subjects + " subjects (" + pass + " pass at the target pose), "
                          + rep.Findings.Count + " findings, sweep " + rep.Sweep.Count + " named subjects; " + desc + " -> " + ReportRel);
                foreach (var f in rep.Findings) Debug.Log("[BW] visibility finding " + f.Id + ": " + f.Text + " | " + f.Arithmetic);

                Assert.AreEqual(BulwarkConst.KeyShots.Length, rep.Shots.Count, "every key shot audited");
                Assert.IsTrue(File.Exists(path), "report written");
                foreach (var sr in rep.Shots)
                {
                    Assert.IsTrue(sr.InLane, sr.Id + " pose outside the walkable lane (lateral " + sr.Lateral + ")");
                    Assert.Greater(sr.Target.Count, 0, sr.Id + " has no subjects");
                    Assert.IsFalse(double.IsNaN(sr.F.lagDeg), sr.Id + " follower lag NaN");
                }
                foreach (var sw in rep.Sweep)
                    Assert.LessOrEqual(sw.SecondsVisible, sw.SecondsInFrame + 1e-9, sw.Sub.Name + " visible time cannot exceed the in-frame time");
                Assert.Greater(w.Colliders, 0, "no built geometry to ray-cast against");
            }
        }

        /// <summary>Solid occluder: not a graybox non-occluder, not transparent, not named like sky / sea / water / mist / glass.</summary>
        static bool Solid(MeshRenderer mr)
        {
            string n = mr.gameObject.name;
            if (GrayboxPath.IsNonOccluder(n)) return false;
            string l = n.ToLowerInvariant();
            if (l.Contains("sky") || l.Contains("sea") || l.Contains("water") || l.Contains("mist") || l.Contains("glass") || l.Contains("plume") || l.Contains("snow") || l.Contains("beam")) return false;
            var m = mr.sharedMaterial;
            if (m != null && m.renderQueue >= 2500) return false;
            return true;
        }

        // ------------------------------------------------------------------ graybox guards

        /// <summary>Visible graybox pieces the spec itself puts inside the walkable lane (decisions logged): the cart at
        /// (2.0, 0, 330) on the crest road (14.5.2), x 1.35..2.65 inside the road lane x -5.4..5.1.</summary>
        static readonly string[] s_inLaneBySpec = { "gb_cart" };

        [Test]
        public void Graybox_StaysOutsideTheCapsuleReach()
        {
            // Every physical graybox surface (walls, floors, rails, posts, lamps, glass, door leaves in their pockets) must
            // stay outside the capsule the player can occupy: a capsule of radius PLAYER_R - 5 mm from 0.30 to 1.90 m above the
            // analytic floor (the CharacterController's body above its step offset) at both lane edges and every 0.5 m or less
            // across the lane, every 0.25 m of s. A hit means the camera can pass within PLAYER_R of visible geometry that nothing collides
            // with (PathGuide's colliders are the lane's, D-01), i.e. the player walks through it.
            using (var w = new BuiltWorld(true, mr => !GrayboxPath.IsNonPhysical(mr.gameObject.name)))
            {
                Assert.IsNotNull(w.Graybox, "Graybox_Path not built");
                w.Graybox.SetDoorForS(BwPath.Length);   // door open (leaves in the wall pockets)
                Physics.SyncTransforms();
                float r = (float)(BulwarkConst.PlayerR - 0.005);
                var hits = new Collider[16];
                var first = new Dictionary<string, string>();
                var count = new Dictionary<string, int>();
                int samples = 0, allowed = 0;
                for (double s = 0.0; s <= BwPath.Length + 1e-9; s += 0.25)
                {
                    BwPath.Lane(s, out double lo, out double hi);
                    double fy = BwPath.FloorY(s);
                    int inner = Math.Max(1, (int)Math.Ceiling((hi - lo) / 0.5));
                    for (int side = 0; side <= inner; side++)
                    {
                        double lat = lo + (hi - lo) * side / inner;   // both lane edges and every <= 0.5 m between them
                        BwPath.World(s, lat, out double x, out _, out double z);
                        var p0 = new Vector3((float)x, (float)(fy + 0.30 + r), (float)z);
                        var p1 = new Vector3((float)x, (float)(fy + 1.90 - r), (float)z);
                        int n = Physics.OverlapCapsuleNonAlloc(p0, p1, r, hits, ~0, QueryTriggerInteraction.Ignore);
                        samples++;
                        for (int k = 0; k < n; k++)
                        {
                            string name = hits[k].gameObject.name;
                            if (Array.IndexOf(s_inLaneBySpec, name) >= 0) { allowed++; continue; }
                            count.TryGetValue(name, out int c);
                            count[name] = c + 1;
                            if (!first.ContainsKey(name))
                                first[name] = "s " + s.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " lateral " + lat.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)
                                              + " (lane " + lo.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ".." + hi.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + ")";
                        }
                    }
                }
                var bad = new List<string>();
                foreach (var kv in count) bad.Add(kv.Key + " x" + kv.Value + " first at " + first[kv.Key]);
                Debug.Log("[BW] graybox capsule clearance: " + samples + " capsules (r " + r.ToString("0.000") + ", 0.30..1.90 m above the floor, lane edges and <= 0.5 m across the lane, every 0.25 m of s), "
                          + w.Colliders + " colliders, " + allowed + " hits on spec in-lane props (" + string.Join(", ", s_inLaneBySpec) + "), " + bad.Count + " violating pieces"
                          + (bad.Count > 0 ? ": " + string.Join("; ", bad) : ""));
                Assert.IsEmpty(bad, "graybox geometry inside the capsule reach of the lane: " + string.Join("; ", bad));
            }
        }

        // ------------------------------------------------------------------ the ReplacesGraybox keys (FRAMEWORK.md 4.0)

        /// <summary>The graybox alone, built with an injected replacement set (no test type in the shipped assembly), with
        /// MeshColliders on every piece for the ray probes.</summary>
        sealed class GrayWorld : IDisposable
        {
            public readonly GameObject RootGo;
            public readonly GrayboxPath G;
            public GrayWorld(Func<string, bool> replaced, bool colliders = true)
            {
                var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(LibraryAsset);
                RootGo = new GameObject("GrayboxReplaceWorld");
                var ctx = new WorldContext { Root = RootGo.transform, Materials = lib };
                G = new GrayboxPath { ReplacedQuery = replaced };
                G.Build(ctx);
                if (!colliders) return;
                foreach (var mr in RootGo.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var mf = mr.GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null) mr.gameObject.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
                }
                Physics.SyncTransforms();
            }
            public void Dispose()
            {
                foreach (var mf in RootGo.GetComponentsInChildren<MeshFilter>(true)) if (mf.sharedMesh != null) UnityEngine.Object.DestroyImmediate(mf.sharedMesh);
                UnityEngine.Object.DestroyImmediate(RootGo);
                foreach (var m in G.RuntimeMaterials) if (m != null) UnityEngine.Object.DestroyImmediate(m);
            }
        }

        /// <summary>name|vertexCount|bounds of every piece under a part group (the identity of that part's geometry).</summary>
        static List<string> Signature(Transform group)
        {
            var o = new List<string>();
            if (group == null) return o;
            foreach (var mf in group.GetComponentsInChildren<MeshFilter>(true))
            {
                var b = mf.sharedMesh.bounds;
                o.Add(mf.gameObject.name + "|" + mf.sharedMesh.vertexCount + "|" + b.center.ToString("F3") + "|" + b.size.ToString("F3"));
            }
            o.Sort(StringComparer.Ordinal);
            return o;
        }

        /// <summary>First graybox piece hit along a ray (MeshColliders of the graybox world only), or null.</summary>
        static string Probe(GameObject root, Vector3 o, Vector3 d, float len)
        {
            var hits = Physics.RaycastAll(o, d, len, ~0, QueryTriggerInteraction.Ignore);
            string best = null; float bd = float.MaxValue;
            foreach (var h in hits)
            {
                if (!h.collider.transform.IsChildOf(root.transform)) continue;
                if (h.distance < bd) { bd = h.distance; best = h.collider.gameObject.name; }
            }
            return best;
        }

        /// <summary>Probes inside a beat's s range: from 1.5 m above the floor at the lane centre down to 2 m below it, and
        /// left / right to the lane half-width + 1.5 m (the floor and walls of every walkable stretch). Returns the number of
        /// probes that hit a graybox piece.</summary>
        static int BeatProbes(GameObject root, int beat, out string firstHit)
        {
            firstHit = null;
            int hits = 0;
            // 6 m inside the beat: the fixed crest pieces split at the bridge ends z 420 / 704 and the tower z 920 / 956
            double s0 = BulwarkConst.BeatS[beat - 1] + 6.0, s1 = BulwarkConst.BeatS[beat] - 6.0;
            for (double s = s0; s <= s1; s += 3.0)
            {
                BwPath.Lane(s, out double lo, out double hi);
                double mid = 0.5 * (lo + hi), half = 0.5 * (hi - lo);
                BwPath.World(s, mid, out double x, out _, out double z);
                BwPath.Tangent(s, out double tx, out double tz);
                var o = new Vector3((float)x, (float)(BwPath.FloorY(s) + 1.5), (float)z);
                var right = new Vector3((float)tz, 0f, (float)-tx);
                string h = Probe(root, o, Vector3.down, 3.5f) ?? Probe(root, o, right, (float)(half + 1.5)) ?? Probe(root, o, -right, (float)(half + 1.5));
                if (h != null) { hits++; if (firstHit == null) firstHit = h + " at s " + s.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture); }
            }
            return hits;
        }

        [Test]
        public void Graybox_HonoursTheReplacesGrayboxKeys()
        {
            // For every key: build the graybox with only that key replaced. (1) the key's group is gone and IsReplacedPart says
            // so; (2) every other key's pieces are bit-identical to the full build (names, vertex counts, bounds); (3) the
            // probes through the walkable stretch of that beat find no graybox surface, while the full build finds floor or
            // walls there; (4) its lamp anchors are not registered ("lights": none at all); (5) sea: no sea plane under a ray
            // at x 100, z 300; sky: never built while WorldSky exists (else built only without the key).
            int n = GrayboxPath.KeyCount;
            var full = new List<string>[n];
            int fullAnchors;
            var fullAnchorsByKey = new int[n];
            var fullProbe = new int[8];
            string fullSea;
            bool fullSky;
            using (var w = new GrayWorld(_ => false))
            {
                for (int k = 0; k < n; k++)
                {
                    full[k] = Signature(w.G.PartGroup(k));
                    Assert.IsFalse(w.G.IsReplacedPart(k), GrayboxPath.PartKeys[k] + " replaced in the full build");
                }
                fullAnchors = w.G.AnchorIds.Count;
                foreach (int id in w.G.AnchorIds)
                {
                    string owner = w.G.LightPool.GetAnchor(id).owner;
                    for (int k = 0; k < GrayboxPath.BeatKeys; k++) if (owner == "graybox_" + GrayboxPath.PartKeys[k]) fullAnchorsByKey[k]++;
                }
                for (int b = 1; b <= 7; b++) { fullProbe[b] = BeatProbes(w.RootGo, b, out _); Assert.Greater(fullProbe[b], 0, "full build: no graybox surface in beat " + b); }
                fullSea = Probe(w.RootGo, new Vector3(100f, 50f, 300f), Vector3.down, 100f);
                fullSky = w.G.BuiltSky;
                Assert.IsNotNull(fullSea, "full build: the sea plane");
                Assert.Greater(fullAnchors, 20, "full build: lamp anchors");
                // sky openings (LocalLightPool _BwOpen): the door (shut until the trigger, then the gap between the leaves),
                // the tower's south portal, vehicle arch and north arch
                var op = w.G.OpeningPool;
                Assert.IsNotNull(op, "full build: the opening pool");
                Assert.AreEqual(4, op.OpeningCount, "door, portal, two arches");
                Assert.GreaterOrEqual(w.G.DoorOpening, 0, "the door opening");
                w.G.SetDoorForS(0.0);
                Assert.AreEqual(0f, op.OpeningPlane(w.G.DoorOpening).w, "door shut at s 0: no light through it");
                w.G.SetDoorForS(BulwarkConst.KeyShots[2].s);
                Vector4 dr = op.OpeningRect(w.G.DoorOpening);
                Assert.AreEqual(1f, op.OpeningPlane(w.G.DoorOpening).w, "door open at KS03");
                Assert.AreEqual(GrayboxPath.DoorGapAt(BulwarkConst.KeyShots[2].s), dr.y - dr.x, 1e-4, "opening width = the door gap");
                Assert.AreEqual(BulwarkConst.DoorPlaneZ, op.OpeningPlane(w.G.DoorOpening).x, 1e-6);
                Assert.AreEqual(GrayboxPath.WorldSkyPresent, !fullSky, "stand-in sky only without WorldSky");
            }
            var log = new List<string>();
            for (int key = 0; key < n; key++)
            {
                string K = GrayboxPath.PartKeys[key];
                using (var w = new GrayWorld(p => p == K))
                {
                    Assert.IsTrue(w.G.IsReplacedPart(key), K + " not marked replaced");
                    Assert.IsNull(w.G.PartGroup(key), K + ": pieces of the replaced part were built");
                    for (int k = 0; k < n; k++)
                    {
                        if (k == key) continue;
                        CollectionAssert.AreEqual(full[k], Signature(w.G.PartGroup(k)), K + " replaced changed the pieces of " + GrayboxPath.PartKeys[k]);
                    }
                    int anchors = w.G.AnchorIds.Count;
                    if (key == GrayboxPath.KeyLights) { Assert.AreEqual(0, anchors, "lights replaced: no graybox lamp anchors"); Assert.IsNull(w.G.LightPool); }
                    else if (key < GrayboxPath.BeatKeys) Assert.AreEqual(fullAnchors - fullAnchorsByKey[key], anchors, K + ": its lamp anchors are skipped, the others kept");
                    else Assert.AreEqual(fullAnchors, anchors, K + ": lamp anchors unchanged");
                    if (key < GrayboxPath.BeatKeys)
                    {
                        int hits = BeatProbes(w.RootGo, key + 1, out string first);
                        Assert.AreEqual(0, hits, K + " replaced but " + hits + " probes still hit graybox geometry (first: " + first + ")");
                    }
                    string sea = Probe(w.RootGo, new Vector3(100f, 50f, 300f), Vector3.down, 100f);
                    if (key == GrayboxPath.KeySea) Assert.IsNull(sea, "sea replaced but the ray hit " + sea);
                    else Assert.AreEqual(fullSea, sea, K + ": the sea probe changed");
                    int openings = w.G.OpeningPool.OpeningCount;
                    Assert.AreEqual(key == 0 ? 3 : key == 3 ? 1 : 4, openings, K + ": the openings of its shell pieces only");
                    Assert.AreEqual(key == 0, w.G.DoorOpening < 0, K + ": door opening");
                    if (key == GrayboxPath.KeySky) Assert.IsFalse(w.G.BuiltSky, "sky replaced but the stand-in sky was built");
                    else Assert.AreEqual(fullSky, w.G.BuiltSky);
                    log.Add(K + ": " + (full[key].Count) + " pieces skipped, anchors " + anchors + "/" + fullAnchors);
                }
            }
            Debug.Log("[BW] graybox replacement keys: " + string.Join("; ", log));
        }

        // ------------------------------------------------------------------ LocalLightPool (14.2.3, Part 10 #6)

        /// <summary>The pool's irradiance at every anchor's pool point (1.5 m below the lamp) within 25 m of the camera,
        /// relative to that lamp's own pool level, between consecutive frames: the largest relative step.</summary>
        static double WorstStep(LocalLightPool pool, Vector3[] probes, double[] level, double[] prev, Vector3 cam, bool first)
        {
            double worst = 0;
            for (int j = 0; j < probes.Length; j++)
            {
                double e = pool.Irradiance(probes[j]);
                if (!first && (probes[j] - cam).magnitude <= 25f) worst = Math.Max(worst, Math.Abs(e - prev[j]) / level[j]);
                prev[j] = e;
            }
            return worst;
        }

        [Test]
        public void LightPool_IsContinuousAlongTheWalkAndUnderAWhipPan()
        {
            // The graybox lamp anchors in a Light-free pool. (1) Walk the cinematic path at 2.5 m/s in 30 fps frames looking
            // along the path: at every pool point within 25 m the light from the 4-light set changes by less than 15 % of that
            // lamp's pool level per frame (a light switching on at full power scores 1.0). (2) A whip pan (180 deg in 3 frames)
            // in the gallery: no step above dt / FadeSeconds (6.7 %) plus the walk's share. (3) Never more than 4 lit.
            // (4) Steady state (capture stills) is a pure function of the pose: two different histories give identical slots.
            using (var w = new GrayWorld(_ => false, false))
            {
                var src = w.G.LightPool;
                Assert.IsNotNull(src, "graybox light pool");
                var pool = new LocalLightPool();
                foreach (int id in w.G.AnchorIds) { var a = src.GetAnchor(id); pool.Add(a.position, a.colorLinear, a.range, a.intensity, a.owner); }
                int N = pool.AnchorCount;
                Assert.Greater(N, 20, "lamp anchors");
                var probes = new Vector3[N];
                var level = new double[N];
                var prev = new double[N];
                for (int a = 0; a < N; a++)
                {
                    var an = pool.GetAnchor(a);
                    probes[a] = an.position + Vector3.down * 1.5f;
                    level[a] = an.intensity * LocalLightPool.Attenuation(1.5f, an.range);
                }
                const float dt = 1f / 30f;
                double step = VisibilityAudit.FollowerSpeed / 30.0, worst = 0, worstS = double.NaN;
                int maxLit = 0;
                bool first = true;
                for (double s = 0.0; s <= BwPath.Length + 1e-9; s += step)
                {
                    BwPath.World(s, BwPath.CinematicLateral(s), out double cx, out _, out double cz);
                    BwPath.Tangent(s, out double tx, out double tz);
                    var cam = new Vector3((float)cx, (float)BwPath.EyeY(s), (float)cz);
                    pool.Step(cam, new Vector3((float)tx, 0f, (float)tz), 64f, dt, false);
                    maxLit = Math.Max(maxLit, pool.LitCount);
                    double d = WorstStep(pool, probes, level, prev, cam, first);
                    if (d > worst) { worst = d; worstS = s; }
                    first = false;
                }
                // whip pan at s 40 (gallery): yaw 0 -> 180 over 3 frames, hold 30 frames, back
                var camG = new Vector3(-1.5f, 1.7f, 40f);
                double worstPan = 0;
                pool.Step(camG, Vector3.forward, 62f, dt, true);
                WorstStep(pool, probes, level, prev, camG, true);
                for (int f = 1; f <= 70; f++)
                {
                    float yaw = f <= 3 ? 60f * f : (f <= 33 ? 180f : (f <= 36 ? 180f - 60f * (f - 33) : 0f));
                    var fw = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
                    pool.Step(camG, fw, 62f, dt, false);
                    maxLit = Math.Max(maxLit, pool.LitCount);
                    worstPan = Math.Max(worstPan, WorstStep(pool, probes, level, prev, camG, false));
                }
                // steady state independence of the history
                var camS = new Vector3(-1.5f, 1.7f, 63f);
                var a1 = new int[LocalLightPool.MaxLights]; var w1 = new float[LocalLightPool.MaxLights];
                pool.Step(camS, Vector3.forward, 62f, dt, true);
                for (int i = 0; i < a1.Length; i++) { a1[i] = pool.SlotAnchor(i); w1[i] = pool.SlotWeight(i); }
                var fresh = new LocalLightPool();
                for (int a = 0; a < N; a++) { var an = pool.GetAnchor(a); fresh.Add(an.position, an.colorLinear, an.range, an.intensity, an.owner); }
                fresh.Step(new Vector3(-1.5f, 1.7f, 10f), Vector3.back, 62f, dt, false);
                fresh.Step(camS, Vector3.forward, 62f, dt, true);
                for (int i = 0; i < a1.Length; i++) { Assert.AreEqual(a1[i], fresh.SlotAnchor(i), "steady slot " + i); Assert.AreEqual(w1[i], fresh.SlotWeight(i), 1e-6, "steady weight " + i); }

                Debug.Log("[BW] light pool continuity: " + N + " anchors, " + Math.Round(BwPath.Length / step) + " frames at 2.5 m/s / 30 fps; worst frame step at a pool "
                          + (100 * worst).ToString("0.0") + " % of its lamp's pool (s " + worstS.ToString("0.0") + "), whip pan " + (100 * worstPan).ToString("0.0")
                          + " % (time fade " + (100 * dt / LocalLightPool.FadeSeconds).ToString("0.0") + " % per frame), max lit " + maxLit);
                Assert.Less(worst, 0.15, "a lamp pool changes by more than 15 % of its level in one 30 fps frame (pop)");
                Assert.Less(worstPan, 0.15, "a whip pan pops a lamp pool");
                Assert.LessOrEqual(maxLit, LocalLightPool.MaxLights, "at most 4 real local lights (14.2.3)");
            }
        }
    }
}
