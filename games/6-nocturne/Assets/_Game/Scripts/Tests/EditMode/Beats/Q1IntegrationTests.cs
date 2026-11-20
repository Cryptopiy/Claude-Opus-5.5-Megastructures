using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    /// <summary>
    /// Phase 3 graybox gate (Q1): integration checks over the whole built world (every IBeatBuilder as GameBootstrap discovers them,
    /// CityField included; VisibilityWorld). Shared geometry agreement 1: every walkable surface has its top at y 0 and the floors
    /// cover the whole path to the lane half-width + 1 m; Part 10 items 8 / 28: no visible geometry inside the body the player can
    /// reach (the CharacterController capsule r 0.35 at the lane edge) or inside the 0.2 m camera sphere at eye height 1.7; no beat
    /// with its own floor still calls GrayboxKit. Opaque meshes only (VisibilityWorld excludes glass, the koi and the sky dome).
    /// </summary>
    public sealed class Q1IntegrationTests
    {
        const double Step = 0.25;            // m along s and across the lane
        const double FloorMargin = 1.0;      // agreement 1: floors reach the lane half-width + 1 m
        const double TopTolerance = 0.01;    // walkable top at y 0 +- 1 cm
        const double ProbeFrom = 0.05;       // rays start 5 cm above the walk plane, straight down
        const double ProbeDepth = 3.0;
        const double CapsuleRadius = PlayerTuningRadius - 0.005;   // the reachable capsule surface (D-021), 5 mm slack
        const double PlayerTuningRadius = 0.35;
        const double EyeSphere = 0.2;        // camera sphere (agreement 12 / 14)

        static VisibilityRaycaster s_world;
        static string s_info;
        static void Report(string line) { TestContext.Out.WriteLine("[Q1] " + line); Debug.Log("[Q1] " + line); }

        static VisibilityRaycaster World()
        {
            if (s_world == null)
            {
                s_world = VisibilityWorld.Build(out VisibilityWorld.Info info);
                s_info = info.ToString();
                Report("world: " + s_info);
            }
            return s_world;
        }

        static string Short(string label)
        {
            if (label == null) return "-";
            int i = label.IndexOf('/');
            return i >= 0 ? label.Substring(i + 1) : label;
        }

        static int BeatOf(double s)
        {
            var b = NocturneConstants.BeatBoundaries;
            int n = 1;
            for (int i = 1; i < NocturneConstants.BeatCount; i++) if (s >= b[i]) n = i + 1;
            return n;
        }

        sealed class Run
        {
            public string kind; public double s0, s1, lat0, lat1, y; public string label; public int count;
            public override string ToString() =>
                FormattableString.Invariant($"{kind} s {s0:0.00}-{s1:0.00} lat {lat0:0.00}..{lat1:0.00} y {y:0.000} ({count} rays, {Short(label)})");
        }

        static void Add(List<Run> runs, string kind, double s, double lat, double y, string label)
        {
            // merge with an open run of the same kind and source when it continues within two steps
            for (int i = runs.Count - 1; i >= 0 && i >= runs.Count - 8; i--)
            {
                var r = runs[i];
                if (r.kind == kind && r.label == label && s - r.s1 <= 2 * Step + 1e-9)
                {
                    r.s1 = s; r.lat0 = Math.Min(r.lat0, lat); r.lat1 = Math.Max(r.lat1, lat); r.count++;
                    if (Math.Abs(y) > Math.Abs(r.y)) r.y = y;
                    return;
                }
            }
            runs.Add(new Run { kind = kind, s0 = s, s1 = s, lat0 = lat, lat1 = lat, y = y, label = label, count = 1 });
        }

        // ------------------------------------------------------------------ agreement 1: the floor
        /// <summary>True when a barrier (rail, balustrade shoe, wall, kerb) stands between the lane edge and the lateral `lat`:
        /// a floor hole behind a barrier is the edge of a deck (tongue tip w(x) 2.5, bridge deck 2.5 < lane 2.15 + 1), not a hole.</summary>
        static bool BehindBarrier(VisibilityRaycaster w, double s, double lane, double lat)
        {
            double sign = Math.Sign(lat);
            Vector3 a = NocLanes.CorridorToWorld(s, sign * lane, 0.0);
            Vector3 b = NocLanes.CorridorToWorld(s, lat, 0.0);
            var d = new D3(b.x - a.x, 0, b.z - a.z);
            double len = d.Length;
            if (len < 1e-6) return false;
            d = d * (1.0 / len);
            foreach (double h in BarrierHeights)
                if (w.Raycast(new D3(a.x, h, a.z), d, len, out double _, out VisClass _, out string _)) return true;
            return false;
        }

        static readonly double[] BarrierHeights = { 0.03, 0.08, 0.15, 0.3, 0.5, 0.8, 1.0, 1.05, 1.08 };
        const double GrooveDepth = 0.015;
        /// <summary>Floor furniture the walk crosses by design. The south tram track at s 162-164 was one (0.07 m rails lying on the
        /// street, +0.041); GE GB-16 embeds the rails (top at +0.008, inside the 1 cm tolerance), so the list is empty and a rail proud
        /// of the street fails again. Deck seams (Beat04 SkybridgeDeck: a 12 mm groove every 4 m) are not steps.</summary>
        static readonly string[] Furniture = { };

        /// <summary>The accepted hole (D-029 GB-05, GE): the KS13 glass strip x 521.3-541.3, |z - 195| <= 1.2 in the bridge deck, under a
        /// NocGlass pane (glass is no occluder here) with PathGuide's floor collider at y -0.05.</summary>
        static bool InGlassStrip(double x, double z) => x >= SkybridgeDims.StripX0 - 1e-6 && x <= SkybridgeDims.StripX1 + 1e-6 && Math.Abs(z - SkybridgeDims.AxisZ) <= SkybridgeDims.StripHalf + 1e-6;

        /// <summary>The glass edge of the pier (AB, D-032 1, proposed decision in docs/logs/AB.md): x 338..371.95, lateral 1.81..w - 0.30
        /// on both sides, a NocGlass pane 3 cm below the top on a steel frame (the cross beams under it are not a floor) with
        /// PathGuide's floor collider at y 0.</summary>
        static bool InGlassLedge(double x, double z) => EdgeDims.InGlassLedge(x, Math.Abs(z - EdgeDims.TongueZ));

        [Test]
        public void A_EveryLaneMetreIsFlooredAtYZero()
        {
            var w = World();
            var runs = new List<Run>();
            long rays = 0, holes = 0, marginHoles = 0, behind = 0, steps = 0, grooves = 0, marginOdd = 0, furniture = 0, strip = 0, ledge = 0;
            var perBeat = new int[NocturneConstants.BeatCount];
            var down = new D3(0, -1, 0);
            double L = NocturneConstants.PathLength;
            for (double s = 0; s <= L + 1e-9; s += Step)
            {
                double lane = NocLanes.LaneHalfWidth(s);
                double hw = lane + FloorMargin;
                int n = (int)Math.Ceiling(2 * hw / Step);
                for (int k = 0; k <= n; k++)
                {
                    // off-grid by a few mm so that no ray runs exactly along a shared mesh edge (a float ray test may miss both
                    // triangles there, a rasteriser never does)
                    double lat = -hw + 2 * hw * k / n + (k > 0 && k < n ? 0.0071 : 0.0);
                    Vector3 p = NocLanes.CorridorToWorld(s + 0.0037, lat, 0.0);
                    var o = new D3(p.x, ProbeFrom, p.z);
                    rays++;
                    bool inLane = Math.Abs(lat) <= lane + PlayerTuningRadius;
                    if (InGlassStrip(p.x, p.z)) { strip++; continue; }          // the pane (and the truss seen through it) is not a floor
                    if (InGlassLedge(p.x, p.z)) { ledge++; continue; }          // the pier's glass edge (its frame seen through it)
                    if (!w.Raycast(o, down, ProbeDepth, out double t, out VisClass _, out string label))
                    {
                        if (inLane) { holes++; perBeat[BeatOf(s) - 1]++; Add(runs, "HOLE", s, lat, double.NaN, null); }
                        else if (BehindBarrier(w, s, lane, lat)) behind++;
                        else { marginHoles++; perBeat[BeatOf(s) - 1]++; Add(runs, "MARGIN-HOLE", s, lat, double.NaN, null); }
                        continue;
                    }
                    double y = ProbeFrom - t;
                    if (Math.Abs(y) <= TopTolerance) continue;
                    if (inLane)
                    {
                        if (y < 0 && y >= -GrooveDepth) { grooves++; Add(runs, "groove", s, lat, y, label); }
                            else if (y > 0 && label != null && Array.Exists(Furniture, f => label.Contains(f))) { furniture++; Add(runs, "on-floor", s, lat, y, label); }
                        else { steps++; perBeat[BeatOf(s) - 1]++; Add(runs, y < 0 ? "LOW" : "HIGH", s, lat, y, label); }
                    }
                    else { marginOdd++; Add(runs, y < 0 ? "margin-low" : "margin-high", s, lat, y, label); }
                }
            }
            var sb = new StringBuilder();
            sb.Append(FormattableString.Invariant($"floor probe: {rays} rays (s 0-{L:0}, every {Step} m, lane + {FloorMargin} m): in-lane {holes} holes and {steps} tops off y 0 by > {TopTolerance} m ({grooves} seam grooves <= {GrooveDepth} m, {furniture} on accepted floor furniture, {strip} over the glass strip, {ledge} over the pier's glass edge); margin {marginHoles} holes not behind a barrier, {behind} beyond a deck edge behind a barrier, {marginOdd} tops off y 0 (furniture, barriers, trenches)"));
            sb.Append("; failures per beat ").Append(string.Join("/", perBeat));
            Report(sb.ToString());
            WriteRuns("q1_floor_probe.txt", sb.ToString(), runs);
            foreach (var r in runs) if (r.kind == r.kind.ToUpperInvariant()) Report("  " + r);
            Assert.AreEqual(0, holes, "holes in the walk floor inside the reachable lane");
            Assert.AreEqual(0, marginHoles, "holes in the floor margin (lane + 1 m) not behind a barrier");
            Assert.AreEqual(0, steps, "walkable tops off y 0 inside the reachable lane");
            Assert.Greater(strip, 0, "the KS13 glass strip is open under its pane (GB-05)");
            Assert.Greater(ledge, 0, "the pier's glass edge is open under its pane (AB, D-032 1)");
        }

        static void WriteRuns(string file, string head, List<Run> runs)
        {
            string dir = Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..")), "QA", "reports");
            Directory.CreateDirectory(dir);
            var sb = new StringBuilder(head).AppendLine();
            foreach (var r in runs) sb.Append(r).AppendLine();
            File.WriteAllText(Path.Combine(dir, file), sb.ToString());
        }

        // ------------------------------------------------------------------ Part 10 items 8 / 28: reachable body and camera
        [Test]
        public void B_NoGeometryInsideTheReachableBodyOrTheCameraSphere()
        {
            var w = World();
            var runs = new List<Run>();
            // 26 sphere directions (cube corners, edges and faces)
            var dirs = new List<D3>();
            for (int a = -1; a <= 1; a++) for (int b = -1; b <= 1; b++) for (int c = -1; c <= 1; c++)
                if (a != 0 || b != 0 || c != 0) dirs.Add(new D3(a, b, c).Normalized);
            var ring = new D3[16];
            for (int i = 0; i < 16; i++) { double phi = i * Math.PI / 8; ring[i] = new D3(Math.Cos(phi), 0, Math.Sin(phi)); }
            double[] heights = { 0.25, 0.75, 1.25, 1.75 };
            long probes = 0, bodyHits = 0, eyeHits = 0;
            double L = NocturneConstants.PathLength;
            for (double s = 0; s <= L - 0.4 + 1e-9; s += 0.5)
            {
                double lane = NocLanes.LaneHalfWidth(s);
                foreach (double lat in new[] { -lane, 0.0, lane })
                {
                    Vector3 p = NocLanes.CorridorToWorld(s, lat, 0.0);
                    probes++;
                    foreach (double h in heights)
                    {
                        var o = new D3(p.x, h, p.z);
                        foreach (var d in ring)
                        {
                            if (!w.Raycast(o, d, CapsuleRadius, out double t, out VisClass _, out string label)) continue;
                            bodyHits++;
                            Add(runs, "BODY", s, lat, h, label);
                        }
                    }
                    var e = new D3(p.x, NocturneConstants.EyeHeight, p.z);
                    foreach (var d in dirs)
                    {
                        if (!w.Raycast(e, d, EyeSphere, out double t, out VisClass _, out string label)) continue;
                        eyeHits++;
                        Add(runs, "EYE", s, lat, NocturneConstants.EyeHeight, label);
                    }
                }
            }
            Report(FormattableString.Invariant($"body / camera probe: {probes} stations (s every 0.5 m, lateral -lane / 0 / +lane): capsule r {CapsuleRadius:0.000} hits {bodyHits}, eye sphere r {EyeSphere} hits {eyeHits}"));
            WriteRuns("q1_body_probe.txt", FormattableString.Invariant($"body hits {bodyHits}, eye hits {eyeHits}"), runs);
            int shown = 0;
            foreach (var r in runs) { if (shown++ < 60) Report("  " + r); }
            Assert.AreEqual(0, bodyHits, "visible geometry inside the reachable capsule");
            Assert.AreEqual(0, eyeHits, "visible geometry inside the camera sphere");
        }

        // ------------------------------------------------------------------ agreement 1: GrayboxKit only for beats without a floor
        [Test]
        public void C_NoBeatWithItsOwnFloorCallsGrayboxKit()
        {
            string beats = Path.Combine(Application.dataPath, "_Game", "Beats");
            var callers = new List<string>();
            foreach (var f in Directory.GetFiles(beats, "*.cs", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(f) == "GrayboxKit.cs") continue;
                foreach (var line in File.ReadAllLines(f))
                {
                    string l = line.Trim();
                    if (l.StartsWith("//", StringComparison.Ordinal)) continue;
                    if (l.Contains("GrayboxKit.")) callers.Add(Path.GetFileName(f) + ": " + l);
                }
            }
            Report("GrayboxKit callers: " + (callers.Count == 0 ? "none" : string.Join("; ", callers)));
            Assert.AreEqual(0, callers.Count, "every beat floors its own range (agreement 1)");
        }
    }
}
