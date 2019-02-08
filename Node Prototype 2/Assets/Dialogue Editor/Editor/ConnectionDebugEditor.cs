using System;
using System.Collections.Generic;
using RPG.Base;
using RPG.Editor.Nodes;
using RPG.Nodes;
using UnityEditor;
using UnityEngine;

namespace RPG.Editor
{
    //#if RPG_DEBUG_MODE
    [CustomEditor(typeof(Connection))]
    public class ConnectionDebugEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Remove from Graph")) RemoveSelfFromGraph();
        }

        private void RemoveSelfFromGraph()
        {
            var window = NodeEditorWindow.CurrentNodeEditorWindow;
            if (window == null) return;
            window.GraphEditor.RemoveConnection((Connection)target);
        }
    }
    //#endif
}
