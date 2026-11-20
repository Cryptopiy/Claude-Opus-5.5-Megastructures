// ProjectConfigurator: PlayerSettings, graphics API, Always Included Shaders, layers and audio settings (MASTER_PROMPT Part 6,
// 7.6, 8.1, 8.6, Part 12; IMPORT_PLAN 8.21; D-008), imported from Thread.
// Direct3D 11 ONLY for StandaloneWindows64 (auto graphics API off; D-008, Part 8.1; never D3D12 from The Ring's ProjectSetup).
// ProductName "Nocturne", CompanyName "Megastructures" (Player.log at %USERPROFILE%\AppData\LocalLow\Megastructures\Nocturne).
// Splash off when the licence allows it, linear colour space, fullscreen window, resizable, run in background, Input System only,
// 48 kHz audio (The Ring's AudioManager m_SampleRate pattern), UnityConnect m_Enabled 0, Always Included = every Megastructure/* shader
// under Assets/_Game/Shaders (discovered by ShaderCatalog, Shaders/Beats included) + the UI Toolkit runtime shaders (NocUiSetup),
// layers 30 "Corridor" and 31 "Safety".
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace Megastructure.EditorTools
{
    public static class ProjectConfigurator
    {
        /// <summary>ProjectSettings.asset activeInputHandler: 0 = legacy, 1 = Input System (new) only, 2 = both.</summary>
        public const int InputSystemOnly = 1;
        public const int DefaultWindowWidth = 1920, DefaultWindowHeight = 1080;
        public const int AudioSampleRate = 48000;
        public static readonly GraphicsDeviceType[] WindowsApis = { GraphicsDeviceType.Direct3D11 };

        public static void Apply(List<string> problems)
        {
            PlayerSettings.companyName = NocPaths.CompanyName;
            PlayerSettings.productName = NocPaths.ProductName;

            // Splash (Part 6): off if the licence allows it; otherwise black and static.
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.SplashScreen.backgroundColor = Color.black;
            PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Static;
            if (PlayerSettings.SplashScreen.show) NocLog.Warn("ProjectConfigurator: the licence keeps the Unity splash screen on (black static fallback set)");

            // Window (Part 12, Part 8.10).
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultIsNativeResolution = true;
            PlayerSettings.defaultScreenWidth = DefaultWindowWidth;
            PlayerSettings.defaultScreenHeight = DefaultWindowHeight;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.allowFullscreenSwitch = true;      // native Alt+Enter; F11 is handled by QualityBootstrap
            PlayerSettings.forceSingleInstance = false;
            PlayerSettings.usePlayerLog = true;
            PlayerSettings.useFlipModelSwapchain = true;
            PlayerSettings.allowHDRDisplaySupport = false;    // SDR grade only

            if (PlayerSettings.colorSpace != ColorSpace.Linear) PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.enableFrameTimingStats = true;     // FrameTimingManager GPU times in the release player (11.7)
            ApplyGraphicsApi();
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);

            CheckInputHandler(problems);
            ApplyAudio(problems);
            EnsureUnityConnectOff(problems);
            EnsureAlwaysIncludedShaders(problems);
            EnsureLayer(NocPaths.CorridorLayer, NocPaths.CorridorLayerName, problems);
            EnsureLayer(NocPaths.SafetyLayer, NocPaths.SafetyLayerName, problems);
            AssetDatabase.SaveAssets();

            NocLog.Info(FormattableString.Invariant($"ProjectConfigurator: {PlayerSettings.companyName}/{PlayerSettings.productName} splash={PlayerSettings.SplashScreen.show} ") +
                        FormattableString.Invariant($"pro={PlayerSettings.advancedLicense} colorSpace={PlayerSettings.colorSpace} apis={ApiList()} ") +
                        FormattableString.Invariant($"backend={PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone)} input={ActiveInputHandler()}"));
        }

        /// <summary>Direct3D 11 only (auto off) for StandaloneWindows64; only touches the setting when it differs.</summary>
        public static void ApplyGraphicsApi()
        {
            var target = BuildTarget.StandaloneWindows64;
            var cur = PlayerSettings.GetGraphicsAPIs(target);
            bool same = !PlayerSettings.GetUseDefaultGraphicsAPIs(target) && cur != null && cur.Length == 1 && cur[0] == GraphicsDeviceType.Direct3D11;
            if (same) return;
            PlayerSettings.SetUseDefaultGraphicsAPIs(target, false);
            PlayerSettings.SetGraphicsAPIs(target, WindowsApis);
            NocLog.Info(FormattableString.Invariant($"ProjectConfigurator: graphics API for {target} set to [Direct3D11] (running device: {SystemInfo.graphicsDeviceType})"));
        }

        public static string ApiList()
        {
            var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
            var parts = new string[apis.Length];
            for (int i = 0; i < apis.Length; i++) parts[i] = apis[i].ToString();
            return (PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64) ? "auto:" : "") + "[" + string.Join(",", parts) + "]";
        }

        public static bool ApiIsD3D11Only()
        {
            var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
            return !PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64) && apis.Length == 1 && apis[0] == GraphicsDeviceType.Direct3D11;
        }

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
            if (p == null) { NocLog.Warn("ProjectConfigurator: activeInputHandler not found in PlayerSettings"); return; }
            if (p.intValue == InputSystemOnly) return;
            p.intValue = InputSystemOnly;
            so.ApplyModifiedPropertiesWithoutUndo();
            NocLog.Problem(problems, "ProjectConfigurator: activeInputHandler was not 'Input System only'; set to 1, RESTART THE EDITOR before building");
        }

        /// <summary>48 kHz output request (the runtime always reads AudioSettings.outputSampleRate).</summary>
        static void ApplyAudio(List<string> problems)
        {
            var am = EditorUtil.LoadProjectSettingsObject("ProjectSettings/AudioManager.asset");
            if (am == null) { NocLog.Problem(problems, "ProjectConfigurator: cannot load AudioManager"); return; }
            var so = new SerializedObject(am);
            var rate = so.FindProperty("m_SampleRate");
            if (rate == null) { NocLog.Warn("ProjectConfigurator: AudioManager.m_SampleRate not found"); return; }
            if (rate.intValue != AudioSampleRate) { rate.intValue = AudioSampleRate; so.ApplyModifiedPropertiesWithoutUndo(); }
        }

        /// <summary>UnityConnectSettings m_Enabled forced to 0 (no Unity cloud services, analytics or crash upload in this offline
        /// project; audit S3 item 13). Only touches the asset when it differs. The Editor itself sets m_Enabled back to 1 when it
        /// starts (signed-in licence), so RebuildAll, BuildWindows and Validate's test re-apply it before they use it.</summary>
        public static void EnsureUnityConnectOff(List<string> problems)
        {
            var uc = EditorUtil.LoadProjectSettingsObject("ProjectSettings/UnityConnectSettings.asset");
            if (uc == null) { NocLog.Problem(problems, "ProjectConfigurator: cannot load UnityConnectSettings"); return; }
            var so = new SerializedObject(uc);
            var en = so.FindProperty("m_Enabled");
            if (en == null) { NocLog.Problem(problems, "ProjectConfigurator: UnityConnectSettings.m_Enabled not found"); return; }
            bool isBool = en.propertyType == SerializedPropertyType.Boolean;
            if (isBool ? !en.boolValue : en.intValue == 0) return;
            if (isBool) en.boolValue = false; else en.intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            NocLog.Info("ProjectConfigurator: UnityConnectSettings m_Enabled set to 0");
        }

        /// <summary>
        /// The Editor turns UnityConnect back on by itself (the template project is linked to a cloud project id and the licence
        /// is signed in), at start-up and sometimes later in the session (seen: on during I0-build01 after the forced 0 at its
        /// start). Every Editor session therefore re-applies the forced 0 once the Editor is idle after a domain reload
        /// (delayCall) and when it quits; RebuildAll, BuildWindows and Validate also apply it explicitly, before and after their
        /// work. EnsureUnityConnectOff only writes when the value differs.
        /// </summary>
        [InitializeOnLoadMethod]
        static void GuardUnityConnect()
        {
            EditorApplication.delayCall += () => { try { EnsureUnityConnectOff(null); } catch (Exception e) { NocLog.Warn("UnityConnect guard: " + e.Message); } };
            EditorApplication.quitting -= QuitGuard;
            EditorApplication.quitting += QuitGuard;
        }

        static void QuitGuard()
        {
            try { EnsureUnityConnectOff(null); } catch (Exception e) { NocLog.Warn("UnityConnect guard at quit: " + e.Message); }
        }

        /// <summary>UnityConnectSettings m_Enabled (0 expected; -1 when unreadable).</summary>
        public static int UnityConnectEnabled()
        {
            var uc = EditorUtil.LoadProjectSettingsObject("ProjectSettings/UnityConnectSettings.asset");
            var p = uc != null ? new SerializedObject(uc).FindProperty("m_Enabled") : null;
            if (p == null) return -1;
            return p.propertyType == SerializedPropertyType.Boolean ? (p.boolValue ? 1 : 0) : p.intValue;
        }

        public static int AudioRate()
        {
            var am = EditorUtil.LoadProjectSettingsObject("ProjectSettings/AudioManager.asset");
            var p = am != null ? new SerializedObject(am).FindProperty("m_SampleRate") : null;
            return p != null ? p.intValue : -1;
        }

        /// <summary>
        /// Every "Megastructure/*" shader found under Assets/_Game/Shaders (ShaderCatalog: the framework and library shaders and the
        /// beat builders' Shaders/Beats/*, discovered from the files) and the UI Toolkit runtime shaders in
        /// GraphicsSettings.m_AlwaysIncludedShaders, so a new beat shader works in the player without anyone touching
        /// ProjectSettings. Additive and idempotent (unresolved entries are reported, never removed: built-in references stay).
        /// Called by RebuildAll (Apply) and by BuildWindows before every build. Returns the number of changes.
        /// </summary>
        public static int EnsureAlwaysIncludedShaders(List<string> problems)
        {
            var gs = EditorUtil.LoadProjectSettingsObject("ProjectSettings/GraphicsSettings.asset");
            if (gs == null) { NocLog.Problem(problems, "ProjectConfigurator: cannot load GraphicsSettings"); return 0; }
            var so = new SerializedObject(gs);
            var arr = so.FindProperty("m_AlwaysIncludedShaders");
            if (arr == null) { NocLog.Problem(problems, "ProjectConfigurator: m_AlwaysIncludedShaders not found"); return 0; }
            int added = 0, unresolved = 0;
            for (int i = 0; i < arr.arraySize; i++) if (arr.GetArrayElementAtIndex(i).objectReferenceValue == null) unresolved++;
            var wanted = new List<Shader>();
            var names = NocPaths.ShaderNames;
            foreach (var name in names)
            {
                var sh = ShaderCatalog.Load(name);
                if (sh == null) NocLog.Problem(problems, FormattableString.Invariant($"ProjectConfigurator: shader {name} ({ShaderCatalog.PathOf(name)}) not imported, not added to Always Included Shaders"));
                else wanted.Add(sh);
            }
            foreach (var name in NocUiSetup.UiRuntimeShaders)
            {
                var sh = Shader.Find(name);
                if (sh == null) NocLog.Warn(FormattableString.Invariant($"ProjectConfigurator: UI runtime shader {name} not found (not added)"));
                else wanted.Add(sh);
            }
            foreach (var sh in wanted)
            {
                bool present = false;
                for (int i = 0; i < arr.arraySize && !present; i++) present = arr.GetArrayElementAtIndex(i).objectReferenceValue == sh;
                if (present) continue;
                arr.InsertArrayElementAtIndex(arr.arraySize);
                arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = sh;
                added++;
            }
            if (added > 0)
            {
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(gs);
                AssetDatabase.SaveAssets();
            }
            NocLog.Info(FormattableString.Invariant($"ProjectConfigurator: Always Included Shaders: {names.Length} Megastructure/* discovered, {added} added, {arr.arraySize} total, {unresolved} unresolved entr(y/ies) kept"));
            return added;
        }

        /// <summary>True when the shader is in GraphicsSettings.m_AlwaysIncludedShaders (Validate).</summary>
        public static bool IsAlwaysIncluded(Shader sh)
        {
            var gs = EditorUtil.LoadProjectSettingsObject("ProjectSettings/GraphicsSettings.asset");
            var arr = gs != null ? new SerializedObject(gs).FindProperty("m_AlwaysIncludedShaders") : null;
            if (arr == null || sh == null) return false;
            for (int i = 0; i < arr.arraySize; i++) if (arr.GetArrayElementAtIndex(i).objectReferenceValue == sh) return true;
            return false;
        }

        static void EnsureLayer(int index, string name, List<string> problems)
        {
            var tm = EditorUtil.LoadProjectSettingsObject("ProjectSettings/TagManager.asset");
            if (tm == null) { NocLog.Problem(problems, "ProjectConfigurator: cannot load TagManager"); return; }
            var so = new SerializedObject(tm);
            var layers = so.FindProperty("layers");
            if (layers == null || layers.arraySize <= index) { NocLog.Problem(problems, "ProjectConfigurator: TagManager layers array not found"); return; }
            var l = layers.GetArrayElementAtIndex(index);
            if (l.stringValue == name) return;
            if (!string.IsNullOrEmpty(l.stringValue)) NocLog.Warn(FormattableString.Invariant($"ProjectConfigurator: layer {index} was '{l.stringValue}', renamed to {name}"));
            l.stringValue = name;
            so.ApplyModifiedPropertiesWithoutUndo();
            NocLog.Info(FormattableString.Invariant($"ProjectConfigurator: layer {index} = {name}"));
        }
    }
}
