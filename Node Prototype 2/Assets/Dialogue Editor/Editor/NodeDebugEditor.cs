using System;
using System.Collections.Generic;
using RPG.Base;
using RPG.Dialogue;
using RPG.Editor.Nodes;
using RPG.Nodes;
using UnityEditor;
using UnityEngine;

namespace RPG.Editor
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
