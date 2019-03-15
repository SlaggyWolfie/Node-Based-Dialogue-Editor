using System;
using UnityEditor;
using UnityEngine;
using WolfEditor.Nodes.Base;
using WolfEditor.Utility.Editor;
using WolfEditor.Variables;

[CustomPropertyDrawer(typeof(Condition))]
public sealed class ConditionDrawer : PropertyDrawer
{
    //private Object _cachedObject = null;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        //Get properties
        SerializedProperty typeProperty = property.FindPropertyRelative("_type");
        SerializedProperty comparisonProperty = property.FindPropertyRelative("_comparison");

        Debug.Assert(typeProperty != null);
        Debug.Assert(comparisonProperty != null);

        SerializedProperty intPair = property.FindPropertyRelative("_intPair");
        SerializedProperty floatPair = property.FindPropertyRelative("_floatPair");
        SerializedProperty stringPair = property.FindPropertyRelative("_stringPair");
        SerializedProperty boolPair = property.FindPropertyRelative("_boolPair");

        Debug.Assert(intPair != null);
        Debug.Assert(floatPair != null);
        Debug.Assert(stringPair != null);
        Debug.Assert(boolPair != null);

        //Calculate positions
        float distance = EditorGUIUtility.standardVerticalSpacing;
        Rect typeRect, varRect, operationRect, otherRect;
        typeRect = varRect = operationRect = otherRect = position;

        // |type| <-distance-> |var| <-distance-> |op| <-distance-> |otherVar|
        typeRect.width = position.width * 2 / 9;
        varRect.xMin = typeRect.xMax + distance;
        varRect.width = typeRect.width;
        operationRect.xMin = varRect.xMax + distance;
        operationRect.width = varRect.width;
        otherRect.xMin = operationRect.xMax + distance;
        otherRect.width = position.width * 1 / 3;

        EditorGUI.BeginChangeCheck();

        //Store old indent level and set it to 0, the PrefixLabel takes care of it
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        //_cachedObject = EditorGUI.ObjectField(varRect, _cachedObject, typeof(Variable), false);

        Condition.Type type = (Condition.Type)EditorGUI.EnumPopup(typeRect, (Condition.Type)typeProperty.enumValueIndex); ;
        typeProperty.enumValueIndex = (int)type;

        if (EditorGUI.EndChangeCheck()) property.serializedObject.ApplyModifiedProperties();

        EditorGUI.BeginChangeCheck();

        Pair pair = null;
        switch (type)
        {
            case Condition.Type.Bool:
                pair = (BoolPair)EditorUtilities.GetTargetObjectOfProperty(boolPair);
                break;
            case Condition.Type.Float:
                pair = (FloatPair)EditorUtilities.GetTargetObjectOfProperty(floatPair);
                break;
            case Condition.Type.Int:
                pair = (IntPair)EditorUtilities.GetTargetObjectOfProperty(intPair);
                break;
            case Condition.Type.String:
                pair = (StringPair)EditorUtilities.GetTargetObjectOfProperty(stringPair);
                break;
            default: throw new ArgumentOutOfRangeException();
        }
        //int result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 0 : 1, _popupOptions, _popupStyle);
       
        //TODO
        EditorGUI.PropertyField(position, , GUIContent.none);

        if (EditorGUI.EndChangeCheck()) property.serializedObject.ApplyModifiedProperties();

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}