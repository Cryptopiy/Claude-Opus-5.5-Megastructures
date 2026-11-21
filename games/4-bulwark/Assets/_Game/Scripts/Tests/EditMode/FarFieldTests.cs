// FarFieldTests: EditMode tests of the analytic far field (MASTER_PROMPT 14.7 H4, 14.10.1 T-NUM, 14.10.2 key-shot rows,
// Part 10 #13): the dips from the crest and the ledge (sin^2(dip) in double on the CPU, the _BwFarCam path), the
// no-cancellation ray-sphere hit in float32 against double, the horizon rows of KS04 / KS05 / KS20 and the KS20 sun pixel
// through the real camera model, the sea horizon agreeing with the fog, and the WorldSky builder contract (type name the
// graybox looks up, shader name, the far-field dome bounds).
using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class FarFieldTests
    {
        const double R = BwCurvature.EarthR;

        // ============================================================================ dips (T-NUM)
        [Test]
        public void Dips_CrestLedgeAndSeaFromTheDoubleCpuPath()
        {
            Assert.AreEqual(BulwarkConst.CrestDipDeg, DipDeg(AtmosphereDriver.FarCam(1.7).y), 0.05, "crest dip 2.486");
            Assert.AreEqual(BulwarkConst.LedgeDipDeg, DipDeg(AtmosphereDriver.FarCam(-88.3).y), 0.05, "ledge dip 2.467");
            Assert.AreEqual(0.100, DipDeg(AtmosphereDriver.FarCam(1.7).z), 0.05, "sea dip 0.100 from the crest");
            Assert.AreEqual(2.486, BwCurvature.HorizonDipD(6001.7) / BwMath.Deg, 0.0005);
            Assert.AreEqual(2.467, BwCurvature.HorizonDipD(5911.7) / BwMath.Deg, 0.0005);
            Assert.Less(AtmosphereDriver.FarCam(-88.3).z, 0f, "the ledge eye is below the sea: no sea sphere from there");
            Assert.Less(AtmosphereDriver.FarCam(-88.3).w, 0f, "the ledge eye is below the crest sphere");
            Assert.Greater(AtmosphereDriver.FarCam(1.7).w, 0f, "the crest eye is above the crest sphere");
        }

        static double DipDeg(float sin2) => Math.Asin(Math.Sqrt(sin2)) / BwMath.Deg;

        // ============================================================================ stable intersection (H4, T-NUM)
        [Test]
        public void RaySphere_Float32MatchesDoubleAndBeatsTheNaiveForm()
        {
            foreach (double eyeY in new[] { 1.7, -88.3 })
            {
                double h = BulwarkConst.AltitudeOffset + eyeY;
                float s2f = AtmosphereDriver.FarCam(eyeY).y;
                float rEye = (float)(R + h);
                double sd = Math.Sqrt((double)s2f);
                double worstT = 0, worstDisc = 0, worstNaive = 0;
                for (int i = 0; i <= 4000; i++)
                {
                    float dy = (float)-(sd - 1e-3 + (1.0 - (sd - 1e-3)) * i / 4000.0);
                    // discriminant in the factored form: float32 vs the double evaluation of the same float inputs
                    float sdf = (float)Math.Sqrt(s2f);
                    float discF = (-dy - sdf) * (-dy + sdf);
                    double discD = (-(double)dy - sd) * (-(double)dy + sd);
                    if (Math.Abs(-(double)dy - sd) > 2e-4) worstDisc = Math.Max(worstDisc, Math.Abs(discF - discD) / Math.Abs(discD));
                    double tD = AtmoRef(R + h, s2f, dy);
                    if (tD <= 0) continue;
                    float tF = BwCurvature.RaySphereNearF(rEye, s2f, dy);
                    Assert.Greater(tF, 0f, "hit at dy " + dy);
                    worstT = Math.Max(worstT, Math.Abs(tF - tD) / tD);
                    float naive = rEye * (-dy - (float)Math.Sqrt(Math.Max(0f, discF)));
                    worstNaive = Math.Max(worstNaive, Math.Abs(naive - tD) / tD);
                }
                Assert.Less(worstDisc, 1e-5, "discriminant relative error (eye y " + eyeY + ")");
                Assert.Less(worstT, 1e-5, "hit distance relative error (eye y " + eyeY + ")");
                Assert.Greater(worstNaive, worstT, "the naive R(-dy - sqrt) form loses digits");
            }
        }

        static double AtmoRef(double rEye, double s2, double dy)
        {
            double sd = Math.Sqrt(s2);
            if (-dy <= sd) return -1;
            double disc = (-dy - sd) * (-dy + sd);
            return rEye * s2 / (-dy + Math.Sqrt(disc));
        }

        [Test]
        public void RaySphere_HorizonDistanceAndNadir()
        {
            double h = 6001.7, s2 = BwCurvature.Sin2Dip(h);
            double tH = (R + h) * Math.Sqrt(s2);
            Assert.AreEqual(Math.Sqrt(h * (2 * R + h)), tH, 1e-6, "tangent distance");
            Assert.AreEqual(276.6e3, tH, 500.0);
            Assert.AreEqual(h, BwAtmosphereMath.RaySphereNear(R + h, s2, -1.0), 1e-6, "straight down hits at the eye altitude");
            Assert.AreEqual(0.0, BwAtmosphereMath.AltAt(h, -1.0, h), 1e-6);
            // a ray hitting the lowland at arc distance D comes back at D (GroundRay geometry)
            BwAtmosphereMath.GroundRay(h, 50e3, out D3 dir, out double t);
            Assert.AreEqual(t, BwAtmosphereMath.RaySphereNear(R + h, s2, dir.y), 1e-3 * t);
        }

        // ============================================================================ key-shot rows (14.10.2) through the camera
        [Test]
        public void KeyShots_HorizonRowsAndTheKs20Sun()
        {
            // KS05: lowland horizon (dip 2.486) at row 180 +-12 of 2160 at yaw -75 pitch -30 vFOV 64
            Assert.AreEqual(180.0, Row(-75, -DipDeg(AtmosphereDriver.FarCam(1.7).y), -75, -30, 64), 12.0, "KS05 horizon row");
            // KS04: sea horizon (0.100) at row 840 +-8 at yaw 80 pitch -8 vFOV 64
            Assert.AreEqual(840.0, Row(80, -DipDeg(AtmosphereDriver.FarCam(1.7).z), 80, -8, 64), 8.0, "KS04 sea horizon row");
            // KS20: lowland horizon (dip 2.467) at row 948 +-8 at yaw -27.5 pitch -7 vFOV 66
            Assert.AreEqual(948.0, Row(-27.5, -DipDeg(AtmosphereDriver.FarCam(-88.3).y), -27.5, -7, 66), 8.0, "KS20 horizon row");
            // KS20 sun disc at (1050, 918) of 3840 x 2160, 16 px across, inside 16:9, 16:10 and 21:9 with a 3 % margin
            BulwarkSchedule.Sun(1452.0, out double az, out double el);
            Project(BwAtmosphereMath.Dir(az, el), -27.5, -7, 66, 3840, 2160, out double x, out double y);
            Assert.AreEqual(1050.0, x, 6.0, "KS20 sun column");
            Assert.AreEqual(918.0, y, 6.0, "KS20 sun row");
            double f = 1080.0 / Math.Tan(33.0 * BwMath.Deg);
            Assert.AreEqual(16.0, 2.0 * BwAtmosphereMath.SunDiscRadiusRad * f, 1.0, "sun disc diameter (px at 4K)");
            foreach (var res in new[] { (1920, 1080), (1680, 1050), (2560, 1080) })
            {
                Project(BwAtmosphereMath.Dir(az, el), -27.5, -7, 66, res.Item1, res.Item2, out double sx, out double sy);
                Assert.Greater(sx, 0.03 * res.Item1); Assert.Less(sx, 0.97 * res.Item1);
                Assert.Greater(sy, 0.03 * res.Item2); Assert.Less(sy, 0.97 * res.Item2);
            }
        }

        static double Row(double az, double el, double yaw, double pitch, double vfov)
        {
            Project(BwAtmosphereMath.Dir(az, el), yaw, pitch, vfov, 3840, 2160, out _, out double y);
            return y;
        }

        /// <summary>Pin-hole projection of a world direction through a yaw / pitch camera (Hor+, vFOV), pixel rows down.</summary>
        internal static void Project(D3 d, double yawDeg, double pitchDeg, double vfov, int w, int h, out double px, out double py)
        {
            double ya = yawDeg * BwMath.Deg, pa = pitchDeg * BwMath.Deg;
            D3 fwd = new D3(Math.Sin(ya) * Math.Cos(pa), Math.Sin(pa), Math.Cos(ya) * Math.Cos(pa));
            D3 right = new D3(Math.Cos(ya), 0.0, -Math.Sin(ya));
            D3 up = new D3(-Math.Sin(ya) * Math.Sin(pa), Math.Cos(pa), -Math.Cos(ya) * Math.Sin(pa));
            double f = 0.5 * h / Math.Tan(0.5 * vfov * BwMath.Deg);
            double z = D3.Dot(d, fwd);
            px = 0.5 * w + f * D3.Dot(d, right) / z;
            py = 0.5 * h - f * D3.Dot(d, up) / z;
        }

        // ============================================================================ sea horizon = fog (Part 10 #13, H1)
        [Test]
        public void SeaHorizon_AgreesWithTheSkyAboveIt()
        {
            foreach (double s in new[] { 160.0, 300.0, 810.0 })
            {
                var g = AtmosphereTests.Globals(s, 1.7, s);
                var p = BulwarkSchedule.Evaluate(s);
                double dip = DipDeg((float)g.Sin2Sea);
                for (int az = 30; az <= 150; az += 30)
                {
                    D3 above = BwAtmosphereMath.Dir(az, -dip + 0.02), below = BwAtmosphereMath.Dir(az, -dip - 0.02);
                    D3 sky = BwAtmosphereMath.SkyVisible(in g, above);
                    double t = BwAtmosphereMath.RaySphereNear(R + g.EyeH, g.Sin2Sea, below.y);
                    Assert.Greater(t, 0.0);
                    // far-sea surface: Fresnel mix of the body colour and the visible sky in the reflected direction (BwFarField FFSea)
                    D3 n = new D3(below.x * t, below.y * t + R + g.EyeH, below.z * t).Normalized;
                    double mu = Math.Max(-D3.Dot(below, n), 1e-4);
                    double F = 0.02 + 0.98 * Math.Pow(1.0 - mu, 5.0);
                    D3 r = below - n * (2.0 * D3.Dot(below, n));
                    r = new D3(r.x, Math.Max(r.y, 1e-3), r.z);
                    D3 body = AtmoGlobals.V(p.SeaBody);
                    D3 sea = body + (BwAtmosphereMath.SkyVisible(in g, r) - body) * F;
                    D3 seen = BwAtmosphereMath.FogApplyFar(in g, sea, below, t);
                    Assert.Less(DeltaE(sky, seen, BulwarkSchedule.Ev(s)), 2.0, "s " + s + " az " + az + ": sea horizon dE");
                }
            }
        }

        /// <summary>CIE76 dE of two linear radiances after the exposure (identity tone curve below 0.76), in sRGB / Lab.</summary>
        internal static double DeltaE(D3 a, D3 b, double ev)
        {
            double k = Math.Pow(2.0, ev);
            Lab(a * k, out double l1, out double a1, out double b1);
            Lab(b * k, out double l2, out double a2, out double b2);
            return Math.Sqrt((l1 - l2) * (l1 - l2) + (a1 - a2) * (a1 - a2) + (b1 - b2) * (b1 - b2));
        }

        static void Lab(D3 c, out double L, out double A, out double B)
        {
            double r = Math.Min(Math.Max(c.x, 0), 1), g = Math.Min(Math.Max(c.y, 0), 1), b = Math.Min(Math.Max(c.z, 0), 1);
            double X = 0.4124 * r + 0.3576 * g + 0.1805 * b, Y = 0.2126 * r + 0.7152 * g + 0.0722 * b, Z = 0.0193 * r + 0.1192 * g + 0.9505 * b;
            double fx = F(X / 0.95047), fy = F(Y), fz = F(Z / 1.08883);
            L = 116 * fy - 16; A = 500 * (fx - fy); B = 200 * (fy - fz);
        }

        static double F(double t) => t > 216.0 / 24389.0 ? Math.Pow(t, 1.0 / 3.0) : (24389.0 / 27.0 * t + 16) / 116.0;

        // ============================================================================ WorldSky builder contract
        [Test]
        public void WorldSky_TypeNameShaderAndQueue()
        {
            var t = Type.GetType("Megastructure.Beats.WorldSky, Megastructure.Runtime");
            Assert.IsNotNull(t, "Megastructure.Beats.WorldSky (GrayboxPath looks this type up by name)");
            var b = (IBeatBuilder)Activator.CreateInstance(t);
            Assert.AreEqual("World_Sky", b.Name);
            string path = Path.Combine(Application.dataPath, "_Game/Shaders/BwFarField.shader");
            Assert.IsTrue(File.Exists(path), path);
            string src = File.ReadAllText(path);
            StringAssert.Contains("Shader \"Megastructure/BwFarField\"", src);
            StringAssert.Contains("\"Queue\" = \"Geometry+499\"", src);
            StringAssert.Contains("ZWrite Off", src);
            StringAssert.Contains("#include \"Include/BulwarkAtmosphere.hlsl\"", src);
            var sh = Shader.Find("Megastructure/BwFarField");
            Assert.IsNotNull(sh, "shader imported");
            Assert.IsTrue(sh.isSupported, "shader compiles on this device");
        }
    }
}
