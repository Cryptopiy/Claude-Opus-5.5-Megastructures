// NocTexturingTests: the Look v2 sampling library through its C# twin NocTexturingMath (docs/TEXTURE_PLAN.md 8.3 TP-0 tests):
// hex weights sum to 1 and are continuous (max jump 1e-4 over 10^5 pairs); module- and panel-shuffle seams fall inside the joint
// band for every lattice (the TEXTURE_PLAN 1.3 lattices and every texmeta that declares a module); triplanar and UV0 surface-
// gradient normals match the analytic gradient within 1 deg; the facade frame sign sigma on synthetic faces of both
// orientations and on tilted normals; the float32 uv error at 2 km under 0.25 texel.
using System;
using System.Collections.Generic;
using System.IO;
using Megastructure.EditorTools;
using NUnit.Framework;
using static Megastructure.NocTexturingMath;

namespace Megastructure.Tests
{
    public class NocTexturingTests
    {
        [Test]
        public void HexWeights_SumToOne_AndAreContinuous()
        {
            var rnd = new Random(11);
            double maxJump = 0, maxGainJump = 0, maxSumErr = 0;
            const double d = 2e-7;                                  // pair separation in tiles
            for (int n = 0; n < 100000; n++)
            {
                double u = (rnd.NextDouble() - 0.5) * 400.0, v = (rnd.NextDouble() - 0.5) * 400.0;
                double ang = rnd.NextDouble() * 2 * Math.PI;
                double u2 = u + d * Math.Cos(ang), v2 = v + d * Math.Sin(ang);
                var a = HexWeights(u, v, NocTexConstants.TexHexRate);
                var b = HexWeights(u2, v2, NocTexConstants.TexHexRate);
                maxSumErr = Math.Max(maxSumErr, Math.Abs(a.w1 + a.w2 + a.w3 - 1.0));
                // compare per lattice vertex (a vertex missing from one side has weight 0 there)
                var ids = new[] { (a.v1x, a.v1y), (a.v2x, a.v2y), (a.v3x, a.v3y), (b.v1x, b.v1y), (b.v2x, b.v2y), (b.v3x, b.v3y) };
                foreach (var (x, y) in ids)
                    maxJump = Math.Max(maxJump, Math.Abs(HexWeightOf(a, x, y) - HexWeightOf(b, x, y)));
                // the colour weights after the S-curve (luminance weighting 1): still continuous
                double a1 = a.w1, a2 = a.w2, a3 = a.w3;
                HexGain(ref a1, ref a2, ref a3, NocTexConstants.TexHexGainColour);
                var ga = new HexTaps { v1x = a.v1x, v1y = a.v1y, v2x = a.v2x, v2y = a.v2y, v3x = a.v3x, v3y = a.v3y, w1 = a1, w2 = a2, w3 = a3 };
                double b1 = b.w1, b2 = b.w2, b3 = b.w3;
                HexGain(ref b1, ref b2, ref b3, NocTexConstants.TexHexGainColour);
                var gb = new HexTaps { v1x = b.v1x, v1y = b.v1y, v2x = b.v2x, v2y = b.v2y, v3x = b.v3x, v3y = b.v3y, w1 = b1, w2 = b2, w3 = b3 };
                foreach (var (x, y) in ids)
                    maxGainJump = Math.Max(maxGainJump, Math.Abs(HexWeightOf(ga, x, y) - HexWeightOf(gb, x, y)));
            }
            TestContext.WriteLine(FormattableString.Invariant($"hex: max |sum - 1| {maxSumErr:E2}, max jump {maxJump:E2}, after gain {maxGainJump:E2}"));
            Assert.Less(maxSumErr, 1e-9, "weights sum to 1");
            Assert.Less(maxJump, 1e-4, "hex weights continuous");
            Assert.Less(maxGainJump, 1e-4, "colour weights (S-curve) continuous");
        }

        [Test]
        public void HexTaps_RotateAboutTheirCentre_AndGradientsFollow()
        {
            // a tap coordinate is affine in uv with the rotation as its Jacobian: finite differences equal R
            var rnd = new Random(3);
            for (int n = 0; n < 2000; n++)
            {
                double u = rnd.NextDouble() * 50, v = rnd.NextDouble() * 50;
                var t = HexWeights(u, v, NocTexConstants.TexHexRate);
                HexTapUV(u, v, t.v1x, t.v1y, NocTexConstants.TexHexRate, 1.0, 77u, out double a0, out double b0);
                HexTapUV(u + 1e-4, v, t.v1x, t.v1y, NocTexConstants.TexHexRate, 1.0, 77u, out double a1, out double b1);
                HexTapUV(u, v + 1e-4, t.v1x, t.v1y, NocTexConstants.TexHexRate, 1.0, 77u, out double a2, out double b2);
                double j00 = (a1 - a0) / 1e-4, j01 = (b1 - b0) / 1e-4, j10 = (a2 - a0) / 1e-4, j11 = (b2 - b0) / 1e-4;
                Assert.AreEqual(1.0, j00 * j11 - j01 * j10, 1e-6, "rotation keeps area (det 1)");
                Assert.AreEqual(1.0, j00 * j00 + j01 * j01, 1e-6, "rotation keeps length");
            }
        }

        // TEXTURE_PLAN 1.3 lattices: (name, tile, module u, module v, running bond)
        static readonly (string name, double tile, double mu, double mv, bool bond)[] s_Lattices =
        {
            ("granite_tile", 2.3, 0.46, 0.46, false),
            ("concrete_tiles_02 (bond along v: axes swapped)", 1.83, 0.61, 0.305, true),
            ("large_floor_tiles_02", 3.0, 1.0, 1.0, false),
            ("long_white_tiles", 1.27, 0.0385, 0.127, false),
            ("brick_wall_08", 1.86, 0.2325, 0.093, true),
        };

        static IEnumerable<(string name, double tile, double mu, double mv, bool bond)> Lattices()
        {
            foreach (var l in s_Lattices) yield return l;
            // every processed texmeta that declares a module (TP-1 writes them)
            string dir = Path.Combine(Path.GetDirectoryName(UnityEngine.Application.dataPath), "Assets", "_Game", "Textures", "External");
            if (!Directory.Exists(dir)) yield break;
            foreach (var f in Directory.GetFiles(dir, "*.texmeta.json", SearchOption.AllDirectories))
            {
                var meta = TexMeta.Load(f);
                if (meta == null || meta.moduleU <= 0 || meta.moduleV <= 0) continue;
                // a half bond along v (concrete_tiles_02, bondAxis "v") is sampled with its axes swapped, as in the shaders
                if (meta.bondAxis == "v") yield return (Path.GetFileName(f) + " (bond along v: axes swapped)", meta.TileVOrU, meta.moduleV, meta.moduleU, meta.runningBond);
                else yield return (Path.GetFileName(f), meta.tile, meta.moduleU, meta.moduleV, meta.runningBond);
            }
        }

        [Test]
        public void ModuleShuffle_SeamsLieInTheJoints()
        {
            var rnd = new Random(5);
            foreach (var l in Lattices())
            {
                double cu = l.tile / l.mu, cv = l.tile / l.mv;
                Assert.AreEqual(Math.Round(cu), cu, 1e-3 * cu, l.name + ": whole modules along u");
                Assert.AreEqual(Math.Round(cv), cv, 1e-3 * cv, l.name + ": whole modules along v");
                if (l.bond) Assert.AreEqual(0, ((int)Math.Round(cv)) % 2, l.name + ": an even row count keeps the running bond");
                for (int n = 0; n < 4000; n++)
                {
                    double um = (rnd.NextDouble() - 0.5) * 400, vm = (rnd.NextDouble() - 0.5) * 400;
                    ModuleUV(um, vm, l.mu, l.mv, cu, cv, l.tile, l.bond, 13u, out double tu, out double tv, out int i, out int j, out int si, out int sj);
                    // a running-bond row is shifted by half a module: its last brick straddles the tile's wrap edge, so tu reaches
                    // 1 + 0.5 module / tile there (the Repeat sampler wraps it onto the same brick; the gradients stay continuous);
                    // an unregistered lattice (whole modules within 1e-3 only) also ends at round(cu) modules, not exactly at 1
                    double tuMax = (Math.Round(cu) * l.mu + (l.bond ? 0.5 * l.mu : 0.0)) / l.tile;
                    Assert.That(tu, Is.InRange(-1e-9, tuMax + 1e-9), l.name + " tu in [0, 1 + bond shift)");
                    Assert.That(tv, Is.InRange(-1e-9, 1.0 + 1e-9), l.name + " tv in [0, 1)");
                    int odd = l.bond ? ((j % 2) + 2) % 2 : 0;
                    Assert.AreEqual(odd, l.bond ? ((sj % 2) + 2) % 2 : 0, l.name + " row parity kept");
                    // the position inside the module is the same in the world and in the texture: a world module boundary (the
                    // middle of a joint, by texprep's registration) maps onto a texture module boundary
                    double fw = (um - 0.5 * l.mu * odd) / l.mu - Math.Floor((um - 0.5 * l.mu * odd) / l.mu);
                    double ft = (tu * l.tile - 0.5 * l.mu * odd) / l.mu - Math.Floor((tu * l.tile - 0.5 * l.mu * odd) / l.mu + 1e-12);
                    Assert.AreEqual(fw, ft, 1e-6, l.name + " in-module position (u)");
                    double gw = vm / l.mv - Math.Floor(vm / l.mv), gt = tv * l.tile / l.mv - Math.Floor(tv * l.tile / l.mv + 1e-12);
                    Assert.AreEqual(gw, gt, 1e-6, l.name + " in-module position (v)");
                }
            }
        }

        [Test]
        public void PanelShuffle_SeamLiesInsideThePanelJoint()
        {
            // the panel uv is continuous inside a panel (cell-local metres) and jumps only where the cell-local coordinate resets:
            // at the cell edge, the centre of the analytic 15 mm joint
            const double P = 2.4, joint = 0.015, tile = 2.0;
            var rnd = new Random(9);
            for (int n = 0; n < 20000; n++)
            {
                double u = (rnd.NextDouble() - 0.5) * 2000;
                double u2 = u + 1e-5;
                int i = (int)Math.Floor(u / P), i2 = (int)Math.Floor(u2 / P);
                PanelUV(u - i * P, 0.5, (uint)i, tile, out double a, out _);
                PanelUV(u2 - i2 * P, 0.5, (uint)i2, tile, out double b, out _);
                bool jump = Math.Abs(b - a) > 1e-4;
                double edge = Math.Abs(u - Math.Round(u / P) * P);
                if (jump) Assert.Less(edge, 0.5 * joint, "a panel seam outside the joint band at u " + u);
            }
            // per-panel tone: within +-12 %, faded to 1 under 2 px
            for (uint h = 0; h < 1000; h++)
            {
                Assert.That(PanelTone(h, 10), Is.InRange(1 - NocTexConstants.TexPanelTone, 1 + NocTexConstants.TexPanelTone));
                Assert.AreEqual(1.0, PanelTone(h, 1.5), 1e-12);
            }
        }

        // analytic per-plane height H_k(cu, cv) = A sin(2 pi cu / L + k) cos(2 pi cv / L2): slope (dH/dcu, dH/dcv)
        const double A = 0.012, L = 0.6, L2 = 0.45;
        static double Hk(int k, double cu, double cv) => A * Math.Sin(2 * Math.PI * cu / L + k) * Math.Cos(2 * Math.PI * cv / L2);
        static (double, double) Sk(int k, double cu, double cv) =>
            (A * 2 * Math.PI / L * Math.Cos(2 * Math.PI * cu / L + k) * Math.Cos(2 * Math.PI * cv / L2),
             -A * 2 * Math.PI / L2 * Math.Sin(2 * Math.PI * cu / L + k) * Math.Sin(2 * Math.PI * cv / L2));

        // the displaced surface S(a, b) = P0 + a T + b B + n h(S0) with h the weighted triplanar height: its normal by central differences
        static V3d DisplacedNormal(V3d p0, V3d n, V3d t, V3d b, Func<V3d, double> h)
        {
            const double e = 1e-5;
            V3d S(double a, double c) { V3d q = p0 + t * a + b * c; return q + n * h(q); }
            V3d da = (S(e, 0) - S(-e, 0)) * (0.5 / e), db = (S(0, e) - S(0, -e)) * (0.5 / e);
            V3d nn = Normalize(Cross(da, db));
            return Dot(nn, n) < 0 ? -nn : nn;
        }

        static void Basis(V3d n, out V3d t, out V3d b)
        {
            V3d r = Math.Abs(n.y) < 0.9 ? new V3d(0, 1, 0) : new V3d(1, 0, 0);
            t = Normalize(Cross(r, n));
            b = Cross(n, t);
        }

        [Test]
        public void Triplanar_SurfaceGradientNormal_MatchesTheAnalyticGradient()
        {
            var rnd = new Random(21);
            var normals = new List<V3d> { new V3d(1, 0, 0), new V3d(-1, 0, 0), new V3d(0, 1, 0), new V3d(0, -1, 0), new V3d(0, 0, 1), new V3d(0, 0, -1) };
            for (int k = 0; k < 40; k++) normals.Add(Normalize(new V3d(rnd.NextDouble() * 2 - 1, rnd.NextDouble() * 2 - 1, rnd.NextDouble() * 2 - 1)));
            double worst = 0;
            foreach (var n in normals)
            {
                V3d w = TriWeights(n);
                Basis(n, out V3d t, out V3d b);
                double H(V3d q) { double s = 0; for (int k = 0; k < 3; k++) { TriPlane(q, k, out double cu, out double cv); s += w[k] * Hk(k, cu, cv); } return s; }
                for (int s = 0; s < 50; s++)
                {
                    V3d p = new V3d(rnd.NextDouble() * 20, rnd.NextDouble() * 20, rnd.NextDouble() * 20);
                    V3d got = TriNormal(p, n, (k, cu, cv) => Sk(k, cu, cv));
                    V3d want = DisplacedNormal(p, n, t, b, H);
                    worst = Math.Max(worst, AngleDeg(got, want));
                }
            }
            TestContext.WriteLine(FormattableString.Invariant($"triplanar: worst normal error {worst:0.000} deg (slopes up to {A * 2 * Math.PI / L2:0.000})"));
            Assert.Less(worst, 1.0, "triplanar surface-gradient normal within 1 deg");
        }

        [Test]
        public void Uv0Frame_SurfaceGradientNormal_MatchesTheAnalyticGradient()
        {
            var rnd = new Random(23);
            double worst = 0, worstT = 0;
            for (int c = 0; c < 400; c++)
            {
                V3d n = Normalize(new V3d(rnd.NextDouble() * 2 - 1, rnd.NextDouble() * 2 - 1, rnd.NextDouble() * 2 - 1));
                Basis(n, out V3d e0, out V3d e1);
                double phi = rnd.NextDouble() * 2 * Math.PI;
                V3d a = e0 * Math.Cos(phi) + e1 * Math.Sin(phi);          // metric u direction
                V3d bdir = Cross(n, a);                                    // metric v direction (either handedness below)
                if (rnd.NextDouble() < 0.5) bdir = -bdir;
                // synthetic screen derivatives of P: two independent in-plane vectors, a few mm per pixel
                V3d dPdx = (e0 * (rnd.NextDouble() - 0.5) + e1 * (rnd.NextDouble() - 0.5)) * 0.004;
                V3d dPdy = (e0 * (rnd.NextDouble() - 0.5) + e1 * (rnd.NextDouble() - 0.5)) * 0.004;
                if (Math.Abs(Dot(Cross(dPdx, dPdy), n)) < 1e-7) continue;
                double dudx = Dot(dPdx, a), dvdx = Dot(dPdx, bdir), dudy = Dot(dPdy, a), dvdy = Dot(dPdy, bdir);
                UvFrame(n, dPdx, dPdy, dudx, dvdx, dudy, dvdy, out V3d T, out V3d B);
                worstT = Math.Max(worstT, Math.Max(AngleDeg(T, a), AngleDeg(B, bdir)));
                V3d p0 = new V3d(rnd.NextDouble() * 10, rnd.NextDouble() * 10, rnd.NextDouble() * 10);
                double h(V3d q) { double u = Dot(q - p0, a) + 3.1, v = Dot(q - p0, bdir) + 1.7; return Hk(0, u, v); }
                var s = Sk(0, 3.1, 1.7);
                V3d got = ResolveNormal(n, SurfGradTB(s.Item1, s.Item2, T, B));
                V3d want = DisplacedNormal(p0, n, a, bdir, h);
                worst = Math.Max(worst, AngleDeg(got, want));
            }
            TestContext.WriteLine(FormattableString.Invariant($"uv0 frame: worst T/B error {worstT:0.0000} deg, normal error {worst:0.000} deg"));
            Assert.Less(worstT, 0.01, "GenBasisTB recovers the metric u and v directions");
            Assert.Less(worst, 1.0, "UV0 surface-gradient normal within 1 deg");
        }

        static double Sigma(V3d n, V3d uDir, V3d dPdx, V3d dPdy)
        {
            FaceFrame(n, Dot(dPdx, uDir), Dot(dPdy, uDir), dPdx, dPdy, out V3d t, out V3d b, out double sigma);
            Assert.Greater(Dot(t, uDir), 0.0, "T points the way u grows");
            Assert.GreaterOrEqual(b.y, 0.0, "B points up");
            return sigma;
        }

        [Test]
        public void FaceFrame_SigmaOnBothOrientationsAndTiltedNormals()
        {
            var rnd = new Random(31);
            for (int c = 0; c < 200; c++)
            {
                // screen derivatives: any two independent in-plane steps (camera at any roll)
                V3d Rand(V3d e0, V3d e1) => (e0 * (rnd.NextDouble() - 0.5) + e1 * (rnd.NextDouble() - 0.5)) * 0.01;
                // face A of the Gulf: n = +x, u = ArcLength grows with z -> sigma -1
                V3d n = new V3d(1, 0, 0), up = new V3d(0, 1, 0), z = new V3d(0, 0, 1), x = new V3d(1, 0, 0);
                Assert.AreEqual(-1.0, Sigma(n, z, Rand(z, up), Rand(z, up)), "face A");
                // the z 275 notch wall: n = -z, u = x -> sigma -1
                n = new V3d(0, 0, -1);
                Assert.AreEqual(-1.0, Sigma(n, x, Rand(x, up), Rand(x, up)), "south notch wall");
                // a city face: n = +z, u = x -> +1; n = -z, u = -x -> +1; n = +x, u = -z -> +1
                Assert.AreEqual(1.0, Sigma(new V3d(0, 0, 1), x, Rand(x, up), Rand(x, up)), "city face +z");
                Assert.AreEqual(1.0, Sigma(new V3d(0, 0, -1), -x, Rand(x, up), Rand(x, up)), "city face -z");
                Assert.AreEqual(1.0, Sigma(new V3d(1, 0, 0), -z, Rand(z, up), Rand(z, up)), "city face +x");
                // tilted normals (tongue hull, spire): n = normalize(cos a, sin a, 0) rotated about y; u along the horizontal tangent or against it
                double tilt = (rnd.NextDouble() - 0.5) * 1.2, yaw = rnd.NextDouble() * 2 * Math.PI;
                V3d nh = new V3d(Math.Cos(yaw), 0, Math.Sin(yaw));
                V3d nt = Normalize(nh * Math.Cos(tilt) + up * Math.Sin(tilt));
                V3d t0 = Normalize(new V3d(nt.z, 0, -nt.x));
                V3d bt = Normalize(Cross(nt, t0));
                Assert.AreEqual(1.0, Sigma(nt, t0, Rand(t0, bt), Rand(t0, bt)), "tilted, u along T0");
                Assert.AreEqual(-1.0, Sigma(nt, -t0, Rand(t0, bt), Rand(t0, bt)), "tilted, u against T0 (the -z hull side)");
            }
            // edge-on (no u change across the pixel): +1
            FaceFrame(new V3d(1, 0, 0), 0, 0, new V3d(0, 0, 0), new V3d(0, 0, 0), out _, out _, out double s0);
            Assert.AreEqual(1.0, s0, "sub-pixel face");
        }

        [Test]
        public void Float32UvError_At2km_IsUnderAQuarterTexel()
        {
            // the plan's reference set (a 2 m tile at 2K) between 1.5 and 2 km from the origin (TEXTURE_PLAN 1.6 "Precision")
            var rnd = new Random(41);
            double worst = 0;
            for (int n = 0; n < 200000; n++)
            {
                double p = 1500.0 + 500.0 * rnd.NextDouble();
                worst = Math.Max(worst, Float32UvErrorTexels(p, 2.0, 2048));
            }
            TestContext.WriteLine(FormattableString.Invariant($"float32 uv error at 1.5-2 km, 2 m / 2048: {worst:0.000} texel"));
            Assert.Less(worst, 0.25);
        }
    }
}
