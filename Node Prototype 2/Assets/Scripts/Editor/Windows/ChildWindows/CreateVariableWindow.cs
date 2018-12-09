using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RPG.Dialogue;
using RPG.Nodes.Base;
using RPG.Nodes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPG.Editor.Nodes
{
    public class CreateVariableWindow : ChildWindow
    {
        private string _variableName = String.Empty;
        private VariableType _variableType = VariableType.Boolean;
        private object _variableValue = null;

        private VariableInventory _variableInventory = null;

        //Called via activator
        public CreateVariableWindow(VariableInventory variableInventory) : this()
        {
            _variableInventory = variableInventory;
            //Debug.Log("Correct constructor");
        }

        public CreateVariableWindow()
        {
            Name = "Add Variable";
        }

        public override void OnGUI(int unusedWindowID)
        {
            //GUILayout.BeginArea(Rect);

            bool enabledGUI = GUI.enabled;

            DrawVariableNameField();
            DrawVariableTypeSelection(enabledGUI);
            DrawVariableTypeValue(enabledGUI);
            DrawButtons(enabledGUI);

            _rect.size = GUILayoutUtility.GetLastRect().size;

            //GUILayout.EndArea();
            GUI.DragWindow();
        }

        private void DrawVariableTypeValue(bool enabledGUI)
        {
            EditorGUILayout.BeginHorizontal();

            switch (_variableType)
            {
                case VariableType.None:
                    break;
                case VariableType.Boolean:
                    _variableValue = DrawTrueAndFalseToggle((bool) _variableValue, enabledGUI);
                    break;
                case VariableType.Float:
                    _variableValue = EditorGUILayout.FloatField((float) _variableValue);
                    //_value = float.Parse(GUILayout.TextField((string)_value));
                    break;
                case VariableType.String:
                    _variableValue = EditorGUILayout.TextField((string) _variableValue);
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
            if (toggled)
            {
                varType = wantedVarType;
                DoDefaultValues(varType);
            }

            GUI.enabled = enabledGui;
            return varType;
        }

        private void DoDefaultValues(VariableType varType)
        {
            switch (varType)
            {
                case VariableType.Boolean:
                    //_value = default(bool);
                    _variableValue = false;
                    break;
                case VariableType.Float:
                    //_value = default(float);
                    _variableValue = 0.0f;
                    break;
                case VariableType.String:
                    //_value = default(string);
                    _variableValue = string.Empty;
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
            if (_variableName == string.Empty || _variableType == VariableType.None || _variableValue == null)
                GUI.enabled = false;

            bool shouldClose = false;
            NodeEditorWindow window = NodeEditorWindow.CurrentNodeEditorWindow;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create"))
            {
                AddVariable();
                shouldClose = true;
            }

            GUI.enabled = enabledGui;

            if (GUILayout.Button("Cancel"))
            {
                shouldClose = true;
            }

            if (shouldClose) window.CloseSubWindow(this);
            EditorGUILayout.EndHorizontal();
        }

        private void AddVariable()
        {
            Variable variable = ScriptableObject.CreateInstance<Variable>();
            
            variable.name = _variableName;
            variable.VariableInventory = _variableInventory;
            variable.EnumType = _variableType;
            variable.UncastValue = _variableValue;

            _variableInventory.AddVariable(variable);

            AssetDatabase.AddObjectToAsset(variable, _variableInventory);
            NodeUtility.AutoSaveAssets();
            Repaint();
        }
    }
}
