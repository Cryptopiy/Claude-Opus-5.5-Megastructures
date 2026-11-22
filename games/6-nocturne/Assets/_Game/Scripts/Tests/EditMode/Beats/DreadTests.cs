// DreadTests: the D-034 objects of package DR (docs/DREAD.md 2.1 the hull over the rift, 3.1 the shadow below; owner DR, wave B) on
// DreadCourse, HullBuilder, ShadowBuilder and DreadLights:
//   * the clock: launch on the first crossing of s_L (or at once, at the cinematic pace, past it), real seconds after it, the
//     smooth start C1 (no velocity jump), the capture tau (s - s_L) / 2.5, and no pop: per 60 fps frame the hull moves <= 0.3 m;
//   * THE ARK's course (D-043): hidden until s 380, then down the rift from the north (-z), monotone and slowing, the bow over
//     z 195 at s 596 (2.5 m/s), the stern at s 1056 (the garden), 2-2.8 km out at KS08, nothing in KS10; its searchlights slow
//     (>= 20 s), pointing down, never containing the walker; its body inside the rift (80 m clear) and the volume y 300..600,
//     under 250 k triangles with its lights; NocArk / NocHull fields linear Vectors;
//   * the shadow's course (fix wave C42): under the KS13 strip (z 195) at both speeds, between the y -520 and y -880 layers; the
//     fade (0 before s 590 and after s 900, 1 over s 622-860), AB's occluder (half-width 63 m about x 470) and AT's haze occluder;
//   * the KS10 / KS12 / KS13 subject layer (x 560, y -520) is never dimmed (AB's NocAbyssOcclusion twin), the river x 470 / y -880 is;
//   * the bodies: valid meshes, hull + shadow < 60 000 triangles, the hull inside the rift (140 m from both faces), its lowest point
//     9 m over the boulevard's y 400 skyway lane (which crosses the rift at z 195) and 560+ while parked (inside the deck), keel bottom
//     y 418, belly light rows at 40 m pitch below the belly; the cabins' cab profile and the shafts' guide lights (DR-07);
//   * the veil: strength in (0, 1), the plane between the belly and the deck, the shape 1 inside r0 and 0 at the rim.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class DreadTests
    {
        const double Eye = 1.7;

        static double Elev(double ex, double ez, double x, double y, double z)
            => Math.Atan2(y - Eye, Math.Sqrt((x - ex) * (x - ex) + (z - ez) * (z - ez))) * 180.0 / Math.PI;

        static double Bearing(double ex, double ez, double x, double z) => Math.Atan2(x - ex, z - ez) * 180.0 / Math.PI;

        static double WrapDeg(double a) { a %= 360.0; if (a > 180.0) a -= 360.0; if (a < -180.0) a += 360.0; return a; }

        /// <summary>Whether direction (yaw, elev) projects inside a 16:9 frame of the pose (yaw0, pitch0, vFOV), with a margin (NDC).</summary>
        static bool InFrame(double yaw0, double pitch0, double fov, double yaw, double elev, double margin = 0.0)
        {
            double d2r = Math.PI / 180.0;
            var dir = new Vector3((float)(Math.Cos(elev * d2r) * Math.Sin(yaw * d2r)), (float)Math.Sin(elev * d2r), (float)(Math.Cos(elev * d2r) * Math.Cos(yaw * d2r)));
            var rot = Quaternion.Euler((float)-pitch0, (float)yaw0, 0f);
            Vector3 v = Quaternion.Inverse(rot) * dir;
            if (v.z <= 0f) return false;
            double t = Math.Tan(0.5 * fov * d2r);
            double ny = v.y / v.z / t, nx = v.x / v.z / (t * 16.0 / 9.0);
            return Math.Abs(nx) <= 1.0 - margin && Math.Abs(ny) <= 1.0 - margin;
        }

        static KeyShot Ks(string tag)
        {
            foreach (var k in NocturneConstants.KeyShots) if (k.tag == tag) return k;
            throw new Exception("no key shot " + tag);
        }

        // ------------------------------------------------------------------ the clock

        [Test]
        public void Clock_LaunchStepAndCapture()
        {
            Assert.AreEqual(0.0, DreadClock.CaptureTau(420.0, 430.0), 1e-12);
            Assert.AreEqual(36.0, DreadClock.CaptureTau(520.0, 430.0), 1e-9);
            var st = new DreadLaunchState();
            Assert.AreEqual(0.0, DreadClock.Step(ref st, 400.0, 0.1, 430.0), 1e-12);   // first frame before the launch: parked
            Assert.IsFalse(st.Launched);
            Assert.AreEqual(0.0, DreadClock.Step(ref st, 429.9, 0.1, 430.0), 1e-12);
            Assert.AreEqual(0.0, DreadClock.Step(ref st, 430.1, 0.1, 430.0), 1e-12);   // the crossing launches (tau 0)
            Assert.IsTrue(st.Launched);
            Assert.AreEqual(0.5, DreadClock.Step(ref st, 431.0, 0.5, 430.0), 1e-12);
            Assert.AreEqual(0.5, DreadClock.Step(ref st, 420.0, 0.0, 430.0), 1e-12);   // never a reset: walking back keeps tau
            Assert.AreEqual(1.5, DreadClock.Step(ref st, 419.0, 1.0, 430.0), 1e-12);
            var late = new DreadLaunchState();
            Assert.AreEqual(DreadClock.CaptureTau(560.0, 430.0), DreadClock.Step(ref late, 560.0, 0.1, 430.0), 1e-12);   // -beat 4 start
        }

        [Test]
        public void Clock_SmoothStartIsC1_AndNeverPops()
        {
            double v = 18.0, r = 10.0, e = 1e-6;
            Assert.AreEqual(0.0, DreadClock.Travel(0.0, v, r), 1e-12);
            Assert.AreEqual(DreadClock.Travel(r - e, v, r), DreadClock.Travel(r + e, v, r), 1e-4);
            double vl = (DreadClock.Travel(r, v, r) - DreadClock.Travel(r - 1e-4, v, r)) / 1e-4;
            double vr = (DreadClock.Travel(r + 1e-4, v, r) - DreadClock.Travel(r, v, r)) / 1e-4;
            Assert.AreEqual(v, vl, 1e-2); Assert.AreEqual(v, vr, 1e-2);
            // per 60 fps frame the Ark moves at most its launch speed / 60 (1.2 m) and the shadow 6 / 60 m (continuous, no pop)
            double maxH = 0.0, maxS = 0.0;
            for (int i = 1; i < 60 * 400; i++)
            {
                double t0 = (i - 1) / 60.0, t1 = i / 60.0;
                maxH = Math.Max(maxH, Math.Abs(HullCourse.SternZ(t1) - HullCourse.SternZ(t0)));
                maxS = Math.Max(maxS, Math.Abs(ShadowCourse.CentreZ(t1) - ShadowCourse.CentreZ(t0)));
            }
            Assert.LessOrEqual(maxH, HullCourse.Velocity(1e-9) / 60.0 + 1e-9);
            Assert.LessOrEqual(maxS, ShadowCourse.Speed / 60.0 + 1e-9);
        }

        // ------------------------------------------------------------------ THE ARK (D-043; replaces the 2.1 hull)

        [Test]
        public void Ark_Course_FromTheNorth_BowOverheadEarlyOnTheBridge_SternGoneInTheGarden()
        {
            // hidden (parked, renderers off) until the launch at s 380; launched it only moves toward -z, slowing to its cruise
            Assert.IsTrue(HullCourse.HiddenAt(HullCourse.TauAtS(250.0)), "KS04 (beat 2): no Ark");
            Assert.IsTrue(HullCourse.HiddenAt(HullCourse.TauAtS(310.0)), "KS05 (beat 2): no Ark");
            Assert.IsTrue(HullCourse.HiddenAt(HullCourse.TauAtS(379.0)));
            Assert.IsFalse(HullCourse.HiddenAt(HullCourse.TauAtS(400.0)));
            Assert.AreEqual(0.0, HullCourse.Travel(0.0), 1e-12);
            Assert.AreEqual(HullCourse.ParkBow, HullCourse.BowZ(0.0), 1e-9);
            double prevV = double.MaxValue, prevZ = double.MaxValue;
            for (double tau = 0.05; tau < 400.0; tau += 0.05)
            {
                double v = HullCourse.Velocity(tau), z = HullCourse.BowZ(tau);
                Assert.LessOrEqual(v, prevV + 1e-9, $"tau {tau}: the speed never rises");
                Assert.Less(z, prevZ, $"tau {tau}: always toward -z");
                Assert.GreaterOrEqual(v, HullCourse.Speed);
                prevV = v; prevZ = z;
                // speed = d travel / d tau (the closed form is consistent)
                double num = (HullCourse.Travel(tau + 1e-4) - HullCourse.Travel(tau - 1e-4)) / 2e-4;
                Assert.AreEqual(v, num, 1e-3);
            }
            // ARK.md: the bow over z 195 early on the bridge (s 590-640) at the cinematic pace, overhead at 22-29 m/s
            Assert.AreEqual(596.0, HullCourse.AbeamS(2.5), 1e-9);
            Assert.AreEqual(195.0, HullCourse.BowZAtS(596.0), 1e-6);
            Assert.AreEqual(195.0, HullCourse.BowZAtSpeed(HullCourse.AbeamS(1.5), 1.5), 1e-6);
            Assert.That(HullCourse.Velocity(HullCourse.BowPassTau), Is.InRange(22.0, 29.0));
            Assert.AreEqual(27.0, HullCourse.Speed, 1e-12);
            // the stern (the north end) leaves the bridge's sky while the walker is in the garden (s 880-1060)
            Assert.That(HullCourse.SternPassS(2.5), Is.InRange(880.0, 1060.0));
            Assert.AreEqual(195.0, HullCourse.SternZ(HullCourse.SternPassTau()), 1e-3);
            // beat 3: far up the rift in the haze at the Edge (>= 2.5 km at s 400), 2.0-2.8 km at KS08, then closing
            Assert.GreaterOrEqual(HullCourse.BowDistance(HullCourse.TauAtS(400.0)), 2500.0);
            Assert.That(HullCourse.BowDistance(HullCourse.TauAtS(428.0)), Is.InRange(2000.0, 2800.0));
            Assert.That(HullCourse.BowDistance(HullCourse.TauAtS(520.0)), Is.InRange(600.0, 1300.0));
            // the deck occluder fades in with the launch; no lift
            Assert.AreEqual(0.0, HullCourse.Emerge(0.0), 1e-12);
            Assert.AreEqual(1.0, HullCourse.Emerge(30.0), 1e-12);
            Assert.AreEqual(0.0, HullCourse.Lift(50.0), 1e-12);
            Assert.AreEqual(330.0, HullCourse.BellyY(50.0), 1e-12);
            Assert.AreEqual(515.3, HullCourse.AxisX, 1e-12);
            // KS10 (frame top +2 deg): no point of the ship's underside is inside the frame at either speed (its subject is the drop)
            KeyShot k10 = Ks("KS10_edge_plunge");
            foreach (double v in new[] { 2.5, 1.5 })
            {
                double tau = Math.Max(0.0, (k10.s - HullCourse.LaunchS) / v), bow = HullCourse.BowZ(tau);
                for (double lz = 0.0; lz <= HullCourse.Length; lz += 100.0)
                    for (double f = -1.0; f <= 1.0; f += 0.5)
                    {
                        double x = HullCourse.AxisX + f * HullBuilder.HalfBeamAt(lz), z = bow + lz;
                        Assert.IsFalse(InFrame(k10.yaw, k10.pitch, k10.fov, Bearing(k10.x, k10.z, x, z), Elev(k10.x, k10.z, x, 330.0, z)),
                                       $"KS10 at {v} m/s: underside point ({x:F0}, 330, {z:F0}) in frame");
                    }
            }
            // KS08 (the look up the rift, s 428): the bow is inside the frame
            KeyShot k08 = Ks("KS08_edge_flank");
            double b08 = HullCourse.BowZAtS(k08.s);
            Assert.IsTrue(InFrame(k08.yaw, k08.pitch, k08.fov, Bearing(k08.x, k08.z, HullCourse.AxisX, b08 + 300.0), Elev(k08.x, k08.z, HullCourse.AxisX, 400.0, b08 + 300.0)),
                          "KS08: the approaching bow in frame");
            Assert.IsFalse(HullCourse.Hidden(HullCourse.SternZAtS(1500.0)));
            Assert.IsTrue(HullCourse.Hidden(HullCourse.HideStern - 1.0));
        }

        [Test]
        public void Ark_Searchlights_SlowDownwardNeverOnTheWalker()
        {
            for (int i = 0; i < HullCourse.SearchCount; i++)
            {
                Assert.GreaterOrEqual(HullCourse.SearchPeriod(i), 20.0, "sweep period >= 20 s (D-010 / D-041)");
                Assert.Greater(HullCourse.SearchLocalZ(i), HullBuilder.SpineZ0, "on the spine (where the keel no longer rises)");
                Assert.Less(HullCourse.SearchLocalZ(i), HullBuilder.SpineZ1);
            }
            double cosLim = Math.Cos((HullCourse.SearchHalfAngle + 5.0) * Math.PI / 180.0);
            for (double tau = 0.5; tau < 320.0; tau += 0.5)
                for (int i = 0; i < HullCourse.SearchCount; i++)
                {
                    double t = 3.7 * tau;
                    HullCourse.Searchlight(i, tau, t, out Vector3 o, out Vector3 d, out double g);
                    Assert.AreEqual(1.0, d.magnitude, 1e-4);
                    Assert.Less(d.y, -0.85f, "pointing down into the abyss");
                    if (g <= 0.0) continue;
                    // never containing the walker anywhere in the rift: the pier and the bridge (x 215-815, z 195 +- 30, eye 1.7) and
                    // the garden notch (z 115-275) - a band of +-80 m about the walk line over the whole width
                    for (double wx = 215.0; wx <= 815.0; wx += 20.0)
                        for (double wz = 115.0; wz <= 275.0; wz += 20.0)
                        {
                            Vector3 e = new Vector3((float)wx, 1.7f, (float)wz) - o;
                            Assert.Less(Vector3.Dot(e.normalized, d), cosLim, $"lamp {i} at tau {tau} contains the walker ({wx}, {wz})");
                        }
                }
        }

        [Test]
        public void Ark_Body_DimensionsBudgetAndClearances()
        {
            var mb = HullBuilder.BuildHull();
            var rep = MeshValidation.Validate(mb, "ark");
            Assert.IsTrue(rep.Ok, rep.Describe());
            Vector3 lo = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), hi = -lo;
            foreach (var p in mb.Positions) { lo = Vector3.Min(lo, p); hi = Vector3.Max(hi, p); }
            Assert.AreEqual(0.0, lo.z, 1.5, "the prow at local z 0");
            Assert.AreEqual(HullCourse.Length, hi.z, 0.01);
            // beam <= 440 (80 m clear of both faces at the neck), all of it inside the Ark volume y 300..600 (AB's layers stay out)
            Assert.LessOrEqual(hi.x, HullCourse.HalfBeam + 1e-3);
            Assert.GreaterOrEqual(lo.x, -HullCourse.HalfBeam - 1e-3);
            Assert.GreaterOrEqual(HullCourse.AxisX + lo.x, 215.3 + 80.0 - 1e-3);
            Assert.LessOrEqual(HullCourse.AxisX + hi.x, 815.3 - 80.0 + 1e-3);
            Assert.GreaterOrEqual(lo.y, HullBuilder.LowestY, $"lowest point {lo.y:F1}");
            Assert.LessOrEqual(hi.y, 600.0);
            Assert.Greater(hi.y, 460.0, "its back is in the cloud deck");
            // the keel gondolas: between the spine and the pods, bottom >= 300 (the Ark volume), clear of the hangar bays' z
            Assert.GreaterOrEqual(HullBuilder.StepY[0] - HullBuilder.KeelGondolaHang - HullBuilder.KeelGondolaH, 300.0);
            Assert.Greater(HullBuilder.KeelGondolaX0, HullBuilder.SpineHalf);
            Assert.Less(HullBuilder.KeelGondolaX1, HullBuilder.PodX - HullBuilder.PodR);
            // the gondolas hang under the outer treads and never below y 380
            foreach (var g in HullBuilder.GondolaSpecs())
            {
                int k = (int)g[0];
                Assert.GreaterOrEqual(HullBuilder.StepY[k + 1] - g[5] - g[4], 380.0);
                Assert.Greater(HullBuilder.StepW[k + 1] - HullBuilder.StepW[k] - HullBuilder.RiserLean - 5.0, 12.0, "gondola width");
            }
            var lights = HullBuilder.BuildLights();
            var sb = ShadowBuilder.BuildBody();
            Assert.IsTrue(MeshValidation.Validate(sb, "shadow").Ok);
            int tris = mb.TriangleCount + 2 * lights.Count;
            Assert.Less(tris, 250000, "ARK.md budget: the Ark under 250 k triangles with its lights");
            TestContext.WriteLine($"ark {mb.TriangleCount} + lights {2 * lights.Count} = {tris} triangles; shadow {sb.TriangleCount}");
        }

        [Test]
        public void Ark_Lights_RowsUnderTheBody_SteadyAndBounded()
        {
            var lights = HullBuilder.BuildLights();
            Assert.That(lights.Count, Is.InRange(3000, 9000));
            int halos = 0, reds = 0;
            foreach (var l in lights)
            {
                Assert.That(l.Gain, Is.InRange(0.1f, 3.0f));
                Assert.LessOrEqual(l.Pos.y, 480f);
                Assert.GreaterOrEqual(l.Pos.y, 300f);
                Assert.LessOrEqual(Math.Abs(l.Pos.x), (float)HullCourse.HalfBeam);
                if (l.Halo) { halos++; Assert.LessOrEqual(l.Size, 20f); continue; }
                if (l.Colour == DreadLights.Red) reds++;
                Assert.That(l.Size, Is.InRange(1.5f, 3.0f));
            }
            Assert.AreEqual(2 * HullBuilder.PodZ.Length + 6 + 2, halos);
            Assert.Greater(reds, 100, "aviation reds on the widest tread");
            var mesh = DreadLights.ToMesh("t", lights, 100f);
            Assert.AreEqual(4 * lights.Count, mesh.vertexCount);
            UnityEngine.Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Hull_Veil_Parameters()
        {
            Assert.That(HullCourse.VeilK, Is.InRange(0.3, 0.7));
            Assert.IsFalse(HullCourse.VeilOn, "r02: the hull's own veil is off (AT's _NocDeckOcc darkens the deck at the hull)");
            Assert.That(HullCourse.DeckOccW, Is.InRange(0.2, 1.0));
            Assert.That(HullCourse.DeckOccHalf * 0.15, Is.InRange(HullCourse.HalfBeam, HullCourse.HalfBeam + 60.0), "AT's ellipse half-width vs the Ark");
            Assert.That(HullCourse.VeilR0, Is.InRange(0.1, 0.8));
            var veil = HullBuilder.BuildVeil();
            Assert.IsTrue(MeshValidation.Validate(veil, "veil").Ok);
        }

        /// <summary>Fix wave DR-08: the ambient fields are Vector properties (raw linear; a Color property is gamma-converted in this
        /// linear project: r01-r06 drew the hull's 0.06 as 0.005 and its paint 0.035 as 0.0027, a black void).</summary>
        [Test]
        public void Hull_AmbientFieldsAreLinearVectors()
        {
            foreach (var name in new[] { "Megastructure/NocHull", "Megastructure/NocArk" })
            {
                var hull = Shader.Find(name);
                Assert.IsNotNull(hull, name);
                foreach (var n in new[] { "_BellyLight", "_SideLight", "_TopLight" })
                {
                    int i = hull.FindPropertyIndex(n);
                    Assert.GreaterOrEqual(i, 0, n);
                    Assert.AreEqual(UnityEngine.Rendering.ShaderPropertyType.Vector, hull.GetPropertyType(i), name + " " + n);
                }
            }
            var body = Shader.Find("Megastructure/NocDreadBody");
            Assert.IsNotNull(body);
            int si = body.FindPropertyIndex("_Sheen");
            Assert.AreEqual(UnityEngine.Rendering.ShaderPropertyType.Vector, body.GetPropertyType(si), "_Sheen");
            // the belly's radiance (paint x light from below) is a dark surface, not the floor: 0.004..0.012 linear
            double rad = DreadBuilder.HullPaint.y * DreadBuilder.HullBellyLight.y;
            Assert.That(rad, Is.InRange(0.004, 0.012), $"belly radiance {rad:F4}");
            // the procedural lights' far mean stays a dark mass: window mean = gain x mean activity x opening share < 0.03 linear
            double winMean = DreadBuilder.ArkWinGain * (0.02 + 0.58 / 5.0) * 0.775 * (1.9 / 3.2) * (1.8 / 4.0);
            Assert.Less(winMean, 0.03, $"far window mean {winMean:F4}");
        }

        // ------------------------------------------------------------------ the shadow (3.1)

        [Test]
        public void Shadow_CourseUnderTheStrip_BothSpeeds()
        {
            KeyShot k13 = Ks("KS13_bridge_glass");
            Assert.AreEqual(ShadowCourse.ParkCentre, ShadowCourse.CentreZAtS(590.0), 1e-9);
            Assert.That(ShadowCourse.Speed, Is.InRange(4.0, 6.0));
            // C42: the centre is under the strip (z 195) at the cinematic KS13 (+-20 m) and, at 1.5 m/s, inside the strip's z band
            // (+-0.686 x the depth of its back: +-440 m) with the whole light row (+-270 m) still in it (+-170 m)
            Assert.AreEqual(195.0, ShadowCourse.CentreZAtSpeed(k13.s, 2.5), 20.0);
            Assert.AreEqual(195.0, ShadowCourse.CentreZAtSpeed(k13.s, 1.5), 170.0);
            for (double s = 712.0; s <= 730.0; s += 2.0)
                Assert.AreEqual(195.0, ShadowCourse.CentreZAtS(s), 60.0, $"s {s}: under the strip");
            // between the y -520 subject layer and the y -880 river (60 m clear of both), never out of the rift
            Assert.LessOrEqual(ShadowCourse.CentreY + ShadowCourse.HalfHeight * 1.1 + ShadowBuilder.CrestH, -520.0 - 60.0);
            Assert.GreaterOrEqual(ShadowCourse.CentreY - ShadowCourse.HalfHeight * 1.1, -880.0 + 60.0);
            double reach = ShadowCourse.HalfWidth * 1.1 + ShadowBuilder.FinReach(0.28);
            Assert.Greater(ShadowCourse.AxisX - reach, 215.3 + 50.0);
            Assert.Less(ShadowCourse.AxisX + reach, 815.3 - 50.0);
        }

        [Test]
        public void Shadow_FadeAndOccluder()
        {
            Assert.AreEqual(0.0, ShadowCourse.Fade(548.0), 1e-12);      // KS10: never there
            Assert.AreEqual(0.0, ShadowCourse.Fade(590.0), 1e-12);
            Assert.AreEqual(1.0, ShadowCourse.Fade(622.0), 1e-12);
            Assert.AreEqual(1.0, ShadowCourse.Fade(660.0), 1e-12);      // KS12
            Assert.AreEqual(1.0, ShadowCourse.Fade(718.0), 1e-12);      // KS13
            Assert.AreEqual(1.0, ShadowCourse.Fade(860.0), 1e-12);
            Assert.AreEqual(0.0, ShadowCourse.Fade(900.0), 1e-12);      // beat 5: gone
            Assert.AreEqual(Vector4.zero, ShadowCourse.Occluder(400.0, 0.0));
            Vector4 h = ShadowCourse.HazeOccluder(350.0, 0.5);
            Assert.AreEqual((float)ShadowCourse.AxisX, h.x, 1e-4f); Assert.AreEqual(350f, h.y, 1e-4f);
            Assert.AreEqual((float)ShadowCourse.OccluderHalf, h.z, 1e-4f);
            // AT's haze ellipse (beam 0.15 x half-length) ends 30 m short of the x 560 subject layer's rays (x 533+ at the plane y -650)
            Assert.Less(ShadowCourse.AxisX + 0.15 * ShadowCourse.OccluderHalf, 533.0 - 10.0);
            Assert.AreEqual((float)(0.5 * ShadowCourse.HazeOccW), h.w, 1e-5f);
            Assert.AreEqual(Vector4.zero, ShadowCourse.HazeOccluder(350.0, 0.0));
            Vector4 o = ShadowCourse.Occluder(350.0, 0.5);
            Assert.AreEqual((float)ShadowCourse.AxisX, o.x, 1e-4f); Assert.AreEqual(350f, o.y, 1e-4f);
            Assert.AreEqual((float)ShadowCourse.OccluderHalf, o.z, 1e-4f);
            Assert.AreEqual((float)(0.5 * ShadowCourse.OccluderStrength), o.w, 1e-5f);
            // the fade is slow: at 2.5 m/s it takes >= 12 s each way (no visible step)
            Assert.GreaterOrEqual((ShadowCourse.FadeIn1 - ShadowCourse.FadeIn0) / 2.5, 12.0);
            Assert.GreaterOrEqual((ShadowCourse.FadeOut1 - ShadowCourse.FadeOut0) / 2.5, 12.0);
            // AB's outline (half-width 0.18 x the half-length passed): inside the body's own half-width, and the x 560 layer outside it
            Assert.LessOrEqual(0.18 * ShadowCourse.OccluderHalf, ShadowCourse.HalfWidth);
            Assert.Less(ShadowCourse.AxisX + 0.18 * ShadowCourse.OccluderHalf, 560.0 - 20.0);
        }

        /// <summary>Twin of AB's NocAbyssOcclusion (NocStreak.shader).</summary>
        static double AbyssOcclusion(Vector4 occ, double x, double y, double z)
        {
            double hx = Math.Max(0.18 * occ.z, 1.0), hz = Math.Max(occ.z, 1.0);
            double r = Math.Sqrt((x - occ.x) * (x - occ.x) / (hx * hx) + (z - occ.y) * (z - occ.y) / (hz * hz));
            return 1.0 - Mathf.Clamp01(occ.w) * (1.0 - DreadClock.Smooth01(0.7, 1.0, r)) * (1.0 - DreadClock.Smooth01(-550.0, -450.0, y));
        }

        [Test]
        public void Shadow_KeepsTheSubjectLayersLit_AndDarkensTheDeepRiver()
        {
            for (double s = 600.0; s <= 900.0; s += 5.0)
            {
                Vector4 occ = ShadowCourse.Occluder(ShadowCourse.CentreZAtS(s), ShadowCourse.Fade(s));
                for (double z = -2000.0; z <= 3000.0; z += 25.0)
                {
                    // the KS10 / KS12 / KS13 subject layer (x 560, y -520) and the near layers (x 350 / 380 / 540 / 600) stay lit
                    Assert.AreEqual(1.0, AbyssOcclusion(occ, 560.0, -520.0, z), 1e-9, $"s {s} z {z}");
                    Assert.AreEqual(1.0, AbyssOcclusion(occ, 540.0, 30.0, z), 1e-9);
                }
            }
            // the river x 470 / y -880 goes dark under the body's middle at KS13 (the lower rivers going dark where it passes)
            Vector4 o13 = ShadowCourse.Occluder(ShadowCourse.CentreZAtS(718.0), 1.0);
            Assert.Less(AbyssOcclusion(o13, 470.0, -880.0, ShadowCourse.CentreZAtS(718.0)), 0.3);
            Assert.Greater(AbyssOcclusion(o13, 470.0, -880.0, ShadowCourse.CentreZAtS(718.0) + 700.0), 0.999, "lit beyond its ends");
        }

        [Test]
        public void Shadow_LightsDimAndOnItsBack()
        {
            var ls = ShadowBuilder.BuildLights();
            Assert.That(ls.Count, Is.InRange(5, 12));
            for (int i = 0; i < ls.Count; i++)
            {
                var l = ls[i];
                Assert.AreEqual(DreadLights.Cold, l.Colour);
                Assert.LessOrEqual(l.Gain, 0.8f);
                Assert.IsTrue(l.Halo, "C42: soft Gaussian glows, not star points");
                Assert.AreEqual(0f, l.Pos.x, 1e-6f, "one row along the spine");
                Assert.Greater(l.Pos.y, 0f);
                Assert.LessOrEqual(Math.Abs(l.Pos.z), ShadowCourse.HalfLength);
                if (i > 0) Assert.AreEqual(90f, l.Pos.z - ls[i - 1].Pos.z, 1e-3f);
                // 3-6 px across (FWHM 2.355 sigma) from the KS13 eye at about 650 m, 1080p (831 px per radian)
                double px = 2.355 * l.Size / 650.0 * 831.0;
                Assert.That(px, Is.InRange(3.0, 6.0), $"glow FWHM {px:F1} px");
            }
        }

        // ------------------------------------------------------------------ the falling panel (3.2)

        [Test]
        public void Panel_FallTumbleAndClearances()
        {
            Assert.AreEqual(470.0, PanelCourse.LaunchS, 1e-12);
            Vector3 p0 = PanelCourse.Centre(0.0);
            Assert.AreEqual(150.0, p0.y, 1e-4);
            Assert.Less(p0.x, 815.3f); Assert.Greater(p0.x, 814.5f);                         // on face B, its back 0.25 m proud
            // falls 600-800 m in 20 s and keeps falling into the haze
            double d20 = PanelCourse.Drop(20.0);
            Assert.That(d20, Is.InRange(600.0, 800.0), $"drop in 20 s {d20:F0}");
            Assert.Less(PanelCourse.Centre(PanelCourse.HideTau).y, -1500f);
            // continuous (no pop): per 60 fps frame the centre moves less than the terminal speed allows, the orientation < 2 deg
            for (int i = 1; i < 60 * 50; i++)
            {
                double t0 = (i - 1) / 60.0, t1 = i / 60.0;
                Assert.Less((PanelCourse.Centre(t1) - PanelCourse.Centre(t0)).magnitude, 50.0 / 60.0);
                Assert.Less(Quaternion.Angle(PanelCourse.Rotation(t1), PanelCourse.Rotation(t0)), 2.0f);
            }
            Assert.Less(Quaternion.Angle(PanelCourse.Rotation(0.0), Quaternion.identity), 1e-3f);
            // the tumble's lit-face rate is within the D-010 modulation limit (<= 0.2 Hz: 2 x 0.08)
            Assert.LessOrEqual(2.0 * PanelCourse.TumbleHz, 0.2);
            // it clears the z 850 cross bridge (deck z 843..857) and the faces while it falls past y -180
            for (double t = 0.0; t < PanelCourse.HideTau; t += 0.25)
            {
                Vector3 c = PanelCourse.Centre(t);
                if (c.y < -150f && c.y > -210f) Assert.Less(c.z + 0.5f * (float)PanelCourse.Width, 843f - 20f);
                Assert.Less(c.x, 815.3f);
            }
            // in the s 470 gaze (yaw 40 world, pitch -6): the panel near the frame centre from the eye at x 285.3
            double el = Elev(285.3, 195.0, p0.x, p0.y, p0.z), br = Bearing(285.3, 195.0, p0.x, p0.z);
            Assert.IsTrue(InFrame(40.0, -6.0, 66.0, br, el, 0.2), $"panel at s 470: elev {el:F1} bearing {br:F1}");
            var mb = PanelBuilder.Build();
            Assert.IsTrue(MeshValidation.Validate(mb, "panel").Ok);
        }

        // ------------------------------------------------------------------ elevators and gantries (2.2)

        [Test]
        public void Cabins_SpeedsWrapsAndShafts()
        {
            var cab = DreadCabins.Cabins();
            Assert.That(cab.Count, Is.InRange(28, 34), "about 30 cabins");
            foreach (var c in cab)
            {
                Assert.That(c.Speed, Is.InRange(3.0, 8.0));
                // exact wrap: the traffic clock's 65 536 s wrap is a whole number of shaft periods
                double periods = c.Speed * DreadCabins.ClockWrap / DreadCabins.ShaftL;
                Assert.AreEqual(Math.Round(periods), periods, 1e-6);
                // on a planar part of a Gulf face, 2 m proud, clear of the tongue root (face A z 120..270) and the notch (face B z 115..275)
                Assert.That(Math.Abs(c.X - 215.3) < 2.01 || Math.Abs(c.X - 815.3) < 2.01, $"x {c.X}");
                Assert.That(c.Z < 110.0 || c.Z > 280.0, $"z {c.Z}");
                Assert.Less(Math.Abs(c.Z - 195.0), 300.0);
                // a shaft takes 6-17 minutes: scale through slowness
                Assert.That(DreadCabins.ShaftL / c.Speed, Is.InRange(360.0, 1020.0));
                // continuous: per 60 fps frame <= 8 / 60 m, except the hidden wrap at the ends where the light is ramped to 0
                for (double t = 0.0; t < 1200.0; t += 1.0 / 60.0)
                {
                    double y0 = DreadCabins.CabinY(c, t, out double r0), y1 = DreadCabins.CabinY(c, t + 1.0 / 60.0, out double r1);
                    if (Math.Abs(y1 - y0) > 8.0 / 60.0 + 1e-6) { Assert.Less(r0 + r1, 1e-3, "a wrap with the light on"); }
                    if (r0 > 0.0) Assert.That(y0, Is.InRange(DreadCabins.Y0, DreadCabins.Y1));
                }
            }
            // the wrap ends are hidden: the shaft top is inside the deck (cloud mix 1 above 560), the bottom in the deep haze
            Assert.GreaterOrEqual(DreadCabins.Y1, 560.0);
            Assert.LessOrEqual(DreadCabins.Y0, -2000.0);
            var m = DreadCabins.CabinMesh();
            Assert.AreEqual(4 * cab.Count, m.vertexCount);
            var u1 = new List<Vector4>();
            m.GetUVs(1, u1);
            foreach (var u in u1) Assert.AreEqual(DreadCabins.CabProfile, u.w, 1e-6f, "DR-07: the lit cab profile");
            // DR-07: the shafts' guide lights, two steady rows a shaft, on the shaft line, inside the haze-to-deck span
            var g = DreadCabins.GuideLights();
            int rows = (int)Math.Floor((DreadCabins.GuideY1 - DreadCabins.GuideY0) / DreadCabins.GuidePitch) + 1;
            Assert.AreEqual(DreadCabins.Shafts.GetLength(0) * 2 * rows, g.Count);
            Assert.Less(2 * g.Count, 6000, "guide lights: a few thousand triangles");
            foreach (var l in g)
            {
                Assert.IsFalse(l.Halo);
                Assert.That(l.Pos.y, Is.InRange((float)DreadCabins.GuideY0, (float)DreadCabins.GuideY1));
                Assert.That(Math.Abs(l.Pos.x - 215.3) < 2.01 || Math.Abs(l.Pos.x - 815.3) < 2.01);
                Assert.Less(l.Gain, DreadCabins.CabinGain);
            }
            Assert.GreaterOrEqual(m.bounds.size.y, (float)DreadCabins.ShaftL);
            UnityEngine.Object.DestroyImmediate(m);
        }

        [Test]
        public void Gantries_SlowAndOnTheFlatFace()
        {
            for (int i = 0; i < DreadCabins.Gantries.GetLength(0); i++)
            {
                double vmax = 0.0, zmin = double.MaxValue, zmax = double.MinValue;
                for (double t = 0.0; t < 4000.0; t += 0.5)
                {
                    double z0 = DreadCabins.GantryZ(i, t), z1 = DreadCabins.GantryZ(i, t + 0.5);
                    vmax = Math.Max(vmax, Math.Abs(z1 - z0) / 0.5);
                    zmin = Math.Min(zmin, z0); zmax = Math.Max(zmax, z0);
                }
                Assert.AreEqual(0.5, vmax, 0.01, "peak 0.5 m/s");
                double half = 0.5 * DreadCabins.GantryL;
                bool north = zmin - half > 280.0 && zmax + half < 1000.0;      // north of the garden notch, face B planar to z 1 000
                bool south = zmin - half > -110.0 && zmax + half < 112.0;     // the planar south flank
                Assert.IsTrue(north || south, $"gantry {i} z {zmin - half:F0}..{zmax + half:F0}");
            }
            Assert.IsTrue(MeshValidation.Validate(DreadCabins.GantryMesh(), "gantry").Ok);
            Assert.IsTrue(MeshValidation.Validate(DreadCabins.RailMesh(), "rails").Ok);
        }
    }
}
