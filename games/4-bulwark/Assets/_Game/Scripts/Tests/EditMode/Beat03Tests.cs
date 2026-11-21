// Beat03Tests: beat 3, the spillway bridge (MASTER_PROMPT 14.5.3 acceptance (1), 14.10.1 T-SCALE / T-MESH): the numbers of
// SpillwayGeometry (piers 4.0, slots 16.0, sill -14.0, bars 40 mm at 80 mm, 14 grated slots, lip speed 8.3 +-0.2 m/s, the
// draw-down and the sheet parabola) and the built beat (mesh validation, triangle budgets, the grating count, nothing
// inside the capsule reach of the road lane, the radial gate raised 5 m, the graybox key).
using System;
using System.Collections.Generic;
using Megastructure.Beats;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Megastructure.Tests
{
    public class Beat03Tests
    {
        const string LibraryAsset = "Assets/_Game/Materials/MaterialLibrary.asset";

        // ------------------------------------------------------------------ numbers
        [Test]
        public void Spillway_DimensionsOf14p5p3()
        {
            Assert.AreEqual(15, BulwarkConst.PierCount);
            Assert.AreEqual(14, BulwarkConst.SlotCount);
            for (int k = 0; k < BulwarkConst.PierCount; k++)
            {
                Assert.AreEqual(4.0, SpillwayGeometry.PierZ1(k) - SpillwayGeometry.PierZ0(k), 0.02, "pier " + k);
                Assert.AreEqual(420.0 + 20.0 * k, SpillwayGeometry.PierZ0(k), 1e-9);
            }
            for (int k = 0; k < BulwarkConst.SlotCount; k++)
            {
                Assert.AreEqual(16.0, SpillwayGeometry.SlotZ1(k) - SpillwayGeometry.SlotZ0(k), 0.02, "slot " + k);
                Assert.AreEqual(SpillwayGeometry.PierZ1(k), SpillwayGeometry.SlotZ0(k), 1e-9, "slot " + k + " starts at its pier");
                Assert.AreEqual(SpillwayGeometry.PierZ0(k + 1), SpillwayGeometry.SlotZ1(k), 1e-9, "slot " + k + " ends at the next pier");
            }
            Assert.AreEqual(-14.0, SpillwayGeometry.SillY, 1e-9, "sill");
            Assert.AreEqual(0.04, BulwarkConst.GratingBar, 1e-9, "bars 40 mm");
            Assert.AreEqual(0.08, BulwarkConst.GratingPitch, 1e-9, "at 80 mm");
            Assert.AreEqual(12.0, BulwarkConst.SpillwayDeckWidth, 1e-9);
        }

        [Test]
        public void DrawDown_ProfileSpeedAndLip()
        {
            Assert.AreEqual(-8.0, SpillwayGeometry.WaterY(46.0), 1e-9, "y -8.0 at x 46");
            Assert.AreEqual(-11.5, SpillwayGeometry.WaterY(-6.0), 1e-9, "y -11.5 at the lip");
            Assert.AreEqual(0.0, SpillwayGeometry.WaterSlope(46.0), 1e-9, "flat where the sea meets it (BwSeaCalm)");
            Assert.AreEqual(0.0, SpillwayGeometry.WaterSlope(-6.0), 1e-9, "horizontal at the lip (the parabola starts level)");
            Assert.AreEqual(0.8, SpillwayGeometry.Speed(46.0), 1e-9, "0.8 m/s at the mouth");
            Assert.AreEqual(8.3, SpillwayGeometry.Speed(-6.0), 0.2, "lip speed 8.3 +-0.2 m/s (Bernoulli for the 3.5 m drop)");
            double prevY = 1, prevV = 0;
            for (double x = 46.0; x >= -6.0; x -= 0.5)
            {
                double y = SpillwayGeometry.WaterY(x), v = SpillwayGeometry.Speed(x);
                Assert.LessOrEqual(y, prevY + 1e-12, "monotonic draw-down");
                Assert.GreaterOrEqual(v, prevV - 1e-12, "accelerating flow");
                prevY = y; prevV = v;
            }
            Assert.Greater(SpillwayGeometry.LipTravelTime, 5.0);
            Assert.Less(SpillwayGeometry.LipTravelTime, 40.0);
        }

        [Test]
        public void Sheet_ParabolaThicknessAndBreakup()
        {
            Assert.AreEqual(-6.0, SpillwayGeometry.SheetX(0.0), 1e-9);
            Assert.AreEqual(-11.5, SpillwayGeometry.SheetY(0.0), 1e-9);
            Assert.AreEqual(-45.0, SpillwayGeometry.SheetX(4.7), 0.05, "x -45 at t 4.7 s");
            Assert.AreEqual(-120.0, SpillwayGeometry.SheetY(4.7), 0.5, "y -120 at t 4.7 s");
            Assert.AreEqual(2.5, SpillwayGeometry.SheetThickness(0.0), 1e-9, "2.5 m thick at the lip");
            Assert.AreEqual(0.44, SpillwayGeometry.SheetThickness(4.7), 0.01, "0.44 m at t 4.7 s (RESEARCH B 3.1)");
            SpillwayGeometry.SheetNormal(0.0, out double nx, out double ny);
            Assert.AreEqual(0.0, nx, 1e-9); Assert.AreEqual(1.0, ny, 1e-9);
            // the mesh dissolves before its end; the analytic curtain fades in over D 70-95 m: the two overlap (no gap)
            double dEnd = SpillwayGeometry.SheetDrop(SpillwayGeometry.BreakupT1);
            Assert.Greater(dEnd, Megastructure.Rendering.MistVolumeMath.CurtainFadeIn1, "mesh still present where the curtain is full");
            Assert.Less(SpillwayGeometry.SheetDrop(SpillwayGeometry.BreakupT0), Megastructure.Rendering.MistVolumeMath.CurtainFadeIn0, "breakup starts above the curtain");
            Assert.Less(SpillwayGeometry.BreakupT1, SpillwayGeometry.SheetT1, "the mesh is gone before its last row");
        }

        [Test]
        public void RadialGate_RaisedFiveMetresAtX9To12()
        {
            double R = SpillwayGeometry.GateRadius;
            Assert.AreEqual(14.0, R, 1e-9, "radius 14 m");
            double phiB = Math.Asin((SpillwayGeometry.GateLipY - SpillwayGeometry.TrunnionY) / R);
            double phiT = phiB + SpillwayGeometry.GateSpanDeg * BwMath.Deg;
            double xLip = SpillwayGeometry.TrunnionX + R * Math.Cos(phiB);
            double yTop = SpillwayGeometry.TrunnionY + R * Math.Sin(phiT);
            Assert.AreEqual(-9.0, SpillwayGeometry.GateLipY, 1e-9, "lip 5 m above the sill");
            Assert.AreEqual(10.0, xLip, 0.1, "lip at x 10");
            Assert.LessOrEqual(SpillwayGeometry.TrunnionX + R, 12.0 + 1e-6, "plate within x 9-12");
            Assert.Less(yTop, 0.5, "plate top under the pier caps");
            Assert.Greater(SpillwayGeometry.WaterY(xLip), -11.5 - 1e-9);
            Assert.Greater(SpillwayGeometry.GateLipY, SpillwayGeometry.WaterY(xLip), "the raised lip clears the racing water");
        }

        [Test]
        public void ReplacesTheGrayboxPartBeat3()
        {
            Assert.IsTrue(GrayboxReplace.IsReplaced("beat3"), "[ReplacesGraybox(\"beat3\")] on Beat03SpillwayBridge");
        }

        // ------------------------------------------------------------------ the built beat
        sealed class Built : IDisposable
        {
            public GameObject Root;
            public Beat03SpillwayBridge Beat;
            public WorldContext Ctx;
            public Built()
            {
                var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(LibraryAsset);
                Root = new GameObject("Beat03TestWorld");
                Ctx = new WorldContext { Root = Root.transform, Materials = lib };
                Beat = new Beat03SpillwayBridge();
                Beat.Build(Ctx);
            }
            public void Dispose()
            {
                foreach (var mf in Root.GetComponentsInChildren<MeshFilter>(true)) if (mf.sharedMesh != null) UnityEngine.Object.DestroyImmediate(mf.sharedMesh);
                UnityEngine.Object.DestroyImmediate(Root);
                foreach (var m in Beat.RuntimeMaterials) if (m != null) UnityEngine.Object.DestroyImmediate(m);
            }
        }

        [Test]
        public void Build_ValidMeshesBudgetsAndFourteenGratings()
        {
            using (var w = new Built())
            {
                Assert.IsEmpty(w.Ctx.MeshProblems, "MeshValidation: " + string.Join("; ", w.Ctx.MeshProblems));
                Assert.LessOrEqual(w.Beat.Triangles, 300000, "14.5.3: about 300 k triangles");
                Assert.LessOrEqual(w.Beat.SheetTriangles, 18000, "14.5.3: near sheets 18 k triangles");
                Assert.Greater(w.Beat.SheetTriangles, 5000);
                Transform grating = w.Root.transform.Find("Beat03_SpillwayBridge/b03_grating");
                Assert.IsNotNull(grating, "grating renderer");
                var mesh = grating.GetComponent<MeshFilter>().sharedMesh;
                Assert.AreEqual(14 * 2, mesh.triangles.Length / 3, "one see-through panel per slot: 14 grated slots");
                var mr = grating.GetComponent<MeshRenderer>();
                Assert.AreEqual(UnityEngine.Rendering.ShadowCastingMode.Off, mr.shadowCastingMode, "40 mm bars cast no cascade shadow (aliasing)");
                if (mr.sharedMaterial != null && mr.sharedMaterial.HasProperty("_BarWidth"))
                {
                    Assert.AreEqual(0.04f, mr.sharedMaterial.GetFloat("_BarWidth"), 1e-4f, "bars 40 mm");
                    Assert.AreEqual(0.08f, mr.sharedMaterial.GetFloat("_BarPitch"), 1e-4f, "at 80 mm");
                }
                var b = mesh.bounds;
                Assert.AreEqual(0.0, b.max.y, 1e-4, "grating top on the walking plane y 0");
                Assert.LessOrEqual(b.min.x, -5.6f + 1e-3f);
                Assert.GreaterOrEqual(b.max.x, 5.3f - 1e-3f);
            }
        }

        [Test]
        public void Rails_RoundBwLinePostsLikeTheCrest()
        {
            // Wave X: round 0.12 m posts through BwLine (H7 >= 1.5 px, the crest's 14.5.2 pattern); the square steel posts showed
            // their sunlit faces as 1-px slivers (mistx-01 back640: 7 NaN-like pixels)
            using (var w = new Built())
            {
                Assert.IsNotNull(w.Root.transform.Find("Beat03_SpillwayBridge/b03_rail_caps"), "caps and base plates");
                if (w.Ctx.Materials == null || w.Ctx.Materials.Rail == null) Assert.Ignore("no material library: square fallback posts");
                Transform posts = w.Root.transform.Find("Beat03_SpillwayBridge/b03_rail_posts");
                Assert.IsNotNull(posts, "post renderer");
                var mr = posts.GetComponent<MeshRenderer>();
                if (mr.sharedMaterial.HasProperty("_LineWidth"))
                    Assert.AreEqual((float)BulwarkConst.LeeRailPostSize, mr.sharedMaterial.GetFloat("_LineWidth"), 1e-4f, "BwLine width 0.12 m");
                var b = posts.GetComponent<MeshFilter>().sharedMesh.bounds;
                Assert.AreEqual(BulwarkConst.LeeRailTop + 0.02, b.max.y, 1e-3, "posts end 2 cm above the top rail");
                Assert.AreEqual(BulwarkConst.LeeRailX - 0.5 * BulwarkConst.LeeRailPostSize, b.min.x, 1e-2, "lee posts on the lee rail axis");
                var grating = w.Root.transform.Find("Beat03_SpillwayBridge/b03_grating").GetComponent<MeshRenderer>().sharedMaterial;
                if (grating != null && grating.HasProperty("_TopWear")) Assert.AreEqual(0.6f, grating.GetFloat("_TopWear"), 1e-4f, "walked grating tops");
            }
        }

        [Test]
        public void Build_NothingInsideTheCapsuleReachOfTheLane()
        {
            // road lane x -5.4 .. 5.1 (capsule centres) + PLAYER_R 0.35: x -5.75 .. 5.45, from 5 cm above the walking plane
            double lo = BulwarkConst.LaneRoadXMin - BulwarkConst.PlayerR, hi = BulwarkConst.LaneRoadXMax + BulwarkConst.PlayerR;
            using (var w = new Built())
            {
                var bad = new List<string>();
                foreach (var mf in w.Root.GetComponentsInChildren<MeshFilter>(true))
                {
                    var m = mf.sharedMesh;
                    if (m == null || !m.isReadable) continue;
                    var v = m.vertices;
                    var tr = mf.transform.localToWorldMatrix;
                    for (int i = 0; i < v.Length; i++)
                    {
                        Vector3 p = tr.MultiplyPoint3x4(v[i]);
                        if (p.z < BulwarkConst.SpillwayZ0 || p.z > BulwarkConst.SpillwayZ1) continue;
                        if (p.y > 0.05f && p.y < 2.5f && p.x > lo + 1e-3 && p.x < hi - 1e-3)
                        {
                            bad.Add(mf.gameObject.name + " (" + p.x.ToString("0.00") + ", " + p.y.ToString("0.00") + ", " + p.z.ToString("0.00") + ")");
                            break;
                        }
                    }
                }
                Assert.IsEmpty(bad, "visible geometry inside the capsule reach of the road lane: " + string.Join("; ", bad));
            }
        }
    }
}
