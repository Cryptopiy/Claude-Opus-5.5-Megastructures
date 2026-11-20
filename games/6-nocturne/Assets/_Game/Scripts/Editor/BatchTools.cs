// BatchTools: the -executeMethod entry points and the Tools/Megastructure menu (MASTER_PROMPT 7.1, Part 12), imported from Thread.
//   RebuildAll       palette asset -> generated HLSL -> refresh -> materials + library -> UI assets -> URP pipeline -> project
//                    settings -> Main.unity -> build settings -> template cleanup -> save  (idempotent: a second run changes nothing)
//   BuildWindows     Builds/Nocturne/Nocturne.exe (StandaloneWindows64, release, Mono, D3D11); NOC_BUILD_DIR=<rel dir> builds into
//                    another folder (one per owner, lesson 10); -dev-build for a development build
//   CompileCheck     exit 0 after a successful batch compile, 1 when scripts failed to compile
//   Validate         scene contract, missing scripts / materials, every material against its contract, every Megastructure/* shader
//                    under Assets/_Game/Shaders (discovered, Shaders/Beats included) compiled per pass and Always Included
//                    (ShaderUtil, lesson 4), pipeline (rain + grade features, postProcessData null) and project settings of D-008
//                    -> table, exit 1 on any problem. Look v2 gates (TEXTURE_PLAN 8.3 TP-0): forward variants against the
//                    committed baseline; D3D11 warnings X3570 / X4000 are failures (R14); the processed External textures import
//                    with their manifest's settings (NocTextureImport.ExpectedFor); the debug-view feature. Warnings only: SRP
//                    Batcher compatibility (internal API), External textures no library material references (a gate after TP-2).
//   CaptureInEditor  opens Main.unity and enters play mode for runtime captures (no -quit; CaptureWatchdog exits)
// Every batch method logs "[NOC] ..." lines, problems as "[NOC-FAIL] ...", and ends with EditorApplication.Exit(0/1) in batch mode.
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
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Megastructure.EditorTools
{
    public static class BatchTools
    {
        public const string Menu = "Tools/Megastructure/";

        // ------------------------------------------------------------------------------------------------ RebuildAll
        [MenuItem(Menu + "Rebuild All", priority = 0)]
        public static void RebuildAll() => Run("RebuildAll", RebuildAllInternal);

        public static List<string> RebuildAllInternal()
        {
            var problems = new List<string>();
            MaterialLibrary lib = null;
            PanelSettings panel = null;
            Step("PaletteWriter", problems, () => { PaletteWriter.Write(); });
            Step("ShaderGen", problems, () => { ShaderGen.WriteAll(); });
            Step("AssetDatabase.Refresh", problems, () => { AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport); });
            Step("MaterialFactory", problems, () => { lib = MaterialFactory.Build(problems); });
            Step("NocUiSetup", problems, () => { panel = NocUiSetup.CreateOrUpdate(problems); });
            Step("PipelineConfigurator", problems, () => { PipelineConfigurator.Apply(lib != null ? lib.Grade : null, problems); });
            Step("ProjectConfigurator", problems, () => { ProjectConfigurator.Apply(problems); });
            Step("SceneBuilder", problems, () => { SceneBuilder.Build(lib, panel, problems); });
            Step("BuildSettings", problems, ApplyBuildSettings);
            Step("TemplateCleanup", problems, () => { TemplateCleanup.Run(problems); });
            Step("SaveAssets", problems, () => { AssetDatabase.SaveAssets(); });
            ProjectConfigurator.EnsureUnityConnectOff(problems);   // the Editor may have re-enabled it meanwhile
            return problems;
        }

        public static void ApplyBuildSettings()
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(NocPaths.MainScene, true) };
            NocLog.Info("BuildSettings: scenes = [" + NocPaths.MainScene + "]");
        }

        [MenuItem(Menu + "Generate HLSL (palette + constants)", priority = 20)]
        public static void GenerateHlsl() => Run("GenerateHlsl", () => { ShaderGen.WriteAll(); return new List<string>(); });

        // ------------------------------------------------------------------------------------------------ BuildWindows
        [MenuItem(Menu + "Build Windows Player", priority = 40)]
        public static void BuildWindows() => Run("BuildWindows", BuildWindowsInternal);

        /// <summary>Builds/Nocturne/Nocturne.exe, or &lt;NOC_BUILD_DIR&gt;/Nocturne.exe (relative to the project root).</summary>
        public static string PlayerExePath()
        {
            string dir = Environment.GetEnvironmentVariable(NocPaths.BuildDirEnv);
            if (string.IsNullOrWhiteSpace(dir)) dir = Path.Combine("Builds", NocPaths.ProductName);
            if (!Path.IsPathRooted(dir)) dir = Path.Combine(NocPaths.ProjectRoot, dir);
            return Path.Combine(dir, NocPaths.ProductName + ".exe");
        }

        static List<string> BuildWindowsInternal()
        {
            var problems = new List<string>();
            if (!File.Exists(NocPaths.Abs(NocPaths.MainScene)))
            {
                NocLog.Problem(problems, "BuildWindows: " + NocPaths.MainScene + " missing (run RebuildAll first)");
                return problems;
            }
            ProjectConfigurator.ApplyGraphicsApi();
            ProjectConfigurator.EnsureUnityConnectOff(problems);   // the Editor re-enables it at start-up (seen 2026-11-14)
            ProjectConfigurator.EnsureAlwaysIncludedShaders(problems);   // every Megastructure/* shader, Shaders/Beats included
            ApplyBuildSettings();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
            {
                NocLog.Info("BuildWindows: switching the active build target to StandaloneWindows64");
                EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Standalone, BuildTarget.StandaloneWindows64);
            }
            // Shader errors are logged, not fatal, in a player build: check every shader first (lesson 4).
            var shaderNames = NocPaths.ShaderNames;
            foreach (var name in shaderNames)
            {
                var sh = ShaderCatalog.Load(name);
                if (sh == null) NocLog.Problem(problems, "BuildWindows: shader " + name + " missing");
                else if (ShaderUtil.ShaderHasError(sh)) NocLog.Problem(problems, "BuildWindows: shader " + name + " (" + ShaderCatalog.PathOf(name) + ") has compile errors");
            }
            NocLog.Info(FormattableString.Invariant($"BuildWindows: {shaderNames.Length} Megastructure/* shaders checked: {string.Join(", ", shaderNames)}"));
            if (problems.Count > 0) return problems;

            string exe = PlayerExePath();
            Directory.CreateDirectory(Path.GetDirectoryName(exe));
            bool dev = CliArgs.Has("-dev-build");
            var opts = new BuildPlayerOptions
            {
                scenes = new[] { NocPaths.MainScene },
                locationPathName = exe,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = dev ? BuildOptions.Development : BuildOptions.None,
            };
            NocLog.Info(FormattableString.Invariant($"BuildWindows: building {(dev ? "DEVELOPMENT" : "release")} player -> {exe} (APIs {ProjectConfigurator.ApiList()}, backend {PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone)})"));
            var sw = Stopwatch.StartNew();
            BuildReport report = BuildPipeline.BuildPlayer(opts);
            NocLog.QuietStackTraces();   // the build restores the default stack-trace logging
            BuildSummary s = report.summary;
            foreach (var step in report.steps)
                foreach (var m in step.messages)
                    if (m.type == LogType.Error || m.type == LogType.Exception)
                        NocLog.Info(FormattableString.Invariant($"BuildWindows: [{step.name}] {m.type}: {m.content}"));
            NocLog.Info(FormattableString.Invariant($"BuildWindows: result={s.result} errors={s.totalErrors} warnings={s.totalWarnings} ") +
                        FormattableString.Invariant($"size={s.totalSize / (1024.0 * 1024.0):F1} MB time={s.totalTime.TotalSeconds:F1} s (wall {sw.Elapsed.TotalSeconds:F1} s) out={s.outputPath}"));
            if (s.result != BuildResult.Succeeded) NocLog.Problem(problems, "BuildWindows: build result " + s.result);
            else if (!File.Exists(exe)) NocLog.Problem(problems, "BuildWindows: build reported success but " + exe + " does not exist");
            ProjectConfigurator.EnsureUnityConnectOff(problems);   // seen re-enabled by the Editor during a build (I0-build01)
            return problems;
        }

        // ------------------------------------------------------------------------------------------------ CompileCheck
        public static void CompileCheck()
        {
            Run("CompileCheck", () =>
            {
                var p = new List<string>();
                if (EditorUtility.scriptCompilationFailed) NocLog.Problem(p, "CompileCheck: script compilation failed");
                NocLog.Info(FormattableString.Invariant($"CompileCheck: Unity {Application.unityVersion}, device {SystemInfo.graphicsDeviceType}, ") +
                            FormattableString.Invariant($"reversedZ {SystemInfo.usesReversedZBuffer}, APIs {ProjectConfigurator.ApiList()}"));
                return p;
            });
        }

        // ------------------------------------------------------------------------------------------------ Validate
        [MenuItem(Menu + "Validate", priority = 41)]
        public static void Validate() => Run("Validate", ValidateInternal);

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
            bool sceneExists = File.Exists(NocPaths.Abs(NocPaths.MainScene));
            Add("scene file", sceneExists, NocPaths.MainScene);
            if (sceneExists)
            {
                var scene = EditorSceneManager.OpenScene(NocPaths.MainScene, OpenSceneMode.Single);
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
                                if (m == null || m.shader == null || m.shader.name == MaterialFactory.ErrorShaderName || !m.shader.isSupported)
                                    Add("material on " + t.name, false, "missing, unsupported or error-shader material on renderer");
                    }
                Add("missing scripts", missing == 0, missing == 0 ? $"{gos} objects scanned" : $"{missing} on {string.Join(", ", missingOn)}");
                CheckSceneContract(scene, Add);
                Add("no skybox (D-008)", RenderSettings.skybox == null, RenderSettings.skybox != null ? RenderSettings.skybox.name : "none");
                Add("unity fog off", !RenderSettings.fog, "RenderSettings.fog = " + RenderSettings.fog);
            }

            // ---- build settings
            var enabled = new List<string>();
            foreach (var s in EditorBuildSettings.scenes) if (s.enabled) enabled.Add(s.path);
            Add("build scenes", enabled.Count == 1 && enabled[0] == NocPaths.MainScene, string.Join(", ", enabled));

            // ---- material contract (every material, every property)
            var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(NocPaths.LibraryAsset);
            var matProblems = new List<string>();
            MaterialFactory.Check(lib, matProblems);
            Add("material contract", matProblems.Count == 0, matProblems.Count == 0 ? $"{MaterialFactory.Specs().Count} materials" : matProblems.Count + " issue(s)");
            foreach (var mp in matProblems) Add("  material", false, mp);

            // ---- shaders: every Megastructure/* shader under Assets/_Game/Shaders (ShaderCatalog: discovered from the files,
            //      Shaders/Beats included). Compile every pass of every library material (its keyword set) and every pass of a
            //      temporary material per discovered shader (default keywords; beat shaders have no library material), then read
            //      the messages.
            var catalog = ShaderCatalog.All();
            var shaderNames = NocPaths.ShaderNames;
            var unnamed = new List<string>();
            var seen = new Dictionary<string, string>(StringComparer.Ordinal);
            var dupes = new List<string>();
            foreach (var e in catalog)
            {
                if (e.name == null || !e.name.StartsWith(NocPaths.ShaderNamePrefix, StringComparison.Ordinal)) unnamed.Add(e.assetPath + " (" + (e.name ?? "no Shader declaration") + ")");
                else if (seen.TryGetValue(e.name, out var first)) dupes.Add(e.name + ": " + first + ", " + e.assetPath);
                else seen[e.name] = e.assetPath;
            }
            Add("shader catalog", catalog.Count > 0 && unnamed.Count == 0 && dupes.Count == 0,
                FormattableString.Invariant($"{catalog.Count} files, {shaderNames.Length} Megastructure/* names") +
                (unnamed.Count > 0 ? "; not Megastructure/*: " + string.Join(", ", unnamed) : "") + (dupes.Count > 0 ? "; duplicate names: " + string.Join("; ", dupes) : ""));
            bool async = ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation = false;
            var temps = new List<Material>();
            try
            {
                if (lib != null)
                    foreach (var f in MaterialFactory.LibraryFields())
                    {
                        var m = f.GetValue(lib) as Material;
                        if (m == null || m.shader == null) continue;
                        for (int p = 0; p < m.passCount; p++) ShaderUtil.CompilePass(m, p, true);
                    }
                foreach (var name in shaderNames)
                {
                    var sh = ShaderCatalog.Load(name);
                    if (sh == null || ShaderUtil.ShaderHasError(sh)) continue;
                    var m = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
                    temps.Add(m);
                    for (int p = 0; p < m.passCount; p++) ShaderUtil.CompilePass(m, p, true);
                }
            }
            catch (Exception e) { NocLog.Warn("Validate: CompilePass threw " + e.Message); }
            finally
            {
                ShaderUtil.allowAsyncCompilation = async;
                foreach (var m in temps) Object.DestroyImmediate(m);
            }

            var gateWarnings = new List<string>();      // R14: X3570 (gradient in a varying branch) and X4000 (uninitialised) fail
            foreach (var name in shaderNames)
            {
                var sh = ShaderCatalog.Load(name);
                if (sh == null) { Add("shader " + name, false, "not imported (" + ShaderCatalog.PathOf(name) + ")"); continue; }
                int errors = 0, warnings = 0;
                foreach (var msg in ShaderUtil.GetShaderMessages(sh))
                {
                    bool err = msg.severity == ShaderCompilerMessageSeverity.Error;
                    if (err) errors++; else warnings++;
                    string mtext = (msg.message ?? "") + " " + (msg.messageDetails ?? "");
                    // Unity strips the fxc codes from the message: match the texts too (INT: two X4000 passed the gate as
                    // "use of potentially uninitialized variable (...)")
                    if (!err && (mtext.Contains("X3570") || mtext.Contains("X4000") || mtext.Contains("potentially uninitialized")
                                 || mtext.Contains("gradient instruction used in a loop") || mtext.Contains("gradient-based operations")))
                        gateWarnings.Add(FormattableString.Invariant($"{name} {msg.file}:{msg.line} {msg.message}"));
                    NocLog.Info(FormattableString.Invariant($"Validate: {name} {(err ? "ERROR" : "warning")} {msg.platform} {msg.file}:{msg.line} {msg.message} {msg.messageDetails}"));
                }
                bool has = ShaderUtil.ShaderHasError(sh);
                Add("shader " + name, !has && errors == 0 && sh.isSupported, FormattableString.Invariant($"{ShaderCatalog.PathOf(name)}: {sh.passCount} passes, {errors} errors, {warnings} warnings, supported {sh.isSupported}"));
                Add("always included " + name, ProjectConfigurator.IsAlwaysIncluded(sh), "GraphicsSettings.m_AlwaysIncludedShaders");
            }
            int uiIncluded = 0;
            foreach (var n in NocUiSetup.UiRuntimeShaders) { var sh = Shader.Find(n); if (sh != null && ProjectConfigurator.IsAlwaysIncluded(sh)) uiIncluded++; }
            Add("UI runtime shaders always included", uiIncluded >= NocUiSetup.UiRuntimeShaders.Length - 2, $"{uiIncluded} of {NocUiSetup.UiRuntimeShaders.Length}");
            CheckGlobalNames(Add);
            CheckVariants(Add);
            Add("shader warnings X3570 / X4000 (R14)", gateWarnings.Count == 0, gateWarnings.Count == 0 ? "none" : string.Join("; ", gateWarnings));
            CheckSrpBatcher(shaderNames);
            CheckExternalTextures(Add, lib);

            // ---- pipeline (D-008)
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(NocPaths.RpAsset);
            Add("default pipeline", GraphicsSettings.defaultRenderPipeline == asset && asset != null, EditorUtil.AssetPathOf(GraphicsSettings.defaultRenderPipeline));
            Add("quality levels", QualitySettings.count == 1 && QualitySettings.GetRenderPipelineAssetAt(0) == asset, string.Join(", ", QualitySettings.names));
            var rd = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(NocPaths.RendererAsset);
            if (asset != null)
            {
                Add("hdr 64-bit / msaa 4", asset.supportsHDR && asset.hdrColorBufferPrecision == HDRColorBufferPrecision._64Bits && asset.msaaSampleCount == NocturneConstants.Msaa,
                    $"HDR {asset.supportsHDR}, {asset.hdrColorBufferPrecision}, MSAA {asset.msaaSampleCount}");
                Add("render scale 1", Mathf.Approximately(asset.renderScale, 1f), asset.renderScale.ToString());
                Add("depth / opaque textures off", !asset.supportsCameraDepthTexture && !asset.supportsCameraOpaqueTexture, $"depth {asset.supportsCameraDepthTexture} opaque {asset.supportsCameraOpaqueTexture}");
                Add("shadows off (14.2.3)", !asset.supportsMainLightShadows && !asset.supportsAdditionalLightShadows && !PipelineConfigurator.GetBool(asset, "m_AnyShadowsSupported", true),
                    $"main {asset.supportsMainLightShadows} additional {asset.supportsAdditionalLightShadows}");
                Add("additional lights per pixel", asset.additionalLightsRenderingMode == LightRenderingMode.PerPixel, asset.additionalLightsRenderingMode.ToString());
                Add("GPU resident drawer off", asset.gpuResidentDrawerMode == GPUResidentDrawerMode.Disabled, asset.gpuResidentDrawerMode.ToString());
                Add("SRP batcher on", asset.useSRPBatcher, asset.useSRPBatcher.ToString());
            }
            if (rd != null)
            {
                Add("forward+", rd.renderingMode == RenderingMode.ForwardPlus, rd.renderingMode.ToString());
                Add("depth Depth_32_Stencil_8", rd.depthAttachmentFormat == DepthFormat.Depth_32_Stencil_8, rd.depthAttachmentFormat.ToString());
                ScriptableRendererFeature grade = null, rain = null;
                bool ssao = false;
                foreach (var f in rd.rendererFeatures)
                {
                    if (f == null) continue;
                    if (f.GetType().FullName == PipelineConfigurator.GradeFeatureType) grade = f;
                    if (f.GetType().FullName == PipelineConfigurator.RainFeatureType) rain = f;
                    if (f.GetType().Name == "ScreenSpaceAmbientOcclusion") ssao = true;
                }
                Material gm = null;
                if (grade != null)
                {
                    var so = new SerializedObject(grade);
                    var p = so.FindProperty(PipelineConfigurator.GradeFeatureMaterialField);
                    gm = p != null ? p.objectReferenceValue as Material : null;
                }
                Add("grade feature", grade != null && grade.isActive && gm != null && gm.shader != null && gm.shader.name == NocPaths.ShaderGrade,
                    grade == null ? "missing" : "material " + (gm != null ? gm.name : "null"));
                // D-005: the rain is one renderer feature with the library's Rain.mat; D-008 / audit item 12: no URP post data.
                Material rm = null;
                if (rain != null)
                {
                    var so = new SerializedObject(rain);
                    var p = so.FindProperty(PipelineConfigurator.GradeFeatureMaterialField);
                    rm = p != null ? p.objectReferenceValue as Material : null;
                }
                Material libRain = lib != null ? lib.Rain : null;
                Add("rain feature (D-005)", rain != null && rain.isActive && rm != null && libRain != null && rm == libRain && rm.shader != null && rm.shader.name == MaterialFactory.ShaderRain,
                    rain == null ? "missing" : FormattableString.Invariant($"active {rain.isActive}, material {(rm != null ? EditorUtil.AssetPathOf(rm) : "null")} (library Rain {(libRain != null ? EditorUtil.AssetPathOf(libRain) : "null")})"));
                // Look v2 (TEXTURE_PLAN 7.1): the debug-view feature, after the grade, with its shader
                int gi = -1, di = -1;
                ScriptableRendererFeature dbg = null;
                for (int i = 0; i < rd.rendererFeatures.Count; i++)
                {
                    var f = rd.rendererFeatures[i];
                    if (f == null) continue;
                    if (f.GetType().FullName == PipelineConfigurator.GradeFeatureType) gi = i;
                    if (f.GetType().FullName == PipelineConfigurator.DebugViewFeatureType) { di = i; dbg = f; }
                }
                Shader dsh = null;
                if (dbg != null)
                {
                    var dp = new SerializedObject(dbg).FindProperty(PipelineConfigurator.DebugViewFeatureShaderField);
                    dsh = dp != null ? dp.objectReferenceValue as Shader : null;
                }
                Add("debug-view feature (TEXTURE_PLAN 7.1)", dbg != null && dbg.isActive && di > gi && dsh != null && dsh.name == PipelineConfigurator.DebugViewShader && dsh.isSupported,
                    dbg == null ? "missing" : FormattableString.Invariant($"index {di} (grade {gi}), shader {(dsh != null ? dsh.name : "null")}"));
                ScriptableRendererFeature pre = null;
                foreach (var f in rd.rendererFeatures) if (f != null && f.GetType().FullName == PipelineConfigurator.FacadePrepassFeatureType) pre = f;
                var pp = pre != null ? new SerializedObject(pre).FindProperty(PipelineConfigurator.DebugViewFeatureShaderField) : null;
                Shader psh = pp != null ? pp.objectReferenceValue as Shader : null;
                Add("facade prepass feature (TEXTURE_PLAN 2.12)", pre != null && pre.isActive && psh != null && psh.name == PipelineConfigurator.FacadePrepassShader && psh.isSupported,
                    pre == null ? "missing" : "shader " + (psh != null ? psh.name : "null"));
                Add("postProcessData null (D-008)", rd.postProcessData == null, rd.postProcessData != null ? EditorUtil.AssetPathOf(rd.postProcessData) : "null");
                Add("no SSAO", !ssao, ssao ? "present" : "absent");
            }

            // ---- project settings
            Add("input handler", ProjectConfigurator.ActiveInputHandler() == ProjectConfigurator.InputSystemOnly, "activeInputHandler = " + ProjectConfigurator.ActiveInputHandler());
            Add("graphics API D3D11 only", ProjectConfigurator.ApiIsD3D11Only(), ProjectConfigurator.ApiList());
            Add("product / company", PlayerSettings.productName == NocPaths.ProductName && PlayerSettings.companyName == NocPaths.CompanyName,
                PlayerSettings.companyName + "/" + PlayerSettings.productName);
            Add("linear colour space", PlayerSettings.colorSpace == ColorSpace.Linear, PlayerSettings.colorSpace.ToString());
            Add("scripting backend Mono", PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone) == ScriptingImplementation.Mono2x, PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone).ToString());
            Add("audio 48 kHz", ProjectConfigurator.AudioRate() == ProjectConfigurator.AudioSampleRate, ProjectConfigurator.AudioRate().ToString());
            ProjectConfigurator.EnsureUnityConnectOff(null);   // the Editor re-enables it at start-up; the build forces it off too
            Add("UnityConnect off", ProjectConfigurator.UnityConnectEnabled() == 0, "UnityConnectSettings m_Enabled = " + ProjectConfigurator.UnityConnectEnabled());
            Add("layers", LayerMask.LayerToName(NocPaths.CorridorLayer) == NocPaths.CorridorLayerName && LayerMask.LayerToName(NocPaths.SafetyLayer) == NocPaths.SafetyLayerName,
                LayerMask.LayerToName(NocPaths.CorridorLayer) + ", " + LayerMask.LayerToName(NocPaths.SafetyLayer));
            Add("UI panel + theme", AssetDatabase.LoadAssetAtPath<PanelSettings>(NocPaths.PanelAsset) != null && AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(NocPaths.ThemeAsset) != null,
                NocPaths.PanelAsset + ", " + NocPaths.ThemeAsset);
            if (PlayerSettings.SplashScreen.show) NocLog.Warn("Validate: splash screen is on (licence)");

            NocLog.Info("VALIDATE | status | item | detail");
            foreach (var r in rows) NocLog.Info(FormattableString.Invariant($"VALIDATE | {r.status,-4} | {r.item} | {r.detail}"));
            NocLog.Info(FormattableString.Invariant($"VALIDATE | {(problems.Count == 0 ? "PASS" : "FAIL")} | {rows.Count} checks, {problems.Count} problem(s)"));
            return problems;
        }

        /// <summary>Lesson 5: no global shader name (ShaderIds, _Noc*) equals a material property name of any Noc shader.</summary>
        static void CheckGlobalNames(Action<string, bool, string> add)
        {
            var globals = new HashSet<string>();
            foreach (var f in typeof(ShaderIds).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                if (f.FieldType == typeof(int)) globals.Add("_Noc" + f.Name);
            var clashes = new List<string>();
            foreach (var name in NocPaths.ShaderNames)
            {
                var sh = ShaderCatalog.Load(name);
                if (sh == null) continue;
                for (int i = 0; i < sh.GetPropertyCount(); i++)
                {
                    string pn = sh.GetPropertyName(i);
                    if (pn.StartsWith("_Noc", StringComparison.Ordinal) && !pn.StartsWith("_NocGrade", StringComparison.Ordinal)) clashes.Add(name + "." + pn);
                }
            }
            add("global vs material names", clashes.Count == 0, clashes.Count == 0 ? $"{globals.Count} globals" : string.Join(", ", clashes));
        }

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
            Need("Audio", SceneBuilder.AudioComponents);
            Need("SkyFill", new string[0]);
            Need("World/City", new string[0]);
            Need("World/Backdrop/NocSky", new[] { "Megastructure.BackdropRig" });
            Need("PathGuide", SceneBuilder.PathGuideComponents);

            var rig = Find("Player/CameraRig");
            var cam = rig != null ? rig.GetComponent<Camera>() : null;
            var acd = cam != null ? cam.GetComponent<UniversalAdditionalCameraData>() : null;
            add("camera", cam != null && rig.CompareTag("MainCamera") && cam.allowHDR && cam.allowMSAA && cam.clearFlags == CameraClearFlags.SolidColor &&
                          Mathf.Approximately(cam.nearClipPlane, NocturneConstants.CameraNear) && Mathf.Approximately(cam.farClipPlane, NocturneConstants.CameraFar),
                cam != null ? $"near {cam.nearClipPlane} far {cam.farClipPlane} HDR {cam.allowHDR} clear {cam.clearFlags}" : "missing");
            add("camera URP data", acd != null && !acd.renderPostProcessing && acd.antialiasing == AntialiasingMode.None && !acd.dithering && !acd.renderShadows,
                acd != null ? $"post {acd.renderPostProcessing} aa {acd.antialiasing} dither {acd.dithering} shadows {acd.renderShadows}" : "missing");
            add("audio listener on the camera only", rig != null && rig.GetComponent<AudioListener>() != null && Find("Audio") != null && Find("Audio").GetComponent<AudioListener>() == null, "listener on CameraRig, generator on Audio");
            var fillGo = Find("SkyFill");
            var light = fillGo != null ? fillGo.GetComponent<Light>() : null;
            add("sky fill light", light != null && light.type == LightType.Directional && light.shadows == LightShadows.None, light != null ? $"{light.type} {light.shadows}" : "missing");
            var dome = Find("World/Backdrop/NocSky");
            var domeR = dome != null ? dome.GetComponent<MeshRenderer>() : null;
            add("sky dome", domeR != null && domeR.sharedMaterial != null && domeR.sharedMaterial.shader != null && domeR.sharedMaterial.shader.name == NocPaths.ShaderSky,
                domeR != null && domeR.sharedMaterial != null ? domeR.sharedMaterial.name : "missing");

            var game = Find("Game");
            var gb = game != null ? game.GetComponent<GameBootstrap>() : null;
            if (gb != null)
                add("GameBootstrap refs", gb.materials != null && gb.cityRoot != null && gb.backdropRoot != null && gb.mainCamera != null,
                    $"materials {gb.materials != null} city {gb.cityRoot != null} backdrop {gb.backdropRoot != null} camera {gb.mainCamera != null}");
            var pd = game != null ? game.GetComponent<ProgressDirector>() : null;
            if (pd != null) add("ProgressDirector sky fill", pd.skyFill != null, pd.skyFill != null ? pd.skyFill.name : "null");
            var ui = game != null ? game.GetComponent<UiDirector>() : null;
            var doc = game != null ? game.GetComponent<UIDocument>() : null;
            add("UI document", ui != null && doc != null && doc.panelSettings != null && ui.panelSettings != null && ui.document == doc,
                doc != null && doc.panelSettings != null ? doc.panelSettings.name : "missing");
        }

        // ------------------------------------------------------------------------------------------------ CaptureInEditor
        [MenuItem(Menu + "Capture In Editor (play mode)", priority = 60)]
        public static void CaptureInEditor()
        {
            NocLog.QuietStackTraces();
            try
            {
                if (Application.isBatchMode && SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                {
                    Debug.LogError(NocLog.FailPrefix + "CaptureInEditor: null graphics device (do not pass -nographics)");
                    EditorApplication.Exit(4);
                    return;
                }
                if (!File.Exists(NocPaths.Abs(NocPaths.MainScene)))
                {
                    Debug.LogError(NocLog.FailPrefix + "CaptureInEditor: " + NocPaths.MainScene + " missing (run RebuildAll)");
                    if (Application.isBatchMode) EditorApplication.Exit(1);
                    return;
                }
                if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(NocPaths.MainScene, OpenSceneMode.Single);
                ShaderUtil.allowAsyncCompilation = false;
                if (Application.isBatchMode) CaptureWatchdog.Arm(CliArgs.GetDouble(CaptureWatchdog.TimeoutArg, CaptureWatchdog.DefaultTimeoutSeconds));
                NocLog.Info(FormattableString.Invariant($"CaptureInEditor: entering play mode (device {SystemInfo.graphicsDeviceType}, args: {string.Join(" ", CliArgs.Args)})"));
                EditorApplication.isPlaying = true;
            }
            catch (Exception e)
            {
                Debug.LogError(NocLog.FailPrefix + "CaptureInEditor threw: " + e);
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        // ------------------------------------------------------------------------------------------------ AnalyzeGcProfile
        /// <summary>
        /// -executeMethod ...AnalyzeGcProfile -profile-in &lt;file.raw&gt; [-profile-out &lt;report.md&gt;]: loads a player profile
        /// (NocAlloc -alloc-profile) and sums every GC.Alloc sample by thread and marker stack (the PlayerLoop phase, the script
        /// method, the package marker), bytes from the sample metadata when present. Diagnostics of the Phase 3 GC task.
        /// </summary>
        public static void AnalyzeGcProfile() => Run("AnalyzeGcProfile", AnalyzeGcProfileInternal);

        static List<string> AnalyzeGcProfileInternal()
        {
            var problems = new List<string>();
            string path = CliArgs.Get("-profile-in");
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) { NocLog.Problem(problems, "AnalyzeGcProfile: -profile-in file missing: " + path); return problems; }
            if (!UnityEditorInternal.ProfilerDriver.LoadProfile(path, false)) { NocLog.Problem(problems, "AnalyzeGcProfile: LoadProfile failed for " + path); return problems; }
            int f0 = UnityEditorInternal.ProfilerDriver.firstFrameIndex, f1 = UnityEditorInternal.ProfilerDriver.lastFrameIndex;
            var bytes = new Dictionary<string, long>();
            var counts = new Dictionary<string, long>();
            long frames = 0, total = 0, n = 0;
            var stack = new List<int>();
            var remain = new List<int>();
            for (int f = f0; f <= f1 && f >= 0; f++)
            {
                frames++;
                for (int t = 0; t < 256; t++)
                {
                    using (var v = UnityEditorInternal.ProfilerDriver.GetRawFrameDataView(f, t))
                    {
                        if (v == null || !v.valid) break;
                        int gcId = v.GetMarkerId("GC.Alloc");
                        if (gcId == UnityEditor.Profiling.FrameDataView.invalidMarkerId) continue;
                        stack.Clear(); remain.Clear();
                        for (int i = 0; i < v.sampleCount; i++)
                        {
                            while (remain.Count > 0 && remain[remain.Count - 1] == 0) { remain.RemoveAt(remain.Count - 1); stack.RemoveAt(stack.Count - 1); }
                            if (remain.Count > 0) remain[remain.Count - 1]--;
                            if (v.GetSampleMarkerId(i) == gcId)
                            {
                                long b = v.GetSampleMetadataCount(i) > 0 ? v.GetSampleMetadataAsLong(i, 0) : 0;
                                var sb = new System.Text.StringBuilder(v.threadName);
                                for (int k = 0; k < stack.Count; k++) sb.Append(" > ").Append(v.GetSampleName(stack[k]));
                                string key = sb.ToString();
                                bytes.TryGetValue(key, out long ob); bytes[key] = ob + b;
                                counts.TryGetValue(key, out long oc); counts[key] = oc + 1;
                                total += b; n++;
                            }
                            int c = v.GetSampleChildrenCount(i);
                            if (c > 0) { stack.Add(i); remain.Add(c); }
                        }
                    }
                }
            }
            var keys = new List<string>(counts.Keys);
            keys.Sort((a, b) => { int r = bytes[b].CompareTo(bytes[a]); return r != 0 ? r : counts[b].CompareTo(counts[a]); });
            var md = new System.Text.StringBuilder();
            md.Append(FormattableString.Invariant($"# GC.Alloc in {Path.GetFileName(path)}\n\n{frames} frames ({f0}..{f1}), {n} allocations, {total} B ({(frames > 0 ? total / (double)frames : 0):F1} B/frame)\n\n| bytes/frame | allocs/frame | thread > marker stack |\n|---|---|---|\n"));
            for (int i = 0; i < keys.Count && i < 60; i++)
            {
                string k = keys[i];
                string line = FormattableString.Invariant($"| {bytes[k] / (double)Math.Max(1, frames):F2} | {counts[k] / (double)Math.Max(1, frames):F3} | {k} |");
                md.Append(line).Append('\n');
                if (i < 25) NocLog.Info("GC.Alloc " + line);
            }
            string outPath = CliArgs.Get("-profile-out") ?? Path.ChangeExtension(path, ".md");
            File.WriteAllText(outPath, md.ToString());
            NocLog.Info(FormattableString.Invariant($"AnalyzeGcProfile: {frames} frames, {n} GC.Alloc samples, {total} B -> {outPath}"));
            return problems;
        }

        // ------------------------------------------------------------------------------------------------ shader variants
        // TEXTURE_PLAN 8.3 TP-0 and 9 (R13): no new multi_compile or shader_feature in Look v2. The baseline
        // QA/baselines/variants_tp0.json was written once, before any Look v2 shader change, by WriteVariantBaseline; Validate
        // compares every forward (colour) pass against it. Per pass: the keyword declarations parsed from the source (the pass,
        // the HLSLINCLUDE blocks and the local includes, recursively), the variant count they give, and Unity's own pass keyword
        // list (ShaderUtil.GetPassKeywords, every stage). A shader that is not in the baseline (a new Look v2 shader) passes when
        // its forward keywords are a subset of the lit template's (NocSurface ForwardLit) and its variant count is not larger.

        /// <summary>The committed variant baseline, relative to the project root.</summary>
        public const string VariantBaselinePath = "QA/baselines/variants_tp0.json";
        /// <summary>The lit template whose forward keyword set bounds every new Look v2 shader.</summary>
        public const string VariantTemplateShader = "Megastructure/NocSurface";

        [Serializable] public sealed class VariantPass
        {
            public int index; public string name; public string lightMode; public bool forward; public long variants;
            public List<string> pragmas = new List<string>(); public List<string> keywords = new List<string>();
        }
        [Serializable] public sealed class VariantShader
        {
            public string name; public string path;
            public List<VariantPass> passes = new List<VariantPass>();
        }
        [Serializable] public sealed class VariantReport
        {
            public int version = 1; public string unity; public string note;
            public List<VariantShader> shaders = new List<VariantShader>();
        }

        static readonly System.Text.RegularExpressions.Regex PragmaKw = new System.Text.RegularExpressions.Regex(
            @"^[ \t]*#[ \t]*pragma[ \t]+((?:multi_compile|shader_feature|dynamic_branch)\w*)\b([^\r\n]*)", System.Text.RegularExpressions.RegexOptions.Multiline);
        static readonly System.Text.RegularExpressions.Regex IncludeRx = new System.Text.RegularExpressions.Regex(
            @"^[ \t]*#[ \t]*include[ \t]+""([^""]+)""", System.Text.RegularExpressions.RegexOptions.Multiline);
        static readonly System.Text.RegularExpressions.Regex PassNameRx = new System.Text.RegularExpressions.Regex(
            @"\bName[ \t]+""([^""]*)""");

        /// <summary>Source text without // and /* */ comments (line breaks kept).</summary>
        static string StripComments(string s)
        {
            var sb = new System.Text.StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '/' && i + 1 < s.Length && s[i + 1] == '/') { while (i < s.Length && s[i] != '\n') i++; if (i < s.Length) sb.Append('\n'); continue; }
                if (s[i] == '/' && i + 1 < s.Length && s[i + 1] == '*')
                {
                    i += 2;
                    while (i + 1 < s.Length && !(s[i] == '*' && s[i + 1] == '/')) { if (s[i] == '\n') sb.Append('\n'); i++; }
                    i++;
                    continue;
                }
                sb.Append(s[i]);
            }
            return sb.ToString();
        }

        /// <summary>Keyword pragmas of a text and of its local includes (recursively, each file once).</summary>
        static void CollectPragmas(string text, string dirAbs, List<string> into, HashSet<string> visited)
        {
            foreach (System.Text.RegularExpressions.Match m in PragmaKw.Matches(text))
                into.Add((m.Groups[1].Value + " " + System.Text.RegularExpressions.Regex.Replace(m.Groups[2].Value.Trim(), @"\s+", " ")).Trim());
            foreach (System.Text.RegularExpressions.Match m in IncludeRx.Matches(text))
            {
                string rel = m.Groups[1].Value;
                if (rel.StartsWith("Packages/", StringComparison.Ordinal)) continue;
                string abs = Path.GetFullPath(rel.StartsWith("Assets/", StringComparison.Ordinal) ? NocPaths.Abs(rel) : Path.Combine(dirAbs, rel));
                if (!visited.Add(abs) || !File.Exists(abs)) continue;
                CollectPragmas(StripComments(File.ReadAllText(abs)), Path.GetDirectoryName(abs), into, visited);
            }
        }

        /// <summary>Variants one keyword declaration gives (multi_compile_instancing: 2; shader_feature with one keyword: 2;
        /// dynamic_branch: 1; otherwise the number of listed keywords, "_" included).</summary>
        static long PragmaVariants(string pragma)
        {
            string[] parts = pragma.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string kind = parts[0];
            if (kind.StartsWith("dynamic_branch", StringComparison.Ordinal)) return 1;
            if (kind == "multi_compile_instancing") return 2;
            if (kind == "multi_compile_fog") return 4;
            int n = parts.Length - 1;
            if (kind.StartsWith("shader_feature", StringComparison.Ordinal) && n == 1) return 2;
            return Math.Max(1, n);
        }

        /// <summary>The Pass blocks of the first SubShader (brace-matched, comments stripped) and the shader-wide text outside them.</summary>
        static List<string> SplitPasses(string text, out string outside)
        {
            var passes = new List<string>();
            var rest = new System.Text.StringBuilder();
            int sub = System.Text.RegularExpressions.Regex.Match(text, @"\bSubShader\b").Index;
            int i = 0;
            var passRx = new System.Text.RegularExpressions.Regex(@"(?<![\w])Pass\s*\{");
            while (i < text.Length)
            {
                var m = passRx.Match(text, i);
                if (!m.Success || m.Index < sub) { if (!m.Success) { rest.Append(text, i, text.Length - i); break; } rest.Append(text, i, m.Index + m.Length - i); i = m.Index + m.Length; continue; }
                rest.Append(text, i, m.Index - i);
                int depth = 0, j = m.Index + m.Length - 1;
                for (; j < text.Length; j++)
                {
                    if (text[j] == '{') depth++;
                    else if (text[j] == '}' && --depth == 0) break;
                }
                passes.Add(text.Substring(m.Index, Math.Min(j + 1, text.Length) - m.Index));
                i = j + 1;
            }
            outside = rest.ToString();
            return passes;
        }

        static readonly HashSet<string> NonColourLightModes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "ShadowCaster", "DepthOnly", "DepthNormals", "DepthNormalsOnly", "Meta", "MotionVectors" };

        /// <summary>Current variant report of every Megastructure/* shader.</summary>
        public static VariantReport BuildVariantReport()
        {
            var rep = new VariantReport { unity = Application.unityVersion };
            foreach (var name in NocPaths.ShaderNames)
            {
                var sh = ShaderCatalog.Load(name);
                string path = ShaderCatalog.PathOf(name);
                var e = new VariantShader { name = name, path = path };
                rep.shaders.Add(e);
                if (sh == null || path == null) continue;
                string abs = NocPaths.Abs(path);
                string text = StripComments(File.ReadAllText(abs));
                var blocks = SplitPasses(text, out string outside);
                var shared = new List<string>();
                var outsideIncludes = new System.Text.StringBuilder();
                foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(outside, @"HLSLINCLUDE(.*?)ENDHLSL", System.Text.RegularExpressions.RegexOptions.Singleline))
                    outsideIncludes.Append(m.Groups[1].Value).Append('\n');
                CollectPragmas(outsideIncludes.ToString(), Path.GetDirectoryName(abs), shared, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                for (int p = 0; p < sh.passCount; p++)
                {
                    var vp = new VariantPass { index = p };
                    var tag = sh.FindPassTagValue(p, new ShaderTagId("LightMode"));
                    vp.lightMode = string.IsNullOrEmpty(tag.name) ? "SRPDefaultUnlit" : tag.name;
                    vp.forward = !NonColourLightModes.Contains(vp.lightMode);
                    string block = p < blocks.Count ? blocks[p] : "";
                    var nm = PassNameRx.Match(block);
                    vp.name = nm.Success ? nm.Groups[1].Value : "#" + p;
                    vp.pragmas.AddRange(shared);
                    CollectPragmas(block, Path.GetDirectoryName(abs), vp.pragmas, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                    vp.pragmas.Sort(StringComparer.Ordinal);
                    vp.variants = 1;
                    foreach (var pr in vp.pragmas) vp.variants *= PragmaVariants(pr);
                    var kw = new SortedSet<string>(StringComparer.Ordinal);
                    try
                    {
                        foreach (var k in ShaderUtil.GetPassKeywords(sh, new PassIdentifier(0, (uint)p))) kw.Add(k.name);
                    }
                    catch (Exception ex) { NocLog.Warn("variants: GetPassKeywords(" + name + ", " + p + ") threw " + ex.Message); }
                    vp.keywords.AddRange(kw);
                    e.passes.Add(vp);
                }
            }
            return rep;
        }

        /// <summary>-executeMethod ...WriteVariantBaseline: writes QA/baselines/variants_tp0.json from the current shaders. Refuses
        /// to overwrite an existing baseline unless -force-baseline is given (the baseline is committed once, before Look v2).</summary>
        public static void WriteVariantBaseline() => Run("WriteVariantBaseline", () =>
        {
            var problems = new List<string>();
            string abs = Path.Combine(NocPaths.ProjectRoot, VariantBaselinePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(abs) && !CliArgs.Has("-force-baseline")) { NocLog.Problem(problems, "WriteVariantBaseline: " + VariantBaselinePath + " exists (pass -force-baseline to overwrite)"); return problems; }
            var rep = BuildVariantReport();
            rep.note = "TEXTURE_PLAN 8.3 TP-0: forward-variant baseline of every Megastructure/* shader before any Look v2 shader change; Validate gates the forward passes against it.";
            Directory.CreateDirectory(Path.GetDirectoryName(abs));
            File.WriteAllText(abs, JsonUtility.ToJson(rep, true) + "\n", new System.Text.UTF8Encoding(false));
            foreach (var s in rep.shaders)
                foreach (var p in s.passes)
                    NocLog.Info(FormattableString.Invariant($"variants: {s.name} pass {p.index} {p.name} [{p.lightMode}] forward {p.forward} variants {p.variants} keywords {string.Join(" ", p.keywords)}"));
            NocLog.Info(FormattableString.Invariant($"WriteVariantBaseline: {rep.shaders.Count} shaders -> {VariantBaselinePath}"));
            return problems;
        });

        /// <summary>Validate gate: every forward pass against the committed baseline (see above).</summary>
        static void CheckVariants(Action<string, bool, string> add)
        {
            string abs = Path.Combine(NocPaths.ProjectRoot, VariantBaselinePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(abs)) { add("forward variants (TP-0 baseline)", false, VariantBaselinePath + " missing"); return; }
            var bl = JsonUtility.FromJson<VariantReport>(File.ReadAllText(abs));
            var cur = BuildVariantReport();
            var baseBy = new Dictionary<string, VariantShader>(StringComparer.Ordinal);
            foreach (var s in bl.shaders) baseBy[s.name] = s;
            VariantPass template = null;
            if (baseBy.TryGetValue(VariantTemplateShader, out var ts))
                foreach (var p in ts.passes) if (p.forward) { template = p; break; }
            var issues = new List<string>();
            int checkedPasses = 0;
            foreach (var s in cur.shaders)
            {
                baseBy.TryGetValue(s.name, out var b);
                foreach (var p in s.passes)
                {
                    VariantPass bp = null;
                    if (b != null) foreach (var q in b.passes) if (q.name == p.name && string.Equals(q.lightMode, p.lightMode, StringComparison.OrdinalIgnoreCase)) { bp = q; break; }
                    if (!p.forward)
                    {
                        if (bp != null && (bp.variants != p.variants || string.Join(" ", bp.keywords) != string.Join(" ", p.keywords)))
                            NocLog.Warn(FormattableString.Invariant($"variants: {s.name} {p.name} (not a colour pass) changed: {bp.variants} -> {p.variants}"));
                        continue;
                    }
                    checkedPasses++;
                    if (b != null)
                    {
                        if (bp == null) { issues.Add(s.name + " new forward pass " + p.name); continue; }
                        if (bp.variants != p.variants) issues.Add(FormattableString.Invariant($"{s.name} {p.name} variants {bp.variants} -> {p.variants}"));
                        if (string.Join(" ", bp.keywords) != string.Join(" ", p.keywords)) issues.Add(s.name + " " + p.name + " keywords [" + string.Join(" ", bp.keywords) + "] -> [" + string.Join(" ", p.keywords) + "]");
                    }
                    else
                    {
                        if (template == null) { issues.Add(s.name + ": no template " + VariantTemplateShader + " in the baseline"); continue; }
                        var allowed = new HashSet<string>(template.keywords, StringComparer.Ordinal);
                        foreach (var k in p.keywords) if (!allowed.Contains(k)) issues.Add(s.name + " " + p.name + " keyword " + k + " is not in the lit template");
                        if (p.variants > template.variants) issues.Add(FormattableString.Invariant($"{s.name} {p.name} variants {p.variants} > template {template.variants}"));
                    }
                }
            }
            add("forward variants (TP-0 baseline)", issues.Count == 0,
                issues.Count == 0 ? FormattableString.Invariant($"{checkedPasses} forward passes of {cur.shaders.Count} shaders match {VariantBaselinePath}") : string.Join("; ", issues));
        }

        // ------------------------------------------------------------------------------------------------ Look v2 textures
        /// <summary>Validate gate (TEXTURE_PLAN 8.3 TP-0, 1.5): every processed set or array under Textures/External has its three
        /// maps, each imported with the settings NocTextureImport.ExpectedFor gives for its manifest (type, sRGB, Standalone format,
        /// mip filter, aniso, max size, coverage, shape); warning, not a gate until TP-2: External textures no library material uses.</summary>
        static void CheckExternalTextures(Action<string, bool, string> add, MaterialLibrary lib)
        {
            var issues = new List<string>();
            var manifests = NocTextureImport.AllManifests();
            var all = new HashSet<string>(StringComparer.Ordinal);
            foreach (var mp in manifests)
            {
                var meta = TexMeta.Load(NocPaths.Abs(mp));
                if (meta == null) { issues.Add(mp + ": unreadable"); continue; }
                string prefix = mp.Substring(0, mp.Length - TexMeta.Suffix.Length);
                foreach (ExtMap map in new[] { ExtMap.Albedo, ExtMap.Normal, ExtMap.Mask })
                {
                    if (!meta.HasMap(map)) continue;              // RoomArr has no Normal, OverlayArr no Albedo, MicroArr no Mask
                    string path = prefix + "_" + map + ".png";
                    all.Add(path);
                    var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (imp == null) { issues.Add(path + ": missing"); continue; }
                    var e = NocTextureImport.ExpectedFor(map, meta);
                    var ps = imp.GetPlatformTextureSettings("Standalone");
                    var st = new TextureImporterSettings();
                    imp.ReadTextureSettings(st);
                    var bad = new List<string>();
                    if (imp.textureType != e.type) bad.Add("type " + imp.textureType);
                    if (imp.sRGBTexture != e.srgb) bad.Add("sRGB " + imp.sRGBTexture);
                    if (!ps.overridden || ps.format != e.format) bad.Add("Standalone format " + ps.format);
                    if (!imp.mipmapEnabled || imp.mipmapFilter != e.mipFilter) bad.Add("mips " + imp.mipmapEnabled + " " + imp.mipmapFilter);
                    if (imp.anisoLevel != e.aniso) bad.Add("aniso " + imp.anisoLevel);
                    if (Math.Abs(imp.mipMapBias) > 1e-6f) bad.Add("mip bias " + imp.mipMapBias);
                    if (imp.streamingMipmaps) bad.Add("streaming on");
                    if (imp.isReadable) bad.Add("readable");
                    if (imp.mipMapsPreserveCoverage != e.preserveCoverage) bad.Add("preserve coverage " + imp.mipMapsPreserveCoverage);
                    if ((st.textureShape == TextureImporterShape.Texture2DArray) != e.array) bad.Add("shape " + st.textureShape);
                    if (bad.Count > 0) issues.Add(path + ": " + string.Join(", ", bad));
                }
            }
            add("External textures import (TEXTURE_PLAN 1.5)", issues.Count == 0,
                issues.Count == 0 ? FormattableString.Invariant($"{manifests.Count} processed set(s) / array(s)") : string.Join("; ", issues));
            if (lib == null || all.Count == 0) return;
            var used = new HashSet<string>(StringComparer.Ordinal);
            foreach (var m in lib.All())
                foreach (var d in AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(m), false)) used.Add(d);
            int unref = 0;
            foreach (var path in all) if (!used.Contains(path)) unref++;
            if (unref > 0) NocLog.Warn(FormattableString.Invariant($"Validate: {unref} of {all.Count} External textures are not referenced by a library material (a gate after TP-2)"));
        }

        /// <summary>Warning only (TEXTURE_PLAN 8.3 TP-0, R13): SRP Batcher compatibility of every Megastructure/* shader through
        /// the editor's internal ShaderUtil.GetSRPBatcherCompatibilityCode (skipped if the internal API is not reachable).</summary>
        static void CheckSrpBatcher(string[] shaderNames)
        {
            var t = typeof(ShaderUtil);
            const System.Reflection.BindingFlags bf = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
            var code = t.GetMethod("GetSRPBatcherCompatibilityCode", bf, null, new[] { typeof(Shader), typeof(int) }, null);
            var reason = t.GetMethod("GetSRPBatcherCompatibilityIssueReason", bf, null, new[] { typeof(Shader), typeof(int), typeof(int) }, null);
            if (code == null) { NocLog.Info("Validate: SRP Batcher compatibility not checked (internal API not found)"); return; }
            int ok = 0, bad = 0;
            foreach (var name in shaderNames)
            {
                var sh = ShaderCatalog.Load(name);
                if (sh == null) continue;
                try
                {
                    int c = (int)code.Invoke(null, new object[] { sh, 0 });
                    if (c == 0) { ok++; continue; }
                    bad++;
                    string why = reason != null ? reason.Invoke(null, new object[] { sh, 0, c }) as string : c.ToString();
                    NocLog.Warn("Validate: SRP Batcher: " + name + " not compatible: " + why);
                }
                catch (Exception e) { NocLog.Info("Validate: SRP Batcher check of " + name + " threw " + e.GetType().Name); }
            }
            NocLog.Info(FormattableString.Invariant($"Validate: SRP Batcher compatible {ok}, not compatible {bad} (warning only)"));
        }

        // ------------------------------------------------------------------------------------------------ plumbing
        static void Step(string name, List<string> problems, Action a)
        {
            var sw = Stopwatch.StartNew();
            int before = problems.Count;
            try { a(); }
            catch (Exception e) { NocLog.Problem(problems, FormattableString.Invariant($"{name} threw: {e}")); }
            NocLog.Info(FormattableString.Invariant($"step {name}: {(problems.Count == before ? "ok" : (problems.Count - before) + " problem(s)")} ({sw.Elapsed.TotalMilliseconds:F0} ms)"));
        }

        /// <summary>Runs a batch method body; in batch mode exits 0 (no problems) or 1.</summary>
        static void Run(string name, Func<List<string>> body)
        {
            NocLog.QuietStackTraces();
            NocLog.Info(FormattableString.Invariant($"{name}: start (Unity {Application.unityVersion}, batch {Application.isBatchMode})"));
            var sw = Stopwatch.StartNew();
            List<string> problems;
            try { problems = body() ?? new List<string>(); }
            catch (Exception e)
            {
                problems = new List<string> { name + " threw: " + e };
                Debug.LogError(NocLog.FailPrefix + name + " threw: " + e);
            }
            foreach (var p in problems) Debug.LogError(NocLog.FailPrefix + "problem: " + p);
            int code = problems.Count == 0 ? 0 : 1;
            NocLog.Info(FormattableString.Invariant($"{name}: {(code == 0 ? "SUCCESS" : "FAILED")} with {problems.Count} problem(s) in {sw.Elapsed.TotalSeconds:F1} s -> exit {code}"));
            if (Application.isBatchMode) EditorApplication.Exit(code);
        }
    }
}
