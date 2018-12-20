using RPG.Editor.Nodes;
using UnityEditor;

namespace RPG.Utility.Editor
{
    public static class EditorUtilities
    {
        public static void AutoSaveAssets()
        {
            if (NodePreferences.Settings.ShouldAutoSave) AssetDatabase.SaveAssets();
        }
    }
}
