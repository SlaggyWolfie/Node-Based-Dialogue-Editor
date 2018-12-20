using RPG.Dialogue;
using RPG.Nodes.Base;
using RPG.Editor.Nodes;
using RPG.Utility.Editor;
using UnityEditor;
using UnityEngine;

namespace RPG.Editor
{
    [CustomConnectionModifierEditor(typeof(EditVariableModifier))]
    public sealed class EditVariableModEditor : ConnectionModifierEditor
    {
        private EditVariableModifier _editVarMod = null;
        private EditVariableModifier EditVariableModifier
        {
            get { return _editVarMod ?? (_editVarMod = (EditVariableModifier)Target); }
        }

        private SerializedProperty _variableProperty = null;
        private SerializedProperty _otherVariableProperty = null;
        //private SerializedProperty _localValueProperty = null;
        private SerializedProperty _usingBuiltInValueProperty = null;
        private SerializedProperty _editOperationProperty = null;

        private VariableType _variableType = VariableType.None;
        //private SerializedProperty _

        protected override void OnEnable()
        {
            _variableProperty = SerializedObject.FindProperty("_variable");
            _otherVariableProperty = SerializedObject.FindProperty("_otherVariable");
            //_localValueProperty = SerializedObject.FindProperty("_localValue");
            _usingBuiltInValueProperty = SerializedObject.FindProperty("_usingBuiltInValue");
            _editOperationProperty = SerializedObject.FindProperty("_operation");
        }

        public override float GetWidth()
        {
            return NodePreferences.STANDARD_NODE_SIZE.x;
        }

        public override void OnGUI()
        {
            SerializedObject.Update();
            bool enabledGUI = GUI.enabled;

            DrawVariable(_variableProperty, enabledGUI);
            //DrawVariable(_variable, enabledGUI);

            if (EditVariableModifier.Variable != null)
            {
                DrawEditOperation(enabledGUI);
                DrawValueChoice(enabledGUI);
                DrawValueOrVariable(enabledGUI);
            }

            SerializedObject.ApplyModifiedProperties();
        }

        private void DrawVariable(SerializedProperty variableProperty, bool enabledGUI)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.ObjectField(variableProperty, typeof(Variable), new GUIContent("Variable"),
                GUILayout.MinWidth(NodePreferences.PROPERTY_MIN_WIDTH));

            if (!EditorGUI.EndChangeCheck()) return;
            SerializedObject.ApplyModifiedProperties();
            SerializedObject.Update();
            if (variableProperty.objectReferenceValue == null) return;

            _variableType = EditVariableModifier.Variable.EnumType;

            GUI.enabled = false;
            EditorGUILayout.TextField(EditVariableModifier.Variable.Value.ToString());
            GUI.enabled = enabledGUI;
        }

        private void DrawEditOperation(bool enabledGUI)
        {
            if (_variableType == VariableType.Float)
            {
                EditorGUILayout.ObjectField(_editOperationProperty);
            }
            else
            {
                EditVariableModifier.Operation = EditVariableModifier.EditOperation.Set;
                GUI.enabled = false;
                EditorGUILayout.EnumPopup(EditVariableModifier.Operation);
                GUI.enabled = enabledGUI;
            }
        }

        private void DrawValueChoice(bool enabledGUI)
        {
            SerializedObject.Update();
            EditorGUILayout.BeginHorizontal();

            //bool builtInValue = EditVariableModifier.UsingBuiltInValue;
            bool builtInValue = _usingBuiltInValueProperty.boolValue;

            EditorGUI.BeginChangeCheck();

            //Draw Left Toggle
            if (builtInValue) GUI.enabled = false;
            builtInValue = GUILayout.Toggle(builtInValue, "Value", GUI.skin.button);
            GUI.enabled = enabledGUI;

            //Draw Right Toggle
            if (!builtInValue) GUI.enabled = false;
            builtInValue = !GUILayout.Toggle(!builtInValue, "Variable", GUI.skin.button);
            GUI.enabled = enabledGUI;

            if (EditorGUI.EndChangeCheck())
            {
                //Undo.RecordObject(EditVariableModifier, "Using Built In Value change");
                //EditVariableModifier.UsingBuiltInValue = builtInValue;
                _usingBuiltInValueProperty.boolValue = builtInValue;

                if (builtInValue) CreateLocalValue();
                else DeleteLocalValue();
            }

            EditorGUILayout.EndHorizontal();
            SerializedObject.ApplyModifiedProperties();
        }

        private void DrawValueOrVariable(bool enabledGUI)
        {
            if (EditVariableModifier.UsingBuiltInValue) DrawValue(enabledGUI);
            else DrawVariable(_otherVariableProperty, enabledGUI);
        }

        private void DrawValue(bool enabledGUI)
        {
            DrawValueTypeInferred(enabledGUI);
            Undo.RecordObject(EditVariableModifier, "Changed the value of the Local Value");
            DrawVariableTypeValue(enabledGUI);
        }

        private void DrawValueTypeInferred(bool enabledGUI)
        {
            EditorGUILayout.BeginHorizontal();
            DrawVariableTypes(_variableType, enabledGUI);
            EditorGUILayout.EndHorizontal();
        }

        private VariableType DrawVariableTypes(VariableType varType, bool enabledGUI)
        {
            GUI.enabled = false;
            EditorGUILayout.BeginHorizontal();

            DrawVariableType(varType, VariableType.Boolean, false);
            DrawVariableType(varType, VariableType.Float, false);
            DrawVariableType(varType, VariableType.String, false);

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

            switch (_variableType)
            {
                case VariableType.Boolean:
                    {
                        bool boolValue = EditVariableModifier.LocalValue.BoolValue;
                        EditorGUI.BeginChangeCheck();
                        boolValue = DrawTrueAndFalseToggle(boolValue, enabledGUI);
                        if (EditorGUI.EndChangeCheck()) EditVariableModifier.LocalValue.BoolValue = boolValue;
                        break;
                    }
                case VariableType.Float:
                    {
                        float floatValue = EditVariableModifier.LocalValue.FloatValue;
                        EditorGUI.BeginChangeCheck();
                        floatValue = EditorGUILayout.FloatField(floatValue);
                        if (EditorGUI.EndChangeCheck()) EditVariableModifier.LocalValue.FloatValue = floatValue;
                        break;
                    }
                case VariableType.String:
                    {
                        string stringValue = EditVariableModifier.LocalValue.StringValue;
                        EditorGUI.BeginChangeCheck();
                        stringValue = EditorGUILayout.TextField(stringValue);
                        if (EditorGUI.EndChangeCheck()) EditVariableModifier.LocalValue.StringValue = stringValue;
                        break;
                    }
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool DrawTrueAndFalseToggle(bool value, bool enabledGUI)
        {
            EditorGUILayout.BeginHorizontal();

            //Draw Left Toggle
            if (value) GUI.enabled = false;
            value = GUILayout.Toggle(value, "True", GUI.skin.button);
            GUI.enabled = enabledGUI;

            //Draw Right Toggle
            if (!value) GUI.enabled = false;
            value = !GUILayout.Toggle(!value, "False", GUI.skin.button);
            GUI.enabled = enabledGUI;

            EditorGUILayout.EndHorizontal();

            return value;
        }

        private void CreateLocalValue()
        {
            if (EditVariableModifier.LocalValue != null) return;

            Value value = ScriptableObject.CreateInstance<Value>();
            value.name = "Edit Variable " + EditVariableModifier.GetHashCode() + " Local Value";
            EditVariableModifier.LocalValue = value;
            AssetDatabase.AddObjectToAsset(value, EditVariableModifier);
            EditorUtilities.AutoSaveAssets();
        }

        private void DeleteLocalValue()
        {
            Object.DestroyImmediate(EditVariableModifier.LocalValue);
            EditVariableModifier.LocalValue = null;
            EditorUtilities.AutoSaveAssets();
        }
    }
}
