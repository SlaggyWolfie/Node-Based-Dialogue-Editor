using System;
using RPG.Editor;
using RPG.Nodes;
using RPG.Nodes.Base;
using RPG.Editor.Nodes;
using UnityEditor;
using UnityEngine;

namespace RPG.Dialogue.Editor
{
    [CustomNodeEditor(typeof(ChoiceNode))]
    public sealed class ChoiceNodeEditor : NodeEditor
    {
        private Vector2 _scroll;
        //private Rect _cachedRect = Rect.zero;

        private ChoiceNode _choiceNode = null;
        private ChoiceNode ChoiceNode
        {
            get { return _choiceNode ?? (_choiceNode = (ChoiceNode)Target); }
        }

        public override void OnBodyGUI()
        {
            SerializedObject.Update();
            
            EditorGUILayout.BeginVertical();
            DrawButtons();
            DrawBranches();
            EditorGUILayout.EndVertical();

            SerializedObject.ApplyModifiedProperties();
        }

        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add +"))
            {
                Undo.RecordObject(ChoiceNode, "Add a Branch to " + ChoiceNode.name + ChoiceNode.GetHashCode());
                ChoiceNode.CreateBranch();
            }

            if (GUILayout.Button("Remove -"))
            {
                Undo.RecordObject(ChoiceNode, "Remove the last Branch from " + ChoiceNode.name + ChoiceNode.GetHashCode());
                ChoiceNode.RemoveBranch(ChoiceNode.BranchCount - 1);
                //EditorUtility.SetDirty(ChoiceNode);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawBranches()
        {
            for (int i = 0; i < ChoiceNode.BranchCount; i++)
            {
                Branch branch = ChoiceNode.GetBranch(i);
                //EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(string.Format("Branch #{0}", i));


                Rect branchRect = GUILayoutUtility.GetLastRect();
                Rect portRect = branchRect;
                portRect.size = NodePreferences.STANDARD_PORT_SIZE;
                portRect.position = new Vector2(branchRect.xMax - portRect.size.x, branchRect.y);

                DrawAndCachePort(branch.OutputPort, portRect);
                //EditorGUILayout.EndHorizontal();
            }
        }
    }
}


//-----------------------//
