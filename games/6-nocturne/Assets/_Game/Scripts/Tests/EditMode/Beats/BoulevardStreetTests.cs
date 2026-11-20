// BoulevardStreetTests (owner BV, Phase 4 wave A): the street level of beat 2 (MASTER_PROMPT 14.5.2; INT-12, INT-22, INT-23,
// CRIT0 #15 / #18) on the built parts and the light specs:
//   * the light budget (14.2.3 row 2, 28 lights; LOOK_V2 6 "lights enabled at once, nearest first"): BoulevardLights' windows
//     never give more than 28 non-zero lights at any camera x, also in the worst case (the tram's three lights counted wherever the
//     walker could stand while it runs); the fades are smooth (no pop: <= 0.01 weight per 60 fps frame at 2.5 m/s); every lamp's
//     pool is at full weight from 40 m ahead of the walker;
//   * the lamp heads are luminaires: a dark housing over every lens (KS05's flat white slabs);
//   * the shops (INT-22): varied states on both sides, never brighter than the lit room (gain <= 0.8 of the 0.6 x room), the state
//     encoding NocShopfront reads, no magenta or violet sign palette, every shop piece inside its arcade or within 1.4 m of the
//     podium face.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class BoulevardStreetTests
    {
        static G1Parts s_parts;
        static G1Parts Parts => s_parts ?? (s_parts = Beat02_Boulevard.BuildParts());

        [Test]
        public void LightBudget_AtMost28_SmoothFades()
        {
            var specs = BoulevardLights.Specs();
            int lamps = 0, blades = 0, soffits = 0;
            foreach (var s in specs)
            {
                if (s.Name.StartsWith("lampSpot")) lamps++;
                else if (s.Name.StartsWith("bladeLight")) blades++;
                else if (s.Name.StartsWith("arcadeSoffit")) soffits++;
            }
            Assert.AreEqual(BoulevardLayout.LampCount, lamps, "one spot under every lamp head");
            Assert.AreEqual(BladeSign.Count, blades);
            Assert.AreEqual(BoulevardLights.Soffits.Length, soffits);
            int max = 0, maxWorst = 0; double at = 0, atWorst = 0;
            for (double xc = -400.0; xc <= 460.0; xc += 0.25)
            {
                int n = BoulevardLights.ActiveCount(specs, xc);
                if (n > max) { max = n; at = xc; }
                int w = 0;
                foreach (var s in specs) if (BoulevardLights.Weight(s.X0, s.X1, xc) > 0.0) w++;
                if (xc < BoulevardLights.TramX1 + 1e-9) w += BoulevardLights.TramLights;   // the walker turned back while the tram runs
                if (w > maxWorst) { maxWorst = w; atWorst = xc; }
            }
            Debug.Log(FormattableString.Invariant($"[NOC] beat 2 lights: {specs.Count} managed + {BoulevardLights.TramLights} tram; at most {max} non-zero (camera x {at:F1}), worst case {maxWorst} (x {atWorst:F1}); cap {BoulevardLights.Cap}"));
            Assert.LessOrEqual(max, BoulevardLights.Cap, "14.2.3 beat 2: lights on at once");
            Assert.LessOrEqual(maxWorst, BoulevardLights.Cap, "14.2.3 beat 2: lights on at once, tram anywhere");
            // smooth: the largest weight change over one 60 fps frame at 2.5 m/s
            double dx = 2.5 / 60.0, dmax = 0.0;
            foreach (var s in specs)
                for (double xc = s.X0 - BoulevardLights.Ramp - 1.0; xc <= s.X1 + BoulevardLights.Ramp + 1.0; xc += dx)
                    dmax = Math.Max(dmax, Math.Abs(BoulevardLights.Weight(s.X0, s.X1, xc + dx) - BoulevardLights.Weight(s.X0, s.X1, xc)));
            Debug.Log(FormattableString.Invariant($"[NOC] beat 2 lights: largest weight step per frame {dmax:F5}"));
            Assert.Less(dmax, 0.01, "a light fades over >= 6 s of walking");
            // the pools ahead: every lamp at full weight while the walker is 0..40 m before it (and up to 14 m past it)
            foreach (var s in specs)
            {
                if (!s.Name.StartsWith("lampSpot")) continue;
                foreach (double d in new[] { -40.0, -20.0, 0.0, 10.0 })
                    Assert.AreEqual(1.0, BoulevardLights.Weight(s.X0, s.X1, s.Position.x + d), 1e-9, s.Name + " at " + d);
                Assert.Greater(s.Colour.r, s.Colour.b, "lamp light is warm");
            }
        }

        [Test]
        public void LampHeads_HaveHousingsOverTheirLenses()
        {
            var lens = Bounds(Parts.Find("lampHeads"));
            var hous = Bounds(Parts.Find("lampHousings"));
            float H = (float)BoulevardLayout.LampH;
            Debug.Log(FormattableString.Invariant($"[NOC] lamp lenses y {lens.min.y:F3}..{lens.max.y:F3}, housings y {hous.min.y:F3}..{hous.max.y:F3}"));
            Assert.AreEqual(H - MedianPromenade.LensDrop - MedianPromenade.LensDepth, lens.min.y, 0.01, "lens bottom");
            Assert.Greater(hous.max.y, lens.max.y + 0.15f, "a housing above every lens");
            Assert.LessOrEqual(hous.max.y, H + 0.05f, "the housing stays at the post top");
            // one lens and one housing shell per lamp: the vertex counts are multiples of the lamp count
            Assert.AreEqual(0, Parts.Find("lampHeads").Mb.VertexCount % BoulevardLayout.LampCount);
            Assert.AreEqual(0, Parts.Find("lampHousings").Mb.VertexCount % BoulevardLayout.LampCount);
        }

        [Test]
        public void Shops_Varied_NeverBrighterThanTheLitRoom()
        {
            foreach (int side in new[] { -1, 1 })
            {
                var bays = ShopFronts.Bays(side);
                var n = new int[5];
                int warm = 0, signs = 0, run = 1, longest = 1;
                for (int i = 0; i < bays.Count; i++)
                {
                    var b = bays[i];
                    n[(int)b.State]++;
                    if (b.Warm) warm++;
                    if (b.SignPalette >= 0) { signs++; Assert.IsTrue(b.SignPalette != 0 && b.SignPalette != 3, "no magenta / violet sign"); }
                    Assert.LessOrEqual(b.Gain, 0.80f + 1e-6f, "never brighter than the lit room");
                    Assert.Greater(b.X1 - b.X0, 3.0f);
                    // NocShopfront's encoding (B = state x 32, G = 1 - gain, A = the half shutter's edge)
                    var c = ShopFronts.PanelColour(b);
                    int st = (int)(c.b / 32.0 + 0.5);
                    int want = b.State == ShopFronts.ShopState.Shutter ? 1 : b.State == ShopFronts.ShopState.HalfShutter ? 2 : b.State == ShopFronts.ShopState.Dark ? 3 : 0;
                    Assert.AreEqual(want, st, "state byte");
                    Assert.AreEqual(b.Gain, 1f - c.g / 255f, 0.004f, "gain byte");
                    if (want == 2) Assert.AreEqual(b.ShutterFrac, c.a / 255f, 0.004f);
                    if (i > 0 && bays[i - 1].State == b.State && bays[i - 1].Warm == b.Warm) { run++; longest = Math.Max(longest, run); } else run = 1;
                }
                Debug.Log(FormattableString.Invariant($"[NOC] shops {(side < 0 ? "south" : "north")}: {bays.Count} bays; lit {n[0]}, dim {n[1]}, shutter {n[2]}, half {n[3]}, dark {n[4]}; warm {warm}; fascia signs {signs}; longest identical run {longest}"));
                Assert.GreaterOrEqual(bays.Count, 30);
                int kinds = 0; foreach (int k in n) if (k > 0) kinds++;
                Assert.GreaterOrEqual(kinds, 4, "at least four shop states per side");
                Assert.LessOrEqual(n[0], (int)(0.45 * bays.Count), "lit shops are a minority");
                Assert.GreaterOrEqual(n[2] + n[3], (int)(0.2 * bays.Count), "shutters");
                Assert.LessOrEqual(longest, 4, "no long row of identical shops");
            }
        }

        [Test]
        public void ShopPieces_StayInTheArcadeOrOnThePodiumFace()
        {
            int checkedParts = 0;
            foreach (var p in Parts.Parts)
            {
                bool shop = p.Name.StartsWith("shop") || p.Name.StartsWith("podiumSignBrackets");
                if (!shop || p.Mb.VertexCount == 0) continue;
                checkedParts++;
                int side = p.Name.EndsWith("S") ? -1 : 1;
                float zb = ShopFronts.BackZ(side), zf = ShopFronts.FaceZ(side), nz = ShopFronts.Nz(side);
                foreach (var q0 in p.Mb.Positions)
                {
                    Vector3 q = q0 + p.Origin;
                    float out1 = (q.z - zf) * nz;           // > 0: out in the street, beyond the column line
                    float in1 = (zb - q.z) * nz;            // > 0: behind the arcade back wall
                    Assert.LessOrEqual(out1, 1.45f, p.Name + " reaches into the street at " + q);
                    Assert.LessOrEqual(in1, 0.06f, p.Name + " behind the back wall at " + q);
                    if (out1 > 0.05f) Assert.GreaterOrEqual(q.y, (float)BoulevardLayout.ArcadeH, p.Name + ": only the podium signs project, above the arcade " + q);
                }
            }
            Assert.GreaterOrEqual(checkedParts, 10);
            foreach (var s in Parts.Signs)
            {
                if (s.size.y > 100f) continue;           // the blades
                if (Mathf.Abs(s.centre.z - 195f) < 70f && s.centre.y < 8f && s.centre.x < 200f)   // (the terminus sign at x 204.9 stands on its wall)
                    Assert.Fail("a small sign stands in the street at " + s.centre);
            }
        }

        [Test]
        public void SmallSigns_1to4m_OnTheFaces_ClearOfTheBlades()
        {
            int total = 0;
            foreach (int side in new[] { -1, 1 })
            {
                var list = ShopFronts.ShaftSigns(side);
                int proj = 0, flat = 0, low = 0;
                float zs = ShopFronts.ShaftFaceZ(side), zp = ShopFronts.FaceZ(side), nz = ShopFronts.Nz(side);
                foreach (var s in list)
                {
                    float big = Mathf.Max(s.W, s.H), small = Mathf.Min(s.W, s.H);
                    Assert.LessOrEqual(big, 4.0f + 1e-4f, "a small sign is at most 4 m");
                    Assert.GreaterOrEqual(small, 0.7f - 1e-4f);
                    Assert.IsTrue(s.Palette != 0 && s.Palette != 3, "no magenta / violet");
                    float zface = s.C.y > 40f ? zs : zp;
                    float outFace = (s.C.z - zface) * nz + (s.Kind == 0 ? 0.5f * s.W : 0f);
                    Assert.GreaterOrEqual(outFace, 0.0f, "in front of its face");
                    Assert.LessOrEqual(outFace, 1.7f, "within 1.7 m of its face");
                    // behind a blade (its x span +-7 m, from its bottom up) only below it: the blade covers that face and its brackets
                    // (x +-4.5 m) cross the gap
                    for (int i = 0; i < BladeSign.Count; i++)
                    {
                        BladeSign.Blade(i, out int bs, out double bx, out double _, out double by0);
                        float hw = s.Kind == 1 ? 0.5f * s.W : 0.05f;
                        if (bs == side && s.C.y > 40f && Mathf.Abs(s.C.x - (float)bx) < 7.0f + hw)
                            Assert.Less(s.C.y + 0.5f * s.H, (float)by0, "sign behind blade " + i + " at " + s.C);
                    }
                    if (s.Kind == 0) proj++; else flat++;
                    if (s.C.y < 80f) low++;
                }
                total += list.Count;
                Debug.Log(FormattableString.Invariant($"[NOC] small signs {(side < 0 ? "south" : "north")}: {list.Count} ({proj} projecting, {flat} flat; {low} below y 80)"));
                Assert.Greater(low, list.Count / 2, "densest low, over the terrace");
            }
            int signs = 0; foreach (var s in Parts.Signs) if (s.size.y < 100f) signs++;
            Debug.Log(FormattableString.Invariant($"[NOC] small signs: {total} on the shafts and podium faces; {signs} NocNeon sign quads of 1-8 m in beat 2 (14.5.2: 6 000 shopfront signs, simplified 2 000)"));
            Assert.Greater(total, 400);
        }

        static Bounds Bounds(G1Parts.Part p)
        {
            Assert.IsNotNull(p);
            var b = new Bounds(p.Mb.Positions[0] + p.Origin, Vector3.zero);
            foreach (var q in p.Mb.Positions) b.Encapsulate(q + p.Origin);
            return b;
        }
    }
}
