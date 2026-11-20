// MaterialTests.Surface: the per-row expectations of the near-surface rows (owner SU; TEXTURE_PLAN 8.3): smoothness variation of
// every large NocSurface / NocWet surface and the 14.4 floor map. Created by TP-0 with the tests moved VERBATIM from
// MaterialTests.cs and handed over to SU.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Megastructure.EditorTools;

namespace Megastructure.Tests
{
    public partial class MaterialTests
    {
        [Test]
        public void EveryLargeSurface_VariesSmoothnessByAtLeast0_1()
        {
            foreach (var spec in MaterialFactory.Specs())
            {
                if (spec.Shader != MaterialFactory.ShaderSurface && spec.Shader != MaterialFactory.ShaderWet) continue;
                Assert.IsTrue(spec.Textures.ContainsKey("_MaskTex"), spec.Field + " has maps");
                var fam = FamilyOf(spec.Textures["_MaskTex"]);
                // spread of the smoothness channel (5th to 95th percentile) over a 128 x 128 grid of the recipe
                var vals = new List<double>();
                for (int j = 0; j < 128; j++) for (int i = 0; i < 128; i++) vals.Add(NocTexRecipes.Sample(fam, (i + 0.5) / 128, (j + 0.5) / 128).smooth);
                vals.Sort();
                double spread = vals[(int)(0.95 * vals.Count)] - vals[(int)(0.05 * vals.Count)];
                double variation;
                if (spec.Shader == MaterialFactory.ShaderSurface)
                    variation = spread * (spec.Floats["_SmoothMax"] - spec.Floats["_SmoothMin"]) + 2.0 * spec.Floats["_MacroSmooth"];
                else
                {
                    // NocWet: alpha_dry x lerp(1.25, 0.8, mask) -> smoothness 1 - sqrt(alpha) spread, plus puddles (alpha 0.08)
                    double a = spec.Floats["_AlphaDry"];
                    double s0 = 1 - Math.Sqrt(a * 1.25), s1 = 1 - Math.Sqrt(a * 0.8);
                    variation = Math.Max(spread * (s1 - s0), (1 - Math.Sqrt(spec.Floats["_AlphaPuddle"])) - (1 - Math.Sqrt(a)));
                }
                Assert.GreaterOrEqual(variation, 0.1, spec.Field + " smoothness variation");
            }
        }

        /// <summary>Look v2 (TEXTURE_PLAN 3.3, 8.3 SU): every NocWet row's _PuddleTh gives its puddle fraction (0.35, the garden path
        /// 0.25) within 0.02 over its own height histogram (independent samples), and the v2 wet smoothness range stays in 0..0.95.</summary>
        [Test]
        public void WetRows_V2PuddleFraction_FromTheirHeightHistograms()
        {
            int rows = 0;
            foreach (var spec in MaterialFactory.Specs())
            {
                if (spec.Shader != MaterialFactory.ShaderWet) continue;
                rows++;
                Assert.IsTrue(spec.Floats.ContainsKey("_PuddleTh"), spec.Field + " has the v2 threshold");
                var fam = FamilyOf(spec.Textures["_MaskTex"]);
                bool plates = spec.Floats["_PlateU"] > 0f;
                double frac = NocSurfaceMath.PuddleFraction(NocSurfaceMath.HeightHistogram(fam, 16384, plates, 5u), spec.Floats["_PuddleTh"], 300000, 99u);
                Assert.AreEqual(spec.Floats["_PuddleFraction"], frac, NocSurfaceConstants.SurfPuddleTol, spec.Field + " puddle fraction");
                if (spec.Field == "FloorMoss")
                {
                    // CRIT0 #10 / FO's request (carried by FX): no puddle blots on the lawn
                    Assert.AreEqual(0f, spec.Floats["_PuddleFraction"], "FloorMoss puddle fraction");
                    Assert.Less(frac, 0.002, "FloorMoss: the threshold sits at the top of the field");
                }
                Assert.That(spec.Floats["_SmoothMin"], Is.InRange(0f, spec.Floats["_SmoothMax"]));
                Assert.LessOrEqual(spec.Floats["_SmoothMax"], 0.95f);
                Debug.Log(FormattableString.Invariant($"[NOC] {spec.Field}: v2 puddle threshold {spec.Floats["_PuddleTh"]:F4}, fraction {frac:F4} (target {spec.Floats["_PuddleFraction"]:F2})"));
                // the photo set (TP-1): its own heights give the same fraction at _PuddleTh2
                if (spec.Floats.TryGetValue("_Tex2", out float on) && on > 0.5f)
                {
                    string key = System.IO.Path.GetFileName(spec.Textures["_MaskTex2"]).Replace("_Mask.png", "");
                    var hs = MaterialFactory.PhotoHeights(key, 16384, plates, 5u);
                    Assert.IsNotNull(hs, spec.Field + " photo heights of " + key);
                    double f2 = NocSurfaceMath.PuddleFraction(hs, spec.Floats["_PuddleTh2"], 300000, 99u);
                    Assert.AreEqual(spec.Floats["_PuddleFraction"], f2, NocSurfaceConstants.SurfPuddleTol, spec.Field + " photo puddle fraction");
                    Debug.Log(FormattableString.Invariant($"[NOC] {spec.Field}: photo {key} threshold {spec.Floats["_PuddleTh2"]:F4}, fraction {f2:F4}"));
                }
            }
            Assert.AreEqual(7, rows, "GrayFloor + the five 14.4 floors + FloorMoss");
        }

        /// <summary>Every SU shader compiles without an error message (the library list does not hold NocGrating, NocLine and
        /// NocGraybox) and the new Look v2 rows exist in the library asset.</summary>
        [Test]
        public void SuShaders_Compile_AndV2RowsExist()
        {
            foreach (var n in new[] { MaterialFactory.ShaderSurface, MaterialFactory.ShaderWet, MaterialFactory.ShaderGlass, MaterialFactory.ShaderGrating,
                                      NocPaths.ShaderLine, NocPaths.ShaderGraybox })
            {
                var sh = MaterialFactory.FindShader(n);
                Assert.IsNotNull(sh, n);
                Assert.IsTrue(sh.isSupported, n + " supported");
                Assert.IsFalse(ShaderUtil.ShaderHasError(sh), n + " has compile errors");
                foreach (var msg in ShaderUtil.GetShaderMessages(sh))
                    Assert.AreNotEqual(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error, msg.severity, n + ": " + msg.message);
            }
            var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(NocPaths.LibraryAsset);
            Assert.IsNotNull(lib);
            Assert.IsNotNull(lib.GlassDark, "GlassDark row");
            Assert.IsNotNull(lib.Grating, "Grating row");
            Assert.AreEqual(MaterialFactory.ShaderGrating, lib.Grating.shader.name);
            Assert.AreEqual(2450, lib.Grating.renderQueue, "AlphaTest queue");
        }

        /// <summary>TEXTURE_PLAN 8.3 SU tests on the photo rows: every module set's tile holds a whole number of modules (whole courses:
        /// of courses), and directional sets (concrete wall stains, bark, gridded sets, plates) never rotate.</summary>
        [Test]
        public void PhotoRows_WholeModules_NoRotationForDirectionalSets()
        {
            var directional = new HashSet<string> { "ConcreteAlley", "StoneWall", "SteelBare", "Brass", "Bark", "Brick", "FloorSteelDeck" };
            int photo = 0;
            foreach (var spec in MaterialFactory.Specs())
            {
                if (!spec.Floats.TryGetValue("_Tex2", out float on) || on < 0.5f) continue;
                photo++;
                if (spec.Floats.TryGetValue("_AntiTiling", out float at) && at > 0.5f)
                {
                    double tile = spec.Floats["_Tile2"];
                    double mu = spec.Floats["_ModuleU"], mv = spec.Floats["_ModuleV"];
                    double nv = tile / mv;
                    Assert.AreEqual(Math.Round(nv), nv, 0.01, spec.Field + " whole modules along v (" + nv.ToString("F3") + ")");
                    if (mu > 0f)
                    {
                        double nu = tile / mu;
                        Assert.AreEqual(Math.Round(nu), nu, 0.01, spec.Field + " whole modules along u (" + nu.ToString("F3") + ")");
                    }
                    Assert.AreEqual(0f, spec.Floats["_HexRotate"], spec.Field + ": a gridded set never rotates");
                }
                if (directional.Contains(spec.Field))
                    Assert.AreEqual(0f, spec.Floats["_HexRotate"], spec.Field + ": directional set, offsets only");
            }
            Assert.GreaterOrEqual(photo, 10, "the photo rows are wired");
        }

        [Test]
        public void FloorForFollows14_4Surfaces()
        {
            var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(NocPaths.LibraryAsset);
            Assert.IsNotNull(lib);
            Assert.AreSame(lib.FloorAlley, lib.FloorFor(60)); Assert.AreSame(lib.FloorStone, lib.FloorFor(300));
            Assert.AreSame(lib.FloorSteelDeck, lib.FloorFor(700)); Assert.AreSame(lib.FloorGardenPath, lib.FloorFor(950));
            Assert.AreSame(lib.FloorStone, lib.FloorFor(1150)); Assert.AreSame(lib.FloorPlaza, lib.FloorFor(1400));
            Assert.AreEqual(MaterialFactory.ShaderWet, lib.GrayFloor.shader.name, "graybox floors use the wet floor");
        }

        static NocTexFamily FamilyOf(string path)
        {
            foreach (var f in NocTexRecipes.All) if (path == NocTextureGen.MaskPath(f)) return f;
            throw new ArgumentException(path);
        }
    }
}
