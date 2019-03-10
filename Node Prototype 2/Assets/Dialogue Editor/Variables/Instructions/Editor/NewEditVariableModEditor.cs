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
    [CustomInstructionEditor(typeof(EditVariableModifier))]
    public sealed class NewEditVariableModEditor : InstructionEditor
    {
        private EditVariableModifier _editVarMod = null;
        private EditVariableModifier EditVariableModifier
        {
            get { return _editVarMod ?? (_editVarMod = (EditVariableModifier)Target); }
        }

        private SerializedProperty _typeProperty = null;
        private SerializedProperty _operationProperty = null;

        private SerializedProperty _intPairProperty = null;
        private SerializedProperty _floatPairProperty = null;
        private SerializedProperty _stringPairProperty = null;
        private SerializedProperty _boolPairProperty = null;

        private SerializedProperty _varProperty = null;
        private SerializedProperty _varRefProperty = null;

        private EditVariableModifier.Type _internalType = EditVariableModifier.Type.Int;

        private int _selectedIndex = -1;
        private Dictionary<NewVariable.Operation, string> _dictionary = new Dictionary<NewVariable.Operation, string>()
        {
            {NewVariable.Operation.Set, "Set" },
            {NewVariable.Operation.Add, "Add" },
            {NewVariable.Operation.Subtract, "Subtract" },
            {NewVariable.Operation.Multiply, "Multiply" },
            {NewVariable.Operation.Divide, "Divide" }
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

            if (!EditorGUI.EndChangeCheck() && _internalType == EditVariableModifier.VariableType) return;
            SerializedObject.ApplyModifiedProperties();

            SerializedProperty selectedPairProperty;
            switch (EditVariableModifier.VariableType)
            {
                case EditVariableModifier.Type.Int: selectedPairProperty = _intPairProperty; break;
                case EditVariableModifier.Type.Float: selectedPairProperty = _floatPairProperty; break;
                case EditVariableModifier.Type.String: selectedPairProperty = _stringPairProperty; break;
                case EditVariableModifier.Type.Bool: selectedPairProperty = _boolPairProperty; break;
                default: throw new ArgumentOutOfRangeException();
            }

            _varProperty = selectedPairProperty.FindPropertyRelative("Var");
            _varRefProperty = selectedPairProperty.FindPropertyRelative("Ref");

            _internalType = EditVariableModifier.VariableType;
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

            var target = GetTargetObjectOfProperty(_varProperty);
            //var target = _varProperty.value;
            if (target == null) return;

            NewVariable variable = (NewVariable)target;
            NewVariable.Operation[] operations = variable.GetPermittedOperations().ToArray();

            GUIContent[] options = new GUIContent[operations.Length];
            for (int i = 0; i < operations.Length; i++)
                options[i] = new GUIContent(_dictionary[operations[i]]);

            _selectedIndex = EditorGUILayout.Popup(label, _selectedIndex, options);

            _operationProperty.enumValueIndex = _selectedIndex;
            //EditorGUILayout.PropertyField(_operationProperty);
        }

        //(Old) Stolen from https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBaseEditor/EditorHelper.cs
        //Stolen from https://github.com/lordofduct/spacepuppy-unity-framework-3.0/blob/master/SpacepuppyUnityFrameworkEditor/EditorHelper.cs
        #region
        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }
        
        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }

        #endregion

        private void DrawVarRef()
        {
            EditorGUILayout.PropertyField(_varRefProperty, new GUIContent("Value"));
        }
    }
}
