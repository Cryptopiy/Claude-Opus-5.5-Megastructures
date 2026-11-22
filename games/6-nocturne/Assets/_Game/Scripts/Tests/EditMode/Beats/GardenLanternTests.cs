// GardenLanternTests (GD, Phase 4): EditMode checks of the beat-5 lanterns, lantern posts, fireflies and lantern field
// (MASTER_PROMPT 14.5.5: 520 instanced 0.35 m icosahedra per tree, 8 840 in all, Lantern x 2.0, pulse 0.1 Hz +-6 %, >= 2.2 px with
// the energy kept; 600 fireflies, min 2.2 px; LOOK_V2 6: 10 lantern clusters lerp(Lantern, WinNeutral, 0.4) r 14 i 3.5), on the real
// builder output (beats 4 and 5 together, like Beat05GardenTests).
using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Megastructure.Tests
{
    public class GardenLanternTests
    {
        G3BeatWorld _world;
        Transform _root;

        [OneTimeSetUp]
        public void Build()
        {
            _world = G3BeatWorld.Build(new Beat04_Skybridge());
            new Beat05_Garden().Build(_world.Ctx);
            _root = _world.Ctx.BeatRoot(Beat05_Garden.BeatIndex);
        }

        [OneTimeTearDown]
        public void TearDown() => _world?.Close();

        [Test]
        public void Lanterns_520PerTree_8840InAll_AsBalls()
        {
            int lanterns = 0, meshes = 0;
            foreach (var t in GardenDims.Trees)
            {
                var mf = _root.Find("trees/lanterns_" + t.Name)?.GetComponent<MeshFilter>();
                Assert.IsNotNull(mf, "lantern mesh of " + t.Name);
                var mesh = mf.sharedMesh;
                Assert.AreEqual(0, mesh.vertexCount % GardenLanterns.BallVertices);
                int n = mesh.vertexCount / GardenLanterns.BallVertices;
                Assert.AreEqual(GardenLanterns.PerTree, n, t.Name + ": 520 lanterns");
                Assert.AreEqual(n * GardenLanterns.BallTriangles, mesh.triangles.Length / 3, "20 triangles a ball");
                var uv = mesh.uv;
                var v = mesh.vertices;
                var nr = mesh.normals;
                for (int i = 0; i < mesh.vertexCount; i += 37)
                {
                    Assert.AreEqual(GardenLanterns.Radius, uv[i].x, 1e-6f, "uv0.x = ball radius (0.35 m icosahedra)");
                    Assert.AreEqual(1f, nr[i].magnitude, 1e-3f);
                    // centre = position - normal x radius is the same for every vertex of a ball
                    Vector3 c = v[i] - nr[i] * uv[i].x;
                    int b = i - i % GardenLanterns.BallVertices;
                    Vector3 c0 = v[b] - nr[b] * uv[b].x;
                    Assert.Less((c - c0).magnitude, 1e-3f, "radial normals about one centre");
                    Assert.Greater(c.y, GardenLanterns.LowestY, "no tree lantern low over the walk");
                    Assert.IsTrue(GardenLeafCards.ClearOfNotchWalls(c), "clear of the notch walls");
                }
                Assert.AreEqual(GardenMaterials.ModeLanterns, mf.GetComponent<MeshRenderer>().sharedMaterial.GetFloat("_LeafMode"));
                lanterns += n;
                meshes++;
            }
            Assert.AreEqual(17, meshes);
            Assert.AreEqual(8840, lanterns, "14.5.5 Acceptance (1): 8 840 lanterns");
        }

        [Test]
        public void Lanterns_HangUnderTheCrowns()
        {
            foreach (var t in GardenDims.Trees)
            {
                var mesh = _root.Find("trees/lanterns_" + t.Name).GetComponent<MeshFilter>().sharedMesh;
                var v = mesh.vertices;
                var nr = mesh.normals;
                for (int i = 0; i < v.Length; i += GardenLanterns.BallVertices * 7)
                {
                    Vector3 c = v[i] - nr[i] * GardenLanterns.Radius;
                    Assert.LessOrEqual(c.y, t.CrownCentre.y + 0.1f, t.Name + ": lanterns under the crown's equator");
                    Assert.GreaterOrEqual(c.y, t.CrownCentre.y - 1.12f * t.CrownRadii.y - GardenLanterns.DropMax - 0.5f, t.Name + ": at most 6 m below the leaves");
                    float rn = GardenLeafCards.NormRadius(t, c);
                    Assert.Greater(rn, GardenDims.CoreFrac * GardenDims.LobeFloor * 0.99f, t.Name + ": outside the core");
                }
            }
        }

        [Test]
        public void Field_LightsTheUndersides()
        {
            var core = _root.Find("trees/crowns").GetComponent<MeshFilter>().sharedMesh;
            var col = core.colors32;
            var nr = core.normals;
            double down = 0, up = 0;
            int nd = 0, nu = 0;
            for (int i = 0; i < col.Length; i++)
            {
                if (nr[i].y < -0.7f) { down += col[i].a; nd++; }
                else if (nr[i].y > 0.7f) { up += col[i].a; nu++; }
            }
            Assert.Greater(nd, 0);
            Assert.Greater(nu, 0);
            down /= nd; up /= nu;
            Debug.Log(FormattableString.Invariant($"[GD] lantern field (colour A) on the core: down-facing mean {down:F1}, up-facing mean {up:F1}"));
            // the core is the deep interior (0.86 of the lobed radius); since GD r05 every lantern hangs under the outer leaf shell
            // (1.04-1.08 + one step), so the core gets only the light that passes the leaves: it must still be lit more from below
            Assert.Greater(down, 2.0, "some lantern light reaches the core's underside");
            Assert.Greater(down, 4.0 * up + 1.0, "the undersides, not the tops");
            // the visible undersides are the leaf cards: their down-facing vertices carry the field
            int lit = 0, cards = 0;
            double cdown = 0, cup = 0;
            int ncd = 0, ncu = 0;
            foreach (var mf in _root.Find("trees").GetComponentsInChildren<MeshFilter>(true))
            {
                if (!mf.name.StartsWith("crown_cards_", StringComparison.Ordinal)) continue;
                cards++;
                var cc = mf.sharedMesh.colors32;
                var cn = mf.sharedMesh.normals;
                bool any = false;
                for (int i = 0; i < cc.Length; i++)
                {
                    if (cc[i].a > 8) any = true;
                    if (cn[i].y < -0.7f) { cdown += cc[i].a; ncd++; }
                    else if (cn[i].y > 0.7f) { cup += cc[i].a; ncu++; }
                }
                if (any) lit++;
            }
            cdown /= Math.Max(ncd, 1); cup /= Math.Max(ncu, 1);
            Debug.Log(FormattableString.Invariant($"[GD] lantern field (colour A) on the cards: down-facing mean {cdown:F1}, up-facing mean {cup:F1}"));
            Assert.AreEqual(GardenDims.Trees.Length, cards);
            Assert.AreEqual(cards, lit, "every card mesh receives lantern light");
            Assert.Greater(cdown, 12.0, "lantern light on the leaves under the crowns");
            Assert.Greater(cdown, 4.0 * cup + 1.0, "the undersides, not the tops");
        }

        [Test]
        public void Posts_TenClusterLights_AndFireflies()
        {
            var g = _root.Find("lanterns");
            Assert.IsNotNull(g);
            var lights = g.GetComponentsInChildren<Light>(true);
            Assert.AreEqual(GardenLanterns.PostCount, lights.Length, "LOOK_V2 6: 10 lantern clusters");
            Color want = GardenLanterns.LightColour();
            foreach (var l in lights)
            {
                Assert.AreEqual(LightType.Point, l.type);
                Assert.AreEqual(LightShadows.None, l.shadows, "no shadow maps");
                Assert.AreEqual(14f, l.range, 1e-4f);
                Assert.AreEqual(3.5f, l.intensity, 1e-4f);
                Assert.AreEqual(want.r, l.color.r, 1e-4f);
                Assert.AreEqual(want.g, l.color.g, 1e-4f);
                Assert.AreEqual(want.b, l.color.b, 1e-4f);
                double lat = G3BeatWorld.Lateral(l.transform.position, 980.0, out double s);
                Assert.GreaterOrEqual(lat, NocLanes.LaneHalfWidth(s) + 0.35, "outside the lane");
            }
            var balls = g.Find("post_lanterns").GetComponent<MeshFilter>().sharedMesh;
            Assert.AreEqual(GardenLanterns.PostCount * GardenLanterns.PostLanterns * GardenLanterns.SphereVertices, balls.vertexCount);
            var flies = g.Find("fireflies").GetComponent<MeshFilter>().sharedMesh;
            Assert.AreEqual(GardenLanterns.FireflyCount * GardenLanterns.BallVertices, flies.vertexCount, "600 fireflies");
            Assert.AreEqual(GardenMaterials.ModeFireflies, g.Find("fireflies").GetComponent<MeshRenderer>().sharedMaterial.GetFloat("_LeafMode"));
            // wander 1.5 m + orbit 0.3 m: a firefly placed at >= 5 m never crosses into the lane (half width + 0.38)
            foreach (var v in flies.vertices)
            {
                double lat = G3BeatWorld.Lateral(v, 980.0, out double s);
                Assert.GreaterOrEqual(lat, NocLanes.LaneHalfWidth(s) + 0.38 + 1.8 - 0.02, "fireflies stay out of the lane");
            }
        }

        [Test]
        public void Lanterns_CarryTheirCrown_ForTheFringeFade()
        {
            // GD fix wave C32: uv1 = (crown centre x, z, y, horizontal radius), colour A = vertical / (2 horizontal)
            foreach (var t in GardenDims.Trees)
            {
                var mesh = _root.Find("trees/lanterns_" + t.Name).GetComponent<MeshFilter>().sharedMesh;
                var uv1 = new System.Collections.Generic.List<Vector4>();
                mesh.GetUVs(1, uv1);
                Assert.AreEqual(mesh.vertexCount, uv1.Count, t.Name + ": uv1 on every lantern vertex");
                var col = mesh.colors32;
                Vector3 cc = t.CrownCentre, rr = t.CrownRadii;
                for (int i = 0; i < uv1.Count; i += 101)
                {
                    Assert.AreEqual(cc.x, uv1[i].x, 1e-3f); Assert.AreEqual(cc.z, uv1[i].y, 1e-3f); Assert.AreEqual(cc.y, uv1[i].z, 1e-3f);
                    Assert.AreEqual(rr.x, uv1[i].w, 1e-3f);
                    Assert.AreEqual(rr.y / (2f * rr.x), col[i].a / 255f, 0.006f, "colour A = Ry / (2 Rh)");
                }
            }
            // the crowns as shader globals for the other-crown fringe test
            Assert.AreEqual((float)GardenDims.Trees.Length, Shader.GetGlobalFloat(GardenLanterns.CrownCountId), "17 crowns in _NocGardenCrownCount");
            var g = Shader.GetGlobalVectorArray(GardenLanterns.CrownsId);
            Assert.IsNotNull(g);
            Assert.GreaterOrEqual(g.Length, 2 * GardenDims.Trees.Length);
            for (int i = 0; i < GardenDims.Trees.Length; i++)
            {
                Assert.AreEqual(GardenDims.Trees[i].CrownCentre.y, g[2 * i].y, 1e-3f);
                Assert.AreEqual(GardenDims.Trees[i].CrownRadii.y, g[2 * i + 1].y, 1e-3f);
            }
            // the post lanterns carry no crown (no fade)
            var post = _root.Find("lanterns/post_lanterns").GetComponent<MeshFilter>().sharedMesh;
            var pu = new System.Collections.Generic.List<Vector4>();
            post.GetUVs(1, pu);
            foreach (var u in pu) Assert.LessOrEqual(u.w, 2f, "post lanterns: no crown radius");
        }

        [Test]
        public void Limbs_AreBark_LitByTheField()
        {
            // GD fix wave C33: the limbs are their own mesh, NocFoliage mode 5 with lib.Bark's photo maps and the lantern field in A
            var mr = _root.Find("trees/limbs")?.GetComponent<MeshRenderer>();
            Assert.IsNotNull(mr, "limbs mesh");
            var mat = mr.sharedMaterial;
            Assert.AreEqual(GardenMaterials.ModeBark, mat.GetFloat("_LeafMode"));
            Assert.IsNotNull(mat.GetTexture("_BarkTex"), "CC0 bark albedo");
            Assert.IsNotNull(mat.GetTexture("_BarkNormalTex"), "CC0 bark normal");
            Assert.AreEqual(GardenDims.BarkTile, mat.GetFloat("_BarkTile"), 1e-5f);
            var mesh = mr.GetComponent<MeshFilter>().sharedMesh;
            var col = mesh.colors32;
            var v = mesh.vertices;
            int lit = 0;
            float top = 0f;
            for (int i = 0; i < col.Length; i++) { if (col[i].a > 12) lit++; top = Mathf.Max(top, v[i].y); }
            Assert.Greater(lit, col.Length / 20, "the field reaches the limbs under the crowns");
            Assert.Greater(top, 30f, "limbs rise into the crowns");
            // the trunks keep lib.Bark (NocSurface, wet bark seen up close from the path)
            var trunkMat = _root.Find("trees/trunks").GetComponent<MeshRenderer>().sharedMaterial;
            Assert.AreNotEqual(GardenMaterials.FoliageShader, trunkMat.shader.name, "trunks stay on the library bark");
        }

        [Test]
        public void Shader_LanternContract()
        {
            var sh = Shader.Find(GardenMaterials.FoliageShader);
            Assert.IsNotNull(sh, "NocFoliage compiles and is found");
            string src = System.IO.File.ReadAllText(AssetDatabase.GetAssetPath(sh));
            StringAssert.Contains("_MinPx (\"Lantern / firefly minimum diameter (px)\", Range(0, 6)) = 2.2", src, "min 2.2 px");
            StringAssert.Contains("_LanternGain (\"Lantern emission (x Lantern)\", Float) = 2.0", src, "Lantern x 2.0");
            StringAssert.Contains("_LanternHz (\"Lantern pulse (Hz)\", Range(0, 0.2)) = 0.1", src, "pulse 0.1 Hz (<= 0.2 Hz)");
            StringAssert.Contains("_LanternPulse (\"Lantern pulse depth\", Range(0, 0.2)) = 0.06", src, "+-6 %");
            StringAssert.Contains("_SwayAmp (\"Leaf sway at the crown tips (m)\", Range(0, 1)) = 0.25", src, "sway 0.25 m");
            StringAssert.Contains("_SwayPeriod (\"Leaf sway period (s)\", Range(1, 20)) = 7", src, "sway period 7 s");
            StringAssert.Contains("v.custom.x = fade / (k * k);", src, "energy kept when a ball grows to the minimum size (x the crown fade)");
            // GD fix wave C32: the floor on the inscribed sphere, flat profile on small balls (same disc mean 1.0)
            StringAssert.Contains("float k = max(1.0, 0.5 * _MinPx * fp / (inK * r));", src, "2.2 px floor on the inscribed sphere");
            StringAssert.Contains("const float inK = 0.795;", src, "icosahedron inradius / circumradius");
            StringAssert.Contains("float hot = lerp(1.0, 0.35 + 1.3 * nv * nv, smoothstep(0.5 * _FlatPx, 1.5 * _FlatPx, f.custom.z));", src, "flat when small");
            Assert.AreEqual(1.0, 0.35 + 1.3 * 0.5, 1e-9, "the hot profile's disc mean (mean of (n.v)^2 = 1 - rho^2 over the disc is 0.5) equals the flat 1.0");
            StringAssert.DoesNotContain("NocFoliageV1", src, "v1 crown deleted");
            StringAssert.DoesNotContain("shader_feature", src, "no new keyword");
            StringAssert.DoesNotContain("multi_compile_local", src, "no new keyword");
            Assert.Less(0.618f / 7f, 0.2f, "both sway partials <= 0.2 Hz");
            Assert.LessOrEqual(1f / 7f, 0.2f);
        }
    }
}
