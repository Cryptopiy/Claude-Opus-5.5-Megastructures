// NocUiSetup: the UI Toolkit assets of the UiDirector (14.11; IMPORT_PLAN 8.6, from The Ring's ProjectSetup.CreateOrUpdateUi):
// Assets/_Game/Settings/NocTheme.tss (the exact text Unity's own PanelSettings creator writes: the built-in default theme) and
// NocPanel.asset (Scale With Screen Size 1920 x 1080, match height, sorting order 100, no clear). The UI runtime shaders are
// added to Always Included Shaders by ProjectConfigurator (the editor-created PanelSettings must serialize its shader references;
// Shader.Find in the player misses them otherwise).
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Megastructure.EditorTools
{
    public static class NocUiSetup
    {
        public const string ThemeText = "@import url(\"unity-theme://default\");\nVisualElement {}\n";
        public static readonly Vector2Int ReferenceResolution = new Vector2Int(1920, 1080);
        public const float MatchHeight = 1f;
        public const float SortingOrder = 100f;

        public static readonly string[] UiRuntimeShaders =
        {
            "Hidden/Internal-UIRDefault", "Hidden/Internal-UIRAtlasBlitCopy", "Hidden/UIR/GaussianBlur", "Hidden/UIR/ColorEffect",
            "Hidden/UIR/DropShadowComposite", "Hidden/TextCore/Distance Field SSD", "Hidden/Internal-GUITextureClipText",
            "Hidden/TextCore/Sprite", "Hidden/Internal-UIE-ColorConversionBlit",
        };

        public static PanelSettings CreateOrUpdate(System.Collections.Generic.List<string> problems)
        {
            EditorUtil.EnsureFolder(NocPaths.SettingsDir);
            if (EditorUtil.WriteTextIfChanged(NocPaths.ThemeAsset, ThemeText))
            {
                AssetDatabase.ImportAsset(NocPaths.ThemeAsset, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                NocLog.Info("NocUiSetup: " + NocPaths.ThemeAsset + " written");
            }
            else if (AssetDatabase.LoadMainAssetAtPath(NocPaths.ThemeAsset) == null)
                AssetDatabase.ImportAsset(NocPaths.ThemeAsset, ImportAssetOptions.ForceSynchronousImport);

            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(NocPaths.ThemeAsset);
            if (theme == null) NocLog.Problem(problems, "NocUiSetup: could not load " + NocPaths.ThemeAsset + " as ThemeStyleSheet (UI text would have no default font)");

            var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(NocPaths.PanelAsset);
            bool created = ps == null;
            if (created)
            {
                ps = ScriptableObject.CreateInstance<PanelSettings>();
                ps.name = "NocPanel";
                AssetDatabase.CreateAsset(ps, NocPaths.PanelAsset);
            }
            ps.themeStyleSheet = theme;
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.referenceResolution = ReferenceResolution;
            ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            ps.match = MatchHeight;
            ps.sortingOrder = SortingOrder;
            ps.clearColor = false;
            EditorUtility.SetDirty(ps);
            AssetDatabase.SaveAssetIfDirty(ps);
            NocLog.Info(FormattableString.Invariant($"NocUiSetup: {NocPaths.PanelAsset} {(created ? "created" : "updated")}: ScaleWithScreenSize {ReferenceResolution.x}x{ReferenceResolution.y}, match height, sorting {SortingOrder}, theme {(theme != null ? "ok" : "MISSING")}"));
            return ps;
        }
    }
}
