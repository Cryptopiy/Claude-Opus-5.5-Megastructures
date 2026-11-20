// MaterialTests: EditMode tests of the PBR material library and the texture generator (MASTER_PROMPT 8.8, 8.11 items 1-4 and
// 9, Part 3.2 item 7, docs/MATERIALS.md): every recipe tiles exactly; the generated maps exist with mipmaps, anisotropy 8-16
// and the right sRGB flags; each albedo variation map averages 0.5 linear (so the palette token is the material's mean);
// every large surface varies its smoothness by at least 0.1 (texture range x material range + macro noise); dielectrics have
// metallic 0 and metals 1; every library shader compiles, is Always Included and is used by a library material.
// Look v2 (TEXTURE_PLAN 8.3 TP-0): this file keeps the generic tests (TP); the per-row expectations are split by owner into
// MaterialTests.Surface.cs (SU), .Facade.cs (FA), .Look.cs (PA) and .Foliage.cs (FO).
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Megastructure.EditorTools;

namespace Megastructure.Tests
{
    public partial class MaterialTests
    {
        [Test]
        public void Recipes_TileExactly()
        {
            var rnd = new System.Random(5);
            foreach (var f in NocTexRecipes.All)
                for (int k = 0; k < 64; k++)
                {
                    double u = rnd.NextDouble(), v = rnd.NextDouble();
                    var a = NocTexRecipes.Sample(f, u, v);
                    var b = NocTexRecipes.Sample(f, u + 1.0, v);
                    var c = NocTexRecipes.Sample(f, u, v + 1.0);
                    Assert.AreEqual(a.h, b.h, 1e-6, f + " u-period (height)"); Assert.AreEqual(a.r, b.r, 1e-6, f + " u-period (albedo)");
                    Assert.AreEqual(a.h, c.h, 1e-6, f + " v-period (height)"); Assert.AreEqual(a.smooth, c.smooth, 1e-6, f + " v-period (smoothness)");
                }
        }

        [Test]
        public void GeneratedTextures_ImportSettings()
        {
            var problems = new List<string>();
            NocTextureGen.Check(problems);
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void AlbedoVariation_MeanIsHalfLinear()
        {
            foreach (var f in NocTexRecipes.All)
            {
                string abs = NocPaths.Abs(NocTextureGen.AlbedoPath(f));
                Assert.IsTrue(File.Exists(abs), abs);
                var t = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                try
                {
                    Assert.IsTrue(t.LoadImage(File.ReadAllBytes(abs)));
                    var px = t.GetPixels32();
                    double r = 0, g = 0, b = 0;
                    foreach (var p in px) { r += NocturnePalette.SrgbToLinear(p.r / 255.0); g += NocturnePalette.SrgbToLinear(p.g / 255.0); b += NocturnePalette.SrgbToLinear(p.b / 255.0); }
                    r /= px.Length; g /= px.Length; b /= px.Length;
                    Assert.AreEqual(0.5, r, 0.01, f + " R"); Assert.AreEqual(0.5, g, 0.01, f + " G"); Assert.AreEqual(0.5, b, 0.01, f + " B");
                }
                finally { UnityEngine.Object.DestroyImmediate(t); }
            }
        }

        [Test]
        public void MetallicIsZeroOrOne()
        {
            foreach (var spec in MaterialFactory.Specs())
                if (spec.Floats.TryGetValue("_Metallic", out float m))
                    Assert.IsTrue(m == 0f || m == 1f, spec.Field + " metallic " + m + " (8.11 item 1: never 0.5 as a compromise)");
        }

        [Test]
        public void LibraryShaders_CompileAndAreIncluded()
        {
            var used = new HashSet<string>();
            foreach (var spec in MaterialFactory.Specs()) used.Add(spec.Shader);
            foreach (var n in MaterialFactory.LibraryShaders)
            {
                var sh = MaterialFactory.FindShader(n);
                Assert.IsNotNull(sh, n);
                Assert.IsTrue(sh.isSupported, n + " supported");
                Assert.IsFalse(ShaderUtil.ShaderHasError(sh), n + " has compile errors");
                foreach (var msg in ShaderUtil.GetShaderMessages(sh))
                    Assert.AreNotEqual(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error, msg.severity, n + ": " + msg.message);
                Assert.IsTrue(ProjectConfigurator.IsAlwaysIncluded(sh), n + " in Always Included Shaders");
                Assert.IsTrue(used.Contains(n), n + " is used by a library material");
            }
        }
    }
}
