// TrainTests: the trains T1-T7 of beat 4 (MASTER_PROMPT 14.5.4, 14.7 H7, 14.10 T-TRAIN; owner TR, Phase 4 wave B) on TrainSchedule,
// TrainBuilder's train mesh and TrainDriver's rules:
//   * the table of 14.5.4 (launch s, tube, direction, origin), 174 m at 38 m/s, x(tau) = x0 -+ 38 tau, capture tau = (s - sL) / 2.5;
//   * T-TRAIN: the meeting points T1 602.0, T3 653.6, T4 699.3, T2 759.1, T7 816.3 and T6's nose under the strip centre at 715.8,
//     analytically and in a stepped play-mode walk at 60 fps (within 0.5 s); the passes differ by <= 7 m between 1.5 and 2.5 m/s;
//     T6 passes under the strip while the walker is on it at both speeds; T1 visible head-on >= 6 s (from 283 m);
//   * never outside the tube: for every train and 400 instants over its run, every vertex of the built train either lies behind a
//     clip plane or inside its tube (r - 0.3 m) between the tunnel ends; a vertex in a tunnel is dark (Lit 0 beyond 2.5 m); at the
//     launch every drawn vertex is behind the origin mouth (the nose on the portal plane); once hidden every vertex is clipped;
//   * the state machine (launch on the crossing, reset only while invisible, relaunch), the light fades (0 at the mouths);
//   * the body: 174.0 m, 3.4 m wide, the body section 3.4 m about the axis (the visibility audit's box), a valid mesh; the guideway
//     and linings inside their tubes; agreement with the audio's vehicle table (NocturneScore.Vehicles).
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class TrainTests
    {
        static Mesh s_mesh;
        static Vector3[] s_v;

        static Vector3[] Verts()
        {
            if (s_v != null) return s_v;
            var mb = new MeshBuilder();
            TrainBuilder.Train(mb);
            s_v = mb.Positions.ToArray();
            return s_v;
        }

        [Test]
        public void Table_14_5_4()
        {
            string[] names = { "T1", "T2", "T3", "T4", "T5", "T6", "T7" };
            double[] sl = { 584.5, 752.0, 639.5, 690.7, 600.0, 695.0, 800.0 };
            int[] tube = { 0, 1, 0, 1, 2, 3, 0 };
            int[] dir = { -1, -1, -1, 1, -1, 1, 1 };
            double[] x0 = { 683.0, 683.0, 683.0, 384.0, 815.3, 215.3, 384.0 };
            Assert.AreEqual(7, TrainSchedule.Trains.Length);
            for (int i = 0; i < 7; i++)
            {
                var t = TrainSchedule.Trains[i];
                Assert.AreEqual(names[i], t.Name); Assert.AreEqual(sl[i], t.LaunchS, 1e-9, t.Name);
                Assert.AreEqual(tube[i], t.Tube, t.Name); Assert.AreEqual(dir[i], t.Dir, t.Name); Assert.AreEqual(x0[i], t.X0, 1e-4, t.Name);
                Assert.AreEqual(t.X0, TrainSchedule.NoseX(i, 0.0), 1e-9, "the nose starts on its origin portal plane");
                Assert.AreEqual(t.X0 + t.Dir * 38.0 * 3.0, TrainSchedule.NoseX(i, 3.0), 1e-9, "x0 -+ 38 tau");
                Assert.AreEqual(0.0, TrainSchedule.CaptureTau(i, t.LaunchS - 5.0), 1e-12);
                Assert.AreEqual(4.0, TrainSchedule.CaptureTau(i, t.LaunchS + 10.0), 1e-12, "capture tau (s - sL) / 2.5");
                // the origin lies on or behind the origin mouth (inside the tunnel): nothing shows before the launch
                Assert.LessOrEqual(TrainSchedule.OpenDepth(t.Tube, t.X0), 1e-6, t.Name + " origin inside its portal");
                Assert.Less(TrainSchedule.ParkPos(i).y + 3.0f, -3300f, t.Name + " parked under the world while hidden (Q1 probes)");
            }
            Assert.AreEqual(174.0, TrainSchedule.Length, 1e-9);
            Assert.AreEqual(38.0, TrainSchedule.Speed, 1e-9);
            // tube geometry of 14.5.4 / 14.12
            Assert.AreEqual(9.0, 2.0 * TrainSchedule.Tubes[0].R, 0.02); Assert.AreEqual(10.0, 2.0 * TrainSchedule.Tubes[2].R, 0.02);
            Assert.AreEqual(384.0, TrainSchedule.Tubes[0].MouthLo, 1e-4); Assert.AreEqual(683.0, TrainSchedule.Tubes[0].MouthHi, 1e-4);
        }

        [Test]
        public void TTrain_MeetingPoints_Analytic_AndStepped()
        {
            var want = new Dictionary<string, double> { { "T1", 602.0 }, { "T3", 653.6 }, { "T4", 699.3 }, { "T2", 759.1 }, { "T7", 816.3 } };
            foreach (var kv in want)
            {
                int i = TrainSchedule.Index(kv.Key);
                double a25 = TrainSchedule.MeetS(i, 2.5), a15 = TrainSchedule.MeetS(i, 1.5);
                Debug.Log(FormattableString.Invariant($"[TR] {kv.Key} meets the walker at s {a25:F3} (2.5 m/s), {a15:F3} (1.5 m/s): {a25 - a15:F2} m apart"));
                Assert.AreEqual(kv.Value, a25, 0.5 * 2.5 * 0.2, kv.Key + " T-TRAIN (0.5 s)");
                Assert.LessOrEqual(Math.Abs(a25 - a15), 7.0, kv.Key + ": passes differ by 7 m or less (14.5.4)");
                // a stepped play-mode walk at 60 fps: the s where the nose first reaches the walker's x
                foreach (double v in new[] { 2.5, 1.5 })
                {
                    var st = new TrainState(); TrainSchedule.Reset(ref st);
                    double dt = 1.0 / 60.0, s = 560.0, hit = double.NaN;
                    int dirI = TrainSchedule.Trains[i].Dir;
                    for (int f = 0; f < 60 * 400 && double.IsNaN(hit); f++)
                    {
                        double tau = TrainSchedule.Step(i, ref st, s, dt);
                        double xw = s - NocturneConstants.EastLegOffset;
                        if (st.Phase == TrainPhase.Running && (TrainSchedule.NoseX(i, tau) - xw) * dirI >= 0.0) hit = s;
                        s += v * dt;
                    }
                    double w = TrainSchedule.MeetS(i, v);
                    Assert.AreEqual(w, hit, 0.5 * v, FormattableString.Invariant($"{kv.Key} stepped walk at {v} m/s within 0.5 s"));
                }
            }
            // T6: the nose under the glass strip's centre (x 531.3) at s 715.8; at both speeds the walker is on the strip (s 706-726)
            int t6 = TrainSchedule.Index("T6");
            double s25 = TrainSchedule.SAtNoseX(t6, 531.3, 2.5), s15 = TrainSchedule.SAtNoseX(t6, 531.3, 1.5);
            Debug.Log(FormattableString.Invariant($"[TR] T6 nose under the strip centre at s {s25:F2} (2.5 m/s), {s15:F2} (1.5 m/s)"));
            Assert.AreEqual(715.8, s25, 0.5 * 2.5 * 0.2);
            Assert.That(s15, Is.InRange(706.0, 726.0)); Assert.That(s25, Is.InRange(706.0, 726.0));
        }

        [Test]
        public void T1_VisibleHeadOn_AtLeast6s_AndKeyShotPositions()
        {
            int t1 = TrainSchedule.Index("T1");
            double meet = TrainSchedule.MeetS(t1, 2.5);
            double seconds = (meet - TrainSchedule.Trains[t1].LaunchS) / 2.5;
            double dist = TrainSchedule.Trains[t1].X0 - (TrainSchedule.Trains[t1].LaunchS - NocturneConstants.EastLegOffset);
            Debug.Log(FormattableString.Invariant($"[TR] T1 head-on for {seconds:F2} s from {dist:F1} m"));
            Assert.GreaterOrEqual(seconds, 6.0, "14.5.4 acceptance (2): T1 visible >= 6 s");
            Assert.Greater(dist, 270.0);
            // KS11 (s 600): nose x 447.4 (32 m ahead of the eye x 415.3, 12 m left); KS13 (s 716): T6's nose x 534.5, past the strip centre
            Assert.AreEqual(447.4, TrainSchedule.NoseX(t1, TrainSchedule.CaptureTau(t1, 600.0)), 0.05);
            int t6 = TrainSchedule.Index("T6");
            double n6 = TrainSchedule.NoseX(t6, TrainSchedule.CaptureTau(t6, 716.0));
            Assert.AreEqual(534.5, n6, 0.05);
            Assert.Less(TrainSchedule.TailX(t6, n6), 521.3, "T6's cars fill the strip's view at KS13");
            // KS12 (s 660): T3 in the north tube across the view (its body over x 400-545)
            int t3 = TrainSchedule.Index("T3");
            double n3 = TrainSchedule.NoseX(t3, TrainSchedule.CaptureTau(t3, 660.0));
            Assert.Less(n3, 475.3); Assert.Greater(TrainSchedule.TailX(t3, n3), 475.3, "T3 abeam the KS12 eye");
        }

        [Test]
        public void NeverOutsideItsTube_DarkInTunnels_HiddenBeforeAndAfter()
        {
            var v = Verts();
            int outside = 0, openAtLaunch = 0, drawnWhenHidden = 0;
            string first = "";
            for (int i = 0; i < TrainSchedule.Trains.Length; i++)
            {
                var spec = TrainSchedule.Trains[i];
                var tb = TrainSchedule.Tubes[spec.Tube];
                double hid = TrainSchedule.HiddenTau(i);
                float dir = spec.Dir;                                      // FrameRot: identity (east) or 180 deg about y (west)
                double lo = tb.EndLo - TrainSchedule.ClipBehindEnd, hi = tb.EndHi + TrainSchedule.ClipBehindEnd, rMax = tb.R - 0.3;
                const int samples = 80;
                for (int k = 0; k <= samples; k++)
                {
                    double tau = hid * k / samples;
                    Vector3 o = TrainSchedule.FramePos(i, tau);
                    for (int j = 0; j < v.Length; j++)
                    {
                        Vector3 p = v[j];
                        double wx = o.x + dir * p.x, wy = o.y + p.y, wz = o.z + dir * p.z;
                        if (wx < lo || wx > hi) continue;                  // behind a clip plane: never drawn
                        double r = Math.Sqrt((wy - tb.AxisY) * (wy - tb.AxisY) + (wz - tb.AxisZ) * (wz - tb.AxisZ));
                        if (r > rMax) { outside++; if (first.Length == 0) first = FormattableString.Invariant($"{spec.Name} ({wx:F2}, {wy:F2}, {wz:F2}) r {r:F2}"); }
                        if (k == 0 && TrainSchedule.OpenDepth(spec.Tube, wx) > 0.06) openAtLaunch++;
                        if (k == samples) drawnWhenHidden++;
                    }
                }
                Assert.IsFalse(TrainSchedule.Active(i, 0.0)); Assert.IsFalse(TrainSchedule.Active(i, hid)); Assert.IsTrue(TrainSchedule.Active(i, 0.5 * hid));
            }
            Debug.Log(FormattableString.Invariant($"[TR] train vertices: outside the tube {outside}, in the open at launch {openAtLaunch}, drawn when hidden {drawnWhenHidden}"));
            Assert.AreEqual(0, outside, "a drawn train vertex outside its tube: " + first);
            Assert.AreEqual(0, openAtLaunch, "at the launch every drawn vertex lies behind the origin mouth (the nose on the portal plane)");
            Assert.AreEqual(0, drawnWhenHidden, "once hidden every vertex is behind the destination clip plane");
            // darkness: lit in the open, black 2.5 m behind a mouth
            for (int t = 0; t < TrainSchedule.Tubes.Length; t++)
            {
                var tb = TrainSchedule.Tubes[t];
                Assert.AreEqual(1.0, TrainSchedule.Lit(t, 0.5 * (tb.MouthLo + tb.MouthHi)), 1e-9);
                Assert.AreEqual(1.0, TrainSchedule.Lit(t, tb.MouthLo), 1e-9);
                Assert.AreEqual(0.0, TrainSchedule.Lit(t, tb.MouthLo - TrainSchedule.MouthFade), 1e-9);
                Assert.AreEqual(0.0, TrainSchedule.Lit(t, tb.MouthHi + TrainSchedule.MouthFade + 0.1), 1e-9);
                Assert.Greater(tb.MouthLo - tb.EndLo, TrainSchedule.MouthFade + 1.0, tb.Name + ": the tunnel is deeper than the fade");
                Assert.Greater(tb.EndHi - tb.MouthHi, TrainSchedule.MouthFade + 1.0, tb.Name + ": the tunnel is deeper than the fade");
            }
        }

        [Test]
        public void StateMachine_LaunchResetRelaunch()
        {
            int i = TrainSchedule.Index("T1");
            double sl = TrainSchedule.Trains[i].LaunchS;
            var st = new TrainState(); TrainSchedule.Reset(ref st);
            Assert.AreEqual(0.0, TrainSchedule.Step(i, ref st, sl - 1.0, 0.1)); Assert.AreEqual(TrainPhase.Waiting, st.Phase);
            TrainSchedule.Step(i, ref st, sl + 0.01, 0.1); Assert.AreEqual(TrainPhase.Running, st.Phase);
            double tau = TrainSchedule.Step(i, ref st, sl + 1.0, 0.5); Assert.AreEqual(0.5, tau, 1e-9);
            // walking back below sL - 12 while it runs: the reset waits until the train is invisible
            tau = TrainSchedule.Step(i, ref st, sl - 13.0, 1.0); Assert.AreEqual(TrainPhase.Running, st.Phase); Assert.AreEqual(1.5, tau, 1e-9);
            for (int k = 0; k < 40 && st.Phase == TrainPhase.Running; k++) TrainSchedule.Step(i, ref st, sl - 13.0, 0.5);
            Assert.AreEqual(TrainPhase.Waiting, st.Phase, "reset applied once the train is hidden");
            TrainSchedule.Step(i, ref st, sl + 0.5, 0.1); Assert.AreEqual(TrainPhase.Running, st.Phase, "relaunch at the next crossing");
            for (int k = 0; k < 40 && st.Phase == TrainPhase.Running; k++) TrainSchedule.Step(i, ref st, sl + 30.0, 0.5);
            Assert.AreEqual(TrainPhase.Done, st.Phase);
            Assert.AreEqual(TrainSchedule.HiddenTau(i), TrainSchedule.Step(i, ref st, sl + 31.0, 0.5), 1e-9, "done stays hidden");
            // a walk that starts beyond the launch launches at once
            var st2 = new TrainState(); TrainSchedule.Reset(ref st2);
            TrainSchedule.Step(i, ref st2, sl + 5.0, 0.016); Assert.AreEqual(TrainPhase.Running, st2.Phase);
        }

        [Test]
        public void Lights_FadeToZeroAtTheMouths()
        {
            for (int t = 0; t < TrainSchedule.Tubes.Length; t++)
            {
                var tb = TrainSchedule.Tubes[t];
                Assert.AreEqual(0.0, TrainSchedule.LightFade(t, tb.MouthLo + TrainBuilder.HeadAhead, TrainDriver.HeadStart, TrainDriver.HeadRamp), 1e-9, "headlight at launch");
                Assert.AreEqual(0.0, TrainSchedule.LightFade(t, tb.MouthHi - 1.0, TrainDriver.HeadStart, TrainDriver.HeadRamp), 1e-9);
                Assert.AreEqual(0.0, TrainSchedule.LightFade(t, tb.MouthLo - 5.0, TrainDriver.WashStart, TrainDriver.WashRamp), 1e-9, "inside a tunnel");
                Assert.AreEqual(1.0, TrainSchedule.LightFade(t, 0.5 * (tb.MouthLo + tb.MouthHi), TrainDriver.HeadStart, TrainDriver.HeadRamp), 1e-9);
            }
            Assert.LessOrEqual(TrainBuilder.HeadRange, 25.0f); Assert.LessOrEqual(TrainBuilder.HeadIntensity, 3.0f, "LOOK_V2 6 row 4: r 25 i 3");
        }

        [Test]
        public void Body_174m_3_4m_AxisBox_ValidMesh()
        {
            var mb = new MeshBuilder();
            TrainBuilder.Train(mb);
            var rep = MeshValidation.Validate(mb, "TrainTests_train");
            Assert.IsTrue(rep.Ok, rep.ToString());
            float xMin = float.MaxValue, xMax = float.MinValue, zMax = 0f, yMinBody = 0f, yMaxBody = 0f;
            foreach (var p in mb.Positions)
            {
                xMin = Mathf.Min(xMin, p.x); xMax = Mathf.Max(xMax, p.x);
                if (p.y > -1.71f && p.y < 1.71f) zMax = Mathf.Max(zMax, Mathf.Abs(p.z));
                if (Mathf.Abs(p.z) > 1.5f) { yMinBody = Mathf.Min(yMinBody, p.y); yMaxBody = Mathf.Max(yMaxBody, p.y); }
            }
            Debug.Log(FormattableString.Invariant($"[TR] train x {xMin:F3}..{xMax:F3}, half width {zMax:F3}, {mb.TriangleCount} triangles"));
            Assert.AreEqual(-174.0, xMin, 0.06, "174 m (7 x 24 m + 6 m nose)");
            Assert.That(xMax, Is.InRange(0.0f, 0.07f), "the nose tip (lamp lenses 4 cm proud) on the frame origin");
            Assert.AreEqual(1.7, zMax, 0.005, "3.4 m wide");
            Assert.LessOrEqual(mb.TriangleCount, 8000, "about 5 k triangles per train");
            // the guideway and the linings stay inside their tubes; the gangways are 0.6 m
            for (int t = 0; t < TrainSchedule.Tubes.Length; t++)
            {
                var g = new MeshBuilder();
                TrainBuilder.Guideway(g, t);
                var tb = TrainSchedule.Tubes[t];
                foreach (var p in g.Positions)
                {
                    double r = Math.Sqrt((p.y - tb.AxisY) * (p.y - tb.AxisY) + (p.z - tb.AxisZ) * (p.z - tb.AxisZ));
                    Assert.LessOrEqual(r, tb.R + 1e-3, tb.Name + " guideway inside the glass");
                }
                Assert.IsTrue(MeshValidation.Validate(g, "guide").Ok);
            }
            TrainBuilder.CarSpan(0, out float r0, out float f0); TrainBuilder.CarSpan(1, out float r1, out float f1);
            Assert.AreEqual(0.6, r0 - f1, 1e-4, "0.6 m gangway");
            Assert.AreEqual(-6.0, f0, 1e-4, "car 0 behind the 6 m nose");
        }

        [Test]
        public void BridgeSway_2to3cm_At0_12Hz_OnlyOnTheSpan()
        {
            double peak = 0.0, peakMid = 0.0, vMax = 0.0;
            double sMid = 0.5 * (SkybridgeDims.SpanX0 + SkybridgeDims.SpanX1) + NocturneConstants.EastLegOffset;
            for (int k = 0; k < 20000; k++)
            {
                double t = k * 0.01;
                double a = BridgeSway.Offset(sMid, t), b = BridgeSway.Offset(sMid, t + 0.01);
                peakMid = Math.Max(peakMid, Math.Abs(a));
                vMax = Math.Max(vMax, Math.Abs(b - a) / 0.01);
                for (double s = 540.0; s <= 900.0; s += 20.0) peak = Math.Max(peak, Math.Abs(BridgeSway.Offset(s, t)));
            }
            Debug.Log(FormattableString.Invariant($"[TR] bridge sway: peak {peak * 100:F2} cm, mid-span {peakMid * 100:F2} cm, lateral speed <= {vMax * 100:F2} cm/s"));
            Assert.LessOrEqual(peak, 0.0301, "D-034 4.3: 2-3 cm");
            Assert.GreaterOrEqual(peakMid, 0.0295, "3 cm at mid-span in a gust");
            Assert.LessOrEqual(vMax, 0.025);
            for (double t = 0.0; t < 30.0; t += 0.37)
            {
                Assert.AreEqual(0.0, BridgeSway.Offset(520.0, t), 1e-12, "no sway before beat 4");
                Assert.AreEqual(0.0, BridgeSway.Offset(SkybridgeDims.SpanX0 + NocturneConstants.EastLegOffset, t), 1e-12, "the bearing is still");
                Assert.AreEqual(0.0, BridgeSway.Offset(900.0, t), 1e-12, "no sway in the garden");
            }
        }

        [Test]
        public void Agrees_WithTheAudioVehicleTable()
        {
            foreach (var veh in NocturneScore.Vehicles)
            {
                if (!veh.Train) continue;
                int i = TrainSchedule.Index(veh.Name);
                Assert.GreaterOrEqual(i, 0, veh.Name);
                var t = TrainSchedule.Trains[i]; var tb = TrainSchedule.Tubes[t.Tube];
                Assert.AreEqual(t.LaunchS, veh.SLaunch, 1e-9, veh.Name); Assert.AreEqual(t.X0, veh.X0, 1e-4, veh.Name);
                Assert.AreEqual(t.Dir, veh.Dir, 1e-9, veh.Name); Assert.AreEqual(TrainSchedule.Speed, veh.Speed, 1e-9);
                Assert.AreEqual(TrainSchedule.Length, veh.Length, 1e-9); Assert.AreEqual(tb.AxisY, veh.TrackY, 1e-4); Assert.AreEqual(tb.AxisZ, veh.TrackZ, 1e-4);
            }
        }
    }
}
