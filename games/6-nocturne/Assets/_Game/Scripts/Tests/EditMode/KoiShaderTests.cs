using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    /// <summary>
    /// The Look v2 koi (KO; TEXTURE_PLAN 6, LOOK_V2 6 "Climaxes", CRIT0 #9) on its C# twin KoiShaderMath and the KoiBuilder mesh:
    /// the shader's look #defines are the twin's constants; the pectoral and dorsal fin vertices are KoiMath.PectoralPoint /
    /// DorsalPoint within 5 cm at 60 poses and the barbels stay at the snout; the mesh validates within 30 k triangles and its rings
    /// close without a seam; the filtered scale composite and the caustic network have the means the shader assumes at every
    /// footprint; the Kohaku patches cover 15-25 % of the body, wrap >= 70 % of the circumference, and are 0 on the head; the
    /// structure factor has mean 0.775 (LOOK_V2: body about 170 at a vein mean of 0.775) and a relative std of >= 8 %; the eye
    /// spots are <= 0.5 x the body; the driver's patch saturation follows 1 - 0.75 smoothstep(1 440, 1 490, s); the front budget
    /// is the 1.5-2.0 clamp and the nominal core stays below it. Numbers are printed with [KOI-LOOK].
    /// </summary>
    public sealed class KoiShaderTests
    {
        static void Report(string line) { TestContext.Out.WriteLine("[KOI-LOOK] " + line); Debug.Log("[KOI-LOOK] " + line); }

        CultureInfo m_culture;
        [OneTimeSetUp] public void Invariant() { m_culture = CultureInfo.CurrentCulture; CultureInfo.CurrentCulture = CultureInfo.InvariantCulture; }
        [OneTimeTearDown] public void Restore() { if (m_culture != null) CultureInfo.CurrentCulture = m_culture; }

        static string s_src;
        static double Def(string n)
        {
            if (s_src == null) s_src = File.ReadAllText(Path.Combine(Application.dataPath, "_Game/Shaders/Beats/NocKoi.shader"));
            var m = Regex.Match(s_src, @"#define\s+" + n + @"\s+([-0-9.]+)");
            Assert.IsTrue(m.Success, "NocKoi defines " + n);
            return double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        [Test]
        public void ShaderLookConstants_AreTheTwins()
        {
            Assert.AreEqual(KoiShaderMath.ScaleColumns, Def("KOI_NC"), 1e-6);
            Assert.AreEqual(KoiShaderMath.RowAspect, Def("KOI_KH"), 1e-6);
            Assert.AreEqual(KoiShaderMath.ScaleRadius, Def("KOI_SR"), 1e-6);
            Assert.AreEqual(KoiShaderMath.RhoMin, Def("KOI_RHOMIN"), 1e-6);
            Assert.AreEqual(KoiShaderMath.HeadA, Def("KOI_HEADA"), 1e-6);
            Assert.AreEqual(KoiShaderMath.GillA, Def("KOI_GILLA"), 1e-6);
            Assert.AreEqual(KoiShaderMath.GillB, Def("KOI_GILLB"), 1e-6);
            Assert.AreEqual(KoiShaderMath.EyeA, Def("KOI_EYEA"), 1e-6);
            Assert.AreEqual(KoiShaderMath.EyeTheta, Def("KOI_EYET"), 1e-6);
            Assert.AreEqual(KoiShaderMath.EyeR, Def("KOI_EYER"), 1e-6);
            Assert.AreEqual(KoiMath.VeilA0, Def("KOI_VEILA0"), 1e-9);
            Assert.AreEqual(KoiMath.VeilLength, Def("KOI_VEILL"), 1e-9);
            Assert.AreEqual(KoiShaderMath.ScaleMean, Def("KOI_SCALEMEAN"), 1e-6);
            Assert.AreEqual(KoiShaderMath.ScaleMeanF0, Def("KOI_SMF0"), 1e-6);
            Assert.AreEqual(KoiShaderMath.ScaleMeanA, Def("KOI_SMA"), 1e-6);
            Assert.AreEqual(KoiShaderMath.ScaleMeanB, Def("KOI_SMB"), 1e-6);
            Assert.AreEqual(KoiShaderMath.CausticMean, Def("KOI_CAUSTMEAN"), 1e-6);
            Assert.AreEqual(KoiShaderMath.StructBase, Def("KOI_STRB"), 1e-6);
            Assert.AreEqual(KoiShaderMath.StructSwing, Def("KOI_STRK"), 1e-6);
            Assert.AreEqual(KoiShaderMath.VeinScale, Def("KOI_VEINS"), 1e-6);
            Assert.AreEqual(KoiShaderMath.CausticAmp, Def("KOI_CAUSTA"), 1e-6);
            Assert.AreEqual(KoiShaderMath.RimPower, Def("KOI_RIMP"), 1e-6);
            Assert.AreEqual(KoiShaderMath.TailFadeA0, Def("KOI_TAILA0"), 1e-6);
            Assert.AreEqual(KoiShaderMath.TailFadeA1, Def("KOI_TAILA1"), 1e-6);
            Assert.AreEqual(0.775, KoiShaderMath.StructBase + 0.5 * KoiShaderMath.StructSwing, 1e-6, "structure mean 0.775 at a vein mean of 0.5 (LOOK_V2 6)");
            Assert.AreEqual(0.12 * KoiMath.Length, KoiShaderMath.HeadA, 1e-4, "head mask 0.12 L");
        }

        [Test]
        public void FinsFollowKoiMath_AndBarbelsStayAtTheSnout()
        {
            var mb = KoiBuilder.Body();
            int pecO = 0, pecI = 0, dor = 0, bar = 0, pel = 0, anal = 0;
            double worstP = 0.0, worstD = 0.0, barMax = 0.0, worstK = 0.0, pecUV = 0.0;
            for (int ia = 0; ia < 12; ia++)
                for (int k = 0; k < 5; k++)
                {
                    double phi = (6.64 + 30.0 * ia + 3.3) * NocMath.Deg, tw = 4.0 * k + 0.37;
                    Vector4 prm = KoiShaderMath.Params(phi, tw);
                    D3 snout = KoiMath.SpinePoint(0.0, phi, tw);
                    for (int i = 0; i < mb.VertexCount; i++)
                    {
                        Vector4 uv2 = mb.UV2[i];
                        if (uv2.x > 0.5f || uv2.w < 1.5f) continue;
                        KoiShaderMath.Vertex(mb.UV0[i], mb.UV1[i], uv2, prm.x, prm.y, out Vector3 p, out _);
                        if (uv2.w < 3.5f)
                        {
                            int side = uv2.w < 2.5f ? 1 : -1;
                            D3 r = KoiMath.PectoralPoint(side, uv2.y, uv2.z, phi, tw);
                            worstP = Math.Max(worstP, (KoiMath.ToVector3(r) - p).magnitude);
                            // inside KoiMath's sampled (u, v) square, so the composition samples bound the fin
                            pecUV = Math.Max(pecUV, Math.Max(Math.Max(-uv2.y, uv2.y - 1.0), Math.Max(-uv2.z, uv2.z - 1.0)));
                            if (ia == 0 && k == 0) { if (side > 0) pecO++; else pecI++; }
                        }
                        else if (uv2.w < 4.5f)
                        {
                            D3 r = KoiMath.DorsalPoint(uv2.y, uv2.z, phi, tw);
                            worstD = Math.Max(worstD, (KoiMath.ToVector3(r) - p).magnitude);
                            if (ia == 0 && k == 0) dor++;
                        }
                        else if (uv2.w < 5.5f)
                        {
                            barMax = Math.Max(barMax, (KoiMath.ToVector3(snout) - p).magnitude);
                            if (ia == 0 && k == 0) bar++;
                        }
                        else
                        {
                            double a, fF, fN, fB;
                            if (uv2.w < 7.5f) KoiShaderMath.PelvicOffset(uv2.w < 6.5f ? 1 : -1, uv2.y, uv2.z, out a, out fF, out fN, out fB);
                            else KoiShaderMath.AnalOffset(uv2.y, uv2.z, out a, out fF, out fN, out fB);
                            D3 r = KoiShaderMath.FramePoint(a, fF, fN, fB, phi, tw);
                            worstK = Math.Max(worstK, (KoiMath.ToVector3(r) - p).magnitude);
                            if (ia == 0 && k == 0) { if (uv2.w < 7.5f) pel++; else anal++; }
                        }
                    }
                }
            Report($"fins vs KoiMath over 60 poses: pectoral {worstP * 100:F2} cm ({pecO} + {pecI} vertices, (u, v) overshoot {pecUV:E1}), dorsal {worstD * 100:F2} cm ({dor}); pelvic + anal vs their frame offsets {worstK * 100:F2} cm ({pel} + {anal}); barbels {bar} vertices within {barMax:F1} m of the snout station");
            Assert.Greater(pecO, 0); Assert.Greater(pecI, 0); Assert.Greater(dor, 0);
            Assert.AreEqual(2 * (KoiBuilder.PelvicU + 1) * (KoiBuilder.PelvicV + 1), pel, "two pelvic fans");
            Assert.AreEqual((KoiBuilder.AnalU + 1) * (KoiBuilder.AnalV + 1), anal, "one anal fin");
            Assert.Less(worstK, 0.05);
            Assert.LessOrEqual(pecUV, 1e-6, "pectoral fans stay inside KoiMath's (u, v) square");
            Assert.AreEqual(4 * (KoiBuilder.BarbelT + 1) * KoiBuilder.BarbelSides, bar, "four barbel tubes");
            Assert.Less(worstP, 0.05);
            Assert.Less(worstD, 0.05);
            Assert.Less(barMax, 45.0, "barbels stay at the mouth");
        }

        [Test]
        public void Mesh_ValidatesWithin30k_AndRingsCloseWithoutASeam()
        {
            var mb = KoiBuilder.Body();
            Mesh mesh = KoiBuilder.BuildMesh();
            var r = MeshValidation.Validate(mesh);
            Assert.IsTrue(r.Ok, r.ToString());
            long tris = WorldContext.TriangleCount(mesh);
            Report($"koi mesh {mesh.vertexCount} vertices, {tris} triangles (budget 30 k)");
            Assert.LessOrEqual(tris, 30000);
            int rv = KoiMath.RingVertices + 1;
            Vector4 prm = KoiShaderMath.Params(1.3, 7.7);
            double worst = 0.0;
            for (int i = 0; i <= KoiMath.BodySegments; i++)
            {
                int j0 = i * rv, j1 = j0 + KoiMath.RingVertices;
                Assert.AreEqual(0.0, mb.UV0[j0].y, 1e-6);
                Assert.AreEqual(2.0 * Math.PI, mb.UV0[j1].y, 1e-5);
                KoiShaderMath.Vertex(mb.UV0[j0], mb.UV1[j0], mb.UV2[j0], prm.x, prm.y, out Vector3 p0, out _);
                KoiShaderMath.Vertex(mb.UV0[j1], mb.UV1[j1], mb.UV2[j1], prm.x, prm.y, out Vector3 p1, out _);
                worst = Math.Max(worst, (p1 - p0).magnitude);
                Assert.AreEqual(mb.UV2[j0].y, mb.UV2[j1].y, 1e-6, "scale row");
            }
            Assert.Less(worst, 0.005, "seam vertices coincide");
            // the scale rows grow from the head, one per 0.6 column widths
            double rows = KoiShaderMath.ScaleRow(KoiMath.Length);
            Report($"scale rows head to tail {rows:F1}; row spacing {KoiShaderMath.RowSpacing(270.0):F2} m at the widest ring, {KoiShaderMath.RowSpacing(KoiMath.Length):F2} m at the tail");
            Assert.That(rows, Is.InRange(120.0, 180.0));
            UnityEngine.Object.DestroyImmediate(mesh);
        }

        [Test]
        public void ScaleAndCausticMeans_AreTheShadersAtEveryFootprint()
        {
            var rnd = new System.Random(1234);
            // every footprint at which the scales still show (the fade ends at a 2.5 px pitch, fw 0.4 columns): the shader
            // subtracts ScaleMeanAt(fw), so the vein mean (and the displayed body) does not change with distance. Sampled over
            // 2 000 x 2 000 cells so the per-scale +-12 % brightness averages out (over 100 x 100 its draw alone moves the mean
            // by -0.0015)
            foreach (float fw in new[] { 0.005f, 0.02f, 0.05f, 0.1f, 0.15f, 0.2f, 0.3f, 0.4f })
            {
                double sum = 0.0, sq = 0.0; int n = 200000;
                for (int i = 0; i < n; i++)
                {
                    float x = (float)(rnd.NextDouble() * 2000.0), y = (float)(rnd.NextDouble() * 2000.0);
                    double s = KoiShaderMath.Scales(x, y, fw, 300f, 10f, 0f, out _);
                    sum += s; sq += s * s;
                }
                double m = sum / n, sd = Math.Sqrt(Math.Max(0.0, sq / n - m * m));
                double fit = KoiShaderMath.ScaleMeanAt(fw);
                Report($"scale shade at footprint {fw:F3} columns: mean {m:F4} (fit {fit:F4}, define {KoiShaderMath.ScaleMean}), std {sd:F3}");
                Assert.AreEqual(fit, m, 0.0015, "the vein subtracts the composite's mean at this footprint (no energy change as it fades)");
                if (fw <= 0.08f) Assert.AreEqual(KoiShaderMath.ScaleMean, m, 0.0015, "sharp scales have the defined mean");
            }
            {
                double sum = 0.0; int n = 400000;
                for (int i = 0; i < n; i++)
                    sum += KoiShaderMath.Caustic((float)(rnd.NextDouble() * 5000.0), (float)(rnd.NextDouble() * 250.0), (float)rnd.NextDouble());
                Report($"caustic mean {sum / n:F4} (define {KoiShaderMath.CausticMean})");
                Assert.AreEqual(KoiShaderMath.CausticMean, sum / n, 0.004);
            }
        }

        [Test]
        public void BodyPattern_ClosesAtTheRingSeam()
        {
            // theta runs 0 .. 2 pi on the body rings (the seam lies on the outer flank, theta 0): scales, patches, caustic and
            // eye must agree on both sides of it
            double worst = 0.0, worstC = 0.0;
            for (int i = 0; i < 400; i++)
            {
                float a = 2f + 896f * i / 399f, rad = (float)KoiMath.Radius(a), sv = (float)KoiShaderMath.ScaleRow(a);
                float ph = (i % 7) / 7f;
                float c0 = KoiShaderMath.Caustic(a, 0f, ph), c1 = KoiShaderMath.Caustic(a, 40f * 2f * MathF.PI, ph);
                worstC = Math.Max(worstC, Math.Abs(c1 - c0));
                foreach (float fw in new[] { 0.01f, 0.1f })
                {
                    float f0 = KoiShaderMath.BodyStructure(a, 0f, sv, rad, fw, fw, 0.2f, c0, out float p0, out float e0) * e0;
                    float f1 = KoiShaderMath.BodyStructure(a, 2f * MathF.PI, sv, rad, fw, fw, 0.2f, c1, out float p1, out float e1) * e1;
                    worst = Math.Max(worst, Math.Max(Math.Abs(f1 - f0), Math.Abs(p1 - p0)));
                }
            }
            Report($"ring seam: caustic jump {worstC:E1}, structure / patch jump {worst:E1}");
            Assert.Less(worstC, 1e-3, "the caustic network closes at theta 2 pi");
            Assert.Less(worst, 2e-3, "the body pattern closes at theta 2 pi");
        }

        [Test]
        public void BackShell_FindsTheFrontShellOnItsViewRay()
        {
            // a back-shell fragment traces its view ray through the local cylinder to the front shell (the patch tint of the
            // light passing the front pigment): the hit lies on the cylinder, on the ray, faces the camera; seen from straight
            // below the top's front point is the belly, seen from the outer side the inner flank's front point is the outer flank
            var rnd = new System.Random(9);
            double worstR = 0.0, worstRay = 0.0; int n = 0, facing = 0;
            for (int i = 0; i < 4000; i++)
            {
                double yaw = rnd.NextDouble() * 2.0 * Math.PI, pitch = (rnd.NextDouble() - 0.5) * 0.3;
                var f = new Vector3((float)(Math.Cos(yaw) * Math.Cos(pitch)), (float)Math.Sin(pitch), (float)(Math.Sin(yaw) * Math.Cos(pitch)));
                Vector3 nn = Vector3.Cross(Vector3.up, f).normalized, b = Vector3.Cross(f, nn);
                float rad = 10f + 40f * (float)rnd.NextDouble(), thB = (float)(rnd.NextDouble() * 2.0 * Math.PI);
                Vector3 nOut = MathF.Cos(thB) * nn + MathF.Sin(thB) * b;
                var vdir = new Vector3((float)(rnd.NextDouble() * 2 - 1), (float)(rnd.NextDouble() * 2 - 1), (float)(rnd.NextDouble() * 2 - 1)).normalized;
                if (Vector3.Dot(nOut, vdir) > -0.05f) continue;                 // a back-shell fragment faces away from the camera
                KoiShaderMath.FrontHit(300f, rad, nOut, f, vdir, out _, out float stF, out Vector3 df);
                Vector3 vp = -vdir - Vector3.Dot(-vdir, f) * f;
                worstR = Math.Max(worstR, Math.Abs(df.magnitude - rad) / rad);
                worstRay = Math.Max(worstRay, Vector3.Cross(df - rad * nOut, vp.normalized).magnitude / rad);
                if (Vector3.Dot(df, vdir) >= -1e-3f * rad) facing++;
                n++;
            }
            Report($"back shell -> front shell over {n} rays: radius error {worstR:E1}, off-ray {worstRay:E1}, front point faces the camera {facing} / {n}");
            Assert.Less(worstR, 1e-3); Assert.Less(worstRay, 1e-3); Assert.AreEqual(n, facing);
            var fx = new Vector3(1f, 0f, 0f);
            Vector3 nx = Vector3.Cross(Vector3.up, fx).normalized, bx = Vector3.Cross(fx, nx);
            KoiShaderMath.FrontHit(400f, 30f, bx, fx, -Vector3.up, out float aBelow, out float stBelow, out _);
            Assert.AreEqual(-1.0, stBelow, 1e-4, "from straight below the top's front point is the belly");
            Assert.AreEqual(400.0, aBelow, 1e-3);
            KoiShaderMath.FrontHit(400f, 30f, -nx, fx, nx, out _, out float stSide, out Vector3 dSide);
            Assert.AreEqual(0.0, stSide, 1e-4); Assert.Greater(Vector3.Dot(dSide, nx), 29.9f, "side view: inner flank -> outer flank");
        }

        [Test]
        public void KohakuPatches_Cover15to25Percent_WrapTheBody_AndSpareTheHead()
        {
            double area = 0.0, patch = 0.0;
            int na = 1800, nt = 360;
            for (int i = 0; i < na; i++)
            {
                double a = KoiMath.Length * (i + 0.5) / na, w = KoiMath.Radius(a);
                for (int j = 0; j < nt; j++)
                {
                    double th = 2.0 * Math.PI * (j + 0.5) / nt;
                    area += w;
                    patch += w * KoiShaderMath.Patch((float)a, (float)Math.Sin(th));
                }
            }
            double share = patch / area;
            double headMax = 0.0;
            for (int i = 0; i <= 200; i++)
                for (int j = 0; j < 72; j++)
                    headMax = Math.Max(headMax, KoiShaderMath.Patch((float)(KoiShaderMath.HeadA - 6.0) * i / 200f, (float)Math.Sin(j * 5.0 * NocMath.Deg)));
            string wraps = "";
            foreach (double c in new[] { 194.0, 356.0, 492.0 })
            {
                int inside = 0;
                for (int j = 0; j < 360; j++) if (KoiShaderMath.Patch((float)c, (float)Math.Sin(j * NocMath.Deg)) > 0.5f) inside++;
                wraps += $" {c:F0} m: {inside / 360.0:P0}";
                Assert.GreaterOrEqual(inside / 360.0, 0.70, $"the patch at a {c} wraps >= 70 % of the circumference (both shells agree)");
            }
            Report($"Kohaku patch share of the body {share:P1} (15-25 %); wrap{wraps}; head max {headMax:F4}");
            Assert.That(share, Is.InRange(0.15, 0.25));
            Assert.AreEqual(0.0, headMax, 1e-6, "no patch on the head (a < 0.12 L)");
        }

        [Test]
        public void Structure_HasTheLadderMean_AndReadableContrast()
        {
            var rnd = new System.Random(77);
            double sum = 0.0, sq = 0.0; int n = 0;
            for (int i = 0; i < 120000; i++)
            {
                float a = (float)(120.0 + rnd.NextDouble() * 740.0), th = (float)(rnd.NextDouble() * 2.0 * Math.PI);
                float sv = (float)KoiShaderMath.ScaleRow(a), rad = (float)KoiMath.Radius(a);
                float caust = KoiShaderMath.Caustic(a, th * 40f, 0.3f);
                float f = KoiShaderMath.BodyStructure(a, th, sv, rad, 0.01f, 0.01f, 0.2f, caust, out _, out _);
                f /= 0.88f + 0.12f * (float)Math.Sin(th);
                sum += f; sq += f * f; n++;
            }
            double m = sum / n, sd = Math.Sqrt(Math.Max(0.0, sq / n - m * m));
            Report($"structure factor {KoiShaderMath.StructBase} + {KoiShaderMath.StructSwing} vein: mean {m:F3} (LOOK_V2 0.775), std {sd:F3} ({sd / m:P1} of the mean)");
            Assert.AreEqual(0.775, m, 0.03);
            Assert.GreaterOrEqual(sd / m, 0.15, "scales, veins and caustics carry >= 15 % (>= 8 codes after the shells, the rim and a 15 px high-pass)");
        }

        [Test]
        public void EyeSpots_AreAtMostHalfTheBody()
        {
            float a = KoiShaderMath.EyeA, rad = (float)KoiMath.Radius(a), sv = (float)KoiShaderMath.ScaleRow(a);
            float cm = KoiShaderMath.CausticMean;
            float fe = KoiShaderMath.BodyStructure(a, KoiShaderMath.EyeTheta, sv, rad, 0.01f, 0.01f, 0.3f, cm, out _, out float eye);
            double ring = 0.0; int n = 0;
            for (int j = 0; j < 36; j++)
            {
                float th = KoiShaderMath.EyeTheta + 0.6f * (j - 18) / 18f;
                float aa = a + 14f;
                ring += KoiShaderMath.BodyStructure(aa, th, (float)KoiShaderMath.ScaleRow(aa), (float)KoiMath.Radius(aa), 0.01f, 0.01f, 0.3f, cm, out _, out float e2) * e2;
                n++;
            }
            double ratio = fe * eye / (ring / n);
            Report($"eye spot {fe * eye:F3} vs head {ring / n:F3}: ratio {ratio:F2} (<= 0.5); eye multiplier at the centre {eye:F2}");
            Assert.LessOrEqual(ratio, 0.5);
            // far away (footprint >= 2.5 x the eye radius) the eye fades out instead of aliasing
            KoiShaderMath.BodyStructure(a, KoiShaderMath.EyeTheta, sv, rad, 0.01f, 0.01f, 3f * KoiShaderMath.EyeR, cm, out _, out float far);
            Assert.AreEqual(1.0, far, 1e-4);
        }

        [Test]
        public void DriverUniforms_FadeThePatchesBeforeTheWindow()
        {
            Assert.AreEqual(1.0, KoiShaderMath.PatchSaturation(1300.0), 1e-12);
            Assert.AreEqual(1.0, KoiShaderMath.PatchSaturation(1440.0), 1e-12);
            Assert.AreEqual(0.625, KoiShaderMath.PatchSaturation(1465.0), 1e-9);
            Assert.AreEqual(0.25, KoiShaderMath.PatchSaturation(1490.0), 1e-12);
            Assert.AreEqual(0.25, KoiShaderMath.PatchSaturation(1500.0), 1e-12);
            double prev = 2.0;
            for (double s = 1400.0; s <= 1500.0; s += 0.5) { double k = KoiShaderMath.PatchSaturation(s); Assert.LessOrEqual(k, prev + 1e-12); prev = k; }
            for (double tw = -200.0; tw < 400.0; tw += 3.7)
            {
                Vector4 p = KoiShaderMath.Params(1.0, tw, 1300.0), t = KoiShaderMath.TimeParams(tw);
                Assert.That(p.y, Is.InRange(0f, 1f)); Assert.That(p.w, Is.InRange(0f, 1f));
                Assert.That(t.x, Is.InRange(0f, 1f)); Assert.That(t.y, Is.InRange(0f, 1f));
            }
            bool cap = NocState.Capture;
            try
            {
                NocState.Capture = true;
                var d = new KoiDriver(null);
                d.OnFrame(new FrameState { s = 1500.0 });
                Assert.AreEqual(0.25, KoiShaderMath.Params(d.HeadAzimuth, d.WaveTime, 1500.0).z, 1e-6);
            }
            finally { NocState.Capture = cap; }
        }

        [Test]
        public void Budgets_ClampTheRimNotTheCore()
        {
            Color body = NocturnePalette.Linear(NocPal.KoiBody), glow = NocturnePalette.Linear(NocPal.KoiGlow);
            double g = NocturnePalette.EmissionKoiBody, rg = NocturnePalette.EmissionKoiRim;
            Assert.That(KoiShaderMath.FrontBudget, Is.InRange(1.5f, 2.0f), "TEXTURE_PLAN 6 body clamp 1.5-2.0");
            // nominal core: vein 0.5, scanline crest, facing the camera (no rim)
            double core = g * 0.775 * 1.1 * Math.Max(body.r, Math.Max(body.g, body.b));
            // worst silhouette: brightest structure and the full rim
            double sil = g * 1.0 * 1.1 * Math.Max(body.r, Math.Max(body.g, body.b)) + rg * Math.Max(glow.r, Math.Max(glow.g, glow.b));
            Report($"body core max channel {core:F2}, silhouette {sil:F2} -> clamp {KoiShaderMath.FrontBudget} (gains {g} / {rg})");
            Assert.Less(core, KoiShaderMath.FrontBudget, "the body core is never clamped");
            Assert.LessOrEqual(KoiShaderMath.FinBudget, KoiShaderMath.FrontBudget);
            Assert.LessOrEqual(KoiShaderMath.VeilBudget, KoiShaderMath.FinBudget);
        }
    }
}
