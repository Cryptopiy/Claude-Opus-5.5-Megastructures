// MaterialTests.Boulevard: the per-row expectations of BV's rows (Phase 4 wave A, D-033; MaterialFactory.Boulevard.cs):
//   - Tram: Megastructure/NocTram compiles without errors or warnings in every pass, the row uses it with the MetalPaint maps
//     (TP's processed set) and the MetalPaint token; LOOK_V2 6 required subjects: the saloon band mean WinGain x luminance
//     (WinNeutral) in 0.25-0.32 linear, the headlight core SignWhite x HeadGain >= 2.0 in every channel's peak; no metallic float
//     (the shader decides per zone); the clip planes of the shader defaults equal TramSchedule's.
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Megastructure.EditorTools;

namespace Megastructure.Tests
{
    public partial class MaterialTests
    {
        static MatSpec BoulevardRow(string field)
        {
            foreach (var s in MaterialFactory.Specs()) if (s.Field == field) return s;
            Assert.Fail("no material row " + field);
            return null;
        }

        [Test]
        public void Boulevard_TramRow_AndShader()
        {
            var row = BoulevardRow("Tram");
            Assert.AreEqual(MaterialFactory.ShaderTram, row.Shader);
            Assert.IsTrue(row.Textures.ContainsKey("_AlbedoTex") && row.Textures["_AlbedoTex"].Contains("MetalPaint"), "MetalPaint albedo");
            Assert.IsTrue(row.Textures.ContainsKey("_NormalTex") && row.Textures.ContainsKey("_MaskTex"));
            Assert.IsFalse(row.Floats.ContainsKey("_Metallic"));
            Color win = NocturnePalette.Linear(NocPal.WinNeutral);
            double lum = 0.2126 * win.r + 0.7152 * win.g + 0.0722 * win.b;
            double band = row.Floats["_WinGain"] * lum;
            Debug.Log(System.FormattableString.Invariant($"[NOC] tram saloon band mean {band:F3} linear (LOOK_V2 6: 0.25-0.32)"));
            Assert.That(band, Is.InRange(0.25, 0.32));
            Color head = NocturnePalette.Linear(NocPal.SignWhite);
            Assert.GreaterOrEqual(row.Floats["_HeadGain"] * Mathf.Max(head.r, Mathf.Max(head.g, head.b)), 2.0f, "headlight core >= 2.0");
            var sh = MaterialFactory.FindShader(MaterialFactory.ShaderTram);
            Assert.IsNotNull(sh, MaterialFactory.ShaderTram);
            Assert.IsFalse(ShaderUtil.ShaderHasError(sh), "NocTram has compile errors");
            foreach (var msg in ShaderUtil.GetShaderMessages(sh))
                Assert.AreEqual(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Warning == msg.severity ? -1 : 0, 0, "NocTram: " + msg.message);
            var m = new Material(sh);
            try
            {
                Vector4 a = m.GetVector("_ClipA"), b = m.GetVector("_ClipB");
                Assert.AreEqual(-TramSchedule.ClipX, a.w, 1e-3); Assert.AreEqual(1f, a.x);
                Assert.AreEqual(-TramSchedule.ClipZ, b.w, 1e-3); Assert.AreEqual(1f, b.z);
            }
            finally { Object.DestroyImmediate(m); }
        }
    }
}
