using System;
using System.Collections.Generic;
using WolfEditor.Base;
using UnityEditor;
using UnityEngine;
using WolfEditor.Editor.Nodes;
using WolfEditor.Nodes;

namespace WolfEditor.Editor
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
