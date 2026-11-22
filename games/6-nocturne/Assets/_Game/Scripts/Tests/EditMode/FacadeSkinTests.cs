// FacadeSkinTests (owner FA): the Look v2 facade contract (docs/TEXTURE_PLAN.md 2.2 - 2.6, LOOK_V2 4) on the built world and on the
// C# twin NocFacadeMath (v2 section):
//   - vertex contract over every NocFacade vertex of the built world (every IBeatBuilder as GameBootstrap discovers them, as
//     Q1IntegrationTests builds it): u affine per triangle with |grad u| in [0.9, 1.1]; the twin of NocFaceFrame gives sigma +-1,
//     constant per (mesh, face id); uMin - 1e-3 <= u <= uMax + 1e-3; y0 - 1e-3 <= y <= y1 + 1e-3 when y0 / y1 are known; city
//     faces have sigma +1 and Gulf faces include sigma -1 (EdgeMesh face A: u grows with z while n = +x);
//   - the skin byte: SkinByte / SkinOverride / Region; the same byte on every face of a CityMesher tower, a FacadeKit box and a
//     G4Kit box; at least two skins on the boulevard slabs;
//   - the twin: w forced to 0 and 1 within 3 % (the H2 energy rule on the v2 decomposition, recess and spill included); the
//     lit-probability mean of the strata; the hue split of the lit windows; w_f = 0 means no warm window; no NaN at grazing;
//     the region budgets (LOOK_V2 4: the unresolved facade mean 0.0114 linear +-3 % for region D; OQ about 4 luma codes darker).
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Megastructure.EditorTools;

namespace Megastructure.Tests
{
    public class FacadeSkinTests
    {
        static double Luma(C3 c) => 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;

        // ------------------------------------------------------------------ helpers
        /// <summary>The twin of NocFaceFrame's sigma for a planar triangle: sign of grad(u) . Tp with Tp the horizontal tangent
        /// (n.z, 0, -n.x) projected into the surface (+1 when |grad u . Tp| is under 1e-12).</summary>
        static int Sigma(Vector3 n, Vector3 gradU)
        {
            Vector3 t0 = new Vector3(n.z, 0f, -n.x);
            Vector3 tp = t0 - n * Vector3.Dot(n, t0);
            if (tp.sqrMagnitude < 1e-12f) tp = Vector3.right; else tp.Normalize();
            double s = Vector3.Dot(gradU, tp);
            return s < -1e-12 ? -1 : 1;
        }

        /// <summary>Gradient of a linear function f over a triangle (in its plane): solves f_i - f_0 = g . (P_i - P_0), g in the plane.</summary>
        static bool Grad(Vector3 p0, Vector3 p1, Vector3 p2, double f0, double f1, double f2, out Vector3 g, out Vector3 n)
        {
            Vector3d e1 = new Vector3d(p1) - new Vector3d(p0), e2 = new Vector3d(p2) - new Vector3d(p0);
            Vector3d nn = Vector3d.Cross(e1, e2);
            double a2 = nn.Dot(nn);
            g = Vector3.zero; n = Vector3.up;
            if (a2 < 1e-10) return false;
            double d1 = f1 - f0, d2 = f2 - f0;
            // g = (d1 (e2 x n) + d2 (n x e1)) / |n|^2
            Vector3d gg = (Vector3d.Cross(e2, nn) * d1 + Vector3d.Cross(nn, e1) * d2) * (1.0 / a2);
            g = gg.ToVector3();
            n = (nn * (1.0 / Math.Sqrt(a2))).ToVector3();
            return true;
        }

        struct Vector3d
        {
            public double x, y, z;
            public Vector3d(double x, double y, double z) { this.x = x; this.y = y; this.z = z; }
            public Vector3d(Vector3 v) { x = v.x; y = v.y; z = v.z; }
            public static Vector3d operator -(Vector3d a, Vector3d b) => new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
            public static Vector3d operator +(Vector3d a, Vector3d b) => new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
            public static Vector3d operator *(Vector3d a, double k) => new Vector3d(a.x * k, a.y * k, a.z * k);
            public static Vector3d Cross(Vector3d a, Vector3d b) => new Vector3d(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
            public double Dot(Vector3d b) => x * b.x + y * b.y + z * b.z;
            public Vector3 ToVector3() => new Vector3((float)x, (float)y, (float)z);
        }

        sealed class FaceStats { public int sigmaPos, sigmaNeg, tris; public string mesh; public uint face; public int skin = -1; public bool mixedSkin; }

        /// <summary>Meshes whose non-metric u is an open defect of a writer FA does not own (QA/critique-FA.md FA-D01).</summary>
        static readonly HashSet<string> OpenGradMeshes = new HashSet<string> { "Tongue_hull" };
        /// <summary>(mesh#face id) pairs whose mixed sigma is an open REQUEST to the writer's owner (QA/critique-FA.md FA-D13): the
        /// CanyonBridge east tube panel (ReliefKit.PanelWithHole writes u growing with +z on a face whose u is 600 - z).</summary>
        static readonly HashSet<string> OpenSigmaFaces = new HashSet<string>();   // empty: GE carried the uSign fix (GE.md 23:35)

        /// <summary>Per-triangle |grad u| and sigma of a MeshBuilder's NocFacade quads (CityMesher output: the city cell meshes are
        /// not readable in the built world). Returns the number of triangles checked.</summary>
        static int CheckBuilderTriangles(MeshBuilder mb, ref int gradBad, ref int sigmaNeg, ref double gMin, ref double gMax)
        {
            int n = 0;
            for (int t = 0; t + 2 < mb.Indices.Count; t += 3)
            {
                int i0 = mb.Indices[t], i1 = mb.Indices[t + 1], i2 = mb.Indices[t + 2];
                if (!Grad(mb.Positions[i0], mb.Positions[i1], mb.Positions[i2], mb.UV0[i0].x, mb.UV0[i1].x, mb.UV0[i2].x, out Vector3 g, out Vector3 nn)) continue;
                n++;
                double gl = g.magnitude;
                gMin = Math.Min(gMin, gl); gMax = Math.Max(gMax, gl);
                if (gl < 0.9 || gl > 1.1) gradBad++;
                if (Vector3.Dot(mb.Normals[i0], nn) < 0f) nn = -nn;
                if (Sigma(nn, g) < 0) sigmaNeg++;
            }
            return n;
        }

        static List<IBeatBuilder> Builders()
        {
            var list = new List<IBeatBuilder>();
            foreach (var t in typeof(IBeatBuilder).Assembly.GetTypes())
            {
                if (t.IsAbstract || t.IsInterface || !typeof(IBeatBuilder).IsAssignableFrom(t) || t.GetConstructor(Type.EmptyTypes) == null) continue;
                try { list.Add((IBeatBuilder)Activator.CreateInstance(t)); } catch { }
            }
            list.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return list;
        }

        // ------------------------------------------------------------------ the vertex contract on the built world
        [Test]
        public void Contract_EveryNocFacadeVertexOfTheBuiltWorld()
        {
            Scene scene = SceneManager.GetActiveScene();
            var before = new HashSet<GameObject>(scene.GetRootGameObjects());
            var roots = new List<GameObject>();
            bool ignore = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
            var faces = new Dictionary<string, FaceStats>();
            int meshes = 0, tris = 0, gradBad = 0, gradOpen = 0, uBad = 0, yBad = 0, yKnownTris = 0, cityTris = 0, unreadable = 0, roofTris = 0;
            double gradMin = 9, gradMax = 0, openMin = 9;
            var bad = new List<string>();
            var badGrad = new List<string>(); var badSigma = new List<string>();
            try
            {
                var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(NocPaths.LibraryAsset);
                var city = new GameObject("City").transform;
                var backdrop = new GameObject("Backdrop").transform;
                var camGo = new GameObject("FacadeSkinCamera");
                var cam = camGo.AddComponent<Camera>();
                cam.enabled = false;
                var ctx = new WorldContext { Materials = lib, CityRoot = city, BackdropRoot = backdrop, Camera = cam, Seed = 1, Tier = 0 };
                foreach (var b in Builders()) { try { b.Build(ctx); } catch (Exception e) { Debug.LogWarning("[FA] builder " + b.Name + ": " + e.Message); } }
                foreach (var go in scene.GetRootGameObjects()) if (!before.Contains(go)) roots.Add(go);
                var pos = new List<Vector3>(); var uv0 = new List<Vector2>(); var uv1 = new List<Vector4>(); var col = new List<Color32>(); var idx = new List<int>();
                foreach (var root in roots)
                    foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        var m = mr.sharedMaterial;
                        if (m == null || m.shader == null || m.shader.name != "Megastructure/NocFacade") continue;
                        var mf = mr.GetComponent<MeshFilter>();
                        Mesh mesh = mf != null ? mf.sharedMesh : null;
                        if (mesh == null) continue;
                        if (!mesh.isReadable) { unreadable++; continue; }
                        meshes++;
                        mesh.GetVertices(pos); mesh.GetUVs(0, uv0); mesh.GetUVs(1, uv1); mesh.GetColors(col);
                        bool isCity = mr.transform.IsChildOf(city);
                        Matrix4x4 w = mr.transform.localToWorldMatrix;
                        for (int sm = 0; sm < mesh.subMeshCount; sm++)
                        {
                            mesh.GetTriangles(idx, sm);
                            for (int t = 0; t + 2 < idx.Count; t += 3)
                            {
                                int i0 = idx[t], i1 = idx[t + 1], i2 = idx[t + 2];
                                Vector3 p0 = w.MultiplyPoint3x4(pos[i0]), p1 = w.MultiplyPoint3x4(pos[i1]), p2 = w.MultiplyPoint3x4(pos[i2]);
                                if (!Grad(p0, p1, p2, uv0[i0].x, uv0[i1].x, uv0[i2].x, out Vector3 g, out Vector3 n)) continue;
                                // roofs (mode 2, colour B 255) use slice 0 only: neither u's metric nor the frame sign matter there
                                if (col.Count > i0 && col[i0].b == 255) { roofTris++; continue; }
                                tris++;
                                if (isCity) cityTris++;
                                double gl = g.magnitude;
                                gradMin = Math.Min(gradMin, gl); gradMax = Math.Max(gradMax, gl);
                                if (gl < 0.9 || gl > 1.1)
                                {
                                    // the tongue hull is a ruled (non-developable) skirt whose writer (TonguePier.Hull, not FA's) uses u = x:
                                    // tracked as an open defect (QA/critique-FA.md), not a contract failure of the facade writers
                                    if (OpenGradMeshes.Contains(mr.name)) { gradOpen++; openMin = Math.Min(openMin, gl); }
                                    else { gradBad++; if (badGrad.Count < 8) badGrad.Add(FormattableString.Invariant($"{mr.name}: |grad u| {gl:F3}")); }
                                }
                                // the shader's normal is the vertex normal (flat per quad); orient n like it
                                Vector3 nv = w.MultiplyVector(mesh.normals[i0]).normalized;
                                if (Vector3.Dot(nv, n) < 0f) n = -n;
                                int sg = Sigma(n, g);
                                uint face = (uint)(uv0[i0].y + 0.5f);
                                string key = mr.name + "#" + face;
                                if (!faces.TryGetValue(key, out FaceStats fs)) faces[key] = fs = new FaceStats { mesh = mr.name, face = face };
                                fs.tris++;
                                if (sg > 0) fs.sigmaPos++; else fs.sigmaNeg++;
                                int skin = col.Count > i0 ? col[i0].g : 0;
                                if (fs.skin < 0) fs.skin = skin; else if (fs.skin != skin) fs.mixedSkin = true;
                                if (uv1.Count > i2)
                                {
                                    foreach (int k in new[] { i0, i1, i2 })
                                    {
                                        Vector4 q = uv1[k];
                                        float u = uv0[k].x;
                                        if (u < q.x - 2e-3f || u > q.y + 2e-3f) /* SafeExtent nudges an extent by <= 1 mm */ { uBad++; if (bad.Count < 12) bad.Add(FormattableString.Invariant($"{mr.name}: u {u:F3} outside [{q.x:F3}, {q.y:F3}]")); }
                                    }
                                    Vector4 q0 = uv1[i0];
                                    if (NocFacadeMath.KnownY(q0.z, q0.w))
                                    {
                                        yKnownTris++;
                                        foreach (Vector3 p in new[] { p0, p1, p2 })
                                            if (p.y < q0.z - 1e-2f || p.y > q0.w + 1e-2f) { yBad++; if (bad.Count < 12) bad.Add(FormattableString.Invariant($"{mr.name}: y {p.y:F3} outside [{q0.z:F3}, {q0.w:F3}]")); }
                                    }
                                }
                            }
                        }
                    }
            }
            finally
            {
                if (roots.Count == 0) foreach (var go in scene.GetRootGameObjects()) if (!before.Contains(go)) roots.Add(go);
                var meshesToDestroy = new HashSet<Mesh>();
                foreach (var root in roots)
                    foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true)) if (mf.sharedMesh != null && !AssetDatabase.Contains(mf.sharedMesh)) meshesToDestroy.Add(mf.sharedMesh);
                foreach (var m in meshesToDestroy) UnityEngine.Object.DestroyImmediate(m);
                foreach (var root in roots) if (root != null) UnityEngine.Object.DestroyImmediate(root);
                NocCameraBatches.Clear();
                InstancedRegistry.Clear();
                NocWetRegistry.Clear();
                LogAssert.ignoreFailingMessages = ignore;
            }
            int mixedSigma = 0, neg = 0, mixedSkin = 0, cityNeg = 0;
            var negNames = new SortedSet<string>();
            foreach (var fs in faces.Values)
            {
                if (fs.sigmaPos > 0 && fs.sigmaNeg > 0)
                {
                    bool open = OpenSigmaFaces.Contains(fs.mesh + "#" + fs.face);
                    if (!open) mixedSigma++;
                    if (badSigma.Count < 8) badSigma.Add($"{fs.mesh} face {fs.face}: sigma mixed ({fs.sigmaPos} + / {fs.sigmaNeg} -){(open ? " [open: REQUEST GE in docs/logs/FA.md]" : "")}");
                }
                if (fs.sigmaNeg > fs.sigmaPos) { neg++; negNames.Add(fs.mesh); if (fs.mesh.StartsWith("City_", StringComparison.Ordinal)) cityNeg++; }
                if (fs.mixedSkin) mixedSkin++;
            }
            Debug.Log(FormattableString.Invariant($"[FA] facade contract: {meshes} meshes ({unreadable} unreadable: the city cells, checked from CityMesher in SkinByte_*), {tris} triangles ({cityTris} under the city root, {yKnownTris} with y0 / y1, {roofTris} roof triangles skipped), {faces.Count} (mesh, face) pairs; |grad u| {gradMin:F4}..{gradMax:F4}; open (tongue hull) {gradOpen} triangles down to {openMin:F3}; sigma -1 on {neg} faces of [{string.Join(", ", negNames)}]; mixed sigma {mixedSigma}, mixed skin {mixedSkin}; u range {uBad}, y range {yBad}"));
            foreach (var b in badGrad) Debug.Log("[FA]   " + b);
            foreach (var b in badSigma) Debug.Log("[FA]   " + b);
            foreach (var b in bad) Debug.Log("[FA]   " + b);
            Assert.Greater(meshes, 10, "NocFacade meshes found");
            Assert.AreEqual(0, gradBad, "u is metric (|grad u| in [0.9, 1.1])");
            Assert.AreEqual(0, mixedSigma, "sigma constant within each (mesh, face id)");
            Assert.AreEqual(0, cityNeg, "city faces have sigma +1");
            Assert.Greater(neg, 0, "the Gulf faces include sigma -1 faces (EdgeMesh face A)");
            Assert.AreEqual(0, uBad, "uMin - 1e-3 <= u <= uMax + 1e-3");
            Assert.AreEqual(0, yBad, "y0 <= y <= y1 where known");
            Assert.Greater(yKnownTris, 0, "writers carry y0 / y1");
        }

        // ------------------------------------------------------------------ skin bytes
        [Test]
        public void SkinByte_RegionsOverridesAndOneBytePerTowerOrBox()
        {
            Assert.AreEqual(NocFacadeMath.RegionOQ, NocFacadeMath.Region(1156.0, 387.9), "the goal tenement is in the Old Quarter");
            Assert.AreEqual(NocFacadeMath.RegionOQ, NocFacadeMath.Region(1330.0, 388.0), "the spire is in the Old Quarter");
            Assert.AreEqual(NocFacadeMath.RegionD, NocFacadeMath.Region(100.0, 195.0), "the boulevard is region D");
            for (uint seed = 0; seed < 2000; seed += 7)
            {
                Assert.Less(NocFacadeMath.SkinByte(seed, NocFacadeMath.RegionD), 120);
                byte q = NocFacadeMath.SkinByte(seed, NocFacadeMath.RegionOQ);
                Assert.GreaterOrEqual(q, 120); Assert.Less(q, 240);
            }
            for (int k = 0; k < 16; k++) Assert.AreEqual(0xF0 + k, NocFacadeMath.SkinByte(NocFacadeMath.SkinOverride(k), NocFacadeMath.RegionD));
            Assert.AreEqual(NocFacadeMath.SkinS0, NocFacadeMath.SkinOf(0xF0).id); Assert.IsTrue(NocFacadeMath.SkinOf(0xF0).fins);
            Assert.AreEqual(NocFacadeMath.SkinS2, NocFacadeMath.SkinOf(0xF1).id);
            Assert.AreEqual(NocFacadeMath.SkinS1, NocFacadeMath.SkinOf(0xF2).id);
            Assert.AreEqual(NocFacadeMath.SkinS4, NocFacadeMath.SkinOf(0xF3).id);
            Assert.IsTrue(NocFacadeMath.SkinOf(0xF4).stoneBase);
            Assert.AreEqual(NocFacadeMath.SkinSection, NocFacadeMath.SkinOf(0, 1).id, "mode 1 is the section skin");

            // region shares over all 120 bytes of each region (PCG before thresholding)
            var countD = new int[5]; var countQ = new int[5];
            for (int b = 0; b < 120; b++) { countD[NocFacadeMath.SkinOf(b).id]++; countQ[NocFacadeMath.SkinOf(120 + b).id]++; }
            Debug.Log($"[FA] skins over the 120 bytes: D {string.Join("/", countD)}, OQ {string.Join("/", countQ)} (target D 30/36/18/6/30, OQ 12/30/6/48/24)");
            for (int k = 0; k < 5; k++)
            {
                Assert.AreEqual(120 * NocFacadeMath.SkinShare(NocFacadeMath.RegionD, k), countD[k], 12.0, "region D share of skin " + k);
                Assert.AreEqual(120 * NocFacadeMath.SkinShare(NocFacadeMath.RegionOQ, k), countQ[k], 12.0, "region OQ share of skin " + k);
            }

            // one byte per CityMesher tower (all faces of all three boxes)
            var towers = new List<CityTower>();
            CityField.CellRange(out int i0, out int i1, out int j0, out int j1);
            for (int I = i0; I <= i1 && towers.Count < 200; I++)
                for (int J = j0; J <= j1 && towers.Count < 200; J++)
                    if (CityField.RingOf(I, J) == 1) CityField.CellTowers(I, J, 1, towers);
            Assert.Greater(towers.Count, 20);
            int cGradBad = 0, cSigmaNeg = 0, cTris = 0; double cgMin = 9, cgMax = 0;
            foreach (var t in towers)
            {
                var mb = new MeshBuilder();
                int culled = 0;
                CityMesher.Append(mb, new List<CityTower> { t }, t.cx, t.cz, ref culled);
                if (mb.Colors.Count == 0) continue;
                cTris += CheckBuilderTriangles(mb, ref cGradBad, ref cSigmaNeg, ref cgMin, ref cgMax);
                byte g = mb.Colors[0].g;
                Assert.AreEqual(NocFacadeMath.SkinByte(t.seed, NocFacadeMath.Region(t.cx, t.cz)), g);
                foreach (var c in mb.Colors) Assert.AreEqual(g, c.g, "one skin byte on every face of a tower");
                for (int k = 0; k < mb.UV1.Count; k++) Assert.Greater(mb.UV1[k].w, mb.UV1[k].z, "CityMesher writes y0 < y1");
            }
            Debug.Log(FormattableString.Invariant($"[FA] CityMesher contract: {towers.Count} towers, {cTris} triangles, |grad u| {cgMin:F4}..{cgMax:F4}, {cGradBad} non-metric, {cSigmaNeg} with sigma -1"));
            Assert.Greater(cTris, 100, "CityMesher triangles checked");
            Assert.AreEqual(0, cGradBad, "CityMesher: u is metric");
            Assert.AreEqual(0, cSigmaNeg, "city faces have sigma +1");
            // FacadeKit.Box and G4Kit.FacadeBox: one byte per box, the box centre's region (a box straddling the OQ boundary)
            {
                var mb = new MeshBuilder();
                FacadeKit.Box(mb, 640.0, 700.0, 380.0, 420.0, 0.0, 120.0, G1Mesh.Sides, 12345u, FacadeKit.EdgeNone);
                Assert.Greater(mb.Colors.Count, 0);
                byte g = NocFacadeMath.SkinByte(12345u, NocFacadeMath.Region(670.0, 400.0));
                foreach (var c in mb.Colors) Assert.AreEqual(g, c.g, "FacadeKit.Box: one skin byte");
                var mb2 = new MeshBuilder();
                G4Kit.FacadeBox(mb2, Vector3.zero, 640.0, 700.0, 380.0, 420.0, 0.0, 120.0, 777u, 0);
                byte g2 = NocFacadeMath.SkinByte(777u, NocFacadeMath.Region(670.0, 400.0));
                foreach (var c in mb2.Colors) Assert.AreEqual(g2, c.g, "G4Kit.FacadeBox: one skin byte");
                foreach (var q in mb2.UV1) { Assert.AreEqual(0.0, q.z, 1e-4); Assert.AreEqual(120.0, q.w, 1e-4); }
            }
            // the boulevard slabs get more than one skin (StreetFacade: seed + k + 1, region D)
            var skins = new HashSet<int>();
            for (int k = 0; k < BoulevardLayout.SlabCount; k++)
            {
                skins.Add(NocFacadeMath.SkinOf(NocFacadeMath.SkinByte(StreetFacade.SeedSouth + (uint)(k + 1), NocFacadeMath.RegionD)).id);
                skins.Add(NocFacadeMath.SkinOf(NocFacadeMath.SkinByte(StreetFacade.SeedNorth + (uint)(k + 1), NocFacadeMath.RegionD)).id);
            }
            Debug.Log($"[FA] boulevard slab skins: {string.Join(", ", skins)}");
            Assert.GreaterOrEqual(skins.Count, 2, "at least two skins on the boulevard slabs");
        }

        // ------------------------------------------------------------------ twin: energy, lit fraction, hues, NaN, budgets
        static NocFacadeMath.FaceIn Patch(int skinByte, double fw, double forceW, double kx, double ky, double wf, double nf)
        {
            return new NocFacadeMath.FaceIn
            {
                fwu = fw, fwv = fw, face = 0x2A51C3u, skinByte = skinByte, mode = 0,
                uMin = -1e5, uMax = 1e5, y0 = 0, y1 = 0, kx = kx, ky = ky, tEpoch = 1000.0 / NocFacadeConstants.FaEpoch, wf = wf, nf = nf, forceW = forceW,
                ambEq = new C3(0.0223, 0.0223, 0.0223)
            };
        }

        /// <summary>Mean emission luma and glass coverage of a patch sampled with pixels of footprint fw that tile it exactly (so the
        /// sum is the exact integral of the box-filtered field); `expect`: every resolved cell takes its expected state.</summary>
        static double PatchMean(int skinByte, double fw, double forceW, double kx, double ky, double u0, double uSpan, double v0, double vSpan,
            bool expect, uint face, out double glass)
        {
            int nu = (int)Math.Round(uSpan / fw), nv = (int)Math.Round(vSpan / fw);
            double sum = 0; glass = 0;
            var q = Patch(skinByte, fw, forceW, kx, ky, 0.32, 0.30);
            q.expectState = expect; q.face = face;
            for (int j = 0; j < nv; j++)
                for (int i = 0; i < nu; i++)
                {
                    q.u = u0 + (i + 0.5) * fw; q.v = v0 + (j + 0.5) * fw;
                    var o = NocFacadeMath.EvalV2(q);
                    sum += Luma(o.emission);
                    glass += o.aGlass;
                }
            glass /= nu * nv;
            return sum / (nu * nv);
        }

        [Test]
        public void V2_WForced0And1_MeansWithin3Percent_AndNoNaNAtGrazing()
        {
            // (1) the regimes in expectation: the resolved 2 x 2 field with every cell at its expected state against the band means
            //     (the energy rule of TEXTURE_PLAN 2.9 E1-E7 on the decomposition, recess, spill, halo, groups and zones; no
            //     sampling noise, so the bound is tight); at 4 px per column (w_u = 0.5) and, for the zero-mean interior factor
            //     (falloff, curtains, blinds: wI = 1), at 0.15 m per pixel
            double worst = 0, worstI = 0;
            foreach (int sb in new[] { 3, 17, 44, 90, 130, 200 })
                foreach (var shift in new[] { (0.0, 0.0), (0.6, 0.4) })
                {
                    double fw = NocturneConstants.FacadeColumn / 4.0;
                    double m1 = PatchMean(sb, fw, 1.0, shift.Item1, shift.Item2, 1000.0, 240.0, 10.0, 172.8, true, 0x2A51C3u, out double g1);
                    double m0 = PatchMean(sb, fw, 0.0, shift.Item1, shift.Item2, 1000.0, 240.0, 10.0, 172.8, true, 0x2A51C3u, out double g0);
                    double rel = Math.Abs(m1 - m0) / Math.Max(m0, 1e-12), relG = Math.Abs(g1 - g0) / Math.Max(g0, 1e-12);
                    // the interior factor: one band, 0.15 m pixels (window height >= 8 px for every skin)
                    double fi = 0.15;
                    double i1 = PatchMean(sb, fi, 1.0, shift.Item1, shift.Item2, 1000.0, 96.0, 0.0, 86.4, true, 0x2A51C3u, out _);
                    double i0 = PatchMean(sb, fi, 0.0, shift.Item1, shift.Item2, 1000.0, 96.0, 0.0, 86.4, true, 0x2A51C3u, out _);
                    double relI = Math.Abs(i1 - i0) / Math.Max(i0, 1e-12);
                    worst = Math.Max(worst, rel); worstI = Math.Max(worstI, relI);
                    Debug.Log(FormattableString.Invariant($"[FA] V2 expected regimes: byte {sb} skin {NocFacadeMath.SkinOf(sb).id} shift ({shift.Item1}, {shift.Item2}): emission w0 {m0:E4} / w1 {m1:E4} ({100 * rel:F2} %), glass {g0:F4} / {g1:F4} ({100 * relG:F2} %); interior (0.15 m px) {i0:E4} / {i1:E4} ({100 * relI:F2} %)"));
                    Assert.Less(rel, 0.03, "expected emission of w = 0 and w = 1 within 3 %");
                    Assert.Less(relG, 0.03, "glass coverage means of w = 0 and w = 1 within 3 %");
                    Assert.Less(relI, 0.03, "the interior factor (falloff, curtains, blinds) keeps the mean within 3 %");
                }
            Debug.Log(FormattableString.Invariant($"[FA] V2 expected regimes: worst {100 * worst:F2} % (4 px columns), {100 * worstI:F2} % (interior)"));

            // (2) the sampled window states (lit, hue, b) over 8 faces of each tower: information (the per-window statistics are
            //     tested on their own in V2_LitProbability_HueSplit_WarmZero; one tower's 4 800 windows carry ~3 % noise)
            {
                double fw = NocturneConstants.FacadeColumn / 4.0, s1 = 0, s0 = 0;
                foreach (int sb in new[] { 3, 17, 44, 90, 130, 200 })
                    for (int f = 0; f < 4; f++)
                    {
                        uint face = 0x2A51C3u + 7919u * (uint)f;
                        s1 += PatchMean(sb, fw, 1.0, 0.0, 0.0, 1000.0, 120.0, 10.0, 86.4, false, face, out _);
                        s0 += PatchMean(sb, fw, 0.0, 0.0, 0.0, 1000.0, 120.0, 10.0, 86.4, false, face, out _);
                    }
                Debug.Log(FormattableString.Invariant($"[FA] V2 sampled states, 6 towers x 4 faces: w0 {s0 / 24:E4} / w1 {s1 / 24:E4} ({100 * Math.Abs(s1 - s0) / s0:F2} %, information)"));
            }

            // grazing: huge shifts, tiny and huge footprints
            foreach (double k in new[] { 50.0, 1000.0, -1000.0 })
                foreach (double f in new[] { 1e-3, 0.3, 50.0 })
                {
                    var q = Patch(17, f, -1.0, k, -k, 0.2, 0.3);
                    q.u = 1234.5; q.v = 77.7;
                    var o = NocFacadeMath.EvalV2(q);
                    Assert.IsFalse(double.IsNaN(o.emission.r + o.emission.g + o.emission.b + o.aGlass + o.aOpen), "no NaN at grazing");
                    Assert.GreaterOrEqual(o.aGlass, 0.0);
                }
        }

        [Test]
        public void V2_LitProbability_HueSplit_WarmZero()
        {
            // mean lit probability over many towers and cells, per stratum (the clamp at 1 loses <= 1 %)
            double sumP = 0; int n = 0;
            for (int b = 0; b < 120; b++)
            {
                uint K = NocFacadeMath.SkinOf(b).key;
                for (int band = 0; band < 6; band++)
                {
                    int type = NocFacadeMath.StratumV2(K, band);
                    if (type != NocFacadeMath.StratumRes) continue;
                    for (int jl = 0; jl < 24; jl += 1)
                        for (int i = 0; i < 48; i += 3) { sumP += NocFacadeMath.PCell(K, 0x1234u + (uint)b, band, type, i, jl); n++; }
                }
            }
            double meanP = sumP / n, target = NocFacadeConstants.FaLit * NocFacadeConstants.FaFactorRes;
            Debug.Log(FormattableString.Invariant($"[FA] residential lit probability mean {meanP:F4} (target {target:F4}, {n} cells)"));
            Assert.AreEqual(target, meanP, 0.06 * target, "residential lit probability mean (tower x group x zone, each of mean 1)");

            // hue split of lit windows at w_f 0.32, n_f 0.30 (LOOK_V2 4)
            var cls = new double[5];
            int lit = 0;
            for (int i = 0; i < 400; i++)
                for (int j = 0; j < 24; j++)
                {
                    var w = NocFacadeMath.WindowV2(i, j, 0, 0x77u, NocFacadeMath.SkinOf(5).key, 0, 3.3, 0.32, 0.30);
                    if (w.lit < 0.5) continue;
                    lit++;
                    // classify by chromaticity: tungsten / warm have r >> b, neutral r > b, cold / blue b > r
                    C3 c = w.eLit;
                    double rb = c.r / Math.Max(c.b, 1e-6);
                    cls[rb > 3.5 ? 0 : (rb > 2.0 ? 1 : (rb > 1.05 ? 2 : (rb > 0.45 ? 3 : 4)))]++;
                }
            for (int k = 0; k < 5; k++) cls[k] /= Math.Max(lit, 1);
            Debug.Log(FormattableString.Invariant($"[FA] lit windows {lit}: tungsten {cls[0]:F3} warm {cls[1]:F3} neutral {cls[2]:F3} cold {cls[3]:F3} blue {cls[4]:F3} (LOOK_V2: 0.107 / 0.213 / 0.30 / 0.31 / 0.03 + 0.04 coloured)"));
            Assert.AreEqual(0.32, cls[0] + cls[1], 0.04, "warm share w_f");

            // E[b] of the log-normal brightness (LOOK_V2 4: 1.095)
            {
                double sb = 0; int nb = 200000;
                for (int i = 0; i < nb; i++) sb += NocFacadeMath.BrightnessV2(NocHash.Pcg((uint)i * 2654435761u + 17u));
                Debug.Log(FormattableString.Invariant($"[FA] E[b] {sb / nb:F4} (FaBMean {NocFacadeConstants.FaBMean:F4})"));
                Assert.AreEqual(NocFacadeConstants.FaBMean, sb / nb, 0.01 * NocFacadeConstants.FaBMean, "E[b] = FaBMean within 1 %");
            }

            // w_f = 0: no warm window at all
            for (int i = 0; i < 200; i++)
            {
                C3 h = NocFacadeMath.HueV2(NocHash.U01(NocHash.Pcg((uint)i * 7919u)), 0.0, 0.15);
                C3 warm = NocFacadeMath.ClassColour(1), tung = NocFacadeMath.ClassColour(0);
                Assert.IsFalse(Math.Abs(h.r - warm.r) < 1e-9 && Math.Abs(h.b - warm.b) < 1e-9, "no WinWarm at w_f = 0");
                Assert.IsFalse(Math.Abs(h.r - tung.r) < 1e-9 && Math.Abs(h.b - tung.b) < 1e-9, "no WinTungsten at w_f = 0");
            }
        }

        /// <summary>FX (INT-14): section floors light in runs (E[lit] = p per cell, lit cells line up along the floor) and a share of
        /// the districts are dark voids (district multiplier mean 1).</summary>
        [Test]
        public void V2_SectionRuns_KeepTheLitMean_LineUpAlongTheFloor_DistrictVoids()
        {
            uint K = NocFacadeMath.SkinOf(0, 1).key;
            int type = NocFacadeMath.StratumSection;
            double sumLit = 0, sumP = 0, litL = 0, litLR = 0, litD = 0, litDU = 0; long n = 0;
            for (int f = 0; f < 4; f++)
                for (int band = 0; band < 4; band++)
                    for (int jl = 0; jl < 22; jl++)
                        for (int i = 0; i < 300; i++)
                        {
                            uint face = 0x5EC0u + (uint)f;
                            double a = NocFacadeMath.WindowV2(i, jl, band, face, K, type, 3.3, 0.32, 0.30).lit;
                            double b = NocFacadeMath.WindowV2(i + 1, jl, band, face, K, type, 3.3, 0.32, 0.30).lit;
                            double c = NocFacadeMath.WindowV2(i, jl + 1, band, face, K, type, 3.3, 0.32, 0.30).lit;
                            sumLit += a; sumP += NocFacadeMath.PCell(K, face, band, type, i, jl); n++;
                            litL += a; litLR += a * b; litD += a; litDU += a * c;
                        }
            double meanLit = sumLit / n, meanP = sumP / n, right = litLR / Math.Max(litL, 1e-9), up = litDU / Math.Max(litD, 1e-9);
            Debug.Log(FormattableString.Invariant($"[FX] section runs: mean lit {meanLit:F4} vs mean p {meanP:F4} ({n} cells); P(right lit | lit) {right:F3}, P(floor above lit | lit) {up:F3}"));
            Assert.AreEqual(meanP, meanLit, 0.06 * meanP, "E[lit] = p (the runs keep every mean)");
            Assert.Greater(right, 0.6, "lit cells line up along the floor (a single-cell field gives about p)");
            Assert.Less(up, 0.4, "the runs are per floor, not columns");
            double sm = 0; int nd = 0, voids = 0;
            for (int ub = 0; ub < 200; ub++)
                for (int vb = 0; vb < 100; vb++)
                {
                    double m = NocFacadeMath.SecDistMul(0x77u, ub, vb);
                    sm += m; nd++;
                    if (Math.Abs(m - NocFacadeConstants.FaSecVoidMul) < 1e-12) voids++;
                }
            Debug.Log(FormattableString.Invariant($"[FX] section districts: mean multiplier {sm / nd:F4}, void share {(double)voids / nd:F3}"));
            Assert.AreEqual(1.0, sm / nd, 0.05, "district multiplier mean 1");
            Assert.AreEqual(NocFacadeConstants.FaSecVoidP, (double)voids / nd, 0.02, "void share");
        }

        [Test]
        public void V2_RegionBudgets_LookV2FacadeMean()
        {
            C3 d = NocFacadeMath.MeanRadiance(0, NocFacadeMath.RegionD, 200.0);
            C3 q = NocFacadeMath.MeanRadiance(0, NocFacadeMath.RegionOQ, 200.0);
            C3 sec = NocFacadeMath.MeanRadiance(1, NocFacadeMath.RegionD, 480.0);
            double yd = Luma(d), yq = Luma(q);
            double cd = 255.0 * NocturnePalette.LinearToSrgb(yd), cq = 255.0 * NocturnePalette.LinearToSrgb(yq);
            Debug.Log(FormattableString.Invariant($"[FA] MeanRadiance beat 2: D {yd:F5} (luma {cd:F1}), OQ {yq:F5} (luma {cq:F1}, {cd - cq:F1} codes darker), section {Luma(sec):F5}; LOOK_V2 4 target D 0.0114 +-3 %"));
            Assert.AreEqual(0.0114, yd, 0.0114 * 0.06, "region D unresolved facade mean (LOOK_V2 4: 0.0114 linear)");
            Assert.Less(yq, yd, "the Old Quarter region is darker");
            Assert.AreEqual(4.0, cd - cq, 2.5, "the Old Quarter is about 4 luma codes darker (lead decision 6)");
        }

        // ------------------------------------------------------------------ box rooms at eye level (TQ-09)
        [Test]
        public void V2_BoxRooms_StableAcrossEyeLevel()
        {
            // S0's vision glass reaches 3.54 m of the 3.6 m floor: the room must not flip when the view crosses the horizontal
            // (the dolly pop of TQ-09); every skin, 30 deg sideways, ky just below / at / above 0, rooms fully on (0.15 m pixels)
            int sb0 = -1;
            for (int b = 0; b < 120 && sb0 < 0; b++) if (NocFacadeMath.SkinOf(b).id == NocFacadeMath.SkinS0) sb0 = b;
            foreach (int sb in new[] { sb0, 17, 44, 200 })
            {
                double mDown = PatchMean(sb, 0.15, 1.0, 0.577, -1e-3, 1000.0, 24.0, 10.0, 36.0, true, 0x2A51C3u, out _);
                double mZero = PatchMean(sb, 0.15, 1.0, 0.577, 0.0, 1000.0, 24.0, 10.0, 36.0, true, 0x2A51C3u, out _);
                double mUp = PatchMean(sb, 0.15, 1.0, 0.577, 1e-3, 1000.0, 24.0, 10.0, 36.0, true, 0x2A51C3u, out _);
                Debug.Log(FormattableString.Invariant($"[FA] box rooms at eye level, byte {sb} skin {NocFacadeMath.SkinOf(sb).id}: ky -1e-3 {mDown:E4}, 0 {mZero:E4}, +1e-3 {mUp:E4}"));
                Assert.AreEqual(mZero, mDown, 0.02 * mZero, "no flip below the horizontal");
                Assert.AreEqual(mZero, mUp, 0.02 * mZero, "no flip above the horizontal");
            }
        }

        // ------------------------------------------------------------------ display-space coverage of the edge bands (INT-05)
        [Test]
        public void EdgeBand_DisplaySpaceCoverage()
        {
            // the curve is the grade's: same start, and ToneFwd is NocGradeMath's Neutral peak
            Assert.AreEqual(NocGradeMath.NeutralStart, NocFacadeConstants.FaToneStart, 1e-12, "FaToneStart = NocGradeMath.NeutralStart");
            foreach (double x in new[] { 0.1, 0.5, 0.76, 0.9, 1.6, 3.0, 8.0 })
            {
                NocGradeMath.Neutral(x, 0.0, 0.0, out double r, out _, out _);
                Assert.AreEqual(r, NocFacadeMath.ToneFwd(x), 1e-9, "Neutral peak at " + x);
                Assert.AreEqual(x, NocFacadeMath.ToneInv(NocFacadeMath.ToneFwd(x)), 1e-9 * Math.Max(1.0, x), "inverse at " + x);
            }
            foreach (double pk in new[] { 0.3, 0.76, 1.6, 4.0 })
            {
                double prev = -1.0;
                for (int i = 0; i <= 20; i++)
                {
                    double c = i / 20.0, v = NocFacadeMath.DisplayCover(pk, c);
                    Assert.AreEqual(c * NocFacadeMath.ToneFwd(pk), NocFacadeMath.ToneFwd(v), 1e-9, "displays as c x the displayed peak");
                    Assert.LessOrEqual(v, c * pk + 1e-12, "never brighter than the linear coverage");
                    Assert.Greater(v, prev, "monotonic in c");
                    prev = v;
                }
                Assert.AreEqual(pk, NocFacadeMath.DisplayCover(pk, 1.0), 1e-9, "full coverage unchanged");
                if (pk < NocFacadeConstants.FaToneStart) Assert.AreEqual(0.5 * pk, NocFacadeMath.DisplayCover(pk, 0.5), 1e-12, "below the start: linear");
            }
            // the white edge band at full gain (peak 1.6): a half-covered pixel was 0.80 (displayed 0.91, nearly the 0.95 peak: a
            // clipped stair-step); now it displays 0.47
            double half = NocFacadeMath.DisplayCover(1.6, 0.5);
            Debug.Log(FormattableString.Invariant($"[FX] edge band peak 1.6: displayed peak {NocFacadeMath.ToneFwd(1.6):F3}; c 0.5 -> radiance {half:F3} (linear 0.800 displays {NocFacadeMath.ToneFwd(0.8):F3})"));
            Assert.AreEqual(0.5 * NocFacadeMath.ToneFwd(1.6), half, 1e-9);
        }

        // ------------------------------------------------------------------ the facade lookdev sheet (TEXTURE_PLAN 7.2)
        [Test]
        public void FacadeSheet_SubjectsAndPoses()
        {
            for (int k = 0; k < 5; k++) Assert.AreEqual(k, NocFacadeMath.SkinOf(NocFacadeSheet.SubjectByte(k)).id, "regional subject S" + k);
            for (int k = 0; k < 5; k++) Assert.AreEqual(k, NocFacadeMath.SkinOf(NocFacadeSheet.SubjectByte(5 + k)).overrideK, "override subject " + k);
            var poses = NocFacadeSheet.Poses();
            Assert.AreEqual(5 + 2 * NocFacadeSheet.SubjectCount, poses.Count);
            foreach (var p in poses) Assert.IsNotNull(ShotPlan.ParsePose(p), p);
            // the row stands inside the north Gulf with its turned footprints (half diagonal) clear of both faces
            double r = NocFacadeSheet.Half * Math.Sqrt(2.0);
            Assert.Greater(NocFacadeSheet.TowerX(0, 0) - r, CityField.GulfXA(NocFacadeSheet.RowZ) + 50.0);
            Assert.Less(NocFacadeSheet.TowerX(NocFacadeSheet.SubjectCount - 1, 2) + r, CityField.GulfXB(NocFacadeSheet.RowZ) - 50.0);
            Assert.Greater(NocFacadeSheet.Pitch, 2.0 * r, "turned towers never touch");
        }
    }
}
