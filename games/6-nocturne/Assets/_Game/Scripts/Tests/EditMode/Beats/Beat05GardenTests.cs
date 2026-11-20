// Beat05GardenTests: EditMode acceptance checks of beat 5, the hanging garden (MASTER_PROMPT 14.5.5 Acceptance (1) and (4), the
// Phase-3 agreements of docs/logs/G3.md), on the real builder output (beats 4 and 5 together, so the deck / terrace seam is real):
// hero tree heights 95/120/88/130/100/125/92/118/104 +- 0.5 and 17 trees, every crown the ellipsoid (r, 0.28 h, r) at 0.66 h within
// its lumps, back trees on the terrace at |z - 195| 55-70 and 110-140 m, crowns inside the notch walls, trunks apart and clear of
// the lane, the path 3.0 +- 0.02, the terrace top flat at y 0, covering the terrace exactly once (area 219 x 160 m) and sharing every
// vertex with the deck end (no T-junction on the floor seam), furniture outside the lane, every mesh valid, <= 750 k triangles.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class Beat05GardenTests
    {
        G3BeatWorld _world;
        Transform _root, _bridge;
        long _tris;
        static readonly string[] TopMeshes = { "path", "moss", "promenade" };

        [OneTimeSetUp]
        public void Build()
        {
            _world = G3BeatWorld.Build(new Beat04_Skybridge());
            long t4 = _world.Ctx.Triangles;
            new Beat05_Garden().Build(_world.Ctx);
            _tris = _world.Ctx.Triangles - t4;
            _root = _world.Ctx.BeatRoot(Beat05_Garden.BeatIndex);
            _bridge = _world.Ctx.BeatRoot(Beat04_Skybridge.BeatIndex);
        }

        [OneTimeTearDown]
        public void TearDown() => _world?.Close();

        [Test]
        public void TreeHeights_And17Trees()
        {
            float[] spec = { 95f, 120f, 88f, 130f, 100f, 125f, 92f, 118f, 104f };
            var trees = GardenDims.Trees;
            Assert.AreEqual(17, trees.Length, "17 trees (14.5.5)");
            var sb = new System.Text.StringBuilder("[G3] trees:");
            for (int i = 0; i < trees.Length; i++)
            {
                var t = trees[i];
                if (i < spec.Length) Assert.AreEqual(spec[i], t.Height, 0.5, t.Name + " height");
                sb.Append(FormattableString.Invariant($" {t.Name}({t.X:F0},{t.Z:F0}) h {t.Height:F1} r {t.CrownR:F1} r0 {t.R0:F2} crownZ {t.CrownZ:F1};"));
                // the crown geometry: one crown alone, bounds = the ellipsoid within its lumps (0.90 .. 1.02)
                var mb = new MeshBuilder();
                GardenTree.Crown(mb, t.CrownCentre, t.CrownRadii, 1000 + i);
                var b = mb.ToMesh("crownTest").bounds;
                Assert.AreEqual(t.CrownCentre.y, b.center.y, 0.06f * t.CrownRadii.y, t.Name + " crown centre at 0.66 h");
                Assert.LessOrEqual(b.max.y, t.CrownCentre.y + 1.021f * t.CrownRadii.y, t.Name + " crown top");
                Assert.GreaterOrEqual(b.max.y, t.CrownCentre.y + 0.89f * t.CrownRadii.y, t.Name + " crown top");
                Assert.LessOrEqual(b.extents.x, 1.021f * t.CrownR, t.Name + " crown radius");
                Assert.GreaterOrEqual(b.extents.x, 0.89f * t.CrownR, t.Name + " crown radius");
            }
            Debug.Log(sb.ToString());
        }

        [Test]
        public void BackTrees_OnTheTerrace_AndCrownsInsideTheNotch()
        {
            foreach (var t in GardenDims.Trees)
            {
                float fr = GardenTree.FootprintRadius(t.R0);
                Assert.GreaterOrEqual(t.X - fr, GardenDims.TerraceX0, t.Name + " trunk on the terrace (x)");
                Assert.LessOrEqual(t.X + fr, GardenDims.TerraceX1, t.Name + " trunk on the terrace (x)");
                Assert.GreaterOrEqual(t.Z - fr, GardenDims.TerraceZ0, t.Name + " trunk on the terrace (z)");
                Assert.LessOrEqual(t.Z + fr, GardenDims.TerraceZ1, t.Name + " trunk on the terrace (z)");
                if (!t.Hero)
                {
                    float dz = Mathf.Abs(t.Z - GardenDims.AxisZ);
                    Assert.That(dz, Is.InRange(GardenDims.BackDzMin, GardenDims.BackDzMax), t.Name + " |z - 195|");
                    Assert.That(t.Height, Is.InRange(GardenDims.BackHMin, GardenDims.BackHMax), t.Name + " height");
                    Assert.LessOrEqual(Mathf.Abs(t.CrownZ - t.Z), GardenDims.BackMaxLean + 1e-4f, t.Name + " lean");
                }
                // a crown that overlaps the notch's x range stays inside its walls (z 115 / 275)
                if (t.X + 1.02f * t.CrownR > GardenDims.FaceBX)
                {
                    Assert.GreaterOrEqual(t.CrownZ - 1.02f * t.CrownR, GardenDims.TerraceZ0, t.Name + " crown vs the south notch wall");
                    Assert.LessOrEqual(t.CrownZ + 1.02f * t.CrownR, GardenDims.TerraceZ1, t.Name + " crown vs the north notch wall");
                }
            }
        }

        [Test]
        public void Trunks_Apart_AndClearOfTheLane()
        {
            var trees = GardenDims.Trees;
            for (int i = 0; i < trees.Length; i++)
            {
                var a = trees[i];
                float ra = GardenTree.FootprintRadius(a.R0);
                for (int j = i + 1; j < trees.Length; j++)
                {
                    var b = trees[j];
                    float d = new Vector2(a.X - b.X, a.Z - b.Z).magnitude;
                    Assert.GreaterOrEqual(d, ra + GardenTree.FootprintRadius(b.R0), a.Name + " / " + b.Name + " trunk footprints overlap");
                }
                double lat = G3BeatWorld.Lateral(new Vector3(a.X, 0f, a.Z), 980.0, out double s);
                Assert.GreaterOrEqual(lat - ra, NocLanes.LaneHalfWidth(s) + 1.0, a.Name + " trunk within 1 m of the lane");
            }
        }

        [Test]
        public void Path_Width3()
        {
            var l = new List<Vector2>(); var r = new List<Vector2>();
            GardenTerrace.PathEdges(l, r, GardenDims.PathHalfWidth, out _);
            for (int i = 0; i < l.Count; i++) Assert.AreEqual(3.0, (l[i] - r[i]).magnitude, 0.02, "flagstone path 3.0 m (14.5.5)");
            for (double s = 885; s <= 1100; s += 5) Assert.AreEqual(3.0, GardenTerrace.PathWidthAt(s), 0.02);
            var m = G3BeatWorld.MeshOf(_root, "path");
            Assert.AreEqual(GardenDims.TerraceX0, m.bounds.min.x, 1e-3, "path starts at gate B's east face");
        }

        [Test]
        public void TerraceTop_FlatAtY0_CoversTheTerraceOnce()
        {
            double area = 0;
            foreach (var name in TopMeshes)
            {
                var m = G3BeatWorld.MeshOf(_root, name);
                var v = m.vertices; var t = m.triangles;
                foreach (var p in v) Assert.AreEqual(0f, p.y, 1e-6f, name + " top at y 0");
                for (int i = 0; i < t.Length; i += 3)
                {
                    Vector3 a = v[t[i]], b = v[t[i + 1]], c = v[t[i + 2]];
                    var cr = Vector3.Cross(b - a, c - a);
                    Assert.Greater(cr.y, 0f, name + " triangle faces up");
                    area += 0.5 * cr.y;
                }
            }
            double want = (GardenDims.TerraceX1 - GardenDims.TerraceX0) * (GardenDims.TerraceZ1 - GardenDims.TerraceZ0);
            Debug.Log(FormattableString.Invariant($"[G3] terrace top area {area:F2} m2 (rectangle {want:F2})"));
            Assert.AreEqual(want, area, 0.5, "the top pieces tile the terrace exactly once (no hole, no overlap)");
        }

        [Test]
        public void FloorSeam_DeckEnd_SharesEveryVertex()
        {
            // every vertex of a floor piece on the line x 695 inside the walkway opening exists on the other side of the seam
            var deck = new HashSet<Vector3>();
            foreach (var p in G3BeatWorld.MeshOf(_bridge, "deckTop").vertices)
                if (Mathf.Abs(p.x - GardenDims.TerraceX0) < 1e-4f) deck.Add(p);
            var terrace = new HashSet<Vector3>();
            foreach (var name in TopMeshes)
                foreach (var p in G3BeatWorld.MeshOf(_root, name).vertices)
                    if (Mathf.Abs(p.x - GardenDims.TerraceX0) < 1e-4f && p.z >= SkybridgeDims.OpeningZ0 - 1e-4f && p.z <= SkybridgeDims.OpeningZ1 + 1e-4f) terrace.Add(p);
            Assert.Greater(deck.Count, 0);
            foreach (var p in deck) Assert.IsTrue(terrace.Contains(p), FormattableString.Invariant($"deck vertex {p} has no terrace twin (T-junction)"));
            foreach (var p in terrace) Assert.IsTrue(deck.Contains(p), FormattableString.Invariant($"terrace vertex {p} has no deck twin (T-junction)"));
        }

        [Test]
        public void Furniture_AndTrees_OutsideTheLaneEnvelope()
        {
            int n = 0;
            foreach (var mf in _root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (Array.IndexOf(TopMeshes, mf.name) >= 0 || mf.name == "slab") continue;
                foreach (var v in mf.sharedMesh.vertices)
                {
                    if (v.y <= 0.02f || v.y >= 2.2f) continue;
                    double lat = G3BeatWorld.Lateral(v, 980.0, out double s);
                    if (s < NocturneConstants.BeatBoundaries[Beat05_Garden.BeatIndex] - 1.0) continue;
                    n++;
                    Assert.GreaterOrEqual(lat, NocLanes.LaneHalfWidth(s) + 0.35 - 1e-3, FormattableString.Invariant($"{mf.name} vertex {v} at s {s:F1} inside the lane"));
                }
            }
            Assert.Greater(n, 0);
            Assert.AreEqual(GardenDims.BenchCount, 12);
        }

        [Test]
        public void Lights_WetSources_Foliage()
        {
            var lights = _root.GetComponentsInChildren<Light>(true);
            Assert.AreEqual(GardenDims.LeafLights, lights.Length, "8 leaf points (14.2.3 row 5)");
            foreach (var l in lights) Assert.AreEqual(LightShadows.None, l.shadows);
            Assert.AreEqual(SkybridgeDims.LampCount + GardenDims.Trees.Length, NocWetRegistry.TotalSources, "one wet source per crown");
            Assert.LessOrEqual(NocWetRegistry.MaxActive(), NocturneConstants.WetSourceMax);
            var sh = Shader.Find(GardenMaterials.FoliageShader);
            Assert.IsNotNull(sh, "NocFoliage compiles and is found");
            var mr = _root.Find("trees/crowns").GetComponent<MeshRenderer>();
            Assert.AreEqual(sh, mr.sharedMaterial.shader, "crowns use NocFoliage");
            // KS15: the two crowns overhead (T3, T7) hang about 33 m up
            Assert.AreEqual(33.0, GardenDims.Trees[2].CrownBottom, 1.0, "T3 crown bottom");
            Assert.AreEqual(33.0, GardenDims.Trees[6].CrownBottom, 2.5, "T7 crown bottom");
        }

        [Test]
        public void Meshes_Valid_AndBudget()
        {
            int n = 0;
            foreach (var mf in _root.GetComponentsInChildren<MeshFilter>(true))
            {
                var r = MeshValidation.Validate(mf.sharedMesh);
                Assert.IsTrue(r.Ok, r.Describe());
                n++;
            }
            Debug.Log(FormattableString.Invariant($"[G3] beat 5: {n} meshes, {_tris} triangles (budget 750 k)"));
            Assert.LessOrEqual(_tris, 750000, "14.5.5 Acceptance (4)");
            foreach (var mr in _root.GetComponentsInChildren<MeshRenderer>(true))
                Assert.IsNotNull(mr.sharedMaterial, mr.name + " has no material");
        }
    }
}
