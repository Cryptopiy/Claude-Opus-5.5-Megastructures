// BatchTools: the -executeMethod entry points and the Tools/Megastructure menu (MASTER_PROMPT 7.1, Part 12). Imported from Thread.
//   RebuildAll       palette asset -> generated HLSL -> refresh -> materials + library -> URP pipeline -> project
//                    settings -> Main.unity -> build settings -> template cleanup -> save
//   BuildWindows     Builds/Bulwark/Bulwark.exe, or <BW_BUILD_DIR>/Bulwark.exe (per-owner build folders, lesson 3.2 #10)
//                    (StandaloneWindows64, Direct3D 11, release; -dev-build for a development build)
//   BuildMac         Builds/Mac/Bulwark.app (StandaloneOSX, optional; unsigned when built on Windows)
//   CompileCheck     a successful batch compile (exit 0) or 1 when scripts failed to compile
//   Validate         Main.unity contract, missing scripts / materials, shader errors (compiled per material) -> table
//   CaptureInEditor  opens Main.unity and enters play mode for runtime captures (no -quit; CaptureWatchdog exits)
// Every batch method logs "[BW] ..." lines, every failure "[BW-FAIL] ..." (D-06; BwLog.Problem / BwLog.Fail, plus one
// "[BW-FAIL] problem: ..." summary line per problem), and ends with EditorApplication.Exit(0/1) in batch mode.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Megastructure.EditorTools
{
    public static class BatchTools
    {
        public const string Menu = "Tools/Megastructure/";

        // ------------------------------------------------------------------------------------------------ RebuildAll
        [MenuItem(Menu + "Rebuild All", priority = 0)]
        public static void RebuildAll()
        {
            Run("RebuildAll", RebuildAllInternal);
        }

        /// <summary>The full regeneration; returns the problem list (empty = success).</summary>
        public static List<string> RebuildAllInternal()
        {
            var problems = new List<string>();
            BulwarkPalette palette = null;
            MaterialLibrary lib = null;
            Step("PaletteWriter", problems, () => { palette = PaletteWriter.Write(); });
            Step("ShaderGen", problems, () => { ShaderGen.WriteAll(); });
            Step("AssetDatabase.Refresh", problems, () => { AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport); });
            Step("MaterialFactory", problems, () => { lib = MaterialFactory.Build(problems); });
            Step("PipelineConfigurator", problems, () => { PipelineConfigurator.Apply(lib != null ? lib.Grade : null, problems); });
            Step("ProjectConfigurator", problems, () => { ProjectConfigurator.Apply(problems); });
            Step("SceneBuilder", problems, () => { SceneBuilder.Build(lib, palette, problems); });
            Step("BuildSettings", problems, ApplyBuildSettings);
            Step("TemplateCleanup", problems, () => { TemplateCleanup.Run(problems); });
            Step("SaveAssets", problems, () => { AssetDatabase.SaveAssets(); });
            return problems;
        }

        public static void ApplyBuildSettings()
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(BwPaths.MainScene, true) };
            BwLog.Info("BuildSettings: scenes = [" + BwPaths.MainScene + "]");
        }

        [MenuItem(Menu + "Generate HLSL (palette + constants)", priority = 20)]
        public static void GenerateHlsl() => Run("GenerateHlsl", () => { var p = new List<string>(); ShaderGen.WriteAll(); return p; });

        [MenuItem(Menu + "Rebuild Materials", priority = 21)]
        public static void RebuildMaterials() => Run("RebuildMaterials", () => { var p = new List<string>(); PaletteWriter.Write(); MaterialFactory.Build(p); return p; });

        [MenuItem(Menu + "Rebuild Scene", priority = 22)]
        public static void RebuildScene() => Run("RebuildScene", () =>
        {
            var p = new List<string>();
            var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(BwPaths.LibraryAsset);
            var pal = AssetDatabase.LoadAssetAtPath<BulwarkPalette>(BwPaths.PaletteAsset);
            SceneBuilder.Build(lib, pal, p);
            ApplyBuildSettings();
            return p;
        });

        // ------------------------------------------------------------------------------------------------ BuildWindows
        [MenuItem(Menu + "Build Windows Player", priority = 40)]
        public static void BuildWindows()
        {
            Run("BuildWindows", BuildWindowsInternal);
        }

        /// <summary>Player exe path: &lt;BW_BUILD_DIR&gt;/Bulwark.exe when the environment variable is set (relative paths are
        /// resolved against the project root), else Builds/Bulwark/Bulwark.exe. One build folder per owner (lesson 3.2 #10).</summary>
        public static string PlayerExePath()
        {
            string dir = Environment.GetEnvironmentVariable(BwPaths.BuildDirEnv);
            if (string.IsNullOrWhiteSpace(dir)) dir = Path.Combine(BwPaths.ProjectRoot, BwPaths.DefaultBuildDir.Replace('/', Path.DirectorySeparatorChar));
            else if (!Path.IsPathRooted(dir)) dir = Path.Combine(BwPaths.ProjectRoot, dir);
            return Path.Combine(dir.Trim().Trim('"'), BwPaths.ProductName + ".exe");
        }

        static List<string> BuildWindowsInternal()
        {
            var problems = new List<string>();
            if (!File.Exists(BwPaths.Abs(BwPaths.MainScene)))
            {
                BwLog.Problem(problems, "BuildWindows: " + BwPaths.MainScene + " missing (run RebuildAll first)");
                return problems;
            }
            if (CliArgs.Get(ProjectConfigurator.GraphicsApiArg) != null) ProjectConfigurator.ApplyGraphicsApi(problems);
            ApplyBuildSettings();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
            {
                BwLog.Info("BuildWindows: switching the active build target to StandaloneWindows64 (prefer -buildTarget win64)");
                EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Standalone, BuildTarget.StandaloneWindows64);
            }

            // Shader errors are logged, not fatal, in a player build (R8 pitfall 3): check the five shaders first.
            foreach (var name in BwPaths.ShaderNames)
            {
                var sh = MaterialFactory.FindShader(name);
                if (sh == null) BwLog.Problem(problems, "BuildWindows: shader " + name + " missing");
                else if (ShaderUtil.ShaderHasError(sh)) BwLog.Problem(problems, "BuildWindows: shader " + name + " has compile errors");
            }

            string exe = PlayerExePath();
            Directory.CreateDirectory(Path.GetDirectoryName(exe));
            bool dev = CliArgs.Has("-dev-build");
            var opts = new BuildPlayerOptions
            {
                scenes = new[] { BwPaths.MainScene },
                locationPathName = exe,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = dev ? BuildOptions.Development : BuildOptions.None,
            };
            BwLog.Info($"BuildWindows: building {(dev ? "DEVELOPMENT" : "release")} player -> {exe} (APIs {ProjectConfigurator.ApiList()})");
            var sw = Stopwatch.StartNew();
            BuildReport report = BuildPipeline.BuildPlayer(opts);
            BuildSummary s = report.summary;
            foreach (var step in report.steps)
                foreach (var m in step.messages)
                    if (m.type == LogType.Error || m.type == LogType.Exception)
                        BwLog.Info($"BuildWindows: [{step.name}] {m.type}: {m.content}");
            BwLog.Info($"BuildWindows: result={s.result} errors={s.totalErrors} warnings={s.totalWarnings} " +
                           $"size={s.totalSize / (1024.0 * 1024.0):F1} MB time={s.totalTime.TotalSeconds:F1} s (wall {sw.Elapsed.TotalSeconds:F1} s) out={s.outputPath}");
            if (s.result != BuildResult.Succeeded) BwLog.Problem(problems, "BuildWindows: build result " + s.result);
            else if (!File.Exists(exe)) BwLog.Problem(problems, "BuildWindows: build reported success but " + exe + " does not exist");
            return problems;
        }

        // ------------------------------------------------------------------------------------------------ BuildMac
        [MenuItem(Menu + "Build macOS Player", priority = 41)]
        public static void BuildMac()
        {
            Run("BuildMac", BuildMacInternal);
        }

        public static string PlayerAppPath() =>
            Path.Combine(BwPaths.ProjectRoot, "Builds", "Mac", BwPaths.ProductName + ".app");

        static List<string> BuildMacInternal()
        {
            var problems = new List<string>();
            if (!File.Exists(BwPaths.Abs(BwPaths.MainScene)))
            {
                BwLog.Problem(problems, "BuildMac: " + BwPaths.MainScene + " missing (run RebuildAll first)");
                return problems;
            }
            ApplyBuildSettings();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneOSX)
                EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Standalone, BuildTarget.StandaloneOSX);
            // one universal binary: Intel and Apple silicon Macs
            EditorUserBuildSettings.SetPlatformSettings("Standalone", "OSXUniversal", "Architecture", "x64ARM64");

            string app = PlayerAppPath();
            Directory.CreateDirectory(Path.GetDirectoryName(app));
            var opts = new BuildPlayerOptions
            {
                scenes = new[] { BwPaths.MainScene },
                locationPathName = app,
                target = BuildTarget.StandaloneOSX,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None,
            };
            BwLog.Info("BuildMac: building release player -> " + app);
            var sw = Stopwatch.StartNew();
            BuildReport report = BuildPipeline.BuildPlayer(opts);
            BuildSummary s = report.summary;
            foreach (var step in report.steps)
                foreach (var m in step.messages)
                    if (m.type == LogType.Error || m.type == LogType.Exception)
                        BwLog.Info($"BuildMac: [{step.name}] {m.type}: {m.content}");
            BwLog.Info($"BuildMac: result={s.result} errors={s.totalErrors} warnings={s.totalWarnings} " +
                           $"size={s.totalSize / (1024.0 * 1024.0):F1} MB time={s.totalTime.TotalSeconds:F1} s (wall {sw.Elapsed.TotalSeconds:F1} s) out={s.outputPath}");
            if (s.result != BuildResult.Succeeded) BwLog.Problem(problems, "BuildMac: build result " + s.result);

            // leave the project on the Windows target the QA tools expect
            EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Standalone, BuildTarget.StandaloneWindows64);
            return problems;
        }

        // ------------------------------------------------------------------------------------------------ CompileCheck
        public static void CompileCheck()
        {
            Run("CompileCheck", () =>
            {
                var p = new List<string>();
                if (EditorUtility.scriptCompilationFailed) BwLog.Problem(p, "CompileCheck: script compilation failed");
                BwLog.Info($"CompileCheck: Unity {Application.unityVersion}, device {SystemInfo.graphicsDeviceType}, " +
                               $"reversedZ {SystemInfo.usesReversedZBuffer}, APIs {ProjectConfigurator.ApiList()}");
                return p;
            });
        }

        // ------------------------------------------------------------------------------------------------ Validate
        [MenuItem(Menu + "Validate", priority = 41)]
        public static void Validate()
        {
            Run("Validate", ValidateInternal);
        }

        sealed class Row { public string item, status, detail; }

        public static List<string> ValidateInternal()
        {
            var problems = new List<string>();
            var rows = new List<Row>();
            void Add(string item, bool ok, string detail)
            {
                rows.Add(new Row { item = item, status = ok ? "OK" : "FAIL", detail = detail });
                if (!ok) problems.Add(item + ": " + detail);
            }

            // ---- scene
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                problems.Add("Validate cancelled (unsaved scene)");
                return problems;
            }
            bool sceneExists = File.Exists(BwPaths.Abs(BwPaths.MainScene));
            Add("scene file", sceneExists, BwPaths.MainScene);
            if (sceneExists)
            {
                var scene = EditorSceneManager.OpenScene(BwPaths.MainScene, OpenSceneMode.Single);
                int missing = 0, gos = 0;
                var missingOn = new List<string>();
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    {
                        gos++;
                        int n = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                        if (n > 0) { missing += n; missingOn.Add(t.name); }
                        foreach (var r in t.GetComponents<Renderer>())
                            foreach (var m in r.sharedMaterials)
                                if (m == null || m.shader == null || m.shader.name == MaterialFactory.ErrorShaderName)
                                    Add("material on " + t.name, false, "missing or error-shader material on renderer");
                    }
                Add("missing scripts", missing == 0, missing == 0 ? $"{gos} objects scanned" : $"{missing} on {string.Join(", ", missingOn)}");
                CheckSceneContract(scene, Add);
                Add("no skybox", RenderSettings.skybox == null, RenderSettings.skybox != null ? RenderSettings.skybox.name : "none (the sky is BwFarField geometry)");
                Add("unity fog off", !RenderSettings.fog, "RenderSettings.fog = " + RenderSettings.fog);
            }

            // ---- build settings
            var enabled = new List<string>();
            foreach (var s in EditorBuildSettings.scenes) if (s.enabled) enabled.Add(s.path);
            Add("build scenes", enabled.Count == 1 && enabled[0] == BwPaths.MainScene, string.Join(", ", enabled));

            // ---- material contract
            var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(BwPaths.LibraryAsset);
            var matProblems = new List<string>();
            MaterialFactory.Check(lib, matProblems);
            Add("material contract", matProblems.Count == 0, matProblems.Count == 0 ? $"{MaterialFactory.Specs().Count} materials" : matProblems.Count + " issue(s)");
            foreach (var mp in matProblems) Add("  material", false, mp);

            // ---- shaders: compile every pass of every library material (its keyword set), then read the messages
            bool async = ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation = false;
            try
            {
                if (lib != null)
                    foreach (var f in MaterialFactory.LibraryFields())
                    {
                        var m = f.GetValue(lib) as Material;
                        if (m == null || m.shader == null) continue;
                        for (int p = 0; p < m.passCount; p++) ShaderUtil.CompilePass(m, p, true);
                    }
            }
            catch (Exception e) { BwLog.Warn("Validate: CompilePass threw " + e.Message); }
            finally { ShaderUtil.allowAsyncCompilation = async; }

            foreach (var name in BwPaths.ShaderNames)
            {
                var sh = MaterialFactory.FindShader(name);
                if (sh == null) { Add("shader " + name, false, "not found"); continue; }
                int errors = 0, warnings = 0;
                foreach (var msg in ShaderUtil.GetShaderMessages(sh))
                {
                    bool err = msg.severity == ShaderCompilerMessageSeverity.Error;
                    if (err) errors++; else warnings++;
                    string line = $"Validate: {name} {(err ? "error" : "warning")} {msg.platform} {msg.file}:{msg.line} {msg.message} {msg.messageDetails}";
                    if (err) BwLog.Fail(line); else BwLog.Info(line);
                }
                bool has = ShaderUtil.ShaderHasError(sh);
                // the error state can be set by the import compile while this session holds no messages for it (seen 2026-11-19:
                // an undeclared identifier in an include, reported by the EditMode test of the same session but not here)
                string why = has && errors == 0 ? " (error state without compiler messages in this session: see 'Shader error in' in the Unity log or ProjectTests.Shaders_ExistAndCompileWithoutErrors)" : "";
                Add("shader " + name, !has && errors == 0, $"{sh.passCount} passes, {errors} errors, {warnings} warnings{why}");
            }

            // ---- pipeline
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(BwPaths.RpAsset);
            Add("default pipeline", GraphicsSettings.defaultRenderPipeline == asset && asset != null, EditorUtil.AssetPathOf(GraphicsSettings.defaultRenderPipeline));
            Add("quality levels", QualitySettings.count == 1 && QualitySettings.GetRenderPipelineAssetAt(0) == asset, string.Join(", ", QualitySettings.names));
            if (asset != null)
            {
                Add("hdr 64-bit / msaa", asset.supportsHDR && asset.hdrColorBufferPrecision == HDRColorBufferPrecision._64Bits && asset.msaaSampleCount == FrameworkConst.Msaa,
                    $"HDR {asset.supportsHDR}, {asset.hdrColorBufferPrecision}, MSAA {asset.msaaSampleCount}");
                Add("shadows 4 x 4096 / 150 m", asset.shadowCascadeCount == PipelineConfigurator.ShadowCascades && asset.mainLightShadowmapResolution == PipelineConfigurator.ShadowResolution
                    && Mathf.Approximately(asset.shadowDistance, PipelineConfigurator.ShadowDistance),
                    $"{asset.shadowCascadeCount} cascades, {asset.mainLightShadowmapResolution}, {asset.shadowDistance} m");
                Add("GPU Resident Drawer off", asset.gpuResidentDrawerMode == GPUResidentDrawerMode.Disabled, asset.gpuResidentDrawerMode.ToString());
            }
            var rd = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(BwPaths.RendererAsset);
            if (rd != null)
            {
                Add("forward+", rd.renderingMode == RenderingMode.ForwardPlus, rd.renderingMode.ToString());
                Add("depth D32_SFloat_S8_UInt", rd.depthAttachmentFormat == DepthFormat.Depth_32_Stencil_8 && rd.depthTextureFormat == DepthFormat.Depth_32_Stencil_8,
                    $"attachment {rd.depthAttachmentFormat}, texture {rd.depthTextureFormat}");
                bool ssao = false;
                foreach (var f in rd.rendererFeatures) if (f != null && f.GetType().Name.IndexOf("ScreenSpaceAmbientOcclusion", StringComparison.Ordinal) >= 0) ssao = true;
                Add("no SSAO feature", !ssao, ssao ? "SSAO present" : "none");
                ScriptableRendererFeature grade = null;
                foreach (var f in rd.rendererFeatures) if (f != null && f.GetType().FullName == PipelineConfigurator.GradeFeatureType) grade = f;
                Material gm = null;
                if (grade != null)
                {
                    var so = new SerializedObject(grade);
                    var p = so.FindProperty(PipelineConfigurator.GradeFeatureMaterialField);
                    gm = p != null ? p.objectReferenceValue as Material : null;
                }
                Add("grade feature", grade != null && grade.isActive && gm != null, grade == null ? "missing" : "material " + (gm != null ? gm.name : "null"));
            }
            Add("input handler", ProjectConfigurator.ActiveInputHandler() == ProjectConfigurator.InputSystemOnly, "activeInputHandler = " + ProjectConfigurator.ActiveInputHandler());
            {
                var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
                bool d3d11 = !PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64) && apis.Length == 1 && apis[0] == GraphicsDeviceType.Direct3D11;
                Add("graphics API exactly [Direct3D11]", d3d11, ProjectConfigurator.ApiList() + " (editor device " + SystemInfo.graphicsDeviceType + ")");
            }
            Add("product / company", PlayerSettings.productName == BwPaths.ProductName && PlayerSettings.companyName == BwPaths.CompanyName,
                PlayerSettings.companyName + "/" + PlayerSettings.productName);
            Add("splash off", !PlayerSettings.SplashScreen.show, "SplashScreen.show = " + PlayerSettings.SplashScreen.show);
            {
                var gs = EditorUtil.LoadProjectSettingsObject("ProjectSettings/GraphicsSettings.asset");
                var arr = gs != null ? new SerializedObject(gs).FindProperty("m_AlwaysIncludedShaders") : null;
                var missingAlways = new List<string>();
                foreach (var name in BwPaths.ShaderNames)
                {
                    var sh = MaterialFactory.FindShader(name);
                    bool present = false;
                    for (int i = 0; arr != null && sh != null && i < arr.arraySize && !present; i++) present = arr.GetArrayElementAtIndex(i).objectReferenceValue == sh;
                    if (!present) missingAlways.Add(name);
                }
                Add("Always Included Bw shaders", missingAlways.Count == 0, missingAlways.Count == 0 ? BwPaths.ShaderNames.Length + " shaders" : "missing " + string.Join(", ", missingAlways));
            }
            {
                // BwGrade owns the look (8.2): no Volume in the scene, and the URP asset's profile is at most the global
                // default profile. That default profile is the URP 17 defaults carrier: URPPreprocessBuild forces every
                // parameter to overrideState = true at each build (VolumeProfileUtils.TryEnsureAllOverridesForDefaultProfile),
                // so its flags are not overrides of ours; the camera renders no URP post at all (row "camera URP data").
                var vp = AssetDatabase.LoadAssetAtPath<VolumeProfile>(BwPaths.DefaultVolumeProfile);
                int sceneVolumes = 0;
                var active = SceneManager.GetActiveScene();
                if (active.IsValid())
                    foreach (var root in active.GetRootGameObjects()) sceneVolumes += root.GetComponentsInChildren<Volume>(true).Length;
                var rpa = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(BwPaths.RpAsset);
                bool assetProfileOk = rpa == null || rpa.volumeProfile == null || rpa.volumeProfile == vp;
                Add("no Volume colour overrides", sceneVolumes == 0 && assetProfileOk,
                    $"{sceneVolumes} Volume components in the scene; URP asset profile {(rpa != null && rpa.volumeProfile != null ? rpa.volumeProfile.name : "none")} " +
                    "(the global default profile only carries the URP defaults; camera post off)");
            }
            {
                var panel = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.PanelSettings>(BwPaths.PanelSettingsAsset);
                Add("PanelSettings asset", panel != null && panel.themeStyleSheet != null, panel == null ? "missing" : "theme " + (panel.themeStyleSheet != null ? "ok" : "MISSING"));
            }
            {
                bool mobile = File.Exists(BwPaths.Abs("Assets/Settings/Mobile_RPAsset.asset")) || File.Exists(BwPaths.Abs("Assets/Settings/Mobile_Renderer.asset"));
                Add("template mobile assets removed", !mobile, mobile ? "Mobile_RPAsset / Mobile_Renderer present" : "gone");
            }

            // ---- one writer of the surface state (14.2.2: AtmosphereDriver writes every global; _BwSurface = rain wetness,
            //      interior factor, lamp gain of the material library, Wave B dusk owner)
            {
                var writers = GlobalWriters("_BwSurface");
                bool one = writers.Count == 1 && writers[0].EndsWith("/Runtime/Systems/AtmosphereDriver.cs", StringComparison.Ordinal);
                Add("one writer of _BwSurface", one, writers.Count == 0 ? "nobody writes _BwSurface (AtmosphereDriver must)" : string.Join(", ", writers));
            }

            // ---- table
            BwLog.Info("VALIDATE | status | item | detail");
            foreach (var r in rows) BwLog.Info($"VALIDATE | {r.status,-4} | {r.item} | {r.detail}");
            BwLog.Info($"VALIDATE | {(problems.Count == 0 ? "PASS" : "FAIL")} | {rows.Count} checks, {problems.Count} problem(s)");
            return problems;
        }

        /// <summary>C# files under Assets/_Game that write the shader global <paramref name="name"/>: a Shader.SetGlobal* call
        /// with the literal name, or with an identifier initialised by Shader.PropertyToID("name") in the same file
        /// (comments ignored). Paths relative to the project, '/' separators, sorted.</summary>
        public static List<string> GlobalWriters(string name)
        {
            var o = new List<string>();
            string root = BwPaths.Abs("Assets/_Game");
            string lit = "\"" + name + "\"";
            var idRx = new System.Text.RegularExpressions.Regex(@"(\w+)\s*=\s*Shader\.PropertyToID\(\s*" + System.Text.RegularExpressions.Regex.Escape(lit) + @"\s*\)");
            foreach (string f in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                var lines = File.ReadAllLines(f);
                var sb = new System.Text.StringBuilder();
                foreach (var l in lines) { int c = l.IndexOf("//", StringComparison.Ordinal); sb.Append(c >= 0 ? l.Substring(0, c) : l).Append('\n'); }
                string code = sb.ToString();
                if (code.IndexOf(lit, StringComparison.Ordinal) < 0) continue;
                bool writes = System.Text.RegularExpressions.Regex.IsMatch(code, @"SetGlobal\w*\(\s*" + System.Text.RegularExpressions.Regex.Escape(lit));
                foreach (System.Text.RegularExpressions.Match m in idRx.Matches(code))
                    if (System.Text.RegularExpressions.Regex.IsMatch(code, @"SetGlobal\w*\(\s*" + m.Groups[1].Value + @"\b")) writes = true;
                if (writes) o.Add(f.Substring(BwPaths.Abs("").Length).Replace('\\', '/').TrimStart('/'));
            }
            o.Sort(StringComparer.Ordinal);
            return o;
        }

        /// <summary>Every object and component of the scene contract exists; GameBootstrap references are set.</summary>
        static void CheckSceneContract(Scene scene, Action<string, bool, string> add)
        {
            GameObject Find(string path)
            {
                string[] parts = path.Split('/');
                foreach (var r in scene.GetRootGameObjects())
                {
                    if (r.name != parts[0]) continue;
                    Transform t = r.transform;
                    for (int i = 1; i < parts.Length && t != null; i++) t = t.Find(parts[i]);
                    return t != null ? t.gameObject : null;
                }
                return null;
            }
            void Need(string path, string[] types)
            {
                var go = Find(path);
                if (go == null) { add("object " + path, false, "missing"); return; }
                foreach (var tn in types)
                {
                    var t = EditorUtil.FindType(tn);
                    bool ok = t != null && go.GetComponent(t) != null;
                    add(path + " : " + tn, ok, t == null ? "type not found" : (ok ? "present" : "component missing"));
                }
            }
            Need("Game", SceneBuilder.GameComponents);
            Need("Player", SceneBuilder.PlayerComponents);
            Need("Player/CameraRig", SceneBuilder.CameraRigComponents);
            Need("Sun", new string[0]);
            Need("World", new string[0]);
            Need("PathGuide", SceneBuilder.PathGuideComponents);

            var rig = Find("Player/CameraRig");
            var cam = rig != null ? rig.GetComponent<Camera>() : null;
            var acd = cam != null ? cam.GetComponent<UniversalAdditionalCameraData>() : null;
            add("camera", cam != null && rig.CompareTag("MainCamera") && cam.allowHDR && cam.allowMSAA && cam.clearFlags == CameraClearFlags.SolidColor &&
                          Mathf.Approximately(cam.nearClipPlane, (float)FrameworkConst.CameraNear) && Mathf.Approximately(cam.farClipPlane, (float)BulwarkSchedule.Far(0)),
                cam != null ? $"near {cam.nearClipPlane} far {cam.farClipPlane} HDR {cam.allowHDR} clear {cam.clearFlags}" : "missing");
            add("camera URP data", acd != null && !acd.renderPostProcessing && acd.antialiasing == AntialiasingMode.None && !acd.dithering && acd.renderShadows,
                acd != null ? $"post {acd.renderPostProcessing} aa {acd.antialiasing} dither {acd.dithering} shadows {acd.renderShadows}" : "missing");
            add("audio listener", rig != null && rig.GetComponent<AudioListener>() != null, "on CameraRig");
            var sun = Find("Sun");
            var light = sun != null ? sun.GetComponent<Light>() : null;
            add("sun light", light != null && light.type == LightType.Directional && light.shadows == LightShadows.Soft, light != null ? $"{light.type} {light.shadows}" : "missing");

            var game = Find("Game");
            var gb = game != null ? game.GetComponent<GameBootstrap>() : null;
            if (gb != null)
                add("GameBootstrap refs", gb.materials != null && gb.palette != null && gb.worldRoot != null && gb.sun != null && gb.mainCamera != null,
                    $"materials {gb.materials != null} palette {gb.palette != null} world {gb.worldRoot != null} sun {gb.sun != null} camera {gb.mainCamera != null}");
            var doc = game != null ? game.GetComponent<UnityEngine.UIElements.UIDocument>() : null;
            add("UIDocument + PanelSettings", doc != null && doc.panelSettings != null, doc == null ? "UIDocument missing" : "panel " + (doc.panelSettings != null ? doc.panelSettings.name : "null"));
        }

        // ------------------------------------------------------------------------------------------------ CaptureInEditor
        [MenuItem(Menu + "Capture In Editor (play mode)", priority = 60)]
        public static void CaptureInEditor()
        {
            BwLog.QuietStackTraces();
            try
            {
                if (Application.isBatchMode && SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                {
                    BwLog.Fail("CaptureInEditor: null graphics device (do not pass -nographics)");
                    EditorApplication.Exit(4);
                    return;
                }
                if (!File.Exists(BwPaths.Abs(BwPaths.MainScene)))
                {
                    BwLog.Fail("CaptureInEditor: " + BwPaths.MainScene + " missing (run RebuildAll)");
                    if (Application.isBatchMode) EditorApplication.Exit(1);
                    return;
                }
                if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(BwPaths.MainScene, OpenSceneMode.Single);
                ShaderUtil.allowAsyncCompilation = false;
                if (Application.isBatchMode) CaptureWatchdog.Arm(CliArgs.GetDouble(CaptureWatchdog.TimeoutArg, CaptureWatchdog.DefaultTimeoutSeconds));
                BwLog.Info($"CaptureInEditor: entering play mode (device {SystemInfo.graphicsDeviceType}, args: {string.Join(" ", CliArgs.Args)})");
                EditorApplication.isPlaying = true;   // same as UTF EnterPlayModeTask; the runtime CaptureHarness takes over
            }
            catch (Exception e)
            {
                BwLog.Fail("CaptureInEditor threw: " + e);
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        // ------------------------------------------------------------------------------------------------ plumbing
        static void Step(string name, List<string> problems, Action a)
        {
            var sw = Stopwatch.StartNew();
            int before = problems.Count;
            try { a(); }
            catch (Exception e) { BwLog.Problem(problems, $"{name} threw: {e}"); }
            BwLog.Info($"step {name}: {(problems.Count == before ? "ok" : (problems.Count - before) + " problem(s)")} ({sw.Elapsed.TotalMilliseconds:F0} ms)");
        }

        /// <summary>Runs a batch method body; in batch mode exits 0 (no problems) or 1.</summary>
        static void Run(string name, Func<List<string>> body)
        {
            BwLog.QuietStackTraces();
            BwLog.Info($"{name}: start (Unity {Application.unityVersion}, batch {Application.isBatchMode})");
            var sw = Stopwatch.StartNew();
            List<string> problems;
            try { problems = body() ?? new List<string>(); }
            catch (Exception e)
            {
                problems = new List<string> { name + " threw: " + e };
                BwLog.Fail(name + " threw: " + e);
            }
            // every problem once more as a tagged summary line (D-06), so rows that were only collected (Validate) are counted too
            foreach (var p in problems) Debug.Log(BwLog.FailPrefix + "problem: " + p);
            int code = problems.Count == 0 ? 0 : 1;
            BwLog.Info($"{name}: {(code == 0 ? "SUCCESS" : "FAILED")} with {problems.Count} problem(s) in {sw.Elapsed.TotalSeconds:F1} s -> exit {code}");
            if (Application.isBatchMode) EditorApplication.Exit(code);
        }
    }
}
