// ProjectConfigurator: PlayerSettings, graphics API, Always Included Shaders, the Corridor / Safety layers, the audio sample
// rate and the UI Toolkit theme + PanelSettings (MASTER_PROMPT Part 6, 7.6, 8.1, 8.6, 14.11, Part 12). Imported from Thread;
// The Ring's CreateOrUpdateUi / UpdateAlwaysIncludedShaders (ProjectSetup.cs) merged in (D-04: a runtime-created
// PanelSettings has no ICU data and no theme, so the asset is created here and assigned by SceneBuilder).
// Bulwark is Direct3D 11 only: -graphics-api d3d11 (default; d3d12 only for diagnostics). It is applied to
// StandaloneWindows64, which the Windows Editor also uses (first API of the list, from its NEXT start).
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Megastructure.EditorTools
{
    public static class ProjectConfigurator
    {
        public const string GraphicsApiArg = "-graphics-api";
        public const string DefaultGraphicsApi = "d3d11";
        /// <summary>ProjectSettings.asset activeInputHandler: 0 = legacy, 1 = Input System (new) only, 2 = both.</summary>
        public const int InputSystemOnly = 1;
        public const int DefaultWindowWidth = 1920, DefaultWindowHeight = 1080;
        /// <summary>14.3: 48 kHz (the runtime reads AudioSettings.outputSampleRate; the device may still run at 44.1).</summary>
        public const int AudioSampleRate = 48000;
        /// <summary>The exact theme text Unity's own PanelSettings creator writes (references the built-in theme).</summary>
        public const string ThemeText = "@import url(\"unity-theme://default\");\nVisualElement {}\n";
        /// <summary>UI Toolkit / TextCore runtime shaders PanelSettings finds with Shader.Find (Always Included, The Ring).</summary>
        public static readonly string[] UiRuntimeShaders =
        {
            "Hidden/Internal-UIRDefault", "Hidden/Internal-UIRAtlasBlitCopy", "Hidden/UIR/GaussianBlur", "Hidden/UIR/ColorEffect",
            "Hidden/UIR/DropShadowComposite", "Hidden/TextCore/Distance Field SSD", "Hidden/Internal-GUITextureClipText",
            "Hidden/TextCore/Sprite", "Hidden/Internal-UIE-ColorConversionBlit",
        };

        public static void Apply(List<string> problems)
        {
            PlayerSettings.companyName = BwPaths.CompanyName;
            PlayerSettings.productName = BwPaths.ProductName;

            // Splash (Part 6.6): off if the licence allows it; otherwise black and static.
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.SplashScreen.backgroundColor = Color.black;
            PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Static;
            if (PlayerSettings.SplashScreen.show) BwLog.Warn("ProjectConfigurator: the licence keeps the Unity splash screen on (black static fallback set)");

            // Window (Part 12, Part 8.10).
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultIsNativeResolution = true;
            PlayerSettings.defaultScreenWidth = DefaultWindowWidth;
            PlayerSettings.defaultScreenHeight = DefaultWindowHeight;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.allowFullscreenSwitch = true;      // native Alt+Enter; F11 is handled in code
            PlayerSettings.forceSingleInstance = false;
            PlayerSettings.usePlayerLog = true;
            PlayerSettings.useFlipModelSwapchain = true;
            PlayerSettings.allowHDRDisplaySupport = false;    // SDR grade only (R1 pitfall 9, R4 pitfall 22)

            // Rendering / scripting.
            if (PlayerSettings.colorSpace != ColorSpace.Linear) PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.enableFrameTimingStats = true;     // FrameTimingManager GPU times in the release player (11.7)
            ApplyGraphicsApi(problems);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);

            CheckInputHandler(problems);
            EnsureAlwaysIncludedShaders(problems);
            EnsureLayer(BwPaths.CorridorLayer, BwPaths.CorridorLayerName, problems);
            EnsureLayer(BwPaths.SafetyLayer, BwPaths.SafetyLayerName, problems);
            ApplyAudio(problems);
            CreateOrUpdateUi(problems);
            AssetDatabase.SaveAssets();

            BwLog.Info($"ProjectConfigurator: {PlayerSettings.companyName}/{PlayerSettings.productName} splash={PlayerSettings.SplashScreen.show} " +
                           $"pro={PlayerSettings.advancedLicense} colorSpace={PlayerSettings.colorSpace} apis={ApiList()} " +
                           $"backend={PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone)} " +
                           $"api-compat={PlayerSettings.GetApiCompatibilityLevel(NamedBuildTarget.Standalone)} frameTiming={PlayerSettings.enableFrameTimingStats}");
        }

        /// <summary>Requested API from the command line (d3d11 | d3d12), default d3d11.</summary>
        public static GraphicsDeviceType RequestedApi(List<string> problems)
        {
            string a = CliArgs.Get(GraphicsApiArg, DefaultGraphicsApi).Trim().ToLowerInvariant();
            if (a == "d3d12" || a == "dx12" || a == "direct3d12") return GraphicsDeviceType.Direct3D12;
            if (a != "d3d11" && a != "dx11" && a != "direct3d11")
                BwLog.Problem(problems, $"ProjectConfigurator: unknown {GraphicsApiArg} '{a}' (use d3d11 or d3d12); using Direct3D11");
            return GraphicsDeviceType.Direct3D11;
        }

        /// <summary>Explicit graphics API (auto off) for StandaloneWindows64; only touches the setting when it differs.</summary>
        public static void ApplyGraphicsApi(List<string> problems)
        {
            var want = RequestedApi(problems);
            var target = BuildTarget.StandaloneWindows64;
            var cur = PlayerSettings.GetGraphicsAPIs(target);
            bool same = !PlayerSettings.GetUseDefaultGraphicsAPIs(target) && cur != null && cur.Length == 1 && cur[0] == want;
            if (same) return;
            PlayerSettings.SetUseDefaultGraphicsAPIs(target, false);
            PlayerSettings.SetGraphicsAPIs(target, new[] { want });
            BwLog.Info($"ProjectConfigurator: graphics API for {target} set to [{want}] (the Editor uses it from its next start; " +
                           $"running device: {SystemInfo.graphicsDeviceType})");
        }

        public static string ApiList()
        {
            var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
            var parts = new string[apis.Length];
            for (int i = 0; i < apis.Length; i++) parts[i] = apis[i].ToString();
            return (PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64) ? "auto:" : "") + "[" + string.Join(",", parts) + "]";
        }

        /// <summary>Serialized PlayerSettings object (ProjectSettings/ProjectSettings.asset).</summary>
        public static SerializedObject PlayerSettingsObject()
        {
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset"))
                if (o != null && o.GetType().Name == "PlayerSettings") return new SerializedObject(o);
            return null;
        }

        public static int ActiveInputHandler()
        {
            var so = PlayerSettingsObject();
            var p = so != null ? so.FindProperty("activeInputHandler") : null;
            return p != null ? p.intValue : -1;
        }

        /// <summary>Part 7.6: Input System only. Changing it needs an Editor restart, so it is only repaired, loudly.</summary>
        static void CheckInputHandler(List<string> problems)
        {
            var so = PlayerSettingsObject();
            var p = so != null ? so.FindProperty("activeInputHandler") : null;
            if (p == null) { BwLog.Warn("ProjectConfigurator: activeInputHandler not found in PlayerSettings"); return; }
            if (p.intValue == InputSystemOnly) return;
            p.intValue = InputSystemOnly;
            so.ApplyModifiedPropertiesWithoutUndo();
            BwLog.Problem(problems, "ProjectConfigurator: activeInputHandler was not 'Input System only'; set to 1, RESTART THE EDITOR before building");
        }

        /// <summary>Adds every Megastructure/Bw* shader and the UI Toolkit runtime shaders to GraphicsSettings.m_AlwaysIncludedShaders.</summary>
        static void EnsureAlwaysIncludedShaders(List<string> problems)
        {
            var gs = EditorUtil.LoadProjectSettingsObject("ProjectSettings/GraphicsSettings.asset");
            if (gs == null) { BwLog.Problem(problems, "ProjectConfigurator: cannot load GraphicsSettings"); return; }
            var so = new SerializedObject(gs);
            var arr = so.FindProperty("m_AlwaysIncludedShaders");
            if (arr == null) { BwLog.Problem(problems, "ProjectConfigurator: m_AlwaysIncludedShaders not found"); return; }
            int added = 0;
            foreach (var uiName in UiRuntimeShaders)
            {
                var ush = Shader.Find(uiName);
                if (ush == null) { BwLog.Warn($"ProjectConfigurator: UI shader {uiName} not found for Always Included Shaders"); continue; }
                bool has = false;
                for (int i = 0; i < arr.arraySize && !has; i++) has = arr.GetArrayElementAtIndex(i).objectReferenceValue == ush;
                if (has) continue;
                arr.InsertArrayElementAtIndex(arr.arraySize);
                arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = ush;
                added++;
            }
            foreach (var name in BwPaths.ShaderNames)
            {
                var sh = MaterialFactory.FindShader(name);
                if (sh == null) { BwLog.Problem(problems, $"ProjectConfigurator: shader {name} not found, not added to Always Included Shaders"); continue; }
                bool present = false;
                for (int i = 0; i < arr.arraySize && !present; i++) present = arr.GetArrayElementAtIndex(i).objectReferenceValue == sh;
                if (present) continue;
                arr.InsertArrayElementAtIndex(arr.arraySize);
                arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = sh;
                added++;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            BwLog.Info($"ProjectConfigurator: Always Included Shaders: {added} added, {arr.arraySize} total");
        }

        /// <summary>TagManager layer index = name (30 "Corridor", 31 "Safety"; PathGuide colliders, scene contract).</summary>
        static void EnsureLayer(int index, string name, List<string> problems)
        {
            var tm = EditorUtil.LoadProjectSettingsObject("ProjectSettings/TagManager.asset");
            if (tm == null) { BwLog.Problem(problems, "ProjectConfigurator: cannot load TagManager"); return; }
            var so = new SerializedObject(tm);
            var layers = so.FindProperty("layers");
            if (layers == null || layers.arraySize <= index) { BwLog.Problem(problems, "ProjectConfigurator: TagManager layers array not found"); return; }
            var l = layers.GetArrayElementAtIndex(index);
            if (l.stringValue == name) return;
            if (!string.IsNullOrEmpty(l.stringValue)) BwLog.Warn($"ProjectConfigurator: layer {index} was '{l.stringValue}', renamed to {name}");
            l.stringValue = name;
            so.ApplyModifiedPropertiesWithoutUndo();
            BwLog.Info($"ProjectConfigurator: layer {index} = {name}");
        }

        /// <summary>AudioManager m_SampleRate = 48000 (14.3; Thread and The Ring found 0 = device default, often 44.1 kHz).</summary>
        static void ApplyAudio(List<string> problems)
        {
            var am = EditorUtil.LoadProjectSettingsObject("ProjectSettings/AudioManager.asset");
            if (am == null) { BwLog.Problem(problems, "ProjectConfigurator: cannot load AudioManager"); return; }
            var so = new SerializedObject(am);
            var sr = so.FindProperty("m_SampleRate");
            if (sr == null) { BwLog.Warn("ProjectConfigurator: AudioManager.m_SampleRate not found"); return; }
            if (sr.intValue == AudioSampleRate) return;
            sr.intValue = AudioSampleRate;
            so.ApplyModifiedPropertiesWithoutUndo();
            BwLog.Info($"ProjectConfigurator: AudioManager sample rate = {AudioSampleRate}");
        }

        /// <summary>
        /// The Ring's CreateOrUpdateUi: the theme .tss (built-in default theme) and the PanelSettings asset (Scale With Screen Size
        /// 1920 x 1080, match height; 14.11). The asset must serialize its UI shaders (m_DefaultShader): a player Shader.Find misses them.
        /// </summary>
        public static PanelSettings CreateOrUpdateUi(List<string> problems)
        {
            EditorUtil.EnsureFolder(BwPaths.SettingsDir);
            string themeAbs = BwPaths.Abs(BwPaths.ThemeAsset);
            bool themeChanged = !System.IO.File.Exists(themeAbs) || System.IO.File.ReadAllText(themeAbs) != ThemeText;
            if (themeChanged)
            {
                System.IO.File.WriteAllText(themeAbs, ThemeText, new System.Text.UTF8Encoding(false));
                AssetDatabase.ImportAsset(BwPaths.ThemeAsset, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                BwLog.Info(BwPaths.ThemeAsset + " written");
            }
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(BwPaths.ThemeAsset);
            if (theme == null) BwLog.Problem(problems, "ProjectConfigurator: could not load " + BwPaths.ThemeAsset + " as ThemeStyleSheet (UI text would have no default font)");

            var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(BwPaths.PanelSettingsAsset);
            if (ps == null)
            {
                ps = ScriptableObject.CreateInstance<PanelSettings>();
                ps.name = "BwPanelSettings";
                AssetDatabase.CreateAsset(ps, BwPaths.PanelSettingsAsset);
            }
            ps.themeStyleSheet = theme;
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.referenceResolution = new Vector2Int(FrameworkConst.RefWidth, FrameworkConst.RefHeight);
            ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            ps.match = 1f;
            ps.sortingOrder = 100f;
            ps.clearColor = false;
            EditorUtility.SetDirty(ps);
            var so = new SerializedObject(ps);
            var sh = so.FindProperty("m_DefaultShader");
            if (sh == null) BwLog.Warn("PanelSettings: m_DefaultShader field not found (cannot verify shader references)");
            else if (sh.objectReferenceValue == null) BwLog.Warn("PanelSettings.m_DefaultShader is null; relying on Always Included Shaders");
            AssetDatabase.SaveAssetIfDirty(ps);
            BwLog.Info($"ProjectConfigurator: {BwPaths.PanelSettingsAsset}: ScaleWithScreenSize {FrameworkConst.RefWidth}x{FrameworkConst.RefHeight}, match height, theme {(theme != null ? "ok" : "MISSING")}");
            return ps;
        }
    }
}
