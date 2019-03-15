using System;
using WolfEditor.Nodes.Base;
using UnityEditor;
using UnityEngine;
using WolfEditor.Utility.Editor;

namespace WolfEditor.Editor.Nodes
{
    public class EditVariableEditor : WindowEditor
    {
        //private 

        private string _variableName = String.Empty;
        private VariableType _variableType = VariableType.Boolean;
        //private object _variableValue = null;
        private bool _boolValue = false;
        private float _floatValue = 0;
        private string _stringValue = string.Empty;

        private Variable _variable = null;

        //Called via activator
        public EditVariableEditor(Variable variable) : this()
        {
            _variable = variable;
            _variableName = variable.name;
            _variableType = variable.EnumType;
            _boolValue = variable.BoolValue;
            _floatValue = variable.FloatValue;
            _stringValue = variable.StringValue;
            //Debug.Log("Correct constructor");
        }

        public EditVariableEditor()
        {
            Name = "Edit Variable";
        }

        public override void OnGUI()
        {
            //GUILayout.BeginArea(Rect);
            bool enabledGUI = GUI.enabled;

            Rect rect = EditorGUILayout.BeginVertical();
            DrawVariableNameField();
            DrawVariableTypeSelection(enabledGUI);
            DrawVariableTypeValue(enabledGUI);
            DrawButtons(enabledGUI);
            EditorGUILayout.EndVertical();

            _rect.size = rect.size;

            //GUILayout.EndArea();
            //GUI.DragWindow();
        }

        private void DrawVariableTypeValue(bool enabledGUI)
        {
            EditorGUILayout.BeginHorizontal();

            switch (_variableType)
            {
                case VariableType.None:
                    break;
                case VariableType.Boolean:
                    _boolValue = DrawTrueAndFalseToggle(_boolValue, enabledGUI);
                    break;
                case VariableType.Float:
                    _floatValue = EditorGUILayout.FloatField(_floatValue);
                    //_value = float.Parse(GUILayout.TextField((string)_value));
                    break;
                case VariableType.String:
                    _stringValue = EditorGUILayout.TextField(_stringValue);
                    //_value = GUILayout.TextField((string)_value);
                    break;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawVariableTypeSelection(bool enabledGUI)
        {
            EditorGUILayout.BeginHorizontal();
            _variableType = DrawVariableTypes(_variableType, enabledGUI);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawVariableNameField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name:");
            _variableName = EditorGUILayout.TextField(_variableName);
            EditorGUILayout.EndHorizontal();
        }

        private VariableType DrawVariableTypes(VariableType varType, bool enabledGui)
        {
            EditorGUILayout.BeginHorizontal();

            varType = DrawVariableType(varType, VariableType.Boolean, enabledGui);
            varType = DrawVariableType(varType, VariableType.Float, enabledGui);
            varType = DrawVariableType(varType, VariableType.String, enabledGui);

            EditorGUILayout.EndHorizontal();

            return varType;
        }

        private VariableType DrawVariableType(VariableType varType, VariableType wantedVarType, bool enabledGui)
        {
            bool isWantedType = varType == wantedVarType;
            if (isWantedType) GUI.enabled = false;

            bool toggled = GUILayout.Toggle(isWantedType, wantedVarType.ToString().Replace("VariableType.", ""), GUI.skin.button);
            if (toggled) varType = wantedVarType;

            GUI.enabled = enabledGui;
            return varType;
        }

        private void DoDefaultValues(VariableType varType)
        {
            switch (varType)
            {
                case VariableType.Boolean:
                    //_value = default(bool);
                    _boolValue = false;
                    break;
                case VariableType.Float:
                    //_value = default(float);
                    _floatValue = 0.0f;
                    break;
                case VariableType.String:
                    //_value = default(string);
                    _stringValue = string.Empty;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("varType", varType, null);
            }
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

        //private bool IsValueValid()
        //{
        //    switch (_variableType)
        //    {
        //        case VariableType.Boolean:
        //            //_value = default(bool);
        //            _boolValue = false;
        //            break;
        //        case VariableType.Float:
        //            //_value = default(float);
        //            _floatValue = 0.0f;
        //            break;
        //        case VariableType.String:
        //            //_value = default(string);
        //            _stringValue = string.Empty;
        //            break;
        //    }
        //}

        private void DrawButtons(bool enabledGui)
        {
            if (_variableName == string.Empty || _variableType == VariableType.None)
                GUI.enabled = false;

            bool shouldClose = false;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                SaveVariable();
                shouldClose = true;
            }

            GUI.enabled = enabledGui;

            if (GUILayout.Button("Cancel"))
            {
                shouldClose = true;
            }

            if (shouldClose) Window.CloseWindow(this);
            EditorGUILayout.EndHorizontal();
        }

        private void SaveVariable()
        {
            _variable.name = _variableName;
            //variable.VariableInventory = _variableInventory;
            _variable.EnumType = _variableType;
            //variable.UncastValue = _variableValue;
            AssignValue(_variable);

            //AssetDatabase.AddObjectToAsset(variable, _variableInventory);
            NodeEditorUtilities.AutoSaveAssets();
            //Repaint();
        }

        private void AssignValue(Variable variable)
        {
            switch (_variableType)
            {
                case VariableType.Boolean: variable.BoolValue = _boolValue; break;
                case VariableType.Float: variable.FloatValue = _floatValue; break;
                case VariableType.String: variable.StringValue = _stringValue; break;
            }
        }
    }
}
