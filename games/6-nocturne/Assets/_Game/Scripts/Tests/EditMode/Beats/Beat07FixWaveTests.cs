// Beat07FixWaveTests (HT, fix wave p4_01): C27 the koi's eye follows the walker at <= 6 deg/s with no step (DREAD 5.2, lead
// ruling), C35 the balcony railing's side returns at a pitch that stays >= 3 px from KS19 at 1080p, D-034 6 the empty lit room
// stays inside the tenement, under the goal room and clear of the other openings.
using System;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class Beat07FixWaveTests
    {
        static readonly D3 Ks18Eye = new D3(998.18, 1.7, 387.88);

        [Test]
        public void KoiEye_TurnsTowardTheCamera_AtMost6DegPerSecond_NoStep()
        {
            // a 12 s walk at 2.5 m/s and 60 fps through KS18 (the seq_koi_ks18 window), capture mode (a pure function of s)
            var st = new KoiEyeState();
            double s0 = 1350.0 - 15.0, dt = 1.0 / 60.0, maxStep = 0.0, maxOff = 0.0, errEnd = 0.0, errStart = 0.0;
            D3 prevO = default, prevI = default;
            for (int i = 0; i <= 720; i++)
            {
                double s = s0 + 2.5 * dt * i;
                D3 cam = new D3(Ks18Eye.x + (s - 1350.0), Ks18Eye.y, Ks18Eye.z);
                double phi = KoiMath.HeadAzimuth(KoiMath.PhaseU(s)), tw = KoiMath.WaveTimeCapture(s);
                if (i == 0)
                {
                    // start the gaze 40 deg off its target, so the slew is exercised
                    st.Step(phi, tw, cam, s, dt, true);
                    D3 t = st.outer;
                    st.outer = KoiEye.Slew(t, new D3(0, 0, 1), 40.0 * NocMath.Deg);
                    st.inner = KoiEye.Slew(st.inner, new D3(0, 0, 1), 40.0 * NocMath.Deg);
                    errStart = KoiEye.AngleDeg(st.outer, KoiEye.Target(phi, tw, cam, 0));
                }
                else
                {
                    st.Step(phi, tw, cam, s, dt, true);
                    maxStep = Math.Max(maxStep, Math.Max(KoiEye.AngleDeg(prevO, st.outer), KoiEye.AngleDeg(prevI, st.inner)));
                }
                Vector4 p = KoiEye.Pupil(st.outer, st.inner);
                maxOff = Math.Max(maxOff, Math.Max(Math.Sqrt(p.x * p.x + p.y * p.y), Math.Sqrt(p.z * p.z + p.w * p.w)));
                prevO = st.outer; prevI = st.inner;
                errEnd = KoiEye.AngleDeg(st.outer, KoiEye.Target(phi, tw, cam, 0));
            }
            Debug.Log(FormattableString.Invariant($"[NOC] koi eye: largest turn {maxStep * 60.0:F3} deg/s (limit {KoiEye.MaxRateDeg}), pupil offset <= {maxOff:F3} m (0.3 R = {KoiEye.Reach * KoiShaderMath.EyeR:F2}), gaze error {errStart:F1} -> {errEnd:F2} deg"));
            Assert.LessOrEqual(maxStep, KoiEye.MaxRateDeg / 60.0 + 1e-6, "pupil turn rate");
            Assert.LessOrEqual(maxOff, KoiEye.Reach * KoiShaderMath.EyeR + 1e-4, "pupil stays inside 0.3 R");
            Assert.Less(errEnd, 1.0, "the gaze catches the walker and keeps following");
        }

        [Test]
        public void KoiEye_StillSnaps_SequenceSlews()
        {
            var st = new KoiEyeState();
            D3 cam = Ks18Eye;
            double phi = KoiMath.HeadAzimuth(KoiMath.PhaseU(1350.0)), tw = KoiMath.WaveTimeCapture(1350.0);
            st.Step(phi, tw, cam, 1350.0, 1.0 / 60.0, true);
            Assert.Less(KoiEye.AngleDeg(st.outer, KoiEye.Target(phi, tw, cam, 0)), 1e-6, "first frame snaps");
            double phi2 = KoiMath.HeadAzimuth(KoiMath.PhaseU(1400.0)), tw2 = KoiMath.WaveTimeCapture(1400.0);
            D3 cam2 = new D3(1048.18, 1.7, 387.88);
            st.Step(phi2, tw2, cam2, 1400.0, 1.0 / 60.0, true);
            Assert.Less(KoiEye.AngleDeg(st.outer, KoiEye.Target(phi2, tw2, cam2, 0)), 1e-6, "a new still (jump > 0.5 m) snaps: a still is a function of s");
            // play mode never snaps after the first frame
            var pl = new KoiEyeState();
            pl.Step(phi, tw, cam, 1350.0, 1.0 / 60.0, false);
            D3 before = pl.outer;
            pl.Step(phi2, tw2, cam2, 1400.0, 1.0 / 60.0, false);
            Assert.LessOrEqual(KoiEye.AngleDeg(before, pl.outer), KoiEye.MaxRateDeg / 60.0 + 1e-6);
        }

        [Test]
        public void Railing_SideReturns_ThreePixelPitchFromKs19()
        {
            Assert.GreaterOrEqual(HeartLayout.RodStepSide, 0.3);
            var one = new MeshBuilder();
            MeshKit.Cylinder(one, Vector3.zero, Vector3.up, 0.01f, 6, false);
            var rods = new MeshBuilder(); var rails = new MeshBuilder();
            OldQuarterBuilder.Railing(rods, rails);
            int n = rods.VertexCount / one.VertexCount;
            double side = (HeartLayout.BalX1 - 0.02) - (HeartLayout.BalX0 + HeartLayout.RailInset);
            double front = HeartLayout.BalW - 2.0 * HeartLayout.RailInset;
            int perSide = Mathf.RoundToInt((float)(side / HeartLayout.RodStepSide)), perFront = Mathf.RoundToInt((float)(front / HeartLayout.RodStep));
            Debug.Log(FormattableString.Invariant($"[NOC] railing: {n} rods ({perFront} front intervals at {front / perFront:F3} m, {perSide} side intervals at {side / perSide:F3} m)"));
            Assert.AreEqual(1 + perSide + perFront + perSide, n, "rods: one at the first corner, then each side's intervals");
            // KS19 eye (1 148.18, 1.7, 387.88): the left return (z = za) seen across its length; the projected pitch at 1080p
            double ex = 1148.18, ey = 1.7, ez = 387.88, za = HeartLayout.GoalZC - 0.5 * HeartLayout.BalW + HeartLayout.RailInset, y = 3.6 + 0.45;
            double xf = HeartLayout.BalX0 + HeartLayout.RailInset, pitch = side / perSide, minPx = double.MaxValue;
            double pxPerRad = 1080.0 / (66.0 * NocMath.Deg);
            for (int i = 0; i < perSide; i++)
            {
                D3 a = new D3(xf + pitch * i, y, za) - new D3(ex, ey, ez), b = new D3(xf + pitch * (i + 1), y, za) - new D3(ex, ey, ez);
                double ang = Math.Acos(NocMath.Clamp(D3.Dot(a.Normalized, b.Normalized), -1.0, 1.0));
                minPx = Math.Min(minPx, ang * pxPerRad);
            }
            Debug.Log(FormattableString.Invariant($"[NOC] railing side return from KS19: smallest projected rod pitch {minPx:F2} px at 1080p"));
            Assert.GreaterOrEqual(minPx, 3.0);
        }

        [Test]
        public void EmptyRoom_InsideTheTenement_UnderTheGoalRoom_ClearOfOtherOpenings()
        {
            var shell = new MeshBuilder(); var wood = new MeshBuilder(); var steel = new MeshBuilder(); var bulb = new MeshBuilder();
            OldQuarterBuilder.EmptyRoom(shell, wood, steel, bulb);
            foreach (var mb in new[] { shell, wood, steel, bulb })
            {
                Assert.Greater(mb.VertexCount, 0);
                foreach (var p in mb.Positions)
                {
                    Vector3 w = p + OldQuarterBuilder.Origin;
                    Assert.GreaterOrEqual(w.x, (float)(HeartLayout.TenX0 + HeartLayout.DarkRevealD) - 1e-3f);
                    Assert.LessOrEqual(w.x, (float)HeartLayout.TenX1);
                    Assert.GreaterOrEqual(w.y, -1e-3f);
                    Assert.Less(w.y, (float)HeartLayout.RoomY0, "under the goal room's floor");
                    Assert.GreaterOrEqual(w.z, (float)HeartLayout.EmptyZ0 - 1e-3f);
                    Assert.LessOrEqual(w.z, (float)HeartLayout.EmptyZ1 + 1e-3f);
                }
            }
            var own = OldQuarterBuilder.EmptyRoomOpening();
            Assert.IsTrue(OldQuarterBuilder.IsEmptyRoom(own), "the empty room's opening exists");
            Assert.IsTrue(own.z0 > HeartLayout.EmptyZ0 && own.z1 < HeartLayout.EmptyZ1 && own.y1 < HeartLayout.EmptyY1);
            foreach (var o in OldQuarterBuilder.Openings())
            {
                if (OldQuarterBuilder.IsEmptyRoom(o) || o.y0 >= HeartLayout.EmptyY1) continue;
                bool clear = o.z1 <= HeartLayout.EmptyZ0 || o.z0 >= HeartLayout.EmptyZ1;
                Assert.IsTrue(clear, FormattableString.Invariant($"opening z {o.z0:F2}-{o.z1:F2} storey {o.storey} looks into the empty room"));
            }
            Vector3 lamp = OldQuarterBuilder.EmptyLampPos;
            Bounds bb = new Bounds(bulb.Positions[0] + OldQuarterBuilder.Origin, Vector3.zero);
            foreach (var p in bulb.Positions) bb.Encapsulate(p + OldQuarterBuilder.Origin);
            Assert.Less((bb.center - lamp).magnitude, 0.01f, "the room light sits in the bulb");
            // the lamp is cold: the goal window stays the only warm window (DREAD 6, 14.5.7)
            Color.RGBToHSV(G4Kit.Lin(NocPal.WinCold), out float h, out _, out _);
            Assert.IsTrue(h * 360f > 150f && h * 360f < 260f, "the empty room's lamp is cold (hue " + (h * 360f).ToString("F1") + ")");
        }
    }
}
