// FoliageTests: EditMode checks of the Look v2 foliage (TEXTURE_PLAN 4 and 8.3 FO; -foliage-v 2), on the real GardenTree /
// GardenLeafCards output of all 17 trees emitted through GardenTree.EmitCrowns:
//   - the budget: 135 k cards and 270 k card triangles or fewer, one card mesh per tree (17 draws), each tree near its target
//     (1.5 x shell area / 4.4 m^2, x 0.5 for the back trees);
//   - every card corner inside 1.10 x the crown ellipsoid and 1 m inside face B's notch walls;
//   - the vertex contract of the shader (slice, hash, octahedral card normal, winding, radial normal within 55 deg, tangents,
//     card sizes 1.2-2.6 m), valid meshes with 32-bit indices;
//   - the core covers the crown minus the card fringe (rays through points inside 0.80 x the lobed crown hit the core);
//   - the crowns are lobed (sub-crowns and valleys break the silhouette) and every limb end stays inside the core;
//   - bark UV0 wraps a whole number of 1.8 m tiles around every ring (seam vertex pairs differ by whole tiles);
//   - the v2 materials (LeafCore queue 2000, Leaves queue 2450 with AlphaToMask on and Cull off; the ForwardLit pass has
//     AlphaToMask [_AlphaToMask]); the v1 path emits only the graybox crowns;
//   - FoliageVisibilityTests: V's audit (VisibilityWorld + VisibilityAudit, GE's code, run unchanged) on the v1 and the v2 world:
//     v2 adds no occlusion / hidden / composition finding (the cards count as opaque triangles there: queue 2450 < 2500, so
//     the check is conservative). It writes no report (GE's visibility.json / .md stay the v1 audit's).
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Megastructure.EditorTools;

namespace Megastructure.Tests
{
    public class FoliageTests
    {
        WorldContext _ctx;
        Transform _city, _backdrop, _group;
        GardenMaterials _mats;
        readonly List<MeshFilter> _cards = new List<MeshFilter>();
        MeshFilter _cores;

        [OneTimeSetUp]
        public void Build()
        {
            GardenDims.FoliageVersionOverride = 2;
            NocWetRegistry.Clear();
            var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(NocPaths.LibraryAsset);
            _city = new GameObject("FoliageTestCity").transform;
            _backdrop = new GameObject("FoliageTestBackdrop").transform;
            _ctx = new WorldContext { Materials = lib, CityRoot = _city, BackdropRoot = _backdrop, Seed = 1 };
            _group = BridgeGardenKit.Group(_city, "trees");
            _mats = new GardenMaterials(lib);
            var trunks = new MeshBuilder();
            var crowns = new MeshBuilder();
            var trees = GardenDims.Trees;
            for (int i = 0; i < trees.Length; i++) GardenTree.Build(trees[i], 1000 + i, trunks, crowns);
            BridgeGardenKit.Emit(_ctx, _group, "trunks", trunks, _mats.Bark);
            GardenTree.EmitCrowns(_ctx, _group, crowns, _mats);
            foreach (var mf in _group.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.name.StartsWith("crown_cards_", StringComparison.Ordinal)) _cards.Add(mf);
                else if (mf.name == "crowns") _cores = mf;
            }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            GardenDims.FoliageVersionOverride = 0;
            NocWetRegistry.Clear();
            if (_city == null) return;
            var meshes = new HashSet<Mesh>();
            foreach (var mf in _city.GetComponentsInChildren<MeshFilter>(true)) if (mf.sharedMesh != null) meshes.Add(mf.sharedMesh);
            UnityEngine.Object.DestroyImmediate(_city.gameObject);
            UnityEngine.Object.DestroyImmediate(_backdrop.gameObject);
            foreach (var m in meshes) UnityEngine.Object.DestroyImmediate(m);
            if (_mats != null)
                foreach (var m in new[] { _mats.Foliage, _mats.LeafCore, _mats.Leaves, _mats.Moss })
                    if (m != null && !AssetDatabase.Contains(m)) UnityEngine.Object.DestroyImmediate(m);
        }

        static GardenTreeSpec TreeOf(MeshFilter mf)
        {
            string name = mf.name.Substring("crown_cards_".Length);
            foreach (var t in GardenDims.Trees) if (t.Name == name) return t;
            Assert.Fail("no tree " + name);
            return default;
        }

        [Test]
        public void Cards_WithinBudget_OneMeshPerTree()
        {
            Assert.AreEqual(GardenDims.Trees.Length, _cards.Count, "one card mesh per tree (17 draws)");
            Assert.AreEqual(0, GardenTree.PendingCardMeshes, "EmitCrowns emitted every pending card mesh");
            long tris = 0;
            int cards = 0, target = 0;
            var sb = new System.Text.StringBuilder("[FO] cards per tree:");
            foreach (var mf in _cards)
            {
                var t = TreeOf(mf);
                int n = mf.sharedMesh.vertexCount / 4;
                int want = GardenLeafCards.TargetCount(t, GardenDims.ShellInner) + GardenLeafCards.TargetCount(t, GardenDims.ShellOuter);
                sb.Append(FormattableString.Invariant($" {t.Name} {n}/{want};"));
                Assert.That(n, Is.InRange((int)(0.75f * want), (int)(1.25f * want) + 1), t.Name + " card count vs its target");
                tris += WorldContext.TriangleCount(mf.sharedMesh);
                cards += n;
                target += want;
            }
            Debug.Log(sb.Append(FormattableString.Invariant($" total {cards} / {target}, {tris} triangles")).ToString());
            Assert.AreEqual(2L * cards, tris, "two triangles per card");
            Assert.LessOrEqual(cards, 135000, "135 k cards or fewer (TEXTURE_PLAN 4)");
            Assert.LessOrEqual(tris, 270000, "270 k card triangles or fewer");
            Assert.That(cards, Is.InRange((int)(0.85f * target), (int)(1.10f * target)), "total near the 1.5x coverage target");
        }

        [Test]
        public void Cards_InsideTheEnvelope_AndClearOfTheNotchWalls()
        {
            var v = new List<Vector3>();
            foreach (var mf in _cards)
            {
                var t = TreeOf(mf);
                GardenLeafCards.Envelope(t, out Vector3 ec, out Vector3 er);
                Assert.AreEqual(t.CrownCentre, ec);
                Assert.AreEqual(t.CrownRadii.x * GardenDims.EnvelopeFrac, er.x, 1e-4f);
                mf.sharedMesh.GetVertices(v);
                float maxR = 0f;
                foreach (var p in v)
                {
                    float r = GardenLeafCards.NormRadius(t, p);
                    maxR = Mathf.Max(maxR, r);
                    Assert.LessOrEqual(r, GardenDims.EnvelopeFrac + 1e-4f, t.Name + FormattableString.Invariant($" card corner {p} outside 1.10 x the crown"));
                    Assert.IsTrue(GardenLeafCards.ClearOfNotchWalls(p), t.Name + FormattableString.Invariant($" card corner {p} within 1 m of a notch wall"));
                }
                Assert.Greater(maxR, 1.0f, t.Name + " the outer shell breaks the silhouette");
            }
        }

        [Test]
        public void Cards_VertexContract_AndValidMeshes()
        {
            var pos = new List<Vector3>();
            var nrm = new List<Vector3>();
            var tan = new List<Vector4>();
            var uv0 = new List<Vector2>();
            var uv1 = new List<Vector4>();
            var col = new List<Color32>();
            var idx = new List<int>();
            float cosMax = Mathf.Cos((GardenDims.CardTiltMaxDeg + 5f) * Mathf.Deg2Rad);
            int glow = 0, total = 0;
            var slices = new int[GardenLeafCards.Sprites];
            foreach (var mf in _cards)
            {
                var m = mf.sharedMesh;
                var rep = MeshValidation.Validate(m);
                Assert.IsTrue(rep.Ok, rep.Describe());
                Assert.AreEqual(m.vertexCount > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16, m.indexFormat);
                m.GetVertices(pos); m.GetNormals(nrm); m.GetTangents(tan); m.GetUVs(0, uv0); m.GetUVs(1, uv1); m.GetColors(col);
                m.GetIndices(idx, 0);
                Assert.AreEqual(pos.Count, tan.Count, mf.name + " tangents");
                Assert.AreEqual(pos.Count, uv1.Count, mf.name + " uv1");
                for (int q = 0; q < pos.Count; q += 4)
                {
                    Vector4 u = uv1[q];
                    int slice = Mathf.RoundToInt(u.x);
                    Assert.AreEqual(slice, u.x, 1e-6f, "slice is an integer");
                    Assert.That(slice, Is.InRange(0, GardenLeafCards.Sprites - 1));
                    slices[slice]++;
                    Assert.That(u.y, Is.InRange(0f, 1f));
                    Vector3 nc = GardenLeafCards.OctDecode(new Vector2(u.z, u.w));
                    for (int k = 1; k < 4; k++) Assert.AreEqual(new Vector2(u.x, u.y), new Vector2(uv1[q + k].x, uv1[q + k].y), "slice and hash constant per card");
                    for (int k = 1; k < 4; k++) Assert.AreEqual(new Vector2(u.z, u.w), new Vector2(uv1[q + k].z, uv1[q + k].w), "card normal constant per card");
                    Vector3 w = Vector3.Cross(pos[q + 1] - pos[q], pos[q + 2] - pos[q]).normalized;
                    Assert.Greater(Vector3.Dot(w, nc), 0.999f, "front face along the card normal");
                    float side = (pos[q + 1] - pos[q]).magnitude;
                    Assert.That(side, Is.InRange(GardenDims.CardMinShrunk - 1e-3f, GardenDims.CardMax + 1e-3f), "card size");
                    Assert.AreEqual(side, (pos[q + 3] - pos[q]).magnitude, 1e-3f, "square card");
                    Vector3 a1 = (pos[q + 1] - pos[q]) / side;
                    for (int k = 0; k < 4; k++)
                    {
                        Assert.GreaterOrEqual(Vector3.Dot(nrm[q + k], nc), cosMax, "radial normal within 55 deg of the card normal");
                        Vector3 tk = tan[q + k];
                        Assert.AreEqual(1f, tan[q + k].w);
                        Assert.Greater(Vector3.Dot(tk, a1), 0.999f, "tangent = card u");
                        Assert.Less(Mathf.Abs(Vector3.Dot(tk, nc)), 1e-3f, "tangent in the card plane");
                    }
                    Assert.AreEqual(new Vector2(0f, 0f), uv0[q]);
                    Assert.AreEqual(new Vector2(1f, 1f), uv0[q + 2]);
                    if (col[q].g > 127) glow++;
                    total++;
                }
            }
            float gf = (float)glow / Mathf.Max(total, 1);
            Assert.That(gf, Is.InRange(0.015f, 0.035f), "bioluminescent cards are sparse (2.5 %)");
            foreach (int s in slices) Assert.Greater(s, total / 8, "every sprite is used");
        }

        [Test]
        public void Core_CoversTheCrownMinusTheFringe()
        {
            Assert.IsNotNull(_cores, "the cores are emitted as 'crowns'");
            Assert.AreEqual(_mats.LeafCore, _cores.GetComponent<MeshRenderer>().sharedMaterial, "cores use LeafCore");
            var trees = GardenDims.Trees;
            int misses = 0, rays = 0;
            for (int i = 0; i < trees.Length; i += 2)
            {
                var t = trees[i];
                var mb = new MeshBuilder();
                GardenTree.CoreV2(mb, t, 1000 + i);
                var shape = GardenLeafCards.Shape(t, 1000 + i);
                Vector3 R = t.CrownRadii, c = t.CrownCentre;
                for (int k = 0; k < 24; k++)
                {
                    Vector3 dir = RandomUnit(i, k, 0);
                    Vector3 u = RandomUnit(i, k, 1);
                    Vector3 w = u * Mathf.Pow(BridgeGardenKit.Hash01(i * 97 + k, 77), 1f / 3f);
                    Vector3 q = c + Vector3.Scale(w, R) * (0.80f * shape.Factor(u));
                    Vector3 o = q - dir * (3f * R.x);
                    rays++;
                    if (!HitsMesh(mb, o, dir)) misses++;
                }
                // the core's smallest normalised radius is CoreFrac x the valley floor x the fine lump; the cards reach 1.10
                float floor = GardenDims.CoreFrac * GardenDims.LobeFloor * (1f - GardenDims.FineLump) - 1e-3f;
                foreach (var p in mb.Positions) Assert.GreaterOrEqual(GardenLeafCards.NormRadius(t, p), floor, t.Name + " core valley floor");
            }
            Assert.AreEqual(0, misses, FormattableString.Invariant($"{misses} of {rays} rays through 0.80 x the crown miss the core"));
        }

        [Test]
        public void Crowns_AreLobed_AndLimbEndsStayInsideTheCore()
        {
            var trees = GardenDims.Trees;
            for (int i = 0; i < trees.Length; i++)
            {
                var t = trees[i];
                var shape = GardenLeafCards.Shape(t, 1000 + i);
                // the outline seen from straight below (per azimuth the widest horizontal extent over the elevations) and a vertical
                // great circle (seen from the side): lobe tops are maxima over +-20 deg that stand 0.03 above both ends of that window
                for (int circle = 0; circle < 2; circle++)
                {
                    const int n = 720, w = 40;
                    var f = new float[n];
                    float lo = 9f, hi = 0f;
                    for (int k = 0; k < n; k++)
                    {
                        float a = 2f * Mathf.PI * k / n;
                        if (circle == 0)
                        {
                            float best = 0f;
                            for (int e = -20; e <= 20; e++)
                            {
                                float th = e * 4f * Mathf.Deg2Rad;
                                var de = new Vector3(Mathf.Cos(th) * Mathf.Cos(a), Mathf.Sin(th), Mathf.Cos(th) * Mathf.Sin(a));
                                best = Mathf.Max(best, shape.Factor(de) * Mathf.Cos(th));
                            }
                            f[k] = best;
                        }
                        else f[k] = shape.Factor(new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f));
                        lo = Mathf.Min(lo, f[k]); hi = Mathf.Max(hi, f[k]);
                    }
                    int peaks = 0;
                    for (int k = 0; k < n; k++)
                    {
                        float b = f[k];
                        if (b == f[(k + n - 1) % n]) continue;
                        bool max = true;
                        for (int j = -w; j <= w; j++) if (f[(k + j + n) % n] > b) { max = false; break; }
                        if (max && b - Mathf.Max(f[(k + n - w) % n], f[(k + w) % n]) > 0.03f) peaks++;
                    }
                    float valleyMax = circle == 0 ? 0.86f : 0.82f;               // from below, other elevations partly fill a valley
                    Assert.LessOrEqual(lo, valleyMax, t.Name + FormattableString.Invariant($" circle {circle}: valleys reach {valleyMax} or less (min {lo:F3})"));
                    Assert.GreaterOrEqual(hi, 0.93f, t.Name + FormattableString.Invariant($" circle {circle}: lobe tops reach 0.93 (max {hi:F3})"));
                    // lobe tops counted on the hero crowns' outline from below (the KS15 view); a single great circle of a crown can
                    // run along one lobe ridge, so the other outlines only need their valley and their top (above)
                    if (t.Hero && circle == 0)
                        Assert.GreaterOrEqual(peaks, 2, t.Name + FormattableString.Invariant($" outline from below: lobe tops ({peaks})"));
                }
                for (int k = 0; k < 64; k++)
                {
                    Vector3 p = t.CrownCentre + Vector3.Scale(RandomUnit(i, k, 4), t.CrownRadii) * (0.3f + 0.9f * BridgeGardenKit.Hash01(i * 71 + k, 78));
                    Vector3 q = GardenTree.InsideCore(t, 1000 + i, p);
                    float coreN = GardenDims.CoreFrac * shape.Factor(GardenLeafCards.NormDir(t, q));
                    Assert.LessOrEqual(GardenLeafCards.NormRadius(t, q), GardenTree.LimbEndInCore * coreN + 1e-4f, t.Name + " limb end inside the core");
                }
            }
        }

        [Test]
        public void Bark_Uv0_WholeTilesAroundEveryRing()
        {
            var trees = GardenDims.Trees;
            for (int v = 1; v <= 2; v++)
            {
                GardenDims.FoliageVersionOverride = v;
                try
                {
                    for (int i = 0; i < trees.Length; i += 4)
                    {
                        var trunk = new MeshBuilder();
                        GardenTree.Build(trees[i], 1000 + i, trunk, new MeshBuilder());
                        // ring 0 of the trunk loft: u 0 .. N x 1.8 with N = TrunkBarkTiles
                        int cols = GardenDims.TrunkSides + 1;
                        int n = GardenTree.TrunkBarkTiles(trees[i]);
                        Assert.AreEqual(0f, trunk.UV0[0].x, 1e-4f);
                        Assert.AreEqual(n * GardenDims.BarkTile, trunk.UV0[cols - 1].x, 1e-4f, "trunk wraps N whole tiles");
                        Assert.Less((trunk.Positions[0] - trunk.Positions[cols - 1]).magnitude, 1e-4f, "seam closes");
                        int pairs = SeamPairs(trunk, out int bad);
                        Assert.Greater(pairs, GardenDims.TrunkRings, "seam pairs found");
                        Assert.AreEqual(0, bad, FormattableString.Invariant($"v{v} {trees[i].Name}: {bad} of {pairs} seam pairs are not whole bark tiles apart"));
                    }
                }
                finally { GardenDims.FoliageVersionOverride = 2; }
            }
            GardenTree.EmitCrowns(_ctx, BridgeGardenKit.Group(_city, "scratch"), new MeshBuilder(), _mats);   // drop the pending cards
        }

        [Test]
        public void Materials_V2_AlphaToCoverage_AndQueues()
        {
            var sh = Shader.Find(GardenMaterials.FoliageShader);
            Assert.IsNotNull(sh, "NocFoliage compiles and is found");
            Assert.IsNotNull(_mats.LeafCore);
            Assert.IsNotNull(_mats.Leaves);
            Assert.AreEqual(sh, _mats.Leaves.shader);
            Assert.AreEqual(GardenMaterials.CardQueue, _mats.Leaves.renderQueue, "cards at AlphaTest 2450");
            Assert.AreEqual(GardenMaterials.CoreQueue, _mats.LeafCore.renderQueue, "core before the cards");
            Assert.AreEqual(GardenMaterials.ModeCards, _mats.Leaves.GetFloat("_LeafMode"));
            Assert.AreEqual(GardenMaterials.ModeCore, _mats.LeafCore.GetFloat("_LeafMode"));
            Assert.AreEqual(1f, _mats.Leaves.GetFloat("_AlphaToMask"), "AlphaToMask on for the cards");
            Assert.AreEqual(0f, _mats.LeafCore.GetFloat("_AlphaToMask"));
            Assert.AreEqual(0f, _mats.Leaves.GetFloat("_Cull"), "cards two-sided");
            foreach (var mf in _cards) Assert.AreEqual(_mats.Leaves, mf.GetComponent<MeshRenderer>().sharedMaterial);
            string src = System.IO.File.ReadAllText(AssetDatabase.GetAssetPath(sh));
            int fwd = src.IndexOf("Name \"ForwardLit\"", StringComparison.Ordinal);
            int next = src.IndexOf("Name \"ShadowCaster\"", StringComparison.Ordinal);
            Assert.Greater(fwd, 0);
            string pass = src.Substring(fwd, next - fwd);
            StringAssert.Contains("AlphaToMask [_AlphaToMask]", pass, "alpha to coverage in the forward pass");
            StringAssert.Contains("#define NOC_ALPHA_OUT", pass);
            StringAssert.Contains("#define NOC_FRONT_FACE", pass);
            StringAssert.DoesNotContain("shader_feature", src, "no new keyword");
            Assert.AreEqual(0f, _mats.Moss != null ? _mats.Moss.GetFloat("_PuddleFraction") : 0f, "no puddle blots on the lawn");
        }

        [Test]
        public void V1_EmitsOnlyTheGrayboxCrowns()
        {
            GardenDims.FoliageVersionOverride = 1;
            try
            {
                var g = BridgeGardenKit.Group(_city, "v1");
                var crowns = new MeshBuilder();
                var t = GardenDims.Trees[0];
                GardenTree.Build(t, 1000, new MeshBuilder(), crowns);
                Assert.AreEqual(0, GardenTree.PendingCardMeshes);
                Assert.AreEqual((GardenDims.CrownLat - 1) * GardenDims.CrownLon + 2, crowns.VertexCount, "one lumpy crown");
                var m1 = new GardenMaterials(null);
                Assert.IsNull(m1.Leaves);
                Assert.IsNull(m1.LeafCore);
                GardenTree.EmitCrowns(_ctx, g, crowns, m1);
                Assert.AreEqual(1, g.childCount);
                Assert.AreEqual("crowns", g.GetChild(0).name);
                Assert.AreEqual(m1.Foliage, g.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial);
                if (m1.Foliage != null) UnityEngine.Object.DestroyImmediate(m1.Foliage);
            }
            finally { GardenDims.FoliageVersionOverride = 2; }
        }

        [Test]
        public void OctahedralNormal_RoundTrips()
        {
            for (int k = 0; k < 2000; k++)
            {
                Vector3 n = RandomUnit(k, 3, 2);
                Vector3 d = GardenLeafCards.OctDecode(GardenLeafCards.OctEncode(n));
                Assert.Greater(Vector3.Dot(n, d), 0.99999f, FormattableString.Invariant($"{n}"));
            }
            foreach (var n in new[] { Vector3.up, Vector3.down, Vector3.forward, Vector3.back, Vector3.left, Vector3.right })
                Assert.Greater(Vector3.Dot(n, GardenLeafCards.OctDecode(GardenLeafCards.OctEncode(n))), 0.99999f);
        }

        // ------------------------------------------------------------------------------------------ helpers

        static Vector3 RandomUnit(int a, int b, int c)
        {
            float z = 2f * BridgeGardenKit.Hash01(a * 131 + b, 900 + c) - 1f;
            float ph = 2f * Mathf.PI * BridgeGardenKit.Hash01(a * 131 + b, 950 + c);
            float r = Mathf.Sqrt(Mathf.Max(0f, 1f - z * z));
            return new Vector3(r * Mathf.Cos(ph), z, r * Mathf.Sin(ph));
        }

        /// <summary>Moller-Trumbore against every triangle of mb (both sides).</summary>
        static bool HitsMesh(MeshBuilder mb, Vector3 o, Vector3 d)
        {
            var p = mb.Positions;
            var ix = mb.Indices;
            for (int t = 0; t < ix.Count; t += 3)
            {
                Vector3 a = p[ix[t]], e1 = p[ix[t + 1]] - a, e2 = p[ix[t + 2]] - a;
                Vector3 h = Vector3.Cross(d, e2);
                float det = Vector3.Dot(e1, h);
                if (Mathf.Abs(det) < 1e-9f) continue;
                float inv = 1f / det;
                Vector3 s = o - a;
                float u = inv * Vector3.Dot(s, h);
                if (u < -1e-5f || u > 1f + 1e-5f) continue;
                Vector3 q = Vector3.Cross(s, e1);
                float v = inv * Vector3.Dot(d, q);
                if (v < -1e-5f || u + v > 1f + 1e-5f) continue;
                if (inv * Vector3.Dot(e2, q) > 0f) return true;
            }
            return false;
        }

        /// <summary>Vertex pairs at the same position and the same v (the u seams of lofts and lathes); bad = pairs whose u differ
        /// by other than a whole number of bark tiles.</summary>
        static int SeamPairs(MeshBuilder mb, out int bad)
        {
            bad = 0;
            int pairs = 0;
            var map = new Dictionary<Vector3Int, List<int>>();
            for (int i = 0; i < mb.VertexCount; i++)
            {
                Vector3 p = mb.Positions[i];
                var key = new Vector3Int(Mathf.RoundToInt(p.x * 1000f), Mathf.RoundToInt(p.y * 1000f), Mathf.RoundToInt(p.z * 1000f));
                if (!map.TryGetValue(key, out var list)) map[key] = list = new List<int>(2);
                foreach (int j in list)
                {
                    if ((mb.Positions[j] - p).sqrMagnitude > 1e-8f || Mathf.Abs(mb.UV0[j].y - mb.UV0[i].y) > 1e-4f) continue;
                    float du = Mathf.Abs(mb.UV0[j].x - mb.UV0[i].x) / GardenDims.BarkTile;
                    if (du < 1e-4f) continue;                                    // shared vertex, not a seam
                    pairs++;
                    if (Mathf.Abs(du - Mathf.Round(du)) > 1e-3f) bad++;
                }
                list.Add(i);
            }
            return pairs;
        }
    }

    public class FoliageVisibilityTests
    {
        static readonly string[] Kinds = { "occlusion", "hidden", "composition", "composition-built", "frame", "readable" };

        static HashSet<string> Findings(int version, out int total, out string world)
        {
            GardenDims.FoliageVersionOverride = version;
            NocWetRegistry.Clear();
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            try
            {
                VisibilityRaycaster built = VisibilityWorld.Build(out VisibilityWorld.Info info);
                world = info.ToString();
                var audit = new VisibilityAudit(new VisibilitySpecWorld(), built.TriangleCount > 0 ? built : null) { BuiltInfo = world };
                audit.Run();
                total = audit.Failures.Count;
                var set = new HashSet<string>();
                foreach (var f in audit.Failures)
                    if (Array.IndexOf(Kinds, f.kind) >= 0) set.Add(f.shot + " | " + f.subject + " | " + f.kind);
                return set;
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = culture;
                GardenDims.FoliageVersionOverride = 0;
                NocWetRegistry.Clear();
            }
        }

        [Test]
        public void V2_AddsNoVisibilityFinding()
        {
            var v1 = Findings(1, out int n1, out string w1);
            var v2 = Findings(2, out int n2, out string w2);
            var added = new List<string>();
            foreach (var f in v2) if (!v1.Contains(f)) added.Add(f);
            Debug.Log(FormattableString.Invariant($"[FO] visibility audit v1: {n1} failures ({v1.Count} of the checked kinds), world {w1}"));
            Debug.Log(FormattableString.Invariant($"[FO] visibility audit v2: {n2} failures ({v2.Count} of the checked kinds), world {w2}"));
            foreach (var f in added) Debug.Log("[FO] v2 adds: " + f);
            Assert.IsEmpty(added, "findings that only the v2 crowns cause: " + string.Join("; ", added));
        }
    }
}
