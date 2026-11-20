using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Megastructure.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Megastructure.Tests
{
    /// <summary>
    /// Project contract (IMPORT_PLAN section 8 "project, shaders compile, scene sanity"; the pattern of Thread's ProjectTests):
    /// the D-008 pipeline and project settings that RebuildAll writes, byte-identical generated HLSL (ShaderGen versus
    /// Tools/gen_hlsl.py), every reflective type name of the scene contract resolves (IMPORT_PLAN 4.9), every Noc shader compiles
    /// and is supported, the material contract, the Main.unity scene (no missing scripts or materials, references set), and
    /// the lesson-5 rule that no global shader name equals a material property name.
    /// </summary>
    public sealed class ProjectTests
    {
        static UniversalRenderPipelineAsset Asset => AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(NocPaths.RpAsset);
        static UniversalRendererData Renderer => AssetDatabase.LoadAssetAtPath<UniversalRendererData>(NocPaths.RendererAsset);

        [Test]
        public void PipelineSettingsOfD008()
        {
            var a = Asset;
            Assert.IsNotNull(a, NocPaths.RpAsset);
            Assert.AreEqual(a, GraphicsSettings.defaultRenderPipeline, "default pipeline");
            Assert.IsTrue(a.supportsHDR); Assert.AreEqual(HDRColorBufferPrecision._64Bits, a.hdrColorBufferPrecision, "64-bit HDR");
            Assert.AreEqual(4, a.msaaSampleCount, "MSAA 4x");
            Assert.AreEqual(1f, a.renderScale);
            Assert.IsFalse(a.supportsCameraDepthTexture, "depth texture off"); Assert.IsFalse(a.supportsCameraOpaqueTexture, "opaque texture off");
            Assert.IsFalse(a.supportsMainLightShadows, "no main-light shadows (14.2.3)"); Assert.IsFalse(a.supportsAdditionalLightShadows, "no additional-light shadows");
            Assert.AreEqual(LightRenderingMode.PerPixel, a.additionalLightsRenderingMode, "additional lights per pixel");
            Assert.AreEqual(GPUResidentDrawerMode.Disabled, a.gpuResidentDrawerMode, "GRD off");
            Assert.IsTrue(a.useSRPBatcher);
            var rd = Renderer;
            Assert.IsNotNull(rd);
            Assert.AreEqual(RenderingMode.ForwardPlus, rd.renderingMode, "Forward+");
            Assert.AreEqual(DepthFormat.Depth_32_Stencil_8, rd.depthAttachmentFormat, "D32_SFloat_S8_UInt");
            Assert.AreEqual(1, QualitySettings.count, "one quality level");
        }

        [Test]
        public void GradeFeatureIsOnTheRendererWithItsMaterial()
        {
            ScriptableRendererFeature grade = null;
            foreach (var f in Renderer.rendererFeatures) if (f != null && f.GetType().FullName == PipelineConfigurator.GradeFeatureType) grade = f;
            Assert.IsNotNull(grade, PipelineConfigurator.GradeFeatureType);
            Assert.IsTrue(grade.isActive);
            var m = new SerializedObject(grade).FindProperty("material").objectReferenceValue as Material;
            Assert.IsNotNull(m); Assert.AreEqual(NocPaths.ShaderGrade, m.shader.name);
            Assert.AreEqual(typeof(NocGradeFeature), EditorUtil.FindType(PipelineConfigurator.GradeFeatureType));
        }

        [Test]
        public void ProjectSettingsContract()
        {
            Assert.AreEqual(ProjectConfigurator.InputSystemOnly, ProjectConfigurator.ActiveInputHandler(), "Active Input Handling = Input System");
            Assert.IsTrue(ProjectConfigurator.ApiIsD3D11Only(), "D3D11 only: " + ProjectConfigurator.ApiList());
            Assert.AreEqual("Nocturne", PlayerSettings.productName);
            Assert.AreEqual("Megastructures", PlayerSettings.companyName);
            Assert.AreEqual(ColorSpace.Linear, PlayerSettings.colorSpace);
            Assert.AreEqual(ProjectConfigurator.AudioSampleRate, ProjectConfigurator.AudioRate(), "48 kHz");
            Assert.AreEqual("Corridor", LayerMask.LayerToName(30)); Assert.AreEqual("Safety", LayerMask.LayerToName(31));
            Assert.IsTrue(PlayerSettings.runInBackground); Assert.IsTrue(PlayerSettings.resizableWindow);
            Assert.AreEqual(FullScreenMode.FullScreenWindow, PlayerSettings.fullScreenMode);
            // the Editor re-enables UnityConnect at start-up; RebuildAll / BuildWindows force it off before they use it
            ProjectConfigurator.EnsureUnityConnectOff(null);
            Assert.AreEqual(0, ProjectConfigurator.UnityConnectEnabled(), "UnityConnectSettings m_Enabled 0 after EnsureUnityConnectOff (audit S3 item 13)");
        }

        /// <summary>Part 7.6 / audit S3 item 20: NocInput is the only runtime file that reads Keyboard / Mouse / Gamepad.</summary>
        [Test]
        public void NocInputIsTheOnlyInputReader()
        {
            string root = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Assets", "_Game");
            var offenders = new List<string>();
            foreach (string f in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                string n = f.Replace(Path.DirectorySeparatorChar, '/');
                if (n.Contains("/Tests/") || n.EndsWith("/NocInput.cs", StringComparison.Ordinal)) continue;
                string text = File.ReadAllText(f);
                if (text.Contains("Keyboard.current") || text.Contains("Mouse.current.") || text.Contains("Gamepad.current") ||
                    text.Contains("using UnityEngine.InputSystem") || text.Contains("UnityEngine.Input."))
                    offenders.Add(n.Substring(n.IndexOf("Assets/", StringComparison.Ordinal)));
            }
            Assert.IsEmpty(offenders, "input read outside NocInput: " + string.Join(", ", offenders));
        }

        /// <summary>Audit S3 item 14: every interpolated [NOC] log line of the runtime formats with the invariant culture.</summary>
        [Test]
        public void LogLinesAreCultureInvariant()
        {
            var apply = typeof(NocCulture).GetMethod("Apply");
            Assert.IsNotNull(apply);
            var attr = (RuntimeInitializeOnLoadMethodAttribute)Attribute.GetCustomAttribute(apply, typeof(RuntimeInitializeOnLoadMethodAttribute));
            Assert.IsNotNull(attr, "NocCulture.Apply runs before the first scene");
            Assert.AreEqual(RuntimeInitializeLoadType.SubsystemRegistration, attr.loadType);
            var prev = System.Threading.Thread.CurrentThread.CurrentCulture;
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
                double x = 1.5;
                Assert.AreEqual("[NOC] 1.50", FormattableString.Invariant($"[NOC] {x:F2}"));
                Assert.AreEqual("1.5", CliArgs.GetDouble("-x", 1.5).ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            finally { System.Threading.Thread.CurrentThread.CurrentCulture = prev; }
        }

        [Test]
        public void TemplateLeftoversAreGone()
        {
            foreach (var p in TemplateCleanup.Candidates)
                Assert.IsTrue(string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(p, AssetPathToGUIDOptions.OnlyExistingAssets)) && !AssetDatabase.IsValidFolder(p), p + " still exists");
        }

        [Test]
        public void ReflectiveTypeNamesResolve()
        {
            foreach (var n in SceneBuilder.AllContractTypes()) Assert.IsNotNull(EditorUtil.FindType(n), n);
            Assert.IsNotNull(EditorUtil.FindType(PipelineConfigurator.GradeFeatureType), PipelineConfigurator.GradeFeatureType);
            Assert.AreEqual(typeof(PathGuide), EditorUtil.FindType(PathGuideProbe.TypeName), "QaCommon.PathGuideProbe.TypeName");
            Assert.IsNotNull(typeof(PathGuide).GetProperty("KillCount"), "PathGuide.KillCount read by reflection");
            Assert.IsNotNull(typeof(PathGuide).GetMethod("CoverageCheck", new[] { typeof(int).MakeByRefType() }), "PathGuide.CoverageCheck(out int)");
        }

        [Test]
        public void GeneratedHlslIsByteIdentical()
        {
            byte[] pal = ShaderGen.Encode(ShaderGen.PaletteHlsl()), con = ShaderGen.Encode(ShaderGen.ConstantsHlsl());
            Assert.IsTrue(ShaderGen.BytesEqual(pal, File.ReadAllBytes(NocPaths.Abs(NocPaths.PaletteHlsl))), "NocPalette.hlsl on disk = ShaderGen");
            Assert.IsTrue(ShaderGen.BytesEqual(con, File.ReadAllBytes(NocPaths.Abs(NocPaths.ConstantsHlsl))), "NocConstantsGen.hlsl on disk = ShaderGen");
            // one header per constants class (TEXTURE_PLAN 8.3 TP-0): no NC_ name twice, every existing class's header on disk
            Assert.IsEmpty(ShaderGen.DuplicateConstantNames(), "NC_ names exported by two classes");
            var headers = new List<KeyValuePair<string, byte[]>>();
            foreach (var c in ShaderGen.ConstantClasses)
            {
                string text = ShaderGen.ConstantsHlsl(c);
                if (text == null) continue;
                byte[] b = ShaderGen.Encode(text);
                headers.Add(new KeyValuePair<string, byte[]>(c.Header, b));
                string abs = NocPaths.Abs(c.AssetPath);
                Assert.IsTrue(File.Exists(abs) && ShaderGen.BytesEqual(b, File.ReadAllBytes(abs)), c.Header + " on disk = ShaderGen (run RebuildAll or gen_hlsl.py)");
            }
            // and the Python bootstrap generator writes the same bytes
            string tmp = Path.Combine(Path.GetTempPath(), "noc_genhlsl_" + Process.GetCurrentProcess().Id);
            try
            {
                var psi = new ProcessStartInfo("python", "-I \"" + Path.Combine(NocPaths.ProjectRoot, "Tools", "gen_hlsl.py") + "\" \"" + tmp + "\"")
                { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
                Process p;
                try { p = Process.Start(psi); }
                catch (Exception e) { Assert.Inconclusive("python not available: " + e.Message); return; }
                p.WaitForExit(60000);
                Assert.AreEqual(0, p.ExitCode, p.StandardError.ReadToEnd());
                Assert.IsTrue(ShaderGen.BytesEqual(pal, File.ReadAllBytes(Path.Combine(tmp, "NocPalette.hlsl"))), "gen_hlsl.py palette");
                Assert.IsTrue(ShaderGen.BytesEqual(con, File.ReadAllBytes(Path.Combine(tmp, "NocConstantsGen.hlsl"))), "gen_hlsl.py constants");
                foreach (var kv in headers)
                {
                    string py = Path.Combine(tmp, kv.Key);
                    Assert.IsTrue(File.Exists(py) && ShaderGen.BytesEqual(kv.Value, File.ReadAllBytes(py)), "gen_hlsl.py " + kv.Key);
                }
            }
            finally { try { if (Directory.Exists(tmp)) Directory.Delete(tmp, true); } catch { } }
        }

        [Test]
        public void EveryNocShaderCompilesAndIsAlwaysIncluded()
        {
            // a builder's new Shaders/Beats shader joins Always Included at the next RebuildAll / BuildWindows; apply that same
            // step here (idempotent), then require every discovered shader to be included
            ProjectConfigurator.EnsureAlwaysIncludedShaders(null);
            foreach (var n in NocPaths.ShaderNames)
            {
                var sh = MaterialFactory.FindShader(n);
                Assert.IsNotNull(sh, n);
                Assert.IsTrue(sh.isSupported, n + " supported");
                Assert.IsFalse(ShaderUtil.ShaderHasError(sh), n + " has compile errors");
                foreach (var msg in ShaderUtil.GetShaderMessages(sh))
                    Assert.AreNotEqual(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error, msg.severity, n + ": " + msg.message);
                Assert.IsTrue(ProjectConfigurator.IsAlwaysIncluded(sh), n + " in Always Included Shaders");
            }
        }

        /// <summary>Phase 3: the shader list is discovered from the files (Shaders/Beats included), never hand-kept.</summary>
        [Test]
        public void ShaderCatalogDiscoversEveryShaderFile()
        {
            var all = ShaderCatalog.All();
            int files = Directory.GetFiles(NocPaths.Abs(NocPaths.ShadersDir), "*.shader", SearchOption.AllDirectories).Length;
            Assert.AreEqual(files, all.Count, "one catalog entry per .shader file under " + NocPaths.ShadersDir);
            var names = new HashSet<string>(NocPaths.ShaderNames);
            foreach (var e in all)
            {
                Assert.IsNotNull(e.name, e.assetPath + " declares a Shader name");
                StringAssert.StartsWith(NocPaths.ShaderNamePrefix, e.name, e.assetPath);
                Assert.IsTrue(names.Contains(e.name), e.name + " in NocPaths.ShaderNames");
                Assert.AreEqual(e.assetPath, ShaderCatalog.PathOf(e.name), e.name);
            }
            Assert.AreEqual(all.Count, names.Count, "no duplicate shader names");
            foreach (var n in new[] { NocPaths.ShaderGraybox, NocPaths.ShaderLine, NocPaths.ShaderSky, NocPaths.ShaderGrade })
                Assert.IsTrue(names.Contains(n), n);
            foreach (var n in MaterialFactory.LibraryShaders) Assert.IsTrue(names.Contains(n), n);
        }

        /// <summary>SceneBuilder keeps Main.unity when only the random local file ids changed (no diff without a reason).</summary>
        [Test]
        public void SceneFileCompareIgnoresIdsButNotContent()
        {
            string text = File.ReadAllText(NocPaths.Abs(NocPaths.MainScene));
            var ids = new List<string>();
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(text, @"^--- !u!\d+ &(\d+)", System.Text.RegularExpressions.RegexOptions.Multiline))
                ids.Add(m.Groups[1].Value);
            Assert.Greater(ids.Count, 10, "documents in Main.unity");
            // renumber every local id (a fresh scene) and reverse the document order
            var map = new Dictionary<string, string>();
            for (int i = 0; i < ids.Count; i++) map[ids[i]] = (900000001 + 7 * i).ToString(System.Globalization.CultureInfo.InvariantCulture);
            string renum = System.Text.RegularExpressions.Regex.Replace(text, @"(&|fileID: )(\d+)", m => map.TryGetValue(m.Groups[2].Value, out var v) ? m.Groups[1].Value + v : m.Value);
            var docs = System.Text.RegularExpressions.Regex.Split(renum, @"(?m)^(?=--- !u!)");
            var head = docs[0];
            var rest = new List<string>(docs);
            rest.RemoveAt(0);
            rest.Reverse();
            string shuffled = head + string.Concat(rest);
            Assert.IsTrue(SceneFileCompare.Equivalent(text, shuffled), "same scene with new ids and another document order");
            Assert.IsFalse(SceneFileCompare.Equivalent(text, text.Replace("m_Name: PathGuide", "m_Name: PathGuidX")), "a renamed object");
            // swap two internal references: the first two GameObjects' first component lines
            var comp = System.Text.RegularExpressions.Regex.Matches(text, @"- component: \{fileID: (\d+)\}");
            Assert.Greater(comp.Count, 2);
            string a = comp[0].Groups[1].Value, b = comp[comp.Count - 1].Groups[1].Value;
            string swapped = text.Replace("component: {fileID: " + a + "}", "component: {fileID: TMP}")
                                 .Replace("component: {fileID: " + b + "}", "component: {fileID: " + a + "}")
                                 .Replace("component: {fileID: TMP}", "component: {fileID: " + b + "}");
            Assert.IsFalse(SceneFileCompare.Equivalent(text, swapped), "two components moved between objects");
        }

        [Test]
        public void MaterialContractHolds()
        {
            var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(NocPaths.LibraryAsset);
            Assert.IsNotNull(lib, NocPaths.LibraryAsset);
            var problems = new List<string>();
            MaterialFactory.Check(lib, problems);
            Assert.IsEmpty(problems, string.Join("\n", problems));
            int n = 0;
            foreach (var m in lib.All()) { n++; Assert.IsTrue(m.shader != null && m.shader.isSupported, m.name); }
            Assert.AreEqual(MaterialFactory.LibraryFields().Count, n, "every library field set");
        }

        [Test]
        public void GlobalNamesNeverEqualMaterialProperties()
        {
            var props = new HashSet<string>();
            foreach (var n in NocPaths.ShaderNames)
            {
                var sh = MaterialFactory.FindShader(n);
                for (int i = 0; i < sh.GetPropertyCount(); i++) props.Add(sh.GetPropertyName(i));
            }
            foreach (var f in typeof(ShaderIds).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            {
                if (f.FieldType != typeof(int)) continue;
                int id = (int)f.GetValue(null);
                foreach (var p in props) Assert.AreNotEqual(Shader.PropertyToID(p), id, "global " + f.Name + " equals material property " + p);
            }
        }

        [Test]
        public void MainSceneSanity()
        {
            Assert.IsTrue(File.Exists(NocPaths.Abs(NocPaths.MainScene)), NocPaths.MainScene);
            CollectionAssert.AreEqual(new[] { NocPaths.MainScene }, Array.ConvertAll(EditorBuildSettings.scenes, s => s.path), "build scenes");
            var scene = EditorSceneManager.OpenScene(NocPaths.MainScene, OpenSceneMode.Additive);
            try
            {
                int missing = 0, renderers = 0;
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    {
                        missing += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                        foreach (var r in t.GetComponents<Renderer>())
                        {
                            renderers++;
                            foreach (var m in r.sharedMaterials) Assert.IsTrue(m != null && m.shader != null && m.shader.isSupported, "material on " + t.name);
                        }
                    }
                Assert.AreEqual(0, missing, "missing scripts");
                Assert.Greater(renderers, 0, "the sky dome renders");
                GameBootstrap gb = null; Camera cam = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (gb == null) gb = root.GetComponentInChildren<GameBootstrap>(true);
                    if (cam == null) cam = root.GetComponentInChildren<Camera>(true);
                }
                Assert.IsNotNull(gb); Assert.IsNotNull(gb.materials); Assert.IsNotNull(gb.mainCamera); Assert.IsNotNull(gb.cityRoot);
                Assert.IsNotNull(cam);
                Assert.AreEqual(0.05f, cam.nearClipPlane, 1e-6f); Assert.AreEqual(12000f, cam.farClipPlane, 1e-3f);
                Assert.AreEqual(CameraClearFlags.SolidColor, cam.clearFlags, "no skybox pass (D-008)");
                var acd = cam.GetComponent<UniversalAdditionalCameraData>();
                Assert.IsFalse(acd.renderPostProcessing); Assert.IsFalse(acd.dithering, "exactly one dither (NocGrade)"); Assert.IsFalse(acd.renderShadows);
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
