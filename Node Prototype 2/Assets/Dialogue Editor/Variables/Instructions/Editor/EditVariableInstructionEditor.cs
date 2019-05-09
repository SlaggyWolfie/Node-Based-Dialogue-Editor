using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WolfEditor.Nodes.Base;
using WolfEditor.Editor.Nodes;
using WolfEditor.Utility.Editor;
using UnityEditor;
using UnityEngine;
using WolfEditor.Base;
using WolfEditor.Dialogue;
using WolfEditor.Variables;
using Object = UnityEngine.Object;

namespace WolfEditor.Editor
{
    [CustomInstructionEditor(typeof(EditVariableInstruction))]
    public sealed class EditVariableInstructionEditor : InstructionEditor
    {
        private EditVariableInstruction _editVarMod = null;
        private EditVariableInstruction EditVariableInstruction
        {
            get { return _editVarMod ?? (_editVarMod = (EditVariableInstruction)Target); }
        }

        private SerializedProperty _typeProperty = null;
        private SerializedProperty _operationProperty = null;

        private SerializedProperty _intPairProperty = null;
        private SerializedProperty _floatPairProperty = null;
        private SerializedProperty _stringPairProperty = null;
        private SerializedProperty _boolPairProperty = null;

        private SerializedProperty _varProperty = null;
        private SerializedProperty _varRefProperty = null;

        private EditVariableInstruction.Type _internalType = EditVariableInstruction.Type.Int;

        private int _selectedIndex = -1;
        private Dictionary<Variable.Operation, string> _dictionary = new Dictionary<Variable.Operation, string>()
        {
            {Variable.Operation.Set, "Set" },
            {Variable.Operation.Add, "Add" },
            {Variable.Operation.Subtract, "Subtract" },
            {Variable.Operation.Multiply, "Multiply" },
            {Variable.Operation.Divide, "Divide" }
        };

        protected override void Awake()
        {
            base.Awake();

            _typeProperty = SerializedObject.FindProperty("_type");
            _operationProperty = SerializedObject.FindProperty("_operation");

            _intPairProperty = SerializedObject.FindProperty("_intPair");
            _stringPairProperty = SerializedObject.FindProperty("_stringPair");
            _floatPairProperty = SerializedObject.FindProperty("_floatPair");
            _boolPairProperty = SerializedObject.FindProperty("_boolPair");
        }

        public override void OnBodyGUI()
        {
            Debug.Assert(_typeProperty != null);
            Debug.Assert(_operationProperty != null);
            Debug.Assert(_floatPairProperty != null);
            Debug.Assert(_intPairProperty != null);
            Debug.Assert(_stringPairProperty != null);
            Debug.Assert(_boolPairProperty != null);

            SerializedObject.Update();

            DrawType();

            if (_varProperty == null || _varRefProperty == null) return;

            DrawVar();
            DrawOperation();
            DrawVarRef();

            SerializedObject.ApplyModifiedProperties();
        }

        private void DrawType()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_typeProperty);

            if (!EditorGUI.EndChangeCheck() && _internalType == EditVariableInstruction.VariableType) return;
            SerializedObject.ApplyModifiedProperties();

            SerializedProperty selectedPairProperty;
            switch (EditVariableInstruction.VariableType)
            {
                case EditVariableInstruction.Type.Int: selectedPairProperty = _intPairProperty; break;
                case EditVariableInstruction.Type.Float: selectedPairProperty = _floatPairProperty; break;
                case EditVariableInstruction.Type.String: selectedPairProperty = _stringPairProperty; break;
                case EditVariableInstruction.Type.Bool: selectedPairProperty = _boolPairProperty; break;
                default: throw new ArgumentOutOfRangeException();
            }

            _varProperty = selectedPairProperty.FindPropertyRelative("variable");
            _varRefProperty = selectedPairProperty.FindPropertyRelative("reference");

            _internalType = EditVariableInstruction.VariableType;
            _selectedIndex = 0;

            SerializedObject.ApplyModifiedProperties();
        }

        private void DrawVar()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_varProperty, new GUIContent("Variable"));
            if (EditorGUI.EndChangeCheck()) SerializedObject.ApplyModifiedProperties();
        }

        private void DrawOperation()
        {
            GUIContent label = new GUIContent("Operation");

            var target = EditorUtilities.GetTargetObjectOfProperty(_varProperty);
            //var target = _varProperty.value;
            if (target == null) return;

            Variable variable = (Variable)target;
            Variable.Operation[] operations = variable.GetPermittedOperations().ToArray();

            GUIContent[] options = new GUIContent[operations.Length];
            for (int i = 0; i < operations.Length; i++)
                options[i] = new GUIContent(_dictionary[operations[i]]);

            _selectedIndex = EditorGUILayout.Popup(label, _selectedIndex, options);

            _operationProperty.enumValueIndex = _selectedIndex;
            //EditorGUILayout.PropertyField(_operationProperty);
        }


        private void DrawVarRef()
        {
            EditorGUILayout.PropertyField(_varRefProperty, new GUIContent("Value"));
        }
    }
}
