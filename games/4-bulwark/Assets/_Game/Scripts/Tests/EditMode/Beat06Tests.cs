// Beat06Tests: EditMode tests of beat 6, the window gallery (MASTER_PROMPT 14.5.6 acceptance (1), 14.7 H3, 14.10.1; Addendum
// A1 1.8 with LEAD DECISIONS 2-4): the pure layout (Beat06Layout) against the spec numbers (hall 28 m, ramp 0.12, -54 / -90,
// ten 20 x 36 m panes at s 1085 + 30 k), the rail and fittings outside the capsule reach, the lamp ramps (continuous, log-linear,
// the A1 click / full / off keys), the s-keyed water state (veil dim, snow bulk flow, amber, sway), the creature's eye path
// (continuous, <= 8 m/s at 2.5 m/s while visible, 0.3 m off the glass, KS18 bearing), the pupil / blink keys, the call table.
using System;
using Megastructure.Beats;
using NUnit.Framework;

namespace Megastructure.Tests
{
    public class Beat06Tests
    {
        const double Tol = 1e-6;

        // ============================================================================ acceptance (1): dimensions
        [Test]
        public void Hall_Is28HighOnThe012RampFromMinus54ToMinus90()
        {
            Assert.AreEqual(28.0, Beat06Layout.Height, 0.05, "hall 28 m");
            Assert.AreEqual(-54.0, Beat06Layout.FloorY(1070.0), 0.01, "floor -54 at s 1070");
            Assert.AreEqual(-90.0, Beat06Layout.FloorY(1370.0), 0.01, "floor -90 at s 1370");
            for (double s = 1080.0; s < 1360.0; s += 10.0)
                Assert.AreEqual(0.12, (Beat06Layout.FloorY(s) - Beat06Layout.FloorY(s + 10.0)) / 10.0, 0.0005, "ramp 0.1200 at s " + s);
            Assert.AreEqual(BulwarkConst.HallWestWall, Beat06Layout.West, Tol);
            Assert.AreEqual(BulwarkConst.HallRail, Beat06Layout.Rail, Tol);
            Assert.AreEqual(BulwarkConst.GlassAxisOffset, Beat06Layout.GlassIn, Tol);
            Assert.AreEqual(BulwarkConst.GlassThickness, Beat06Layout.GlassOut - Beat06Layout.GlassIn, Tol, "glass 2.4 m");
            Assert.AreEqual(BulwarkConst.GlassRevealDepth, Beat06Layout.RevealEnd - Beat06Layout.GlassOut, Tol, "reveal 3.5 m");
            Assert.AreEqual(BulwarkConst.HallTrenchDepth, Beat06Layout.Trench, Tol);
        }

        [Test]
        public void Panes_AreTen20By36AtThe14p4Centres()
        {
            Assert.AreEqual(10, Beat06Layout.Panes);
            for (int k = 0; k < 10; k++)
            {
                Assert.AreEqual(1085.0 + 30.0 * k, Beat06Layout.PaneS(k), Tol);
                Assert.AreEqual(k, Beat06Layout.PaneAt(Beat06Layout.PaneS(k)));
                Assert.AreEqual(k, Beat06Layout.PaneAt(Beat06Layout.PaneS(k) + 9.99));
                Assert.AreEqual(-1, Beat06Layout.PaneAt(Beat06Layout.PaneS(k) + 15.0), "pier between panes " + k);
            }
            Assert.AreEqual(20.0, Beat06Layout.PaneWidth, 0.05);
            Assert.AreEqual(36.0, Beat06Layout.PaneTop - Beat06Layout.PaneBottom, 0.05, "pane 36 m: floor - 8 to floor + 28");
            Assert.AreEqual(BulwarkConst.PierWidth6, Beat06Layout.PanePitch - Beat06Layout.PaneWidth, Tol, "piers 10 m");
        }

        [Test]
        public void Fittings_StayOutsideTheCapsuleReach()
        {
            double reach = BulwarkConst.PlayerR;
            // rail tube inner surface beyond the lane's right edge + the capsule radius
            Assert.GreaterOrEqual(Beat06Layout.Rail + Beat06Layout.RailSetback - 0.5 * Beat06Layout.RailDiameter, BulwarkConst.LaneGalleryRight + reach - 1e-9);
            // west wall fittings (hatch frames 0.06, bollards 0.145 deep) inside the 0.15 m niche west of the collider face
            Assert.LessOrEqual(Beat06Layout.West + 0.145, BulwarkConst.LaneGalleryLeft - reach + 1e-9);
            Assert.LessOrEqual(Beat06Layout.West + 0.06, BulwarkConst.LaneGalleryLeft - reach + 1e-9);
        }

        // ============================================================================ lamps (A1 1.8 table)
        [Test]
        public void Lamps_FollowTheA1KeysAndRampContinuously()
        {
            Assert.AreEqual(0.0, Beat06Layout.LampA(1203.9), Tol, "A off before the click");
            Assert.AreEqual(1.0, Beat06Layout.LampA(1213.0), 1e-3, "A full at 1213");
            Assert.AreEqual(1.0, Beat06Layout.LampA(1230.0), Tol);
            Assert.AreEqual(0.0, Beat06Layout.LampA(1245.0), Tol, "A off at 1245");
            Assert.AreEqual(0.0, Beat06Layout.LampB(1246.9), Tol);
            Assert.AreEqual(1.0, Beat06Layout.LampB(1256.0), 1e-3, "B full at 1256");
            Assert.AreEqual(1.0, Beat06Layout.LampB(1270.0), Tol);
            Assert.AreEqual(0.0, Beat06Layout.LampB(1287.0), Tol);
            // continuity: no step above 2 % of full per 0.01 m of s anywhere (log-linear ramps with a soft start)
            double prevA = 0.0, prevB = 0.0;
            for (double s = 1190.0; s <= 1300.0; s += 0.01)
            {
                double a = Beat06Layout.LampA(s), b = Beat06Layout.LampB(s);
                Assert.Less(Math.Abs(a - prevA), 0.02, "lamp A step at s " + s);
                Assert.Less(Math.Abs(b - prevB), 0.02, "lamp B step at s " + s);
                prevA = a; prevB = b;
            }
            // log-linear: the luminance ratio between equal s steps is constant in the middle of the ramp
            double r1 = Beat06Layout.LampA(1208.0) / Beat06Layout.LampA(1207.0), r2 = Beat06Layout.LampA(1211.0) / Beat06Layout.LampA(1210.0);
            Assert.AreEqual(r1, r2, 1e-6, "log-linear ramp");
        }

        // ============================================================================ water state keyed to s
        [Test]
        public void WaterState_IsKeyedToSAndContinuous()
        {
            Assert.AreEqual(1.0, Beat06Layout.VeilGain(1100.0), Tol);
            Assert.AreEqual(0.62, Beat06Layout.VeilGain(1145.0), 1e-6, "upper veil 62 % while the arm passes");
            Assert.AreEqual(0.62, Beat06Layout.VeilGain(1166.0), 1e-6, "still dim at KS16: the arm is above");
            Assert.AreEqual(1.0, Beat06Layout.VeilGain(1190.0), Tol, "the light is back once the arm drifted out");
            Assert.AreEqual(0.0, Beat06Layout.SnowBulk(1100.0), Tol);
            Assert.Greater(Beat06Layout.SnowBulk(1160.0), 1.0, "the bulk flow moved the snow");
            Assert.AreEqual(Beat06Layout.SnowBulk(1160.0), Beat06Layout.SnowBulk(1300.0), Tol, "and stopped by s 1160");
            Assert.AreEqual(0.0, Beat06Layout.Amber(1339.0), Tol);
            Assert.AreEqual(1.0, Beat06Layout.Amber(1370.0), Tol);
            Assert.LessOrEqual(Beat06Layout.SwayDeg(1220.0), 0.05 * 1.6 + 1e-9);
            Assert.AreEqual(0.0, Beat06Layout.SwayDeg(1150.0), Tol, "no sway while nothing presses");
        }

        // ============================================================================ the creature (A1 1.8, LEAD DECISIONS 2, 4)
        [Test]
        public void Eye_PathIsContinuousSlowAndOutsideTheGlass()
        {
            double prevL = 0, prevY = 0, prevZ = 0, maxV = 0.0;
            const double ds = 0.05, walk = 2.5;
            for (double s = 1070.0; s <= 1372.0; s += ds)
            {
                Beat06Layout.EyeAt(s, out double l, out double y, out double z);
                // the eyeball (r 10) and its brow (20.6 m along the axis) stay >= 0.3 m beyond the outer glass
                Assert.GreaterOrEqual(l - Beat06Layout.GlassOut, 20.6 + 0.3, "eye centre too near the glass at s " + s);
                if (s > 1070.0 + 1e-9)
                {
                    double v = Math.Sqrt((l - prevL) * (l - prevL) + (y - prevY) * (y - prevY) + (z - prevZ) * (z - prevZ)) / ds * walk;
                    Assert.Less(v, 40.0, "no jump at s " + s);
                    if (s >= 1226.0 && s <= 1345.0) maxV = Math.Max(maxV, v);
                }
                prevL = l; prevY = y; prevZ = z;
            }
            Assert.LessOrEqual(maxV, 8.0, "visible eye moves at most 8 m/s at 2.5 m/s (LEAD DECISION 2)");
            Beat06Layout.EyeAt(1268.0, out double hl, out double hy, out double hz);
            Assert.AreEqual(Beat06Layout.EyeHoldLateral, hl, 0.01, "hold lateral");
            Assert.AreEqual(1268.0, hz, 3.0, "hold abeam pane 6 (z 1268)");
        }

        [Test]
        public void Eye_AtKS18_IsBelowAtTheA1Bearing()
        {
            Beat06Layout.EyeAt(1320.0, out double l, out double y, out double z);
            BwPath.World(z, l, out double x, out _, out double wz);
            double dx = x - 8.00, dy = y - (-82.30), dz = wz - 1320.0;
            double yaw = Math.Atan2(dx, dz) * 180.0 / Math.PI, pitch = Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz)) * 180.0 / Math.PI;
            Assert.AreEqual(62.0, yaw, 4.0, "eye cluster near yaw 62 (A1 KS18)");
            Assert.Less(pitch, -12.0, "below the eye level");
            Assert.Greater(pitch, -24.0, "above the trench edge seen from the KS18 eye (at -27 the floor edge hides it)");
        }

        [Test]
        public void CrossArm_IsSlowBeyondTheGlassAndThereAtKS16()
        {
            const double ds = 0.05, walk = 2.5;
            double maxV = 0.0, minClear = 1e9;
            Beat06Layout.CrossArmAt(Beat06Layout.CrossArmS0, out double ps, out double pl, out double py);
            for (double s = Beat06Layout.CrossArmS0 + ds; s <= Beat06Layout.CrossArmS1; s += ds)
            {
                Beat06Layout.CrossArmAt(s, out double ts, out double tl, out double ty);
                double v = Math.Sqrt((ts - ps) * (ts - ps) + (tl - pl) * (tl - pl) + (ty - py) * (ty - py)) / ds * walk;
                maxV = Math.Max(maxV, v);
                // the glass side of the tip and of the knee (larger radius, further out) >= 1 m beyond the outer glass, and
                // beyond the dam's sea face (lateral 11.9) for the knee, which lies along the face
                minClear = Math.Min(minClear, tl - Beat06Layout.CrossArmSwayL - Beat06Layout.CrossArmTipR - Beat06Layout.GlassOut);
                minClear = Math.Min(minClear, tl + Beat06Layout.CrossArmMid[1] - Beat06Layout.CrossArmSwayL - Beat06Layout.CrossArmMidR - Beat06Layout.GlassOut);
                minClear = Math.Min(minClear, tl + Beat06Layout.CrossArmKnee[1] - Beat06Layout.CrossArmKneeR - Beat06Layout.RevealEnd);
                Assert.Less(ty + Beat06Layout.CrossArmRoot[2] + Beat06Layout.CrossArmRootR, -8.0, "nothing breaks the surface (A1 1.8)");
                ps = ts; pl = tl; py = ty;
            }
            Assert.LessOrEqual(maxV, 8.0, "every point of the arm moves at most 8 m/s at 2.5 m/s (LEAD DECISION 2)");
            Assert.GreaterOrEqual(minClear, 1.0, "the arm stays >= 1 m beyond the glass and the dam face");
            // KS16 (s 1166, eye (5.68, -63.82), yaw 70, pitch 40): the tip lies ahead, so the band crosses yaw 55-64
            Beat06Layout.CrossArmAt(1166.0, out double ks, out double kl, out double ky);
            double yawTip = Math.Atan2(kl, ks - 1166.0) * 180.0 / Math.PI;
            Assert.Less(yawTip, 55.0, "the tip is ahead of the KS16 band (yaw 55-64 is crossed by the arm)");
            double elev = Math.Atan2(ky - (-63.82), kl) * 180.0 / Math.PI;
            Assert.AreEqual(50.0, elev, 10.0, "the arm's axis at 40-60 deg elevation in the KS16 frame (pitch 40)");
            // parked out of view outside the window: 60+ m beyond the glass
            Beat06Layout.CrossArmAt(1070.0, out _, out double l0, out _);
            Beat06Layout.CrossArmAt(1215.0, out _, out double l1, out _);
            Assert.Greater(l0 - Beat06Layout.GlassOut, 60.0);
            Assert.Greater(l1 - Beat06Layout.GlassOut, 60.0);
        }

        [Test]
        public void GlassArm_RisesSlowlyAndStaysPressed()
        {
            Assert.AreEqual(0.0, Beat06Layout.GlassArm(1149.0), Tol, "nothing on the glass before s 1150");
            Assert.AreEqual(1.0, Beat06Layout.GlassArm(1210.0), Tol, "pressed by s 1200");
            Assert.AreEqual(1.0, Beat06Layout.GlassArm(1370.0), Tol, "never lets go in view");
            Assert.AreEqual(0.0, Beat06Layout.Withdraw(1300.0), Tol, "no Withdraw weight beside GlideAlong (full glass clamp)");
            double maxRate = 0.0;
            for (double s = 1150.0; s < 1200.0; s += 0.05)
                maxRate = Math.Max(maxRate, (Beat06Layout.GlassArm(s + 0.05) - Beat06Layout.GlassArm(s)) / 0.05 * 2.5);
            Assert.Less(maxRate, 0.1, "the weight rises over >= 10 s at 2.5 m/s");
            // S2-4: the plane comes in from the black instead (full clamp all the hall long), outside the dam face, <= 8 m/s
            Assert.AreEqual(Beat06Layout.GlassArmFarLateral, Beat06Layout.GlassPlaneAt(1100.0), Tol);
            Assert.AreEqual(Beat06Layout.GlassPlaneLateral, Beat06Layout.GlassPlaneAt(1232.0), Tol);
            Assert.GreaterOrEqual(Beat06Layout.GlassPlaneLateral, Beat06Layout.RevealEnd + 1.0, "1 m outside the dam face");
            double maxV = 0.0;
            for (double s = 1140.0; s < 1210.0; s += 0.05)
                maxV = Math.Max(maxV, Math.Abs(Beat06Layout.GlassPlaneAt(s + 0.05) - Beat06Layout.GlassPlaneAt(s)) / 0.05 * 2.5);
            Assert.Less(maxV, 8.0, "plane speed at 2.5 m/s");
        }

        [Test]
        public void Pupil_SnapsAsLampBReachesFull_AndTheBlinkIsAt1272()
        {
            Assert.AreEqual(1.0, Beat06Layout.Pupil(1250.0), Tol, "round in the dark");
            Assert.AreEqual(0.0, Beat06Layout.Pupil(1258.0), Tol, "slit once lamp B is full");
            Assert.AreEqual(3.0, 1257.5 - 1254.5, Tol, "1.2 s at 2.5 m/s");
            Assert.AreEqual(-1.0, Beat06Layout.Blink(1200.0), Tol, "automatic outside the encounter");
            Assert.AreEqual(0.0, Beat06Layout.Blink(1271.9), Tol);
            Assert.AreEqual(1.0, Beat06Layout.Blink(1276.0), Tol, "closed at 1276");
            Assert.AreEqual(0.0, Beat06Layout.Blink(1281.0), Tol, "open again");
            Assert.AreEqual(1.0, Beat06Layout.Blink(1290.0), Tol, "closed while it withdraws (S3-9)");
            Assert.AreEqual(0.0, Beat06Layout.Blink(1308.0), Tol, "open again before KS18");
            Assert.AreEqual(-1.0, Beat06Layout.Blink(1320.0), Tol, "automatic at KS18");
            Assert.AreEqual(1.0, Beat06Layout.EyeScaleAt(1268.0), Tol, "full scale at the hold");
            Assert.AreEqual(0.0, Beat06Layout.EyeScaleAt(1296.0), Tol, "socket size once withdrawn");
            for (double s = 1288.0; s <= 1296.0; s += 0.5)
                Assert.GreaterOrEqual(Beat06Layout.Blink(s), 0.99, "the eyeball shrinks only under closed lids, s " + s);
        }

        [Test]
        public void Calls_CoverTheA1MomentsInOrder()
        {
            var c = Beat06Layout.Calls;
            Assert.AreEqual(Enum.GetValues(typeof(Beat06Layout.CallId)).Length * 3, c.Length);
            for (int i = 1; i < c.Length / 3; i++) Assert.Greater(c[3 * i], c[3 * (i - 1)], "calls in s order");
            for (int i = 0; i < c.Length / 3; i++)
            {
                Assert.That(c[3 * i], Is.InRange(1070.0, 1370.0));
                Assert.That(c[3 * i + 2], Is.InRange(0.0, 1.0));
                Assert.Greater(c[3 * i + 1], 0.0);
            }
        }

        [Test]
        public void Calls_LeaveNoGapOver3sAtTheWalkSpeed()
        {
            // critic S2-6: at 1.5 m/s no gap over 3 s from the first press (1196) to the strike (1300), apart from the designed
            // pier silence (1241-1250); the same at 2.5 m/s; durations scale by 2.5 / clamp(v, 1.5, 2.5)
            var c = Beat06Layout.Calls;
            int n = c.Length / 3;
            Assert.AreEqual(n, Beat06Layout.CallHoldUntil.Length);
            foreach (double v in new[] { 1.5, 2.5 })
            {
                double gapMax = 0.0, gapAt = 1196.0;
                for (double s = 1196.0; s <= 1300.0; s += 0.05)
                {
                    if (s >= 1241.0 && s <= 1250.0) { gapAt = s; continue; }
                    bool on = false;
                    for (int i = 0; i < n; i++)
                    {
                        double s0 = c[3 * i], s1 = s0 + Beat06Layout.CallDuration(i, v) * v;
                        if (s >= s0 && s <= s1) { on = true; break; }
                    }
                    if (on) gapAt = s;
                    else gapMax = Math.Max(gapMax, (s - gapAt) / v);
                }
                Assert.LessOrEqual(gapMax, 3.0, "silence over 3 s at " + v + " m/s");
            }
            // the sustains hold to their s keys at the slow walk, the table durations scale
            Assert.AreEqual((1245.0 - 1196.0) / 1.5 + Beat06Layout.CallRelease, Beat06Layout.CallDuration(1, 1.5), 1e-9);
            Assert.AreEqual((1287.0 - 1251.0) / 1.5 + Beat06Layout.CallRelease, Beat06Layout.CallDuration(5, 1.5), 1e-9);
            Assert.AreEqual(8.0 * 2.5 / 1.5, Beat06Layout.CallDuration(0, 1.5), 1e-9);
            Assert.AreEqual(8.0, Beat06Layout.CallDuration(0, 25.0), 1e-9, "the fast autotest uses the 2.5 m/s table");
        }

        [Test]
        public void Gaze_FollowerHoldsTheEyeInFrame()
        {
            // critic S2-3: the walker's follower (gaze evaluated at s + lead, lead = 2 v / omega, critically damped omega 2.5,
            // 18 / 13 deg/s) on Beat06Layout.GazeKeys (monotone cubic, R4) keeps the eye centre within 15 deg of the frame
            // centre over s 1252-1276 at 1.5 and 2.5 m/s, and within 2 deg at KS17 (s 1268)
            var k = Beat06Layout.GazeKeys;
            for (int i = 3; i < k.Length; i += 3)
            {
                Assert.Greater(k[i], k[i - 3], "keys in s order");
                double dt25 = (k[i] - k[i - 3]) / 2.5;
                Assert.LessOrEqual(Math.Abs(k[i + 1] - k[i - 2]) / dt25, 18.0, "yaw rate at s " + k[i]);
                Assert.LessOrEqual(Math.Abs(k[i + 2] - k[i - 1]) / dt25, 13.0, "pitch rate at s " + k[i]);
            }
            foreach (double v in new[] { 1.5, 2.5 })
            {
                const double om = 2.5, dt = 1.0 / 60.0;
                double lead = 2.0 / om * v, s = 1166.0;
                double yaw = Mono(k, 1, s + lead), pitch = Mono(k, 2, s + lead), vy = 0.0, vp = 0.0, worst = 0.0, at1268 = double.NaN;
                while (s < 1300.0)
                {
                    double ty = Mono(k, 1, s + lead), tp = Mono(k, 2, s + lead);
                    vy += (om * om * (ty - yaw) - 2.0 * om * vy) * dt; vy = Math.Max(-18.0, Math.Min(18.0, vy)); yaw += vy * dt;
                    vp += (om * om * (tp - pitch) - 2.0 * om * vp) * dt; vp = Math.Max(-13.0, Math.Min(13.0, vp)); pitch += vp * dt;
                    Beat06Layout.EyeAt(s, out double l, out double y, out double z);
                    double cy = Beat06Layout.FloorY(s) + 1.7, lat = 1.5;
                    double ey = Math.Atan2(l - lat, z - s) * 180.0 / Math.PI;
                    double ep = Math.Atan2(y - cy, Math.Sqrt((l - lat) * (l - lat) + (z - s) * (z - s))) * 180.0 / Math.PI;
                    double err = Math.Max(Math.Abs(ey - yaw), Math.Abs(ep - pitch));
                    if (s >= 1252.0 && s <= 1276.0) worst = Math.Max(worst, err);
                    if (double.IsNaN(at1268) && s >= 1268.0) at1268 = err;
                    s += v * dt;
                }
                Assert.LessOrEqual(worst, 15.0, "eye off the frame centre at " + v + " m/s");
                Assert.LessOrEqual(at1268, 2.0, "KS17 lag at " + v + " m/s");
            }
        }

        /// <summary>Monotone cubic (Fritsch-Carlson) through (s, value) of a stride-3 key table, column c (1 yaw, 2 pitch).</summary>
        static double Mono(double[] k, int c, double s)
        {
            int n = k.Length / 3;
            if (s <= k[0]) return k[c];
            if (s >= k[3 * (n - 1)]) return k[3 * (n - 1) + c];
            int i = 0;
            while (i + 2 < n && s >= k[3 * (i + 1)]) i++;
            double h = k[3 * (i + 1)] - k[3 * i], t = (s - k[3 * i]) / h;
            double m0 = Tan(k, c, i, n), m1 = Tan(k, c, i + 1, n);
            double y0 = k[3 * i + c], y1 = k[3 * (i + 1) + c];
            return (2 * t * t * t - 3 * t * t + 1) * y0 + (t * t * t - 2 * t * t + t) * h * m0 + (-2 * t * t * t + 3 * t * t) * y1 + (t * t * t - t * t) * h * m1;
        }

        static double Sec(double[] k, int c, int i) => (k[3 * (i + 1) + c] - k[3 * i + c]) / (k[3 * (i + 1)] - k[3 * i]);

        static double Tan(double[] k, int c, int i, int n)
        {
            if (i == 0 || i == n - 1) return 0.0;
            double d0 = Sec(k, c, i - 1), d1 = Sec(k, c, i);
            if (d0 * d1 <= 0.0) return 0.0;
            double m = 0.5 * (d0 + d1);
            // Fritsch-Carlson limit on both adjacent intervals
            if (m / d0 > 3.0) m = 3.0 * d0;
            if (m / d1 > 3.0) m = 3.0 * d1;
            return m;
        }
    }
}
