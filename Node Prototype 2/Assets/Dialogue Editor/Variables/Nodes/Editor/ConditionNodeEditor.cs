using UnityEditor;
using UnityEngine;
using WolfEditor.Editor.Nodes;
using WolfEditor.Utility.Editor;

namespace WolfEditor.Variables.Editor
{
    [CustomNodeEditor(typeof(ConditionNode))]
    public sealed class ConditionNodeEditor : NodeEditor
    {
        private const string CONDITIONS_SIZE_PATH = "_conditions.Array.size";
        private const string CONDITIONS_ARRAY_ACCESSOR_PATH = "_conditions.Array.data[{0}]";

        private Vector2 _scrollAmount = Vector2.zero;
        //private SerializedProperty _conditionsSizeProperty = null;
        private SerializedList<Condition> _conditionList = null;

        private ConditionNode _conditionNode = null;
        private ConditionNode ConditionNode
        {
            get { return _conditionNode ?? (_conditionNode = (ConditionNode)Target); }
        }

        public override float GetWidth() { return NodePreferences.STANDARD_NODE_SIZE.x * 1.5f; }

        public override void OnBodyGUI()
        {
            SerializedObject.Update();

            SerializedProperty andProperty = SerializedObject.FindProperty("_logicalAND");

            if (_conditionList == null)
                _conditionList = new SerializedList<Condition>(SerializedObject,
                    CONDITIONS_SIZE_PATH, CONDITIONS_ARRAY_ACCESSOR_PATH);
            //_conditionsSizeProperty = SerializedObject.FindProperty(CONDITIONS_SIZE_PATH);

            //Draw Conditions
            EditorGUILayout.PrefixLabel("Conditions");
            if (_conditionList.sizeProperty.intValue > 3)
            {
                _scrollAmount = EditorGUILayout.BeginScrollView(_scrollAmount);
                for (int i = 0; i < _conditionList.sizeProperty.intValue; i++)
                    DrawCondition(i);
                EditorGUILayout.EndScrollView();
            }
            else if (_conditionList.sizeProperty.intValue > 0)
            {
                for (int i = 0; i < _conditionList.sizeProperty.intValue; i++)
                    DrawCondition(i);
            }

            //Draw Buttons & Options
            if (GUILayout.Button("+")) _conditionList.Add();
            DrawAndOrToggle(andProperty, GUI.enabled);

            DrawIfElsePorts();

            SerializedObject.ApplyModifiedProperties();
        }

        private void DrawCondition(int index)
        {
            GUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(_conditionList.GetProperty(index));

            bool oldEnabled = GUI.enabled;
            GUI.enabled &= _conditionList.IsNotMinIndex(index);
            if (GUILayout.Button("↑")) _conditionList.Swap(index, index - 1);

            GUI.enabled = oldEnabled && _conditionList.IsNotMaxIndex(index);
            if (GUILayout.Button("↓")) _conditionList.Swap(index, index + 1);

            GUI.enabled = oldEnabled;
            if (GUILayout.Button("✕")) _conditionList.Remove(index);

            GUILayout.EndHorizontal();
        }

        private static void DrawAndOrToggle(SerializedProperty boolProperty, bool gui = true)
        {
            EditorGUILayout.BeginHorizontal();

            bool value = boolProperty.boolValue;

            //Draw Left Toggle
            if (value) GUI.enabled = false;
            value = GUILayout.Toggle(value, "AND", GUI.skin.button);
            GUI.enabled = gui;

            //Draw Right Toggle
            if (!value) GUI.enabled = false;
            value = !GUILayout.Toggle(!value, "OR", GUI.skin.button);
            GUI.enabled = gui;

            //Set property's value
            boolProperty.boolValue = value;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawIfElsePorts()
        {
            EditorGUILayout.LabelField("TRUE");

            Rect previousRect = GUILayoutUtility.GetLastRect();
            Rect ifRect = previousRect;
            ifRect.size = NodePreferences.STANDARD_PORT_SIZE;
            ifRect.position = new Vector2(previousRect.xMax - ifRect.size.x, previousRect.y);

            DrawAndCachePort(ConditionNode.IfOutputPort, ifRect);

            EditorGUILayout.LabelField("FALSE");

            previousRect = GUILayoutUtility.GetLastRect();
            Rect elseRect = previousRect;
            elseRect.size = NodePreferences.STANDARD_PORT_SIZE;
            elseRect.position = new Vector2(previousRect.xMax - elseRect.size.x, previousRect.y);

            DrawAndCachePort(ConditionNode.ElseOutputPort, elseRect);
        }
    }
}
