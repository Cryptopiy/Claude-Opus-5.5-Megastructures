// Beat06SignsTests: EditMode acceptance of beat 6, the Wall of signs (MASTER_PROMPT 14.5.6 Acceptance (1), Photosensitivity;
// 14.10 KS16 and T-WAVE; 14.4 lanes and floors; geometry agreements 1 and 6; Part 10 items 8, 27, 29):
//   slab 40.0 x 245 +-0.05 m with the face at x 874.00 +-0.01; KS16 wall points at their 14.5.6 NDC (+-0.02) and the wall's share of
//   the frame (59 % at 16:9, 46 % at 21:9, ray-cast against the slab with the floor in front); the Look v2 ramp stops of D-010
//   (teal / ice / amber / cyan, luminance 0.229 / 0.301 / 0.269 / 0.287, red share <= 0.59, no violet, periodic, amber -> ice
//   over s 1 230-1 290), the R4 wall-light saturation (<= 0.45), every modulation <= 0.2 Hz and the T-WAVE bound (a wall pixel's relative
//   luminance changes <= 0.12 per second; R4 computes 0.007 for the D-010 wall); the wrapped time phases agree with the unwrapped
//   formula; the promenade floor (plus G3's terrace south of z 275 and the heart floor east of x 914) covers the lane + 1 m;
//   nothing of beat 6 stands in the lane, and nothing within 0.25 m of it at camera height; every mesh validates; lights and
//   triangles within the 14.2.3 / 14.5.6 budgets.
// SG Phase 4 wave A (the LED screen facade, the glyph signs, the arcade): the imagery's display periods divide the 600 s clock,
// its programmes average m about 1 and stay <= 0.11 Hz; the photosensitivity model (60 s at 30 fps over the KS16 wall: T-WAVE,
// zero flashes, dominant frequency <= 0.5 Hz per point); the bloom bound with the imagery and the resolved LED dots; failure
// and age rates; the nine glyph signs (60-120 m, y 20-420, off KS16's wall-point sight lines, off the lane); the arcade below
// its 38 m roof line and off the lane; the H4 wall plane of kind 2. Fix wave (panel p4_01): the eye (C28) once inside KS16's
// window and through the photosensitivity model; the cradles and ledges (C17): speed, wraps, clearances, on the KS16 frame.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class Beat06SignsTests
    {
        static KeyShot Shot(string id)
        {
            foreach (var k in NocturneConstants.KeyShots) if (k.tag.StartsWith(id, StringComparison.Ordinal)) return k;
            throw new ArgumentException(id);
        }

        static Bounds WorldBounds(MeshBuilder mb, Vector3 origin)
        {
            var b = new Bounds(mb.Positions[0] + origin, Vector3.zero);
            foreach (var p in mb.Positions) b.Encapsulate(p + origin);
            return b;
        }

        [Test]
        public void Slab_Is40By245_FaceAt874()
        {
            var face = ScreenWallBuilder.ScreenFace();
            var mass = ScreenWallBuilder.MassFaces();
            Bounds bf = WorldBounds(face, ScreenWallBuilder.Origin), bm = WorldBounds(mass, ScreenWallBuilder.Origin);
            Bounds all = bm; all.Encapsulate(bf);
            Debug.Log(FormattableString.Invariant($"[NOC] Beat06 slab: x {all.min.x:F3}-{all.max.x:F3} ({all.size.x:F3} m), z {all.min.z:F3}-{all.max.z:F3} ({all.size.z:F3} m), y {all.min.y:F1}..{all.max.y:F1}; face x {bf.min.x:F4}..{bf.max.x:F4}"));
            Assert.AreEqual(40.0, all.size.x, 0.05, "slab depth (x 834 - 874)");
            Assert.AreEqual(245.0, all.size.z, 0.05, "slab length (z 275 - 520)");
            Assert.AreEqual(874.0, bf.min.x, 0.01, "screen face x");
            Assert.AreEqual(874.0, bf.max.x, 0.01, "screen face x (planar)");
            Assert.AreEqual(-1500.0, all.min.y, 0.05); Assert.AreEqual(560.0, all.max.y, 0.05);
            // the screen face points east, toward the promenade
            foreach (var n in face.Normals) Assert.Greater(n.x, 0.999f);
        }

        [Test]
        public void KS16_WallPoints_AtTheirNdc()
        {
            var k = Shot("KS16");
            var v = new QaView(k.x, k.y, k.z, k.yaw, k.pitch, k.fov, 1920, 1080);
            var pts = new[] { new D3(874, 10, 300), new D3(874, 150, 350), new D3(874, 300, 420) };
            var exp = new[] { new Vector2(-0.15f, -0.62f), new Vector2(0.13f, 0.80f), new Vector2(0.19f, 0.93f) };
            for (int i = 0; i < pts.Length; i++)
            {
                Assert.IsTrue(QaProjection.ProjectPoint(v, pts[i], out double x, out double y));
                Debug.Log(FormattableString.Invariant($"[NOC] KS16 wall point ({pts[i].x:F0}, {pts[i].y:F0}, {pts[i].z:F0}): ndc ({x:F3}, {y:F3}), 14.5.6 ({exp[i].x:F2}, {exp[i].y:F2})"));
                Assert.AreEqual(exp[i].x, x, QaFraming.ExpectTolerance);
                Assert.AreEqual(exp[i].y, y, QaFraming.ExpectTolerance);
                Assert.That(pts[i].z, Is.InRange(SignsLayout.WallZ0, SignsLayout.WallZ1), "the point lies on the face");
            }
        }

        /// <summary>Fraction of the frame whose ray hits the screen face above the promenade floor (y 0..560, z 275..520).</summary>
        static double WallShare(double aspect)
        {
            var k = Shot("KS16");
            QaProjection.Basis(k.yaw, k.pitch, out D3 f, out D3 r, out D3 u);
            double t = Math.Tan(0.5 * k.fov * NocMath.Deg);
            int h = 216, w = (int)Math.Round(h * aspect), hit = 0;
            for (int j = 0; j < h; j++)
                for (int i = 0; i < w; i++)
                {
                    double nx = ((i + 0.5) / w * 2.0 - 1.0) * t * aspect, ny = (1.0 - (j + 0.5) / h * 2.0) * t;
                    D3 d = f + r * nx + u * ny;
                    if (d.x >= 0.0) continue;
                    double tt = (SignsLayout.WallX1 - k.x) / d.x;
                    double y = k.y + tt * d.y, z = k.z + tt * d.z;
                    // below y 0 the promenade floor (x 874 - 914) lies in front of the face
                    if (z >= SignsLayout.WallZ0 && z <= SignsLayout.WallZ1 && y >= 0.0 && y <= SignsLayout.WallY1) hit++;
                }
            return (double)hit / (w * h);
        }

        [Test]
        public void KS16_WallShare_59PercentAt16x9()
        {
            double s169 = WallShare(16.0 / 9.0), s219 = WallShare(21.0 / 9.0), s1610 = WallShare(16.0 / 10.0);
            Debug.Log(FormattableString.Invariant($"[NOC] KS16 wall share: 16:9 {s169:P1} (14.5.6: 59 %), 21:9 {s219:P1} (46 %), 16:10 {s1610:P1}"));
            Assert.AreEqual(0.59, s169, 0.015);
            Assert.AreEqual(0.46, s219, 0.015);
        }

        [Test]
        public void Ramp_D010_StopsAndRedShare()
        {
            // Look v2 ramp (LOOK_V2 10, R6, R7): teal, ice (SignWhite), amber, cyan and back to teal, each desaturated 35 % toward its
            // luminance and mixed 15 % toward ScreenBase, then luminance-normalised with k 0.8 (D-010) to a geometric mean of 0.27
            // (NocturneConstants.ScreenRampY): Y 0.229 / 0.301 / 0.269 / 0.287 (+-13 %), red share <= 0.59, never violet
            double[] expY = { 0.229, 0.301, 0.269, 0.287, 0.229 };
            for (int i = 0; i < NocScreenMath.StopCount; i++)
            {
                D3 c = NocScreenMath.StopColour(i, 1.0);
                double y = NocMath.Luma709(c.x, c.y, c.z), red = c.x / Math.Max(1e-9, c.x + c.y + c.z);
                double hue = HueOf(c);
                Debug.Log(FormattableString.Invariant($"[NOC] screen ramp stop {i}: ({c.x:F4}, {c.y:F4}, {c.z:F4}) Y {y:F4} (LOOK_V2: {expY[i]:F3}), red share {red:F3}, hue {hue:F0}"));
                Assert.AreEqual(expY[i], y, 0.003, "D-010 luminance-normalised stop");
                Assert.LessOrEqual(red, 0.59 + 1e-3, "14.5.6 red share of the ramp stops");
                Assert.IsFalse(hue >= 250 && hue <= 340, "R7: no violet stop");
            }
            double gm = Math.Pow(expY[0] * expY[1] * expY[2] * expY[3], 0.25);
            Assert.AreEqual(NocturneConstants.ScreenRampY, gm, 0.002, "geometric-mean luminance of the stops");
            // periodic: the last stop is the first (no seam where h wraps)
            D3 a0 = NocScreenMath.Ramp(0.0, 1.0), a1 = NocScreenMath.Ramp(0.999999, 1.0);
            Assert.AreEqual(a0.x, a1.x, 1e-5); Assert.AreEqual(a0.y, a1.y, 1e-5); Assert.AreEqual(a0.z, a1.z, 1e-5);
            // R6: the amber stop turns into the ice stop over s 1 230-1 290 (weight 0 = ice exactly)
            D3 ice = NocScreenMath.StopColour(1, 1.0), amber0 = NocScreenMath.StopColour(2, 0.0);
            Assert.AreEqual(ice.x, amber0.x, 1e-12); Assert.AreEqual(ice.z, amber0.z, 1e-12);
            // the emission stays below the bloom threshold (0.9 exposed linear) at the brightest breath
            double peak = 0.0;
            for (int i = 0; i < NocScreenMath.StopCount; i++) { D3 c = NocScreenMath.StopColour(i, 1.0); peak = Math.Max(peak, Math.Max(c.x, Math.Max(c.y, c.z))); }
            peak *= SignsLayout.EmitGain * (SignsLayout.PanelLevel + SignsLayout.BreathAmp);
            Debug.Log(FormattableString.Invariant($"[NOC] screen emission peak channel {peak:F3} (bloom threshold 0.9)"));
            Assert.Less(peak, 0.9);
            // the LED wall (imagery v3): the brightest colour of the ramp and of its saturated accents, the brightest programme (m
            // capped at ContentMaxM / (1 + DotGain wp), so cap x the resolved dot's peak gain 1 + DotGain wp is ContentMaxM at any
            // dot fade), the brightest panel (1.04), module (1.03) and white point (1.05): still below the bloom threshold, so the wall
            // reads as screens, not as glare; the accents stay within the 14.5.6 red share
            double dot = NocScreenMath.LedFloor + (1.0 - NocScreenMath.LedFloor) / (NocScreenMath.LedDot * NocScreenMath.LedDot);
            Assert.AreEqual(dot - 1.0, NocScreenMath.DotGain, 1e-9);
            Assert.AreEqual(0.842195, NocScreenMath.DotGain, 1e-5, "SCR_DOTG in NocScreenWave.hlsl");
            double peakC = 0.0, redAcc = 0.0;
            for (int i = 0; i <= 400; i++)
            {
                D3 r = NocScreenMath.Ramp(i / 400.0, 1.0), c = NocScreenMath.Accent(i / 400.0, 1.0);
                peakC = Math.Max(peakC, Math.Max(Math.Max(r.x, r.y), Math.Max(r.z, Math.Max(c.x, Math.Max(c.y, c.z)))));
                redAcc = Math.Max(redAcc, c.x / Math.Max(1e-9, c.x + c.y + c.z));
                double mx = Math.Max(c.x, Math.Max(c.y, c.z)), sat = mx > 0 ? (mx - Math.Min(c.x, Math.Min(c.y, c.z))) / mx : 0.0;
                Assert.IsFalse(HueOf(c) >= 250 && HueOf(c) <= 340 && sat > 0.20, "R7: no violet accent (LOOK_V2 R1: hue 250-340 at saturation > 0.35)");
            }
            double led = peakC * SignsLayout.EmitGain * (SignsLayout.PanelLevel + SignsLayout.BreathAmp) * NocScreenMath.ContentMaxM * 1.04 * 1.03 * 1.05;
            Debug.Log(FormattableString.Invariant($"[NOC] LED wall peak channel {led:F3} (colour peak {peakC:F3}, imagery cap x dot = {NocScreenMath.ContentMaxM}, dot x {dot:F2}); accent red share max {redAcc:F3}"));
            Assert.Less(led, 0.9);
            Assert.LessOrEqual(redAcc, 0.70, "14.5.6 red share <= 0.70 for the saturated accents");
        }

        [Test]
        public void TWave_LuminanceRate_AndModulationFrequencies()
        {
            // every panel breath is n / 600 Hz with n 30..60: 0.05 - 0.10 Hz (D-010), below the 0.2 Hz photosensitivity limit
            for (int iz = -2; iz < 10; iz++)
                for (int iy = -80; iy < 30; iy++)
                {
                    NocScreenMath.PanelBreath(iz, iy, out int n, out _);
                    Assert.That(n, Is.InRange(30, 60));
                }
            // T-WAVE: the relative luminance of a wall pixel changes <= 0.12 per second (14.10). The D-010 wave alone (Simplified
            // tier, R4 3.5: about 0.007 /s) and the Full tier's LED wall (three waves, imagery: the programme crossfades and the
            // glyph cores dominate; design bound 0.06 /s, half the limit)
            foreach (var tier in new[] { 1, 0 })
            {
                double worst = 0.0;
                var rng = new System.Random(6);
                for (int p = 0; p < 60; p++)
                {
                    double z = SignsLayout.WallZ0 + 245.0 * rng.NextDouble(), y = 560.0 * rng.NextDouble();
                    double prev = double.NaN;
                    for (double t = 0.0; t <= 240.0; t += 0.1)
                    {
                        double lum = NocScreenMath.PixelLuminance(z, y, t, SignsLayout.Waves(tier), SignsLayout.Content(tier));
                        if (!double.IsNaN(prev)) worst = Math.Max(worst, Math.Abs(lum - prev) / 0.1);
                        prev = lum;
                    }
                }
                Debug.Log(FormattableString.Invariant($"[NOC] T-WAVE tier {tier}: max wall-pixel luminance rate {worst:F4} /s (14.10 limit 0.12)"));
                Assert.LessOrEqual(worst, 0.12);
                Assert.LessOrEqual(worst, tier == 0 ? 0.06 : 0.012, tier == 0 ? "the LED wall's design bound (half the 14.10 limit; v3 at the LOOK_V2 wall mean)" : "the D-010 wall (R4 3.5: about 0.007 /s)");
            }
        }

        [Test]
        public void TimePhases_WrapExactly()
        {
            // the uploaded phases reproduce the unwrapped formula of 14.5.6 at any clock value (no jump at the 58 / 60 / 600 s wraps)
            var rng = new System.Random(7);
            for (int i = 0; i < 2000; i++)
            {
                double t = 20000.0 * rng.NextDouble(), z = 275 + 245 * rng.NextDouble(), y = -1500 + 2060 * rng.NextDouble();
                double hRef = 0.5 + 0.28 * Math.Sin(2 * Math.PI * ((0.85 * z + 0.53 * y) / 520.0 - t / 58.0))
                            + 0.18 * Math.Sin(2 * Math.PI * ((-0.60 * z + 0.80 * y) / 360.0 + t / 60.0))
                            + 0.12 * Math.Sin(2 * Math.PI * ((0.30 * z - 0.95 * y) / 700.0 - t / 58.0));
                hRef -= Math.Floor(hRef);
                double h = NocScreenMath.IndexAt(z, y, t, SignsLayout.WavesFull);
                double d = Math.Abs(h - hRef); d = Math.Min(d, 1.0 - d);
                Assert.Less(d, 1e-6);
            }
        }

        [Test]
        public void Lights_R4_SaturationOfPaLights()
        {
            // LOOK_V2 R4: a light that washes surfaces (range > 10 m) has an sRGB HSV saturation <= 0.45 (palette-table convention):
            // the 12 screen-wall lights over the whole ramp and amber weight, the boulevard blades, and the lamp tokens of PA's beat
            // files (LampWarm 0.31, SignWhite 0.14); the goal lamp (0.60) and the koi light (0.64) are the two exceptions
            double worst = 0.0;
            for (int a = 0; a <= 4; a++)
                for (int i = 0; i <= 400; i++)
                {
                    Color u = ScreenWallBuilder.UnitColour(NocScreenMath.Ramp(i / 400.0, a / 4.0));
                    worst = Math.Max(worst, SrgbSat(u));
                }
            Debug.Log(FormattableString.Invariant($"[NOC] R4: screen-wall light saturation max {worst:F3}"));
            Assert.LessOrEqual(worst, 0.45, "screen-wall lights");
            for (int i = 0; i < BladeSign.Count; i++)
            {
                double s = SrgbSat(BladeSign.LightColour(i));
                Assert.LessOrEqual(s, 0.45, "blade light " + i + " (" + BladeSign.Token(i) + ")");
                Assert.AreNotEqual(NocPal.NeonMagenta, BladeSign.Token(i), "no magenta blade (LOOK_V2 R1)");
                Assert.AreNotEqual(NocPal.NeonViolet, BladeSign.Token(i), "no violet blade (R7)");
                Assert.AreNotEqual(NocPal.AviationRed, BladeSign.Token(i), "no red blade");
            }
            Assert.AreEqual(0.314, SrgbSat(NocturnePalette.Linear(NocPal.LampWarm)), 0.002, "LampWarm 0.31");
            Assert.LessOrEqual(SrgbSat(NocturnePalette.Linear(NocPal.SignWhite)), 0.15);
            Assert.Greater(SrgbSat(NocturnePalette.Linear(NocPal.GoalLamp)), 0.45, "the goal lamp is the R4 exception");
        }

        /// <summary>HSV saturation of the sRGB encoding of a linear light colour (its brightest channel scaled to 1).</summary>
        static double SrgbSat(Color c)
        {
            double m = Math.Max(c.r, Math.Max(c.g, c.b));
            if (m < 1e-9) return 0.0;
            double r = NocturnePalette.LinearToSrgb(c.r / m), g = NocturnePalette.LinearToSrgb(c.g / m), b = NocturnePalette.LinearToSrgb(c.b / m);
            double mx = Math.Max(r, Math.Max(g, b)), mn = Math.Min(r, Math.Min(g, b));
            return mx > 0 ? (mx - mn) / mx : 0.0;
        }

        /// <summary>HSV hue (deg) of a linear colour (0 for greys).</summary>
        static double HueOf(D3 c)
        {
            double max = Math.Max(c.x, Math.Max(c.y, c.z)), min = Math.Min(c.x, Math.Min(c.y, c.z)), d = max - min;
            if (d < 1e-9) return 0.0;
            double h = max == c.x ? ((c.y - c.z) / d) % 6.0 : (max == c.y ? (c.z - c.x) / d + 2.0 : (c.x - c.y) / d + 4.0);
            h *= 60.0;
            return h < 0 ? h + 360.0 : h;
        }

        [Test]
        public void Imagery_PeriodsMeansAndFrequencies()
        {
            // every display period divides the 600 s clock (the wrap lands on an epoch boundary: no jump), every display edge lies on
            // a seam, programmes of every kind occur, and the imagery's mean m times the LED hardware's mean (seams, bezels, joints,
            // panel brightness, failures) is WallMeanGain (1.43) +- 8 %: the LED wall is the 14.5.6 mean emission (ramp x 0.45 x 0.55)
            // x 1.43, the LOOK_V2 KS16 wall of 60-85 codes (SG r03: 51.5 codes at 0.97); the peak stays capped (bloom check above)
            var kinds = new int[NocScreenMath.ProgramCount];
            double sum = 0.0; int n = 0;
            var rng = new System.Random(16);
            for (int i = 0; i < 4000; i++)
            {
                double zl = 245.0 * rng.NextDouble(), y = -200.0 + 760.0 * rng.NextDouble(), t = 600.0 * rng.NextDouble();
                NocScreenMath.Display(zl, y, out uint id, out double lx, out double ly, out double w, out double h);
                double T = NocScreenMath.Period(id);
                Assert.AreEqual(0.0, 600.0 % T, 1e-9, "display period divides 600 s");
                Assert.That(lx, Is.InRange(0.0, w)); Assert.That(ly, Is.InRange(0.0, h));
                Assert.AreEqual(0.0, Math.IEEERemainder(h, SignsLayout.PanelPitchY), 1e-6, "display height: whole panels");
                Assert.AreEqual(0.0, Math.IEEERemainder(y - ly, SignsLayout.PanelPitchY), 1e-6, "display bottom on a seam");
                kinds[NocScreenMath.ProgramOf(id, (uint)(i % 12))]++;
                NocScreenMath.Content(zl, y, t, 0.5, out double m, out double a);
                Assert.That(m, Is.InRange(0.10, NocScreenMath.ContentMaxM + 1e-9)); Assert.That(a, Is.InRange(0.0, 1.0));
                sum += m; n++;
            }
            double mean = sum / n;
            double hw = 0.0; int np = 0;
            for (int iz = 0; iz < 7; iz++)
                for (int iy = -74; iy < 28; iy++)
                {
                    NocScreenMath.Panel(iz, iy, out double kb, out _, out _, out double d, out double mm, out _);
                    hw += kb * d * mm; np++;
                }
            hw = hw / np * NocScreenMath.SeamMean;
            Debug.Log(FormattableString.Invariant($"[NOC] imagery: mean m {mean:F3}, LED hardware mean {hw:F3}, product {mean * hw:F3}; programmes {string.Join(" / ", kinds)} ({string.Join(", ", NocScreenMath.ProgramNames)})"));
            Assert.That(mean * hw / NocScreenMath.WallMeanGain, Is.InRange(0.92, 1.08), "the LED wall at its design mean: the 14.5.6 wave x 1.43 (LOOK_V2 KS16 wall 60-85)");
            foreach (var k in kinds) Assert.Greater(k, 80, "every programme occurs (the samples cluster on a few displays: the 6 % wave gave 149 of 4000)");
            // the wrapped clock: content at t600 and at t600 + 600 (one wrap later) is identical, and continuous across the wrap
            for (int i = 0; i < 200; i++)
            {
                double zl = 245.0 * rng.NextDouble(), y = 560.0 * rng.NextDouble();
                NocScreenMath.Content(zl, y, 599.99, 0.5, out double m0, out _);
                NocScreenMath.Content(zl, y, 0.0, 0.5, out double m1, out _);
                Assert.Less(Math.Abs(m1 - m0), 0.01, "no jump at the 600 s wrap");
            }
        }

        [Test]
        public void Photosensitivity_Model_KS16Wall60s()
        {
            // 14.10 photosensitivity on the wall model (the -sequence signs capture checks the picture): 48 points of the KS16 wall
            // (z 280-515, y 0-450), 60 s at 30 fps from t 300, and (fix wave C28) the eye's window t 445 - 505 with 16 more points on
            // its canvas: per point the relative luminance changes <= 0.12 /s (T-WAVE), no flash (a rise and a fall of >= 10 % of the
            // point's maximum within 1 s), and the dominant non-DC frequency <= 0.5 Hz
            Photosens(300.0, 0, out double r0, out int f0, out double d0);
            Photosens(445.0, 16, out double r1, out int f1, out double d1);
            Debug.Log(FormattableString.Invariant($"[NOC] photosensitivity model (48 KS16 wall points, 60 s from t 300): max rate {r0:F4} /s, flashes {f0}, dominant frequency max {d0:F3} Hz; eye window (t 445, + 16 canvas points): max rate {r1:F4} /s, flashes {f1}, dominant {d1:F3} Hz"));
            Assert.LessOrEqual(Math.Max(r0, r1), 0.12, "T-WAVE");
            Assert.AreEqual(0, f0 + f1, "zero flashes");
            Assert.LessOrEqual(Math.Max(d0, d1), 0.5, "dominant frequency of every point");
        }

        static void Photosens(double t0, int eyePoints, out double worstRate, out int flashes, out double worstF)
        {
            const int fps = 30, frames = 1800;
            var rng = new System.Random(1150);
            worstRate = 0; worstF = 0; flashes = 0;
            var L = new double[frames];
            for (int p = 0; p < 48 + eyePoints; p++)
            {
                double z = 280.0 + 235.0 * rng.NextDouble(), y = 450.0 * rng.NextDouble();
                if (p >= 48)
                {
                    z = NocScreenMath.EyeZ0 + (NocScreenMath.EyeZ1 - NocScreenMath.EyeZ0) * rng.NextDouble();
                    y = NocScreenMath.EyeY0 + (NocScreenMath.EyeY1 - NocScreenMath.EyeY0) * rng.NextDouble();
                }
                double lmax = 0;
                for (int f = 0; f < frames; f++) { L[f] = NocScreenMath.PixelLuminance(z, y, t0 + f / (double)fps, SignsLayout.WavesFull, 1.0); lmax = Math.Max(lmax, L[f]); }
                for (int f = 1; f < frames; f++) worstRate = Math.Max(worstRate, Math.Abs(L[f] - L[f - 1]) * fps);
                // 14.10's flash: an increase AND a decrease of >= 10 % of the maximum within 1 s (a peak or a trough); per point, so
                // stricter than the picture test's 25 % of the screen. A monotonic ramp into a plateau is not a flash (SG-t01: the
                // first version counted any window whose maximum was interior with one end 10 % lower).
                for (int f = 0; f + fps < frames; f += 5)
                {
                    double hi = double.MinValue, lo = double.MaxValue; int iHi = f, iLo = f;
                    for (int g = f; g <= f + fps; g++) { if (L[g] > hi) { hi = L[g]; iHi = g; } if (L[g] < lo) { lo = L[g]; iLo = g; } }
                    double minBefore = double.MaxValue, minAfter = double.MaxValue, maxBefore = double.MinValue, maxAfter = double.MinValue;
                    for (int g = f; g <= iHi; g++) minBefore = Math.Min(minBefore, L[g]);
                    for (int g = iHi; g <= f + fps; g++) minAfter = Math.Min(minAfter, L[g]);
                    for (int g = f; g <= iLo; g++) maxBefore = Math.Max(maxBefore, L[g]);
                    for (int g = iLo; g <= f + fps; g++) maxAfter = Math.Max(maxAfter, L[g]);
                    bool peak = hi - minBefore >= 0.1 * lmax && hi - minAfter >= 0.1 * lmax;
                    bool trough = maxBefore - lo >= 0.1 * lmax && maxAfter - lo >= 0.1 * lmax;
                    if (peak || trough) flashes++;
                }
                // dominant frequency (DFT of the mean-removed series, 1/60 Hz bins up to 5 Hz)
                double mean = 0; for (int f = 0; f < frames; f++) mean += L[f]; mean /= frames;
                double best = 0, bestF = 0;
                for (int k = 1; k <= 300; k++)
                {
                    double re = 0, im = 0, w = 2.0 * Math.PI * k / frames;
                    for (int f = 0; f < frames; f++) { double v = L[f] - mean; re += v * Math.Cos(w * f); im += v * Math.Sin(w * f); }
                    double pw = re * re + im * im;
                    if (pw > best) { best = pw; bestF = k / 60.0; }
                }
                worstF = Math.Max(worstF, bestF);
            }
        }

        [Test]
        public void Eye_OnceInTheKS16Window_OnTheFrame()
        {
            // D-034 5.3 / fix wave C28: one eye per 600 s, its canvas weight zero outside t 448 - 484, fully open over 460 - 472 (the
            // KS16 still is t 460, AmbientForS(1150)); the event lies inside KS16's readable window s 1 134 - 1 196 (t 453.6 - 478.4
            // at 2.5 m/s) from the opening to the closing; the canvas edges lie on panel seams; the eye's centre is on the KS16 frame
            var k = Shot("KS16");
            double tKs = ShotPlan.AmbientForS(k.s), tw0 = 1134.0 / NocturneConstants.CinematicSpeed, tw1 = 1196.5 / NocturneConstants.CinematicSpeed;
            int on = 0;
            for (int i = 0; i < 6000; i++)
            {
                double t = i * 0.1;
                double env = NocScreenMath.EyeEnvelope(t, out double open);
                if (env > 0.0) { on++; Assert.That(t, Is.InRange(448.0, 484.0), "the canvas only in 448 - 484"); }
                if (open > 0.0) Assert.That(t, Is.InRange(tw0, tw1), "the lids move inside the KS16 readable window");
                if (t >= 460.0 && t <= 472.0) Assert.AreEqual(1.0, open, 1e-9, "fully open 460 - 472");
            }
            NocScreenMath.EyeEnvelope(tKs, out double openKs);
            Assert.AreEqual(1.0, openKs, 1e-9, "the KS16 still shows the eye open");
            Assert.AreEqual(0.0, NocScreenMath.EyeEnvelope(0.0, out _), 1e-12);
            Assert.AreEqual(NocScreenMath.EyeEnvelope(470.0, out _), NocScreenMath.EyeEnvelope(470.0 - 600.0 + 600.0, out _), 1e-12);
            foreach (double z in new[] { NocScreenMath.EyeZ0, NocScreenMath.EyeZ1 })
                Assert.AreEqual(0.0, Math.IEEERemainder(z - NocScreenMath.Z0, NocScreenMath.PitchZ), 1e-9, "canvas z edge on a seam");
            foreach (double y in new[] { NocScreenMath.EyeY0, NocScreenMath.EyeY1 })
                Assert.AreEqual(0.0, Math.IEEERemainder(y, NocScreenMath.PitchY), 1e-9, "canvas y edge on a seam");
            // no glyph-sign housing on the canvas (SG_f02: a board covered the iris)
            foreach (var g in SignsLayout.GlyphSigns)
            {
                double r = g.blade ? SignsLayout.BladeRim : SignsLayout.BoardRim;
                double za = g.blade ? g.z - 0.5 * SignsLayout.BladeT : g.z - 0.5 * g.w - r, zb = g.blade ? g.z + 0.5 * SignsLayout.BladeT : g.z + 0.5 * g.w + r;
                bool clear = zb < NocScreenMath.EyeZ0 || za > NocScreenMath.EyeZ1 || g.y0 - r > NocScreenMath.EyeY1 || g.y0 + g.h + r < NocScreenMath.EyeY0;
                Assert.IsTrue(clear, FormattableString.Invariant($"the sign at z {g.z} lies on the eye's canvas"));
            }
            var v = new QaView(k.x, k.y, k.z, k.yaw, k.pitch, k.fov, 1920, 1080);
            Assert.IsTrue(QaProjection.ProjectPoint(v, new D3(874.0, NocScreenMath.EyeCy, NocScreenMath.EyeCz), out double nx, out double ny));
            Debug.Log(FormattableString.Invariant($"[NOC] eye: canvas on {on / 10.0:F1} s of 600, KS16 still at t {tKs:F1} (open {openKs:F2}), centre at KS16 ndc ({nx:F3}, {ny:F3})"));
            Assert.That(nx, Is.InRange(-0.9, 0.9)); Assert.That(ny, Is.InRange(-0.9, 0.9));
            // the open eye is bright where the closed canvas is dark (it reads), and never above the bloom bound
            D3 open1 = NocScreenMath.Eye(424.2, 71.4, 465.0, 0.5, NocScreenMath.ContentMaxM, 1.0, out double w1);   // sclera
            D3 field = NocScreenMath.Eye(330.0, 115.0, 465.0, 0.5, NocScreenMath.ContentMaxM, 1.0, out double w2);  // canvas field
            Assert.AreEqual(1.0, w1, 1e-9); Assert.AreEqual(1.0, w2, 1e-9);
            Assert.Greater(NocMath.Luma709(open1.x, open1.y, open1.z), 3.0 * NocMath.Luma709(field.x, field.y, field.z));
        }

        [Test]
        public void ScaleMarkers_CradlesAndLedges()
        {
            // fix wave C17: four cradles riding at <= 0.2 m/s with exact 600 s wraps, clear of the ledges and of each other; the ledges
            // flush (within the audit's 0.5 m) on row seams; at least two cradles and a ledge lamp row on the KS16 frame at t 460
            var k = Shot("KS16");
            var v = new QaView(k.x, k.y, k.z, k.yaw, k.pitch, k.fov, 1920, 1080);
            int inFrame = 0;
            for (int i = 0; i < SignsLayout.CradleZ.Length; i++)
            {
                double vmax = 0, lo = double.MaxValue, hi = double.MinValue;
                for (int f = 0; f < 6000; f++)
                {
                    double y0 = SignsLayout.CradleY(i, f * 0.1), y1 = SignsLayout.CradleY(i, (f + 1) * 0.1);
                    vmax = Math.Max(vmax, Math.Abs(y1 - y0) / 0.1); lo = Math.Min(lo, y0); hi = Math.Max(hi, y0);
                }
                Assert.LessOrEqual(vmax, 0.2, "cradle speed <= 0.2 m/s");
                Assert.AreEqual(SignsLayout.CradleY(i, 0.0), SignsLayout.CradleY(i, 600.0), 1e-9, "exact wrap");
                Assert.That(lo, Is.InRange(30.0, 250.0)); Assert.That(hi + SignsLayout.CradleH, Is.InRange(30.0, 250.0));
                foreach (double ly in SignsLayout.LedgeY)
                    Assert.IsTrue(hi + 2.0 < ly - 0.5 * SignsLayout.LedgeT || lo > ly + 0.5 * SignsLayout.LedgeT + 0.12, FormattableString.Invariant($"cradle {i} clears the ledge at y {ly}"));
                if (QaProjection.ProjectPoint(v, new D3(SignsLayout.CradleX0 + 0.5, SignsLayout.CradleY(i, 460.0) + 0.75, SignsLayout.CradleZ[i]), out double x, out double y)
                    && Math.Abs(x) < 0.97 && Math.Abs(y) < 0.97) inFrame++;
            }
            int lamps = 0;
            for (double z = SignsLayout.LedgeLampZ0; z <= SignsLayout.WallZ1 - 2.0; z += SignsLayout.LedgeLampPitch)
                if (QaProjection.ProjectPoint(v, new D3(874.24, SignsLayout.LedgeY[0] - 0.2, z), out double x, out double y) && Math.Abs(x) < 1.0 && Math.Abs(y) < 1.0) lamps++;
            foreach (double ly in SignsLayout.LedgeY) Assert.AreEqual(0.0, Math.IEEERemainder(ly, SignsLayout.PanelPitchY), 1e-9, "ledge on a row seam");
            Assert.Less(SignsLayout.LedgeX1 - SignsLayout.WallX1, 0.5, "the ledge is flush");
            Debug.Log(FormattableString.Invariant($"[NOC] scale markers: {inFrame} cradles and {lamps} lamps of the y 81.6 ledge on the KS16 frame at t 460"));
            Assert.GreaterOrEqual(inFrame, 2); Assert.GreaterOrEqual(lamps, 6);
            var ledges = ScreenWallBuilder.Ledges(new MeshBuilder(), null);
            AssertClearOfLane("ledges", ledges, ScreenWallBuilder.Origin, 1200);
            AssertClearOfLane("cables", ScreenWallBuilder.Cables(), ScreenWallBuilder.Origin, 1200);
        }

        [Test]
        public void Panels_FailureAndAgeRates_AreStatic()
        {
            int dead = 0, deadMod = 0, stuck = 0, aged = 0, n = 0;
            double kbSum = 0;
            for (int iz = 0; iz < 7; iz++)
                for (int iy = -74; iy < 28; iy++)
                {
                    NocScreenMath.Panel(iz, iy, out double kb, out D3 tint, out bool ag, out double d, out double mm, out int sc);
                    n++; kbSum += kb;
                    if (d < 1) dead++; if (mm < 1) deadMod++; if (sc >= 0) stuck++; if (ag) aged++;
                    Assert.That(tint.x, Is.InRange(0.9, 1.1)); Assert.That(tint.z, Is.InRange(0.8, 1.06));
                    // static: the same hash gives the same panel at any time (nothing flashes or pops)
                    NocScreenMath.Panel(iz, iy, out double kb2, out _, out _, out _, out _, out _);
                    Assert.AreEqual(kb, kb2);
                }
            Debug.Log(FormattableString.Invariant($"[NOC] panels: {n}, dead {dead} ({100.0 * dead / n:F1} %), dead modules {deadMod}, stuck columns {stuck}, aged {aged} ({100.0 * aged / n:F0} %), mean brightness {kbSum / n:F3}"));
            Assert.That(dead / (double)n, Is.InRange(0.002, 0.03));
            Assert.That(aged / (double)n, Is.InRange(0.12, 0.25));
            Assert.Greater(deadMod, 0); Assert.Greater(stuck, 0);
            Assert.That(kbSum / n, Is.InRange(0.93, 0.99));
        }

        [Test]
        public void GlyphSigns_NineGiants_OffSightLinesAndLane()
        {
            Assert.AreEqual(9, SignsLayout.GlyphSigns.Length, "nine giant glyph signs (14.5.6)");
            foreach (var g in SignsLayout.GlyphSigns)
            {
                Assert.That(g.h, Is.InRange(60.0, 120.0), "60-120 m");
                Assert.GreaterOrEqual(g.y0, 20.0); Assert.LessOrEqual(g.y0 + g.h, 420.0, "y +20..+420");
                Assert.That(g.z - (g.blade ? 0.5 * SignsLayout.BladeT : 0.5 * g.w), Is.GreaterThan(SignsLayout.WallZ0 + 5.0));
                Assert.That(g.z + (g.blade ? 0.5 * SignsLayout.BladeT : 0.5 * g.w), Is.LessThan(SignsLayout.WallZ1 - 5.0));
                Assert.AreNotEqual(0, g.palette, "no magenta (LOOK_V2 R1)"); Assert.AreNotEqual(3, g.palette); Assert.AreNotEqual(6, g.palette);
            }
            var faces = new List<ArcadeBuilder.Sign>();
            var housings = ScreenWallBuilder.SignHousings(faces);
            Assert.AreEqual(11, faces.Count, "two blades x 2 faces + seven boards (fix wave C16: z 438, 490, 505 became flush boards)");
            AssertClearOfLane("glyph signs", housings, ScreenWallBuilder.Origin, 1200);
            // KS16's three wall points stay visible: the eye -> point segments miss every housing box
            var k = Shot("KS16");
            var eye = new D3(k.x, k.y, k.z);
            foreach (var pt in new[] { new D3(874, 10, 300), new D3(874, 150, 350), new D3(874, 300, 420) })
                foreach (var g in SignsLayout.GlyphSigns)
                {
                    double x0 = SignsLayout.WallX1 - 0.1, x1 = g.blade ? SignsLayout.BladeX0 + g.w + 0.3 : SignsLayout.BoardX;
                    double r = g.blade ? SignsLayout.BladeRim : SignsLayout.BoardRim;
                    double z0 = g.blade ? g.z - 0.5 * SignsLayout.BladeT : g.z - 0.5 * g.w - r, z1 = g.blade ? g.z + 0.5 * SignsLayout.BladeT : g.z + 0.5 * g.w + r;
                    double y0 = g.y0 - r, y1 = g.y0 + g.h + r + (g.blade ? 14.0 : 0.0);
                    Assert.IsFalse(SegmentHitsBox(eye, pt, x0, x1, y0, y1, z0, z1), FormattableString.Invariant($"sign at z {g.z} hides the KS16 wall point ({pt.y}, {pt.z})"));
                }
        }

        static bool SegmentHitsBox(D3 a, D3 b, double x0, double x1, double y0, double y1, double z0, double z1)
        {
            double t0 = 0, t1 = 0.999;
            double[] o = { a.x, a.y, a.z }, d = { b.x - a.x, b.y - a.y, b.z - a.z }, lo = { x0, y0, z0 }, hi = { x1, y1, z1 };
            for (int i = 0; i < 3; i++)
            {
                if (Math.Abs(d[i]) < 1e-12) { if (o[i] < lo[i] || o[i] > hi[i]) return false; continue; }
                double ta = (lo[i] - o[i]) / d[i], tb = (hi[i] - o[i]) / d[i];
                if (ta > tb) { var tmp = ta; ta = tb; tb = tmp; }
                t0 = Math.Max(t0, ta); t1 = Math.Min(t1, tb);
                if (t0 > t1) return false;
            }
            return true;
        }

        [Test]
        public void Arcade_BelowRoofLine_OffLane_ShopsAndBackSide()
        {
            var signs = new List<ArcadeBuilder.Sign>(); var heads = new List<Vector3>();
            var parts = ArcadeBuilder.BuildParts(signs, heads);
            int shops = 0;
            foreach (var p in parts)
            {
                var b = WorldBounds(p.Mb, ArcadeBuilder.Origin);
                Assert.LessOrEqual(b.max.y, SignsLayout.ArcH + 0.011, p.Name + ": nothing above the 38 m roof line (KS17 koi composition)");
                Assert.GreaterOrEqual(b.min.x, SignsLayout.ArcX0 - 0.35f, p.Name); Assert.LessOrEqual(b.max.x, SignsLayout.ArcX1 + 1.3f, p.Name);
                Assert.GreaterOrEqual(b.min.z, SignsLayout.ArcZ0 - 0.4f, p.Name); Assert.LessOrEqual(b.max.z, SignsLayout.ArcZ1 + 1.3f, p.Name);
                AssertClearOfLane("arcade " + p.Name, p.Mb, ArcadeBuilder.Origin, 1240);
                if (p.Mat >= ArcadeBuilder.Mat.ShopCold) shops += p.Mb.TriangleCount / 2;
            }
            Debug.Log(FormattableString.Invariant($"[NOC] arcade: {parts.Count} parts, {shops} shop rooms, {signs.Count} shop signs, {heads.Count} maintenance lights"));
            Assert.That(shops, Is.InRange(30, 37), "one shop per bay (14 + 9 + 14 bays, the corner bays cut back to the core)");
            Assert.Greater(signs.Count, 20); Assert.Greater(heads.Count, 20);
            foreach (var s in signs) Assert.AreNotEqual(0, s.palette);
            // no warm shop sign on the faces seen from the Heart (LOOK_V2 R6: beat 7 has no amber)
            foreach (var s in signs) if (s.centre.x > SignsLayout.ArcX0 + 2f) Assert.AreNotEqual(2, s.palette, "amber sign off the promenade face");
            double dc = NocLanes.DistanceOutsideLanes(new Vector3((float)SignsLayout.ArcX0, 0f, (float)SignsLayout.ArcZ1), 1240);
            Debug.Log(FormattableString.Invariant($"[NOC] arcade NW corner {dc:F3} m outside the lane"));
            Assert.Greater(dc, 2.0);
            // the sign-wall base: one room per bay, signs on the open ones, everything west of x 882.3 and below the canopy
            var bsigns = new List<ArcadeBuilder.Sign>();
            var rooms = Beat06_Signs.SignBaseRooms(bsigns);
            int nRooms = 0; foreach (var r in rooms) nRooms += r.TriangleCount / 2;
            Assert.AreEqual(SignBaseRelief.ShopBays().Count, nRooms);
            foreach (var s in bsigns) { Assert.Less(s.centre.x + 0.5f * s.size.x, 882.3f); Assert.Less(s.centre.y + 0.5f * s.size.y, 6.0f); }
        }

        [Test]
        public void ShopStates_MixGainsAndColours()
        {
            // INT-34 / r01: the rooms read as a band of flat white boxes. Every shop panel now carries a NocShopfront state (BV's vertex
            // colour): a mix of lit, dim, half-shuttered, dark and shuttered shops, no panel above the lit room's gain, and the brightest
            // room's mean emission (token x ShopLight x 0.6 x gain) stays under 3 x the LED wall's mean emission, so the wall stays the
            // subject of KS16 and the rooms are accents
            int n = 0, lit = 0, dim = 0, half = 0, dark = 0, shut = 0;
            for (uint i = 0; i < 4000; i++)
            {
                var p = G4Kit.PickShop(NocHash.Pcg(i * 2654435761u + 7u), (i % 6u) == 0u);
                n++;
                Assert.That(p.Gain, Is.InRange(0.10, 0.70));
                var c = G4Kit.ShopColour(p);
                Assert.AreEqual(p.State * 32, c.b);
                Assert.AreEqual(p.Open, p.State == 0 || p.State == 2);
                if (p.State == 0 && p.Gain >= 0.4) lit++; else if (p.State == 0) dim++; else if (p.State == 2) half++; else if (p.State == 3) dark++; else shut++;
            }
            Debug.Log(FormattableString.Invariant($"[NOC] shop states: lit {100.0 * lit / n:F1} %, dim {100.0 * dim / n:F1} %, half {100.0 * half / n:F1} %, dark {100.0 * dark / n:F1} %, shutter {100.0 * shut / n:F1} %"));
            Assert.That((double)lit / n, Is.InRange(0.28, 0.45)); Assert.Greater((double)shut / n, 0.15); Assert.Greater((double)dim / n, 0.10);
            Assert.Greater((double)half / n, 0.06); Assert.Greater((double)dark / n, 0.06);
            double room = 0.0;
            foreach (var (tok, k) in new[] { (NocPal.WinCold, 1.0), (NocPal.SignWhite, 0.9), (NocPal.WinNeutral, 0.85), (NocPal.WinBlue, 0.8) })
            {
                Color w = NocturnePalette.Linear(tok);
                room = Math.Max(room, NocMath.Luma709(w.r, w.g, w.b) * SignsLayout.ShopLight * k * 0.6 * 0.85);
            }
            D3 wall = NocScreenMath.MirrorMacro(SignsLayout.WavesFull, 1.0);
            double lw = NocMath.Luma709(wall.x, wall.y, wall.z);
            Debug.Log(FormattableString.Invariant($"[NOC] brightest shop room mean Y {room:F4} = {room / lw:F2} x the LED wall mean {lw:F4}"));
            Assert.Less(room, 3.0 * lw);
            // the built rows: every arcade bay and every sign-base bay has a state; closed bays are shutters
            int states = 0;
            foreach (var r in Beat06_Signs.SignBaseRooms(null)) states += r.TriangleCount / 2;
            Assert.AreEqual(SignBaseRelief.ShopBays().Count, states);
        }

        [Test]
        public void MirrorPlane_Kind2_MeanOfTheLedWall()
        {
            D3 full = NocScreenMath.MirrorMacro(SignsLayout.WavesFull, 1.0), wave = NocScreenMath.MeanEmission(SignsLayout.WavesFull);
            double lf = NocMath.Luma709(full.x, full.y, full.z), lw = NocMath.Luma709(wave.x, wave.y, wave.z);
            Debug.Log(FormattableString.Invariant($"[NOC] H4 wall plane kind {ScreenWallBuilder.MirrorKind}: macro ({full.x:F4}, {full.y:F4}, {full.z:F4}) Y {lf:F4}; the wave alone Y {lw:F4}"));
            Assert.AreEqual(2, ScreenWallBuilder.MirrorKind);
            Assert.That(lf / lw / NocScreenMath.WallMeanGain, Is.InRange(0.65, 0.95), "the LED wall's mean (seams, panels, view angle) below the wall's design mean");
        }

        static void LanePoints(double s0, double s1, double margin, List<Vector3> into)
        {
            for (double s = s0; s <= s1; s += 0.5)
            {
                double h = NocLanes.LaneHalfWidth(s) + margin;
                for (double l = -h; l <= h + 1e-9; l += 0.5) into.Add(NocLanes.CorridorToWorld(s, l, 0.0));
            }
        }

        [Test]
        public void Floors_CoverTheLanePlusOneMetre()
        {
            var pts = new List<Vector3>(20000);
            LanePoints(NocturneConstants.BeatBoundaries[5], NocturneConstants.BeatBoundaries[6], 1.0, pts);
            int terrace = 0, prom = 0, heart = 0;
            foreach (var p in pts)
            {
                if (p.z <= SignsLayout.PromZ0 + 1e-3) { Assert.That(p.x, Is.InRange(695f, 914f), "G3's terrace x 695 - 914 (agreement 6)"); terrace++; continue; }
                if (p.x >= SignsLayout.PromX0 && p.x <= SignsLayout.PromX1 && p.z <= SignsLayout.PromZ1) { prom++; continue; }
                Assert.IsTrue(HeartFloorBuilder.Covers(p.x, p.z), FormattableString.Invariant($"lane point ({p.x:F2}, {p.z:F2}) not floored"));
                heart++;
            }
            Debug.Log(FormattableString.Invariant($"[NOC] Beat06 lane + 1 m: {terrace} points on G3's terrace (z <= 275), {prom} on the promenade, {heart} on the heart floor"));
            Assert.Greater(prom, 1000);
        }

        /// <summary>Every vertex above the floor lies outside the lane (>= 0.05 m), and >= 0.25 m outside it at camera height.</summary>
        internal static void AssertClearOfLane(string name, MeshBuilder mb, Vector3 origin, double hint)
        {
            double worst = double.MaxValue;
            foreach (var q in mb.Positions)
            {
                Vector3 p = q + origin;
                if (p.y <= 0.02f) continue;
                // global projection (hint 0): a hint far from the point lets the windowed search extrapolate a straight piece
                double d = NocLanes.DistanceOutsideLanes(p);
                if (p.y > 1.3f && p.y < 2.1f) Assert.GreaterOrEqual(d, 0.25, FormattableString.Invariant($"{name}: vertex ({p.x:F2}, {p.y:F2}, {p.z:F2}) within 0.25 m of the lane at camera height"));
                Assert.GreaterOrEqual(d, 0.05, FormattableString.Invariant($"{name}: vertex ({p.x:F2}, {p.y:F2}, {p.z:F2}) inside the lane"));
                worst = Math.Min(worst, d);
            }
            Debug.Log(FormattableString.Invariant($"[NOC] {name}: nearest vertex above the floor {worst:F3} m outside the lane"));
        }

        [Test]
        public void Furniture_StaysOutOfTheLane()
        {
            var planters = new MeshBuilder(); var hedges = new MeshBuilder();
            PromenadeBuilder.Planters(planters, hedges);
            AssertClearOfLane("planters", planters, PromenadeBuilder.Origin, 1200);
            AssertClearOfLane("hedges", hedges, PromenadeBuilder.Origin, 1200);
            var coping = new MeshBuilder(); PromenadeBuilder.Planters(new MeshBuilder(), new MeshBuilder(), coping);
            AssertClearOfLane("planter coping", coping, PromenadeBuilder.Origin, 1200);
            var posts = new MeshBuilder(); var heads = new MeshBuilder(); var lenses = new MeshBuilder();
            PromenadeBuilder.Lamps(posts, heads, lenses);
            AssertClearOfLane("lamp posts", posts, PromenadeBuilder.Origin, 1200);
            AssertClearOfLane("lamp fixtures", heads, PromenadeBuilder.Origin, 1200);
            AssertClearOfLane("lamp lenses", lenses, PromenadeBuilder.Origin, 1200);
            var pts = new List<Vector3>(); var yaws = new List<float>();
            PromenadeBuilder.BollardPoints(pts, yaws);
            var bollards = new MeshBuilder();
            PromenadeBuilder.Bollards(bollards, pts, yaws, PromenadeBuilder.Origin);
            AssertClearOfLane("bollards", bollards, PromenadeBuilder.Origin, 1200);
            var rings = new MeshBuilder();
            Assert.Greater(PromenadeBuilder.BollardRings(rings, pts, PromenadeBuilder.Origin), 5);
            AssertClearOfLane("bollard rings", rings, PromenadeBuilder.Origin, 1200);
            Assert.AreEqual(12, SignsLayout.LampZ.Length);
            // the lamp lens hangs over the planter strip, 6 m up (14.5.6 lamp posts 6 m)
            for (int i = 0; i < SignsLayout.LampZ.Length; i++) Assert.AreEqual(5.89, PromenadeBuilder.LensCentre(i).y, 0.02);
        }

        [Test]
        public void Meshes_Validate_AndBudgets()
        {
            var list = new List<KeyValuePair<string, MeshBuilder>>
            {
                new KeyValuePair<string, MeshBuilder>("screenFace", ScreenWallBuilder.ScreenFace()),
                new KeyValuePair<string, MeshBuilder>("screenMass", ScreenWallBuilder.MassFaces()),
                new KeyValuePair<string, MeshBuilder>("promenadeFloor", PromenadeBuilder.Floor()),
                new KeyValuePair<string, MeshBuilder>("glyphSignHousings", ScreenWallBuilder.SignHousings(null)),
                new KeyValuePair<string, MeshBuilder>("cradleCables", ScreenWallBuilder.Cables()),
            };
            var ledgeLenses = new MeshBuilder();
            list.Add(new KeyValuePair<string, MeshBuilder>("serviceLedges", ScreenWallBuilder.Ledges(ledgeLenses, null)));
            list.Add(new KeyValuePair<string, MeshBuilder>("serviceLedgeLamps", ledgeLenses));
            for (int i = 0; i < SignsLayout.CradleZ.Length; i++)
            {
                var cs = new MeshBuilder(); var cc = new MeshBuilder(); ScreenWallBuilder.Cradle(cs, cc);
                list.Add(new KeyValuePair<string, MeshBuilder>("cradleSteel", cs)); list.Add(new KeyValuePair<string, MeshBuilder>("cradleCabin", cc));
            }
            foreach (var p in ArcadeBuilder.BuildParts(null, null)) list.Add(new KeyValuePair<string, MeshBuilder>(p.Name, p.Mb));
            foreach (var r in Beat06_Signs.SignBaseRooms(null)) list.Add(new KeyValuePair<string, MeshBuilder>("signBaseShops", r));
            var planters = new MeshBuilder(); var hedges = new MeshBuilder(); PromenadeBuilder.Planters(planters, hedges);
            var posts = new MeshBuilder(); var heads = new MeshBuilder(); var lenses = new MeshBuilder(); PromenadeBuilder.Lamps(posts, heads, lenses);
            var pts = new List<Vector3>(); var yaws = new List<float>(); PromenadeBuilder.BollardPoints(pts, yaws);
            var bollards = new MeshBuilder(); PromenadeBuilder.Bollards(bollards, pts, yaws, PromenadeBuilder.Origin);
            var rings = new MeshBuilder(); PromenadeBuilder.BollardRings(rings, pts, PromenadeBuilder.Origin);
            list.Add(new KeyValuePair<string, MeshBuilder>("lampLenses", lenses));
            list.Add(new KeyValuePair<string, MeshBuilder>("bollardRings", rings));
            list.Add(new KeyValuePair<string, MeshBuilder>("planters", planters));
            list.Add(new KeyValuePair<string, MeshBuilder>("hedges", hedges));
            list.Add(new KeyValuePair<string, MeshBuilder>("lampPosts", posts));
            list.Add(new KeyValuePair<string, MeshBuilder>("lampHeads", heads));
            list.Add(new KeyValuePair<string, MeshBuilder>("bollards", bollards));
            long tris = 0;
            foreach (var kv in list)
            {
                Mesh m = kv.Value.ToMesh(kv.Key);
                var r = MeshValidation.Validate(m);
                Assert.IsTrue(r.Ok, r.ToString());
                tris += WorldContext.TriangleCount(m);
                UnityEngine.Object.DestroyImmediate(m);
            }
            int lights = SignsLayout.WallLightCount + SignsLayout.LampZ.Length;
            Debug.Log(FormattableString.Invariant($"[NOC] Beat06: {tris} triangles (budget 120 k), {lights} real lights (14.2.3 row 6: 24), {pts.Count} bollards"));
            Assert.LessOrEqual(tris, 120000);
            Assert.LessOrEqual(lights, 24);
        }
    }
}
