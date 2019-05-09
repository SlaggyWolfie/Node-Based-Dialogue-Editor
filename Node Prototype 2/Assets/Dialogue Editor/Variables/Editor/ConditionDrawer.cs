using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WolfEditor.Utility.Editor;

namespace WolfEditor.Variables.Editor
{
    [CustomPropertyDrawer(typeof(Condition))]
    public sealed class ConditionDrawer : PropertyDrawer
    {
        private bool _initialized = false;
        private SerializedProperty _typeProperty;
        private SerializedProperty _comparisonProperty;
        private SerializedProperty _intPair;
        private SerializedProperty _floatPair;
        private SerializedProperty _stringPair;
        private SerializedProperty _boolPair;

        private GUIContent[] _options = null;
        private PairPropertyHolder _pairPropertyHolder = null;

        private Dictionary<Variable.Comparison, string> _dictionary = new Dictionary<Variable.Comparison, string>()
    {
        {Variable.Comparison.Equal, "Equal" },
        {Variable.Comparison.NotEqual, "Not Equal" },
        {Variable.Comparison.GreaterThan, "Greater Than" },
        {Variable.Comparison.LesserThan, "Lesser Than" },
        {Variable.Comparison.GreaterThanOrEqual, "Greater Than or Equal" },
        {Variable.Comparison.LesserThanOrEqual, "Lesser Than or Equal" }
    };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            Initialize(property);

            Debug.Assert(_typeProperty != null);
            Debug.Assert(_comparisonProperty != null);
            Debug.Assert(_intPair != null);
            Debug.Assert(_floatPair != null);
            Debug.Assert(_stringPair != null);
            Debug.Assert(_boolPair != null);

            Rect typeRect, varRect, operationRect, otherRect;
            CalculateRects(position, out typeRect, out varRect, out operationRect, out otherRect);

            EditorGUI.BeginChangeCheck();

            //Store old indent level and set it to 0, the PrefixLabel takes care of it
            //int indent = EditorGUI.indentLevel;
            //EditorGUI.indentLevel = 0;

            Condition.Type type = (Condition.Type)EditorGUI.EnumPopup(typeRect, (Condition.Type)_typeProperty.enumValueIndex); ;
            _typeProperty.enumValueIndex = (int)type;

            if (EditorGUI.EndChangeCheck() || _pairPropertyHolder == null)
            {
                property.serializedObject.ApplyModifiedProperties();

                switch (type)
                {
                    case Condition.Type.Bool: _pairPropertyHolder = new PairPropertyHolder(_boolPair); break;
                    case Condition.Type.Float: _pairPropertyHolder = new PairPropertyHolder(_floatPair); break;
                    case Condition.Type.Int: _pairPropertyHolder = new PairPropertyHolder(_intPair); break;
                    case Condition.Type.String: _pairPropertyHolder = new PairPropertyHolder(_stringPair); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(varRect, _pairPropertyHolder.variableProperty, GUIContent.none);
            if (EditorGUI.EndChangeCheck() || _options == null)
            {
                property.serializedObject.ApplyModifiedProperties();

                Variable.Comparison[] comparisons = _pairPropertyHolder.pair.GetGenericVariable().GetPermittedComparisons().ToArray();
                _options = new GUIContent[comparisons.Length];
                for (int i = 0; i < comparisons.Length; i++) _options[i] = new GUIContent(_dictionary[comparisons[i]]);
            }

            _comparisonProperty.enumValueIndex = EditorGUI.Popup(operationRect, _comparisonProperty.enumValueIndex, _options);

            EditorGUI.PropertyField(otherRect, _pairPropertyHolder.referenceProperty, GUIContent.none);
            if (EditorGUI.EndChangeCheck()) property.serializedObject.ApplyModifiedProperties();

            //EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private void Initialize(SerializedProperty property)
        {
            //Get properties
            if (_initialized) return;
            _typeProperty = property.FindPropertyRelative("_type");
            _comparisonProperty = property.FindPropertyRelative("_comparison");

            _intPair = property.FindPropertyRelative("_intPair");
            _floatPair = property.FindPropertyRelative("_floatPair");
            _stringPair = property.FindPropertyRelative("_stringPair");
            _boolPair = property.FindPropertyRelative("_boolPair");

            _initialized = true;
        }

        private static void CalculateRects(Rect position,
            out Rect typeRect, out Rect varRect,
            out Rect operationRect, out Rect otherRect)
        {
            //Calculate positions
            float distance = EditorGUIUtility.standardVerticalSpacing;
            typeRect = varRect = operationRect = otherRect = position;

            // |type| <-distance-> |var| <-distance-> |op| <-distance-> |otherVar|
            typeRect.width = position.width * 2 / 9;
            varRect.xMin = typeRect.xMax + distance;
            varRect.width = typeRect.width;
            operationRect.xMin = varRect.xMax + distance;
            operationRect.width = varRect.width;
            otherRect.xMin = operationRect.xMax + distance;
            otherRect.width = position.width * 1 / 3;
        }

        private class PairPropertyHolder
        {
            public Pair pair = null;
            //public SerializedProperty pairProperty = null;
            public SerializedProperty variableProperty = null;
            public SerializedProperty referenceProperty = null;

            public PairPropertyHolder(SerializedProperty pairProperty)
            {
                //this.pairProperty = pairProperty;
                pair = (Pair)EditorUtilities.GetTargetObjectOfProperty(pairProperty);
                variableProperty = pairProperty.FindPropertyRelative("variable");
                referenceProperty = pairProperty.FindPropertyRelative("reference");
            }
        }
    }
}