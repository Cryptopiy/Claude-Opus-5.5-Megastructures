// SurfaceTests: EditMode tests of the Look v2 near surfaces and wet floors (TEXTURE_PLAN 3.3, 3.5, 8.3 SU; owner SU) on their C#
// twins (NocSurfaceMath, NocSurfaceConstants) and the leak-anchor bake (VertexBake.TopDistance):
//   the puddle threshold of the noise alone is its 65th percentile; a height distribution moves the threshold and the fraction
//   stays 0.35 +- 0.02; low height pools first; the rings stay inside the 2 x 2 lookup and are much smaller and fainter than v1;
//   leak anchoring writes the distance below the face top into colour B on test boxes (separate faces, shared-vertex chamfer
//   boxes, strips); the leak decal's v stays inside its content and pad; the NocLine paint keeps its mean and fades to it.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class SurfaceTests
    {
        static List<double> Uniform(int n, double lo, double hi, int seed)
        {
            var r = new System.Random(seed);
            var l = new List<double>(n);
            for (int i = 0; i < n; i++) l.Add(lo + (hi - lo) * r.NextDouble());
            return l;
        }

        [Test]
        public void PuddleThreshold_NoiseAlone_Is65thPercentile()
        {
            double th = NocSurfaceMath.PuddleThreshold(null, 0.35, 400000);
            Assert.AreEqual(NocSurfaceConstants.SurfPuddleTh0, th, 0.004, "SurfPuddleTh0 = the 65th percentile of the 2-octave noise");
            double f = NocSurfaceMath.PuddleFraction(null, NocSurfaceConstants.SurfPuddleTh0, 400000);
            Assert.AreEqual(0.35, f, NocSurfaceConstants.SurfPuddleTol);
        }

        [Test]
        public void PuddleThreshold_HeightHistogram_KeepsTheFraction_LowFillsFirst()
        {
            // a broad and a narrow height histogram (joints: a low tail)
            var broad = Uniform(20000, 0.0, 1.0, 11);
            var joints = Uniform(18000, 0.55, 0.75, 12);
            joints.AddRange(Uniform(2000, 0.0, 0.15, 13));
            foreach (var hs in new[] { broad, joints })
            {
                double th = NocSurfaceMath.PuddleThreshold(hs, 0.35, 400000);
                double f = NocSurfaceMath.PuddleFraction(hs, th, 400000);
                Assert.AreEqual(NocSurfaceConstants.SurfPuddleFraction, f, NocSurfaceConstants.SurfPuddleTol, "fraction from the measured threshold");
            }
            // low spots fill first: the wet share of the low tail is far above the share of the plateau
            double thj = NocSurfaceMath.PuddleThreshold(joints, 0.35, 400000);
            double lowWet = NocSurfaceMath.PuddleFraction(Uniform(5000, 0.0, 0.15, 14), thj, 200000);
            double highWet = NocSurfaceMath.PuddleFraction(Uniform(5000, 0.55, 0.75, 15), thj, 200000);
            Assert.Greater(lowWet, highWet + 0.25, FormattableString.Invariant($"joints wet {lowWet:F3} vs plateau {highWet:F3}"));
        }

        [Test]
        public void PuddleNoise_SmallerScalesThanV1()
        {
            // the v2 octaves are 2.0 / 0.6 m (v1 4.0 / 1.3 m): the mean blot size halves (CRIT0 #8, GB-10)
            Assert.Less(NocSurfaceConstants.SurfPuddleScale0, NocturneConstants.PuddleScale0 * 0.6);
            Assert.Less(NocSurfaceConstants.SurfPuddleScale1, NocturneConstants.PuddleScale1 * 0.6);
            // the noise stays in [0, 1] and continuous (max step over 1 mm below 0.01)
            double prev = NocSurfaceMath.PuddleNoise(0, 0), maxStep = 0;
            for (int i = 1; i < 20000; i++)
            {
                double v = NocSurfaceMath.PuddleNoise(i * 0.001, 0.37 * i * 0.001);
                Assert.That(v, Is.InRange(0.0, 1.0));
                maxStep = Math.Max(maxStep, Math.Abs(v - prev));
                prev = v;
            }
            Assert.Less(maxStep, 0.01);
        }

        [Test]
        public void Ripples_InsideLookup_SmallAndFaint()
        {
            Assert.LessOrEqual(NocSurfaceMath.RippleReach, 0.75 * NocSurfaceConstants.SurfRippleCell + 1e-9, "every ring reaching a point lies in the 2 x 2 cells");
            double rMax = NocSurfaceConstants.SurfRippleR0 + NocSurfaceConstants.SurfRippleSpeed * NocSurfaceConstants.SurfRippleLife;
            Assert.That(rMax, Is.InRange(0.05, 0.2), "rings of 0.1-0.4 m diameter (CRIT0 #29)");
            Assert.Less(NocSurfaceConstants.SurfRippleSlope, NocturneConstants.WetRippleSlope, "fainter than the v1 target rings");
            // the slope stays bounded and has no NaN over a patch and a few seconds
            double maxG = 0;
            for (int i = 0; i < 200; i++)
                for (int j = 0; j < 200; j++)
                    for (int k = 0; k < 3; k++)
                    {
                        NocSurfaceMath.RippleGrad(i * 0.0073, j * 0.0071, 0.4 * k + 0.13, 1.0, out double gx, out double gz);
                        Assert.IsFalse(double.IsNaN(gx) || double.IsNaN(gz));
                        maxG = Math.Max(maxG, Math.Sqrt(gx * gx + gz * gz));
                    }
            Assert.Less(maxG, 2.0, "bounded ring slope (x SurfRippleSlope in the shader)");
            Assert.Greater(maxG, 0.1, "rings exist");
            // the lobe's ripple variance constant: mean |g|^2 per unit rho over random points and times (linear in rho)
            var rnd = new System.Random(3);
            foreach (double rho in new[] { 1.0, 0.5 })
            {
                double acc = 0; const int n = 200000;
                for (int i = 0; i < n; i++)
                {
                    NocSurfaceMath.RippleGrad(60.0 * rnd.NextDouble() - 30.0, 60.0 * rnd.NextDouble() - 30.0, 100.0 * rnd.NextDouble(), rho, out double gx, out double gz);
                    acc += gx * gx + gz * gz;
                }
                Assert.AreEqual(NocSurfaceConstants.SurfRippleVar * rho, acc / n, 0.1 * NocSurfaceConstants.SurfRippleVar * rho, FormattableString.Invariant($"mean |g|^2 at rho {rho}"));
            }
        }

        static Mesh BakedMesh(Action<MeshBuilder> build)
        {
            var mb = new MeshBuilder();
            build(mb);
            var mesh = mb.ToMesh("leakTest");
            VertexBake.TopDistance(mesh);
            return mesh;
        }

        static void AssertB(Mesh m, Func<Vector3, float> expectedTop)
        {
            var v = m.vertices;
            var c = m.colors32;
            for (int i = 0; i < v.Length; i++)
            {
                float d = expectedTop(v[i]) - v[i].y;
                int want = Mathf.RoundToInt(255f * Mathf.Clamp01(d / VertexBake.TopDistanceRange));
                Assert.AreEqual(want, c[i].b, 1, FormattableString.Invariant($"vertex {i} at y {v[i].y:F2}: d_top {d:F2} m"));
            }
        }

        [Test]
        public void LeakAnchor_BakesDistanceBelowFaceTop()
        {
            // a flat-shaded box (every face its own vertices): each side face's top is the box top
            var box = BakedMesh(mb => G1Mesh.Box(mb, new Vector3(0, 1, 0), new Vector3(2, 4, 3), G1Mesh.Sides));
            AssertB(box, p => 4f);
            // a chamfered box (flat facets, 96 vertices): every vertical facet (sides and edge chamfers) hangs from y 6 - bevel
            var ch = BakedMesh(mb => G1Mesh.Chamfer(mb, new Vector3(0, 0, 0), new Vector3(1, 6, 1), 0.02f));
            var cv = ch.vertices; var cn = ch.normals; var cc = ch.colors32;
            int vertical = 0;
            for (int i = 0; i < cv.Length; i++)
            {
                if (Mathf.Abs(cn[i].y) > 0.2f) continue;
                vertical++;
                int want = Mathf.RoundToInt(255f * Mathf.Clamp01((6f - 0.02f - cv[i].y) / VertexBake.TopDistanceRange));
                Assert.AreEqual(want, cc[i].b, 1, FormattableString.Invariant($"chamfer vertex {i} at y {cv[i].y:F3}"));
            }
            Assert.GreaterOrEqual(vertical, 32, "4 sides + 4 vertical chamfers");
            // two separate quads at different heights keep their own tops; R and G are kept
            var two = BakedMesh(mb =>
            {
                mb.Color = new Color32(200, 30, 255, 255);
                G1Mesh.Quad(mb, new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 3, 0), new Vector3(0, 3, 0), Vector3.back);
                G1Mesh.Quad(mb, new Vector3(0, 10, 1), new Vector3(1, 10, 1), new Vector3(1, 12.5f, 1), new Vector3(0, 12.5f, 1), Vector3.back);
            });
            AssertB(two, p => p.y <= 3.001f ? 3f : 12.5f);
            foreach (var c in two.colors32) { Assert.AreEqual(200, c.r); Assert.AreEqual(30, c.g); }
            // the range clamps at 25.5 m
            var tall = BakedMesh(mb => G1Mesh.Box(mb, new Vector3(0, 0, 0), new Vector3(1, 40, 1), G1Mesh.PX));
            AssertB(tall, p => 40f);
            Assert.AreEqual(25.5, NocSurfaceConstants.SurfLeakMaxD, 1e-9);
            Assert.AreEqual(VertexBake.TopDistanceRange, (float)NocSurfaceConstants.SurfLeakMaxD, 1e-6f);
        }

        [Test]
        public void LeakDecal_VStaysInContentAndPad()
        {
            Assert.AreEqual(1.0, NocSurfaceMath.LeakV(0, 1), 1e-12, "the decal's top row sits on the anchored edge");
            Assert.AreEqual(0.5, NocSurfaceMath.LeakV(NocSurfaceConstants.SurfLeakLen, 1), 1e-12, "1 m below: the content ends");
            Assert.AreEqual(0.5, NocSurfaceMath.LeakV(2.0 * NocSurfaceConstants.SurfLeakLen, 2), 1e-12, "stretch 2: 2 m long");
            Assert.AreEqual(0.25, NocSurfaceMath.LeakV(20, 1), 1e-12, "clamped in the middle of the pad (no wrap to the top row)");
            for (int i = 0; i < 1000; i++)
            {
                NocSurfaceMath.LeakColumn(i * 0.37 - 100, 5u, out double st, out double off);
                Assert.That(st, Is.InRange(NocSurfaceConstants.SurfLeakStretch0, NocSurfaceConstants.SurfLeakStretch1));
                Assert.That(off, Is.InRange(0.0, 1.0));
            }
            Assert.AreEqual(25.5, NocSurfaceMath.LeakDecode(255), 1e-12);
        }

        [Test]
        public void PuddleNormal_IsUpAtFullPuddle_WetnessPoolsLow()
        {
            // TEXTURE_PLAN 8.3 SU tests: the puddle normal equals up at m = 1 (any texture slope), and wetness pools in low height
            var n = NocSurfaceMath.FloorNormal(0.4, -0.3, 1.0, 0.0, 0.0);
            Assert.AreEqual(0.0, n.x, 1e-12); Assert.AreEqual(1.0, n.y, 1e-12); Assert.AreEqual(0.0, n.z, 1e-12);
            var dry = NocSurfaceMath.FloorNormal(0.4, -0.3, 0.0, 0.0, 0.0);
            Assert.Less(dry.y, 0.95, "the dry floor keeps its relief");
            double low = NocSurfaceMath.SurfaceWet(0.5, 1.0, 0.0, 0.1), high = NocSurfaceMath.SurfaceWet(0.5, 1.0, 0.0, 0.9);
            Assert.Greater(low, high + 0.2, "up faces: low height is wetter");
            Assert.AreEqual(NocSurfaceMath.SurfaceWet(1.0, 0.0, 0.0, 0.1), NocSurfaceMath.SurfaceWet(1.0, 0.0, 0.0, 0.9), 1e-12, "vertical: no pooling");
            Assert.AreEqual(NocSurfaceConstants.SurfWetBase, NocSurfaceMath.SurfaceWet(1.0, 0.0, 0.0, 0.5), 1e-12, "vertical wet level 0.55 (LOOK_V2 5.2: ~0.6)");
        }

        [Test]
        public void Grating_MeanCoverage035_FadesToIt_SolidAtGrazing()
        {
            // the Grating row's bars (MaterialFactory.Surface): pitch 62.5 mm, widths 10 / 14 mm, depth 30 mm
            const double p = 0.0625, w0 = 0.010, w1 = 0.014, depth = 0.03;
            double mean = 1.0 - (1.0 - w0 / p) * (1.0 - w1 / p);
            Assert.AreEqual(0.35, mean, 0.005, "TEXTURE_PLAN 3.5: the mesh's mean coverage 0.35");
            // seen from above at 2 x 10^5 random points over 10 x 10 m: the area mean; at a footprint of 3 pitches: the mean exactly
            var rnd = new System.Random(17);
            double sum = 0; int n = 0;
            for (int i = 0; i < 200000; i++) { sum += NocSurfaceMath.GratingCover(10.0 * rnd.NextDouble(), 10.0 * rnd.NextDouble(), 0, 1, 0, 1e-4, p, w0, w1, depth); n++; }
            Assert.AreEqual(mean, sum / n, 0.01, "near: the area mean");
            Assert.AreEqual(mean, NocSurfaceMath.GratingCover(0.37, 1.91, 0, 1, 0, 3.0 * p, p, w0, w1, depth), 1e-9, "far: the mean, no tap");
            // grazing view (15 deg above the plate, along x): the bars' depth closes the gaps across x
            double el = 15.0 * Math.PI / 180.0, gsum = 0; n = 0;
            for (int i = 0; i < 200000; i++) { gsum += NocSurfaceMath.GratingCover(10.0 * rnd.NextDouble(), 10.0 * rnd.NextDouble(), Math.Cos(el), Math.Sin(el), 0, 1e-4, p, w0, w1, depth); n++; }
            Assert.Greater(gsum / n, 0.85, FormattableString.Invariant($"grazing coverage {gsum / n:F3}: reads solid from a low angle"));
            // continuous: no step above 0.6 between samples 0.1 mm apart at a 1 mm footprint (box filtered)
            double prev = NocSurfaceMath.GratingCover(0, 0.03, 0, 1, 0, 1e-3, p, w0, w1, depth), maxStep = 0;
            for (int i = 1; i < 2000; i++)
            {
                double c = NocSurfaceMath.GratingCover(i * 1e-4, 0.03, 0, 1, 0, 1e-3, p, w0, w1, depth);
                maxStep = Math.Max(maxStep, Math.Abs(c - prev)); prev = c;
            }
            Assert.Less(maxStep, 0.15);
        }

        [Test]
        public void G1Quad_MetricUV0_PanelSizeInUV1_NoFacadePart()
        {
            // TEXTURE_PLAN 8.3 SU tests: G1Mesh.Quad UV0 is metric (panel-local, u along the bottom edge, v up) with the panel size in
            // UV1, whatever the corner order; and no NocFacade part of beats 1-2 is built through it (FacadeKit owns those UVs)
            var mb = new MeshBuilder();
            G1Mesh.Quad(mb, new Vector3(4, 1, 7), new Vector3(4, 4, 7), new Vector3(6, 4, 7), new Vector3(6, 1, 7), Vector3.forward);
            var mesh = mb.ToMesh("quadTest");
            var uv0 = new List<Vector2>(); mesh.GetUVs(0, uv0);
            var uv1 = new List<Vector4>(); mesh.GetUVs(1, uv1);
            var v = mesh.vertices;
            for (int i = 0; i < v.Length; i++)
            {
                // facing +z, seen from outside the viewer's right is -x: u = 6 - x, v = y - 1
                Assert.AreEqual(6f - v[i].x, uv0[i].x, 1e-5f, "u along the bottom edge (m)");
                Assert.AreEqual(v[i].y - 1f, uv0[i].y, 1e-5f, "v up (m)");
                Assert.AreEqual(2f, uv1[i].x, 1e-5f); Assert.AreEqual(3f, uv1[i].y, 1e-5f);
            }
            Assert.Greater(Vector3.Dot(mesh.normals[0], Vector3.forward), 0.99f, "winding kept");
            G1Mesh.QuadBuilders = new HashSet<MeshBuilder>();
            try
            {
                foreach (var parts in new[] { Beat01_Alley.BuildParts(), Beat02_Boulevard.BuildParts() })
                    foreach (var part in parts.Parts)
                        if (part.Mat == G1Mat.Facade)
                            Assert.IsFalse(G1Mesh.QuadBuilders.Contains(part.Mb), part.Name + " (NocFacade) is built through G1Mesh.Quad");
                Assert.Greater(G1Mesh.QuadBuilders.Count, 10, "the quads were tracked");
            }
            finally { G1Mesh.QuadBuilders = null; }
        }

        [Test]
        public void LinePaint_KeepsItsMean_AndFades()
        {
            // a vertical post above the splash band, sampled densely: the mean is 1 + 0.6 chipP (chips lighten), the noise averages out
            double sum = 0; int n = 0;
            for (int p = 0; p < 40; p++)
                for (double y = 0.5; y < 3.0; y += 0.002) { sum += NocSurfaceMath.LinePaint(3.1 * p, y, -7.3 * p, 1e-4); n++; }
            double want = 1.0 + 0.6 * NocSurfaceConstants.SurfLineChipP;
            Assert.AreEqual(want, sum / n, 0.02, "near mean");
            // far away (footprint 1 m) every detail is at its mean
            Assert.AreEqual(want, NocSurfaceMath.LinePaint(1.3, 1.7, 2.9, 1.0), 1e-9, "far: the mean exactly");
            // the splash band darkens the foot
            Assert.AreEqual(want * NocSurfaceConstants.SurfLineSplashDark, NocSurfaceMath.LinePaint(1.3, 0.0, 2.9, 1.0), 1e-9);
        }
    }
}
