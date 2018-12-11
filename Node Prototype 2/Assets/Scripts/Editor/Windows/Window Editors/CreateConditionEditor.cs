using System;
using RPG.Dialogue;
using RPG.Nodes.Base;
using UnityEditor;
using UnityEngine;

namespace RPG.Editor.Nodes
{
    public class CreateConditionEditor : WindowEditor
    {
        private enum ComparisonTypeSimple { None, IsEqual, IsNotEqual }

        private Variable _variable = null;
        private ComparisonType _comparisonType = ComparisonType.IsEqual;
        private ComparisonTypeSimple _comparisonTypeSimple = ComparisonTypeSimple.IsEqual;

        private bool _valueOrVariable = true;
        private Variable _checkingVariable = null;
        
        private VariableType _valueType = VariableType.Boolean;
        private bool _boolValue = false;
        private float _floatValue = 0;
        private string _stringValue = string.Empty;


        private ConditionNode _conditionNode = null;

        //Called via activator
        public CreateConditionEditor(ConditionNode conditionNode) : this()
        {
            _conditionNode = conditionNode;
            //Debug.Log("Correct constructor");
        }

        public CreateConditionEditor()
        {
            Name = "Create Condition";
        }

        public override void OnGUI()
        {
            //GUILayout.BeginArea(Rect);
            bool enabledGUI = GUI.enabled;

            Rect rect = EditorGUILayout.BeginVertical();
            DrawVariable(ref _variable, enabledGUI);

            if (_variable != null)
            {
                DrawComparison();
                DrawValueChoice(enabledGUI);
                DrawValueOrVariable(enabledGUI);
            }

            DrawButtons(enabledGUI);
            EditorGUILayout.EndVertical();

            //_rect.size = GUILayoutUtility.GetLastRect().size;

            //GUILayout.EndArea();
            //GUI.DragWindow();
        }

        private void DrawVariable(ref Variable variable, bool enabledGUI)
        {
            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.PrefixLabel("Variable:");

            variable = (Variable)EditorGUILayout.ObjectField("Variable", variable, typeof(Variable), true,
                GUILayout.MinWidth(NodePreferences.PROPERTY_MIN_WIDTH));

            if (variable != null)
            {
                _valueType = variable.EnumType;

                GUI.enabled = false;
                EditorGUILayout.TextField(variable.Value.ToString());
                GUI.enabled = enabledGUI;
            }
            //EditorGUILayout.EndHorizontal();
        }

        private void DrawComparison()
        {
            switch (_variable.EnumType)
            {
                case VariableType.Float:
                    _comparisonType = (ComparisonType)EditorGUILayout.EnumPopup("Comparison:", _comparisonType);
                    break;
                case VariableType.Boolean:
                case VariableType.String:
                default:
                    _comparisonTypeSimple = (ComparisonTypeSimple)EditorGUILayout.EnumPopup("Comparison:", _comparisonTypeSimple);
                    _comparisonType = (ComparisonType)_comparisonTypeSimple;
                    break;
            }
        }

        private void DrawValueChoice(bool enabledGUI)
        {
            EditorGUILayout.BeginHorizontal();

            //Draw Left Toggle
            if (_valueOrVariable) GUI.enabled = false;
            _valueOrVariable = GUILayout.Toggle(_valueOrVariable, "Value", GUI.skin.button);
            GUI.enabled = enabledGUI;

            //Draw Right Toggle
            if (!_valueOrVariable) GUI.enabled = false;
            _valueOrVariable = !GUILayout.Toggle(!_valueOrVariable, "Variable", GUI.skin.button);
            GUI.enabled = enabledGUI;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawValueOrVariable(bool enabledGUI)
        {
            if (_valueOrVariable) DrawValue(enabledGUI);
            else DrawVariable(ref _checkingVariable, enabledGUI);
        }

        private void DrawValue(bool enabledGUI)
        {
            DrawValueTypeSelected(enabledGUI);
            DrawVariableTypeValue(enabledGUI);
        }

        private void DrawValueTypeSelected(bool enabledGUI)
        {
            EditorGUILayout.BeginHorizontal();
            _valueType = DrawVariableTypes(_valueType, enabledGUI);
            EditorGUILayout.EndHorizontal();
        }

        private VariableType DrawVariableTypes(VariableType varType, bool enabledGUI)
        {
            GUI.enabled = false;
            EditorGUILayout.BeginHorizontal();

            varType = DrawVariableType(varType, VariableType.Boolean, false);
            varType = DrawVariableType(varType, VariableType.Float, false);
            varType = DrawVariableType(varType, VariableType.String, false);

            EditorGUILayout.EndHorizontal();
            GUI.enabled = enabledGUI;

            return varType;
        }

        private VariableType DrawVariableType(VariableType varType, VariableType wantedVarType, bool enabledGUI)
        {
            bool isWantedType = varType == wantedVarType;
            if (isWantedType) GUI.enabled = false;

            bool toggled = GUILayout.Toggle(isWantedType, wantedVarType.ToString().Replace("VariableType.", ""), GUI.skin.button);
            if (toggled) varType = wantedVarType;

            GUI.enabled = enabledGUI;
            return varType;
        }

        private void DrawVariableTypeValue(bool enabledGUI)
        {
            EditorGUILayout.BeginHorizontal();

            switch (_valueType)
            {
                case VariableType.None:
                    break;
                case VariableType.Boolean:
                    _boolValue = DrawTrueAndFalseToggle(_boolValue, enabledGUI);
                    break;
                case VariableType.Float:
                    _floatValue = EditorGUILayout.FloatField(_floatValue);
                    break;
                case VariableType.String:
                    _stringValue = EditorGUILayout.TextField(_stringValue);
                    break;
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool DrawTrueAndFalseToggle(bool value, bool enabledGui)
        {
            EditorGUILayout.BeginHorizontal();

            //Draw Left Toggle
            if (value) GUI.enabled = false;
            value = GUILayout.Toggle(value, "True", GUI.skin.button);
            GUI.enabled = enabledGui;

            //Draw Right Toggle
            if (!value) GUI.enabled = false;
            value = !GUILayout.Toggle(!value, "False", GUI.skin.button);
            GUI.enabled = enabledGui;

            EditorGUILayout.EndHorizontal();

            return value;
        }

        private void DrawButtons(bool enabledGUI)
        {
            bool shouldClose = false;
            EditorGUILayout.BeginHorizontal();

            if (_variable == null) GUI.enabled = false;

            if (!_valueOrVariable)
            {
                if (_checkingVariable == null || (_variable != null && _checkingVariable.EnumType != _variable.EnumType)) GUI.enabled = false;
                if (GUILayout.Button("Create"))
                {
                    CreateCondition(null);
                    shouldClose = true;
                }
            }
            else
            {
                if (_valueType == VariableType.None) GUI.enabled = false;
                if (GUILayout.Button("Create"))
                {
                    CreateCondition(CreateValue());
                    shouldClose = true;
                }

            }
            GUI.enabled = enabledGUI;

            if (GUILayout.Button("Cancel"))
            {
                shouldClose = true;
            }

            if (shouldClose) Window.CloseWindow(this);
            EditorGUILayout.EndHorizontal();
        }

        private Value CreateValue()
        {
            Value value = ScriptableObject.CreateInstance<Value>();
            
            value.EnumType = _valueType;
            AssignValue(ref value);
            _conditionNode.AddValue(value);

            AssetDatabase.AddObjectToAsset(value, _conditionNode);
            OtherUtilities.AutoSaveAssets();

            return value;
            //Repaint();
        }

        private void AssignValue(ref Value value)
        {
            switch (value.EnumType)
            {
                case VariableType.Boolean: value.BoolValue = _boolValue; break;
                case VariableType.Float: value.FloatValue = _floatValue; break;
                case VariableType.String: value.StringValue = _stringValue; break;
            }
        }

        private void CreateCondition(Value value)
        {
            Condition condition = new Condition();
            condition.IsOutsideVariable = !_valueOrVariable;
            condition.Variable = _variable;
            condition.LocalValue = value;
            condition.OtherVariable = _checkingVariable;
            condition.ComparisonType = _comparisonType;
            _conditionNode.AddCondition(condition);
        }
    }
}
