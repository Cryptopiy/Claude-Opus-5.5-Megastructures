// PaletteWriter: writes Assets/_Game/Settings/BulwarkPalette.asset from BulwarkPalette.Table (MASTER_PROMPT 14.2.1). Imported from Thread.
// The asset keeps its GUID across rebuilds (GameBootstrap references it from Main.unity).
using UnityEditor;
using UnityEngine;

namespace Megastructure.EditorTools
{
    public static class PaletteWriter
    {
        public static BulwarkPalette Write()
        {
            EditorUtil.EnsureFolder(BwPaths.SettingsDir);
            var pal = AssetDatabase.LoadAssetAtPath<BulwarkPalette>(BwPaths.PaletteAsset);
            bool created = pal == null;
            if (created)
            {
                pal = ScriptableObject.CreateInstance<BulwarkPalette>();
                pal.name = "BulwarkPalette";
                pal.FillFromTable();
                AssetDatabase.CreateAsset(pal, BwPaths.PaletteAsset);
            }
            else
            {
                pal.FillFromTable();
                EditorUtility.SetDirty(pal);
            }
            AssetDatabase.SaveAssetIfDirty(pal);
            BwLog.Info($"PaletteWriter: {(created ? "created" : "updated")} {BwPaths.PaletteAsset} ({pal.tokens.Count} tokens)");
            return pal;
        }
    }
}
