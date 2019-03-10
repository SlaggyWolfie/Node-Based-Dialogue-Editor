using UnityEditor;
using WolfEditor.Editor.Nodes;

namespace WolfEditor.Utility.Editor
{
    public static class EditorUtilities
    {
        public static void AutoSaveAssets()
        {
            if (NodePreferences.Settings.ShouldAutoSave) AssetDatabase.SaveAssets();
        }
    }
}
