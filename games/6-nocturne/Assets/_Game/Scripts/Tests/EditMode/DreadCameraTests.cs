using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    /// <summary>
    /// D-034 camera and facade layer of the fix wave p4_01 (FX): the long look-ups at KS05 / KS09 / KS17 (4.1) on the runtime
    /// follower, the KS10 vertigo envelope (4.2), the C12 / C45 / D-040 key edits, and the darkness wave (5.1, NocDarkWave).
    /// </summary>
    public sealed class DreadCameraTests
    {
        static void Trace(double s0, double s1, out List<double> ts, out List<double> ty, out List<double> tp)
        {
            ts = new List<double>(); ty = new List<double>(); tp = new List<double>();
            VisibilityFollower.Run(s0, new[] { s1 }, new double[1], new double[1], ts, ty, tp);
        }

        static double Heading(double s) => NocLanes.Path.HeadingDeg(NocMath.Clamp(s, 0.0, NocLanes.Path.Length));

        [Test]
        public void LongLookUps_ReachTheTop_HoldTwoSeconds_WithinRates()
        {
            // (window, top pitch) for KS05 (before it), the Ark's belly (D-043: after KS13, before KS14; it replaces the D-034 KS09
            // look-up at the 1 800 m hull) and KS17 (after the reveal hold)
            var cases = new[] { (240.0, 320.0, 62.0), (740.0, 846.0, 66.0), (1290.0, 1350.0, 58.0) };
            foreach (var (s0, s1, top) in cases)
            {
                Trace(s0, s1, out var ts, out var ty, out var tp);
                double atTop = 0.0, maxP = 0.0, maxY = 0.0, dt = 1.0 / VisibilityFollower.Fps;
                for (int i = 1; i < ts.Count; i++)
                {
                    if (tp[i] >= top) atTop += (ts[i] - ts[i - 1]) / NocturneConstants.CinematicSpeed;
                    maxP = Math.Max(maxP, Math.Abs(tp[i] - tp[i - 1]) / dt);
                    maxY = Math.Max(maxY, Math.Abs(NocMath.DeltaAngle(ty[i - 1], ty[i])) / dt);
                }
                Debug.Log(FormattableString.Invariant($"[NOC] look-up s {s0}-{s1}: {atTop:F2} s at >= {top} deg, peak pitch {maxP:F2} / yaw {maxY:F2} deg/s"));
                Assert.GreaterOrEqual(atTop, 2.0, "2 s hold at the top (DREAD 4.1)");
                Assert.LessOrEqual(maxP, NocturneConstants.PitchRateMax + 1e-6);
                Assert.LessOrEqual(maxY, NocturneConstants.YawRateMax + 1e-6);
            }
        }

        [Test]
        public void KeyShotPoses_FollowerOnPose_ReadableWindows()
        {
            // the follower stays readable (8 / 6 deg) >= 5.6 s on every touched key pose
            Trace(0.0, 1400.0, out var ts, out var ty, out var tp);
            foreach (string tag in new[] { "KS02", "KS04", "KS05", "KS06", "KS08", "KS09", "KS10", "KS11", "KS12", "KS13", "KS14", "KS17", "KS18" })
            {
                KeyShot k = default;
                foreach (var q in NocturneConstants.KeyShots) if (q.tag.StartsWith(tag, StringComparison.Ordinal)) k = q;
                VisibilityFollower.ReadableWindow(ts, ty, tp, k.s, k.yaw, k.pitch, out double a, out double b);
                Debug.Log(FormattableString.Invariant($"[NOC] {tag} s {k.s}: readable {a:F1}-{b:F1} ({(b - a) / 2.5:F2} s)"));
                Assert.IsFalse(double.IsNaN(a), tag + " readable at its s");
                Assert.GreaterOrEqual((b - a) / NocturneConstants.CinematicSpeed, 5.6, tag + " readable >= 5.6 s");
            }
            // C12: the gaze is within about 18 deg of forward by s 564 (gate A's tunnel from s 556.7)
            for (int i = 0; i < ts.Count; i++)
                if (ts[i] >= 564.0)
                {
                    double off = Math.Abs(NocMath.DeltaAngle(Heading(ts[i]), ty[i]));
                    Debug.Log(FormattableString.Invariant($"[NOC] s {ts[i]:F2}: gaze {off:F2} deg from forward"));
                    Assert.LessOrEqual(off, 20.0, "forward by s 564");
                    break;
                }
            // D-040: KS04 at pitch 27
            foreach (var q in NocturneConstants.KeyShots) if (q.tag.StartsWith("KS04", StringComparison.Ordinal)) Assert.AreEqual(27.0, q.pitch, 1e-12);
        }

        /// <summary>D-043 (THE ARK, DR's course: down the rift from the north, the bow over z 195 at s 596): the three Ark moments of
        /// the gaze hold their poses on the runtime follower (8 / 6 deg, 14.5 readable, yaw + 2): the look at the coming bow (rel -80 /
        /// +16) >= 6 s inside s 476-520, the bow over the bridge's head (rel -56 / +38) >= 4.5 s inside s 568-594 (held still while the bridge lamp at s 584.8 leaves the frame), the belly from the
        /// north (rel -95 / +28) >= 5 s and near the zenith (rel -77 / +75) >= 6 s inside s 740-846; all within the 22 / 18 deg/s
        /// limits.</summary>
        [Test]
        public void ArkGaze_ComingBow_BowOverhead_Belly_HoldTheirPoses()
        {
            Trace(380.0, 900.0, out var ts, out var ty, out var tp);
            var poses = new[] { (476.0, 520.0, -80.0, 16.0, 6.0), (568.0, 594.0, -56.0, 38.0, 4.5), (740.0, 800.0, -95.0, 28.0, 5.0), (790.0, 830.0, -77.0, 75.0, 6.0) };
            double dt = 1.0 / VisibilityFollower.Fps;
            foreach (var (s0, s1, rel, pitch, need) in poses)
            {
                double held = 0.0, maxP = 0.0, maxY = 0.0;
                for (int i = 1; i < ts.Count; i++)
                {
                    if (ts[i] < s0 || ts[i] > s1) continue;
                    double yaw = Heading(ts[i]) + rel;
                    bool near = Math.Abs(NocMath.DeltaAngle(yaw, ty[i])) <= VisibilityFollower.ReadableYaw + 2.0 && Math.Abs(tp[i] - pitch) <= VisibilityFollower.ReadablePitch;
                    if (near) held += (ts[i] - ts[i - 1]) / NocturneConstants.CinematicSpeed;
                    maxP = Math.Max(maxP, Math.Abs(tp[i] - tp[i - 1]) / dt);
                    maxY = Math.Max(maxY, Math.Abs(NocMath.DeltaAngle(ty[i - 1], ty[i])) / dt);
                }
                Debug.Log(FormattableString.Invariant($"[NOC] Ark gaze s {s0}-{s1} rel {rel} / {pitch}: held {held:F2} s, peak pitch {maxP:F2} / yaw {maxY:F2} deg/s"));
                Assert.GreaterOrEqual(held, need, "Ark pose held (D-043)");
                Assert.LessOrEqual(maxP, NocturneConstants.PitchRateMax + 1e-6);
                Assert.LessOrEqual(maxY, NocturneConstants.YawRateMax + 1e-6);
            }
        }

        [Test]
        public void Vertigo_Envelope_FovAndRates()
        {
            Assert.AreEqual(0.0, DreadCamera.Vertigo(500.0), 1e-12);
            Assert.AreEqual(0.0, DreadCamera.Vertigo(DreadCamera.VertigoS0), 1e-12);
            Assert.AreEqual(1.0, DreadCamera.Vertigo(DreadCamera.VertigoS1), 1e-12);
            Assert.AreEqual(0.0, DreadCamera.Vertigo(DreadCamera.VertigoS2), 1e-12);
            Assert.AreEqual(0.0, DreadCamera.Vertigo(600.0), 1e-12);
            double s1 = DreadCamera.VertigoS1;
            Assert.AreEqual(74.0, NocSchedule.Fov(s1) + DreadCamera.VertigoFov * DreadCamera.Vertigo(s1), 1e-9);
            Assert.AreEqual(4.0, (DreadCamera.VertigoS1 - DreadCamera.VertigoS0) / NocturneConstants.CinematicSpeed, 1e-9, "widens over 4 s");
            Assert.AreEqual(3.0, (DreadCamera.VertigoS2 - DreadCamera.VertigoS1) / NocturneConstants.CinematicSpeed, 1e-9, "returns over 3 s");
            double rate = DreadCamera.VertigoFov * DreadCamera.VertigoMaxSlope() * NocturneConstants.CinematicSpeed;
            Assert.AreEqual(3.75, rate, 1e-9, "peak vFOV rate deg/s (the return)");
            // inside the plunge hold, ended before gate A (s 556.7); the still keeps 68; the lean stays inside the lane
            Assert.GreaterOrEqual(DreadCamera.VertigoS0, 536.0);
            Assert.Less(DreadCamera.VertigoS2, 556.7);
            foreach (var q in NocturneConstants.KeyShots) if (q.tag.StartsWith("KS10", StringComparison.Ordinal)) Assert.AreEqual(68.0, q.fov, 1e-12);
            for (double s = DreadCamera.VertigoS0; s <= DreadCamera.VertigoS2; s += 0.5)
                Assert.GreaterOrEqual(NocLanes.LaneHalfWidth(s) - DreadCamera.VertigoLean, 1.6, "lean inside the lane at s " + s);
        }

        [Test]
        public void DarkWave_TopDown_RampAndReturn()
        {
            for (int k = 0; k < NocDarkWave.DarkWaveS.Length; k++)
            {
                var t = NocDarkWave.TowerOf(k);
                Debug.Log(FormattableString.Invariant($"[NOC] dark wave {k}: tower centre ({t.cx:F1}, {t.cz:F1}) yaw {t.yaw:F1} half {t.hx:F1} x {t.hz:F1}, y {t.y0}..{t.y1}"));
                double c = Math.Cos(t.yaw * NocMath.Deg), sn = Math.Sin(t.yaw * NocMath.Deg);
                // a point on the tower's local +x face, and one 6 m outside it (the next shaft)
                double fx = t.cx + c * t.hx, fz = t.cz - sn * t.hx;
                double ox = t.cx + c * (t.hx + 6.0), oz = t.cz - sn * (t.hx + 6.0);
                Assert.AreEqual(1.0, NocDarkWave.Factor(k, -1.0, fx, 300.0, fz), 1e-12, "before the wave");
                Assert.AreEqual(1.0, NocDarkWave.Factor(k, 10.0, ox, 300.0, oz), 1e-12, "outside the tower");
                Assert.AreEqual(0.0, NocDarkWave.Factor(k, 10.0, fx, 300.0, fz), 1e-12, "dark after the front");
                Assert.AreEqual(1.0, NocDarkWave.Factor(k, 60.0, fx, 300.0, fz), 1e-12, "returned");
                Assert.Less(NocDarkWave.Factor(k, 1.0, fx, 540.0, fz), NocDarkWave.Factor(k, 1.0, fx, 20.0, fz), "top down");
                // every point fades over the 1.2 s toggle (never faster); the front covers the shaft in 20-30 s
                double y = 200.0, tOn = double.NaN, tOff = double.NaN;
                for (double tau = 0.0; tau < 30.0; tau += 0.001)
                {
                    double f = NocDarkWave.Factor(k, tau, fx, y, fz);
                    if (double.IsNaN(tOn) && f < 1.0) tOn = tau;
                    if (double.IsNaN(tOff) && f <= 0.0) tOff = tau;
                }
                Assert.AreEqual(NocDarkWave.DarkWaveRampT, tOff - tOn, 0.003, "1.2 s fade per point");
                double whole = (t.y1 - t.y0) / NocDarkWave.DarkWaveSpeed;
                Assert.That(whole, Is.InRange(20.0, 30.0), "front over 20-30 s (DREAD 5.1)");
                Assert.That(NocDarkWave.DarkWaveSpeed, Is.InRange(120.0, 200.0));
            }
            // beat 7's tower lies near the centre of the KS18 frame
            KeyShot ks = default;
            foreach (var q in NocturneConstants.KeyShots) if (q.tag.StartsWith("KS18", StringComparison.Ordinal)) ks = q;
            var r = NocDarkWave.TowerOf(1);
            double yr = ks.yaw * NocMath.Deg, pr = ks.pitch * NocMath.Deg;
            var f3 = new Vector3((float)(Math.Sin(yr) * Math.Cos(pr)), (float)Math.Sin(pr), (float)(Math.Cos(yr) * Math.Cos(pr)));
            var d3 = new Vector3((float)(r.cx - ks.x), (float)(280.0 - ks.y), (float)(r.cz - ks.z));
            float ang = Vector3.Angle(f3, d3);
            Debug.Log(FormattableString.Invariant($"[NOC] beat-7 dark tower {ang:F1} deg from the KS18 gaze"));
            Assert.Less(ang, 35.0f);
        }
    }
}
