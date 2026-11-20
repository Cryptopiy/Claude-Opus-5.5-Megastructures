// TramTests: the tram T_A of beat 2 (MASTER_PROMPT 14.5.2 "Moving", 14.7 H7, 14.10 T-TRAIN; owner BV, Phase 4 wave A) on
// TramSchedule, TramDriver.Place and the built module meshes (TramBuilder.Car):
//   * the schedule: x(tau) = 205 - 7 tau on the straight, capture mode tau = max(0, (s - 230) / 2.5), the head at x 109.8 at s 264,
//     abeam at s 272.0 (2.5 m/s) and 258.2 (1.5 m/s) analytically and in a stepped play-mode walk (within 0.5 s, T-TRAIN);
//   * the pass distance 32 +- 1 m (the track's distance from the walk axis; the body face 30.3 m);
//   * the state machine: launch on the first crossing of s 230 (or at once beyond it), the reset armed below s 218 and applied
//     only while the tram is invisible (no jump while it is on the street), the relaunch at the next crossing;
//   * never a pop: at launch every drawn vertex lies inside the terminus (x 204.9..214.47, behind the portal plane), once
//     hidden every vertex is clipped, the depot clip plane is deeper than any walker (s >= 218) can see into the tunnel, the
//     moving lights are off before they could shine out of a sealed mesh;
//   * the body: 60.0 x 3.4 x 3.6 m (y 0.3..3.9) on the straight, the pantograph's head under the contact wire, valid meshes, the
//     KS06 rectangle of the built modules (x -0.24..+0.23, y -0.43..-0.29, tolerance 0.06 as in Beat02BoulevardTests).
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class TramTests
    {
        [Test]
        public void Schedule_StraightAndCaptureMode()
        {
            Assert.AreEqual(205.0, TramSchedule.HeadX(0.0), 1e-9);
            Assert.AreEqual(205.0 - 7.0 * 10.0, TramSchedule.HeadX(10.0), 1e-9);
            Assert.AreEqual(0.0, TramSchedule.CaptureTau(120.0), 1e-12);
            Assert.AreEqual(0.0, TramSchedule.CaptureTau(230.0), 1e-12);
            Assert.AreEqual(10.0, TramSchedule.CaptureTau(255.0), 1e-12);
            // KS06 (s 264, capture mode): head x 109.8, tail 169.8 (14.5.2)
            double tau = TramSchedule.CaptureTau(264.0);
            Assert.AreEqual(13.6, tau, 1e-9);
            Assert.AreEqual(109.8, TramSchedule.HeadX(tau), 0.02);
            TramSchedule.Point(TramSchedule.HeadP(tau), out double x, out double z, out double hx, out _);
            Assert.AreEqual(109.8, x, 0.02); Assert.AreEqual(227.0, z, 1e-9); Assert.AreEqual(-1.0, hx, 1e-12);
            TramSchedule.Point(TramSchedule.HeadP(tau) - TramSchedule.Length, out double xt, out _, out _, out _);
            Assert.AreEqual(169.8, xt, 0.02);
        }

        [Test]
        public void Abeam_At272_And258_AndPassDistance32()
        {
            double a25 = TramSchedule.AbeamS(2.5), a15 = TramSchedule.AbeamS(1.5);
            Debug.Log(FormattableString.Invariant($"[NOC] tram abeam s {a25:F3} (2.5 m/s), {a15:F3} (1.5 m/s)"));
            Assert.AreEqual(272.0, a25, 0.5 * 2.5 * 0.1);       // T-TRAIN: within 0.5 s; 272.03 computed
            Assert.AreEqual(258.2, a15, 0.5 * 1.5 * 0.1);
            // a stepped play-mode walk at constant speed (60 fps): the s where the head first reaches the walker's x
            foreach (double v in new[] { 2.5, 1.5 })
            {
                var st = new TramState(); TramSchedule.Reset(ref st);
                double dt = 1.0 / 60.0, s = 200.0, hit = double.NaN;
                for (int i = 0; i < 60 * 200 && double.IsNaN(hit); i++)
                {
                    double tau = TramSchedule.Step(ref st, s, dt);
                    if (st.Phase == TramPhase.Running && TramSchedule.HeadX(tau) <= s - NocturneConstants.EastLegOffset) hit = s;
                    s += v * dt;
                }
                double want = v > 2.0 ? 272.0 : 258.2;
                Debug.Log(FormattableString.Invariant($"[NOC] tram abeam in a stepped walk at {v} m/s: s {hit:F3} ({(hit - want) / v:F3} s from {want})"));
                Assert.AreEqual(want, hit, 0.5 * v, "meeting point within 0.5 s (T-TRAIN)");
            }
            // 14.5.2 (5): the tram passes within 32 +- 1 m (track centre to the walk axis); the near body face at 30.3 m
            Assert.AreEqual(32.0, TramSchedule.TrackZ - TramSchedule.WalkAxisZ, 1.0);
            Assert.AreEqual(30.3, TramSchedule.TrackZ - 0.5 * TramSchedule.Width - TramSchedule.WalkAxisZ, 1e-9);
        }

        [Test]
        public void StateMachine_LaunchResetRelaunch()
        {
            var st = new TramState(); TramSchedule.Reset(ref st);
            const double dt = 1.0 / 60.0;
            // waiting before s 230
            for (double s = 200.0; s < 229.9; s += 0.05) Assert.AreEqual(0.0, TramSchedule.Step(ref st, s, dt));
            Assert.AreEqual(TramPhase.Waiting, st.Phase);
            TramSchedule.Step(ref st, 230.0, dt);
            Assert.AreEqual(TramPhase.Running, st.Phase, "launched at the crossing of s 230");
            // run 10 s, then the walker turns back below 218: the tram keeps running (visible on the street: no jump)
            double tau = 0.0;
            for (int i = 0; i < 600; i++) tau = TramSchedule.Step(ref st, 231.0, dt);
            double before = tau;
            tau = TramSchedule.Step(ref st, 217.0, dt);
            Assert.IsTrue(st.ResetArmed, "reset armed below s 218");
            Assert.AreEqual(TramPhase.Running, st.Phase, "no jump while visible");
            Assert.Greater(tau, before);
            // ... until it is hidden in the depot: then it is back at the terminus, waiting, and relaunches at the next crossing
            for (int i = 0; i < 60 * 60 && st.Phase == TramPhase.Running; i++) tau = TramSchedule.Step(ref st, 217.0, dt);
            Assert.AreEqual(TramPhase.Waiting, st.Phase, "reset applied once hidden");
            Assert.GreaterOrEqual(TramSchedule.HeadP(tau), 0.0);
            Assert.AreEqual(0.0, TramSchedule.Step(ref st, 225.0, dt));
            TramSchedule.Step(ref st, 230.5, dt);
            Assert.AreEqual(TramPhase.Running, st.Phase, "relaunch at the next crossing");
            // a reset while the tram still stands at the portal applies at once
            TramSchedule.Step(ref st, 216.0, dt);
            Assert.AreEqual(TramPhase.Waiting, st.Phase, "reset at the terminus is immediate");
            // a walk that starts beyond s 230 launches at once; an unbroken walk ends Done (hidden) and stays so
            var st2 = new TramState(); TramSchedule.Reset(ref st2);
            TramSchedule.Step(ref st2, 300.0, dt);
            Assert.AreEqual(TramPhase.Running, st2.Phase);
            for (int i = 0; i < 60 * 70; i++) TramSchedule.Step(ref st2, 300.0 + i * 0.04, dt);
            Assert.AreEqual(TramPhase.Done, st2.Phase);
            Assert.AreEqual(50.33, TramSchedule.HiddenTau, 0.01);
        }

        /// <summary>World vertices of every module for a head at p.</summary>
        static List<Vector3> TramVertices(double p, List<int> zoneOf = null)
        {
            var l = new List<Vector3>(4096);
            for (int k = 0; k < TramSchedule.Cars; k++)
            {
                var mb = new MeshBuilder();
                TramBuilder.Car(mb, k);
                TramDriver.Place(p, k, out Vector3 pos, out Quaternion rot);
                var m = Matrix4x4.TRS(pos, rot, Vector3.one);
                for (int i = 0; i < mb.Positions.Count; i++)
                {
                    l.Add(m.MultiplyPoint3x4(mb.Positions[i]));
                    zoneOf?.Add(Mathf.RoundToInt(mb.Colors[i].r / 32f));
                }
            }
            return l;
        }

        static bool Clipped(Vector3 v) => v.x > TramSchedule.ClipX || v.z > TramSchedule.ClipZ;

        [Test]
        public void NeverPops_ClipPlanesInsideSealedMeshes()
        {
            // at launch: everything drawn stands inside the terminus hall (behind the portal plane x 205, the nose lenses 5 cm proud)
            var v0 = TramVertices(0.0);
            int drawn = 0;
            foreach (var v in v0)
            {
                if (Clipped(v)) continue;
                drawn++;
                Assert.GreaterOrEqual(v.x, 204.85f, "drawn part at launch lies in the portal or the hall (lenses and coupler cover 0.11 m proud)");
                Assert.Less(v.y, (float)BoulevardLayout.PortalH, "under the portal head");
                Assert.Less(Mathf.Abs(v.z - 227f), 0.5f * (float)BoulevardLayout.PortalW, "inside the portal width");
            }
            Assert.Greater(drawn, 100, "the head module shows in the portal");
            // the terminus clip plane lies 2 cm behind the hall's east interior wall (x 214.45), inside the building
            Assert.AreEqual(BoulevardLayout.TerminusX1 - TramTerminus.EastRecess - TramTerminus.Reveal + 0.02, TramSchedule.ClipX, 0.005);
            Assert.Less(TramSchedule.ClipX, BoulevardLayout.TerminusX1);
            // once hidden: every vertex clipped
            foreach (var v in TramVertices(TramSchedule.HiddenP + 0.01)) Assert.IsTrue(Clipped(v), "hidden tram vertex " + v);
            Assert.IsTrue(TramSchedule.Hidden(TramSchedule.HeadP(TramSchedule.HiddenTau + 0.01)));
            // the depot clip plane: inside the tunnel, deeper than any walker at s >= 218 can see past the portal
            Assert.Greater(TramSchedule.ClipZ, TramSchedule.PortalZ);
            Assert.Less(TramSchedule.ClipZ, TramSchedule.TunnelZ1);
            double hw = 0.5 * TramSchedule.PortalW, maxDepth = 0.0;
            for (double s = 218.0; s <= 400.0; s += 2.0)
                for (double z = 183.0; z <= 207.0; z += 2.0)
                {
                    double xe = s - NocturneConstants.EastLegOffset;
                    // the deepest sight line: through the east jamb's inner edge to the west wall (the walker is east of the portal)
                    double edgeX = TramSchedule.PortalX + hw, wallX = TramSchedule.PortalX - hw;
                    double depth = (edgeX - wallX) * (TramSchedule.PortalZ - z) / (xe - edgeX);
                    maxDepth = Math.Max(maxDepth, depth);
                }
            Debug.Log(FormattableString.Invariant($"[NOC] depot tunnel: deepest sight line from s >= 218 {maxDepth:F2} m, clip plane {TramSchedule.ClipZ - TramSchedule.PortalZ:F1} m in"));
            Assert.Less(maxDepth + 1.0, TramSchedule.ClipZ - TramSchedule.PortalZ);
            // moving lights: the headlight is off 2 m before the nose reaches the portal plane; the saloon lights are off inside
            // the terminus (p <= -6) and the depot
            Assert.AreEqual(0.0, TramSchedule.HeadlightFade(TramSchedule.PortalP - 2.0), 1e-9);
            Assert.AreEqual(1.0, TramSchedule.HeadlightFade(200.0), 1e-9);
            Assert.AreEqual(0.0, TramSchedule.RideLightFade(-6.0), 1e-9);
            Assert.AreEqual(0.0, TramSchedule.RideLightFade(TramSchedule.PortalP - 1.0), 1e-9);
            Assert.AreEqual(1.0, TramSchedule.RideLightFade(100.0), 1e-9);
        }

        [Test]
        public void Body_Is60x3_4x3_6_OnTheStraight()
        {
            var zones = new List<int>();
            var vs = TramVertices(100.0, zones);
            float xmin = float.MaxValue, xmax = float.MinValue, zmin = float.MaxValue, zmax = float.MinValue, ymin = float.MaxValue, ymaxBody = float.MinValue, ymax = float.MinValue;
            for (int i = 0; i < vs.Count; i++)
            {
                var v = vs[i];
                ymax = Mathf.Max(ymax, v.y);
                if (zones[i] == TramBuilder.ZSteel || zones[i] == TramBuilder.ZDark && v.y > 3.9f) continue;   // roof gear, pantograph
                if (zones[i] == TramBuilder.ZDark && v.y <= 0.32f) continue;                                    // bogies under the body
                if (zones[i] == TramBuilder.ZHead || zones[i] == TramBuilder.ZTail) continue;                  // lamp lenses (proud)
                if (zones[i] == TramBuilder.ZDark && Mathf.Abs(v.y - 0.42f) < 0.1f) continue;                  // coupler covers (proud)
                xmin = Mathf.Min(xmin, v.x); xmax = Mathf.Max(xmax, v.x);
                zmin = Mathf.Min(zmin, v.z); zmax = Mathf.Max(zmax, v.z);
                ymin = Mathf.Min(ymin, v.y); ymaxBody = Mathf.Max(ymaxBody, v.y);
            }
            double head = TramSchedule.StartX - 100.0;
            Debug.Log(FormattableString.Invariant($"[NOC] tram body: x {xmin:F3}..{xmax:F3} (length {xmax - xmin:F3}), z {zmin:F3}..{zmax:F3} (width {zmax - zmin:F3}), y {ymin:F3}..{ymaxBody:F3}, top {ymax:F3}"));
            Assert.AreEqual(head, xmin, 0.02, "head end");
            Assert.AreEqual(60.0, xmax - xmin, 0.05, "length 60.0");
            Assert.AreEqual(3.4, zmax - zmin, 0.05, "width 3.4");
            Assert.AreEqual(0.3, ymin, 0.02); Assert.AreEqual(3.9, ymaxBody, 0.02);
            Assert.AreEqual(3.6, ymaxBody - ymin, 0.05, "height 3.6");
            Assert.Less(ymax, (float)TramSchedule.WireY, "the pantograph head stays under the contact wire");
            Assert.Greater(ymax, (float)TramSchedule.WireY - 0.05f, "and touches it");
            Assert.Less(TramSchedule.WireY + 0.2, BoulevardLayout.PortalH, "the wire passes the terminus portal");
            Assert.Less(TramSchedule.WireY + 0.2, TramSchedule.PortalH, "and the depot portal");
        }

        [Test]
        public void Meshes_Valid_AndKS06Rectangle()
        {
            long tris = 0;
            for (int k = 0; k < TramSchedule.Cars; k++)
            {
                var mb = new MeshBuilder();
                TramBuilder.Car(mb, k);
                var rep = MeshValidation.Validate(mb, "tramCar" + k);
                Assert.IsTrue(rep.Ok, rep.ToString());
                tris += mb.TriangleCount;
            }
            Debug.Log(FormattableString.Invariant($"[NOC] tram modules: {tris} triangles"));
            Assert.Less(tris, 6000);
            // KS06: the built modules at s 264 (capture mode) project inside the 14.5.2 rectangle
            var v6 = G1TestGeo.View(G1TestGeo.Shot("KS06_tram"));
            var pts = new List<Vector3>();
            var zones = new List<int>();
            var all = TramVertices(TramSchedule.HeadP(TramSchedule.CaptureTau(264.0)), zones);
            for (int i = 0; i < all.Count; i++) if (all[i].y <= 3.91f && all[i].y >= 0.29f && zones[i] != TramBuilder.ZSteel) pts.Add(all[i]);
            var r = G1TestGeo.Rect(v6, pts);
            G1TestGeo.Inside(r, -0.24, 0.23, -0.43, -0.29, 0.06, "KS06 tram (built modules)");
        }
    }
}
