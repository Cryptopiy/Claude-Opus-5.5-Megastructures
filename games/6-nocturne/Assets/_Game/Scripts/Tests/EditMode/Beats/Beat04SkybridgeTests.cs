// Beat04SkybridgeTests: EditMode acceptance checks of beat 4, Skybridge (MASTER_PROMPT 14.5.4 Acceptance (1) and (5), 14.10 numeric
// checks of the bridge geometry, the Phase-3 agreements of docs/logs/G3.md), on the real builder output (temporary root objects, the MaterialLibrary
// asset): deck 5.0 +- 0.02 with its top exactly at y 0 and a seam every 4 m, rails 1.10 +- 0.01 with their
// inner faces on the deck edge, tube diameter 9.0 +- 0.02 (face tubes 10.0), span 300 +- 0.05 between the truss bearings, gate
// blocks / openings / frames at their 14.5.4 extents, 21 lamps with 21 lights (r 12, i 4) and 21 wet sources, nothing of the beat
// inside the lane envelope (capsule r 0.35 up to 2.2 m; the camera sphere is inside it), every mesh valid, <= 520 k triangles.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Megastructure.EditorTools;

namespace Megastructure.Tests
{
    /// <summary>
    /// Builds G3's beats under fresh root objects in the test runner's scene (an untitled scene cannot take an additive one in
    /// batch mode; V's VisibilityWorld does the same) and destroys everything it created afterwards (objects, meshes, derived
    /// materials), clearing the static wet registry before and after.
    /// </summary>
    sealed class G3BeatWorld
    {
        public WorldContext Ctx;
        Transform _city, _backdrop;

        public static G3BeatWorld Build(params IBeatBuilder[] builders)
        {
            NocWetRegistry.Clear();
            var w = new G3BeatWorld();
            var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(NocPaths.LibraryAsset);
            w._city = new GameObject("G3TestCity").transform;
            w._backdrop = new GameObject("G3TestBackdrop").transform;
            w.Ctx = new WorldContext { Materials = lib, CityRoot = w._city, BackdropRoot = w._backdrop, Seed = 1 };
            foreach (var b in builders) b.Build(w.Ctx);
            return w;
        }

        public void Close()
        {
            NocWetRegistry.Clear();
            if (_city == null) return;
            var meshes = new HashSet<Mesh>();
            var mats = new HashSet<Material>();
            foreach (var mf in _city.GetComponentsInChildren<MeshFilter>(true)) if (mf.sharedMesh != null) meshes.Add(mf.sharedMesh);
            foreach (var mr in _city.GetComponentsInChildren<MeshRenderer>(true))
                foreach (var m in mr.sharedMaterials) if (m != null && !AssetDatabase.Contains(m)) mats.Add(m);
            UnityEngine.Object.DestroyImmediate(_city.gameObject);
            UnityEngine.Object.DestroyImmediate(_backdrop.gameObject);
            foreach (var m in meshes) UnityEngine.Object.DestroyImmediate(m);
            foreach (var m in mats) UnityEngine.Object.DestroyImmediate(m);
        }

        public static Mesh MeshOf(Transform root, string path)
        {
            var t = root.Find(path);
            Assert.IsNotNull(t, "missing renderer " + path);
            return t.GetComponent<MeshFilter>().sharedMesh;
        }

        /// <summary>Lateral distance of a point from the walk centre line (projection near hintS) and its progress.</summary>
        public static double Lateral(Vector3 p, double hintS, out double s)
        {
            s = NocLanes.ProjectS(p.x, p.z, hintS, out double d);
            return d;
        }
    }

    public class Beat04SkybridgeTests
    {
        G3BeatWorld _world;
        Transform _root;
        long _tris;

        [OneTimeSetUp]
        public void Build()
        {
            _world = G3BeatWorld.Build(new Beat04_Skybridge());
            _root = _world.Ctx.BeatRoot(Beat04_Skybridge.BeatIndex);
            _tris = _world.Ctx.Triangles;
        }

        [OneTimeTearDown]
        public void TearDown() => _world?.Close();

        [Test]
        public void Deck_Width5_TopAtY0_SeamEvery4m()
        {
            var m = G3BeatWorld.MeshOf(_root, "deckTop");
            float zMin = float.MaxValue, zMax = float.MinValue, yMax = float.MinValue, yMin = float.MaxValue;
            foreach (var v in m.vertices)
            {
                yMax = Mathf.Max(yMax, v.y); yMin = Mathf.Min(yMin, v.y);
                if (v.x > SkybridgeDims.GateBX0 - 0.01f) continue;
                zMin = Mathf.Min(zMin, v.z); zMax = Mathf.Max(zMax, v.z);
            }
            Debug.Log(FormattableString.Invariant($"[G3] deck width {zMax - zMin:F4} m (z {zMin:F3}..{zMax:F3}), top y {yMax:F4}, seam bottom {yMin:F4}, seams {SkybridgeDeck.SeamCount()}"));
            Assert.AreEqual(5.0, zMax - zMin, 0.02, "deck 5.0 m between the rails (14.5.4)");
            Assert.AreEqual(0.0, yMax, 1e-6, "deck top is the walk plane y 0 (H4 mirror plane)");
            Assert.AreEqual(-SkybridgeDims.SeamDepth, yMin, 1e-5, "seam depth");
            Assert.AreEqual(79, SkybridgeDeck.SeamCount(), "a seam every 4 m over x 375.3-695");
            var slab = G3BeatWorld.MeshOf(_root, "deckSlab").bounds;
            Assert.AreEqual(-SkybridgeDims.DeckThickness, slab.min.y, 1e-4, "0.5 m steel slab");
        }

        [Test]
        public void GlassStrip_20x2_4_HoleUnderGlass()
        {
            // 14.5.4 Acceptance (1) "strip 20.0 x 2.4"; D-029 GB-05: a hole in the deck (top and soffit) under a NocGlass pane
            float x0 = SkybridgeDims.StripX0, x1 = SkybridgeDims.StripX1, h = SkybridgeDims.StripHalf, zc = SkybridgeDims.AxisZ;
            Assert.AreEqual(20.0, x1 - x0, 1e-4); Assert.AreEqual(2.4, 2f * h, 1e-4);
            Assert.AreEqual(706.0, SkybridgeDims.SOfX(x0), 0.01, "s 706"); Assert.AreEqual(726.0, SkybridgeDims.SOfX(x1), 0.01, "s 726");
            foreach (var name in new[] { "deckTop", "deckSlab" })
            {
                var m = G3BeatWorld.MeshOf(_root, name);
                var v = m.vertices; var t = m.triangles; int inside = 0;
                for (int i = 0; i < t.Length; i += 3)
                {
                    Vector3 c = (v[t[i]] + v[t[i + 1]] + v[t[i + 2]]) / 3f;
                    bool horizontal = Mathf.Abs(Vector3.Cross(v[t[i + 1]] - v[t[i]], v[t[i + 2]] - v[t[i]]).normalized.y) > 0.9f;
                    if (horizontal && c.x > x0 + 1e-3f && c.x < x1 - 1e-3f && Mathf.Abs(c.z - zc) < h - 1e-3f) inside++;
                }
                Assert.AreEqual(0, inside, name + ": no horizontal face over the strip");
            }
            var g = G3BeatWorld.MeshOf(_root, "glassStrip_back").bounds;
            Assert.AreEqual(x0, g.min.x, 1e-3); Assert.AreEqual(x1, g.max.x, 1e-3);
            Assert.AreEqual(zc - h, g.min.z, 1e-3); Assert.AreEqual(zc + h, g.max.z, 1e-3);
            Assert.AreEqual(SkybridgeDims.StripGlassY, g.center.y, 1e-4, "pane 3 cm under the deck top");
            Assert.IsNotNull(_root.Find("glassStrip_back"), "glass back faces (D-008)");
            Assert.IsNotNull(_root.Find("glassStrip_front"), "glass front faces (D-008)");
        }

        [Test]
        public void Rails_Height110_InnerFacesOnTheDeckEdge()
        {
            var top = G3BeatWorld.MeshOf(_root, "rails/topRail");
            Assert.AreEqual(1.10, top.bounds.max.y, 0.01, "rails 1.10 m (14.5.4)");
            float inner = float.MaxValue;
            foreach (var name in new[] { "rails/topRail", "rails/posts", "rails/rods" })
                foreach (var v in G3BeatWorld.MeshOf(_root, name).vertices) inner = Mathf.Min(inner, Mathf.Abs(v.z - SkybridgeDims.AxisZ));
            Assert.AreEqual(2.5, inner, 1e-3, "rail inner faces on the deck edge (5.0 m between rails)");
            Assert.AreEqual(150, SkybridgeRails.PostsPerSide(), "posts every 2 m between the gate faces");
            Debug.Log(FormattableString.Invariant($"[G3] rail top {top.bounds.max.y:F4} m, inner face {inner:F4} m from the axis, {SkybridgeRails.PostsPerSide()} posts per side"));
        }

        [Test]
        public void Tubes_Diameter9_AndFaceTubes10()
        {
            var m = G3BeatWorld.MeshOf(_root, "tubes/bridgeTubes_back");
            float rMin = float.MaxValue, rMax = 0f;
            foreach (var v in m.vertices)
            {
                float zc = v.z < SkybridgeDims.AxisZ ? SkybridgeDims.TubeSZ : SkybridgeDims.TubeNZ;
                float r = new Vector2(v.y - SkybridgeDims.TubeAxisY, v.z - zc).magnitude;
                rMin = Mathf.Min(rMin, r); rMax = Mathf.Max(rMax, r);
            }
            Assert.AreEqual(9.0, 2f * rMin, 0.02, "tube diameter 9.0 (14.5.4)");
            Assert.AreEqual(9.0, 2f * rMax, 0.02);
            Assert.AreEqual(SkybridgeDims.TubeX0, m.bounds.min.x, 1e-3, "tubes start at gate A's mouths");
            Assert.AreEqual(SkybridgeDims.TubeX1, m.bounds.max.x, 1e-3, "tubes end at gate B's mouths");
            var f = G3BeatWorld.MeshOf(_root, "tubes/faceTubes_back");
            Assert.AreEqual(SkybridgeDims.FaceAX, f.bounds.min.x, 1e-3, "upper / lower tubes from face A");
            Assert.AreEqual(SkybridgeDims.FaceBX, f.bounds.max.x, 1e-3, "to face B");
            Assert.AreEqual(SkybridgeDims.UpperY + SkybridgeDims.FaceTubeR, f.bounds.max.y, 1e-3);
            Assert.AreEqual(SkybridgeDims.LowerY - SkybridgeDims.FaceTubeR, f.bounds.min.y, 1e-3);
            // glass on two renderers: back faces at 3000, front faces at 3001 (D-008)
            var back = _root.Find("tubes/bridgeTubes_back").GetComponent<MeshRenderer>().sharedMaterial;
            var front = _root.Find("tubes/bridgeTubes_front").GetComponent<MeshRenderer>().sharedMaterial;
            if (back != null && front != null) Assert.Less(back.renderQueue, front.renderQueue, "back faces before front faces");
            Debug.Log(FormattableString.Invariant($"[G3] tube diameter {2f * rMin:F4}..{2f * rMax:F4} m, x {m.bounds.min.x}..{m.bounds.max.x}"));
        }

        [Test]
        public void Span300_BetweenTheTrussBearings()
        {
            var m = G3BeatWorld.MeshOf(_root, "truss");
            float yBot = SkybridgeDims.TrussTopY - SkybridgeDims.TrussDepth;
            float x0 = float.MaxValue, x1 = float.MinValue;
            foreach (var v in m.vertices)
            {
                if (v.y > yBot - 0.2f) continue;            // the bottom flanges of the bottom chord only
                x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x);
            }
            Debug.Log(FormattableString.Invariant($"[G3] bottom chord x {x0:F3}..{x1:F3}: span {x1 - x0:F4} m; gate faces {SkybridgeDims.GateAX1}..{SkybridgeDims.GateBX0} ({SkybridgeDims.GateBX0 - SkybridgeDims.GateAX1} m)"));
            Assert.AreEqual(300.0, x1 - x0, 0.05, "span 300 (14.5.4; the neck x 375.3-675.3 of 14.4)");
            Assert.AreEqual(300.0, SkybridgeTruss.Span, 0.05);
            Assert.AreEqual(SkybridgeDims.TrussDepth, SkybridgeDims.TrussTopY - (m.bounds.min.y + 0.5f * SkybridgeDims.TrussMember), 0.3f, "10 m deep");
        }

        [Test]
        public void Gates_Blocks_Openings_Frames()
        {
            foreach (bool a in new[] { true, false })
            {
                string g = a ? "gateA" : "gateB";
                var b = G3BeatWorld.MeshOf(_root, g + "/block").bounds;
                Assert.AreEqual(a ? SkybridgeDims.GateAX0 : SkybridgeDims.GateBX0, b.min.x, 1e-3, g + " x0");
                Assert.AreEqual(a ? SkybridgeDims.GateAX1 : SkybridgeDims.GateBX1, b.max.x, 1e-3, g + " x1");
                Assert.AreEqual(175f, b.min.z, 1e-3); Assert.AreEqual(215f, b.max.z, 1e-3);
                Assert.AreEqual(10f, b.max.y, 1e-3, "10 m high");
                var fr = G3BeatWorld.MeshOf(_root, g + "/frame").bounds;
                Assert.AreEqual(SkybridgeDims.FrameTop, fr.max.y, 1e-3, "frame 16 m high");
                // the walkway opening: nothing of the block between z 189 and 201 below y 8 above the floor
                foreach (var v in G3BeatWorld.MeshOf(_root, g + "/block").vertices)
                    if (v.z > SkybridgeDims.OpeningZ0 + 1e-3f && v.z < SkybridgeDims.OpeningZ1 - 1e-3f && v.y > 1e-3f && v.y < SkybridgeDims.OpeningTop - 1e-3f)
                        Assert.Fail(FormattableString.Invariant($"{g} vertex {v} inside the 12 x 8 m walkway opening"));
                foreach (var v in G3BeatWorld.MeshOf(_root, g + "/frame").vertices)
                    if (v.z > SkybridgeDims.OpeningZ0 - 1e-3f && v.z < SkybridgeDims.OpeningZ1 + 1e-3f && v.y < SkybridgeDims.LintelBottom - 1e-3f)
                        Assert.Fail(FormattableString.Invariant($"{g} frame vertex {v} inside the opening"));
            }
        }

        [Test]
        public void Lamps21_LightsAndWetSources()
        {
            var lights = _root.GetComponentsInChildren<Light>(true);
            Assert.AreEqual(SkybridgeDims.LampCount, lights.Length, "21 realtime lamp lights (14.2.3 row 4)");
            foreach (var l in lights)
            {
                Assert.AreEqual(LightShadows.None, l.shadows, "no shadow maps (14.2.3)");
                Assert.AreEqual(SkybridgeDims.LampRange, l.range, 1e-4);
                // LOOK_V2 6 row 4 (PA, SkybridgeLamps): i 3.5; the v1 14.2.3 value 4.0 stays in SkybridgeDims
                Assert.AreEqual(SkybridgeLamps.LampIntensityV2, l.intensity, 1e-4, "lamp intensity (LOOK_V2 6 beat 4)");
            }
            Assert.AreEqual(SkybridgeDims.LampCount, NocWetRegistry.TotalSources, "one wet source per lamp head");
            Assert.LessOrEqual(NocWetRegistry.MaxActive(), NocturneConstants.WetSourceMax);
            Assert.AreEqual(SkybridgeDims.LampHeight, G3BeatWorld.MeshOf(_root, "lamps/poles").bounds.max.y, 0.01, "lamp posts 4.2 m");
            Assert.Greater(SkybridgeDims.LampX(0), SkybridgeDims.GateAX1);
            Assert.Less(SkybridgeDims.LampX(SkybridgeDims.LampCount - 1), SkybridgeDims.GateBX0);
        }

        [Test]
        public void Nothing_InsideTheLaneEnvelope()
        {
            int checkedV = 0;
            foreach (var mf in _root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.name == "deckTop") continue;
                foreach (var v in mf.sharedMesh.vertices)
                {
                    if (v.y <= 0.02f || v.y >= 2.2f) continue;
                    if (v.x < SkybridgeDims.DeckX0 - 5f || v.x > SkybridgeDims.DeckX1 + 5f) continue;
                    double lat = G3BeatWorld.Lateral(v, SkybridgeDims.SOfX(v.x), out double s);
                    double reach = NocLanes.LaneHalfWidth(s) + 0.35;
                    checkedV++;
                    Assert.GreaterOrEqual(lat, reach - 1e-3, FormattableString.Invariant($"{mf.name} vertex {v} at s {s:F1}: lateral {lat:F3} < lane + capsule {reach:F3}"));
                }
            }
            Assert.Greater(checkedV, 0);
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
            Debug.Log(FormattableString.Invariant($"[G3] beat 4: {n} meshes, {_tris} triangles (budget 520 k)"));
            Assert.LessOrEqual(_tris, 520000, "14.5.4 Acceptance (5)");
            foreach (var mr in _root.GetComponentsInChildren<MeshRenderer>(true))
                Assert.IsNotNull(mr.sharedMaterial, mr.name + " has no material");
        }
    }
}
