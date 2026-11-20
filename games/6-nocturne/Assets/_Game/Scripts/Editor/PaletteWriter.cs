// PaletteWriter: writes Assets/_Game/Settings/NocturnePalette.asset from NocturnePalette.Table (MASTER_PROMPT 14.7.8 a).
// The asset keeps its GUID across rebuilds (GameBootstrap references it from Main.unity).
using UnityEditor;
using UnityEngine;

namespace Megastructure.EditorTools
{
    public static class PaletteWriter
    {
        public static NocturnePalette Write()
        {
            EditorUtil.EnsureFolder(NocPaths.SettingsDir);
            var pal = AssetDatabase.LoadAssetAtPath<NocturnePalette>(NocPaths.PaletteAsset);
            bool created = pal == null;
            if (created)
            {
                pal = ScriptableObject.CreateInstance<NocturnePalette>();
                pal.name = "NocturnePalette";
                pal.FillFromTable();
                AssetDatabase.CreateAsset(pal, NocPaths.PaletteAsset);
            }
            else
            {
                pal.FillFromTable();
                EditorUtility.SetDirty(pal);
            }
            AssetDatabase.SaveAssetIfDirty(pal);
            NocLog.Info($"PaletteWriter: {(created ? "created" : "updated")} {NocPaths.PaletteAsset} ({pal.tokens.Count} tokens)");
            return pal;
        }
    }
}
