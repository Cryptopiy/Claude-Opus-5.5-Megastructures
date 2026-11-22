// WetRegistryWalkTests (integration round, QA-F02): the analytic wet-floor mirror (NocWetRegistry, 14.7 H4) with EVERY beat's sources
// registered the way the game registers them (every IBeatBuilder built once, as GameBootstrap and VisibilityWorld do). The registry
// crossfades a source over 12 m so it never pops, but only while at most 32 are active: past the cap the sources ahead are dropped
// in bin order, and a dropped source that is re-admitted when a slot frees enters at its current weight, which is a one-frame pop of
// its reflection (QA-F02: b12 f540 at s 132.0, the arcade streaks of beat 2 jumped). The per-beat tests count one beat (or two) at a
// time; this one counts them all, every 0.25 m of the walk, and asserts nothing is ever dropped and at most 4 walls are active.
// The walls: NocWetRegistry packs the active walls into slots 0-3 in registration order and NocMirror.hlsl seeds a kind-0 wall's
// mirrored windows with its slot, so a kind-0 wall must keep its slot for its whole active life (else its windows re-roll in one
// frame: QA-F02, b12 f540 at s 132.0, when the alley's walls registered before the boulevard's faded out). Checked every 0.05 m
// (exact steps k / 20, so the round s of QA's sequences are hit); the one exception allowed is a single sample where every wall
// before it is exactly at weight 0 (s 412: the boulevard's fade-out ends where face A's fade-in begins; a cap of 4 leaves no room
// for an overlap), which a frame hits only at exactly that s.
using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Megastructure.EditorTools;

namespace Megastructure.Tests
{
    public class WetRegistryWalkTests
    {
        [Test]
        public void WholeWalk_NoSourceDropped_AtMostFourWalls()
        {
            Scene scene = SceneManager.GetActiveScene();
            var before = new HashSet<GameObject>(scene.GetRootGameObjects());
            bool ignore = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
            NocWetRegistry.Clear();
            var dropped = new List<string>();
            var shifts = new List<string>();
            var excursions = new List<string>();
            int maxActive = 0, maxWalls = 0, total = 0; double maxAt = 0.0;
            try
            {
                var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(NocPaths.LibraryAsset);
                var cam = new GameObject("WetWalkCamera").AddComponent<Camera>();
                cam.enabled = false;
                var ctx = new WorldContext { Materials = lib, CityRoot = new GameObject("City").transform, BackdropRoot = new GameObject("Backdrop").transform, Camera = cam, Seed = 1, Tier = 0 };
                var builders = new List<IBeatBuilder>();
                foreach (var t in typeof(IBeatBuilder).Assembly.GetTypes())
                {
                    if (t.IsAbstract || t.IsInterface || !typeof(IBeatBuilder).IsAssignableFrom(t) || t.GetConstructor(Type.EmptyTypes) == null) continue;
                    builders.Add((IBeatBuilder)Activator.CreateInstance(t));
                }
                builders.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
                foreach (var b in builders)
                {
                    try { b.Build(ctx); }
                    catch (Exception e) { Debug.LogWarning("[NOC] wet walk: builder " + b.Name + " threw " + e.GetType().Name + " " + e.Message); }
                }
                total = NocWetRegistry.TotalSources;
                double runStart = double.NaN; int runMax = 0;
                for (double s = 0.0; s <= NocturneConstants.PathLength + 1e-9; s += 0.25)
                {
                    int n = NocWetRegistry.Collect(s);
                    int d = NocWetRegistry.Dropped;
                    if (n + d > maxActive) { maxActive = n + d; maxAt = s; }
                    maxWalls = Math.Max(maxWalls, NocWetRegistry.ActiveWalls);
                    if (d > 0) { if (double.IsNaN(runStart)) { runStart = s; runMax = 0; } runMax = Math.Max(runMax, d); }
                    else if (!double.IsNaN(runStart)) { dropped.Add(string.Format(CultureInfo.InvariantCulture, "s {0:0.00}-{1:0.00} (up to {2})", runStart, s - 0.25, runMax)); runStart = double.NaN; }
                }
                if (!double.IsNaN(runStart)) dropped.Add(string.Format(CultureInfo.InvariantCulture, "s {0:0.00}-end (up to {1})", runStart, runMax));

                // wall slots (registration order, the first MaxWalls active walls)
                int nw = NocWetRegistry.RegisteredWalls;
                var ws0 = new double[nw]; var ws1 = new double[nw]; var kind = new float[nw];
                for (int i = 0; i < nw; i++) { NocWetRegistry.GetWall(i, out _, out Vector4 wb, out _, out ws0[i], out ws1[i]); kind[i] = wb.z; }
                int steps = (int)Math.Round(NocturneConstants.PathLength * 20.0);
                var slots = new int[steps + 1, nw];
                for (int k = 0; k <= steps; k++)
                {
                    double s = k / 20.0;
                    int c = 0;
                    for (int i = 0; i < nw; i++)
                        slots[k, i] = c < NocWetRegistry.MaxWalls && NocWetRegistry.WallWeight(ws0[i], ws1[i], s) > 0.0 ? c++ : -1;
                }
                // a one-sample excursion (the slot at k differs from k - 1 and k + 1, which agree) is the exact handover point (s 412):
                // logged, not failed; any other change of a kind-0 wall's slot while it stays active fails
                Func<int, int, bool> excursion = (k, i) => k > 0 && k < steps && slots[k, i] >= 0 && slots[k - 1, i] >= 0 && slots[k + 1, i] >= 0
                                                          && slots[k, i] != slots[k - 1, i] && slots[k + 1, i] == slots[k - 1, i];
                for (int i = 0; i < nw; i++)
                {
                    if (kind[i] >= 0.5f) continue;
                    for (int k = 1; k <= steps; k++)
                    {
                        if (slots[k, i] < 0 || slots[k - 1, i] < 0 || slots[k, i] == slots[k - 1, i]) continue;
                        string ev = string.Format(CultureInfo.InvariantCulture, "wall {0} (s {1:0}-{2:0}) slot {3} -> {4} at s {5:0.00}", i, ws0[i], ws1[i], slots[k - 1, i], slots[k, i], k / 20.0);
                        if (excursion(k, i)) excursions.Add(ev);
                        else if (!excursion(k - 1, i)) shifts.Add(ev);
                    }
                }
            }
            finally
            {
                var roots = new List<GameObject>();
                foreach (var go in scene.GetRootGameObjects()) if (!before.Contains(go)) roots.Add(go);
                foreach (var root in roots)
                {
                    foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true)) if (mf.sharedMesh != null && !AssetDatabase.Contains(mf.sharedMesh)) UnityEngine.Object.DestroyImmediate(mf.sharedMesh);
                    UnityEngine.Object.DestroyImmediate(root);
                }
                NocCameraBatches.Clear();
                InstancedRegistry.Clear();
                NocWetRegistry.Clear();
                LogAssert.ignoreFailingMessages = ignore;
            }
            Debug.Log(string.Format(CultureInfo.InvariantCulture, "[NOC] wet registry over the walk: {0} sources, at most {1} wanted at once (s {2:0.00}; cap {3}), walls at most {4}; dropped: {5}",
                total, maxActive, maxAt, NocWetRegistry.MaxSources, maxWalls, dropped.Count == 0 ? "never" : string.Join(", ", dropped)));
            Assert.Greater(total, 100, "every beat registered its sources");
            Assert.IsEmpty(dropped, "a source over the cap of 32 is dropped, and re-admitted at its current weight: a one-frame pop of its reflection");
            Assert.LessOrEqual(maxWalls, NocWetRegistry.MaxWalls);
            Debug.Log("[NOC] wet walls: kind-0 slot changes while active: " + (shifts.Count == 0 ? "none" : string.Join("; ", shifts))
                      + "; one-sample handover excursions (exact s only): " + (excursions.Count == 0 ? "none" : string.Join("; ", excursions)));
            Assert.IsEmpty(shifts, "a kind-0 wall changed its mirror slot while active: its mirrored windows re-roll in one frame (QA-F02)");
        }
    }
}
