// BoulevardStreetTests (owner BV, Phase 4 wave A): the street level of beat 2 (MASTER_PROMPT 14.5.2; INT-12, INT-22, INT-23,
// CRIT0 #15 / #18) on the built parts and the light specs:
//   * the light budget (14.2.3 row 2, 28 lights; LOOK_V2 6 "lights enabled at once, nearest first"): BoulevardLights' windows
//     never give more than 28 non-zero lights at any camera x, also in the worst case (the tram's three lights counted wherever the
//     walker could stand while it runs); the fades are smooth (no pop: <= 0.01 weight per 60 fps frame at 2.5 m/s); every lamp's
//     pool is at full weight from 40 m ahead of the walker;
//   * the lamp heads are luminaires: a dark housing over every lens (KS05's flat white slabs);
//   * the shops (INT-22): varied states on both sides, never brighter than the lit room (gain <= 0.8 of the 0.6 x room), the state
//     encoding NocShopfront reads, no magenta or violet sign palette, every shop piece inside its arcade or within 1.4 m of the
//     podium face;
//   * the canyon bridge's soffit luminaires (KS04 / KS05) between the ribs, inside the soffit, no deeper than the ribs;
//   * the overhead-line pole luminaires (fifth start): outside the tram sight sector, clear above the tram and its wire, one
//     lens per lamp, wet sources only at x 60..170, and beats 1-2 still under the 32 wet-source cap with no drop.
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
                if (Mathf.Abs(s.centre.z - 195f) < 70f && s.centre.y < 8f && s.centre.x < 200f && !OnAKiosk(s.centre))   // (the terminus sign at x 204.9 stands on its wall; the kiosks' fascia signs)
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
            // fifth start: the flat boxes and the high projecting signs were removed (confetti over the window grid, BV_r02 / r03):
            // about 170 projecting signs over the terrace remain (plus the podium faces' projecting signs, PodiumSignList)
            Assert.Greater(total, 150);
        }

        [Test]
        public void CanyonBridge_SoffitLamps_BetweenTheRibs()
        {
            var lamps = CanyonBridge.SoffitLamps();
            Assert.AreEqual(23 * CanyonBridge.LampRowsX.Length, lamps.Count, "three rows in every whole bay z 104..288");
            double p = GateDims.CbRibPitch, halfRib = 0.5 * GateDims.CbRibW;
            foreach (var (x, z) in lamps)
            {
                // the housing (lens + 0.1 m) inside the soffit's x span east of face A (where the soffit runs the full z range)
                Assert.GreaterOrEqual(x - 0.5 * CanyonBridge.LampL - 0.1, EdgeDims.FaceXA, "x " + x);
                Assert.LessOrEqual(x + 0.5 * CanyonBridge.LampL + 0.1, GateDims.CbX1 - 0.3 - 0.5, "clear of the east fascia");
                Assert.GreaterOrEqual(z - 0.5 * CanyonBridge.LampW - 0.1, GateDims.CbZ0);
                Assert.LessOrEqual(z + 0.5 * CanyonBridge.LampW + 0.1, GateDims.CbZ1);
                double dz = Math.Abs(z - p * Math.Round(z / p));
                Assert.GreaterOrEqual(dz - halfRib - 0.5 * CanyonBridge.LampW - 0.1, 2.0, "2 m clear of the nearest rib at z " + z);
            }
            double yLow = GateDims.CbY0 - CanyonBridge.LampDrop - 0.03;
            Assert.GreaterOrEqual(yLow, 105.6, "nothing below y 105.6 (the bridge's floor)");
            Assert.Greater(yLow, GateDims.CbY0 - GateDims.CbRibD, "the lenses hang less deep than the ribs");
        }

        [Test]
        public void PoleLamps_OutsideTheSector_AboveTheTram()
        {
            var bases = TramBuilder.PoleLampBases();
            Assert.GreaterOrEqual(bases.Count, 6, "a luminaire on every straight pole east of x 0");
            var lens = Bounds(Parts.Find("tramPoleLampLenses"));
            var shell = Bounds(Parts.Find("tramPoleLampHousings"));
            var mast = Bounds(Parts.Find("tramPoleLampMasts"));
            Debug.Log(FormattableString.Invariant($"[NOC] pole lamps: {bases.Count}; lenses x {lens.min.x:F2}..{lens.max.x:F2} y {lens.min.y:F3}..{lens.max.y:F3} z {lens.min.z:F3}..{lens.max.z:F3}; shells z {shell.min.z:F3}..{shell.max.z:F3}; masts y ..{mast.max.y:F3}"));
            foreach (var b in new[] { lens, shell, mast })
            {
                Assert.Greater(b.min.z, BoulevardLayout.SectorZ1 + 5.0, "north of the sight sector z 195-225 and the track");
                Assert.Greater(b.min.y, TramSchedule.MessengerY + 0.3, "above the messenger wire (and the tram roof 3.9 m)");
                Assert.Less(b.max.z, BoulevardLayout.NorthZ - BoulevardLayout.ArcadeDepth, "in front of the arcade");
            }
            Assert.AreEqual(0, Parts.Find("tramPoleLampLenses").Mb.VertexCount % bases.Count, "one lens per lamp");
            NocWetRegistry.Clear();
            try
            {
                TramBuilder.RegisterSources();
                int n = NocWetRegistry.TotalSources;
                int want = 0; foreach (var b in bases) if (b.x >= TramBuilder.PoleLampWetX0 && b.x <= TramBuilder.PoleLampWetX1) want++;
                Assert.AreEqual(want, n);
                NocWetRegistry.Clear();
                Beat01_Alley.RegisterWet();
                Beat02_Boulevard.RegisterWet();
                int max = NocWetRegistry.MaxActive();
                Debug.Log(FormattableString.Invariant($"[NOC] beats 1-2 wet sources with the pole lamps: {NocWetRegistry.TotalSources} total, at most {max} active (cap 32)"));
                Assert.LessOrEqual(max, 31);
                for (double s = 100; s <= 400; s += 2) { NocWetRegistry.Collect(s); Assert.AreEqual(0, NocWetRegistry.Dropped, "s " + s); }
            }
            finally { NocWetRegistry.Clear(); }
        }

        /// <summary>True for a sign on a promenade kiosk's fascia (BV fix wave: the kiosks' small signs over their hatches).</summary>
        static bool OnAKiosk(Vector3 c)
        {
            foreach (var o in MedianPromenade.PromenadeFurniture())
                if (o.Kind == MedianPromenade.ObstacleKind.Kiosk && Mathf.Abs(c.z - (195f + o.Dz)) < 0.7f && Mathf.Abs(c.x - (o.X - 0.5f * (float)BoulevardLayout.KioskW)) < 0.05f && c.y < o.Size.y)
                    return true;
            return false;
        }

        // ------------------------------------------------------------------ BV fix wave (panel p4_01 C15, scale-06)
        /// <summary>C15: the promenade furniture east of C1: 12 benches, planters, 2 kiosks; every piece off the autowalk line
        /// (capsule 0.35 + 0.3 m), inside the wall bots' lines (lane 11.0 - inset 0.40 - capsule 0.35 - 0.10 m), only benches
        /// (&lt;= 0.5 m) in the north half of the tram sight sector, nothing above 0.5 m across x 100 (the median-width probe), a
        /// 0.6 m passage between any two pieces; one Default-layer collider per piece; KS04 and KS06 each see a 0.45-1 m piece
        /// 5-15 m from the eye inside the frame.</summary>
        [Test]
        public void Promenade_HumanScaleFurniture_NearTheWalker_ClearOfTheLanes()
        {
            var list = MedianPromenade.PromenadeFurniture();
            int benches = 0, planters = 0, kiosks = 0;
            double botLine = 11.0 - 0.40 - 0.35 - 0.10;
            for (int i = 0; i < list.Count; i++)
            {
                var o = list[i];
                if (o.Kind == MedianPromenade.ObstacleKind.Bench) benches++; else if (o.Kind == MedianPromenade.ObstacleKind.Planter) planters++; else kiosks++;
                MedianPromenade.DzSpan(o, out float near, out float far);
                Assert.GreaterOrEqual(near, 0.35f + 0.3f, "off the autowalk line at x " + o.X);
                Assert.LessOrEqual(far, (float)botLine, "inside the wall bots lines at x " + o.X);
                Assert.Greater(o.X - 0.5f * o.Size.x, 24f, "east of the C1 inner arc (the east leg)");
                Assert.Less(o.X + 0.5f * o.Size.x, 205f, "west of the terminus");
                bool sectorX = o.X + 0.5f * o.Size.x > BoulevardLayout.SectorX0 && o.X - 0.5f * o.Size.x < BoulevardLayout.SectorX1;
                if (sectorX && o.Dz + 0.5f * o.Size.z > 0f) Assert.LessOrEqual(o.Size.y, (float)BoulevardLayout.SectorMaxH, "only pieces of at most 0.5 m in the sector north half at x " + o.X);
                if (o.X - 0.5f * o.Size.x < 100f && o.X + 0.5f * o.Size.x > 100f) Assert.LessOrEqual(o.Size.y, 0.5f, "x 100 stays clear above 0.5 m");
                for (int j = 0; j < i; j++)
                {
                    var q = list[j];
                    bool ox = Mathf.Abs(o.X - q.X) < 0.5f * (o.Size.x + q.Size.x) + 0.6f, oz = Mathf.Abs(o.Dz - q.Dz) < 0.5f * (o.Size.z + q.Size.z) + 0.6f;
                    Assert.IsFalse(ox && oz, "pieces at x " + o.X + " and " + q.X + " leave no 0.6 m passage");
                }
            }
            Assert.AreEqual(BoulevardLayout.Benches, benches, "the 12 benches of 14.5.2");
            Assert.AreEqual(2, kiosks); Assert.GreaterOrEqual(planters, 6);
            Assert.Greater(Parts.Find("promenadeSeats").Mb.TriangleCount, 0);
            Assert.Greater(Parts.Find("promenadePlanters").Mb.TriangleCount, 0);
            Assert.AreEqual(3 * benches + kiosks + 6 * planters, Parts.Find("promenadePools").Mb.TriangleCount / 2, "three pools per bench, one per kiosk, six per planter");
            var seats = Bounds(Parts.Find("promenadeSeats"));
            Assert.AreEqual((float)BoulevardLayout.BenchH, seats.max.y, 0.005f, "seat top 0.45 m");
            var pools = Bounds(Parts.Find("promenadePools"));
            Assert.AreEqual(0.01f, pools.max.y, 1e-4f, "pools 1 cm over the floor");
            // colliders
            var root = new GameObject("promenadeTestRoot");
            try
            {
                Assert.AreEqual(list.Count, MedianPromenade.AddColliders(root.transform));
                var cols = root.GetComponentsInChildren<BoxCollider>();
                Assert.AreEqual(list.Count, cols.Length);
                foreach (var c in cols)
                {
                    Assert.AreEqual(0, c.gameObject.layer, "Default layer (not the corridor layer 30)");
                    Assert.GreaterOrEqual(c.size.y, MedianPromenade.ObstacleColliderH - 1e-4f);
                    Assert.IsFalse(c.isTrigger);
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            // the floor of the east leg lane (s 190-395, before the widening onto the tongue, every 0.25 m, lane + 0.35): every beat-2 top off y 0 is promenade
            // furniture (Q1IntegrationTests' floor probe then needs only the "/promenade" exception: REQUEST AB)
            // (BV-t06: a brute-force ray test over every beat-2 triangle took > 10 min: only the parts whose bounds reach into the
            // median above y 0.02 are loaded; floors, facades and the barriers' outer faces cannot stand in the lane)
            var names = new HashSet<string>();
            foreach (var pp in Parts.Parts)
            {
                if (pp.Mb.VertexCount == 0) continue;
                var bb = Bounds(pp);
                if (bb.max.y > 0.02f && bb.min.x < 216f && bb.max.x > -20f && bb.min.z < 206.6f && bb.max.z > 183.4f) names.Add(pp.Name);
            }
            var geo = new G1TestGeo(Parts, n => names.Contains(n));
            int rays = 0, tops = 0, other = 0; string firstOther = null;
            for (double sp = 190.0; sp <= 395.0; sp += 0.5)
            {
                double lane = NocLanes.LaneHalfWidth(sp) + 0.35;
                for (double lat = -lane; lat <= lane + 1e-9; lat += 0.5)
                {
                    Vector3 q = NocLanes.CorridorToWorld(sp + 0.0037, lat + 0.0071, 0.0);
                    rays++;
                    if (!geo.Ray(new Vector3(q.x, 3.5f, q.z), Vector3.down, 4.0f, out float t, out string part)) continue;
                    float y = 3.5f - t;
                    if (Mathf.Abs(y) <= 0.01f) continue;
                    if (part != null && part.StartsWith("promenade")) tops++;
                    else { other++; if (firstOther == null) firstOther = part + " at s " + sp + " lat " + lat + " y " + y; }
                }
            }
            Debug.Log(FormattableString.Invariant($"[NOC] east-leg lane floor: {rays} rays over {names.Count} parts, {tops} on promenade furniture, {other} other tops off y 0 ({firstOther})"));
            Assert.AreEqual(0, other, "only the promenade furniture stands in the lane: " + firstOther);
            Assert.Greater(tops, 60);
            // KS04 / KS06: a 0.45-1 m piece 5-15 m from the eye, its top inside the frame
            foreach (string tag in new[] { "KS04_boulevard_axis", "KS06_tram" })
            {
                var k = Array.Find(NocturneConstants.KeyShots, t => t.tag == tag);
                var v = new QaView(k.x, k.y, k.z, k.yaw, k.pitch, k.fov, 1920, 1080);
                int seen = 0; string what = "";
                foreach (var o in list)
                {
                    if (o.Size.y > 1.01f) continue;
                    var top = new D3(o.X, o.Size.y, 195.0 + o.Dz);
                    double dx = top.x - k.x, dy = top.y - k.y, dz = top.z - k.z, d = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    if (d < 5.0 || d > 15.0) continue;
                    if (!QaProjection.ProjectPoint(v, top, out double nx, out double ny)) continue;
                    if (Math.Abs(nx) > 0.97 || Math.Abs(ny) > 0.97) continue;
                    seen++; what += FormattableString.Invariant($" {o.Kind} x {o.X:F1} dz {o.Dz:F1} at {d:F1} m (ndc {nx:F2}, {ny:F2});");
                }
                Debug.Log("[NOC] " + tag + ": " + seen + " human-scale pieces 5-15 m from the eye:" + what);
                Assert.GreaterOrEqual(seen, tag.StartsWith("KS04") ? 2 : 1, tag);
            }
        }

        /// <summary>D-037 (10): podium-level shop signs on both street walls: 1-4 m, at the column line, y 3.6..7.6 (under the arcade
        /// head), within 1.4 m of the face (out of every lane: |z - 195| &gt; 70), cased (a dark case larger than the faces), no
        /// magenta / violet, about one per shop bay.</summary>
        [Test]
        public void ArcadeSigns_PodiumLevel_OnTheColumns_Housed()
        {
            foreach (int side in new[] { -1, 1 })
            {
                var pals = new List<int>();
                var list = ShopFronts.ArcadeSignList(side, pals);
                float zf = ShopFronts.FaceZ(side);
                int cols = 0;
                for (int k = BoulevardLayout.ColumnK0; k <= BoulevardLayout.ColumnK1; k++)
                {
                    double x = StreetFacade.ColumnX(k);
                    if (side < 0 ? (x >= BoulevardLayout.SouthPodiumX0 + 2.0 && x <= 207.0) : (x >= -36.0 && x <= 199.0)) cols++;
                }
                Debug.Log(FormattableString.Invariant($"[NOC] arcade signs {(side < 0 ? "south" : "north")}: {list.Count} on {cols} columns"));
                Assert.GreaterOrEqual(list.Count, (int)(0.6 * cols), "about one per bay");
                for (int i = 0; i < list.Count; i++)
                {
                    var s = list[i];
                    Assert.GreaterOrEqual(s.y - 0.5f * s.w, ShopFronts.ArcadeSignY0 - 1e-4f);
                    Assert.LessOrEqual(s.y + 0.5f * s.w, ShopFronts.ArcadeSignY1 + 1e-4f, "under the arcade head");
                    float big = Mathf.Max(s.z, s.w);
                    Assert.LessOrEqual(big, 4.0f); Assert.GreaterOrEqual(big, 1.0f);
                    Assert.IsTrue(pals[i] != 0 && pals[i] != 3, "no magenta / violet");
                    Assert.AreEqual(0.0, Math.IEEERemainder(s.x - BoulevardLayout.ColumnX0, BoulevardLayout.ColumnPitch), 1e-4, "on a column line");
                }
                var cases = Bounds(Parts.Find("arcadeSignCases" + (side < 0 ? "S" : "N")));
                Assert.LessOrEqual(side < 0 ? cases.max.z - zf : zf - cases.min.z, 1.4f, "within 1.4 m of the face");
                Assert.GreaterOrEqual(Mathf.Abs((side < 0 ? cases.max.z : cases.min.z) - 195f), 70f, "out of every lane");
                Assert.LessOrEqual(cases.max.y, (float)BoulevardLayout.ArcadeH, "under the podium face");
            }
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
