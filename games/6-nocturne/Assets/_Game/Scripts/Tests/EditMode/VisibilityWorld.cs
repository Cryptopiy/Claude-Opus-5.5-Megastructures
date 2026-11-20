using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using Unity.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Megastructure.EditorTools;
using Debug = UnityEngine.Debug;

namespace Megastructure.Tests
{
    /// <summary>
    /// Builds the real world in EditMode for the 11.13 audit (V): new root objects in the test runner's scene (an untitled scene cannot
    /// take an additive scene), a WorldContext with the MaterialLibrary
    /// asset, a camera and the City / Backdrop roots, then every IBeatBuilder exactly as GameBootstrap discovers them (reflection over
    /// the Runtime assembly, parameterless constructors, ordered by Name; CityBuilder "City00_Field" included). The opaque MeshRenderers
    /// become the occluder triangles of a VisibilityRaycaster: materials with a render queue >= 2500 (NocGlass, the koi, rain) and
    /// anything under the BackdropRoot (the camera-following sky dome) or named "koi" are excluded; instanced batches (signs, streaks,
    /// traffic, beacons) are not MeshRenderers. Meshes uploaded as non-readable are read back through Mesh.AcquireReadOnlyMeshData,
    /// and CityField cell meshes ("City_T{ring}_{I}_{J}") are regenerated with CityField / CityMesher (the same code path as
    /// CityBuilder) when the read-back fails. Everything created is destroyed and the static registries are cleared afterwards.
    /// </summary>
    public static class VisibilityWorld
    {
        public sealed class Info
        {
            public readonly List<string> Builders = new List<string>();
            public readonly List<string> Failed = new List<string>();
            public int Renderers, Used, Transparent, Backdrop, Koi, Unreadable, CityRegenerated, ReadBack;
            public long Triangles;
            public double BuildSeconds, BvhSeconds;
            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append(FormattableString.Invariant($"{Builders.Count} builders ({string.Join(", ", Builders)}); {Used} of {Renderers} mesh renderers as occluders, {Triangles} triangles"));
                sb.Append(FormattableString.Invariant($" (excluded: {Transparent} transparent, {Backdrop} backdrop, {Koi} koi; {Unreadable} unreadable; {ReadBack} read back; {CityRegenerated} city cells regenerated)"));
                sb.Append(FormattableString.Invariant($"; world {BuildSeconds:0.0} s, BVH {BvhSeconds:0.0} s"));
                if (Failed.Count > 0) sb.Append("; builder failures: ").Append(string.Join("; ", Failed));
                return sb.ToString();
            }
        }

        public static VisibilityRaycaster Build(out Info info)
        {
            info = new Info();
            var rc = new VisibilityRaycaster();
            Scene scene = SceneManager.GetActiveScene();
            var before = new HashSet<GameObject>(scene.GetRootGameObjects());
            var roots = new List<GameObject>();
            bool ignore = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
            var sw = Stopwatch.StartNew();
            try
            {
                var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(NocPaths.LibraryAsset);
                var city = new GameObject("City").transform;
                var backdrop = new GameObject("Backdrop").transform;
                var camGo = new GameObject("VisibilityCamera");
                var cam = camGo.AddComponent<Camera>();
                cam.enabled = false;
                var ctx = new WorldContext { Materials = lib, CityRoot = city, BackdropRoot = backdrop, Camera = cam, Seed = 1, Tier = 0 };
                foreach (var b in DiscoverBuilders())
                {
                    info.Builders.Add(b.Name);
                    try { b.Build(ctx); }
                    catch (Exception e) { info.Failed.Add(b.Name + ": " + e.GetType().Name + " " + e.Message); }
                }
                info.BuildSeconds = sw.Elapsed.TotalSeconds;
                var pos = new List<Vector3>(4096);
                var idx = new List<int>(8192);
                foreach (var go in scene.GetRootGameObjects()) if (!before.Contains(go)) roots.Add(go);
                foreach (var root in roots)
                {
                    foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        info.Renderers++;
                        var mf = mr.GetComponent<MeshFilter>();
                        Mesh mesh = mf != null ? mf.sharedMesh : null;
                        if (mesh == null) continue;
                        string path = PathOf(mr.transform);
                        if (mr.transform.IsChildOf(backdrop)) { info.Backdrop++; continue; }
                        if (path.IndexOf("koi", StringComparison.OrdinalIgnoreCase) >= 0) { info.Koi++; continue; }
                        Material m = mr.sharedMaterial;
                        if (m != null && (m.renderQueue >= 2500 || (m.shader != null && m.shader.name.IndexOf("NocSky", StringComparison.Ordinal) >= 0))) { info.Transparent++; continue; }
                        if (!Read(mesh, pos, idx, info))
                        {
                            if (!CityCell(mesh.name, pos, idx)) { info.Unreadable++; continue; }
                            info.CityRegenerated++;
                        }
                        rc.AddMesh(path, pos, idx, mr.transform.localToWorldMatrix);
                        info.Used++;
                    }
                }
                var bw = Stopwatch.StartNew();
                rc.Build();
                info.BvhSeconds = bw.Elapsed.TotalSeconds;
                info.Triangles = rc.TriangleCount;
            }
            finally
            {
                if (roots.Count == 0) foreach (var go in scene.GetRootGameObjects()) if (!before.Contains(go)) roots.Add(go);
                Cleanup(roots);
                NocCameraBatches.Clear();
                InstancedRegistry.Clear();
                NocWetRegistry.Clear();
                LogAssert.ignoreFailingMessages = ignore;
            }
            return rc;
        }

        static List<IBeatBuilder> DiscoverBuilders()
        {
            var list = new List<IBeatBuilder>();
            foreach (var t in typeof(IBeatBuilder).Assembly.GetTypes())
            {
                if (t.IsAbstract || t.IsInterface || !typeof(IBeatBuilder).IsAssignableFrom(t)) continue;
                if (t.GetConstructor(Type.EmptyTypes) == null) continue;
                try { list.Add((IBeatBuilder)Activator.CreateInstance(t)); }
                catch (Exception e) { Debug.LogWarning("[VIS] cannot create builder " + t.Name + ": " + e.Message); }
            }
            list.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return list;
        }

        static string PathOf(Transform t)
        {
            var sb = new StringBuilder(t.name);
            for (var p = t.parent; p != null; p = p.parent) sb.Insert(0, p.name + "/");
            return sb.ToString();
        }

        static bool Read(Mesh mesh, List<Vector3> pos, List<int> idx, Info info)
        {
            pos.Clear(); idx.Clear();
            if (mesh.isReadable)
            {
                mesh.GetVertices(pos);
                var tmp = new List<int>();
                for (int s = 0; s < mesh.subMeshCount; s++)
                {
                    if (mesh.GetTopology(s) != MeshTopology.Triangles) continue;
                    mesh.GetTriangles(tmp, s);
                    idx.AddRange(tmp);
                }
                return idx.Count > 0;
            }
            try
            {
                using (var data = Mesh.AcquireReadOnlyMeshData(mesh))
                {
                    var md = data[0];
                    using (var v = new NativeArray<Vector3>(md.vertexCount, Allocator.Temp))
                    {
                        md.GetVertices(v);
                        for (int i = 0; i < v.Length; i++) pos.Add(v[i]);
                    }
                    for (int s = 0; s < md.subMeshCount; s++)
                    {
                        var sd = md.GetSubMesh(s);
                        if (sd.topology != MeshTopology.Triangles) continue;
                        using (var ix = new NativeArray<int>(sd.indexCount, Allocator.Temp))
                        {
                            md.GetIndices(ix, s, true);
                            for (int i = 0; i < ix.Length; i++) idx.Add(ix[i]);
                        }
                    }
                }
                if (idx.Count > 0 && pos.Count > 0) { info.ReadBack++; return true; }
            }
            catch (Exception) { }
            pos.Clear(); idx.Clear();
            return false;
        }

        /// <summary>Regenerates a CityField cell mesh named "City_T{ring}_{I}_{J}" (CityBuilder's naming) in cell-local coordinates.</summary>
        static bool CityCell(string name, List<Vector3> pos, List<int> idx)
        {
            pos.Clear(); idx.Clear();
            if (string.IsNullOrEmpty(name) || !name.StartsWith("City_T", StringComparison.Ordinal)) return false;
            var parts = name.Substring(6).Split('_');
            if (parts.Length != 3) return false;
            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int ring)) return false;
            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int I)) return false;
            if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int J)) return false;
            var towers = new List<CityTower>(4);
            CityField.CellTowers(I, J, ring, towers);
            if (towers.Count == 0) return false;
            double cell = NocturneConstants.CityCell2;
            var mb = new MeshBuilder();
            int culled = 0;
            CityMesher.Append(mb, towers, (I + 0.5) * cell, (J + 0.5) * cell, ref culled);
            pos.AddRange(mb.Positions);
            idx.AddRange(mb.Indices);
            return idx.Count > 0;
        }

        static void Cleanup(List<GameObject> roots)
        {
            var meshes = new HashSet<Mesh>();
            var mats = new HashSet<Material>();
            foreach (var root in roots)
            {
                if (root == null) continue;
                foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true)) if (mf.sharedMesh != null && !AssetDatabase.Contains(mf.sharedMesh)) meshes.Add(mf.sharedMesh);
                foreach (var r in root.GetComponentsInChildren<Renderer>(true))
                    foreach (var m in r.sharedMaterials) if (m != null && !AssetDatabase.Contains(m)) mats.Add(m);
            }
            foreach (var m in meshes) UnityEngine.Object.DestroyImmediate(m);
            foreach (var m in mats) UnityEngine.Object.DestroyImmediate(m);
            foreach (var root in roots) if (root != null) UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
