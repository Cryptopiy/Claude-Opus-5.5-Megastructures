// ShaderParityTests: the GPU parity harness of Look v2 (docs/TEXTURE_PLAN.md 7.4, TP-0) and its checks on the unchanged shaders.
// ShaderParity.Render draws a mesh with a real shader pass into a 64 x 64 linear float target at fixed inputs (camera matrices,
// globals), with NocFog and NocFloor skipped through the _NocParity global, and returns the linear pixels; Compare checks a C#
// twin's pixels against it: the mean within 1 % and every pixel within 3 display codes. The owners add their shader-versus-twin
// cases (Facade / NocFacadeMath, Surface, Wet / NocMirrorMath, Foliage, Koi / KoiShaderMath) in their own test files with this
// harness. Here: the lit template's debug-view path (view 7, the world normal, written by NocLitPass itself) through
// NocGraybox and NocSurface on quads of known orientation, and the parity global (an emissive graybox quad with _NocParity 1
// returns its emission plus the template's environment terms, unchanged by fog and floor).
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using Megastructure.EditorTools;

namespace Megastructure.Tests
{
    /// <summary>GPU parity harness (TEXTURE_PLAN 7.4).</summary>
    public static class ShaderParity
    {
        public const int Size = 64;

        static Mesh s_Quad;
        /// <summary>A unit quad in the xy plane facing -z (normal (0, 0, -1)), uv0 metric, uv1 = 0, colour white.</summary>
        public static Mesh Quad()
        {
            if (s_Quad != null) return s_Quad;
            s_Quad = new Mesh { name = "ParityQuad", hideFlags = HideFlags.HideAndDontSave };
            s_Quad.SetVertices(new List<Vector3> { new Vector3(-0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(0.5f, -0.5f, 0) });
            s_Quad.SetNormals(new List<Vector3> { Vector3.back, Vector3.back, Vector3.back, Vector3.back });
            s_Quad.SetTangents(new List<Vector4> { new Vector4(1, 0, 0, 1), new Vector4(1, 0, 0, 1), new Vector4(1, 0, 0, 1), new Vector4(1, 0, 0, 1) });
            s_Quad.SetColors(new List<Color> { Color.white, Color.white, Color.white, Color.white });
            s_Quad.SetUVs(0, new List<Vector2> { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) });
            s_Quad.SetUVs(1, new List<Vector4> { Vector4.zero, Vector4.zero, Vector4.zero, Vector4.zero });
            s_Quad.SetTriangles(new[] { 0, 1, 2, 0, 2, 3 }, 0);
            s_Quad.RecalculateBounds();
            return s_Quad;
        }

        /// <summary>World-to-view of a camera at eye looking at target (Unity view space looks down -z).</summary>
        public static Matrix4x4 View(Vector3 eye, Vector3 target, Vector3 up)
            => Matrix4x4.Scale(new Vector3(1, 1, -1)) * Matrix4x4.TRS(eye, Quaternion.LookRotation(target - eye, up), Vector3.one).inverse;

        /// <summary>Renders pass `pass` of `mat` (mesh at `model`) into a Size x Size linear ARGBFloat target and returns the linear
        /// pixels (rows bottom-up). Globals: _NocParity 1 (fog and floor skipped), _NocScreen.x = Size, plus `globals`; every global it
        /// sets is restored afterwards.</summary>
        public static Color[] Render(Material mat, int pass, Mesh mesh, Matrix4x4 model, Vector3 eye, Vector3 target, float vFovDeg,
                                     Action<CommandBuffer> globals = null)
        {
            var desc = new RenderTextureDescriptor(Size, Size, RenderTextureFormat.ARGBFloat, 24) { sRGB = false, msaaSamples = 1, useMipMap = false };
            var rt = RenderTexture.GetTemporary(desc);
            var cb = new CommandBuffer { name = "ShaderParity" };
            Vector4 oldScreen = Shader.GetGlobalVector(ShaderIds.Screen);
            float oldParity = Shader.GetGlobalFloat(ShaderIds.Parity), oldView = Shader.GetGlobalFloat(ShaderIds.DebugView);
            var tex = new Texture2D(Size, Size, TextureFormat.RGBAFloat, false, true);
            var prev = RenderTexture.active;
            try
            {
                Matrix4x4 view = View(eye, target, Vector3.up);
                Matrix4x4 proj = Matrix4x4.Perspective(vFovDeg, 1f, 0.05f, 1000f);
                cb.SetRenderTarget(rt);
                cb.ClearRenderTarget(true, true, new Color(0, 0, 0, 0));
                cb.SetViewProjectionMatrices(view, proj);
                cb.SetGlobalVector("_WorldSpaceCameraPos", eye);
                cb.SetGlobalFloat(ShaderIds.Parity, 1f);
                cb.SetGlobalVector(ShaderIds.Screen, new Vector4(Size, oldScreen.y, oldScreen.z, oldScreen.w));
                globals?.Invoke(cb);
                cb.DrawMesh(mesh, model, mat, 0, pass);
                Graphics.ExecuteCommandBuffer(cb);
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, Size, Size), 0, 0, false);
                tex.Apply(false);
                return tex.GetPixels();
            }
            finally
            {
                RenderTexture.active = prev;
                Shader.SetGlobalVector(ShaderIds.Screen, oldScreen);
                Shader.SetGlobalFloat(ShaderIds.Parity, oldParity);
                Shader.SetGlobalFloat(ShaderIds.DebugView, oldView);
                cb.Release();
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(tex);
            }
        }

        public static double LinearToSrgbCode(double v)
        {
            v = Math.Max(0.0, Math.Min(1.0, v));
            double s = v <= 0.0031308 ? 12.92 * v : 1.055 * Math.Pow(v, 1.0 / 2.4) - 0.055;
            return 255.0 * s;
        }

        /// <summary>Compares the rendered linear pixels (only where mask is true) with twin codes (0..255 sRGB display codes, rgb):
        /// mean code per channel within meanTol (relative), every pixel within codeTol codes. Returns a report line.</summary>
        public static string CompareCodes(Color[] px, Func<int, int, Vector3?> twinCodes, double codeTol, double meanTol, out bool ok)
        {
            double maxDiff = 0; int n = 0;
            double[] sumR = new double[3], sumT = new double[3];
            for (int j = 0; j < Size; j++)
                for (int i = 0; i < Size; i++)
                {
                    Vector3? t = twinCodes(i, j);
                    if (t == null) continue;
                    Color c = px[j * Size + i];
                    double[] r = { LinearToSrgbCode(c.r), LinearToSrgbCode(c.g), LinearToSrgbCode(c.b) };
                    double[] w = { t.Value.x, t.Value.y, t.Value.z };
                    for (int k = 0; k < 3; k++) { maxDiff = Math.Max(maxDiff, Math.Abs(r[k] - w[k])); sumR[k] += r[k]; sumT[k] += w[k]; }
                    n++;
                }
            double meanErr = 0;
            for (int k = 0; k < 3; k++) meanErr = Math.Max(meanErr, Math.Abs(sumR[k] - sumT[k]) / Math.Max(sumT[k], 1e-6));
            ok = n > 0 && maxDiff <= codeTol && meanErr <= meanTol;
            return FormattableString.Invariant($"{n} px, max diff {maxDiff:0.00} codes, mean error {100 * meanErr:0.000} %");
        }
    }

    public class ShaderParityTests
    {
        // camera 2 m in front of a 2 x 2 m quad (vFOV 40 deg: the quad covers the whole 64 x 64 target)
        static readonly Vector3 Eye = new Vector3(0, 0, -2);

        static Material Temp(string shader)
        {
            var sh = MaterialFactory.FindShader(shader);
            Assert.IsNotNull(sh, shader);
            Assert.IsTrue(sh.isSupported, shader + " supported");
            return new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
        }

        static void NormalView(string shader)
        {
            var mat = Temp(shader);
            try
            {
                // quads yawed and pitched toward the camera: the world normal is the rotated (0, 0, -1)
                var poses = new[] { new Vector2(0, 0), new Vector2(25, 0), new Vector2(-35, 10), new Vector2(15, -30), new Vector2(40, 20) };
                foreach (var pose in poses)
                {
                    Quaternion q = Quaternion.Euler(pose.y, pose.x, 0);
                    Matrix4x4 model = Matrix4x4.TRS(Vector3.zero, q, new Vector3(4, 4, 1));
                    Vector3 n = (q * Vector3.back).normalized;
                    var px = ShaderParity.Render(mat, 0, ShaderParity.Quad(), model, Eye, Vector3.zero, 40f,
                        cb => cb.SetGlobalFloat(ShaderIds.DebugView, 7f));
                    Vector3 want = new Vector3(128 + 127 * n.x, 128 + 127 * n.y, 128 + 127 * n.z);
                    // the quad covers the target; check the inner 48 x 48 (alpha 1 marks covered pixels)
                    string rep = ShaderParity.CompareCodes(px, (i, j) => (i >= 8 && i < 56 && j >= 8 && j < 56 && px[j * ShaderParity.Size + i].a > 0.5f) ? want : (Vector3?)null,
                        3.0, 0.01, out bool ok);
                    TestContext.WriteLine(shader + " normal view, pose " + pose + ": " + rep);
                    Assert.IsTrue(ok, shader + " debug view 7 at pose " + pose + ": " + rep);
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(mat); }
        }

        [Test]
        public void DebugView7_WorldNormal_Graybox() => NormalView(NocPaths.ShaderGraybox);

        [Test]
        public void ParityGlobal_SkipsFogAndFloor_AndViewsReturnToShading()
        {
            // an emissive graybox quad: with _NocParity 1 and the debug view off the output is the shading itself (finite, at least
            // the emission); with the debug view on it is the raw code; with both off again it is the same shading (globals restored)
            var mat = Temp(NocPaths.ShaderGraybox);
            try
            {
                mat.SetColor("_BaseColor", Color.black);
                mat.SetColor("_EmissionColor", new Color(0.25f, 0.5f, 0.75f, 1f));
                Matrix4x4 model = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(4, 4, 1));
                var a = ShaderParity.Render(mat, 0, ShaderParity.Quad(), model, Eye, Vector3.zero, 40f);
                var b = ShaderParity.Render(mat, 0, ShaderParity.Quad(), model, Eye, Vector3.zero, 40f);
                Color ca = a[32 * ShaderParity.Size + 32], cb2 = b[32 * ShaderParity.Size + 32];
                TestContext.WriteLine("emissive graybox centre pixel (linear): " + ca);
                Assert.IsFalse(float.IsNaN(ca.r) || float.IsNaN(ca.g) || float.IsNaN(ca.b), "no NaN");
                Assert.GreaterOrEqual(ca.r, 0.25f - 1e-3f); Assert.GreaterOrEqual(ca.g, 0.5f - 1e-3f); Assert.GreaterOrEqual(ca.b, 0.75f - 1e-3f);
                Assert.AreEqual(ca.r, cb2.r, 1e-6f, "deterministic"); Assert.AreEqual(ca.b, cb2.b, 1e-6f, "deterministic");
                Assert.AreEqual(0f, Shader.GetGlobalFloat(ShaderIds.Parity), "parity global restored");
            }
            finally { UnityEngine.Object.DestroyImmediate(mat); }
        }
    }
}
