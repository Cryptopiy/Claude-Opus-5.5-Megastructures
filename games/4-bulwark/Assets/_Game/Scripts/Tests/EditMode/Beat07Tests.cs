// Beat07Tests: EditMode tests of beat 7, the sluice tunnel and the lee ledge (MASTER_PROMPT 14.5.7 acceptance (1), (3),
// 14.10.1 T-SCALE, Part 10 #8 / #9 capsule reach, 14.7 H8 opening clamp, 14.2.5 lantern flicker, 14.12): the pure layout
// (Beat07Layout) against the spec numbers, the walls / rails / kerbs / ring pockets outside the capsule reach of the lane,
// the apron under the lane triangle beyond the face, the conduit flow offsets wrapping on exact periods of the shader's
// noise lattice, the schedule rows the beat depends on (sun descent, city lights, wetness, the opening clamp, _BwSurface).
using System;
using System.IO;
using Megastructure.Beats;
using NUnit.Framework;

namespace Megastructure.Tests
{
    public class Beat07Tests
    {
        const double Tol = 0.02;

        static string Abs(string rel) => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", rel));

        // ============================================================================ acceptance (1): dimensions
        [Test]
        public void Tunnel_Is3p2By3p4WithBars40At80()
        {
            Assert.AreEqual(BulwarkConst.TunnelWidth, 2.0 * Beat07Layout.WallLateral, Tol, "tunnel width 3.2");
            Assert.AreEqual(BulwarkConst.TunnelHeight, Beat07Layout.CeilingY - Beat07Layout.FloorY, Tol, "tunnel height 3.4");
            Assert.AreEqual(0.04, BulwarkConst.GratingBar, 1e-9, "bars 40 mm");
            Assert.AreEqual(0.08, BulwarkConst.GratingPitch, 1e-9, "at 80 mm");
            Assert.AreEqual(BulwarkConst.GratingPanelLength, Beat07Layout.PanelLength, 0.05, "grating panel 43 m (A3 end .. the mouth)");
            Assert.AreEqual(BulwarkConst.GratingPanelHeight, Beat07Layout.PanelTopY - Beat07Layout.FloorY, Tol, "grating panel 2.8 m");
            Assert.AreEqual(BulwarkConst.ConduitWidth, Beat07Layout.ConduitV1 - (Beat07Layout.ConduitV0 + 0.3), Tol, "conduit 8 m wide");
            Assert.AreEqual(BulwarkConst.ConduitHeight, Beat07Layout.ConduitCeilingY - Beat07Layout.ConduitFloorY, Tol, "conduit 6 m high");
            Assert.AreEqual(BulwarkConst.TunnelLampPitch, Beat07Layout.LampS(1) - Beat07Layout.LampS(0), 1e-9, "cages every 6 m");
            Assert.Less(Beat07Layout.LampS(Beat07Layout.LampCount - 1), BulwarkConst.TunnelMouthS, "last cage inside the tunnel");
        }

        [Test]
        public void Mouth_IsTheObliqueCutAtTheFace()
        {
            Beat07Layout.Mouth(out double z0, out double z1);
            Assert.AreEqual(1415.23, z0, 0.05, "west wall crosses the face at z 1415.2");
            Assert.AreEqual(1425.57, z1, 0.05, "east wall crosses the face at z 1425.6");
            Assert.AreEqual(BulwarkConst.TunnelMouthCut, z1 - z0, 0.1, "an oblique cut 10.4 m long");
            BwPath.Point(BulwarkConst.TunnelMouthS, out double x, out double z);
            Assert.AreEqual(BulwarkConst.LeeFaceX, x, 0.01, "the axis crosses the face at s 1422.85");
            Assert.AreEqual(BulwarkConst.LeeFaceCrossZ, z, 0.05, "at z 1420.4");
        }

        [Test]
        public void Ledge_RailKerbAndLantern()
        {
            Assert.AreEqual(6.4, Beat07Layout.LedgeX1 - Beat07Layout.LedgeX0, Tol, "ledge 6.4 wide");
            Assert.AreEqual(1.10, Beat07Layout.RailTop, 0.01, "rail 1.10");
            Assert.AreEqual(0.10, Beat07Layout.RailDiameter, 1e-9, "round rails 0.10");
            Assert.AreEqual(0.12, Beat07Layout.PostSize, 1e-9, "posts 0.12");
            Assert.AreEqual(2.0, Beat07Layout.PostPitch, 1e-9, "every 2 m");
            Assert.AreEqual(0.30, Beat07Layout.RingDiameter, 1e-9, "mooring rings 0.3 m");
            Assert.AreEqual(6.0, Beat07Layout.RingPitch, 1e-9, "every 6 m");
            Assert.AreEqual(1.40, Beat07Layout.LanternPost, Tol, "lantern post 1.40");
            Assert.AreEqual(0.30, Beat07Layout.LanternGlass, 1e-9, "glass 0.30");
            Assert.AreEqual(-9.2, Beat07Layout.LanternX, 1e-9);
            Assert.AreEqual(-90.0, Beat07Layout.LanternY, 1e-9);
            Assert.AreEqual(1499.0, Beat07Layout.LanternZ, 1e-9);
            Assert.AreEqual(BulwarkConst.LeeFaceAboveLedge, Beat07Layout.FaceY1 - Beat07Layout.FloorY, 1e-9, "90 m of face above the ledge");
        }

        // ============================================================================ capsule reach (Part 10 #8, #9; Wave A rule)
        [Test]
        public void Geometry_StaysOutsideTheCapsuleReach()
        {
            double r = BulwarkConst.PlayerR;
            // tunnel walls at the lane edge + 0.35 (the panel plane included)
            Assert.GreaterOrEqual(Beat07Layout.WallLateral, Beat07Layout.LaneHalf + r - 1e-9, "tunnel walls");
            // ledge: rail posts and rails, kerb inner face west of the lane's capsule reach; the face east of it
            double reachW = BulwarkConst.LaneLedgeXMin - r, reachE = BulwarkConst.LaneLedgeXMax + r;
            Assert.LessOrEqual(Beat07Layout.RailX + 0.5 * Beat07Layout.PostSize, reachW, "rail posts");
            Assert.LessOrEqual(Beat07Layout.RailX + 0.5 * Beat07Layout.RailDiameter, reachW, "rails");
            Assert.LessOrEqual(Beat07Layout.KerbX1, reachW, "kerb");
            Assert.GreaterOrEqual(Beat07Layout.FaceX, reachE, "the face and the ring pockets (recessed into it)");
            // the ledge ends at the elevator (ADDENDUM A1 00): the walker stops in the cabin at s 1500, its north wall beyond the
            // capsule; the north kerb and rail leave the 1.6 m door gap clear of the door lane's capsule (+-0.45 + 0.35)
            BwPath.Point(BwPath.Length, out double xEnd, out double zEnd);
            Assert.Greater(BulwarkConst.CabinZ1, zEnd + r, "cabin north wall beyond the capsule at s 1500");
            Assert.LessOrEqual(Beat07Layout.DoorGapX0, xEnd - BulwarkConst.FootDoorHalfLane - r + 1e-9, "door gap west edge");
            Assert.GreaterOrEqual(Beat07Layout.DoorGapX1, xEnd + BulwarkConst.FootDoorHalfLane + r - 1e-9, "door gap east edge");
            Assert.AreEqual(BulwarkConst.LedgeEndZ, Beat07Layout.LedgeZ1, 1e-12, "the slab ends at the top station");
        }

        [Test]
        public void Apron_CoversTheLaneBeyondTheFace()
        {
            // every capsule position of the tunnel lane that lies west of the face stands on the slab (z >= LedgeZ0 + r)
            double worst = double.MaxValue;
            for (double s = 1415.0; s <= BulwarkConst.LaneLedgeOpenS0; s += 0.05)
                for (double l = -Beat07Layout.LaneHalf; l <= Beat07Layout.LaneHalf + 1e-9; l += 0.05)
                {
                    BwPath.World(s, l, out double x, out _, out double z);
                    if (x > Beat07Layout.FaceX) continue;
                    worst = Math.Min(worst, z - BulwarkConst.PlayerR - Beat07Layout.LedgeZ0);
                }
            Assert.GreaterOrEqual(worst, 0.0, "the capsule west of the face reaches z below the apron edge");
            Assert.Less(Beat07Layout.RailZS + 0.5 * Beat07Layout.PostSize, Beat07Layout.LedgeZ0 + worst + 1e-9 + BulwarkConst.PlayerR, "apron rail beyond the capsule");
        }

        // ============================================================================ conduit flow (Part 10 #33)
        [Test]
        public void Conduit_FlowOffsetsWrapOnTheNoisePeriod()
        {
            // the C# updater wraps the 14 m/s offset on 64 x 3 m: the shader's lattice period (CD_PERIOD x CD_CELL_U)
            string sh = File.ReadAllText(Abs("Assets/_Game/Shaders/BwConduit.shader"));
            StringAssert.Contains("#define CD_PERIOD 64", sh);
            StringAssert.Contains("#define CD_CELL_U 3.0", sh);
            StringAssert.Contains("#define CD_ROLL 9.0", sh);
            string cs = File.ReadAllText(Abs("Assets/_Game/Beats/Beat07_SluiceLedge/Beat07SluiceLedge.cs"));
            StringAssert.Contains("P = 64.0 * 3.0, Lroll = 9.0", cs);
            Assert.AreEqual(14.0, Beat07Layout.FlowSpeed, 1e-9, "the sea at 14 m/s");
        }

        // ============================================================================ schedule rows beat 7 relies on
        [Test]
        public void Sun_DescentOverTheLedge()
        {
            // 14.5.7 (3) as corrected by the Wave A arbitration: el(s) = -0.9 - 1.2 S5((s-1420)/80) gives 0.0238 deg/m over
            // s 1440-1480 (0.015 is its mean over s 1420-1500)
            double rate = (BulwarkSchedule.SunElB7(1440.0) - BulwarkSchedule.SunElB7(1480.0)) / 40.0;
            Assert.AreEqual(0.024, rate, 0.003, "sun descent over s 1440-1480");
            Assert.AreEqual(0.015, (BulwarkSchedule.SunElB7(1420.0) - BulwarkSchedule.SunElB7(1500.0)) / 80.0, 0.003, "mean over s 1420-1500");
            Assert.AreEqual(-1.28, BulwarkSchedule.SunElB7(1452.0), 0.01, "s 1452: 1.19 deg above the visible horizon");
            Assert.AreEqual(1.19, BulwarkSchedule.SunElB7(1452.0) + BulwarkConst.LedgeDipDeg, 0.02);
        }

        [Test]
        public void CityLights_NoneAt1420AndOnAt1470()
        {
            Assert.AreEqual(0.0, BulwarkSchedule.CityN(1420.0), 1e-9, "none at s 1420");
            Assert.Greater(BulwarkSchedule.CityN(1470.0), 0.8, "on at s 1470");
            string ff = File.ReadAllText(Abs("Assets/_Game/Shaders/BwFarField.shader"));
            StringAssert.Contains("smoothstep(0.55, 0.8, FFFbm(ctr / 90.0, 0.0, 3))", ff, "the H4 pattern smoothstep(0.55, 0.8, fbm(p / 90 m))");
            StringAssert.Contains("(FF_CITY_K * _BwFar0.x * rho)", ff, "radiance N(s) x rho x pattern");
            StringAssert.DoesNotContain("FF_CITY_FARMS", ff, "no uniform sprinkle floor");
        }

        [Test]
        public void Surface_WetnessIsContinuousAndPublished()
        {
            for (double s = 0.0; s < 1500.0; s += 0.1)
                Assert.Less(Math.Abs(BulwarkSchedule.Wetness(s + 0.1) - BulwarkSchedule.Wetness(s)), 0.01, "wetness step at s " + s);
            Assert.AreEqual(1.0, BulwarkSchedule.Wetness(300.0), 1e-9, "the storm crest is wet");
            Assert.AreEqual(0.0, BulwarkSchedule.Wetness(700.0), 1e-9, "dry from the noon window");
            Assert.AreEqual(0.0, BulwarkSchedule.Wetness(1450.0), 1e-9, "dry ledge");
            var p = BulwarkSchedule.Evaluate(930.0);
            Assert.AreEqual(Math.Pow(2.0, 1.3 * (1.0 - BwMath.Smootherstep01(10.0 / 40.0))), p.LampBoost, 1e-4, "lamp gain = the portal boost");
            string drv = File.ReadAllText(Abs("Assets/_Game/Scripts/Runtime/Systems/AtmosphereDriver.cs"));
            StringAssert.Contains("PropertyToID(\"_BwSurface\")", drv);
            StringAssert.Contains("SetGlobalVector(IdSurface, new Vector4(p.Wetness, p.Interior, p.LampBoost, 0f))", drv);
        }

        [Test]
        public void Opening_ClampIs12InsideAndOpenOnTheLedge()
        {
            var ks19 = BulwarkConst.KeyShots[18];
            var ks20 = BulwarkConst.KeyShots[19];
            Assert.AreEqual(BulwarkConst.ClampOpening, BulwarkSchedule.OpeningClamp(ks19.x, ks19.y, ks19.z), 0.01, "inside the tunnel: 12");
            Assert.Greater(BulwarkSchedule.OpeningClamp(ks20.x, ks20.y, ks20.z), 1000.0, "on the ledge: open");
            // continuous across the mouth (2 m soft edge): no step of more than 25 % per 0.083 m (a 30 fps frame at 2.5 m/s)
            double prev = BulwarkSchedule.OpeningClamp(-5.0, -88.3, 1420.0);
            for (double s = 1415.0; s <= 1440.0; s += 0.0833)
            {
                BwPath.EyeWorld(s, 0.0, out double x, out double y, out double z);
                double c = BulwarkSchedule.OpeningClamp(x, y, z);
                if (s > 1415.0) Assert.Less(Math.Abs(Math.Log(c / prev)), Math.Log(1.25) + 1e-9, "clamp step at s " + s);
                prev = c;
            }
        }

        [Test]
        public void Lantern_FlickerIs3PercentBelow1Hz()
        {
            double lo = double.MaxValue, hi = double.MinValue;
            for (double t = 0.0; t < 600.0; t += 0.01)
            {
                double g = LanternFlicker.Gain(t);
                lo = Math.Min(lo, g); hi = Math.Max(hi, g);
            }
            Assert.AreEqual(0.03, Math.Max(hi - 1.0, 1.0 - lo), 0.01, "flicker 3 +-1 %");
            foreach (double hz in LanternFlicker.Hz) Assert.Less(hz, 1.0, "no component above 1 Hz");
        }

        [Test]
        public void Mouth_SkyLightFormFactorAndThresholdLamp()
        {
            Beat07Layout.Mouth(out double z0, out double z1);
            double yc = 0.5 * (Beat07Layout.FloorY + Beat07Layout.CeilingY), zc = 0.5 * (z0 + z1);
            // a point 1 mm inside the opening's centre facing it sees a full hemisphere of opening; west of the face nothing
            Assert.AreEqual(1.0, Beat07Layout.MouthFormFactor(Beat07Layout.FaceX + 1e-3, yc, zc, -1, 0, 0), 0.01);
            Assert.AreEqual(0.0, Beat07Layout.MouthFormFactor(Beat07Layout.FaceX - 0.5, yc, zc, -1, 0, 0), 1e-12);
            // far point on the axis 20 m in: the small-source limit A cos / (pi d^2) of the 10.34 x 3.4 rectangle (cos = 1, the
            // normal along -x), within 10 %
            double far = Beat07Layout.MouthFormFactor(Beat07Layout.FaceX + 20.0, yc, zc, -1, 0, 0);
            double a = (z1 - z0) * (Beat07Layout.CeilingY - Beat07Layout.FloorY);
            Assert.AreEqual(a / (Math.PI * 400.0), far, 0.1 * a / (Math.PI * 400.0));
            // at the reach F is below 0.02 even facing the opening squarely (the fade over the last 30 % removes the rest)
            Assert.Less(Beat07Layout.MouthFormFactor(Beat07Layout.FaceX + Beat07Layout.MouthLightReach, yc, zc, -1, 0, 0), 0.02);
            // the shader term exists, is added to the interior ambient (continuous at the face) and beat 7 publishes the mouth
            string atm = File.ReadAllText(Abs("Assets/_Game/Shaders/Include/BulwarkAtmosphere.hlsl"));
            StringAssert.Contains("float3 BwMouthLight(float3 n, float3 p)", atm);
            StringAssert.Contains("lerp(ext, inn + BwMouthLight(normalWS, posWS), BwInteriorWeight(posWS))", atm);
            string cs = File.ReadAllText(Abs("Assets/_Game/Beats/Beat07_SluiceLedge/Beat07SluiceLedge.cs"));
            StringAssert.Contains("_BwMouth0", cs);
            // threshold reinforcement: only the cage nearest the mouth burns brighter
            int last = Beat07Layout.LampCount - 1;
            Assert.AreEqual(1418.0, Beat07Layout.LampS(last), 1e-9);
            Assert.AreEqual(Beat07Layout.LampIntensity * Beat07Layout.ThresholdLampGain, Beat07Layout.LampIntensityAt(last), 1e-12);
            Assert.AreEqual(Beat07Layout.LampIntensity, Beat07Layout.LampIntensityAt(last - 1), 1e-12);
        }
    }
}
