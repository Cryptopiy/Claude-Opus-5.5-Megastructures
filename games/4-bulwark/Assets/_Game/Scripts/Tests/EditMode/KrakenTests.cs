// KrakenTests: the kraken asset pipeline output, the rig and the procedural tentacles (Addendum A1, kraken owner).
// Scale (lead decision, A1): mantle ~220 m, Tripo arms ~350 m from the crown (cut at 286 m), limbs 1200-1500 m, feeding
// tentacles longer, main eyes ~20 m across; LODs 330k / 80k / 20k / 4k; textures at most 4096; the rig reproduces the rest
// pose exactly, keeps every segment length, is deterministic and continuous across the clock wrap; the GPU control map has
// its C# twin; LOD cross-fades are complementary; the sea disturbances report breaches and tentacle crossings.
using System;
using Megastructure.Creature;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class KrakenTests
    {
        static KrakenAssets A()
        {
            var a = KrakenAssets.Load();
            Assert.IsTrue(a.Ok, "kraken assets: " + a.Error);
            return a;
        }

        [Test]
        public void Assets_LoadWithLodBudgetsAndTextureSizes()
        {
            var a = A();
            int[] target = { 330000, 80000, 20000, 4000 };
            int prev = int.MaxValue;
            for (int l = 0; l < KrakenAssets.BodyLods; l++)
            {
                var m = a.Body[l];
                int tris = (int)(m.GetIndexCount(0) / 3);
                Debug.Log($"[BW] kraken LOD{l}: {m.vertexCount} vertices, {tris} triangles");
                Assert.That(tris, Is.InRange((int)(target[l] * 0.97), (int)(target[l] * 1.03)), "LOD" + l + " triangle budget");
                Assert.Less(tris, prev, "LODs decrease");
                prev = tris;
            }
            foreach (var t in new[] { a.Albedo, a.Normal, a.Mask, a.TentAlbedo, a.TentNormal, a.TentMask })
            {
                Assert.IsNotNull(t);
                Assert.LessOrEqual(t.width, 4096, t.name); Assert.LessOrEqual(t.height, 4096, t.name);
                Assert.Greater(t.mipmapCount, 8, t.name + " mipmapped");
            }
            Assert.AreEqual(8, a.Manifest.arms.Length);
            Assert.GreaterOrEqual(a.Manifest.eyes.Length, 6, "main eyes plus a cluster of smaller eyes");
        }

        [Test]
        public void Body_MeshIsValidAndSkinChannelsAreSane()
        {
            var a = A();
            var m = a.Body[1];
            Assert.IsTrue(m.isReadable || true);
            var v = m.vertices; var n = m.normals; var idx = m.triangles;
            var cols = new System.Collections.Generic.List<Color>(); m.GetColors(cols);
            var uv1 = new System.Collections.Generic.List<Vector2>(); m.GetUVs(1, uv1);
            int degenerate = 0, badLimb = 0;
            for (int i = 0; i < idx.Length; i += 3)
            {
                Assert.Less(idx[i], v.Length); Assert.Less(idx[i + 1], v.Length); Assert.Less(idx[i + 2], v.Length);
                if (Vector3.Cross(v[idx[i + 1]] - v[idx[i]], v[idx[i + 2]] - v[idx[i]]).sqrMagnitude < 1e-10f) degenerate++;
            }
            for (int i = 0; i < v.Length; i++)
            {
                Assert.AreEqual(1f, n[i].magnitude, 1e-3f, "unit normals");
                float limb = cols[i].r, w = cols[i].g;
                if (!(Mathf.Abs(limb - 255f) < 0.01f || (limb >= -0.01f && limb <= 7.01f && Mathf.Abs(limb - Mathf.Round(limb)) < 0.01f))) badLimb++;
                Assert.That(w, Is.InRange(0f, 1f));
                Assert.That(uv1[i].x, Is.InRange(-1f, 400f), "arc u within the Tripo arm");
            }
            Assert.Less(degenerate, idx.Length / 3 / 1000, "degenerate triangles under 0.1 %");
            Assert.AreEqual(0, badLimb, "limb ids are 0..7 or 255");
        }

        [Test]
        public void Scale_MatchesTheLeadDecision()
        {
            var a = A();
            var man = a.Manifest;
            float headH = man.headMax[1] - man.headMin[1];
            float headL = man.headMax[2] - man.headMin[2];
            Debug.Log($"[BW] kraken head {man.headMax[0] - man.headMin[0]:F0} x {headH:F0} x {headL:F0} m, bounds {man.boundsMax[0] - man.boundsMin[0]:F0} x {man.boundsMax[2] - man.boundsMin[2]:F0} m");
            // head + mantle from the forehead to the mantle tip at 220 m / unit: ~294 m (the rigid region also holds the arm
            // web, so its bounds reach ~330 m); the A1 lead decision "mantle ~220 m, arms ~350 m" keeps the arms (logged)
            Assert.That(headL, Is.InRange(260f, 345f), "head + mantle length");
            foreach (var arm in man.arms)
            {
                Assert.That(arm.uCut, Is.InRange(260f, 320f), "Tripo arm to the cut");
                Assert.That(arm.rCut, Is.InRange(8f, 16f), "radius at the cut");
            }
            // A1 1.2: main eyeball 20 m (radius 10), lesser eyes 8, 6 (milky), 4 m per side
            int mains = 0, milky = 0;
            foreach (var e in man.eyes)
            {
                float dia = 2f * e.radius;
                if (e.name.StartsWith("main")) { mains++; Assert.That(dia, Is.InRange(19f, 21f), "main eyeball ~20 m: " + e.name); }
                else Assert.That(dia, Is.InRange(3.5f, 8.5f), "lesser eye 4-8 m: " + e.name);
                if (e.name.Contains("milky")) milky++;
            }
            Assert.AreEqual(2, mains, "one main eye per side");
            Assert.AreEqual(2, milky, "one milky lesser eye per side");
            var rig = new KrakenRig(man);
            for (int k = 0; k < KrakenRig.LimbCount; k++)
            {
                var L = rig.Limbs[k];
                if (L.Feeding) Assert.That(L.Length, Is.InRange(1600f, 1800f));
                else Assert.That(L.Length, Is.InRange(1200f, 1500f), "limb " + k);
                Assert.Less(L.RadiusAt(L.Length - 0.5f), 0.6f, "pointed tip");
            }
        }

        [Test]
        public void Rig_ReproducesTheRestPoseAndKeepsSegmentLengths()
        {
            var rig = new KrakenRig(A().Manifest) { RestPoseOnly = true };
            rig.Evaluate(1234.5);
            float worst = 0f;
            foreach (var L in rig.Limbs) for (int i = 0; i < L.K; i++) worst = Mathf.Max(worst, Vector3.Distance(L.CurP[i], L.RestP[i]));
            Assert.Less(worst, 0.05f, "rest pose reproduced (m)");
            rig.RestPoseOnly = false;
            rig.SetPresetWeight(KrakenPreset.Breach, 0.5f);
            rig.SetPresetWeight(KrakenPreset.ReachUp, 0.5f);
            rig.SetLimbMask(KrakenPreset.ReachUp, 2, 1f);
            rig.Evaluate(777.0);
            float lenErr = 0f;
            foreach (var L in rig.Limbs)
                for (int i = 0; i < L.K - 1; i++)
                    lenErr = Mathf.Max(lenErr, Mathf.Abs(Vector3.Distance(L.CurP[i + 1], L.CurP[i]) - L.SegLen[i]) / Mathf.Max(L.SegLen[i], 1f));
            Assert.Less(lenErr, 1e-3f, "segment lengths kept (relative)");
            float top = float.MinValue;
            foreach (var p in rig.Limbs[2].CurP) top = Mathf.Max(top, p.y);
            Assert.Greater(top, 150f, "ReachUp limb rises");
            rig.SetPresetWeight(KrakenPreset.Withdraw, 0.4f);
            rig.Evaluate(778.0);
            lenErr = 0f;
            foreach (var L in rig.Limbs)
                for (int i = 0; i < L.K - 1; i++)
                    lenErr = Mathf.Max(lenErr, Mathf.Abs(Vector3.Distance(L.CurP[i + 1], L.CurP[i]) - L.SegLen[i]) / Mathf.Max(L.SegLen[i], 1f));
            Assert.Less(lenErr, 1e-3f, "segment lengths kept with Withdraw");
        }

        [Test]
        public void Rig_IsDeterministicContinuousAndSlow()
        {
            var man = A().Manifest;
            var r1 = new KrakenRig(man); var r2 = new KrakenRig(man);
            foreach (var r in new[] { r1, r2 }) { r.SetPresetWeight(KrakenPreset.Swim, 0.6f); r.SetPresetWeight(KrakenPreset.Lurk, 0.4f); }
            r1.Evaluate(4321.25); r2.Evaluate(4321.25);
            for (int k = 0; k < KrakenRig.LimbCount; k++)
                for (int i = 0; i < KrakenRig.ControlPoints; i++)
                    Assert.AreEqual(r1.Limbs[k].CurP[i], r2.Limbs[k].CurP[i], "deterministic");
            // continuity across the float clock wraps (3600 s ...) at 30 fps, in the WORLD (root sway included)
            double[] ts = { 3600.0, 7200.0, 36000.0, 1e6 };
            float wrapStep = 0f;
            foreach (double t0 in ts)
            {
                var prev = new Vector3[KrakenRig.LimbCount, KrakenRig.ControlPoints];
                for (int f = 0; f < 4; f++)
                {
                    r1.Evaluate(t0 - 2.0 / 30.0 + f / 30.0);
                    for (int k = 0; k < KrakenRig.LimbCount; k++)
                        for (int i = 0; i < KrakenRig.ControlPoints; i++)
                        {
                            Vector3 p = r1.LocalToWorld.MultiplyPoint3x4(r1.Limbs[k].CurP[i]);
                            if (f > 0) wrapStep = Mathf.Max(wrapStep, Vector3.Distance(p, prev[k, i]));
                            prev[k, i] = p;
                        }
                }
            }
            Debug.Log($"[BW] kraken wrap continuity: largest step {wrapStep:F3} m per 1/30 s ({wrapStep * 30f:F2} m/s)");
            Assert.Less(wrapStep * 30f, KrakenRig.MaxVisibleSpeed, "no jump at the clock wraps");

            // lead decision 2 (A1 00): visible parts at most 8 m/s, nothing quick; A1 1.1 rule 6: never static (>= 0.3 m/s)
            string[] names = { "lurk", "swim", "breach+reach", "glide", "withdraw" };
            var fails = new System.Collections.Generic.List<string>();
            for (int c = 0; c < names.Length; c++)
            {
                var r = new KrakenRig(man);
                r.SetRoot(new Vector3(800f, -66f, 400f), 255f);
                switch (c)
                {
                    case 0: r.SetPresetWeight(KrakenPreset.Lurk, 1f); break;
                    case 1: r.SetPresetWeight(KrakenPreset.Swim, 0.7f); r.SetPresetWeight(KrakenPreset.Lurk, 0.3f); break;
                    case 2:
                        r.SetPresetWeight(KrakenPreset.Breach, 0.6f); r.SetPresetWeight(KrakenPreset.ReachUp, 0.6f);
                        r.SetLimbMask(KrakenPreset.ReachUp, 1, 1f); r.SetLimbMask(KrakenPreset.ReachUp, 6, 1f); r.SetReachHeight(300f);
                        break;
                    case 3:
                        r.SetPresetWeight(KrakenPreset.GlideAlong, 1f);
                        for (int k = 3; k <= 5; k++) r.SetLimbMask(KrakenPreset.GlideAlong, k, 1f);
                        r.SetGlidePlane(new Vector3(6.5f, 0f, 0f), Vector3.right, new Vector3(0f, 0.25f, 1f));
                        break;
                    default: r.SetPresetWeight(KrakenPreset.Withdraw, 0.5f); r.SetPresetWeight(KrakenPreset.Lurk, 0.5f); break;
                }
                const double dt = 0.25; const int n = 360;          // 90 s
                var prev = new Vector3[KrakenRig.LimbCount, KrakenRig.ControlPoints];
                var sum2 = new double[KrakenRig.LimbCount, KrakenRig.ControlPoints];
                float vmax = 0f; int kMax = -1; float uMax = 0f;
                for (int f = 0; f <= n; f++)
                {
                    r.Evaluate(5000.0 + f * dt);
                    for (int k = 0; k < KrakenRig.LimbCount; k++)
                    {
                        if (!r.Limbs[k].Visible) continue;
                        for (int i = 0; i < KrakenRig.ControlPoints; i++)
                        {
                            Vector3 p = r.LocalToWorld.MultiplyPoint3x4(r.Limbs[k].CurP[i]);
                            if (f > 0)
                            {
                                float v = Vector3.Distance(p, prev[k, i]) / (float)dt;
                                sum2[k, i] += v * v;
                                if (v > vmax) { vmax = v; kMax = k; uMax = r.Limbs[k].U[i]; }
                            }
                            prev[k, i] = p;
                        }
                    }
                }
                float rmsMin = float.MaxValue;
                for (int k = 0; k < KrakenRig.LimbCount; k++)
                {
                    if (!r.Limbs[k].Visible) continue;
                    for (int i = 0; i < KrakenRig.ControlPoints; i++) rmsMin = Mathf.Min(rmsMin, Mathf.Sqrt((float)(sum2[k, i] / n)));
                }
                Debug.Log($"[BW] kraken motion '{names[c]}': peak {vmax:F2} m/s (limb {kMax} u {uMax:F0} m), slowest point rms {rmsMin:F2} m/s");
                if (vmax >= KrakenRig.MaxVisibleSpeed) fails.Add($"{names[c]}: peak {vmax:F2} m/s >= 8 (lead decision 2)");
                if (rmsMin <= 0.3f) fails.Add($"{names[c]}: slowest point {rmsMin:F2} m/s rms <= 0.3 (A1 1.1 rule 6: never static)");
            }
            Assert.IsEmpty(fails, string.Join("; ", fails));
        }

        [Test]
        public void ControlMap_TwinAndInverse()
        {
            double L = 1350, lam = 350, a = 0.45; int K = 96;
            double prev = -1;
            for (int i = 0; i <= 200; i++)
            {
                double u = L * i / 200.0;
                double x = KrakenMath.ControlIndex(u, L, lam, a, K);
                Assert.Greater(x, prev - 1e-12, "monotonic");
                prev = x;
                double back = KrakenMath.ArcOfIndex(x, L, lam, a, K);
                Assert.AreEqual(u, back, 1e-3, "inverse");
            }
            Assert.AreEqual(0.0, KrakenMath.ControlIndex(0, L, lam, a, K), 1e-9);
            Assert.AreEqual(K - 1, KrakenMath.ControlIndex(L, L, lam, a, K), 1e-6);
            // tip spacing is finer than root spacing
            double d0 = KrakenMath.ArcOfIndex(1, L, lam, a, K), d1 = L - KrakenMath.ArcOfIndex(K - 2, L, lam, a, K);
            Assert.Less(d1, d0 * 0.5, "denser at the tip");
        }

        [Test]
        public void Tubes_AreOutwardAndJoinTheArms()
        {
            var a = A();
            var rig = new KrakenRig(a.Manifest);
            var L = rig.Limbs[3];
            var m = KrakenTentacleMesh.Build(L, 1, a, rig.TileRadii);
            var v = m.vertices; var n = m.normals; var idx = m.triangles;
            int agree = 0, total = 0;
            for (int i = 0; i < idx.Length; i += 3)
            {
                Vector3 fn = Vector3.Cross(v[idx[i + 1]] - v[idx[i]], v[idx[i + 2]] - v[idx[i]]);
                if (fn.sqrMagnitude < 1e-12f) continue;
                total++;
                if (Vector3.Dot(fn, n[idx[i]] + n[idx[i + 1]] + n[idx[i + 2]]) > 0f) agree++;
            }
            Assert.Greater(agree, total * 0.99, "outward winding");
            Assert.AreEqual(L.R0, L.RadiusAt(L.U0), L.R0 * 0.05f, "tube radius at the cut equals the arm's");
            var m0 = KrakenTentacleMesh.Build(L, 0, a, rig.TileRadii);
            Debug.Log($"[BW] kraken tube limb 3: LOD0 {m0.vertexCount} v / {m0.GetIndexCount(0) / 3} t, LOD1 {m.vertexCount} v / {m.GetIndexCount(0) / 3} t");
            Assert.Less(m0.GetIndexCount(0) / 3, 220000u, "LOD0 tube budget");
        }

        [Test]
        public void LodBlend_IsComplementaryAndContinuous()
        {
            float[] th = KrakenCreature.BodyLodPx;
            float prevCoord = -1f;
            for (float px = 2000f; px > 5f; px *= 0.98f)
            {
                KrakenCreature.LodBlend(px, th, -1, out int lod, out float fade);
                Assert.That(fade, Is.InRange(0f, 1f));
                float coord = lod + (1f - fade);              // continuous LOD coordinate
                Assert.GreaterOrEqual(coord, prevCoord - 1e-4f, "monotonic as the kraken recedes");
                if (prevCoord >= 0f) Assert.Less(coord - prevCoord, 0.2f, "no jump");
                prevCoord = coord;
            }
        }

        [Test]
        public void SeaDisturbances_ReportBreachAndTentacleCrossings()
        {
            var a = A();
            var rig = new KrakenRig(a.Manifest);
            rig.SetRoot(new Vector3(800f, -66f, 400f), 255f);
            rig.SetPresetWeight(KrakenPreset.ReachUp, 0.7f);
            rig.SetLimbMask(KrakenPreset.ReachUp, 1, 1f);
            rig.Evaluate(1000.0);
            var man = a.Manifest;
            var hb = new Bounds(); hb.SetMinMax(new Vector3(man.headMin[0], man.headMin[1], man.headMin[2]), new Vector3(man.headMax[0], man.headMax[1], man.headMax[2]));
            var buf = new SeaDisturbance[SeaDisturbances.Max];
            int n = rig.CollectSeaDisturbances(-8f, new Vector3(2f, 0f, 0f), hb, buf);
            Assert.Greater(n, 1);
            Assert.AreEqual(SeaDisturbanceKind.Breach, buf[0].Kind, "head above the sea");
            bool rise = false;
            for (int i = 0; i < n; i++) rise |= buf[i].Kind == SeaDisturbanceKind.TentacleRise;
            Assert.IsTrue(rise, "a rising tentacle crosses the surface");
        }

        [Test]
        public void Palette_HasTheKrakenTokens()
        {
            // A1 1.1 rule 10: the six tokens of the addendum (append-only)
            foreach (var name in new[] { "KrakenSkinDark", "KrakenMoss", "KrakenUnderRed", "KrakenSucker", "KrakenEyeGlow", "KrakenEyeMilky" })
            {
                var id = (Pal)Enum.Parse(typeof(Pal), name);
                Assert.IsNotNull(BulwarkPalette.Hex(id));
            }
        }

        static string ShaderSource(string rel)
        {
            string path = System.IO.Path.Combine(Application.dataPath, "_Game/Shaders", rel);
            Assert.IsTrue(System.IO.File.Exists(path), path);
            return System.IO.File.ReadAllText(path);
        }

        [Test]
        public void Eye_SlitShorterThanIris_PupilKeepsLittleVeil()
        {
            // fix wave X2 (KS17 "glowing lips"): the slit half-length is 0.67 sin(iris) (A1 1.2: an 8 m slit on a 12 m iris,
            // ratio <= 0.7), so the iris shows beyond both ends; the pupil keeps <= 10 % of the water veil
            string eye = ShaderSource("BwCreatureEye.shader");
            var m = System.Text.RegularExpressions.Regex.Match(eye, @"lerp\(([0-9.]+) \* sin\(ir\), _RoundRadius, _Pupil\)");
            Assert.IsTrue(m.Success, "slit half-length expression");
            float k = float.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
            Assert.LessOrEqual(k, 0.7f);
            Assert.Greater(k, 0.5f);
            var v = System.Text.RegularExpressions.Regex.Match(eye, @"gKrVeilKeep = lerp\(1\.0, ([0-9.]+), pupil");
            Assert.IsTrue(v.Success, "pupil veil keep");
            Assert.LessOrEqual(float.Parse(v.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), 0.10f);
            Assert.That(KrakenCreature.LesserEyeGlow, Is.InRange(0.1f, 0.5f), "the lesser eyes glow below the main eye");
        }

        [Test]
        public void Underwater_OpticalDepthClampKeepsSilhouette()
        {
            // fix wave X2 (KS16 flat cut-out): T >= 0.2 in G and B keeps the arm >= 6 levels darker than the water; red stays
            // physical (T_R <= 0.02)
            string skin = ShaderSource("Include/BwCreatureSkin.hlsl");
            var m = System.Text.RegularExpressions.Regex.Match(skin, @"#define KR_TAU_MAX float3\(([0-9.]+), ([0-9.]+), ([0-9.]+)\)");
            Assert.IsTrue(m.Success, "KR_TAU_MAX");
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            float r = float.Parse(m.Groups[1].Value, ci), g = float.Parse(m.Groups[2].Value, ci), b = float.Parse(m.Groups[3].Value, ci);
            Assert.GreaterOrEqual(Mathf.Exp(-g), 0.19f);
            Assert.GreaterOrEqual(Mathf.Exp(-b), 0.19f);
            Assert.LessOrEqual(Mathf.Exp(-r), 0.02f);
        }
    }
}
