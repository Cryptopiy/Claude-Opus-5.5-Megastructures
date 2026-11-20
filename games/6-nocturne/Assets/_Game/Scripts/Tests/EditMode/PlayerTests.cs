using System;
using NUnit.Framework;

namespace Megastructure.Tests
{
    /// <summary>
    /// Player maths (the pattern of Thread's PlayerTests, IMPORT_PLAN section 8): angle wrapping, the critically damped helpers,
    /// the stride and head bob of 14.1 / IMPORT_PLAN 3.6 (continuous in the speed, Part 3.1 (e)), the 14.4 surface table and the
    /// rate limiter of the gaze follower.
    /// </summary>
    public sealed class PlayerTests
    {
        [Test]
        public void WrapDegIsInMinus180To180()
        {
            Assert.AreEqual(0.0, PlayerMath.WrapDeg(360.0), 1e-12);
            Assert.AreEqual(-180.0, PlayerMath.WrapDeg(180.0), 1e-12);
            Assert.AreEqual(170.0, PlayerMath.WrapDeg(-190.0), 1e-12);
            Assert.AreEqual(-20.0, NocMath.DeltaAngle(10.0, 350.0), 1e-12);
        }

        [Test]
        public void CritDampNeverOvershootsFromRest()
        {
            double x = 0, v = 0;
            for (int i = 0; i < 600; i++)
            {
                PlayerMath.CritDamp(ref x, ref v, 1.0, 0.2, 1.0 / 60.0);
                Assert.LessOrEqual(x, 1.0 + 1e-12);
            }
            Assert.AreEqual(1.0, x, 1e-6);
            double y = 0, w = 0;
            for (int i = 0; i < 600; i++) { y = NocMath.CriticalDamp(y, ref w, 1.0, 2.5, 1.0 / 60.0); Assert.LessOrEqual(y, 1.0 + 1e-12); }
            Assert.AreEqual(1.0, y, 1e-4);
        }

        [Test]
        public void StrideIsContinuousInSpeed()
        {
            Assert.AreEqual(0.75, PlayerMath.Stride(1.5), 1e-12);
            Assert.AreEqual(0.85, PlayerMath.Stride(2.5), 1e-12);
            Assert.AreEqual(0.80, PlayerMath.Stride(2.0), 1e-12);
            double prev = PlayerMath.Stride(0);
            for (double v = 0; v <= 3; v += 0.01) { double s = PlayerMath.Stride(v); Assert.GreaterOrEqual(s, prev); Assert.Less(s - prev, 0.002); prev = s; }
        }

        [Test]
        public void HeadBobTinyAndZeroAtRest()
        {
            for (double ph = 0; ph < 1; ph += 0.01)
            {
                Assert.AreEqual(0.0, PlayerMath.HeadBob(ph, 0.0), 0.0);
                Assert.LessOrEqual(Math.Abs(PlayerMath.HeadBob(ph, 2.5)), 0.5 * NocturneConstants.HeadBobMax);
            }
        }

        [Test]
        public void SurfacesOf14_4()
        {
            Assert.AreEqual(FootSurface.WetConcrete, NocEvents.SurfaceAt(10));
            Assert.AreEqual(FootSurface.WetStone, NocEvents.SurfaceAt(300));
            Assert.AreEqual(FootSurface.WetStone, NocEvents.SurfaceAt(500));
            Assert.AreEqual(FootSurface.Steel, NocEvents.SurfaceAt(600));
            Assert.AreEqual(FootSurface.Glass, NocEvents.SurfaceAt(716));
            Assert.AreEqual(FootSurface.Garden, NocEvents.SurfaceAt(900));
            Assert.AreEqual(FootSurface.WetStone, NocEvents.SurfaceAt(1100));
            Assert.AreEqual(FootSurface.PlazaStone, NocEvents.SurfaceAt(1400));
        }

        [Test]
        public void LimitRatePinsTheStep()
        {
            double x = 10, v = 50;
            NocMath.LimitRate(ref x, ref v, 0.0, 22.0, 1.0 / 60.0);
            Assert.AreEqual(22.0 / 60.0, x, 1e-12); Assert.AreEqual(22.0, v, 1e-12);
            x = -10; v = -50;
            NocMath.LimitRate(ref x, ref v, 0.0, 18.0, 1.0 / 60.0);
            Assert.AreEqual(-18.0 / 60.0, x, 1e-12); Assert.AreEqual(-18.0, v, 1e-12);
            x = 0.1; v = 3;
            NocMath.LimitRate(ref x, ref v, 0.0, 22.0, 1.0 / 60.0);
            Assert.AreEqual(0.1, x, 1e-12); Assert.AreEqual(3.0, v, 1e-12);
        }

        [Test]
        public void CorridorSegmentsCoverThePath()
        {
            var b = PathGuide.SegmentBounds();
            Assert.AreEqual(0.0, b[0], 1e-12);
            Assert.AreEqual(NocLanes.Path.Length, b[b.Count - 1], 1e-9);
            for (int i = 1; i < b.Count; i++)
            {
                double len = b[i] - b[i - 1];
                Assert.Greater(len, 0.0);
                Assert.LessOrEqual(len, PathGuide.MaxSegment + 1e-9);
                if (NocLanes.Path.OnArc(0.5 * (b[i] + b[i - 1]))) Assert.LessOrEqual(len, PathGuide.CurvedSegment + 1e-9, "arc segment at s " + b[i - 1]);
            }
            // the wall face keeps radius + gap from the lane edge
            for (double s = 0; s <= NocLanes.Path.Length; s += 7.3)
                Assert.GreaterOrEqual(PathGuide.WallOffset(s) - NocLanes.LaneHalfWidth(s), NocturneConstants.PlayerRadius + PlayerTuning.WallGap - 1e-9);
        }

        /// <summary>Audit S3 item 15: the wall gap is the controller skin, so the reachable centre is exactly the 14.4 lane and
        /// the collider gap is lane + 2 (radius + skin) = lane + 0.76 m on a straight constant-width leg.</summary>
        [Test]
        public void WallGapIsTheControllerSkin()
        {
            Assert.AreEqual(PlayerTuning.ControllerSkin, PlayerTuning.WallGap, 0f);
            double s = 300.0;   // median, straight, half-width 11.0
            Assert.AreEqual(11.0, NocLanes.LaneHalfWidth(s), 1e-12);
            Assert.AreEqual(2 * 11.0 + 0.76, 2 * PathGuide.WallOffset(s), 1e-6);
        }

        /// <summary>14.6 / audit S2 item 1: beat roots stay active; the 8 m / 12 m window is for events only.</summary>
        [Test]
        public void BeatRootsStayActiveAndTheWindowIsForEvents()
        {
            var city = new UnityEngine.GameObject("CityTest");
            var host = new UnityEngine.GameObject("BeatDirectorTest");
            try
            {
                var ctx = new WorldContext { CityRoot = city.transform };
                for (int b = 0; b < NocturneConstants.BeatCount; b++) ctx.BeatRoot(b);
                ctx.BeatRoots[3].gameObject.SetActive(false);
                var bd = host.AddComponent<BeatDirector>();
                bd.Init(ctx);
                for (int b = 0; b < NocturneConstants.BeatCount; b++) Assert.IsTrue(ctx.BeatRoots[b].gameObject.activeSelf, "beat root " + b);
                Assert.IsTrue(BeatDirector.InEventWindow(1, 120.0 - 8.0));
                Assert.IsFalse(BeatDirector.InEventWindow(1, 120.0 - 8.01));
                Assert.IsTrue(BeatDirector.InEventWindow(1, 400.0 + 12.0));
                Assert.IsFalse(BeatDirector.InEventWindow(1, 400.0 + 12.01));
            }
            finally { UnityEngine.Object.DestroyImmediate(host); UnityEngine.Object.DestroyImmediate(city); }
        }

        /// <summary>GE's GB-19 requests (carried by FX, D-032 9): a -beat N start sits exactly on the boundary (the float feet
        /// position alone may project short of it), and the beat order accepts a walk that starts mid-path.</summary>
        [Test]
        public void BeatStart_SitsOnTheBoundaryAndOrderAcceptsAMidPathStart()
        {
            for (int b = 1; b < NocturneConstants.BeatCount; b++)
            {
                double s0 = NocturneConstants.BeatBoundaries[b];
                UnityEngine.Vector3 feet = NocLanes.CorridorToWorld(s0, 0.0, 0.0);
                double proj = NocLanes.Path.Project(feet.x, feet.z, s0, 25.0, out double dist);
                Assert.Less(dist, 1e-3, "feet on the centre line");
                Assert.AreEqual(s0, proj, 1e-3, "the float projection is within a millimetre");
                double held = PlayerMath.AnchoredS(proj, true, s0, 0.0);
                Assert.AreEqual(s0, held, 0.0, "beat " + (b + 1) + ": the start s is exact while the feet stand still");
                Assert.AreEqual(b, NocMath.BeatIndex(held), "the start lies in beat " + (b + 1) + ", not in the beat before");
                Assert.AreEqual(proj, PlayerMath.AnchoredS(proj, true, s0, PlayerMath.AnchorRadius), 0.0, "released once the feet move");
                Assert.AreEqual(proj, PlayerMath.AnchoredS(proj, false, s0, 0.0), 0.0);
            }
            // order: walk from s 0, and walks from beat 3 / beat 6
            for (int start = 0; start < NocturneConstants.BeatCount; start++)
            {
                for (int n = 0; start + n < NocturneConstants.BeatCount; n++) Assert.IsTrue(BeatDirector.OrderStepOk(start + n, n, start));
                if (start > 0) Assert.IsFalse(BeatDirector.OrderStepOk(start - 1, 0, start), "the beat before the start is out of order");
                if (start + 2 < NocturneConstants.BeatCount) Assert.IsFalse(BeatDirector.OrderStepOk(start + 2, 1, start), "a skipped beat");
            }
        }
    }
}
