using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RPG.Dialogue;
using RPG.Nodes.Base;
using UnityEditor;
using UnityEngine;

namespace RPG.Nodes.Editor
{
    public class AddVariableSubWindow : SubWindow
    {
        private string _variableName = String.Empty;
        private VariableType _variableType = VariableType.Boolean;
        private object _value = null;

        private ConditionNode _conditionNode = null;

        //Called via activator
        public AddVariableSubWindow(ConditionNode conditionNode) : this()
        {
            _conditionNode = conditionNode;
        }

        public AddVariableSubWindow()
        {
            Name = "Add Variable";
        }

        public override void OnGUI(int unusedWindowID)
        {
            GUILayout.BeginArea(Rect);

            bool enabledGUI = GUI.enabled;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Name:");
            _variableName = EditorGUILayout.TextField(_variableName);
            EditorGUILayout.BeginHorizontal();

            _variableType = DrawVariableTypes(_variableType, enabledGUI);

            switch (_variableType)
            {
                case VariableType.None:
                    break;
                case VariableType.Boolean:
                    _value = DrawTrueAndFalseToggle((bool)_value, enabledGUI);
                    break;
                case VariableType.Float:
                    _value = float.Parse(GUILayout.TextField((string)_value));
                    break;
                case VariableType.String:
                    _value = GUILayout.TextField((string)_value);
                    break;
            }

            DrawButtons(enabledGUI);

            GUILayout.EndArea();
            GUI.DragWindow();
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
            if (toggled)
            {
                varType = wantedVarType;
                DoDefaultConstructor(varType);
            }

            GUI.enabled = enabledGui;
            return varType;
        }

        private void DoDefaultConstructor(VariableType varType)
        {
            switch (varType)
            {
                case VariableType.Boolean:
                    //_value = default(bool);
                    _value = false;
                    break;
                case VariableType.Float:
                    //_value = default(float);
                    _value = 0.0f;
                    break;
                case VariableType.String:
                    //_value = default(string);
                    _value = string.Empty;
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
        
        private void DrawButtons(bool enabledGui)
        {
            if (_variableName == string.Empty || _variableType == VariableType.None || _value == null)
                GUI.enabled = false;

            NodeEditorWindow window = NodeEditorWindow.CurrentNodeEditorWindow;

            if (GUILayout.Button("Create"))
            {
                //window.AddVariable();
                window.CloseSubWindow(this);
            }

            GUI.enabled = enabledGui;

            if (GUILayout.Button("Cancel"))
            {
                window.CloseSubWindow(this);
            }
        }
    }
}
