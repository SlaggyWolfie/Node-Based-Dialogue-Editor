using UnityEditor;
using UnityEngine;
using WolfEditor.Editor.Nodes;
using WolfEditor.Utility.Editor;
using WolfEditor.Variables;

namespace WolfEditor.Editor
{
    [CustomInstructionEditor(typeof(ConditionInstruction))]
    public sealed class ConditionInstructionEditor : InstructionEditor
    {
        //private ConditionInstruction _conditionInstruction = null;
        //private ConditionInstruction ConditionInstruction
        //{
        //    get { return _conditionInstruction ?? (_conditionInstruction = (ConditionInstruction)Target); }
        //}
        
        public override float GetWidth() { return NodePreferences.STANDARD_NODE_SIZE.x * 1.5f; }

        private const string CONDITIONS_SIZE_PATH = "_conditions.Array.size";
        private const string CONDITIONS_ARRAY_ACCESSOR_PATH = "_conditions.Array.data[{0}]";

        private Vector2 _scrollAmount = Vector2.zero;
        private SerializedList<Condition> _conditionList = null;

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
    }

    
    //[CustomInstructionEditor(typeof(ConditionModifier))]
    //public sealed class ConditionInstructionEditor : InstructionEditor
    //{
    //    private Vector2 _scroll;
    //    private Rect _cachedRect = Rect.zero;

    //    private SerializedProperty _andOrProperty = null;
    //    private ConditionModifier _conditionModifier = null;
    //    private ConditionModifier ConditionModifier
    //    {
    //        get { return _conditionModifier ?? (_conditionModifier = (ConditionModifier)Target); }
    //    }
        
    //    protected override void Awake() { _andOrProperty = SerializedObject.FindProperty("_isAnd"); }
    //    public override float GetWidth() { return NodePreferences.STANDARD_NODE_SIZE.x * 1.5f; }

    //    public override void OnBodyGUI()
    //    {
    //        SerializedObject.Update();
    //        bool enabledGui = GUI.enabled;

    //        EditorGUILayout.BeginVertical();
    //        DrawConditionsAndScroll(enabledGui);
    //        DrawButtons();
    //        DrawAndOrToggle(enabledGui);
    //        EditorGUILayout.EndVertical();

    //        _cachedRect = GUILayoutUtility.GetLastRect();
    //        SerializedObject.ApplyModifiedProperties();
    //    }

    //    private void DrawConditionsAndScroll(bool enabledGUI)
    //    {
    //        EditorGUILayout.PrefixLabel("Conditions");
    //        if (ConditionModifier.ConditionCount == 0) return;

    //        //TODO FIX SCROLL VIEW
    //        _scroll = EditorGUILayout.BeginScrollView(_scroll, false, false,
    //            GUILayout.MinHeight(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing),
    //            GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 6), GUILayout.ExpandHeight(false));

    //        for (int i = 0; i < ConditionModifier.ConditionCount; i++)
    //        {
    //            Condition condition = ConditionModifier.GetCondition(i);

    //            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.ExpandWidth(true));
    //            GUI.enabled = false;

    //            EditorGUILayout.ObjectField(GUIContent.none, condition.Variable,
    //                condition.Variable.GetType(), false, GUILayout.MinWidth(50));

    //            DrawConditionValues(condition, EditorStyles.label.CalcSize(new GUIContent("False")).x/*, enabledGUI*/);

    //            GUI.enabled = enabledGUI;
    //            EditorGUILayout.EndHorizontal();
    //        }

    //        EditorGUILayout.EndScrollView();
    //    }

    //    private void DrawButtons()
    //    {
    //        EditorGUILayout.BeginHorizontal();

    //        if (GUILayout.Button("Add +"))
    //        {
    //            Undo.RecordObject(ConditionModifier, "Add Condition to " + ConditionModifier.name + ConditionModifier.GetHashCode());
    //            Window.GetChildWindow<CreateConditionEditor>(new Vector2(_cachedRect.x, _cachedRect.max.y), ConditionModifier);
    //        }

    //        if (GUILayout.Button("Remove -"))
    //        {
    //            Undo.RecordObject(ConditionModifier, "Remove Last Condition from " + ConditionModifier.name + ConditionModifier.GetHashCode());
    //            ConditionModifier.RemoveCondition(ConditionModifier.ConditionCount - 1);
    //        }
            
    //        EditorGUILayout.EndHorizontal();
    //    }

    //    private void DrawAndOrToggle(bool enabledGui)
    //    {
    //        EditorGUILayout.BeginHorizontal();
    //        //Get Property's value
    //        bool value = _andOrProperty.boolValue;

    //        //Draw Left Toggle
    //        if (value) GUI.enabled = false;
    //        value = GUILayout.Toggle(value, "AND &&", GUI.skin.button);
    //        GUI.enabled = enabledGui;

    //        //Draw Right Toggle
    //        if (!value) GUI.enabled = false;
    //        value = !GUILayout.Toggle(!value, "OR ||", GUI.skin.button);
    //        GUI.enabled = enabledGui;

    //        //Set property's value
    //        _andOrProperty.boolValue = value;

    //        EditorGUILayout.EndHorizontal();
    //    }
        
    //    private static void DrawConditionValues(Condition condition, float perPieceWidth/*, bool enabledGUI*/)
    //    {
    //        ComparisonSign(condition.ComparisonType, EditorStyles.label.CalcSize(new GUIContent("==")).x);
    //        if (condition.UsingBuiltInValue)
    //        {
    //            EditorGUILayout.ObjectField(GUIContent.none, condition.OtherVariable,
    //                condition.OtherVariable.GetType(), false, GUILayout.MinWidth(50));
    //        }
    //        else EditorGUILayout.TextField(condition.LocalValue.Value.ToString(), GUILayout.Width(perPieceWidth));
    //    }

    //    private static void ComparisonSign(ComparisonType comparison, float width)
    //    {
    //        string sign = "==";
    //        switch (comparison)
    //        {
    //            case ComparisonType.IsEqual: break;
    //            case ComparisonType.IsNotEqual: sign = "!="; break;
    //            case ComparisonType.GreaterThan: sign = ">"; break;
    //            case ComparisonType.LesserThan: sign = "<"; break;
    //            case ComparisonType.GreaterThanOrEqual: sign = ">="; break;
    //            case ComparisonType.LesserThanOrEqual: sign = "<="; break;
    //            default: throw new ArgumentOutOfRangeException("comparison", comparison, null);
    //        }

    //        EditorGUILayout.TextField(sign, GUILayout.Width(width));
    //    }
    //}
}
