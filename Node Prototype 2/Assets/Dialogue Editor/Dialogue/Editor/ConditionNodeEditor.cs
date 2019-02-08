using System;
using RPG.Editor;
using RPG.Nodes;
using RPG.Nodes.Base;
using RPG.Editor.Nodes;
using UnityEditor;
using UnityEngine;

namespace RPG.Dialogue.Editor
{
    [CustomNodeEditor(typeof(ConditionNode))]
    public sealed class ConditionNodeEditor : NodeEditor
    {
        private Vector2 _scroll;
        private Rect _cachedRect = Rect.zero;

        private SerializedProperty _andOrProperty = null;
        private ConditionNode _conditionNode = null;
        private ConditionNode ConditionNode
        {
            get { return _conditionNode ?? (_conditionNode = (ConditionNode)Target); }
        }

        protected override void Awake() { _andOrProperty = SerializedObject.FindProperty("_isAnd"); }
        public override float GetWidth() { return NodePreferences.STANDARD_NODE_SIZE.x * 1.5f; }

        public override void OnBodyGUI()
        {
            SerializedObject.Update();
            bool enabledGui = GUI.enabled;

            EditorGUILayout.BeginVertical();
            DrawConditionsAndScroll(enabledGui);
            DrawButtons();
            DrawAndOrToggle(enabledGui);
            DrawIfElsePorts();
            EditorGUILayout.EndVertical();

            _cachedRect = GUILayoutUtility.GetLastRect();
            SerializedObject.ApplyModifiedProperties();
        }

        private void DrawConditionsAndScroll(bool enabledGUI)
        {
            EditorGUILayout.PrefixLabel("Conditions");
            if (ConditionNode.ConditionCount == 0) return;

            //TODO FIX SCROLL VIEW
            _scroll = EditorGUILayout.BeginScrollView(_scroll, false, false,
                GUILayout.MinHeight(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing),
                GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 6), GUILayout.ExpandHeight(false));

            for (int i = 0; i < ConditionNode.ConditionCount; i++)
            {
                Condition condition = ConditionNode.GetCondition(i);

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.ExpandWidth(true));

                GUI.enabled = false;

                EditorGUILayout.ObjectField(GUIContent.none, condition.Variable,
                    condition.Variable.GetType(), false, GUILayout.MinWidth(50));

                DrawConditionValues(condition, EditorStyles.label.CalcSize(new GUIContent("False")).x/*, enabledGUI*/);

                GUI.enabled = enabledGUI;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add +"))
            {
                Undo.RecordObject(ConditionNode, "Add Condition to " + ConditionNode.name + ConditionNode.GetHashCode());
                //ConditionNode._CreateDefaultCondition();
                //NodeEditorWindow.CurrentNodeEditorWindow.OpenChildWindow<CreateVariableBehaviour>(
                //    new Vector2(_cachedRect.x, _cachedRect.max.y), ConditionNode);
                Window.GetChildWindow<CreateConditionEditor>(new Vector2(_cachedRect.x, _cachedRect.max.y), ConditionNode);
                //EditorUtility.SetDirty(ConditionNode);
            }

            if (GUILayout.Button("Remove -"))
            {
                Undo.RecordObject(ConditionNode, "Remove Last Condition from " + ConditionNode.name + ConditionNode.GetHashCode());
                ConditionNode.RemoveCondition(ConditionNode.ConditionCount - 1);
                //EditorUtility.SetDirty(ConditionNode);
            }

            //if (GUILayout.Button("Edit *"))
            //{
            //    Undo.RecordObject(ConditionNode, "Edit Condition from " + ConditionNode.name + ConditionNode.GetHashCode());
            //    //Window.GetChildWindow<EditConditionEditor>(new Vector2(_cachedRect.x, _cachedRect.max.y), ConditionNode);
            //}

            EditorGUILayout.EndHorizontal();
        }

        private void DrawAndOrToggle(bool enabledGui)
        {
            EditorGUILayout.BeginHorizontal();
            //Get Property's value
            bool value = _andOrProperty.boolValue;

            //Draw Left Toggle
            if (value) GUI.enabled = false;
            value = GUILayout.Toggle(value, "AND &&", GUI.skin.button);
            GUI.enabled = enabledGui;

            //Draw Right Toggle
            if (!value) GUI.enabled = false;
            value = !GUILayout.Toggle(!value, "OR ||", GUI.skin.button);
            GUI.enabled = enabledGui;

            //Set property's value
            _andOrProperty.boolValue = value;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawIfElsePorts()
        {
            EditorGUILayout.LabelField("IF");

            Rect previousRect = GUILayoutUtility.GetLastRect();
            Rect ifRect = previousRect;
            ifRect.size = NodePreferences.STANDARD_PORT_SIZE;
            ifRect.position = new Vector2(previousRect.xMax - ifRect.size.x, previousRect.y);

            DrawAndCachePort(ConditionNode.IfOutputPort, ifRect);

            EditorGUILayout.LabelField("ELSE");

            previousRect = GUILayoutUtility.GetLastRect();
            Rect elseRect = previousRect;
            elseRect.size = NodePreferences.STANDARD_PORT_SIZE;
            elseRect.position = new Vector2(previousRect.xMax - elseRect.size.x, previousRect.y);

            DrawAndCachePort(ConditionNode.ElseOutputPort, elseRect);
        }

        private static void DrawConditionValues(Condition condition, float perPieceWidth/*, bool enabledGUI*/)
        {
            //EditorGUILayout.TextField(condition.Variable.Value.ToString(), GUILayout.Width(perPieceWidth));
            ComparisonSign(condition.ComparisonType, EditorStyles.label.CalcSize(new GUIContent("==")).x);
            if (condition.UsingBuiltInValue)
            {
                //GUI.enabled = false;

                EditorGUILayout.ObjectField(GUIContent.none, condition.OtherVariable,
                    condition.OtherVariable.GetType(), false, GUILayout.MinWidth(50));
            }
            else EditorGUILayout.TextField(condition.LocalValue.Value.ToString(), GUILayout.Width(perPieceWidth));
        }

        private static void ComparisonSign(ComparisonType comparison, float width)
        {
            string sign = "==";
            switch (comparison)
            {
                case ComparisonType.IsEqual: break;
                case ComparisonType.IsNotEqual: sign = "!="; break;
                case ComparisonType.GreaterThan: sign = ">"; break;
                case ComparisonType.LesserThan: sign = "<"; break;
                case ComparisonType.GreaterThanOrEqual: sign = ">="; break;
                case ComparisonType.LesserThanOrEqual: sign = "<="; break;
                default: throw new ArgumentOutOfRangeException("comparison", comparison, null);
            }

            EditorGUILayout.TextField(sign, GUILayout.Width(width));
        }
        }
}
