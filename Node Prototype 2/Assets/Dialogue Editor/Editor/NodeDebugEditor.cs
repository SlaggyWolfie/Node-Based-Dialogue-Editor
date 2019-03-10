using System;
using System.Collections.Generic;
using WolfEditor.Base;
using WolfEditor.Dialogue;
using UnityEditor;
using UnityEngine;
using WolfEditor.Editor.Nodes;
using WolfEditor.Nodes;

namespace WolfEditor.Editor
{
    //#if RPG_DEBUG_MODE
    [CustomEditor(typeof(Node), true)]
    public class NodeDebugEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            //Debug.Log("YOLO");
            DrawDefaultInspector();
            if (GUILayout.Button("Remove from Graph")) RemoveSelfFromGraph();
        }

        private void RemoveSelfFromGraph()
        {
            var window = NodeEditorWindow.CurrentNodeEditorWindow;
            if (window != null) window.GraphEditor.RemoveNode((Node)target);
        }
    }
    //#endif
}
