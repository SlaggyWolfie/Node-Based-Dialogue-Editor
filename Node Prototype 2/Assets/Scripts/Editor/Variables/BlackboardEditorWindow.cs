using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RPG.Editor.Nodes;
using RPG.Nodes;
using RPG.Nodes.Base;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace RPG.Editor.Variables
{
    public class BlackboardEditorWindow : EditorWindow
    {
        //TODO: Finish this.
        private static BlackboardEditorWindow _currentBlackboardEditorWindow = null;
        public static BlackboardEditorWindow CurrentBlackboardEditorWindow
        {
            get { return _currentBlackboardEditorWindow; }
            private set { _currentBlackboardEditorWindow = value; }
        }

        [OnOpenAsset(1)]
        public static bool OnOpen(int instanceID, int line)
        {
            VariableInventory variableInventory = EditorUtility.InstanceIDToObject(instanceID) as VariableInventory;
            if (variableInventory == null) return false;

            BlackboardEditorWindow window = (BlackboardEditorWindow)GetWindow(typeof(BlackboardEditorWindow), false, "RPG Node Editor", true);
            window.wantsMouseMove = true;
            return true;
        }

        //public static NodeEditorWindow OpenWindow()
        //{
        //    BlackboardEditorWindow window = CreateInstance<BlackboardEditorWindow>();
        //    //window.titleContent = new GUIContent("Node Editor");
        //    //window.wantsMouseMove = true;
        //    //window.Show();
        //    return window;
        //}

        private Blackboard _blackboard = null;
        private Blackboard Blackboard { get { return _blackboard ?? Blackboard.Instance; } }

        private void OnFocus()
        {
            CurrentBlackboardEditorWindow = this;
            if (Blackboard != null) NodeUtility.AutoSaveAssets();
        }

        private void OnGUI()
        {
            if (Blackboard == null) return;
            //Draw Global Inventory
            //Draw Local Inventory
        }
        
        //public void Save()
        //{
        //    if (AssetDatabase.Contains(_graph))
        //    {
        //        EditorUtility.SetDirty(_graph);
        //        NodeUtility.AutoSaveAssets();
        //    }
        //    else SaveAs();
        //}

        //public void SaveAs()
        //{
        //    string path = EditorUtility.SaveFilePanelInProject("Save Node Graph", "NewNodeGraph", "asset", "");
        //    if (string.IsNullOrEmpty(path)) return;

        //    NodeGraph existingGraph = AssetDatabase.LoadAssetAtPath<NodeGraph>(path);
        //    if (existingGraph != null) AssetDatabase.DeleteAsset(path);
        //    AssetDatabase.CreateAsset(_graph, path);
        //    EditorUtility.SetDirty(_graph);
        //    NodeUtility.AutoSaveAssets();
        //}
    }
}
