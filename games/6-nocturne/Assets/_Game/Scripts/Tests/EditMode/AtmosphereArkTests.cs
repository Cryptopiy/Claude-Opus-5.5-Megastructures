// AtmosphereArkTests (AT, D-043 the Ark, 2026-11-21): EditMode tests of AT's part of the Ark on the double mirror (NocFogMath /
// NocArkAir, the twins of NocAtmosphere.hlsl's NocArkShape / NocArkAir / NocArkPool) with DR's course (HullCourse):
//   - identity: no Ark (zero strength and gains) leaves the deck colour and the air untouched; nothing is lit before the launch;
//   - the deck occluder only darkens, is 1 on the axis mid-length, 0 beyond the penumbra and the ends, monotone across the beam;
//   - the searchlights (DR's emitters, HullCourse.Searchlight): sweep periods >= 20 s (D-010), the lamps light one by one over >= 8 s
//     each, every lit beam points down and stays 3 r + 150 m from the walker and off the walk line over the pier and the bridge at
//     every s of beats 3-5;
//   - the in-scatter twin equals a numeric line integral of the beam / tube densities (the closed form of NocTubeIn);
//   - the sheet lightning on the Ark: the Ark's stadium keeps <= 0.1 of the flash; the shader uses every term where the twin says.
using System;
using System.IO;
using NUnit.Framework;

namespace Megastructure.Tests
{
    public class AtmosphereArkTests
    {
        static NocAtmoState ArkState(double s, double time)
        {
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, s, time);
            double tau = HullCourse.TauAtS(s);
            D3 p = NocMath.PathPoint(s);
            double vis = HullCourse.HiddenAt(tau) ? 0.0 : 1.0;
            NocArkAir.Update(st, HullCourse.AxisX, HullCourse.CentreZ(tau), 0.5 * HullCourse.Length, HullCourse.HalfBeam, HullCourse.BellyY(tau),
                             tau, HullCourse.Emerge(tau), vis);
            double tanHalf = Math.Tan(HullCourse.SearchHalfAngle * Math.PI / 180.0);
            for (int i = 0; i < Math.Min(HullCourse.SearchCount, NocArkAir.Beams); i++)
            {
                HullCourse.Searchlight(i, tau, time, out UnityEngine.Vector3 o, out UnityEngine.Vector3 d, out double g);
                NocArkAir.SetBeam(st, i, o.x, o.y, o.z, d.x, d.y, d.z, g, tanHalf, tau, vis, p.x, NocturneConstants.EyeHeight, p.z);
            }
            st.CamX = p.x; st.CamY = NocturneConstants.EyeHeight; st.CamZ = p.z;
            return st;
        }

        [Test]
        public void Ark_IdentityWithoutTheArkAndBeforeTheLaunch()
        {
            var st = new NocAtmoState();
            NocFogMath.StateAt(st, 500, 200.0);
            var rnd = new Random(43);
            for (int i = 0; i < 200; i++)
            {
                double x = 515.3 + 1600 * (rnd.NextDouble() - 0.5), z = 195 + 4000 * (rnd.NextDouble() - 0.5);
                var c0 = NocFogMath.CloudColor(st, x, z, 2.0);
                st.ArkHalfBeam = 220; st.ArkX = 515.3; st.ArkZ = 195; st.ArkHalf = 2500;   // shape without strength
                var c1 = NocFogMath.CloudColor(st, x, z, 2.0);
                Assert.AreEqual(c0.Luma, c1.Luma, 1e-15, "zero strength is the identity");
                var a = NocFogMath.ArkAir(st, 0.3, 0.5, 0.81, 3000);
                Assert.AreEqual(0.0, a.Luma, 1e-15, "zero gains add no light");
            }
            var pre = ArkState(HullCourse.LaunchS - 10.0, 100.0);
            Assert.AreEqual(0.0, pre.ArkW, "no occluder while parked");
            Assert.AreEqual(0.0, pre.ArkGlow, "no glow while parked");
            Assert.AreEqual(0.0, pre.ArkBeamsOn, "no searchlight while parked");
        }

        [Test]
        public void Ark_DeckOccluderOnlyDarkensAndHasAStadiumShape()
        {
            var st = ArkState(700, 280.0);
            Assert.Greater(st.ArkW, 0.5, "the occluder is on in beat 4");
            Assert.AreEqual(1.0, NocFogMath.ArkShape(st, st.ArkX, st.ArkZ), 1e-9, "1 on the axis mid-length");
            Assert.AreEqual(0.0, NocFogMath.ArkShape(st, st.ArkX + st.ArkHalfBeam + st.ArkPen + 1.0, st.ArkZ), 1e-12, "0 past the penumbra");
            Assert.AreEqual(0.0, NocFogMath.ArkShape(st, st.ArkX, st.ArkZ + st.ArkHalf + 1.0), 1e-12, "0 past the ends");
            double prev = 2.0;
            for (int i = 0; i <= 100; i++)
            {
                double v = NocFogMath.ArkShape(st, st.ArkX + 6.0 * i, st.ArkZ + 0.3 * st.ArkHalf);
                Assert.LessOrEqual(v, prev + 1e-12, "monotone across the beam");
                prev = v;
            }
            var off = new NocAtmoState();
            NocFogMath.StateAt(off, 700, 280.0);
            var rnd = new Random(7);
            for (int i = 0; i < 300; i++)
            {
                double x = st.ArkX + 1200 * (rnd.NextDouble() - 0.5), z = st.ArkZ + 3.0 * st.ArkHalf * (rnd.NextDouble() - 0.5);
                Assert.LessOrEqual(NocFogMath.CloudColor(st, x, z, 2.0).Luma, NocFogMath.CloudColor(off, x, z, 2.0).Luma + 1e-12, "it only darkens");
            }
        }

        [Test]
        public void Ark_SearchlightsSlowSoftAndNeverAtTheWalkerOrTheBridge()
        {
            for (int i = 0; i < HullCourse.SearchCount; i++)
                Assert.GreaterOrEqual(HullCourse.SearchPeriod(i), 20.0, "sweep period >= 20 s (D-010: no light faster than 0.2 Hz)");
            Assert.GreaterOrEqual(NocArkAir.BeamOnRamp, 8.0, "each lamp fades in over >= 8 s");
            int lit = 0;
            double minClear = double.MaxValue;
            for (double s = 400; s <= 1100; s += 2.5)
            {
                for (double tt = 0; tt < 60; tt += 3.7)
                {
                    var st = ArkState(s, 100.0 + tt);
                    for (int i = 0; i < NocArkAir.Beams; i++)
                    {
                        double g = st.ArkBeamP[4 * i + 3];
                        double ax = st.ArkBeamP[4 * i], ay = st.ArkBeamP[4 * i + 1], az = st.ArkBeamP[4 * i + 2];
                        double dx = st.ArkBeamD[4 * i], dy = st.ArkBeamD[4 * i + 1], dz = st.ArkBeamD[4 * i + 2];
                        Assert.AreEqual(1.0, dx * dx + dy * dy + dz * dz, 1e-9, "unit axis");
                        if (g <= 1e-6) continue;
                        Assert.Less(dy, -0.8, "every lit beam points down (never up at the walker)");
                        lit++;
                        double wx = st.CamX - ax, wy = st.CamY - ay, wz = st.CamZ - az;
                        double u = Math.Max(0.0, wx * dx + wy * dy + wz * dz);
                        double qx = wx - u * dx, qy = wy - u * dy, qz = wz - u * dz;
                        double clear = Math.Sqrt(qx * qx + qy * qy + qz * qz) - 3.0 * (NocArkAir.BeamR0 + u * st.ArkBeamD[4 * i + 3]);
                        minClear = Math.Min(minClear, clear);
                        Assert.Greater(clear, NocArkAir.ClearWalker - 1e-6, "a lit beam keeps 3 r + 150 m from the walker, s " + s);
                        double ub = (ay - NocArkAir.BridgeY) / -dy, xb = ax + dx * ub, zb = az + dz * ub;
                        if (xb > NocArkAir.BridgeX0 + 50.0 && xb < NocArkAir.BridgeX1 - 50.0)
                            Assert.Greater(Math.Abs(zb - NocArkAir.BridgeZ), NocArkAir.BridgeClear0 - 1e-6, "a lit beam never crosses the walk line, s " + s);
                    }
                }
            }
            UnityEngine.Debug.Log(FormattableString.Invariant($"[NOC] ark beams lit samples {lit}, closest lit beam {minClear:F0} m beyond 3 r of the eye"));
            Assert.Greater(lit, 100, "the searchlights are lit over beats 3-5");
        }

        [Test]
        public void Ark_InScatterTwinMatchesTheNumericIntegral()
        {
            var st = ArkState(720, 300.0);
            var rnd = new Random(11);
            int checkedRays = 0;
            for (int n = 0; n < 400 && checkedRays < 40; n++)
            {
                double dx = rnd.NextDouble() - 0.5, dy = 0.9 * rnd.NextDouble() - 0.6, dz = rnd.NextDouble() - 0.5;
                double l = Math.Sqrt(dx * dx + dy * dy + dz * dz); dx /= l; dy /= l; dz /= l;
                double len = 3000.0;
                double closed = NocFogMath.ArkAir(st, dx, dy, dz, len).Luma;
                if (closed < 1e-4) continue;
                // numeric: the same densities sampled along the ray (r, I, window and fog at the closest approach, as the closed form)
                double num = 0.0;
                C3 gc = C3.Lerp(NocFogMath.Pal(NocPal.WinNeutral), NocFogMath.Pal(NocPal.WinCold), 0.6);
                var sw = NocFogMath.Pal(NocPal.SignWhite);
                if (st.ArkGlow > 0)
                {
                    NocFogMath.LineApproach(st.CamX - st.ArkX, st.CamY - st.ArkGlowY, st.CamZ - st.ArkZ, dx, dy, dz, 0, 0, 1, out double t0, out double u0, out double h2, out double sn);
                    double r = st.ArkGlowR;
                    if (h2 < 6 * r * r)
                    {
                        double win = NocFogMath.S01((st.ArkHalf - Math.Abs(u0)) / st.ArkEnd), tf = Math.Max(t0, 0), ft = st.SigmaR * NocFogMath.Down(st.CamY + tf * dy) * tf;
                        double G = st.ArkGlow * win / (Math.Sqrt(Math.PI) * r) * Math.Exp(-ft * ft) * (sn / Math.Max(sn, 0.1)) * (1.0 - NocMath.Smoothstep(4.0, 6.0, h2 / (r * r))), acc = 0;
                        for (double t = 0.25; t < len; t += 0.5) { double d2 = h2 + sn * sn * (t - t0) * (t - t0); acc += Math.Exp(-d2 / (r * r)) * 0.5; }
                        num += gc.Luma * G * acc;
                    }
                }
                for (int i = 0; i < NocArkAir.Beams; i++)
                {
                    double g = st.ArkBeamP[4 * i + 3];
                    if (g <= 0) continue;
                    NocFogMath.LineApproach(st.CamX - st.ArkBeamP[4 * i], st.CamY - st.ArkBeamP[4 * i + 1], st.CamZ - st.ArkBeamP[4 * i + 2], dx, dy, dz,
                                            st.ArkBeamD[4 * i], st.ArkBeamD[4 * i + 1], st.ArkBeamD[4 * i + 2], out double t0, out double u0, out double h2, out double sn);
                    double r0 = st.ArkR0, r = r0 + Math.Max(u0, 0) * st.ArkBeamD[4 * i + 3];
                    if (h2 >= 6 * r * r) continue;
                    double I = g * (r0 / r) * Math.Exp(-Math.Max(u0, 0) / st.ArkFall) * NocMath.Smoothstep(-r0, 2 * r0, u0);
                    double tf = Math.Max(t0, 0), ft = st.SigmaR * NocFogMath.Down(st.CamY + tf * dy) * tf;
                    double G = I / (Math.Sqrt(Math.PI) * r) * Math.Exp(-ft * ft) * (sn / Math.Max(sn, 0.1)) * (1.0 - NocMath.Smoothstep(4.0, 6.0, h2 / (r * r))), acc = 0;
                    for (double t = 0.25; t < len; t += 0.5) { double d2 = h2 + sn * sn * (t - t0) * (t - t0); acc += Math.Exp(-d2 / (r * r)) * 0.5; }
                    num += sw.Luma * G * acc;
                }
                Assert.AreEqual(num, closed, 2e-3 * Math.Max(num, 1e-3) + 1e-6, "closed form = numeric integral");
                checkedRays++;
            }
            Assert.Greater(checkedRays, 5, "rays through the Ark's light were checked");
        }

        [Test]
        public void Ark_FlashOutlinesTheArkAndTheShaderUsesEveryTerm()
        {
            var st = ArkState(790, 316.0);
            double zc = st.ArkZ;
            st.FlashX = st.ArkX; st.FlashZ = zc; st.FlashSigma = NocAir.FlashSigma; st.ArkFlashStretch = NocArkAir.FlashStretch;
            st.FlashAmp = 0; var h0 = NocFogMath.CloudColor(st, st.ArkX, zc, 1.0);
            st.FlashAmp = NocAir.FlashAmp; var h1 = NocFogMath.CloudColor(st, st.ArkX, zc, 1.0);
            double side = st.ArkX + st.ArkHalfBeam + st.ArkPen + 60.0;
            var e1 = NocFogMath.CloudColor(st, side, zc, 1.0); st.FlashAmp = 0; var e0 = NocFogMath.CloudColor(st, side, zc, 1.0);
            double inArk = h1.Luma / h0.Luma - 1.0, outside = e1.Luma / e0.Luma - 1.0;
            UnityEngine.Debug.Log(FormattableString.Invariant($"[NOC] ark flash gain over the Ark {inArk:P2}, beside it {outside:P2}"));
            Assert.LessOrEqual(inArk, 0.1 * NocAir.FlashAmp + 1e-9, "the Ark stays dark: the flash outlines it");
            Assert.Greater(outside, 3.0 * inArk, "the deck beside the Ark flashes");

            string src = File.ReadAllText(Path.Combine(UnityEngine.Application.dataPath, "_Game/Shaders/Include/NocAtmosphere.hlsl"));
            StringAssert.Contains("ark = NocArkShape(xz);", src);
            StringAssert.Contains("fl *= 1.0 - NA_FlashHull * ark;", src);
            StringAssert.Contains("col += NocArkAir(v / max(l, 1e-3), l);", src);
            StringAssert.Contains("NocArkPool(h, yk)", src);
            string sky = File.ReadAllText(Path.Combine(UnityEngine.Application.dataPath, "_Game/Shaders/NocSky.shader"));
            StringAssert.Contains("c += NocArkAir(d,", sky);
        }
    }
}
