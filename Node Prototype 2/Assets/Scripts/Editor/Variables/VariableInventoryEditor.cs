using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RPG.Base;
using RPG.Editor.Nodes;
using RPG.Nodes.Base;
using UnityEditor;
using UnityEngine;

namespace RPG.Editor.Variables
{
    public class VariableInventoryEditor : BaseEditor<VariableInventoryEditor, VariableInventory>
    {
        private Vector2 _scroll = Vector2.zero;
        //private Rect _cachedRect = Rect.zero;

        public void OnGUI()
        {
            if (Target == null)
            {
                Debug.Log("Variable Inventory Target is null");
                return;
            }

            EditorGUILayout.BeginVertical();
            OnHeaderGUI();
            OnBodyGUI();
            EditorGUILayout.EndVertical();
        }

        public void OnHeaderGUI()
        {
            EditorGUILayout.LabelField(Target.name, MyResources.Styles.nodeHeader, GUILayout.Height(NodePreferences.PROPERTY_HEIGHT));
        }

        public void OnBodyGUI()
        {
            SerializedObject.Update();
            bool enabledGui = GUI.enabled;

            EditorGUILayout.BeginVertical();
            DrawVariables(enabledGui);
            DrawButtons();
            EditorGUILayout.EndVertical();

            //_cachedRect = GUILayoutUtility.GetLastRect();
            SerializedObject.ApplyModifiedProperties();
        }

        private void DrawVariables(bool enabledGui)
        {
            EditorGUILayout.PrefixLabel("Variables");
            if (Target.VariableCount == 0) return;

            //TODO FIX SCROLL VIEW
            _scroll = EditorGUILayout.BeginScrollView(_scroll, false, false,
                GUILayout.MinHeight(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing),
                GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 6), GUILayout.ExpandHeight(false));

            //TODO Make stuff selectable
            for (int i = 0; i < Target.VariableCount; i++)
            {
                Variable variable = Target.GetVariable(i);

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.ExpandWidth(true));

                GUI.enabled = false;

                EditorGUILayout.ObjectField(GUIContent.none, variable, variable.GetType(), false);
                EditorGUILayout.TextField(variable.Value.ToString(),
                    GUILayout.MinWidth(NodePreferences.PROPERTY_MIN_WIDTH),
                    GUILayout.MaxWidth(NodePreferences.PROPERTY_MIN_WIDTH * 4));

                GUI.enabled = enabledGui;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add +"))
            {
                Undo.RecordObject(Target, string.Format("Add Variable to {0} {1}", Target.name, Target.GetHashCode()));
                //ConditionNode._CreateDefaultCondition();
                //NodeEditorWindow.CurrentNodeEditorWindow.OpenSubWindow<CreateVariableWindow>(
                //    new Vector2(_cachedRect.x, _cachedRect.max.y), Target);
                Window.GetUtilityWindow<CreateVariableEditor>(Target);
                //EditorUtility.SetDirty(ConditionNode);
            }

            if (GUILayout.Button("Remove -"))
            {
                Variable var = Target.GetVariable(Target.VariableCount - 1);
                if (var != null)
                {
                    Undo.RecordObject(Target, string.Format("Remove Variable {2} from {0} {1}", Target.name, Target.GetHashCode(), var.name));
                    Target.RemoveVariable(var);
                    UnityEngine.Object.DestroyImmediate(var, true);
                    OtherUtilities.AutoSaveAssets();
                }
                //EditorUtility.SetDirty(ConditionNode);
            }

            if (GUILayout.Button("Edit *"))
            {
                Variable var = Target.GetVariable(Target.VariableCount - 1);
                if (var != null)
                {
                    Undo.RecordObject(Target, string.Format("Edit Variable {2} from {0} {1}", Target.name, Target.GetHashCode(), var.name));
                    Window.GetUtilityWindow<EditVariableEditor>(var);
                    //OtherUtilities.AutoSaveAssets();
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
